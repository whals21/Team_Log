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
    /// </summary>
    public static class BattleTestSceneBuilder
    {
        private const string SCENE_PATH = "Assets/01.Scenes/BattleTestScene.unity";
        private const string SOURCE_SCENE = "Assets/01.Scenes/BattleScene.unity";
        private const string CHAR_PATH = "Assets/03.Data/Characters";
        private const string RELIC_PATH = "Assets/03.Data/Relics";

        // 색상 토큰 — BattleUISceneBuilder와 동일
        private static readonly Color BgDark = new(0.06f, 0.06f, 0.12f, 0.98f);
        private static readonly Color AccentYellow = new(0.96f, 0.82f, 0.25f);
        private static readonly Color AccentGreen = new(0.15f, 0.68f, 0.38f);
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextDim = new(0.82f, 0.82f, 0.87f);

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
                    // - 설정 모드에서 전투 UI가 화면에 표시되어 ConfigCanvas와 겹치는 문제 방지
                    // - GO를 활성 상태로 유지하여 Awake/Start가 정상 호출되도록 보장
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

                // 5. ConfigCanvas + 패널 + 드롭다운 생성
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
        //  ConfigCanvas + Panel + 드롭다운 생성
        // ══════════════════════════════════════════════════════════

        private class ConfigRefs
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

        private static ConfigRefs CreateConfigCanvas(Scene scene)
        {
            var canvasGO = new GameObject("ConfigCanvas");
            SceneManager.MoveGameObjectToScene(canvasGO, scene);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200; // BattleUICanvas(100) 위에 표시

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // 패널 — 중앙 정렬 어두운 반투명 (96% 너비 — 어떤 화면 비율에서도 잘림 방지)
            var panel = NewRect("ConfigPanel", canvasGO.transform);
            panel.anchorMin = new Vector2(0.02f, 0.02f);
            panel.anchorMax = new Vector2(0.98f, 0.98f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            var panelBg = panel.gameObject.AddComponent<Image>();
            panelBg.color = BgDark;
            var panelVlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            panelVlg.spacing = 14;
            panelVlg.padding = new RectOffset(24, 24, 24, 24);
            panelVlg.childAlignment = TextAnchor.MiddleCenter;  // 수직/수평 중앙 정렬 (패널이 커도 중앙에 모임)
            panelVlg.childControlWidth = true;
            panelVlg.childControlHeight = false;
            panelVlg.childForceExpandWidth = true;
            panelVlg.childForceExpandHeight = false;

            // 제목
            AddLabel(panel, "Battle Test 설정", 28, AccentYellow, 50f);

            var refs = new ConfigRefs { ConfigPanel = panel.gameObject };

            // 파티 행 (4 슬롯) + 템플릿 행
            refs.PartySlots = CreateDropdownRow(panel, "파티:", 4, 30f);
            CreateTemplateRow(panel, "파티 템플릿:",
                out refs.PartyTemplateNameInput, out refs.PartyTemplateDropdown,
                out refs.PartyTemplateSaveButton, out refs.PartyTemplateLoadButton, out refs.PartyTemplateDeleteButton);

            // 유물 행 (6 슬롯) + 템플릿 행
            refs.RelicSlots = CreateDropdownRow(panel, "유물 (최대 6):", 6, 30f);
            CreateTemplateRow(panel, "유물 템플릿:",
                out refs.RelicTemplateNameInput, out refs.RelicTemplateDropdown,
                out refs.RelicTemplateSaveButton, out refs.RelicTemplateLoadButton, out refs.RelicTemplateDeleteButton);

            // 적 행 (4 슬롯) + 템플릿 행
            refs.EnemySlots = CreateDropdownRow(panel, "적:", 4, 30f);

            // 층 + 보스 토글 행
            var floorRow = NewRect("FloorRow", panel);
            AddHeight(floorRow, 36f);
            var floorHlg = floorRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            floorHlg.spacing = 12;
            floorHlg.childAlignment = TextAnchor.MiddleCenter;
            floorHlg.childControlWidth = false;
            floorHlg.childForceExpandWidth = false;

            AddLabel(floorRow, "층:", 18, TextWhite, 50f);
            refs.FloorDropdown = CreateDropdown(floorRow, 140f, 30f);
            AddLabel(floorRow, "  ", 18, TextWhite, 20f);
            refs.BossToggle = CreateToggle(floorRow, "보스", 120f, 30f);

            // 적 템플릿 행 (floor + boss 포함)
            CreateTemplateRow(panel, "적 템플릿:",
                out refs.EnemyTemplateNameInput, out refs.EnemyTemplateDropdown,
                out refs.EnemyTemplateSaveButton, out refs.EnemyTemplateLoadButton, out refs.EnemyTemplateDeleteButton);

            // 시작 버튼
            var btnRow = NewRect("ButtonRow", panel);
            AddHeight(btnRow, 60f);
            var btnHlg = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            btnHlg.childAlignment = TextAnchor.MiddleCenter;
            refs.StartButton = CreateButton(btnRow, "전투 시작", 280f, 50f, AccentGreen);

            return refs;
        }

        private static TMP_Dropdown[] CreateDropdownRow(Transform parent, string label, int count, float slotHeight)
        {
            var row = NewRect($"{label}Row", parent);
            AddHeight(row, slotHeight + 4f);
            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.padding = new RectOffset(12, 12, 2, 2);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = false;

            // 행 라벨 — 고정 폭, 확장 안 함 (모든 행이 동일한 들여쓰기로 정렬)
            var lblRect = AddLabel(row, label, 16, TextDim, 110f);
            var lblLe = lblRect.gameObject.GetComponent<LayoutElement>();
            lblLe.flexibleWidth = 0;

            var slots = new TMP_Dropdown[count];
            for (int i = 0; i < count; i++)
            {
                // 드롭다운만 배치 — 번호 라벨 제거 (visual clutter)
                // 드롭다운의 flexibleWidth=1이 행 폭을 균등 분할하여 모든 슬롯이 동일 폭으로 정렬
                slots[i] = CreateDropdown(row, 150f, slotHeight);
            }
            return slots;
        }

        /// <summary>
        /// 템플릿 행 — 이름 입력 + 템플릿 목록 드롭다운 + 저장/불러오기/삭제 버튼.
        /// 각 카테고리(파티/유물/적)별로 독립적인 템플릿 목록 관리.
        /// </summary>
        private static void CreateTemplateRow(Transform parent, string label,
            out TMP_InputField nameInput, out TMP_Dropdown dropdown,
            out Button saveBtn, out Button loadBtn, out Button deleteBtn)
        {
            var row = NewRect($"{label}Row", parent);
            AddHeight(row, 34f);
            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.padding = new RectOffset(12, 12, 2, 2);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childForceExpandWidth = false;

            AddLabel(row, label, 14, TextDim, 110f);
            nameInput = CreateInputField(row, 160f, 28f, "이름...");
            dropdown = CreateDropdown(row, 160f, 28f);
            saveBtn = CreateButton(row, "저장", 60f, 28f, new Color(0.15f, 0.50f, 0.28f), 14);
            loadBtn = CreateButton(row, "불러오기", 80f, 28f, new Color(0.22f, 0.38f, 0.62f), 14);
            deleteBtn = CreateButton(row, "삭제", 60f, 28f, new Color(0.58f, 0.18f, 0.18f), 14);
        }

        /// <summary>
        /// TMP_InputField 생성 — 텍스트 입력 필드 (템플릿 이름용).
        /// TMP_DefaultControls.CreateInputField() 표준 구조: Root(Image) > Text Area(RectMask2D) > [Placeholder, Text]
        /// </summary>
        private static TMP_InputField CreateInputField(Transform parent, float width, float height, string placeholder)
        {
            var rect = NewRect("InputField", parent);
            var le = rect.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.minWidth = width;
            le.preferredHeight = height;
            le.minHeight = height;

            var bg = rect.gameObject.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);

            var input = rect.gameObject.AddComponent<TMP_InputField>();
            input.targetGraphic = bg;

            // Text Area — RectMask2D로 캐럿 클리핑 (TMP 표준)
            var textAreaRect = NewRect("Text Area", rect);
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(6, 2);
            textAreaRect.offsetMax = new Vector2(-6, -2);
            textAreaRect.gameObject.AddComponent<RectMask2D>();

            // Placeholder
            var placeholderRect = NewRect("Placeholder", textAreaRect);
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            var placeholderTmp = placeholderRect.gameObject.AddComponent<TextMeshProUGUI>();
            placeholderTmp.text = placeholder;
            placeholderTmp.fontSize = 14;
            placeholderTmp.fontStyle = FontStyles.Italic;
            placeholderTmp.color = new Color(0.5f, 0.5f, 0.55f, 0.5f);
            placeholderTmp.alignment = TextAlignmentOptions.Left;

            // Text
            var textRect = NewRect("Text", textAreaRect);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var textTmp = textRect.gameObject.AddComponent<TextMeshProUGUI>();
            textTmp.text = "";
            textTmp.fontSize = 14;
            textTmp.color = TextWhite;
            textTmp.alignment = TextAlignmentOptions.Left;

            input.textViewport = textAreaRect;
            input.textComponent = textTmp;
            input.placeholder = placeholderTmp;

            return input;
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
        //  UI 유틸리티 (BattleUISceneBuilder 헬퍼와 동일 패턴, 자체 복제)
        // ══════════════════════════════════════════════════════════

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static void AddHeight(RectTransform rect, float height)
        {
            var le = rect.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
        }

        private static RectTransform AddLabel(Transform parent, string text, int fontSize, Color color, float width)
        {
            var rect = NewRect("Label", parent);
            var le = rect.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.minWidth = width;

            var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Left;
            return rect;
        }

        private static TMP_Dropdown CreateDropdown(Transform parent, float width, float height)
        {
            var rect = NewRect("Dropdown", parent);
            var le = rect.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.minWidth = 80;          // 축소 허용 (행이 좁아질 때)
            le.flexibleWidth = 1;      // 남은 행 폭을 균등 분할 (모든 슬롯 동일 폭)
            le.preferredHeight = height;
            le.minHeight = height;

            var bg = rect.gameObject.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.22f, 0.95f);

            var dropdown = rect.gameObject.AddComponent<TMP_Dropdown>();
            dropdown.targetGraphic = bg;

            // Label
            var labelRect = NewRect("Label", rect);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10, 2);
            labelRect.offsetMax = new Vector2(-25, -2);
            var labelTmp = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            labelTmp.fontSize = 16;
            labelTmp.color = TextWhite;
            labelTmp.alignment = TextAlignmentOptions.Left;
            dropdown.captionText = labelTmp;

            // Arrow
            var arrowRect = NewRect("Arrow", rect);
            arrowRect.anchorMin = new Vector2(1, 0.5f);
            arrowRect.anchorMax = new Vector2(1, 0.5f);
            arrowRect.pivot = new Vector2(1, 0.5f);
            arrowRect.sizeDelta = new Vector2(20, 20);
            arrowRect.anchoredPosition = new Vector2(-4, 0);
            var arrowTmp = arrowRect.gameObject.AddComponent<TextMeshProUGUI>();
            arrowTmp.text = "▼";
            arrowTmp.fontSize = 12;
            arrowTmp.color = TextDim;
            arrowTmp.alignment = TextAlignmentOptions.Center;

            // Template — 드롭다운 펼침 영역
            // Template 높이 320px: 파티 9옵션(없음+8캐릭터)이 32px 간격으로 모두 표시 가능.
            // 유물(43옵션)은 스크롤로 탐색. TMP_Dropdown이 Template에 자체 Canvas(sortingOrder=30000) 추가.
            var templateRect = NewRect("Template", rect);
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = new Vector2(0, 2);
            templateRect.sizeDelta = new Vector2(0, 320);
            var templateBg = templateRect.gameObject.AddComponent<Image>();
            templateBg.color = new Color(0.1f, 0.1f, 0.16f, 0.98f);
            var scroll = templateRect.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;              // 세로 전용
            scroll.scrollSensitivity = 32f;         // 항목 1개씩 스크롤
            scroll.movementType = ScrollRect.MovementType.Clamped;
            // ★ Template에는 Mask를 두지 않음 — Mask는 Viewport에만 (표준 TMP 구조).
            //   Template에 Mask를 추가하면 showMaskGraphic=true여도 자식이 클리핑되어 스크롤이 끊길 수 있음.

            // Scrollbar — 세로 스크롤 표시 (유물 43옵션 등 긴 목록에서 스크롤 가능성 암시)
            var scrollbarRect = NewRect("Scrollbar", templateRect);
            scrollbarRect.anchorMin = new Vector2(1, 0);
            scrollbarRect.anchorMax = new Vector2(1, 1);
            scrollbarRect.pivot = new Vector2(1, 0.5f);
            scrollbarRect.sizeDelta = new Vector2(16, 0);
            scrollbarRect.anchoredPosition = Vector2.zero;
            var scrollbarImg = scrollbarRect.gameObject.AddComponent<Image>();
            scrollbarImg.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);
            var scrollbar = scrollbarRect.gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            var slidingAreaRect = NewRect("Sliding Area", scrollbarRect);
            slidingAreaRect.anchorMin = Vector2.zero;
            slidingAreaRect.anchorMax = Vector2.one;
            slidingAreaRect.offsetMin = new Vector2(4, 4);
            slidingAreaRect.offsetMax = new Vector2(-4, -4);
            var handleRect = NewRect("Handle", slidingAreaRect);
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;
            var handleImg = handleRect.gameObject.AddComponent<Image>();
            handleImg.color = new Color(0.4f, 0.4f, 0.5f, 0.8f);
            scrollbar.targetGraphic = handleImg;
            scrollbar.handleRect = handleRect;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scroll.verticalScrollbarSpacing = -3;  // TMP_DefaultControls 표준값

            // Viewport — Mask로 항목 클리핑 (TMP_DefaultControls 표준 구조: 자식 순서 Viewport 먼저)
            var viewportRect = NewRect("Viewport", templateRect);
            viewportRect.anchorMin = new Vector2(0, 0);
            viewportRect.anchorMax = new Vector2(1, 1);
            viewportRect.pivot = new Vector2(0, 1);
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = new Vector2(-16, 0);
            var viewportImg = viewportRect.gameObject.AddComponent<Image>();
            var viewportMask = viewportRect.gameObject.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            // ★ Mask.Reset() (Editor 전용)이 Image.color와 pivot을 리셋할 수 있으므로
            // Mask 추가 후에 다시 설정해야 함
            viewportImg.color = Color.white;
            viewportRect.pivot = new Vector2(0, 1);
            scroll.viewport = viewportRect;

            // Content — 자식으로 Item 배치
            // ★ VerticalLayoutGroup / ContentSizeFitter 제거 — TMP_Dropdown.Show()가
            // 항목 위치/크기를 수동 설정(TMP_Dropdown.cs L958-965)하므로 레이아웃 컴포넌트가
            // 간섭하면 항목이 0px로 collapse되어 빈 목록이 됨. TMP_DefaultControls 표준은
            // Content에 어떤 레이아웃 컴포넌트도 두지 않음.
            var itemsRect = NewRect("Content", viewportRect);
            itemsRect.anchorMin = new Vector2(0, 1);
            itemsRect.anchorMax = new Vector2(1, 1);
            itemsRect.pivot = new Vector2(0.5f, 1f);
            itemsRect.sizeDelta = new Vector2(0, 28);
            itemsRect.anchoredPosition = Vector2.zero;
            scroll.content = itemsRect;

            // Item — 표준 TMP Dropdown 구조와 동일하게 Item > [Item Background, Item Checkmark, Item Label]
            // 주의: Item 자체는 Toggle만 가지며, targetGraphic=Item Background (자식 Image).
            // TMP_Dropdown.SetupTemplate()이 Show() 시점에 자동으로 DropdownItem 컴포넌트를 Item에 추가하고
            // m_ItemText/m_ItemImage 필드를 dropdown.itemText/itemImage에서 가져옵니다.
            // (DropdownItem은 protected internal class라 외부에서 수동 AddComponent 불가)
            var itemRect = NewRect("Item", itemsRect);
            itemRect.anchorMin = new Vector2(0, 0.5f);
            itemRect.anchorMax = new Vector2(1, 0.5f);
            itemRect.sizeDelta = new Vector2(0, 28);
            var itemToggle = itemRect.gameObject.AddComponent<Toggle>();
            itemToggle.isOn = false;

            // Item Background — Toggle.targetGraphic (선택/하이라이트 배경)
            var itemBgRect = NewRect("Item Background", itemRect);
            itemBgRect.anchorMin = Vector2.zero;
            itemBgRect.anchorMax = Vector2.one;
            itemBgRect.offsetMin = Vector2.zero;
            itemBgRect.offsetMax = Vector2.zero;
            var itemBgImg = itemBgRect.gameObject.AddComponent<Image>();
            itemBgImg.color = new Color(0.18f, 0.18f, 0.26f, 0.95f);
            itemToggle.targetGraphic = itemBgImg;

            // Item Checkmark — Toggle.graphic (선택 시 표시)
            var checkRect = NewRect("Item Checkmark", itemRect);
            checkRect.anchorMin = new Vector2(0, 0.5f);
            checkRect.anchorMax = new Vector2(0, 0.5f);
            checkRect.pivot = new Vector2(0, 0.5f);
            checkRect.sizeDelta = new Vector2(20, 20);
            checkRect.anchoredPosition = new Vector2(8, 0);
            var checkImg = checkRect.gameObject.AddComponent<Image>();
            checkImg.color = AccentYellow;
            itemToggle.graphic = checkImg;

            // Item Label — 옵션 텍스트 (TMP)
            var itemLabelRect = NewRect("Item Label", itemRect);
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(32, 1);
            itemLabelRect.offsetMax = new Vector2(-10, -2);
            var itemLabelTmp = itemLabelRect.gameObject.AddComponent<TextMeshProUGUI>();
            itemLabelTmp.fontSize = 16;
            itemLabelTmp.color = TextWhite;
            itemLabelTmp.alignment = TextAlignmentOptions.Left;

            // ★ LayoutElement 제거 — TMP_Dropdown.Show()가 수동으로 itemRect.sizeDelta를 설정(TMP_Dropdown.cs L964).
            // LayoutElement가 있으면 VerticalLayoutGroup 없이도 ContentSizeFitter 등에 의해
            // preferredHeight가 잘못 해석될 수 있음. TMP_DefaultControls 표준은 Item에 LayoutElement 없음.

            // ★ TMP_Dropdown 핵심 참조 — itemText가 없으면 AddItem에서 텍스트가 설정되지 않음
            dropdown.template = templateRect;
            dropdown.itemText = itemLabelTmp;
            dropdown.targetGraphic = bg;
            templateRect.gameObject.SetActive(false);

            // 기본 옵션 1개 (씬 Start에서 PopulateDropdowns가 다시 채움)
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string> { "(로딩 중)" });
            dropdown.value = 0;
            dropdown.RefreshShownValue();

            return dropdown;
        }

        private static Toggle CreateToggle(Transform parent, string label, float width, float height)
        {
            var rect = NewRect("Toggle", parent);
            var le = rect.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.minWidth = width;
            le.preferredHeight = height;
            le.minHeight = height;

            var toggle = rect.gameObject.AddComponent<Toggle>();

            // Background
            var bgRect = NewRect("Background", rect);
            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(0, 0.5f);
            bgRect.pivot = new Vector2(0, 0.5f);
            bgRect.sizeDelta = new Vector2(height - 4, height - 4);
            bgRect.anchoredPosition = new Vector2(4, 0);
            var bg = bgRect.gameObject.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.22f, 0.95f);
            toggle.targetGraphic = bg;

            // Checkmark
            var checkRect = NewRect("Checkmark", bgRect);
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.sizeDelta = Vector2.zero;
            checkRect.offsetMin = new Vector2(4, 4);
            checkRect.offsetMax = new Vector2(-4, -4);
            var check = checkRect.gameObject.AddComponent<Image>();
            check.color = AccentGreen;
            toggle.graphic = check;
            toggle.isOn = false;

            // Label
            var labelRect = NewRect("Label", rect);
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.offsetMin = new Vector2(height + 8, 0);
            labelRect.offsetMax = Vector2.zero;
            var tmp = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 16;
            tmp.color = TextWhite;
            tmp.alignment = TextAlignmentOptions.Left;

            return toggle;
        }

        private static Button CreateButton(Transform parent, string label, float width, float height, Color color, int fontSize = 22)
        {
            var rect = NewRect("Button", parent);
            var le = rect.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.minWidth = width;
            le.preferredHeight = height;
            le.minHeight = height;

            var bg = rect.gameObject.AddComponent<Image>();
            bg.color = color;

            var btn = rect.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Hover 색상 변화
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = colors;

            var labelRect = NewRect("Label", rect);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
            var tmp = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            return btn;
        }

        // ══════════════════════════════════════════════════════════
        //  Reflection 유틸리티
        // ══════════════════════════════════════════════════════════

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}
#endif
