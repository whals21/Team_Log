using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using TeamLog.UI.Battle;

namespace TeamLog.Editor
{
    /// <summary>
    /// Battle UI 씬 빌더 — 오버레이 UI 생성 (CharacterPopup, BattleEndOverlay, TooltipUI)
    /// 진입점+TopBar+BottomBar+유틸리티: BattleUISceneBuilder.UI.cs
    /// 사이드바: BattleUISceneBuilder.UI.Sidebar.cs
    /// </summary>
    public partial class BattleUISceneBuilder
    {
        // ══════════════════════════════════════════════════════════
        //  Character Popup
        // ══════════════════════════════════════════════════════════

        private static void CreateCharacterPopup(RectTransform parent)
        {
            // ── 오버레이 (전체 화면 반투명 배경, 클릭으로 닫기) ──
            var overlay = NewRect("CharacterPopup", parent);
            SetFillParent(overlay);
            overlay.gameObject.SetActive(false);

            var bgBtn = overlay.gameObject.AddComponent<Button>();
            var bgImg = overlay.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.6f);
            bgBtn.targetGraphic = bgImg;

            overlay.gameObject.AddComponent<CharacterPopupUI>();

            // ── 패널 (고정 크기 520×620, 중앙, VerticalLayoutGroup으로 자동 배치) ──
            var panel = NewRect("Panel", overlay);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(520, 620);
            var panelImg = panel.gameObject.AddComponent<Image>();
            panelImg.color = PopupPanelBg;
            var panelOl = panel.gameObject.AddComponent<Outline>();
            panelOl.effectColor = BorderRed;
            panelOl.effectDistance = new Vector2(2, -2);

            // VerticalLayoutGroup: 자식들을 위에서부터 순서대로 자동 배치
            var panelVlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            panelVlg.padding = new RectOffset(12, 12, 8, 8);
            panelVlg.spacing = 4;
            panelVlg.childAlignment = TextAnchor.UpperCenter;
            panelVlg.childControlWidth = true;
            panelVlg.childControlHeight = true;
            panelVlg.childForceExpandWidth = true;
            panelVlg.childForceExpandHeight = false;

            CreatePopupHeader(panel);
            CreatePopupHPBar(panel);
            CreatePopupStats(panel);
            CreatePopupTabs(panel);
            CreatePopupContent(panel);
        }

        /// <summary>
        /// LayoutElement로 높이를 지정하는 헬퍼 (VerticalLayoutGroup용)
        /// </summary>
        private static LayoutElement SetFixedHeight(RectTransform rect, float height)
        {
            var le = rect.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0;
            return le;
        }

