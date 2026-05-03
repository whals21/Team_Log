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

namespace TeamLog.Editor
{
    public static class MapSceneBuilder
    {
        private const string SCENE_PATH = "Assets/01.Scenes/MapScene.unity";
        private const string KOREAN_FONT_SDF = "Assets/08.Resource/Fonts/NanumGothic SDF.asset";
        private const string PREFAB_DIR = "Assets/03.Data/Prefabs";
        private const string CHAR_DIR = "Assets/03.Data/Characters";
        private const string SKILL_DIR = "Assets/03.Data/Skills";
        private const string ITEM_DIR = "Assets/03.Data/Items";
        private const string EVENT_DIR = "Assets/03.Data/Events";

        // 색상 팔레트 (기존 BattleUI와 통일)
        private static readonly Color BgDark = new Color(0.08f, 0.08f, 0.16f);
        private static readonly Color PanelDark = new Color(0.12f, 0.12f, 0.22f);
        private static readonly Color OverlayBg = new Color(0f, 0f, 0f, 0.7f);
        private static readonly Color ContentPanel = new Color(0.1f, 0.1f, 0.2f, 0.95f);
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextDim = new Color(0.7f, 0.7f, 0.75f);
        private static readonly Color AccentGold = new Color(0.96f, 0.82f, 0.25f);

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
            canvasObj.AddComponent<CanvasScaler>();
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
                new Vector2(0.65f, 0f), new Vector2(0.9f, 1f));

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

            // CharacterData
            WireProperty(setupSer, "_testWarriorData",
                AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_DIR}/Char_Warrior.asset"));
            WireProperty(setupSer, "_testMageData",
                AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_DIR}/Char_Mage.asset"));
            WireProperty(setupSer, "_testHealerData",
                AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_DIR}/Char_Healer.asset"));
            WireProperty(setupSer, "_testRogueData",
                AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_DIR}/Char_Rogue.asset"));

            // EventData
            var eventAssets = LoadAllAssets<EventData>(EVENT_DIR);
            var testEventsProp = setupSer.FindProperty("_testEvents");
            if (testEventsProp != null && eventAssets.Count > 0)
            {
                testEventsProp.arraySize = eventAssets.Count;
                for (int i = 0; i < eventAssets.Count; i++)
                    testEventsProp.GetArrayElementAtIndex(i).objectReferenceValue = eventAssets[i];
            }

            // SkillData pool
            var skillAssets = LoadAllAssets<SkillData>(SKILL_DIR);
            var skillPoolProp = setupSer.FindProperty("_skillPool");
            if (skillPoolProp != null && skillAssets.Count > 0)
            {
                skillPoolProp.arraySize = skillAssets.Count;
                for (int i = 0; i < skillAssets.Count; i++)
                    skillPoolProp.GetArrayElementAtIndex(i).objectReferenceValue = skillAssets[i];
            }

            // ItemData pool
            var itemAssets = LoadAllAssets<ItemData>(ITEM_DIR);
            var itemPoolProp = setupSer.FindProperty("_itemPool");
            if (itemPoolProp != null && itemAssets.Count > 0)
            {
                itemPoolProp.arraySize = itemAssets.Count;
                for (int i = 0; i < itemAssets.Count; i++)
                    itemPoolProp.GetArrayElementAtIndex(i).objectReferenceValue = itemAssets[i];
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
            Debug.Log($"[MapSceneBuilder] 캐릭터: {LoadAllAssets<CharacterData>(CHAR_DIR).Count}개");
            Debug.Log($"[MapSceneBuilder] 스킬 풀: {skillAssets.Count}개, 아이템 풀: {itemAssets.Count}개, 이벤트: {eventAssets.Count}개");
        }

        #region Event Panel

        private static GameObject BuildEventPanel(Transform parent, TMP_FontAsset font)
        {
            // 오버레이 배경
            var overlay = CreateFullImage("EventPanel", parent, OverlayBg);
            overlay.SetActive(false);

            // 콘텐츠 패널 (중앙)
            var content = CreatePanel("Content", overlay.transform,
                new Vector2(0.15f, 0.1f), new Vector2(0.85f, 0.9f), ContentPanel);

            // 제목
            var title = CreateText("TitleLabel", content.transform, font,
                "이벤트", 28, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(title.GetComponent<RectTransform>(),
                new Vector2(0f, 0.85f), new Vector2(1f, 1f));

            // 설명
            var desc = CreateText("DescLabel", content.transform, font,
                "", 18, TextWhite, TextAlignmentOptions.Center);
            var descRect = desc.GetComponent<RectTransform>();
            desc.enableWordWrapping = true;
            SetAnchors(descRect, new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.82f));

            // 선택지 컨테이너
            var choiceContainer = CreateUIObject("ChoiceContainer", content.transform);
            SetAnchors(choiceContainer.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.52f));
            choiceContainer.AddComponent<VerticalLayoutGroup>().spacing = 8;

