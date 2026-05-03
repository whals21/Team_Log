using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI.Map;
using TeamLog.UI.Reward;
using TeamLog.UI.Shop;

namespace TeamLog.Editor
{
    /// <summary>
    /// Phase 3 프리팹 일괄 생성 에디터
    /// </summary>
    public static class Phase3PrefabBuilder
    {
        private const string PREFAB_DIR = "Assets/03.Data/Prefabs";
        private const string FONT_SDF = "Assets/08.Resource/Fonts/NanumGothic SDF.asset";

        // 색상 팔레트
        private static readonly Color BgDark = new Color(0.12f, 0.12f, 0.22f);
        private static readonly Color BgPanel = new Color(0.1f, 0.1f, 0.18f, 0.9f);
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextDim = new Color(0.7f, 0.7f, 0.75f);
        private static readonly Color AccentYellow = new Color(0.96f, 0.82f, 0.25f);
        private static readonly Color HighlightYellow = new Color(0.96f, 0.82f, 0.25f, 0.3f);
        private static readonly Color VisitedDark = new Color(0.05f, 0.05f, 0.1f, 0.6f);
        private static readonly Color SoldDark = new Color(0.05f, 0.05f, 0.1f, 0.7f);

        private static TMP_FontAsset _font;

        [MenuItem("TeamLog/Prefabs/Create Phase 3 Prefabs")]
        public static void CreateAllPrefabs() => CreateAllPrefabs(true);

        [MenuItem("TeamLog/Prefabs/Force Rebuild Phase 3 Prefabs")]
        public static void ForceRebuildPrefabs() => CreateAllPrefabs(false);

        private static void CreateAllPrefabs(bool skipExisting)
        {
            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_SDF);

            EnsureFolder();

            if (!skipExisting) DeleteExistingPrefabs();

