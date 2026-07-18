#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI;
using TeamLog.UI.PartySelection;

namespace TeamLog.Editor
{
    /// <summary>
    /// PartySelectionSceneBuilder — 영역별 파츠 (UI-D.2 재작성).
    /// ★ 모든 자식에 LayoutElement로 명시적 크기. ForceExpand=false 표준.
    /// </summary>
    public static partial class PartySelectionSceneBuilder
    {
        // =========================================================
        // HEADER (56px) — 로고+타이틀(좌) / 메타 pill+설정(우)
        // =========================================================
        public static void BuildHeader(RectTransform parent)
        {
            // 헤더 루트 — 부모 영역 전체 스트레치
            var headerGo = new GameObject("HeaderPanel", typeof(RectTransform), typeof(Image));
            headerGo.transform.SetParent(parent, false);
            var headerRect = headerGo.GetComponent<RectTransform>();
            StretchToParent(headerRect);
            // ★ LayoutElement 제거 — CreateSectionSlot이 preferredHeight=56, flexibleHeight=0 관리
            var headerImg = headerGo.GetComponent<Image>();
            headerImg.sprite = LoadSprite(SPRITE_SLATE);
            headerImg.color = Color.white;
            headerImg.type = Image.Type.Sliced;
            headerImg.raycastTarget = false;

            // 하단 골드 라인 (자식, 맨 아래)
            var goldLineGo = new GameObject("GoldLine", typeof(RectTransform), typeof(Image));
            goldLineGo.transform.SetParent(headerGo.transform, false);
            var glRect = goldLineGo.GetComponent<RectTransform>();
            glRect.anchorMin = new Vector2(0, 0);
            glRect.anchorMax = new Vector2(1, 0);
            glRect.pivot = new Vector2(0.5f, 0);
            glRect.sizeDelta = new Vector2(0, 3);
            glRect.anchoredPosition = Vector2.zero;
            var glImg = goldLineGo.GetComponent<Image>();
            glImg.sprite = LoadSprite(SPRITE_GOLD_BORDER_THIN);
            glImg.color = Color.white;
            glImg.type = Image.Type.Sliced;

            // 메인 HLG — ★ controlWidth/Height=true (자식 flexibleWidth/Height 존중)
            var mainHLG_go = new GameObject("MainLayout", typeof(RectTransform));
            mainHLG_go.transform.SetParent(headerGo.transform, false);
            StretchToParent(mainHLG_go.GetComponent<RectTransform>());
            var mlLe = mainHLG_go.AddComponent<LayoutElement>();
            mlLe.flexibleWidth = 1;
            mlLe.flexibleHeight = 1;
            var hlg = AddHLG(mainHLG_go, TextAnchor.MiddleCenter, 24,
                controlWidth: true, controlHeight: true,
                padLeft: 36, padRight: 36, padTop: 8, padBottom: 8);

            // ── 좌측 (로고 + 타이틀) ──
            var leftGo = CreateLayoutChild("Left", mainHLG_go.transform, flexW: 1, flexH: 1);
            leftGo.GetComponent<LayoutElement>().minWidth = 250;
            var leftHLG = AddHLG(leftGo, TextAnchor.MiddleLeft, 12, true, true);

            var crestGo = CreateLayoutChild("Crest", leftGo.transform, prefW: 34, prefH: 34);
            var crestImg = crestGo.AddComponent<Image>();
            crestImg.sprite = LoadSprite(SPRITE_CREST_LOGO);
            crestImg.color = Color.white;
            crestImg.raycastTarget = false;

            var titleGroup = CreateLayoutChild("TitleGroup", leftGo.transform, flexW: 1, flexH: 1);
            titleGroup.GetComponent<LayoutElement>().minWidth = 150;
            var tVlg = AddVLG(titleGroup, TextAnchor.MiddleLeft, 0, true, true);

            var titleTmp = CreateText("Title", titleGroup.transform,
                "TEAM LOG", 22, P.DFGoldL, _fontCinzelBlack ?? _fontKorean, TextAlignmentOptions.Left);
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.characterSpacing = 4;

            var subtitleTmp = CreateText("Subtitle", titleGroup.transform,
                "A ROGUELIKE CHRONICLE", 9, P.DFInkFaint, _fontCinzelRegular ?? _fontKorean, TextAlignmentOptions.Left);
            subtitleTmp.characterSpacing = 4;

            // ── 우측 (메타 pill + 설정) ──
            var rightGo = CreateLayoutChild("Right", mainHLG_go.transform, flexW: 1, flexH: 1);
            rightGo.GetComponent<LayoutElement>().minWidth = 400;
            var rightHLG = AddHLG(rightGo, TextAnchor.MiddleRight, 12, true, true);

            CreateMetaPill(rightGo.transform, "STAGE", "I — Grey Forest", P.DFGoldL);
            CreateMetaPill(rightGo.transform, "ASCENSION", "5", P.DFBloodL);
            CreateMetaPill(rightGo.transform, "GOLD", "◐ 248", P.DFGoldL);

            var settingsBtn = CreateButton("BtnSettings", rightGo.transform,
                LoadSprite(SPRITE_SLATE_LIGHT), Color.white,
                "⚙", 14, P.DFGold, _fontKorean, 32, 32);
        }

        private static void CreateMetaPill(Transform parent, string label, string value, Color valueColor)
        {
            var pillGo = CreateLayoutChild($"Pill_{label}", parent, prefH: 28);
            // ★ Pill에 명시적 preferredWidth 부여 — 텍스트 길이 기반 (대략)
            var pillLe = pillGo.GetComponent<LayoutElement>();
            pillLe.preferredWidth = Mathf.Max(80, label.Length * 7 + value.Length * 9 + 32);
            pillLe.minWidth = 80;
            var pillImg = pillGo.AddComponent<Image>();
            pillImg.sprite = LoadSprite(SPRITE_SLATE_LIGHT);
            pillImg.color = Color.white;
            pillImg.type = Image.Type.Sliced;
            pillImg.raycastTarget = false;
            AddHLG(pillGo, TextAnchor.MiddleCenter, 8, true, true,
                padLeft: 12, padRight: 12, padTop: 4, padBottom: 4);

            var labelTmp = CreateText("Label", pillGo.transform,
                label, 9, P.DFInkFaint, _fontCinzelRegular ?? _fontKorean, TextAlignmentOptions.Left);
            var valueTmp = CreateText("Value", pillGo.transform,
                value, 11, valueColor, _fontCinzelBold ?? _fontKorean, TextAlignmentOptions.Left);
            valueTmp.fontStyle = FontStyles.Bold;
        }

