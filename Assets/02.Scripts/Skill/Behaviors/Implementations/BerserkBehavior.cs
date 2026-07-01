using Character = TeamLog.Characters.Character;

namespace TeamLog.Skill.Behaviors.Implementations
{
    /// <summary>
    /// Berserk — HP 절반 이하일 때 위력 2배 (Phase ARCH-2 추출).
    /// 기존 SkillExecutor.ExecuteAttack의 if(Berserk) 블록과 동일한 로직.
    /// </summary>
    public class BerserkBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.Berserk;
        public ExecutionPhase Phases => ExecutionPhase.PowerModify;
        public int Order => 50; // 위력 수정 중 먼저 (다른 PowerModify Behavior가 갱신된 위력을 받도록)

        public int ModifyPower(int power, SkillExecContext ctx)
        {
            var c = ctx.Caster;
            if (c == null || c.Health == null) return power;

            // HP 50% 이하 (CurrentHP * 2 <= MaxHP) 일 때 위력 2배
            if (c.Health.MaxHP > 0
                && c.Health.CurrentHP * 2 <= c.Health.MaxHP)
            {
                return power * 2;
            }
            return power;
        }
    }
}
