using System.Collections.Generic;
using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;
using SkillInstance = TeamLog.Characters.SkillInstance;
using TurnContext = TeamLog.Combat.Turn.TurnContext;

namespace TeamLog.Skill.Behaviors
{
    /// <summary>
    /// 스킬 실행 파이프라인의 공유 상태 (Phase ARCH).
    /// 모든 Behavior가 이 컨텍스트를 읽고 갱신하며 통신한다.
    /// 글로벌 변수 없이 Behavior 간 데이터 교환을 담당.
    /// </summary>
    public class SkillExecContext
    {
        // ── 입력 (생성 시 설정, 실행 중 읽기 전용 권장) ──

        /// <summary>시전자.</summary>
        public Character Caster { get; set; }

        /// <summary>초기 타겟 (플레이어가 클릭한 대상). TargetModify 후에는 CurrentTargets 사용.</summary>
        public Character InitialTarget { get; set; }

        /// <summary>사용된 스킬 정적 데이터.</summary>
        public SkillData Skill { get; set; }

        /// <summary>스킬 인스턴스 (증강 포함). null 가능.</summary>
        public SkillInstance Instance { get; set; }

        /// <summary>현재 턴 컨텍스트 (AP 등).</summary>
        public TurnContext TurnCtx { get; set; }

        /// <summary>플레이어 파티 전체 (Chain/Bounce가 무작위 선택에 사용).</summary>
        public IReadOnlyList<Character> PlayerParty { get; set; }

        /// <summary>적 전체 (Chain/Bounce가 무작위 선택에 사용).</summary>
        public IReadOnlyList<Character> Enemies { get; set; }

        /// <summary>powerMultiplier (Spread/AOEAuto 등의 위력 분배).</summary>
        public float PowerMultiplier { get; set; } = 1f;

        // ── 진행 중 상태 (Behavior들이 갱신) ──

        /// <summary>현재 위력. PowerModify Phase에서 갱신.</summary>
        public int CurrentPower { get; set; }

        /// <summary>현재 타겟 리스트. TargetModify Phase에서 갱신. DamageApply부터는 이 리스트를 순회.</summary>
        public List<Character> CurrentTargets { get; set; }

        /// <summary>Pierce 등 커스텀 ApplyMain Behavior가 true 설정 — 타입별 기본 본 효과(DefaultDamage/Heal/Shield/Effect/Purify) 스킵.</summary>
        public bool SkipDefaultApply { get; set; }

        // ── 결과 기록 (후속 Behavior가 참조) ──

        /// <summary>직전 타격의 실제 입힌 데미지 (Lifesteal 회복량 계산용).</summary>
        public int LastActualDamage { get; set; }

        /// <summary>이 스킬로 사망한 대상 목록 (OnKill 훅이 참조).</summary>
        public List<Character> KilledTargets { get; } = new List<Character>();
    }
}
