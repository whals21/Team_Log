#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using TeamLog.UI;
using TeamLog.UI.Map.Rework;

namespace TeamLog.Editor
{
    /// <summary>
    /// Map Scene Rework 씬 빌더 (Phase 4 — 2026-07-19).
    ///
    /// ★ PartySelectionSceneBuilder와 동일 패턴 (UIBestPractices §7 SceneBuilder 패턴 준거).
    /// 3-컬럼 그리드 (좌측 Party 사이드바 / 중앙 Map Codex / 우측 Context Panel) +
    /// Header (Stage/Floor/Gold/Ascension) + Footer (Skills/Traits/Options 메뉴).
    ///
    /// 메뉴: TeamLog/Scene/Build Map Scene (Rework)
    /// 출력 씬: Assets/01.Scenes/MapSceneRework.unity (기존 MapScene 보존)
    /// </summary>
    public static partial class MapSceneReworkBuilder
    {
        private const string OUTPUT_SCENE = "Assets/01.Scenes/MapSceneRework.unity";
        private const string TEMPLATE_SCENE = "Assets/01.Scenes/MapScene.unity";

        // ★ Phase 2 (재완성 — Priority 2): 고딕 폰트 캐시. PartySelectionSceneBuilder.LoadFont 재사용.
        private static TMP_FontAsset _fontCinzelBold;
        private static TMP_FontAsset _fontCinzelBlack;
        private static TMP_FontAsset _fontCinzelRegular;
        private static TMP_FontAsset _fontCormorantItalic;
        private static TMP_FontAsset _fontKorean;

        [MenuItem("TeamLog/Scene/Build Map Scene (Rework)")]
        public static void BuildScene()
        {
            // 빈 씬에서 새로 빌드 — 필요한 인스펙터 참조(Sprite, 프리팹, Container)는
            // 마지막에 WireField (SerializedObject 패턴)로 자동 연결.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ★ Phase 1 (재완성): 빈 씬이므로 Camera / Light / EventSystem을 직접 생성.
            // 이게 누락되면 "No cameras rendering" 메시지만 나오는 치명 버그 발생.
            CreateEssSceneInfra();

            // ★ Phase 2 (Priority 2): 고딕 폰트 로드. Cinzel/Cormorant.
            LoadGothicFonts();

            // 1. 프리팹 먼저 빌드 (View가 참조해야 하므로)
            BuildAllPrefabs();

            // 2. 캔버스 + 각 섹션 생성
            var canvas = BuildCanvas();

            // 3. MapReworkView의 Container 자식 추가 + Sprite/Prefab/Container 필드 자동 연결
            SetupMapReworkViewContainers(canvas);
            WireAllFieldsToView(canvas);

            EditorSceneManager.SaveScene(scene, OUTPUT_SCENE);

            // ★ Build Settings에 씬 자동 등록 (SceneManager.LoadScene 작동하려면 필요)
            EnsureSceneInBuildSettings(OUTPUT_SCENE);

            Debug.Log($"[MapSceneReworkBuilder] 씬 빌드 완료 → {OUTPUT_SCENE}");
        }

        /// <summary>
        /// Build Settings에 씬 등록 — SceneManager.LoadScene이 작동하려면 Scenes In Build에 있어야 함.
        /// 이미 등록된 경우 스킵.
        /// </summary>
        private static void EnsureSceneInBuildSettings(string scenePath)
        {
            var scenes = new List<UnityEditor.EditorBuildSettingsScene>(UnityEditor.EditorBuildSettings.scenes);

            // 이미 등록되어 있으면 스킵
            foreach (var s in scenes)
            {
                if (s.path == scenePath)
                {
                    if (!s.enabled)
                    {
                        s.enabled = true;
                        UnityEditor.EditorBuildSettings.scenes = scenes.ToArray();
                    }
                    return;
                }
            }

            // 신규 등록 — PartySelectionScene 다음 위치에 삽입 (파이프 순서 보장)
            int insertIndex = scenes.FindIndex(s => s.path.Contains("PartySelectionScene")) + 1;
            if (insertIndex <= 0) insertIndex = scenes.Count;

            scenes.Insert(insertIndex, new UnityEditor.EditorBuildSettingsScene(scenePath, true));
            UnityEditor.EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[MapSceneReworkBuilder] Build Settings에 등록 → {scenePath} (index {insertIndex})");
        }

