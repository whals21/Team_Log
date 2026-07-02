using TeamLog.Combat;
using Character = TeamLog.Characters.Character;

namespace TeamLog.Skill.Behaviors.Implementations
{
    /// <summary>
    /// Phase ARCH-4 신규 Behavior 9종 (상태 추적 불필요 — 컨셉 6/7/11/12/15/16/17/18/21).
    /// 모두 ctx.Caster/InitialTarget의 현재 상태만 체크하여 위력/데미지/킬 시 보상 적용.
    /// 상태 추적이 필요한 컨셉(5 FollowUp/8 Fatigue/9 Momentum/13 Escalation/14 Mastery/19 LimitBreak)은
    /// usesThisBattle 인프라 구축 후 별도 작업 (이번 Phase에서 보류).
    /// 타겟팅 컨셉(1 Distribute/2 TargetHighestHP/3 MultiStrike/4 TargetFullHP/10 Echo/20 Flank)은
    /// TurnManager 수정이 필요하므로 별도 작업.
    /// </summary>

    // ═══════════════════════════════════════════
    // PowerModify Phase — 상황/상태 기반 위력 보너스
    // ═══════════════════════════════════════════

    /// <summary>FirstBlood (컨셉 6) — 풀피 대상 +rank 위력.</summary>
    public class FirstBloodBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.FirstBlood;
        public ExecutionPhase Phases => ExecutionPhase.PowerModify;
        public int Order => 60; // Berserk(50) 후

