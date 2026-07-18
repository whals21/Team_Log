#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using TeamLog.Characters;
using TeamLog.UI;
using TeamLog.UI.PartySelection;

namespace TeamLog.Editor
{
    /// <summary>
    /// Party Selection Scene 자동 빌더 (UI-D.2 재작성) — 다크 판타지 고딕 디자인.
    /// ★ 핵심 교훈 (UI-C 버그 수정):
    ///   - 모든 LayoutGroup은 ForceExpand=false (자식이 preferredSize 존중)
    ///   - 모든 자식 GameObject는 LayoutElement로 명시적 크기 필수
    ///   - 빈 영역 채우려면 flexibleWidth/Height=1 사용
    ///   - 모든 Image는 Anchor (0,0)-(1,1) + Size Delta (0,0)으로 부모 영역 전체 채움
    ///
    /// Partial files:
    /// - PartySelectionSceneBuilder.cs          — 진입점 + 씬 생성 + Canvas + 헬퍼
    /// - PartySelectionSceneBuilder.Parts.cs    — Header / Stage / Carousel / Footer
    /// </summary>
    public static partial class PartySelectionSceneBuilder
    {
        // ── 경로 상수 ──
        private const string SCENE_PATH = "Assets/01.Scenes/PartySelectionScene.unity";
        private const string SPRITE_DIR = "Assets/03.Data/UI/PartySelection";
        private const string FONT_DIR = "Assets/08.Resource/Fonts";

        private const string SPRITE_GOLD_BORDER = "GoldBorder_9Slice";
        private const string SPRITE_GOLD_BORDER_THIN = "GoldBorderThin_9Slice";
        private const string SPRITE_PARCHMENT = "ParchmentPanel_9Slice";
        private const string SPRITE_PARCHMENT_DARK = "ParchmentDark_9Slice";
        private const string SPRITE_SLATE = "SlatePanel_9Slice";
        private const string SPRITE_SLATE_LIGHT = "SlatePanelLight_9Slice";
        private const string SPRITE_BLOOD_BTN_NORMAL = "BloodButton_Normal";
        private const string SPRITE_BLOOD_BTN_HOVER = "BloodButton_Hover";
        private const string SPRITE_BLOOD_BTN_PRESSED = "BloodButton_Pressed";
        private const string SPRITE_RUNE_OVERLAY = "RuneOverlay_Tile";
        private const string SPRITE_CREST_LOGO = "Crest_Logo";
        private const string SPRITE_VIGNETTE = "Shadow_Vignette";

        private static readonly Dictionary<string, Sprite> _spriteCache = new();
        private static TMP_FontAsset _fontCinzelBold;
        private static TMP_FontAsset _fontCinzelBlack;
        private static TMP_FontAsset _fontCinzelRegular;
        private static TMP_FontAsset _fontCormorantItalic;
        private static TMP_FontAsset _fontKorean;
        private static CharacterCarouselItem _cachedCarouselItemTemplate;

        [MenuItem("TeamLog/Scene/Build Party Selection Scene")]
        public static void BuildScene()
        {
            _spriteCache.Clear();
            _fontCinzelBold = LoadFont("Cinzel-Bold SDF");
            _fontCinzelBlack = LoadFont("Cinzel-Black SDF");
            _fontCinzelRegular = LoadFont("Cinzel-Regular SDF");
            _fontCormorantItalic = LoadFont("CormorantGaramond-Italic SDF");
            _fontKorean = LoadFont("NanumGothic SDF");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            Canvas canvas = CreateCanvas();
            CreateEventSystem();
            CreateBackground(canvas.transform);

            // ★ MainFrame — Canvas 전체 채우는 단일 컨테이너 (VerticalLayoutGroup이 4단계 자동 배치)
            var mainFrameGo = new GameObject("MainFrame", typeof(RectTransform));
            mainFrameGo.transform.SetParent(canvas.transform, false);
            StretchToParent(mainFrameGo.GetComponent<RectTransform>());
            var mfLe = mainFrameGo.AddComponent<LayoutElement>();
            mfLe.flexibleWidth = 1;
            mfLe.flexibleHeight = 1;
            mfLe.minWidth = 1200;
            mfLe.minHeight = 780;
            var mfVlg = AddVLG(mainFrameGo, TextAnchor.UpperCenter, 12,
                controlWidth: true, controlHeight: true,  // ★ true로 변경 — 자식 flexibleHeight 존중
                padLeft: 20, padRight: 20, padTop: 20, padBottom: 20);
            mfVlg.childForceExpandWidth = true;    // 폭은 부모 꽉 채움
            mfVlg.childForceExpandHeight = false;  // 강제 늘어남 금지 (자기 flexibleHeight 존중)

            // 4단계 섹션 (모두 LayoutElement로 명시적 높이 + 폭은 꽉 채움)
            var headerRect = CreateSectionSlot("Header", mainFrameGo.transform, height: 56);
            var stageRect = CreateSectionSlot("Stage", mainFrameGo.transform, height: 480, flexibleHeight: 1);
            var carouselRect = CreateSectionSlot("Carousel", mainFrameGo.transform, height: 110);
            var footerRect = CreateSectionSlot("Footer", mainFrameGo.transform, height: 86);

            // 각 영역 빌드
            BuildHeader(headerRect);
            BuildStage(stageRect);
            BuildCarousel(carouselRect);
            BuildFooter(footerRect);

            // 컨트롤러 부착 + 필드 바인딩
            var controllerGo = new GameObject("PartySelectionController");
            controllerGo.transform.SetParent(canvas.transform, false);
            var controller = controllerGo.AddComponent<PartySelectionController>();
            ControllerSetup(canvas, controller);

            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log($"[PartySelectionSceneBuilder] Scene saved: {SCENE_PATH}");
        }

