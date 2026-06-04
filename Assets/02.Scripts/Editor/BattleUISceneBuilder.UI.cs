using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using TeamLog.UI.Battle;

namespace TeamLog.Editor
{
    /// <summary>
    /// Battle UI 씬 빌더 — UI 요소 생성 메서드
    /// </summary>
    public partial class BattleUISceneBuilder
    {
        // ── Popup 전용 색상 ──
        private static readonly Color PopupBg = new Color(0.02f, 0.02f, 0.06f, 0.95f);
        private static readonly Color PopupPanelBg = new Color(0.05f, 0.05f, 0.12f, 0.98f);
        private static readonly Color PopupHeaderBg = new Color(0.04f, 0.03f, 0.08f, 0.95f);
        private static readonly Color EntryBg = new Color(0.07f, 0.07f, 0.15f, 0.9f);
        // ══════════════════════════════════════════════════════════
        //  Top Bar
        // ══════════════════════════════════════════════════════════

        private static void CreateTopBar(RectTransform parent)
        {
            var bar = NewRect("TopBar", parent);
            bar.anchorMin = new Vector2(0, 1);
            bar.anchorMax = new Vector2(1, 1);
            bar.pivot = new Vector2(0.5f, 1);
            bar.sizeDelta = new Vector2(0, 60);
            var topBarImg = bar.gameObject.AddComponent<Image>();
            var topBarSprite = LoadSprite(SPRITE_TOPBAR);
            if (topBarSprite != null)
            {
                topBarImg.sprite = topBarSprite;
                Set9Slice(topBarImg);
            }
            else
                topBarImg.color = new Color(0.03f, 0.03f, 0.08f, 0.95f);

            // 하단 구분선
            var div = NewRect("Divider", bar);
            div.anchorMin = new Vector2(0, 0);
            div.anchorMax = new Vector2(1, 0);
            div.pivot = new Vector2(0.5f, 0);
            div.sizeDelta = new Vector2(0, 2);
            div.gameObject.AddComponent<Image>().color = AccentRed;

            // 턴 카운터
            var counter = NewRect("TurnCounter", bar);
            counter.anchorMin = new Vector2(0, 0.5f);
            counter.anchorMax = new Vector2(0, 0.5f);
            counter.pivot = new Vector2(0, 0.5f);
            counter.anchoredPosition = new Vector2(20, 0);
            counter.sizeDelta = new Vector2(80, 40);
            var ct = counter.gameObject.AddComponent<TextMeshProUGUI>();
            ct.font = GetOrCreateKoreanFont();
            ct.text = "4/4";
            ct.fontSize = 28;
            ct.fontStyle = FontStyles.Bold;
            ct.alignment = TextAlignmentOptions.Left;
            ct.color = AccentYellow;

            // AP 표시
            var apRect = NewRect("APText", bar);
            apRect.anchorMin = new Vector2(0.5f, 0.5f);
            apRect.anchorMax = new Vector2(0.5f, 0.5f);
            apRect.pivot = new Vector2(0.5f, 0.5f);
            apRect.sizeDelta = new Vector2(120, 40);
            var apT = apRect.gameObject.AddComponent<TextMeshProUGUI>();
            apT.font = GetOrCreateKoreanFont();
            apT.text = "AP 4/4";
            apT.fontSize = 28;
            apT.fontStyle = FontStyles.Bold;
            apT.alignment = TextAlignmentOptions.Center;
            apT.color = AccentYellow;

            // 리롤 카운트 표시
            var rerollRect = NewRect("RerollText", bar);
            rerollRect.anchorMin = new Vector2(1, 0.5f);
            rerollRect.anchorMax = new Vector2(1, 0.5f);
            rerollRect.pivot = new Vector2(1, 0.5f);
            rerollRect.anchoredPosition = new Vector2(-310, 0);
            rerollRect.sizeDelta = new Vector2(120, 40);
            var rerollT = rerollRect.gameObject.AddComponent<TextMeshProUGUI>();
            rerollT.font = GetOrCreateKoreanFont();
            rerollT.text = "리롤 2/2";
            rerollT.fontSize = 20;
            rerollT.fontStyle = FontStyles.Bold;
            rerollT.alignment = TextAlignmentOptions.Center;
            rerollT.color = ShieldBrown;

            // 파티 상태 위젯 (HP 총합, 골드, 층)
            var partyHP = NewRect("PartyHP", bar);
            partyHP.anchorMin = new Vector2(1, 0.5f);
            partyHP.anchorMax = new Vector2(1, 0.5f);
            partyHP.pivot = new Vector2(1, 0.5f);
            partyHP.anchoredPosition = new Vector2(-180, 0);
            partyHP.sizeDelta = new Vector2(80, 30);
            var partyHPT = partyHP.gameObject.AddComponent<TextMeshProUGUI>();
            partyHPT.font = GetOrCreateKoreanFont();
            partyHPT.text = "HP 200/200";
            partyHPT.fontSize = 16;
            partyHPT.fontStyle = FontStyles.Bold;
            partyHPT.alignment = TextAlignmentOptions.Center;
            partyHPT.color = AccentGreen;

            var partyGold = NewRect("PartyGold", bar);
            partyGold.anchorMin = new Vector2(1, 0.5f);
            partyGold.anchorMax = new Vector2(1, 0.5f);
            partyGold.pivot = new Vector2(1, 0.5f);
            partyGold.anchoredPosition = new Vector2(-100, 0);
            partyGold.sizeDelta = new Vector2(70, 30);
            var partyGoldT = partyGold.gameObject.AddComponent<TextMeshProUGUI>();
            partyGoldT.font = GetOrCreateKoreanFont();
            partyGoldT.text = "100G";
            partyGoldT.fontSize = 16;
            partyGoldT.fontStyle = FontStyles.Bold;
            partyGoldT.alignment = TextAlignmentOptions.Center;
            partyGoldT.color = AccentYellow;

            // PartyStatusWidget 컴포넌트를 TopBar에 추가
            bar.gameObject.AddComponent<PartyStatusWidget>();

            // 턴 종료 버튼
            var btn = NewRect("EndTurnButton", bar);
            btn.anchorMin = new Vector2(1, 0.5f);
            btn.anchorMax = new Vector2(1, 0.5f);
            btn.pivot = new Vector2(1, 0.5f);
            btn.anchoredPosition = new Vector2(-20, 0);
            btn.sizeDelta = new Vector2(160, 40);
            var b = btn.gameObject.AddComponent<Button>();
            var bImg = btn.gameObject.AddComponent<Image>();
            var endTurnSprite = LoadSprite(SPRITE_ENDTURN_BTN);
            if (endTurnSprite != null)
            {
                bImg.sprite = endTurnSprite;
                Set9Slice(bImg);
            }
            else
                bImg.color = AccentRed;
            b.targetGraphic = bImg;
            var c = b.colors;
            c.highlightedColor = new Color(0.9f, 0.2f, 0.3f);
            c.pressedColor = new Color(0.5f, 0.08f, 0.15f);
            b.colors = c;

            var txt = NewRect("Text", btn);
            SetFillParent(txt);
            var t = txt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = GetOrCreateKoreanFont();
            t.text = "턴 종료 [T]";
            t.fontSize = 18;
            t.fontStyle = FontStyles.Bold;
            t.alignment = TextAlignmentOptions.Center;
            t.color = TextWhite;
        }

