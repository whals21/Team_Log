#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using TeamLog.Characters;
using TeamLog.Map;

namespace TeamLog.Editor
{
    /// <summary>
    /// DataGenerator.Stages — 스테이지 테마 에셋 자동 생성 (StageDesign.md 기반)
    /// 진입점/스킬/캐릭터/패턴/유틸리티: DataGenerator.cs
    /// 증강 데이터/스폰 패턴: DataGenerator.Augments.cs
    /// 이벤트 데이터: DataGenerator.Events.cs
    /// 유물 데이터: DataGenerator.Relics.cs
    /// 팔레트 (UI/오디오/VFX): DataGenerator.Palettes.cs
    /// 스테이지 테마: DataGenerator.Stages.cs
    /// </summary>
    public static partial class DataGenerator
    {
        private const string STAGE_PATH = "Assets/03.Data/Stages";

        /// <summary>
        /// 12개 스테이지 테마 에셋 생성 — StageDesign.md 기준
        /// 4스테이지 × 3테마 = 81가지 조합 (런 시작 시 스테이지마다 무작위 1개 채택).
        ///
        /// 적 풀 전략 (Phase 7D):
        /// - 신규 적 에셋 생성 없이, 기존 F1/F2/F3 적 풀을 재조합하여 테마별 차별화
        /// - 스테이지 4는 GetFloorScaling(2.0f)으로 자동 난이도 상승
        /// - 테마 키워드/설명은 StageDesign 그대로 반영 (UI 노출용)
        /// - 향후 Phase 7D-2에서 테마별 신규 적 점진적 추가 가능
        /// </summary>
        [MenuItem("TeamLog/Generate Stage Themes", false, 110)]
        public static void GenerateStageThemes()
        {
            EnsureFolder(STAGE_PATH);

            // ── Stage 1: 튜토리얼 (F1 적 풀 기반) ──
            CreateTheme(
                themeId: "GreyForest",
                displayName: "잿빛 숲",
                stageNumber: 1,
                normals: new[] { "Enemy_Slime", "Enemy_Goblin", "Enemy_Wolf", "Enemy_Mushroom" },
                elites: new[] { "Enemy_EliteKnight", "Enemy_EliteMage", "Enemy_EliteDarkSlime" },
                boss: "Enemy_BossGoblinKing",
                spawnTable: "SpawnPatterns_F1",
                keywords: new[] { "재생", "독" },
                desc: "튜토리얼 스테이지. AP 관리와 타겟 우선순위 학습.");

            CreateTheme(
                themeId: "FrostedPass",
                displayName: "서리 고개",
                stageNumber: 1,
                normals: new[] { "Enemy_Wolf", "Enemy_Mushroom", "Enemy_Slime", "Enemy_Goblin" },
                elites: new[] { "Enemy_EliteKnight", "Enemy_EliteMage", "Enemy_EliteDarkSlime" },
                boss: "Enemy_BossGoblinKing",
                spawnTable: "SpawnPatterns_F1",
                keywords: new[] { "둔화", "빙결" },
                desc: "빙결과 둔화로 AP를 압박하는 튜토리얼 변형.");

            CreateTheme(
                themeId: "SunscorchedPlains",
                displayName: "모래 평원",
                stageNumber: 1,
                normals: new[] { "Enemy_Goblin", "Enemy_Wolf", "Enemy_Mushroom", "Enemy_Slime" },
                elites: new[] { "Enemy_EliteKnight", "Enemy_EliteMage", "Enemy_EliteDarkSlime" },
                boss: "Enemy_BossGoblinKing",
                spawnTable: "SpawnPatterns_F1",
                keywords: new[] { "은폐", "회피" },
                desc: "회피와 은폐로 명중 관리를 요구하는 튜토리얼 변형.");

            // ── Stage 2: 체력 관리 (F2 적 풀 기반) ──
            CreateTheme(
                themeId: "CrimsonChapel",
                displayName: "혈련 예배당",
                stageNumber: 2,
                normals: new[] { "Enemy_Bat", "Enemy_Mummy", "Enemy_Skeleton", "Enemy_SkeletonArcher" },
                elites: new[] { "Enemy_EliteKnight", "Enemy_EliteMage", "Enemy_EliteDarkSlime" },
                boss: "Enemy_BossDragon",
                spawnTable: "SpawnPatterns_F2",
                keywords: new[] { "흡혈", "부활" },
                desc: "흡혈과 부활로 HP를 뺏기는 체력 관리 스테이지.");

            CreateTheme(
                themeId: "RotbloomBog",
                displayName: "부패 늪",
                stageNumber: 2,
                normals: new[] { "Enemy_Mushroom", "Enemy_Slime", "Enemy_Bat", "Enemy_Mummy" },
                elites: new[] { "Enemy_EliteKnight", "Enemy_EliteMage", "Enemy_EliteDarkSlime" },
                boss: "Enemy_BossDragon",
                spawnTable: "SpawnPatterns_F2",
                keywords: new[] { "독", "전염" },
                desc: "독과 전염으로 도트 데미지를 입히는 늪지대.");

            CreateTheme(
                themeId: "RuinedTemple",
                displayName: "유적 잔해",
                stageNumber: 2,
                normals: new[] { "Enemy_Skeleton", "Enemy_Bat", "Enemy_Mummy", "Enemy_SkeletonArcher" },
                elites: new[] { "Enemy_EliteKnight", "Enemy_EliteMage", "Enemy_EliteDarkSlime" },
                boss: "Enemy_BossDragon",
                spawnTable: "SpawnPatterns_F2",
                keywords: new[] { "언데드", "저주" },
                desc: "언데드와 저주로 상태이상 정화의 가치를 학습.");

            // ── Stage 3: 자원 압박 (F3 적 풀 기반) ──
            CreateTheme(
                themeId: "AbyssalTrench",
                displayName: "심연 해구",
                stageNumber: 3,
                normals: new[] { "Enemy_Wraith", "Enemy_Gargoyle", "Enemy_Shadow", "Enemy_DemonSoldier" },
                elites: new[] { "Enemy_EliteGoblinShaman", "Enemy_EliteSkeletonCaptain", "Enemy_EliteDemonMage" },
                boss: "Enemy_BossDemonLord",
                spawnTable: "SpawnPatterns_F3",
                keywords: new[] { "흡수", "속박" },
                desc: "흡수와 속박으로 쉴드 운영을 압박하는 심연.");

            CreateTheme(
                themeId: "Stormpeak",
                displayName: "번개 봉우리",
                stageNumber: 3,
                normals: new[] { "Enemy_Gargoyle", "Enemy_Shadow", "Enemy_Wraith", "Enemy_DemonSoldier" },
                elites: new[] { "Enemy_EliteGoblinShaman", "Enemy_EliteSkeletonCaptain", "Enemy_EliteDemonMage" },
                boss: "Enemy_BossDemonLord",
                spawnTable: "SpawnPatterns_F3",
                keywords: new[] { "기절", "연쇄" },
                desc: "기절과 연쇄 공격으로 행동 차단을 시도하는 봉우리.");

            CreateTheme(
                themeId: "ShadowsGlade",
                displayName: "그림자 골짜기",
                stageNumber: 3,
                normals: new[] { "Enemy_Shadow", "Enemy_Bat", "Enemy_Wraith", "Enemy_Gargoyle" },
                elites: new[] { "Enemy_EliteGoblinShaman", "Enemy_EliteSkeletonCaptain", "Enemy_EliteDemonMage" },
                boss: "Enemy_BossDemonLord",
                spawnTable: "SpawnPatterns_F3",
                keywords: new[] { "은신", "회피" },
                desc: "은신과 회피로 예측을 어렵게 만드는 골짜기.");

            // ── Stage 4: 클라이맥스 (F3 적 풀 + 마왕, GetFloorScaling 2.0) ──
            CreateTheme(
                themeId: "EmberThrone",
                displayName: "불꽃왕좌",
                stageNumber: 4,
                normals: new[] { "Enemy_DemonSoldier", "Enemy_Mummy", "Enemy_Wraith", "Enemy_Gargoyle" },
                elites: new[] { "Enemy_EliteDemonMage", "Enemy_EliteSkeletonCaptain", "Enemy_EliteGoblinShaman" },
                boss: "Enemy_BossDemonLord",
                spawnTable: "SpawnPatterns_F3",
                keywords: new[] { "화염", "폭발" },
                desc: "화염과 폭발로 고데미지를 입히는 클라이맥스. 모든 시스템 통합 운영 필요.");

            CreateTheme(
                themeId: "EternalTundra",
                displayName: "영원동토",
                stageNumber: 4,
                normals: new[] { "Enemy_Wraith", "Enemy_Gargoyle", "Enemy_DemonSoldier", "Enemy_Shadow" },
                elites: new[] { "Enemy_EliteDemonMage", "Enemy_EliteSkeletonCaptain", "Enemy_EliteGoblinShaman" },
                boss: "Enemy_BossDemonLord",
                spawnTable: "SpawnPatterns_F3",
                keywords: new[] { "빙결", "봉쇄" },
                desc: "빙결과 행동 봉쇄로 파티를 굳히는 영구 동토.");

            CreateTheme(
                themeId: "DemonCitadel",
                displayName: "마왕성 심장",
                stageNumber: 4,
                normals: new[] { "Enemy_DemonSoldier", "Enemy_Shadow", "Enemy_Wraith", "Enemy_Gargoyle" },
                elites: new[] { "Enemy_EliteDemonMage", "Enemy_EliteSkeletonCaptain", "Enemy_EliteGoblinShaman" },
                boss: "Enemy_BossDemonLord",
                spawnTable: "SpawnPatterns_F3",
                keywords: new[] { "소환", "다중페이즈" },
                desc: "소환과 다중 페이즈로 지속적인 전멸 위협을 주는 마왕성 심장.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DataGenerator] 스테이지 테마 12종 생성 완료 (4스테이지 × 3테마) — StageDesign.md 기준");
        }