        private static void CreatePopupHeader(RectTransform panel)
        {
            var header = NewRect("Header", panel);
            SetFixedHeight(header, 56);
            header.gameObject.AddComponent<Image>().color = PopupHeaderBg;

            var portrait = NewRect("Portrait", header);
            portrait.anchorMin = new Vector2(0, 0.5f);
            portrait.anchorMax = new Vector2(0, 0.5f);
            portrait.pivot = new Vector2(0, 0.5f);
            portrait.anchoredPosition = new Vector2(12, 0);
            portrait.sizeDelta = new Vector2(44, 44);
            portrait.gameObject.AddComponent<Image>().color = AccentRed;

            var nameRect = NewRect("Name", header);
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.offsetMin = new Vector2(64, 0);
            nameRect.offsetMax = new Vector2(-44, -4);
            AddText(nameRect, "캐릭터명", 20, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);

            var classRect = NewRect("Class", header);
            classRect.anchorMin = new Vector2(0, 0);
            classRect.anchorMax = new Vector2(1, 0.5f);
            classRect.offsetMin = new Vector2(64, 4);
            classRect.offsetMax = new Vector2(-44, 0);
            AddText(classRect, "클래스", 13, FontStyles.Normal, TextAlignmentOptions.Left, TextDim);

            var closeBtn = NewRect("CloseBtn", header);
            closeBtn.anchorMin = new Vector2(1, 0.5f);
            closeBtn.anchorMax = new Vector2(1, 0.5f);
            closeBtn.pivot = new Vector2(1, 0.5f);
            closeBtn.anchoredPosition = new Vector2(-8, 0);
            closeBtn.sizeDelta = new Vector2(32, 32);
            var cb = closeBtn.gameObject.AddComponent<Button>();
            var cbImg = closeBtn.gameObject.AddComponent<Image>();
            cbImg.color = new Color(0.5f, 0.1f, 0.1f, 0.8f);
            cb.targetGraphic = cbImg;
            var cbLabel = NewRect("X", closeBtn);
            SetFillParent(cbLabel);
            AddText(cbLabel, "X", 16, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
        }

        private static void CreatePopupHPBar(RectTransform panel)
        {
            var hpArea = NewRect("HPArea", panel);
            SetFixedHeight(hpArea, 28);

            var hpBg = NewRect("HPBarBg", hpArea);
            SetFillParent(hpBg);
            hpBg.offsetMax = new Vector2(-80, 0);
            hpBg.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

            var hpFill = NewRect("Fill", hpBg);
            hpFill.anchorMin = Vector2.zero;
            hpFill.anchorMax = new Vector2(1f, 1f);
            hpFill.offsetMin = new Vector2(2, 2);
            hpFill.offsetMax = new Vector2(-2, -2);
            hpFill.gameObject.AddComponent<Image>().color = AccentGreen;

            var hpLabel = NewRect("HPText", hpArea);
            hpLabel.anchorMin = new Vector2(1, 0);
            hpLabel.anchorMax = new Vector2(1, 1);
            hpLabel.pivot = new Vector2(1, 0.5f);
            hpLabel.offsetMin = new Vector2(-76, 0);
            hpLabel.offsetMax = Vector2.zero;
            AddText(hpLabel, "HP 55/55", 14, FontStyles.Bold, TextAlignmentOptions.Right, TextWhite);
        }

        private static void CreatePopupStats(RectTransform panel)
        {
            var statsArea = NewRect("StatsArea", panel);
            SetFixedHeight(statsArea, 24);

            var atkRect = NewRect("ATK", statsArea);
            atkRect.anchorMin = Vector2.zero;
            atkRect.anchorMax = new Vector2(0.5f, 1);
            atkRect.offsetMin = Vector2.zero;
            atkRect.offsetMax = new Vector2(-4, 0);
            atkRect.gameObject.AddComponent<Image>().color = EntryBg;
            AddText(NewRect("T", atkRect), "ATK 10", 14, FontStyles.Bold, TextAlignmentOptions.Center, AccentRed);

            var defRect = NewRect("DEF", statsArea);
            defRect.anchorMin = new Vector2(0.5f, 0);
            defRect.anchorMax = Vector2.one;
            defRect.offsetMin = new Vector2(4, 0);
            defRect.offsetMax = Vector2.zero;
            defRect.gameObject.AddComponent<Image>().color = EntryBg;
            AddText(NewRect("T", defRect), "DEF 5", 14, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.3f, 0.6f, 0.9f));
        }

        private static void CreatePopupTabs(RectTransform panel)
        {
            var tabArea = NewRect("TabArea", panel);
            SetFixedHeight(tabArea, 36);
            tabArea.gameObject.AddComponent<Image>().color = new Color(0.03f, 0.03f, 0.06f, 0.5f);

            // 탭1: 스킬 목록
            var tab1 = NewRect("TabSkill", tabArea);
            tab1.anchorMin = Vector2.zero;
            tab1.anchorMax = new Vector2(0.5f, 1);
            tab1.offsetMin = Vector2.zero;
            tab1.offsetMax = Vector2.zero;
            tab1.gameObject.AddComponent<Button>();
            tab1.gameObject.AddComponent<Image>().color = Color.clear;
            var t1Label = NewRect("T", tab1);
            SetFillParent(t1Label);
            AddText(t1Label, "스킬 목록", 14, FontStyles.Bold, TextAlignmentOptions.Center, AccentYellow);

            var t1Ind = NewRect("Indicator", tab1);
            t1Ind.anchorMin = new Vector2(0, 0);
            t1Ind.anchorMax = new Vector2(1, 0);
            t1Ind.pivot = new Vector2(0.5f, 0);
            t1Ind.sizeDelta = new Vector2(0, 2);
            t1Ind.gameObject.AddComponent<Image>().color = AccentYellow;

            // 탭2: 상태 효과
            var tab2 = NewRect("TabStatus", tabArea);
            tab2.anchorMin = new Vector2(0.5f, 0);
            tab2.anchorMax = Vector2.one;
            tab2.offsetMin = Vector2.zero;
            tab2.offsetMax = Vector2.zero;
            tab2.gameObject.AddComponent<Button>();
            tab2.gameObject.AddComponent<Image>().color = Color.clear;
            var t2Label = NewRect("T", tab2);
            SetFillParent(t2Label);
            AddText(t2Label, "상태 효과", 14, FontStyles.Normal, TextAlignmentOptions.Center, TextDim);

            var t2Ind = NewRect("Indicator", tab2);
            t2Ind.anchorMin = new Vector2(0, 0);
            t2Ind.anchorMax = new Vector2(1, 0);
            t2Ind.pivot = new Vector2(0.5f, 0);
            t2Ind.sizeDelta = new Vector2(0, 2);
            t2Ind.gameObject.AddComponent<Image>().color = AccentYellow;
            t2Ind.gameObject.SetActive(false);
        }

