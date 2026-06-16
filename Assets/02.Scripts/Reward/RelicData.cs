using System.Collections.Generic;
using UnityEngine;
using TeamLog.Skill;

namespace TeamLog.Reward
{
    public enum RelicTrigger
    {
        None,
        BattleStart,
        TurnStart,
        TurnEnd,
        OnDamageDealt,
        OnDamageReceived,
        OnKill,
        OnHealApplied,
        OnShieldGained,
        OnGoldEarned,
        OnSkillUsed,
        OnRerollUsed
    }

    /// <summary>
    /// 유물 데이터 — 트리거 + 효과 조합
    /// </summary>
    [CreateAssetMenu(fileName = "RelicData", menuName = "TeamLog/Relic Data")]
    public class RelicData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string _relicName;
        [TextArea(2, 3)]
        [SerializeField] private string _description;
        [SerializeField] private Sprite _icon;

        [Header("트리거")]
        [SerializeField] private RelicTrigger _trigger;

        [Header("효과")]
        [SerializeField] private int _effectValue;
        [SerializeField] private RewardRarity _rarity;

        [Header("가격")]
        [SerializeField] private int _price;

        [Header("키워드 효과")]
        [SerializeField] private KeywordEntry[] _keywords;

        public string RelicName => _relicName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public RelicTrigger Trigger => _trigger;
        public int EffectValue => _effectValue;
        public RewardRarity Rarity => _rarity;
        public int Price => _price;
        public IReadOnlyList<KeywordEntry> Keywords => _keywords;
    }
}
