using System.Collections.Generic;
using TeamLog.Map;
using TeamLog.Skill;

using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;
using SkillInstance = TeamLog.Characters.SkillInstance;
using SkillType = TeamLog.Characters.SkillType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;
using StatType = TeamLog.Characters.StatType;

namespace TeamLog.Combat.Turn
{
    /// <summary>
    /// 스킬 실행 엔진 — 타겟별 스킬 적용, 증강 해석, 키워드 해석
    /// TurnManager가 대상을 결정한 후 이 클래스가 효과를 적용
    /// </summary>
    public class SkillExecutor
    {
        private readonly List<Character> _playerParty;
        private readonly List<Character> _enemies;

        /// <summary>
        /// 스킬 효과 적용 후 이벤트 — 스킬 타입별 사운드/VFX 분기용
        /// </summary>
        public static event System.Action<SkillData, Character> OnSkillApplied;

        public SkillExecutor(List<Character> playerParty, List<Character> enemies)
        {
            _playerParty = playerParty;
            _enemies = enemies;
        }

        /// <summary>
        /// 단일 대상에게 스킬 효과 적용 (타겟 분류는 TurnManager가 담당)
        /// </summary>
        public void ExecuteSkillInternal(Character caster, SkillData skill, Character target,
            SkillInstance instance = null, float powerMultiplier = 1f)
        {
            switch (skill.Type)
            {
                case SkillType.Attack:
                    ExecuteAttack(caster, target, skill, instance, powerMultiplier);
                    break;
                case SkillType.Heal:
                    ExecuteHeal(target, skill, instance, powerMultiplier);
                    break;
                case SkillType.Buff:
                    ApplyEffect(skill, target, instance);
                    break;
                case SkillType.Debuff:
                    ApplyEffect(skill, target, instance);
                    break;
                case SkillType.Shield:
                    ExecuteShield(target, skill, instance);
                    break;
                case SkillType.Purify:
                    ExecutePurify(target);
                    break;
            }

            // VenomTouch / BurningTouch 증강: 공격 스킬에 추가 상태이상
            if (skill.Type == SkillType.Attack && instance != null)
            {
                if (instance.HasAugment(AugmentType.VenomTouch) && target.IsAlive)
                    target.StatusEffects.ApplyEffect(StatusEffectType.Poison, 2, System.Math.Max(1, (int)(skill.Power * 0.3f)));
                if (instance.HasAugment(AugmentType.BurningTouch) && target.IsAlive)
                    target.StatusEffects.ApplyEffect(StatusEffectType.Burn, 2, System.Math.Max(1, (int)(skill.Power * 0.3f)));
            }

            OnSkillApplied?.Invoke(skill, target);
            CombatEventBus.FireSkillUsed(skill, caster);
        }

        private void ExecuteAttack(Character caster, Character target, SkillData skill,
            SkillInstance instance = null, float powerMultiplier = 1f)
        {
            int basePower = instance != null ? instance.EffectivePower : skill.Power;

            // 키워드: PowerMul with HPBelow — 조건부 위력 배율 (EffectivePower는 Passive만 적용)
            if (instance != null)
            {
                var kw = instance.GetAllKeywords();
                float conditionalMul = KeywordResolver.SumConditional(kw, KeywordType.PowerMul,
                    caster.Health.CurrentHP, caster.Health.MaxHP);
                if (conditionalMul > 0f)
                    basePower = System.Math.Max(1, (int)(basePower * conditionalMul));
            }

            int power = System.Math.Max(1, (int)(basePower * powerMultiplier));

            // 키워드: DamageTakenMul — 대상이 이 키워드 보유 시 받는 피해 배율
            float takenMul = GetKeywordMulForCharacter(target, KeywordType.DamageTakenMul);
            if (takenMul > 0f && takenMul != 1f)
                power = (int)(power * takenMul);

            // Pierce: 쉴드 무시 + 방어력 50% 무시
            if (instance != null && instance.HasAugment(AugmentType.Pierce))
            {
                int atk = caster.Stats.GetStat(StatType.ATK) + power;
                int defense = target.Stats.GetStat(StatType.DEF) / 2;
                target.Health.TakeDamage(System.Math.Max(1, atk - defense));

                // 유물 트리거
                CombatEventBus.FireDamageDealt(caster, target, System.Math.Max(1, atk - defense));
                CombatEventBus.FireDamageReceived(target, System.Math.Max(1, atk - defense));
            }
            else
            {
                DamageCalculator.DealDamage(caster, target, power);
            }

            // 키워드: DamageDealtHealPercent — 준 데미지의 % 회복
            if (instance != null && caster.IsAlive)
            {
                var kw = instance.GetAllKeywords();
                float healPercent = KeywordResolver.SumKeyword(kw, KeywordType.DamageDealtHealPercent);
                if (healPercent > 0f)
                {
                    int healAmount = System.Math.Max(1, (int)(power * healPercent));
                    caster.Health.Heal(healAmount);
                    CombatEventBus.FireHealApplied(caster, healAmount);
                }
            }

            // Chain: 타격 후 인접 적에게 위력 50% 연쇄
            if (instance != null && instance.HasAugment(AugmentType.Chain))
            {
                foreach (var enemy in _enemies)
                {
                    if (enemy.IsAlive && enemy != target)
                    {
                        int chainPower = System.Math.Max(1, power / 2);
                        DamageCalculator.DealDamage(caster, enemy, chainPower);
                        break; // 한 명에게만 연쇄
                    }
                }
            }

            // 키워드: OnKillHeal — 대상 사망 시 시전자 HP 회복
            if (target.IsDead && caster.IsAlive)
            {
                int killHeal = GetAllKeywordSum(caster, KeywordType.OnKillHeal);
                if (killHeal > 0)
                {
                    caster.Health.Heal(killHeal);
                    CombatEventBus.FireHealApplied(caster, killHeal);
                }
            }
        }

