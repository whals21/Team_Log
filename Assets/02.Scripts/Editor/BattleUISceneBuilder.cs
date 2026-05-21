using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI.Battle;
using TeamLog.Combat;
using TeamLog.Characters;

namespace TeamLog.Editor
{
    public class BattleUISceneBuilder
    {

        // 색상 팔레트
        private static readonly Color BgDark = new Color(0.08f, 0.08f, 0.16f);
        private static readonly Color AccentRed = new Color(0.77f, 0.12f, 0.23f);
        private static readonly Color AccentGreen = new Color(0.15f, 0.68f, 0.38f);
        private static readonly Color AccentYellow = new Color(0.96f, 0.82f, 0.25f);
        private static readonly Color BorderRed = new Color(0.6f, 0.1f, 0.18f, 0.8f);
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextDim = new Color(0.7f, 0.7f, 0.75f);
        private const string KOREAN_FONT_TTF = "Assets/08.Resource/Fonts/NanumGothic.ttf";
        private const string KOREAN_FONT_SDF = "Assets/08.Resource/Fonts/NanumGothic SDF.asset";
        private static TMP_FontAsset _koreanFont;

        private static TMP_FontAsset GetOrCreateKoreanFont()
        {
            // GUID로 에셋 검색
            var guids = AssetDatabase.FindAssets("NanumGothic SDF t:TMP_FontAsset");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (font != null)
                {
                    Debug.Log("[BattleUISceneBuilder] Found NanumGothic SDF via search: " + path);
                    SetupFallbackFont(font);
                    return font;
                }
            }

            // TTF에서 즉시 생성 (기본 ASCII만 포함해도 fallback으로 작동)
            var ttfFont = AssetDatabase.LoadAssetAtPath<Font>(KOREAN_FONT_TTF);
            if (ttfFont != null)
            {
                var sdf = TMP_FontAsset.CreateFontAsset(ttfFont);
                if (sdf != null)
                {
                    Debug.Log("[BattleUISceneBuilder] Created NanumGothic SDF from TTF");
                    SetupFallbackFont(sdf);
                    return sdf;
                }
            }

            Debug.LogWarning("[BattleUISceneBuilder] Could not load Korean font");
            return null;
        }

        private static void SetupFallbackFont(TMP_FontAsset koreanFont)
        {
            var fallbacks = TMPro.TMP_Settings.fallbackFontAssets;
            if (fallbacks == null)
                fallbacks = new System.Collections.Generic.List<TMP_FontAsset>();

            if (!fallbacks.Contains(koreanFont))
            {
                fallbacks.Add(koreanFont);
                TMPro.TMP_Settings.fallbackFontAssets = fallbacks;
                Debug.Log("[BattleUISceneBuilder] Added NanumGothic as TMP fallback font");
            }
        }

        [MenuItem("Tools/Battle UI/Build Battle Scene (with BG)", false, 99)]
        public static void BuildBattleScene()
        {
            const string path = "Assets/01.Scenes/BattleScene.unity";

            // 기존 BattleScene 로드
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // 기존 TestCombatManager (스크립트 없는 빈 오브젝트) 제거
            var oldManager = GameObject.Find("TestCombatManager");
            if (oldManager != null)
                Object.DestroyImmediate(oldManager);

            // BattleUICanvas가 이미 있으면 제거 (재구축)
            var oldCanvas = GameObject.Find("BattleUICanvas");
            if (oldCanvas != null)
                Object.DestroyImmediate(oldCanvas);

            // 한글 폰트 로드
            _koreanFont = GetOrCreateKoreanFont();

            // Canvas 생성 (Camera, EventSystem은 이미 씬에 있음)
            var canvas = CreateCanvas(scene);

            // UI 구축
            CreateBattleUI(canvas);

            // 스크립트 세팅 (BattleSceneSetup, BattleUIManager 등)
            SetupScriptsInCurrentScene();

            // 저장
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[BattleSceneBuilder] BattleScene built and saved to {path}");
            AssetDatabase.Refresh();
        }


