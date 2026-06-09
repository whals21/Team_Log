using UnityEngine;
using TeamLog.Characters;

namespace TeamLog.Map
{
    /// <summary>
    /// 적 스폰 패턴 — 전투마다 하나의 패턴이 무작위 선택되어 적 집단을 구성
    /// ScriptableObject: SpawnPatternTable.cs
    /// </summary>
    [System.Serializable]
    public class EnemySpawnPattern
    {
        public string patternName;
        public EnemySpawnEntry[] enemies;

        /// <summary>
        /// 패턴의 대략적 전투력 (HP + ATK*5 합)
        /// </summary>
        public int EstimatedPower
        {
            get
            {
                int total = 0;
                if (enemies == null) return total;
                foreach (var e in enemies)
                {
                    if (e.enemyData != null)
                        total += e.enemyData.BaseHP + e.enemyData.BaseATK * 5;
                }
                return total;
            }
        }
    }

    [System.Serializable]
    public class EnemySpawnEntry
    {
        public CharacterData enemyData;
        public int count = 1;
    }
}
