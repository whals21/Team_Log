using System;

namespace TeamLog.Skill.Behaviors
{
    /// <summary>
    /// 스킬 실행 파이프라인의 단계 정의 (Phase ARCH).
    /// 각 Behavior가 자신이 개입할 Phase를 Flags로 지정.
    /// Phase 순서는 절대: PowerModify → TargetModify → DamageApply → PostDamage → OnKill → TurnEnd
    /// </summary>
    [Flags]
    public enum ExecutionPhase
    {
        None = 0,

        /// <summary>위력 계산 단계. 위력 배율/가산 Behavior가 개입 (Berserk, HeavyHit, Desperation, Wound, PowerUp 등).</summary>
        PowerModify = 1 << 0,

        /// <summary>타겟 리스트 결정 단계. 범위/연쇄 Behavior가 개입 (Spread, Bounce, Chain, MultiHit, Distribute 등).</summary>
        TargetModify = 1 << 1,

        /// <summary>데미지 적용 단계. 특수 적용 방식이 필요한 Behavior가 개입 (Pierce 등). 기본 DealDamage를 스킵하려면 ctx.SkipDefaultDamage = true 설정.</summary>
        DamageApply = 1 << 2,

        /// <summary>데미지 후처리 단계. 적중 후 효과 Behavior가 개입 (Lifesteal, AllIn, FollowUp, Touch 계열 등).</summary>
        PostDamage = 1 << 3,

        /// <summary>대상 사망 시 처리. 킬 트리거 Behavior가 개입 (Reaper, Bounty 등).</summary>
        OnKill = 1 << 4,

        /// <summary>턴 종료 시 처리. 지속 효과 Behavior가 개입 (Lingering 등).</summary>
        TurnEnd = 1 << 5,
    }
}
