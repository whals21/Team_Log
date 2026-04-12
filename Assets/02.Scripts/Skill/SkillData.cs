using UnityEngine;

namespace TeamLog.Characters
{
    /// <summary>
    /// 스킬 정적 데이터 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "SkillData", menuName = "TeamLog/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string _skillName;
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        [Header("스킬 타입")]
        [SerializeField] private SkillType _skillType;
        [SerializeField] private TargetType _targetType;

        [Header("수치")]
        [SerializeField] private int _power = 10;
        [SerializeField] private int _cost = 0;

        [Header("드로우 가중치")]
        [Range(1, 100)]
        [SerializeField] private int _weight = 50;

        [Header("추가 효과")]
        [SerializeField] private StatusEffect _statusEffect;
        [SerializeField] private int _effectDuration;
        [SerializeField] private int _effectValue;

        #region Properties
        public string SkillName => _skillName;
        public string Description => _description;
        public SkillType Type => _skillType;
        public TargetType Target => _targetType;
        public int Power => _power;
        public int Cost => _cost;
        public int Weight => _weight;
        public StatusEffect StatusEffect => _statusEffect;
        public int EffectDuration => _effectDuration;
        public int EffectValue => _effectValue;
        #endregion
    }

    /// <summary>
    /// 스킬 타입
    /// </summary>
    public enum SkillType
    {
        Attack,     // 공격
        Heal,       // 치유
        Buff,       // 버프
        Debuff      // 디버프
    }

    /// <summary>
    /// 타겟 타입
    /// </summary>
    public enum TargetType
    {
        SingleEnemy,    // 단일 적
        AllEnemies,     // 전체 적
        SingleAlly,     // 단일 아군
        AllAllies,      // 전체 아군
        Self            // 자신
    }

    /// <summary>
    /// 상태이상 타입
    /// </summary>
    public enum StatusEffect
    {
        None,
        Poison,     // 독
        Burn,       // 화상
        Stun,       // 기절
        Bleed,      // 출혈
        DefenseUp,  // 방어 증가
        DefenseDown,// 방어 감소
        AttackUp,   // 공격 증가
        AttackDown  // 공격 감소
    }
}
