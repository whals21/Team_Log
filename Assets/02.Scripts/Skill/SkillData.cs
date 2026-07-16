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

        [Tooltip("스킬 사용에 필요한 최소 자원 스택 (Phase CC-2A — Umbra Eviscerate용). 0이면 비활성. Eviscerate=3 (Shadows 최대치).")]
        [SerializeField] private int _minResourceRequired;

        [Header("쉴드 속성 (Phase CC P1 — Shield 타입 전용)")]
        [Tooltip("GivesChargeOnAbsorb: 쉴드 흡수 시 공격자에게 Charge 부여 (Taranis Grounding Field).")]
        [SerializeField] private ShieldFlag _shieldFlags = ShieldFlag.None;

        [Header("발견 스킬 (Phase CC-2E — Cael Alchemist)")]
        [Tooltip("발견(Discover) 스킬 여부. true면 시전 시 모달 팝업으로 3개 선택지 제공.")]
        [SerializeField] private bool _isDiscover;
        [Tooltip("발견 풀 데이터 (발견 스킬 전용). 풀에서 N개 무작위 추출 → 플레이어 선택 → 발동.")]
        [SerializeField] private DiscoverPoolData _discoverPool;

        [Header("시체 스킬 (Phase CC-2F — Mortis Necromancer)")]
        [Tooltip("시체 기본 스킬 여부. Necromancer 시체가 매 턴 무작위로 시전하는 스킬.")]
        [SerializeField] private bool _isCorpseSkill;
        [Tooltip("어느 적에게서 빼앗은 스킬인지 추적용 (시체 스킬 교체 시). 빈 문자열 = 기본 시체 스킬.")]
        [SerializeField] private string _sourceEnemyId = "";
        [Tooltip("Mortis 본인 스킬의 시체 액션 종류 — EmpowerNext/MassEmpower/SoulLink. 시전 후 Pipeline이 시체에 효과 적용.")]
        [SerializeField] private CorpseActionType _corpseAction = CorpseActionType.None;
        [Tooltip("시체 액션 수치 — EmpowerNext: 위력 가산, MassEmpower: 영구 가산, SoulLink: 회복 비율(0.5=50%).")]
        [SerializeField] private int _corpseActionValue;

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
        public int MinResourceRequired => _minResourceRequired;

        // Phase CC P1: 쉴드 속성 (Shield 타입 전용)
        public ShieldFlag ShieldFlags => _shieldFlags;

        // Phase CC-2E: 발견 스킬 (Cael Alchemist)
        public bool IsDiscover => _isDiscover;
        public DiscoverPoolData DiscoverPool => _discoverPool;

        // Phase CC-2F: 시체 스킬 (Mortis Necromancer)
        public bool IsCorpseSkill => _isCorpseSkill;
        public string SourceEnemyId => _sourceEnemyId;
        public CorpseActionType CorpseAction => _corpseAction;
        public int CorpseActionValue => _corpseActionValue;
        #endregion
    }

    /// <summary>
    /// Phase CC-2F: Mortis 본인 스킬이 시체에 적용하는 효과 종류.
    /// Pipeline.ExecuteSkill 끝에서 처리 — caster.Corpse가 null이 아닐 때만 동작.
    /// </summary>
    public enum CorpseActionType
    {
        None,           // 시체 액션 없음 (일반 스킬/시체 기본 스킬)
        EmpowerNext,    // 다음 시체 스킬 위력 +N (1회 버프)
        MassEmpower,    // 시체 모든 스킬 위력 +N (이번 전투 영구)
        SoulLink,       // 2턴간 시체가 준 데미지 N%를 Necromancer HP 회복
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
