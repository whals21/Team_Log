using System.Collections.Generic;
using TeamLog.Characters;

namespace TeamLog.Combat.AI
{
    /// <summary>
    /// 적 행동 패턴 — 상황인식 가중치 기반으로 다음 스킬을 무작위 선택.
    /// HP/약한 적/첫 턴 등 전장 상황에 따라 스킬 가중치를 동적으로 조정한다.
    /// </summary>
    public class EnemyActionPattern
    {
        private const int DefaultWeight = 25;

        private readonly List<SkillData> _skills;
        private readonly List<int> _baseWeights;
        private int _turnNumber;

        public IReadOnlyList<SkillData> Skills => _skills;

        /// <summary>
        /// 스킬 목록으로 패턴 생성. weights가 null이거나 길이가 맞지 않으면 기본값(25) 사용.
        /// </summary>
        public EnemyActionPattern(IEnumerable<SkillData> skills, IEnumerable<int> weights = null)
        {
            _skills = new List<SkillData>(skills);
            _baseWeights = new List<int>();
            _turnNumber = 1;

            if (weights != null)
            {
                foreach (var w in weights)
                    _baseWeights.Add(w);
            }

            // _skills와 길이를 맞춤 — 부족하면 기본값, 초과면 잘라냄
            while (_baseWeights.Count < _skills.Count)
                _baseWeights.Add(DefaultWeight);
            while (_baseWeights.Count > _skills.Count)
                _baseWeights.RemoveAt(_baseWeights.Count - 1);
        }

        /// <summary>
        /// 현재 전장 상황을 반영하여 가중치를 계산하고 무작위로 스킬을 선택.
        /// 선택 후 내부 턴 카운터가 증가한다.
        /// </summary>
        public SkillData GetNextSkill(Character self, IReadOnlyList<Character> players)
        {
            if (_skills.Count == 0) return null;

            float total = 0f;
            var cumulative = new float[_skills.Count];

            for (int i = 0; i < _skills.Count; i++)
            {
                float effective = CalculateEffectiveWeight(_skills[i], _baseWeights[i], self, players);
                total += effective;
                cumulative[i] = total;
            }

            // 모든 가중치가 0 이하인 경우 첫 스킬을 반환(안전장치)
            if (total <= 0f)
                return _skills[0];

            float roll = UnityEngine.Random.Range(0f, total);
            for (int i = 0; i < cumulative.Length; i++)
            {
                if (roll <= cumulative[i])
                {
                    _turnNumber++;
                    return _skills[i];
                }
            }

            // 부동소수 오차 안전장치
            _turnNumber++;
            return _skills[_skills.Count - 1];
        }

        /// <summary>
        /// 기본 가중치에 상황 규칙(5종)을 곱하여 유효 가중치 반환.
        /// 동일 조건군 내 규칙은 상호배타(예: HP&lt;30% 힐은 x3.0만 적용).
        /// </summary>
        private float CalculateEffectiveWeight(SkillData skill, int baseWeight,
            Character self, IReadOnlyList<Character> players)
        {
            float w = baseWeight;
            if (w <= 0) w = DefaultWeight;

            float hpRatio = GetHPRatio(self);

            // 규칙 1/2: 자신 HP 위기 시 Heal/Shield 강조 (상호배타)
            if (IsHealOrShield(skill))
            {
                if (hpRatio < 0.3f) return w * 3.0f;
                if (hpRatio < 0.5f) return w * 2.0f;
            }

            // 규칙 3: 약한 플레이어(HP<30%) 존재 시 Attack 강조
            if (skill.Type == SkillType.Attack && HasWeakPlayer(players))
                return w * 2.5f;

            // 규칙 4: 첫 턴 Buff 강조
            if (_turnNumber == 1 && skill.Type == SkillType.Buff)
                return w * 2.0f;

            // 규칙 5: 자신 HP < 50% 시 Debuff(공격감소/방어감소) 가중
            if (hpRatio < 0.5f && skill.Type == SkillType.Debuff)
                return w * 1.5f;

            return w;
        }

        private static bool IsHealOrShield(SkillData skill)
            => skill.Type == SkillType.Heal || skill.Type == SkillType.Shield;

        private static float GetHPRatio(Character c)
        {
            if (c == null || c.Health.MaxHP <= 0) return 1f;
            return (float)c.Health.CurrentHP / c.Health.MaxHP;
        }

        private static bool HasWeakPlayer(IReadOnlyList<Character> players)
        {
            if (players == null) return false;
            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p != null && p.IsAlive && GetHPRatio(p) < 0.3f)
                    return true;
            }
            return false;
        }
    }
}
