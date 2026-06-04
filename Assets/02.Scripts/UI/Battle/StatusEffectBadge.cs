using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 단일 상태이상 뱃지 — 배경색 + 라벨 + 턴 수, 클릭 시 상세 정보 이벤트 발생
    /// </summary>
    public class StatusEffectBadge : MonoBehaviour
    {
        private static readonly Vector2 BadgeSize = new Vector2(50, 20);
        private static readonly float BadgeFontSize = 11f;

        public static event Action<string, string> OnBadgeClicked;

        private ActiveEffect _effect;

        public static StatusEffectBadge Create(Transform parent, ActiveEffect effect)
        {
            var go = new GameObject("Badge");
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = BadgeSize;

            var bg = go.AddComponent<Image>();
            bg.color = BattleDisplayUtil.GetEffectColor(effect.Type);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;

            var labelObj = new GameObject("T");
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.SetParent(rect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = BadgeFontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;

            string name = BattleDisplayUtil.GetEffectLabel(effect.Type);
            string initial = BattleDisplayUtil.GetEffectInitial(effect.Type);
            tmp.text = effect.RemainingTurns > 0 ? $"{initial}{effect.RemainingTurns}" : initial;

            var badge = go.AddComponent<StatusEffectBadge>();
            badge._effect = effect;
            btn.onClick.AddListener(badge.OnClick);

            // 툴팁 추가
            var tooltip = go.AddComponent<TooltipTarget>();
            string tooltipTitle = BattleDisplayUtil.GetEffectLabel(effect.Type);
            string tooltipDesc = BattleDisplayUtil.GetEffectDescription(effect.Type);
            tooltip.SetContent(tooltipTitle, tooltipDesc);

            return badge;
        }

        private void OnClick()
        {
            if (_effect == null) return;

            string label = BattleDisplayUtil.GetEffectLabel(_effect.Type);
            string desc = BattleDisplayUtil.GetEffectDescription(_effect.Type);

            string detail = desc;
            if (_effect.Value > 0)
                detail += $" (수치: {_effect.Value})";
            if (_effect.RemainingTurns > 0)
                detail += $" — 남은 {_effect.RemainingTurns}턴";

            OnBadgeClicked?.Invoke(label, detail);
        }
    }
}
