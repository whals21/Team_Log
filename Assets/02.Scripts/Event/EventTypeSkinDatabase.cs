using UnityEngine;

namespace TeamLog.Event
{
    /// <summary>
    /// ★ Stained Glass Event UI — EventType별 스킨 매핑 (정적 DB).
    /// Story / Treasure / Trap / NPC / Shrine 5종.
    /// 각 타입은 고유 색상 + 엠블럼 기호 + GlassWindow Sprite 이름 + 주요 사운드 ID 보유.
    ///
    /// 사용처:
    ///   - EventReworkView.ShowEvent — 표시 중인 EventData.EventType으로 스킨을 가져와 적용
    ///   - EventSceneReworkBuilder — GlassWindow에 Sprite를 동적 할당
    /// </summary>
    public static class EventTypeSkinDatabase
    {
        /// <summary>
        /// EventType → 스킨 정보 조회. unknown은 Shrine 폴백.
        /// </summary>
        public static EventTypeSkin Get(EventType type)
        {
            return type switch
            {
                EventType.Story    => new EventTypeSkin
                {
                    Type = EventType.Story,
                    DisplayName = "STORY",
                    EmblemSymbol = "✎",
                    PrimaryColor = HexColor("#6e7a9c"),      // 차분한 청회
                    GlowColor    = HexColor("#9ba8c8"),
                    GlassWindowSprite = "GlassWindow_Story.png",
                    AmbientSfxId = "event_story_ambient",
                    Motif = "깃펜 + 잉크 — 회상의 분위기"
                },
                EventType.Treasure => new EventTypeSkin
                {
                    Type = EventType.Treasure,
                    DisplayName = "TREASURE",
                    EmblemSymbol = "◆",
                    PrimaryColor = HexColor("#d4af37"),      // 황금
                    GlowColor    = HexColor("#f4d35e"),
                    GlassWindowSprite = "GlassWindow_Treasure.png",
                    AmbientSfxId = "event_treasure_ambient",
                    Motif = "보석 + 황금 — 탐욕의 유혹"
                },
                EventType.Trap     => new EventTypeSkin
                {
                    Type = EventType.Trap,
                    DisplayName = "TRAP",
                    EmblemSymbol = "☠",
                    PrimaryColor = HexColor("#a83232"),      // 적갈
                    GlowColor    = HexColor("#c0392b"),
                    GlassWindowSprite = "GlassWindow_Trap.png",
                    AmbientSfxId = "event_trap_ambient",
                    Motif = "해골 + 가시 — 위험의 경고"
                },
                EventType.NPC      => new EventTypeSkin
                {
                    Type = EventType.NPC,
                    DisplayName = "ENCOUNTER",
                    EmblemSymbol = "◉",
                    PrimaryColor = HexColor("#c98a3a"),      // 따뜻한 호박
                    GlowColor    = HexColor("#e0a85a"),
                    GlassWindowSprite = "GlassWindow_NPC.png",
                    AmbientSfxId = "event_npc_ambient",
                    Motif = "인장 + 왁스 — 낯선 자와의 조우"
                },
                EventType.Shrine   => new EventTypeSkin
                {
                    Type = EventType.Shrine,
                    DisplayName = "SHRINE",
                    EmblemSymbol = "✦",
                    PrimaryColor = HexColor("#6fa8a3"),      // 청록 신성
                    GlowColor    = HexColor("#9bc8c4"),
                    GlassWindowSprite = "GlassWindow_Shrine.png",
                    AmbientSfxId = "event_shrine_ambient",
                    Motif = "룬문자 + 후광 — 경외의 장소"
                },
                _                  => Get(EventType.Shrine)
            };
        }

        private static Color HexColor(string hex)
        {
            hex = hex.Replace("#", "");
            byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
            return new Color32(r, g, b, 255);
        }
    }

    /// <summary>
    /// 단일 EventType 스킨 정보.
    /// </summary>
    public struct EventTypeSkin
    {
        public EventType Type;
        public string DisplayName;          // UI 상 대문자 라벨
        public string EmblemSymbol;         // GlassWindow 중앙 엠블럼 (1~2글자)
        public Color PrimaryColor;          // 주 색상 — 엠블럼/타입 라벨
        public Color GlowColor;             // 글로우 색상 (밝은 쪽)
        public string GlassWindowSprite;    // EventSceneSpriteGenerator 출력 파일명
        public string AmbientSfxId;         // 환경음 ID (AudioManager에서 사용)
        public string Motif;                // 디자인 의도 (에디터 노트용)
    }
}
