using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 캐릭터 고유 자원(Ember/Vengeance/Frost/Prophecy) 표시 배지 (Phase CC).
    /// 자원별 고유 색상 + 현재/최대 스택 + 툴팁.
    /// 스택 변화 시 DOTween 펀치 스케일로 시각 강조 (증가=확대, 감소=축소).
    /// StatusEffectBadge 패턴 참고 — 동적 생성, 씬 수정 불필요.
    /// </summary>
    public class ResourceBadge : MonoBehaviour
    {
        private static readonly Vector2 BadgeSize = new Vector2(90, 24);
        private static readonly float FontSize = 12f;
        private static readonly float PunchScale = 0.25f;
        private static readonly float PunchDuration = 0.25f;

        private CharacterResourceComponent _resource;
        private TextMeshProUGUI _labelTmp;
        private Image _bg;
        private int _lastStacks = -1;

        /// <summary>부모 아래 자원 배지 동적 생성. 자원 null이면 null 반환.</summary>
        public static ResourceBadge Create(Transform parent, CharacterResourceComponent resource)
        {
            if (resource == null) return null;

            var go = new GameObject("ResourceBadge");
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = BadgeSize;

            var bg = go.AddComponent<Image>();
            bg.color = BattleDisplayUtil.GetResourceColor(resource.Resource);

            var labelObj = new GameObject("T");
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.SetParent(rect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            UIKoreanFont.EnsureFont(tmp);
            tmp.fontSize = FontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;

            var badge = go.AddComponent<ResourceBadge>();
            badge._resource = resource;
            badge._bg = bg;
            badge._labelTmp = tmp;

            // 툴팁
            var tooltip = go.AddComponent<TooltipTarget>();
            string label = BattleDisplayUtil.GetResourceLabel(resource.Resource);
            string desc = BattleDisplayUtil.GetResourceDescription(resource.Resource);
            tooltip.SetContent(label, desc);

            badge.Refresh();
            return badge;
        }

        /// <summary>외부에서 매 턴/스킬 사용 후 갱신 호출.</summary>
        public void Refresh()
        {
            if (_resource == null || _labelTmp == null) return;

            int current = _resource.CurrentStacks;
            int max = _resource.MaxStacks;
            string initial = BattleDisplayUtil.GetResourceInitial(_resource.Resource);
            _labelTmp.text = $"{initial} {current}/{max}";

            // 스택 변화 시 펀치 애니 (최초 생성 시엔 미동작 — _lastStacks=-1)
            if (_lastStacks >= 0 && _lastStacks != current)
            {
                float dir = current > _lastStacks ? 1f : -0.5f;
                Punch(dir);
            }
            _lastStacks = current;
        }

        private void Punch(float direction)
        {
            var scale = transform.localScale;
            var target = scale + new Vector3(PunchScale * direction, PunchScale * direction, 0f);
            DOTween.To(
                () => transform.localScale,
                x => transform.localScale = x,
                target,
                PunchDuration * 0.5f)
                .OnComplete(() =>
                {
                    DOTween.To(
                        () => transform.localScale,
                        x => transform.localScale = x,
                        scale,
                        PunchDuration * 0.5f);
                });
        }
    }
}
