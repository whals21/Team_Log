using System.Collections.Generic;
using TeamLog.Skill;
using TeamLog.Skill.Behaviors;
using TeamLog.Map;
using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;
using SkillInstance = TeamLog.Characters.SkillInstance;
using SkillType = TeamLog.Characters.SkillType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;

namespace TeamLog.Combat.Turn
{
    /// <summary>
    /// 스킬 실행 파이프라인 (통합 파이프라인 — 2026-07-02).
    ///
    /// ★ 완전 통일 파이프라인:
    /// 모든 스킬 타입(Attack/Heal/Shield/Buff/Debuff/Purify)이 동일한 5 Phase를 거친다.
    /// 타입별 차이는 ApplyMain Phase의 Default 헬퍼(ApplyDefaultByType)에서만 처리.
    /// 새 효과(정화, 임계값 쉴드 등)는 Behavior만 추가하면 됨 — Pipeline/SkillExecutor 코드 수정 0줄.
    ///
    /// Phase 순서: PowerModify → TargetModify → ApplyMain → PostApply → OnKill
    /// </summary>
    public class SkillExecutionPipeline
    {
        private readonly List<Character> _playerParty;
        private readonly List<Character> _enemies;

        public SkillExecutionPipeline(List<Character> playerParty, List<Character> enemies)
        {
            _playerParty = playerParty;
            _enemies = enemies;
        }

        /// <summary>
        /// 모든 스킬 타입의 통합 진입점.
        /// 타입 불문 동일한 Phase 순서로 실행. Open-Closed 원칙 달성.
        /// </summary>
        public void ExecuteSkill(Character caster, SkillData skill, Character target,
            SkillInstance instance = null, float powerMultiplier = 1f, TurnContext turnCtx = null)
        {
            if (caster == null || target == null) return;

            // Context 초기화
            var ctx = new SkillExecContext
            {
                Caster = caster,
                InitialTarget = target,
                Skill = skill,
                Instance = instance,
                TurnCtx = turnCtx,
                PlayerParty = _playerParty,
                Enemies = _enemies,
                PowerMultiplier = powerMultiplier,
            };

            IReadOnlyList<BehaviorTag> tags = instance?.GetCombinedBehaviors() ?? skill.Behaviors;
            bool isAttack = skill.Type == SkillType.Attack;

            // ═══════════════════════════════════════════
            // Phase 1: PowerModify
            // ═══════════════════════════════════════════

            int basePower = instance != null ? instance.EffectivePower : skill.Power;
            ctx.CurrentPower = System.Math.Max(1, basePower);

            // 1A. Behavior PowerModify (Berserk, FirstBlood, TargetFreeze 등)
            foreach (var b in BehaviorRegistry.GetForPhase(tags, ExecutionPhase.PowerModify))
                ctx.CurrentPower = b.ModifyPower(ctx.CurrentPower, ctx);

            // 1B. Phase CC 자원 비례 위력 (모든 타입에 적용 — Brand of Ash, Phoenix Renewal 등)
            if (caster.Resource != null && skill.ResourcePowerPerStack > 0)
                ctx.CurrentPower += caster.Resource.CurrentStacks * skill.ResourcePowerPerStack;

            // 1C. 타입별 키워드 배율 (Attack=PowerMul, Heal=HealMul, Shield=ShieldMul)
            float typeMul = GetTypeSpecificMul(caster, skill.Type);
            ctx.CurrentPower = System.Math.Max(1, (int)(ctx.CurrentPower * powerMultiplier * typeMul));

            // 1D. Attack 전용 글로벌 훅 (대상 기준 PowerMul + 일시적 버프)
            if (isAttack)
                ApplyAttackGlobalPowerModifiers(ctx, caster, target, instance);

            // ═══════════════════════════════════════════
            // Phase 2: TargetModify
            // (TurnManager가 타겟팅 분해를 담당하므로 여기서는 단일 타겟 유지.
            //  향후 Spread/Bounce/MultiHit 이관 시 이 자리에서 Behavior 호출.)
            // ═══════════════════════════════════════════
            ctx.CurrentTargets = new List<Character> { target };
            foreach (var b in BehaviorRegistry.GetForPhase(tags, ExecutionPhase.TargetModify))
                ctx.CurrentTargets = b.ModifyTargets(ctx.CurrentTargets, ctx);

            // ═══════════════════════════════════════════
            // Phase 3+4+5: 각 타겟별 본 효과 → 후처리 → 킬 처리
            // ═══════════════════════════════════════════
            var applyMainBehaviors = BehaviorRegistry.GetForPhase(tags, ExecutionPhase.ApplyMain);
            var postApplyBehaviors = BehaviorRegistry.GetForPhase(tags, ExecutionPhase.PostApply);
            var onKillBehaviors = BehaviorRegistry.GetForPhase(tags, ExecutionPhase.OnKill);

            var relicHandler = GameRunState.Instance?.RelicHandler;

            foreach (var currentTarget in ctx.CurrentTargets)
            {
                if (currentTarget == null || !currentTarget.IsAlive) continue;

                ctx.InitialTarget = currentTarget;
                ctx.LastActualDamage = 0;
                ctx.SkipDefaultApply = false;

                // ── Phase 3: ApplyMain ──
                // 커스텀 Behavior (Pierce 등)
                foreach (var b in applyMainBehaviors)
                    b.ApplyMain(ctx);

                // Default 본 효과 (타입별 — Attack/Heal/Shield/Buff/Debuff/Purify)
                if (!ctx.SkipDefaultApply)
                    ApplyDefaultByType(ctx, caster, currentTarget, skill, instance);

                // Attack 스킬의 StatusEffect 부여 (Wire-Charge 등 — 메인 타겟)
                if (isAttack && skill.StatusEffect != StatusEffectType.None && currentTarget.IsAlive)
                    ApplyStatusEffect(ctx, caster, skill, currentTarget, instance);

                // 글로벌 훅: DamageDealtHealPercent (Attack 전용)
                if (isAttack && caster.IsAlive)
                {
                    float healPercent = SkillExecutor.GetAllKeywordSum(caster, KeywordType.DamageDealtHealPercent);
                    if (healPercent > 0f)
                    {
                        int healAmount = System.Math.Max(1, (int)(ctx.CurrentPower * healPercent));
                        caster.Health.Heal(healAmount);
                        CombatEventBus.FireHealApplied(caster, healAmount);
                    }
                }

                // ── Phase 4: PostApply ──
                foreach (var b in postApplyBehaviors)
                    b.OnPostApply(ctx);

                // ── Phase 5: OnKill ──
                if (currentTarget.IsDead)
                {
                    ctx.KilledTargets.Add(currentTarget);

                    foreach (var b in onKillBehaviors)
                        b.OnKill(ctx);

                    // 글로벌 훅: 유물/특성 OnKillHeal
                    if (caster.IsAlive)
                        ApplyOnKillHeal(ctx, caster, relicHandler);
                }
            }
        }

