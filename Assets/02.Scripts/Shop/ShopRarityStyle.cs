using UnityEngine;

namespace TeamLog.Shop
{
    /// <summary>
    /// ★ Stained Glass Shop UI — Rarity 5종별 시각/청각 스타일 (정적 DB).
    /// Common / Rare / Unique / Relic / Cursed.
    ///
    /// ShopSlot은 ContentType(Augment/Relic)만 가지고 Rarity는 별도 추정해야 함:
    ///   - ContentType == Relic → Relic
    ///   - ContentType == Augment → AugmentData.Tier 기반 (1=Common, 2=Rare, 3=Unique)
    ///   - IsCursed 플래그가 있으면 Cursed가 최우선 (저주 증강)
    /// </summary>
    public static class ShopRarityStyle
    {
        public static ShopRarityVisual Get(ShopRarity rarity)
        {
            return rarity switch
            {
                ShopRarity.Common => new ShopRarityVisual
                {
                    Rarity = ShopRarity.Common,
                    DisplayName = "COMMON",
                    EmblemSymbol = "·",
                    BorderColor = HexColor("#9d9d9d"),
                    GlowColor    = HexColor("#c0c0c0"),
                    TextColor    = HexColor("#dcdcdc"),
                    GlowIntensity = 0f
                },
                ShopRarity.Rare => new ShopRarityVisual
                {
                    Rarity = ShopRarity.Rare,
                    DisplayName = "RARE",
                    EmblemSymbol = "◆",
                    BorderColor = HexColor("#4a9eff"),
                    GlowColor    = HexColor("#7ab8ff"),
                    TextColor    = HexColor("#9dc4ff"),
                    GlowIntensity = 0.5f
                },
                ShopRarity.Unique => new ShopRarityVisual
                {
                    Rarity = ShopRarity.Unique,
                    DisplayName = "UNIQUE",
                    EmblemSymbol = "★",
                    BorderColor = HexColor("#c084fc"),
                    GlowColor    = HexColor("#d8b4fe"),
                    TextColor    = HexColor("#e0c4ff"),
                    GlowIntensity = 0.8f
                },
                ShopRarity.Relic => new ShopRarityVisual
                {
                    Rarity = ShopRarity.Relic,
                    DisplayName = "RELIC",
                    EmblemSymbol = "◈",
                    BorderColor = HexColor("#f4d35e"),
                    GlowColor    = HexColor("#ffe89a"),
                    TextColor    = HexColor("#f4d35e"),
                    GlowIntensity = 1f
                },
                ShopRarity.Cursed => new ShopRarityVisual
                {
                    Rarity = ShopRarity.Cursed,
                    DisplayName = "CURSED",
                    EmblemSymbol = "☠",
                    BorderColor = HexColor("#c0392b"),
                    GlowColor    = HexColor("#ff6b6b"),
                    TextColor    = HexColor("#ff8888"),
                    GlowIntensity = 0.7f,
                    IsCursed = true
                },
                _ => Get(ShopRarity.Common)
            };
        }

        /// <summary>
        /// ShopSlot에서 Rarity 추정 — ContentType + Tier + IsCursed 기반.
        /// </summary>
        public static ShopRarity EstimateFromSlot(ShopSlot slot)
        {
            if (slot == null) return ShopRarity.Common;

            // Relic는 항상 Relic
            if (slot.ContentType == ShopSlot.SlotContentType.Relic)
                return ShopRarity.Relic;

            // Augment — IsCursed 최우선
            if (slot.Augment != null)
            {
                if (slot.Augment.IsCursed)
                    return ShopRarity.Cursed;

                int tier = Mathf.Max(1, slot.Augment.Tier);
                return tier switch
                {
                    1 => ShopRarity.Common,
                    2 => ShopRarity.Rare,
                    _ => ShopRarity.Unique
                };
            }

            return ShopRarity.Common;
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
    /// 상점 Rarity 5종.
    /// </summary>
    public enum ShopRarity
    {
        Common,
        Rare,
        Unique,
        Relic,
        Cursed
    }

    /// <summary>
    /// 단일 Rarity 시각 정보.
    /// </summary>
    public struct ShopRarityVisual
    {
        public ShopRarity Rarity;
        public string DisplayName;
        public string EmblemSymbol;
        public Color BorderColor;       // 슬롯 테두리
        public Color GlowColor;         // 글로우 (밝은 쪽)
        public Color TextColor;         // 배지 텍스트 색
        public float GlowIntensity;     // 0~1 — 펄스 효과 강도
        public bool IsCursed;           // 저주 — 경고 표시
    }
}
