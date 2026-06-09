using System.Collections.Generic;
using UnityEngine;
using TeamLog.Characters;

namespace TeamLog.Map
{
    /// <summary>
    /// 층별 스폰 패턴 테이블 — 전투마다 패턴 중 하나를 무작위 선택하여 적 집단 구성
    /// 데이터 모델: EnemySpawnPattern.cs
    /// </summary>
    [CreateAssetMenu(fileName = "SpawnPatternTable", menuName = "TeamLog/SpawnPatternTable")]
    public class SpawnPatternTable : ScriptableObject
    {
        [SerializeField] private EnemySpawnPattern[] _normalPatterns;
        [SerializeField] private EnemySpawnPattern[] _elitePatterns;

        public EnemySpawnPattern[] NormalPatterns => _normalPatterns;
        public EnemySpawnPattern[] ElitePatterns => _elitePatterns;

        /// <summary>
        /// 일반 전투 패턴 중 하나를 무작위 선택하여 적 리스트 생성
        /// </summary>
        public List<Character> RollNormalPattern()
        {
            if (_normalPatterns == null || _normalPatterns.Length == 0)
                return new List<Character>();

            var pattern = _normalPatterns[Random.Range(0, _normalPatterns.Length)];
            return InstantiatePattern(pattern);
        }

        /// <summary>
        /// 엘리트 전투 패턴 중 하나를 무작위 선택하여 적 리스트 생성
        /// </summary>
        public List<Character> RollElitePattern()
        {
            if (_elitePatterns == null || _elitePatterns.Length == 0)
                return new List<Character>();

            var pattern = _elitePatterns[Random.Range(0, _elitePatterns.Length)];
            return InstantiatePattern(pattern);
        }

        private List<Character> InstantiatePattern(EnemySpawnPattern pattern)
        {
            var result = new List<Character>();
            if (pattern.enemies == null) return result;

            foreach (var entry in pattern.enemies)
            {
                if (entry.enemyData == null) continue;
                for (int i = 0; i < entry.count; i++)
                    result.Add(new Character(entry.enemyData));
            }
            return result;
        }
    }
}
