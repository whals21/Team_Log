using System.Collections.Generic;
using UnityEngine;
using TeamLog.Skill;

namespace TeamLog.Characters
{
    /// <summary>
    /// 캐릭터 장착형 특성(Loadout) 정적 데이터 (ScriptableObject).
    /// 캐릭터당 1개 장착 가능. KeywordEntry[] 기반 — 증강/유물과 동일한 키워드 시스템 사용.
    /// Phase 8A: 메타프로세션 해금 대상.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterTrait", menuName = "TeamLog/Character Trait")]
    public class CharacterTraitData : ScriptableObject
    {
        [Header("식별자")]
        [SerializeField] private string _traitId;
        [SerializeField] private string _displayName;
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        [Header("대상 직업")]
        [SerializeField] private CharacterClass _targetClass;

        [Header("키워드 효과")]
        [SerializeField] private KeywordEntry[] _keywords;

        [Header("해금 정책")]
        [SerializeField] private bool _isDefault;           // 런 시작 시 기본 장착 가능
        [SerializeField] private int _unlockCost;           // 기억의 조각 비용
        [SerializeField] private int _soulUnlockCost;       // 영혼 비용 (강력 특성만)

        [Header("아이콘")]
        [SerializeField] private Sprite _icon;

        public string TraitId => _traitId;
        public string DisplayName => _displayName;
        public string Description => _description;
        public CharacterClass TargetClass => _targetClass;
        public IReadOnlyList<KeywordEntry> Keywords => _keywords;
        public bool IsDefault => _isDefault;
        public int UnlockCost => _unlockCost;
        public int SoulUnlockCost => _soulUnlockCost;
        public Sprite Icon => _icon;
    }
}