        /// <summary>
        /// MainFrame 안의 섹션 슬롯 — LayoutElement로 명시적 높이 + 폭은 부모 꽉 채움.
        /// </summary>
        private static RectTransform CreateSectionSlot(string name, Transform parent, float height, float flexibleHeight = 0)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            StretchToParent(rect);

            var img = go.GetComponent<Image>();
            img.color = Color.clear;
            img.raycastTarget = false;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.flexibleHeight = flexibleHeight;
            le.flexibleWidth = 1;     // 폭은 부모 꽉 채움
            le.minWidth = 1000;

            return rect;
        }

        // =========================================================
        // 기본 인프라
        // =========================================================
        private static void CreateCamera()
        {
            var go = new GameObject("Main Camera");
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.020f, 0.020f, 0.035f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            go.tag = "MainCamera";
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 820);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateEventSystem()
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private static void CreateBackground(Transform parent)
        {
            var palette = UIPalette.Default;

            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(parent, false);
            StretchToParent(bgGo.GetComponent<RectTransform>());
            bgGo.GetComponent<Image>().color = palette.DFAbyss;

            // 룬 오버레이
            var runeGo = new GameObject("RuneOverlay", typeof(RectTransform), typeof(Image));
            runeGo.transform.SetParent(bgGo.transform, false);
            StretchToParent(runeGo.GetComponent<RectTransform>());
            var runeImg = runeGo.GetComponent<Image>();
            runeImg.sprite = LoadSprite(SPRITE_RUNE_OVERLAY);
            runeImg.type = Image.Type.Tiled;
            runeImg.color = new Color(1, 1, 1, 0.4f);
            runeImg.raycastTarget = false;

            // 비네팅 4개 코너
            var vignette = LoadSprite(SPRITE_VIGNETTE);
            CreateVignette(bgGo.transform, "TL", new Vector2(0, 0.6f), new Vector2(0.4f, 1f), vignette);
            CreateVignette(bgGo.transform, "TR", new Vector2(0.6f, 0.6f), new Vector2(1f, 1f), vignette);
            CreateVignette(bgGo.transform, "BL", new Vector2(0, 0f), new Vector2(0.4f, 0.4f), vignette);
            CreateVignette(bgGo.transform, "BR", new Vector2(0.6f, 0f), new Vector2(1f, 0.4f), vignette);
        }

