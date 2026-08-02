namespace TeamLog.Characters
{
    /// <summary>
    /// 적 패시브 특성 종류 — 각 특성은 파훼 가능한 전략적 메커니즘을 제공
    /// </summary>
    public enum EnemyTrait
    {
        None,

        // === 일반 적 특성 ===
        Regenerate,     // 재생: 턴 시작 시 HP 5 회복, 단 도트(Poison/Burn) 시 불가
        Opportunist,    // 약자 노림: 항상 최저 HP 파티원 타겟, Taunt 무시
        PhaseShift,     // 위상 변이: 홀수 턴 DEF+4, 짝수 턴 ATK+4
        Counter,        // 반격: 피격 시 공격자에게 고정 3 데미지 반격
        Thorns,         // 가시: 피격 시 받은 피해의 30%를 공격자에게 반사
        Shell,          // 껍질: 매 턴 첫 번째 상태이상 무효화

        // === 엘리트 특성 ===
        Sturdy,         // 견고: 매 턴 첫 공격 데미지 50% 감소
        ArcaneFury,     // 마력 폭주: HP 50% 이하 시 ATK +5
        Corrosive,      // 부식: 피해를 입힌 대상에게 방어감소 디버프

        // === 보스 특성 ===
        Rally,          // 소집령: HP 50% 이하 시 AttackUp+DefenseUp 획득
        Rampage,        // 연소: 피해를 입지 않은 턴마다 ATK +3 누적, 피해를 입으면 초기화
        Immortal,       // 불사: 치명적 피해 시 HP 1로 생존 (1회)

        // === Phase GF (잿빛 숲) 전용 특성 ===
        Venomous,       // 맹독: 공격 적중 시 대상에게 Poison 1 (2턴) 부여 — Blightbed Crawler
        Corpsebloom,    // 시체개화: 아군 사망 시 30% HP로 부활 (전투당 1회) — The Compost King
        Provoke         // 도발부: 매 턴 시작 시 자동 Taunt 부여 — Mossbulwark
    }

    /// <summary>
    /// 적 특성 처리기 — 순수 C# 클래스
    /// </summary>
    public class EnemyTraitHandler
    {
        private readonly EnemyTrait _trait;
        private readonly Character _owner;

        // Sturdy: 매 턴 첫 공격에만 적용
        private bool _sturdyAvailable;

        // Immortal: 1회용
        private bool _immortalUsed;

        // ArcaneFury / Rally: 1회 발동
        private bool _thresholdTriggered;

        // PhaseShift: 이전 턴에 적용한 스탯 타입 추적
        private bool _phaseWasAttack;

        // Rampage: 누적 스택 + 피해 여부 추적
        private int _rampageStacks;
        private bool _wasDamagedThisTurn;

        // Shell: 매 턴 1회 차단
        private bool _shellUsedThisTurn;

        // Corpsebloom: 1회용 부활
        private bool _corpsebloomUsed;

        public EnemyTrait Trait => _trait;
        public bool HasTrait => _trait != EnemyTrait.None;

        public EnemyTraitHandler(EnemyTrait trait, Character owner)
        {
            _trait = trait;
            _owner = owner;
        }

        /// <summary>
        /// 턴 시작 시 발동
        /// </summary>
        public void OnTurnStart(int turnNumber = 1)
        {
            if (!HasTrait) return;

            switch (_trait)
            {
                case EnemyTrait.Regenerate:
                    // 도트 데미지 상태가 아니면 HP 회복.
                    // ★ Phase BALANCE (2026-07-22): 5 → 2로 축소.
                    // 기존 5는 밸런스 축소(HP ×0.25) 전 수치. HP 7 적이 매 턴 +5면 사실상 안 죽음.
                    // 2로 축소하여 Charge 3 데미지가 재생 2를 초과 → 순 1/턴 데미지.
                    if (!_owner.StatusEffects.HasEffect(StatusEffectType.Poison)
                        && !_owner.StatusEffects.HasEffect(StatusEffectType.Burn))
                        _owner.Health.Heal(2);
                    break;

                case EnemyTrait.Sturdy:
                    _sturdyAvailable = true;
                    break;

                case EnemyTrait.PhaseShift:
                    ApplyPhaseShift(turnNumber);
                    break;

                case EnemyTrait.Rampage:
                    ApplyRampage();
                    break;

                case EnemyTrait.Shell:
                    _shellUsedThisTurn = false;
                    break;

                // Phase GF: Provoke — 매 턴 자동 도발 부여
                case EnemyTrait.Provoke:
                    _owner.StatusEffects.ApplyEffect(StatusEffectType.Taunt, 1, 0);
                    break;
            }
        }

        /// <summary>
        /// 들어오는 데미지 수정 (Sturdy 절반)
        /// </summary>
        public int ModifyIncomingDamage(int damage)
        {
            if (!HasTrait || damage <= 0) return damage;

            switch (_trait)
            {
                case EnemyTrait.Sturdy:
                    if (_sturdyAvailable)
                    {
                        _sturdyAvailable = false;
                        return damage / 2;
                    }
                    return damage;

                default:
                    return damage;
            }
        }

        /// <summary>
        /// 데미지를 받은 후 발동
        /// </summary>
        public void OnDamageReceived(Character attacker = null, int damage = 0)
        {
            if (!HasTrait) return;

            switch (_trait)
            {
                // Counter: 공격자에게 고정 3 데미지 반격
                case EnemyTrait.Counter:
                    if (attacker != null && attacker.IsAlive)
                        attacker.Health.TakeDamage(3);
                    break;

                // Thorns: 받은 피해의 30% 반사
                case EnemyTrait.Thorns:
                    if (attacker != null && attacker.IsAlive && damage > 0)
                        attacker.Health.TakeDamage(damage * 3 / 10);
                    break;

                // Rampage: 피해 받음 기록 (다음 턴 리셋용)
                case EnemyTrait.Rampage:
                    _wasDamagedThisTurn = true;
                    break;

                // ArcaneFury: HP 임계치 즉시 체크
                case EnemyTrait.ArcaneFury:
                    if (!_thresholdTriggered)
                        CheckThreshold(5);
                    break;

                // Rally: HP 임계치 → AttackUp + DefenseUp
                case EnemyTrait.Rally:
                    if (!_thresholdTriggered)
                    {
                        float hpRatio = (float)_owner.Health.CurrentHP / _owner.Health.MaxHP;
                        if (hpRatio <= 0.5f)
                        {
                            _owner.StatusEffects.ApplyEffect(StatusEffectType.AttackUp, 2, 8);
                            _owner.StatusEffects.ApplyEffect(StatusEffectType.DefenseUp, 2, 4);
                            _owner.ApplyStatModifiers();
                            _thresholdTriggered = true;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 상대에게 데미지를 입혔을 때 발동 (Corrosive 방어감소 디버프 / Venomous 독 부여)
        /// </summary>
        public void OnDamageDealtTo(Character target)
        {
            if (target == null || target.IsDead) return;

            switch (_trait)
            {
                case EnemyTrait.Corrosive:
                    target.StatusEffects.ApplyEffect(StatusEffectType.DefenseDown, 2, 2);
                    target.ApplyStatModifiers();
                    break;

                // Phase GF: Venomous — 공격 적중 시 Poison 1 (2턴) 부여
                case EnemyTrait.Venomous:
                    target.StatusEffects.ApplyEffect(StatusEffectType.Poison, 2, 1);
                    break;
            }
        }

        /// <summary>
        /// Phase GF: Corpsebloom — 아군 사망 시 30% HP로 부활 (전투당 1회).
        /// 호출부(TurnManager/BattleSceneSetup)가 CombatEventBus.OnEnemyDied 구독에서
        /// 살아있는 모든 아군의 TryReviveAlly를 호출하여 첫 번째 성공자가 부활 처리.
        /// </summary>
        public bool TryReviveAlly(Character deadAlly)
        {
            if (_trait != EnemyTrait.Corpsebloom || _corpsebloomUsed) return false;
            if (!_owner.IsAlive) return false;          // 부활자 본인이 죽었으면 불가
            if (deadAlly == null || deadAlly.IsAlive) return false;  // 대상이 이미 살아있으면 불가

            _corpsebloomUsed = true;
            deadAlly.Health.Revive(0.3f);  // MaxHP의 30%로 부활
            return true;
        }

        /// <summary>
        /// Phase GF: Corpsebloom — 현재 부홬 가능 여부 (UI 표시/디버그용)
        /// </summary>
        public bool CanReviveAlly => _trait == EnemyTrait.Corpsebloom && !_corpsebloomUsed && _owner.IsAlive;

        /// <summary>
        /// 치명적 피해 시 사망 방지 (Immortal: HP 1로 생존, 1회)
        /// </summary>
        public bool PreventDeath()
        {
            if (_trait != EnemyTrait.Immortal || _immortalUsed)
                return false;

            _immortalUsed = true;
            return true;
        }

        /// <summary>
        /// Shell: 매 턴 첫 상태이상 무효화 여부
        /// </summary>
        public bool ShouldBlockEffect()
        {
            if (_trait != EnemyTrait.Shell || _shellUsedThisTurn)
                return false;

            _shellUsedThisTurn = true;
            return true;
        }

        // === Private helpers ===

        private void CheckThreshold(int atkBonus)
        {
            float hpRatio = (float)_owner.Health.CurrentHP / _owner.Health.MaxHP;
            if (hpRatio > 0.5f) return;

            _owner.Stats.AddModifier(StatType.ATK, atkBonus);
            _owner.ApplyStatModifiers();
            _thresholdTriggered = true;
        }

        private void ApplyPhaseShift(int turnNumber)
        {
            // 이전 턴에 적용한 스탯 제거
            if (_phaseWasAttack)
                _owner.Stats.AddModifier(StatType.ATK, -4);
            else if (turnNumber > 1)
                _owner.Stats.AddModifier(StatType.DEF, -4);

            // 홀수 턴: DEF +4, 짝수 턴: ATK +4
            if (turnNumber % 2 == 1)
            {
                _owner.Stats.AddModifier(StatType.DEF, 4);
                _phaseWasAttack = false;
            }
            else
            {
                _owner.Stats.AddModifier(StatType.ATK, 4);
                _phaseWasAttack = true;
            }

            _owner.ApplyStatModifiers();
        }

        private void ApplyRampage()
        {
            // 이전 턴에 피해를 입지 않았으면 누적
            if (!_wasDamagedThisTurn && _rampageStacks >= 0)
                _rampageStacks += 3;

            // 피해를 입었으면 초기화
            if (_wasDamagedThisTurn)
                _rampageStacks = 0;

            // 누적 ATK 보너스 적용
            if (_rampageStacks > 0)
            {
                _owner.Stats.AddModifier(StatType.ATK, _rampageStacks);
                _owner.ApplyStatModifiers();
            }

            _wasDamagedThisTurn = false;
        }
    }
}