        public int ModifyPower(int power, SkillExecContext ctx)
        {
            var target = ctx.InitialTarget;
            if (target == null || target.Health == null) return power;
            if (target.Health.CurrentHP == target.Health.MaxHP)
                return power + ctx.Instance?.GetBehaviorRank(BehaviorKeyword.FirstBlood) ?? 0;
            return power;
        }
    }

    /// <summary>Cull (컨셉 7) — 절반 이하 대상 +rank 위력.</summary>
    public class CullBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.Cull;
        public ExecutionPhase Phases => ExecutionPhase.PowerModify;
        public int Order => 60;

        public int ModifyPower(int power, SkillExecContext ctx)
        {
            var target = ctx.InitialTarget;
            if (target == null || target.Health == null || target.Health.MaxHP <= 0) return power;
            if (target.Health.CurrentHP * 2 <= target.Health.MaxHP)
                return power + ctx.Instance?.GetBehaviorRank(BehaviorKeyword.Cull) ?? 0;
            return power;
        }
    }

    /// <summary>GiantSlayer (컨셉 15) — 적 MaxHP 100+ 시 +rank 위력. 거인살해자.</summary>
    public class GiantSlayerBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.GiantSlayer;
        public ExecutionPhase Phases => ExecutionPhase.PowerModify;
        public int Order => 60;
        private const int Threshold = 100; // 밸런스 튠 대상 — 엘리트/보스 기준

        public int ModifyPower(int power, SkillExecContext ctx)
        {
            var target = ctx.InitialTarget;
            if (target == null || target.Health == null) return power;
            if (target.Health.MaxHP >= Threshold)
                return power + ctx.Instance?.GetBehaviorRank(BehaviorKeyword.GiantSlayer) ?? 0;
            return power;
        }
    }

    /// <summary>Dominance (컨셉 17) — 적 HP < 나 HP 시 +rank 위력.</summary>
    public class DominanceBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.Dominance;
        public ExecutionPhase Phases => ExecutionPhase.PowerModify;
        public int Order => 60;

        public int ModifyPower(int power, SkillExecContext ctx)
        {
            var caster = ctx.Caster;
            var target = ctx.InitialTarget;
            if (caster == null || target == null) return power;
            if (caster.Health == null || target.Health == null) return power;
            if (target.Health.CurrentHP < caster.Health.CurrentHP)
                return power + ctx.Instance?.GetBehaviorRank(BehaviorKeyword.Dominance) ?? 0;
            return power;
        }
    }

    /// <summary>Bulwark (컨셉 18) — 쉴드 보유 시 +rank 위력.</summary>
    public class BulwarkBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.Bulwark;
        public ExecutionPhase Phases => ExecutionPhase.PowerModify;
        public int Order => 60;

        public int ModifyPower(int power, SkillExecContext ctx)
        {
            var caster = ctx.Caster;
            if (caster == null || caster.Health == null) return power;
            if (caster.Health.CurrentShield > 0)
                return power + ctx.Instance?.GetBehaviorRank(BehaviorKeyword.Bulwark) ?? 0;
            return power;
        }
    }

    /// <summary>Desperation (컨셉 11) — 잃은 HP당 +위력/rank. rank = 위력 1당 필요 잃은 HP.</summary>
    public class DesperationBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.Desperation;
        public ExecutionPhase Phases => ExecutionPhase.PowerModify;
        public int Order => 70; // 위력 비례라 Berserk/조건부 후에 적용

        public int ModifyPower(int power, SkillExecContext ctx)
        {
            var caster = ctx.Caster;
            if (caster == null || caster.Health == null || caster.Health.MaxHP <= 0) return power;
            int rank = ctx.Instance?.GetBehaviorRank(BehaviorKeyword.Desperation) ?? 0;
            if (rank <= 0) return power;
            int lostHP = caster.Health.MaxHP - caster.Health.CurrentHP;
            if (lostHP <= 0) return power;
            return power + (lostHP / rank);
        }
    }

    /// <summary>Wound (컨셉 12) — 잃은 HP당 -위력/rank. Desperation의 음수 버전. 의도적 약점 부여용.</summary>
    public class WoundBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.Wound;
        public ExecutionPhase Phases => ExecutionPhase.PowerModify;
        public int Order => 70;

        public int ModifyPower(int power, SkillExecContext ctx)
        {
            var caster = ctx.Caster;
            if (caster == null || caster.Health == null || caster.Health.MaxHP <= 0) return power;
            int rank = ctx.Instance?.GetBehaviorRank(BehaviorKeyword.Wound) ?? 0;
            if (rank <= 0) return power;
            int lostHP = caster.Health.MaxHP - caster.Health.CurrentHP;
            if (lostHP <= 0) return power;
            return System.Math.Max(1, power - (lostHP / rank));
        }
    }

    // ═══════════════════════════════════════════
    // PostApply Phase
    // ═══════════════════════════════════════════

    /// <summary>AllIn (컨셉 16) — 사용 후 AP가 0이면 +rank 위력. PostApply에서 체크하여 추가 데미지.</summary>
    public class AllInBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.AllIn;
        public ExecutionPhase Phases => ExecutionPhase.PostApply;
        public int Order => 30; // Execution(10) 전에 발동하여 추가 데미지로 처치 유도

        public void OnPostApply(SkillExecContext ctx)
        {
            // AP 0 조건: 스킬 사용 후 남은 AP가 0
            if (ctx.TurnCtx == null) return;
            if (ctx.TurnCtx.CurrentAP != 0) return;

            int bonus = ctx.Instance?.GetBehaviorRank(BehaviorKeyword.AllIn) ?? 0;
            if (bonus <= 0) return;

            var target = ctx.InitialTarget;
            var caster = ctx.Caster;
            if (target == null || !target.IsAlive || caster == null) return;

            // 추가 데미지 적용 (직접 DealDamage)
            int hpBefore = target.Health.CurrentHP;
            DamageCalculator.DealDamage(caster, target, bonus);
            int dealt = hpBefore - target.Health.CurrentHP;
            ctx.LastActualDamage += dealt;

            CombatEventBus.FireDamageDealt(caster, target, dealt);
            CombatEventBus.FireDamageReceived(target, dealt);
        }
    }

    // ═══════════════════════════════════════════
    // OnKill Phase
    // ═══════════════════════════════════════════

    /// <summary>Bounty (컨셉 21) — 킬 시 AP 회수. rank = 회수 AP 양.</summary>
    public class BountyBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.Bounty;
        public ExecutionPhase Phases => ExecutionPhase.OnKill;
        public int Order => 50;

        public void OnKill(SkillExecContext ctx)
        {
            int apRefund = ctx.Instance?.GetBehaviorRank(BehaviorKeyword.Bounty) ?? 0;
            if (apRefund <= 0) return;
            if (ctx.TurnCtx == null) return;

            // 현재 maxAP를 유지하면서 currentAP 증가
            // TurnContext의 AP 회복 API가 제한적이므로 간접 처리
            // (정식 구현 시 TurnContext에 AddAP 메서드 추가 권장)
            // 임시: 킬 시 ctx.LastActualDamage를 회복량으로 환산하여 힐로 대체 (밸런스 placeholder)
            // TODO: TurnContext.AddAP 구현 후 AP 회수로 전환
            var caster = ctx.Caster;
            if (caster != null && caster.IsAlive)
            {
                int healAmount = apRefund * 3; // 임시 보상 — AP 회수 대신 힐
                caster.Health.Heal(healAmount);
                CombatEventBus.FireHealApplied(caster, healAmount);
            }
        }
    }
}
