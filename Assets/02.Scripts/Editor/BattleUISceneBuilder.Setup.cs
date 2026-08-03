using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI.Battle;
using TeamLog.Combat;
using TeamLog.Characters;

namespace TeamLog.Editor
{
    /// <summary>
    /// Battle UI 씬 빌더 — 스크립트 설정 및 참조 자동 연결
    /// </summary>
    public partial class BattleUISceneBuilder
    {
        /// <summary>
        /// Phase CC-2E: 현재 씬에 DiscoverModal만 추가 — 기존 씬 디자인 100% 보존.
        /// BattleScene.unity 또는 BattleTestScene.unity 열어두고 실행.
        /// 이미 DiscoverModal이 있으면 스킵 (idempotent).
        /// </summary>
        [MenuItem("Tools/Battle UI/Add Discover Modal to Current Scene", false, 102)]
        public static void AddDiscoverModalToCurrentScene()
        {
            // 부모 후보 탐색 — BattleUIRoot 우선, 없으면 BattleUICanvas
            var rootGO = GameObject.Find("BattleUIRoot");
            if (rootGO == null)
                rootGO = GameObject.Find("BattleUICanvas");
            if (rootGO == null)
            {
                Debug.LogError("[DiscoverModal] BattleUIRoot 또는 BattleUICanvas를 찾을 수 없음 — BattleScene/BattleTestScene을 먼저 여세요.");
                return;
            }

            // 이미 DiscoverModal이 있으면 스킵 (idempotent)
            var existing = GameObject.Find("DiscoverModal");
            if (existing != null)
            {
                Debug.Log("[DiscoverModal] 이미 존재함 — 스킵. 재생성 원할 경우 먼저 수동 삭제하세요.");
                WireDiscoverModalToUIManager();
                return;
            }

            var rootRect = rootGO.GetComponent<RectTransform>();
            if (rootRect == null)
            {
                Debug.LogError("[DiscoverModal] 부모 GameObject에 RectTransform 없음");
                return;
            }

            // CreateDiscoverModal 호출 (Overlay.cs에 정의 — 같은 partial class)
            CreateDiscoverModal(rootRect);

            // BattleUIManager의 _discoverModal 필드 와이어링
            WireDiscoverModalToUIManager();

            EditorSceneManager.MarkSceneDirty(rootGO.scene);
            // 자동 저장 — 사용자가 저장 단계 놓치는 것 방지
            EditorSceneManager.SaveScene(rootGO.scene);
            Debug.Log($"[DiscoverModal] 현재 씬({rootGO.scene.name})에 DiscoverModal 추가 + 자동 저장 완료.");
        }

        /// <summary>현재 씬의 BattleUIManager를 찾아 _discoverModal 필드 와이어링.</summary>
        private static void WireDiscoverModalToUIManager()
        {
            // rootGO(BattleUIRoot 또는 BattleUICanvas)에서 BattleUIManager 탐색 — FindObjectOfType 금지 가드레일 준수
            var rootGO = GameObject.Find("BattleUIRoot");
            if (rootGO == null) rootGO = GameObject.Find("BattleUICanvas");
            if (rootGO == null)
            {
                Debug.LogWarning("[DiscoverModal] BattleUIRoot/BattleUICanvas를 찾을 수 없음 — 수동으로 _discoverModal 필드 연결 필요");
                return;
            }

            var uiManager = rootGO.GetComponent<BattleUIManager>()
                ?? rootGO.GetComponentInChildren<BattleUIManager>(true);
            if (uiManager == null)
            {
                Debug.LogWarning("[DiscoverModal] BattleUIManager 컴포넌트를 찾을 수 없음 — 수동으로 _discoverModal 필드 연결 필요");
                return;
            }

            var modalGO = GameObject.Find("DiscoverModal");
            if (modalGO == null)
            {
                Debug.LogWarning("[DiscoverModal] DiscoverModal GameObject를 찾을 수 없음");
                return;
            }

            var modalUI = modalGO.GetComponent<DiscoverModalUI>();
            if (modalUI == null)
            {
                Debug.LogError("[DiscoverModal] DiscoverModalUI 컴포넌트 없음 — 프리팹 생성 실패?");
                return;
            }

            var ser = new SerializedObject(uiManager);
            var prop = ser.FindProperty("_discoverModal");
            if (prop != null)
            {
                prop.objectReferenceValue = modalUI;
                ser.ApplyModifiedProperties();
                EditorUtility.SetDirty(uiManager);
                Debug.Log($"[DiscoverModal] BattleUIManager._discoverModal 와이어링 완료 → {modalGO.name}");
            }
            else
            {
                Debug.LogError("[DiscoverModal] BattleUIManager._discoverModal 필드를 찾을 수 없음 — 스키마 변경 확인 필요");
            }
        }

