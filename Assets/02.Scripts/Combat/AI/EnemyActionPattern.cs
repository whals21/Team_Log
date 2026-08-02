using System.Collections.Generic;
using TeamLog.Characters;
using UnityEngine;

namespace TeamLog.Combat.AI
{
    /// <summary>
    /// 적 행동 패턴 — 상황인식 가중치 기반으로 다음 스킬을 무작위 선택.
    /// HP/약한 적/첫 턴/연속 사용 등 전장 상황에 따라 스킬 가중치를 동적으로 조정.
    ///
    /// ★ 2026-08-02 P0-1 개선 (StS 표준 — 원패턴 해소):
    ///   1. 연속 사용 패널티: 직전 스킬 가중치 -10 (같은 스킬 반복 억제)
    ///   2. HP 70% 단계 추가: 기존 30%/50% → 30%/50%/70% 3단계 세분화
    ///   목적: 같은 적과 3번 싸워도 매번 다른 스킬 전개 유도
    /// </summary>
    public class EnemyActionPattern
    {
        private const int DefaultWeight = 25;
        private const float RepeatPenalty = 10f;   // 직전 스킬 가중치 차감량
        private const float MinWeight = 5f;         // 패널티 하한선 (음수 가중치 방지)

        private readonly List<SkillData> _skills;
        private readonly List<int> _baseWeights;
        private int _turnNumber;
        private int _lastUsedIndex = -1;            // ★ 직전 사용 스킬 인덱스 추적

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
        /// 선택 후 내부 턴 카운터가 증가하며, 직전 사용 스킬 인덱스가 갱신된다.
        /// </summary>
        public SkillData GetNextSkill(Character self, IReadOnlyList<Character> players)
        {
            if (_skills.Count == 0) return null;

            float total = 0f;
            var cumulative = new float[_skills.Count];

            for (int i = 0; i < _skills.Count; i++)
            {
                float effective = CalculateEffectiveWeight(_skills[i], _baseWeights[i], i, self, players);
                total += effective;
                cumulative[i] = total;
            }

            // 모든 가중치가 0 이하인 경우 첫 스킬을 반환(안전장치)
            if (total <= 0f)
                return _skills[0];

            float roll = Random.Range(0f, total);
            for (int i = 0; i < cumulative.Length; i++)
            {
                if (roll <= cumulative[i])
                {
                    _lastUsedIndex = i;   // ★ 직전 사용 인덱스 갱신
                    _turnNumber++;
                    return _skills[i];
                }
            }

            // 부동소수 오차 안전장치
            _lastUsedIndex = _skills.Count - 1;
            _turnNumber++;
            return _skills[_skills.Count - 1];
        }

        /// <summary>
        /// 기본 가중치에 상황 규칙(총 7종)을 적용하여 유효 가중치 반환.
        /// 동일 조건군 내 규칙은 상호배타(예: HP&lt;30% 힐은 x3.0만 적용).
        ///
        /// 규칙 목록:
        ///   0. ★ 연속 사용 패널티 (신규) — 직전 스킬 -10
        ///   1. 자신 HP&lt;30% 시 Heal/Shield ×3.0
        ///   2. 자신 HP&lt;50% 시 Heal/Shield ×2.0
        ///   3. ★ 자신 HP&lt;70% 시 Heal/Shield ×1.3 (신규)
        ///   4. 약한 플레이어(HP&lt;30%) 존재 시 Attack ×2.5
        ///   5. 첫 턴 Buff ×2.0
        ///   6. 자신 HP&lt;50% 시 Debuff ×1.5
        /// </summary>
        private float CalculateEffectiveWeight(SkillData skill, int baseWeight, int skillIndex,
            Character self, IReadOnlyList<Character> players)
        {
            float w = baseWeight;
            if (w <= 0) w = DefaultWeight;

            // 규칙 0 (★ 신규, StS 표준): 직전 사용 스킬 패널티
            // 같은 스킬 반복을 억제하여 전투 다양성 확보
            // 단일 스킬 적은 어차피 같은 스킬만 쓰므로 패널티 제외 (P1-1 리뷰 반영)
            if (_skills.Count > 1 && skillIndex == _lastUsedIndex)
                w = Mathf.Max(MinWeight, w - RepeatPenalty);

            float hpRatio = GetHPRatio(self);

            // 규칙 1/2/3: 자신 HP 위기 시 Heal/Shield 강조 (3단계 세분화, 상호배타)
            if (IsHealOrShield(skill))
            {
                if (hpRatio < 0.3f) return w * 3.0f;
                if (hpRatio < 0.5f) return w * 2.0f;
                if (hpRatio < 0.7f) return w * 1.3f;   // ★ 신규 단계
            }

            // 규칙 4: 약한 플레이어(HP<30%) 존재 시 Attack 강조
            if (skill.Type == SkillType.Attack && HasWeakPlayer(players))
                return w * 2.5f;

            // 규칙 5: 첫 턴 Buff 강조
            if (_turnNumber == 1 && skill.Type == SkillType.Buff)
                return w * 2.0f;

            // 규칙 6: 자신 HP < 50% 시 Debuff(공격감소/방어감소) 가중
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
