using System.Collections.Generic;
using UnityEngine;
using TeamLog.Skill; // Phase BK: BehaviorTag 참조

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

        [Header("아이콘")]
        [SerializeField] private Sprite _icon;

        [Header("추가 효과")]
        [SerializeField] private StatusEffectType _statusEffect;
        [SerializeField] private int _effectDuration;
        [SerializeField] private int _effectValue;

        [Header("행동 키워드 (Phase BK)")]
        [SerializeField] private BehaviorTag[] _behaviors = new BehaviorTag[0];

        [Header("자원 (Phase CC — 캐릭터 고유 메카닉)")]
        [Tooltip("스킬 사용 시 획득하는 자원 스택 (양수). 자원 없는 캐릭터는 None.")]
        [SerializeField] private ResourceType _resourceGainType = ResourceType.None;
        [SerializeField] private int _resourceGainAmount;

        [Tooltip("스킬 사용 시 소모하는 자원 스택 (양수). 소모 불가 시 스킬 사용 불가.")]
        [SerializeField] private ResourceType _resourceCostType = ResourceType.None;
        [SerializeField] private int _resourceCostAmount;

        [Tooltip("자원 1스택당 추가 위력 (Phase CC). Brand of Ash=3(Ember×3), Revenge Strike=1(Vengearce×1) 등. 0이면 비활성.")]
        [SerializeField] private int _resourcePowerPerStack;

        [Tooltip("자원 전량 소모 여부 (Phase CC). true면 ResourceCostAmount 무시하고 보유 스택 전부 소모. Revenge Strike용.")]
        [SerializeField] private bool _consumeAllResource;

        [Header("쉴드 속성 (Phase CC P1 — Shield 타입 전용)")]
        [Tooltip("GivesChargeOnAbsorb: 쉴드 흡수 시 공격자에게 Charge 부여 (Taranis Grounding Field).")]
        [SerializeField] private ShieldFlag _shieldFlags = ShieldFlag.None;

        #region Properties
        public string SkillName => _skillName;
        public string Description => _description;
        public SkillType Type => _skillType;
        public TargetType Target => _targetType;
        public int Power => _power;
        public int Cost => _cost;
        public int Weight => _weight;
        public Sprite Icon => _icon;
        public StatusEffectType StatusEffect => _statusEffect;
        public int EffectDuration => _effectDuration;
        public int EffectValue => _effectValue;
        public IReadOnlyList<BehaviorTag> Behaviors => _behaviors ?? System.Array.Empty<BehaviorTag>();

        // Phase CC 자원 프로퍼티
        public ResourceType ResourceGainType => _resourceGainType;
        public int ResourceGainAmount => _resourceGainAmount;
        public ResourceType ResourceCostType => _resourceCostType;
        public int ResourceCostAmount => _resourceCostAmount;
        public int ResourcePowerPerStack => _resourcePowerPerStack;
        public bool ConsumeAllResource => _consumeAllResource;

        // Phase CC P1: 쉴드 속성 (Shield 타입 전용)
        public ShieldFlag ShieldFlags => _shieldFlags;
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
        Debuff,     // 디버프
        Shield,     // 쉴드 (일시적 보호막)
        Purify      // 정화 (상태이상 제거)
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
}
