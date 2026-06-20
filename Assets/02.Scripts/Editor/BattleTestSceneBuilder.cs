#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.EditorDebug;
using TeamLog.Reward;

namespace TeamLog.Editor
{
    /// <summary>
    /// BattleTestScene 자동 빌더 — BattleScene.unity를 복제하여 BattleTestScene.unity 생성.
    /// 기존 BattleUISceneBuilder가 만든 모든 인스펙터 참조를 그대로 보존 (BattleUIManager, BattleEndOverlay 등).
    /// 추가로 ConfigCanvas + BattleTestConfigPanel을 얹고 BattleTestSceneSetup 컴포넌트에 모든 에셋/참조 바인딩.
    ///
    /// 메뉴: TeamLog/Scene/Build Battle Test Scene
    ///
    /// Partial files:
    /// - BattleTestSceneBuilder.cs        — 진입점 + 상수/색상 + 씬 복제 오케스트레이션 + 에셋 로드 + 바인딩 + 유틸
    /// - BattleTestSceneBuilder.UI.cs     — UI 생성 (ConfigCanvas, 드롭다운, 토글, 버튼, 인풋필드)
    /// </summary>
    public static partial class BattleTestSceneBuilder
    {
        private const string SCENE_PATH = "Assets/01.Scenes/BattleTestScene.unity";
        private const string SOURCE_SCENE = "Assets/01.Scenes/BattleScene.unity";
        private const string CHAR_PATH = "Assets/03.Data/Characters";
        private const string RELIC_PATH = "Assets/03.Data/Relics";

        // 색상 토큰 — BattleUISceneBuilder와 동일
        internal static readonly Color BgDark = new(0.06f, 0.06f, 0.12f, 0.98f);
        internal static readonly Color AccentYellow = new(0.96f, 0.82f, 0.25f);
        internal static readonly Color AccentGreen = new(0.15f, 0.68f, 0.38f);
        internal static readonly Color TextWhite = Color.white;
        internal static readonly Color TextDim = new(0.82f, 0.82f, 0.87f);

        [MenuItem("TeamLog/Scene/Build Battle Test Scene", false, 100)]
        public static void BuildScene()
        {
            // 1. 현재 BattleTestScene이 열려 있으면 저장 후 닫기 (덮어쓰기 안전)
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var opened = EditorSceneManager.GetSceneAt(i);
                if (opened.path == SCENE_PATH)
                {
                    EditorSceneManager.SaveScene(opened);
                    break;
                }
            }

            // 2. 기존 BattleTestScene 삭제 후 BattleScene 복사
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(SCENE_PATH) != null)
                AssetDatabase.DeleteAsset(SCENE_PATH);

            if (!AssetDatabase.CopyAsset(SOURCE_SCENE, SCENE_PATH))
            {
                Debug.LogError($"[BattleTestSceneBuilder] 씬 복사 실패: {SOURCE_SCENE} → {SCENE_PATH}");
                return;
            }
            AssetDatabase.Refresh();

            // 3. 씬 열기
            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

