using System.Collections.Generic;

namespace TeamLog.Skill
{
    /// <summary>
    /// 행동 키워드 타입 — 스킬 본체/증강/특성이 공유하는 "행동 변경" 식별자.
    /// 수치 효과(위력/코스트/배율)는 기존 KeywordEntry를 병행 사용하며,
    /// BehaviorTag는 "어떤 행동인지 + rank 파라미터"만 담당한다.
    /// </summary>
    public enum BehaviorKeyword
    {
        None,

        // ── 위력/코스트 변형 ──
        HeavyHit,        // 위력 2배, 코스트 +1 (rank 미사용)
        Berserk,         // 위력 2배, HP 절반 이하 발동 (rank 미사용)
        BloodPact,       // 위력 +5, 매턴 HP -2 [저주] (rank 미사용)
        GlassCannon,     // 위력 +8, 받는 피해 2배 [저주] (rank 미사용)
        PowerUp,         // 위력 +N (rank = N)

        // ── 타겟팅/범위 ──
        Spread,          // 단일 → 광역 (위력 그대로)
        Bounce,          // 무작위 적 N회 추가 타격 (rank = 횟수, 중복 허용)
        Chain,           // 무작위 N명 연쇄 (rank = 대상 수)
        MultiHit,        // 동일 대상 N회 추가 타격 (rank = 횟수)
        Explosion,       // 광역 후 무작위 N명 추가 (rank = 대상 수)
        Pierce,          // 쉴드 + 방어 완전 무시 (DEF=0)
        Execution,       // HP rank 이하 적 즉사 (보스 제외, rank = 절대 HP 임계값)
        AOEAuto,         // 단일 자동 광역 (위력 그대로). 코스트 +2 [저주]

        // ── 생존/회복 ──
        Lifesteal,       // 준 데미지 절반 회복
        Reaper,          // 처치 시 HP +N [저주] (rank = 회복량)

        // ── 코스트/가중치 ──
        CostDown,        // 코스트 -N (rank = 감소량, 최소 0)
        QuickDraw,       // 가중치 0, 위력 절반 (rank 미사용)

        // ── 상태이상 계열 ──
        Intensify,       // 버프/디버프 효과 2배
        Lingering,       // 지속시간 +N턴 (rank = 턴 수)
        VenomTouch,      // 중독 N스택 부여 (rank = 스택 수)
        BurningTouch,    // 화상 N스택 부여 (rank = 스택 수)
        FreezeTouch,     // 빙결 N스택 부여 (rank = 스택 수)

        // ── 힐/쉴드 ──
        ShieldBonus,     // 쉴드 2배
        HealBonus,       // 힐 2배

        // ── Phase ARCH-4 신규 후보 (SkillConceptBacklog.md 컨셉 5~21) ──
        // PowerModify Phase — 상황/상태 기반 위력 보너스
        FollowUp,        // 이번 턴 이미 공격받은 대상 +N (rank = 보너스 위력)
        FirstBlood,      // 풀피 대상 +N (rank = 보너스 위력)
        Cull,            // 절반 이하 대상 +N (rank = 보너스 위력)
        GiantSlayer,     // 적 MaxHP 임계값+ 시 +N (rank = 보너스 위력)
        Dominance,       // 적 HP < 나 HP 시 +N (rank = 보너스 위력)
        Bulwark,         // 쉴드 보유 시 +N (rank = 보너스 위력)
        Desperation,     // 잃은 HP당 +N/rank (rank = 위력 1당 필요 잃은 HP)
        Wound,           // 잃은 HP당 -N/rank (rank = 위력 1 감소당 잃은 HP)
        Fatigue,         // 매 사용 시 위력 -rank (상태 추적 필요 — usesThisBattle)
        Momentum,        // 매 사용 시 위력 +rank (상태 추적 필요 — usesThisBattle)

        // PostDamage Phase — 데미지 후처리
        AllIn,           // 사용 후 AP 0 시 +N (rank = 보너스 위력)
        Bounty,          // 킬 시 자원 회수 (rank = 보상 양, 별도 필드로 보상 유형 지정)

        // 특수 — 사용 제약/상태 추적
        LimitBreak,      // 전투당 1회 사용 (상태 추적 필요 — usedThisBattle)

