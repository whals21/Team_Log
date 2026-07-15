using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace TeamLog.Editor
{
    /// <summary>
    /// 목업 기반 독립 UI 테스트 씬 빌더 — 런타임 의존성 제로.
    /// 슬롯 내부에 VLG 대신 명시적 앵커 배치 사용 (레이아웃 충돌 원천 차단).
    /// LeftContent + RightColumn 분리 구조로 컬럼 정렬 100% 보장.
    /// 메뉴: Tools/Battle UI/Build UI Test Scene
    /// </summary>
    public static class BattleUITestSceneBuilder
    {
        // ── 색상 ──
        private static readonly Color BgDark = new Color(0.06f, 0.06f, 0.12f);
        private static readonly Color TopBarBg = new Color(0.09f, 0.13f, 0.24f, 0.95f);
        private static readonly Color BottomBarBg = new Color(0.08f, 0.08f, 0.14f, 0.95f);
        private static readonly Color PanelBg = new Color(0.12f, 0.16f, 0.24f, 0.95f);
        private static readonly Color SlotBg = new Color(0.12f, 0.16f, 0.24f, 0.95f);
        private static readonly Color DividerC = new Color(0.16f, 0.16f, 0.30f, 0.80f);
        private static readonly Color AccentRed = new Color(0.77f, 0.12f, 0.23f);
        private static readonly Color AccentGreen = new Color(0.15f, 0.68f, 0.38f);
        private static readonly Color AccentYellow = new Color(0.96f, 0.82f, 0.25f);
        private static readonly Color APCyan = new Color(0.3f, 0.75f, 0.97f);
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextDim = new Color(0.72f, 0.72f, 0.80f);

        private const string SPRITE_BASE = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components";
        private const string SPRITE_SOLID_FRAME = SPRITE_BASE + "/Frame/BasicFrame_Round12.png";
        private const string SPRITE_PLAYER_PANEL = SPRITE_BASE + "/Frame/BasicFrame_Round12_Gradient.png";
        private const string FONT_SDF = "Assets/08.Resource/Fonts/NanumGothic SDF.asset";

        private const int BOTTOM_BAR_HEIGHT = 300; // 1080의 ~28%
        private const int TOP_BAR_HEIGHT = 44;
        private const int RIGHT_COL_WIDTH = 140;
        private const int SLOT_SPACING = 8;
        private const int ROW_SPACING = 6;

        [MenuItem("Tools/Battle UI/Build UI Test Scene", false, 98)]
        public static void BuildTestScene()
        {
            const string scenePath = "Assets/01.Scenes/BattleUITestScene.unity";

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_SDF);
            if (font == null)
            {
                var guids = AssetDatabase.FindAssets("NanumGothic SDF t:TMP_FontAsset");
                if (guids.Length > 0)
                    font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            // Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            SceneManager.MoveGameObjectToScene(camGo, scene);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.backgroundColor = BgDark;
            cam.clearFlags = CameraClearFlags.SolidColor;

            // Canvas
            var canvasGo = new GameObject("TestCanvas");
            SceneManager.MoveGameObjectToScene(canvasGo, scene);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var root = CreateRect("UIRoot", canvas.transform);
            SetFill(root);
            root.gameObject.AddComponent<Image>().color = BgDark;

            // ── Top Bar ──
            CreateTopBar(root, font);

            // ── Center (Enemy panels) ──
            CreateCenter(root, font);

            // ── Bottom Bar (핵심 — 2행 그리드) ──
            CreateBottomBar(root, font);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("[TestSceneBuilder] BattleUITestScene created at " + scenePath);
        }

        // ════════════════════════════════════════
        //  Top Bar
        // ════════════════════════════════════════
        private static void CreateTopBar(RectTransform parent, TMP_FontAsset font)
        {
            var bar = CreateRect("TopBar", parent);
            bar.anchorMin = new Vector2(0, 1);
            bar.anchorMax = new Vector2(1, 1);
            bar.pivot = new Vector2(0.5f, 1);
            bar.sizeDelta = new Vector2(0, TOP_BAR_HEIGHT);
            AddImage(bar, TopBarBg);

            // Turn info (left)
            var turn = CreateText("TurnInfo", bar, "턴 2  ·  2층 (2/4)", font, 14, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);
            turn.anchorMin = new Vector2(0, 0);
            turn.anchorMax = new Vector2(0.5f, 1);
            turn.offsetMin = new Vector2(16, 0);
            turn.offsetMax = new Vector2(0, 0);

            // Buttons (right)
            var btns = new[] { ("파티", new Color(0.17f, 0.17f, 0.27f, 0.9f)), ("로그", new Color(0.17f, 0.17f, 0.27f, 0.9f)), ("1x", new Color(0.85f, 0.45f, 0.1f, 0.95f)) };
            for (int i = 0; i < btns.Length; i++)
            {
                var btn = CreateRect($"Btn_{btns[i].Item1}", bar);
                btn.anchorMin = new Vector2(1, 0.5f);
                btn.anchorMax = new Vector2(1, 0.5f);
                btn.pivot = new Vector2(1, 0.5f);
                btn.anchoredPosition = new Vector2(-12 - i * 64, 0);
                btn.sizeDelta = new Vector2(56, 28);
                AddImage(btn, btns[i].Item2);
                var t = CreateText("T", btn, btns[i].Item1, font, 13, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
                SetFill(t);
            }

            // Divider
            var div = CreateRect("Div", bar);
            div.anchorMin = new Vector2(0, 0);
            div.anchorMax = new Vector2(1, 0);
            div.pivot = new Vector2(0.5f, 0);
            div.sizeDelta = new Vector2(0, 2);
            AddImage(div, DividerC);
        }

        // ════════════════════════════════════════
        //  Center (적 패널)
        // ════════════════════════════════════════
        private static void CreateCenter(RectTransform parent, TMP_FontAsset font)
        {
            var center = CreateRect("CenterArea", parent);
            center.anchorMin = Vector2.zero;
            center.anchorMax = Vector2.one;
            center.offsetMin = new Vector2(0, BOTTOM_BAR_HEIGHT + 4);
            center.offsetMax = new Vector2(0, -TOP_BAR_HEIGHT - 2);

            var hlg = center.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.padding = new RectOffset(16, 16, 16, 16);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 2개 적 패널 — 각각 다른 타겟 지정
            CreateEnemyPanel(hlg.GetComponent<RectTransform>(), "고블린", "30/30", font,
                targetName: "아셰", targetColor: new Color(0.85f, 0.3f, 0.15f));
            CreateEnemyPanel(hlg.GetComponent<RectTransform>(), "고블린", "30/30", font,
                targetName: "듀란", targetColor: new Color(0.25f, 0.45f, 0.85f));
        }

        private static void CreateEnemyPanel(RectTransform parent, string name, string hp, TMP_FontAsset font,
            string targetName = "", Color targetColor = default)
        {
            var panel = CreateRect($"Enemy_{name}", parent);
            var img = AddImage(panel, PanelBg);
            var sprite = LoadSprite(SPRITE_SOLID_FRAME);
            if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; }

            // Intent (패널 "위" 외부 — ignoreLayout)
            // [공격 아이콘][데미지][공격] → [타겟 초상화][타겟명] — 간격 최소화
            var intent = CreateRect("IntentSlot", panel);
            intent.anchorMin = new Vector2(0.5f, 1f);
            intent.anchorMax = new Vector2(0.5f, 1f);
            intent.pivot = new Vector2(0.5f, 0f);
            intent.anchoredPosition = new Vector2(0, 2);
            intent.sizeDelta = new Vector2(200, 28);
            intent.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            AddImage(intent, new Color(0.15f, 0.05f, 0.05f, 0.9f));
            // ★ HLG 제거 — 명시적 좌표 배치로 자식 너비 100% 보장
            // 자식 합 = 20+22+48+12+18+62 = 182, +spacing(2×5=10) +padding(4+4=8) = 200 = 슬롯 너비

            // ── 의도 섹션 ──
            // x=4: Icon(20) → x=26: Value(22) → x=50: Text(48)

            // Icon
            var intIcon = CreateRect("Icon", intent);
            AnchorLeft(intIcon, 4, 20, 20);
            AddImage(intIcon, AccentRed);

            // Value
            var intVal = CreateText("Value", intent, "6", font, 15, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);
            AnchorLeft(intVal, 26, 22, 20);

            // Text
            var intText = CreateText("Text", intent, "공격", font, 10, FontStyles.Normal, TextAlignmentOptions.Left, new Color(1, 1, 1, 0.8f));
            AnchorLeft(intText, 50, 48, 20);

            // ── 타겟 섹션 ──
            if (!string.IsNullOrEmpty(targetName))
            {
                // x=100: Arrow(12) → x=114: Portrait(18) → x=134: Name(62) → x=196(+4 padding) = 200
                var arrow = CreateText("Arrow", intent, "→", font, 11, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.9f, 0.5f, 0.3f));
                AnchorLeft(arrow, 100, 12, 20);

                // 타겟 초상화 배지
                var portrait = CreateRect("TargetPortrait", intent);
                AnchorLeft(portrait, 114, 18, 18);
                AddImage(portrait, targetColor);
                var pLabel = CreateText("L", portrait, targetName.Substring(0, 1), font, 10, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
                SetFill(pLabel);

                // 타겟명
                var targetT = CreateText("TargetName", intent, targetName, font, 9, FontStyles.Bold, TextAlignmentOptions.Left, new Color(1f, 0.7f, 0.5f));
                AnchorLeft(targetT, 134, 62, 20);
                targetT.gameObject.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;
                targetT.gameObject.GetComponent<TextMeshProUGUI>().overflowMode = TextOverflowModes.Ellipsis;
            }

            // VLG for panel content
            var vlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4; vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true; vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            var le = panel.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 200; le.minWidth = 160;
            le.preferredHeight = 280; le.minHeight = 220; le.flexibleWidth = 0;

            // Avatar
            var avatar = CreateRect("Avatar", panel);
            avatar.sizeDelta = new Vector2(0, 110);
            AddImage(avatar, new Color(0.05f, 0.08f, 0.15f, 0.95f));
            var aLabel = CreateText("L", avatar, "적", font, 24, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.4f, 0.5f, 0.6f, 0.7f));
            SetFill(aLabel);

            // Name
            var nameR = CreateText("Name", panel, name, font, 16, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
            nameR.sizeDelta = new Vector2(0, 22);

            // HP Bar
            var hpCont = CreateRect("HPBar", panel);
            hpCont.sizeDelta = new Vector2(0, 22);
            AddImage(hpCont, new Color(0.2f, 0.1f, 0.1f));
            var fill = CreateRect("Fill", hpCont);
            fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(1f, 1f);
            fill.offsetMin = new Vector2(2, 2); fill.offsetMax = new Vector2(-2, -2);
            AddImage(fill, AccentRed);
            var hpT = CreateText("HPText", hpCont, hp, font, 13, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
            SetFill(hpT);
        }

        // ════════════════════════════════════════
        //  Bottom Bar — 핵심 구조
        // ════════════════════════════════════════
        private static void CreateBottomBar(RectTransform parent, TMP_FontAsset font)
        {
            var bar = CreateRect("BottomBar", parent);
            bar.anchorMin = Vector2.zero;
            bar.anchorMax = new Vector2(1, 0);
            bar.pivot = new Vector2(0.5f, 0);
            bar.sizeDelta = new Vector2(0, BOTTOM_BAR_HEIGHT);
            AddImage(bar, BottomBarBg);

            // Divider
            var div = CreateRect("Div", bar);
            div.anchorMin = new Vector2(0, 1); div.anchorMax = new Vector2(1, 1);
            div.pivot = new Vector2(0.5f, 1); div.sizeDelta = new Vector2(0, 2);
            AddImage(div, DividerC);

            // ★ 핵심: LeftContent + RightColumn 분리
            // HLG(bar): [LeftContent flex=1] [RightColumn prefW=140 flex=0]
            var barHlg = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            barHlg.spacing = 8;
            barHlg.padding = new RectOffset(8, 8, 6, 6);
            barHlg.childAlignment = TextAnchor.UpperCenter;
            barHlg.childControlWidth = true;
            barHlg.childControlHeight = true;
            barHlg.childForceExpandWidth = false;
            barHlg.childForceExpandHeight = true;

            // ── LeftContent (캐릭터 행 + 스킬 행, 같은 너비 공유) ──
            var left = CreateRect("LeftContent", bar);
            var leftLe = left.gameObject.AddComponent<LayoutElement>();
            leftLe.flexibleWidth = 1;
            var leftVlg = left.gameObject.AddComponent<VerticalLayoutGroup>();
            leftVlg.spacing = ROW_SPACING;
            leftVlg.padding = new RectOffset(0, 0, 0, 0);
            leftVlg.childAlignment = TextAnchor.UpperCenter;
            leftVlg.childControlWidth = true;
            leftVlg.childControlHeight = true;
            leftVlg.childForceExpandWidth = true;
            leftVlg.childForceExpandHeight = true;

            // 행1: 캐릭터 카드 4개
            CreateCharacterRow(left, font);
            // 행2: 스킬 슬롯 4개
            CreateSkillRow(left, font);

            // ── RightColumn (AP + 버튼, 같은 컬럼) ──
            var right = CreateRect("RightColumn", bar);
            var rightLe = right.gameObject.AddComponent<LayoutElement>();
            rightLe.preferredWidth = RIGHT_COL_WIDTH;
            rightLe.minWidth = 120;
            rightLe.flexibleWidth = 0;
            var rightVlg = right.gameObject.AddComponent<VerticalLayoutGroup>();
            rightVlg.spacing = ROW_SPACING;
            rightVlg.padding = new RectOffset(0, 0, 0, 0);
            rightVlg.childAlignment = TextAnchor.UpperCenter;
            rightVlg.childControlWidth = true;
            rightVlg.childControlHeight = true;
            rightVlg.childForceExpandWidth = true;
            rightVlg.childForceExpandHeight = true;

            // APArea (행1에 해당)
            CreateAPArea(right, font);
            // ButtonArea (행2에 해당)
            CreateButtonArea(right, font);
        }

        // ── 캐릭터 행 ──
        private static void CreateCharacterRow(RectTransform parent, TMP_FontAsset font)
        {
            var row = CreateRect("CharacterRow", parent);
            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = SLOT_SPACING; hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;

            string[] names = { "아셰", "듀란", "루미", "시빌" };
            string[] hps = { "88/88", "120/120", "75/75", "55/55" };
            for (int i = 0; i < 4; i++)
                CreateCharacterCard(row, names[i], hps[i], font);
        }

        private static void CreateCharacterCard(RectTransform parent, string name, string hp, TMP_FontAsset font)
        {
            var card = CreateRect($"Char_{name}", parent);
            var le = card.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 120; le.flexibleWidth = 1;

            var img = AddImage(card, PanelBg);
            var sprite = LoadSprite(SPRITE_PLAYER_PANEL);
            if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; }
            var ol = card.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(0.6f, 0.1f, 0.18f, 0.5f);
            ol.effectDistance = new Vector2(1, -1);

            var vlg = card.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2; vlg.padding = new RectOffset(6, 6, 4, 4);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true; vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            // Name
            var nameT = CreateText("Name", card, name, font, 14, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);
            nameT.sizeDelta = new Vector2(0, 18);

            // HP Bar
            var hpBar = CreateRect("HPBar", card);
            hpBar.sizeDelta = new Vector2(0, 20);
            AddImage(hpBar, new Color(0.15f, 0.15f, 0.15f));
            var fill = CreateRect("Fill", hpBar);
            fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(1f, 1f);
            fill.offsetMin = new Vector2(2, 2); fill.offsetMax = new Vector2(-2, -2);
            AddImage(fill, AccentGreen);
            var hpT = CreateText("HPText", hpBar, hp, font, 11, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
            SetFill(hpT);

            // Stats
            var stats = CreateText("Stats", card, "ATK 10", font, 10, FontStyles.Normal, TextAlignmentOptions.Left, TextDim);
            stats.sizeDelta = new Vector2(0, 14);
        }

        // ── 스킬 행 ──
        private static void CreateSkillRow(RectTransform parent, TMP_FontAsset font)
        {
            var row = CreateRect("SkillRow", parent);
            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = SLOT_SPACING; hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;

            string[] names = { "잿더미", "복수 일격", "빙결창", "예언" };
            string[] casters = { "아셰", "듀란", "루미", "시빌" };
            string[] effects = { "단일 5 + Burn", "단일 10 + 분노 5+", "단일 5 + Freeze", "1턴 뒤 발동" };
            int[] costs = { 1, 2, 1, 1 };
            string[] types = { "공격", "공격", "공격", "버프" };
            Color[] typeColors = {
                new Color(0.78f, 0.16f, 0.16f, 0.95f),
                new Color(0.78f, 0.16f, 0.16f, 0.95f),
                new Color(0.78f, 0.16f, 0.16f, 0.95f),
                new Color(0.85f, 0.66f, 0.14f, 0.95f)
            };

            for (int i = 0; i < 4; i++)
                CreateSkillSlot(row, names[i], casters[i], effects[i], costs[i], types[i], typeColors[i], font);
        }

        // ── 스킬 슬롯 (VLG 없이 명시적 앵커 배치) ──
        private static void CreateSkillSlot(RectTransform parent, string skillName, string caster,
            string effect, int cost, string typeLabel, Color typeColor, TMP_FontAsset font)
        {
            var slot = CreateRect("SkillSlot", parent);
            var le = slot.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 120; le.flexibleWidth = 1;

            // 배경
            var bg = AddImage(slot, SlotBg);
            var sprite = LoadSprite(SPRITE_SOLID_FRAME);
            if (sprite != null) { bg.sprite = sprite; bg.type = Image.Type.Sliced; }
            var ol = slot.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(0.27f, 0.27f, 0.27f, 0.80f);
            ol.effectDistance = new Vector2(1, -1);

            // ★ VLG 사용 안 함 — 명시적 앵커로 각 요소 배치
            // 슬롯 높이는 부모 HLG가 결정 (~140px). 각 요소는 위에서부터 고정 위치.

            // Header: CasterName (좌) + TypeTag (우) — top=2, height=14
            // HLG 없이 명시적 앵커만 사용 (레이아웃 충돌 원천 차단)
            var header = CreateRect("Header", slot);
            AnchorTopFill(header, 2, 14);

            // CasterName: 좌측 절반
            var casterT = CreateText("CasterNameText", header, caster, font, 10, FontStyles.Normal, TextAlignmentOptions.Left, TextDim);
            casterT.anchorMin = new Vector2(0, 0);
            casterT.anchorMax = new Vector2(0.5f, 1);
            casterT.offsetMin = new Vector2(4, 0);
            casterT.offsetMax = new Vector2(-2, 0);

            // TypeTag: 우측 고정 36x12
            var typeTag = CreateRect("TypeTag", header);
            typeTag.anchorMin = new Vector2(1, 0.5f);
            typeTag.anchorMax = new Vector2(1, 0.5f);
            typeTag.pivot = new Vector2(1, 0.5f);
            typeTag.anchoredPosition = new Vector2(-4, 0);
            typeTag.sizeDelta = new Vector2(36, 12);
            AddImage(typeTag, typeColor);
            var typeT = CreateText("T", typeTag, typeLabel, font, 8, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
            SetFill(typeT);

            // SkillIcon — top=18, height=42, centered
            var icon = CreateRect("SkillIcon", slot);
            AnchorTopCentered(icon, 18, 42, 42);
            AddImage(icon, typeColor);

            // SkillNameText — top=64, height=16
            var nameR = CreateRect("SkillNameText", slot);
            AnchorTopFill(nameR, 64, 16);
            var nameT = nameR.gameObject.AddComponent<TextMeshProUGUI>();
            nameT.font = font; nameT.text = skillName;
            nameT.fontSize = 13; nameT.fontStyle = FontStyles.Bold;
            nameT.alignment = TextAlignmentOptions.Center;
            nameT.color = TextWhite; nameT.raycastTarget = false;
            nameT.enableWordWrapping = false; nameT.overflowMode = TextOverflowModes.Ellipsis;

            // EffectText — top=82, height=20
            var effectR = CreateRect("EffectText", slot);
            AnchorTopFill(effectR, 82, 20);
            var effectT = effectR.gameObject.AddComponent<TextMeshProUGUI>();
            effectT.font = font; effectT.text = effect;
            effectT.fontSize = 9;
            effectT.alignment = TextAlignmentOptions.Center;
            effectT.color = TextDim; effectT.raycastTarget = false;
            effectT.enableWordWrapping = true; effectT.overflowMode = TextOverflowModes.Ellipsis;

            // CostBadge — bottom=2, centered, 26x18
            var costBg = CreateRect("CostBadge", slot);
            AnchorBottomCentered(costBg, 2, 26, 18);
            AddImage(costBg, new Color(0.2f, 0.4f, 0.8f, 0.9f));
            var costT = CreateText("CostText", costBg, cost.ToString(), font, 12, FontStyles.Bold, TextAlignmentOptions.Center, Color.black);
            SetFill(costT);

            // 오버레이들은 모두 VLG가 없으므로 앵커만으로 배치됨 — 충돌 불가
        }

        // ── AP Area ──
        private static void CreateAPArea(RectTransform parent, TMP_FontAsset font)
        {
            var ap = CreateRect("APArea", parent);
            AddImage(ap, new Color(0.09f, 0.13f, 0.24f, 0.9f));
            var vlg = ap.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4; vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true; vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            var label = CreateText("Label", ap, "AP", font, 13, FontStyles.Bold, TextAlignmentOptions.Center, APCyan);
            label.sizeDelta = new Vector2(0, 18);
            var num = CreateText("APText", ap, "5/5", font, 22, FontStyles.Bold, TextAlignmentOptions.Center, APCyan);
            num.sizeDelta = new Vector2(0, 28);
            var bar = CreateRect("APBar", ap);
            bar.sizeDelta = new Vector2(0, 10);
            AddImage(bar, new Color(0.12f, 0.12f, 0.18f));
            var fill = CreateRect("Fill", bar);
            fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one;
            fill.offsetMin = Vector2.zero; fill.offsetMax = Vector2.zero;
            AddImage(fill, APCyan);
        }

        // ── Button Area ──
        private static void CreateButtonArea(RectTransform parent, TMP_FontAsset font)
        {
            var ba = CreateRect("ButtonArea", parent);
            var vlg = ba.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6; vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = true;

            // Reroll button
            var reroll = CreateRect("RerollButton", ba);
            AddImage(reroll, new Color(0.85f, 0.45f, 0.1f, 0.95f));
            var rt = CreateText("T", reroll, "리롤\n2/2", font, 12, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
            SetFill(rt);

            // End Turn button
            var endBtn = CreateRect("EndTurnButton", ba);
            AddImage(endBtn, AccentRed);
            var et = CreateText("T", endBtn, "턴 종료\n[T]", font, 14, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
            SetFill(et);
        }

        // ════════════════════════════════════════
        //  유틸리티 — 앵커 헬퍼
        // ════════════════════════════════════════

        /// <summary>부모 좌측에서 xOffset, 세로 중앙, 고정 크기</summary>
        private static void AnchorLeft(RectTransform rt, float xOffset, float width, float height)
        {
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = new Vector2(xOffset, 0);
            rt.sizeDelta = new Vector2(width, height);
        }

        /// <summary>부모 상단에서 yOffset 아래, 너비 꽉 채움, 높이 고정</summary>
        private static void AnchorTopFill(RectTransform rt, float yOffset, float height)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -yOffset);
            rt.sizeDelta = new Vector2(0, height);
        }

        /// <summary>부모 상단 중앙, 고정 크기</summary>
        private static void AnchorTopCentered(RectTransform rt, float yOffset, float width, float height)
        {
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -yOffset);
            rt.sizeDelta = new Vector2(width, height);
        }

        /// <summary>부모 하단에서 yOffset 위, 중앙, 고정 크기</summary>
        private static void AnchorBottomCentered(RectTransform rt, float yOffset, float width, float height)
        {
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, yOffset);
            rt.sizeDelta = new Vector2(width, height);
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        private static void SetFill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Image AddImage(RectTransform rt, Color color)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static RectTransform CreateText(string name, Transform parent, string text,
            TMP_FontAsset font, float size, FontStyles style, TextAlignmentOptions align, Color color)
        {
            var rt = CreateRect(name, parent);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = color;
            tmp.raycastTarget = false;
            return rt;
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
