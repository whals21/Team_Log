using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

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
    }
}
