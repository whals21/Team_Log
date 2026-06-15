#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat.AI;
using TeamLog.Event;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.Skill;

using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;

namespace TeamLog.Editor
{
    /// <summary>
    /// 자동 밸런스 시뮬레이터 — 헤드리스 전투/런 시뮬레이션으로 객관적 밸런스 데이터를 CSV로 출력.
    /// 모든 시스템(TurnManager, GameRunState, RewardManager 등)이 순수 C# 클래스 기반이라
    /// 씬 로드 없이 동기 실행 가능. 이후 모든 밸런스 변경의 객관적 기준점 역할.
    ///
    /// 파일 분할:
    ///   BalanceSimulator.cs         — 진입점 + 상수 + 메뉴 + 에셋 로드
    ///   BalanceSimulator.Combat.cs  — SimulatedPlayerAI + Quick Combat
    ///   BalanceSimulator.Run.cs     — Full Run + 노드 자동 결정
    ///   BalanceSimulator.Report.cs  — 통계 집계 + CSV 출력
    /// </summary>
    public static partial class BalanceSimulator
    {
        // ── 경로 상수 ──
        private const string REPORT_DIR = "Assets/09.Docs/BalanceReports";
        private const string CHAR_PATH = "Assets/03.Data/Characters";
        private const string PATTERN_PATH = "Assets/03.Data/Patterns";
        private const string SPAWN_PATH = "Assets/03.Data/SpawnPatterns";
        private const string RELIC_PATH = "Assets/03.Data/Relics";
        private const string AUGMENT_PATH = "Assets/03.Data/Augments";
        private const string EVENT_PATH = "Assets/03.Data/Events";

        // ── 시뮬레이션 상수 ──
        public const int QuickCombatPacks = 1000;
        public const int FullRunCount = 100;
        public const int MaxTurnsPerCombat = 50;
        private const int DefaultStartingGold = 50;

        // ── 캐싱된 에셋 풀 (EnsureAssetsLoaded로 1회 로드) ──
        private static List<CharacterData> _playerCharPool;
        private static List<CharacterData> _bossDataPool;
        private static Dictionary<int, SpawnPatternTable> _spawnTables;
        private static Dictionary<string, EnemyPatternData> _enemyPatterns;
        private static List<RelicData> _relicPool;
        private static List<AugmentData> _augmentPool;
        private static List<EventData> _eventPool;

        // 기본 파티 클래스 — Char_Warrior, Char_Mage, Char_Healer, Char_Rogue
        private static readonly string[] DefaultPartyIds = { "Char_Warrior", "Char_Mage", "Char_Healer", "Char_Rogue" };

        // 층별 보스 에셋 파일명
        private static readonly string[] FloorBossIds = { "Enemy_BossGoblinKing", "Enemy_BossDragon", "Enemy_BossDemonLord" };

        // ═══════════════════════════════════════════
        // 메뉴 진입점
        // ═══════════════════════════════════════════

        [MenuItem("TeamLog/Balance/Quick Combat (1000 packs)", false, 200)]
        public static void RunQuickCombatMenu()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[BalanceSimulator] 플레이 모드에서는 실행할 수 없습니다.");
                return;
            }

            EnsureAssetsLoaded();
            if (!VerifyAssets("Quick Combat")) return;

