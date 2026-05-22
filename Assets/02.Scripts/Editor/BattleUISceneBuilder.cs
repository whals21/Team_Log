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
        private static readonly Color TextDim = new Color(0.7f, 0.7f, 0.75f);
        private static readonly Color ShieldBrown = new Color(0.72f, 0.45f, 0.2f);
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
            content.offsetMin = new Vector2(0, 100);
            content.offsetMax = new Vector2(0, -60);

            CreateLeftSidebar(content);
            CreateRightSidebar(content);
            CreateCenterArea(content);

            CreateCharacterPopup(root);
        }
    }
}