        private static void CreatePopupContent(RectTransform panel)
        {
            // ── 스킬 콘텐츠 (flexibleHeight=1로 남은 공간 자동 채움) ──
            var skillContent = NewRect("SkillContent", panel);
            var skillLe = skillContent.gameObject.AddComponent<LayoutElement>();
            skillLe.minHeight = 100;
            skillLe.flexibleHeight = 1;

            skillContent.gameObject.AddComponent<RectMask2D>();
            var skillScroll = skillContent.gameObject.AddComponent<ScrollRect>();
            skillScroll.horizontal = false;
            skillScroll.vertical = true;
            skillScroll.scrollSensitivity = 20;
            skillScroll.movementType = ScrollRect.MovementType.Elastic;

            // 내부 Content: top-anchored, sizeDelta=(0,0)
            var skillList = NewRect("Content", skillContent);
            skillList.anchorMin = new Vector2(0, 1);
            skillList.anchorMax = new Vector2(1, 1);
            skillList.pivot = new Vector2(0.5f, 1);
            skillList.sizeDelta = new Vector2(0, 0);

            var skillVlg = skillList.gameObject.AddComponent<VerticalLayoutGroup>();
            skillVlg.spacing = 6;
            skillVlg.padding = new RectOffset(0, 0, 4, 4);
            skillVlg.childAlignment = TextAnchor.UpperCenter;
            skillVlg.childControlWidth = true;
            skillVlg.childControlHeight = false;
            skillVlg.childForceExpandWidth = true;
            skillVlg.childForceExpandHeight = false;

            var skillCsf = skillList.gameObject.AddComponent<ContentSizeFitter>();
            skillCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            skillCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            skillScroll.content = skillList;

            // ── 상태 효과 콘텐츠 (숨김, 동일 구조) ──
            var statusContent = NewRect("StatusContent", panel);
            var statusLe = statusContent.gameObject.AddComponent<LayoutElement>();
            statusLe.minHeight = 100;
            statusLe.flexibleHeight = 1;
            statusContent.gameObject.SetActive(false);

            statusContent.gameObject.AddComponent<RectMask2D>();
            var statusScroll = statusContent.gameObject.AddComponent<ScrollRect>();
            statusScroll.horizontal = false;
            statusScroll.vertical = true;
            statusScroll.scrollSensitivity = 20;
            statusScroll.movementType = ScrollRect.MovementType.Elastic;

            var statusList = NewRect("Content", statusContent);
            statusList.anchorMin = new Vector2(0, 1);
            statusList.anchorMax = new Vector2(1, 1);
            statusList.pivot = new Vector2(0.5f, 1);
            statusList.sizeDelta = new Vector2(0, 0);

            var statusVlg = statusList.gameObject.AddComponent<VerticalLayoutGroup>();
            statusVlg.spacing = 6;
            statusVlg.padding = new RectOffset(0, 0, 4, 4);
            statusVlg.childAlignment = TextAnchor.UpperCenter;
            statusVlg.childControlWidth = true;
            statusVlg.childControlHeight = false;
            statusVlg.childForceExpandWidth = true;
            statusVlg.childForceExpandHeight = false;

            var statusCsf = statusList.gameObject.AddComponent<ContentSizeFitter>();
            statusCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            statusCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            statusScroll.content = statusList;
        }

        // ══════════════════════════════════════════════════════════
        //  Battle End Overlay
        // ══════════════════════════════════════════════════════════

