using System.Collections.Generic;
using System.Linq;
using TeamLog.Skill;

namespace TeamLog.Characters
{
    /// <summary>
    /// 스킬 인벤토리 및 드로우 관리 컴포넌트
    /// 고정 스킬셋 + 증강(Augment) 부착 관리
    /// </summary>
    public class SkillInventoryComponent
    {
        private readonly List<SkillInstance> _instances = new();

        /// <summary>하위 호환: SkillData 목록</summary>
        public IReadOnlyList<SkillData> Skills => _instances.Select(i => i.Data).ToList();

        /// <summary>SkillInstance 목록</summary>
        public IReadOnlyList<SkillInstance> SkillInstances => _instances;

        /// <summary>스킬 수</summary>
        public int Count => _instances.Count;

        /// <summary>고정 스킬셋 초기화 (캐릭터 생성 시 1회만 호출)</summary>
        public void Initialize(IEnumerable<SkillData> skills)
        {
            _instances.Clear();
            if (skills != null)
                foreach (var skill in skills)
                    _instances.Add(new SkillInstance(skill));
        }

        /// <summary>특정 스킬에 증강 부착 — 성공 시 true</summary>
        public bool ApplyAugmentToSkill(SkillInstance instance, AugmentData augment)
        {
            if (instance == null || augment == null) return false;
            if (!_instances.Contains(instance)) return false;
            return instance.AddAugment(augment);
        }

        /// <summary>가중치 기반 랜덤 스킬 드로우 (SkillInstance 반환)</summary>
        /// <param name="bonusWeight">유물 등 외부 가중치 보너스</param>
        public SkillInstance DrawSkillInstance(int bonusWeight = 0)
        {
            if (_instances.Count == 0) return null;

            // QuickDraw 행동 키워드가 있는 스킬은 항상 우선 뽑힘 (Phase BK)
            var quickDraw = _instances.FirstOrDefault(i => i.HasBehavior(BehaviorKeyword.QuickDraw));
            if (quickDraw != null) return quickDraw;

            int totalWeight = 0;
            foreach (var inst in _instances)
                totalWeight += inst.EffectiveWeight + bonusWeight;

            int randomValue = UnityEngine.Random.Range(1, totalWeight + 1);
            int cumulative = 0;

            foreach (var inst in _instances)
            {
                cumulative += inst.EffectiveWeight + bonusWeight;
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
