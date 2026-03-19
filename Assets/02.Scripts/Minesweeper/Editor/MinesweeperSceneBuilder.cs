#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Minesweeper.Editor
{
    /// <summary>
    /// 지뢰찾기 씬 자동 생성 에디터
    /// </summary>
    public static class MinesweeperSceneBuilder
    {
        [MenuItem("Minesweeper/Build Scene", false, 1)]
        public static void BuildMinesweeperScene()
        {
            // 씬 생성
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 메인 카메라 설정
            var camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = 6;
                camera.backgroundColor = new Color(0.2f, 0.2f, 0.3f);
            }

            // Canvas 생성
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem 확인
            var eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                var eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
            }

            // 게임 매니저 생성
            var gameManagerObj = new GameObject("MinesweeperGame");
            var gameManager = gameManagerObj.AddComponent<MinesweeperGame>();

            // UI 배경 패널
            var backgroundPanel = CreateUIObject("Background", canvasObj.transform);
            var bgImage = backgroundPanel.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.4f);
            var bgRect = backgroundPanel.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // 상단 UI 패널
            var headerPanel = CreateUIObject("Header", canvasObj.transform);
            var headerRect = headerPanel.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = new Vector2(0, -10);
            headerRect.sizeDelta = new Vector2(-20, 50);

            // 지뢰 카운트 텍스트
            var mineCountText = CreateText("MineCountText", headerPanel.transform, "지뢰: 10");
            var mineCountRect = mineCountText.GetComponent<RectTransform>();
            mineCountRect.anchorMin = new Vector2(0, 0.5f);
            mineCountRect.anchorMax = new Vector2(0, 0.5f);
            mineCountRect.pivot = new Vector2(0, 0.5f);
            mineCountRect.anchoredPosition = new Vector2(20, 0);
            mineCountRect.sizeDelta = new Vector2(150, 40);

            // 타이머 텍스트
            var timerText = CreateText("TimerText", headerPanel.transform, "시간: 0s");
            var timerRect = timerText.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(1, 0.5f);
            timerRect.anchorMax = new Vector2(1, 0.5f);
            timerRect.pivot = new Vector2(1, 0.5f);
            timerRect.anchoredPosition = new Vector2(-20, 0);
            timerRect.sizeDelta = new Vector2(150, 40);

            // 재시작 버튼
            var restartButton = CreateButton("RestartButton", headerPanel.transform, "재시작");
            var restartRect = restartButton.GetComponent<RectTransform>();
            restartRect.anchorMin = new Vector2(0.5f, 0.5f);
            restartRect.anchorMax = new Vector2(0.5f, 0.5f);
            restartRect.pivot = new Vector2(0.5f, 0.5f);
            restartRect.anchoredPosition = Vector2.zero;
            restartRect.sizeDelta = new Vector2(100, 35);

            // 그리드 컨테이너
            var gridContainer = CreateUIObject("GridContainer", canvasObj.transform);
            var gridRect = gridContainer.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.5f, 0.5f);
            gridRect.anchorMax = new Vector2(0.5f, 0.5f);
            gridRect.pivot = new Vector2(0.5f, 0.5f);
            gridRect.anchoredPosition = new Vector2(0, 30);
            gridRect.sizeDelta = new Vector2(400, 400);

            var gridLayout = gridContainer.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(40, 40);
            gridLayout.spacing = new Vector2(2, 2);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 9;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            // 셀 프리팹 생성
            var cellPrefab = CreateCellPrefab();

            // 게임 오버 패널
            var gameOverPanel = CreateUIObject("GameOverPanel", canvasObj.transform);
            var gameOverImage = gameOverPanel.AddComponent<Image>();
            gameOverImage.color = new Color(0, 0, 0, 0.8f);
            var gameOverRect = gameOverPanel.GetComponent<RectTransform>();
            gameOverRect.anchorMin = Vector2.zero;
            gameOverRect.anchorMax = Vector2.one;
            gameOverRect.sizeDelta = Vector2.zero;
            gameOverPanel.SetActive(false);

            // 결과 텍스트
            var resultText = CreateText("ResultText", gameOverPanel.transform, "승리!");
            var resultTextComp = resultText.GetComponent<Text>();
            resultTextComp.fontSize = 48;
            resultTextComp.alignment = TextAnchor.MiddleCenter;
            var resultRect = resultText.GetComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.5f, 0.6f);
            resultRect.anchorMax = new Vector2(0.5f, 0.6f);
            resultRect.pivot = new Vector2(0.5f, 0.5f);
            resultRect.sizeDelta = new Vector2(300, 80);

            // 게임 매니저에 연결
            SerializedObject so = new SerializedObject(gameManager);
            so.FindProperty("_gridContainer").objectReferenceValue = gridContainer.transform;
            so.FindProperty("_cellPrefab").objectReferenceValue = cellPrefab;
            so.FindProperty("_mineCountText").objectReferenceValue = mineCountText.GetComponent<Text>();
            so.FindProperty("_timerText").objectReferenceValue = timerText.GetComponent<Text>();
            so.FindProperty("_restartButton").objectReferenceValue = restartButton.GetComponent<Button>();
            so.FindProperty("_gameOverPanel").objectReferenceValue = gameOverPanel;
            so.FindProperty("_resultText").objectReferenceValue = resultText.GetComponent<Text>();
            so.ApplyModifiedProperties();

            // 프리팹 저장
            string prefabPath = "Assets/02.Scripts/Minesweeper/CellPrefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(cellPrefab, prefabPath);
            Object.DestroyImmediate(cellPrefab);

            // 씬 저장
            string scenePath = "Assets/01.Scenes/Minesweeper.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log("Minesweeper scene created successfully at: " + scenePath);
            Debug.Log("Cell prefab saved at: " + prefabPath);

            AssetDatabase.Refresh();
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private static GameObject CreateText(string name, Transform parent, string text)
        {
            var obj = CreateUIObject(name, parent);
            var textComp = obj.AddComponent<Text>();
            textComp.text = text;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 20;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;
            return obj;
        }

        private static GameObject CreateButton(string name, Transform parent, string text)
        {
            var obj = CreateUIObject(name, parent);
            var image = obj.AddComponent<Image>();
            image.color = new Color(0.5f, 0.5f, 0.6f);
            obj.AddComponent<Button>();

            var textObj = CreateText("Text", obj.transform, text);
            var textComp = textObj.GetComponent<Text>();
            textComp.fontSize = 16;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return obj;
        }

        private static GameObject CreateCellPrefab()
        {
            var cellObj = new GameObject("CellPrefab");

            // 배경 이미지
            var bgImage = cellObj.AddComponent<Image>();
            bgImage.color = new Color(0.7f, 0.7f, 0.7f);

            // 버튼
            var button = cellObj.AddComponent<Button>();

            // 셀 컴포넌트
            var cell = cellObj.AddComponent<Cell>();

            // 클릭 핸들러
            cellObj.AddComponent<CellClickHandler>();

            // 숫자 텍스트
            var textObj = new GameObject("NumberText");
            textObj.transform.SetParent(cellObj.transform, false);
            var text = textObj.AddComponent<Text>();
            text.text = "";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            // 깃발 아이콘 (간단한 텍스트로 대체)
            var flagObj = new GameObject("FlagIcon");
            flagObj.transform.SetParent(cellObj.transform, false);
            var flagText = flagObj.AddComponent<Text>();
            flagText.text = "F";
            flagText.fontSize = 20;
            flagText.fontStyle = FontStyle.Bold;
            flagText.alignment = TextAnchor.MiddleCenter;
            flagText.color = Color.red;
            var flagRect = flagObj.GetComponent<RectTransform>();
            flagRect.anchorMin = Vector2.zero;
            flagRect.anchorMax = Vector2.one;
            flagRect.sizeDelta = Vector2.zero;
            flagObj.SetActive(false);

            // 지뢰 아이콘
            var mineObj = new GameObject("MineIcon");
            mineObj.transform.SetParent(cellObj.transform, false);
            var mineText = mineObj.AddComponent<Text>();
            mineText.text = "X";
            mineText.fontSize = 24;
            mineText.fontStyle = FontStyle.Bold;
            mineText.alignment = TextAnchor.MiddleCenter;
            mineText.color = Color.black;
            var mineRect = mineObj.GetComponent<RectTransform>();
            mineRect.anchorMin = Vector2.zero;
            mineRect.anchorMax = Vector2.one;
            mineRect.sizeDelta = Vector2.zero;
            mineObj.SetActive(false);

            // RectTransform 설정 (Image가 이미 추가했으므로 가져오기만 함)
            var rect = cellObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(40, 40);

            // SerializedObject로 연결
            SerializedObject so = new SerializedObject(cell);
            so.FindProperty("_button").objectReferenceValue = button;
            so.FindProperty("_numberText").objectReferenceValue = text;
            so.FindProperty("_backgroundImage").objectReferenceValue = bgImage;
            so.FindProperty("_flagIcon").objectReferenceValue = flagText;
            so.FindProperty("_mineIcon").objectReferenceValue = mineText;
            so.ApplyModifiedProperties();

            return cellObj;
        }
    }
}
#endif
