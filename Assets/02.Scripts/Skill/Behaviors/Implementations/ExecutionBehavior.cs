using TeamLog.Combat;

namespace TeamLog.Skill.Behaviors.Implementations
{
    /// <summary>
    /// Execution — HP rank 이하 적 즉사 (보스 제외, Phase ARCH-2 추출).
    /// 기존 SkillExecutor.ExecuteAttack의 if(Execution) 블록과 동일한 로직.
    /// PostDamage Phase에서 데미지 적용 후 남은 HP가 임계값 이하면 즉사.
    /// </summary>
    public class ExecutionBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.Execution;
        public ExecutionPhase Phases => ExecutionPhase.PostDamage;
        public int Order => 10; // PostDamage 중 가장 먼저 (Lifesteal/Chain보다 먼저 사망 판정)

        public void OnPostDamage(SkillExecContext ctx)
        {
            var target = ctx.InitialTarget;
            if (target == null || !target.IsAlive) return;

            // 보스는 즉사 면역
            if (target.Data != null && target.Data.IsBoss) return;

            // 스킬 본체/증강에서 Execution 태그의 rank 조회 (절대 HP 임계값)
            var tags = ctx.Instance?.GetCombinedBehaviors() ?? ctx.Skill?.Behaviors;
            var exec = BehaviorTagResolver.First(tags, BehaviorKeyword.Execution);
            if (!exec.HasValue) return;

            // 현재 HP가 rank 이하면 즉사
            if (target.Health.CurrentHP <= exec.Value.Rank)
            {
                int execDamage = target.Health.CurrentHP;
                target.Health.TakeDirectDamage(execDamage);

                CombatEventBus.FireDamageDealt(ctx.Caster, target, execDamage);
                CombatEventBus.FireDamageReceived(target, execDamage);

                // Lifesteal이 Execution 데미지 기반으로 회복할 수 있도록 LastActualDamage 갱신
                ctx.LastActualDamage = execDamage;
            }
        }
    }
}