        private static void CreateVignette(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Sprite sprite)
        {
            var go = new GameObject($"Vignette_{name}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = new Color(0, 0, 0, 0.7f);
            img.raycastTarget = false;
        }

        // =========================================================
        // 헬퍼 (레이아웃 유틸)
        // =========================================================

        /// <summary>RectTransform을 부모 영역 전체로 스트레치 (Anchor 0,0 ~ 1,1).</summary>
        public static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        /// <summary>RectTransform을 부모 중앙에 고정 크기로 배치.</summary>
        public static void SetCentered(RectTransform rect, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = Vector2.zero;
        }

        /// <summary>자동 레이아웃용 자식 GameObject 생성 — LayoutElement 옵션 포함.</summary>
        public static GameObject CreateLayoutChild(string name, Transform parent,
            float prefW = -1, float prefH = -1,
            float minW = -1, float minH = -1,
            float flexW = -1, float flexH = -1)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            if (prefW >= 0) le.preferredWidth = prefW;
            if (prefH >= 0) le.preferredHeight = prefH;
            if (minW >= 0) le.minWidth = minW;
            if (minH >= 0) le.minHeight = minH;
            if (flexW >= 0) le.flexibleWidth = flexW;
            if (flexH >= 0) le.flexibleHeight = flexH;
            return go;
        }

