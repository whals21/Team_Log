using System.Collections.Generic;
using TeamLog.Characters;

namespace TeamLog.Combat.AI
{
    /// <summary>
    /// 적 행동 패턴 — SkillData 리스트를 순환하며 다음 스킬을 반환
    /// </summary>
    public class EnemyActionPattern
    {
        private readonly List<SkillData> _skills;
        private int _currentIndex;

        public IReadOnlyList<SkillData> Skills => _skills;

        public EnemyActionPattern(IEnumerable<SkillData> skills)
        {
            _skills = new List<SkillData>(skills);
            _currentIndex = 0;
        }

        /// <summary>
        /// 다음 스킬을 반환하고 인덱스를 순환
        /// </summary>
        public SkillData GetNextSkill()
        {
            if (_skills.Count == 0) return null;

            var skill = _skills[_currentIndex];
            _currentIndex = (_currentIndex + 1) % _skills.Count;
            return skill;
        }

        /// <summary>
        /// 다음 스킬을 엿보기 (인덱스 이동 없음)
        /// </summary>
        public SkillData PeekNextSkill()
        {
            if (_skills.Count == 0) return null;
            return _skills[_currentIndex];
        }

        public void Reset()
        {
            _currentIndex = 0;
        }
    }
}