        // =========================================================
        // STAGE — BtnPrev / MainArea(초상화+정보) / BtnNext
        // =========================================================
        public static void BuildStage(RectTransform parent)
        {
            var stageGo = new GameObject("StageContainer", typeof(RectTransform));
            stageGo.transform.SetParent(parent, false);
            StretchToParent(stageGo.GetComponent<RectTransform>());
            // ★ 핵심: 부모 LayoutGroup(없지만 안전장치) 또는 자식 LayoutGroup이
            // StageContainer 크기를 잘못 계산하지 않도록 flexibleWidth/Height=1 명시
            var stageLe = stageGo.AddComponent<LayoutElement>();
            stageLe.flexibleWidth = 1;
            stageLe.flexibleHeight = 1;
            stageLe.minWidth = 800;
            stageLe.minHeight = 400;

            // 메인 HLG — 3단 (좌 버튼 / 중앙 / 우 버튼)
            var hlg = AddHLG(stageGo, TextAnchor.MiddleCenter, 8, true, true);

            // 좌 버튼
            var prevBtn = CreateButton("BtnPrev", stageGo.transform,
                LoadSprite(SPRITE_SLATE), Color.white,
                "‹", 36, P.DFGold, _fontCinzelBlack ?? _fontKorean, 50, 50);
            var prevLe = prevBtn.GetComponent<LayoutElement>();
            prevLe.preferredWidth = 50;
            prevLe.flexibleWidth = 0;
            prevLe.flexibleHeight = 1;

            // 중앙 MainArea — flexibleWidth=1로 남은 공간 모두 차지
            var mainArea = CreateLayoutChild("MainArea", stageGo.transform, flexW: 1, flexH: 1);
            var mainLe = mainArea.GetComponent<LayoutElement>();
            mainLe.minWidth = 600;
            mainLe.minHeight = 400;
            var mainHLG = AddHLG(mainArea, TextAnchor.UpperCenter, 20, true, true,
                padLeft: 8, padRight: 8, padTop: 8, padBottom: 8);

            // ── 초상화 영역 (320px 고정) ──
            var portraitArea = CreateLayoutChild("PortraitArea", mainArea.transform, prefW: 320, flexH: 1);
            var paLe = portraitArea.GetComponent<LayoutElement>();
            paLe.minWidth = 320;
            paLe.flexibleWidth = 0;
            var pVlg = AddVLG(portraitArea, TextAnchor.UpperCenter, 8, false, true);

            BuildPortraitArea(portraitArea.transform);

            // ── 정보 패널 (남은 공간 모두 차지) ──
            var infoArea = CreateLayoutChild("InfoArea", mainArea.transform, flexW: 1, flexH: 1);
            var iaLe = infoArea.GetComponent<LayoutElement>();
            iaLe.minWidth = 300;
            var iVlg = AddVLG(infoArea, TextAnchor.UpperCenter, 8, true, false,
                padLeft: 4, padRight: 4);

            BuildInfoArea(infoArea.transform);

            // 우 버튼
            var nextBtn = CreateButton("BtnNext", stageGo.transform,
                LoadSprite(SPRITE_SLATE), Color.white,
                "›", 36, P.DFGold, _fontCinzelBlack ?? _fontKorean, 50, 50);
            var nextLe = nextBtn.GetComponent<LayoutElement>();
            nextLe.preferredWidth = 50;
            nextLe.flexibleWidth = 0;
            nextLe.flexibleHeight = 1;
        }

