using StatusEffectType = TeamLog.Characters.StatusEffectType;

namespace TeamLog.Skill.Behaviors.Implementations
{
    /// <summary>
    /// Touch 계열 3종 Behavior (Phase ARCH-3 추출).
    /// 기존 SkillExecutor.ApplyTouchEffects 로직을 각 Behavior의 OnPostDamage로 이관.
    /// rank = 부여할 상태이상 스택 수. 위력은 별도 (스킬 본체의 30%, 최소 1 — 기존 로직 유지).
    /// PostDamage Phase에서 Execution(10)·Lifesteal(50) 이후, Chain(200) 이전에 작동.
    /// </summary>

    /// <summary>VenomTouch — 중독 rank스택 부여.</summary>
    public class VenomTouchBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.VenomTouch;
        public ExecutionPhase Phases => ExecutionPhase.PostDamage;
        public int Order => 100;

        public void OnPostDamage(SkillExecContext ctx)
        {
            var target = ctx.InitialTarget;
            if (target == null || !target.IsAlive) return;

            int stacks = BehaviorTagResolver.RankSum(
                ctx.Instance?.GetCombinedBehaviors() ?? ctx.Skill?.Behaviors,
                BehaviorKeyword.VenomTouch);
            if (stacks > 0)
                target.StatusEffects.ApplyEffect(StatusEffectType.Poison, stacks, stacks);
        }
    }

    /// <summary>BurningTouch — 화상 rank스택 부여.</summary>
    public class BurningTouchBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.BurningTouch;
        public ExecutionPhase Phases => ExecutionPhase.PostDamage;
        public int Order => 110;

        public void OnPostDamage(SkillExecContext ctx)
        {
            var target = ctx.InitialTarget;
            if (target == null || !target.IsAlive) return;

            int stacks = BehaviorTagResolver.RankSum(
                ctx.Instance?.GetCombinedBehaviors() ?? ctx.Skill?.Behaviors,
                BehaviorKeyword.BurningTouch);
            if (stacks > 0)
                target.StatusEffects.ApplyEffect(StatusEffectType.Burn, stacks, stacks);
        }
    }

    /// <summary>FreezeTouch — 빙결 rank스택 부여.</summary>
    public class FreezeTouchBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.FreezeTouch;
        public ExecutionPhase Phases => ExecutionPhase.PostDamage;
        public int Order => 120;

        public void OnPostDamage(SkillExecContext ctx)
        {
            var target = ctx.InitialTarget;
            if (target == null || !target.IsAlive) return;

            int stacks = BehaviorTagResolver.RankSum(
                ctx.Instance?.GetCombinedBehaviors() ?? ctx.Skill?.Behaviors,
                BehaviorKeyword.FreezeTouch);
            if (stacks > 0)
                target.StatusEffects.ApplyEffect(StatusEffectType.Freeze, stacks, stacks);
        }
    }
}