        // ═══════════════════════════════════════════
        // 헬퍼: 타입별 Default 본 효과
        // ═══════════════════════════════════════════

        private static void ApplyDefaultByType(SkillExecContext ctx, Character caster,
            Character target, SkillData skill, SkillInstance instance)
        {
            switch (skill.Type)
            {
                case SkillType.Attack:
                    ApplyDefaultDamage(ctx, caster, target);
                    break;
                case SkillType.Heal:
                    ApplyDefaultHeal(ctx, caster, target);
                    break;
                case SkillType.Shield:
                    ApplyDefaultShield(ctx, caster, target, skill);
                    break;
                case SkillType.Buff:
                case SkillType.Debuff:
                    ApplyDefaultEffect(ctx, caster, target, skill, instance);
                    break;
                case SkillType.Purify:
                    ApplyDefaultPurify(target);
                    break;
            }
        }

        private static void ApplyDefaultDamage(SkillExecContext ctx, Character caster, Character target)
        {
            int hpBefore = target.Health.CurrentHP;
            DamageCalculator.DealDamage(caster, target, ctx.CurrentPower);
            ctx.LastActualDamage = hpBefore - target.Health.CurrentHP;

            // Phase CC: 피격 시 자원 훅 (Duran Vengeance)
            if (target.Resource != null && ctx.LastActualDamage > 0)
                target.Resource.OnDamageTaken(target, ctx.LastActualDamage);
        }

        private static void ApplyDefaultHeal(SkillExecContext ctx, Character caster, Character target)
        {
            target.Health.Heal(ctx.CurrentPower);
            CombatEventBus.FireHealApplied(target, ctx.CurrentPower);
        }

        private static void ApplyDefaultShield(SkillExecContext ctx, Character caster,
            Character target, SkillData skill)
        {
            target.Health.AddShield(caster, ctx.CurrentPower, skill.ShieldFlags);
            CombatEventBus.FireShieldGained(target, ctx.CurrentPower);
        }

