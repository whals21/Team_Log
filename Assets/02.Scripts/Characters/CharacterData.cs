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

        [Header("스킬")]
        [SerializeField] private List<SkillData> _skills = new List<SkillData>(4);

        #region Properties
        public string CharacterName => _characterName;
        public CharacterClass Class => _characterClass;
        public string Description => _description;
        public int BaseHP => _baseHP;
        public int BaseATK => _baseATK;
        public int BaseDEF => _baseDEF;
        public IReadOnlyList<SkillData> Skills => _skills;
        #endregion
    }

    /// <summary>
    /// 캐릭터 직업 클래스
    /// </summary>
    public enum CharacterClass
    {
        Warrior,    // 전사
        Mage,       // 마법사
        Healer,     // 힐러
        Rogue       // 도적
    }
}
