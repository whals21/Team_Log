using System.Collections.Generic;
using TeamLog.Map;
using TeamLog.Skill;

using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;
using SkillInstance = TeamLog.Characters.SkillInstance;
using SkillType = TeamLog.Characters.SkillType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;
using StatType = TeamLog.Characters.StatType;

// Phase BK: BehaviorKeyword는 TeamLog.Skill 네임스페이스
using BehaviorKeyword = TeamLog.Skill.BehaviorKeyword;
using BehaviorTag = TeamLog.Skill.BehaviorTag;
using BehaviorTagResolver = TeamLog.Skill.BehaviorTagResolver;

namespace TeamLog.Combat.Turn
{
    /// <summary>
    /// 스킬 실행 엔진 — 타겟별 스킬 적용, 증강 해석, 키워드 해석
    /// TurnManager가 대상을 결정한 후 이 클래스가 효과를 적용
    ///
    /// Phase ARCH-3: Attack 케이스는 _pipeline.ExecuteAttack으로 우회 (조립식 파이프라인).
    /// 기존 ExecuteAttack/ApplyTouchEffects 메서드는 더 이상 사용 안 함 (회귀 대비 유지).
    /// Heal/Buff/Debuff/Shield/Purify는 기존 로직 유지.
    /// </summary>
    public class SkillExecutor
    {
        private readonly List<Character> _playerParty;
        private readonly List<Character> _enemies;
        // Phase ARCH-3: 조립식 파이프라인 — Attack 케이스를 위임
        private readonly SkillExecutionPipeline _pipeline;
        // Phase BK: 직전 타격의 실제 데미지 (Lifesteal 회복량 계산용) — 구 로직 잔여, Pipeline 사용 시 미사용
        private int lastActualDamage;

        /// <summary>
        /// 스킬 효과 적용 후 이벤트 — 스킬 타입별 사운드/VFX 분기용
        /// </summary>
        public static event System.Action<SkillData, Character> OnSkillApplied;

        public SkillExecutor(List<Character> playerParty, List<Character> enemies)
        {
            _playerParty = playerParty;
            _enemies = enemies;
            _pipeline = new SkillExecutionPipeline(playerParty, enemies);
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
                    // Phase ARCH-3: 조립식 파이프라인으로 우회 (Behavior 자동 처리 + 글로벌 훅 포함)
                    // 기존 ExecuteAttack + ApplyTouchEffects를 Pipeline이 통합 담당.
                    _pipeline.ExecuteAttack(caster, skill, target, instance, powerMultiplier);
                    break;
                case SkillType.Heal:
                    ExecuteHeal(caster, target, skill, instance, powerMultiplier);
                    break;
                case SkillType.Buff:
                    ApplyEffect(caster, skill, target, instance);
                    break;
                case SkillType.Debuff:
                    ApplyEffect(caster, skill, target, instance);
                    break;
                case SkillType.Shield:
                    ExecuteShield(caster, target, skill, instance);
                    break;
                case SkillType.Purify:
                    ExecutePurify(target);
                    break;
            }

            // Phase ARCH-3: Touch 계열은 Pipeline.ExecuteAttack 내부 PostDamage Phase에서 처리.
            // 비-Attack 타입이거나 Pipeline 미사용 시를 대비한 폴백 (현재는 Pipeline이 담당하므로 미호출).
            // 기존 ApplyTouchEffects 호출 제거 — Pipeline이 BehaviorRegistry를 통해 자동 호출.

            OnSkillApplied?.Invoke(skill, target);
            CombatEventBus.FireSkillUsed(skill, caster);
        }

