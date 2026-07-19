#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using TeamLog.UI;
using TeamLog.UI.Map.Rework;

namespace TeamLog.Editor
{
    /// <summary>
    /// MapSceneReworkBuilder 파트 — 각 섹션(Header/LeftSidebar/MapCodex/RightPanel/Footer) 생성.
    /// ★ Phase 3 (재완성 — 2026-07-19): 비주얼 디자인 강화.
    ///   - 칩/버튼 배경을 SlatePanel 9-Slice Sprite로
    ///   - MapCodex에 ParchmentRadial 배경 + 코너 룬 4종
    ///   - ThemeBanner 자식 구조 (StageLabel/ThemeName/Tagline/KeywordContainer)
    ///   - HeaderController 컴포넌트 추가 (런타임 헤더 칩 갱신)
    /// </summary>
    public static partial class MapSceneReworkBuilder
    {
        // =========================================================
        // Header — Brand / Stage / Floor / Gold / Ascension
        // =========================================================
        /// <summary>
        /// ★ Priority 5: anchor 기반 Header 배치. (LayoutGroup 없이 정확한 높이 보장)
        /// </summary>
        private static RectTransform BuildHeader(Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
        {
            var header = CreateAnchoredSection("Header", parent,
                anchorMin: anchorMin, anchorMax: anchorMax,
                pivot: pivot, sizeDelta: sizeDelta);

            // HLG는 자식 가로 배치용 (Header 높이엔 영향 안 줌 — anchor가 결정)
            var hlg = header.GameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(16, 16, 0, 0);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // 배경 — SlatePanel 9-Slice
            var bg = header.GameObject.GetComponent<Image>();
            var slateSprite = LoadSharedSprite("SlatePanel_9Slice.png");
            if (slateSprite != null)
            {
                bg.sprite = slateSprite;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.85f, 0.85f, 1f, 1f);
            }
            else
            {
                bg.color = new Color(0.1f, 0.1f, 0.2f, 0.98f);
            }
            bg.raycastTarget = false;

            // 구분선 (하단) — GoldBorderThin
            var divider = new GameObject("HeaderDivider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            divider.transform.SetParent(header.Rect, false);
            var dividerRt = divider.GetComponent<RectTransform>();
            dividerRt.anchorMin = new Vector2(0, 0);
            dividerRt.anchorMax = new Vector2(1, 0);
            dividerRt.pivot = new Vector2(0.5f, 0);
            dividerRt.sizeDelta = new Vector2(0, 2);
            var dividerImg = divider.GetComponent<Image>();
            var goldBorderSprite = LoadSharedSprite("GoldBorderThin_9Slice.png");
            if (goldBorderSprite != null)
            {
                dividerImg.sprite = goldBorderSprite;
                dividerImg.type = Image.Type.Sliced;
            }
            dividerImg.color = UIPalette.Default.DFGold;
            dividerImg.raycastTarget = false;

            // Brand
            CreateHeaderBrand(header.Rect);
            // Stage chip
            CreateHeaderChip(header.Rect, "StageChip", "Stage", "—");
            // Floor chip
            CreateHeaderChip(header.Rect, "FloorChip", "Floor", "1 / 4");
            // Gold chip
            CreateHeaderChip(header.Rect, "GoldChip", "Gold", "0");

            // Spacer (flexibleWidth=1)
            var spacer = new GameObject("HeaderSpacer", typeof(RectTransform));
            spacer.transform.SetParent(header.Rect, false);
            UIAutoBindHelper.EnsureLayoutElement(spacer, flexW: 1);

            // Ascension
            CreateHeaderAscension(header.Rect);

            // ★ HeaderController 부착
            header.GameObject.AddComponent<HeaderController>();

            return header.Rect;
        }

        private static void CreateHeaderBrand(Transform parent)
        {
            // ★ Priority 4 — prefH 56→36, BrandMark 38→28, spacing 12→8
            var brandGo = new GameObject("Brand", typeof(RectTransform));
            brandGo.transform.SetParent(parent, false);
            var hlg = brandGo.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            UIAutoBindHelper.EnsureLayoutElement(brandGo, prefH: 36, prefW: 200);

            // 브랜드 마크 (원형) — Crest_Logo Sprite 있으면 사용
            var markGo = new GameObject("BrandMark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            markGo.transform.SetParent(brandGo.transform, false);
            var markImg = markGo.GetComponent<Image>();
            var crestSprite = LoadSharedSprite("Crest_Logo.png");
            if (crestSprite != null) markImg.sprite = crestSprite;
            markImg.color = UIPalette.Default.DFGold;
            markImg.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(markGo, prefW: 28, prefH: 28);

            // 브랜드 텍스트
            var textGo = new GameObject("BrandText", typeof(RectTransform), typeof(CanvasRenderer));
            textGo.transform.SetParent(brandGo.transform, false);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "TEAM LOG";
            tmp.font = FontLabel();
            tmp.fontSize = 13;
            tmp.color = UIPalette.Default.DFGoldL;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(textGo, prefW: 150, prefH: 28);
        }

        private static void CreateHeaderChip(Transform parent, string name, string label, string value)
        {
            // ★ Priority 3 재작성: chipGo(배경 Image + LayoutGroup) → Border 자식(Image) + Content 자식(LayoutGroup)
            // ★ Priority 4 — prefH 34→30, prefW 170→150
            var chipGo = new GameObject(name, typeof(RectTransform));
            chipGo.transform.SetParent(parent, false);
            UIAutoBindHelper.EnsureLayoutElement(chipGo, prefH: 30, prefW: 150);

            // ★ 칩 배경 — SlatePanel Sprite, color 밝게
            var bg = chipGo.AddComponent<Image>();
            var slateSprite = LoadSharedSprite("SlatePanel_9Slice.png");
            if (slateSprite != null)
            {
                bg.sprite = slateSprite;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.85f, 0.85f, 1f, 1f);
            }
            else
            {
                bg.color = new Color(0.12f, 0.12f, 0.22f, 0.98f);
            }
            bg.raycastTarget = false;

            // ★ 칩 외곽 — GoldBorderThin (배경 위, 라벨/값 아래)
            var borderGo = new GameObject($"{name}_Border", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            borderGo.transform.SetParent(chipGo.transform, false);
            var borderRt = borderGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(borderRt);
            var borderImg = borderGo.GetComponent<Image>();
            var goldBorderSprite = LoadSharedSprite("GoldBorderThin_9Slice.png");
            if (goldBorderSprite != null)
            {
                borderImg.sprite = goldBorderSprite;
                borderImg.type = Image.Type.Sliced;
            }
            borderImg.color = UIPalette.Default.DFGold;
            borderImg.raycastTarget = false;

            // ★ Content 컨테이너 — LayoutElement.ignoreLayout 아님, 부모 배경 Image는 Layout 무시
            var contentGo = new GameObject($"{name}_Content", typeof(RectTransform));
            contentGo.transform.SetParent(chipGo.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(contentRt);
            var contentLe = contentGo.AddComponent<LayoutElement>();
            contentLe.ignoreLayout = true; // 부모가 LayoutGroup이 아니지만 안전장치
            var hlg = contentGo.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 8;
            hlg.padding = new RectOffset(14, 14, 6, 6);
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // 라벨
            var labelGo = new GameObject($"{name}_Label", typeof(RectTransform), typeof(CanvasRenderer));
            labelGo.transform.SetParent(contentGo.transform, false);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.font = FontItalic();
            labelTmp.fontStyle = FontStyles.Italic;
            labelTmp.fontSize = 11;
            labelTmp.color = UIPalette.Default.DFInkDim;
            labelTmp.alignment = TextAlignmentOptions.Left;
            labelTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(labelGo, prefW: 50, prefH: 22);

            // 값
            var valueGo = new GameObject($"{name}_Value", typeof(RectTransform), typeof(CanvasRenderer));
            valueGo.transform.SetParent(contentGo.transform, false);
            var valueTmp = valueGo.AddComponent<TextMeshProUGUI>();
            valueTmp.text = value;
            valueTmp.font = FontLabel();
            valueTmp.fontSize = 13;
            valueTmp.color = UIPalette.Default.DFGoldL;
            valueTmp.alignment = TextAlignmentOptions.Left;
            valueTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(valueGo, flexW: 1, prefH: 22);
        }

        private static void CreateHeaderAscension(Transform parent)
        {
            // ★ Priority 3 재작성: 배경 + 테두리 + Content 3-레이어 구조
            // ★ Priority 4 — prefH 34→30, prefW 190→170
            var ascGo = new GameObject("AscensionDisplay", typeof(RectTransform));
            ascGo.transform.SetParent(parent, false);
            UIAutoBindHelper.EnsureLayoutElement(ascGo, prefH: 30, prefW: 170);

            // 배경 — BloodButton Sprite
            var bg = ascGo.AddComponent<Image>();
            var bloodSprite = LoadSharedSprite("BloodButton_Normal.png");
            if (bloodSprite != null)
            {
                bg.sprite = bloodSprite;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.95f, 0.5f, 0.5f, 1f); // 핏빛 밝게
            }
            else
            {
                bg.color = new Color(0.35f, 0.05f, 0.05f, 0.98f);
            }
            bg.raycastTarget = false;

            // 외곽 — GoldBorderThin
            var borderGo = new GameObject("AscBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            borderGo.transform.SetParent(ascGo.transform, false);
            var borderRt = borderGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(borderRt);
            var borderImg = borderGo.GetComponent<Image>();
            var goldBorderSprite = LoadSharedSprite("GoldBorderThin_9Slice.png");
            if (goldBorderSprite != null)
            {
                borderImg.sprite = goldBorderSprite;
                borderImg.type = Image.Type.Sliced;
            }
            borderImg.color = UIPalette.Default.DFGold;
            borderImg.raycastTarget = false;

            // Content
            var contentGo = new GameObject("AscContent", typeof(RectTransform));
            contentGo.transform.SetParent(ascGo.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(contentRt);
            var contentLe = contentGo.AddComponent<LayoutElement>();
            contentLe.ignoreLayout = true;
            var hlg = contentGo.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 8;
            hlg.padding = new RectOffset(14, 14, 6, 6);
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // 라벨
            var labelGo = new GameObject("AscLabel", typeof(RectTransform), typeof(CanvasRenderer));
            labelGo.transform.SetParent(contentGo.transform, false);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = "Ascension";
            labelTmp.font = FontItalic();
            labelTmp.fontStyle = FontStyles.Italic;
            labelTmp.fontSize = 11;
            labelTmp.color = UIPalette.Default.DFInkDim;
            labelTmp.alignment = TextAlignmentOptions.Left;
            labelTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(labelGo, prefW: 80, prefH: 22);

            // 값
            var valueGo = new GameObject("AscValue", typeof(RectTransform), typeof(CanvasRenderer));
            valueGo.transform.SetParent(contentGo.transform, false);
            var valueTmp = valueGo.AddComponent<TextMeshProUGUI>();
            valueTmp.text = "0";
            valueTmp.font = FontLabel();
            valueTmp.fontSize = 14;
            valueTmp.color = UIPalette.Default.DFGoldL;
            valueTmp.alignment = TextAlignmentOptions.Left;
            valueTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(valueGo, flexW: 1, prefH: 22);
        }

        // =========================================================
        // LeftSidebar — Party / Relics / Augments (★ Run Log 제거됨)
        // =========================================================
        private static void BuildLeftSidebar(Transform parent)
        {
            var sidebar = CreateSection("LeftSidebar", parent, prefW: 320);
            var bg = sidebar.GameObject.GetComponent<Image>();
            var slateSprite = LoadSharedSprite("SlatePanel_9Slice.png");
            if (slateSprite != null)
            {
                bg.sprite = slateSprite;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.8f, 0.8f, 0.95f, 1f); // 밝게
            }
            else
            {
                bg.color = new Color(0.12f, 0.12f, 0.22f, 0.95f);
            }
            bg.raycastTarget = false;

            // 스크롤 가능한 컨테이너
            var scrollGo = new GameObject("LeftSidebarScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGo.transform.SetParent(sidebar.Rect, false);
            UIAutoBindHelper.StretchToParent(scrollGo.GetComponent<RectTransform>());
            var scrollImg = scrollGo.GetComponent<Image>();
            scrollImg.color = new Color(0, 0, 0, 0);
            scrollImg.raycastTarget = false;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            var content = CreateChildContainer("LeftSidebarContent", scrollGo.transform, padding: 16, spacing: 18);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            scroll.content = content;

            // Party 섹션
            CreatePanelHeader("Party", content);
            var partyListGo = new GameObject("PartyListContainer", typeof(RectTransform));
            partyListGo.transform.SetParent(content, false);
            var partyVlg = partyListGo.AddComponent<VerticalLayoutGroup>();
            partyVlg.childControlWidth = true;
            partyVlg.childControlHeight = true;
            partyVlg.spacing = 8;
            UIAutoBindHelper.EnsureLayoutElement(partyListGo, flexW: 1, prefH: 4 * 60);

            // ★ PartySidebarPanel 컴포넌트 부착
            var sidebarPanel = partyListGo.AddComponent<PartySidebarPanel>();

            // Relics 섹션
            CreatePanelHeader("Relics", content);
            var relicGridGo = new GameObject("RelicGridContainer", typeof(RectTransform));
            relicGridGo.transform.SetParent(content, false);
            var relicGrid = relicGridGo.AddComponent<GridLayoutGroup>();
            relicGrid.cellSize = new Vector2(48, 48);
            relicGrid.spacing = new Vector2(6, 6);
            relicGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            relicGrid.constraintCount = 5;
            UIAutoBindHelper.EnsureLayoutElement(relicGridGo, flexW: 1, prefH: 48 * 2 + 6);
            relicGridGo.AddComponent<RelicGridPanel>();

            // Augments 섹션
            CreatePanelHeader("Augments", content);
            var augmentListGo = new GameObject("AugmentListContainer", typeof(RectTransform));
            augmentListGo.transform.SetParent(content, false);
            var augVlg = augmentListGo.AddComponent<VerticalLayoutGroup>();
            augVlg.childControlWidth = true;
            augVlg.childControlHeight = true;
            augVlg.spacing = 6;
            UIAutoBindHelper.EnsureLayoutElement(augmentListGo, flexW: 1);
            augmentListGo.AddComponent<AugmentListPanel>();
        }

        // =========================================================
        // MapCodex — 중앙 노드 맵
        // =========================================================
        private static void BuildMapCodex(Transform parent)
        {
            var codex = CreateSection("MapCodex", parent, flexW: 1);
            var bg = codex.GameObject.GetComponent<Image>();
            bg.color = UIPalette.Default.DFVoid;
            bg.raycastTarget = false;

            // ★ 중앙 배경 — ParchmentRadial 라디얼
            var radialGo = new GameObject("ParchmentRadialBg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            radialGo.transform.SetParent(codex.Rect, false);
            var radialRt = radialGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(radialRt);
            var radialImg = radialGo.GetComponent<Image>();
            var parchmentRadialSprite = LoadMapSprite("ParchmentRadial.png");
            if (parchmentRadialSprite != null) radialImg.sprite = parchmentRadialSprite;
            radialImg.color = Color.white;
            radialImg.raycastTarget = false;
            radialGo.transform.SetAsFirstSibling(); // 가장 뒤로

            // ★ 코너 룬 4종 (TL/TR/BL/BR) — Cinzel 폰트로 고대 룬 문자
            CreateCornerRunes(codex.Rect);

            // ThemeBanner (상단) — 자식 구조와 Sprite 강화
            BuildThemeBanner(codex.Rect);

            // MapReworkView — 노드 컨테이너
            var mapGo = new GameObject("MapReworkView", typeof(RectTransform));
            mapGo.transform.SetParent(codex.Rect, false);
            var mapRt = mapGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(mapRt);
            mapGo.AddComponent<MapReworkView>();
        }

        /// <summary>
        /// ★ 코너 룬 4종 — TL/TR/BL/BR에 고대 룬 텍스트 배치.
        /// 다크 판타지 고딕 분위기 강조용 장식 요소.
        /// </summary>
        private static void CreateCornerRunes(Transform parent)
        {
            CreateCornerRune(parent, "RuneTL", new Vector2(0, 1), "ᚱ");
            CreateCornerRune(parent, "RuneTR", new Vector2(1, 1), "ᚦ");
            CreateCornerRune(parent, "RuneBL", new Vector2(0, 0), "ᛟ");
            CreateCornerRune(parent, "RuneBR", new Vector2(1, 0), "ᛞ");
        }

        private static void CreateCornerRune(Transform parent, string name, Vector2 anchor, string runeChar)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(80, 80);
            // 코너에서 살짝 안쪽으로
            float offsetX = anchor.x < 0.5f ? 50f : -50f;
            float offsetY = anchor.y < 0.5f ? 50f : -50f;
            rt.anchoredPosition = new Vector2(offsetX, offsetY);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = runeChar;
            tmp.font = FontTitle(); // Cinzel Black — 룬은 고딕으로 강조
            tmp.fontSize = 42;
            tmp.color = new Color(0.83f, 0.65f, 0.24f, 0.5f); // DFGold + alpha 상향 (0.25→0.5)
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }

        /// <summary>
        /// ★ ThemeBanner — 자식 구조 (StageLabel / ThemeName / Tagline / KeywordContainer) +
        /// ThemeBannerBg Sprite 배경. ThemeBanner.cs가 자동 바인딩 사용.
        /// </summary>
        private static void BuildThemeBanner(Transform parent)
        {
            var bannerGo = new GameObject("ThemeBanner", typeof(RectTransform));
            bannerGo.transform.SetParent(parent, false);
            var bannerRt = bannerGo.GetComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(0.5f, 1);
            bannerRt.anchorMax = new Vector2(0.5f, 1);
            bannerRt.pivot = new Vector2(0.5f, 1);
            bannerRt.sizeDelta = new Vector2(580, 120);
            bannerRt.anchoredPosition = new Vector2(0, -20);

            // 배경 — ThemeBannerBg Sprite
            var bgImg = bannerGo.AddComponent<Image>();
            var bannerBgSprite = LoadMapSprite("ThemeBannerBg.png");
            if (bannerBgSprite != null) bgImg.sprite = bannerBgSprite;
            bgImg.color = Color.white;
            bgImg.raycastTarget = false;

            // ★ ThemeBanner 컴포넌트 부착 (자식 자동 바인딩)
            var banner = bannerGo.AddComponent<ThemeBanner>();

            // 자식 구조 — VerticalLayoutGroup으로 3줄 + 키워드 행
            var vlg = bannerGo.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(20, 20, 12, 12);
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperCenter;

            // StageLabel — "— Stage I —" (Cormorant Italic)
            var stageGo = CreateBannerText("StageLabel", "— Stage I —", 12, UIPalette.Default.DFGold, FontStyles.Italic, FontItalic());
            stageGo.transform.SetParent(bannerGo.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(stageGo, prefH: 18);

            // ThemeName — 큰 메인 타이틀 (Cinzel Black)
            var themeGo = CreateBannerText("ThemeName", "Grey Forest", 26, UIPalette.Default.DFGoldL, FontStyles.Bold, FontTitle());
            themeGo.transform.SetParent(bannerGo.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(themeGo, prefH: 36);

            // Tagline — 분위기 묘사 (Cormorant Italic, 한국어는 NanumGothic fallback)
            var taglineGo = CreateBannerText("Tagline", "고요한 숲. 안개 속에서 오래된 발자국이 보인다.", 12, UIPalette.Default.DFInkDim, FontStyles.Italic, FontItalic());
            taglineGo.transform.SetParent(bannerGo.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(taglineGo, prefH: 18);

            // KeywordContainer — 키워드 칩 컨테이너
            var kwGo = new GameObject("KeywordContainer", typeof(RectTransform));
            kwGo.transform.SetParent(bannerGo.transform, false);
            var kwHlg = kwGo.AddComponent<HorizontalLayoutGroup>();
            kwHlg.childControlWidth = true;
            kwHlg.childControlHeight = true;
            kwHlg.childForceExpandWidth = false;
            kwHlg.childForceExpandHeight = false;
            kwHlg.spacing = 8;
            kwHlg.childAlignment = TextAnchor.UpperCenter;
            UIAutoBindHelper.EnsureLayoutElement(kwGo, prefH: 22, flexW: 1);
        }

        private static GameObject CreateBannerText(string name, string text, int fontSize, Color color, FontStyles style, TMP_FontAsset font = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.font = font ?? FontBody(); // 기본 Cinzel Regular
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return go;
        }

        // =========================================================
        // RightPanel — NodeDetail
        // =========================================================
        private static void BuildRightPanel(Transform parent)
        {
            var right = CreateSection("RightPanel", parent, prefW: 360);
            var bg = right.GameObject.GetComponent<Image>();
            var darkPanelSprite = LoadSharedSprite("ParchmentDark_9Slice.png");
            if (darkPanelSprite != null)
            {
                bg.sprite = darkPanelSprite;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.9f, 0.85f, 0.7f, 1f);
            }
            else
            {
                bg.color = new Color(0.15f, 0.13f, 0.1f, 0.95f);
            }
            bg.raycastTarget = false;

            var content = CreateChildContainer("RightPanelContent", right.Rect, padding: 16, spacing: 14);

            CreatePanelHeader("Selected", content);

            // ★ Phase C: NodeDetailPanel 컨테이너 + 자식 6종
            BuildNodeDetailPanel(content);
        }

        /// <summary>
        /// ★ Phase C + Node Detail Preview 파이프 — NodeDetailPanel 내부 자식 8종 생성.
        /// 자동 바인딩 이름 규칙:
        ///   NodeIcon / NodeTitle / NodeSubtitle / NodeDescription /
        ///   EnemyListContainer (★ 신규) / RewardInfoContainer (★ 신규) /
        ///   StatContainer (레거시) / ActionButton
        /// </summary>
        private static void BuildNodeDetailPanel(RectTransform parent)
        {
            var detailGo = new GameObject("NodeDetailPanel", typeof(RectTransform), typeof(CanvasRenderer));
            detailGo.transform.SetParent(parent, false);
            var detailLe = detailGo.AddComponent<LayoutElement>();
            detailLe.flexibleWidth = 1;
            detailLe.preferredHeight = 520;  // ★ 380 → 520 (Enemy/Reward 컨테이너 공간)

            // 배경 — SlatePanel 9-Slice + Color 밝게
            var bgImg = detailGo.AddComponent<Image>();
            var slateSprite = LoadSharedSprite("SlatePanel_9Slice.png");
            if (slateSprite != null)
            {
                bgImg.sprite = slateSprite;
                bgImg.type = Image.Type.Sliced;
                bgImg.color = new Color(0.7f, 0.7f, 0.9f, 1f);
            }
            else
            {
                bgImg.color = new Color(0.13f, 0.13f, 0.22f, 1f);
            }
            bgImg.raycastTarget = false;

            // VLG (자식 세로 배치)
            var vlg = detailGo.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(14, 14, 14, 14);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;

            // 1. NodeIcon (56x56 원형 + 자식 TMP 이니셜)
            var iconGo = new GameObject("NodeIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(detailGo.transform, false);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.color = new Color(0.04f, 0.04f, 0.08f, 1f); // 어두운 배경
            iconImg.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(iconGo, prefW: 56, prefH: 56);

            var iconInitialGo = new GameObject("Symbol", typeof(RectTransform), typeof(CanvasRenderer));
            iconInitialGo.transform.SetParent(iconGo.transform, false);
            var iconInitialRt = iconInitialGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(iconInitialRt);
            var iconInitialTmp = iconInitialGo.AddComponent<TextMeshProUGUI>();
            iconInitialTmp.text = "?";
            iconInitialTmp.font = FontTitle();
            iconInitialTmp.fontSize = 28;
            iconInitialTmp.color = UIPalette.Default.DFGoldL;
            iconInitialTmp.alignment = TextAlignmentOptions.Center;
            iconInitialTmp.raycastTarget = false;

            // 2. NodeTitle (Cinzel Bold, 중앙 정렬)
            var titleGo = new GameObject("NodeTitle", typeof(RectTransform), typeof(CanvasRenderer));
            titleGo.transform.SetParent(detailGo.transform, false);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "Selected Node";
            titleTmp.font = FontLabel();
            titleTmp.fontSize = 15;
            titleTmp.color = UIPalette.Default.DFGoldL;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(titleGo, flexW: 1, prefH: 22);

            // 3. NodeSubtitle (Cormorant Italic)
            var subtitleGo = new GameObject("NodeSubtitle", typeof(RectTransform), typeof(CanvasRenderer));
            subtitleGo.transform.SetParent(detailGo.transform, false);
            var subtitleTmp = subtitleGo.AddComponent<TextMeshProUGUI>();
            subtitleTmp.text = "Awaiting selection";
            subtitleTmp.font = FontItalic();
            subtitleTmp.fontStyle = FontStyles.Italic;
            subtitleTmp.fontSize = 11;
            subtitleTmp.color = UIPalette.Default.DFInkDim;
            subtitleTmp.alignment = TextAlignmentOptions.Center;
            subtitleTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(subtitleGo, flexW: 1, prefH: 16);

            // 4. NodeDescription (Cormorant, 본문)
            var descGo = new GameObject("NodeDescription", typeof(RectTransform), typeof(CanvasRenderer));
            descGo.transform.SetParent(detailGo.transform, false);
            var descTmp = descGo.AddComponent<TextMeshProUGUI>();
            descTmp.text = "Click a node on the map to inspect details.";
            descTmp.font = FontItalic();
            descTmp.fontSize = 12;
            descTmp.color = UIPalette.Default.DFParchment;
            descTmp.alignment = TextAlignmentOptions.Center;
            descTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(descGo, flexW: 1, prefH: 50);

            // 5. ★ EnemyListContainer (Node Detail Preview 파이프 — 적 목록 행들)
            var enemyListGo = new GameObject("EnemyListContainer", typeof(RectTransform));
            enemyListGo.transform.SetParent(detailGo.transform, false);
            var enemyVlg = enemyListGo.AddComponent<VerticalLayoutGroup>();
            enemyVlg.childControlWidth = true;
            enemyVlg.childControlHeight = true;
            enemyVlg.childForceExpandWidth = true;
            enemyVlg.childForceExpandHeight = false;
            enemyVlg.spacing = 4;
            var enemyFitter = enemyListGo.AddComponent<ContentSizeFitter>();
            enemyFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            UIAutoBindHelper.EnsureLayoutElement(enemyListGo, flexW: 1, prefH: 80);

            // 6. ★ RewardInfoContainer (Node Detail Preview 파이프 — 보상 행들)
            var rewardListGo = new GameObject("RewardInfoContainer", typeof(RectTransform));
            rewardListGo.transform.SetParent(detailGo.transform, false);
            var rewardVlg = rewardListGo.AddComponent<VerticalLayoutGroup>();
            rewardVlg.childControlWidth = true;
            rewardVlg.childControlHeight = true;
            rewardVlg.childForceExpandWidth = true;
            rewardVlg.childForceExpandHeight = false;
            rewardVlg.spacing = 4;
            var rewardFitter = rewardListGo.AddComponent<ContentSizeFitter>();
            rewardFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            UIAutoBindHelper.EnsureLayoutElement(rewardListGo, flexW: 1, prefH: 80);

            // 7. StatContainer (레거시 — GridLayoutGroup, 2열. preview 파이프에서는 사용 안 함. 하위 호환)
            var statGo = new GameObject("StatContainer", typeof(RectTransform));
            statGo.transform.SetParent(detailGo.transform, false);
            var statGlg = statGo.AddComponent<GridLayoutGroup>();
            statGlg.cellSize = new Vector2(140, 36);
            statGlg.spacing = new Vector2(6, 6);
            statGlg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            statGlg.constraintCount = 2;
            statGlg.childAlignment = TextAnchor.UpperCenter;
            UIAutoBindHelper.EnsureLayoutElement(statGo, flexW: 1, prefH: 36 * 2 + 6);

            // 8. ActionButton (전체 너비, 핏빛 배경)
            var btnGo = new GameObject("ActionButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(detailGo.transform, false);
            var btnImg = btnGo.GetComponent<Image>();
            var bloodSprite = LoadSharedSprite("BloodButton_Normal.png");
            if (bloodSprite != null)
            {
                btnImg.sprite = bloodSprite;
                btnImg.type = Image.Type.Sliced;
                btnImg.color = new Color(0.95f, 0.5f, 0.5f, 1f);
            }
            else
            {
                btnImg.color = new Color(0.35f, 0.05f, 0.05f, 1f);
            }
            btnImg.raycastTarget = true;

            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;

            var btnLabelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            btnLabelGo.transform.SetParent(btnGo.transform, false);
            var btnLabelRt = btnLabelGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(btnLabelRt);
            var btnLabelTmp = btnLabelGo.AddComponent<TextMeshProUGUI>();
            btnLabelTmp.text = "CONFIRM";
            btnLabelTmp.font = FontLabel();
            btnLabelTmp.fontSize = 12;
            btnLabelTmp.color = UIPalette.Default.DFGoldL;
            btnLabelTmp.alignment = TextAlignmentOptions.Center;
            btnLabelTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(btnGo, flexW: 1, prefH: 36);

            // ★ NodeDetailPanel 컴포넌트 부착 — 자동 바인딩 활성
            detailGo.AddComponent<NodeDetailPanel>();
        }

        // =========================================================
        // Footer — Skills / Traits / Options / Abandon
        // =========================================================
        /// <summary>
        /// ★ Priority 5: anchor 기반 Footer 배치. (LayoutGroup 없이 정확한 높이 보장)
        /// </summary>
        private static RectTransform BuildFooter(Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
        {
            var footer = CreateAnchoredSection("Footer", parent,
                anchorMin: anchorMin, anchorMax: anchorMax,
                pivot: pivot, sizeDelta: sizeDelta);

            var bg = footer.GameObject.GetComponent<Image>();
            var slateSprite = LoadSharedSprite("SlatePanel_9Slice.png");
            if (slateSprite != null)
            {
                bg.sprite = slateSprite;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.7f, 0.7f, 0.85f, 1f);
            }
            else
            {
                bg.color = new Color(0.08f, 0.08f, 0.15f, 0.98f);
            }
            bg.raycastTarget = false;

            var hlg = footer.GameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(16, 16, 0, 0);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            CreateFooterButton(footer.Rect, "SkillsBtn", "Skills");
            CreateFooterButton(footer.Rect, "TraitsBtn", "Traits");
            CreateFooterButton(footer.Rect, "OptionsBtn", "Options");
            CreateFooterButton(footer.Rect, "AbandonBtn", "Abandon");

            // Spacer
            var spacer = new GameObject("FooterSpacer", typeof(RectTransform));
            spacer.transform.SetParent(footer.Rect, false);
            UIAutoBindHelper.EnsureLayoutElement(spacer, flexW: 1);

            // Run stats (Cormorant Italic)
            var statsGo = new GameObject("RunStats", typeof(RectTransform), typeof(CanvasRenderer));
            statsGo.transform.SetParent(footer.Rect, false);
            var statsTmp = statsGo.AddComponent<TextMeshProUGUI>();
            statsTmp.text = "Floor 1/4   ·   Battles Won 0   ·   Time 00:00";
            statsTmp.font = FontItalic();
            statsTmp.fontSize = 11;
            statsTmp.color = UIPalette.Default.DFInkDim;
            statsTmp.alignment = TextAlignmentOptions.Right;
            statsTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(statsGo, prefW: 380, prefH: 18);

            return footer.Rect;
        }

        private static void CreateFooterButton(Transform parent, string name, string label)
        {
            var btnGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);

            // ★ 버튼 배경 — SlatePanel Sprite, color 밝게
            var btnImg = btnGo.GetComponent<Image>();
            var slateSprite = LoadSharedSprite("SlatePanel_9Slice.png");
            if (slateSprite != null)
            {
                btnImg.sprite = slateSprite;
                btnImg.type = Image.Type.Sliced;
                btnImg.color = new Color(0.75f, 0.75f, 0.9f, 1f);
            }
            else
            {
                btnImg.color = new Color(0.15f, 0.15f, 0.25f, 1f);
            }
            btnImg.raycastTarget = true;

            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;

            var hlg = btnGo.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(12, 12, 6, 6);
            hlg.spacing = 8;
            UIAutoBindHelper.EnsureLayoutElement(btnGo, prefH: 28, prefW: 95);

            // 텍스트 (Cinzel Bold)
            var labelGo = new GameObject($"{name}_Label", typeof(RectTransform), typeof(CanvasRenderer));
            labelGo.transform.SetParent(btnGo.transform, false);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.font = FontLabel();
            labelTmp.fontSize = 12;
            labelTmp.color = UIPalette.Default.DFGoldL;
            labelTmp.alignment = TextAlignmentOptions.Left;
            labelTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(labelGo, flexW: 1, prefH: 18);
        }
    }
}
#endif