        // ── 초상화 영역 ──
        private static void BuildPortraitArea(Transform parent)
        {
            // ★ PortraitArea VLG 설정 보정 — 자식 controlHeight 존중, spacing/padding 정리
            var paVLG = parent.GetComponent<VerticalLayoutGroup>();
            if (paVLG != null)
            {
                paVLG.childControlHeight = true;
                paVLG.childForceExpandHeight = false;
                paVLG.spacing = 8;
                paVLG.padding = new RectOffset(0, 0, 0, 0);
            }

            // PortraitFrame — 320×370 (NamePlate 내부 하단 50px 포함, MechanicBox 100px와 합이 478)
            var portraitGo = CreateLayoutChild("PortraitFrame", parent, prefW: 320, prefH: 370);
            portraitGo.GetComponent<LayoutElement>().minWidth = 320;
            var portrait = portraitGo.AddComponent<CharacterPortraitBig>();

            // Frame 외곽 — 좌/상/우/하 골드 테두리 + 어두운 배경
            var frameGo = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            frameGo.transform.SetParent(portraitGo.transform, false);
            StretchToParent(frameGo.GetComponent<RectTransform>());
            var frameImg = frameGo.GetComponent<Image>();
            frameImg.sprite = LoadSprite(SPRITE_GOLD_BORDER);
            frameImg.color = Color.white;
            frameImg.type = Image.Type.Sliced;

            // InnerBG — 보이드 배경 (조금 더 넓게)
            var bgGo = new GameObject("InnerBG", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(frameGo.transform, false);
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.04f, 0.04f);
            bgRect.anchorMax = new Vector2(0.96f, 0.96f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.color = P.DFVoid;
            bgImg.raycastTarget = false;

            // 플레이스홀더 (Glow + Initial) — ★ Glow alpha 0.3 → 0.55 강화
            var placeholderGo = new GameObject("Placeholder", typeof(RectTransform));
            placeholderGo.transform.SetParent(bgGo.transform, false);
            StretchToParent(placeholderGo.GetComponent<RectTransform>());

            var glowImg = CreateStretchImage("Glow", placeholderGo.transform,
                LoadSprite(SPRITE_PARCHMENT_DARK), new Color(1f, 0.42f, 0.21f, 0.55f));

            // Initial — 더 크게 (180 → 220) 및 alpha 강화 (0.25 → 0.35)
            var initialTmp = CreateText("Initial", placeholderGo.transform,
                "?", 220, new Color(1f, 0.55f, 0.30f, 0.40f),
                _fontCinzelBlack ?? _fontKorean, TextAlignmentOptions.Center);
            initialTmp.fontStyle = FontStyles.Bold;
            initialTmp.raycastTarget = false;

            var portraitImgGo = new GameObject("PortraitImage", typeof(RectTransform), typeof(Image));
            portraitImgGo.transform.SetParent(bgGo.transform, false);
            StretchToParent(portraitImgGo.GetComponent<RectTransform>());
            var portraitImg = portraitImgGo.GetComponent<Image>();
            portraitImg.color = Color.white;
            portraitImg.gameObject.SetActive(false);

            // ★ 자원 배지 — 우상단, 더 크고 명확하게 (56×56 → 64×64, 자원색 테두리)
            var badgeGo = new GameObject("ResourceBadge", typeof(RectTransform), typeof(Image));
            badgeGo.transform.SetParent(portraitGo.transform, false);
            var bRect = badgeGo.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(1, 1);
            bRect.anchorMax = new Vector2(1, 1);
            bRect.pivot = new Vector2(1, 1);
            bRect.anchoredPosition = new Vector2(12, -12);
            bRect.sizeDelta = new Vector2(64, 64);
            var bImg = badgeGo.GetComponent<Image>();
            bImg.sprite = LoadSprite(SPRITE_GOLD_BORDER);
            bImg.color = Color.white;
            bImg.type = Image.Type.Sliced;
            bImg.raycastTarget = false;

            // 배지 내부 자원색 채우기 (Glow 효과)
            var badgeFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            badgeFill.transform.SetParent(badgeGo.transform, false);
            var bfRect = badgeFill.GetComponent<RectTransform>();
            bfRect.anchorMin = new Vector2(0.12f, 0.12f);
            bfRect.anchorMax = new Vector2(0.88f, 0.88f);
            bfRect.offsetMin = Vector2.zero;
            bfRect.offsetMax = Vector2.zero;
            var bfImg = badgeFill.GetComponent<Image>();
            bfImg.color = new Color(1f, 0.42f, 0.21f, 0.9f); // Ember 색 (Ashe 기본)
            bfImg.raycastTarget = false;

            var badgeContent = new GameObject("BadgeContent", typeof(RectTransform));
            badgeContent.transform.SetParent(badgeGo.transform, false);
            var bcRect = badgeContent.GetComponent<RectTransform>();
            bcRect.anchorMin = new Vector2(0.1f, 0.1f);
            bcRect.anchorMax = new Vector2(0.9f, 0.9f);
            bcRect.offsetMin = Vector2.zero;
            bcRect.offsetMax = Vector2.zero;
            AddVLG(badgeContent, TextAnchor.MiddleCenter, 1, true, true);

            var resInitial = CreateText("ResInitial", badgeContent.transform,
                "?", 22, Color.white, _fontCinzelBold ?? _fontKorean);
            resInitial.fontStyle = FontStyles.Bold;
            resInitial.outlineColor = Color.black;
            resInitial.outlineWidth = 0.3f;

            var resLabel = CreateText("ResLabel", badgeContent.transform,
                "?", 8, P.DFInk, _fontCinzelRegular ?? _fontKorean);
            resLabel.characterSpacing = 1;

            // 잠금 마크 (중앙)
            var lockGo = new GameObject("LockMark", typeof(RectTransform));
            lockGo.transform.SetParent(portraitGo.transform, false);
            var lRect = lockGo.GetComponent<RectTransform>();
            SetCentered(lRect, 80, 80);
            var lockTmp = CreateText("LockText", lockGo.transform,
                "[L]", 42, P.DFBloodL, _fontKorean);
            lockGo.SetActive(false);

            // ★ NamePlate — PortraitFrame 내부 하단에 배치 (Y=-40 → 25)
            // 이전: Y=-40이면 하단에서 -65~-15로 PortraitFrame 영역 밖으로 65px 삐져나가 MechanicBox와 겹침
            // 이후: Y=25면 하단에서 0~50 영역으로 PortraitFrame 내부 하단에 딱 맞음
            // (초상화 하단과만 겹치고 MechanicBox와는 완전 분리)
            var plateGo = new GameObject("NamePlate", typeof(RectTransform), typeof(Image));
            plateGo.transform.SetParent(portraitGo.transform, false);
            var plRect = plateGo.GetComponent<RectTransform>();
            plRect.anchorMin = new Vector2(0.5f, 0);
            plRect.anchorMax = new Vector2(0.5f, 0);
            plRect.pivot = new Vector2(0.5f, 0.5f);
            plRect.anchoredPosition = new Vector2(0, 25);  // ★ 내부 하단에 배치
            plRect.sizeDelta = new Vector2(280, 50);
            var plImg = plateGo.GetComponent<Image>();
            plImg.sprite = LoadSprite(SPRITE_GOLD_BORDER);
            plImg.color = Color.white;
            plImg.type = Image.Type.Sliced;
            plImg.raycastTarget = false;

            // Plate 내부 어두운 배경
            var plateBg = new GameObject("InnerBG", typeof(RectTransform), typeof(Image));
            plateBg.transform.SetParent(plateGo.transform, false);
            var pbgRect = plateBg.GetComponent<RectTransform>();
            pbgRect.anchorMin = new Vector2(0.04f, 0.04f);
            pbgRect.anchorMax = new Vector2(0.96f, 0.96f);
            pbgRect.offsetMin = Vector2.zero;
            pbgRect.offsetMax = Vector2.zero;
            var pbgImg = plateBg.GetComponent<Image>();
            pbgImg.color = P.DFSlate2;
            pbgImg.raycastTarget = false;

            var plateContent = new GameObject("PlateContent", typeof(RectTransform));
            plateContent.transform.SetParent(plateGo.transform, false);
            StretchToParent(plateContent.GetComponent<RectTransform>());
            AddVLG(plateContent, TextAnchor.MiddleCenter, 1, true, true, 6, 6, 6, 6);

            var nameTmp = CreateText("Name", plateContent.transform,
                "UNKNOWN", 22, P.DFGoldL, _fontCinzelBold ?? _fontKorean);
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.characterSpacing = 3;
            nameTmp.outlineColor = new Color(0, 0, 0, 0.8f);
            nameTmp.outlineWidth = 0.2f;

            var titleTmp = CreateText("Title", plateContent.transform,
                "", 12, P.DFInkDim, _fontCormorantItalic ?? _fontKorean);
            titleTmp.fontStyle = FontStyles.Italic;

            // ★ 메커니즘 박스 — 320×100 (Desc 공간 확보)
            var mechGo = CreateLayoutChild("MechanicBox", parent, prefW: 320, prefH: 100);
            mechGo.GetComponent<LayoutElement>().minWidth = 320;
            var mechImg = mechGo.AddComponent<Image>();
            mechImg.sprite = LoadSprite(SPRITE_PARCHMENT_DARK);
            mechImg.color = Color.white;
            mechImg.type = Image.Type.Sliced;
            mechImg.raycastTarget = false;
            AddVLG(mechGo, TextAnchor.UpperLeft, 6, true, true, 12, 12, 12, 12);

            var mechTitle = CreateText("Title", mechGo.transform,
                "•  RESOURCE  MECHANIC", 10, P.DFGold,
                _fontCinzelRegular ?? _fontKorean, TextAlignmentOptions.Left);
            mechTitle.characterSpacing = 4;
            mechTitle.fontStyle = FontStyles.Bold;
            var mechDesc = CreateText("Desc", mechGo.transform,
                "", 11, P.DFParchment, _fontKorean, TextAlignmentOptions.Left);
            mechDesc.richText = true;

            var mechanicBox = mechGo.AddComponent<ResourceMechanicBox>();

            // ── 필드 바인딩 ──
            BindField(portrait, "_portraitFrame", frameImg);
            BindField(portrait, "_frameSprite", LoadSprite(SPRITE_GOLD_BORDER));
            BindField(portrait, "_innerBackground", bgImg);
            BindField(portrait, "_placeholderGroup", placeholderGo);
            BindField(portrait, "_glowImage", glowImg);
            BindField(portrait, "_glowSprite", LoadSprite(SPRITE_PARCHMENT_DARK));
            BindField(portrait, "_initialText", initialTmp);
            BindField(portrait, "_portraitImage", portraitImg);
            BindField(portrait, "_resourceBadge", bImg);
            BindField(portrait, "_resourceInitialText", resInitial);
            BindField(portrait, "_resourceLabelText", resLabel);
            BindField(portrait, "_lockMark", lockGo);
            BindField(portrait, "_plateBackground", plImg);
            BindField(portrait, "_plateSprite", LoadSprite(SPRITE_GOLD_BORDER));
            BindField(portrait, "_nameText", nameTmp);
            BindField(portrait, "_titleText", titleTmp);

            BindField(mechanicBox, "_background", mechImg);
            BindField(mechanicBox, "_boxSprite", LoadSprite(SPRITE_PARCHMENT_DARK));
            BindField(mechanicBox, "_titleText", mechTitle);
            BindField(mechanicBox, "_descText", mechDesc);
        }

        // ── 정보 패널 ──
        private static void BuildInfoArea(Transform parent)
        {
            // InfoArea 자체에 LayoutElement 명시 (부모 HLG 안에서 남은 공간 모두 차지)
            // parent가 InfoArea GameObject. LayoutElement minWidth=300, flexibleWidth=1, flexibleHeight=1
            // (BuildStage에서 CreateLayoutChild로 이미 설정)

            // ★ 핵심: InfoArea 내부 VLG를 controlHeight=true로 변경하여 자식 preferredHeight 존중
            // 자식들의 부모 영역 밖 삐져나감(겹침) 방지
            var infoAreaVLG = parent.GetComponent<VerticalLayoutGroup>();
            if (infoAreaVLG != null)
            {
                infoAreaVLG.childControlHeight = true;
                infoAreaVLG.childForceExpandHeight = false;
                infoAreaVLG.spacing = 6;  // 8 → 6으로 축소 (공간 절약)
                infoAreaVLG.padding = new RectOffset(4, 4, 0, 0);
            }

            // IdentityQuote (30px — 축소)
            var quoteGo = CreateLayoutChild("IdentityQuote", parent, prefH: 30, flexW: 1);
            var quoteImg = quoteGo.AddComponent<Image>();
            quoteImg.sprite = LoadSprite(SPRITE_PARCHMENT_DARK);
            quoteImg.color = new Color(1, 1, 1, 0.4f);
            quoteImg.type = Image.Type.Sliced;
            quoteImg.raycastTarget = false;

            var quoteTmp = CreateText("Text", quoteGo.transform,
                "", 11, P.DFParchment, _fontCormorantItalic ?? _fontKorean, TextAlignmentOptions.Left);
            quoteTmp.fontStyle = FontStyles.Italic;
            var qRect = quoteTmp.GetComponent<RectTransform>();
            qRect.offsetMin = new Vector2(20, 4);
            qRect.offsetMax = new Vector2(-8, -4);

            // StatsRow (42px — 축소)
            var statsGo = CreateLayoutChild("StatsRow", parent, prefH: 42, flexW: 1);
            AddHLG(statsGo, TextAnchor.UpperCenter, 8, true, true);

            CreateStatCell(statsGo.transform, "Stat_Vigor", "VIGOR", P.DFGoldL);
            CreateStatCell(statsGo.transform, "Stat_Resource", "RESOURCE", P.DFGoldL);
            CreateStatCell(statsGo.transform, "Stat_Role", "ROLE", P.DFGoldL);

            // StrengthWeaknessRow (38px — 축소)
            var swGo = CreateLayoutChild("StrengthWeaknessRow", parent, prefH: 38, flexW: 1);
            AddHLG(swGo, TextAnchor.UpperCenter, 8, true, true);

            CreateTraitBox(swGo.transform, "StrengthBox", "• STRENGTH", new Color(0.15f, 0.30f, 0.15f, 0.9f), new Color(0.49f, 0.64f, 0.29f));
            CreateTraitBox(swGo.transform, "WeaknessBox", "X VULNERABILITY", new Color(0.40f, 0.15f, 0.15f, 0.9f), P.DFBloodL);

            // ★ SkillGrid 확대 — cellSize 가로폭 InfoArea에 가깝게 (220→360), 높이 96→114
            var skillGridGo = CreateLayoutChild("SkillGrid", parent, prefH: 236, flexW: 1);
            var slg = skillGridGo.AddComponent<GridLayoutGroup>();
            slg.cellSize = new Vector2(360, 114);  // 220×96 → 360×114 (대폭 확대)
            slg.spacing = new Vector2(8, 8);
            slg.childAlignment = TextAnchor.UpperCenter;
            slg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            slg.constraintCount = 2;

            for (int i = 0; i < 4; i++) BuildSkillCard(skillGridGo.transform, $"Skill{i + 1}");

            // TraitGrid (3열, 90px — 축소)
            var traitGridGo = CreateLayoutChild("TraitGrid", parent, prefH: 90, flexW: 1);
            AddHLG(traitGridGo, TextAnchor.UpperCenter, 7, true, true);

            for (int i = 0; i < 3; i++) BuildTraitCard(traitGridGo.transform, $"Trait{i + 1}");
        }

        private static void CreateStatCell(Transform parent, string name, string label, Color valueColor)
        {
            var cellGo = CreateLayoutChild(name, parent, prefH: 56, flexW: 1);
            var cellImg = cellGo.AddComponent<Image>();
            cellImg.sprite = LoadSprite(SPRITE_SLATE);
            cellImg.color = Color.white;
            cellImg.type = Image.Type.Sliced;
            cellImg.raycastTarget = false;
            AddVLG(cellGo, TextAnchor.MiddleCenter, 1, true, true, 4, 4, 4, 4);

            var labelTmp = CreateText("Label", cellGo.transform,
                label, 9, P.DFInkFaint, _fontCinzelRegular ?? _fontKorean);
            labelTmp.characterSpacing = 3;
            var valueTmp = CreateText("Value", cellGo.transform,
                "—", 18, valueColor, _fontCinzelBold ?? _fontKorean);
            valueTmp.fontStyle = FontStyles.Bold;
            var subTmp = CreateText("Sub", cellGo.transform,
                "", 9, P.DFInkDim, _fontKorean);
        }

        private static void CreateTraitBox(Transform parent, string name, string label,
            Color bgColor, Color labelColor)
        {
            var boxGo = CreateLayoutChild(name, parent, prefH: 52, flexW: 1);
            var boxImg = boxGo.AddComponent<Image>();
            boxImg.sprite = LoadSprite(SPRITE_PARCHMENT_DARK);
            boxImg.color = bgColor;
            boxImg.type = Image.Type.Sliced;
            boxImg.raycastTarget = false;
            AddVLG(boxGo, TextAnchor.UpperLeft, 2, true, false, 8, 8, 8, 8);

            var labelTmp = CreateText("Label", boxGo.transform,
                label, 9, labelColor, _fontCinzelBold ?? _fontKorean, TextAlignmentOptions.Left);
            labelTmp.fontStyle = FontStyles.Bold;
            labelTmp.characterSpacing = 3;

            var descTmp = CreateText("Desc", boxGo.transform,
                "—", 10.5f, P.DFParchment, _fontKorean, TextAlignmentOptions.Left);
        }

        // ★ CreateSectionLabel 제거됨 (GC 2026-07-18) — 섹션 라벨 미사용

        private static void BuildSkillCard(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var card = go.AddComponent<SkillDetailCard>();
            var bg = CreateStretchImage("BG", go.transform, LoadSprite(SPRITE_SLATE), Color.white, Image.Type.Sliced);

            // 좌측 타입 띠
            var barGo = new GameObject("TypeBar", typeof(RectTransform), typeof(Image));
            barGo.transform.SetParent(go.transform, false);
            var barRect = barGo.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0, 0);
            barRect.anchorMax = new Vector2(0, 1);
            barRect.pivot = new Vector2(0, 0.5f);
            barRect.sizeDelta = new Vector2(4, 0);
            barRect.anchoredPosition = Vector2.zero;
            var barImg = barGo.GetComponent<Image>();
            barImg.color = P.SkillAttack;
            barImg.raycastTarget = false;

            // Head (좌상단, Height=36 — 44에서 축소)
            var headGo = new GameObject("Head", typeof(RectTransform));
            headGo.transform.SetParent(go.transform, false);
            var hRect = headGo.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0, 1);
            hRect.anchorMax = new Vector2(1, 1);
            hRect.pivot = new Vector2(0.5f, 1);
            hRect.sizeDelta = new Vector2(-20, 36);
            hRect.anchoredPosition = new Vector2(0, -4);
            // ★ controlWidth/Height=true로 변경 — Icon LayoutElement.preferredWidth 존중
            AddHLG(headGo, TextAnchor.MiddleLeft, 6, true, true, 8, 0, 0, 0);

