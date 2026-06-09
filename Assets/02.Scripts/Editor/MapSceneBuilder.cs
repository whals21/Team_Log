using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Event;
using TeamLog.Reward;
using TeamLog.UI.Event;
using TeamLog.UI.Map;
using TeamLog.UI.Reward;
using TeamLog.UI.Shop;
using TeamLog.UI;
using TeamLog.UI.Title;
using TeamLog.Skill;
using TeamLog.Map;

namespace TeamLog.Editor
{
    /// <summary>
    /// MapSceneBuilder — 진입점, 씬 빌드, 데이터 와이어링
    /// 패널 생성: MapSceneBuilder.Panels.cs
    /// 헬퍼: MapSceneBuilder.Helpers.cs
    /// </summary>
    public static partial class MapSceneBuilder
    {
        private const string SCENE_PATH = "Assets/01.Scenes/MapScene.unity";
        private const string KOREAN_FONT_SDF = "Assets/08.Resource/Fonts/NanumGothic SDF.asset";
        private const string PREFAB_DIR = "Assets/03.Data/Prefabs";
        private const string CHAR_DIR = "Assets/03.Data/Characters";
        private const string SKILL_DIR = "Assets/03.Data/Skills";
        private const string EVENT_DIR = "Assets/03.Data/Events";
        private const string AUGMENT_DIR = "Assets/03.Data/Augments";
        private const string SPAWN_PATTERN_DIR = "Assets/03.Data/SpawnPatterns";

        // 색상 팔레트 (기존 BattleUI와 통일)
        private static readonly Color BgDark = new Color(0.08f, 0.08f, 0.16f);
        private static readonly Color PanelDark = new Color(0.12f, 0.12f, 0.22f);
        private static readonly Color OverlayBg = new Color(0f, 0f, 0f, 0.7f);
        private static readonly Color ContentPanel = new Color(0.1f, 0.1f, 0.2f, 0.95f);
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextDim = new Color(0.7f, 0.7f, 0.75f);
        private static readonly Color AccentGold = new Color(0.96f, 0.82f, 0.25f);

        [MenuItem("TeamLog/Scene/Build Title Scene")]
        public static void BuildTitleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KOREAN_FONT_SDF);

            // 카메라
            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = BgDark;
            camObj.tag = "MainCamera";

            // 캔버스
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // 배경
            CreateFullImage("Background", canvasObj.transform, BgDark);

            // 타이틀 텍스트
            var titleText = CreateText("TitleText", canvasObj.transform, font,
                "TEAM LOG", 64, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(titleText.GetComponent<RectTransform>(),
                new Vector2(0.2f, 0.65f), new Vector2(0.8f, 0.8f));

            // 통계 라벨
            var statsLabel = CreateText("StatsLabel", canvasObj.transform, font,
                "", 22, TextDim, TextAlignmentOptions.Center);
            SetAnchors(statsLabel.GetComponent<RectTransform>(),
                new Vector2(0.3f, 0.55f), new Vector2(0.7f, 0.65f));

            // 버튼 컨테이너
            var btnContainer = CreateUIObject("ButtonContainer", canvasObj.transform);
            SetAnchors(btnContainer.GetComponent<RectTransform>(),
                new Vector2(0.3f, 0.25f), new Vector2(0.7f, 0.5f));

            var layout = btnContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var newGameBtn = CreateButton("NewGameButton", btnContainer.transform, font,
                "새 게임", 32, TextWhite);
            var newGameLayout = newGameBtn.AddComponent<LayoutElement>();
            newGameLayout.minHeight = 60;

            var continueBtn = CreateButton("ContinueButton", btnContainer.transform, font,
                "이어하기", 32, TextWhite);
            var continueLayout = continueBtn.AddComponent<LayoutElement>();
            continueLayout.minHeight = 60;

            // 이어하기 차단 오버레이
            var continueBlock = CreateFullImage("ContinueBlock", continueBtn.transform,
                new Color(0f, 0f, 0f, 0.6f));
            var blockText = CreateText("BlockText", continueBlock.transform, font,
                "저장 데이터 없음", 20, TextDim, TextAlignmentOptions.Center);
            SetAnchors(blockText.GetComponent<RectTransform>(),
                Vector2.zero, Vector2.one);

            // TitleSceneSetup 컴포넌트
            var setupObj = new GameObject("TitleSceneSetup");
            var setup = setupObj.AddComponent<TeamLog.UI.Title.TitleSceneSetup>();
            var setupSer = new SerializedObject(setup);
            WireProperty(setupSer, "_newGameButton", newGameBtn.GetComponent<Button>());
            WireProperty(setupSer, "_continueButton", continueBtn.GetComponent<Button>());
            WireProperty(setupSer, "_statsLabel", statsLabel);
            WireProperty(setupSer, "_continueBlock", continueBlock);
            setupSer.ApplyModifiedProperties();

            // EventSystem
            var eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // 씬 저장
            const string TITLE_SCENE_PATH = "Assets/01.Scenes/TitleScene.unity";
            EditorSceneManager.SaveScene(scene, TITLE_SCENE_PATH);

            // Build Settings에 TitleScene 추가 (Index 0)
            AddSceneToBuildSettings(TITLE_SCENE_PATH, 0);

            Debug.Log($"[MapSceneBuilder] 타이틀 씬 생성 완료: {TITLE_SCENE_PATH}");
        }

