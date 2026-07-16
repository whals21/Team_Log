using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TeamLog.Characters;

using SkillData = TeamLog.Characters.SkillData;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// Phase CC-2F: Necromancer 전용 시체 패널 — Mortis 패널 "위쪽"에 별도 표시.
    /// 4개 슬롯을 2x2 그리드로 큼직하게 표시. 시체 행동 시 해당 슬롯 강조.
    /// 강화 수치(MassEmpower/KillEmpower) 노란색으로 표시.
    /// 비활성(Necromancer 사망) 시 회색 톤.
    /// </summary>
    public class CorpseSlotPanel : MonoBehaviour
    {
        private Character _necromancer;
        private readonly List<TextMeshProUGUI> _slotTexts = new();
        private readonly List<Image> _slotBgs = new();
        private TextMeshProUGUI _titleText;

        // 색상 토큰
        private static readonly Color PanelBg = new Color(0.10f, 0.06f, 0.18f, 0.95f);
        private static readonly Color SlotNormalBg = new Color(0.18f, 0.12f, 0.28f, 0.95f);
        private static readonly Color SlotHighlightBg = new Color(0.65f, 0.30f, 0.85f, 0.95f);
        private static readonly Color SlotInactiveBg = new Color(0.10f, 0.10f, 0.12f, 0.6f);
        private static readonly Color TextNormal = new Color(0.88f, 0.82f, 0.95f);
        private static readonly Color TextEmpowered = new Color(0.98f, 0.85f, 0.30f);
        private static readonly Color TextDead = new Color(0.45f, 0.42f, 0.50f);
        private static readonly Color TitleColor = new Color(0.75f, 0.55f, 0.90f);

        /// <summary>
        /// 부모 컨테이너(Mortis VBox)에 시체 슬롯 패널 생성.
        /// 너비는 Mortis 패널과 동일, 높이 약 70px.
        /// </summary>
        public static CorpseSlotPanel Create(Transform parent, Character necromancer, float width = 180f)
        {
            // 최상위 패널
            var panelGO = new GameObject("CorpseSlotPanel",
                typeof(RectTransform), typeof(Image));
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.SetParent(parent, false);

            var panelBg = panelGO.GetComponent<Image>();
            panelBg.color = PanelBg;
            panelBg.raycastTarget = false;

            var panelLe = panelGO.AddComponent<LayoutElement>();
            panelLe.preferredWidth = width;
            panelLe.preferredHeight = 78f;
            panelLe.minHeight = 78f;
            panelLe.flexibleWidth = 0;
            panelLe.flexibleHeight = 0;

            // 패널 내부 VerticalLayoutGroup — Title + Grid
            var panelVlg = panelGO.AddComponent<VerticalLayoutGroup>();
            panelVlg.padding = new RectOffset(4, 4, 2, 4);
            panelVlg.spacing = 2;
            panelVlg.childAlignment = TextAnchor.UpperCenter;
            panelVlg.childControlWidth = true;
            panelVlg.childControlHeight = false;
            panelVlg.childForceExpandWidth = true;
            panelVlg.childForceExpandHeight = false;

            var panelCsf = panelGO.AddComponent<ContentSizeFitter>();
            panelCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            panelCsf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var panel = panelGO.AddComponent<CorpseSlotPanel>();

            // 타이틀 — "💀 시체"
            var titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.SetParent(panelRect, false);
            var titleLe = titleGO.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 14f;
            titleLe.flexibleHeight = 0;

            var title = titleGO.GetComponent<TextMeshProUGUI>();
            title.text = "💀 시체";
            title.fontSize = 10;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color = TitleColor;
            title.raycastTarget = false;
            title.enableWordWrapping = false;
            panel._titleText = title;

            // 그리드 컨테이너 (2x2)
            var gridGO = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            var gridRect = gridGO.GetComponent<RectTransform>();
            gridRect.SetParent(panelRect, false);
            var gridLe = gridGO.AddComponent<LayoutElement>();
            gridLe.flexibleHeight = 1;
            gridLe.minHeight = 50;

            var grid = gridGO.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2((width - 16) / 2f, 24f);
            grid.spacing = new Vector2(2, 2);
            grid.padding = new RectOffset(2, 2, 0, 0);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            // 4개 슬롯
            for (int i = 0; i < 4; i++)
            {
                var slotGO = new GameObject($"Slot{i}", typeof(RectTransform), typeof(Image));
                var slotRect = slotGO.GetComponent<RectTransform>();
                slotRect.SetParent(gridRect, false);

                var bg = slotGO.GetComponent<Image>();
                bg.color = SlotNormalBg;
                bg.raycastTarget = false;
                panel._slotBgs.Add(bg);

                // 텍스트 자식
                var textGO = new GameObject("T", typeof(RectTransform), typeof(TextMeshProUGUI));
                var textRect = textGO.GetComponent<RectTransform>();
                textRect.SetParent(slotRect, false);
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                var text = textGO.GetComponent<TextMeshProUGUI>();
                text.fontSize = 11;
                text.alignment = TextAlignmentOptions.Center;
                text.color = TextNormal;
                text.raycastTarget = false;
                text.enableWordWrapping = false;
                text.overflowMode = TextOverflowModes.Ellipsis;
                panel._slotTexts.Add(text);
            }

            panel.Initialize(necromancer);
            return panel;
        }

        private void Initialize(Character necromancer)
        {
            _necromancer = necromancer;
            Refresh();
        }

        /// <summary>시체 슬롯 4개 표시 갱신 — 스킬 이름 + 강화 수치.</summary>
        public void Refresh()
        {
            if (this == null) return;
            if (_necromancer?.Corpse == null)
            {
                gameObject.SetActive(false);
                return;
            }

            bool isActive = _necromancer.Corpse.IsActive && _necromancer.IsAlive;
            gameObject.SetActive(true);

            if (_titleText != null)
                _titleText.color = isActive ? TitleColor : TextDead;

            int massBonus = _necromancer.Corpse.MassEmpowerBonus;
            int killBonus = _necromancer.Corpse.KillEmpowerBonus;
            int totalBonus = massBonus + killBonus;

            for (int i = 0; i < 4; i++)
            {
                var skill = _necromancer.Corpse.Slots[i];
                var text = _slotTexts[i];
                var bg = _slotBgs[i];

                if (skill == null)
                {
                    text.text = "-";
                    text.color = TextDead;
                    bg.color = SlotInactiveBg;
                    continue;
                }

                // 스킬 이름 표시 (길면 축약)
                string displayName = GetShortName(skill.SkillName);
                string bonusStr = totalBonus > 0 ? $"+{totalBonus}" : "";
                text.text = string.IsNullOrEmpty(bonusStr) ? displayName : $"{displayName} {bonusStr}";
                text.color = totalBonus > 0
                    ? TextEmpowered
                    : (isActive ? TextNormal : TextDead);
                bg.color = isActive ? SlotNormalBg : SlotInactiveBg;
            }
        }

        /// <summary>지정 슬롯을 잠깐 강조 (시체 행동 시).</summary>
        public void HighlightSlot(int slotIdx)
        {
            if (this == null || slotIdx < 0 || slotIdx >= _slotBgs.Count) return;

            var bg = _slotBgs[slotIdx];
            if (bg == null) return;
            bg.color = SlotHighlightBg;

            // 0.5초 후 원래 색으로 복구
            DOTween.To(() => 0f, x =>
            {
                if (bg == null) return;
                bg.color = Color.Lerp(SlotHighlightBg, SlotNormalBg, x);
            }, 1f, 0.5f).SetUpdate(true);
        }

        /// <summary>스킬 이름을 축약 — 4글자 이하면 그대로, 이상이면 첫 4글자.</summary>
        private static string GetShortName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "?";
            return name.Length <= 4 ? name : name.Substring(0, 4);
        }
    }
}
