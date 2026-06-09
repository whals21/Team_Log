using System.Collections.Generic;
using UnityEngine;
using TeamLog.Characters;

namespace TeamLog.Skill
{
    /// <summary>
    /// 증강 정적 데이터 (ScriptableObject)
    /// 스킬에 부착하여 효과를 강화하거나 변형하는 아이템
    /// </summary>
    [CreateAssetMenu(fileName = "AugmentData", menuName = "TeamLog/Augment")]
    public class AugmentData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string _augmentName;
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        [Header("증강 타입")]
        [SerializeField] private AugmentType _type;

        [Header("호환성")]
        [SerializeField] private SkillType _compatibleSkillType; // All이면 모든 스킬 호환

        [Header("등급")]
        [SerializeField] private int _tier = 1; // 1=일반, 2=희귀, 3=전설

        [Header("아이콘")]
        [SerializeField] private Sprite _icon;

        [Header("저주")]
        [SerializeField] private bool _isCursed;
        [TextArea(1, 3)]
        [SerializeField] private string _curseDescription;

        [Header("키워드 효과")]
        [SerializeField] private KeywordEntry[] _keywords;

        #region Properties
        public string AugmentName => _augmentName;
        public string Description => _description;
        public AugmentType Type => _type;
        public SkillType CompatibleSkillType => _compatibleSkillType;
        public int Tier => _tier;
        public Sprite Icon => _icon;
        public bool IsCursed => _isCursed;
        public string CurseDescription => _curseDescription;
        public IReadOnlyList<KeywordEntry> Keywords => _keywords;
        #endregion
    }
}
