using UnityEngine;

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
        OnSkillUsed
    }

    public enum RelicEffectType
    {
        None,
        BonusDamage,
        DamageReduction,
        HealPerTurn,
        BonusGold,
        BonusShield,
        BonusDrawWeight,
        CounterDamage,
        ExtraAP,
        HealOnKill,
        StackingPowerOnKill
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
        [SerializeField] private RelicEffectType _effectType;
        [SerializeField] private int _effectValue;
        [SerializeField] private RewardRarity _rarity;

        public string RelicName => _relicName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public RelicTrigger Trigger => _trigger;
        public RelicEffectType EffectType => _effectType;
        public int EffectValue => _effectValue;
        public RewardRarity Rarity => _rarity;
    }
}