        private static Canvas CreateCanvas(Scene scene)
        {
            var canvasGO = new GameObject("BattleUICanvas");
            SceneManager.MoveGameObjectToScene(canvasGO, scene);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        #region Main Structure

        private static void CreateBattleUI(Canvas canvas)
        {
            var root = NewRect("BattleUIRoot", canvas.transform);
            SetFillParent(root);
            root.gameObject.AddComponent<Image>().color = BgDark;

            CreateTopBar(root);
            CreateBottomBar(root);

            var content = NewRect("ContentArea", root);
            content.anchorMin = Vector2.zero;
            content.anchorMax = Vector2.one;
            content.offsetMin = new Vector2(0, 100);
            content.offsetMax = new Vector2(0, -60);

            CreateLeftSidebar(content);
            CreateRightSidebar(content);
            CreateCenterArea(content);

            CreateCharacterPopup(root);
        }

        #endregion

        #region Top Bar

        private static void CreateTopBar(RectTransform parent)
        {
            var bar = NewRect("TopBar", parent);
            bar.anchorMin = new Vector2(0, 1);
            bar.anchorMax = new Vector2(1, 1);
            bar.pivot = new Vector2(0.5f, 1);
            bar.sizeDelta = new Vector2(0, 60);
            bar.gameObject.AddComponent<Image>().color = new Color(0.03f, 0.03f, 0.08f, 0.95f);

            // 하단 구분선
            var div = NewRect("Divider", bar);
            div.anchorMin = new Vector2(0, 0);
            div.anchorMax = new Vector2(1, 0);
            div.pivot = new Vector2(0.5f, 0);
            div.sizeDelta = new Vector2(0, 2);
            div.gameObject.AddComponent<Image>().color = AccentRed;

            // 턴 카운터
            var counter = NewRect("TurnCounter", bar);
            counter.anchorMin = new Vector2(0, 0.5f);
            counter.anchorMax = new Vector2(0, 0.5f);
            counter.pivot = new Vector2(0, 0.5f);
            counter.anchoredPosition = new Vector2(20, 0);
            counter.sizeDelta = new Vector2(80, 40);
            var ct = counter.gameObject.AddComponent<TextMeshProUGUI>();
            ct.font = GetOrCreateKoreanFont();
            ct.text = "4/4";
            ct.fontSize = 28;
            ct.fontStyle = FontStyles.Bold;
            ct.alignment = TextAlignmentOptions.Left;
            ct.color = AccentYellow;

            // 턴 종료 버튼
            var btn = NewRect("EndTurnButton", bar);
            btn.anchorMin = new Vector2(1, 0.5f);
            btn.anchorMax = new Vector2(1, 0.5f);
            btn.pivot = new Vector2(1, 0.5f);
            btn.anchoredPosition = new Vector2(-20, 0);
            btn.sizeDelta = new Vector2(160, 40);
            var b = btn.gameObject.AddComponent<Button>();
            var bImg = btn.gameObject.AddComponent<Image>();
            bImg.color = AccentRed;
            b.targetGraphic = bImg;
            var c = b.colors;
            c.highlightedColor = new Color(0.9f, 0.2f, 0.3f);
            c.pressedColor = new Color(0.5f, 0.08f, 0.15f);
            b.colors = c;

            var txt = NewRect("Text", btn);
            SetFillParent(txt);
            var t = txt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = GetOrCreateKoreanFont();
            t.text = "턴 종료 [T]";
            t.fontSize = 18;
            t.fontStyle = FontStyles.Bold;
            t.alignment = TextAlignmentOptions.Center;
            t.color = TextWhite;
        }

        #endregion

        #region Bottom Bar

        private static void CreateBottomBar(RectTransform parent)
        {
            var bar = NewRect("BottomBar", parent);
            bar.anchorMin = Vector2.zero;
            bar.anchorMax = new Vector2(1, 0);
            bar.pivot = new Vector2(0.5f, 0);
            bar.sizeDelta = new Vector2(0, 100);
            bar.gameObject.AddComponent<Image>().color = new Color(0.03f, 0.03f, 0.08f, 0.95f);

            var div = NewRect("Divider", bar);
            div.anchorMin = new Vector2(0, 1);
            div.anchorMax = new Vector2(1, 1);
            div.pivot = new Vector2(0.5f, 1);
            div.sizeDelta = new Vector2(0, 2);
            div.gameObject.AddComponent<Image>().color = AccentRed;

            var turn = NewRect("CurrentTurnText", bar);
            turn.anchorMin = new Vector2(0, 0);
            turn.anchorMax = new Vector2(0.15f, 1);
            turn.offsetMin = Vector2.zero;
            turn.offsetMax = Vector2.zero;
            var tt = turn.gameObject.AddComponent<TextMeshProUGUI>();
            tt.font = GetOrCreateKoreanFont();
            tt.text = "쉘레이아, 턴";
            tt.fontSize = 14;
            tt.fontStyle = FontStyles.Bold;
            tt.alignment = TextAlignmentOptions.Left;
            tt.margin = new Vector4(8, 0, 4, 0);
            tt.color = AccentYellow;
        }

        #endregion

        #region Left Sidebar

        private static void CreateLeftSidebar(RectTransform parent)
        {
            var sidebar = NewRect("LeftSidebar", parent);
            sidebar.anchorMin = Vector2.zero;
            sidebar.anchorMax = new Vector2(0.24f, 1);
            sidebar.offsetMin = new Vector2(5, 5);
            sidebar.offsetMax = new Vector2(-2, -5);
            sidebar.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.1f, 0.8f);

            var vlg = sidebar.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            string[] names = { "카인", "쉘레이아", "아트카나", "샤이비어" };
            string[] hps = { "88/88", "55/55", "45/45", "50/50" };
            string[] skills = { "방어막", "연속 베기", "원죽 방패", "치명 오라" };

            for (int i = 0; i < 4; i++)
                CreatePlayerPanel(sidebar, i + 1, names[i], hps[i], skills[i]);
        }