            // ★ Icon 축소: 32 → 22, Image의 자동 preferredSize override
            var iconGo = CreateLayoutChild("Icon", headGo.transform, prefW: 22, prefH: 22);
            var iconLe = iconGo.GetComponent<LayoutElement>();
            iconLe.minWidth = 22;
            iconLe.minHeight = 22;
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.color = P.DFGoldL;
            iconImg.sprite = LoadSprite(SPRITE_GOLD_BORDER_THIN);
            iconImg.type = Image.Type.Sliced;
            iconImg.raycastTarget = false;

            var titleGroup = CreateLayoutChild("TitleGroup", headGo.transform, flexW: 1, flexH: 1);
            titleGroup.GetComponent<LayoutElement>().minWidth = 100;
            // ★ controlHeight=true — Title/Cost TMP 높이 명시
            AddVLG(titleGroup, TextAnchor.MiddleLeft, 0, true, true);

            var nameTmp = CreateText("Name", titleGroup.transform,
                "(skill)", 11, P.DFGoldL, _fontCinzelBold ?? _fontKorean, TextAlignmentOptions.Left);
            nameTmp.fontStyle = FontStyles.Bold;
            var costTmp = CreateText("Cost", titleGroup.transform,
                "AP 1", 8, P.DFGoldL, _fontCinzelBold ?? _fontKorean, TextAlignmentOptions.Left);

