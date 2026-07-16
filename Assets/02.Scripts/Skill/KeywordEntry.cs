using System.Collections.Generic;

namespace TeamLog.Skill
{
    /// <summary>
    /// 키워드 효과 타입 — 증강/유물이 공유하는 수치 효과
    /// 행동 변경(Spread, Pierce 등)은 Phase BK에서 BehaviorKeyword enum으로 이관
    /// </summary>
    public enum KeywordType
    {
        None,

        // ── 위력 ──
        PowerMul,                   // 위력 배율 (1.5 = 1.5배)
        PowerAdd,                   // 위력 가산 (+5)

        // ── 코스트 ──
        CostAdd,                    // 코스트 변화 (+1, -1)

        // ── 드로우 ──
        DrawWeightOverride,         // 드로우 가중치 덮어쓰기 (0 = 무조건)
        DrawWeightAdd,              // 드로우 가중치 가산

        // ── 효과 배율 ──
        ShieldMul,                  // 쉴드 배율
        HealMul,                    // 힐 배율
        EffectMul,                  // 버프/디버프 효과 배율
        DurationAdd,                // 상태이상 지속시간 추가 (턴)

        // ── 턴 기반 ──
        HPPerTurn,                  // 매 턴 HP 변화 (음수=감소, 양수=회복)
        ShieldPerTurn,              // 매 턴 쉴드 획득

        // ── 피해 관련 ──
        DamageTakenMul,             // 받는 피해 배율 (1.5 = +50%)
        BonusOutgoingDamage,        // 추가 고정 데미지
        DamageReduction,            // 고정 피해 감소
        CounterDamage,              // 반사 피해

        // ── 처치/흡혈 ──
        OnKillHeal,                 // 처치 시 HP 회복
        DamageDealtHealPercent,     // 준 데미지의 % 회복 (0.3 = 30%)
        StackingPowerOnKill,        // 처치당 공격력 누적

        // ── 스탯 영구 증가 (전투 시작 시) ──
        MaxHPUp,                    // 최대 HP 증가
        ATKUp,                      // ATK 영구 증가
        DEFUp,                      // DEF 영구 증가

        // ── AP ──
        ExtraAP,                    // 매 턴 추가 AP

        // ── 경제 ──
        BonusGold,                  // 골드 획득 시 추가 골드

        // ── Phase CC-2A: Umbra 특성 전용 ──
        ShadowsMaxUp,               // 자원 최대치 증가 (value = 보너스 양). "그림자 심화" 특성.
        PowerAddVsDebuff,           // 도트 디버프 적 대상 위력 가산 (value = +N). "약점 포착" 특성.

        // ── Phase CC-2B: Aster 특성 전용 ──
        ComboMaxPowerBonus,         // Combo가 최대치일 때 모든 스킬 위력 가산 (value = +N). "명사수" 특성.
        PowerAddVsMark,             // Mark 상태 적 대상 위력 가산 (value = +N). "약점 포착" 특성.

        // ── Phase CC-2C: Elara 특성 전용 ──
        AutoHealBonus,              // 연결고리 자동 힐 위력 가산 (value = +N). "축복" 특성.
        MercyCleanseBonus,          // 정화 시 Mercy 추가 축전 (value = +N). "순수 치유" 특성.
        MercyBurstTargets,          // Mercy 버스트 대상 수 (value = N). "신성 방패" 특성. 기본 1.

        // ── Phase CC-2D: Calliope 특성 전용 ──
        EchoPowerMul,               // 부 선율 배율 (value = 0.75 등). "전투 노래" 특성. 기본 0.5.
        RepeatNoPenalty,            // 같은 스킬 연속 시 부 선율 무효화 페널티 제거 (value = 1 플래그). "용기의 화음" 특성.
        EchoBonusEffect,            // EchoMelody 발동 시 추가 효과 (value = 1 플래그). "치유 멜로디" 특성.

        // ── Phase CC-2E: Cael 특성 전용 ──
        DiscoverChoicesAdd,         // 발견 선택지 수 증가 (value = N). 기본 3 → 3+N. "물약 명인" 특성.
        DiscoverWeightBonus,        // Crippling 카테고리 가중치 배수 (value = 2.0 등). "독성 폭발" 특성.
        DiscoverApplyAll,           // 전투당 1회, 발견 선택지 모두 적용 (value = 1 플래그). "강화 물약" 특성.

        // ── Phase CC-2F: Mortis 특성 전용 ──
        SoulLinkMul,                // Soul Link 회복 비율 배수 (value = 0.75). 기본 0.5. "생명력 흡수" 특성.
        CurseExtraDamage,           // Curse 걸린 적에게 추가 데미지 (value = N). "저주의 대가" 특성.
        CorpseKillEmpower,          // 적 처치 시 시체 영구 강화 +N. "죽음의 수확" 특성.
    }

