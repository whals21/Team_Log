using System.Collections.Generic;
using UnityEngine;

namespace TeamLog.Characters
{
    /// <summary>
    /// 스킬 인벤토리 및 드로우 관리 컴포넌트
    /// </summary>
    public class SkillInventoryComponent
    {
        private readonly List<SkillData> _skills = new();
        private SkillData _drawnSkill;

        public IReadOnlyList<SkillData> Skills => _skills;
        public SkillData DrawnSkill => _drawnSkill;
        public int SkillCount => _skills.Count;

        public event System.Action<SkillData> OnSkillDrawn;

        public void Initialize(IEnumerable<SkillData> skills)
        {
            _skills.Clear();
            if (skills != null)
                _skills.AddRange(skills);
        }

        public void AddSkill(SkillData skill)
        {
            if (skill != null && !_skills.Contains(skill))
                _skills.Add(skill);
        }

        public void RemoveSkill(SkillData skill)
        {
            _skills.Remove(skill);
        }

        /// <summary>
        /// 가중치 기반 랜덤 스킬 드로우
        /// </summary>
        public SkillData DrawSkill()
        {
            if (_skills.Count == 0)
            {
                _drawnSkill = null;
                return null;
            }

            int totalWeight = 0;
            foreach (var skill in _skills)
                totalWeight += skill.Weight;

            int randomValue = Random.Range(1, totalWeight + 1);
            int cumulative = 0;

            foreach (var skill in _skills)
            {
                cumulative += skill.Weight;
                if (randomValue <= cumulative)
                {
                    _drawnSkill = skill;
                    OnSkillDrawn?.Invoke(skill);
                    return skill;
                }
            }

            _drawnSkill = _skills[0];
            OnSkillDrawn?.Invoke(_drawnSkill);
            return _drawnSkill;
        }

        public void ClearDrawnSkill()
        {
            _drawnSkill = null;
        }
    }
}
