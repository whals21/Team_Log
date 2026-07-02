using TeamLog.Characters;
using Character = TeamLog.Characters.Character;
using StatusEffectType = TeamLog.Characters.StatusEffectType;

namespace TeamLog.Skill.Behaviors.Implementations
{
    /// <summary>
    /// TargetFreeze — 대상이 Freeze 상태일 때 +rank 위력.
    /// PowerModify Phase. Lumi Frost Bite(서리 이빨) 강화 조건: "대상 이미 Freeze 상태 시 +위력 3".
    /// Lumi의 Frost Armor/Blizzard로 먼저 Freeze 건 후 Frostbolt 쓰면 보너스. 콤보 유도.
    /// </summary>
    public class TargetFreezeBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.TargetFreeze;
        public ExecutionPhase Phases => ExecutionPhase.PowerModify;
        public int Order => 60; // Berserk(50) 다음, FirstBlood(70) 이전

        public int ModifyPower(int power, SkillExecContext ctx)
        {
            Character target = ctx.InitialTarget;
            if (target == null || !target.IsAlive) return power;
            if (!target.StatusEffects.HasEffect(StatusEffectType.Freeze)) return power;

            var tags = ctx.Instance?.GetCombinedBehaviors() ?? ctx.Skill?.Behaviors;
            int rank = BehaviorTagResolver.RankSum(tags, BehaviorKeyword.TargetFreeze);
            return power + rank;
        }
    }
}
