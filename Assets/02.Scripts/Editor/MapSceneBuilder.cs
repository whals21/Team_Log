using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI.Map;

namespace TeamLog.Editor
{
    public static class MapSceneBuilder
    {
        private const string SCENE_PATH = "Assets/01.Scenes/MapScene.unity";
        private const string KOREAN_FONT_SDF = "Assets/08.Resource/Fonts/NanumGothic SDF.asset";

        // 색상 팔레트 (기존 BattleUI와 통일)
        private static readonly Color BgDark = new Color(0.08f, 0.08f, 0.16f);
        private static readonly Color PanelDark = new Color(0.12f, 0.12f, 0.22f);
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextDim = new Color(0.7f, 0.7f, 0.75f);
        private static readonly Color AccentGold = new Color(0.96f, 0.82f, 0.25f);

        [MenuItem("TeamLog/Scene/Build Map Scene")]
        public static void BuildMapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KOREAN_FONT_SDF);

            // 캔버스 생성
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // 배경
            var bg = CreateUIObject("Background", canvasObj.transform);
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = BgDark;
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // 상단 정보 패널
            var topPanel = CreatePanel("TopPanel", canvasObj.transform,
                new Vector2(0, 0.92f), new Vector2(1, 1), PanelDark);

            var floorLabel = CreateText("FloorLabel", topPanel.transform, font,
                "층 1", 28, TextWhite, TextAlignmentOptions.Center);
            SetAnchors(floorLabel.GetComponent<RectTransform>(),
                new Vector2(0.35f, 0f), new Vector2(0.65f, 1f));

            var goldLabel = CreateText("GoldLabel", topPanel.transform, font,
                "0 G", 24, AccentGold, TextAlignmentOptions.Center);
            SetAnchors(goldLabel.GetComponent<RectTransform>(),
                new Vector2(0.65f, 0f), new Vector2(0.9f, 1f));

            // 맵 컨테이너 (노드가 배치되는 영역)
            var nodeContainer = CreateUIObject("NodeContainer", canvasObj.transform);
            var nodeRect = nodeContainer.GetComponent<RectTransform>();
            SetAnchors(nodeRect, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.9f));

            // 연결선 컨테이너
            var lineContainer = CreateUIObject("LineContainer", canvasObj.transform);
            var lineRect = lineContainer.GetComponent<RectTransform>();
            SetAnchors(lineRect, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.9f));

            // MapView 컴포넌트 추가
            var mapView = canvasObj.AddComponent<MapView>();

            // 프리팹 에셋이 없으므로 인스펙터에서 연결해야 함
            // SetRectTransform 및 SerializeField 바인딩
            var serializedObj = new SerializedObject(mapView);
            var nodeContainerProp = serializedObj.FindProperty("_nodeContainer");
            var lineContainerProp = serializedObj.FindProperty("_lineContainer");
            var floorLabelProp = serializedObj.FindProperty("_floorLabel");
            var goldLabelProp = serializedObj.FindProperty("_goldLabel");

            if (nodeContainerProp != null)
            {
                nodeContainerProp.objectReferenceValue = nodeRect;
                lineContainerProp.objectReferenceValue = lineRect;
                floorLabelProp.objectReferenceValue = floorLabel;
                goldLabelProp.objectReferenceValue = goldLabel;
                serializedObj.ApplyModifiedProperties();
            }

            // MapSceneSetup 추가
            var setupObj = new GameObject("MapSceneSetup");
            var setup = setupObj.AddComponent<MapSceneSetup>();

            var setupSerialized = new SerializedObject(setup);
            var mapViewProp = setupSerialized.FindProperty("_mapView");
            if (mapViewProp != null)
            {
                mapViewProp.objectReferenceValue = mapView;
                setupSerialized.ApplyModifiedProperties();
            }

            // 씬 저장
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log($"[MapSceneBuilder] 맵 씬 생성 완료: {SCENE_PATH}");
            Debug.Log("[MapSceneBuilder] 프리팹(NodeButton, ConnectionLine, PlayerMarker)을 인스펙터에 연결하세요.");
        }

        #region UI Helpers

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

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.sizeDelta = Vector2.zero;
        }

        #endregion
    }
}