            // Badges (중간, Height=14 — 16에서 축소)
            var badgesGo = new GameObject("Badges", typeof(RectTransform));
            badgesGo.transform.SetParent(go.transform, false);
            var badgesRect = badgesGo.GetComponent<RectTransform>();
            badgesRect.anchorMin = new Vector2(0, 1);
            badgesRect.anchorMax = new Vector2(1, 1);
            badgesRect.pivot = new Vector2(0.5f, 1);
            badgesRect.sizeDelta = new Vector2(-20, 14);
            badgesRect.anchoredPosition = new Vector2(0, -42);
            // ★ controlWidth=true
            AddHLG(badgesGo, TextAnchor.MiddleLeft, 4, true, false, 8, 0, 0, 0);

            var typeBadge = CreateText("TypeBadge", badgesGo.transform,
                "공격", 8, P.SkillAttack, _fontKorean, TextAlignmentOptions.Left);
            var targetBadge = CreateText("TargetBadge", badgesGo.transform,
                "단일 적", 8, P.AccentRed, _fontKorean, TextAlignmentOptions.Left);
            var powerBadge = CreateText("PowerBadge", badgesGo.transform,
                "<b>5</b> 위력", 8, P.DFGoldL, _fontKorean, TextAlignmentOptions.Left);

            // Desc (중앙 여백)
            var descGo = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
            descGo.transform.SetParent(go.transform, false);
            var descRect = descGo.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(1, 1);
            descRect.offsetMin = new Vector2(10, 26);
            descRect.offsetMax = new Vector2(-10, -70);
            var descTmp = descGo.GetComponent<TextMeshProUGUI>();
            descTmp.fontSize = 10.5f;
            descTmp.color = P.DFInk;
            descTmp.alignment = TextAlignmentOptions.TopLeft;
            descTmp.raycastTarget = false;
            UIKoreanFont.EnsureFont(descTmp);

            // BonusBox (하단, Height=22)
            var bonusGo = new GameObject("BonusBox", typeof(RectTransform), typeof(Image));
            bonusGo.transform.SetParent(go.transform, false);
            var bonusRect = bonusGo.GetComponent<RectTransform>();
            bonusRect.anchorMin = new Vector2(0, 0);
            bonusRect.anchorMax = new Vector2(1, 0);
            bonusRect.pivot = new Vector2(0.5f, 0);
            bonusRect.sizeDelta = new Vector2(-20, 22);
            bonusRect.anchoredPosition = new Vector2(0, 4);
            var bonusImg = bonusGo.GetComponent<Image>();
            bonusImg.color = new Color(0.83f, 0.69f, 0.22f, 0.15f);
            bonusImg.raycastTarget = false;

            var bonusTmp = CreateText("Text", bonusGo.transform,
                "⚡ —", 9.5f, P.DFParchment, _fontKorean, TextAlignmentOptions.Left);
            var btRect = bonusTmp.GetComponent<RectTransform>();
            btRect.offsetMin = new Vector2(6, 2);
            btRect.offsetMax = new Vector2(-6, -2);

            BindField(card, "_background", bg);
            BindField(card, "_panelSprite", LoadSprite(SPRITE_SLATE));
            BindField(card, "_panelHoverSprite", LoadSprite(SPRITE_SLATE_LIGHT));
            BindField(card, "_typeColorBar", barImg);
            BindField(card, "_skillIcon", iconImg);
            BindField(card, "_nameText", nameTmp);
            BindField(card, "_costText", costTmp);
            BindField(card, "_typeBadge", typeBadge);
            BindField(card, "_targetBadge", targetBadge);
            BindField(card, "_powerBadge", powerBadge);
            BindField(card, "_descText", descTmp);
            BindField(card, "_bonusBox", bonusGo);
            BindField(card, "_bonusBackground", bonusImg);
            BindField(card, "_bonusText", bonusTmp);
        }