            // 결과 패널
            var resultPanel = CreatePanel("ResultPanel", content.transform,
                new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.85f), new Color(0.08f, 0.08f, 0.16f));
            resultPanel.SetActive(false);

            var resultLabel = CreateText("ResultLabel", resultPanel.transform, font,
                "", 18, TextWhite, TextAlignmentOptions.Center);
            resultLabel.enableWordWrapping = true;
            SetAnchors(resultLabel.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.9f));

            var confirmBtn = CreateButton("ConfirmButton", resultPanel.transform, font,
                "확인", 20, AccentGold);
            SetAnchors(confirmBtn.GetComponent<RectTransform>(),
                new Vector2(0.3f, 0.05f), new Vector2(0.7f, 0.2f));

            // ChoiceButton 프리팹 로딩
            var choicePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_DIR}/ChoiceButton.prefab");

            // EventUI 컴포넌트
            var eventUI = overlay.AddComponent<EventUI>();
            var ser = new SerializedObject(eventUI);
            WireProperty(ser, "_eventTitleLabel", title);
            WireProperty(ser, "_eventDescLabel", desc);
            WireProperty(ser, "_choiceContainer", choiceContainer.transform);
            WireProperty(ser, "_choiceButtonPrefab", choicePrefab);
            WireProperty(ser, "_resultPanel", resultPanel);
            WireProperty(ser, "_resultLabel", resultLabel);
            WireProperty(ser, "_resultConfirmButton", confirmBtn.GetComponent<Button>());
            ser.ApplyModifiedProperties();

            return overlay;
        }

        #endregion

        #region Shop Panel

        private static GameObject BuildShopPanel(Transform parent, TMP_FontAsset font)
        {
            var overlay = CreateFullImage("ShopPanel", parent, OverlayBg);
            overlay.SetActive(false);

            var content = CreatePanel("Content", overlay.transform,
                new Vector2(0.15f, 0.05f), new Vector2(0.85f, 0.95f), ContentPanel);

            // 제목
            var title = CreateText("TitleLabel", content.transform, font,
                "상점", 28, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(title.GetComponent<RectTransform>(),
                new Vector2(0f, 0.9f), new Vector2(1f, 1f));

            // 골드
            var gold = CreateText("GoldLabel", content.transform, font,
                "0 G", 22, AccentGold, TextAlignmentOptions.Right);
            SetAnchors(gold.GetComponent<RectTransform>(),
                new Vector2(0.6f, 0.9f), new Vector2(0.95f, 1f));

            // 슬롯 컨테이너
            var slotContainer = CreateUIObject("SlotContainer", content.transform);
            SetAnchors(slotContainer.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.87f));
            slotContainer.AddComponent<VerticalLayoutGroup>().spacing = 8;

            // 나가기 버튼
            var exitBtn = CreateButton("ExitButton", content.transform, font,
                "나가기", 20, TextDim);
            SetAnchors(exitBtn.GetComponent<RectTransform>(),
                new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.12f));

            // ShopItemSlot 프리팹
            var shopSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_DIR}/ShopItemSlot.prefab");

            // ShopUI 컴포넌트
            var shopUI = overlay.AddComponent<ShopUI>();
            var ser = new SerializedObject(shopUI);
            WireProperty(ser, "_slotContainer", slotContainer.transform);
            WireProperty(ser, "_shopSlotPrefab", shopSlotPrefab);
            WireProperty(ser, "_goldLabel", gold);
            WireProperty(ser, "_titleLabel", title);
            WireProperty(ser, "_exitButton", exitBtn.GetComponent<Button>());
            ser.ApplyModifiedProperties();

            return overlay;
        }

        #endregion

        #region Reward Panel

        private static GameObject BuildRewardPanel(Transform parent, TMP_FontAsset font)
        {
            var overlay = CreateFullImage("RewardPanel", parent, OverlayBg);
            overlay.SetActive(false);

            var content = CreatePanel("Content", overlay.transform,
                new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.85f), ContentPanel);

            // 제목
            var title = CreateText("TitleLabel", content.transform, font,
                "보상을 선택하세요", 28, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(title.GetComponent<RectTransform>(),
                new Vector2(0f, 0.82f), new Vector2(1f, 1f));

            // 카드 컨테이너 (수평)
            var cardContainer = CreateUIObject("CardContainer", content.transform);
            SetAnchors(cardContainer.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.78f));
            var hLayout = cardContainer.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 20;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = true;

            // RewardCard 프리팹
            var rewardCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_DIR}/RewardCard.prefab");

            // RewardUI 컴포넌트
            var rewardUI = overlay.AddComponent<RewardUI>();
            var ser = new SerializedObject(rewardUI);
            WireProperty(ser, "_cardContainer", cardContainer.transform);
            WireProperty(ser, "_titleLabel", title);
            WireProperty(ser, "_rewardCardPrefab", rewardCardPrefab);
            ser.ApplyModifiedProperties();

            return overlay;
        }

        #endregion

        #region Helpers

        private static void WireProperty(SerializedObject ser, string property, Object value)
        {
            var prop = ser.FindProperty(property);
            if (prop != null && value != null)
                prop.objectReferenceValue = value;
        }

        private static List<T> LoadAllAssets<T>(string folder) where T : Object
        {
            var result = new List<T>();
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });
            foreach (var guid in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) result.Add(asset);
            }
            return result;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private static GameObject CreatePanel(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var obj = CreateUIObject(name, parent);
            var image = obj.AddComponent<Image>();
            image.color = color;
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = Vector2.zero;
            return obj;
        }

        private static GameObject CreateFullImage(string name, Transform parent, Color color)
        {
            var obj = CreateUIObject(name, parent);
            var image = obj.AddComponent<Image>();
            image.color = color;
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            return obj;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent,
            TMP_FontAsset font, string text, int fontSize, Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.Left)
        {
            var obj = CreateUIObject(name, parent);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            return tmp;
        }

        private static GameObject CreateButton(string name, Transform parent,
            TMP_FontAsset font, string text, int fontSize, Color textColor)
        {
            var obj = CreateUIObject(name, parent);
            var bg = obj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.25f);
            var button = obj.AddComponent<Button>();
            button.targetGraphic = bg;

            var textObj = CreateUIObject("Text", obj.transform);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return obj;
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.sizeDelta = Vector2.zero;
        }

        #endregion
    }
}
