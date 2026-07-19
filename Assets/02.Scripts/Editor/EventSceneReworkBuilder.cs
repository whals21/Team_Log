#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using TeamLog.UI;
using TeamLog.UI.Event;

namespace TeamLog.Editor
{
    /// <summary>
    /// ★ Stained Glass Event UI — EventReworkView Prefab 빌더.
    ///
    /// ★ PartySelectionSceneBuilder / MapSceneReworkBuilder와 동일한 패턴 (UIBestPractices §7).
    /// 구조:
    /// - EventReworkView (CanvasGroup + DimBackground)
    /// - DimBackground (전체 화면 어둠)
    /// - GlassFrame (중앙 카드 720x760)
    ///   - GlassWindow (상단 320px — Image 스테인드글라스)
    ///     - Emblem (TMP, 중앙 엠블럼 기호)
    ///   - GlassPanel (하단 — Image + VLG)
    ///     - TopBar (HLG: ThemeTag + EventTypeTag)
    ///     - EventTitle (TMP, 메인 타이틀)
    ///     - Narrative (TMP, 이야기 묘사)
    ///     - ChoiceContainer (VLG — 선택지 행들)
    ///     - ResultPanel (초기 비활성 — ResultText + ResultConfirmButton)
    ///   - CloseButton (우측 상단 X)
    ///
    /// 메뉴: TeamLog/UI/Build Event Rework View Prefab
    /// 출력 Prefab: Assets/03.Data/UI/EventScene/Prefabs/EventReworkViewPrefab.prefab
    /// </summary>
    public static partial class EventSceneReworkBuilder
    {
        private const string OUTPUT_PREFAB = "Assets/03.Data/UI/EventScene/Prefabs/EventReworkViewPrefab.prefab";
        private const string SPRITE_DIR = "Assets/03.Data/UI/EventScene";

        // ★ 폰트 캐시 — MapSceneReworkBuilder와 동일 패턴
        private static TMP_FontAsset _fontCinzelBlack;
        private static TMP_FontAsset _fontCinzelBold;
        private static TMP_FontAsset _fontCinzelRegular;
        private static TMP_FontAsset _fontCormorantItalic;
        private static TMP_FontAsset _fontKorean;

        [MenuItem("TeamLog/UI/Build Event Rework View Prefab")]
        public static void BuildPrefab()
        {
            LoadFonts();
            EnsurePrefabDirectory();

            // 빈 GameObject에서 EventReworkView 트리 구성
            var go = new GameObject("EventReworkView", typeof(RectTransform), typeof(CanvasRenderer));
            var rt = go.GetComponent<RectTransform>();
            // 전체 화면 stretch (MapReworkCanvas 자식으로 배치되도록)
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            // ★ CanvasGroup 추가 (FadeIn/FadeOut용)
            go.AddComponent<CanvasGroup>();

            // EventReworkView 컴포넌트 부착 — 자동 바인딩 활성
            var view = go.AddComponent<EventReworkView>();

            // 배경 (Dim) 자식 추가
            BuildDimBackground(go.transform);

            // GlassFrame (중앙 카드)
            BuildGlassFrame(go.transform);

            // 자식 컨테이너 모두 구축 후 view에 자동 연결
            WireEventReworkView(view);

            // Prefab 저장
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, OUTPUT_PREFAB);
            Object.DestroyImmediate(go);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[EventSceneReworkBuilder] EventReworkView Prefab 생성 완료 → {OUTPUT_PREFAB}");
        }

        /// <summary>
        /// ★ MapSceneReworkBuilder 통합용 — 폰트 로딩 + Sprite 로딩을 외부에서 호출 가능.
        /// </summary>
        public static void EnsureInitialized()
        {
            LoadFonts();
            EnsurePrefabDirectory();
        }

        private static void EnsurePrefabDirectory()
        {
            if (!AssetDatabase.IsValidFolder(SPRITE_DIR))
            {
                if (!AssetDatabase.IsValidFolder("Assets/03.Data/UI"))
                    AssetDatabase.CreateFolder("Assets/03.Data", "UI");
                AssetDatabase.CreateFolder("Assets/03.Data/UI", "EventScene");
            }
            if (!AssetDatabase.IsValidFolder(SPRITE_DIR + "/Prefabs"))
                AssetDatabase.CreateFolder(SPRITE_DIR, "Prefabs");
        }

        private static void LoadFonts()
        {
            _fontCinzelBlack     = PartySelectionSceneBuilder.LoadFont("Cinzel-Black SDF");
            _fontCinzelBold      = PartySelectionSceneBuilder.LoadFont("Cinzel-Bold SDF");
            _fontCinzelRegular   = PartySelectionSceneBuilder.LoadFont("Cinzel-Regular SDF");
            _fontCormorantItalic = PartySelectionSceneBuilder.LoadFont("CormorantGaramond-Italic SDF");
            _fontKorean          = PartySelectionSceneBuilder.LoadFont("NanumGothic SDF");

            Debug.Log($"[EventSceneReworkBuilder] Fonts — Black:{(_fontCinzelBlack != null)} " +
                      $"Bold:{(_fontCinzelBold != null)} Italic:{(_fontCormorantItalic != null)}");
        }

        // 폰트 헬퍼 (MapSceneReworkBuilder와 동일)
        public static TMP_FontAsset FontTitle()  => _fontCinzelBlack     ?? TMP_Settings.defaultFontAsset;
        public static TMP_FontAsset FontLabel()  => _fontCinzelBold      ?? TMP_Settings.defaultFontAsset;
        public static TMP_FontAsset FontBody()   => _fontCinzelRegular   ?? TMP_Settings.defaultFontAsset;
        public static TMP_FontAsset FontItalic() => _fontCormorantItalic ?? TMP_Settings.defaultFontAsset;

        // =========================================================
        // 공통 유틸
        // =========================================================
        public static void WireField(Object target, string fieldName, Object value)
        {
            if (target == null || value == null) return;
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[EventSceneReworkBuilder] 필드 '{fieldName}'을(를) {target.GetType().Name}에서 찾을 수 없음");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }

        public static GameObject FindDescendant(Transform current, string name)
        {
            for (int i = 0; i < current.childCount; i++)
            {
                var child = current.GetChild(i);
                if (child.name == name) return child.gameObject;
                var found = FindDescendant(child, name);
                if (found != null) return found;
            }
            return null;
        }

        public static Sprite LoadEventSprite(string fileName)
        {
            string path = $"{SPRITE_DIR}/{fileName}";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                Debug.LogWarning($"[EventSceneReworkBuilder] Sprite 로드 실패: {path} — EventSceneSpriteGenerator 실행 필요");
            return sprite;
        }
    }
}
#endif