        /// <summary>
        /// Build Settings에 씬 추가 — 지정 인덱스에 삽입
        /// </summary>
        private static void AddSceneToBuildSettings(string scenePath, int insertIndex)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            // 이미 존재하면 스킵
            foreach (var s in scenes)
                if (s.path == scenePath) return;

            // 인덱스 위치에 삽입
            if (insertIndex < 0) insertIndex = 0;
            if (insertIndex > scenes.Count) insertIndex = scenes.Count;
            scenes.Insert(insertIndex, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        [MenuItem("TeamLog/Scene/Build Map Scene")]
        public static void BuildMapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KOREAN_FONT_SDF);

            // 카메라
            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = BgDark;
            camObj.tag = "MainCamera";

            // 캔버스
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // 배경
            CreateFullImage("Background", canvasObj.transform, BgDark);

            // 상단 정보 패널
            var topPanel = CreatePanel("TopPanel", canvasObj.transform,
                new Vector2(0, 0.92f), new Vector2(1, 1), PanelDark);

            var floorLabel = CreateText("FloorLabel", topPanel.transform, font,
                "층 1", 28, TextWhite, TextAlignmentOptions.Center);
            SetAnchors(floorLabel.GetComponent<RectTransform>(),
                new Vector2(0.35f, 0f), new Vector2(0.65f, 1f));

            var goldLabel = CreateText("GoldLabel", topPanel.transform, font,
                "0 G", 24, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(goldLabel.GetComponent<RectTransform>(),
                new Vector2(0.65f, 0f), new Vector2(0.85f, 1f));

            // 덱 버튼
            var deckBtn = CreateButton("DeckButton", topPanel.transform, font,
                "덱", 20, TextWhite);
            SetAnchors(deckBtn.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.05f), new Vector2(0.15f, 0.95f));