            CreateMapNodeButtonPrefab();
            CreateMapConnectionLinePrefab();
            CreateMapPlayerMarkerPrefab();
            CreateRewardCardPrefab();
            CreateShopItemSlotPrefab();
            CreateChoiceButtonPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase3PrefabBuilder] 프리팹 6개 생성 완료!");
        }

        private static void DeleteExistingPrefabs()
        {
            string[] prefabNames = {
                "MapNodeButton", "MapConnectionLine", "MapPlayerMarker",
                "RewardCard", "ShopItemSlot", "ChoiceButton"
            };
            foreach (var name in prefabNames)
            {
                var path = $"{PREFAB_DIR}/{name}.prefab";
                AssetDatabase.DeleteAsset(path);
            }
            AssetDatabase.Refresh();
            Debug.Log("[Phase3PrefabBuilder] 기존 프리팹 삭제 완료");
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/03.Data/Prefabs"))
                AssetDatabase.CreateFolder("Assets/03.Data", "Prefabs");
        }

        #region MapNodeButton

        private static void CreateMapNodeButtonPrefab()
        {
            const string path = PREFAB_DIR + "/MapNodeButton.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) { Debug.Log("[Prefab] MapNodeButton 이미 존재, 스킵"); return; }

            var root = NewRoot("MapNodeButton", 80f, 80f);

            // Background + Button
            var bg = root.gameObject.AddComponent<Image>();
            bg.color = BgPanel;
            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = bg;

            // ActiveHighlight (전체 채움)
            var highlight = NewRect("ActiveHighlight", root);
            SetFill(highlight);
            highlight.gameObject.AddComponent<Image>().color = HighlightYellow;
            highlight.gameObject.SetActive(false);

            // Label
            var label = NewRect("Label", root);
            label.anchorMin = new Vector2(0, 0);
            label.anchorMax = new Vector2(1, 0.4f);
            label.offsetMin = new Vector2(4, 2);
            label.offsetMax = new Vector2(-4, -2);
            AddTMP(label, "전투", 14, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

            // Icon
            var icon = NewRect("Icon", root);
            icon.anchorMin = new Vector2(0.2f, 0.4f);
            icon.anchorMax = new Vector2(0.8f, 0.9f);
            icon.offsetMin = Vector2.zero;
            icon.offsetMax = Vector2.zero;
            icon.gameObject.AddComponent<Image>().color = AccentYellow;

            // VisitedOverlay
            var visited = NewRect("VisitedOverlay", root);
            SetFill(visited);
            visited.gameObject.AddComponent<Image>().color = VisitedDark;
            visited.gameObject.SetActive(false);

            // 컴포넌트
            root.gameObject.AddComponent<MapNodeReference>();
            var nodeBtn = root.gameObject.AddComponent<MapNodeButton>();

            // SerializeField 자동 연결
            Wire(nodeBtn, "_iconImage", icon.GetComponent<Image>());
            Wire(nodeBtn, "_backgroundImage", bg);
            Wire(nodeBtn, "_labelText", label.GetComponent<TextMeshProUGUI>());
            Wire(nodeBtn, "_button", button);
            Wire(nodeBtn, "_visitedOverlay", visited.gameObject);
            Wire(nodeBtn, "_activeHighlight", highlight.gameObject);

            SavePrefab(root.gameObject, path);
        }

        #endregion

        #region MapConnectionLine

        private static void CreateMapConnectionLinePrefab()
        {
            const string path = PREFAB_DIR + "/MapConnectionLine.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) { Debug.Log("[Prefab] MapConnectionLine 이미 존재, 스킵"); return; }

            var root = NewRoot("MapConnectionLine", 100f, 3f);

            var lineImage = root.gameObject.AddComponent<Image>();
            lineImage.color = new Color(0.5f, 0.5f, 0.6f, 0.5f);

            var lineComp = root.gameObject.AddComponent<MapConnectionLine>();
            Wire(lineComp, "_lineImage", lineImage);

            SavePrefab(root.gameObject, path);
        }

        #endregion

        #region MapPlayerMarker

        private static void CreateMapPlayerMarkerPrefab()
        {
            const string path = PREFAB_DIR + "/MapPlayerMarker.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) { Debug.Log("[Prefab] MapPlayerMarker 이미 존재, 스킵"); return; }

            var root = NewRoot("MapPlayerMarker", 30f, 30f);

            var markerImage = root.gameObject.AddComponent<Image>();
            markerImage.color = AccentYellow;
            markerImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            var markerComp = root.gameObject.AddComponent<MapPlayerMarker>();
            Wire(markerComp, "_markerImage", markerImage);

            SavePrefab(root.gameObject, path);
        }

        #endregion

        #region RewardCard

        private static void CreateRewardCardPrefab()
        {
            const string path = PREFAB_DIR + "/RewardCard.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) { Debug.Log("[Prefab] RewardCard 이미 존재, 스킵"); return; }

            var root = NewRoot("RewardCard", 150f, 200f);

            // Background + Button
            var bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.18f, 0.08f);
            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = bg;
            root.gameObject.AddComponent<Outline>().effectColor = new Color(0.4f, 0.35f, 0.15f);

            // TitleLabel (상단)
            var title = NewRect("TitleLabel", root);
            title.anchorMin = new Vector2(0, 0.7f);
            title.anchorMax = new Vector2(1, 0.95f);
            title.offsetMin = new Vector2(8, 0);
            title.offsetMax = new Vector2(-8, 0);
            AddTMP(title, "골드", 20, FontStyles.Bold, TextAlignmentOptions.Center, AccentYellow);

            // DescLabel (중앙)
            var desc = NewRect("DescLabel", root);
            desc.anchorMin = new Vector2(0, 0.1f);
            desc.anchorMax = new Vector2(1, 0.65f);
            desc.offsetMin = new Vector2(8, 0);
            desc.offsetMax = new Vector2(-8, 0);
            AddTMP(desc, "30 골드", 16, FontStyles.Normal, TextAlignmentOptions.Center, TextDim);

            // RewardCard 컴포넌트
            var card = root.gameObject.AddComponent<RewardCard>();
            Wire(card, "_backgroundImage", bg);
            Wire(card, "_titleLabel", title.GetComponent<TextMeshProUGUI>());
            Wire(card, "_descLabel", desc.GetComponent<TextMeshProUGUI>());
            Wire(card, "_button", button);

            SavePrefab(root.gameObject, path);
        }

        #endregion

        #region ShopItemSlot

        private static void CreateShopItemSlotPrefab()
        {
            const string path = PREFAB_DIR + "/ShopItemSlot.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) { Debug.Log("[Prefab] ShopItemSlot 이미 존재, 스킵"); return; }

            var root = NewRoot("ShopItemSlot", 300f, 60f);

            // Background
            var bg = root.gameObject.AddComponent<Image>();
            bg.color = BgPanel;
            root.gameObject.AddComponent<Outline>().effectColor = new Color(0.3f, 0.3f, 0.4f, 0.5f);

            // BuyButton (전체 클릭 영역)
            var buyBtn = root.gameObject.AddComponent<Button>();
            buyBtn.targetGraphic = bg;

            // NameLabel (좌측 상단)
            var nameRect = NewRect("NameLabel", root);
            nameRect.anchorMin = new Vector2(0, 0.55f);
            nameRect.anchorMax = new Vector2(0.65f, 1f);
            nameRect.offsetMin = new Vector2(10, 0);
            nameRect.offsetMax = new Vector2(-4, -4);
            AddTMP(nameRect, "아이템명", 16, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);

            // DescLabel (좌측 하단)
            var descRect = NewRect("DescLabel", root);
            descRect.anchorMin = new Vector2(0, 0f);
            descRect.anchorMax = new Vector2(0.65f, 0.5f);
            descRect.offsetMin = new Vector2(10, 4);
            descRect.offsetMax = new Vector2(-4, 0);
            AddTMP(descRect, "설명 텍스트", 12, FontStyles.Normal, TextAlignmentOptions.Left, TextDim);

            // PriceLabel (우측)
            var priceRect = NewRect("PriceLabel", root);
            priceRect.anchorMin = new Vector2(0.65f, 0f);
            priceRect.anchorMax = new Vector2(1f, 1f);
            priceRect.offsetMin = new Vector2(0, 0);
            priceRect.offsetMax = new Vector2(-10, 0);
            AddTMP(priceRect, "50 G", 16, FontStyles.Bold, TextAlignmentOptions.Right, AccentYellow);

            // SoldOverlay
            var sold = NewRect("SoldOverlay", root);
            SetFill(sold);
            sold.gameObject.AddComponent<Image>().color = SoldDark;
            sold.gameObject.SetActive(false);

            // ShopItemSlot 컴포넌트
            var slot = root.gameObject.AddComponent<ShopItemSlot>();
            Wire(slot, "_nameLabel", nameRect.GetComponent<TextMeshProUGUI>());
            Wire(slot, "_descLabel", descRect.GetComponent<TextMeshProUGUI>());
            Wire(slot, "_priceLabel", priceRect.GetComponent<TextMeshProUGUI>());
            Wire(slot, "_buyButton", buyBtn);
            Wire(slot, "_soldOverlay", sold.gameObject);

            SavePrefab(root.gameObject, path);
        }

        #endregion

        #region ChoiceButton

        private static void CreateChoiceButtonPrefab()
        {
            const string path = PREFAB_DIR + "/ChoiceButton.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) { Debug.Log("[Prefab] ChoiceButton 이미 존재, 스킵"); return; }

            var root = NewRoot("ChoiceButton", 400f, 50f);

            // Background + Button
            var bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.25f);
            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = bg;

            // Navigation 기본 색상
            var colors = button.colors;
            colors.highlightedColor = new Color(0.25f, 0.25f, 0.4f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.18f);
            button.colors = colors;

            // TMP 텍스트 (자식 오브젝트로 분리 — Image와 같은 GameObject 불가)
            var textRect = NewRect("Text", root);
            SetFill(textRect);
            AddTMP(textRect, "선택지 텍스트", 16, FontStyles.Normal, TextAlignmentOptions.Center, TextWhite);

            SavePrefab(root.gameObject, path);
        }

        #endregion

        #region Helpers

        private static RectTransform NewRoot(string name, float width, float height)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        private static RectTransform NewRect(string name, RectTransform parent)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static void SetFill(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI AddTMP(RectTransform parent, string text, float size,
            FontStyles style, TextAlignmentOptions align, Color color)
        {
            var tmp = parent.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = color;
            if (_font != null) tmp.font = _font;
            return tmp;
        }

        private static void Wire(Object target, string field, Object value)
        {
            if (target == null || value == null) return;
            var prop = target.GetType().GetField(field,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop != null)
                prop.SetValue(target, value);
            else
                Debug.LogWarning($"[PrefabBuilder] 필드 '{field}' 을(를) {target.GetType().Name}에서 찾을 수 없음");
        }

        private static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[PrefabBuilder] 생성 완료: {path}");
        }

        #endregion
    }
}
