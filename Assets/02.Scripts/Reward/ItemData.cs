using UnityEngine;

namespace TeamLog.Reward
{
    /// <summary>
    /// 아이템 타입
    /// </summary>
    public enum ItemType
    {
        PassiveBuff,     // 영구 버프
        Consumable,      // 소모품
        Relic            // 유물
    }

    /// <summary>
    /// 아이템 효과 타입
    /// </summary>
    public enum ItemEffectType
    {
        MaxHPUp,
        ATKUp,
        DEFUp,
        HealPerTurn,
        ExtraGold,
        DrawWeightBonus
    }

    /// <summary>
    /// 아이템 정적 데이터 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData", menuName = "TeamLog/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string _itemName;
        [TextArea(2, 3)]
        [SerializeField] private string _description;

        [Header("아이템 타입")]
        [SerializeField] private ItemType _itemType;
        [SerializeField] private ItemEffectType _effectType;
        [SerializeField] private int _effectValue;

        [Header("가격")]
        [SerializeField] private int _price;
        [SerializeField] private RewardRarity _rarity;

        public string ItemName => _itemName;
        public string Description => _description;
        public ItemType Type => _itemType;
        public ItemEffectType EffectType => _effectType;
        public int EffectValue => _effectValue;
        public int Price => _price;
        public RewardRarity Rarity => _rarity;
    }
}
