using System.Collections.Generic;
using UnityEngine;
using TeamLog.Characters;

namespace TeamLog.Event
{
    /// <summary>
    /// 이벤트 분류
    /// </summary>
    public enum EventType
    {
        Story,      // 이야기
        Treasure,   // 보물
        Trap,       // 함정
        NPC,        // NPC 조우
        Shrine      // 신전
    }

    /// <summary>
    /// 이벤트 위험도 (UI 색상 코딩용)
    /// </summary>
    public enum EventRiskLevel
    {
        Safe,       // 안전 (회색) — 순수 이익
        Normal,     // 일반 (기본) — 일반적 선택지
        Gamble,     // 도박 (노랑) — 확률 기반
        Dangerous   // 위험 (빨강) — 큰 손실 가능
    }

    /// <summary>
    /// 이벤트 선택지 결과
    /// </summary>
    [System.Serializable]
    public class EventOutcome
    {
        [TextArea(2, 3)]
        public string ResultText;
        public int GoldChange;
        [Range(-100, 100)]
        public int HPPercentChange;     // 파티 전체 HP 비율 변화 (-50% ~ +50%)
        public bool GiveRandomSkill;
        public bool GiveRandomItem;

        [Header("영구 강화 (런 내 영구)")]
        public int PermanentAtkBonus;   // 파티 전원 ATK 영구 증가
        public int PermanentDefBonus;   // 파티 전원 DEF 영구 증가
        public int RerollTokensBonus;   // GameRunState.RerollTokens 가산

        [Header("저주 / 부정적 효과")]
        public StatusEffectType ApplyStatusEffect;
        public int StatusEffectDuration;
        public int StatusEffectValue;

        [Header("확률 기반 결과 (선택)")]
        [Tooltip("비어있으면 단일 결과. 있으면 OutcomeWeights 기반 추첨 (비어있으면 균등)")]
        public List<EventOutcome> RandomOutcomes = new();
        public List<float> OutcomeWeights = new();

        [Header("연쇄 (분기) 이벤트")]
        [Tooltip("비어있으면 이벤트 종료. 있으면 해당 ID의 이벤트를 이어서 표시")]
        public string NextEventId;

        /// <summary>
        /// 위험도 자동 분류 — Outcome 내용 기반
        /// </summary>
        public EventRiskLevel GetRiskLevel()
        {
            if (RandomOutcomes != null && RandomOutcomes.Count > 0)
                return EventRiskLevel.Gamble;
            if (HPPercentChange < -15 || PermanentAtkBonus < 0 || PermanentDefBonus < 0)
                return EventRiskLevel.Dangerous;
            if (GoldChange >= 0 && HPPercentChange >= 0 && PermanentAtkBonus >= 0 && PermanentDefBonus >= 0)
                return EventRiskLevel.Safe;
            return EventRiskLevel.Normal;
        }
    }

    /// <summary>
    /// 이벤트 선택지
    /// </summary>
    [System.Serializable]
    public class EventChoice
    {
        public string ChoiceText;
        [TextArea(2, 3)]
        public string ChoiceDescription;

        [Header("등장 조건 (0/기본값 = 제한 없음)")]
        public int MinGoldRequired;
        [Range(0f, 1f)]
        public float MinPartyHPPercent;     // 0 = 제한 없음
        public int RequiresAliveMembers;    // 0 = 제한 없음

        public EventOutcome Outcome;
    }

    /// <summary>
    /// 이벤트 정적 데이터 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "EventData", menuName = "TeamLog/Event Data")]
    public class EventData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string _eventName;
        [TextArea(3, 6)]
        [SerializeField] private string _description;
        [SerializeField] private EventType _eventType;

        [Header("선택지")]
        [SerializeField] private List<EventChoice> _choices = new();

        [Header("등장 제어 (Phase E1)")]
        [Tooltip("높을수록 자주 등장. 기본 10")]
        [SerializeField] private int _weight = 10;
        [Tooltip("비어있으면 공통 이벤트. 테마 ID(예: S1_GreyForest) 지정 시 해당 테마에서만 등장")]
        [SerializeField] private string _exclusiveThemeId = "";

        public string EventName => _eventName;
        public string Description => _description;
        public EventType Type => _eventType;
        public IReadOnlyList<EventChoice> Choices => _choices;
        public int Weight => _weight;
        public string ExclusiveThemeId => _exclusiveThemeId ?? "";
    }
}
