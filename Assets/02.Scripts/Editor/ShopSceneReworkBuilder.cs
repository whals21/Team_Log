#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using TeamLog.UI;
using TeamLog.UI.Shop;

namespace TeamLog.Editor
{
    /// <summary>
    /// ★ Stained Glass Shop UI — ShopReworkView Prefab 빌더 (B 시안).
    ///
    /// 메뉴: TeamLog/UI/Build Shop Rework View Prefab
    /// 출력: Assets/03.Data/UI/ShopScene/Prefabs/ShopReworkViewPrefab.prefab
    /// </summary>
    public static partial class ShopSceneReworkBuilder
    {
        private const string OUTPUT_PREFAB = "Assets/03.Data/UI/ShopScene/Prefabs/ShopReworkViewPrefab.prefab";
        private const string SPRITE_DIR = "Assets/03.Data/UI/ShopScene";

        // 폰트 캐시
        private static TMP_FontAsset _fontCinzelBlack;
        private static TMP_FontAsset _fontCinzelBold;
        private static TMP_FontAsset _fontCinzelRegular;
        private static TMP_FontAsset _fontCormorantItalic;
        private static TMP_FontAsset _fontKorean;

        [MenuItem("TeamLog/UI/Build Shop Rework View Prefab")]
        public static void BuildPrefab()
        {
            LoadFonts();
            EnsurePrefabDirectory();

            var go = new GameObject("ShopReworkView", typeof(RectTransform), typeof(CanvasRenderer));
            var rt = go.GetComponent<RectTransform>();
            // 전체 화면 stretch
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            go.AddComponent<CanvasGroup>();
            var view = go.AddComponent<ShopReworkView>();

            BuildDimBackground(go.transform);
            BuildReliquaryFrame(go.transform);

            WireShopReworkView(view);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, OUTPUT_PREFAB);
            Object.DestroyImmediate(go);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ShopSceneReworkBuilder] ShopReworkView Prefab 생성 완료 → {OUTPUT_PREFAB}");
        }

        /// <summary>
        /// ★ MapSceneReworkBuilder 통합용 — 외부에서 호출 시 폰트/디렉토리 보장.
        /// </summary>
        public static void EnsureInitialized()
        {
            LoadFonts();
            EnsurePrefabDirectory();
        }

        /// <summary>
        /// ★ MapSceneReworkBuilder 통합용 — Prefab 로드 (없으면 빌드).
        /// </summary>
        public static GameObject LoadShopReworkViewPrefab()
        {
            EnsureInitialized();
            if (!System.IO.File.Exists(OUTPUT_PREFAB))
                BuildPrefab();
            return AssetDatabase.LoadAssetAtPath<GameObject>(OUTPUT_PREFAB);
        }

        private static void EnsurePrefabDirectory()
        {
            if (!AssetDatabase.IsValidFolder(SPRITE_DIR))
            {
                if (!AssetDatabase.IsValidFolder("Assets/03.Data/UI"))
                    AssetDatabase.CreateFolder("Assets/03.Data", "UI");
                AssetDatabase.CreateFolder("Assets/03.Data/UI", "ShopScene");
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

            Debug.Log($"[ShopSceneReworkBuilder] Fonts — Black:{(_fontCinzelBlack != null)} " +
                      $"Bold:{(_fontCinzelBold != null)} Italic:{(_fontCormorantItalic != null)}");
        }

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
                Debug.LogWarning($"[ShopSceneReworkBuilder] 필드 '{fieldName}'을(를) {target.GetType().Name}에서 찾을 수 없음");
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

        public static Sprite LoadShopSprite(string fileName)
        {
            string path = $"{SPRITE_DIR}/{fileName}";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                Debug.LogWarning($"[ShopSceneReworkBuilder] Sprite 로드 실패: {path} — ShopSceneSpriteGenerator 실행 필요");
            return sprite;
        }
    }
}
#endif
