using UnityEngine;

namespace TeamLog.Event
{
    /// <summary>
    /// ★ Stained Glass Event UI — RiskLevel별 시각/청각 스타일 (정적 DB).
    /// Safe / Normal / Gamble / Dangerous 4종.
    ///
    /// 위험도는 EventOutcome.GetRiskLevel()이 자동 분류하므로,
    /// 본 DB는 분류된 값을 기반으로 선택지 행의 테두리 색/이모티콘/애니메이션/사운드를 제공.
    /// </summary>
    public static class EventRiskStyle
    {
        public static EventRiskVisual Get(EventRiskLevel risk)
        {
            return risk switch
            {
                EventRiskLevel.Safe => new EventRiskVisual
                {
                    Risk = EventRiskLevel.Safe,
                    DisplayName = "SAFE",
                    EmblemSymbol = "☘",
                    BorderColor = HexColor("#5d9b6f"),       // 부드러운 녹색
                    GlowColor    = HexColor("#7dc090"),
                    TextColor    = HexColor("#9bd8ad"),
                    Pulse = false,
                    Shake = false,
                    ClickSfxId = "event_risk_safe_click"
                },
                EventRiskLevel.Normal => new EventRiskVisual
                {
                    Risk = EventRiskLevel.Normal,
                    DisplayName = "NORMAL",
                    EmblemSymbol = "·",
                    BorderColor = HexColor("#b8b8b8"),       // 중간 회색
                    GlowColor    = HexColor("#dcdcdc"),
                    TextColor    = HexColor("#d4c5a0"),      // parchment
                    Pulse = false,
                    Shake = false,
                    ClickSfxId = "ui_click"
                },
                EventRiskLevel.Gamble => new EventRiskVisual
                {
                    Risk = EventRiskLevel.Gamble,
                    DisplayName = "GAMBLE",
                    EmblemSymbol = "⚛",
                    BorderColor = HexColor("#d4af37"),       // 황금 펄스
                    GlowColor    = HexColor("#f4d35e"),
                    TextColor    = HexColor("#f4d35e"),
                    Pulse = true,                            // 1Hz 맥동
                    Shake = false,
                    ClickSfxId = "event_risk_gamble_click"
                },
                EventRiskLevel.Dangerous => new EventRiskVisual
                {
                    Risk = EventRiskLevel.Dangerous,
                    DisplayName = "DANGER",
                    EmblemSymbol = "☠",
                    BorderColor = HexColor("#b22222"),       // 강렬한 빨강
                    GlowColor    = HexColor("#ff4444"),
                    TextColor    = HexColor("#ff6b6b"),
                    Pulse = true,
                    Shake = true,                            // 미세 떨림 (위험 경고)
                    ClickSfxId = "event_risk_danger_click"
                },
                _ => Get(EventRiskLevel.Normal)
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
    /// 단일 RiskLevel 시각 정보.
    /// </summary>
    public struct EventRiskVisual
    {
        public EventRiskLevel Risk;
        public string DisplayName;
        public string EmblemSymbol;
        public Color BorderColor;          // 선택지 행 테두리
        public Color GlowColor;            // 글로우 (밝은 쪽)
        public Color TextColor;            // RiskTag 텍스트 색
        public bool Pulse;                 // true면 1Hz 펄스 애니메이션
        public bool Shake;                 // true면 미세 떨림 (Dangerous만)
        public string ClickSfxId;          // 클릭 사운드 ID
    }
}