        // ══════════════════════════════════════════════════════════
        //  Bottom Bar
        // ══════════════════════════════════════════════════════════

        private static void CreateBottomBar(RectTransform parent)
        {
            var bar = NewRect("BottomBar", parent);
            bar.anchorMin = Vector2.zero;
            bar.anchorMax = new Vector2(1, 0);
            bar.pivot = new Vector2(0.5f, 0);
            bar.sizeDelta = new Vector2(0, 100);
            var bottomBarImg = bar.gameObject.AddComponent<Image>();
            var bottomSprite = LoadSprite(SPRITE_BOTTOM);
            if (bottomSprite != null)
            {
                bottomBarImg.sprite = bottomSprite;
                Set9Slice(bottomBarImg);
            }
            else
                bottomBarImg.color = new Color(0.03f, 0.03f, 0.08f, 0.95f);

            var div = NewRect("Divider", bar);
            div.anchorMin = new Vector2(0, 1);
            div.anchorMax = new Vector2(1, 1);
            div.pivot = new Vector2(0.5f, 1);
            div.sizeDelta = new Vector2(0, 2);
            div.gameObject.AddComponent<Image>().color = AccentRed;

            var turn = NewRect("CurrentTurnText", bar);
            turn.anchorMin = new Vector2(0, 0);
            turn.anchorMax = new Vector2(0.15f, 1);
            turn.offsetMin = Vector2.zero;
            turn.offsetMax = Vector2.zero;
            var tt = turn.gameObject.AddComponent<TextMeshProUGUI>();
            tt.font = GetOrCreateKoreanFont();
            tt.text = "쉘레이아, 턴";
            tt.fontSize = 14;
            tt.fontStyle = FontStyles.Bold;
            tt.alignment = TextAlignmentOptions.Left;
            tt.margin = new Vector4(8, 0, 4, 0);
            tt.color = AccentYellow;
        }

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