        /// <summary>
        /// 메인 Canvas + 3-컬럼 그리드 생성. Canvas 반환 (후속 WireField용).
        /// </summary>
        private static Canvas BuildCanvas()
        {
            var canvasGo = new GameObject("MapReworkCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // MainFrame — 전체 화면 채우는 컨테이너 (★ 자식은 VLG 대신 anchor 직접 배치)
            var mainFrame = CreateSection("MainFrame", canvasGo.transform);
            UIAutoBindHelper.StretchToParent(mainFrame.Rect);

            // ★ Priority 5 (재작성): MainFrame에 VerticalLayoutGroup을 두지 않고
            // Header/Body/Footer를 anchor로 직접 배치 (VLG가 LayoutElement를 무시하는 문제 회피).
            const float HEADER_H = 52f;
            const float FOOTER_H = 40f;

            // Header — 상단 고정 (52px)
            var headerRect = BuildHeaderAnchored(mainFrame.Rect, HEADER_H);

            // Body — Header 아래 ~ Footer 위 (가로 3-컬럼)
            var body = CreateAnchoredSection("Body", mainFrame.Rect,
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(1, 1),
                offsetMin: new Vector2(0, FOOTER_H), offsetMax: new Vector2(0, -HEADER_H));
            var hlg = body.GameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.spacing = 0;

            BuildLeftSidebar(body.Rect);          // 320px
            BuildMapCodex(body.Rect);              // flexibleWidth=1
            BuildRightPanel(body.Rect);            // 360px

            // Footer — 하단 고정 (40px)
            var footerRect = BuildFooterAnchored(mainFrame.Rect, FOOTER_H);

            return canvas;
        }

        // =========================================================
        // 공통 빌더 유틸
        // =========================================================

        /// <summary>
        /// LayoutElement 달린 섹션 컨테이너 생성. (Body 자식용 — HLG 안에 있음)
        /// </summary>
        private static SectionRef CreateSection(string name, Transform parent,
            float prefW = -1, float prefH = -1, float flexW = -1, float flexH = -1)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(rt);
            var le = go.AddComponent<LayoutElement>();
            if (prefW >= 0) le.preferredWidth = prefW;
            if (prefH >= 0) le.preferredHeight = prefH;
            if (flexW >= 0) le.flexibleWidth = flexW;
            if (flexH >= 0) le.flexibleHeight = flexH;

            // 기본 배경 (어두운 slate)
            var bg = go.AddComponent<Image>();
            bg.color = UIPalette.Default.BgDark;
            bg.raycastTarget = false;

            return new SectionRef { GameObject = go, Rect = rt };
        }

        /// <summary>
        /// ★ Priority 5 — anchor 기반 섹션 컨테이너 생성. LayoutElement를 추가하지 않음.
        /// VerticalLayoutGroup이 LayoutElement를 무시하는 문제를 회피.
        /// anchorMin==anchorMax (단일 anchor) 모드: sizeDelta로 크기 지정.
        /// anchorMin!=anchorMax (stretch) 모드: offsetMin/Max로 마진 지정.
        /// </summary>
        private static SectionRef CreateAnchoredSection(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2? offsetMin = null, Vector2? offsetMax = null,
            Vector2? pivot = null,
            Vector2? sizeDelta = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            if (offsetMin != null) rt.offsetMin = offsetMin.Value;
            if (offsetMax != null) rt.offsetMax = offsetMax.Value;
            rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            if (sizeDelta != null) rt.sizeDelta = sizeDelta.Value;
            else if (anchorMin == anchorMax) rt.sizeDelta = Vector2.zero; // 단일 anchor 기본
            rt.anchoredPosition = Vector2.zero;

            // 기본 배경 (어두운 slate)
            var bg = go.AddComponent<Image>();
            bg.color = UIPalette.Default.BgDark;
            bg.raycastTarget = false;

            return new SectionRef { GameObject = go, Rect = rt };
        }

