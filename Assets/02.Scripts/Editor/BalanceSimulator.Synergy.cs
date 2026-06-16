#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Combat.AI;
using TeamLog.Combat.Turn;
using TeamLog.Map;
using TeamLog.Reward;

using Character = TeamLog.Characters.Character;

namespace TeamLog.Editor
{
    /// <summary>
    /// BalanceSimulator partial — 유물 3-세트 시너지 밸런스 테스트.
    /// 각 카테고리별 대표 3종 유물 강제 지급 후 전투 시뮬레이션.
    /// 진입점/상수/에셋 로드: BalanceSimulator.cs
    /// </summary>
    public static partial class BalanceSimulator
    {
        // ═══════════════════════════════════════════
        // 시너지 테스트 진입점
        // ═══════════════════════════════════════════

        [MenuItem("TeamLog/Balance/Relic Synergy Test (3-set)", false, 203)]
        public static void RunRelicSynergyTestMenu()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[BalanceSimulator] 플레이 모드에서는 실행할 수 없습니다.");
                return;
            }

            EnsureAssetsLoaded();
            if (!VerifyAssets("Relic Synergy Test")) return;

            var results = RunRelicSynergySimulation();
            PrintSynergySummary(results);
            AssetDatabase.Refresh();
        }

        // ═══════════════════════════════════════════
        // 시너지 테스트 데이터 구조
        // ═══════════════════════════════════════════

        public class SynergyResult
        {
            public string CategoryName;
            public string[] RelicNames;
            public int TotalBattles;
            public int Victories;
            public float AvgRemainingHP;
            public float AvgTurns;
            public int BossVictories;
            public int BossBattles;
        }

        // ═══════════════════════════════════════════
        // 카테고리 정의 (3-세트 완성된 것만)
        // ═══════════════════════════════════════════

        private static readonly string[][] SynergyCategories =
        {
            // A: 성전의 루프 (골드/처치 체인)
            new[] { "Relic_ReliquaryCross", "Relic_IndulgenceCoin", "Relic_PilgrimCoin" },
            // B: 쉴드 공명
            new[] { "Relic_AegisCharm", "Relic_AegisCounter", "Relic_AegisStrike" },
            // C: 생명 순환
            new[] { "Relic_VerdantSeed", "Relic_SanguineBond", "Relic_MercyBlade" },
            // F: 집중 사격
            new[] { "Relic_DeadeyeLens", "Relic_CriticalFocus", "Relic_ExecutionerBlade" },
            // H: 전우의 맹세
            new[] { "Relic_BrothersInArms", "Relic_UnitedFront", "Relic_VowOfGuardian" },
            // I: 리스크/보상
            new[] { "Relic_BloodPact", "Relic_RecklessFury", "Relic_CursedDoll" },
            // 비교군: 유물 없음 (baseline)
            new[] { "", "", "" },
        };

        private static readonly string[] SynergyCategoryNames =
        {
            "A_성전루프", "B_쉴드공명", "C_생명순환", "F_집중사격", "H_전우맹세", "I_리스크", "(baseline)유물없음"
        };

        private const int SynergyTestPacks = 30;
        private const int SynergyBossPacks = 15;
        private const int SynergyTestFloor = 2; // F2 (F1은 너무 쉬워서 차이 안 보임)

        // ═══════════════════════════════════════════
        // 시뮬레이션 본체
        // ═══════════════════════════════════════════

        private static List<SynergyResult> RunRelicSynergySimulation()
        {
            var results = new List<SynergyResult>();
            int catIdx = 0;
            int totalCats = SynergyCategories.Length;

            try
            {
                foreach (var relicIds in SynergyCategories)
                {
                    string catName = SynergyCategoryNames[catIdx];
                    ShowProgress(catIdx, totalCats, "Relic Synergy 시뮬레이션", $"카테고리: {catName}");

                    var result = RunCategoryTest(catName, relicIds);
                    results.Add(result);

                    catIdx++;
                }
            }
            finally
            {
                ClearProgress();
            }

            return results;
        }

        private static SynergyResult RunCategoryTest(string categoryName, string[] relicIds)
        {
            var result = new SynergyResult
            {
                CategoryName = categoryName,
                RelicNames = relicIds,
                TotalBattles = 0,
                Victories = 0,
                AvgRemainingHP = 0f,
                AvgTurns = 0f,
                BossVictories = 0,
                BossBattles = 0,
            };

            // F1 일반 SynergyTestPacks팩 + F1 보스 SynergyBossPacks팩
            int totalPacks = SynergyTestPacks + SynergyBossPacks;
            float hpSum = 0f;
            int turnSum = 0;
            int victoryCount = 0;

            for (int i = 0; i < totalPacks; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar(
                    "Relic Synergy 시뮬레이션",
                    $"{categoryName} ({i + 1}/{totalPacks})",
                    (float)i / totalPacks))
                {
                    Debug.Log($"[BalanceSimulator] {categoryName} — 사용자 취소");
                    break;
                }

                bool isBoss = i >= SynergyTestPacks;
                var party = CreateDefaultParty();
                float scaling = GetFloorScalingFor(SynergyTestFloor);
                var enemies = CreateEnemiesFor(SynergyTestFloor, false, isBoss, scaling);

                // GameRunState 생성 + 유물 지급
                var runState = GameRunState.Create(party, DefaultStartingGold);
                runState.SetDataPools(_relicPool, _augmentPool);
                runState.RelicHandler.SetPlayerParty(party);
                // ★ 트리거 기반 유물 작동을 위해 이벤트 구독 필수
                runState.RelicHandler.SubscribeEvents();

                foreach (var rid in relicIds)
                {
                    if (string.IsNullOrEmpty(rid)) continue;
                    var relic = _relicPool.Find(r => r.name == rid);
                    if (relic != null)
                        runState.AcquireRelic(relic);
                    else
                        Debug.LogWarning($"[BalanceSimulator] 유물을 찾을 수 없음: {rid}");
                }

                // 전투 실행 (GameRunState.Instance가 설정되어 유물 효과 적용)
                var combatResult = RunSingleCombat(
                    party, enemies,
                    isBoss ? MapNodeType.Boss : MapNodeType.Battle,
                    SynergyTestFloor, $"{categoryName}_{i + 1:000}", categoryName);

                // 전투 후 정리 (이벤트 구독 해제 + 싱글톤 정리)
                runState.RelicHandler.UnsubscribeEvents();
                GameRunState.Destroy();

                result.TotalBattles++;
                if (combatResult.Victory)
                {
                    result.Victories++;
                    victoryCount++;
                }
                hpSum += combatResult.AvgRemainingHP;
                turnSum += combatResult.TurnCount;

                if (isBoss)
                {
                    result.BossBattles++;
                    if (combatResult.Victory) result.BossVictories++;
                }
            }

            ClearCombatEventBus();

            if (result.TotalBattles > 0)
            {
                result.AvgRemainingHP = hpSum / result.TotalBattles;
                result.AvgTurns = (float)turnSum / result.TotalBattles;
            }

            return result;
        }

        // ═══════════════════════════════════════════
        // 요약 출력
        // ═══════════════════════════════════════════

        private static void PrintSynergySummary(List<SynergyResult> results)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine("  유물 시너지 테스트 결과 (3-세트)");
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine($"카테고리 | 일반승률 | 보스승률 | 평균잔여HP | 평균턴수");
            sb.AppendLine("───────────────────────────────────────────");

            SynergyResult baseline = null;
            foreach (var r in results)
            {
                if (r.CategoryName.Contains("baseline")) baseline = r;
            }

            foreach (var r in results)
            {
                float normalRate = r.TotalBattles - r.BossBattles > 0
                    ? (float)(r.Victories - r.BossVictories) / (r.TotalBattles - r.BossBattles) * 100f
                    : 0f;
                float bossRate = r.BossBattles > 0
                    ? (float)r.BossVictories / r.BossBattles * 100f
                    : 0f;

                string diff = "";
                if (baseline != null && r != baseline)
                {
                    float normalDiff = normalRate - ((float)(baseline.Victories - baseline.BossVictories) / System.Math.Max(1, baseline.TotalBattles - baseline.BossBattles) * 100f);
                    diff = $" ({(normalDiff >= 0 ? "+" : "")}{normalDiff:F1}%)";
                }

                sb.AppendLine($"{r.CategoryName,-12} | {normalRate,5:F1}%{diff,-8} | {bossRate,5:F1}% | {r.AvgRemainingHP * 100,5:F1}% | {r.AvgTurns,5:F1}");
            }

            sb.AppendLine("───────────────────────────────────────────");
            sb.AppendLine($"기준: F{SynergyTestFloor} 일반 {SynergyTestPacks}팩 + F{SynergyTestFloor} 보스 {SynergyBossPacks}팩");
            sb.AppendLine("확률: 시너지 배수 = (일반승률 - baseline 일반승률)");

            Debug.Log(sb.ToString());

            // CSV 출력
            EnsureReportDir();
            string csvPath = $"{REPORT_DIR}/RelicSynergy_{System.DateTime.Now:yyyyMMdd_HHmm}.csv";
            var csv = new StringBuilder();
            csv.AppendLine("Category,Relics,TotalBattles,Victories,NormalWinRate,BossWinRate,AvgRemainingHP,AvgTurns");
            foreach (var r in results)
            {
                float normalRate = r.TotalBattles - r.BossBattles > 0
                    ? (float)(r.Victories - r.BossVictories) / (r.TotalBattles - r.BossBattles) * 100f
                    : 0f;
                float bossRate = r.BossBattles > 0
                    ? (float)r.BossVictories / r.BossBattles * 100f
                    : 0f;
                csv.AppendLine($"{r.CategoryName},\"{string.Join("|", r.RelicNames)}\",{r.TotalBattles},{r.Victories},{normalRate:F1},{bossRate:F1},{r.AvgRemainingHP:F3},{r.AvgTurns:F2}");
            }
            System.IO.File.WriteAllText(csvPath, csv.ToString());
            Debug.Log($"[BalanceSimulator] 시너지 CSV 저장: {csvPath}");
        }
    }
}
#endif
