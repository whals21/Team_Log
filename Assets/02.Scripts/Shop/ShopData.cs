using UnityEngine;
using TeamLog.Reward;
using TeamLog.Skill;

namespace TeamLog.Shop
{
    /// <summary>
    /// 상점 슬롯 — 판매 증강/유물 하나
    /// </summary>
    [System.Serializable]
    public class ShopSlot
    {
        public enum SlotContentType { Augment, Relic }

        public SlotContentType ContentType;
        public AugmentData Augment;
        public RelicData Relic;
        public int Price;
        public bool IsSold;

        public string Name => ContentType == SlotContentType.Augment ?
            (Augment != null ? Augment.AugmentName : "???") :
            (Relic != null ? Relic.RelicName : "???");

        public string Desc => ContentType == SlotContentType.Augment ?
            (Augment != null ? Augment.Description : "") :
            (Relic != null ? Relic.Description : "");

        public int EffectValue => ContentType == SlotContentType.Augment ?
            (int)(Augment != null ? Augment.Tier : 0) :
            (Relic != null ? Relic.EffectValue : 0);

        public Sprite Icon => ContentType == SlotContentType.Augment ?
            (Augment != null ? Augment.Icon : null) :
            (Relic != null ? Relic.Icon : null);
    }
}
