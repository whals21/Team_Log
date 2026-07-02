using TeamLog.Characters;
using Character = TeamLog.Characters.Character;
using StatusEffectType = TeamLog.Characters.StatusEffectType;

namespace TeamLog.Skill.Behaviors.Implementations
{
    /// <summary>
    /// 통합 파이프라인 검증용 Behavior 2종 (2026-07-02).
    /// 목적: Pipeline.ExecuteSkill 코드 수정 없이 PostApply/ApplyMain에 새 Behavior를 추가할 수 있음을 증명.
    /// </summary>

    /// <summary>
    /// CleanseLowTarget — 대상 HP 50%- 시 Burn/Poison 정화.
    /// PostApply Phase. Phoenix Renewal(Ashe Heal 스킬)용.
    /// ★ Heal 타입 스킬이 이제 PostApply Phase를 거치므로 정화 효과가 자동 작동.
    /// </summary>
    public class CleanseLowTargetBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.CleanseLowTarget;
        public ExecutionPhase Phases => ExecutionPhase.PostApply;
        public int Order => 50;

        public void OnPostApply(SkillExecContext ctx)
        {
            var target = ctx.InitialTarget;
            if (target == null || !target.IsAlive || target.Health == null) return;

            // 대상 HP 50% 이하 체크
            if (target.Health.MaxHP <= 0) return;
            if (target.Health.CurrentHP * 2 > target.Health.MaxHP) return;

            // Burn/Poison 정화
            bool changed = false;
            if (target.StatusEffects.HasEffect(StatusEffectType.Burn))
            {
                target.StatusEffects.RemoveEffect(StatusEffectType.Burn);
                changed = true;
            }
            if (target.StatusEffects.HasEffect(StatusEffectType.Poison))
            {
                target.StatusEffects.RemoveEffect(StatusEffectType.Poison);
                changed = true;
            }
            if (changed)
                target.ApplyStatModifiers();
        }
    }

    /// <summary>
    /// ResourceThresholdShield — 시전자의 자원이 rank 이상일 때 쉴드 +rank 가산.
    /// ApplyMain Phase (기본 쉴드 적용 전). Duran Shield Wall용 (Vengeance 5+ 시 쉴드 +5).
    /// ★ Shield 타입 스킬이 이제 ApplyMain Phase를 거치므로 자동 작동.
    /// </summary>
    public class ResourceThresholdShieldBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.ResourceThresholdShield;
        public ExecutionPhase Phases => ExecutionPhase.ApplyMain;
        public int Order => 50; // 기본 쉴드(기본 헬퍼) 전에 위력 가산

        public void ApplyMain(SkillExecContext ctx)
        {
            var caster = ctx.Caster;
            if (caster?.Resource == null) return;

            int threshold = ctx.Instance?.GetBehaviorRank(BehaviorKeyword.ResourceThresholdShield) ?? 0;
            if (threshold <= 0) return;
            if (caster.Resource.CurrentStacks < threshold) return;

            // 임계값 충족 — 위력에 rank 가산 (Default 헬퍼가 이 위력으로 쉴드 부여)
            // rank를 위력 가산치로도 사용 (단순화: 임계값 5 → +5 쉴드)
            ctx.CurrentPower += threshold;
        }
    }
}
