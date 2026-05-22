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

                var turnCounter = topBar.Find("TurnCounter");
                if (turnCounter != null)
                {
                    var tmp = turnCounter.GetComponent<TMPro.TextMeshProUGUI>();
                    if (tmp != null)
                        tmp.text = "1/4";
                }

                // AP 텍스트 자동 연결
                var apText = topBar.Find("APText");
                if (apText != null)
                    SetPrivateField(topBarUI, "_apText", apText.GetComponent<TMPro.TextMeshProUGUI>());

                // 리롤 카운트 텍스트 자동 연결
                var rerollText = topBar.Find("RerollText");
                if (rerollText != null)
                    SetPrivateField(topBarUI, "_rerollText", rerollText.GetComponent<TMPro.TextMeshProUGUI>());
            }

            // 3) BottomBar에 ActionBarUI 추가
            var bottomBar = root.transform.Find("BottomBar");
            if (bottomBar != null)
            {
                SetupActionBar(bottomBar);
            }

            // 4) LeftSidebar 패널에 PlayerSidebarPanel 추가
            var leftSidebar = root.transform.Find("ContentArea/LeftSidebar");
            if (leftSidebar != null)
            {
                foreach (Transform child in leftSidebar)
                {
                    if (child.GetComponent<PlayerSidebarPanel>() == null)
                        child.gameObject.AddComponent<PlayerSidebarPanel>();
                }
            }

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

            // 6) BattleLogUI 추가
            var rightSidebar = root.transform.Find("ContentArea/RightSidebar");
            if (rightSidebar != null)
            {
                if (rightSidebar.GetComponent<BattleLogUI>() == null)
                    rightSidebar.gameObject.AddComponent<BattleLogUI>();
            }

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

            // CharacterData 에셋 할당
            var warriorData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Char_Warrior.asset");
            var mageData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Char_Mage.asset");
            var healerData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Char_Healer.asset");
            var rogueData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Char_Rogue.asset");

            SetPrivateField(sceneSetup, "_testWarriorData", warriorData);
            SetPrivateField(sceneSetup, "_testMageData", mageData);
            SetPrivateField(sceneSetup, "_testHealerData", healerData);
            SetPrivateField(sceneSetup, "_testRogueData", rogueData);
            SetPrivateField(sceneSetup, "_battleUIManager", uiManager);

            var actionBar = bottomBar?.GetComponent<ActionBarUI>();
            if (actionBar != null)
                SetPrivateField(sceneSetup, "_actionBar", actionBar);

            var slimeData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Enemy_Slime.asset");
            var goblinData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Enemy_Goblin.asset");
            SetPrivateField(sceneSetup, "_testEnemyData", new CharacterData[] { goblinData, goblinData });

            EditorUtility.SetDirty(sceneSetup);

            // 8) UI 참조 자동 연결
            AutoWireBattleUIManager(uiManager);
            if (actionBar != null)
                AutoWireActionBar(actionBar);

            // 9) 정적 패널 제거
            RemoveStaticPanels(leftSidebar, centerArea);

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

            // 액션 슬롯 컨테이너
            var slotContainer = bottomBar.Find("ActionSlotContainer");
            if (slotContainer == null)
            {
                var container = NewRect("ActionSlotContainer", bottomBar as RectTransform);
                container.anchorMin = new Vector2(0.15f, 0);
                container.anchorMax = new Vector2(0.85f, 1);
                container.offsetMin = Vector2.zero;
                container.offsetMax = Vector2.zero;

                var hlg = container.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 8;
                hlg.padding = new RectOffset(12, 12, 10, 10);
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = true;
            }

            // 액션 디테일 패널
            var detailPanel = bottomBar.Find("ActionDetailPanel");
            if (detailPanel == null)
            {
                var detail = NewRect("ActionDetailPanel", bottomBar as RectTransform);
                detail.anchorMin = new Vector2(0, 1);
                detail.anchorMax = new Vector2(1, 1);
                detail.pivot = new Vector2(0.5f, 0);
                detail.anchoredPosition = new Vector2(0, 0);
                detail.sizeDelta = new Vector2(0, 80);
                detail.gameObject.SetActive(false);
                detail.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.1f, 0.95f);

                var titleT = NewRect("TitleText", detail);
                titleT.anchorMin = new Vector2(0, 0.5f);
                titleT.anchorMax = new Vector2(0.5f, 1);
                titleT.offsetMin = new Vector2(12, 4);
                titleT.offsetMax = new Vector2(-4, -4);
                AddText(titleT, "스킬명", 16, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);

                var descT = NewRect("DescText", detail);
                descT.anchorMin = new Vector2(0.5f, 0.5f);
                descT.anchorMax = new Vector2(1, 1);
                descT.offsetMin = new Vector2(4, 4);
                descT.offsetMax = new Vector2(-12, -4);
                AddText(descT, "설명", 13, FontStyles.Normal, TextAlignmentOptions.Left, TextDim);
            }

            CreateActionSlotPrefab();
        }

        private static void CreateActionSlotPrefab()
        {
            const string prefabPath = "Assets/03.Data/Prefabs/ActionSlotUI.prefab";
            // 기존 프리팹 삭제 (리롤 버튼 추가로 구조 변경)
            AssetDatabase.DeleteAsset(prefabPath);

            if (!AssetDatabase.IsValidFolder("Assets/03.Data/Prefabs"))
                AssetDatabase.CreateFolder("Assets/03.Data", "Prefabs");

            var go = new GameObject("ActionSlotUI");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 80);

            var layoutEl = go.AddComponent<LayoutElement>();
            layoutEl.preferredWidth = 150;
            layoutEl.preferredHeight = 80;
            layoutEl.minWidth = 150;
            layoutEl.minHeight = 80;

            go.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.18f, 0.9f);
            go.AddComponent<Outline>().effectColor = BorderRed;
            go.AddComponent<Button>();

            // 스킬 아이콘
            var icon = NewRect("SkillIcon", rect);
            icon.anchorMin = new Vector2(0, 0.5f);
            icon.anchorMax = new Vector2(0, 0.5f);
            icon.pivot = new Vector2(0, 0.5f);
            icon.anchoredPosition = new Vector2(4, 0);
            icon.sizeDelta = new Vector2(60, 60);
            icon.gameObject.AddComponent<Image>().color = AccentRed;

            // 스킬명
            var nameT = NewRect("SkillNameText", rect);
            nameT.anchorMin = new Vector2(0, 0.5f);
            nameT.anchorMax = new Vector2(1, 1);
            nameT.offsetMin = new Vector2(68, -4);
            nameT.offsetMax = new Vector2(-32, -4);
            AddText(nameT, "---", 16, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);

            // 비용 뱃지
            var costBadge = NewRect("CostBadge", rect);
            costBadge.anchorMin = new Vector2(1, 1);
            costBadge.anchorMax = new Vector2(1, 1);
            costBadge.pivot = new Vector2(1, 1);
            costBadge.anchoredPosition = new Vector2(-4, -4);
            costBadge.sizeDelta = new Vector2(28, 28);
            costBadge.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.4f, 0.8f, 0.9f);

            var costT = NewRect("CostText", costBadge);
            SetFillParent(costT);
            AddText(costT, "0", 14, FontStyles.Bold, TextAlignmentOptions.Center, Color.black);

            // 시전자명
            var casterT = NewRect("CasterNameText", rect);
            casterT.anchorMin = new Vector2(0, 0);
            casterT.anchorMax = new Vector2(1, 0.5f);
            casterT.offsetMin = new Vector2(68, 4);
            casterT.offsetMax = new Vector2(-8, 4);
            AddText(casterT, "", 12, FontStyles.Normal, TextAlignmentOptions.Left, TextDim);

            // 선택 테두리
            var selBorder = NewRect("SelectionBorder", rect);
            SetFillParent(selBorder);
            selBorder.gameObject.AddComponent<Image>().color = Color.clear;
            var selOutline = selBorder.gameObject.AddComponent<Outline>();
            selOutline.effectColor = AccentYellow;
            selOutline.effectDistance = new Vector2(3, -3);
            selBorder.gameObject.SetActive(false);

            // 실행 순서 뱃지
            var orderBadge = NewRect("ExecutionOrderBadge", rect);
            orderBadge.anchorMin = new Vector2(1, 1);
            orderBadge.anchorMax = new Vector2(1, 1);
            orderBadge.pivot = new Vector2(1, 1);
            orderBadge.anchoredPosition = new Vector2(-2, -36);
            orderBadge.sizeDelta = new Vector2(24, 24);
            orderBadge.gameObject.AddComponent<Image>().color = AccentYellow;
            var orderText = NewRect("OrderText", orderBadge);
            SetFillParent(orderText);
            AddText(orderText, "1", 12, FontStyles.Bold, TextAlignmentOptions.Center, Color.black);
            orderBadge.gameObject.SetActive(false);

            // 할당 오버레이
            var assigned = NewRect("AssignedOverlay", rect);
            SetFillParent(assigned);
            assigned.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.4f, 0.15f);
            assigned.gameObject.SetActive(false);

            // 리롤 버튼 (우측 하단 작은 버튼)
            var rerollBtn = NewRect("RerollBtn", rect);
            rerollBtn.anchorMin = new Vector2(1, 0);
            rerollBtn.anchorMax = new Vector2(1, 0);
            rerollBtn.pivot = new Vector2(1, 0);
            rerollBtn.anchoredPosition = new Vector2(-2, 2);
            rerollBtn.sizeDelta = new Vector2(24, 24);
            var rerollBtnComp = rerollBtn.gameObject.AddComponent<Button>();
            var rerollImg = rerollBtn.gameObject.AddComponent<Image>();
            rerollImg.color = ShieldBrown;
            rerollBtnComp.targetGraphic = rerollImg;
            var rerollTxt = NewRect("T", rerollBtn);
            SetFillParent(rerollTxt);
            AddText(rerollTxt, "↻", 14, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
            rerollBtn.gameObject.SetActive(false);

            // ActionSlotUI 컴포넌트 자동 와이어링
            var slotUI = go.AddComponent<ActionSlotUI>();
            SetPrivateField(slotUI, "_skillIcon", icon.gameObject.GetComponent<Image>());
            SetPrivateField(slotUI, "_skillNameText", nameT.gameObject.GetComponent<TMPro.TextMeshProUGUI>());
            SetPrivateField(slotUI, "_costText", costT.gameObject.GetComponent<TMPro.TextMeshProUGUI>());
            SetPrivateField(slotUI, "_casterNameText", casterT.gameObject.GetComponent<TMPro.TextMeshProUGUI>());
            SetPrivateField(slotUI, "_selectionBorder", selBorder.gameObject);
            SetPrivateField(slotUI, "_executionOrderBadge", orderBadge.gameObject);
            SetPrivateField(slotUI, "_executionOrderText", orderText.gameObject.GetComponent<TMPro.TextMeshProUGUI>());
            SetPrivateField(slotUI, "_assignedOverlay", assigned.gameObject);
            SetPrivateField(slotUI, "_button", go.GetComponent<Button>());
            SetPrivateField(slotUI, "_rerollButton", rerollBtn.gameObject.GetComponent<Button>());

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

            // Player panel container
            var leftSidebar = root.Find("ContentArea/LeftSidebar");
            if (leftSidebar != null)
                SetPrivateField(uiManager, "_playerPanelContainer", leftSidebar);

            // Player panel prefab
            Transform firstPanel = leftSidebar != null && leftSidebar.childCount > 0 ? leftSidebar.GetChild(0) : null;
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

            // BattleLog
            var rightSidebar = root.Find("ContentArea/RightSidebar");
            if (rightSidebar != null)
                SetPrivateField(uiManager, "_battleLog", rightSidebar.GetComponent<BattleLogUI>());

            // CurrentTurnText
            var currentTurnText = root.Find("BottomBar/CurrentTurnText");
            if (currentTurnText != null)
                SetPrivateField(uiManager, "_currentTurnText", currentTurnText.GetComponent<TMPro.TextMeshProUGUI>());

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

            EditorUtility.SetDirty(uiManager);
        }

        private static void AutoWireActionBar(ActionBarUI actionBar)
        {
            var bottomBar = actionBar.transform;

            var slotContainer = bottomBar.Find("ActionSlotContainer");
            SetPrivateField(actionBar, "_actionMenuContainer", slotContainer);

            var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03.Data/Prefabs/ActionSlotUI.prefab");
            if (slotPrefab != null)
                SetPrivateField(actionBar, "_actionSlotPrefab", slotPrefab.GetComponent<ActionSlotUI>());

            var detailPanel = bottomBar.Find("ActionDetailPanel");
            SetPrivateField(actionBar, "_actionDetailPanel", detailPanel?.gameObject);

            var titleText = detailPanel?.Find("TitleText");
            SetPrivateField(actionBar, "_actionTitleText", titleText?.GetComponent<TMPro.TextMeshProUGUI>());

            var descText = detailPanel?.Find("DescText");
            SetPrivateField(actionBar, "_actionDescText", descText?.GetComponent<TMPro.TextMeshProUGUI>());

            var endTurnBtn = actionBar.transform.root.Find("BattleUIRoot/TopBar/EndTurnButton");
            SetPrivateField(actionBar, "_endTurnButton", endTurnBtn?.GetComponent<Button>());

            EditorUtility.SetDirty(actionBar);
        }

        // ══════════════════════════════════════════════════════════
        //  유틸리티
        // ══════════════════════════════════════════════════════════

        private static void RemoveStaticPanels(Transform leftSidebar, Transform centerArea)
        {
            if (leftSidebar != null)
            {
                for (int i = leftSidebar.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(leftSidebar.GetChild(i).gameObject);
                Debug.Log("[Setup] LeftSidebar static panels removed");
            }

            if (centerArea != null)
            {
                for (int i = centerArea.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(centerArea.GetChild(i).gameObject);
                Debug.Log("[Setup] CenterArea static panels removed");
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