        // ══════════════════════════════════════════════════════════
        //  Character Popup
        // ══════════════════════════════════════════════════════════

        private static void CreateCharacterPopup(RectTransform parent)
        {
            // ── 오버레이 (전체 화면 반투명 배경, 클릭으로 닫기) ──
            var overlay = NewRect("CharacterPopup", parent);
            SetFillParent(overlay);
            overlay.gameObject.SetActive(false);

            var bgBtn = overlay.gameObject.AddComponent<Button>();
            var bgImg = overlay.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.6f);
            bgBtn.targetGraphic = bgImg;

            overlay.gameObject.AddComponent<CharacterPopupUI>();

            // ── 패널 (고정 크기 520×620, 중앙, VerticalLayoutGroup으로 자동 배치) ──
            var panel = NewRect("Panel", overlay);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(520, 620);
            var panelImg = panel.gameObject.AddComponent<Image>();
            panelImg.color = PopupPanelBg;
            var panelOl = panel.gameObject.AddComponent<Outline>();
            panelOl.effectColor = BorderRed;
            panelOl.effectDistance = new Vector2(2, -2);

            // VerticalLayoutGroup: 자식들을 위에서부터 순서대로 자동 배치 (수동 위치 계산 불필요)
            var panelVlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            panelVlg.padding = new RectOffset(12, 12, 8, 8);
            panelVlg.spacing = 4;
            panelVlg.childAlignment = TextAnchor.UpperCenter;
            panelVlg.childControlWidth = true;
            panelVlg.childControlHeight = true;
            panelVlg.childForceExpandWidth = true;
            panelVlg.childForceExpandHeight = false;

            // ContentSizeFitter은 패널 자체에는 사용하지 않음 (고정 크기 520×620)
            // 각 섹션은 LayoutElement로 높이 지정

            CreatePopupHeader(panel);
            CreatePopupHPBar(panel);
            CreatePopupStats(panel);
            CreatePopupTabs(panel);
            CreatePopupContent(panel);
        }

        /// <summary>
        /// LayoutElement로 높이를 지정하는 헬퍼 (VerticalLayoutGroup용)
        /// </summary>
        private static LayoutElement SetFixedHeight(RectTransform rect, float height)
        {
            var le = rect.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0;
            return le;
        }

