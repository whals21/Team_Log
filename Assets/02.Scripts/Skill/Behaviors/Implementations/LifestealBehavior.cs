using TeamLog.Combat;

namespace TeamLog.Skill.Behaviors.Implementations
{
    /// <summary>
    /// Lifesteal — 준 데미지 절반 회복 (Phase ARCH-2 추출).
    /// PostApply Phase에서 ctx.LastActualDamage 기반 회복.
    /// </summary>
    public class LifestealBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.Lifesteal;
        public ExecutionPhase Phases => ExecutionPhase.PostApply;
        public int Order => 50; // Execution(10) 이후, Chain(200) 이전

        public void OnPostApply(SkillExecContext ctx)
        {
            var caster = ctx.Caster;
            if (caster == null || !caster.IsAlive) return;
            if (ctx.LastActualDamage <= 0) return;

            int healAmount = System.Math.Max(1, ctx.LastActualDamage / 2);
            caster.Health.Heal(healAmount);

            CombatEventBus.FireHealApplied(caster, healAmount);
        }
    }
}
