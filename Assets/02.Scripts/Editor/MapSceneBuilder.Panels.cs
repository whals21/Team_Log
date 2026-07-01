using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI.Event;
using TeamLog.UI.Map;
using TeamLog.UI.Shop;
using TeamLog.UI.Reward;
using TeamLog.UI.Meta;
using TeamLog.UI;
using TeamLog.Characters;

namespace TeamLog.Editor
{
    /// <summary>
    /// MapSceneBuilder — 서브 UI 패널 생성 (Event, Shop, Reward, Confirmation, Rest)
    /// 진입점+와이어링: MapSceneBuilder.cs
    /// 헬퍼: MapSceneBuilder.Helpers.cs
    /// </summary>
    public static partial class MapSceneBuilder
    {
        #region Event Panel

        private static GameObject BuildEventPanel(Transform parent, TMP_FontAsset font)
        {
            // 오버레이 배경
            var overlay = CreateOverlay("EventPanel", parent, OverlayBg);

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
            var overlay = CreateOverlay("ShopPanel", parent, OverlayBg);

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

            // 탭 버튼 영역
            var tabContainer = CreateUIObject("TabContainer", content.transform);
            SetAnchors(tabContainer.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.89f));
            var tabLayout = tabContainer.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 4;
            tabLayout.childAlignment = TextAnchor.MiddleLeft;
            tabLayout.childControlWidth = true;
            tabLayout.childControlHeight = true;
            tabLayout.childForceExpandWidth = false;
            tabLayout.childForceExpandHeight = true;

            var buyTab = CreateButton("BuyTab", tabContainer.transform, font,
                "구매", 18, AccentGold);
            var sellTab = CreateButton("SellTab", tabContainer.transform, font,
                "판매", 18, TextDim);

            // 구매 슬롯 컨테이너 (BuyContainer)
            var buyContainerObj = CreateUIObject("BuyContainer", content.transform);
            SetAnchors(buyContainerObj.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.84f));
            var slotContentRect = CreateVerticalScrollView("ScrollView", buyContainerObj.transform, spacing: 8);
            var slotContent = slotContentRect.gameObject;