        private static void ApplyDefaultEffect(SkillExecContext ctx, Character caster,
            Character target, SkillData skill, SkillInstance instance)
        {
            if (skill.StatusEffect == StatusEffectType.None) return;

            // Shell 특성: 매 턴 첫 상태이상 무효화
            if (target.TraitHandler.ShouldBlockEffect()) return;

            int duration = skill.EffectDuration;
            int value = skill.EffectValue;

            // 증강 DurationAdd
            if (instance != null)
                duration += (int)KeywordResolver.SumKeyword(instance.GetAllKeywords(), KeywordType.DurationAdd);
            // 장착 특성 DurationAdd
            if (caster != null && caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
                duration += caster.PlayerTraitHandler.QueryKeywordSum(KeywordType.DurationAdd);

            // 증강 EffectMul
            float effectMul = 1f;
            if (instance != null)
                effectMul *= KeywordResolver.MulKeyword(instance.GetAllKeywords(), KeywordType.EffectMul);
            if (caster != null && caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
                effectMul *= caster.PlayerTraitHandler.QueryKeywordMul(KeywordType.EffectMul);
            value = System.Math.Max(1, (int)(value * effectMul));

            target.StatusEffects.ApplyEffect(skill.StatusEffect, duration, value);
            target.ApplyStatModifiers();
        }

        private static void ApplyDefaultPurify(Character target)
        {
            target.StatusEffects.ClearAllEffects();
            target.ApplyStatModifiers();
        }

        // ═══════════════════════════════════════════
        // 헬퍼: 위력 배율 / 글로벌 훅
        // ═══════════════════════════════════════════

        private static float GetTypeSpecificMul(Character caster, SkillType type)
        {
            switch (type)
            {
                case SkillType.Attack:
                    // 키워드 conditionalMul (PowerMul with HPBelow)
                    return 1f; // Attack은 별도 처리 (ApplyAttackGlobalPowerModifiers)
                case SkillType.Heal:
                    return SkillExecutor.GetAllKeywordMul(caster, KeywordType.HealMul);
                case SkillType.Shield:
                    return SkillExecutor.GetAllKeywordMul(caster, KeywordType.ShieldMul);
                default:
                    return 1f;
            }
        }

        private static void ApplyAttackGlobalPowerModifiers(SkillExecContext ctx, Character caster,
            Character target, SkillInstance instance)
        {
            // 키워드 conditionalMul (PowerMul with HPBelow)
            if (instance != null)
            {
                var kw = instance.GetAllKeywords();
                float conditionalMul = KeywordResolver.SumConditional(kw, KeywordType.PowerMul,
                    caster.Health.CurrentHP, caster.Health.MaxHP);
                if (conditionalMul > 0f)
                    ctx.CurrentPower = System.Math.Max(1, (int)(ctx.CurrentPower * conditionalMul));
            }

            // 유물 PowerMul
            float relicPowerMul = SkillExecutor.GetAllKeywordMul(caster, KeywordType.PowerMul);
            if (relicPowerMul > 0f && relicPowerMul != 1f)
                ctx.CurrentPower = System.Math.Max(1, (int)(ctx.CurrentPower * relicPowerMul));

            var relicHandler = GameRunState.Instance?.RelicHandler;

            // 유물 OnEnemyLowHP PowerMul (대상 기준)
            if (relicHandler != null)
            {
                float lowHPMul = relicHandler.GetEnemyLowHPPowerMul(target.Health.CurrentHP, target.Health.MaxHP);
                if (lowHPMul > 1f)
                    ctx.CurrentPower = (int)(ctx.CurrentPower * lowHPMul);
            }

            // 장착 특성 OnEnemyLowHP PowerMul
            if (caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
            {
                float traitLowHPMul = caster.PlayerTraitHandler.GetEnemyLowHPPowerMul(
                    target.Health.CurrentHP, target.Health.MaxHP);
                if (traitLowHPMul > 1f)
                    ctx.CurrentPower = (int)(ctx.CurrentPower * traitLowHPMul);
            }

            // 유물 "다음 공격 강화" 버프 소비
            if (relicHandler != null)
            {
                int nextBonus = relicHandler.ConsumeNextAttackBonus();
                if (nextBonus > 0)
                    ctx.CurrentPower += nextBonus;
            }

            // 장착 특성 일시적 버프 소비
            if (caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
            {
                int traitBonus = caster.PlayerTraitHandler.ConsumeNextAttackBonus();
                if (traitBonus > 0)
                    ctx.CurrentPower += traitBonus;
            }

            // 대상 DamageTakenMul
            float takenMul = SkillExecutor.GetAllKeywordMul(target, KeywordType.DamageTakenMul);
            if (takenMul > 0f && takenMul != 1f)
                ctx.CurrentPower = (int)(ctx.CurrentPower * takenMul);
        }

        private static void ApplyStatusEffect(SkillExecContext ctx, Character caster,
            SkillData skill, Character target, SkillInstance instance)
        {
            // 기존 ApplyEffectViaPipeline 로직 이관 (Wire-Charge 부여 등)
            ApplyDefaultEffect(ctx, caster, target, skill, instance);
        }

        private static void ApplyOnKillHeal(SkillExecContext ctx, Character caster,
            Reward.RelicHandler relicHandler)
        {
            int killHeal = SkillExecutor.GetAllKeywordSum(caster, KeywordType.OnKillHeal);
            if (relicHandler != null)
                killHeal += relicHandler.GetOnKillHealValue();
            if (caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
                killHeal += caster.PlayerTraitHandler.GetOnKillHealValue();
            if (killHeal > 0)
            {
                caster.Health.Heal(killHeal);
                CombatEventBus.FireHealApplied(caster, killHeal);
            }
        }
    }
}