        /// <summary>
        /// ★ Header — 상단 고정 anchor 배치 (높이 지정).
        /// BuildHeader를 anchor 기반으로 호출.
        /// </summary>
        private static RectTransform BuildHeaderAnchored(Transform parent, float height)
        {
            return BuildHeader(parent,
                anchorMin: new Vector2(0, 1),
                anchorMax: new Vector2(1, 1),
                pivot: new Vector2(0.5f, 1),
                sizeDelta: new Vector2(0, height));
        }

        /// <summary>
        /// ★ Footer — 하단 고정 anchor 배치 (높이 지정).
        /// </summary>
        private static RectTransform BuildFooterAnchored(Transform parent, float height)
        {
            return BuildFooter(parent,
                anchorMin: new Vector2(0, 0),
                anchorMax: new Vector2(1, 0),
                pivot: new Vector2(0.5f, 0),
                sizeDelta: new Vector2(0, height));
        }

        /// <summary>
        /// 패널 헤더 (오너먼트 + 제목) 생성. PartySelectionSceneBuilder와 동일 패턴.
        /// </summary>
        private static GameObject CreatePanelHeader(string title, Transform parent)
        {
            var headerGo = new GameObject($"PanelHeader_{title}", typeof(RectTransform));
            headerGo.transform.SetParent(parent, false);
            var hlg = headerGo.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 8;

            var le = headerGo.AddComponent<LayoutElement>();
            le.preferredHeight = 28;
            le.flexibleWidth = 1;

            // 왼쪽 오너먼트
            var leftLine = new GameObject("OrnamentLeft", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            leftLine.transform.SetParent(headerGo.transform, false);
            var leftLineImg = leftLine.GetComponent<Image>();
            leftLineImg.color = new Color(0.545f, 0.412f, 0.078f, 0.7f);
            leftLineImg.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(leftLine, flexW: 1, prefH: 1);

            // 타이틀
            var titleGo = new GameObject("PanelTitle", typeof(RectTransform), typeof(CanvasRenderer));
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = title;
            titleTmp.font = FontLabel();
            titleTmp.fontSize = 13;
            titleTmp.color = UIPalette.Default.DFGoldL;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(titleGo, prefW: 100, prefH: 20);

            // 오른쪽 오너먼트
            var rightLine = new GameObject("OrnamentRight", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            rightLine.transform.SetParent(headerGo.transform, false);
            var rightLineImg = rightLine.GetComponent<Image>();
            rightLineImg.color = new Color(0.545f, 0.412f, 0.078f, 0.7f);
            rightLineImg.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(rightLine, flexW: 1, prefH: 1);

            return headerGo;
        }

        /// <summary>
        /// 자식 컨테이너 (VerticalLayoutGroup 포함).
        /// </summary>
        private static RectTransform CreateChildContainer(string name, Transform parent,
            float padding = 16, float spacing = 8, bool controlWidth = true, bool controlHeight = true)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(rt);

            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
            vlg.spacing = spacing;
            vlg.childControlWidth = controlWidth;
            vlg.childControlHeight = controlHeight;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return rt;
        }

        /// <summary>
        /// WireField 헬퍼 — Unity Object 참조 필드 자동 할당.
        /// ★ CLAUDE.md 가드레일 #15 준거: update_component는 Unity Object 참조 할당 불가 →
        /// SerializedObject.FindProperty + objectReferenceValue + ApplyModifiedProperties 패턴 필수.
        /// </summary>
        private static void WireField(Object target, string fieldName, Object value)
        {
            if (target == null || value == null) return;
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[MapSceneReworkBuilder] 필드 '{fieldName}'을(를) {target.GetType().Name}에서 찾을 수 없음");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }

        /// <summary>
        /// ★ Phase 1 (재완성) — 카메라/라이트/이벤트시스템 생성.
        /// NewSceneSetup.EmptyScene이 이들을 자동 생성하지 않으므로 직접 만들어야 함.
        /// PartySelectionSceneBuilder와 동일 패턴.
        /// </summary>
        private static void CreateEssSceneInfra()
        {
            // 1. Main Camera (UI는 ScreenSpaceOverlay라 카메라가 필요 없지만, Game View 표시용)
            var camGo = new GameObject("Main Camera",
                typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.035f); // DFVoid
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.nearClipPlane = -100f;
            cam.farClipPlane = 100f;
            // 카메라를 살짝 뒤로 (UI 캔버스가 항상 카메라 앞에 오도록 — ScreenSpaceOverlay는 영향 없지만 안전)
            camGo.transform.position = new Vector3(0, 0, -10f);

            // 2. Directional Light (URP 호환)
            var lightGo = new GameObject("Directional Light", typeof(Light));
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightGo.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            light.color = new Color(1f, 0.96f, 0.85f);

            // 3. EventSystem (UI 클릭용)
            var eventGo = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
            // ★ New Input System이 활성화된 환경에서는 StandaloneInputModule이 경고를 낼 수 있으나,
            // 프로젝트의 기존 씬(TitleScene/PartySelectionScene)이 동일 패턴을 사용하므로 호환됨.
        }

        /// <summary>
        /// ★ Priority 2 — 고딕 폰트 로드. PartySelectionSceneBuilder.LoadFont 재사용.
        /// Cinzel(고딕 세리프) — 골드 텍스트/타이틀
        /// CormorantGaramond Italic — 라벨/태그라인 (이탤릭)
        /// NanumGothic — 한국어 (라벨에 한국어가 포함될 수 있으므로)
        /// </summary>
        private static void LoadGothicFonts()
        {
            _fontCinzelBold     = PartySelectionSceneBuilder.LoadFont("Cinzel-Bold SDF");
            _fontCinzelBlack    = PartySelectionSceneBuilder.LoadFont("Cinzel-Black SDF");
            _fontCinzelRegular  = PartySelectionSceneBuilder.LoadFont("Cinzel-Regular SDF");
            _fontCormorantItalic = PartySelectionSceneBuilder.LoadFont("CormorantGaramond-Italic SDF");
            _fontKorean         = PartySelectionSceneBuilder.LoadFont("NanumGothic SDF");

            Debug.Log($"[MapSceneReworkBuilder] 고딕 폰트 로드 — " +
                      $"Cinzel:{(_fontCinzelBold != null ? "✓" : "✗")} " +
                      $"Cormorant:{(_fontCormorantItalic != null ? "✓" : "✗")} " +
                      $"Nanum:{(_fontKorean != null ? "✓" : "✗")}");
        }

        /// <summary>골드 메인 타이틀 (Cinzel Black)</summary>
        private static TMP_FontAsset FontTitle() => _fontCinzelBlack != null ? _fontCinzelBlack : TMP_Settings.defaultFontAsset;

        /// <summary>서브 타이틀 / 칩 값 (Cinzel Bold)</summary>
        private static TMP_FontAsset FontLabel() => _fontCinzelBold != null ? _fontCinzelBold : TMP_Settings.defaultFontAsset;

        /// <summary>본문 / 태그라인 (Cinzel Regular)</summary>
        private static TMP_FontAsset FontBody() => _fontCinzelRegular != null ? _fontCinzelRegular : TMP_Settings.defaultFontAsset;

        /// <summary>이탤릭 라벨 (Cormorant Italic)</summary>
        private static TMP_FontAsset FontItalic() => _fontCormorantItalic != null ? _fontCormorantItalic : TMP_Settings.defaultFontAsset;

        private struct SectionRef
        {
            public GameObject GameObject;
            public RectTransform Rect;
        }
    }
}
#endif
