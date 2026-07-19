using UnityEngine;

namespace TeamLog.Shop
{
    /// <summary>
    /// ★ Stained Glass Shop UI — SlotType(Augment/Relic) 2종 스타일 (정적 DB).
    /// ShopSlot.ContentType 기반으로 슬롯 상단 띠 색상 + 엠블럼 결정.
    /// </summary>
    public static class ShopSlotTypeStyle
    {
        public static ShopSlotTypeVisual Get(ShopSlot.SlotContentType type)
        {
            return type switch
            {
                ShopSlot.SlotContentType.Augment => new ShopSlotTypeVisual
                {
                    Type = ShopSlot.SlotContentType.Augment,
                    DisplayName = "AUGMENT",
                    DefaultEmblem = "✦",
                    AccentColor = HexColor("#c98a3a"),     // 호박색
                    BgTint = new Color(0.4f, 0.25f, 0.1f, 0.4f)
                },
                ShopSlot.SlotContentType.Relic => new ShopSlotTypeVisual
                {
                    Type = ShopSlot.SlotContentType.Relic,
                    DisplayName = "RELIC",
                    DefaultEmblem = "◈",
                    AccentColor = HexColor("#d4af37"),     // 황금
                    BgTint = new Color(0.4f, 0.3f, 0.08f, 0.4f)
                },
                _ => Get(ShopSlot.SlotContentType.Augment)
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
    /// 단일 SlotType 시각 정보.
    /// </summary>
    public struct ShopSlotTypeVisual
    {
        public ShopSlot.SlotContentType Type;
        public string DisplayName;
        public string DefaultEmblem;
        public Color AccentColor;     // 슬롯 상단 띠 / 좌측 테두리
        public Color BgTint;          // 배경 틴트 (어두운 배경 위에 얹히는 색)
    }
}