        [MenuItem("Tools/Battle UI/Setup Scripts in Current Scene", false, 101)]
        public static void SetupScriptsInCurrentScene()
        {
            var root = GameObject.Find("BattleUIRoot");
            if (root == null)
            {
                Debug.LogError("[Setup] BattleUIRoot not found!");
                return;
            }

            // 1) BattleUIManager 추가
            var uiManager = root.GetComponent<BattleUIManager>();
            if (uiManager == null)
                uiManager = root.AddComponent<BattleUIManager>();

            // 2) TopBar에 TopBarUI 추가
            var topBar = root.transform.Find("TopBar");
            if (topBar != null)
            {
                var topBarUI = topBar.GetComponent<TopBarUI>();
                if (topBarUI == null)
                    topBarUI = topBar.gameObject.AddComponent<TopBarUI>();

                // PartyStatusWidget 자동 연결
                var partyWidget = topBar.GetComponent<PartyStatusWidget>();
            }

            // 3) BottomBar에 ActionBarUI 추가 + TopBarUI 요소 와이어링
            var bottomBar = root.transform.Find("BottomBar");
            if (bottomBar != null)
            {
                SetupActionBar(bottomBar);

                // TopBarUI가 AP/속도 요소를 참조하도록 와이어링
                // AP는 BottomBar/PlayerStrip/APArea에, 속도는 TopBar에 위치
                var topBarUI = topBar?.GetComponent<TopBarUI>();
                if (topBarUI != null)
                {
                    var topBarSer = new SerializedObject(topBarUI);

                    var apText = bottomBar.Find("RightColumn/APArea/APText");
                    if (apText != null)
                    {
                        var apTextProp = topBarSer.FindProperty("_apText");
                        if (apTextProp != null) apTextProp.objectReferenceValue = apText.GetComponent<TMPro.TextMeshProUGUI>();
                    }

                    // 속도 버튼은 TopBar에 위치
                    var speedBtn = topBar?.Find("SpeedButton");
                    if (speedBtn != null)
                    {
                        var speedBtnProp = topBarSer.FindProperty("_speedToggleButton");
                        if (speedBtnProp != null) speedBtnProp.objectReferenceValue = speedBtn.GetComponent<Button>();
                    }

                    var speedLabel = topBar?.Find("SpeedButton/SpeedLabel");
                    if (speedLabel != null)
                    {
                        var speedLabelProp = topBarSer.FindProperty("_speedLabel");
                        if (speedLabelProp != null) speedLabelProp.objectReferenceValue = speedLabel.GetComponent<TMPro.TextMeshProUGUI>();
                    }

                    // Turn 배지 / 층 정보 텍스트 와이어링
                    var turnBadge = topBar?.Find("TurnBadge/T");
                    if (turnBadge != null)
                    {
                        var turnProp = topBarSer.FindProperty("_turnText");
                        if (turnProp != null) turnProp.objectReferenceValue = turnBadge.GetComponent<TMPro.TextMeshProUGUI>();
                    }

                    var floorInfo = topBar?.Find("FloorInfo");
                    if (floorInfo != null)
                    {
                        var floorProp = topBarSer.FindProperty("_floorInfoText");
                        if (floorProp != null) floorProp.objectReferenceValue = floorInfo.GetComponent<TMPro.TextMeshProUGUI>();
                    }

                    topBarSer.ApplyModifiedProperties();
                }
            }

            // 4) PlayerStrip 카드에 PlayerSidebarPanel 추가 — BottomBar/LeftContent/PlayerStrip 내부
            var playerStrip = root.transform.Find("BottomBar/LeftContent/PlayerStrip");
            if (playerStrip != null)
            {
                foreach (Transform child in playerStrip)
                {
                    if (child.GetComponent<PlayerSidebarPanel>() == null)
                        child.gameObject.AddComponent<PlayerSidebarPanel>();
                }
            }

            // 4b) 토글 버튼 ↔ 오버레이 패널 연결
            WireToggleButtons(root.transform);

            // 5) CenterArea 패널에 EnemyDetailPanel 추가
            var centerArea = root.transform.Find("ContentArea/CenterArea");
            if (centerArea != null)
            {
                foreach (Transform child in centerArea)
                {
                    if (child.GetComponent<EnemyDetailPanel>() == null)
                        child.gameObject.AddComponent<EnemyDetailPanel>();
                }
            }

            // 6) BattleLogUI — 삭제됨 (전투로그 UI 제거)

            // 7) BattleSceneSetup 추가
            var setupGO = GameObject.Find("BattleSceneSetup");
            if (setupGO == null)
            {
                setupGO = new GameObject("BattleSceneSetup");
                setupGO.transform.SetParent(root.transform.parent);
            }
            var sceneSetup = setupGO.GetComponent<BattleSceneSetup>();
            if (sceneSetup == null)
                sceneSetup = setupGO.AddComponent<BattleSceneSetup>();

            // CharacterData 에셋 할당 (Phase CC-2A GC: Warrior→Duran, Mage→Ashe 대체)
            var warriorData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Char_Duran.asset");
            var mageData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Char_Ashe.asset");
            var healerData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Char_Healer.asset");
            var rogueData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Char_Umbra.asset");

            SetPrivateField(sceneSetup, "_testWarriorData", warriorData);
            SetPrivateField(sceneSetup, "_testMageData", mageData);
            SetPrivateField(sceneSetup, "_testHealerData", healerData);
            SetPrivateField(sceneSetup, "_testRogueData", rogueData);
            SetPrivateField(sceneSetup, "_battleUIManager", uiManager);

            // _mainCanvasRect 연결
            var canvasGO = GameObject.Find("BattleUICanvas");
            if (canvasGO != null)
                SetPrivateField(sceneSetup, "_mainCanvasRect", canvasGO.GetComponent<RectTransform>());

            // _titleManager 연결 — BattleTitleManager를 BattleUIRoot에 추가
            if (root != null)
            {
                var titleMgr = root.GetComponent<BattleTitleManager>();
                if (titleMgr == null)
                    titleMgr = root.gameObject.AddComponent<BattleTitleManager>();
                SetPrivateField(sceneSetup, "_titleManager", titleMgr);
            }

            // BattleEndOverlay 연결
            var endOverlayGO = root.transform.Find("BattleEndOverlay");
            if (endOverlayGO != null)
            {
                var endOverlay = endOverlayGO.GetComponent<BattleEndOverlay>();
                if (endOverlay != null)
                    SetPrivateField(sceneSetup, "_battleEndOverlay", endOverlay);
            }

            var actionBar = bottomBar?.GetComponent<ActionBarUI>();
            if (actionBar != null)
                SetPrivateField(sceneSetup, "_actionBar", actionBar);

            // BattleRelicBarUI 연결 — TopBar 좌측에 위치
            var relicBar = topBar?.Find("RelicBar");
            if (relicBar != null)
                SetPrivateField(sceneSetup, "_relicBarUI", relicBar.GetComponent<BattleRelicBarUI>());

            // BattleLogUI 연결 — 토글 오버레이 (BattleLogOverlay)
            var logOverlay = root.transform.Find("BattleLogOverlay");
            if (logOverlay != null)
            {
                var logUI = logOverlay.GetComponent<BattleLogUI>();
                SetPrivateField(uiManager, "_battleLog", logUI);
            }

            var slimeData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Enemy_Slime.asset");
            var goblinData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Enemy_Goblin.asset");
            SetPrivateField(sceneSetup, "_testEnemyData", new CharacterData[] { goblinData, goblinData });

            EditorUtility.SetDirty(sceneSetup);

            // 8) UI 참조 자동 연결
            AutoWireBattleUIManager(uiManager);
            if (actionBar != null)
                AutoWireActionBar(actionBar);

            // 9) 정적 패널 제거
            RemoveStaticPanels(playerStrip, centerArea);

            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Setup] 스크립트 세팅 완료! 씬을 저장하세요.");
        }

