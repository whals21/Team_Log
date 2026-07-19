#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI;
using TeamLog.UI.Event;

namespace TeamLog.Editor
{
    /// <summary>
    /// EventSceneReworkBuilder 파트 — UI 부품 생성.
    /// ★ MapSceneReworkBuilder.Parts와 동일 패턴 (anchor 기반 직접 배치).
    /// </summary>
    public static partial class EventSceneReworkBuilder
    {
        // =========================================================
        // DimBackground — 전체 화면 어둠 오버레이 (이벤트 카드 뒤)
        // =========================================================
        private static void BuildDimBackground(Transform parent)
        {
            var go = new GameObject("DimBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(rt);

            var img = go.GetComponent<Image>();
            var sprite = LoadEventSprite("DimBackground.png");
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Simple;
            }
            img.color = Color.white;
            img.raycastTarget = true; // 배경 클릭 시 닫기 방지 (UI가 이벤트 받음)
        }

        // =========================================================
        // GlassFrame — 중앙 카드 컨테이너 (720x720, 가운데 정렬)
        // =========================================================
        private static void BuildGlassFrame(Transform parent)
        {
            var go = new GameObject("GlassFrame", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            // 중앙 정렬 + 720x720 고정 크기
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(720f, 760f);
            rt.anchoredPosition = Vector2.zero;

            // GlassFrame 내부에 GlassWindow(상단) + GlassPanel(하단) 배치
            BuildGlassWindow(go.transform);    // 상단 320px (스테인드글라스)
            BuildGlassPanel(go.transform);     // 하단 440px (텍스트+선택지)
            BuildCloseButton(go.transform);    // 우측 상단 X 버튼
        }

        // =========================================================
        // GlassWindow — 상단 스테인드글라스 (720x320)
        // =========================================================
        private static void BuildGlassWindow(Transform parent)
        {
            var go = new GameObject("GlassWindow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            // 상단 고정, 320px 높이
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 320f);
            rt.anchoredPosition = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.sprite = LoadEventSprite("GlassWindow_Shrine.png"); // 기본 — 런타임에 View가 EventType별로 교체
            img.type = Image.Type.Simple;
            img.color = Color.white;
            img.raycastTarget = false;

            // Emblem 자식 — 중앙 엠블럼 기호 (Cinzel Black 140pt)
            var emblemGo = new GameObject("Emblem", typeof(RectTransform), typeof(CanvasRenderer));
            emblemGo.transform.SetParent(go.transform, false);
            var emblemRt = emblemGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(emblemRt);
            var emblemTmp = emblemGo.AddComponent<TextMeshProUGUI>();
            emblemTmp.text = "✦";
            emblemTmp.font = FontTitle();
            emblemTmp.fontSize = 140;
            emblemTmp.fontStyle = FontStyles.Bold;
            emblemTmp.color = Color.white;
            emblemTmp.alignment = TextAlignmentOptions.Center;
            emblemTmp.raycastTarget = false;

            // Outline 효과 (엠블럼 강조)
            var outline = emblemGo.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.7f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        // =========================================================
        // GlassPanel — 하단 텍스트+선택지 영역 (720x440)
        // =========================================================
        private static void BuildGlassPanel(Transform parent)
        {
            var go = new GameObject("GlassPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            // 상단은 GlassWindow(320) 아래, 하단은 끝까지
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.offsetMin = new Vector2(0f, 0f);
            rt.offsetMax = new Vector2(0f, -320f); // 상단 320px은 GlassWindow가 차지
            rt.anchoredPosition = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.sprite = LoadEventSprite("PanelBackground.png");
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            img.raycastTarget = false;

            // VLG — 자식 세로 배치
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(40, 40, 24, 32);
            vlg.spacing = 14;
            vlg.childAlignment = TextAnchor.UpperCenter;

            // 1. TopBar — HLG (ThemeTag / EventTypeTag)
            BuildTopBar(go.transform);

            // 2. EventTitle — 메인 타이틀 (Cinzel Black 32pt)
            var titleGo = new GameObject("EventTitle", typeof(RectTransform), typeof(CanvasRenderer));
            titleGo.transform.SetParent(go.transform, false);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "Crimson Fountain";
            titleTmp.font = FontTitle();
            titleTmp.fontSize = 32;
            titleTmp.color = Color.white;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(titleGo, flexW: 1, prefH: 42);

            // 3. Narrative — 이야기 묘사 (Cormorant Italic 16pt)
            var narrGo = new GameObject("Narrative", typeof(RectTransform), typeof(CanvasRenderer));
            narrGo.transform.SetParent(go.transform, false);
            var narrTmp = narrGo.AddComponent<TextMeshProUGUI>();
            narrTmp.text = "붉은 물이 흐르는 분수대가 당신 앞에 솟아 있다...";
            narrTmp.font = FontItalic();
            narrTmp.fontStyle = FontStyles.Italic;
            narrTmp.fontSize = 16;
            narrTmp.color = new Color(0.84f, 0.78f, 0.65f, 1f); // parchment
            narrTmp.alignment = TextAlignmentOptions.Center;
            narrTmp.raycastTarget = false;
            narrTmp.enableWordWrapping = true;
            UIAutoBindHelper.EnsureLayoutElement(narrGo, flexW: 1, prefH: 80);

            // 4. ChoiceContainer — VLG (선택지 행들)
            var choicesGo = new GameObject("ChoiceContainer", typeof(RectTransform));
            choicesGo.transform.SetParent(go.transform, false);
            var choiceVlg = choicesGo.AddComponent<VerticalLayoutGroup>();
            choiceVlg.childControlWidth = true;
            choiceVlg.childControlHeight = true;
            choiceVlg.childForceExpandWidth = true;
            choiceVlg.childForceExpandHeight = false;
            choiceVlg.spacing = 8;
            choiceVlg.childAlignment = TextAnchor.UpperCenter;
            var fitter = choicesGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            UIAutoBindHelper.EnsureLayoutElement(choicesGo, flexW: 1, prefH: 140);

            // 5. ResultPanel — 초기 비활성
            BuildResultPanel(go.transform);
        }

        // =========================================================
        // TopBar — HLG (ThemeTag / EventTypeTag)
        // =========================================================
        private static void BuildTopBar(Transform parent)
        {
            var go = new GameObject("TopBar", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            UIAutoBindHelper.EnsureLayoutElement(go, flexW: 1, prefH: 22);

            // ThemeTag — 좌측
            var themeGo = new GameObject("ThemeTag", typeof(RectTransform), typeof(CanvasRenderer));
            themeGo.transform.SetParent(go.transform, false);
            var themeTmp = themeGo.AddComponent<TextMeshProUGUI>();
            themeTmp.text = "— Crimson Chapel · L4 —";
            themeTmp.font = FontItalic();
            themeTmp.fontStyle = FontStyles.Italic;
            themeTmp.fontSize = 11;
            themeTmp.color = new Color(0.65f, 0.6f, 0.45f, 1f);
            themeTmp.alignment = TextAlignmentOptions.Left;
            themeTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(themeGo, flexW: 1, prefH: 20);

            // EventTypeTag — 우측
            var typeGo = new GameObject("EventTypeTag", typeof(RectTransform), typeof(CanvasRenderer));
            typeGo.transform.SetParent(go.transform, false);
            var typeTmp = typeGo.AddComponent<TextMeshProUGUI>();
            typeTmp.text = "✦  SHRINE";
            typeTmp.font = FontLabel();
            typeTmp.fontSize = 11;
            typeTmp.color = new Color(0.61f, 0.78f, 0.77f, 1f);
            typeTmp.alignment = TextAlignmentOptions.Right;
            typeTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(typeGo, flexW: 1, prefH: 20);
        }

        // =========================================================
        // ResultPanel — 결과 표시 (초기 비활성)
        // =========================================================
        private static void BuildResultPanel(Transform parent)
        {
            var go = new GameObject("ResultPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            UIAutoBindHelper.EnsureLayoutElement(go, flexW: 1, prefH: 180);

            var img = go.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.5f);
            img.raycastTarget = false;

            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.spacing = 14;
            vlg.childAlignment = TextAnchor.MiddleCenter;

            // ResultText — 결과 설명
            var textGo = new GameObject("ResultText", typeof(RectTransform), typeof(CanvasRenderer));
            textGo.transform.SetParent(go.transform, false);
            var textTmp = textGo.AddComponent<TextMeshProUGUI>();
            textTmp.text = "결과 텍스트가 여기 표시됩니다.";
            textTmp.font = FontItalic();
            textTmp.fontStyle = FontStyles.Italic;
            textTmp.fontSize = 16;
            textTmp.color = Color.white;
            textTmp.alignment = TextAlignmentOptions.Center;
            textTmp.raycastTarget = false;
            textTmp.enableWordWrapping = true;
            UIAutoBindHelper.EnsureLayoutElement(textGo, flexW: 1, prefH: 90);

            // ResultConfirmButton — 확인/계속
            var btnGo = new GameObject("ResultConfirmButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(go.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(btnGo, flexW: 1, prefH: 40);

            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = new Color(0.2f, 0.05f, 0.05f, 1f); // 핏빛
            btnImg.raycastTarget = true;

            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;

            var btnLabelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            btnLabelGo.transform.SetParent(btnGo.transform, false);
            var btnLabelRt = btnLabelGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(btnLabelRt);
            var btnLabelTmp = btnLabelGo.AddComponent<TextMeshProUGUI>();
            btnLabelTmp.text = "확인";
            btnLabelTmp.font = FontLabel();
            btnLabelTmp.fontSize = 13;
            btnLabelTmp.color = new Color(0.9f, 0.78f, 0.31f, 1f); // gold light
            btnLabelTmp.alignment = TextAlignmentOptions.Center;
            btnLabelTmp.raycastTarget = false;

            // 초기 비활성
            go.SetActive(false);
        }

        // =========================================================
        // CloseButton — 우측 상단 X (결과 패널 표시 중에만 동작)
        // =========================================================
        private static void BuildCloseButton(Transform parent)
        {
            var go = new GameObject("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            // 우측 상단 고정
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(32f, 32f);
            rt.anchoredPosition = new Vector2(-12f, -12f);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.05f, 0.05f, 0.08f, 0.7f);
            img.raycastTarget = true;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;

            // X 기호 자식
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(labelRt);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = "✕";
            labelTmp.font = FontLabel();
            labelTmp.fontSize = 18;
            labelTmp.color = new Color(0.83f, 0.63f, 0.24f, 0.8f);
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.raycastTarget = false;
        }
    }
}
#endif
