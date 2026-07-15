using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using TeamLog.UI.Battle;

namespace TeamLog.Editor
{
    /// <summary>
    /// Battle UI 씬 빌더 — 사이드바, 패널 UI, PlayerCard 생성
    /// 진입점+TopBar+BottomBar+PlayerStrip+유틸리티: BattleUISceneBuilder.UI.cs
    /// 오버레이: BattleUISceneBuilder.UI.Overlay.cs
    /// </summary>
    public partial class BattleUISceneBuilder
    {
        // ══════════════════════════════════════════════════════════
        //  Player Card (가로 카드 — PlayerStrip 내부)
        // ══════════════════════════════════════════════════════════

        private static void CreatePlayerCard(RectTransform parent, string name, string hp)
        {
            var card = NewRect($"CharCard_{name}", parent);
            card.sizeDelta = new Vector2(240, 64);

            // 5컬럼 통일: minWidth=120, flexibleWidth=1만 지정 (preferredWidth 제거)
            var layoutEl = card.gameObject.AddComponent<LayoutElement>();
            layoutEl.minWidth = 120;
            layoutEl.flexibleWidth = 1;
            layoutEl.preferredHeight = 64;
            layoutEl.minHeight = 64;

            var cardImg = card.gameObject.AddComponent<Image>();
            var playerSprite = LoadSprite(SPRITE_PLAYER_PANEL);
            if (playerSprite != null)
            {
                cardImg.sprite = playerSprite;
                Set9Slice(cardImg);
                cardImg.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);
            }
            else
                cardImg.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);

            var ol = card.gameObject.AddComponent<Outline>();
            ol.effectColor = BorderRed;
            ol.effectDistance = new Vector2(1, -1);

