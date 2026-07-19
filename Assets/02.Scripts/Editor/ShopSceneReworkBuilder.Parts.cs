#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI;
using TeamLog.UI.Shop;

namespace TeamLog.Editor
{
    /// <summary>
    /// ShopSceneReworkBuilder 파트 — UI 부품 생성.
    /// ★ MapSceneReworkBuilder.Parts / EventSceneReworkBuilder.Parts와 동일 패턴.
    /// </summary>
    public static partial class ShopSceneReworkBuilder
    {
        // =========================================================
        // DimBackground
        // =========================================================
        private static void BuildDimBackground(Transform parent)
        {
            var go = new GameObject("DimBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(rt);

            var img = go.GetComponent<Image>();
            var sprite = LoadShopSprite("DimBackground.png");
            if (sprite != null) img.sprite = sprite;
            img.color = Color.white;
            img.raycastTarget = true;
        }

        // =========================================================
        // ReliquaryFrame — 중앙 카드 (900×760)
        // =========================================================
        private static void BuildReliquaryFrame(Transform parent)
        {
            var go = new GameObject("ReliquaryFrame", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(900f, 760f);
            rt.anchoredPosition = Vector2.zero;

            BuildGlassCrown(go.transform);          // 상단 80px
            BuildReliquaryPanel(go.transform);      // 하단 본문
        }

        // =========================================================
        // GlassCrown — 상단 80px 스테인드글라스 장식
        // =========================================================
        private static void BuildGlassCrown(Transform parent)
        {
            var go = new GameObject("GlassCrown", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 80f);
            rt.anchoredPosition = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.sprite = LoadShopSprite("GlassCrown.png");
            img.type = Image.Type.Simple;
            img.color = Color.white;
            img.raycastTarget = false;

            // Crown 중앙 엠블럼 (◈)
            var emblemGo = new GameObject("Emblem", typeof(RectTransform), typeof(CanvasRenderer));
            emblemGo.transform.SetParent(go.transform, false);
            var emblemRt = emblemGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(emblemRt);
            var emblemTmp = emblemGo.AddComponent<TextMeshProUGUI>();
            emblemTmp.text = "✦";
            emblemTmp.font = FontTitle();
            emblemTmp.fontSize = 42;
            emblemTmp.color = new Color(0f, 0f, 0f, 0.75f);
            emblemTmp.alignment = TextAlignmentOptions.Center;
            emblemTmp.raycastTarget = false;
        }

        // =========================================================
        // ReliquaryPanel — 본문 (하단, 어두운 패널)
        // =========================================================
        private static void BuildReliquaryPanel(Transform parent)
        {
            var go = new GameObject("ReliquaryPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.offsetMin = new Vector2(0f, 0f);
            rt.offsetMax = new Vector2(0f, -80f); // 상단 80px GlassCrown
            rt.anchoredPosition = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.sprite = LoadShopSprite("PanelBackground.png");
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            img.raycastTarget = false;

            // VLG
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(40, 40, 24, 24);
            vlg.spacing = 14;
            vlg.childAlignment = TextAnchor.UpperCenter;

            BuildTopBar(go.transform);
            BuildGoldBar(go.transform);
            BuildBuyContainer(go.transform);
            BuildSellContainer(go.transform);
            BuildFooter(go.transform);
        }

        // =========================================================
        // TopBar — HLG: TitleBlock + BuyTab + SellTab
        // =========================================================
        private static void BuildTopBar(Transform parent)
        {
            var go = new GameObject("TopBar", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            UIAutoBindHelper.EnsureLayoutElement(go, flexW: 1, prefH: 48);

            // TitleBlock
            var titleBlock = new GameObject("TitleBlock", typeof(RectTransform));
            titleBlock.transform.SetParent(go.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(titleBlock, flexW: 1, prefH: 48);
            var titleVlg = titleBlock.AddComponent<VerticalLayoutGroup>();
            titleVlg.childControlWidth = true;
            titleVlg.childControlHeight = true;
            titleVlg.spacing = 1;
            titleVlg.childAlignment = TextAnchor.UpperLeft;

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer));
            titleGo.transform.SetParent(titleBlock.transform, false);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "RELICS OF THE FOLD";
            titleTmp.font = FontTitle();
            titleTmp.fontSize = 22;
            titleTmp.color = new Color(0.9f, 0.78f, 0.31f, 1f);
            titleTmp.alignment = TextAlignmentOptions.Left;
            titleTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(titleGo, flexW: 1, prefH: 28);

            var subGo = new GameObject("Subtitle", typeof(RectTransform), typeof(CanvasRenderer));
            subGo.transform.SetParent(titleBlock.transform, false);
            var subTmp = subGo.AddComponent<TextMeshProUGUI>();
            subTmp.text = "— Floor 2 · Sanctum —";
            subTmp.font = FontItalic();
            subTmp.fontStyle = FontStyles.Italic;
            subTmp.fontSize = 11;
            subTmp.color = new Color(0.65f, 0.6f, 0.45f, 1f);
            subTmp.alignment = TextAlignmentOptions.Left;
            subTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(subGo, flexW: 1, prefH: 16);

            // BuyTab
            BuildTabButton(go.transform, "BuyTab", "BUY", active: true);
            // SellTab
            BuildTabButton(go.transform, "SellTab", "SELL", active: false);
        }

        private static void BuildTabButton(Transform parent, string name, string label, bool active)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            UIAutoBindHelper.EnsureLayoutElement(go, prefW: 90, prefH: 32);

            var img = go.GetComponent<Image>();
            img.sprite = LoadShopSprite(active ? "TabButtonActive.png" : "TabButton.png");
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            img.raycastTarget = true;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(labelRt);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.font = FontLabel();
            labelTmp.fontSize = 11;
            labelTmp.color = active ? new Color(0.05f, 0.05f, 0.08f, 1f) : new Color(0.83f, 0.63f, 0.24f, 0.8f);
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.raycastTarget = false;
        }

        // =========================================================
        // GoldBar
        // =========================================================
        private static void BuildGoldBar(Transform parent)
        {
            var go = new GameObject("GoldBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            UIAutoBindHelper.EnsureLayoutElement(go, flexW: 1, prefH: 38);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.05f, 0.04f, 0.02f, 0.7f);
            img.raycastTarget = false;

            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(18, 18, 6, 6);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // "✦ GOLD ✦" 라벨
            var labelGo = new GameObject("GoldLabel", typeof(RectTransform), typeof(CanvasRenderer));
            labelGo.transform.SetParent(go.transform, false);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = "✦ GOLD ✦";
            labelTmp.font = FontLabel();
            labelTmp.fontSize = 11;
            labelTmp.color = new Color(0.55f, 0.43f, 0.18f, 1f);
            labelTmp.alignment = TextAlignmentOptions.Left;
            labelTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(labelGo, flexW: 1, prefH: 22);

            // GoldValue
            var valueGo = new GameObject("GoldValue", typeof(RectTransform), typeof(CanvasRenderer));
            valueGo.transform.SetParent(go.transform, false);
            var valueTmp = valueGo.AddComponent<TextMeshProUGUI>();
            valueTmp.text = "0 G";
            valueTmp.font = FontLabel();
            valueTmp.fontSize = 22;
            valueTmp.color = new Color(0.9f, 0.78f, 0.31f, 1f);
            valueTmp.alignment = TextAlignmentOptions.Right;
            valueTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(valueGo, flexW: 1, prefH: 26);
        }

        // =========================================================
        // BuyContainer + SlotContainer (3×2 GridLayout)
        // =========================================================
        private static void BuildBuyContainer(Transform parent)
        {
            var go = new GameObject("BuyContainer", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UIAutoBindHelper.EnsureLayoutElement(go, flexW: 1, prefH: 380);

            var rt = go.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(rt);

            // SlotContainer (GridLayout)
            var slotGo = new GameObject("SlotContainer", typeof(RectTransform));
            slotGo.transform.SetParent(go.transform, false);
            var slotRt = slotGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(slotRt);

            var glg = slotGo.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(260, 120);
            glg.spacing = new Vector2(10, 10);
            glg.padding = new RectOffset(0, 0, 0, 0);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;
            glg.childAlignment = TextAnchor.UpperCenter;
        }

        // =========================================================
        // SellContainer (초기 비활성)
        // =========================================================
        private static void BuildSellContainer(Transform parent)
        {
            var go = new GameObject("SellContainer", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UIAutoBindHelper.EnsureLayoutElement(go, flexW: 1, prefH: 380);
            var rt = go.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(rt);

            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 8;
            vlg.padding = new RectOffset(0, 0, 8, 8);

            // Hint TMP
            var hintGo = new GameObject("Hint", typeof(RectTransform), typeof(CanvasRenderer));
            hintGo.transform.SetParent(go.transform, false);
            var hintTmp = hintGo.AddComponent<TextMeshProUGUI>();
            hintTmp.text = "판매할 수 있는 유물이 없습니다.";
            hintTmp.font = FontItalic();
            hintTmp.fontStyle = FontStyles.Italic;
            hintTmp.fontSize = 13;
            hintTmp.color = new Color(0.65f, 0.6f, 0.45f, 0.8f);
            hintTmp.alignment = TextAlignmentOptions.Center;
            hintTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(hintGo, flexW: 1, prefH: 24);

            // SellSlotContainer
            var sellSlotGo = new GameObject("SellSlotContainer", typeof(RectTransform));
            sellSlotGo.transform.SetParent(go.transform, false);
            var sellSlotVlg = sellSlotGo.AddComponent<VerticalLayoutGroup>();
            sellSlotVlg.childControlWidth = true;
            sellSlotVlg.childControlHeight = true;
            sellSlotVlg.childForceExpandWidth = true;
            sellSlotVlg.childForceExpandHeight = false;
            sellSlotVlg.spacing = 6;
            UIAutoBindHelper.EnsureLayoutElement(sellSlotGo, flexW: 1, prefH: 300);

            go.SetActive(false);
        }

        // =========================================================
        // Footer
        // =========================================================
        private static void BuildFooter(Transform parent)
        {
            var go = new GameObject("Footer", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            UIAutoBindHelper.EnsureLayoutElement(go, flexW: 1, prefH: 40);

            // Hint
            var hintGo = new GameObject("Hint", typeof(RectTransform), typeof(CanvasRenderer));
            hintGo.transform.SetParent(go.transform, false);
            var hintTmp = hintGo.AddComponent<TextMeshProUGUI>();
            hintTmp.text = "\"모든 거래는 신성한 서약이다.\"";
            hintTmp.font = FontItalic();
            hintTmp.fontStyle = FontStyles.Italic;
            hintTmp.fontSize = 12;
            hintTmp.color = new Color(0.55f, 0.43f, 0.18f, 0.8f);
            hintTmp.alignment = TextAlignmentOptions.Left;
            hintTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(hintGo, flexW: 1, prefH: 22);

            // LeaveButton
            var btnGo = new GameObject("LeaveButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(go.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(btnGo, prefW: 120, prefH: 32);

            var btnImg = btnGo.GetComponent<Image>();
            btnImg.sprite = LoadShopSprite("LeaveButton.png");
            btnImg.type = Image.Type.Sliced;
            btnImg.color = Color.white;
            btnImg.raycastTarget = true;

            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;

            var btnLabelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            btnLabelGo.transform.SetParent(btnGo.transform, false);
            var btnLabelRt = btnLabelGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(btnLabelRt);
            var btnLabelTmp = btnLabelGo.AddComponent<TextMeshProUGUI>();
            btnLabelTmp.text = "LEAVE";
            btnLabelTmp.font = FontLabel();
            btnLabelTmp.fontSize = 12;
            btnLabelTmp.color = new Color(0.9f, 0.78f, 0.31f, 1f);
            btnLabelTmp.alignment = TextAlignmentOptions.Center;
            btnLabelTmp.raycastTarget = false;
        }
    }
}
#endif