        private static void CreateTheme(
            string themeId, string displayName, int stageNumber,
            string[] normals, string[] elites, string boss,
            string spawnTable, string[] keywords, string desc)
        {
            string path = $"{STAGE_PATH}/Theme_{themeId}.asset";
            var theme = GetOrCreateAsset<StageThemeData>(path);
            theme.name = $"Theme_{themeId}";

            theme.themeId = themeId;
            theme.displayName = displayName;
            theme.stageNumber = stageNumber;
            theme.description = desc;

            // Normal enemies
            theme.normalEnemies = LoadCharactersByNames(normals);

            // Elite enemies
            theme.eliteEnemies = LoadCharactersByNames(elites);

            // Boss
            var bossAsset = AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_PATH}/{boss}.asset");
            theme.boss = bossAsset;

            // Spawn pattern table
            var table = AssetDatabase.LoadAssetAtPath<SpawnPatternTable>($"{SPAWN_PATTERN_PATH}/{spawnTable}.asset");
            theme.spawnPatternTable = table;

            // Keywords
            theme.themeKeywords = keywords.ToList();

            EditorUtility.SetDirty(theme);
        }

        private static List<CharacterData> LoadCharactersByNames(string[] names)
        {
            var list = new List<CharacterData>();
            foreach (var n in names)
            {
                var asset = AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_PATH}/{n}.asset");
                if (asset != null)
                    list.Add(asset);
                else
                    Debug.LogWarning($"[DataGenerator.Stages] 적 에셋 누락: {n}");
            }
            return list;
        }
    }
}
#endif