        private static void CreatePopupHeader(RectTransform panel)
        {
            var header = NewRect("Header", panel);
            // 수동 위치 지정 제거 — VerticalLayoutGroup이 자동 배치
            SetFixedHeight(header, 56);
            header.gameObject.AddComponent<Image>().color = PopupHeaderBg;

            var portrait = NewRect("Portrait", header);
            portrait.anchorMin = new Vector2(0, 0.5f);
            portrait.anchorMax = new Vector2(0, 0.5f);
            portrait.pivot = new Vector2(0, 0.5f);
            portrait.anchoredPosition = new Vector2(12, 0);
            portrait.sizeDelta = new Vector2(44, 44);
            portrait.gameObject.AddComponent<Image>().color = AccentRed;

            var nameRect = NewRect("Name", header);
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.offsetMin = new Vector2(64, 0);
            nameRect.offsetMax = new Vector2(-44, -4);
            AddText(nameRect, "캐릭터명", 20, FontStyles.Bold, TextAlignmentOptions.Left, TextWhite);

            var classRect = NewRect("Class", header);
            classRect.anchorMin = new Vector2(0, 0);
            classRect.anchorMax = new Vector2(1, 0.5f);
            classRect.offsetMin = new Vector2(64, 4);
            classRect.offsetMax = new Vector2(-44, 0);
            AddText(classRect, "클래스", 13, FontStyles.Normal, TextAlignmentOptions.Left, TextDim);

            var closeBtn = NewRect("CloseBtn", header);
            closeBtn.anchorMin = new Vector2(1, 0.5f);
            closeBtn.anchorMax = new Vector2(1, 0.5f);
            closeBtn.pivot = new Vector2(1, 0.5f);
            closeBtn.anchoredPosition = new Vector2(-8, 0);
            closeBtn.sizeDelta = new Vector2(32, 32);
            var cb = closeBtn.gameObject.AddComponent<Button>();
            var cbImg = closeBtn.gameObject.AddComponent<Image>();
            cbImg.color = new Color(0.5f, 0.1f, 0.1f, 0.8f);
            cb.targetGraphic = cbImg;
            var cbLabel = NewRect("X", closeBtn);
            SetFillParent(cbLabel);
            AddText(cbLabel, "X", 16, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
        }

        private static void CreatePopupHPBar(RectTransform panel)
        {
            var hpArea = NewRect("HPArea", panel);
            // 수동 위치 지정 제거 — VerticalLayoutGroup이 자동 배치
            SetFixedHeight(hpArea, 28);

            var hpBg = NewRect("HPBarBg", hpArea);
            SetFillParent(hpBg);
            hpBg.offsetMax = new Vector2(-80, 0);
            hpBg.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

            var hpFill = NewRect("Fill", hpBg);
            hpFill.anchorMin = Vector2.zero;
            hpFill.anchorMax = new Vector2(1f, 1f);
            hpFill.offsetMin = new Vector2(2, 2);
            hpFill.offsetMax = new Vector2(-2, -2);
            hpFill.gameObject.AddComponent<Image>().color = AccentGreen;

            var hpLabel = NewRect("HPText", hpArea);
            hpLabel.anchorMin = new Vector2(1, 0);
            hpLabel.anchorMax = new Vector2(1, 1);
            hpLabel.pivot = new Vector2(1, 0.5f);
            hpLabel.offsetMin = new Vector2(-76, 0);
            hpLabel.offsetMax = Vector2.zero;
            AddText(hpLabel, "HP 55/55", 14, FontStyles.Bold, TextAlignmentOptions.Right, TextWhite);
        }

        private static void CreatePopupStats(RectTransform panel)
        {
            var statsArea = NewRect("StatsArea", panel);
            // 수동 위치 지정 제거 — VerticalLayoutGroup이 자동 배치
            SetFixedHeight(statsArea, 24);

            var atkRect = NewRect("ATK", statsArea);
            atkRect.anchorMin = Vector2.zero;
            atkRect.anchorMax = new Vector2(0.5f, 1);
            atkRect.offsetMin = Vector2.zero;
            atkRect.offsetMax = new Vector2(-4, 0);
            atkRect.gameObject.AddComponent<Image>().color = EntryBg;
            AddText(NewRect("T", atkRect), "ATK 10", 14, FontStyles.Bold, TextAlignmentOptions.Center, AccentRed);

            var defRect = NewRect("DEF", statsArea);
            defRect.anchorMin = new Vector2(0.5f, 0);
            defRect.anchorMax = Vector2.one;
            defRect.offsetMin = new Vector2(4, 0);
            defRect.offsetMax = Vector2.zero;
            defRect.gameObject.AddComponent<Image>().color = EntryBg;
            AddText(NewRect("T", defRect), "DEF 5", 14, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.3f, 0.6f, 0.9f));
        }