        private static void CreateBattleEndOverlay(RectTransform parent)
        {
            var overlay = NewRect("BattleEndOverlay", parent);
            SetFillParent(overlay);
            var bgImg = overlay.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.85f);
            bgImg.raycastTarget = true;
            overlay.gameObject.SetActive(false);

            // 중앙 컨테이너
            var container = NewRect("Container", overlay);
            container.anchorMin = new Vector2(0.25f, 0.25f);
            container.anchorMax = new Vector2(0.75f, 0.75f);
            container.offsetMin = Vector2.zero;
            container.offsetMax = Vector2.zero;
            var containerBg = container.gameObject.AddComponent<Image>();
            var borderSprite = LoadSprite(SPRITE_CARD_BORDER);
            if (borderSprite != null)
            {
                containerBg.sprite = borderSprite;
                Set9Slice(containerBg);
            }
            else
                containerBg.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);

            // CanvasGroup 추가 (애니메이션용)
            container.gameObject.AddComponent<CanvasGroup>();

            // 결과 텍스트
            var resultRect = NewRect("ResultText", container);
            resultRect.anchorMin = new Vector2(0f, 0.45f);
            resultRect.anchorMax = new Vector2(1f, 0.85f);
            resultRect.offsetMin = Vector2.zero;
            resultRect.offsetMax = Vector2.zero;
            var resultT = resultRect.gameObject.AddComponent<TextMeshProUGUI>();
            resultT.font = GetOrCreateKoreanFont();
            resultT.text = "승리!";
            resultT.fontSize = 64;
            resultT.fontStyle = FontStyles.Bold;
            resultT.alignment = TextAlignmentOptions.Center;
            resultT.color = AccentYellow;

            // 계속하기 버튼
            var btnRect = NewRect("ContinueButton", container);
            btnRect.anchorMin = new Vector2(0.2f, 0.08f);
            btnRect.anchorMax = new Vector2(0.8f, 0.35f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            var btnImg = btnRect.gameObject.AddComponent<Image>();
            btnImg.color = AccentRed;
            var btn = btnRect.gameObject.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var btnLabelRect = NewRect("Label", btnRect);
            SetFillParent(btnLabelRect);
            var btnLabel = btnLabelRect.gameObject.AddComponent<TextMeshProUGUI>();
            btnLabel.font = GetOrCreateKoreanFont();
            btnLabel.text = "계속하기";
            btnLabel.fontSize = 32;
            btnLabel.fontStyle = FontStyles.Bold;
            btnLabel.alignment = TextAlignmentOptions.Center;
            btnLabel.color = TextWhite;

            // BattleEndOverlay 컴포넌트
            var endOverlay = overlay.gameObject.AddComponent<BattleEndOverlay>();
            var ser = new UnityEditor.SerializedObject(endOverlay);
            var resultProp = ser.FindProperty("_resultText");
            if (resultProp != null) resultProp.objectReferenceValue = resultT;
            var btnProp = ser.FindProperty("_continueButton");
            if (btnProp != null) btnProp.objectReferenceValue = btn;
            var labelProp = ser.FindProperty("_continueLabel");
            if (labelProp != null) labelProp.objectReferenceValue = btnLabel;
            ser.ApplyModifiedProperties();
        }

        // ══════════════════════════════════════════════════════════
        //  Tooltip UI
        // ══════════════════════════════════════════════════════════

        private static void CreateTooltipUI(RectTransform parent)
        {
            var tooltip = NewRect("TooltipUI", parent);
            tooltip.sizeDelta = new Vector2(280, 0);
            tooltip.pivot = new Vector2(0, 1);

            var tooltipBg = tooltip.gameObject.AddComponent<Image>();
            tooltipBg.color = new Color(0.02f, 0.02f, 0.06f, 0.95f);
            tooltipBg.raycastTarget = false;

            var outline = tooltip.gameObject.AddComponent<Outline>();
            outline.effectColor = AccentYellow;
            outline.effectDistance = new Vector2(1, -1);

            var vlg = tooltip.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 8, 8);
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = tooltip.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var title = NewRect("Title", tooltip);
            var tt = title.gameObject.AddComponent<TextMeshProUGUI>();
            tt.font = GetOrCreateKoreanFont();
            tt.text = "";
            tt.fontSize = 15;
            tt.fontStyle = FontStyles.Bold;
            tt.alignment = TextAlignmentOptions.Left;
            tt.color = AccentYellow;
            tt.raycastTarget = false;

            // Subtitle: 비용/타입/타겟 (필요시에만 표시)
            var subtitle = NewRect("Subtitle", tooltip);
            var st = subtitle.gameObject.AddComponent<TextMeshProUGUI>();
            st.font = GetOrCreateKoreanFont();
            st.text = "";
            st.fontSize = 11;
            st.fontStyle = FontStyles.Normal;
            st.alignment = TextAlignmentOptions.Left;
            st.color = new Color(0.7f, 0.7f, 0.8f, 0.9f);
            st.raycastTarget = false;
            st.enableWordWrapping = false;
            subtitle.gameObject.SetActive(false);

            // 구분선
            var div = NewRect("Divider", tooltip);
            var divLe = div.gameObject.AddComponent<LayoutElement>();
            divLe.preferredHeight = 1;
            divLe.flexibleHeight = 0;
            div.gameObject.AddComponent<Image>().color = new Color(0.4f, 0.4f, 0.5f, 0.5f);

            var desc = NewRect("Desc", tooltip);
            var dt = desc.gameObject.AddComponent<TextMeshProUGUI>();
            dt.font = GetOrCreateKoreanFont();
            dt.text = "";
            dt.fontSize = 13;
            dt.alignment = TextAlignmentOptions.Left;
            dt.color = TextWhite;
            dt.enableWordWrapping = true;
            dt.raycastTarget = false;

            tooltip.gameObject.AddComponent<TooltipUI>();
            tooltip.gameObject.SetActive(false);
        }

        // ══════════════════════════════════════════════════════════
        //  Party Status Overlay (토글 오버레이 — 좌측 상단)
        // ══════════════════════════════════════════════════════════

        private static void CreatePartyStatusOverlay(RectTransform parent)
        {
            var overlay = NewRect("PartyStatusOverlay", parent);
            overlay.anchorMin = new Vector2(0, 0.4f);
            overlay.anchorMax = new Vector2(0.22f, 1f);
            overlay.offsetMin = new Vector2(12, 12);
            overlay.offsetMax = new Vector2(-4, -56);
            overlay.gameObject.SetActive(false);

            var bg = overlay.gameObject.AddComponent<Image>();
            bg.color = PanelBgNavy;
            var ol = overlay.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(0.15f, 0.6f, 0.25f, 0.8f);
            ol.effectDistance = new Vector2(2, -2);
            overlay.gameObject.AddComponent<CanvasGroup>();

            var vlg = overlay.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 10, 10);
            vlg.spacing = 6;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 타이틀
            var title = NewRect("Title", overlay);
            SetFixedHeight(title, 24);
            AddText(title, "파티 상태", 14, FontStyles.Bold, TextAlignmentOptions.Left, AccentYellow);

            // HP
            var hpRect = NewRect("HPText", overlay);
            SetFixedHeight(hpRect, 22);
            AddText(hpRect, "HP 288/370", 14, FontStyles.Bold, TextAlignmentOptions.Left, AccentRed);

            // 골드
            var goldRect = NewRect("GoldText", overlay);
            SetFixedHeight(goldRect, 22);
            AddText(goldRect, "골드 142G", 14, FontStyles.Bold, TextAlignmentOptions.Left, AccentYellow);

            // 층
            var floorRect = NewRect("FloorText", overlay);
            SetFixedHeight(floorRect, 22);
            AddText(floorRect, "F1 일반", 13, FontStyles.Normal, TextAlignmentOptions.Left, TextDim);

            // PartyStatusWidget 컴포넌트
            var widget = overlay.gameObject.AddComponent<PartyStatusWidget>();
            var ser = new SerializedObject(widget);
            var hpProp = ser.FindProperty("_hpText");
            if (hpProp != null) hpProp.objectReferenceValue = hpRect.GetComponent<TextMeshProUGUI>();
            var goldProp = ser.FindProperty("_goldText");
            if (goldProp != null) goldProp.objectReferenceValue = goldRect.GetComponent<TextMeshProUGUI>();
            var floorProp = ser.FindProperty("_floorText");
            if (floorProp != null) floorProp.objectReferenceValue = floorRect.GetComponent<TextMeshProUGUI>();
            ser.ApplyModifiedProperties();
        }

        // ══════════════════════════════════════════════════════════
        //  Battle Log Overlay (토글 오버레이 — 우측 상단)
        // ══════════════════════════════════════════════════════════

        private static void CreateBattleLogOverlay(RectTransform parent)
        {
            var overlay = NewRect("BattleLogOverlay", parent);
            overlay.anchorMin = new Vector2(0.78f, 0.4f);
            overlay.anchorMax = new Vector2(1f, 1f);
            overlay.offsetMin = new Vector2(4, 12);
            overlay.offsetMax = new Vector2(-12, -56);
            overlay.gameObject.SetActive(false);

            var bg = overlay.gameObject.AddComponent<Image>();
            bg.color = PanelBgNavy;
            var ol = overlay.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(0.5f, 0.4f, 0.7f, 0.8f);
            ol.effectDistance = new Vector2(2, -2);
            overlay.gameObject.AddComponent<CanvasGroup>();

            var vlg = overlay.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 8, 8);
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 타이틀
            var title = NewRect("Title", overlay);
            SetFixedHeight(title, 22);
            AddText(title, "전투 로그", 13, FontStyles.Bold, TextAlignmentOptions.Left, new Color(0.7f, 0.6f, 0.85f));

            // Viewport + LogText (ScrollRect)
            var viewport = NewRect("Viewport", overlay);
            var vpLe = viewport.gameObject.AddComponent<LayoutElement>();
            vpLe.flexibleHeight = 1;
            viewport.gameObject.AddComponent<RectMask2D>();

            var log = NewRect("LogText", viewport);
            log.anchorMin = new Vector2(0, 1);
            log.anchorMax = new Vector2(1, 1);
            log.pivot = new Vector2(0.5f, 1);
            log.sizeDelta = new Vector2(0, 0);
            var lt = log.gameObject.AddComponent<TextMeshProUGUI>();
            lt.font = GetOrCreateKoreanFont();
            lt.text = "";
            lt.fontSize = 12;
            lt.alignment = TextAlignmentOptions.TopLeft;
            lt.color = TextDim;
            lt.enableWordWrapping = true;
            lt.overflowMode = TextOverflowModes.Overflow;
            lt.raycastTarget = false;

            var csf = log.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = overlay.gameObject.AddComponent<ScrollRect>();
            scrollRect.content = log;
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 20;

            // BattleLogUI 컴포넌트
            var logUI = overlay.gameObject.AddComponent<BattleLogUI>();
            var ser = new SerializedObject(logUI);
            var logTextProp = ser.FindProperty("_logText");
            if (logTextProp != null) logTextProp.objectReferenceValue = lt;
            var scrollProp = ser.FindProperty("_scrollRect");
            if (scrollProp != null) scrollProp.objectReferenceValue = scrollRect;
            ser.ApplyModifiedProperties();
        }

        // ══════════════════════════════════════════════════════════
        //  Discover Modal (Phase CC-2E — Cael Alchemist)
        //  하스스톤 발견 메커니즘: 스킬 시전 시 3-4개 카드 팝업
        // ══════════════════════════════════════════════════════════

        private static void CreateDiscoverModal(RectTransform parent)
        {
            // ── 최상위 오버레이 (sortingOrder=200으로 다른 UI 위에) ──
            var overlayGO = new GameObject("DiscoverModal",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
            var overlayRect = overlayGO.GetComponent<RectTransform>();
            overlayRect.SetParent(parent, false);
            SetFillParent(overlayRect);
            overlayGO.SetActive(false);

            // 별도 Canvas — 최상위 오버레이로 동작 (BattleUICanvas=100 위)
            var overlayCanvas = overlayGO.GetComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 200;
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayGO.GetComponent<GraphicRaycaster>();

            var overlayCg = overlayGO.GetComponent<CanvasGroup>();
            overlayCg.alpha = 0f;
            overlayCg.blocksRaycasts = true;
            overlayCg.interactable = true;

            // 반투명 어두운 배경 (클릭 차단용 Button)
            var bgBtn = overlayGO.AddComponent<Button>();
            var bgImg = overlayGO.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.6f);
            bgBtn.targetGraphic = bgImg;

            // ── 중앙 패널 ──
            var panel = NewRect("Panel", overlayRect);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(900, 360);
            var panelImg = panel.gameObject.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.12f, 0.98f);
            var panelOl = panel.gameObject.AddComponent<Outline>();
            panelOl.effectColor = new Color(0.85f, 0.65f, 0.20f, 0.9f); // 황금 테두리
            panelOl.effectDistance = new Vector2(3, -3);

            // 패널 내부 VerticalLayoutGroup — Title + CardContainer
            var panelVlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            panelVlg.padding = new RectOffset(20, 20, 14, 14);
            panelVlg.spacing = 12;
            panelVlg.childAlignment = TextAnchor.UpperCenter;
            panelVlg.childControlWidth = true;
            panelVlg.childControlHeight = false;
            panelVlg.childForceExpandWidth = true;
            panelVlg.childForceExpandHeight = false;

            // 타이틀
            var titleRect = NewRect("Title", panel);
            SetFixedHeight(titleRect, 36);
            var title = titleRect.gameObject.AddComponent<TextMeshProUGUI>();
            title.font = GetOrCreateKoreanFont();
            title.text = "발견";
            title.fontSize = 24;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color = AccentYellow;

            // 카드 컨테이너 (HorizontalLayoutGroup)
            var cardContainer = NewRect("CardContainer", panel);
            var containerLe = cardContainer.gameObject.AddComponent<LayoutElement>();
            containerLe.preferredHeight = 260;
            containerLe.flexibleHeight = 0;
            var hlg = cardContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(12, 12, 8, 8);
            hlg.spacing = 16;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // DiscoverCard 프리팹 생성
            CreateDiscoverCardPrefab();

            // 프리팹이 AssetDatabase에 인식되었는지 보장 — Refresh 후 로드
            AssetDatabase.Refresh();
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03.Data/Prefabs/DiscoverCard.prefab");
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03.Data/Prefabs/DiscoverCard.prefab");
            if (cardPrefab == null)
                Debug.LogError("[DiscoverModal] DiscoverCard.prefab 로드 실패 — 모달 카드가 빈 상태로 표시됩니다. Assets/03.Data/Prefabs/DiscoverCard.prefab 파일 확인 필요");

            // DiscoverModalUI 컴포넌트 추가 + 필드 와이어링
            var modalUI = overlayGO.AddComponent<DiscoverModalUI>();
            var ser = new SerializedObject(modalUI);
            var cardContainerProp = ser.FindProperty("_cardContainer");
            if (cardContainerProp != null) cardContainerProp.objectReferenceValue = cardContainer;
            var titleProp = ser.FindProperty("_titleLabel");
            if (titleProp != null) titleProp.objectReferenceValue = title;
            var bgBtnProp = ser.FindProperty("_backgroundButton");
            if (bgBtnProp != null) bgBtnProp.objectReferenceValue = bgBtn;
            var cgProp = ser.FindProperty("_canvasGroup");
            if (cgProp != null) cgProp.objectReferenceValue = overlayCg;

            if (cardPrefab != null)
            {
                var cardPrefabProp = ser.FindProperty("_discoverCardPrefab");
                if (cardPrefabProp != null) cardPrefabProp.objectReferenceValue = cardPrefab;
            }

            ser.ApplyModifiedProperties();
        }

        /// <summary>DiscoverCard 프리팹 생성 — RewardCard 패턴 차용.</summary>
        private static void CreateDiscoverCardPrefab()
        {
            const string prefabPath = "Assets/03.Data/Prefabs/DiscoverCard.prefab";
            if (!AssetDatabase.IsValidFolder("Assets/03.Data/Prefabs"))
                AssetDatabase.CreateFolder("Assets/03.Data", "Prefabs");
            AssetDatabase.DeleteAsset(prefabPath);

            var go = new GameObject("DiscoverCard");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 260);

            var layoutEl = go.AddComponent<LayoutElement>();
            layoutEl.preferredWidth = 200;
            layoutEl.preferredHeight = 260;
            layoutEl.flexibleWidth = 1;
            layoutEl.flexibleHeight = 1;

            // 배경 — 스킬 타입별 색상 (기본 어두운 남색)
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);
            var bgOutline = go.AddComponent<Outline>();
            bgOutline.effectColor = new Color(0.85f, 0.65f, 0.20f, 0.7f);
            bgOutline.effectDistance = new Vector2(2, -2);
            go.AddComponent<Button>();

            // VerticalLayoutGroup으로 자식 자동 배치
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 8, 8);
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 단축키 번호 (좌상단 배지)
            var shortcutBadge = NewRect("ShortcutBadge", rect);
            shortcutBadge.anchorMin = new Vector2(0, 1);
            shortcutBadge.anchorMax = new Vector2(0, 1);
            shortcutBadge.pivot = new Vector2(0, 1);
            shortcutBadge.anchoredPosition = new Vector2(4, -4);
            shortcutBadge.sizeDelta = new Vector2(28, 28);
            // LayoutGroup 영향 받지 않도록
            var shortcutLe = shortcutBadge.gameObject.AddComponent<LayoutElement>();
            shortcutLe.ignoreLayout = true;
            shortcutBadge.gameObject.AddComponent<Image>().color = new Color(0.95f, 0.82f, 0.25f, 0.95f);
            var shortcutText = NewRect("T", shortcutBadge);
            SetFillParent(shortcutText);
            var shortcutTmp = AddText(shortcutText, "1", 16, FontStyles.Bold, TextAlignmentOptions.Center, Color.black);

            // 아이콘 영역
            var iconRect = NewRect("Icon", rect);
            SetFixedHeight(iconRect, 80);
            var iconImg = iconRect.gameObject.AddComponent<Image>();
            iconImg.color = new Color(0.2f, 0.25f, 0.35f, 0.5f);
            iconImg.preserveAspect = true;

            // 스킬명
            var nameRect = NewRect("NameText", rect);
            SetFixedHeight(nameRect, 32);
            var nameTmp = nameRect.gameObject.AddComponent<TextMeshProUGUI>();
            nameTmp.font = GetOrCreateKoreanFont();
            nameTmp.text = "---";
            nameTmp.fontSize = 18;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = TextWhite;
            nameTmp.enableWordWrapping = true;

            // 타입 라벨
            var typeRect = NewRect("TypeText", rect);
            SetFixedHeight(typeRect, 18);
            var typeTmp = typeRect.gameObject.AddComponent<TextMeshProUGUI>();
            typeTmp.font = GetOrCreateKoreanFont();
            typeTmp.text = "";
            typeTmp.fontSize = 12;
            typeTmp.fontStyle = FontStyles.Bold;
            typeTmp.alignment = TextAlignmentOptions.Center;
            typeTmp.color = AccentYellow;

            // 효과 설명
            var descRect = NewRect("DescText", rect);
            var descLe = descRect.gameObject.AddComponent<LayoutElement>();
            descLe.flexibleHeight = 1;
            descLe.minHeight = 60;
            var descTmp = descRect.gameObject.AddComponent<TextMeshProUGUI>();
            descTmp.font = GetOrCreateKoreanFont();
            descTmp.text = "";
            descTmp.fontSize = 12;
            descTmp.alignment = TextAlignmentOptions.Center;
            descTmp.color = TextDim;
            descTmp.enableWordWrapping = true;
            descTmp.overflowMode = TextOverflowModes.Ellipsis;

            // DiscoverCard 컴포넌트 + 필드 와이어링
            var card = go.AddComponent<DiscoverCard>();
            var ser = new SerializedObject(card);
            var bgProp = ser.FindProperty("_backgroundImage");
            if (bgProp != null) bgProp.objectReferenceValue = bg;
            var iconProp = ser.FindProperty("_iconImage");
            if (iconProp != null) iconProp.objectReferenceValue = iconImg;
            var shortcutProp = ser.FindProperty("_shortcutLabel");
            if (shortcutProp != null) shortcutProp.objectReferenceValue = shortcutTmp;
            var titleProp = ser.FindProperty("_titleLabel");
            if (titleProp != null) titleProp.objectReferenceValue = nameTmp;
            var typeLabelProp = ser.FindProperty("_typeLabel");
            if (typeLabelProp != null) typeLabelProp.objectReferenceValue = typeTmp;
            var descProp = ser.FindProperty("_descLabel");
            if (descProp != null) descProp.objectReferenceValue = descTmp;
            var btnProp = ser.FindProperty("_button");
            if (btnProp != null) btnProp.objectReferenceValue = go.GetComponent<Button>();
            ser.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            AssetDatabase.Refresh(); // 생성 직후 AssetDatabase가 인식하도록 강제 갱신
            Debug.Log("[DiscoverModal] DiscoverCard prefab created at " + prefabPath);
        }
    }
}
