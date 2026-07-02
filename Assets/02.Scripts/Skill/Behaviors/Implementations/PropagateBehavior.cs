using System.Collections.Generic;
using TeamLog.Characters;
using Character = TeamLog.Characters.Character;
using StatusEffectType = TeamLog.Characters.StatusEffectType;

namespace TeamLog.Skill.Behaviors.Implementations
{
    /// <summary>
    /// Propagate — Taranis 전용 전파 메카닉.
    /// PostDamage Phase에서 메인 타겟 이외의 다른 적 N명(전하 보유 적 우선)에게 Charge 1스택 부여.
    /// rank = 전파 대상 수.
    /// 기획: Wire 사용 시 자동으로 다른 적 1명에게도 전하 1스택 전파.
    /// </summary>
    public class PropagateBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.Propagate;
        public ExecutionPhase Phases => ExecutionPhase.PostApply;
        public int Order => 300; // Chain(200) 이후

        public void OnPostApply(SkillExecContext ctx)
        {
            var tags = ctx.Instance?.GetCombinedBehaviors() ?? ctx.Skill?.Behaviors;
            int propagateCount = BehaviorTagResolver.RankSum(tags, BehaviorKeyword.Propagate);
            if (propagateCount <= 0) return;

            Character mainTarget = ctx.InitialTarget;

            // 후보 목록: 메인 타겟 제외 살아있는 적
            // 전하 보유 적을 우선으로 (기획: 전파는 기존 전하 적 우선)
            var chargedCandidates = new List<Character>();
            var normalCandidates = new List<Character>();
            foreach (var e in ctx.Enemies)
            {
                if (e == null || !e.IsAlive) continue;
                if (mainTarget != null && e == mainTarget) continue;
                if (e.StatusEffects.HasEffect(StatusEffectType.Charge))
                    chargedCandidates.Add(e);
                else
                    normalCandidates.Add(e);
            }

            // duration=3은 자연 소멄 전에 안 사라지도록 (Charge는 StatusEffectComponent에서 duration 소멸 스킵)
            int propagated = 0;
            foreach (var target in chargedCandidates)
            {
                if (propagated >= propagateCount) break;
                target.StatusEffects.ApplyEffect(StatusEffectType.Charge, 3, 1);
                propagated++;
            }
            foreach (var target in normalCandidates)
            {
                if (propagated >= propagateCount) break;
                target.StatusEffects.ApplyEffect(StatusEffectType.Charge, 3, 1);
                propagated++;
            }
        }
    }
}
