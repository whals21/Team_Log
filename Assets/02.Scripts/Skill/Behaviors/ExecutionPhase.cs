using System;

namespace TeamLog.Skill.Behaviors
{
    /// <summary>
    /// 스킬 실행 파이프라인의 단계 정의 (Phase ARCH + 통합 파이프라인).
    /// 각 Behavior가 자신이 개입할 Phase를 Flags로 지정.
    /// Phase 순서는 절대: PowerModify → TargetModify → ApplyMain → PostApply → OnKill → TurnEnd
    ///
    /// ★ 통합 파이프라인 (2026-07-02):
    /// ApplyMain은 모든 스킬 타입(Attack/Heal/Shield/Buff/Debuff/Purify)의 본 효과에 해당.
    /// PostApply는 모든 타입의 후처리(Lifesteal/Touch/Chain/Cleanse/Propagate 등)에 해당.
    /// 타입별 차이는 Pipeline 본체의 Default 헬퍼(ApplyDefaultByType)에서 처리.
    /// </summary>
    [Flags]
    public enum ExecutionPhase
    {
        None = 0,

        /// <summary>위력 계산 단계. 위력 배율/가산 Behavior가 개입 (Berserk, HeavyHit, Desperation, Wound, PowerUp 등).</summary>
        PowerModify = 1 << 0,

        /// <summary>타겟 리스트 결정 단계. 범위/연쇄 Behavior가 개입 (Spread, Bounce, Chain, MultiHit, Distribute 등).</summary>
        TargetModify = 1 << 1,

        /// <summary>본 효과 적용 단계. 타입별 본 효과 + 커스텀 적용 방식이 필요한 Behavior가 개입 (Pierce 등).
        /// 기본 본 효과(DefaultDamage/Heal/Shield/Effect/Purify)를 스킵하려면 ctx.SkipDefaultApply = true 설정.</summary>
        ApplyMain = 1 << 2,

        /// <summary>후처리 단계. 본 효과 후 추가 동작 Behavior가 개입 (Lifesteal, AllIn, Touch 계열, Cleanse, Propagate 등).</summary>
        PostApply = 1 << 3,

        /// <summary>대상 사망 시 처리. 킬 트리거 Behavior가 개입 (Reaper, Bounty 등).</summary>
        OnKill = 1 << 4,

        /// <summary>턴 종료 시 처리. 지속 효과 Behavior가 개입 (Lingering 등).</summary>
        TurnEnd = 1 << 5,
    }
}
