using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Skill;

using Character = TeamLog.Characters.Character;
using StatType = TeamLog.Characters.StatType;

namespace TeamLog.Combat
{
    /// <summary>
    /// 중앙화된 데미지 계산/적용 — 특성 훅, 유물 훅, 반사 피해, 처치 이벤트
    /// TurnManager와 EnemyAIController 모두에서 사용
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// 회피 발생 시 이벤트 (FloatingText "MISS" 표시용)
        /// </summary>
        public static event System.Action<Character> OnAttackMissed;

        /// <summary>
        /// 중앙화된 데미지 계산 공식
        /// </summary>
        public static int CalculateDamage(int attackPower, int defense)
        {
            return System.Math.Max(1, attackPower - defense);
        }

        /// <summary>
        /// 중앙화된 데미지 적용: 공격자 ATK + bonusPower - 대상 DEF + 특성 훅 + 유물 훅
        /// </summary>
        public static void DealDamage(Character attacker, Character target, int bonusPower = 0)
        {
            int damage = attacker.Stats.GetStat(StatType.ATK) + bonusPower;
            int defense = target.Stats.GetStat(StatType.DEF);

            // 유물: 추가 데미지 / 피해 감소
            var relicHandler = GameRunState.Instance?.RelicHandler;
            if (relicHandler != null)
            {
                damage += relicHandler.GetBonusOutgoingDamage();
                defense += relicHandler.GetDamageReduction();
            }

            // Phase 8C: 장착 특성 — 공격자 PowerAdd/BonusOutgoingDamage + 대상 DamageReduction
            if (attacker.PlayerTraitHandler != null && attacker.PlayerTraitHandler.HasTrait)
                damage += attacker.PlayerTraitHandler.GetBonusOutgoingDamage();
            if (target.PlayerTraitHandler != null && target.PlayerTraitHandler.HasTrait)
                defense += target.PlayerTraitHandler.GetDamageReduction();

            int calculatedDamage = CalculateDamage(damage, defense);

            // 대상 특성: 들어오는 데미지 수정 (Sturdy 절반)
            calculatedDamage = target.TraitHandler.ModifyIncomingDamage(calculatedDamage);

            // 회피 시 MISS 처리
            if (calculatedDamage == 0)
            {
                OnAttackMissed?.Invoke(target);
                return;
            }

            target.Health.TakeDamage(calculatedDamage);

            // 공격자 특성: 피해를 입혔을 때 (Corrosive 방어감소)
            attacker.TraitHandler.OnDamageDealtTo(target);

            // 대상 특성: 피해를 받은 후 (Counter/Thorns/Rampage/ArcaneFury/Rally)
            target.TraitHandler.OnDamageReceived(attacker, calculatedDamage);

            // 유물: 반사 피해 (ThornArmor) — 피해받은 대상이 플레이어인 경우
            if (relicHandler != null)
            {
                int counterDmg = relicHandler.GetCounterDamage();
                if (counterDmg > 0 && attacker.IsAlive)
                {
                    var party = GameRunState.Instance.PlayerParty;
                    if (party != null)
                    {
                        foreach (var member in party)
                        {
                            if (member == target)
                            {
                                int actual = System.Math.Max(1, counterDmg);
                                attacker.Health.TakeDamage(actual);
                                CombatEventBus.FireDamageDealt(target, attacker, actual);
                                break;
                            }
                        }
                    }
                }
            }

            // CombatEventBus: 유물 트리거
            CombatEventBus.FireDamageDealt(attacker, target, calculatedDamage);
            CombatEventBus.FireDamageReceived(target, calculatedDamage);

            // 사망 시 Kill 이벤트
            if (target.IsDead)
                CombatEventBus.FireKill(target);
        }

        /// <summary>
        /// 정적 이벤트 정리 — 전투 종료 시 호출하여 람다 누적 방지
        /// </summary>
        public static void ClearEvents()
        {
            OnAttackMissed = null;
        }
    }
}
