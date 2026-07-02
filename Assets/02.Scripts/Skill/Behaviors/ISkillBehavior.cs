using System.Collections.Generic;
using Character = TeamLog.Characters.Character;

namespace TeamLog.Skill.Behaviors
{
    /// <summary>
    /// 개별 BehaviorKeyword의 로직을 캡슐화하는 인터페이스 (Phase ARCH + 통합 파이프라인).
    /// 각 BehaviorKeyword은 하나의 ISkillBehavior 구현체를 가진다.
    ///
    /// ★ 통합 파이프라인 (2026-07-02):
    /// 모든 스킬 타입(Attack/Heal/Shield/Buff/Debuff/Purify)이 동일한 Phase 순서를 거친다.
    /// ApplyMain/PostApply가 모든 타입에 적용되며, 타입별 본 효과는 Pipeline 본체의 Default 헬퍼가 담당.
    /// </summary>
    public interface ISkillBehavior
    {
        /// <summary>이 Behavior가 나타내는 BehaviorKeyword.</summary>
        BehaviorKeyword Keyword { get; }

        /// <summary>개입할 Phase (복수 지정 가능, Flags).</summary>
        ExecutionPhase Phases { get; }

        /// <summary>같은 Phase 내 세부 실행 순서 (낮을수록 먼저, 기본 100).</summary>
        int Order => 100;

        // ── Phase별 훅 (기본 구현: 아무 것도 안 함 — 필요한 것만 오버라이드) ──

        /// <summary>PowerModify: 위력 수정. 기본 power 그대로 반환.</summary>
        int ModifyPower(int power, SkillExecContext ctx) => power;

        /// <summary>TargetModify: 타겟 리스트 변경. 기본 targets 그대로 반환.</summary>
        List<Character> ModifyTargets(List<Character> targets, SkillExecContext ctx) => targets;

        /// <summary>ApplyMain: 본 효과 적용. ctx.SkipDefaultApply = true 설정 시 기본 본 효과 스킵 (Pierce 등).</summary>
        void ApplyMain(SkillExecContext ctx) { }

        /// <summary>PostApply: 본 효과 후처리 (Lifesteal 회복, Touch 상태이상 부여, Cleanse 등).</summary>
        void OnPostApply(SkillExecContext ctx) { }

        /// <summary>OnKill: 이 스킬로 대상 사망 시킨 시점. (ctx.InitialTarget이 방금 사망)</summary>
        void OnKill(SkillExecContext ctx) { }
    }
}