        private static void CreatePopupTabs(RectTransform panel)
        {
            var tabArea = NewRect("TabArea", panel);
            // 수동 위치 지정 제거 — VerticalLayoutGroup이 자동 배치
            SetFixedHeight(tabArea, 36);
            tabArea.gameObject.AddComponent<Image>().color = new Color(0.03f, 0.03f, 0.06f, 0.5f);

            // 탭1: 스킬 목록
            var tab1 = NewRect("TabSkill", tabArea);
            tab1.anchorMin = Vector2.zero;
            tab1.anchorMax = new Vector2(0.5f, 1);
            tab1.offsetMin = Vector2.zero;
            tab1.offsetMax = Vector2.zero;
            tab1.gameObject.AddComponent<Button>();
            tab1.gameObject.AddComponent<Image>().color = Color.clear;
            var t1Label = NewRect("T", tab1);
            SetFillParent(t1Label);
            AddText(t1Label, "스킬 목록", 14, FontStyles.Bold, TextAlignmentOptions.Center, AccentYellow);

            var t1Ind = NewRect("Indicator", tab1);
            t1Ind.anchorMin = new Vector2(0, 0);
            t1Ind.anchorMax = new Vector2(1, 0);
            t1Ind.pivot = new Vector2(0.5f, 0);
            t1Ind.sizeDelta = new Vector2(0, 2);
            t1Ind.gameObject.AddComponent<Image>().color = AccentYellow;

            // 탭2: 상태 효과
            var tab2 = NewRect("TabStatus", tabArea);
            tab2.anchorMin = new Vector2(0.5f, 0);
            tab2.anchorMax = Vector2.one;
            tab2.offsetMin = Vector2.zero;
            tab2.offsetMax = Vector2.zero;
            tab2.gameObject.AddComponent<Button>();
            tab2.gameObject.AddComponent<Image>().color = Color.clear;
            var t2Label = NewRect("T", tab2);
            SetFillParent(t2Label);
            AddText(t2Label, "상태 효과", 14, FontStyles.Normal, TextAlignmentOptions.Center, TextDim);

            var t2Ind = NewRect("Indicator", tab2);
            t2Ind.anchorMin = new Vector2(0, 0);
            t2Ind.anchorMax = new Vector2(1, 0);
            t2Ind.pivot = new Vector2(0.5f, 0);
            t2Ind.sizeDelta = new Vector2(0, 2);
            t2Ind.gameObject.AddComponent<Image>().color = AccentYellow;
            t2Ind.gameObject.SetActive(false);
        }

        private static void CreatePopupContent(RectTransform panel)
        {
            // ── 스킬 콘텐츠 (flexibleHeight=1로 남은 공간 자동 채움) ──
            var skillContent = NewRect("SkillContent", panel);
            // 수동 offset 제거 — VerticalLayoutGroup + LayoutElement flexibleHeight로 자동 배치
            var skillLe = skillContent.gameObject.AddComponent<LayoutElement>();
            skillLe.minHeight = 100;
            skillLe.flexibleHeight = 1;

            skillContent.gameObject.AddComponent<RectMask2D>();
            var skillScroll = skillContent.gameObject.AddComponent<ScrollRect>();
            skillScroll.horizontal = false;
            skillScroll.vertical = true;
            skillScroll.scrollSensitivity = 20;
            skillScroll.movementType = ScrollRect.MovementType.Elastic;

            // 내부 Content: top-anchored, sizeDelta=(0,0)
            var skillList = NewRect("Content", skillContent);
            skillList.anchorMin = new Vector2(0, 1);
            skillList.anchorMax = new Vector2(1, 1);
            skillList.pivot = new Vector2(0.5f, 1);
            skillList.sizeDelta = new Vector2(0, 0);

            var skillVlg = skillList.gameObject.AddComponent<VerticalLayoutGroup>();
            skillVlg.spacing = 6;
            skillVlg.padding = new RectOffset(0, 0, 4, 4);
            skillVlg.childAlignment = TextAnchor.UpperCenter;
            skillVlg.childControlWidth = true;
            skillVlg.childControlHeight = false;
            skillVlg.childForceExpandWidth = true;
            skillVlg.childForceExpandHeight = false;

            var skillCsf = skillList.gameObject.AddComponent<ContentSizeFitter>();
            skillCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            skillCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            skillScroll.content = skillList;

            // ── 상태 효과 콘텐츠 (숨김, 동일 구조) ──
            var statusContent = NewRect("StatusContent", panel);
            var statusLe = statusContent.gameObject.AddComponent<LayoutElement>();
            statusLe.minHeight = 100;
            statusLe.flexibleHeight = 1;
            statusContent.gameObject.SetActive(false);

            statusContent.gameObject.AddComponent<RectMask2D>();
            var statusScroll = statusContent.gameObject.AddComponent<ScrollRect>();
            statusScroll.horizontal = false;
            statusScroll.vertical = true;
            statusScroll.scrollSensitivity = 20;
            statusScroll.movementType = ScrollRect.MovementType.Elastic;

            var statusList = NewRect("Content", statusContent);
            statusList.anchorMin = new Vector2(0, 1);
            statusList.anchorMax = new Vector2(1, 1);
            statusList.pivot = new Vector2(0.5f, 1);
            statusList.sizeDelta = new Vector2(0, 0);

            var statusVlg = statusList.gameObject.AddComponent<VerticalLayoutGroup>();
            statusVlg.spacing = 6;
            statusVlg.padding = new RectOffset(0, 0, 4, 4);
            statusVlg.childAlignment = TextAnchor.UpperCenter;
            statusVlg.childControlWidth = true;
            statusVlg.childControlHeight = false;
            statusVlg.childForceExpandWidth = true;
            statusVlg.childForceExpandHeight = false;

            var statusCsf = statusList.gameObject.AddComponent<ContentSizeFitter>();
            statusCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            statusCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            statusScroll.content = statusList;
        }

