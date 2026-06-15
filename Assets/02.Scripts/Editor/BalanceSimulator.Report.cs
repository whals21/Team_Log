#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TeamLog.Editor
{
    /// <summary>
    /// BalanceSimulator partial — 통계 집계 + CSV 출력 + 콘솔 요약.
    /// 모든 출력은 Assets/09.Docs/BalanceReports/ 디렉토리에 저장.
    /// </summary>
    public static partial class BalanceSimulator
    {
        // Quick Combat 카테고리 순서 (층/등급별 정렬용)
        private static readonly string[] QuickCombatCategoryOrder =
        {
            "F1_Normal", "F1_Elite", "F1_Boss",
            "F2_Normal", "F2_Elite", "F2_Boss",
            "F3_Normal", "F3_Elite", "F3_Boss"
        };

        // ═══════════════════════════════════════════
        // Quick Combat CSV / 요약
        // ═══════════════════════════════════════════

        public static class ReportUtils
        {
            private const string CsvHeaderQuick = "ScenarioName,Category,Floor,EnemyName,Victory,TurnCount,AvgRemainingHP,Survivors";
            private const string CsvHeaderRun = "RunIndex,RunCompleted,FloorReached,DeathLocation,TotalBattles,TotalTurns,FinalPartyHP,Survivors,GoldEarned,RelicsAcquired,AugmentsAcquired";

            // ── Quick Combat ──

            public static void WriteQuickCombatCsv(List<CombatResult> results)
            {
                EnsureReportDir();
                string path = $"{REPORT_DIR}/QuickCombat_{TimeStamp()}.csv";
                var sb = new StringBuilder();
                sb.AppendLine(CsvHeaderQuick);
                foreach (var r in results)
                {
                    sb.Append(Escape(r.ScenarioName)).Append(',');
                    sb.Append(Escape(r.Category)).Append(',');
                    sb.Append(r.Floor).Append(',');
                    sb.Append(Escape(r.EnemyName)).Append(',');
                    sb.Append(r.Victory ? "True" : "False").Append(',');
                    sb.Append(r.TurnCount).Append(',');
                    sb.Append(r.AvgRemainingHP.ToString("F3", CultureInfo.InvariantCulture)).Append(',');
                    sb.Append(r.Survivors);
                    sb.AppendLine();
                }
                File.WriteAllText(path, sb.ToString());
                Debug.Log($"[BalanceSimulator] Quick Combat CSV 저장: {path} ({results.Count}행)");
            }

            public static void PrintQuickCombatSummary(List<CombatResult> results)
            {
                if (results == null || results.Count == 0)
                {
                    Debug.LogWarning("[BalanceSimulator] Quick Combat 결과가 비어있습니다.");
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════");
                sb.AppendLine($"=== Quick Combat 요약 ({results.Count}팩) ===");
                sb.AppendLine("═══════════════════════════════════════════");

                // 전체 통계
                int wins = results.FindAll(r => r.Victory).Count;
                float winRate = 100f * wins / results.Count;
                float avgTurns = Average(results.ConvertAll(r => (float)r.TurnCount));
                float avgHP = Average(results.ConvertAll(r => r.AvgRemainingHP)) * 100f;

                sb.AppendLine($"전체 승률: {winRate:F1}%  ({wins}/{results.Count})");
                sb.AppendLine($"평균 턴 수: {avgTurns:F2}");
                sb.AppendLine($"평균 잔여 HP: {avgHP:F1}%");
                sb.AppendLine();

                // 카테고리별 통계
                sb.AppendLine("── 카테고리별 승률 ──");
                foreach (var category in QuickCombatCategoryOrder)
                {
                    var subset = results.FindAll(r => r.Category == category);
                    if (subset.Count == 0) continue;
                    int catWins = subset.FindAll(r => r.Victory).Count;
                    float catWinRate = 100f * catWins / subset.Count;
                    float catAvgTurn = Average(subset.ConvertAll(r => (float)r.TurnCount));
                    float catAvgHP = Average(subset.ConvertAll(r => r.AvgRemainingHP)) * 100f;
                    sb.AppendLine($"  {category,-12} 승률 {catWinRate,5:F1}%  ({catWins,3}/{subset.Count,3})  " +
                                  $"평균턴 {catAvgTurn:F2}  평균HP {catAvgHP,5:F1}%");
                }
                sb.AppendLine("═══════════════════════════════════════════");

                Debug.Log(sb.ToString());
            }

            // ── Full Run ──

            public static void WriteFullRunCsv(List<FullRunResult> results)
            {
                EnsureReportDir();
                string path = $"{REPORT_DIR}/FullRun_{TimeStamp()}.csv";
                var sb = new StringBuilder();
                sb.AppendLine(CsvHeaderRun);
                foreach (var r in results)
                {
                    sb.Append(r.RunIndex).Append(',');
                    sb.Append(r.RunCompleted ? "True" : "False").Append(',');
                    sb.Append(r.FloorReached).Append(',');
                    sb.Append(Escape(r.DeathLocation)).Append(',');
                    sb.Append(r.TotalBattles).Append(',');
                    sb.Append(r.TotalTurns).Append(',');
                    sb.Append(r.FinalPartyHP.ToString("F3", CultureInfo.InvariantCulture)).Append(',');
                    sb.Append(r.Survivors).Append(',');
                    sb.Append(r.GoldEarned).Append(',');
                    sb.Append(r.RelicsAcquired).Append(',');
                    sb.Append(r.AugmentsAcquired);
                    sb.AppendLine();
                }
                File.WriteAllText(path, sb.ToString());
                Debug.Log($"[BalanceSimulator] Full Run CSV 저장: {path} ({results.Count}행)");
            }

            public static void PrintFullRunSummary(List<FullRunResult> results)
            {
                if (results == null || results.Count == 0)
                {
                    Debug.LogWarning("[BalanceSimulator] Full Run 결과가 비어있습니다.");
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════");
                sb.AppendLine($"=== Full Run 요약 ({results.Count}회) ===");
                sb.AppendLine("═══════════════════════════════════════════");

                int clears = results.FindAll(r => r.RunCompleted).Count;
                float clearRate = 100f * clears / results.Count;
                float avgFloor = Average(results.ConvertAll(r => (float)r.FloorReached));
                float avgBattles = Average(results.ConvertAll(r => (float)r.TotalBattles));
                float avgTurns = Average(results.ConvertAll(r => (float)r.TotalTurns));
                float avgFinalHP = Average(results.ConvertAll(r => r.FinalPartyHP)) * 100f;
                float avgRelics = Average(results.ConvertAll(r => (float)r.RelicsAcquired));
                float avgAugments = Average(results.ConvertAll(r => (float)r.AugmentsAcquired));
                float avgGold = Average(results.ConvertAll(r => (float)r.GoldEarned));

                sb.AppendLine($"클리어율: {clearRate:F1}%  ({clears}/{results.Count})");
                sb.AppendLine($"평균 도달 층: {avgFloor:F2}");
                sb.AppendLine($"평균 전투 수: {avgBattles:F2}");
                sb.AppendLine($"평균 총 턴 수: {avgTurns:F2}");
                sb.AppendLine($"평균 최종 파티 HP: {avgFinalHP:F1}%");
                sb.AppendLine($"평균 획득 유물: {avgRelics:F2}");
                sb.AppendLine($"평균 획득 증강: {avgAugments:F2}");
                sb.AppendLine($"평균 획득 골드: {avgGold:F1}");
                sb.AppendLine();

                // 사망 분포 — 층별
                sb.AppendLine("── 사망 분포 (층별) ──");
                for (int f = 1; f <= 3; f++)
                {
                    int deaths = results.FindAll(r => !r.RunCompleted && r.DeathLocation != null && r.DeathLocation.StartsWith($"F{f}_")).Count;
                    float pct = 100f * deaths / results.Count;
                    sb.AppendLine($"  F{f} 사망: {deaths,3}회 ({pct:F1}%)");
                }
                int cleared = results.FindAll(r => r.RunCompleted).Count;
                sb.AppendLine($"  클리어: {cleared,3}회 ({100f * cleared / results.Count:F1}%)");
                sb.AppendLine();

                // 사망 분포 — 노드 타입별
                sb.AppendLine("── 사망 분포 (노드 타입별) ──");
                var nodeTypes = new[] { "Battle", "Elite", "Boss" };
                foreach (var nt in nodeTypes)
                {
                    int deaths = results.FindAll(r => !r.RunCompleted && r.DeathLocation != null && r.DeathLocation.EndsWith($"_{nt}")).Count;
                    sb.AppendLine($"  {nt,-8} 사망: {deaths,3}회");
                }
                sb.AppendLine();

                // 층별 클리어 진입률 — 해당 층에 도달한 런 비율
                sb.AppendLine("── 층별 도달률 ──");
                for (int f = 1; f <= 3; f++)
                {
                    int reached = results.FindAll(r => r.FloorReached >= f).Count;
                    sb.AppendLine($"  F{f} 도달: {reached,3}/{results.Count} ({100f * reached / results.Count:F1}%)");
                }
                sb.AppendLine("═══════════════════════════════════════════");

                Debug.Log(sb.ToString());
            }

            // ── 유틸리티 ──

            private static float Average(List<float> values)
            {
                if (values == null || values.Count == 0) return 0f;
                float sum = 0f;
                foreach (var v in values) sum += v;
                return sum / values.Count;
            }

            private static string Escape(string s)
            {
                if (string.IsNullOrEmpty(s)) return "";
                if (s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0) return s;
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            }

            private static string TimeStamp()
            {
                return System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            }
        }
    }
}
#endif