        // ══════════════════════════════════════════════════════════
        //  ActionBar 설정
        // ══════════════════════════════════════════════════════════

        private static void SetupActionBar(Transform bottomBar)
        {
            var actionBar = bottomBar.GetComponent<ActionBarUI>();
            if (actionBar == null)
                actionBar = bottomBar.gameObject.AddComponent<ActionBarUI>();

            // ActionSlotContainer는 CreateBottomBar에서 SkillRow 내부에 이미 생성됨
            CreateActionSlotPrefab();
        }

        private static void CreateActionSlotPrefab()
        {
            const string prefabPath = "Assets/03.Data/Prefabs/ActionSlotUI.prefab";
            AssetDatabase.DeleteAsset(prefabPath);

            if (!AssetDatabase.IsValidFolder("Assets/03.Data/Prefabs"))
                AssetDatabase.CreateFolder("Assets/03.Data", "Prefabs");

            var go = new GameObject("ActionSlotUI");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240, 110);

            var layoutEl = go.AddComponent<LayoutElement>();
            layoutEl.minWidth = 120;
            layoutEl.flexibleWidth = 1;
            layoutEl.preferredHeight = 110;
            layoutEl.minHeight = 100;

            // 배경 — 남색 + 라운드 코너
            var bg = go.AddComponent<Image>();
            var bgSprite = LoadSprite(SPRITE_SOLID_FRAME);
            if (bgSprite != null) { bg.sprite = bgSprite; bg.type = Image.Type.Sliced; }
            bg.color = SlotBgNavy;
            var bgOutline = go.AddComponent<Outline>();
            bgOutline.effectColor = new Color(0.27f, 0.27f, 0.27f, 0.80f);
            bgOutline.effectDistance = new Vector2(1, -1);
            go.AddComponent<Button>();

