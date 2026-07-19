#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using TeamLog.UI;
using TeamLog.UI.Shop;

namespace TeamLog.Editor
{
    /// <summary>
    /// ShopSceneReworkBuilder Wiring — View에 Sprite/Container/Prefab 자동 연결 +
    /// ShopItemRow Prefab 생성.
    /// </summary>
    public static partial class ShopSceneReworkBuilder
    {
        private const string SLOT_ROW_PREFAB = "Assets/03.Data/UI/ShopScene/Prefabs/ShopItemRowPrefab.prefab";

        /// <summary>
        /// ★ BuildPrefab()에서 호출 — View에 모든 자식 참조 + Sprite 자동 연결.
        /// </summary>
        private static void WireShopReworkView(ShopReworkView view)
        {
            if (view == null) return;
            var root = view.transform;

            // Frame
            var crownGo = FindDescendant(root, "GlassCrown");
            if (crownGo != null)
                WireField(view, "_glassCrownImage", crownGo.GetComponent<Image>());

            var panelGo = FindDescendant(root, "ReliquaryPanel");
            if (panelGo != null)
                WireField(view, "_reliquaryPanelImage", panelGo.GetComponent<Image>());

            // TopBar 자식들
            var titleGo = FindDescendant(root, "Title");
            if (titleGo != null)
                WireField(view, "_titleLabel", titleGo.GetComponent<TextMeshProUGUI>());

            var subGo = FindDescendant(root, "Subtitle");
            if (subGo != null)
                WireField(view, "_subtitleLabel", subGo.GetComponent<TextMeshProUGUI>());

            var buyTabGo = FindDescendant(root, "BuyTab");
            if (buyTabGo != null)
            {
                WireField(view, "_buyTabButton", buyTabGo.GetComponent<Button>());
                WireField(view, "_buyTabBackground", buyTabGo.GetComponent<Image>());
            }

            var sellTabGo = FindDescendant(root, "SellTab");
            if (sellTabGo != null)
            {
                WireField(view, "_sellTabButton", sellTabGo.GetComponent<Button>());
                WireField(view, "_sellTabBackground", sellTabGo.GetComponent<Image>());
            }

            // Gold
            var goldValueGo = FindDescendant(root, "GoldValue");
            if (goldValueGo != null)
                WireField(view, "_goldValueText", goldValueGo.GetComponent<TextMeshProUGUI>());

            // Buy Container
            var buyContainerGo = FindDescendant(root, "BuyContainer");
            if (buyContainerGo != null)
                WireField(view, "_buyContainer", buyContainerGo);

            var slotContainerGo = FindDescendant(root, "SlotContainer");
            if (slotContainerGo != null)
                WireField(view, "_slotContainer", slotContainerGo.transform);

            // Sell Container
            var sellContainerGo = FindDescendant(root, "SellContainer");
            if (sellContainerGo != null)
                WireField(view, "_sellContainer", sellContainerGo);

            var sellSlotGo = FindDescendant(root, "SellSlotContainer");
            if (sellSlotGo != null)
                WireField(view, "_sellSlotContainer", sellSlotGo.transform);

            // Footer
            var leaveBtnGo = FindDescendant(root, "LeaveButton");
            if (leaveBtnGo != null)
                WireField(view, "_leaveButton", leaveBtnGo.GetComponent<Button>());

            // Hint는 Footer의 Hint만 검색 (ShopReworkView._hintLabel — SellContainer의 Hint와 구분 위해 Footer에서)
            var footerGo = FindDescendant(root, "Footer");
            if (footerGo != null)
            {
                var hintInFooter = FindDescendant(footerGo.transform, "Hint");
                if (hintInFooter != null)
                    WireField(view, "_hintLabel", hintInFooter.GetComponent<TextMeshProUGUI>());
            }

            // ★ ShopItemRow Prefab 생성 후 _shopSlotPrefab / _sellRowPrefab에 동일 연결
            BuildShopSlotRowPrefab();
            var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SLOT_ROW_PREFAB);
            WireField(view, "_shopSlotPrefab", slotPrefab);
            WireField(view, "_sellRowPrefab", slotPrefab); // Sell도 같은 prefab 재사용

            EditorUtility.SetDirty(view.gameObject);
        }