        private static void BuildTraitCard(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 90;
            le.flexibleWidth = 1;

            var card = go.AddComponent<TraitDetailCard>();
            var bg = CreateStretchImage("BG", go.transform, LoadSprite(SPRITE_SLATE), Color.white, Image.Type.Sliced);

            // Highlight ✦ (우상단)
            var hlGo = new GameObject("Highlight", typeof(RectTransform));
            hlGo.transform.SetParent(go.transform, false);
            var hlRect = hlGo.GetComponent<RectTransform>();
            hlRect.anchorMin = new Vector2(1, 1);
            hlRect.anchorMax = new Vector2(1, 1);
            hlRect.pivot = new Vector2(1, 1);
            hlRect.anchoredPosition = new Vector2(-4, -4);
            hlRect.sizeDelta = new Vector2(16, 16);
            var hlTmp = CreateText("Text", hlGo.transform,
                "✦", 14, P.DFGoldL, _fontKorean);
            hlGo.SetActive(false);

            // Content
            var contentGo = new GameObject("TraitContent", typeof(RectTransform));
            contentGo.transform.SetParent(go.transform, false);
            var cRect = contentGo.GetComponent<RectTransform>();
            cRect.anchorMin = Vector2.zero;
            cRect.anchorMax = Vector2.one;
            cRect.offsetMin = new Vector2(10, 6);
            cRect.offsetMax = new Vector2(-10, -6);
            AddVLG(contentGo, TextAnchor.UpperLeft, 3, true, false);

            var headGo = CreateLayoutChild("Head", contentGo.transform, flexW: 1);
            AddHLG(headGo, TextAnchor.MiddleLeft, 6, false, false);

            var nameTmp = CreateText("Name", headGo.transform,
                "(trait)", 10.5f, P.DFGoldL, _fontCinzelBold ?? _fontKorean, TextAlignmentOptions.Left);
            nameTmp.fontStyle = FontStyles.Bold;
            var tagTmp = CreateText("Tag", headGo.transform,
                "BASE", 8, Color.white, _fontCinzelRegular ?? _fontKorean, TextAlignmentOptions.Right);
            tagTmp.fontStyle = FontStyles.Bold;

            var descTmp = CreateText("Desc", contentGo.transform,
                "—", 10, P.DFInk, _fontKorean, TextAlignmentOptions.Left);

            var unlockGo = CreateLayoutChild("UnlockRow", contentGo.transform, flexW: 1);
            AddHLG(unlockGo, TextAnchor.MiddleLeft, 4, false, false);
            var lockIcon = CreateText("Icon", unlockGo.transform,
                "🔒", 9, P.DFBloodL, _fontKorean, TextAlignmentOptions.Left);
            var unlockTmp = CreateText("Text", unlockGo.transform,
                "잠김", 9, P.DFBloodL, _fontKorean, TextAlignmentOptions.Left);
            unlockGo.SetActive(false);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;

            BindField(card, "_background", bg);
            BindField(card, "_button", btn);
            BindField(card, "_selectedHighlight", hlTmp);
            BindField(card, "_normalSprite", LoadSprite(SPRITE_SLATE));
            BindField(card, "_hoverSprite", LoadSprite(SPRITE_SLATE_LIGHT));
            BindField(card, "_selectedSprite", LoadSprite(SPRITE_SLATE_LIGHT));
            BindField(card, "_nameText", nameTmp);
            BindField(card, "_tagText", tagTmp);
            BindField(card, "_descText", descTmp);
            BindField(card, "_unlockRow", unlockGo);
            BindField(card, "_unlockText", unlockTmp);
        }

        // =========================================================
        // CAROUSEL (110px)
        // =========================================================
        public static void BuildCarousel(RectTransform parent)
        {
            var carouselGo = new GameObject("CarouselPanel", typeof(RectTransform), typeof(Image));
            carouselGo.transform.SetParent(parent, false);
            StretchToParent(carouselGo.GetComponent<RectTransform>());
            var clLe = carouselGo.AddComponent<LayoutElement>();
            clLe.flexibleWidth = 1;
            clLe.flexibleHeight = 1;
            clLe.minWidth = 800;
            clLe.minHeight = 110;
            var cImg = carouselGo.GetComponent<Image>();
            cImg.color = P.DFVoid;
            cImg.raycastTarget = false;

            // 상단 골드 라인
            var goldLineGo = new GameObject("GoldLine", typeof(RectTransform), typeof(Image));
            goldLineGo.transform.SetParent(carouselGo.transform, false);
            var glRect = goldLineGo.GetComponent<RectTransform>();
            glRect.anchorMin = new Vector2(0, 1);
            glRect.anchorMax = new Vector2(1, 1);
            glRect.pivot = new Vector2(0.5f, 1);
            glRect.sizeDelta = new Vector2(0, 2);
            glRect.anchoredPosition = Vector2.zero;
            var glImg = goldLineGo.GetComponent<Image>();
            glImg.sprite = LoadSprite(SPRITE_GOLD_BORDER_THIN);
            glImg.color = Color.white;
            glImg.type = Image.Type.Sliced;

            // ScrollView
            var scrollGo = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGo.transform.SetParent(carouselGo.transform, false);
            var sRect = scrollGo.GetComponent<RectTransform>();
            sRect.anchorMin = Vector2.zero;
            sRect.anchorMax = Vector2.one;
            sRect.offsetMin = new Vector2(24, 10);
            sRect.offsetMax = new Vector2(-24, -10);
            var scrollImg = scrollGo.GetComponent<Image>();
            scrollImg.color = Color.clear;
            scrollImg.raycastTarget = false;  // ★ 자식 Button 클릭 차단 방지
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = false;

            // Content
            var contentGo = new GameObject("CarouselContent", typeof(RectTransform));
            contentGo.transform.SetParent(scrollGo.transform, false);
            var ctRect = contentGo.GetComponent<RectTransform>();
            ctRect.anchorMin = new Vector2(0, 0);
            ctRect.anchorMax = new Vector2(0, 1);
            ctRect.pivot = new Vector2(0, 0.5f);
            ctRect.sizeDelta = new Vector2(0, 0);
            ctRect.anchoredPosition = Vector2.zero;
            AddHLG(contentGo, TextAnchor.MiddleCenter, 12, false, true);
            ctRect.GetComponent<HorizontalLayoutGroup>().childForceExpandHeight = true;
            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = ctRect;

            _cachedCarouselItemTemplate = CreateCarouselItemTemplate(contentGo.transform);
            _cachedCarouselItemTemplate.gameObject.SetActive(false);
        }

