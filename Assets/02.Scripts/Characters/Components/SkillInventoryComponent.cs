using System.Collections.Generic;
using System.Linq;

namespace TeamLog.Characters
{
    /// <summary>
    /// 스킬 인벤토리 및 드로우 관리 컴포넌트
    /// SkillInstance 기반으로 업그레이드 상태를 관리
    /// </summary>
    public class SkillInventoryComponent
    {
        private readonly List<SkillInstance> _instances = new();

        /// <summary>하위 호환: SkillData 목록</summary>
        public IReadOnlyList<SkillData> Skills => _instances.Select(i => i.Data).ToList();

        /// <summary>SkillInstance 목록</summary>
        public IReadOnlyList<SkillInstance> SkillInstances => _instances;

        public void Initialize(IEnumerable<SkillData> skills)
        {
            _instances.Clear();
            if (skills != null)
                foreach (var skill in skills)
                    _instances.Add(new SkillInstance(skill));
        }

        /// <summary>기존 스킬 추가 (SkillData)</summary>
        public void AddSkill(SkillData skill)
        {
            if (skill != null && !_instances.Any(i => i.Data == skill))
                _instances.Add(new SkillInstance(skill));
        }

        /// <summary>SkillInstance 직접 추가</summary>
        public void AddInstance(SkillInstance instance)
        {
            if (instance != null && !_instances.Any(i => i.Data == instance.Data))
                _instances.Add(instance);
        }

        /// <summary>스킬 제거</summary>
        public void RemoveSkill(SkillData skill)
        {
            _instances.RemoveAll(i => i.Data == skill);
        }

        /// <summary>SkillInstance 제거</summary>
        public void RemoveInstance(SkillInstance instance)
        {
            _instances.Remove(instance);
        }

        /// <summary>가중치 기반 랜덤 스킬 드로우 (SkillData 반환 — 하위 호환)</summary>
        public SkillData DrawSkill()
        {
            var instance = DrawSkillInstance();
            return instance?.Data;
        }

        /// <summary>가중치 기반 랜덤 스킬 드로우 (SkillInstance 반환)</summary>
        public SkillInstance DrawSkillInstance()
        {
            if (_instances.Count == 0) return null;

            int totalWeight = 0;
            foreach (var inst in _instances)
                totalWeight += inst.EffectiveWeight;

            int randomValue = UnityEngine.Random.Range(1, totalWeight + 1);
            int cumulative = 0;

            foreach (var inst in _instances)
            {
                cumulative += inst.EffectiveWeight;
                if (randomValue <= cumulative)
                    return inst;
            }

            return _instances[0];
        }

        /// <summary>특정 SkillData에 해당하는 SkillInstance 조회</summary>
        public SkillInstance FindInstance(SkillData data)
        {
            return _instances.FirstOrDefault(i => i.Data == data);
        }
    }
}