        // ══════════════════════════════════════════════════════════
        //  Battle End Overlay
        // ══════════════════════════════════════════════════════════

        private static void CreateBattleEndOverlay(RectTransform parent)
        {
            var overlay = NewRect("BattleEndOverlay", parent);
            SetFillParent(overlay);
            var bgImg = overlay.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.85f);
            bgImg.raycastTarget = true;
            overlay.gameObject.SetActive(false);

            // 중앙 컨테이너 — 크고 눈에 띄게
            var container = NewRect("Container", overlay);
            container.anchorMin = new Vector2(0.25f, 0.25f);
            container.anchorMax = new Vector2(0.75f, 0.75f);
            container.offsetMin = Vector2.zero;
            container.offsetMax = Vector2.zero;
            var containerBg = container.gameObject.AddComponent<Image>();
            var borderSprite = LoadSprite(SPRITE_CARD_BORDER);
            if (borderSprite != null)
            {
                containerBg.sprite = borderSprite;
                Set9Slice(containerBg);
            }
            else
                containerBg.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);

            // CanvasGroup 추가 (애니메이션용)
            container.gameObject.AddComponent<CanvasGroup>();

            // 결과 텍스트 — 큼직하게 상단
            var resultRect = NewRect("ResultText", container);
            resultRect.anchorMin = new Vector2(0f, 0.45f);
            resultRect.anchorMax = new Vector2(1f, 0.85f);
            resultRect.offsetMin = Vector2.zero;
            resultRect.offsetMax = Vector2.zero;
            var resultT = resultRect.gameObject.AddComponent<TextMeshProUGUI>();
            resultT.font = GetOrCreateKoreanFont();
            resultT.text = "승리!";
            resultT.fontSize = 64;
            resultT.fontStyle = FontStyles.Bold;
            resultT.alignment = TextAlignmentOptions.Center;
            resultT.color = AccentYellow;

            // 계속하기 버튼 — 하단에 큼직하게
            var btnRect = NewRect("ContinueButton", container);
            btnRect.anchorMin = new Vector2(0.2f, 0.08f);
            btnRect.anchorMax = new Vector2(0.8f, 0.35f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            // Image를 Button보다 먼저 추가해야 targetGraphic이 자동 연결됨
            var btnImg = btnRect.gameObject.AddComponent<Image>();
            btnImg.color = AccentRed;
            var btn = btnRect.gameObject.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var btnLabelRect = NewRect("Label", btnRect);
            SetFillParent(btnLabelRect);
            var btnLabel = btnLabelRect.gameObject.AddComponent<TextMeshProUGUI>();
            btnLabel.font = GetOrCreateKoreanFont();
            btnLabel.text = "계속하기";
            btnLabel.fontSize = 32;
            btnLabel.fontStyle = FontStyles.Bold;
            btnLabel.alignment = TextAlignmentOptions.Center;
            btnLabel.color = TextWhite;

            // BattleEndOverlay 컴포넌트
            var endOverlay = overlay.gameObject.AddComponent<BattleEndOverlay>();
            var ser = new UnityEditor.SerializedObject(endOverlay);
            var resultProp = ser.FindProperty("_resultText");
            if (resultProp != null) resultProp.objectReferenceValue = resultT;
            var btnProp = ser.FindProperty("_continueButton");
            if (btnProp != null) btnProp.objectReferenceValue = btn;
            var labelProp = ser.FindProperty("_continueLabel");
            if (labelProp != null) labelProp.objectReferenceValue = btnLabel;
            ser.ApplyModifiedProperties();
        }

        // ══════════════════════════════════════════════════════════
        //  Tooltip UI
        // ══════════════════════════════════════════════════════════

        private static void CreateTooltipUI(RectTransform parent)
        {
            var tooltip = NewRect("TooltipUI", parent);
            tooltip.sizeDelta = new Vector2(280, 0);
            tooltip.pivot = new Vector2(0, 1);

            var tooltipBg = tooltip.gameObject.AddComponent<Image>();
            tooltipBg.color = new Color(0.02f, 0.02f, 0.06f, 0.95f);
            tooltipBg.raycastTarget = false;

            var outline = tooltip.gameObject.AddComponent<Outline>();
            outline.effectColor = AccentYellow;
            outline.effectDistance = new Vector2(1, -1);

            var vlg = tooltip.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 8, 8);
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = tooltip.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var title = NewRect("Title", tooltip);
            var tt = title.gameObject.AddComponent<TextMeshProUGUI>();
            tt.font = GetOrCreateKoreanFont();
            tt.text = "";
            tt.fontSize = 15;
            tt.fontStyle = FontStyles.Bold;
            tt.alignment = TextAlignmentOptions.Left;
            tt.color = AccentYellow;
            tt.raycastTarget = false;

            var desc = NewRect("Desc", tooltip);
            var dt = desc.gameObject.AddComponent<TextMeshProUGUI>();
            dt.font = GetOrCreateKoreanFont();
            dt.text = "";
            dt.fontSize = 13;
            dt.alignment = TextAlignmentOptions.Left;
            dt.color = TextWhite;
            dt.enableWordWrapping = true;
            dt.raycastTarget = false;

            tooltip.gameObject.AddComponent<TooltipUI>();
            tooltip.gameObject.SetActive(false);
        }

        // ══════════════════════════════════════════════════════════
        //  UI 유틸리티
        // ══════════════════════════════════════════════════════════

        private static void CreateBar(RectTransform parent, string name, string text, float ratio, Color fillCol, float yOffset, Vector2 size)
        {
            var bar = NewRect(name, parent);
            bar.anchorMin = new Vector2(0.5f, 0.5f);
            bar.anchorMax = new Vector2(0.5f, 0.5f);
            bar.pivot = new Vector2(0.5f, 0.5f);
            bar.anchoredPosition = new Vector2(0, yOffset);
            bar.sizeDelta = size;
            bar.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

            var fill = NewRect("Fill", bar);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(ratio, 1f);
            fill.offsetMin = new Vector2(2, 2);
            fill.offsetMax = new Vector2(-2, -2);
            fill.gameObject.AddComponent<Image>().color = fillCol;

            var tRect = NewRect("Text", bar);
            SetFillParent(tRect);
            AddText(tRect, text, 13, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static RectTransform NewRect(string name, RectTransform parent)
        {
            return NewRect(name, parent.transform);
        }

        private static void SetFillParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI AddText(RectTransform parent, string text, float size, FontStyles style, TextAlignmentOptions align, Color color)
        {
            var tmp = parent.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = color;
            tmp.font = GetOrCreateKoreanFont();
            return tmp;
        }

        private static TextMeshProUGUI AddTextNoWrap(RectTransform parent, string text, float size, FontStyles style, TextAlignmentOptions align, Color color)
        {
            var tmp = AddText(parent, text, size, style, align, color);
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        private static Sprite LoadSprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                Debug.LogWarning($"[BattleUISceneBuilder] Sprite not found: {path}");
            return sprite;
        }

        private static void Set9Slice(Image img)
        {
            if (img?.sprite == null) return;
            // 9-slice를 위해 Image.type을 Sliced로 설정
            img.type = Image.Type.Sliced;
        }
    }
}
