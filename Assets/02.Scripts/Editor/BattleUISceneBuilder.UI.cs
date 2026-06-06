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

            // 유물 바 — BottomBar 우측 영역
            CreateRelicBar(bar);
        }

        // ══════════════════════════════════════════════════════════
        //  Relic Bar (BottomBar 우측)
        // ══════════════════════════════════════════════════════════

        private static void CreateRelicBar(RectTransform parent)
        {
            var bar = NewRect("RelicBar", parent);
            bar.anchorMin = new Vector2(0.15f, 0);
            bar.anchorMax = new Vector2(0.5f, 1);
            bar.offsetMin = Vector2.zero;
            bar.offsetMax = Vector2.zero;

            var hlg = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.padding = new RectOffset(4, 4, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var countLabel = NewRect("RelicCount", bar);
            countLabel.anchorMin = new Vector2(0, 0.5f);
            countLabel.anchorMax = new Vector2(0, 0.5f);
            countLabel.pivot = new Vector2(0, 0.5f);
            countLabel.anchoredPosition = Vector2.zero;
            countLabel.sizeDelta = new Vector2(80, 30);
            var ct = countLabel.gameObject.AddComponent<TextMeshProUGUI>();
            ct.font = GetOrCreateKoreanFont();
            ct.text = "";
            ct.fontSize = 14;
            ct.fontStyle = FontStyles.Bold;
            ct.alignment = TextAlignmentOptions.Left;
            ct.color = new Color(0.7f, 0.3f, 0.9f);

            bar.gameObject.AddComponent<BattleRelicBarUI>();

            // SerializedObject로 필드 와이어링
            var ser = new UnityEditor.SerializedObject(bar.GetComponent<BattleRelicBarUI>());
            var containerProp = ser.FindProperty("_iconContainer");
            if (containerProp != null) containerProp.objectReferenceValue = bar;
            var countProp = ser.FindProperty("_countLabel");
            if (countProp != null) countProp.objectReferenceValue = ct;
            ser.ApplyModifiedProperties();
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
