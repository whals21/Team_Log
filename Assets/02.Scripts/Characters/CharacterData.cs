using UnityEngine;
using System.Collections.Generic;

namespace TeamLog.Characters
{
    /// <summary>
    /// 캐릭터 정적 데이터 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterData", menuName = "TeamLog/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string _characterName;
        [SerializeField] private CharacterClass _characterClass;
        [TextArea(3, 5)]
        [SerializeField] private string _description;

        [Header("기본 스탯")]
        [SerializeField] private int _baseHP = 100;
        [SerializeField] private int _baseATK = 10;
        [SerializeField] private int _baseDEF = 5;

        [Header("적 특성")]
        [SerializeField] private EnemyTrait _enemyTrait;

        [Header("보스 여부 (Execution 제외용)")]
        [SerializeField] private bool _isBoss;

        [Header("스킬")]
        [SerializeField] private List<SkillData> _skills = new List<SkillData>(4);

        [Header("잠금해제")]
        [SerializeField] private bool _isDefault = true;  // 기본 해금 여부
        [SerializeField] private string _unlockCondition; // 잠금해제 조건 텍스트

        [Header("고유 자원 (Phase CC)")]
        [Tooltip("캐릭터 고유 자원. None이면 자원 없는 기존 캐릭터. Ashe=Ember, Duran=Vengeance 등.")]
        [SerializeField] private ResourceType _resourceType = ResourceType.None;

        #region Properties
        public string CharacterName => _characterName;
        public CharacterClass Class => _characterClass;
        public string Description => _description;
        public int BaseHP => _baseHP;
        public int BaseATK => _baseATK;
        public int BaseDEF => _baseDEF;
        public IReadOnlyList<SkillData> Skills => _skills;
        public EnemyTrait Trait => _enemyTrait;
        public bool IsBoss => _isBoss;
        public bool IsDefault => _isDefault;
        public string UnlockCondition => _unlockCondition;
        public ResourceType ResourceType => _resourceType;
        #endregion
    }

    /// <summary>
    /// 캐릭터 직업 클래스
    /// </summary>
    public enum CharacterClass
    {
        Warrior,        // 전사
        Mage,           // 마법사
        Healer,         // 힐러
        Rogue,          // 도적
        Archer,         // 궁수
        Necromancer,    // 네크로맨서
        Alchemist,      // 연금술사
        Bard,           // 음유시인

        // Phase CC 신규 직업
        Pyromancer,     // Ashe — 화염 마법사 (Mage 분할)
        Cryomancer,     // Lumi — 냉기 마법사
        Stormcaller,    // Taranis — 번개 마법사
        Oracle,         // Sibyl — 예언자
    }
}
