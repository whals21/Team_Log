using TeamLog.Characters;
using Character = TeamLog.Characters.Character;

namespace TeamLog.Skill.Behaviors.Implementations
{
    /// <summary>
    /// StrongVsDebuff (Phase CC-2A — Umbra Backstab).
    /// 대상이 도트 디버프(Poison/Burn/Bleed/Freeze/Stun) 상태일 때 위력 ×2.
    /// 서사: "약해진 적을 기습한다" — Rogue Backstab의 핵심 조건.
    /// </summary>
    public class StrongVsDebuffBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.StrongVsDebuff;
        public ExecutionPhase Phases => ExecutionPhase.PowerModify;
        public int Order => 55; // Berserk(50) 직후, FirstBlood(60) 전

        public int ModifyPower(int power, SkillExecContext ctx)
        {
            var target = ctx.InitialTarget;
            if (target == null || target.StatusEffects == null) return power;

            if (HasDotDebuff(target))
                return power * 2;

            return power;
        }

        /// <summary>대상이 도트/행동봉쇄 디버프 상태인지 확인.</summary>
        private static bool HasDotDebuff(Character target)
        {
            foreach (var effect in target.StatusEffects.GetAllEffects())
            {
                if (effect.Type == StatusEffectType.Poison ||
                    effect.Type == StatusEffectType.Burn ||
                    effect.Type == StatusEffectType.Bleed ||
                    effect.Type == StatusEffectType.Freeze ||
                    effect.Type == StatusEffectType.Stun)
                    return true;
            }
            return false;
        }
    }
}