        private static void CreatePlayerPanel(RectTransform parent, int num, string name, string hp, string skill)
        {
            var panel = NewRect($"CharPanel_{name}", parent);
            panel.sizeDelta = new Vector2(0, 130);
            var panelImg = panel.gameObject.AddComponent<Image>();
            panelImg.color = new Color(0.06f, 0.06f, 0.14f, 0.95f);
            var ol = panel.gameObject.AddComponent<Outline>();
            ol.effectColor = BorderRed;
            ol.effectDistance = new Vector2(1, -1);

            // 패널 클릭 버튼 (팝업 열기용)
            var panelBtn = panel.gameObject.AddComponent<Button>();
            panelBtn.targetGraphic = panelImg;

            // 번호 뱃지
            var badge = NewRect("NumberBadge", panel);
            badge.anchorMin = new Vector2(0, 1);
            badge.anchorMax = new Vector2(0, 1);
            badge.pivot = new Vector2(0, 1);
            badge.anchoredPosition = new Vector2(5, -5);
            badge.sizeDelta = new Vector2(24, 24);
            badge.gameObject.AddComponent<Image>().color = AccentRed;
            var bt = AddText(NewRect("T", badge), num.ToString(), 14, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
            SetFillParent(bt.rectTransform);

            // X 버튼
            var xBtn = NewRect("CloseBtn", panel);
            xBtn.anchorMin = new Vector2(1, 1);
            xBtn.anchorMax = new Vector2(1, 1);
            xBtn.pivot = new Vector2(1, 1);
            xBtn.anchoredPosition = new Vector2(-5, -5);
            xBtn.sizeDelta = new Vector2(20, 20);
            xBtn.gameObject.AddComponent<Button>();
            xBtn.gameObject.AddComponent<Image>().color = new Color(0.5f, 0.1f, 0.1f, 0.8f);
            var xt = AddText(NewRect("T", xBtn), "X", 12, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
            SetFillParent(xt.rectTransform);

            // 이름
            var nRect = NewRect("Name", panel);
            nRect.anchorMin = new Vector2(0, 1);
            nRect.anchorMax = new Vector2(1, 1);
            nRect.pivot = new Vector2(0.5f, 1);
            nRect.anchoredPosition = new Vector2(0, -32);
            nRect.sizeDelta = new Vector2(-16, 22);
            AddTextNoWrap(nRect, name, 15, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

            // HP 바 — 패널 하단 영역에 가로로 넓게
            var hpBar = NewRect("HPBar", panel);
            hpBar.anchorMin = new Vector2(0, 0);
            hpBar.anchorMax = new Vector2(1, 0);
            hpBar.pivot = new Vector2(0.5f, 0);
            hpBar.anchoredPosition = new Vector2(0, 50);
            hpBar.sizeDelta = new Vector2(-16, 20);
            hpBar.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

            var hpFill = NewRect("Fill", hpBar);
            hpFill.anchorMin = Vector2.zero;
            hpFill.anchorMax = new Vector2(1f, 1f);
            hpFill.offsetMin = new Vector2(2, 2);
            hpFill.offsetMax = new Vector2(-2, -2);
            hpFill.gameObject.AddComponent<Image>().color = AccentGreen;

            // HP 텍스트 — 바 위에 가로로 (줄바꿈 방지)
            var hpTxt = NewRect("Text", hpBar);
            SetFillParent(hpTxt);
            AddTextNoWrap(hpTxt, hp, 12, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

            // 퍼센트
            var pct = NewRect("Pct", panel);
            pct.anchorMin = new Vector2(0, 0);
            pct.anchorMax = new Vector2(1, 0);
            pct.pivot = new Vector2(0.5f, 0);
            pct.anchoredPosition = new Vector2(0, 34);
            pct.sizeDelta = new Vector2(-16, 16);
            AddTextNoWrap(pct, "100%", 11, FontStyles.Normal, TextAlignmentOptions.Center, AccentGreen);

            // 스킬명
            var sk = NewRect("Skill", panel);
            sk.anchorMin = new Vector2(0, 0);
            sk.anchorMax = new Vector2(1, 0);
            sk.pivot = new Vector2(0.5f, 0);
            sk.anchoredPosition = new Vector2(0, 12);
            sk.sizeDelta = new Vector2(-16, 18);
            AddTextNoWrap(sk, skill, 11, FontStyles.Normal, TextAlignmentOptions.Center, TextDim);
        }

        #endregion

        #region Center Area

        private static void CreateCenterArea(RectTransform parent)
        {
            var center = NewRect("CenterArea", parent);
            center.anchorMin = new Vector2(0.24f, 0);
            center.anchorMax = new Vector2(0.78f, 1);
            center.offsetMin = new Vector2(3, 5);
            center.offsetMax = new Vector2(-3, -5);

            var hlg = center.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.padding = new RectOffset(12, 12, 12, 12);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            CreateEnemyPanel(center, "고블린", "30/30");
            CreateEnemyPanel(center, "고블린", "30/30");
        }

        private static void CreateEnemyPanel(RectTransform parent, string name, string hp)
        {
            var panel = NewRect($"Enemy_{name}", parent);
            var panelImg = panel.gameObject.AddComponent<Image>();
            panelImg.color = new Color(0.06f, 0.04f, 0.08f, 0.95f);
            var ol = panel.gameObject.AddComponent<Outline>();
            ol.effectColor = BorderRed;
            ol.effectDistance = new Vector2(2, -2);

            // 패널 클릭 버튼 (팝업 열기용)
            var panelBtn = panel.gameObject.AddComponent<Button>();
            panelBtn.targetGraphic = panelImg;

            var vlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(10, 10, 12, 12);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 적 패널 고정 크기 — 수에 관계없이 일정한 크기 유지
            var le = panel.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 180;
            le.minWidth = 120;
            le.preferredHeight = 280;
            le.minHeight = 200;

            // 아바타
            var avatar = NewRect("Avatar", panel);
            avatar.sizeDelta = new Vector2(0, 100);
            avatar.gameObject.AddComponent<Image>().color = AccentRed;
            var aLabel = NewRect("Label", avatar);
            SetFillParent(aLabel);
            AddText(aLabel, "적 초상화", 13, FontStyles.Normal, TextAlignmentOptions.Center, new Color(1, 1, 1, 0.5f));

            // 이름
            var nRect = NewRect("Name", panel);
            nRect.sizeDelta = new Vector2(0, 24);
            AddText(nRect, name, 17, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

            // HP 바 (빨강)
            var hpCont = NewRect("HPBarContainer", panel);
            hpCont.sizeDelta = new Vector2(0, 24);
            hpCont.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.1f, 0.1f);

            var fill = NewRect("Fill", hpCont);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(1f, 1f);
            fill.offsetMin = new Vector2(2, 2);
            fill.offsetMax = new Vector2(-2, -2);
            fill.gameObject.AddComponent<Image>().color = AccentRed;

            var hpText = NewRect("HPText", hpCont);
            SetFillParent(hpText);
            AddText(hpText, hp, 14, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

            // 버튼 영역
            var btnArea = NewRect("ButtonArea", panel);
            btnArea.sizeDelta = new Vector2(0, 40);
            var bhlg = btnArea.gameObject.AddComponent<HorizontalLayoutGroup>();
            bhlg.spacing = 8;
            bhlg.childAlignment = TextAnchor.MiddleCenter;
            bhlg.childControlWidth = false;
            bhlg.childControlHeight = false;

            CreateActionBtn(btnArea, "가디언", AccentRed);
            CreateActionBtn(btnArea, "아크카", new Color(0.4f, 0.15f, 0.55f));

            // 수량 정보
            var info = NewRect("Info", panel);
            info.sizeDelta = new Vector2(0, 22);
            AddText(info, "수량: 상시발동", 12, FontStyles.Normal, TextAlignmentOptions.Center, TextDim);
        }

        private static void CreateActionBtn(RectTransform parent, string label, Color bg)
        {
            var btn = NewRect($"Btn_{label}", parent);
            btn.sizeDelta = new Vector2(110, 36);
            var b = btn.gameObject.AddComponent<Button>();
            var img = btn.gameObject.AddComponent<Image>();
            img.color = bg;
            b.targetGraphic = img;

            var t = NewRect("T", btn);
            SetFillParent(t);
            AddText(t, label, 14, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
        }

        #endregion

        #region Right Sidebar

        private static void CreateRightSidebar(RectTransform parent)
        {
            var sidebar = NewRect("RightSidebar", parent);
            sidebar.anchorMin = new Vector2(0.78f, 0);
            sidebar.anchorMax = new Vector2(1, 1);
            sidebar.offsetMin = new Vector2(2, 5);
            sidebar.offsetMax = new Vector2(-5, -5);
            sidebar.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.1f, 0.8f);
            var ol = sidebar.gameObject.AddComponent<Outline>();
            ol.effectColor = BorderRed;
            ol.effectDistance = new Vector2(1, -1);

            // 타이틀
            var title = NewRect("Title", sidebar);
            title.anchorMin = new Vector2(0, 1);
            title.anchorMax = new Vector2(1, 1);
            title.pivot = new Vector2(0.5f, 1);
            title.sizeDelta = new Vector2(0, 36);
            title.gameObject.AddComponent<Image>().color = new Color(0.05f, 0.03f, 0.08f, 0.9f);
            var tRect = NewRect("T", title);
            SetFillParent(tRect);
            AddText(tRect, "전투 로그", 16, FontStyles.Bold, TextAlignmentOptions.Center, AccentYellow);

            // 구분선
            var div = NewRect("Divider", sidebar);
            div.anchorMin = new Vector2(0, 1);
            div.anchorMax = new Vector2(1, 1);
            div.pivot = new Vector2(0.5f, 1);
            div.anchoredPosition = new Vector2(0, -36);
            div.sizeDelta = new Vector2(0, 2);
            div.gameObject.AddComponent<Image>().color = AccentRed;

            // 로그 텍스트
            var log = NewRect("LogText", sidebar);
            log.anchorMin = Vector2.zero;
            log.anchorMax = Vector2.one;
            log.offsetMin = new Vector2(10, 10);
            log.offsetMax = new Vector2(-10, -40);
            var lt = log.gameObject.AddComponent<TextMeshProUGUI>();
            lt.font = GetOrCreateKoreanFont();
            lt.text = "전투가 시작되었습니다.\n\n카인이 방어막을 사용했습니다.\n\n쉘레이아의 턴입니다.";
            lt.fontSize = 14;
            lt.alignment = TextAlignmentOptions.TopLeft;
            lt.color = TextDim;
            lt.enableWordWrapping = true;
            lt.overflowMode = TextOverflowModes.Ellipsis;
            if (_koreanFont != null)
                lt.font = _koreanFont;
        }

        #endregion

        #region Utilities

        private static void CreateBar(RectTransform parent, string name, string text, float ratio, Color fillCol, float yOffset, Vector2 size)
        {
            var bar = NewRect(name, parent);
            bar.anchorMin = new Vector2(0.5f, 0.5f);
            bar.anchorMax = new Vector2(0.5f, 0.5f);
            bar.pivot = new Vector2(0.5f, 0.5f);
            bar.anchoredPosition = new Vector2(0, yOffset);
            bar.sizeDelta = size;
            bar.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

            var fill = NewRect("Fill", bar);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(ratio, 1f);
            fill.offsetMin = new Vector2(2, 2);
            fill.offsetMax = new Vector2(-2, -2);
            fill.gameObject.AddComponent<Image>().color = fillCol;

            var tRect = NewRect("Text", bar);
            SetFillParent(tRect);
            AddText(tRect, text, 13, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static RectTransform NewRect(string name, RectTransform parent)
        {
            return NewRect(name, parent.transform);
        }

        private static void SetFillParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI AddText(RectTransform parent, string text, float size, FontStyles style, TextAlignmentOptions align, Color color)
        {
            var tmp = parent.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = color;
            tmp.font = GetOrCreateKoreanFont();
            return tmp;
        }

        /// <summary>
        /// 줄바꿈 방지 텍스트 — 좁은 영역에서 세로 표시 방지
        /// </summary>
        private static TextMeshProUGUI AddTextNoWrap(RectTransform parent, string text, float size, FontStyles style, TextAlignmentOptions align, Color color)
        {
            var tmp = AddText(parent, text, size, style, align, color);
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        #endregion

        #region Character Popup

        [MenuItem("Tools/Battle UI/Setup Scripts in Current Scene", false, 101)]
        public static void SetupScriptsInCurrentScene()
        {
            var root = GameObject.Find("BattleUIRoot");
            if (root == null)
            {
                Debug.LogError("[Setup] BattleUIRoot not found!");
                return;
            }

            // 1) BattleUIManager 추가
            var uiManager = root.GetComponent<BattleUIManager>();
            if (uiManager == null)
                uiManager = root.AddComponent<BattleUIManager>();

            // 2) TopBar에 TopBarUI 추가
            var topBar = root.transform.Find("TopBar");
            if (topBar != null)
            {
                var topBarUI = topBar.GetComponent<TopBarUI>();
                if (topBarUI == null)
                    topBar.gameObject.AddComponent<TopBarUI>();

                var turnCounter = topBar.Find("TurnCounter");
                if (turnCounter != null)
                {
                    var tmp = turnCounter.GetComponent<TMPro.TextMeshProUGUI>();
                    if (tmp != null)
                        tmp.text = "1/4";
                }
            }

            // 3) BottomBar에 ActionBarUI 추가 (필요한 자식 오브젝트도 생성)
            var bottomBar = root.transform.Find("BottomBar");
            if (bottomBar != null)
            {
                SetupActionBar(bottomBar);
            }

            // 4) LeftSidebar 패널에 PlayerSidebarPanel 추가
            var leftSidebar = root.transform.Find("ContentArea/LeftSidebar");
            if (leftSidebar != null)
            {
                int idx = 0;
                foreach (Transform child in leftSidebar)
                {
                    var psp = child.GetComponent<PlayerSidebarPanel>();
                    if (psp == null)
                        child.gameObject.AddComponent<PlayerSidebarPanel>();
                    idx++;
                }
            }

            // 5) CenterArea 패널에 EnemyDetailPanel 추가
            var centerArea = root.transform.Find("ContentArea/CenterArea");
            if (centerArea != null)
            {
                foreach (Transform child in centerArea)
                {
                    var edp = child.GetComponent<EnemyDetailPanel>();
                    if (edp == null)
                        child.gameObject.AddComponent<EnemyDetailPanel>();
                }
            }

            // 6) BattleLogUI 추가
            var rightSidebar = root.transform.Find("ContentArea/RightSidebar");
            if (rightSidebar != null)
            {
                var logUI = rightSidebar.GetComponent<BattleLogUI>();
                if (logUI == null)
                    rightSidebar.gameObject.AddComponent<BattleLogUI>();
            }

            // 7) BattleSceneSetup 추가
            var setupGO = GameObject.Find("BattleSceneSetup");
            if (setupGO == null)
            {
                setupGO = new GameObject("BattleSceneSetup");
                setupGO.transform.SetParent(root.transform.parent); // Canvas 아래
            }
            var sceneSetup = setupGO.GetComponent<BattleSceneSetup>();
            if (sceneSetup == null)
                sceneSetup = setupGO.AddComponent<BattleSceneSetup>();

            // CharacterData 에셋 로드해서 할당
            var warriorData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Char_Warrior.asset");
            var mageData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Char_Mage.asset");
            var healerData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Char_Healer.asset");
            var rogueData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Char_Rogue.asset");

            SetPrivateField(sceneSetup, "_testWarriorData", warriorData);
            SetPrivateField(sceneSetup, "_testMageData", mageData);
            SetPrivateField(sceneSetup, "_testHealerData", healerData);
            SetPrivateField(sceneSetup, "_testRogueData", rogueData);
            SetPrivateField(sceneSetup, "_battleUIManager", uiManager);

            // ActionBarUI 찾아서 할당
            var actionBar = bottomBar?.GetComponent<ActionBarUI>();
            if (actionBar != null)
                SetPrivateField(sceneSetup, "_actionBar", actionBar);

            // EnemyData 배열
            var slimeData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Enemy_Slime.asset");
            var goblinData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/03.Data/Characters/Enemy_Goblin.asset");
            SetPrivateField(sceneSetup, "_testEnemyData", new CharacterData[] { goblinData, goblinData });

            EditorUtility.SetDirty(sceneSetup);

            // 8) UI 참조 자동 연결
            AutoWireBattleUIManager(uiManager);
            if (actionBar != null)
                AutoWireActionBar(actionBar);

            // 9) 정적 패널 제거 (런타임에 프리팹으로 동적 생성하므로 빌더가 만든 샘플 패널은 삭제)
            RemoveStaticPanels(leftSidebar, centerArea);

            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Setup] 스크립트 세팅 완료! 씬을 저장하세요.");
        }

        private static void SetupActionBar(Transform bottomBar)
        {
            var actionBar = bottomBar.GetComponent<ActionBarUI>();
            if (actionBar == null)
                actionBar = bottomBar.gameObject.AddComponent<ActionBarUI>();

            // 액션 슬롯 컨테이너 추가
            var slotContainer = bottomBar.Find("ActionSlotContainer");
            if (slotContainer == null)
            {
                var container = NewRect("ActionSlotContainer", bottomBar as RectTransform);
                container.anchorMin = new Vector2(0.15f, 0);
                container.anchorMax = new Vector2(0.85f, 1);
                container.offsetMin = Vector2.zero;
                container.offsetMax = Vector2.zero;

                var hlg = container.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 8;
                hlg.padding = new RectOffset(12, 12, 10, 10);
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = true;
            }

            // 액션 디테일 패널 (숨김)
            var detailPanel = bottomBar.Find("ActionDetailPanel");
            if (detailPanel == null)
            {
                var detail = NewRect("ActionDetailPanel", bottomBar as RectTransform);
                detail.anchorMin = new Vector2(0, 1);
                detail.anchorMax = new Vector2(1, 1);
                detail.pivot = new Vector2(0.5f, 0);
                detail.anchoredPosition = new Vector2(0, 0);
                detail.sizeDelta = new Vector2(0, 80);
                detail.gameObject.SetActive(false);
                detail.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.1f, 0.95f);

                var titleT = NewRect("TitleText", detail);
                titleT.anchorMin = new Vector2(0, 0.5f);
                titleT.anchorMax = new Vector2(0.5f, 1);
                titleT.offsetMin = new Vector2(12, 4);
                titleT.offsetMax = new Vector2(-4, -4);
                AddText(titleT, "스킬명", 16, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);

                var descT = NewRect("DescText", detail);
                descT.anchorMin = new Vector2(0.5f, 0.5f);
                descT.anchorMax = new Vector2(1, 1);
                descT.offsetMin = new Vector2(4, 4);
                descT.offsetMax = new Vector2(-12, -4);
                AddText(descT, "설명", 13, FontStyles.Normal, TextAlignmentOptions.Left, TextDim);
            }

            // 액션 슬롯 프리팹 생성
            CreateActionSlotPrefab();

            // 취소 버튼 연결
            var cancelButton = bottomBar.Find("ActionDetailPanel")?.Find("CancelButton")?.GetComponent<Button>();
        }

        private static void CreateActionSlotPrefab()
        {
            const string prefabPath = "Assets/03.Data/Prefabs/ActionSlotUI.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return;

            // 폴더 생성
            if (!AssetDatabase.IsValidFolder("Assets/03.Data/Prefabs"))
                AssetDatabase.CreateFolder("Assets/03.Data", "Prefabs");

            var go = new GameObject("ActionSlotUI");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 80);

            // LayoutElement로 고정 크기 지정 (childForceExpandWidth = false 환경에서 preferredSize 사용)
            var layoutEl = go.AddComponent<LayoutElement>();
            layoutEl.preferredWidth = 150;
            layoutEl.preferredHeight = 80;
            layoutEl.minWidth = 150;
            layoutEl.minHeight = 80;

            go.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.18f, 0.9f);
            go.AddComponent<Outline>().effectColor = BorderRed;
            go.AddComponent<Button>();

            // 스킬 아이콘 (60x60, 좌측 중앙)
            var icon = NewRect("SkillIcon", rect);
            icon.anchorMin = new Vector2(0, 0.5f);
            icon.anchorMax = new Vector2(0, 0.5f);
            icon.pivot = new Vector2(0, 0.5f);
            icon.anchoredPosition = new Vector2(4, 0);
            icon.sizeDelta = new Vector2(60, 60);
            icon.gameObject.AddComponent<Image>().color = AccentRed;

            // 스킬명 텍스트 (상단 영역, 아이콘 오른쪽)
            var nameT = NewRect("SkillNameText", rect);
            nameT.anchorMin = new Vector2(0, 0.5f);
            nameT.anchorMax = new Vector2(1, 1);
            nameT.offsetMin = new Vector2(68, -4);
            nameT.offsetMax = new Vector2(-32, -4);
            AddText(nameT, "---", 16, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);

            // 비용 뱃지 (우측 상단 28x28 원형)
            var costBadge = NewRect("CostBadge", rect);
            costBadge.anchorMin = new Vector2(1, 1);
            costBadge.anchorMax = new Vector2(1, 1);
            costBadge.pivot = new Vector2(1, 1);
            costBadge.anchoredPosition = new Vector2(-4, -4);
            costBadge.sizeDelta = new Vector2(28, 28);
            costBadge.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.4f, 0.8f, 0.9f);

            // 비용 텍스트 (뱃지 내부)
            var costT = NewRect("CostText", costBadge);
            SetFillParent(costT);
            AddText(costT, "0", 14, FontStyles.Bold, TextAlignmentOptions.Center, Color.black);

            // 시전자명 텍스트 (하단 영역, 아이콘 오른쪽, 작은 흐린 글씨)
            var casterT = NewRect("CasterNameText", rect);
            casterT.anchorMin = new Vector2(0, 0);
            casterT.anchorMax = new Vector2(1, 0.5f);
            casterT.offsetMin = new Vector2(68, 4);
            casterT.offsetMax = new Vector2(-8, 4);
            AddText(casterT, "", 12, FontStyles.Normal, TextAlignmentOptions.Left, TextDim);

            // 선택 테두리
            var selBorder = NewRect("SelectionBorder", rect);
            SetFillParent(selBorder);
            selBorder.gameObject.AddComponent<Image>().color = Color.clear;
            var selOutline = selBorder.gameObject.AddComponent<Outline>();
            selOutline.effectColor = AccentYellow;
            selOutline.effectDistance = new Vector2(3, -3);
            selBorder.gameObject.SetActive(false);

            // 실행 순서 뱃지
            var orderBadge = NewRect("ExecutionOrderBadge", rect);
            orderBadge.anchorMin = new Vector2(1, 1);
            orderBadge.anchorMax = new Vector2(1, 1);
            orderBadge.pivot = new Vector2(1, 1);
            orderBadge.anchoredPosition = new Vector2(-2, -36);
            orderBadge.sizeDelta = new Vector2(24, 24);
            orderBadge.gameObject.AddComponent<Image>().color = AccentYellow;
            var orderText = NewRect("OrderText", orderBadge);
            SetFillParent(orderText);
            AddText(orderText, "1", 12, FontStyles.Bold, TextAlignmentOptions.Center, Color.black);
            orderBadge.gameObject.SetActive(false);

            // 할당 오버레이
            var assigned = NewRect("AssignedOverlay", rect);
            SetFillParent(assigned);
            assigned.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.4f, 0.15f);
            assigned.gameObject.SetActive(false);

            // ActionSlotUI 컴포넌트 추가
            var slotUI = go.AddComponent<ActionSlotUI>();

            // 프리팹 내부 필드 자동 와이어링
            SetPrivateField(slotUI, "_skillIcon", icon.gameObject.GetComponent<Image>());
            SetPrivateField(slotUI, "_skillNameText", nameT.gameObject.GetComponent<TMPro.TextMeshProUGUI>());
            SetPrivateField(slotUI, "_costText", costT.gameObject.GetComponent<TMPro.TextMeshProUGUI>());
            SetPrivateField(slotUI, "_casterNameText", casterT.gameObject.GetComponent<TMPro.TextMeshProUGUI>());
            SetPrivateField(slotUI, "_selectionBorder", selBorder.gameObject);
            SetPrivateField(slotUI, "_executionOrderBadge", orderBadge.gameObject);
            SetPrivateField(slotUI, "_executionOrderText", orderText.gameObject.GetComponent<TMPro.TextMeshProUGUI>());
            SetPrivateField(slotUI, "_assignedOverlay", assigned.gameObject);
            SetPrivateField(slotUI, "_button", go.GetComponent<Button>());

            // 프리팹 저장
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            Debug.Log("[Setup] ActionSlotUI prefab created at " + prefabPath);
        }

        private static void AutoWireBattleUIManager(BattleUIManager uiManager)
        {
            var root = uiManager.transform;

            // TopBar
            var topBar = root.Find("TopBar");
            if (topBar != null)
                SetPrivateField(uiManager, "_topBar", topBar.GetComponent<TopBarUI>() ?? topBar.gameObject.AddComponent<TopBarUI>());

            // Player panel container
            var leftSidebar = root.Find("ContentArea/LeftSidebar");
            if (leftSidebar != null)
                SetPrivateField(uiManager, "_playerPanelContainer", leftSidebar);

            // Player panel prefab - 첫 번째 패널로부터 생성 (항상 덮어쓰기)
            var firstPanel = leftSidebar?.GetChild(0);
            if (firstPanel != null)
            {
                const string prefabPath = "Assets/03.Data/Prefabs/PlayerSidebarPanel.prefab";
                if (!AssetDatabase.IsValidFolder("Assets/03.Data/Prefabs"))
                    AssetDatabase.CreateFolder("Assets/03.Data", "Prefabs");
                // 기존 프리팹이 있으면 삭제 후 재생성
                AssetDatabase.DeleteAsset(prefabPath);
                var prefab = PrefabUtility.SaveAsPrefabAsset(firstPanel.gameObject, prefabPath);
                SetPrivateField(uiManager, "_playerPanelPrefab", prefab != null ? prefab.GetComponent<PlayerSidebarPanel>() : null);
            }

            // Enemy panel container
            var centerArea = root.Find("ContentArea/CenterArea");
            if (centerArea != null)
                SetPrivateField(uiManager, "_enemyPanelContainer", centerArea);

            // Enemy panel prefab (항상 덮어쓰기)
            var firstEnemy = centerArea?.GetChild(0);
            if (firstEnemy != null)
            {
                const string prefabPath = "Assets/03.Data/Prefabs/EnemyDetailPanel.prefab";
                if (!AssetDatabase.IsValidFolder("Assets/03.Data/Prefabs"))
                    AssetDatabase.CreateFolder("Assets/03.Data", "Prefabs");
                AssetDatabase.DeleteAsset(prefabPath);
                var prefab = PrefabUtility.SaveAsPrefabAsset(firstEnemy.gameObject, prefabPath);
                SetPrivateField(uiManager, "_enemyPanelPrefab", prefab != null ? prefab.GetComponent<EnemyDetailPanel>() : null);
            }

            // BattleLog
            var rightSidebar = root.Find("ContentArea/RightSidebar");
            if (rightSidebar != null)
                SetPrivateField(uiManager, "_battleLog", rightSidebar.GetComponent<BattleLogUI>());

            // CurrentTurnText
            var currentTurnText = root.Find("BottomBar/CurrentTurnText");
            if (currentTurnText != null)
                SetPrivateField(uiManager, "_currentTurnText", currentTurnText.GetComponent<TMPro.TextMeshProUGUI>());

            // CharacterPopup
            var popup = root.Find("CharacterPopup");
            if (popup != null)
                SetPrivateField(uiManager, "_characterPopup", popup.GetComponent<CharacterPopupUI>());

            EditorUtility.SetDirty(uiManager);
        }

        private static void AutoWireActionBar(ActionBarUI actionBar)
        {
            var bottomBar = actionBar.transform;

            var slotContainer = bottomBar.Find("ActionSlotContainer");
            SetPrivateField(actionBar, "_actionMenuContainer", slotContainer);

            var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03.Data/Prefabs/ActionSlotUI.prefab");
            if (slotPrefab != null)
                SetPrivateField(actionBar, "_actionSlotPrefab", slotPrefab.GetComponent<ActionSlotUI>());

            var detailPanel = bottomBar.Find("ActionDetailPanel");
            SetPrivateField(actionBar, "_actionDetailPanel", detailPanel?.gameObject);

            var titleText = detailPanel?.Find("TitleText");
            SetPrivateField(actionBar, "_actionTitleText", titleText?.GetComponent<TMPro.TextMeshProUGUI>());

            var descText = detailPanel?.Find("DescText");
            SetPrivateField(actionBar, "_actionDescText", descText?.GetComponent<TMPro.TextMeshProUGUI>());

            // EndTurnButton - TopBar에 있음
            var endTurnBtn = actionBar.transform.root.Find("BattleUIRoot/TopBar/EndTurnButton");
            SetPrivateField(actionBar, "_endTurnButton", endTurnBtn?.GetComponent<Button>());

            EditorUtility.SetDirty(actionBar);
        }

        private static readonly Color PopupBg = new Color(0.02f, 0.02f, 0.06f, 0.95f);
        private static readonly Color PopupPanelBg = new Color(0.05f, 0.05f, 0.12f, 0.98f);
        private static readonly Color PopupHeaderBg = new Color(0.04f, 0.03f, 0.08f, 0.95f);
        private static readonly Color EntryBg = new Color(0.07f, 0.07f, 0.15f, 0.9f);
        private static readonly Color AccentPurple = new Color(0.4f, 0.15f, 0.55f);

        private static void CreateCharacterPopup(RectTransform parent)
        {
            // 전체 화면 오버레이 (클릭하면 닫히는 배경)
            var overlay = NewRect("CharacterPopup", parent);
            SetFillParent(overlay);
            overlay.gameObject.SetActive(false);

            var bgBtn = overlay.gameObject.AddComponent<Button>();
            var bgImg = overlay.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.6f);
            bgBtn.targetGraphic = bgImg;

            var popup = overlay.gameObject.AddComponent<CharacterPopupUI>();

            // 패널 (중앙 정렬, 520x620)
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

            // ── 헤더 ──
            var header = NewRect("Header", panel);
            header.anchorMin = new Vector2(0, 1);
            header.anchorMax = new Vector2(1, 1);
            header.pivot = new Vector2(0.5f, 1);
            header.sizeDelta = new Vector2(0, 56);
            header.gameObject.AddComponent<Image>().color = PopupHeaderBg;

            // 초상화 아이콘
            var portrait = NewRect("Portrait", header);
            portrait.anchorMin = new Vector2(0, 0.5f);
            portrait.anchorMax = new Vector2(0, 0.5f);
            portrait.pivot = new Vector2(0, 0.5f);
            portrait.anchoredPosition = new Vector2(12, 0);
            portrait.sizeDelta = new Vector2(44, 44);
            portrait.gameObject.AddComponent<Image>().color = AccentRed;

            // 이름
            var nameRect = NewRect("Name", header);
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.offsetMin = new Vector2(64, 0);
            nameRect.offsetMax = new Vector2(-44, -4);
            var nameTmp = AddText(nameRect, "캐릭터명", 20, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);

            // 클래스
            var classRect = NewRect("Class", header);
            classRect.anchorMin = new Vector2(0, 0);
            classRect.anchorMax = new Vector2(1, 0.5f);
            classRect.offsetMin = new Vector2(64, 4);
            classRect.offsetMax = new Vector2(-44, 0);
            AddText(classRect, "클래스", 13, FontStyles.Normal, TextAlignmentOptions.Left, TextDim);

            // 닫기 버튼
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

            // ── HP 바 ──
            var hpArea = NewRect("HPArea", panel);
            hpArea.anchorMin = new Vector2(0, 1);
            hpArea.anchorMax = new Vector2(1, 1);
            hpArea.pivot = new Vector2(0.5f, 1);
            hpArea.anchoredPosition = new Vector2(0, -62);
            hpArea.sizeDelta = new Vector2(-24, 28);

            var hpBg = NewRect("HPBarBg", hpArea);
            hpBg.anchorMin = Vector2.zero;
            hpBg.anchorMax = new Vector2(1, 1);
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
            hpLabel.sizeDelta = new Vector2(76, 0);
            AddText(hpLabel, "HP 55/55", 14, FontStyles.Bold, TextAlignmentOptions.Right, TextWhite);

            // ── 스탯 영역 ──
            var statsArea = NewRect("StatsArea", panel);
            statsArea.anchorMin = new Vector2(0, 1);
            statsArea.anchorMax = new Vector2(1, 1);
            statsArea.pivot = new Vector2(0.5f, 1);
            statsArea.anchoredPosition = new Vector2(0, -94);
            statsArea.sizeDelta = new Vector2(-24, 24);

            var atkRect = NewRect("ATK", statsArea);
            atkRect.anchorMin = Vector2.zero;
            atkRect.anchorMax = new Vector2(0.5f, 1);
            atkRect.offsetMax = new Vector2(-4, 0);
            atkRect.gameObject.AddComponent<Image>().color = EntryBg;
            AddText(NewRect("T", atkRect), "ATK 10", 14, FontStyles.Bold, TextAlignmentOptions.Center, AccentRed);

            var defRect = NewRect("DEF", statsArea);
            defRect.anchorMin = new Vector2(0.5f, 0);
            defRect.anchorMax = Vector2.one;
            defRect.offsetMin = new Vector2(4, 0);
            defRect.gameObject.AddComponent<Image>().color = EntryBg;
            AddText(NewRect("T", defRect), "DEF 5", 14, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.3f, 0.6f, 0.9f));

            // ── 탭 영역 ──
            var tabArea = NewRect("TabArea", panel);
            tabArea.anchorMin = new Vector2(0, 1);
            tabArea.anchorMax = new Vector2(1, 1);
            tabArea.pivot = new Vector2(0.5f, 1);
            tabArea.anchoredPosition = new Vector2(0, -122);
            tabArea.sizeDelta = new Vector2(0, 36);
            tabArea.gameObject.AddComponent<Image>().color = new Color(0.03f, 0.03f, 0.06f, 0.5f);

            // 탭1: 스킬 목록
            var tab1 = NewRect("TabSkill", tabArea);
            tab1.anchorMin = Vector2.zero;
            tab1.anchorMax = new Vector2(0.5f, 1);
            var t1Btn = tab1.gameObject.AddComponent<Button>();
            t1Btn.gameObject.AddComponent<Image>().color = Color.clear;
            var t1Label = NewRect("T", tab1);
            SetFillParent(t1Label);
            AddText(t1Label, "스킬 목록", 14, FontStyles.Bold, TextAlignmentOptions.Center, AccentYellow);

            // 탭1 인디케이터 (활성 상태 표시줄)
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
            var t2Btn = tab2.gameObject.AddComponent<Button>();
            t2Btn.gameObject.AddComponent<Image>().color = Color.clear;
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

            // ── 스킬 콘텐츠 영역 ──
            var skillContent = NewRect("SkillContent", panel);
            skillContent.anchorMin = new Vector2(0, 0);
            skillContent.anchorMax = new Vector2(1, 1);
            skillContent.offsetMin = new Vector2(12, 12);
            skillContent.offsetMax = new Vector2(-12, -162);

            var skillVlg = skillContent.gameObject.AddComponent<VerticalLayoutGroup>();
            skillVlg.spacing = 6;
            skillVlg.padding = new RectOffset(0, 0, 4, 4);
            skillVlg.childAlignment = TextAnchor.UpperCenter;
            skillVlg.childControlWidth = true;
            skillVlg.childControlHeight = false;
            skillVlg.childForceExpandWidth = true;
            skillVlg.childForceExpandHeight = false;

            var skillCsf = skillContent.gameObject.AddComponent<ContentSizeFitter>();
            skillCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 샘플 스킬 항목들 (런타임에 동적으로 생성됨, 여기서는 프리팹 대용)
            CreatePopupSkillEntry(skillContent, "난도질", 1, "적 1회에게 10 피해.", SkillType.Attack);
            CreatePopupSkillEntry(skillContent, "처엄타", 2, "적 1회에게 18 피해. 회피 +50%.", SkillType.Attack);
            CreatePopupSkillEntry(skillContent, "연속 베기", 2, "적 1회에게 8+8 피해 (2회 공격).", SkillType.Attack);

            // ── 상태 효과 콘텐츠 영역 (숨김) ──
            var statusContent = NewRect("StatusContent", panel);
            statusContent.anchorMin = new Vector2(0, 0);
            statusContent.anchorMax = new Vector2(1, 1);
            statusContent.offsetMin = new Vector2(12, 12);
            statusContent.offsetMax = new Vector2(-12, -162);
            statusContent.gameObject.SetActive(false);

            var statusVlg = statusContent.gameObject.AddComponent<VerticalLayoutGroup>();
            statusVlg.spacing = 6;
            statusVlg.padding = new RectOffset(0, 0, 4, 4);
            statusVlg.childAlignment = TextAnchor.UpperCenter;
            statusVlg.childControlWidth = true;
            statusVlg.childControlHeight = false;
            statusVlg.childForceExpandWidth = true;
            statusVlg.childForceExpandHeight = false;

            // 상태 효과 샘플
            CreatePopupStatusEntry(statusContent, "독", "5 (2턴)");
            CreatePopupStatusEntry(statusContent, "공격 증가", "3 (1턴)");
        }

