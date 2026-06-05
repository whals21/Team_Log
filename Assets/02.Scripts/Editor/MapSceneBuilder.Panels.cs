using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI.Event;
using TeamLog.UI.Map;
using TeamLog.UI.Shop;
using TeamLog.UI.Reward;
using TeamLog.UI;

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

        #region Confirmation Dialog

        private static GameObject BuildConfirmationDialog(Transform parent, TMP_FontAsset font)
        {
            var dialog = CreateFullImage("ConfirmationDialog", parent, new Color(0f, 0f, 0f, 0.6f));
            dialog.SetActive(false);

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
            var overlay = CreateFullImage("RestPanel", parent, OverlayBg);
            overlay.SetActive(false);

            var content = CreatePanel("Content", overlay.transform,
                new Vector2(0.25f, 0.2f), new Vector2(0.75f, 0.8f), ContentPanel);

            // 제목
            var title = CreateText("TitleLabel", content.transform, font,
                "캠프파이어", 28, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(title.GetComponent<RectTransform>(),
                new Vector2(0f, 0.82f), new Vector2(1f, 1f));

            // 선택지 버튼 3개
            var restBtn = CreateButton("RestButton", content.transform, font,
                "휴식 — HP 30% 회복", 20, new Color(0.4f, 0.85f, 0.4f));
            SetAnchors(restBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.7f));

            var trainBtn = CreateButton("TrainButton", content.transform, font,
                "수련 — ATK+1 영구 증가", 20, new Color(0.85f, 0.65f, 0.25f));
            SetAnchors(trainBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.5f));

            var meditateBtn = CreateButton("MeditateButton", content.transform, font,
                "명상 — 다음 전투 AP+1", 20, new Color(0.45f, 0.55f, 0.9f));
            SetAnchors(meditateBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.3f));

            var restUI = overlay.AddComponent<RestUI>();
            var ser = new SerializedObject(restUI);
            WireProperty(ser, "_panel", overlay);
            WireProperty(ser, "_restButton", restBtn.GetComponent<Button>());
            WireProperty(ser, "_trainButton", trainBtn.GetComponent<Button>());
            WireProperty(ser, "_meditateButton", meditateBtn.GetComponent<Button>());
            ser.ApplyModifiedProperties();

            return overlay;
        }

        #endregion
    }
}
