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
    /// EventSceneReworkBuilder Wiring — EventReworkView에 Sprite/Container/Prefab 자동 연결 +
    /// EventChoiceRow Prefab 생성.
    /// </summary>
    public static partial class EventSceneReworkBuilder
    {
        private const string CHOICE_ROW_PREFAB = "Assets/03.Data/UI/EventScene/Prefabs/EventChoiceRowPrefab.prefab";

        /// <summary>
        /// ★ BuildPrefab()에서 호출 — View에 모든 자식 참조 + Sprite 자동 연결.
        /// </summary>
        private static void WireEventReworkView(EventReworkView view)
        {
            if (view == null) return;
            var root = view.transform;

            // Glass Window 자식
            var glassWindowGo = FindDescendant(root, "GlassWindow");
            if (glassWindowGo != null)
                WireField(view, "_glassWindowImage", glassWindowGo.GetComponent<Image>());

            var emblemGo = FindDescendant(root, "Emblem");
            if (emblemGo != null)
                WireField(view, "_emblemText", emblemGo.GetComponent<TextMeshProUGUI>());

            // Glass Panel
            var glassPanelGo = FindDescendant(root, "GlassPanel");
            if (glassPanelGo != null)
                WireField(view, "_glassPanelImage", glassPanelGo.GetComponent<Image>());

            // TopBar 자식들
            var themeTagGo = FindDescendant(root, "ThemeTag");
            if (themeTagGo != null)
                WireField(view, "_themeTag", themeTagGo.GetComponent<TextMeshProUGUI>());

            var eventTypeTagGo = FindDescendant(root, "EventTypeTag");
            if (eventTypeTagGo != null)
                WireField(view, "_eventTypeTag", eventTypeTagGo.GetComponent<TextMeshProUGUI>());

            var titleGo = FindDescendant(root, "EventTitle");
            if (titleGo != null)
                WireField(view, "_eventTitle", titleGo.GetComponent<TextMeshProUGUI>());

            var narrGo = FindDescendant(root, "Narrative");
            if (narrGo != null)
                WireField(view, "_narrative", narrGo.GetComponent<TextMeshProUGUI>());

            var choiceContainerGo = FindDescendant(root, "ChoiceContainer");
            if (choiceContainerGo != null)
                WireField(view, "_choiceContainer", choiceContainerGo.transform);

            // Result Panel
            var resultPanelGo = FindDescendant(root, "ResultPanel");
            if (resultPanelGo != null)
                WireField(view, "_resultPanel", resultPanelGo);

            var resultTextGo = FindDescendant(root, "ResultText");
            if (resultTextGo != null)
                WireField(view, "_resultText", resultTextGo.GetComponent<TextMeshProUGUI>());

            var resultConfirmGo = FindDescendant(root, "ResultConfirmButton");
            if (resultConfirmGo != null)
                WireField(view, "_resultConfirmButton", resultConfirmGo.GetComponent<Button>());

            // Close
            var closeGo = FindDescendant(root, "CloseButton");
            if (closeGo != null)
                WireField(view, "_closeButton", closeGo.GetComponent<Button>());

            // ★ GlassWindow Sprite 5종 (EventType별) 자동 연결
            WireField(view, "_glassWindowStory",    LoadEventSprite("GlassWindow_Story.png"));
            WireField(view, "_glassWindowTreasure", LoadEventSprite("GlassWindow_Treasure.png"));
            WireField(view, "_glassWindowTrap",     LoadEventSprite("GlassWindow_Trap.png"));
            WireField(view, "_glassWindowNPC",      LoadEventSprite("GlassWindow_NPC.png"));
            WireField(view, "_glassWindowShrine",   LoadEventSprite("GlassWindow_Shrine.png"));

            // Panel 배경 Sprite
            WireField(view, "_panelBackgroundSprite", LoadEventSprite("PanelBackground.png"));

            // ★ ChoiceRow Prefab 생성 후 _choiceRowPrefab에 연결
            BuildEventChoiceRowPrefab();
            WireField(view, "_choiceRowPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(CHOICE_ROW_PREFAB));

            // EditorUtility.SetDirty — 프리팹 직렬화 강제
            EditorUtility.SetDirty(view.gameObject);
        }

        /// <summary>
        /// ★ EventChoiceRow Prefab — ChoiceRow (Image 배경 + Button + HLG) 자식 4종.
        /// </summary>
        public static void BuildEventChoiceRowPrefab()
        {
            EnsureInitialized(); // 폰트 보장

            var go = new GameObject("EventChoiceRowPrefab", typeof(RectTransform), typeof(CanvasRenderer));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(640f, 64f);

            // 배경 — 9-slice
            var bgImg = go.AddComponent<Image>();
            bgImg.sprite = LoadEventSprite("ChoiceRow_Bg.png");
            bgImg.type = Image.Type.Sliced;
            bgImg.color = Color.white;
            bgImg.raycastTarget = true;

            // Button
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bgImg;

            // HLG — ChoiceText(좌flex) / RiskTag(우prefW)
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(16, 12, 8, 8);
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // 좌측 세로 컨테이너 (ChoiceText + ChoiceDesc)
            var leftGo = new GameObject("LeftColumn", typeof(RectTransform));
            leftGo.transform.SetParent(go.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(leftGo, flexW: 1, prefH: 48);
            var leftVlg = leftGo.AddComponent<VerticalLayoutGroup>();
            leftVlg.childControlWidth = true;
            leftVlg.childControlHeight = true;
            leftVlg.spacing = 2;
            leftVlg.childAlignment = TextAnchor.MiddleLeft;

            // ChoiceText — 메인 텍스트 (Cinzel Bold 15pt)
            var textGo = new GameObject("ChoiceText", typeof(RectTransform), typeof(CanvasRenderer));
            textGo.transform.SetParent(leftGo.transform, false);
            var textTmp = textGo.AddComponent<TextMeshProUGUI>();
            textTmp.text = "선택지 텍스트";
            textTmp.font = FontLabel();
            textTmp.fontSize = 15;
            textTmp.color = Color.white;
            textTmp.alignment = TextAlignmentOptions.Left;
            textTmp.raycastTarget = false;
            textTmp.enableWordWrapping = true;
            UIAutoBindHelper.EnsureLayoutElement(textGo, flexW: 1, prefH: 22);

            // ChoiceDesc — 작은 설명 (Cormorant Italic 12pt)
            var descGo = new GameObject("ChoiceDesc", typeof(RectTransform), typeof(CanvasRenderer));
            descGo.transform.SetParent(leftGo.transform, false);
            var descTmp = descGo.AddComponent<TextMeshProUGUI>();
            descTmp.text = "설명 텍스트";
            descTmp.font = FontItalic();
            descTmp.fontStyle = FontStyles.Italic;
            descTmp.fontSize = 12;
            descTmp.color = new Color(0.65f, 0.6f, 0.5f, 1f);
            descTmp.alignment = TextAlignmentOptions.Left;
            descTmp.raycastTarget = false;
            descTmp.enableWordWrapping = true;
            UIAutoBindHelper.EnsureLayoutElement(descGo, flexW: 1, prefH: 18);

            // 우측 세로 컨테이너 (RiskTag + DisabledReason)
            var rightGo = new GameObject("RightColumn", typeof(RectTransform));
            rightGo.transform.SetParent(go.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(rightGo, prefW: 110, prefH: 48);
            var rightVlg = rightGo.AddComponent<VerticalLayoutGroup>();
            rightVlg.childControlWidth = true;
            rightVlg.childControlHeight = true;
            rightVlg.spacing = 4;
            rightVlg.childAlignment = TextAnchor.UpperRight;

            // RiskTag — Image 배경 + TMP 자식
            var riskTagGo = new GameObject("RiskTag", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            riskTagGo.transform.SetParent(rightGo.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(riskTagGo, prefW: 100, prefH: 22);
            var riskTagImg = riskTagGo.GetComponent<Image>();
            riskTagImg.sprite = LoadEventSprite("ChoiceRow_RiskTag.png");
            riskTagImg.type = Image.Type.Sliced;
            riskTagImg.color = new Color(1f, 1f, 1f, 0.15f); // 기본 — 런타임에 RiskLevel이 override
            riskTagImg.raycastTarget = false;

            var riskTagLabelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            riskTagLabelGo.transform.SetParent(riskTagGo.transform, false);
            var riskLabelRt = riskTagLabelGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(riskLabelRt);
            var riskLabelTmp = riskTagLabelGo.AddComponent<TextMeshProUGUI>();
            riskLabelTmp.text = "☘  SAFE";
            riskLabelTmp.font = FontLabel();
            riskLabelTmp.fontSize = 10;
            riskLabelTmp.color = Color.white;
            riskLabelTmp.alignment = TextAlignmentOptions.Center;
            riskLabelTmp.raycastTarget = false;

            // DisabledReason — 비활성 사유 (별도 작은 TMP, 초기 비활성)
            var reasonGo = new GameObject("DisabledReason", typeof(RectTransform), typeof(CanvasRenderer));
            reasonGo.transform.SetParent(rightGo.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(reasonGo, flexW: 1, prefH: 18);
            var reasonTmp = reasonGo.AddComponent<TextMeshProUGUI>();
            reasonTmp.text = "골드 50 필요";
            reasonTmp.font = FontItalic();
            reasonTmp.fontStyle = FontStyles.Italic;
            reasonTmp.fontSize = 10;
            reasonTmp.color = new Color(0.8f, 0.3f, 0.3f, 1f);
            reasonTmp.alignment = TextAlignmentOptions.Right;
            reasonTmp.raycastTarget = false;
            reasonGo.SetActive(false);

            // EventChoiceRowRework 컴포넌트 부착 — 자동 바인딩 활성
            go.AddComponent<EventChoiceRowRework>();

            // Prefab 저장
            PrefabUtility.SaveAsPrefabAsset(go, CHOICE_ROW_PREFAB);
            Object.DestroyImmediate(go);

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// ★ MapSceneReworkBuilder 통합용 — EventReworkView Prefab 로드 헬퍼.
        /// </summary>
        public static GameObject LoadEventReworkViewPrefab()
        {
            EnsureInitialized();

            // Prefab이 없으면 빌드
            if (!System.IO.File.Exists(OUTPUT_PREFAB))
            {
                BuildPrefab();
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(OUTPUT_PREFAB);
        }
    }
}
#endif
