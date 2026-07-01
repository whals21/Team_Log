using System.Collections.Generic;
using TeamLog.Skill;
using TeamLog.Skill.Behaviors;
using TeamLog.Map;
using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;
using SkillInstance = TeamLog.Characters.SkillInstance;
using SkillType = TeamLog.Characters.SkillType;
using StatType = TeamLog.Characters.StatType;

namespace TeamLog.Combat.Turn
{
    /// <summary>
    /// 스킬 실행 파이프라인 (Phase ARCH).
    /// SkillExecutor.ExecuteAttack의 하드코딩 if문을 대체하는 조립식 실행 엔진.
    ///
    /// Phase ARCH-3 완료:
    /// - 기존 SkillExecutor.ExecuteAttack의 모든 로직(유물/특성/키워드 훅 포함) 이식
    /// - 5종 ARCH-2 Behavior(Berserk/Pierce/Execution/Lifesteal/Chain) + 3종 ARCH-3 Behavior(Touch) 자동 작동
    /// - SkillExecutor.ExecuteSkillInternal의 Attack 케이스가 Pipeline.ExecuteAttack 호출
    /// - 타겟팅 분해(Spread/Bounce/MultiHit/Explosion/AOEAuto)는 TurnManager가 계속 담당 (회귀 안전)
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
        /// Attack 타입 스킬 실행 — 기존 SkillExecutor.ExecuteAttack을 1:1 대체.
        /// Pipeline Phase 순서: PowerModify → TargetModify → DamageApply → PostDamage → OnKill.
        /// 각 Phase에서 BehaviorRegistry.GetForPhase로 조회한 Behavior들의 훅을 Order순 호출.
        /// 글로벌 훅(유물/특성/키워드)은 Pipeline 본체에서 Behavior 호출 사이에 개입.
        /// </summary>
        public void ExecuteAttack(Character caster, SkillData skill, Character target,
            SkillInstance instance = null, float powerMultiplier = 1f, TurnContext turnCtx = null)
        {
            if (caster == null || target == null || !target.IsAlive) return;

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

            // ═══════════════════════════════════════════
            // Phase 1: PowerModify
            // ═══════════════════════════════════════════

            // 기본 위력 (instance.EffectivePower가 키워드 기반 가산 포함)
            int basePower = instance != null ? instance.EffectivePower : skill.Power;
            ctx.CurrentPower = System.Math.Max(1, basePower);

            // 1A. Behavior PowerModify (Berserk 등 — 기존 if(Berserk) 블록)
            foreach (var b in BehaviorRegistry.GetForPhase(tags, ExecutionPhase.PowerModify))
                ctx.CurrentPower = b.ModifyPower(ctx.CurrentPower, ctx);

            // 1B. 키워드 conditionalMul (PowerMul with HPBelow) — 기존 line 114-122
            if (instance != null)
            {
                var kw = instance.GetAllKeywords();
                float conditionalMul = KeywordResolver.SumConditional(kw, KeywordType.PowerMul,
                    caster.Health.CurrentHP, caster.Health.MaxHP);
                if (conditionalMul > 0f)
                    ctx.CurrentPower = System.Math.Max(1, (int)(ctx.CurrentPower * conditionalMul));
            }

            // 1C. 유물 PowerMul (Passive) — 기존 line 124-128
            float relicPowerMul = SkillExecutor.GetAllKeywordMul(caster, KeywordType.PowerMul);
            if (relicPowerMul > 0f && relicPowerMul != 1f)
                ctx.CurrentPower = System.Math.Max(1, (int)(ctx.CurrentPower * relicPowerMul));

            // 1D. powerMultiplier 적용
            ctx.CurrentPower = System.Math.Max(1, (int)(ctx.CurrentPower * powerMultiplier));

            // 1E. 유물 OnEnemyLowHP PowerMul (대상 기준) — 기존 line 131-138
            var relicHandler = GameRunState.Instance?.RelicHandler;
            if (relicHandler != null)
            {
                float lowHPMul = relicHandler.GetEnemyLowHPPowerMul(target.Health.CurrentHP, target.Health.MaxHP);
                if (lowHPMul > 1f)
                    ctx.CurrentPower = (int)(ctx.CurrentPower * lowHPMul);
            }

            // 1F. 장착 특성 OnEnemyLowHP PowerMul — 기존 line 140-147
            if (caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
            {
                float traitLowHPMul = caster.PlayerTraitHandler.GetEnemyLowHPPowerMul(
                    target.Health.CurrentHP, target.Health.MaxHP);
                if (traitLowHPMul > 1f)
                    ctx.CurrentPower = (int)(ctx.CurrentPower * traitLowHPMul);
            }

            // 1G. 유물 "다음 공격 강화" 버프 소비 — 기존 line 149-155
            if (relicHandler != null)
            {
                int nextBonus = relicHandler.ConsumeNextAttackBonus();
                if (nextBonus > 0)
                    ctx.CurrentPower += nextBonus;
            }

            // 1H. 장착 특성 일시적 버프 소비 — 기존 line 157-163
            if (caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
            {
                int traitBonus = caster.PlayerTraitHandler.ConsumeNextAttackBonus();
                if (traitBonus > 0)
                    ctx.CurrentPower += traitBonus;
            }

            // 1I. 대상 DamageTakenMul — 기존 line 165-168
            float takenMul = SkillExecutor.GetAllKeywordMul(target, KeywordType.DamageTakenMul);
            if (takenMul > 0f && takenMul != 1f)
                ctx.CurrentPower = (int)(ctx.CurrentPower * takenMul);

            // ═══════════════════════════════════════════
            // Phase 2: TargetModify
            // (ARCH-3에서는 TurnManager가 타겟팅 분해를 담당하므로 단일 타겟 유지.
            //  향후 Spread/Bounce/MultiHit 등을 Behavior로 이관할 자리.)
            // ═══════════════════════════════════════════
            ctx.CurrentTargets = new List<Character> { target };
            foreach (var b in BehaviorRegistry.GetForPhase(tags, ExecutionPhase.TargetModify))
                ctx.CurrentTargets = b.ModifyTargets(ctx.CurrentTargets, ctx);

            // ═══════════════════════════════════════════
            // Phase 3+4+5: 각 타겟별 데미지 적용 → 후처리 → 킬 처리
            // ═══════════════════════════════════════════
            var damageApplyBehaviors = BehaviorRegistry.GetForPhase(tags, ExecutionPhase.DamageApply);
            var postDamageBehaviors = BehaviorRegistry.GetForPhase(tags, ExecutionPhase.PostDamage);
            var onKillBehaviors = BehaviorRegistry.GetForPhase(tags, ExecutionPhase.OnKill);

            foreach (var currentTarget in ctx.CurrentTargets)
            {
                if (currentTarget == null || !currentTarget.IsAlive) continue;

                ctx.InitialTarget = currentTarget;
                ctx.LastActualDamage = 0;
                ctx.SkipDefaultDamage = false;

                // ── Phase 3: DamageApply ──
                foreach (var b in damageApplyBehaviors)
                    b.ApplyDamage(ctx);

                if (!ctx.SkipDefaultDamage)
                    ApplyDefaultDamage(ctx, caster, currentTarget);

                // ── 글로벌 훅: 유물 DamageDealtHealPercent — 기존 line 213-223 ──
                // (Chain 등 PostDamage Behavior보다 먼저 적용해야 회복량 기준 power가 정확)
                // 주의: 기존엔 Lifesteal 후 Chain 전 순서였으나, healPercent는 power 기반이라
                // Behavior의 LastActualDamage와 무관. Pipeline에서는 PostDamage Behaviors 전에 적용.
                if (caster.IsAlive)
                {
                    float healPercent = SkillExecutor.GetAllKeywordSum(caster, KeywordType.DamageDealtHealPercent);
                    if (healPercent > 0f)
                    {
                        int healAmount = System.Math.Max(1, (int)(ctx.CurrentPower * healPercent));
                        caster.Health.Heal(healAmount);
                        CombatEventBus.FireHealApplied(caster, healAmount);
                    }
                }

                // ── Phase 4: PostDamage (Execution → Lifesteal → Touch 3종 → Chain) ──
                foreach (var b in postDamageBehaviors)
                    b.OnPostDamage(ctx);

                // ── Phase 5: OnKill ──
                if (currentTarget.IsDead)
                {
                    ctx.KilledTargets.Add(currentTarget);

                    // Behavior OnKill (Reaper 등 — 향후 추가)
                    foreach (var b in onKillBehaviors)
                        b.OnKill(ctx);

                    // 글로벌 훅: 유물/특성 OnKillHeal — 기존 line 241-256
                    if (caster.IsAlive)
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
        }

        /// <summary>기본 데미지 적용 — DamageApply Phase에서 커스텀 처리가 없을 때 호출.</summary>
        private static void ApplyDefaultDamage(SkillExecContext ctx, Character caster, Character target)
        {
            int hpBefore = target.Health.CurrentHP;
            DamageCalculator.DealDamage(caster, target, ctx.CurrentPower);
            ctx.LastActualDamage = hpBefore - target.Health.CurrentHP;

            CombatEventBus.FireDamageDealt(caster, target, ctx.LastActualDamage);
            CombatEventBus.FireDamageReceived(target, ctx.LastActualDamage);
        }
    }
}
