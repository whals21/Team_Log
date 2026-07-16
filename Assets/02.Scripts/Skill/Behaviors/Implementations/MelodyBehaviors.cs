using TeamLog.Characters;
using Character = TeamLog.Characters.Character;

namespace TeamLog.Skill.Behaviors.Implementations
{
    /// <summary>
    /// Phase CC-2D: Calliope Melody 자원 관련 Behavior 4종.
    /// 기획: ReworkDrafts/06_Bard.md
    /// 각 스킬이 ApplyMain Phase에서 자신의 선율을 CurrentMelody로 설정.
    /// 부 선율 자동 발동은 MelodyResourceComponent.OnTurnStart에서 처리.
    /// </summary>

    /// <summary>Mending Song — CurrentMelody=Healing 설정.</summary>
    public class MelodyHealingBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.MelodyHealing;
        public ExecutionPhase Phases => ExecutionPhase.ApplyMain;
        public int Order => 90;

        public void ApplyMain(SkillExecContext ctx)
        {
            if (ctx.Caster?.Resource is MelodyResourceComponent melody)
                melody.SetCurrentMelody(MelodyType.Healing);
        }
    }

    /// <summary>Anthem of Valor — CurrentMelody=Valor 설정.</summary>
    public class MelodyValorBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.MelodyValor;
        public ExecutionPhase Phases => ExecutionPhase.ApplyMain;
        public int Order => 90;

        public void ApplyMain(SkillExecContext ctx)
        {
            if (ctx.Caster?.Resource is MelodyResourceComponent melody)
                melody.SetCurrentMelody(MelodyType.Valor);
        }
    }

    /// <summary>Dissonant Chord — CurrentMelody=Dissonance 설정.</summary>
    public class MelodyDissonanceBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.MelodyDissonance;
        public ExecutionPhase Phases => ExecutionPhase.ApplyMain;
        public int Order => 90;

        public void ApplyMain(SkillExecContext ctx)
        {
            if (ctx.Caster?.Resource is MelodyResourceComponent melody)
                melody.SetCurrentMelody(MelodyType.Dissonance);
        }
    }

    /// <summary>Inspiring Refrain — CurrentMelody=Inspiration 설정.</summary>
    public class MelodyInspirationBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.MelodyInspiration;
        public ExecutionPhase Phases => ExecutionPhase.ApplyMain;
        public int Order => 90;

        public void ApplyMain(SkillExecContext ctx)
        {
            if (ctx.Caster?.Resource is MelodyResourceComponent melody)
                melody.SetCurrentMelody(MelodyType.Inspiration);
        }
    }
}
