using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using TeamLog.UI.Battle;

namespace TeamLog.Editor
{
    /// <summary>
    /// Battle UI 씬 빌더 — 사이드바 및 패널 UI 생성
    /// 진입점+TopBar+BottomBar+유틸리티: BattleUISceneBuilder.UI.cs
    /// 오버레이: BattleUISceneBuilder.UI.Overlay.cs
    /// </summary>
    public partial class BattleUISceneBuilder
    {
        // ══════════════════════════════════════════════════════════
        //  Left Sidebar
        // ══════════════════════════════════════════════════════════

        private static void CreateLeftSidebar(RectTransform parent)
        {
            var sidebar = NewRect("LeftSidebar", parent);
            sidebar.anchorMin = Vector2.zero;
            sidebar.anchorMax = new Vector2(0.24f, 1);
            sidebar.offsetMin = new Vector2(5, 5);
            sidebar.offsetMax = new Vector2(-2, -5);
            var leftImg = sidebar.gameObject.AddComponent<Image>();
            var leftSprite = LoadSprite(SPRITE_LOG_SIDEBAR);
            if (leftSprite != null)
            {
                leftImg.sprite = leftSprite;
                Set9Slice(leftImg);
            }
            else
                leftImg.color = new Color(0.04f, 0.04f, 0.1f, 0.8f);

            var vlg = sidebar.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            string[] names = { "카인", "쉘레이아", "아트카나", "샤이비어" };
            string[] hps = { "88/88", "55/55", "45/45", "50/50" };
            string[] skills = { "방어막", "연속 베기", "원죽 방패", "치명 오라" };

            for (int i = 0; i < 4; i++)
                CreatePlayerPanel(sidebar, i + 1, names[i], hps[i], skills[i]);
        }

        private static void CreatePlayerPanel(RectTransform parent, int num, string name, string hp, string skill)
        {
            var panel = NewRect($"CharPanel_{name}", parent);
            panel.sizeDelta = new Vector2(0, 160);
            var panelImg = panel.gameObject.AddComponent<Image>();
            var playerSprite = LoadSprite(SPRITE_PLAYER_PANEL);
            if (playerSprite != null)
            {
                panelImg.sprite = playerSprite;
                Set9Slice(panelImg);
            }
            else
                panelImg.color = new Color(0.06f, 0.06f, 0.14f, 0.95f);
            var ol = panel.gameObject.AddComponent<Outline>();
            ol.effectColor = BorderRed;
            ol.effectDistance = new Vector2(1, -1);

            var panelBtn = panel.gameObject.AddComponent<Button>();
            panelBtn.targetGraphic = panelImg;

            // 그림자
            var shadow = panel.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.3f);
            shadow.effectDistance = new Vector2(3, -3);

