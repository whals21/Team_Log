using System.Collections.Generic;
using UnityEngine;
using TeamLog.Characters;

namespace TeamLog.EditorDebug
{
    /// <summary>
    /// 인터랙티브 전투 테스트 씬(BattleTestScene)용 파티/적/보스 생성 헬퍼.
    /// 씬 빌더가 인스펙터에 미리 바인딩한 CharacterData 배열을 기반으로
    /// 런타임 인덱스 선택 → 캐릭터 인스턴스 생성 + 층별 스케일 적용.
    /// (AssetDatabase 미사용 — 런타임 호환)
    /// </summary>
    public static class BattleTestConfig
    {
        // GameRunState.GetFloorScaling와 동일 — 본 클래스는 GameRunState에 의존하지 않으므로 로컬 복제
        public static float GetFloorScaling(int floor) => floor switch
        {
            1 => 1.0f,
            2 => 1.3f,
            3 => 1.6f,
            4 => 2.0f,
            _ => 1.0f + (floor - 1) * 0.3f
        };

        /// <summary>
        /// 파티 생성. indices[i] == 0 은 "(없음)" 슬롯.
        /// </summary>
        public static List<Character> BuildParty(IList<CharacterData> available, int[] indices)
        {
            var party = new List<Character>();
            if (available == null || indices == null) return party;

            foreach (var idx in indices)
            {
                if (idx <= 0 || idx > available.Count) continue;
                var data = available[idx - 1];
                if (data != null)
                    party.Add(new Character(data));
            }
            return party;
        }

        /// <summary>
        /// 적 생성.
        /// - 보스 모드: bossPool[floor-1] 1마리 (F4는 마지막 보스로 폴백)
        /// - 일반 모드: enemyPool(일반+엘리트 통합)에서 indices로 다중 선택
        /// 모든 적에게 층별 스케일링 적용.
        /// </summary>
        public static List<Character> BuildEnemies(
            IList<CharacterData> enemyPool,
            IList<CharacterData> bossPool,
            int[] indices,
            int floor,
            bool isBoss)
        {
            var enemies = new List<Character>();
            float scaling = GetFloorScaling(floor);

            if (isBoss)
            {
                if (bossPool == null || bossPool.Count == 0) return enemies;
                int idx = Mathf.Clamp(floor - 1, 0, bossPool.Count - 1);
                if (bossPool[idx] == null) return enemies;
                var boss = new Character(bossPool[idx]);
                boss.ApplyFloorScaling(scaling);
                enemies.Add(boss);
                return enemies;
            }

            if (enemyPool == null || indices == null) return enemies;
            foreach (var idx in indices)
            {
                if (idx <= 0 || idx > enemyPool.Count) continue;
                var data = enemyPool[idx - 1];
                if (data == null) continue;
                var enemy = new Character(data);
                enemy.ApplyFloorScaling(scaling);
                enemies.Add(enemy);
            }
            return enemies;
        }
    }
}
