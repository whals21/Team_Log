using UnityEngine;
using TeamLog.Characters;

namespace TeamLog.Reward
{
    /// <summary>
    /// 보상 희귀도
    /// </summary>
    public enum RewardRarity
    {
        Common,
        Rare,
        Unique
    }

    /// <summary>
    /// 보상 타입
    /// </summary>
    public enum RewardType
    {
        Gold,
        Skill,
        Item
    }

    /// <summary>
    /// 보상 정적 데이터 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "RewardData", menuName = "TeamLog/Reward Data")]
    public class RewardData : ScriptableObject
    {
        [Header("보상 정보")]
        [SerializeField] private RewardType _rewardType;
        [SerializeField] private RewardRarity _rarity;
        [TextArea(2, 3)]
        [SerializeField] private string _description;

        [Header("골드 보상")]
        [SerializeField] private int _goldMin;
        [SerializeField] private int _goldMax;

        [Header("스킬 보상")]
        [SerializeField] private SkillData _skill;

        [Header("아이템 보상")]
        [SerializeField] private ItemData _item;

        public RewardType Type => _rewardType;
        public RewardRarity Rarity => _rarity;
        public string Description => _description;
        public int GoldMin => _goldMin;
        public int GoldMax => _goldMax;
        public SkillData Skill => _skill;
        public ItemData Item => _item;
    }
}
