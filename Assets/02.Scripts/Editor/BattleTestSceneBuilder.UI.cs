#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace TeamLog.Editor
{
    /// <summary>
    /// BattleTestSceneBuilder.UI — UI 생성 partial.
    /// 진입점/오케스트레이션/바인딩은 BattleTestSceneBuilder.cs 참조.
    ///
    /// 담당:
    /// - CreateConfigCanvas: ConfigCanvas + Panel + 모든 행/드롭다운/토글/버튼 생성
    /// - CreateDropdownRow / CreateTemplateRow / CreateInputField
    /// - NewRect / AddHeight / AddLabel (UI 유틸)
    /// - CreateDropdown / CreateToggle / CreateButton
    /// </summary>
    public static partial class BattleTestSceneBuilder
    {
        // ══════════════════════════════════════════════════════════
        //  ConfigCanvas + Panel + 드롭다운 생성
        // ══════════════════════════════════════════════════════════

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
            panelVlg.childAlignment = TextAnchor.MiddleCenter;  // 수직/수평 중앙 정렬
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

        // ══════════════════════════════════════════════════════════
        //  행/템플릿 행 생성
        // ══════════════════════════════════════════════════════════

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

            // 행 라벨 — 고정 폭, 확장 안 함
            var lblRect = AddLabel(row, label, 16, TextDim, 110f);
            var lblLe = lblRect.gameObject.GetComponent<LayoutElement>();
            lblLe.flexibleWidth = 0;

            var slots = new TMP_Dropdown[count];
            for (int i = 0; i < count; i++)
            {
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
        /// TMP_InputField 생성 — TMP_DefaultControls.CreateInputField() 표준 구조.
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
        //  UI 유틸리티 (BattleUISceneBuilder 헬퍼와 동일 패턴, 자체 복제)
        // ══════════════════════════════════════════════════════════

        internal static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        internal static void AddHeight(RectTransform rect, float height)
        {
            var le = rect.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
        }

        internal static RectTransform AddLabel(Transform parent, string text, int fontSize, Color color, float width)
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

        // ══════════════════════════════════════════════════════════
        //  드롭다운 / 토글 / 버튼
        // ══════════════════════════════════════════════════════════

        internal static TMP_Dropdown CreateDropdown(Transform parent, float width, float height)
        {
            var rect = NewRect("Dropdown", parent);
            var le = rect.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.minWidth = 80;
            le.flexibleWidth = 1;
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
            var templateRect = NewRect("Template", rect);
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = new Vector2(0, 2);
            templateRect.sizeDelta = new Vector2(0, 320);
            var templateBg = templateRect.gameObject.AddComponent<Image>();
            templateBg.color = new Color(0.1f, 0.1f, 0.16f, 0.98f);
            var scroll = templateRect.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.scrollSensitivity = 32f;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            // Scrollbar
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
            scroll.verticalScrollbarSpacing = -3;

            // Viewport — Mask로 항목 클리핑 (TMP_DefaultControls 표준)
            var viewportRect = NewRect("Viewport", templateRect);
            viewportRect.anchorMin = new Vector2(0, 0);
            viewportRect.anchorMax = new Vector2(1, 1);
            viewportRect.pivot = new Vector2(0, 1);
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = new Vector2(-16, 0);
            var viewportImg = viewportRect.gameObject.AddComponent<Image>();
            var viewportMask = viewportRect.gameObject.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            // ★ Mask.Reset() (Editor 전용)이 Image.color와 pivot을 리셋할 수 있으므로 Mask 추가 후에 다시 설정
            viewportImg.color = Color.white;
            viewportRect.pivot = new Vector2(0, 1);
            scroll.viewport = viewportRect;

            // Content — VerticalLayoutGroup/ContentSizeFitter 없음 (TMP_Dropdown.Show()가 수동 위치 지정)
            var itemsRect = NewRect("Content", viewportRect);
            itemsRect.anchorMin = new Vector2(0, 1);
            itemsRect.anchorMax = new Vector2(1, 1);
            itemsRect.pivot = new Vector2(0.5f, 1f);
            itemsRect.sizeDelta = new Vector2(0, 28);
            itemsRect.anchoredPosition = Vector2.zero;
            scroll.content = itemsRect;

            // Item — 표준 TMP Dropdown 구조: Item > [Item Background, Item Checkmark, Item Label]
            var itemRect = NewRect("Item", itemsRect);
            itemRect.anchorMin = new Vector2(0, 0.5f);
            itemRect.anchorMax = new Vector2(1, 0.5f);
            itemRect.sizeDelta = new Vector2(0, 28);
            var itemToggle = itemRect.gameObject.AddComponent<Toggle>();
            itemToggle.isOn = false;

            // Item Background
            var itemBgRect = NewRect("Item Background", itemRect);
            itemBgRect.anchorMin = Vector2.zero;
            itemBgRect.anchorMax = Vector2.one;
            itemBgRect.offsetMin = Vector2.zero;
            itemBgRect.offsetMax = Vector2.zero;
            var itemBgImg = itemBgRect.gameObject.AddComponent<Image>();
            itemBgImg.color = new Color(0.18f, 0.18f, 0.26f, 0.95f);
            itemToggle.targetGraphic = itemBgImg;

            // Item Checkmark
            var checkRect = NewRect("Item Checkmark", itemRect);
            checkRect.anchorMin = new Vector2(0, 0.5f);
            checkRect.anchorMax = new Vector2(0, 0.5f);
            checkRect.pivot = new Vector2(0, 0.5f);
            checkRect.sizeDelta = new Vector2(20, 20);
            checkRect.anchoredPosition = new Vector2(8, 0);
            var checkImg = checkRect.gameObject.AddComponent<Image>();
            checkImg.color = AccentYellow;
            itemToggle.graphic = checkImg;

            // Item Label
            var itemLabelRect = NewRect("Item Label", itemRect);
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(32, 1);
            itemLabelRect.offsetMax = new Vector2(-10, -2);
            var itemLabelTmp = itemLabelRect.gameObject.AddComponent<TextMeshProUGUI>();
            itemLabelTmp.fontSize = 16;
            itemLabelTmp.color = TextWhite;
            itemLabelTmp.alignment = TextAlignmentOptions.Left;

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

        internal static Toggle CreateToggle(Transform parent, string label, float width, float height)
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

        internal static Button CreateButton(Transform parent, string label, float width, float height, Color color, int fontSize = 22)
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
    }
}
#endif
