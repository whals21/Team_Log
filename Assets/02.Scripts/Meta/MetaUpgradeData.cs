using UnityEngine;

namespace TeamLog.Meta
{
    /// <summary>
    /// 메타 강화 유형 — 일회성 영구 해금.
    /// RelicUnlock: 개별 유물 해금 (드롭 풀에 추가)
    /// StartingRelicSlot: 런 시작 시 유물 1개 지급
    /// StartingRelicChoice: 런 시작 시 유물 3개 중 1개 선택
    /// ExtraReroll: 턴당 리롤 +1
    /// PartyHealBoost: 휴식지 힐량 +10%
    /// </summary>
    public enum MetaUpgradeType
    {
        RelicUnlock,
        StartingRelicSlot,
        StartingRelicChoice,
        ExtraReroll,
        PartyHealBoost
    }

    /// <summary>
    /// 일회성 메타 강화 정적 데이터 (ScriptableObject).
    /// 기억의 조각/영혼으로 해금. Phase 8A: DataGenerator.MetaUpgrades.cs에서 46종 생성.
    /// </summary>
    [CreateAssetMenu(fileName = "MetaUpgrade", menuName = "TeamLog/Meta Upgrade")]
    public class MetaUpgradeData : ScriptableObject
    {
        [Header("식별자")]
        [SerializeField] private string _upgradeId;
        [SerializeField] private string _displayName;
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        [Header("강화 타입")]
        [SerializeField] private MetaUpgradeType _type;

        [Header("비용")]
        [SerializeField] private int _memoryCost;
        [SerializeField] private int _soulCost;

        [Header("RelicUnlock 전용")]
        [Tooltip("RelicUnlock 타입일 때 대상 유물 에셋 (에셋 경로로 저장)")]
        [SerializeField] private string _targetRelicId;

        public string UpgradeId => _upgradeId;
        public string DisplayName => _displayName;
        public string Description => _description;
        public MetaUpgradeType Type => _type;
        public int MemoryCost => _memoryCost;
        public int SoulCost => _soulCost;
        public string TargetRelicId => _targetRelicId;
    }
}