        private static CharacterCarouselItem CreateCarouselItemTemplate(Transform parent)
        {
            var go = new GameObject("CarouselItemTemplate", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 78;
            le.preferredHeight = 100;

            var item = go.AddComponent<CharacterCarouselItem>();
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = null; // 아래 Image에서 설정

            var pVlg = AddVLG(go, TextAnchor.UpperCenter, 4, false, true);

            // Portrait (70×70)
            var portraitGo = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portraitGo.transform.SetParent(go.transform, false);
            var pRect = portraitGo.GetComponent<RectTransform>();
            pRect.sizeDelta = new Vector2(70, 70);
            var pImg = portraitGo.GetComponent<Image>();
            pImg.sprite = LoadSprite(SPRITE_GOLD_BORDER_THIN);
            pImg.color = P.DFGoldL;
            pImg.type = Image.Type.Sliced;
            pImg.raycastTarget = true;
            var pLe = portraitGo.AddComponent<LayoutElement>();
            pLe.preferredWidth = 70;
            pLe.preferredHeight = 70;

            btn.targetGraphic = pImg;

            var initialTmp = CreateText("Initial", portraitGo.transform,
                "?", 28, Color.white, _fontCinzelBlack ?? _fontKorean);
            initialTmp.fontStyle = FontStyles.Bold;

            var inPartyGo = new GameObject("InPartyBadge", typeof(RectTransform), typeof(Image));
            inPartyGo.transform.SetParent(portraitGo.transform, false);
            var ipRect = inPartyGo.GetComponent<RectTransform>();
            ipRect.anchorMin = new Vector2(1, 1);
            ipRect.anchorMax = new Vector2(1, 1);
            ipRect.pivot = new Vector2(1, 1);
            ipRect.anchoredPosition = new Vector2(6, -6);
            ipRect.sizeDelta = new Vector2(22, 22);
            var ipImg = inPartyGo.GetComponent<Image>();
            ipImg.sprite = LoadSprite(SPRITE_GOLD_BORDER_THIN);
            ipImg.color = P.DFBloodL;
            ipImg.type = Image.Type.Sliced;
            var checkTmp = CreateText("Check", inPartyGo.transform,
                "✓", 14, Color.white, _fontKorean);
            inPartyGo.SetActive(false);

            var lockGo = new GameObject("LockOverlay", typeof(RectTransform), typeof(Image));
            lockGo.transform.SetParent(portraitGo.transform, false);
            StretchToParent(lockGo.GetComponent<RectTransform>());
            var lockImg = lockGo.GetComponent<Image>();
            lockImg.color = new Color(0, 0, 0, 0.7f);
            var lockTmp = CreateText("Lock", lockGo.transform,
                "🔒", 18, P.DFBloodL, _fontKorean);
            lockGo.SetActive(false);

            var activeGo = new GameObject("ActiveRing", typeof(RectTransform), typeof(Image));
            activeGo.transform.SetParent(portraitGo.transform, false);
            var aRect = activeGo.GetComponent<RectTransform>();
            aRect.anchorMin = new Vector2(-0.05f, -0.05f);
            aRect.anchorMax = new Vector2(1.05f, 1.05f);
            aRect.offsetMin = Vector2.zero;
            aRect.offsetMax = Vector2.zero;
            var activeImg = activeGo.GetComponent<Image>();
            activeImg.sprite = LoadSprite(SPRITE_GOLD_BORDER);
            activeImg.color = new Color(1, 1, 1, 0);
            activeImg.type = Image.Type.Sliced;
            activeGo.SetActive(false);

            var nameGo = new GameObject("Name", typeof(RectTransform));
            nameGo.transform.SetParent(go.transform, false);
            var nRect = nameGo.GetComponent<RectTransform>();
            nRect.sizeDelta = new Vector2(78, 16);
            var nameLe = nameGo.AddComponent<LayoutElement>();
            nameLe.preferredWidth = 78;
            nameLe.preferredHeight = 16;
            var nameTmp = CreateText("Text", nameGo.transform,
                "???", 9, P.DFInkDim, _fontCinzelRegular ?? _fontKorean);
            nameTmp.characterSpacing = 1;

            BindField(item, "_button", btn);
            BindField(item, "_portraitImage", pImg);
            BindField(item, "_initialText", initialTmp);
            BindField(item, "_nameText", nameTmp);
            BindField(item, "_inPartyBadge", inPartyGo);
            BindField(item, "_lockOverlay", lockGo);
            BindField(item, "_activeRing", activeGo);

            return item;
        }

        // =========================================================
        // FOOTER (86px) — 파티 슬롯 4 + 버튼 3
        // =========================================================
        public static void BuildFooter(RectTransform parent)
        {
            var footerGo = new GameObject("FooterPanel", typeof(RectTransform), typeof(Image));
            footerGo.transform.SetParent(parent, false);
            StretchToParent(footerGo.GetComponent<RectTransform>());
            // LayoutElement는 CreateSectionSlot이 관리 (preferredHeight=86, flexibleHeight=0)
            var fImg = footerGo.GetComponent<Image>();
            fImg.color = P.DFAbyss;
            fImg.raycastTarget = false;

            // 상단 골드 라인 — 더 두껍게 (2 → 3) 및 색상 강화
            var goldLineGo = new GameObject("GoldLine", typeof(RectTransform), typeof(Image));
            goldLineGo.transform.SetParent(footerGo.transform, false);
            var glRect = goldLineGo.GetComponent<RectTransform>();
            glRect.anchorMin = new Vector2(0, 1);
            glRect.anchorMax = new Vector2(1, 1);
            glRect.pivot = new Vector2(0.5f, 1);
            glRect.sizeDelta = new Vector2(0, 3);
            glRect.anchoredPosition = Vector2.zero;
            var glImg = goldLineGo.GetComponent<Image>();
            glImg.sprite = LoadSprite(SPRITE_GOLD_BORDER_THIN);
            glImg.color = Color.white;
            glImg.type = Image.Type.Sliced;
            glImg.raycastTarget = false;

            // 메인 HLG — ★ controlWidth/Height=true로 자식 크기 보장
            var mainLayoutGo = new GameObject("MainLayout", typeof(RectTransform));
            mainLayoutGo.transform.SetParent(footerGo.transform, false);
            StretchToParent(mainLayoutGo.GetComponent<RectTransform>());
            var mlLe = mainLayoutGo.AddComponent<LayoutElement>();
            mlLe.flexibleWidth = 1;
            mlLe.flexibleHeight = 1;
            AddHLG(mainLayoutGo, TextAnchor.MiddleCenter, 24, true, true,
                padLeft: 36, padRight: 36, padTop: 14, padBottom: 14);

            // PartySlotPanel — FooterPanel에 부착
            var panel = footerGo.AddComponent<PartySlotPanel>();

            // ── 좌측 (PartyLabel + SlotsContainer) ──
            var leftGo = CreateLayoutChild("Left", mainLayoutGo.transform, flexW: 1, flexH: 1);
            leftGo.GetComponent<LayoutElement>().minWidth = 400;
            AddHLG(leftGo, TextAnchor.MiddleLeft, 14, true, true);

            var partyLabel = CreateText("PartyLabel", leftGo.transform,
                "PARTY", 12, P.DFGold, _fontCinzelBold ?? _fontKorean, TextAlignmentOptions.Left);
            partyLabel.fontStyle = FontStyles.Bold;
            partyLabel.characterSpacing = 6;

            var slotsGo = CreateLayoutChild("SlotsContainer", leftGo.transform, flexW: 1, flexH: 1);
            slotsGo.GetComponent<LayoutElement>().minWidth = 280;
            AddHLG(slotsGo, TextAnchor.MiddleLeft, 14, true, true);

            // ── 우측 (버튼 3종) ──
            var rightGo = CreateLayoutChild("Right", mainLayoutGo.transform, flexW: 0, flexH: 1);
            rightGo.GetComponent<LayoutElement>().minWidth = 420;
            AddHLG(rightGo, TextAnchor.MiddleRight, 14, true, true);

            var randomBtn = CreateButton("BtnRandom", rightGo.transform,
                LoadSprite(SPRITE_SLATE_LIGHT), Color.white,
                "RANDOM", 12, P.DFGoldL, _fontCinzelBold ?? _fontKorean, 120, 40);
            randomBtn.GetComponent<LayoutElement>().preferredWidth = 120;
            randomBtn.GetComponent<LayoutElement>().preferredHeight = 40;

            var clearBtn = CreateButton("BtnClear", rightGo.transform,
                LoadSprite(SPRITE_SLATE_LIGHT), Color.white,
                "X CLEAR", 12, P.DFGoldL, _fontCinzelBold ?? _fontKorean, 100, 40);
            clearBtn.GetComponent<LayoutElement>().preferredWidth = 100;
            clearBtn.GetComponent<LayoutElement>().preferredHeight = 40;

            var embarkBtn = CreateButton("BtnEmbark", rightGo.transform,
                LoadSprite(SPRITE_BLOOD_BTN_NORMAL), Color.white,
                "EMBARK >", 18, P.DFGoldL, _fontCinzelBlack ?? _fontKorean, 200, 48);
            embarkBtn.GetComponent<LayoutElement>().preferredWidth = 200;
            embarkBtn.GetComponent<LayoutElement>().preferredHeight = 48;
            var ebTmp = embarkBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (ebTmp != null)
            {
                ebTmp.characterSpacing = 6;
                ebTmp.fontStyle = FontStyles.Bold;
            }

            // SpriteSwap transition — 핏빛 3-state
            embarkBtn.transition = Selectable.Transition.SpriteSwap;
            embarkBtn.spriteState = new SpriteState
            {
                highlightedSprite = LoadSprite(SPRITE_BLOOD_BTN_HOVER),
                pressedSprite = LoadSprite(SPRITE_BLOOD_BTN_PRESSED),
                disabledSprite = LoadSprite(SPRITE_SLATE),
            };
            embarkBtn.interactable = false;

            // 슬롯 템플릿 생성 + PartySlotPanel 필드 바인딩
            var slotTemplate = CreateSlotTemplate(slotsGo.transform);
            BindField(panel, "_slotsContainer", slotsGo.transform);
            BindField(panel, "_slotPrefab", slotTemplate);
            BindField(panel, "_embarkButton", embarkBtn);
            BindField(panel, "_randomButton", randomBtn);
            BindField(panel, "_clearButton", clearBtn);
            slotTemplate.gameObject.SetActive(false);
        }

        private static PartySlotItem CreateSlotTemplate(Transform parent)
        {
            var go = new GameObject("SlotTemplate", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 64;
            le.preferredHeight = 64;
            le.minWidth = 64;
            le.minHeight = 64;

            var slot = go.AddComponent<PartySlotItem>();
            var btn = go.AddComponent<Button>();

            // BG — 골드 테두리 (둥근 느낌은 Sprite가 사각이라 Image.type=Simple로)
            var bgGo = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(go.transform, false);
            StretchToParent(bgGo.GetComponent<RectTransform>());
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = LoadSprite(SPRITE_GOLD_BORDER);
            bgImg.color = Color.white;
            bgImg.type = Image.Type.Sliced;
            bgImg.raycastTarget = true;

            // 어두운 내부 배경 (빈 슬롯 느낌)
            var innerBg = new GameObject("InnerBG", typeof(RectTransform), typeof(Image));
            innerBg.transform.SetParent(go.transform, false);
            var ibRect = innerBg.GetComponent<RectTransform>();
            ibRect.anchorMin = new Vector2(0.08f, 0.08f);
            ibRect.anchorMax = new Vector2(0.92f, 0.92f);
            ibRect.offsetMin = Vector2.zero;
            ibRect.offsetMax = Vector2.zero;
            var ibImg = innerBg.GetComponent<Image>();
            ibImg.color = P.DFVoid;
            ibImg.raycastTarget = false;

            // Content — 채워진 슬롯일 때 자원색 표시
            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(Image));
            contentGo.transform.SetParent(go.transform, false);
            var cRect = contentGo.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.12f, 0.12f);
            cRect.anchorMax = new Vector2(0.88f, 0.88f);
            cRect.offsetMin = Vector2.zero;
            cRect.offsetMax = Vector2.zero;
            var contentImg = contentGo.GetComponent<Image>();
            contentImg.color = Color.clear;
            contentImg.raycastTarget = false;

            // Initial (중앙 이니셜 또는 +)
            var initialTmp = CreateText("Initial", go.transform,
                "+", 24, P.DFInkFaint, _fontCinzelBlack ?? _fontKorean);
            initialTmp.fontStyle = FontStyles.Bold;

            // 슬롯 번호 배지 (상단 중앙, 작은 원)
            var numGo = new GameObject("SlotNum", typeof(RectTransform), typeof(Image));
            numGo.transform.SetParent(go.transform, false);
            var nRect = numGo.GetComponent<RectTransform>();
            nRect.anchorMin = new Vector2(0.5f, 1);
            nRect.anchorMax = new Vector2(0.5f, 1);
            nRect.pivot = new Vector2(0.5f, 0.5f);
            nRect.anchoredPosition = new Vector2(0, 4);
            nRect.sizeDelta = new Vector2(22, 22);
            var numImg = numGo.GetComponent<Image>();
            numImg.sprite = LoadSprite(SPRITE_GOLD_BORDER_THIN);
            numImg.color = Color.white;
            numImg.type = Image.Type.Sliced;
            numImg.raycastTarget = false;
            var numTmp = CreateText("Text", numGo.transform,
                "1", 12, P.DFGoldL, _fontCinzelBold ?? _fontKorean);
            numTmp.fontStyle = FontStyles.Bold;

            btn.targetGraphic = bgImg;

            BindField(slot, "_button", btn);
            BindField(slot, "_background", bgImg);
            BindField(slot, "_slotNumberText", numTmp);
            BindField(slot, "_contentImage", contentImg);
            BindField(slot, "_initialText", initialTmp);
            BindField(slot, "_emptySprite", LoadSprite(SPRITE_GOLD_BORDER));
            BindField(slot, "_filledSprite", LoadSprite(SPRITE_GOLD_BORDER));

            return slot;
        }
    }
}
#endif
