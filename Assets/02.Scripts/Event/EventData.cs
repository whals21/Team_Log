using System.Collections.Generic;
using UnityEngine;

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

        public string EventName => _eventName;
        public string Description => _description;
        public EventType Type => _eventType;
        public IReadOnlyList<EventChoice> Choices => _choices;
    }
}