        /// <summary>
        /// Phase BK: Touch 계열 행동 키워드 적용 — VenomTouch/BurningTouch/FreezeTouch.
        /// rank가 스택 수. 위력은 고정값 사용 (스킬 본체 위력의 30%, 최소 1).
        /// </summary>
        private static void ApplyTouchEffects(IReadOnlyList<BehaviorTag> behaviors, Character target)
        {
            if (behaviors == null || target == null || !target.IsAlive) return;

            int venomStacks = BehaviorTagResolver.RankSum(behaviors, BehaviorKeyword.VenomTouch);
            int burnStacks = BehaviorTagResolver.RankSum(behaviors, BehaviorKeyword.BurningTouch);
            int freezeStacks = BehaviorTagResolver.RankSum(behaviors, BehaviorKeyword.FreezeTouch);

            if (venomStacks > 0)
                target.StatusEffects.ApplyEffect(StatusEffectType.Poison, venomStacks, venomStacks);
            if (burnStacks > 0)
                target.StatusEffects.ApplyEffect(StatusEffectType.Burn, burnStacks, burnStacks);
            if (freezeStacks > 0)
                target.StatusEffects.ApplyEffect(StatusEffectType.Freeze, freezeStacks, freezeStacks);
        }

        private void ExecuteAttack(Character caster, Character target, SkillData skill,
            SkillInstance instance = null, float powerMultiplier = 1f)
        {
            int basePower = instance != null ? instance.EffectivePower : skill.Power;
            IReadOnlyList<BehaviorTag> behaviors = instance?.GetCombinedBehaviors();

            // Phase BK: Berserk 행동 키워드 — HP 절반 이하일 때 위력 2배.
            // (기존에는 KeywordType.PowerMul + HPBelow 0.5 키워드로 구현됐으나, 행동으로 승격)
            if (behaviors != null && BehaviorTagResolver.Has(behaviors, BehaviorKeyword.Berserk)
                && caster.Health.MaxHP > 0
                && caster.Health.CurrentHP * 2 <= caster.Health.MaxHP)
            {
                basePower *= 2;
            }

            // 키워드: PowerMul with HPBelow — 조건부 위력 배율 (EffectivePower는 Passive만 적용)
            if (instance != null)
            {
                var kw = instance.GetAllKeywords();
                float conditionalMul = KeywordResolver.SumConditional(kw, KeywordType.PowerMul,
                    caster.Health.CurrentHP, caster.Health.MaxHP);
                if (conditionalMul > 0f)
                    basePower = System.Math.Max(1, (int)(basePower * conditionalMul));
            }

            // 유물 PowerMul (Passive) — 시전자 기준
            float relicPowerMul = GetAllKeywordMul(caster, KeywordType.PowerMul);
            if (relicPowerMul > 0f && relicPowerMul != 1f)
                basePower = System.Math.Max(1, (int)(basePower * relicPowerMul));

            int power = System.Math.Max(1, (int)(basePower * powerMultiplier));

            // 유물: OnEnemyLowHP PowerMul (F3 ExecutionerBlade 등)
            var relicHandler = GameRunState.Instance?.RelicHandler;
            if (relicHandler != null)
            {
                float lowHPMul = relicHandler.GetEnemyLowHPPowerMul(target.Health.CurrentHP, target.Health.MaxHP);
                if (lowHPMul > 1f)
                    power = (int)(power * lowHPMul);
            }

            // Phase 8C: 장착 특성 OnEnemyLowHP PowerMul (궁수 약점 포착 등)
            if (caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
            {
                float traitLowHPMul = caster.PlayerTraitHandler.GetEnemyLowHPPowerMul(
                    target.Health.CurrentHP, target.Health.MaxHP);
                if (traitLowHPMul > 1f)
                    power = (int)(power * traitLowHPMul);
            }

            // 유물: 일시적 "다음 공격 강화" 버프 소비 (B2 AegisStrike, C3 MercyBlade 등)
            if (relicHandler != null)
            {
                int nextBonus = relicHandler.ConsumeNextAttackBonus();
                if (nextBonus > 0)
                    power += nextBonus;
            }

            // Phase 8C: 장착 특성 일시적 버프 소비
            if (caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
            {
                int traitBonus = caster.PlayerTraitHandler.ConsumeNextAttackBonus();
                if (traitBonus > 0)
                    power += traitBonus;
            }

            // 키워드: DamageTakenMul — 대상이 이 키워드 보유 시 받는 피해 배율 (스킬+유물)
            float takenMul = GetAllKeywordMul(target, KeywordType.DamageTakenMul);
            if (takenMul > 0f && takenMul != 1f)
                power = (int)(power * takenMul);

            // Phase BK: Pierce 행동 키워드 — 쉴드 + 방어 완전 무시 (DEF=0).
            if (behaviors != null && BehaviorTagResolver.Has(behaviors, BehaviorKeyword.Pierce))
            {
                int pierceDamage = System.Math.Max(1, caster.Stats.GetStat(StatType.ATK) + power);
                int hpBefore = target.Health.CurrentHP;
                target.Health.TakeDirectDamage(pierceDamage);
                int actualDealt = hpBefore - target.Health.CurrentHP;

                CombatEventBus.FireDamageDealt(caster, target, actualDealt);
                CombatEventBus.FireDamageReceived(target, actualDealt);

                // Lifesteal은 밖에서 공통 처리하도록 raw 데미지 기록
                lastActualDamage = actualDealt;
            }
            else
            {
                int hpBefore = target.Health.CurrentHP;
                DamageCalculator.DealDamage(caster, target, power);
                lastActualDamage = hpBefore - target.Health.CurrentHP;
            }

            // Phase BK: Execution 행동 키워드 — HP rank 이하 적 즉사 (보스 제외).
            var exec = behaviors != null
                ? BehaviorTagResolver.First(behaviors, BehaviorKeyword.Execution)
                : (BehaviorTag?)null;
            if (exec.HasValue && target.IsAlive && !target.Data.IsBoss
                && target.Health.CurrentHP <= exec.Value.Rank)
            {
                int execDamage = target.Health.CurrentHP;
                target.Health.TakeDirectDamage(execDamage);
                CombatEventBus.FireDamageDealt(caster, target, execDamage);
                CombatEventBus.FireDamageReceived(target, execDamage);
            }

            // Phase BK: Lifesteal 행동 키워드 — 준 데미지 절반 회복.
            if (behaviors != null && BehaviorTagResolver.Has(behaviors, BehaviorKeyword.Lifesteal)
                && caster.IsAlive && lastActualDamage > 0)
            {
                int healAmount = System.Math.Max(1, lastActualDamage / 2);
                caster.Health.Heal(healAmount);
                CombatEventBus.FireHealApplied(caster, healAmount);
            }

            // 키워드: DamageDealtHealPercent — 준 데미지의 % 회복 (유물 전용, Aug_Drain 폐지 후에도 유지)
            if (caster.IsAlive)
            {
                float healPercent = GetAllKeywordSum(caster, KeywordType.DamageDealtHealPercent);
                if (healPercent > 0f)
                {
                    int healAmount = System.Math.Max(1, (int)(power * healPercent));
                    caster.Health.Heal(healAmount);
                    CombatEventBus.FireHealApplied(caster, healAmount);
                }
            }

            // Phase BK: Chain 행동 키워드 — 무작위 N명 연쇄 (rank = 연쇄 대상 수).
            if (behaviors != null)
            {
                int chainCount = BehaviorTagResolver.RankSum(behaviors, BehaviorKeyword.Chain);
                if (chainCount > 0)
                {
                    var others = _enemies.FindAll(e => e.IsAlive && e != target);
                    for (int i = 0; i < chainCount && others.Count > 0; i++)
                    {
                        int idx = UnityEngine.Random.Range(0, others.Count);
                        DamageCalculator.DealDamage(caster, others[idx], power);
                        if (!others[idx].IsAlive) others.RemoveAt(idx);
                    }
                }
            }

            // 키워드: OnKillHeal — 대상 사망 시 시전자 HP 회복
            if (target.IsDead && caster.IsAlive)
            {
                int killHeal = GetAllKeywordSum(caster, KeywordType.OnKillHeal);
                // OnKill 트리거 유물(VampireFang 등)은 GetAllKeywordSum이 Passive만 합산하므로 별도 조회
                if (relicHandler != null)
                    killHeal += relicHandler.GetOnKillHealValue();
                // Phase 8C: 장착 특성 OnKillHeal (힐러 순수 치유 등)
                if (caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
                    killHeal += caster.PlayerTraitHandler.GetOnKillHealValue();
                if (killHeal > 0)
                {
                    caster.Health.Heal(killHeal);
                    CombatEventBus.FireHealApplied(caster, killHeal);
                }
            }
        }

        private void ExecuteHeal(Character caster, Character target, SkillData skill, SkillInstance instance = null,
            float powerMultiplier = 1f)
        {
            float multiplier = powerMultiplier;

            // 키워드: HealMul (시전자의 스킬 + 유물)
            multiplier *= GetAllKeywordMul(caster, KeywordType.HealMul);

            int amount = System.Math.Max(1, (int)((skill.Power) * multiplier));
            target.Health.Heal(amount);
            CombatEventBus.FireHealApplied(target, amount);
        }

        private void ApplyEffect(Character caster, SkillData skill, Character target, SkillInstance instance = null)
        {
            if (skill.StatusEffect != StatusEffectType.None)
            {
                // Shell 특성: 매 턴 첫 상태이상 무효화
                if (target.TraitHandler.ShouldBlockEffect())
                    return;

                int duration = skill.EffectDuration;
                int value = skill.EffectValue;

                // 키워드: DurationAdd (증강)
                if (instance != null)
                    duration += (int)KeywordResolver.SumKeyword(instance.GetAllKeywords(), KeywordType.DurationAdd);

                // Phase 8C: 장착 특성 DurationAdd (도적 독 마스터, 연금술사 독성 폭발)
                if (caster != null && caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
                    duration += caster.PlayerTraitHandler.QueryKeywordSum(KeywordType.DurationAdd);

                // 키워드: EffectMul (증강)
                float effectMul = 1f;
                if (instance != null)
                    effectMul *= KeywordResolver.MulKeyword(instance.GetAllKeywords(), KeywordType.EffectMul);
                // Phase 8C: 장착 특성 EffectMul (네크로맨서 저주의 대가)
                if (caster != null && caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
                    effectMul *= caster.PlayerTraitHandler.QueryKeywordMul(KeywordType.EffectMul);
                value = System.Math.Max(1, (int)(value * effectMul));

                target.StatusEffects.ApplyEffect(skill.StatusEffect, duration, value);
                target.ApplyStatModifiers();
            }
        }

        private void ExecuteShield(Character caster, Character target, SkillData skill, SkillInstance instance = null)
        {
            float multiplier = 1f;

            // 키워드: ShieldMul (시전자의 스킬 + 유물)
            multiplier = GetAllKeywordMul(caster, KeywordType.ShieldMul);

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
        /// 캐릭터의 모든 스킬 키워드에서 지정 타입 합산 값 반환 (증강만)
        /// </summary>
        public static int GetKeywordSumForCharacter(Character character, KeywordType type)
        {
            int total = 0;
            foreach (var inst in character.SkillInventory.SkillInstances)
                total += (int)KeywordResolver.SumKeyword(inst.GetAllKeywords(), type);
            return total;
        }

        /// <summary>
        /// 캐릭터의 모든 스킬 + 유물 + 장착 특성 키워드에서 지정 타입 합산
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
            // Phase 8C: 장착 특성 키워드 합산
            if (character.PlayerTraitHandler != null && character.PlayerTraitHandler.HasTrait)
                total += character.PlayerTraitHandler.QueryKeywordSum(type);
            return total;
        }

        /// <summary>
        /// 캐릭터의 모든 스킬 + 유물 + 장착 특성 키워드에서 지정 타입 곱 배율 반환
        /// </summary>
        public static float GetAllKeywordMul(Character character, KeywordType type)
        {
            float result = 1f;
            foreach (var inst in character.SkillInventory.SkillInstances)
                result *= KeywordResolver.MulKeyword(inst.GetAllKeywords(), type);
            // 유물 키워드 배율 추가
            var relicHandler = GameRunState.Instance?.RelicHandler;
            if (relicHandler != null)
            {
                foreach (var relic in relicHandler.Relics)
                {
                    if (relic.Keywords == null) continue;
                    foreach (var kw in relic.Keywords)
                    {
                        if (kw.Type == type && kw.Trigger == KeywordTrigger.Passive)
                            result *= kw.Value;
                    }
                }
            }
            // Phase 8C: 장착 특성 키워드 배율 추가
            if (character.PlayerTraitHandler != null && character.PlayerTraitHandler.HasTrait)
                result *= character.PlayerTraitHandler.QueryKeywordMul(type);
            return result;
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