    /// <summary>
    /// 키워드 발동 조건
    /// </summary>
    public enum KeywordTrigger
    {
        Passive,            // 항상 적용 (EffectivePower/Cost 계산 시)
        OnTurnStart,        // 턴 시작 시
        OnTurnEnd,          // 턴 종료 시
        OnBattleStart,      // 전투 시작 시
        OnDamageDealt,      // 데미지를 줄 때
        OnDamageReceived,   // 데미지를 받을 때
        OnKill,             // 적 처치 시
        OnHealApplied,      // 힐 적용 시
        OnShieldGained,     // 쉴드 획득 시
        OnSkillUsed,        // 스킬 사용 시
        OnGoldEarned,       // 골드 획득 시
        HPBelow,            // HP가 일정 비율 미만일 때만 적용
        OnEnemyLowHP,       // 대상 적 HP가 일정 비율 미만일 때 (ConditionParam = threshold)
        OnRerollUsed,       // 리롤 1회 소비 시
    }

    /// <summary>
    /// 키워드 효과 인스턴스 — 직렬화 가능
    /// 증강/유물 모두 이 구조를 사용하여 수치 효과를 정의
    /// </summary>
    [System.Serializable]
    public struct KeywordEntry
    {
        public KeywordType Type;
        public float Value;
        public KeywordTrigger Trigger;
        public float ConditionParam;   // HPBelow: threshold (0.3 = 30%)

        public KeywordEntry(KeywordType type, float value, KeywordTrigger trigger = KeywordTrigger.Passive, float conditionParam = 0f)
        {
            Type = type;
            Value = value;
            Trigger = trigger;
            ConditionParam = conditionParam;
        }
    }

    /// <summary>
    /// 키워드 해석 유틸리티 — 증강/유물 공통
    /// </summary>
    public static class KeywordResolver
    {
        /// <summary>키워드 목록에서 지정 타입의 합산 값 반환</summary>
        public static float SumKeyword(IReadOnlyList<KeywordEntry> keywords, KeywordType type)
        {
            float total = 0f;
            if (keywords == null) return total;
            for (int i = 0; i < keywords.Count; i++)
            {
                if (keywords[i].Type == type && keywords[i].Trigger == KeywordTrigger.Passive)
                    total += keywords[i].Value;
            }
            return total;
        }

        /// <summary>키워드 목록에서 지정 타입의 곱 배율 반환 (1.0 = 변화없음)</summary>
        public static float MulKeyword(IReadOnlyList<KeywordEntry> keywords, KeywordType type)
        {
            float result = 1f;
            if (keywords == null) return result;
            for (int i = 0; i < keywords.Count; i++)
            {
                if (keywords[i].Type == type && keywords[i].Trigger == KeywordTrigger.Passive)
                    result *= keywords[i].Value;
            }
            return result;
        }

        /// <summary>키워드 목록에서 지정 트리거의 모든 키워드 수집</summary>
        public static List<KeywordEntry> CollectByTrigger(IReadOnlyList<KeywordEntry> keywords, KeywordTrigger trigger)
        {
            var result = new List<KeywordEntry>();
            if (keywords == null) return result;
            for (int i = 0; i < keywords.Count; i++)
            {
                if (keywords[i].Trigger == trigger)
                    result.Add(keywords[i]);
            }
            return result;
        }

        /// <summary>HPBelow 조건 충족 여부</summary>
        public static bool IsHPConditionMet(KeywordEntry kw, int currentHP, int maxHP)
        {
            if (kw.Trigger != KeywordTrigger.HPBelow) return true;
            if (maxHP <= 0) return false;
            return (float)currentHP / maxHP <= kw.ConditionParam;
        }

        /// <summary>HPBelow 조건 키워드만 필터링하여 조건 충족 시 효과 값 합산</summary>
        public static float SumConditional(IReadOnlyList<KeywordEntry> keywords, KeywordType type, int currentHP, int maxHP)
        {
            float total = 0f;
            if (keywords == null) return total;
            for (int i = 0; i < keywords.Count; i++)
            {
                var kw = keywords[i];
                if (kw.Type == type && kw.Trigger == KeywordTrigger.HPBelow && IsHPConditionMet(kw, currentHP, maxHP))
                    total += kw.Value;
            }
            return total;
        }

        /// <summary>키워드 목록에서 특정 타입 존재 여부</summary>
        public static bool HasKeyword(IReadOnlyList<KeywordEntry> keywords, KeywordType type)
        {
            if (keywords == null) return false;
            for (int i = 0; i < keywords.Count; i++)
            {
                if (keywords[i].Type == type) return true;
            }
            return false;
        }
    }
}