            var cardBtn = card.gameObject.AddComponent<Button>();
            cardBtn.targetGraphic = cardImg;

            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.3f);
            shadow.effectDistance = new Vector2(2, -2);

            // ── 아바타 (48x48, 좌측) ──
            var avatar = NewRect("Avatar", card);
            avatar.anchorMin = new Vector2(0, 0.5f);
            avatar.anchorMax = new Vector2(0, 0.5f);
            avatar.pivot = new Vector2(0, 0.5f);
            avatar.anchoredPosition = new Vector2(4, 0);
            avatar.sizeDelta = new Vector2(48, 48);
            avatar.gameObject.AddComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f);

            var aLabel = NewRect("Label", avatar);
            SetFillParent(aLabel);
            AddText(aLabel, "?", 18, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1, 1, 1, 0.7f));

            // ── RightSection (아바타 우측) ──
            var right = NewRect("RightSection", card);
            right.anchorMin = new Vector2(0, 0);
            right.anchorMax = new Vector2(1, 1);
            right.offsetMin = new Vector2(56, 4);
            right.offsetMax = new Vector2(-6, -4);

            // NameRow: 이름(좌) + Stats(우)
            var nameRow = NewRect("NameRow", right);
            nameRow.anchorMin = new Vector2(0, 1);
            nameRow.anchorMax = new Vector2(1, 1);
            nameRow.pivot = new Vector2(0, 1);
            nameRow.sizeDelta = new Vector2(0, 20);

            var nameT = NewRect("Name", nameRow);
            nameT.anchorMin = Vector2.zero;
            nameT.anchorMax = new Vector2(0.5f, 1);
            nameT.offsetMax = new Vector2(-2, 0);
            AddTextNoWrap(nameT, name, 14, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);

            var statsT = NewRect("Stats", nameRow);
            statsT.anchorMin = new Vector2(0.5f, 0);
            statsT.anchorMax = Vector2.one;
            statsT.offsetMin = new Vector2(2, 0);
            AddTextNoWrap(statsT, "ATK 12 DEF 5", 12, FontStyles.Bold, TextAlignmentOptions.Right, TextDim);

            // HP 바 (20px)
            var hpBar = NewRect("HPBar", right);
            hpBar.anchorMin = new Vector2(0, 1);
            hpBar.anchorMax = new Vector2(1, 1);
            hpBar.pivot = new Vector2(0, 1);
            hpBar.anchoredPosition = new Vector2(0, -22);
            hpBar.sizeDelta = new Vector2(0, 20);
            hpBar.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

            var hpFill = NewRect("Fill", hpBar);
            hpFill.anchorMin = Vector2.zero;
            hpFill.anchorMax = new Vector2(1f, 1f);
            hpFill.offsetMin = new Vector2(2, 2);
            hpFill.offsetMax = new Vector2(-2, -2);
            hpFill.gameObject.AddComponent<Image>().color = AccentGreen;

            // 쉴드 바
            var shieldFill = NewRect("ShieldFill", hpBar);
            shieldFill.anchorMin = Vector2.zero;
            shieldFill.anchorMax = Vector2.zero;
            shieldFill.offsetMin = new Vector2(2, 2);
            shieldFill.offsetMax = new Vector2(-2, -2);
            shieldFill.gameObject.AddComponent<Image>().color = ShieldBrown;

            // HP 텍스트
            var hpTxt = NewRect("HPText", hpBar);
            SetFillParent(hpTxt);
            AddTextNoWrap(hpTxt, hp, 11, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

            // HP 라벨 (색맹 지원)
            var hpLabel = NewRect("HPLabel", hpBar);
            hpLabel.anchorMin = new Vector2(0, 0);
            hpLabel.anchorMax = new Vector2(0, 1);
            hpLabel.pivot = new Vector2(0, 0.5f);
            hpLabel.anchoredPosition = new Vector2(4, 0);
            hpLabel.sizeDelta = new Vector2(22, 0);
            var hpl = hpLabel.gameObject.AddComponent<TextMeshProUGUI>();
            hpl.font = GetOrCreateKoreanFont();
            hpl.text = "HP";
            hpl.fontSize = 8;
            hpl.fontStyle = FontStyles.Bold;
            hpl.alignment = TextAlignmentOptions.Left;
            hpl.color = new Color(1, 1, 1, 0.6f);
            hpl.raycastTarget = false;
            hpl.enableWordWrapping = false;

            // 상태이상 컨테이너 (HP 바 아래, 16px)
            var statusCont = NewRect("StatusContainer", right);
            statusCont.anchorMin = new Vector2(0, 0);
            statusCont.anchorMax = new Vector2(1, 0);
            statusCont.pivot = new Vector2(0, 0);
            statusCont.anchoredPosition = new Vector2(0, 0);
            statusCont.sizeDelta = new Vector2(0, 16);
            var sHlg = statusCont.gameObject.AddComponent<HorizontalLayoutGroup>();
            sHlg.spacing = 3;
            sHlg.childAlignment = TextAnchor.LowerLeft;
            sHlg.childControlWidth = false;
            sHlg.childControlHeight = false;
            sHlg.childForceExpandWidth = false;
            sHlg.childForceExpandHeight = false;

            // 선택 하이라이트
            var selHL = NewRect("SelectionHighlight", card);
            SetFillParent(selHL);
            var selImg = selHL.gameObject.AddComponent<Image>();
            selImg.color = new Color(0.96f, 0.82f, 0.25f, 0.10f);
            selImg.raycastTarget = false;
            var selOl = selHL.gameObject.AddComponent<Outline>();
            selOl.effectColor = AccentYellow;
            selOl.effectDistance = new Vector2(2, -2);
            selHL.gameObject.SetActive(false);

            // 타겟팅 하이라이트 (적 타겟 — 빨간색 테두리)
            var targetedHL = NewRect("TargetedHighlight", card);
            SetFillParent(targetedHL);
            var targetedImg = targetedHL.gameObject.AddComponent<Image>();
            targetedImg.color = new Color(0.85f, 0.2f, 0.2f, 0.10f);
            targetedImg.raycastTarget = false;
            var targetedOl = targetedHL.gameObject.AddComponent<Outline>();
            targetedOl.effectColor = new Color(0.85f, 0.2f, 0.2f, 0.9f);
            targetedOl.effectDistance = new Vector2(2, -2);
            targetedHL.gameObject.SetActive(false);
        }

        // ══════════════════════════════════════════════════════════
        //  Center Area
        // ══════════════════════════════════════════════════════════

        private static void CreateCenterArea(RectTransform parent)
        {
            var center = NewRect("CenterArea", parent);
            center.anchorMin = new Vector2(0, 0);
            center.anchorMax = new Vector2(1, 1);
            center.offsetMin = new Vector2(5, 5);
            center.offsetMax = new Vector2(-5, -5);

            var hlg = center.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.padding = new RectOffset(12, 12, 12, 12);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            CreateEnemyPanel(center, "고블린", "30/30");
            CreateEnemyPanel(center, "고블린", "30/30");
        }

        private static void CreateEnemyPanel(RectTransform parent, string name, string hp)
        {
            var panel = NewRect($"Enemy_{name}", parent);
            var panelImg = panel.gameObject.AddComponent<Image>();
            var enemySprite = LoadSprite(SPRITE_ENEMY_PANEL);
            if (enemySprite != null)
            {
                panelImg.sprite = enemySprite;
                Set9Slice(panelImg);
                panelImg.color = PanelBgNavy;
            }
            else
                panelImg.color = PanelBgNavy;
            var ol = panel.gameObject.AddComponent<Outline>();
            ol.effectColor = BorderRed;
            ol.effectDistance = new Vector2(2, -2);

            var panelBtn = panel.gameObject.AddComponent<Button>();
            panelBtn.targetGraphic = panelImg;

            // 그림자
            var shadow = panel.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.3f);
            shadow.effectDistance = new Vector2(3, -3);

            var vlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var le = panel.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 180;
            le.minWidth = 150;
            le.preferredHeight = 280;
            le.minHeight = 220;
            le.flexibleWidth = 0;

            // ── IntentSlot (패널 "위" 외부 — 명시적 앵커, HLG 없음) ──
            // 200px = 적 패널 preferredWidth와 일치
            var intentSlot = NewRect("IntentSlot", panel);
            intentSlot.anchorMin = new Vector2(0.5f, 1f);
            intentSlot.anchorMax = new Vector2(0.5f, 1f);
            intentSlot.pivot = new Vector2(0.5f, 0f);
            intentSlot.anchoredPosition = new Vector2(0, 2);
            intentSlot.sizeDelta = new Vector2(200, 28);
            intentSlot.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            intentSlot.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.05f, 0.05f, 0.9f);
            // ★ HLG 없음 — AnchorLeft로 각 요소 명시 배치

            // IntentIcon: x=4, 20x20
            var intentIcon = NewRect("IntentIcon", intentSlot);
            AnchorLeft(intentIcon, 4, 20, 20);
            var intentIconImg = intentIcon.gameObject.AddComponent<Image>();
            intentIconImg.color = AccentRed;
            intentIconImg.raycastTarget = false;

            // IntentValue: x=26, 22x20
            var intentValue = NewRect("IntentValue", intentSlot);
            AnchorLeft(intentValue, 26, 22, 20);
            var ivTmp = intentValue.gameObject.AddComponent<TextMeshProUGUI>();
            ivTmp.font = GetOrCreateKoreanFont();
            ivTmp.text = "";
            ivTmp.fontSize = 15;
            ivTmp.fontStyle = FontStyles.Bold;
            ivTmp.alignment = TextAlignmentOptions.Left;
            ivTmp.color = TextWhite;
            ivTmp.raycastTarget = false;
            ivTmp.enableWordWrapping = false;

            // IntentText: x=50, 나머지 (200-50-4=146)
            var intentText = NewRect("IntentText", intentSlot);
            AnchorLeft(intentText, 50, 146, 20);
            var intentTmp = intentText.gameObject.AddComponent<TextMeshProUGUI>();
            intentTmp.font = GetOrCreateKoreanFont();
            intentTmp.text = "";
            intentTmp.fontSize = 10;
            intentTmp.fontStyle = FontStyles.Normal;
            intentTmp.alignment = TextAlignmentOptions.Left;
            intentTmp.color = new Color(1, 1, 1, 0.8f);
            intentTmp.raycastTarget = false;
            intentTmp.enableWordWrapping = false;
            intentTmp.overflowMode = TextOverflowModes.Ellipsis;

            // ── TargetBox (panel 내부 상단 — IntentSlot 바로 아래, panel 너비와 일치) ──
            var targetBox = NewRect("TargetBox", panel);
            targetBox.anchorMin = new Vector2(0.5f, 1f);
            targetBox.anchorMax = new Vector2(0.5f, 1f);
            targetBox.pivot = new Vector2(0.5f, 1f);
            targetBox.anchoredPosition = new Vector2(0, -2);
            targetBox.sizeDelta = new Vector2(180, 22); // panel.preferredWidth=180과 일치
            targetBox.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var targetBg = targetBox.gameObject.AddComponent<Image>();
            targetBg.color = new Color(0.2f, 0.08f, 0.08f, 0.85f);
            var targetOl = targetBox.gameObject.AddComponent<Outline>();
            targetOl.effectColor = new Color(0.85f, 0.2f, 0.2f, 0.6f);
            targetOl.effectDistance = new Vector2(1, -1);

            var targetHlg = targetBox.gameObject.AddComponent<HorizontalLayoutGroup>();
            targetHlg.spacing = 4;
            targetHlg.padding = new RectOffset(6, 6, 2, 2);
            targetHlg.childAlignment = TextAnchor.MiddleLeft;
            targetHlg.childControlWidth = false;
            targetHlg.childControlHeight = false;
            targetHlg.childForceExpandWidth = false;
            targetHlg.childForceExpandHeight = false;

            // 타겟 화살표
            var targetArrow = NewRect("Arrow", targetBox);
            var arrowLe = targetArrow.gameObject.AddComponent<LayoutElement>();
            arrowLe.preferredWidth = 14;
            arrowLe.preferredHeight = 14;
            AddText(targetArrow, "→", 12, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.9f, 0.3f, 0.3f));

            // 타겟 초상화 (작은 박스)
            var targetPortrait = NewRect("Portrait", targetBox);
            var portLe = targetPortrait.gameObject.AddComponent<LayoutElement>();
            portLe.preferredWidth = 18;
            portLe.preferredHeight = 18;
            targetPortrait.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.2f, 0.35f, 0.9f);

            // 타겟 이름
            var targetName = NewRect("Name", targetBox);
            var nameLe = targetName.gameObject.AddComponent<LayoutElement>();
            nameLe.preferredWidth = 100;
            nameLe.flexibleWidth = 1;
            var targetNameTmp = targetName.gameObject.AddComponent<TextMeshProUGUI>();
            targetNameTmp.font = GetOrCreateKoreanFont();
            targetNameTmp.text = "";
            targetNameTmp.fontSize = 10;
            targetNameTmp.fontStyle = FontStyles.Bold;
            targetNameTmp.alignment = TextAlignmentOptions.Left;
            targetNameTmp.color = new Color(1f, 0.55f, 0.5f);
            targetNameTmp.raycastTarget = false;
            targetNameTmp.enableWordWrapping = false;

            targetBox.gameObject.SetActive(false);

            // ── 아바타 (큰 영역 — 초상화 placeholder) ──
            var avatar = NewRect("Avatar", panel);
            avatar.sizeDelta = new Vector2(0, 120);
            var avatarImg = avatar.gameObject.AddComponent<Image>();
            avatarImg.color = new Color(0.05f, 0.08f, 0.15f, 0.95f);
            var avatarOl = avatar.gameObject.AddComponent<Outline>();
            avatarOl.effectColor = new Color(0.2f, 0.3f, 0.4f, 0.5f);
            avatarOl.effectDistance = new Vector2(2, -2);
            var aLabel = NewRect("Label", avatar);
            SetFillParent(aLabel);
            AddText(aLabel, "적", 24, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.4f, 0.5f, 0.6f, 0.7f));
            // "초상화" 작은 라벨 (우측 하단)
            var portraitLabel = NewRect("PortraitLabel", avatar);
            portraitLabel.anchorMin = new Vector2(1, 0);
            portraitLabel.anchorMax = new Vector2(1, 0);
            portraitLabel.pivot = new Vector2(1, 0);
            portraitLabel.anchoredPosition = new Vector2(-4, 2);
            portraitLabel.sizeDelta = new Vector2(40, 12);
            AddText(portraitLabel, "초상화", 8, FontStyles.Normal, TextAlignmentOptions.Right, new Color(0.3f, 0.4f, 0.5f, 0.6f));

            // ── 이름 ──
            var nRect = NewRect("Name", panel);
            nRect.sizeDelta = new Vector2(0, 22);
            AddText(nRect, name, 16, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

            // ── HP 바 ──
            var hpCont = NewRect("HPBarContainer", panel);
            hpCont.sizeDelta = new Vector2(0, 22);
            hpCont.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.1f, 0.1f);

            var fill = NewRect("Fill", hpCont);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(1f, 1f);
            fill.offsetMin = new Vector2(2, 2);
            fill.offsetMax = new Vector2(-2, -2);
            fill.gameObject.AddComponent<Image>().color = AccentRed;

            // 쉴드 바 (HP 바 위에 겹침)
            var shieldFill = NewRect("ShieldFill", hpCont);
            shieldFill.anchorMin = Vector2.zero;
            shieldFill.anchorMax = Vector2.zero;
            shieldFill.offsetMin = new Vector2(2, 2);
            shieldFill.offsetMax = new Vector2(-2, -2);
            shieldFill.gameObject.AddComponent<Image>().color = ShieldBrown;

            var hpText = NewRect("HPText", hpCont);
            SetFillParent(hpText);
            AddText(hpText, hp, 13, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

            // HP 라벨 (색맹 지원)
            var ehpLabel = NewRect("HPLabel", hpCont);
            ehpLabel.anchorMin = new Vector2(0, 0);
            ehpLabel.anchorMax = new Vector2(0, 1);
            ehpLabel.pivot = new Vector2(0, 0.5f);
            ehpLabel.anchoredPosition = new Vector2(4, 0);
            ehpLabel.sizeDelta = new Vector2(22, 0);
            var ehpl = ehpLabel.gameObject.AddComponent<TextMeshProUGUI>();
            ehpl.font = GetOrCreateKoreanFont();
            ehpl.text = "HP";
            ehpl.fontSize = 9;
            ehpl.fontStyle = FontStyles.Bold;
            ehpl.alignment = TextAlignmentOptions.Left;
            ehpl.color = new Color(1, 1, 1, 0.6f);
            ehpl.raycastTarget = false;
            ehpl.enableWordWrapping = false;

            // ── Info (특성/스탯 — 한 줄, 런타임에 교체됨) ──
            var infoRect = NewRect("Info", panel);
            infoRect.sizeDelta = new Vector2(0, 18);
            AddTextNoWrap(infoRect, "", 11, FontStyles.Normal, TextAlignmentOptions.Center, TextDim);

            // ── Stats (숨김 — 툴팁/팝업에서만 참조) ──
            var statsRect = NewRect("Stats", panel);
            statsRect.sizeDelta = new Vector2(0, 0);
            AddTextNoWrap(statsRect, "ATK 10  DEF 5", 12, FontStyles.Bold, TextAlignmentOptions.Center, TextDim);
            statsRect.gameObject.SetActive(false);

            // ── 버튼 영역 (런타임에 특성 뱃지로 대체, 축소) ──
            var btnArea = NewRect("ButtonArea", panel);
            btnArea.sizeDelta = new Vector2(0, 24);
            var bhlg = btnArea.gameObject.AddComponent<HorizontalLayoutGroup>();
            bhlg.spacing = 4;
            bhlg.childAlignment = TextAnchor.MiddleCenter;
            bhlg.childControlWidth = false;
            bhlg.childControlHeight = false;

            // ── 상태이상 뱃지 컨테이너 ──
            var statusCont = NewRect("StatusContainer", panel);
            statusCont.sizeDelta = new Vector2(0, 18);
            var shlg = statusCont.gameObject.AddComponent<HorizontalLayoutGroup>();
            shlg.spacing = 3;
            shlg.childAlignment = TextAnchor.MiddleCenter;
            shlg.childControlWidth = false;
            shlg.childControlHeight = false;
            shlg.childForceExpandWidth = false;
            shlg.childForceExpandHeight = false;

            // ── 타겟 인디케이터 ──
            var targetInd = NewRect("TargetIndicator", panel);
            SetFillParent(targetInd);
            var tiImg = targetInd.gameObject.AddComponent<Image>();
            tiImg.color = new Color(0.85f, 0.2f, 0.2f, 0.10f);
            tiImg.raycastTarget = false;
            var tiOl = targetInd.gameObject.AddComponent<Outline>();
            tiOl.effectColor = AccentRed;
            tiOl.effectDistance = new Vector2(3, -3);
            targetInd.gameObject.SetActive(false);
        }

        // ══════════════════════════════════════════════════════════
        //  Right Sidebar
        // ══════════════════════════════════════════════════════════

        private static void CreateRightSidebar(RectTransform parent)
        {
            var sidebar = NewRect("RightSidebar", parent);
            sidebar.anchorMin = new Vector2(0.80f, 0);
            sidebar.anchorMax = new Vector2(1, 1);
            sidebar.offsetMin = new Vector2(2, 5);
            sidebar.offsetMax = new Vector2(-5, -5);
            var sidebarImg = sidebar.gameObject.AddComponent<Image>();
            var rightSprite = LoadSprite(SPRITE_SOLID_FRAME);
            if (rightSprite != null)
            {
                sidebarImg.sprite = rightSprite;
                Set9Slice(sidebarImg);
            }
            sidebarImg.color = new Color(0.06f, 0.06f, 0.09f, 0.95f);
            var ol = sidebar.gameObject.AddComponent<Outline>();
            ol.effectColor = BorderRed;
            ol.effectDistance = new Vector2(1, -1);

            // 타이틀
            var title = NewRect("Title", sidebar);
            title.anchorMin = new Vector2(0, 1);
            title.anchorMax = new Vector2(1, 1);
            title.pivot = new Vector2(0.5f, 1);
            title.sizeDelta = new Vector2(0, 36);
            title.gameObject.AddComponent<Image>().color = new Color(0.05f, 0.03f, 0.08f, 0.9f);
            var tRect = NewRect("T", title);
            SetFillParent(tRect);
            AddText(tRect, "전투 로그", 16, FontStyles.Bold, TextAlignmentOptions.Center, AccentYellow);

            // 구분선
            var div = NewRect("Divider", sidebar);
            div.anchorMin = new Vector2(0, 1);
            div.anchorMax = new Vector2(1, 1);
            div.pivot = new Vector2(0.5f, 1);
            div.anchoredPosition = new Vector2(0, -36);
            div.sizeDelta = new Vector2(0, 2);
            div.gameObject.AddComponent<Image>().color = AccentRed;

            // 로그 텍스트 (ScrollRect 포함)
            // Viewport
            var viewport = NewRect("Viewport", sidebar);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(2, 2);
            viewport.offsetMax = new Vector2(-2, -40);
            viewport.gameObject.AddComponent<RectMask2D>();

            // Content (로그 텍스트)
            var log = NewRect("LogText", viewport);
            log.anchorMin = new Vector2(0, 1);
            log.anchorMax = new Vector2(1, 1);
            log.pivot = new Vector2(0.5f, 1);
            log.sizeDelta = new Vector2(0, 0);
            var lt = log.gameObject.AddComponent<TextMeshProUGUI>();
            lt.font = GetOrCreateKoreanFont();
            lt.text = "전투가 시작되었습니다.\n\n카인이 방어막을 사용했습니다.\n\n쉘레이아의 턴입니다.";
            lt.fontSize = 14;
            lt.alignment = TextAlignmentOptions.TopLeft;
            lt.color = TextDim;
            lt.enableWordWrapping = true;
            lt.overflowMode = TextOverflowModes.Overflow;
            lt.lineSpacing = 50f;
            if (_koreanFont != null)
                lt.font = _koreanFont;

            var csf = log.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ScrollRect
            var scrollRect = sidebar.gameObject.AddComponent<ScrollRect>();
            scrollRect.content = log;
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 20;
        }
    }
}