        /// <summary>
        /// ★ ShopItemRow Prefab — 슬롯 1개 행.
        /// 구조:
        ///   ShopItemRow (Image 배경 SlotBg + Button + VLG)
        ///     - TypeBar (Image, 상단 2px 띠 — 부모 anchorMin/Max 상단)
        ///     - RarityBadge (Image 배경 + Label 자식 TMP — 우측 상단)
        ///     - SlotTop (HLG: IconFrame + NameAndDesc + Price)
        ///     - CursedWarning (작은 ☠ — 좌측 상단, 기본 비활성)
        ///     - SoldOverlay (어두운 배경 + "SOLD" 텍스트 — 중앙, 기본 비활성)
        /// </summary>
        public static void BuildShopSlotRowPrefab()
        {
            EnsureInitialized();

            var go = new GameObject("ShopItemRowPrefab", typeof(RectTransform), typeof(CanvasRenderer));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(260, 120);

            // 배경 — 9-slice SlotBg
            var bgImg = go.AddComponent<Image>();
            bgImg.sprite = LoadShopSprite("SlotBg.png");
            bgImg.type = Image.Type.Sliced;
            bgImg.color = Color.white;
            bgImg.raycastTarget = true;

            // Button
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bgImg;

            // VLG (자식 세로 배치)
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(12, 12, 10, 10);
            vlg.spacing = 6;
            vlg.childAlignment = TextAnchor.UpperCenter;

            // TypeBar — 상단 2px 띠 (별도 Image, anchor 상단 고정)
            var typeBarGo = new GameObject("TypeBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            typeBarGo.transform.SetParent(go.transform, false);
            var typeBarRt = typeBarGo.GetComponent<RectTransform>();
            typeBarRt.anchorMin = new Vector2(0f, 1f);
            typeBarRt.anchorMax = new Vector2(1f, 1f);
            typeBarRt.pivot = new Vector2(0.5f, 1f);
            typeBarRt.sizeDelta = new Vector2(0f, 3f);
            typeBarRt.anchoredPosition = Vector2.zero;
            var typeBarImg = typeBarGo.GetComponent<Image>();
            typeBarImg.color = new Color(0.85f, 0.54f, 0.23f, 1f); // 호박 기본
            typeBarImg.raycastTarget = false;
            // 부모 VLG에서 TypeBar 제외
            var typeBarLe = typeBarGo.AddComponent<LayoutElement>();
            typeBarLe.ignoreLayout = true;

            // RarityBadge — 우측 상단 배지 (Image + 자식 Label TMP)
            var badgeGo = new GameObject("RarityBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            badgeGo.transform.SetParent(go.transform, false);
            var badgeRt = badgeGo.GetComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(1f, 1f);
            badgeRt.anchorMax = new Vector2(1f, 1f);
            badgeRt.pivot = new Vector2(1f, 1f);
            badgeRt.sizeDelta = new Vector2(70, 18);
            badgeRt.anchoredPosition = new Vector2(-8, -8);
            var badgeImg = badgeGo.GetComponent<Image>();
            badgeImg.color = new Color(0.3f, 0.3f, 0.3f, 0.4f);
            badgeImg.raycastTarget = false;
            var badgeLe = badgeGo.AddComponent<LayoutElement>();
            badgeLe.ignoreLayout = true;

            var badgeLabelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            badgeLabelGo.transform.SetParent(badgeGo.transform, false);
            var badgeLabelRt = badgeLabelGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(badgeLabelRt);
            var badgeTmp = badgeLabelGo.AddComponent<TextMeshProUGUI>();
            badgeTmp.text = "·  COMMON";
            badgeTmp.font = FontLabel();
            badgeTmp.fontSize = 9;
            badgeTmp.color = Color.white;
            badgeTmp.alignment = TextAlignmentOptions.Center;
            badgeTmp.raycastTarget = false;

            // CursedWarning — 좌측 상단 ☠
            var warnGo = new GameObject("CursedWarning", typeof(RectTransform), typeof(CanvasRenderer));
            warnGo.transform.SetParent(go.transform, false);
            var warnRt = warnGo.GetComponent<RectTransform>();
            warnRt.anchorMin = new Vector2(0f, 1f);
            warnRt.anchorMax = new Vector2(0f, 1f);
            warnRt.pivot = new Vector2(0f, 1f);
            warnRt.sizeDelta = new Vector2(24, 24);
            warnRt.anchoredPosition = new Vector2(8, -8);
            var warnTmp = warnGo.AddComponent<TextMeshProUGUI>();
            warnTmp.text = "☠";
            warnTmp.font = FontTitle();
            warnTmp.fontSize = 16;
            warnTmp.color = new Color(0.95f, 0.3f, 0.3f, 1f);
            warnTmp.alignment = TextAlignmentOptions.Center;
            warnTmp.raycastTarget = false;
            var warnLe = warnGo.AddComponent<LayoutElement>();
            warnLe.ignoreLayout = true;
            warnGo.SetActive(false);

            // SlotTop — HLG (IconFrame + NameAndDesc + Price)
            var topGo = new GameObject("SlotTop", typeof(RectTransform));
            topGo.transform.SetParent(go.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(topGo, flexW: 1, prefH: 50);
            var topHlg = topGo.AddComponent<HorizontalLayoutGroup>();
            topHlg.childControlWidth = true;
            topHlg.childControlHeight = true;
            topHlg.childForceExpandWidth = false;
            topHlg.childForceExpandHeight = false;
            topHlg.spacing = 10;
            topHlg.padding = new RectOffset(4, 4, 12, 4);  // 상단 12px — TypeBar/Badge 영역 확보
            topHlg.childAlignment = TextAnchor.MiddleLeft;

            // IconFrame — Image + 자식 TMP
            var iconFrameGo = new GameObject("IconFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconFrameGo.transform.SetParent(topGo.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(iconFrameGo, prefW: 40, prefH: 40);
            var iconFrameImg = iconFrameGo.GetComponent<Image>();
            iconFrameImg.color = new Color(0.1f, 0.08f, 0.04f, 0.8f);
            iconFrameImg.raycastTarget = false;

            var iconTextGo = new GameObject("IconText", typeof(RectTransform), typeof(CanvasRenderer));
            iconTextGo.transform.SetParent(iconFrameGo.transform, false);
            var iconTextRt = iconTextGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(iconTextRt);
            var iconTmp = iconTextGo.AddComponent<TextMeshProUGUI>();
            iconTmp.text = "";  // ★ 기본 빈 문자열 — Setup 전까지는 아무것도 표시 안 함 (깜빡임 방지)
            iconTmp.font = FontTitle();
            iconTmp.fontSize = 22;
            iconTmp.color = new Color(0.9f, 0.78f, 0.31f, 1f);
            iconTmp.alignment = TextAlignmentOptions.Center;
            iconTmp.raycastTarget = false;

            // NameAndDesc — VLG (Name + Desc)
            var nameDescGo = new GameObject("NameAndDesc", typeof(RectTransform));
            nameDescGo.transform.SetParent(topGo.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(nameDescGo, flexW: 1, prefH: 44);
            var ndVlg = nameDescGo.AddComponent<VerticalLayoutGroup>();
            ndVlg.childControlWidth = true;
            ndVlg.childControlHeight = true;
            ndVlg.spacing = 2;
            ndVlg.childAlignment = TextAnchor.MiddleLeft;

            var nameGo = new GameObject("Name", typeof(RectTransform), typeof(CanvasRenderer));
            nameGo.transform.SetParent(nameDescGo.transform, false);
            var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            nameTmp.text = "Item Name";
            nameTmp.font = FontLabel();
            nameTmp.fontSize = 13;
            nameTmp.color = new Color(0.92f, 0.84f, 0.65f, 1f);
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.raycastTarget = false;
            nameTmp.enableWordWrapping = true;
            UIAutoBindHelper.EnsureLayoutElement(nameGo, flexW: 1, prefH: 20);

            var descGo = new GameObject("Desc", typeof(RectTransform), typeof(CanvasRenderer));
            descGo.transform.SetParent(nameDescGo.transform, false);
            var descTmp = descGo.AddComponent<TextMeshProUGUI>();
            descTmp.text = "Item description";
            descTmp.font = FontItalic();
            descTmp.fontStyle = FontStyles.Italic;
            descTmp.fontSize = 10;
            descTmp.color = new Color(0.7f, 0.65f, 0.5f, 1f);
            descTmp.alignment = TextAlignmentOptions.Left;
            descTmp.raycastTarget = false;
            descTmp.enableWordWrapping = true;
            UIAutoBindHelper.EnsureLayoutElement(descGo, flexW: 1, prefH: 18);

            // Price (우측)
            var priceGo = new GameObject("Price", typeof(RectTransform), typeof(CanvasRenderer));
            priceGo.transform.SetParent(topGo.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(priceGo, prefW: 70, prefH: 44);
            var priceTmp = priceGo.AddComponent<TextMeshProUGUI>();
            priceTmp.text = "45 G";
            priceTmp.font = FontLabel();
            priceTmp.fontSize = 16;
            priceTmp.color = new Color(0.9f, 0.78f, 0.31f, 1f);
            priceTmp.alignment = TextAlignmentOptions.Right;
            priceTmp.raycastTarget = false;

            // SoldOverlay — 중앙 어두운 반투명 + "SOLD" 텍스트
            var soldGo = new GameObject("SoldOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            soldGo.transform.SetParent(go.transform, false);
            var soldRt = soldGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(soldRt);
            var soldImg = soldGo.GetComponent<Image>();
            soldImg.color = new Color(0f, 0f, 0f, 0.65f);
            soldImg.raycastTarget = false;  // 버튼이 여전히 raycast 받게 (interactable=false로 처리)
            var soldLe = soldGo.AddComponent<LayoutElement>();
            soldLe.ignoreLayout = true;

            var soldTextGo = new GameObject("SoldText", typeof(RectTransform), typeof(CanvasRenderer));
            soldTextGo.transform.SetParent(soldGo.transform, false);
            var soldTextRt = soldTextGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(soldTextRt);
            var soldTmp = soldTextGo.AddComponent<TextMeshProUGUI>();
            soldTmp.text = "SOLD";
            soldTmp.font = FontTitle();
            soldTmp.fontSize = 24;
            soldTmp.color = new Color(0.7f, 0.3f, 0.3f, 1f);
            soldTmp.alignment = TextAlignmentOptions.Center;
            soldTmp.raycastTarget = false;

            soldGo.SetActive(false);

            // ShopItemRowRework 컴포넌트 부착 — 자동 바인딩 활성
            go.AddComponent<ShopItemRowRework>();

            PrefabUtility.SaveAsPrefabAsset(go, SLOT_ROW_PREFAB);
            Object.DestroyImmediate(go);

            AssetDatabase.SaveAssets();
        }
    }
}
#endif
