using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using TeamLog.UI.Battle;

namespace TeamLog.Editor
{
    /// <summary>
    /// Battle UI 씬 빌더 — TopBar, BottomBar, 공통 UI 유틸리티
    /// 사이드바: BattleUISceneBuilder.UI.Sidebar.cs
    /// 오버레이: BattleUISceneBuilder.UI.Overlay.cs
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
            bar.sizeDelta = new Vector2(0, 44);
            var topBarImg = bar.gameObject.AddComponent<Image>();
            var solidSprite = LoadSprite(SPRITE_SOLID_FRAME);
            if (solidSprite != null)
            {
                topBarImg.sprite = solidSprite;
                Set9Slice(topBarImg);
            }
            topBarImg.color = TopBarBgNavy;

            // 하단 구분선
            var div = NewRect("Divider", bar);
            div.anchorMin = new Vector2(0, 0);
            div.anchorMax = new Vector2(1, 0);
            div.pivot = new Vector2(0.5f, 0);
            div.sizeDelta = new Vector2(0, 2);
            div.gameObject.AddComponent<Image>().color = DividerNavy;

            // 좌측: Turn 배지 + 층 정보 + RelicBar
            CreateTopBarLeftSection(bar);
            // 우측: 토글 버튼(파티/로그) + 속도 버튼
            CreateTopBarRightSection(bar);