            // 맵 컨테이너
            var nodeContainer = CreateUIObject("NodeContainer", canvasObj.transform);
            SetAnchors(nodeContainer.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.9f));

            var lineContainer = CreateUIObject("LineContainer", canvasObj.transform);
            SetAnchors(lineContainer.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.9f));

            // ─── 서브 UI 패널 생성 ───
            var eventPanel = BuildEventPanel(canvasObj.transform, font);
            var shopPanel = BuildShopPanel(canvasObj.transform, font);
            var rewardPanel = BuildRewardPanel(canvasObj.transform, font);
            var confirmationDialog = BuildConfirmationDialog(canvasObj.transform, font);
            var restPanel = BuildRestPanel(canvasObj.transform, font);
            var runEndOverlay = BuildRunEndOverlay(canvasObj.transform, font);
            var relicBar = BuildRelicBar(canvasObj.transform, font);
            var deckViewerPanel = BuildDeckViewerPanel(canvasObj.transform, font);
            var tutorialOverlay = BuildTutorialOverlay(canvasObj.transform, font);
            var characterSelectPanel = BuildCharacterSelectPanel(canvasObj.transform, font);

            // ShopUI에 ConfirmationDialog 참조 연결
            var shopUISer = new SerializedObject(shopPanel.GetComponent<ShopUI>());
            WireProperty(shopUISer, "_confirmationDialog", confirmationDialog.GetComponent<ConfirmationDialog>());
            shopUISer.ApplyModifiedProperties();

            // ─── MapView 컴포넌트 ───
            var mapView = canvasObj.AddComponent<MapView>();

            var nodeButtonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_DIR}/MapNodeButton.prefab");
            var connectionLinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_DIR}/MapConnectionLine.prefab");
            var playerMarkerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_DIR}/MapPlayerMarker.prefab");

            var mapViewSer = new SerializedObject(mapView);
            WireProperty(mapViewSer, "_nodeContainer", nodeContainer.GetComponent<RectTransform>());
            WireProperty(mapViewSer, "_lineContainer", lineContainer.GetComponent<RectTransform>());
            WireProperty(mapViewSer, "_floorLabel", floorLabel);
            WireProperty(mapViewSer, "_goldLabel", goldLabel);
            WireProperty(mapViewSer, "_nodeButtonPrefab", nodeButtonPrefab);
            WireProperty(mapViewSer, "_connectionLinePrefab", connectionLinePrefab);
            WireProperty(mapViewSer, "_playerMarkerPrefab", playerMarkerPrefab);
            mapViewSer.ApplyModifiedProperties();

            // ─── MapSceneSetup ───
            var setupObj = new GameObject("MapSceneSetup");
            var setup = setupObj.AddComponent<MapSceneSetup>();

            var setupSer = new SerializedObject(setup);
            WireProperty(setupSer, "_mapView", mapView);
            WireProperty(setupSer, "_eventUI", eventPanel.GetComponent<EventUI>());
            WireProperty(setupSer, "_shopUI", shopPanel.GetComponent<ShopUI>());
            WireProperty(setupSer, "_rewardUI", rewardPanel.GetComponent<RewardUI>());
            WireProperty(setupSer, "_confirmationDialog", confirmationDialog.GetComponent<ConfirmationDialog>());
            WireProperty(setupSer, "_restUI", restPanel.GetComponent<RestUI>());
            WireProperty(setupSer, "_runEndOverlay", runEndOverlay.GetComponent<RunEndOverlay>());
            WireProperty(setupSer, "_relicBarUI", relicBar.GetComponent<RelicBarUI>());
            WireProperty(setupSer, "_deckViewerUI", deckViewerPanel.GetComponent<DeckViewerUI>());
            WireProperty(setupSer, "_deckButton", deckBtn.GetComponent<Button>());
            WireProperty(setupSer, "_tutorialUI", tutorialOverlay.GetComponent<TutorialUI>());

            // CharacterData
            WireProperty(setupSer, "_testWarriorData",
                AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_DIR}/Char_Warrior.asset"));
            WireProperty(setupSer, "_testMageData",
                AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_DIR}/Char_Mage.asset"));
            WireProperty(setupSer, "_testHealerData",
                AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_DIR}/Char_Healer.asset"));
            WireProperty(setupSer, "_testRogueData",
                AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_DIR}/Char_Rogue.asset"));

            // EnemyData pools — 층별(floor-based) 적 풀 구성
            var floorPoolsProp = setupSer.FindProperty("_floorPools");

            // 층별 매핑 정의
            string[][] floorNormals = new string[][]
            {
                new[] { "Enemy_Slime", "Enemy_Goblin", "Enemy_Wolf", "Enemy_Mushroom" },           // Floor 1: 숲
                new[] { "Enemy_Skeleton", "Enemy_Bat", "Enemy_Mummy", "Enemy_SkeletonArcher" },    // Floor 2: 유적
                new[] { "Enemy_Wraith", "Enemy_Shadow", "Enemy_DemonSoldier", "Enemy_Gargoyle" }   // Floor 3: 심연
            };
            string[][] floorElites = new string[][]
            {
                new string[0],                                                                               // Floor 1: 엘리트 없음
                new[] { "Enemy_EliteKnight", "Enemy_EliteMage", "Enemy_EliteDarkSlime" },                    // Floor 2
                new[] { "Enemy_EliteGoblinShaman", "Enemy_EliteSkeletonCaptain", "Enemy_EliteDemonMage" }    // Floor 3
            };
            string[] floorBosses = new[] { "Enemy_BossGoblinKing", "Enemy_BossDragon", "Enemy_BossDemonLord" };

            if (floorPoolsProp != null)
            {
                floorPoolsProp.arraySize = 3;
                for (int f = 0; f < 3; f++)
                {
                    var poolProp = floorPoolsProp.GetArrayElementAtIndex(f);

                    // Normal enemies
                    var normalProp = poolProp.FindPropertyRelative("normalEnemies");
                    var normals = LoadAssetsByNames<CharacterData>(CHAR_DIR, floorNormals[f]);
                    normalProp.arraySize = normals.Count;
                    for (int i = 0; i < normals.Count; i++)
                        normalProp.GetArrayElementAtIndex(i).objectReferenceValue = normals[i];

                    // Elite enemies
                    var eliteProp = poolProp.FindPropertyRelative("eliteEnemies");
                    var elites = LoadAssetsByNames<CharacterData>(CHAR_DIR, floorElites[f]);
                    eliteProp.arraySize = elites.Count;
                    for (int i = 0; i < elites.Count; i++)
                        eliteProp.GetArrayElementAtIndex(i).objectReferenceValue = elites[i];

                    // Boss
                    var bossAsset = AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_DIR}/{floorBosses[f]}.asset");
                    poolProp.FindPropertyRelative("boss").objectReferenceValue = bossAsset;
                }
            }

            // EventData
            var eventAssets = LoadAllAssets<EventData>(EVENT_DIR);
            var testEventsProp = setupSer.FindProperty("_testEvents");
            if (testEventsProp != null && eventAssets.Count > 0)
            {
                testEventsProp.arraySize = eventAssets.Count;
                for (int i = 0; i < eventAssets.Count; i++)
                    testEventsProp.GetArrayElementAtIndex(i).objectReferenceValue = eventAssets[i];
            }

            // SpawnPatternTable — 층별 스폰 패턴 테이블 (F1/F2/F3)
            var spawnPatternProp = setupSer.FindProperty("_spawnPatternTables");
            if (spawnPatternProp != null)
            {
                string[] patternNames = { "SpawnPatterns_F1", "SpawnPatterns_F2", "SpawnPatterns_F3" };
                spawnPatternProp.arraySize = patternNames.Length;
                for (int i = 0; i < patternNames.Length; i++)
                {
                    var path = $"{SPAWN_PATTERN_DIR}/{patternNames[i]}.asset";
                    // LoadAssetAtPath<SpawnPatternTable>이 스크립트 참조 누락으로 null을 반환할 수 있으므로
                    // AssetDatabase.LoadMainAssetAtPath로 우회
                    var obj = AssetDatabase.LoadMainAssetAtPath(path);
                    var table = obj as SpawnPatternTable;
                    if (table == null && obj != null)
                        Debug.LogWarning($"[MapSceneBuilder] {patternNames[i]}: 로드됨({obj.GetType().Name})이지만 SpawnPatternTable로 캐스팅 실패");
                    else if (table == null)
                        Debug.LogWarning($"[MapSceneBuilder] {patternNames[i]}: 에셋 로드 실패 (path={path})");
                    spawnPatternProp.GetArrayElementAtIndex(i).objectReferenceValue = table;
                }
            }

            // AugmentData pool
            var augmentAssets = LoadAllAssets<AugmentData>(AUGMENT_DIR);
            var augmentPoolProp = setupSer.FindProperty("_augmentPool");
            if (augmentPoolProp != null && augmentAssets.Count > 0)
            {
                augmentPoolProp.arraySize = augmentAssets.Count;
                for (int i = 0; i < augmentAssets.Count; i++)
                    augmentPoolProp.GetArrayElementAtIndex(i).objectReferenceValue = augmentAssets[i];
            }

            // All Characters — Char_ 접두사 에셋만 (적 제외)
            var allCharProp = setupSer.FindProperty("_allCharacters");
            if (allCharProp != null)
            {
                string[] charNames = {
                    "Char_Warrior", "Char_Mage", "Char_Healer", "Char_Rogue",
                    "Char_Archer", "Char_Necromancer", "Char_Alchemist", "Char_Bard"
                };
                var charAssets = LoadAssetsByNames<CharacterData>(CHAR_DIR, charNames);
                allCharProp.arraySize = charAssets.Count;
                for (int i = 0; i < charAssets.Count; i++)
                    allCharProp.GetArrayElementAtIndex(i).objectReferenceValue = charAssets[i];
            }

            // CharacterSelectUI — 씬에 생성된 패널 와이어링
            WireProperty(setupSer, "_characterSelectUI", characterSelectPanel.GetComponent<CharacterSelectUI>());

            // RelicData pool
            var relicAssets = LoadAllAssets<RelicData>("Assets/03.Data/Relics");
            var relicPoolProp = setupSer.FindProperty("_relicPool");
            if (relicPoolProp != null && relicAssets.Count > 0)
            {
                relicPoolProp.arraySize = relicAssets.Count;
                for (int i = 0; i < relicAssets.Count; i++)
                    relicPoolProp.GetArrayElementAtIndex(i).objectReferenceValue = relicAssets[i];
            }

            setupSer.ApplyModifiedProperties();

            // EventSystem
            var eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // 씬 저장
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log($"[MapSceneBuilder] 맵 씬 생성 완료: {SCENE_PATH}");
            Debug.Log($"[MapSceneBuilder] 프리팹: Node={nodeButtonPrefab != null}, Line={connectionLinePrefab != null}, Marker={playerMarkerPrefab != null}");
            Debug.Log($"[MapSceneBuilder] 증강 풀: {augmentAssets.Count}개, 유물 풀: {relicAssets.Count}개, 이벤트: {eventAssets.Count}개");
        }
    }
}
