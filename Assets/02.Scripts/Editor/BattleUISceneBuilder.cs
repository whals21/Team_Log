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
    /// <summary>
    /// Battle UI 씬 빌더 — 진입점, 상수, 폰트 유틸
    /// UI 생성: BattleUISceneBuilder.UI.cs
    /// 스크립트 연결: BattleUISceneBuilder.Setup.cs
    /// </summary>
    public partial class BattleUISceneBuilder
    {
        // ── 색상 팔레트 ──
        private static readonly Color BgDark = new Color(0.08f, 0.08f, 0.16f);
        private static readonly Color AccentRed = new Color(0.77f, 0.12f, 0.23f);
        private static readonly Color AccentGreen = new Color(0.15f, 0.68f, 0.38f);
        private static readonly Color AccentYellow = new Color(0.96f, 0.82f, 0.25f);
        private static readonly Color BorderRed = new Color(0.6f, 0.1f, 0.18f, 0.8f);
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextDim = new Color(0.82f, 0.82f, 0.87f);
        private static readonly Color ShieldBrown = new Color(0.72f, 0.45f, 0.2f);

        // ── 남색 톤 패널 색상 (목업 #1e2a3e 계열 통일) ──
        private static readonly Color PanelBgNavy = new Color(0.12f, 0.16f, 0.24f, 0.95f);   // #1e2a3e
        private static readonly Color TopBarBgNavy = new Color(0.09f, 0.13f, 0.24f, 0.95f);  // #16213e
        private static readonly Color BottomBarBgNavy = new Color(0.08f, 0.08f, 0.14f, 0.95f);
        private static readonly Color SlotBgNavy = new Color(0.12f, 0.16f, 0.24f, 0.95f);    // #1e2a3e
        private static readonly Color DividerNavy = new Color(0.16f, 0.16f, 0.30f, 0.80f);   // #2a2a4e

        // ── GUI 에셋 스프라이트 경로 ──
        private const string SPRITE_BASE = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components";
        private const string SPRITE_TOPBAR = SPRITE_BASE + "/Frame/PanelFrame03_Topbar.png";
        private const string SPRITE_BOTTOM = SPRITE_BASE + "/Frame/PanelFrame06_Bottom.png";
        private const string SPRITE_PLAYER_PANEL = SPRITE_BASE + "/Frame/BasicFrame_Round12_Gradient.png";
        private const string SPRITE_SOLID_FRAME = SPRITE_BASE + "/Frame/BasicFrame_Round12.png";
        private const string SPRITE_ENEMY_PANEL = SPRITE_BASE + "/Frame/CardFrame03_Single_Blue.png";
        private const string SPRITE_HP_BG = SPRITE_BASE + "/Slider/Slider_Basic04_Bg.png";
        private const string SPRITE_HP_FILL_GREEN = SPRITE_BASE + "/Slider/Slider_Basic04_Fill_Green.png";
        private const string SPRITE_HP_FILL_RED = SPRITE_BASE + "/Slider/Slider_Basic04_Fill_Red.png";
        private const string SPRITE_ENDTURN_BTN = SPRITE_BASE + "/Button/Button01_175_Red.png";
        private const string SPRITE_LOG_SIDEBAR = SPRITE_BASE + "/Frame/ListFrame03_Single_Bg_Blue.png";
        private const string SPRITE_BADGE_BG = SPRITE_BASE + "/Button/Button_Circle128_Dark.png";
        private const string SPRITE_CARD_BORDER = SPRITE_BASE + "/Frame/CardFrame01_Border.png";
        private const string SPRITE_CARD_GRADIENT = SPRITE_BASE + "/Frame/CardFrame01_Gradient.png";

        // ── 스킬 아이콘 스프라이트 경로 ──
        private const string ICON_BASE = SPRITE_BASE;
        private const string ICON_ATTACK = ICON_BASE + "/Icon_ItemIcons/128/Icon_Sword.png";
        private const string ICON_HEAL = ICON_BASE + "/Icon_ItemIcons/128/Icon_Heart.png";
        private const string ICON_SHIELD = ICON_BASE + "/Icon_ItemIcons/128/Icon_Shield.png";
        private const string ICON_BUFF = ICON_BASE + "/Icon_RuneIcons/256/RuneIcon0_Buff.png";
        private const string ICON_DEBUFF = ICON_BASE + "/Icon_RuneIcons/256/RuneIcon0_Debuff.png";
        private const string ICON_PURIFY = ICON_BASE + "/Icon_RuneIcons/256/RuneIcon0_Ball_Health.png";

        private const string KOREAN_FONT_TTF = "Assets/08.Resource/Fonts/NanumGothic.ttf";
        private const string KOREAN_FONT_SDF = "Assets/08.Resource/Fonts/NanumGothic SDF.asset";
        private static TMP_FontAsset _koreanFont;

        // ── 폰트 유틸 ──

        private static TMP_FontAsset GetOrCreateKoreanFont()
        {
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

        // ── 진입점 ──

        [MenuItem("Tools/Battle UI/Build Battle Scene (with BG)", false, 99)]
        public static void BuildBattleScene()
        {
            const string path = "Assets/01.Scenes/BattleScene.unity";

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            var oldManager = GameObject.Find("TestCombatManager");
            if (oldManager != null)
                Object.DestroyImmediate(oldManager);

            var oldCanvas = GameObject.Find("BattleUICanvas");
            if (oldCanvas != null)
                Object.DestroyImmediate(oldCanvas);

            _koreanFont = GetOrCreateKoreanFont();

            var canvas = CreateCanvas(scene);
            CreateBattleUI(canvas);

            SetupScriptsInCurrentScene();

            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[BattleSceneBuilder] BattleScene built and saved to {path}");
            AssetDatabase.Refresh();
        }

        private static Canvas CreateCanvas(Scene scene)
        {
            // Main Camera 확보 — ScreenSpaceCamera Canvas의 worldCamera로 사용
            var mainCam = EnsureMainCamera(scene);

            var canvasGO = new GameObject("BattleUICanvas");
            SceneManager.MoveGameObjectToScene(canvasGO, scene);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = mainCam;
            canvas.sortingOrder = 100;
            canvas.planeDistance = 10f;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        /// <summary>
        /// Main Camera 확보 + 설정 정규화 — ScreenSpaceCamera Canvas의 기준.
        /// VFXManager는 이 카메라를 URP Base로 사용해 VFX Overlay Camera를 Stacking.
        /// orthographicSize = 5.4 (1080/200) — VFX Camera와 정합성 유지.
        /// cullingMask에서 VFX 레이어(30) 제외 — VFX는 Overlay Camera 전용.
        /// </summary>
        private static Camera EnsureMainCamera(Scene scene)
        {
            const int VFX_LAYER = 30;
            const float PIXELS_PER_UNIT = 100f;

            var mainCam = Camera.main;
            if (mainCam == null)
            {
                var camGO = new GameObject("Main Camera");
                SceneManager.MoveGameObjectToScene(camGO, scene);
                mainCam = camGO.AddComponent<Camera>();
                camGO.tag = "MainCamera";
            }
            else
            {
                // 씬 소속 보장 (DontDestroyOnLoad 씬에 있을 수 있음)
                if (!scene.IsValid() || mainCam.gameObject.scene != scene)
                    SceneManager.MoveGameObjectToScene(mainCam.gameObject, scene);
            }

            // Transform 정규화 — 기존 scale(0.01) 비정상값 교정
            mainCam.transform.position = new Vector3(0f, 0f, -10f);
            mainCam.transform.rotation = Quaternion.identity;
            mainCam.transform.localScale = Vector3.one;

            // Projection 설정 — VFX Camera와 정합
            mainCam.orthographic = true;
            mainCam.orthographicSize = Screen.height * 0.5f / PIXELS_PER_UNIT;
            mainCam.nearClipPlane = 0.3f;
            mainCam.farClipPlane = 100f;
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = BgDark;
            mainCam.depth = -1;
            // VFX 레이어 제외 — VFX는 Overlay Camera에서만 렌더링 (중복 방지)
            mainCam.cullingMask = ~(1 << VFX_LAYER);

            // AudioListener 중복 방지
            if (mainCam.GetComponent<AudioListener>() == null)
            {
                var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
                if (listeners.Length == 0)
                    mainCam.gameObject.AddComponent<AudioListener>();
            }

            return mainCam;
        }

        // ── UI 오케스트레이터 ──

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
            content.offsetMin = new Vector2(0, 284);
            content.offsetMax = new Vector2(0, -46);

            CreateCenterArea(content);

            // 파티 상태 / 배틀로그 토글 오버레이 (Step 4)
            CreatePartyStatusOverlay(root);
            CreateBattleLogOverlay(root);

            CreateCharacterPopup(root);
            CreateBattleEndOverlay(root);
            CreateTooltipUI(root);
        }
    }
}
