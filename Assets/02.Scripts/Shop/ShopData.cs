using UnityEngine;
using TeamLog.Characters;
using TeamLog.Reward;

namespace TeamLog.Shop
{
    /// <summary>
    /// 상점 슬롯 — 판매 아이템/스킬 하나
    /// </summary>
    [System.Serializable]
    public class ShopSlot
    {
        public enum SlotContentType { Skill, Item }

        public SlotContentType ContentType;
        public SkillData Skill;
        public ItemData Item;
        public int Price;
        public bool IsSold;

        public string Name => ContentType == SlotContentType.Skill ?
            (Skill != null ? Skill.SkillName : "???") :
            (Item != null ? Item.ItemName : "???");

        public string Desc => ContentType == SlotContentType.Skill ?
            (Skill != null ? Skill.Description : "") :
            (Item != null ? Item.Description : "");

        public int EffectValue => ContentType == SlotContentType.Skill ?
            (Skill != null ? Skill.Power : 0) :
            (Item != null ? Item.EffectValue : 0);
    }

    /// <summary>
    /// 상점 정적 데이터 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "ShopData", menuName = "TeamLog/Shop Data")]
    public class ShopData : ScriptableObject
    {
        [Header("상점 구성")]
        [SerializeField] private ShopSlot[] _slots;

        public ShopSlot[] Slots => _slots;
    }
}