            // ★ VLG 없음 — 명시적 앵커 배치 (레이아웃 충돌 원천 차단)

            // ── Header: CasterName (좌) + TypeTag (우) — top=2, h=14 ──
            var header = NewRect("Header", rect);
            AnchorTopFill(header, 2, 14);
            // CasterName: 좌측 절반
            var casterT = NewRect("CasterNameText", header);
            casterT.anchorMin = new Vector2(0, 0); casterT.anchorMax = new Vector2(0.5f, 1);
            casterT.offsetMin = new Vector2(4, 0); casterT.offsetMax = new Vector2(-2, 0);
            var casterTmp = casterT.gameObject.AddComponent<TextMeshProUGUI>();
            casterTmp.font = GetOrCreateKoreanFont(); casterTmp.text = "";
            casterTmp.fontSize = 10; casterTmp.alignment = TextAlignmentOptions.Left;
            casterTmp.color = TextDim; casterTmp.raycastTarget = false;
            casterTmp.enableWordWrapping = false; casterTmp.overflowMode = TextOverflowModes.Ellipsis;
            // TypeTag: 우측 고정 36x12
            var typeTag = NewRect("TypeTag", header);
            typeTag.anchorMin = new Vector2(1, 0.5f); typeTag.anchorMax = new Vector2(1, 0.5f);
            typeTag.pivot = new Vector2(1, 0.5f); typeTag.anchoredPosition = new Vector2(-4, 0);
            typeTag.sizeDelta = new Vector2(36, 12);
            var typeTagImg = typeTag.gameObject.AddComponent<Image>();
            typeTagImg.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
            var typeTagText = NewRect("T", typeTag);
            SetFillParent(typeTagText);
            var typeTagTmp = typeTagText.gameObject.AddComponent<TextMeshProUGUI>();
            typeTagTmp.font = GetOrCreateKoreanFont(); typeTagTmp.text = "";
            typeTagTmp.fontSize = 8; typeTagTmp.fontStyle = FontStyles.Bold;
            typeTagTmp.alignment = TextAlignmentOptions.Center; typeTagTmp.color = TextWhite;
            typeTagTmp.raycastTarget = false;