            // TopBarUI 컴포넌트 (AP/속도 제어는 BottomBar에 있는 요소를 참조)
            bar.gameObject.AddComponent<TopBarUI>();
        }

        // ══════════════════════════════════════════════════════════
        //  TopBar 좌측 섹션 — Turn 배지 + 층 정보 + RelicBar
        // ══════════════════════════════════════════════════════════

        private static void CreateTopBarLeftSection(RectTransform bar)
        {
            // Turn 배지 (빨간 색)
            var turnBadge = NewRect("TurnBadge", bar);
            turnBadge.anchorMin = new Vector2(0, 0.5f);
            turnBadge.anchorMax = new Vector2(0, 0.5f);
            turnBadge.pivot = new Vector2(0, 0.5f);
            turnBadge.anchoredPosition = new Vector2(12, 0);
            turnBadge.sizeDelta = new Vector2(90, 28);
            var tbImg = turnBadge.gameObject.AddComponent<Image>();
            tbImg.color = AccentRed;
            var tbLabel = NewRect("T", turnBadge);
            SetFillParent(tbLabel);
            AddText(tbLabel, "Turn 1", 14, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

            // 층 정보 텍스트
            var floorInfo = NewRect("FloorInfo", bar);
            floorInfo.anchorMin = new Vector2(0, 0.5f);
            floorInfo.anchorMax = new Vector2(0, 0.5f);
            floorInfo.pivot = new Vector2(0, 0.5f);
            floorInfo.anchoredPosition = new Vector2(110, 0);
            floorInfo.sizeDelta = new Vector2(180, 28);
            AddText(floorInfo, "F1 일반 · 층 1/4", 12, FontStyles.Normal, TextAlignmentOptions.Left, TextDim);

            // RelicBar — 좌측 영역 (층 정보 우측)
            var relicBar = NewRect("RelicBar", bar);
            relicBar.anchorMin = new Vector2(0, 0.5f);
            relicBar.anchorMax = new Vector2(0, 0.5f);
            relicBar.pivot = new Vector2(0, 0.5f);
            relicBar.anchoredPosition = new Vector2(300, 0);
            relicBar.sizeDelta = new Vector2(360, 32);

            var hlg = relicBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.padding = new RectOffset(2, 2, 2, 2);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 빈 상태 플레이스홀더 — Refresh()에서 유물이 있으면 자동 제거됨
            var placeholder = NewRect("Placeholder", relicBar);
            placeholder.sizeDelta = new Vector2(28, 28);
            var phImg = placeholder.gameObject.AddComponent<Image>();
            phImg.color = new Color(0.15f, 0.15f, 0.22f, 0.5f);
            var phLabel = NewRect("T", placeholder);
            SetFillParent(phLabel);
            var phT = phLabel.gameObject.AddComponent<TextMeshProUGUI>();
            phT.font = GetOrCreateKoreanFont();
            phT.text = "유물";
            phT.fontSize = 9;
            phT.alignment = TextAlignmentOptions.Center;
            phT.color = new Color(0.5f, 0.5f, 0.6f, 0.6f);
            phT.raycastTarget = false;

            relicBar.gameObject.AddComponent<BattleRelicBarUI>();
            var ser = new UnityEditor.SerializedObject(relicBar.GetComponent<BattleRelicBarUI>());
            var containerProp = ser.FindProperty("_iconContainer");
            if (containerProp != null) containerProp.objectReferenceValue = relicBar;
            ser.ApplyModifiedProperties();
        }

        // ══════════════════════════════════════════════════════════
        //  TopBar 우측 섹션 — 파티 토글 + 로그 토글 + 속도 버튼
        // ══════════════════════════════════════════════════════════

        private static void CreateTopBarRightSection(RectTransform bar)
        {
            // 우측 컨테이너 (속도 버튼을 가장 우측에 두고, 토글 버튼들을 그 왼쪽에)
            // 속도 토글 버튼 (가장 우측)
            var speedBtn = NewRect("SpeedButton", bar);
            speedBtn.anchorMin = new Vector2(1, 0.5f);
            speedBtn.anchorMax = new Vector2(1, 0.5f);
            speedBtn.pivot = new Vector2(1, 0.5f);
            speedBtn.anchoredPosition = new Vector2(-12, 0);
            speedBtn.sizeDelta = new Vector2(50, 28);
            var speedB = speedBtn.gameObject.AddComponent<Button>();
            var speedImg = speedBtn.gameObject.AddComponent<Image>();
            speedImg.color = new Color(0.1f, 0.1f, 0.1f, 0.7f); // ★ D 시안: 미니멀 어둡게
            speedB.targetGraphic = speedImg;
            var speedLabel = NewRect("SpeedLabel", speedBtn);
            SetFillParent(speedLabel);
            var speedT = speedLabel.gameObject.AddComponent<TextMeshProUGUI>();
            speedT.font = GetOrCreateKoreanFont();
            speedT.text = "1x";
            speedT.fontSize = 14;
            speedT.fontStyle = FontStyles.Bold;
            speedT.alignment = TextAlignmentOptions.Center;
            speedT.color = TextWhite;
            speedT.raycastTarget = false;

            // 로그 토글 버튼 (속도 버튼 좌측)
            var logToggle = NewRect("LogToggleButton", bar);
            logToggle.anchorMin = new Vector2(1, 0.5f);
            logToggle.anchorMax = new Vector2(1, 0.5f);
            logToggle.pivot = new Vector2(1, 0.5f);
            logToggle.anchoredPosition = new Vector2(-70, 0);
            logToggle.sizeDelta = new Vector2(80, 28);
            var logBtn = logToggle.gameObject.AddComponent<Button>();
            var logImg = logToggle.gameObject.AddComponent<Image>();
            logImg.color = new Color(0.1f, 0.1f, 0.1f, 0.7f); // ★ D 시안
            logBtn.targetGraphic = logImg;
            var logLabel = NewRect("T", logToggle);
            SetFillParent(logLabel);
            AddText(logLabel, "로그", 12, FontStyles.Bold, TextAlignmentOptions.Center, TextDim);
            logToggle.gameObject.AddComponent<UIToggleButton>();
            // _buttonImage 자동 연결을 위해 SerializedObject 사용
            var logSer = new UnityEditor.SerializedObject(logToggle.GetComponent<UIToggleButton>());
            var logBtnImgProp = logSer.FindProperty("_buttonImage");
            if (logBtnImgProp != null) logBtnImgProp.objectReferenceValue = logImg;
            logSer.ApplyModifiedProperties();

            // 파티 토글 버튼 (로그 토글 좌측)
            var partyToggle = NewRect("PartyToggleButton", bar);
            partyToggle.anchorMin = new Vector2(1, 0.5f);
            partyToggle.anchorMax = new Vector2(1, 0.5f);
            partyToggle.pivot = new Vector2(1, 0.5f);
            partyToggle.anchoredPosition = new Vector2(-158, 0);
            partyToggle.sizeDelta = new Vector2(80, 28);
            var partyBtn = partyToggle.gameObject.AddComponent<Button>();
            var partyImg = partyToggle.gameObject.AddComponent<Image>();
            partyImg.color = new Color(0.1f, 0.1f, 0.1f, 0.7f); // ★ D 시안
            partyBtn.targetGraphic = partyImg;
            var partyLabel = NewRect("T", partyToggle);
            SetFillParent(partyLabel);
            AddText(partyLabel, "파티", 12, FontStyles.Bold, TextAlignmentOptions.Center, TextDim);
            partyToggle.gameObject.AddComponent<UIToggleButton>();
            var partySer = new UnityEditor.SerializedObject(partyToggle.GetComponent<UIToggleButton>());
            var partyBtnImgProp = partySer.FindProperty("_buttonImage");
            if (partyBtnImgProp != null) partyBtnImgProp.objectReferenceValue = partyImg;
            partySer.ApplyModifiedProperties();
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
            bar.sizeDelta = new Vector2(0, 280);
            var bottomBarImg = bar.gameObject.AddComponent<Image>();
            var bottomSolidSprite = LoadSprite(SPRITE_SOLID_FRAME);
            if (bottomSolidSprite != null)
            {
                bottomBarImg.sprite = bottomSolidSprite;
                Set9Slice(bottomBarImg);
            }
            bottomBarImg.color = BottomBarBgNavy;

            // 상단 구분선
            var div = NewRect("Divider", bar);
            div.anchorMin = new Vector2(0, 1);
            div.anchorMax = new Vector2(1, 1);
            div.pivot = new Vector2(0.5f, 1);
            div.sizeDelta = new Vector2(0, 2);
            div.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            div.gameObject.AddComponent<Image>().color = DividerNavy;

            // ★ LeftContent + RightColumn 분리 구조 (컬럼 정렬 구조적 보장)
            var barHlg = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            barHlg.spacing = 8;
            barHlg.padding = new RectOffset(8, 8, 6, 6);
            barHlg.childAlignment = TextAnchor.UpperCenter;
            barHlg.childControlWidth = true;
            barHlg.childControlHeight = true;
            barHlg.childForceExpandWidth = false;
            barHlg.childForceExpandHeight = true;

            // ── LeftContent (캐릭터 행 + 스킬 행 — 같은 너비 공유) ──
            var left = NewRect("LeftContent", bar);
            var leftLe = left.gameObject.AddComponent<LayoutElement>();
            leftLe.flexibleWidth = 1;
            var leftVlg = left.gameObject.AddComponent<VerticalLayoutGroup>();
            leftVlg.spacing = 6;
            leftVlg.padding = new RectOffset(0, 0, 0, 0);
            leftVlg.childAlignment = TextAnchor.UpperCenter;
            leftVlg.childControlWidth = true;
            leftVlg.childControlHeight = true;
            leftVlg.childForceExpandWidth = true;
            leftVlg.childForceExpandHeight = true;

            // 행1: PlayerStrip (캐릭터 카드 4개)
            var charRow = NewRect("PlayerStrip", left);
            var charHlg = charRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            charHlg.spacing = 8;
            charHlg.padding = new RectOffset(0, 0, 0, 0);
            charHlg.childAlignment = TextAnchor.MiddleCenter;
            charHlg.childControlWidth = true;
            charHlg.childControlHeight = true;
            charHlg.childForceExpandWidth = true;
            charHlg.childForceExpandHeight = true;

            string[] names = { "카인", "쉘레이아", "아트카나", "샤이비어" };
            string[] hps = { "88/88", "55/55", "45/45", "50/50" };
            for (int i = 0; i < 4; i++)
                CreatePlayerCard(charRow, names[i], hps[i]);

            // 행2: SkillRow (ActionSlotContainer만 — ButtonArea는 RightColumn으로)
            var skillRow = NewRect("SkillRow", left);
            var slotHlg = skillRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            slotHlg.spacing = 8;
            slotHlg.padding = new RectOffset(0, 0, 0, 0);
            slotHlg.childAlignment = TextAnchor.MiddleCenter;
            slotHlg.childControlWidth = true;
            slotHlg.childControlHeight = true;
            slotHlg.childForceExpandWidth = true;
            slotHlg.childForceExpandHeight = true;

            // ActionSlotContainer (이름 유지: Setup.cs 호환)
            var slotContainer = NewRect("ActionSlotContainer", skillRow);
            var slotContainerLe = slotContainer.gameObject.AddComponent<LayoutElement>();
            slotContainerLe.flexibleWidth = 1;
            var slotContainerHlg = slotContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            slotContainerHlg.spacing = 8;
            slotContainerHlg.padding = new RectOffset(0, 0, 0, 0);
            slotContainerHlg.childAlignment = TextAnchor.MiddleCenter;
            slotContainerHlg.childControlWidth = true;
            slotContainerHlg.childControlHeight = true;
            slotContainerHlg.childForceExpandWidth = true;
            slotContainerHlg.childForceExpandHeight = true;

            // ── RightColumn (AP + 버튼 — VLG 없이 명시적 앵커) ──
            var right = NewRect("RightColumn", bar);
            var rightLe = right.gameObject.AddComponent<LayoutElement>();
            rightLe.preferredWidth = 160;
            rightLe.minWidth = 140;
            rightLe.flexibleWidth = 0;
            // ★ VLG 없음 — APArea/ButtonArea를 명시적 앵커로 배치

            // APArea (상단 고정)
            CreateAPArea(right);
            // ButtonArea (나머지 공간 전부)
            CreateButtonArea(right);
        }

        // ══════════════════════════════════════════════════════════
        //  AP 영역 (CharRow 우측 — 행1 열5)
        // ══════════════════════════════════════════════════════════

        // ★ D 시안: AP 강조색을 골드로 변경 (남색 → 미니멀 골드)
        private static readonly Color APCyan = new Color(0.9f, 0.78f, 0.31f); // #e6c878 gold-light

        private static void CreateAPArea(RectTransform rightColumn)
        {
            var apArea = NewRect("APArea", rightColumn);
            // 상단 고정 — 전체 너비, 높이 88px (RerollButton과 충분한 거리 확보)
            apArea.anchorMin = new Vector2(0, 1);
            apArea.anchorMax = new Vector2(1, 1);
            apArea.pivot = new Vector2(0.5f, 1);
            apArea.anchoredPosition = new Vector2(0, 0);
            apArea.sizeDelta = new Vector2(0, 88);
            // ★ D 시안: AP 영역을 명확히 구분 — 불투명 배경 + 하단 구분선
            var apBg = apArea.gameObject.AddComponent<Image>();
            apBg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f); // ★ 불투명하게 (경계 명확화)
            var apOutline = apArea.gameObject.AddComponent<Outline>();
            apOutline.effectColor = new Color(0.9f, 0.78f, 0.31f, 0.3f); // 골드 테두리
            apOutline.effectDistance = new Vector2(2, -2);

            // ★ 중앙 정렬 — PipRow와 APText가 APArea 중앙에 위치하여 리롤 영역과 분리
            var apVlg = apArea.gameObject.AddComponent<VerticalLayoutGroup>();
            apVlg.spacing = 6;
            apVlg.padding = new RectOffset(8, 8, 10, 10);
            apVlg.childAlignment = TextAnchor.MiddleCenter; // ★ UpperCenter → MiddleCenter
            apVlg.childControlWidth = true;
            apVlg.childControlHeight = false;
            apVlg.childForceExpandWidth = true;
            apVlg.childForceExpandHeight = false;

            // AP 파이프 행 (5개 동그라미)
            var pipRow = NewRect("PipRow", apArea);
            var pipLe = pipRow.gameObject.AddComponent<LayoutElement>();
            pipLe.preferredHeight = 20;
            pipLe.flexibleHeight = 0;
            var pipHlg = pipRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            pipHlg.spacing = 6;
            pipHlg.childAlignment = TextAnchor.MiddleCenter;
            pipHlg.childControlWidth = false;
            pipHlg.childControlHeight = false;
            pipHlg.childForceExpandWidth = false;
            pipHlg.childForceExpandHeight = false;
            for (int i = 0; i < 5; i++)
            {
                var pip = NewRect($"Pip{i + 1}", pipRow);
                pip.sizeDelta = new Vector2(18, 18);
                var pipImg = pip.gameObject.AddComponent<Image>();
                pipImg.color = APCyan; // 파란색 (활성)
            }

            // AP 숫자 텍스트 ("5/5")
            var apText = NewRect("APText", apArea);
            SetFixedHeight(apText, 30);
            var apT = apText.gameObject.AddComponent<TextMeshProUGUI>();
            apT.font = GetOrCreateKoreanFont();
            apT.text = "AP 5/5";
            apT.fontSize = 24;
            apT.fontStyle = FontStyles.Bold;
            apT.alignment = TextAlignmentOptions.Center;
            apT.color = APCyan;
        }

        // ══════════════════════════════════════════════════════════
        //  ButtonArea (SkillRow 우측 — 행2 열5: 리롤 + 턴종료)
        // ══════════════════════════════════════════════════════════

        private static void CreateButtonArea(RectTransform rightColumn)
        {
            // ButtonArea: APArea(88px) + 여백(8px) = 상단 96px 제외, 나머지 전부 채움
            var buttonArea = NewRect("ButtonArea", rightColumn);
            buttonArea.anchorMin = new Vector2(0, 0);
            buttonArea.anchorMax = new Vector2(1, 1);
            buttonArea.offsetMin = new Vector2(0, 0);    // 하단 끝
            buttonArea.offsetMax = new Vector2(0, -96);   // 상단 96px(APArea+spacing) 제외
            // ★ VLG 없음 — 인스펙터에서 직접 크기 조정 가능

            // 리롤 버튼: 상단 65% (anchorMin.y=0.35 → 위쪽 약간 축소)
            var rerollBtn = NewRect("RerollButton", buttonArea);
            rerollBtn.anchorMin = new Vector2(0, 0.35f);
            rerollBtn.anchorMax = new Vector2(1, 0.95f);
            rerollBtn.offsetMin = new Vector2(6, 4);
            rerollBtn.offsetMax = new Vector2(-6, -4);
            var rerollB = rerollBtn.gameObject.AddComponent<Button>();
            var rerollImg = rerollBtn.gameObject.AddComponent<Image>();
            rerollImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // ★ D 시안: 미니멀 회색
            rerollB.targetGraphic = rerollImg;
            var rerollLabel = NewRect("T", rerollBtn);
            SetFillParent(rerollLabel);
            AddText(rerollLabel, "리롤\n2/2", 13, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);

            // 턴 종료 버튼: 하단 35%
            var endBtn = NewRect("EndTurnButton", buttonArea);
            endBtn.anchorMin = new Vector2(0, 0);
            endBtn.anchorMax = new Vector2(1, 0.33f);
            endBtn.offsetMin = new Vector2(6, 4);
            endBtn.offsetMax = new Vector2(-6, -4);
            var endB = endBtn.gameObject.AddComponent<Button>();
            var endImg = endBtn.gameObject.AddComponent<Image>();
            var endTurnSprite = LoadSprite(SPRITE_ENDTURN_BTN);
            if (endTurnSprite != null)
            {
                endImg.sprite = endTurnSprite;
                Set9Slice(endImg);
            }
            else
                endImg.color = AccentRed;
            endB.targetGraphic = endImg;
            var ec = endB.colors;
            ec.highlightedColor = new Color(0.9f, 0.2f, 0.3f);
            ec.pressedColor = new Color(0.5f, 0.08f, 0.15f);
            endB.colors = ec;

            var endLabel = NewRect("Text", endBtn);
            SetFillParent(endLabel);
            AddText(endLabel, "턴 종료\n[T]", 15, FontStyles.Bold, TextAlignmentOptions.Center, TextWhite);
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

        // ── 명시적 앵커 헬퍼 (VLG 없이 요소 배치) ──

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

        /// <summary>부모 좌측에서 xOffset, 세로 중앙, 고정 크기</summary>
        private static void AnchorLeft(RectTransform rt, float xOffset, float width, float height)
        {
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = new Vector2(xOffset, 0);
            rt.sizeDelta = new Vector2(width, height);
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
