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
            topBarImg.color = new Color(0.06f, 0.06f, 0.09f, 0.95f);

            // 하단 구분선
            var div = NewRect("Divider", bar);
            div.anchorMin = new Vector2(0, 0);
            div.anchorMax = new Vector2(1, 0);
            div.pivot = new Vector2(0.5f, 0);
            div.sizeDelta = new Vector2(0, 2);
            div.gameObject.AddComponent<Image>().color = AccentRed;

            // PartyStatusWidget 컴포넌트
            bar.gameObject.AddComponent<PartyStatusWidget>();

            // TopBarUI 컴포넌트 (AP/속도 제어는 BottomBar에 있는 요소를 참조)
            bar.gameObject.AddComponent<TopBarUI>();

            // 유물 바 — TopBar 전체 너비
            CreateRelicBar(bar);
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
            var bottomSolidSprite = LoadSprite(SPRITE_SOLID_FRAME);
            if (bottomSolidSprite != null)
            {
                bottomBarImg.sprite = bottomSolidSprite;
                Set9Slice(bottomBarImg);
            }
            bottomBarImg.color = new Color(0.06f, 0.06f, 0.09f, 0.95f);

            var div = NewRect("Divider", bar);
            div.anchorMin = new Vector2(0, 1);
            div.anchorMax = new Vector2(1, 1);
            div.pivot = new Vector2(0.5f, 1);
            div.sizeDelta = new Vector2(0, 2);
            div.gameObject.AddComponent<Image>().color = AccentRed;

            // ── AP 표시 (좌측) ──
            var apRect = NewRect("APText", bar);
            apRect.anchorMin = new Vector2(0, 0.5f);
            apRect.anchorMax = new Vector2(0, 0.5f);
            apRect.pivot = new Vector2(0, 0.5f);
            apRect.anchoredPosition = new Vector2(16, 6);
            apRect.sizeDelta = new Vector2(120, 36);
            var apT = apRect.gameObject.AddComponent<TextMeshProUGUI>();
            apT.font = GetOrCreateKoreanFont();
            apT.text = "AP 4/4";
            apT.fontSize = 24;
            apT.fontStyle = FontStyles.Bold;
            apT.alignment = TextAlignmentOptions.Center;
            apT.color = AccentYellow;

            // AP 게이지 바
            var apBar = NewRect("APBar", bar);
            apBar.anchorMin = new Vector2(0, 0);
            apBar.anchorMax = new Vector2(0, 0);
            apBar.pivot = new Vector2(0, 1);
            apBar.anchoredPosition = new Vector2(16, -8);
            apBar.sizeDelta = new Vector2(120, 8);
            apBar.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

            var apFill = NewRect("APFill", apBar);
            apFill.anchorMin = Vector2.zero;
            apFill.anchorMax = new Vector2(1f, 1f);
            apFill.offsetMin = Vector2.zero;
            apFill.offsetMax = Vector2.zero;
            apFill.gameObject.AddComponent<Image>().color = AccentYellow;

            // ── 속도 토글 버튼 (우측 영역) ──
            var speedBtn = NewRect("SpeedButton", bar);
            speedBtn.anchorMin = new Vector2(1, 0.5f);
            speedBtn.anchorMax = new Vector2(1, 0.5f);
            speedBtn.pivot = new Vector2(1, 0.5f);
            speedBtn.anchoredPosition = new Vector2(-175, 6);
            speedBtn.sizeDelta = new Vector2(50, 36);
            var speedB = speedBtn.gameObject.AddComponent<Button>();
            var speedImg = speedBtn.gameObject.AddComponent<Image>();
            speedImg.color = new Color(0.2f, 0.2f, 0.35f, 0.9f);
            speedB.targetGraphic = speedImg;
            var speedLabel = NewRect("SpeedLabel", speedBtn);
            SetFillParent(speedLabel);
            var speedT = speedLabel.gameObject.AddComponent<TextMeshProUGUI>();
            speedT.font = GetOrCreateKoreanFont();
            speedT.text = "1x";
            speedT.fontSize = 16;
            speedT.fontStyle = FontStyles.Bold;
            speedT.alignment = TextAlignmentOptions.Center;
            speedT.color = TextWhite;
            speedT.raycastTarget = false;

            // ── 턴 종료 버튼 (우측 끝) ──
            var btn = NewRect("EndTurnButton", bar);
            btn.anchorMin = new Vector2(1, 0.5f);
            btn.anchorMax = new Vector2(1, 0.5f);
            btn.pivot = new Vector2(1, 0.5f);
            btn.anchoredPosition = new Vector2(-12, 6);
            btn.sizeDelta = new Vector2(155, 40);
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

            // ── 리롤 카운트 (속도 버튼 아래) ──
            var rerollRect = NewRect("RerollText", bar);
            rerollRect.anchorMin = new Vector2(1, 0);
            rerollRect.anchorMax = new Vector2(1, 0);
            rerollRect.pivot = new Vector2(1, 0);
            rerollRect.anchoredPosition = new Vector2(-12, 8);
            rerollRect.sizeDelta = new Vector2(155, 22);
            var rerollT = rerollRect.gameObject.AddComponent<TextMeshProUGUI>();
            rerollT.font = GetOrCreateKoreanFont();
            rerollT.text = "리롤 2/2";
            rerollT.fontSize = 13;
            rerollT.fontStyle = FontStyles.Bold;
            rerollT.alignment = TextAlignmentOptions.Center;
            rerollT.color = ShieldBrown;
            rerollT.raycastTarget = false;
        }

        // ══════════════════════════════════════════════════════════
        //  Player Strip (BottomBar 위, 가로 카드 배치)
        // ══════════════════════════════════════════════════════════

        private static void CreatePlayerStrip(RectTransform parent)
        {
            var strip = NewRect("PlayerStrip", parent);
            strip.anchorMin = new Vector2(0, 0);
            strip.anchorMax = new Vector2(1, 0);
            strip.pivot = new Vector2(0.5f, 0);
            strip.anchoredPosition = new Vector2(0, 100);
            strip.sizeDelta = new Vector2(0, 64);

            var stripImg = strip.gameObject.AddComponent<Image>();
            var solidSprite = LoadSprite(SPRITE_SOLID_FRAME);
            if (solidSprite != null)
            {
                stripImg.sprite = solidSprite;
                Set9Slice(stripImg);
            }
            stripImg.color = new Color(0.06f, 0.06f, 0.09f, 0.95f);

            // 하단 구분선 (AccentRed) — HLG의 레이아웃 영향에서 제외
            var div = NewRect("Divider", strip);
            div.anchorMin = new Vector2(0, 0);
            div.anchorMax = new Vector2(1, 0);
            div.pivot = new Vector2(0.5f, 0);
            div.sizeDelta = new Vector2(0, 2);
            div.gameObject.AddComponent<Image>().color = AccentRed;
            var divLE = div.gameObject.AddComponent<LayoutElement>();
            divLE.ignoreLayout = true;

            var hlg = strip.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.padding = new RectOffset(8, 8, 4, 4);
            hlg.childAlignment = TextAnchor.LowerCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 정적 샘플 카드 4개
            string[] names = { "카인", "쉘레이아", "아트카나", "샤이비어" };
            string[] hps = { "88/88", "55/55", "45/45", "50/50" };
            for (int i = 0; i < 4; i++)
                CreatePlayerCard(strip, names[i], hps[i]);
        }

        // ══════════════════════════════════════════════════════════
        //  Relic Bar (TopBar 좌측)
        // ══════════════════════════════════════════════════════════

        private static void CreateRelicBar(RectTransform parent)
        {
            var bar = NewRect("RelicBar", parent);
            bar.anchorMin = Vector2.zero;
            bar.anchorMax = new Vector2(1f, 1f);
            bar.offsetMin = new Vector2(8, 0);
            bar.offsetMax = new Vector2(-8, 0);

            var hlg = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.padding = new RectOffset(4, 4, 8, 8);
            hlg.childAlignment = TextAnchor.LowerLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 빈 상태 플레이스홀더 — Refresh()에서 유물이 있으면 자동 제거됨
            var placeholder = NewRect("Placeholder", bar);
            placeholder.sizeDelta = new Vector2(40, 40);
            var phImg = placeholder.gameObject.AddComponent<Image>();
            phImg.color = new Color(0.15f, 0.15f, 0.22f, 0.5f);
            var phLabel = NewRect("T", placeholder);
            SetFillParent(phLabel);
            var phT = phLabel.gameObject.AddComponent<TextMeshProUGUI>();
            phT.font = GetOrCreateKoreanFont();
            phT.text = "유물";
            phT.fontSize = 10;
            phT.alignment = TextAlignmentOptions.Center;
            phT.color = new Color(0.5f, 0.5f, 0.6f, 0.6f);
            phT.raycastTarget = false;

            bar.gameObject.AddComponent<BattleRelicBarUI>();

            // SerializedObject로 필드 와이어링
            var ser = new UnityEditor.SerializedObject(bar.GetComponent<BattleRelicBarUI>());
            var containerProp = ser.FindProperty("_iconContainer");
            if (containerProp != null) containerProp.objectReferenceValue = bar;
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
