using System.Collections.Generic;
using TeamLog.Combat;
using Character = TeamLog.Characters.Character;

namespace TeamLog.Skill.Behaviors.Implementations
{
    /// <summary>
    /// Chain — 무작위 N명 연쇄 (Phase ARCH-2 추출).
    /// 기존 SkillExecutor.ExecuteAttack의 chainCount 블록과 동일한 로직.
    /// PostDamage Phase에서 메인 타겟 제외 무작위 N명에게 추가 데미지.
    /// rank = 연쇄 대상 수.
    /// </summary>
    public class ChainBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.Chain;
        public ExecutionPhase Phases => ExecutionPhase.PostApply;
        public int Order => 200; // Execution(10)/Lifesteal(50) 이후 마지막

        public void OnPostApply(SkillExecContext ctx)
        {
            // Chain rank 합산 (연쇄 횟수)
            var tags = ctx.Instance?.GetCombinedBehaviors() ?? ctx.Skill?.Behaviors;
            int chainCount = BehaviorTagResolver.RankSum(tags, BehaviorKeyword.Chain);
            if (chainCount <= 0) return;

            Character mainTarget = ctx.InitialTarget;
            Character caster = ctx.Caster;

            // 메인 타겟 제외한 살아있는 적 목록
            var others = new List<Character>();
            foreach (var e in ctx.Enemies)
            {
                if (e == null || !e.IsAlive) continue;
                if (mainTarget != null && e == mainTarget) continue;
                others.Add(e);
            }

            for (int i = 0; i < chainCount && others.Count > 0; i++)
            {
                int idx = UnityEngine.Random.Range(0, others.Count);
                DamageCalculator.DealDamage(caster, others[idx], ctx.CurrentPower);
                if (!others[idx].IsAlive)
                    others.RemoveAt(idx);
            }
        }
    }
}