        // CostModify Phase (Phase ARCH-5)
        Escalation,      // 매 사용 시 AP cost +rank (상태 추적 필요)
        Mastery,         // 매 사용 시 AP cost -rank (상태 추적 필요)

        // 타겟팅/순차 — TurnManager 수정 필요 (이번 Phase에서 보류)
        Echo,            // 위력 절반 2회 시전 (순차 타겟팅 UI 필요)
        Distribute,      // 무작위 분배 (데미지/힐 양쪽, TurnManager 수정)
        TargetHighestHP, // 가장 튼튼한 적 자동 선택
        MultiStrike,     // 매 타격 자유 지정
        TargetFullHP,    // 풀피 적에게만 사용 가능 (사용 제약)
        Flank,           // 행 가장자리 대상만 (선행: 적 행/열 시스템)

        // ── Phase CC: 캐릭터 고유 메카닉 ──
        Propagate,       // Taranis 전용 — 메인 타겟 이외의 다른 적 N명에게 Charge 부여 (rank = 전파 대상 수)
        TargetFreeze,    // Lumi 등 — 대상이 Freeze 상태 시 +rank 위력 (PowerModify)

        // ── Phase CC-2A: Umbra 리워크 ──
        StrongVsDebuff,  // Umbra Backstab — 대상 도트 디버프(Poison/Burn/Bleed/Freeze/Stun) 시 위력 ×2 (PowerModify)

        // ── 통합 파이프라인 검증용 (2026-07-02) — Pipeline 수정 0줄로 추가 가능한지 증명 ──
        CleanseLowTarget,         // Phoenix Renewal용 — 대상 HP 50%- 시 Burn/Poison 정화 (PostApply)
        ResourceThresholdShield,  // Duran Shield Wall용 — 자원 ≥ rank 시 쉴드 +N (ApplyMain)
    }

    /// <summary>
    /// 직렬화 가능한 행동 태그 — BehaviorKeyword + rank 파라미터.
    /// 동일 키워드가 여러 번 나타날 수 있으며, 이 경우 rank가 합산된다.
    /// </summary>
    [System.Serializable]
    public struct BehaviorTag
    {
        public BehaviorKeyword Keyword;
        public int Rank;

        public BehaviorTag(BehaviorKeyword keyword, int rank = 0)
        {
            Keyword = keyword;
            Rank = rank;
        }

        public override string ToString() => Rank > 0 ? $"{Keyword}({Rank})" : Keyword.ToString();
    }

    /// <summary>
    /// BehaviorTag 조회 유틸리티 — null/빈 목록에 안전.
    /// </summary>
    public static class BehaviorTagResolver
    {
        /// <summary>지정 키워드가 하나라도 존재하는지.</summary>
        public static bool Has(IReadOnlyList<BehaviorTag> tags, BehaviorKeyword keyword)
        {
            if (tags == null) return false;
            for (int i = 0; i < tags.Count; i++)
                if (tags[i].Keyword == keyword) return true;
            return false;
        }

        /// <summary>지정 키워드의 첫 번째 태그를 반환 (없으면 null).</summary>
        public static BehaviorTag? First(IReadOnlyList<BehaviorTag> tags, BehaviorKeyword keyword)
        {
            if (tags == null) return null;
            for (int i = 0; i < tags.Count; i++)
                if (tags[i].Keyword == keyword) return tags[i];
            return null;
        }

        /// <summary>지정 키워드의 모든 태그를 반환.</summary>
        public static List<BehaviorTag> All(IReadOnlyList<BehaviorTag> tags, BehaviorKeyword keyword)
        {
            var result = new List<BehaviorTag>();
            if (tags == null) return result;
            for (int i = 0; i < tags.Count; i++)
                if (tags[i].Keyword == keyword) result.Add(tags[i]);
            return result;
        }

        /// <summary>지정 키워드의 rank 합산 값.</summary>
        public static int RankSum(IReadOnlyList<BehaviorTag> tags, BehaviorKeyword keyword)
        {
            int total = 0;
            if (tags == null) return total;
            for (int i = 0; i < tags.Count; i++)
                if (tags[i].Keyword == keyword) total += tags[i].Rank;
            return total;
        }
    }
}