        private static void CreatePopupSkillEntry(RectTransform parent, string skillName, int level, string desc, SkillType type)
        {
            var entry = NewRect("SkillEntry", parent);
            entry.sizeDelta = new Vector2(0, 72);
            entry.gameObject.AddComponent<Image>().color = EntryBg;

            // 상단: 아이콘 + 이름 + 레벨
            var topRow = NewRect("TopRow", entry);
            topRow.anchorMin = new Vector2(0, 0.5f);
            topRow.anchorMax = new Vector2(1, 1);
            topRow.offsetMin = new Vector2(8, 0);
            topRow.offsetMax = new Vector2(-8, -4);

            // 스킬 아이콘
            var icon = NewRect("Icon", topRow);
            icon.anchorMin = new Vector2(0, 0);
            icon.anchorMax = new Vector2(0, 1);
            icon.pivot = new Vector2(0, 0.5f);
            icon.sizeDelta = new Vector2(28, 28);
            icon.gameObject.AddComponent<Image>().color = GetSkillTypeColor(type);

            // 스킬명
            var nameRect = NewRect("Name", topRow);
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(0.7f, 1);
            nameRect.offsetMin = new Vector2(36, 0);
            AddText(nameRect, skillName, 15, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);

            // 레벨 뱃지
            var lvlRect = NewRect("Level", topRow);
            lvlRect.anchorMin = new Vector2(1, 0.5f);
            lvlRect.anchorMax = new Vector2(1, 0.5f);
            lvlRect.pivot = new Vector2(1, 0.5f);
            lvlRect.sizeDelta = new Vector2(50, 22);
            lvlRect.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.1f, 0.02f, 0.9f);
            var lvlTxt = NewRect("T", lvlRect);
            SetFillParent(lvlTxt);
            AddText(lvlTxt, $"Lv.{level}", 12, FontStyles.Bold, TextAlignmentOptions.Center, AccentYellow);