        /// <summary>Image 자식 생성 (부모 영역 전체 스트레치).</summary>
        public static Image CreateStretchImage(string name, Transform parent, Sprite sprite, Color color,
            Image.Type type = Image.Type.Simple)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.type = type;
            if (type == Image.Type.Sliced) img.fillCenter = true;
            img.raycastTarget = false;
            StretchToParent(go.GetComponent<RectTransform>());
            return img;
        }

        public static TextMeshProUGUI CreateText(string name, Transform parent,
            string content, float fontSize, Color color,
            TMP_FontAsset font = null,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center,
            bool flexibleSize = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = SafeText(content);   // ★ 특수 기호를 ASCII로 변환 (폰트 미지원 문자 방지)
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
            if (font != null) tmp.font = font;
            else UIKoreanFont.EnsureFont(tmp);
            StretchToParent(go.GetComponent<RectTransform>());

            if (flexibleSize)
            {
                var le = go.AddComponent<LayoutElement>();
                le.flexibleWidth = 1;
                le.flexibleHeight = 1;
                le.minWidth = 10;
                le.minHeight = 10;
            }
            return tmp;
        }

        /// <summary>
        /// 특수 Unicode 기호를 ASCII로 변환.
        /// NanumGothic SDF/Cinzel SDF가 지원하지 않는 기호가 □로 표시되는 문제 방지.
        /// 사용자가 폰트 fallback을 확장하면 제거 가능.
        /// </summary>
        public static string SafeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text
                .Replace("⚙", "CFG")
                .Replace("⚜", "")      // 제거 (이미 텍스트로 표현)
                .Replace("⚡", ">>")     // 조건부 보너스
                .Replace("⚠", "! ")     // 사용 제약
                .Replace("◈", "•")      // 메커니즘 타이틀
                .Replace("✦", "*")      // 특성 강조
                .Replace("✕", "X")      // CLEAR / Weakness
                .Replace("▶", ">")      // EMBARK
                .Replace("‹", "<")      // BtnPrev
                .Replace("›", ">")      // BtnNext
                .Replace("◐", "G ")     // 골드
                .Replace("✚", "+")      // Mercy
                .Replace("♪", "M")      // Melody
                .Replace("☠", "X")      // Corpse
                .Replace("⚗", "A")      // Discover
                .Replace("✓", "V")      // 파티 소속 체크
                .Replace("🔒", "[L]");  // 잠금
        }

        public static Button CreateButton(string name, Transform parent,
            Sprite sprite, Color color,
            string label, float fontSize, Color labelColor,
            TMP_FontAsset font = null,
            float width = -1, float height = -1)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.type = Image.Type.Sliced;
            img.raycastTarget = true;
            if (width > 0 && height > 0)
            {
                var le = go.AddComponent<LayoutElement>();
                le.preferredWidth = width;
                le.preferredHeight = height;
            }

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = SafeText(label);   // ★ 특수 기호 ASCII 변환
            tmp.fontSize = fontSize;
            tmp.color = labelColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            else UIKoreanFont.EnsureFont(tmp);
            StretchToParent(labelGo.GetComponent<RectTransform>());

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            return btn;
        }

        /// <summary>
        /// VerticalLayoutGroup 표준 설정 — ForceExpand=false (자식이 preferredSize 존중).
        /// </summary>
        public static VerticalLayoutGroup AddVLG(GameObject go,
            TextAnchor alignment = TextAnchor.UpperCenter,
            float spacing = 0,
            bool controlWidth = true, bool controlHeight = true,
            int padLeft = 0, int padRight = 0, int padTop = 0, int padBottom = 0)
        {
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = alignment;
            vlg.spacing = spacing;
            vlg.padding = new RectOffset(padLeft, padRight, padTop, padBottom);
            vlg.childControlWidth = controlWidth;
            vlg.childControlHeight = controlHeight;
            vlg.childForceExpandWidth = false;     // ★ 핵심 — false
            vlg.childForceExpandHeight = false;    // ★ 핵심 — false
            vlg.childScaleWidth = false;
            vlg.childScaleHeight = false;
            return vlg;
        }

        public static HorizontalLayoutGroup AddHLG(GameObject go,
            TextAnchor alignment = TextAnchor.UpperLeft,
            float spacing = 0,
            bool controlWidth = true, bool controlHeight = true,
            int padLeft = 0, int padRight = 0, int padTop = 0, int padBottom = 0)
        {
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = alignment;
            hlg.spacing = spacing;
            hlg.padding = new RectOffset(padLeft, padRight, padTop, padBottom);
            hlg.childControlWidth = controlWidth;
            hlg.childControlHeight = controlHeight;
            hlg.childForceExpandWidth = false;     // ★ 핵심 — false
            hlg.childForceExpandHeight = false;    // ★ 핵심 — false
            hlg.childScaleWidth = false;
            hlg.childScaleHeight = false;
            return hlg;
        }

        // ── 스프라이트/폰트 로드 ──
        public static Sprite LoadSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return null;
            if (_spriteCache.TryGetValue(spriteName, out var s)) return s;
            string path = $"{SPRITE_DIR}/{spriteName}.png";
            s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null) Debug.LogWarning($"[PartySelectionSceneBuilder] Sprite not found: {path}");
            else _spriteCache[spriteName] = s;
            return s;
        }

        public static TMP_FontAsset LoadFont(string fontName)
        {
            if (string.IsNullOrEmpty(fontName)) return null;
            var guids = AssetDatabase.FindAssets($"{fontName} t:TMP_FontAsset");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (font != null && font.name == fontName) return font;
            }
            string directPath = $"{FONT_DIR}/{fontName}.asset";
            var direct = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(directPath);
            if (direct != null) return direct;

            if (fontName != "NanumGothic SDF")
            {
                var fallback = LoadFont("NanumGothic SDF");
                if (fallback != null)
                    Debug.LogWarning($"[PartySelectionSceneBuilder] Font '{fontName}' not found, falling back to NanumGothic.");
                return fallback;
            }
            return null;
        }

        // ── 필드 자동 바인딩 ──
        public static void BindField(UnityEngine.Object component, string fieldName, UnityEngine.Object reference)
        {
            if (component == null || string.IsNullOrEmpty(fieldName)) return;
            var so = new SerializedObject(component);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[BindField] Field '{fieldName}' not found on {component.GetType().Name}");
                return;
            }
            prop.objectReferenceValue = reference;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static Transform FindDescendant(Transform root, string name)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == name) return child;
                var found = FindDescendant(child, name);
                if (found != null) return found;
            }
            return null;
        }

        // ── 팔레트 단축 ──
        private static UIPalette P => UIPalette.Default;

        // =========================================================
        // ControllerSetup — 컨트롤러 필드 자동 바인딩 + 캐릭터 데이터 로드
        // =========================================================
        private static void ControllerSetup(Canvas canvas, PartySelectionController controller)
        {
            var root = canvas.transform;

            var portraitGo = FindDescendant(root, "PortraitFrame");
            BindField(controller, "_portraitBig", portraitGo?.GetComponent<CharacterPortraitBig>());

            var mechGo = FindDescendant(root, "MechanicBox");
            BindField(controller, "_mechanicBox", mechGo?.GetComponent<ResourceMechanicBox>());

            var quoteGo = FindDescendant(root, "IdentityQuote");
            if (quoteGo != null)
            {
                var quoteText = FindDescendant(quoteGo.transform, "Text")?.GetComponent<TextMeshProUGUI>();
                BindField(controller, "_identityQuoteText", quoteText);
            }

            var statVigor = FindDescendant(root, "Stat_Vigor");
            var statRes = FindDescendant(root, "Stat_Resource");
            var statRole = FindDescendant(root, "Stat_Role");
            if (statVigor != null) BindField(controller, "_statHpValue",
                FindDescendant(statVigor, "Value")?.GetComponent<TextMeshProUGUI>());
            if (statRes != null)
            {
                BindField(controller, "_statResValue",
                    FindDescendant(statRes, "Value")?.GetComponent<TextMeshProUGUI>());
                BindField(controller, "_statResName",
                    FindDescendant(statRes, "Sub")?.GetComponent<TextMeshProUGUI>());
            }
            if (statRole != null)
            {
                BindField(controller, "_statRoleValue",
                    FindDescendant(statRole, "Value")?.GetComponent<TextMeshProUGUI>());
                BindField(controller, "_statRoleKo",
                    FindDescendant(statRole, "Sub")?.GetComponent<TextMeshProUGUI>());
            }

            var strengthBox = FindDescendant(root, "StrengthBox");
            var weaknessBox = FindDescendant(root, "WeaknessBox");
            if (strengthBox != null) BindField(controller, "_strengthText",
                FindDescendant(strengthBox, "Desc")?.GetComponent<TextMeshProUGUI>());
            if (weaknessBox != null) BindField(controller, "_weaknessText",
                FindDescendant(weaknessBox, "Desc")?.GetComponent<TextMeshProUGUI>());

            // 스킬 카드 4개
            var so = new SerializedObject(controller);
            var skillCardsProp = so.FindProperty("_skillCards");
            if (skillCardsProp != null)
            {
                skillCardsProp.arraySize = 4;
                for (int i = 0; i < 4; i++)
                {
                    var cardGo = FindDescendant(root, $"Skill{i + 1}");
                    var card = cardGo?.GetComponent<SkillDetailCard>();
                    if (card != null)
                        skillCardsProp.GetArrayElementAtIndex(i).objectReferenceValue = card;
                }
            }

            // 특성 카드 3개
            var traitCardsProp = so.FindProperty("_traitCards");
            if (traitCardsProp != null)
            {
                traitCardsProp.arraySize = 3;
                for (int i = 0; i < 3; i++)
                {
                    var cardGo = FindDescendant(root, $"Trait{i + 1}");
                    var card = cardGo?.GetComponent<TraitDetailCard>();
                    if (card != null)
                        traitCardsProp.GetArrayElementAtIndex(i).objectReferenceValue = card;
                }
            }

            // 캐러셀
            var contentGo = FindDescendant(root, "CarouselContent");
            BindField(controller, "_carouselContent", contentGo?.transform);
            BindField(controller, "_carouselItemPrefab", _cachedCarouselItemTemplate);

            var prevBtnGo = FindDescendant(root, "BtnPrev");
            var nextBtnGo = FindDescendant(root, "BtnNext");
            BindField(controller, "_prevButton", prevBtnGo?.GetComponent<Button>());
            BindField(controller, "_nextButton", nextBtnGo?.GetComponent<Button>());

            var footerGo = FindDescendant(root, "FooterPanel");
            BindField(controller, "_partySlotPanel", footerGo?.GetComponent<PartySlotPanel>());

            so.ApplyModifiedPropertiesWithoutUndo();

            // 캐릭터 데이터 자동 로드
            LoadCharacterData(controller);
        }

        private static void LoadCharacterData(PartySelectionController controller)
        {
            // CharacterData 로드
            var guids = AssetDatabase.FindAssets("Char_ t:CharacterData", new[] { "Assets/03.Data/Characters" });
            var list = new System.Collections.Generic.List<CharacterData>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var cd = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
                if (cd != null) list.Add(cd);
            }

            // CharacterTraitData 로드
            var traitGuids = AssetDatabase.FindAssets("t:CharacterTraitData", new[] { "Assets/03.Data/CharacterTraits" });
            var traitList = new System.Collections.Generic.List<CharacterTraitData>();
            foreach (var guid in traitGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var trait = AssetDatabase.LoadAssetAtPath<CharacterTraitData>(path);
                if (trait != null) traitList.Add(trait);
            }

            var so = new SerializedObject(controller);
            var arrProp = so.FindProperty("_availableCharacters");
            if (arrProp != null)
            {
                arrProp.arraySize = list.Count;
                for (int i = 0; i < list.Count; i++)
                    arrProp.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
            }
            var traitArrProp = so.FindProperty("_allTraits");
            if (traitArrProp != null)
            {
                traitArrProp.arraySize = traitList.Count;
                for (int i = 0; i < traitList.Count; i++)
                    traitArrProp.GetArrayElementAtIndex(i).objectReferenceValue = traitList[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log($"[PartySelectionSceneBuilder] Loaded {list.Count} CharacterData + {traitList.Count} CharacterTraitData.");
        }
    }
}
#endif