            var results = RunQuickCombatSimulation();
            ReportUtils.WriteQuickCombatCsv(results);
            ReportUtils.PrintQuickCombatSummary(results);
            AssetDatabase.Refresh();
        }

        [MenuItem("TeamLog/Balance/Full Run (100 runs)", false, 201)]
        public static void RunFullRunMenu()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[BalanceSimulator] 플레이 모드에서는 실행할 수 없습니다.");
                return;
            }

            EnsureAssetsLoaded();
            if (!VerifyAssets("Full Run")) return;

            var results = RunFullRunSimulation();
            ReportUtils.WriteFullRunCsv(results);
            ReportUtils.PrintFullRunSummary(results);
            AssetDatabase.Refresh();
        }

        [MenuItem("TeamLog/Balance/Run All (Quick + Full)", false, 202)]
        public static void RunAllMenu()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[BalanceSimulator] 플레이 모드에서는 실행할 수 없습니다.");
                return;
            }

            EnsureAssetsLoaded();
            if (!VerifyAssets("Run All")) return;

            var combatResults = RunQuickCombatSimulation();
            ReportUtils.WriteQuickCombatCsv(combatResults);
            ReportUtils.PrintQuickCombatSummary(combatResults);

            var runResults = RunFullRunSimulation();
            ReportUtils.WriteFullRunCsv(runResults);
            ReportUtils.PrintFullRunSummary(runResults);

            AssetDatabase.Refresh();
        }

        // ═══════════════════════════════════════════
        // 에셋 로드 / 검증
        // ═══════════════════════════════════════════

        private static void EnsureAssetsLoaded()
        {
            if (_playerCharPool != null) return;

            _playerCharPool = new List<CharacterData>();
            foreach (var id in DefaultPartyIds)
            {
                var data = AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_PATH}/{id}.asset");
                if (data != null) _playerCharPool.Add(data);
            }

            // 보스 데이터
            _bossDataPool = new List<CharacterData>();
            foreach (var id in FloorBossIds)
            {
                var data = AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_PATH}/{id}.asset");
                if (data != null) _bossDataPool.Add(data);
            }

            // 스폰 패턴 테이블 (층별)
            _spawnTables = new Dictionary<int, SpawnPatternTable>();
            for (int f = 1; f <= 3; f++)
            {
                var table = AssetDatabase.LoadAssetAtPath<SpawnPatternTable>($"{SPAWN_PATH}/SpawnPatterns_F{f}.asset");
                if (table != null) _spawnTables[f] = table;
            }

            // 적 패턴 데이터 전체 로드 (EnemyPatternData → EnemyActionPattern 생성용)
            _enemyPatterns = new Dictionary<string, EnemyPatternData>();
            foreach (var guid in AssetDatabase.FindAssets("t:EnemyPatternData", new[] { PATTERN_PATH }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var pattern = AssetDatabase.LoadAssetAtPath<EnemyPatternData>(path);
                if (pattern != null && !string.IsNullOrEmpty(pattern.EnemyId))
                    _enemyPatterns[pattern.EnemyId] = pattern;
            }

            // 유물 / 증강 / 이벤트 풀
            _relicPool = LoadAllAssets<RelicData>(RELIC_PATH);
            _augmentPool = LoadAllAssets<AugmentData>(AUGMENT_PATH);
            _eventPool = LoadAllAssets<EventData>(EVENT_PATH);
        }

        private static List<T> LoadAllAssets<T>(string path) where T : UnityEngine.Object
        {
            var list = new List<T>();
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { path }))
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) list.Add(asset);
            }
            return list;
        }

        private static bool VerifyAssets(string label)
        {
            if (_playerCharPool.Count < DefaultPartyIds.Length)
            {
                Debug.LogError($"[BalanceSimulator][{label}] 기본 파티 캐릭터 에셋 누락 (found {_playerCharPool.Count}/{DefaultPartyIds.Length}). " +
                               "TeamLog/Generate Test Data 먼저 실행하세요.");
                return false;
            }
            if (_spawnTables.Count < 3)
            {
                Debug.LogError($"[BalanceSimulator][{label}] 스폰 패턴 테이블 누락 (found {_spawnTables.Count}/3).");
                return false;
            }
            if (_bossDataPool.Count < FloorBossIds.Length)
            {
                Debug.LogError($"[BalanceSimulator][{label}] 보스 캐릭터 에셋 누락 (found {_bossDataPool.Count}/{FloorBossIds.Length}).");
                return false;
            }
            return true;
        }

        // ═══════════════════════════════════════════
        // 헬퍼 — 파티 / 패턴 / 보스 생성
        // ═══════════════════════════════════════════

        /// <summary>기본 파티 새 인스턴스 생성 (HP/상태 초기화). 풀에서 랜덤이 아닌 고정 4종.</summary>
        private static List<Character> CreateDefaultParty()
        {
            var party = new List<Character>(_playerCharPool.Count);
            foreach (var data in _playerCharPool)
                party.Add(new Character(data));
            return party;
        }

        /// <summary>층별 보스 데이터 반환 (1-base). 없으면 null.</summary>
        private static CharacterData GetBossData(int floor)
        {
            int idx = floor - 1;
            if (idx < 0 || idx >= _bossDataPool.Count) return null;
            return _bossDataPool[idx];
        }

        /// <summary>
        /// 적 캐릭터용 EnemyActionPattern 생성.
        /// EnemyPatternData 에셋 우선, 없으면 enemy.Data.Skills + 기본 가중치 25 폴백.
        /// </summary>
        private static EnemyActionPattern CreatePatternFor(Character enemy)
        {
            // 에셋 이름 규칙: Pattern_{enemy asset fileName} (예: Pattern_Enemy_Slime)
            // EnemyPatternData.EnemyId는 대개 에셋 이름과 동일
            if (_enemyPatterns != null)
            {
                // 1순위: EnemyId가 enemy.Data 이름과 일치하는 패턴
                if (!string.IsNullOrEmpty(enemy.Data.name) && _enemyPatterns.TryGetValue(enemy.Data.name, out var p1))
                    return p1.CreateRuntimePattern();

                // 2순위: 파일명 기반 추정 (Pattern_<Data.name>)
                string guessKey = $"Pattern_{enemy.Data.name}";
                if (_enemyPatterns.TryGetValue(guessKey, out var p2))
                    return p2.CreateRuntimePattern();
            }

            // 폴백: 스킬 목록 + 기본 가중치 25
            var skills = new List<SkillData>();
            var weights = new List<int>();
            foreach (var skill in enemy.Data.Skills)
            {
                if (skill == null) continue;
                skills.Add(skill);
                weights.Add(25);
            }
            return new EnemyActionPattern(skills, weights);
        }

        // ═══════════════════════════════════════════
        // 진행률 바 헬퍼
        // ═══════════════════════════════════════════

        private static void ShowProgress(int current, int total, string title, string label)
        {
            float ratio = total > 0 ? (float)current / total : 0f;
            EditorUtility.DisplayCancelableProgressBar(title, $"{label} ({current}/{total})", ratio);
        }

        private static void ClearProgress()
        {
            EditorUtility.ClearProgressBar();
        }

        /// <summary>디렉토리 보장 (없으면 생성).</summary>
        private static void EnsureReportDir()
        {
            if (!AssetDatabase.IsValidFolder(REPORT_DIR))
            {
                string parent = System.IO.Path.GetDirectoryName(REPORT_DIR).Replace('\\', '/');
                string leaf = System.IO.Path.GetFileName(REPORT_DIR);
                if (!AssetDatabase.IsValidFolder(parent))
                    AssetDatabase.CreateFolder(parent, leaf);
                else
                    AssetDatabase.CreateFolder(parent, leaf);
            }
        }
    }
}
#endif