            // 판매 컨테이너
            var sellContainerObj = CreateUIObject("SellContainer", content.transform);
            SetAnchors(sellContainerObj.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.84f));
            var sellContentRect = CreateVerticalScrollView("ScrollView", sellContainerObj.transform, spacing: 4);
            var sellContent = sellContentRect.gameObject;
            sellContainerObj.SetActive(false);

            // 나가기 버튼
            var exitBtn = CreateButton("ExitButton", content.transform, font,
                "나가기", 20, TextDim);
            SetAnchors(exitBtn.GetComponent<RectTransform>(),
                new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.12f));

            // 증강 배정 패널 (AugmentSelectPanel)
            var shopAugmentSelectPanel = BuildAugmentSelectPanel(content.transform, font);

            // ShopItemSlot 프리팹
            var shopSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_DIR}/ShopItemSlot.prefab");

            // ShopUI 컴포넌트
            var shopUI = overlay.AddComponent<ShopUI>();
            var ser = new SerializedObject(shopUI);
            WireProperty(ser, "_slotContainer", slotContent.transform);
            WireProperty(ser, "_shopSlotPrefab", shopSlotPrefab);
            WireProperty(ser, "_goldLabel", gold);
            WireProperty(ser, "_titleLabel", title);
            WireProperty(ser, "_exitButton", exitBtn.GetComponent<Button>());
            WireProperty(ser, "_buyTabButton", buyTab.GetComponent<Button>());
            WireProperty(ser, "_sellTabButton", sellTab.GetComponent<Button>());
            WireProperty(ser, "_buyContainer", buyContainerObj);
            WireProperty(ser, "_sellContainer", sellContent.transform);
            WireProperty(ser, "_augmentSelectPanel", shopAugmentSelectPanel);
            ser.ApplyModifiedProperties();

            return overlay;
        }

        #endregion

        #region Reward Panel

        private static GameObject BuildRewardPanel(Transform parent, TMP_FontAsset font)
        {
            var overlay = CreateOverlay("RewardPanel", parent, OverlayBg);

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
                new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.78f));
            var hLayout = cardContainer.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 20;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = true;

            // 하단 버튼 영역 (리롤 + 스킵)
            var bottomBar = CreateUIObject("BottomBar", content.transform);
            SetAnchors(bottomBar.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.15f));
            var bottomLayout = bottomBar.AddComponent<HorizontalLayoutGroup>();
            bottomLayout.spacing = 20;
            bottomLayout.childAlignment = TextAnchor.MiddleCenter;
            bottomLayout.childControlWidth = true;
            bottomLayout.childControlHeight = true;
            bottomLayout.childForceExpandWidth = true;
            bottomLayout.childForceExpandHeight = true;

            var rerollBtn = CreateButton("RerollButton", bottomBar.transform, font,
                "리롤 (0)", 20, new Color(0.3f, 0.6f, 1f));
            var rerollLabel = rerollBtn.GetComponentInChildren<TextMeshProUGUI>();

            var skipBtn = CreateButton("SkipButton", bottomBar.transform, font,
                "건너뛰기 +15G", 20, TextDim);
            var skipLabel = skipBtn.GetComponentInChildren<TextMeshProUGUI>();

            // 증강 배정 패널 (AugmentSelectPanel — 상점 호환)
            var augmentSelectPanel = BuildAugmentSelectPanel(content.transform, font);

            // RewardCard 프리팹
            var rewardCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_DIR}/RewardCard.prefab");

            // RewardUI 컴포넌트
            var rewardUI = overlay.AddComponent<RewardUI>();
            var ser = new SerializedObject(rewardUI);
            WireProperty(ser, "_cardContainer", cardContainer.transform);
            WireProperty(ser, "_titleLabel", title);
            WireProperty(ser, "_rewardCardPrefab", rewardCardPrefab);
            WireProperty(ser, "_rerollButton", rerollBtn.GetComponent<Button>());
            WireProperty(ser, "_rerollLabel", rerollLabel);
            WireProperty(ser, "_skipButton", skipBtn.GetComponent<Button>());
            WireProperty(ser, "_augmentSelectPanel", augmentSelectPanel);
            ser.ApplyModifiedProperties();

            return overlay;
        }

        #endregion

        #region Confirmation Dialog

        private static GameObject BuildConfirmationDialog(Transform parent, TMP_FontAsset font)
        {
            var dialog = CreateOverlay("ConfirmationDialog", parent, new Color(0f, 0f, 0f, 0.6f));

            var content = CreatePanel("Content", dialog.transform,
                new Vector2(0.25f, 0.35f), new Vector2(0.75f, 0.65f), ContentPanel);

            var message = CreateText("MessageText", content.transform, font,
                "", 20, TextWhite, TextAlignmentOptions.Center);
            message.enableWordWrapping = true;
            SetAnchors(message.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.9f));

            var yesBtn = CreateButton("YesButton", content.transform, font,
                "예", 18, AccentGold);
            SetAnchors(yesBtn.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.05f), new Vector2(0.45f, 0.3f));

            var noBtn = CreateButton("NoButton", content.transform, font,
                "아니오", 18, TextDim);
            SetAnchors(noBtn.GetComponent<RectTransform>(),
                new Vector2(0.55f, 0.05f), new Vector2(0.95f, 0.3f));

            var comp = dialog.AddComponent<ConfirmationDialog>();
            var ser = new SerializedObject(comp);
            WireProperty(ser, "_messageText", message);
            WireProperty(ser, "_yesButton", yesBtn.GetComponent<Button>());
            WireProperty(ser, "_noButton", noBtn.GetComponent<Button>());
            ser.ApplyModifiedProperties();

            return dialog;
        }

        #endregion

        #region Rest Panel

        private static GameObject BuildRestPanel(Transform parent, TMP_FontAsset font)
        {
            var overlay = CreateOverlay("RestPanel", parent, OverlayBg);

            var content = CreatePanel("Content", overlay.transform,
                new Vector2(0.25f, 0.15f), new Vector2(0.75f, 0.85f), ContentPanel);

            // 제목
            var title = CreateText("TitleLabel", content.transform, font,
                "캠프파이어", 28, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(title.GetComponent<RectTransform>(),
                new Vector2(0f, 0.84f), new Vector2(1f, 1f));

            // 선택지 버튼 4개
            var restBtn = CreateButton("RestButton", content.transform, font,
                "휴식 — HP 30% 회복", 18, new Color(0.4f, 0.85f, 0.4f));
            SetAnchors(restBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.76f));

            var trainBtn = CreateButton("TrainButton", content.transform, font,
                "수련 — ATK+1 영구 증가", 18, new Color(0.85f, 0.65f, 0.25f));
            SetAnchors(trainBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.44f), new Vector2(0.9f, 0.58f));

            var meditateBtn = CreateButton("MeditateButton", content.transform, font,
                "명상 — 다음 전투 AP+1", 18, new Color(0.45f, 0.55f, 0.9f));
            SetAnchors(meditateBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.26f), new Vector2(0.9f, 0.4f));

            var rerollBtn = CreateButton("RerollButton", content.transform, font,
                "리롤 토큰 +1 획득", 18, new Color(0.7f, 0.4f, 0.85f));
            SetAnchors(rerollBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.22f));

            var restUI = overlay.AddComponent<RestUI>();
            var ser = new SerializedObject(restUI);
            WireProperty(ser, "_panel", overlay);
            WireProperty(ser, "_restButton", restBtn.GetComponent<Button>());
            WireProperty(ser, "_trainButton", trainBtn.GetComponent<Button>());
            WireProperty(ser, "_meditateButton", meditateBtn.GetComponent<Button>());
            WireProperty(ser, "_rerollButton", rerollBtn.GetComponent<Button>());
            ser.ApplyModifiedProperties();

            return overlay;
        }

        #endregion

        #region Run End Overlay

        private static GameObject BuildRunEndOverlay(Transform parent, TMP_FontAsset font)
        {
            var overlay = CreateOverlay("RunEndOverlay", parent, OverlayBg, withCanvasGroup: true);
            var canvasGroup = overlay.GetComponent<CanvasGroup>();

            var content = CreatePanel("Content", overlay.transform,
                new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.8f), ContentPanel);

            // 결과 텍스트
            var resultText = CreateText("ResultText", content.transform, font,
                "", 48, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(resultText.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.7f), new Vector2(0.95f, 0.9f));

            // 통계 텍스트
            var statsText = CreateText("StatsText", content.transform, font,
                "", 24, TextWhite, TextAlignmentOptions.Center);
            statsText.enableWordWrapping = true;
            SetAnchors(statsText.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f));

            // 타이틀로 버튼
            var toTitleBtn = CreateButton("ToTitleButton", content.transform, font,
                "타이틀로", 24, TextWhite);
            SetAnchors(toTitleBtn.GetComponent<RectTransform>(),
                new Vector2(0.3f, 0.1f), new Vector2(0.7f, 0.28f));

            var runEndOverlay = overlay.AddComponent<RunEndOverlay>();
            var ser = new SerializedObject(runEndOverlay);
            WireProperty(ser, "_canvasGroup", canvasGroup);
            WireProperty(ser, "_resultText", resultText);
            WireProperty(ser, "_statsText", statsText);
            WireProperty(ser, "_toTitleButton", toTitleBtn.GetComponent<Button>());
            ser.ApplyModifiedProperties();

            return overlay;
        }

        #endregion

        #region Deck Viewer

        private static GameObject BuildDeckViewerPanel(Transform parent, TMP_FontAsset font)
        {
            var overlay = CreateOverlay("DeckViewerPanel", parent, OverlayBg, withCanvasGroup: true);
            var canvasGroup = overlay.GetComponent<CanvasGroup>();

            var content = CreatePanel("Content", overlay.transform,
                new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.95f), ContentPanel);

            // 제목
            var title = CreateText("TitleLabel", content.transform, font,
                "덱 조회", 28, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(title.GetComponent<RectTransform>(),
                new Vector2(0f, 0.92f), new Vector2(0.7f, 1f));

            // 닫기 버튼
            var closeBtn = CreateButton("CloseButton", content.transform, font,
                "닫기", 20, TextDim);
            SetAnchors(closeBtn.GetComponent<RectTransform>(),
                new Vector2(0.75f, 0.92f), new Vector2(0.95f, 1f));

            // 스크롤 뷰
            var scrollView = CreateUIObject("ScrollView", content.transform);
            SetAnchors(scrollView.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.9f));
            var scrollContentRect = CreateVerticalScrollView("Inner", scrollView.transform,
                spacing: 2, childAlignment: TextAnchor.UpperCenter);
            var scrollContent = scrollContentRect.gameObject;

            var deckViewer = overlay.AddComponent<DeckViewerUI>();
            var ser = new SerializedObject(deckViewer);
            WireProperty(ser, "_contentContainer", scrollContent.transform);
            WireProperty(ser, "_closeButton", closeBtn.GetComponent<Button>());
            WireProperty(ser, "_titleLabel", title);
            ser.ApplyModifiedProperties();

            return overlay;
        }

        #endregion

        #region Tutorial Overlay

        private static GameObject BuildTutorialOverlay(Transform parent, TMP_FontAsset font)
        {
            var overlay = CreateOverlay("TutorialOverlay", parent, new Color(0f, 0f, 0f, 0.7f), withCanvasGroup: true);
            var canvasGroup = overlay.GetComponent<CanvasGroup>();

            // 하이라이트 영역 (반투명 테두리)
            var highlight = CreatePanel("HighlightArea", overlay.transform,
                new Vector2(0.15f, 0.3f), new Vector2(0.85f, 0.7f), new Color(0.2f, 0.6f, 1f, 0.15f));
            var highlightImg = highlight.GetComponent<Image>();
            var hlOutline = highlight.AddComponent<Outline>();
            hlOutline.effectColor = new Color(0.3f, 0.7f, 1f, 0.8f);
            hlOutline.effectDistance = new Vector2(3, -3);

            // 설명 패널 (하단)
            var descPanel = CreatePanel("DescPanel", overlay.transform,
                new Vector2(0.2f, 0.05f), new Vector2(0.8f, 0.28f), new Color(0.06f, 0.06f, 0.14f, 0.95f));

            var titleText = CreateText("TitleText", descPanel.transform, font,
                "", 24, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(titleText.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.7f), new Vector2(0.95f, 0.95f));

            var descText = CreateText("DescText", descPanel.transform, font,
                "", 16, TextWhite, TextAlignmentOptions.Center);
            descText.enableWordWrapping = true;
            SetAnchors(descText.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.65f));

            var nextBtn = CreateButton("NextButton", descPanel.transform, font,
                "다음", 18, AccentGold);
            SetAnchors(nextBtn.GetComponent<RectTransform>(),
                new Vector2(0.55f, 0.05f), new Vector2(0.75f, 0.3f));

            var skipBtn = CreateButton("SkipButton", descPanel.transform, font,
                "건너뛰기", 14, TextDim);
            SetAnchors(skipBtn.GetComponent<RectTransform>(),
                new Vector2(0.78f, 0.05f), new Vector2(0.95f, 0.3f));

            var nextLabel = nextBtn.GetComponentInChildren<TextMeshProUGUI>();

            var tutorialUI = overlay.AddComponent<TutorialUI>();
            var ser = new SerializedObject(tutorialUI);
            WireProperty(ser, "_overlay", overlay);
            WireProperty(ser, "_highlightArea", highlightImg);
            WireProperty(ser, "_titleText", titleText);
            WireProperty(ser, "_descText", descText);
            WireProperty(ser, "_nextButton", nextBtn.GetComponent<Button>());
            WireProperty(ser, "_skipButton", skipBtn.GetComponent<Button>());
            WireProperty(ser, "_nextLabel", nextLabel);
            ser.ApplyModifiedProperties();

            return overlay;
        }

        #endregion

        #region Relic Bar

        private static GameObject BuildRelicBar(Transform parent, TMP_FontAsset font)
        {
            // 하단 유물 바 배경
            var bar = CreatePanel("RelicBar", parent,
                new Vector2(0f, 0f), new Vector2(1f, 0.06f), PanelDark);

            // 유물 카운트 라벨
            var countLabel = CreateText("CountLabel", bar.transform, font,
                "", 18, AccentGold, TextAlignmentOptions.Left);
            SetAnchors(countLabel.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0f), new Vector2(0.15f, 1f));

            // 아이콘 컨테이너 (수평)
            var iconContainer = CreateUIObject("IconContainer", bar.transform);
            SetAnchors(iconContainer.GetComponent<RectTransform>(),
                new Vector2(0.16f, 0.05f), new Vector2(0.98f, 0.95f));
            var layout = iconContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            var relicBarUI = bar.AddComponent<RelicBarUI>();
            var ser = new SerializedObject(relicBarUI);
            WireProperty(ser, "_iconContainer", iconContainer.transform);
            WireProperty(ser, "_countLabel", countLabel);
            ser.ApplyModifiedProperties();

            return bar;
        }

        #endregion

        #region Augment Select Panel

        private static AugmentSelectPanel BuildAugmentSelectPanel(Transform parent, TMP_FontAsset font)
        {
            var panel = CreateOverlay("AugmentSelectPanel", parent, new Color(0f, 0f, 0f, 0.85f));

            var assignTitle = CreateText("Title", panel.transform, font,
                "증강 적용: 캐릭터 선택", 24, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(assignTitle.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.95f));

            var btnContainer = CreateUIObject("ButtonContainer", panel.transform);
            SetAnchors(btnContainer.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.18f), new Vector2(0.9f, 0.78f));
            var vlg = btnContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var skipBtn = CreateButton("SkipButton", panel.transform, font,
                "건너뛰기", 20, TextDim);
            SetAnchors(skipBtn.GetComponent<RectTransform>(),
                new Vector2(0.3f, 0.05f), new Vector2(0.7f, 0.15f));

            var comp = panel.AddComponent<AugmentSelectPanel>();
            var ser = new SerializedObject(comp);
            WireProperty(ser, "_panel", panel);
            WireProperty(ser, "_titleLabel", assignTitle);
            WireProperty(ser, "_buttonContainer", btnContainer.transform);
            WireProperty(ser, "_skipButton", skipBtn.GetComponent<Button>());
            ser.ApplyModifiedProperties();

            return comp;
        }

        #endregion

        #region Stage Bonus Panel (엘리트/스테이지 클리어 보상)

        private static GameObject BuildStageBonusPanel(Transform parent, TMP_FontAsset font)
        {
            var overlay = CreateOverlay("StageBonusPanel", parent, OverlayBg, withCanvasGroup: true);
            var canvasGroup = overlay.GetComponent<CanvasGroup>();

            var content = CreatePanel("Content", overlay.transform,
                new Vector2(0.25f, 0.15f), new Vector2(0.75f, 0.85f), ContentPanel);

            // 제목
            var title = CreateText("TitleLabel", content.transform, font,
                "보너스 선택", 28, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(title.GetComponent<RectTransform>(),
                new Vector2(0f, 0.85f), new Vector2(1f, 1f));

            // 설명
            var desc = CreateText("DescLabel", content.transform, font,
                "", 16, TextDim, TextAlignmentOptions.Center);
            desc.enableWordWrapping = true;
            SetAnchors(desc.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.74f), new Vector2(0.95f, 0.85f));

            // 선택지 컨테이너
            var choiceContainer = CreateUIObject("ChoiceContainer", content.transform);
            SetAnchors(choiceContainer.GetComponent<RectTransform>(),
                new Vector2(0.08f, 0.1f), new Vector2(0.92f, 0.72f));
            var vLayout = choiceContainer.AddComponent<VerticalLayoutGroup>();
            vLayout.spacing = 12;
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = true;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = true;

            // 선택지 버튼 3개
            var buttons = new Button[3];
            var labels = new TextMeshProUGUI[3];
            for (int i = 0; i < 3; i++)
            {
                var btn = CreateButton($"Choice{i}", choiceContainer.transform, font,
                    "", 20, TextWhite);
                buttons[i] = btn.GetComponent<Button>();
                labels[i] = btn.GetComponentInChildren<TextMeshProUGUI>();
            }

            var comp = overlay.AddComponent<StageBonusUI>();
            var ser = new SerializedObject(comp);
            WireProperty(ser, "_panel", overlay);
            WireProperty(ser, "_canvasGroup", canvasGroup);
            WireProperty(ser, "_titleLabel", title);
            WireProperty(ser, "_descLabel", desc);

            // 배열 필드 와이어링
            var buttonsProp = ser.FindProperty("_choiceButtons");
            if (buttonsProp != null && buttonsProp.isArray)
            {
                buttonsProp.arraySize = 3;
                for (int i = 0; i < 3; i++)
                    buttonsProp.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
            }
            var labelsProp = ser.FindProperty("_choiceLabels");
            if (labelsProp != null && labelsProp.isArray)
            {
                labelsProp.arraySize = 3;
                for (int i = 0; i < 3; i++)
                    labelsProp.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];
            }
            ser.ApplyModifiedProperties();

            // 초기 비활성화 (사용 전까지 숨김)
            overlay.SetActive(false);

            return overlay;
        }

        #endregion

        #region Character Select Panel

        private static GameObject BuildCharacterSelectPanel(Transform parent, TMP_FontAsset font)
        {
            var overlay = CreateOverlay("CharacterSelectPanel", parent, OverlayBg);

            var content = CreatePanel("Content", overlay.transform,
                new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f), ContentPanel);

            // 제목
            var title = CreateText("TitleLabel", content.transform, font,
                "파티 구성", 28, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(title.GetComponent<RectTransform>(),
                new Vector2(0f, 0.88f), new Vector2(0.6f, 1f));

            // 카운트 라벨
            var countLabel = CreateText("CountLabel", content.transform, font,
                "선택: 0/4", 20, TextWhite, TextAlignmentOptions.Right);
            SetAnchors(countLabel.GetComponent<RectTransform>(),
                new Vector2(0.6f, 0.88f), new Vector2(1f, 1f));

            // 캐릭터 카드 컨테이너 (수평 스크롤)
            var cardContainer = CreateUIObject("CardContainer", content.transform);
            SetAnchors(cardContainer.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.85f));
            var hlg = cardContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // 시작 버튼
            var startBtn = CreateButton("StartButton", content.transform, font,
                "모험 시작", 24, AccentGold);
            SetAnchors(startBtn.GetComponent<RectTransform>(),
                new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.14f));

            var comp = overlay.AddComponent<CharacterSelectUI>();
            var ser = new SerializedObject(comp);
            WireProperty(ser, "_panel", overlay);
            WireProperty(ser, "_characterContainer", cardContainer.transform);
            WireProperty(ser, "_startButton", startBtn.GetComponent<Button>());
            WireProperty(ser, "_titleLabel", title);
            WireProperty(ser, "_countLabel", countLabel);
            ser.ApplyModifiedProperties();

            return overlay;
        }

        #endregion

        #region Character Trait Select Panel (Phase 8D)

        /// <summary>
        /// 캐릭터 장착 특성 선택 패널 — CharacterSelectUI 이후 표시.
        /// 필드: _panel, _rowsContainer, _startButton, _backButton, _titleLabel, _hintLabel
        /// </summary>
        private static GameObject BuildCharacterTraitSelectPanel(Transform parent, TMP_FontAsset font)
        {
            var overlay = CreateOverlay("CharacterTraitSelectPanel", parent, OverlayBg);

            var content = CreatePanel("Content", overlay.transform,
                new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f), ContentPanel);

            // 제목
            var title = CreateText("TitleLabel", content.transform, font,
                "특성 선택", 28, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(title.GetComponent<RectTransform>(),
                new Vector2(0f, 0.9f), new Vector2(0.6f, 1f));

            // 힌트 라벨
            var hint = CreateText("HintLabel", content.transform, font,
                "각 캐릭터의 장착 특성을 선택하세요", 18, TextDim, TextAlignmentOptions.Right);
            SetAnchors(hint.GetComponent<RectTransform>(),
                new Vector2(0.6f, 0.9f), new Vector2(1f, 1f));

            // 행 컨테이너 (수직 스크롤)
            var rowsContainer = CreateUIObject("RowsContainer", content.transform);
            SetAnchors(rowsContainer.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.85f));
            var vlg = rowsContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // ContentSizeFitter로 행 높이 자동
            var csf = rowsContainer.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 시작 버튼 (좌측 하단)
            var startBtn = CreateButton("StartButton", content.transform, font,
                "모험 시작", 24, AccentGold);
            SetAnchors(startBtn.GetComponent<RectTransform>(),
                new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.14f));

            // 뒤로 버튼 (우측 하단)
            var backBtn = CreateButton("BackButton", content.transform, font,
                "뒤로", 20, TextWhite);
            SetAnchors(backBtn.GetComponent<RectTransform>(),
                new Vector2(0.75f, 0.03f), new Vector2(0.95f, 0.14f));

            var comp = overlay.AddComponent<CharacterTraitSelectUI>();
            var ser = new SerializedObject(comp);
            WireProperty(ser, "_panel", overlay);
            WireProperty(ser, "_rowsContainer", rowsContainer.transform);
            WireProperty(ser, "_startButton", startBtn.GetComponent<Button>());
            WireProperty(ser, "_backButton", backBtn.GetComponent<Button>());
            WireProperty(ser, "_titleLabel", title);
            WireProperty(ser, "_hintLabel", hint);
            ser.ApplyModifiedProperties();

            return overlay;
        }

        #endregion

        #region Meta Shop Panel (Phase 8D)

        /// <summary>
        /// 메타 상점 패널 (타이틀 씬) — 3탭 구조 (특성/유물/강화).
        /// 필드: _panel, _closeButton, 3개 탭 버튼, _memoryLabel, _soulLabel,
        ///       _contentContainer, _allTraits, _allUpgrades, _allRelics
        /// </summary>
        private static GameObject BuildMetaShopPanel(Transform parent, TMP_FontAsset font)
        {
            var overlay = CreateOverlay("MetaShopPanel", parent, OverlayBg);

            var content = CreatePanel("Content", overlay.transform,
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), ContentPanel);

            // 상단 바 — 잔고 표시
            var topBar = CreateUIObject("TopBar", content.transform);
            SetAnchors(topBar.GetComponent<RectTransform>(),
                new Vector2(0f, 0.92f), new Vector2(1f, 1f));

            var titleLabel = CreateText("TitleLabel", topBar.transform, font,
                "메타 상점", 28, AccentGold, TextAlignmentOptions.Left);
            SetAnchors(titleLabel.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0f), new Vector2(0.4f, 1f));

            var memoryLabel = CreateText("MemoryLabel", topBar.transform, font,
                "기억: 0", 20, new Color(0.6f, 0.85f, 1f), TextAlignmentOptions.Right);
            SetAnchors(memoryLabel.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0f), new Vector2(0.75f, 1f));

            var soulLabel = CreateText("SoulLabel", topBar.transform, font,
                "영혼: 0", 20, new Color(0.85f, 0.6f, 1f), TextAlignmentOptions.Right);
            SetAnchors(soulLabel.GetComponent<RectTransform>(),
                new Vector2(0.75f, 0f), new Vector2(0.95f, 1f));

            // 탭 버튼 행
            var tabContainer = CreateUIObject("TabContainer", content.transform);
            SetAnchors(tabContainer.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.85f), new Vector2(0.98f, 0.92f));
            var thlg = tabContainer.AddComponent<HorizontalLayoutGroup>();
            thlg.spacing = 6;
            thlg.childAlignment = TextAnchor.UpperLeft;
            thlg.childControlWidth = true;
            thlg.childControlHeight = true;
            thlg.childForceExpandWidth = true;
            thlg.childForceExpandHeight = true;

            var traitsTab = CreateButton("TraitsTab", tabContainer.transform, font,
                "특성", 22, AccentGold);
            var relicsTab = CreateButton("RelicsTab", tabContainer.transform, font,
                "유물", 22, TextWhite);
            var upgradesTab = CreateButton("UpgradesTab", tabContainer.transform, font,
                "강화", 22, TextWhite);

            // 닫기 버튼 (우측 상단)
            var closeBtn = CreateButton("CloseButton", content.transform, font,
                "닫기", 22, TextDim);
            SetAnchors(closeBtn.GetComponent<RectTransform>(),
                new Vector2(0.85f, 0.92f), new Vector2(0.98f, 0.99f));

            // 콘텐츠 스크롤 영역
            var scrollObj = CreateUIObject("ContentScroll", content.transform);
            SetAnchors(scrollObj.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.84f));
            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewport = CreateUIObject("Viewport", scrollObj.transform);
            SetAnchors(viewport.GetComponent<RectTransform>(),
                Vector2.zero, Vector2.one);
            var viewportMask = viewport.AddComponent<RectMask2D>();

            var contentContainer = CreateUIObject("ContentContainer", viewport.transform);
            SetAnchors(contentContainer.GetComponent<RectTransform>(),
                new Vector2(0f, 1f), Vector2.one);
            var contentContainerRect = contentContainer.GetComponent<RectTransform>();
            contentContainerRect.pivot = new Vector2(0.5f, 1f);
            var contentVlg = contentContainer.AddComponent<VerticalLayoutGroup>();
            contentVlg.spacing = 6;
            contentVlg.childAlignment = TextAnchor.UpperCenter;
            contentVlg.childControlWidth = true;
            contentVlg.childControlHeight = false;
            contentVlg.childForceExpandWidth = true;
            contentVlg.childForceExpandHeight = false;
            var contentCsf = contentContainer.AddComponent<ContentSizeFitter>();
            contentCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentContainerRect;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            var comp = overlay.AddComponent<MetaShopUI>();
            var ser = new SerializedObject(comp);
            WireProperty(ser, "_panel", overlay);
            WireProperty(ser, "_closeButton", closeBtn.GetComponent<Button>());
            WireProperty(ser, "_traitsTabButton", traitsTab.GetComponent<Button>());
            WireProperty(ser, "_relicsTabButton", relicsTab.GetComponent<Button>());
            WireProperty(ser, "_upgradesTabButton", upgradesTab.GetComponent<Button>());
            WireProperty(ser, "_memoryLabel", memoryLabel);
            WireProperty(ser, "_soulLabel", soulLabel);
            WireProperty(ser, "_contentContainer", contentContainer.transform);
            ser.ApplyModifiedProperties();

            return overlay;
        }

        #endregion

        #region Ascension UI (Phase ASC)

        /// <summary>
        /// 어센션 레벨 표시/선택 패널 (우측 하단). TitleScene에서만 사용.
        /// ▼ 버튼 + 라벨 + ▲ 버튼 가로 배치. TitleSceneSetup의 SerializeField와 연결됨.
        /// out 파라미터로 Button/Label 참조를 직접 반환 (Find 계열 사용 회피).
        /// </summary>
        private static GameObject BuildAscensionPanel(Transform parent, TMP_FontAsset font,
            out Button upButton, out Button downButton, out TextMeshProUGUI label)
        {
            var panel = CreateUIObject("AscensionPanel", parent);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-30f, 30f);
            rt.sizeDelta = new Vector2(320f, 70f);

            var hlg = panel.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(4, 4, 4, 4);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var downBtn = CreateButton("AscensionDownButton", panel.transform, font,
                "▼", 36, TextWhite);
            var downLe = downBtn.AddComponent<LayoutElement>();
            downLe.minWidth = 60;
            downLe.minHeight = 60;

            var labelTmp = CreateText("AscensionLabel", panel.transform, font,
                "Ascension 0", 24, TextWhite, TextAlignmentOptions.Center);
            var labelLe = labelTmp.gameObject.AddComponent<LayoutElement>();
            labelLe.minWidth = 180;
            labelLe.minHeight = 60;

            var upBtn = CreateButton("AscensionUpButton", panel.transform, font,
                "▲", 36, TextWhite);
            var upLe = upBtn.AddComponent<LayoutElement>();
            upLe.minWidth = 60;
            upLe.minHeight = 60;

            downButton = downBtn.GetComponent<Button>();
            upButton = upBtn.GetComponent<Button>();
            label = labelTmp;
            return panel;
        }

        #endregion
    }
}