        private void ExecuteHeal(Character target, SkillData skill, SkillInstance instance = null,
            float powerMultiplier = 1f)
        {
            float multiplier = powerMultiplier;

            // 키워드: HealMul
            if (instance != null)
                multiplier *= KeywordResolver.MulKeyword(instance.GetAllKeywords(), KeywordType.HealMul);

            int amount = System.Math.Max(1, (int)((skill.Power) * multiplier));
            target.Health.Heal(amount);
            CombatEventBus.FireHealApplied(target, amount);
        }

        private void ApplyEffect(SkillData skill, Character target, SkillInstance instance = null)
        {
            if (skill.StatusEffect != StatusEffectType.None)
            {
                // Shell 특성: 매 턴 첫 상태이상 무효화
                if (target.TraitHandler.ShouldBlockEffect())
                    return;

                int duration = skill.EffectDuration;
                int value = skill.EffectValue;

                // 키워드: DurationAdd
                if (instance != null)
                    duration += (int)KeywordResolver.SumKeyword(instance.GetAllKeywords(), KeywordType.DurationAdd);

                // 키워드: EffectMul
                if (instance != null)
                    value = System.Math.Max(1, (int)(value * KeywordResolver.MulKeyword(instance.GetAllKeywords(), KeywordType.EffectMul)));

                target.StatusEffects.ApplyEffect(skill.StatusEffect, duration, value);
                target.ApplyStatModifiers();
            }
        }

        private void ExecuteShield(Character target, SkillData skill, SkillInstance instance = null)
        {
            float multiplier = 1f;

            // 키워드: ShieldMul
            if (instance != null)
                multiplier = KeywordResolver.MulKeyword(instance.GetAllKeywords(), KeywordType.ShieldMul);

            int amount = System.Math.Max(1, (int)(skill.Power * multiplier));
            target.Health.AddShield(amount);
            CombatEventBus.FireShieldGained(target, amount);
        }

        private void ExecutePurify(Character target)
        {
            target.StatusEffects.ClearAllEffects();
            target.ApplyStatModifiers();
        }

        // ── 키워드 헬퍼 ──

        /// <summary>
        /// 캐릭터의 모든 스킬 키워드에서 지정 타입 합산 값 반환
        /// </summary>
        public static int GetKeywordSumForCharacter(Character character, KeywordType type)
        {
            int total = 0;
            foreach (var inst in character.SkillInventory.SkillInstances)
                total += (int)KeywordResolver.SumKeyword(inst.GetAllKeywords(), type);
            return total;
        }

        /// <summary>
        /// 캐릭터의 모든 스킬 키워드에서 지정 타입 곱 배율 반환
        /// </summary>
        public static float GetKeywordMulForCharacter(Character character, KeywordType type)
        {
            float result = 1f;
            foreach (var inst in character.SkillInventory.SkillInstances)
                result *= KeywordResolver.MulKeyword(inst.GetAllKeywords(), type);
            return result;
        }

        /// <summary>
        /// 캐릭터의 모든 스킬 + 유물 키워드에서 지정 타입 합산
        /// </summary>
        public static int GetAllKeywordSum(Character character, KeywordType type)
        {
            int total = GetKeywordSumForCharacter(character, type);
            // 유물 키워드도 합산
            var relicHandler = GameRunState.Instance?.RelicHandler;
            if (relicHandler != null)
            {
                foreach (var relic in relicHandler.Relics)
                {
                    if (relic.Keywords == null) continue;
                    foreach (var kw in relic.Keywords)
                    {
                        if (kw.Type == type && kw.Trigger == KeywordTrigger.Passive)
                            total += (int)kw.Value;
                    }
                }
            }
            return total;
        }

        /// <summary>
        /// 정적 이벤트 정리 — 전투 종료 시 호출하여 람다 누적 방지
        /// </summary>
        public static void ClearEvents()
        {
            OnSkillApplied = null;
        }
    }
}
