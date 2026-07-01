using System.Collections.Generic;
using TeamLog.Skill;
using TeamLog.Skill.Behaviors;
using TeamLog.Map;
using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;
using SkillInstance = TeamLog.Characters.SkillInstance;
using SkillType = TeamLog.Characters.SkillType;
using StatType = TeamLog.Characters.StatType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;

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

            // Phase CC: 자원 기반 위력 보너스 (Ember×3, Vengeance×1 등)
            // skill.ResourcePowerPerStack이 설정된 경우, 시전자의 현재 자원 스택 × perStack을 위력에 가산.
            // Brand of Ash: 8 + Ember×3 (Ember 5일 때 23). Revenge Strike: 10 + Vengeance×1.
            if (caster.Resource != null && skill.ResourcePowerPerStack > 0)
                ctx.CurrentPower += caster.Resource.CurrentStacks * skill.ResourcePowerPerStack;

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

            // Phase CC: 피격 시 자원 훅 (Duran Vengeance 축적 등)
            if (target.Resource != null && ctx.LastActualDamage > 0)
                target.Resource.OnDamageTaken(target, ctx.LastActualDamage);
        }

        // ═══════════════════════════════════════════
        // Phase CC 통합: 모든 스킬 타입을 Pipeline으로 처리
        // Attack은 기존 ExecuteAttack 호출, Heal/Shield/Buff/Debuff/Purify는
        // 각 전용 메서드에서 자원 비례 위력 + BehaviorTag 적용.
        // ═══════════════════════════════════════════

        /// <summary>모든 스킬 타입의 통합 진입점. 타입별로 적절한 실행 메서드 분기.</summary>
        public void ExecuteSkill(Character caster, SkillData skill, Character target,
            SkillInstance instance = null, float powerMultiplier = 1f, TurnContext turnCtx = null)
        {
            if (caster == null || target == null) return;

            switch (skill.Type)
            {
                case SkillType.Attack:
                    ExecuteAttack(caster, skill, target, instance, powerMultiplier, turnCtx);
                    break;
                case SkillType.Heal:
                    ExecuteHealViaPipeline(caster, skill, target, instance, powerMultiplier);
                    break;
                case SkillType.Shield:
                    ExecuteShieldViaPipeline(caster, skill, target, instance, powerMultiplier);
                    break;
                case SkillType.Buff:
                case SkillType.Debuff:
                    ApplyEffectViaPipeline(caster, skill, target, instance);
                    break;
                case SkillType.Purify:
                    target.StatusEffects.ClearAllEffects();
                    target.ApplyStatModifiers();
                    break;
            }
        }

        /// <summary>Heal 타입 Pipeline 처리 — 자원 비례 위력 + Behavior PowerModify 적용.</summary>
        private void ExecuteHealViaPipeline(Character caster, SkillData skill, Character target,
            SkillInstance instance, float powerMultiplier)
        {
            var ctx = new SkillExecContext
            {
                Caster = caster, InitialTarget = target, Skill = skill, Instance = instance,
                PlayerParty = _playerParty, Enemies = _enemies, PowerMultiplier = powerMultiplier,
            };
            IReadOnlyList<BehaviorTag> tags = instance?.GetCombinedBehaviors() ?? skill.Behaviors;

            // 위력 계산
            int basePower = instance != null ? instance.EffectivePower : skill.Power;
            ctx.CurrentPower = System.Math.Max(1, basePower);

            // Behavior PowerModify (강화 조건 Behavior — FirstBlood/Cull 등)
            foreach (var b in BehaviorRegistry.GetForPhase(tags, ExecutionPhase.PowerModify))
                ctx.CurrentPower = b.ModifyPower(ctx.CurrentPower, ctx);

            // Phase CC: 자원 비례 위력 (Heal에도 적용 — Phoenix Renewal Ember×3 등)
            if (caster.Resource != null && skill.ResourcePowerPerStack > 0)
                ctx.CurrentPower += caster.Resource.CurrentStacks * skill.ResourcePowerPerStack;

            // 키워드 HealMul 배율
            float healMul = SkillExecutor.GetAllKeywordMul(caster, KeywordType.HealMul);
            ctx.CurrentPower = System.Math.Max(1, (int)(ctx.CurrentPower * powerMultiplier * healMul));

            // 힐 적용
            target.Health.Heal(ctx.CurrentPower);
            CombatEventBus.FireHealApplied(target, ctx.CurrentPower);
        }

        /// <summary>Shield 타입 Pipeline 처리 — 자원 비례 위력 + Behavior PowerModify 적용.</summary>
        private void ExecuteShieldViaPipeline(Character caster, SkillData skill, Character target,
            SkillInstance instance, float powerMultiplier)
        {
            var ctx = new SkillExecContext
            {
                Caster = caster, InitialTarget = target, Skill = skill, Instance = instance,
                PlayerParty = _playerParty, Enemies = _enemies, PowerMultiplier = powerMultiplier,
            };
            IReadOnlyList<BehaviorTag> tags = instance?.GetCombinedBehaviors() ?? skill.Behaviors;

            int basePower = instance != null ? instance.EffectivePower : skill.Power;
            ctx.CurrentPower = System.Math.Max(1, basePower);

            foreach (var b in BehaviorRegistry.GetForPhase(tags, ExecutionPhase.PowerModify))
                ctx.CurrentPower = b.ModifyPower(ctx.CurrentPower, ctx);

            if (caster.Resource != null && skill.ResourcePowerPerStack > 0)
                ctx.CurrentPower += caster.Resource.CurrentStacks * skill.ResourcePowerPerStack;

            float shieldMul = SkillExecutor.GetAllKeywordMul(caster, KeywordType.ShieldMul);
            ctx.CurrentPower = System.Math.Max(1, (int)(ctx.CurrentPower * powerMultiplier * shieldMul));

            target.Health.AddShield(ctx.CurrentPower);
            CombatEventBus.FireShieldGained(target, ctx.CurrentPower);
        }

        /// <summary>Buff/Debuff 타입 Pipeline 처리 — 기존 ApplyEffect 로직 이관 + 특성 훅.</summary>
        private void ApplyEffectViaPipeline(Character caster, SkillData skill, Character target,
            SkillInstance instance)
        {
            if (skill.StatusEffect == StatusEffectType.None) return;

            // Shell 특성: 매 턴 첫 상태이상 무효화
            if (target.TraitHandler.ShouldBlockEffect()) return;

            int duration = skill.EffectDuration;
            int value = skill.EffectValue;

            // 증강 DurationAdd
            if (instance != null)
                duration += (int)KeywordResolver.SumKeyword(instance.GetAllKeywords(), KeywordType.DurationAdd);
            // 장착 특성 DurationAdd (도적 독 마스터 등)
            if (caster != null && caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
                duration += caster.PlayerTraitHandler.QueryKeywordSum(KeywordType.DurationAdd);

            // 증강 EffectMul
            float effectMul = 1f;
            if (instance != null)
                effectMul *= KeywordResolver.MulKeyword(instance.GetAllKeywords(), KeywordType.EffectMul);
            // 장착 특성 EffectMul (네크로맨서 저주의 대가 등)
            if (caster != null && caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
                effectMul *= caster.PlayerTraitHandler.QueryKeywordMul(KeywordType.EffectMul);
            value = System.Math.Max(1, (int)(value * effectMul));

            target.StatusEffects.ApplyEffect(skill.StatusEffect, duration, value);
            target.ApplyStatModifiers();
        }
    }
}