            // 번호 뱃지
            var badge = NewRect("NumberBadge", panel);
            badge.anchorMin = new Vector2(0, 1);
            badge.anchorMax = new Vector2(0, 1);
            badge.pivot = new Vector2(0, 1);
            badge.anchoredPosition = new Vector2(5, -5);
            badge.sizeDelta = new Vector2(24, 24);
            badge.gameObject.AddComponent<Image>().color = AccentRed;
            var bt = AddText(NewRect("T", badge), num.ToString(), 14, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
            SetFillParent(bt.rectTransform);

            // X 버튼
            var xBtn = NewRect("CloseBtn", panel);
            xBtn.anchorMin = new Vector2(1, 1);
            xBtn.anchorMax = new Vector2(1, 1);
            xBtn.pivot = new Vector2(1, 1);
            xBtn.anchoredPosition = new Vector2(-5, -5);
            xBtn.sizeDelta = new Vector2(20, 20);
            xBtn.gameObject.AddComponent<Button>();
            xBtn.gameObject.AddComponent<Image>().color = new Color(0.5f, 0.1f, 0.1f, 0.8f);
            var xt = AddText(NewRect("T", xBtn), "X", 12, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
            SetFillParent(xt.rectTransform);

            // 이름
            var nRect = NewRect("Name", panel);
            nRect.anchorMin = new Vector2(0, 1);
            nRect.anchorMax = new Vector2(1, 1);
            nRect.pivot = new Vector2(0.5f, 1);
            nRect.anchoredPosition = new Vector2(0, -32);
            nRect.sizeDelta = new Vector2(-16, 22);
            AddTextNoWrap(nRect, name, 15, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

            // ATK / DEF 스탯
            var statsRect = NewRect("Stats", panel);
            statsRect.anchorMin = new Vector2(0, 1);
            statsRect.anchorMax = new Vector2(1, 1);
            statsRect.pivot = new Vector2(0.5f, 1);
            statsRect.anchoredPosition = new Vector2(0, -54);
            statsRect.sizeDelta = new Vector2(-16, 18);
            AddTextNoWrap(statsRect, "ATK 10  DEF 5", 12, FontStyles.Bold, TextAlignmentOptions.Center, TextDim);

            // HP 바
            var hpBar = NewRect("HPBar", panel);
            hpBar.anchorMin = new Vector2(0, 0);
            hpBar.anchorMax = new Vector2(1, 0);
            hpBar.pivot = new Vector2(0.5f, 0);
            hpBar.anchoredPosition = new Vector2(0, 60);
            hpBar.sizeDelta = new Vector2(-16, 20);
            hpBar.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

            var hpFill = NewRect("Fill", hpBar);
            hpFill.anchorMin = Vector2.zero;
            hpFill.anchorMax = new Vector2(1f, 1f);
            hpFill.offsetMin = new Vector2(2, 2);
            hpFill.offsetMax = new Vector2(-2, -2);
            hpFill.gameObject.AddComponent<Image>().color = AccentGreen;

            // 쉴드 바 (HP 바 위에 겹침)
            var shieldFill = NewRect("ShieldFill", hpBar);
            shieldFill.anchorMin = Vector2.zero;
            shieldFill.anchorMax = Vector2.zero;
            shieldFill.offsetMin = new Vector2(2, 2);
            shieldFill.offsetMax = new Vector2(-2, -2);
            shieldFill.gameObject.AddComponent<Image>().color = ShieldBrown;

            var hpTxt = NewRect("Text", hpBar);
            SetFillParent(hpTxt);
            AddTextNoWrap(hpTxt, hp, 12, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

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
            hpl.fontSize = 9;
            hpl.fontStyle = FontStyles.Bold;
            hpl.alignment = TextAlignmentOptions.Left;
            hpl.color = new Color(1, 1, 1, 0.6f);
            hpl.raycastTarget = false;
            hpl.enableWordWrapping = false;

            // 퍼센트
            var pct = NewRect("Pct", panel);
            pct.anchorMin = new Vector2(0, 0);
            pct.anchorMax = new Vector2(1, 0);
            pct.pivot = new Vector2(0.5f, 0);
            pct.anchoredPosition = new Vector2(0, 44);
            pct.sizeDelta = new Vector2(-16, 16);
            AddTextNoWrap(pct, "100%", 11, FontStyles.Normal, TextAlignmentOptions.Center, AccentGreen);

            // 상태이상 뱃지 컨테이너
            var statusCont = NewRect("StatusContainer", panel);
            statusCont.anchorMin = new Vector2(0, 0);
            statusCont.anchorMax = new Vector2(1, 0);
            statusCont.pivot = new Vector2(0.5f, 0);
            statusCont.anchoredPosition = new Vector2(0, 26);
            statusCont.sizeDelta = new Vector2(-16, 18);
            var hlg = statusCont.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 3;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 스킬명
            var sk = NewRect("Skill", panel);
            sk.anchorMin = new Vector2(0, 0);
            sk.anchorMax = new Vector2(1, 0);
            sk.pivot = new Vector2(0.5f, 0);
            sk.anchoredPosition = new Vector2(0, 8);
            sk.sizeDelta = new Vector2(-16, 18);
            AddTextNoWrap(sk, skill, 11, FontStyles.Normal, TextAlignmentOptions.Center, TextDim);
        }

        // ══════════════════════════════════════════════════════════
        //  Center Area
        // ══════════════════════════════════════════════════════════

        private static void CreateCenterArea(RectTransform parent)
        {
            var center = NewRect("CenterArea", parent);
            center.anchorMin = new Vector2(0.24f, 0);
            center.anchorMax = new Vector2(0.78f, 1);
            center.offsetMin = new Vector2(3, 5);
            center.offsetMax = new Vector2(-3, -5);

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
            }
            else
                panelImg.color = new Color(0.06f, 0.04f, 0.08f, 0.95f);
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
            vlg.spacing = 6;
            vlg.padding = new RectOffset(10, 10, 12, 12);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var le = panel.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 180;
            le.minWidth = 120;
            le.preferredHeight = 280;
            le.minHeight = 200;

            // 아바타
            var avatar = NewRect("Avatar", panel);
            avatar.sizeDelta = new Vector2(0, 100);
            avatar.gameObject.AddComponent<Image>().color = AccentRed;
            var aLabel = NewRect("Label", avatar);
            SetFillParent(aLabel);
            AddText(aLabel, "적 초상화", 13, FontStyles.Normal, TextAlignmentOptions.Center, new Color(1, 1, 1, 0.5f));

            // 이름
            var nRect = NewRect("Name", panel);
            nRect.sizeDelta = new Vector2(0, 24);
            AddText(nRect, name, 17, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

            // HP 바
            var hpCont = NewRect("HPBarContainer", panel);
            hpCont.sizeDelta = new Vector2(0, 24);
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
            AddText(hpText, hp, 14, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

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
            ehpl.fontSize = 10;
            ehpl.fontStyle = FontStyles.Bold;
            ehpl.alignment = TextAlignmentOptions.Left;
            ehpl.color = new Color(1, 1, 1, 0.6f);
            ehpl.raycastTarget = false;
            ehpl.enableWordWrapping = false;

            // ATK / DEF 스탯
            var statsRect = NewRect("Stats", panel);
            statsRect.sizeDelta = new Vector2(0, 18);
            AddTextNoWrap(statsRect, "ATK 10  DEF 5", 12, FontStyles.Bold, TextAlignmentOptions.Center, TextDim);

            // 버튼 영역
            var btnArea = NewRect("ButtonArea", panel);
            btnArea.sizeDelta = new Vector2(0, 40);
            var bhlg = btnArea.gameObject.AddComponent<HorizontalLayoutGroup>();
            bhlg.spacing = 8;
            bhlg.childAlignment = TextAnchor.MiddleCenter;
            bhlg.childControlWidth = false;
            bhlg.childControlHeight = false;

            CreateActionBtn(btnArea, "", AccentRed);
            CreateActionBtn(btnArea, "", new Color(0.4f, 0.15f, 0.55f));

            // 수량 정보
            var info = NewRect("Info", panel);
            info.sizeDelta = new Vector2(0, 22);
            AddText(info, "수량: 상시발동", 12, FontStyles.Normal, TextAlignmentOptions.Center, TextDim);

            // 상태이상 뱃지 컨테이너
            var statusCont = NewRect("StatusContainer", panel);
            statusCont.sizeDelta = new Vector2(0, 20);
            var shlg = statusCont.gameObject.AddComponent<HorizontalLayoutGroup>();
            shlg.spacing = 3;
            shlg.childAlignment = TextAnchor.MiddleCenter;
            shlg.childControlWidth = false;
            shlg.childControlHeight = false;
            shlg.childForceExpandWidth = false;
            shlg.childForceExpandHeight = false;
        }

        private static void CreateActionBtn(RectTransform parent, string label, Color bg)
        {
            var btn = NewRect($"Btn_{label}", parent);
            btn.sizeDelta = new Vector2(110, 36);
            var b = btn.gameObject.AddComponent<Button>();
            var img = btn.gameObject.AddComponent<Image>();
            img.color = bg;
            b.targetGraphic = img;

            var t = NewRect("T", btn);
            SetFillParent(t);
            AddText(t, label, 14, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
        }

        // ══════════════════════════════════════════════════════════
        //  Right Sidebar
        // ══════════════════════════════════════════════════════════

        private static void CreateRightSidebar(RectTransform parent)
        {
            var sidebar = NewRect("RightSidebar", parent);
            sidebar.anchorMin = new Vector2(0.78f, 0);
            sidebar.anchorMax = new Vector2(1, 1);
            sidebar.offsetMin = new Vector2(2, 5);
            sidebar.offsetMax = new Vector2(-5, -5);
            var sidebarImg = sidebar.gameObject.AddComponent<Image>();
            var logSprite = LoadSprite(SPRITE_LOG_SIDEBAR);
            if (logSprite != null)
            {
                sidebarImg.sprite = logSprite;
                Set9Slice(sidebarImg);
            }
            else
                sidebarImg.color = new Color(0.04f, 0.04f, 0.1f, 0.8f);
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
