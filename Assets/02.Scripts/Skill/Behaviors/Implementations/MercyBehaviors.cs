using TeamLog.Characters;
using Character = TeamLog.Characters.Character;
using SkillType = TeamLog.Characters.SkillType;

namespace TeamLog.Skill.Behaviors.Implementations
{
    /// <summary>
    /// Phase CC-2C: Elara Mercy 자원 관련 Behavior 3종.
    /// 기획: ReworkDrafts/01_Healer.md
    /// </summary>

    /// <summary>
    /// MercyAccumulate — 힐 스킬 시전 후 Mercy +N (힐량 기반).
    /// PostApply Phase. Mend Wounds/Sanctuary에 적용.
    /// </summary>
    public class MercyAccumulateBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.MercyAccumulate;
        public ExecutionPhase Phases => ExecutionPhase.PostApply;
        public int Order => 70;

        public void OnPostApply(SkillExecContext ctx)
        {
            var caster = ctx.Caster;
            if (caster?.Resource == null || caster.Resource.Resource != ResourceType.Mercy) return;

            // 힐 스킬에만 적용
            if (ctx.Skill.Type != SkillType.Heal) return;

            int healAmount = ctx.Skill.Power;
            if (healAmount <= 0) return;

            // MercyResourceComponent의 AccumulateFromDirectHeal 호출
            if (caster.Resource is MercyResourceComponent mercy)
            {
                var target = ctx.InitialTarget;
                mercy.AccumulateFromDirectHeal(target, healAmount);
            }
        }
    }

    /// <summary>
    /// MercyConsume — Mercy N 소모. 부족 시 스킬이 자동 약화 (위력 0 처리).
    /// PowerModify Phase. Blessing of Mercy/Sanctuary에 적용.
    /// 일단 단순화: 소모 로직은 TurnManager의 일반 자원 소모 파이프라인(costType=Mercy)에 위임.
    /// 이 Behavior는 사전 검사(표시용)만 담당 — 실제 소모는 TurnManager.
    /// </summary>
    public class MercyConsumeBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.MercyConsume;
        public ExecutionPhase Phases => ExecutionPhase.PowerModify;
        public int Order => 10; // 가장 먼저 검사

        public int ModifyPower(int power, SkillExecContext ctx)
        {
            // 사전 검사만 — 실제 소모는 TurnManager 기본 자원 파이프라인
            // (표시/검증용. 빈 구현이지만 BehaviorRegistry 등록으로 스킬 툴팁에 표시 가능)
            return power;
        }
    }

    /// <summary>
    /// BondLinkBoost — 대상에게 BondBoost 상태 부여 (자동 힐 3→6).
    /// ApplyMain Phase. Bond Link 스킬에 적용.
    /// </summary>
    public class BondLinkBoostBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.BondLinkBoost;
        public ExecutionPhase Phases => ExecutionPhase.ApplyMain;
        public int Order => 50;

        public void ApplyMain(SkillExecContext ctx)
        {
            var target = ctx.InitialTarget;
            if (target?.StatusEffects == null || !target.IsAlive) return;

            // BondBoost 상태 부여 (자동 힐 강화)
            target.StatusEffects.ApplyEffect(StatusEffectType.BondBoost, 2, 1); // 2턴, value=1 (표시용)
        }
    }
}
