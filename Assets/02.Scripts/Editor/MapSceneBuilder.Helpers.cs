using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Meta;
using TeamLog.Reward;

namespace TeamLog.Editor
{
    /// <summary>
    /// MapSceneBuilder — UI 헬퍼 및 유틸리티 메서드
    /// 진입점+와이어링: MapSceneBuilder.cs
    /// 패널 생성: MapSceneBuilder.Panels.cs
    /// </summary>
    public static partial class MapSceneBuilder
    {
        private static void WireProperty(SerializedObject ser, string property, Object value)
        {
            var prop = ser.FindProperty(property);
            if (prop != null && value != null)
                prop.objectReferenceValue = value;
        }

        /// <summary>
        /// 메타 상점 데이터 풀 바인딩 (Phase 8D).
        /// _allTraits / _allUpgrades / _allRelics — 각 에셋 디렉토리에서 로드.
        /// </summary>
        private static void WireMetaShopDataPools(SerializedObject ser)
        {
            // 캐릭터 특성 풀
            var traitAssets = LoadAllAssets<CharacterTraitData>("Assets/03.Data/CharacterTraits");
            var traitsProp = ser.FindProperty("_allTraits");
            if (traitsProp != null && traitAssets.Count > 0)
            {
                traitsProp.arraySize = traitAssets.Count;
                for (int i = 0; i < traitAssets.Count; i++)
                    traitsProp.GetArrayElementAtIndex(i).objectReferenceValue = traitAssets[i];
            }

            // 메타 강화 풀
            var upgradeAssets = LoadAllAssets<MetaUpgradeData>("Assets/03.Data/MetaUpgrades");
            var upgradesProp = ser.FindProperty("_allUpgrades");
            if (upgradesProp != null && upgradeAssets.Count > 0)
            {
                upgradesProp.arraySize = upgradeAssets.Count;
                for (int i = 0; i < upgradeAssets.Count; i++)
                    upgradesProp.GetArrayElementAtIndex(i).objectReferenceValue = upgradeAssets[i];
            }

            // 유물 풀 (메타 상점 표시용)
            var relicAssets = LoadAllAssets<RelicData>("Assets/03.Data/Relics");
            var relicsProp = ser.FindProperty("_allRelics");
            if (relicsProp != null && relicAssets.Count > 0)
            {
                relicsProp.arraySize = relicAssets.Count;
                for (int i = 0; i < relicAssets.Count; i++)
                    relicsProp.GetArrayElementAtIndex(i).objectReferenceValue = relicAssets[i];
            }
        }

        private static List<T> LoadAllAssets<T>(string folder, string namePrefix = null) where T : Object
        {
            var result = new List<T>();
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                // namePrefix가 지정되면 파일명(에셋 경로의 마지막 부분)으로 필터링
                if (namePrefix != null)
                {
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                    if (!fileName.StartsWith(namePrefix)) continue;
                }
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) result.Add(asset);
            }
            return result;
        }

        private static List<T> LoadAssetsByNames<T>(string folder, string[] names) where T : Object
        {
            var result = new List<T>();
            foreach (var name in names)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>($"{folder}/{name}.asset");
                if (asset != null) result.Add(asset);
            }
            return result;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private static GameObject CreatePanel(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var obj = CreateUIObject(name, parent);
            var image = obj.AddComponent<Image>();
            image.color = color;
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = Vector2.zero;
            return obj;
        }

        private static GameObject CreateFullImage(string name, Transform parent, Color color)
        {
            var obj = CreateUIObject(name, parent);
            var image = obj.AddComponent<Image>();
            image.color = color;
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            return obj;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent,
            TMP_FontAsset font, string text, int fontSize, Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.Left)
        {
            var obj = CreateUIObject(name, parent);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            return tmp;
        }

        private static GameObject CreateButton(string name, Transform parent,
            TMP_FontAsset font, string text, int fontSize, Color textColor)
        {
            var obj = CreateUIObject(name, parent);
            var bg = obj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.25f);
            var button = obj.AddComponent<Button>();
            button.targetGraphic = bg;

            var textObj = CreateUIObject("Text", obj.transform);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return obj;
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.sizeDelta = Vector2.zero;
        }

        /// <summary>
        /// 오버레이 패널 생성 — 전체 화면 이미지 + 비활성화 + 선택적 CanvasGroup
        /// </summary>
        private static GameObject CreateOverlay(string name, Transform parent, Color color, bool withCanvasGroup = false)
        {
            var overlay = CreateFullImage(name, parent, color);
            overlay.SetActive(false);
            if (withCanvasGroup)
            {
                var cg = overlay.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
            }
            return overlay;
        }

        /// <summary>
        /// 수직 스크롤 뷰 생성 — ScrollRect + Viewport + Content(RectMask2D, VerticalLayoutGroup, ContentSizeFitter)
        /// </summary>
        private static RectTransform CreateVerticalScrollView(string name, Transform parent,
            int spacing = 8, TextAnchor childAlignment = TextAnchor.UpperLeft)
        {
            var scrollObj = CreateUIObject(name, parent);
            var scrollRect = scrollObj.AddComponent<ScrollRect>();

            var viewport = CreateUIObject("Viewport", scrollObj.transform);
            SetAnchors(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            viewport.AddComponent<RectMask2D>();

            var contentObj = CreateUIObject("Content", viewport.transform);
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            var vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing;
            vlg.childAlignment = childAlignment;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            return contentRect;
        }
    }
}