            try
            {
                // 4. 기존 BattleSceneSetup GO 비활성화 + _useTestData=false
                var battleSetupGO = FindBattleSceneSetupGO(scene);
                Canvas battleUICanvas = null;
                if (battleSetupGO != null)
                {
                    var setup = battleSetupGO.GetComponent<BattleSceneSetup>();
                    if (setup != null)
                        SetPrivateField(setup, "_useTestData", false);
                    battleSetupGO.SetActive(false); // Start()에서 _pendingTestBattle 분기 시 활성화

                    // 부모 BattleUICanvas의 Canvas 컴포넌트 비활성화 (렌더링/레이캐스트만 차단, GO는 활성 유지)
                    battleUICanvas = battleSetupGO.GetComponentInParent<Canvas>();
                    if (battleUICanvas != null)
                    {
                        battleUICanvas.enabled = false;
                        Debug.Log("[BattleTestSceneBuilder] BattleUICanvas 렌더링 비활성화 (Canvas.enabled=false)");
                    }

                    Debug.Log("[BattleTestSceneBuilder] BattleSceneSetup GO 비활성화 + _useTestData=false");
                }
                else
                {
                    Debug.LogWarning("[BattleTestSceneBuilder] BattleSceneSetup GO를 찾지 못했습니다. BattleUISceneBuilder로 BattleScene을 먼저 빌드하세요.");
                }

                // 5. ConfigCanvas + 패널 + 드롭다운 생성 (UI partial)
                var refs = CreateConfigCanvas(scene);

                // 6. BattleTestSceneSetup GO + 컴포넌트 + 에셋/참조 바인딩
                BindBattleTestSceneSetup(scene, refs, battleSetupGO, battleUICanvas);

                // 7. EventSystem 확인 (없으면 추가 — 씬 복사 시 보통 유지되지만 안전장치)
                EnsureEventSystem(scene);

                // 8. 저장
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                // 9. 빌드 세팅에 추가
                AddSceneToBuildSettings(SCENE_PATH);

                Debug.Log($"[BattleTestSceneBuilder] BattleTestScene 빌드 완료! → {SCENE_PATH}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BattleTestSceneBuilder] 빌드 중 예외: {e}");
                EditorSceneManager.SaveScene(scene); // 부분 결과라도 저장
            }
        }

        // ══════════════════════════════════════════════════════════
        //  BattleSceneSetup GO 탐색
        // ══════════════════════════════════════════════════════════

        private static GameObject FindBattleSceneSetupGO(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                // 루트 자체에 BattleSceneSetup이 있는 경우
                var direct = root.GetComponent<BattleSceneSetup>();
                if (direct != null) return root;

                // 자식에서 탐색 (BattleUISceneBuilder는 BattleUIRoot의 부모에 추가)
                var inChildren = root.GetComponentInChildren<BattleSceneSetup>(true);
                if (inChildren != null) return inChildren.gameObject;
            }
            return null;
        }

        // ══════════════════════════════════════════════════════════
        //  ConfigRefs — UI partial에서 생성, 여기서 바인딩에 사용
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// ConfigCanvas의 모든 UI 참조를 담는 컨테이너. UI partial에서 채워지고
        /// BindBattleTestSceneSetup에서 BattleTestSceneSetup 컴포넌트로 바인딩.
        /// </summary>
        internal class ConfigRefs
        {
            public TMP_Dropdown[] PartySlots = new TMP_Dropdown[4];
            public TMP_Dropdown[] RelicSlots = new TMP_Dropdown[6];
            public TMP_Dropdown[] EnemySlots = new TMP_Dropdown[4];
            public TMP_Dropdown FloorDropdown;
            public Toggle BossToggle;
            public Button StartButton;
            public GameObject ConfigPanel;

            // 템플릿 UI — 카테고리별 독립
            public TMP_InputField PartyTemplateNameInput;
            public TMP_Dropdown PartyTemplateDropdown;
            public Button PartyTemplateSaveButton;
            public Button PartyTemplateLoadButton;
            public Button PartyTemplateDeleteButton;

            public TMP_InputField RelicTemplateNameInput;
            public TMP_Dropdown RelicTemplateDropdown;
            public Button RelicTemplateSaveButton;
            public Button RelicTemplateLoadButton;
            public Button RelicTemplateDeleteButton;

            public TMP_InputField EnemyTemplateNameInput;
            public TMP_Dropdown EnemyTemplateDropdown;
            public Button EnemyTemplateSaveButton;
            public Button EnemyTemplateLoadButton;
            public Button EnemyTemplateDeleteButton;
        }

        // ══════════════════════════════════════════════════════════
        //  BattleTestSceneSetup 컴포넌트 바인딩
        // ══════════════════════════════════════════════════════════

        private static void BindBattleTestSceneSetup(Scene scene, ConfigRefs refs, GameObject battleSetupGO, Canvas battleUICanvas)
        {
            var setupGO = new GameObject("BattleTestSceneSetup");
            SceneManager.MoveGameObjectToScene(setupGO, scene);
            var setup = setupGO.AddComponent<BattleTestSceneSetup>();

            // 에셋 풀 로드
            var players = LoadAllCharacters("Char_");
            var normalEnemies = LoadAllCharacters("Enemy_");
            var eliteEnemies = LoadAllCharacters("Enemy_Elite");
            var bosses = LoadAllCharacters("Enemy_Boss");
            var relics = LoadAllRelics();

            // 일반 적과 엘리트 적을 하나의 통합 풀로 결합
            var allEnemies = new List<CharacterData>();
            allEnemies.AddRange(normalEnemies);
            allEnemies.AddRange(eliteEnemies);

            SetPrivateField(setup, "_allPlayers", players);
            SetPrivateField(setup, "_allEnemies", allEnemies.ToArray());
            SetPrivateField(setup, "_allBosses", bosses);
            SetPrivateField(setup, "_allRelics", relics);

            // UI 참조 바인딩
            SetPrivateField(setup, "_partySlots", refs.PartySlots);
            SetPrivateField(setup, "_relicSlots", refs.RelicSlots);
            SetPrivateField(setup, "_enemySlots", refs.EnemySlots);
            SetPrivateField(setup, "_floorDropdown", refs.FloorDropdown);
            SetPrivateField(setup, "_bossToggle", refs.BossToggle);
            SetPrivateField(setup, "_startButton", refs.StartButton);
            SetPrivateField(setup, "_configPanel", refs.ConfigPanel);
            SetPrivateField(setup, "_battleSceneSetupGO", battleSetupGO);
            SetPrivateField(setup, "_battleUICanvas", battleUICanvas);

            // 템플릿 UI 바인딩
            SetPrivateField(setup, "_partyTemplateNameInput", refs.PartyTemplateNameInput);
            SetPrivateField(setup, "_partyTemplateDropdown", refs.PartyTemplateDropdown);
            SetPrivateField(setup, "_partyTemplateSaveButton", refs.PartyTemplateSaveButton);
            SetPrivateField(setup, "_partyTemplateLoadButton", refs.PartyTemplateLoadButton);
            SetPrivateField(setup, "_partyTemplateDeleteButton", refs.PartyTemplateDeleteButton);
            SetPrivateField(setup, "_relicTemplateNameInput", refs.RelicTemplateNameInput);
            SetPrivateField(setup, "_relicTemplateDropdown", refs.RelicTemplateDropdown);
            SetPrivateField(setup, "_relicTemplateSaveButton", refs.RelicTemplateSaveButton);
            SetPrivateField(setup, "_relicTemplateLoadButton", refs.RelicTemplateLoadButton);
            SetPrivateField(setup, "_relicTemplateDeleteButton", refs.RelicTemplateDeleteButton);
            SetPrivateField(setup, "_enemyTemplateNameInput", refs.EnemyTemplateNameInput);
            SetPrivateField(setup, "_enemyTemplateDropdown", refs.EnemyTemplateDropdown);
            SetPrivateField(setup, "_enemyTemplateSaveButton", refs.EnemyTemplateSaveButton);
            SetPrivateField(setup, "_enemyTemplateLoadButton", refs.EnemyTemplateLoadButton);
            SetPrivateField(setup, "_enemyTemplateDeleteButton", refs.EnemyTemplateDeleteButton);

            EditorUtility.SetDirty(setup);

            Debug.Log($"[BattleTestSceneBuilder] 에셋 바인딩: Players={players.Length}, " +
                      $"Enemies={allEnemies.Count} (Normal={normalEnemies.Length}, Elite={eliteEnemies.Length}), " +
                      $"Bosses={bosses.Length}, Relics={relics.Length}, " +
                      $"BattleUICanvas={(battleUICanvas != null ? "OK" : "MISSING")}");
        }

        // ══════════════════════════════════════════════════════════
        //  에셋 로드
        // ══════════════════════════════════════════════════════════

        private static CharacterData[] LoadAllCharacters(string namePrefix)
        {
            var list = new List<CharacterData>();
            foreach (var guid in AssetDatabase.FindAssets("t:CharacterData", new[] { CHAR_PATH }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
                if (data == null) continue;

                // 접두사 매칭 — 파일 경로/이름 기반 (CLAUDE.md MapSceneBuilder 규칙)
                if (!data.name.StartsWith(namePrefix)) continue;

                // Enemy_ 접두사일 때 Elite/Boss는 제외
                if (namePrefix == "Enemy_" && (data.name.StartsWith("Enemy_Elite") || data.name.StartsWith("Enemy_Boss")))
                    continue;

                list.Add(data);
            }
            return list.ToArray();
        }

        private static RelicData[] LoadAllRelics()
        {
            var list = new List<RelicData>();
            foreach (var guid in AssetDatabase.FindAssets("t:RelicData", new[] { RELIC_PATH }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<RelicData>(path);
                if (data != null) list.Add(data);
            }
            return list.ToArray();
        }

        // ══════════════════════════════════════════════════════════
        //  EventSystem / Build Settings
        // ══════════════════════════════════════════════════════════

        private static void EnsureEventSystem(Scene scene)
        {
            var existing = Object.FindObjectOfType<EventSystem>();
            if (existing != null) return;

            var go = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            Debug.Log("[BattleTestSceneBuilder] EventSystem 생성");
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int idx = scenes.FindIndex(s => s.path == scenePath);
            if (idx >= 0)
            {
                scenes[idx].enabled = true;
            }
            else
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ══════════════════════════════════════════════════════════
        //  Reflection 유틸리티
        // ══════════════════════════════════════════════════════════

        internal static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}
#endif