            // 하단: 설명 + 타입
            var bottomRow = NewRect("BottomRow", entry);
            bottomRow.anchorMin = new Vector2(0, 0);
            bottomRow.anchorMax = new Vector2(1, 0.5f);
            bottomRow.offsetMin = new Vector2(44, 4);
            bottomRow.offsetMax = new Vector2(-8, 0);

            // 설명
            var descRect = NewRect("Desc", bottomRow);
            descRect.anchorMin = Vector2.zero;
            descRect.anchorMax = new Vector2(0.65f, 1);
            AddText(descRect, desc, 12, FontStyles.Normal, TextAlignmentOptions.Left, TextDim);

            // 타입 뱃지
            var typeRect = NewRect("Type", bottomRow);
            typeRect.anchorMin = new Vector2(1, 0);
            typeRect.anchorMax = new Vector2(1, 1);
            typeRect.pivot = new Vector2(1, 0.5f);
            typeRect.sizeDelta = new Vector2(60, 20);
            typeRect.gameObject.AddComponent<Image>().color = GetSkillTypeColor(type);
            var typeTxt = NewRect("T", typeRect);
            SetFillParent(typeTxt);
            AddText(typeTxt, GetSkillTypeLabel(type), 11, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
        }

        private static void CreatePopupStatusEntry(RectTransform parent, string effectName, string valueText)
        {
            var entry = NewRect("StatusEntry", parent);
            entry.sizeDelta = new Vector2(0, 36);
            entry.gameObject.AddComponent<Image>().color = EntryBg;

            var nameRect = NewRect("Name", entry);
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = new Vector2(0.6f, 1);
            nameRect.offsetMin = new Vector2(12, 0);
            AddText(nameRect, effectName, 14, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);

            var valRect = NewRect("Value", entry);
            valRect.anchorMin = new Vector2(0.6f, 0);
            valRect.anchorMax = Vector2.one;
            valRect.offsetMax = new Vector2(-12, 0);
            AddText(valRect, valueText, 13, FontStyles.Normal, TextAlignmentOptions.Right, AccentYellow);
        }

        private static Color GetSkillTypeColor(SkillType type) => type switch
        {
            SkillType.Attack => AccentRed,
            SkillType.Heal => AccentGreen,
            SkillType.Buff => AccentYellow,
            SkillType.Debuff => AccentPurple,
            _ => Color.gray
        };

        private static string GetSkillTypeLabel(SkillType type) => type switch
        {
            SkillType.Attack => "공격",
            SkillType.Heal => "치유",
            SkillType.Buff => "강화",
            SkillType.Debuff => "약화",
            _ => type.ToString()
        };

        #endregion

        private static void RemoveStaticPanels(Transform leftSidebar, Transform centerArea)
        {
            if (leftSidebar != null)
            {
                for (int i = leftSidebar.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(leftSidebar.GetChild(i).gameObject);
                Debug.Log("[Setup] LeftSidebar static panels removed");
            }

            if (centerArea != null)
            {
                for (int i = centerArea.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(centerArea.GetChild(i).gameObject);
                Debug.Log("[Setup] CenterArea static panels removed");
            }
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}