            // ── SkillIcon — top=18, 38x38 중앙 ──
            var icon = NewRect("SkillIcon", rect);
            AnchorTopCentered(icon, 18, 38, 38);
            var iconImg = icon.gameObject.AddComponent<Image>();
            iconImg.color = AccentRed; iconImg.preserveAspect = true;
            var defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_ATTACK);
            if (defaultIcon != null) iconImg.sprite = defaultIcon;

            // ── SkillNameText — top=60, h=16 ──
            var nameT = NewRect("SkillNameText", rect);
            AnchorTopFill(nameT, 60, 16);
            var nameTmp = nameT.gameObject.AddComponent<TextMeshProUGUI>();
            nameTmp.font = GetOrCreateKoreanFont(); nameTmp.text = "---";
            nameTmp.fontSize = 13; nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.alignment = TextAlignmentOptions.Center; nameTmp.color = TextWhite;
            nameTmp.raycastTarget = false; nameTmp.enableWordWrapping = false;
            nameTmp.overflowMode = TextOverflowModes.Ellipsis;

            // ── EffectText — top=78, h=22 (★ 2026-08-03: 글자 크기 8→11, 높이 18→22 가독성 개선)
            var effectT = NewRect("EffectText", rect);
            AnchorTopFill(effectT, 78, 22);
            var effectTmp = effectT.gameObject.AddComponent<TextMeshProUGUI>();
            effectTmp.font = GetOrCreateKoreanFont(); effectTmp.text = "";
            effectTmp.fontSize = 11; effectTmp.alignment = TextAlignmentOptions.Center;
            effectTmp.color = TextDim; effectTmp.raycastTarget = false;
            effectTmp.enableWordWrapping = true; effectTmp.overflowMode = TextOverflowModes.Ellipsis;

            // ── CostBadge — bottom=2, 26x18 중앙 ──
            var costBadge = NewRect("CostBadge", rect);
            AnchorBottomCentered(costBadge, 2, 26, 18);
            costBadge.gameObject.AddComponent<Image>().color = new Color(0.9f, 0.78f, 0.31f, 0.95f); // ★ D 시안: 골드
            var costT = NewRect("CostText", costBadge);
            SetFillParent(costT);
            var costTmp = AddText(costT, "0", 12, FontStyles.Bold, TextAlignmentOptions.Center, Color.black);

            // ── 오버레이 (VLG가 없으므로 ignoreLayout 불필요 — 앵커만으로 배치) ──

            // 선택 테두리
            var selBorder = NewRect("SelectionBorder", rect);
            SetFillParent(selBorder);
            selBorder.gameObject.AddComponent<Image>().color = Color.clear;
            var selOutline = selBorder.gameObject.AddComponent<Outline>();
            selOutline.effectColor = AccentYellow; selOutline.effectDistance = new Vector2(3, -3);
            selBorder.gameObject.SetActive(false);

            // 실행 순서 뱃지
            var orderBadge = NewRect("ExecutionOrderBadge", rect);
            orderBadge.anchorMin = new Vector2(1, 1); orderBadge.anchorMax = new Vector2(1, 1);
            orderBadge.pivot = new Vector2(1, 1); orderBadge.anchoredPosition = new Vector2(-2, -2);
            orderBadge.sizeDelta = new Vector2(28, 28);
            orderBadge.gameObject.AddComponent<Image>().color = AccentYellow;
            var orderText = NewRect("OrderText", orderBadge);
            SetFillParent(orderText);
            AddText(orderText, "1", 15, FontStyles.Bold, TextAlignmentOptions.Center, Color.black);
            orderBadge.gameObject.SetActive(false);

            // 할당 오버레이
            var assigned = NewRect("AssignedOverlay", rect);
            SetFillParent(assigned);
            assigned.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.4f, 0.15f);
            assigned.gameObject.SetActive(false);

            // AP 부족 테두리
            var apShortageBorder = NewRect("APShortageBorder", rect);
            SetFillParent(apShortageBorder);
            var apBorderImg = apShortageBorder.gameObject.AddComponent<Image>();
            apBorderImg.color = Color.clear;
            var apOutline = apShortageBorder.gameObject.AddComponent<Outline>();
            apOutline.effectColor = new Color(0.85f, 0.15f, 0.15f, 0.9f);
            apOutline.effectDistance = new Vector2(2, -2);
            apShortageBorder.gameObject.SetActive(false);

            // 리롤 버튼
            var rerollBtn = NewRect("RerollBtn", rect);
            rerollBtn.anchorMin = new Vector2(1, 0); rerollBtn.anchorMax = new Vector2(1, 0);
            rerollBtn.pivot = new Vector2(1, 0); rerollBtn.anchoredPosition = new Vector2(-2, 2);
            rerollBtn.sizeDelta = new Vector2(22, 22);
            var rerollBtnComp = rerollBtn.gameObject.AddComponent<Button>();
            var rerollImg = rerollBtn.gameObject.AddComponent<Image>();
            rerollImg.color = ShieldBrown; rerollBtnComp.targetGraphic = rerollImg;
            var rerollTxt = NewRect("T", rerollBtn);
            SetFillParent(rerollTxt);
            AddText(rerollTxt, "R", 12, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
            rerollBtn.gameObject.SetActive(false);

            // ActionSlotUI 컴포넌트 자동 와이어링
            var slotUI = go.AddComponent<ActionSlotUI>();
            SetPrivateField(slotUI, "_skillIcon", iconImg);
            SetPrivateField(slotUI, "_skillNameText", nameTmp);
            SetPrivateField(slotUI, "_costText", costTmp);
            SetPrivateField(slotUI, "_casterNameText", casterTmp);
            SetPrivateField(slotUI, "_effectText", effectTmp);
            SetPrivateField(slotUI, "_selectionBorder", selBorder.gameObject);
            SetPrivateField(slotUI, "_executionOrderBadge", orderBadge.gameObject);
            SetPrivateField(slotUI, "_executionOrderText", orderText.gameObject.GetComponent<TextMeshProUGUI>());
            SetPrivateField(slotUI, "_assignedOverlay", assigned.gameObject);
            SetPrivateField(slotUI, "_button", go.GetComponent<Button>());
            SetPrivateField(slotUI, "_rerollButton", rerollBtnComp);
            SetPrivateField(slotUI, "_apShortageBorder", apShortageBorder.gameObject);
            SetPrivateField(slotUI, "_typeTagImage", typeTagImg);
            SetPrivateField(slotUI, "_typeTagText", typeTagTmp);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            Debug.Log("[Setup] ActionSlotUI prefab created at " + prefabPath);
        }

        // ══════════════════════════════════════════════════════════
        //  Auto-Wire
        // ══════════════════════════════════════════════════════════

        private static void AutoWireBattleUIManager(BattleUIManager uiManager)
        {
            var root = uiManager.transform;

            // TopBar
            var topBar = root.Find("TopBar");
            if (topBar != null)
                SetPrivateField(uiManager, "_topBar", topBar.GetComponent<TopBarUI>() ?? topBar.gameObject.AddComponent<TopBarUI>());

            // Player panel container — BottomBar/LeftContent/PlayerStrip
            var playerStrip = root.Find("BottomBar/LeftContent/PlayerStrip");
            if (playerStrip != null)
                SetPrivateField(uiManager, "_playerPanelContainer", playerStrip);

            // Player panel prefab — 첫 번째 자식 사용
            Transform firstPanel = null;
            if (playerStrip != null && playerStrip.childCount > 0)
            {
                firstPanel = playerStrip.GetChild(0);
            }
            if (firstPanel != null)
            {
                const string prefabPath = "Assets/03.Data/Prefabs/PlayerSidebarPanel.prefab";
                if (!AssetDatabase.IsValidFolder("Assets/03.Data/Prefabs"))
                    AssetDatabase.CreateFolder("Assets/03.Data", "Prefabs");
                AssetDatabase.DeleteAsset(prefabPath);
                var prefab = PrefabUtility.SaveAsPrefabAsset(firstPanel.gameObject, prefabPath);
                SetPrivateField(uiManager, "_playerPanelPrefab", prefab != null ? prefab.GetComponent<PlayerSidebarPanel>() : null);
            }

            // Enemy panel container
            var centerArea = root.Find("ContentArea/CenterArea");
            if (centerArea != null)
                SetPrivateField(uiManager, "_enemyPanelContainer", centerArea);

            // Enemy panel prefab
            Transform firstEnemy = centerArea != null && centerArea.childCount > 0 ? centerArea.GetChild(0) : null;
            if (firstEnemy != null)
            {
                const string prefabPath = "Assets/03.Data/Prefabs/EnemyDetailPanel.prefab";
                if (!AssetDatabase.IsValidFolder("Assets/03.Data/Prefabs"))
                    AssetDatabase.CreateFolder("Assets/03.Data", "Prefabs");
                AssetDatabase.DeleteAsset(prefabPath);
                var prefab = PrefabUtility.SaveAsPrefabAsset(firstEnemy.gameObject, prefabPath);
                SetPrivateField(uiManager, "_enemyPanelPrefab", prefab != null ? prefab.GetComponent<EnemyDetailPanel>() : null);
            }

            // BattleLog — 삭제됨 (전투로그 UI 제거)

            // ActionBarUI
            var bottomBar = root.Find("BottomBar");
            if (bottomBar != null)
            {
                var actionBar = bottomBar.GetComponent<ActionBarUI>();
                if (actionBar != null)
                    SetPrivateField(uiManager, "_actionBar", actionBar);
            }

            // CharacterPopup
            var popup = root.Find("CharacterPopup");
            if (popup != null)
                SetPrivateField(uiManager, "_characterPopup", popup.GetComponent<CharacterPopupUI>());

            // DiscoverModal (Phase CC-2E — Cael Alchemist)
            var discoverModal = root.Find("DiscoverModal");
            if (discoverModal != null)
            {
                var modalUI = discoverModal.GetComponent<DiscoverModalUI>();
                if (modalUI != null)
                {
                    var uiSer = new SerializedObject(uiManager);
                    var modalProp = uiSer.FindProperty("_discoverModal");
                    if (modalProp != null) modalProp.objectReferenceValue = modalUI;
                    uiSer.ApplyModifiedProperties();
                }
            }

            EditorUtility.SetDirty(uiManager);
        }

        private static void AutoWireActionBar(ActionBarUI actionBar)
        {
            var bottomBar = actionBar.transform;

            // ActionSlotContainer는 BottomBar/LeftContent/SkillRow 내부에 위치
            var slotContainer = bottomBar.Find("LeftContent/SkillRow/ActionSlotContainer");
            SetPrivateField(actionBar, "_actionMenuContainer", slotContainer);

            var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03.Data/Prefabs/ActionSlotUI.prefab");
            if (slotPrefab != null)
                SetPrivateField(actionBar, "_actionSlotPrefab", slotPrefab.GetComponent<ActionSlotUI>());

            // EndTurnButton은 BottomBar/RightColumn/ButtonArea 내부에 위치
            var endTurnBtn = actionBar.transform.root.Find("BattleUIRoot/BottomBar/RightColumn/ButtonArea/EndTurnButton");
            SetPrivateField(actionBar, "_endTurnButton", endTurnBtn?.GetComponent<Button>());

            // 리롤 텍스트는 RerollButton 내부 T 요소 (별도 RerollText 제거, 중복 해소)
            var rerollText = bottomBar.Find("RightColumn/ButtonArea/RerollButton/T");
            SetPrivateField(actionBar, "_rerollText", rerollText?.GetComponent<TMPro.TextMeshProUGUI>());

            EditorUtility.SetDirty(actionBar);
        }

        // ══════════════════════════════════════════════════════════
        //  유틸리티
        // ══════════════════════════════════════════════════════════

        private static void RemoveStaticPanels(Transform playerStrip, Transform centerArea)
        {
            if (playerStrip != null)
            {
                // PlayerStrip 내부의 정적 카드 전부 제거 (APArea는 RightColumn에 있으므로 제외 불필요)
                for (int i = playerStrip.childCount - 1; i >= 0; i--)
                {
                    var child = playerStrip.GetChild(i);
                    Object.DestroyImmediate(child.gameObject);
                }
                Debug.Log("[Setup] PlayerStrip static panels removed");
            }

            if (centerArea != null)
            {
                for (int i = centerArea.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(centerArea.GetChild(i).gameObject);
                Debug.Log("[Setup] CenterArea static panels removed");
            }
        }

        /// <summary>
        /// TopBar 토글 버튼(Party/Log)과 오버레이 패널을 연결.
        /// </summary>
        private static void WireToggleButtons(Transform root)
        {
            // 파티 토글 ↔ PartyStatusOverlay
            var partyToggle = root.Find("TopBar/PartyToggleButton");
            var partyOverlay = root.Find("PartyStatusOverlay");
            if (partyToggle != null && partyOverlay != null)
            {
                var toggle = partyToggle.GetComponent<UIToggleButton>();
                if (toggle != null) toggle.SetTarget(partyOverlay.gameObject);
            }

            // 로그 토글 ↔ BattleLogOverlay
            var logToggle = root.Find("TopBar/LogToggleButton");
            var logOverlay = root.Find("BattleLogOverlay");
            if (logToggle != null && logOverlay != null)
            {
                var toggle = logToggle.GetComponent<UIToggleButton>();
                if (toggle != null) toggle.SetTarget(logOverlay.gameObject);
            }
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}
