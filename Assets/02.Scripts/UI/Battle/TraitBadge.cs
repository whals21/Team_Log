using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 캐릭터 장착 특성(CharacterTraitData) 표시 배지 (Phase 8 + Phase CC).
    /// 특성 이름 + 툴팁(설명) + 클릭 시 상세 정보.
    /// StatusEffectBadge/ResourceBadge와 동일한 동적 생성 패턴.
    /// </summary>
    public class TraitBadge : MonoBehaviour
    {
        private static readonly Vector2 BadgeSize = new Vector2(100, 22);
        private static readonly float FontSize = 11f;

        private CharacterTraitData _trait;
        private TextMeshProUGUI _labelTmp;

        /// <summary>부모 아래 특성 배지 동적 생성. trait null이면 null 반환.</summary>
        public static TraitBadge Create(Transform parent, CharacterTraitData trait)
        {
            if (trait == null) return null;

            var go = new GameObject("TraitBadge");
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = BadgeSize;

            var bg = go.AddComponent<Image>();
            bg.color = UIPalette.Default.RarityUnique; // 특성 = 해금 등급 색 (보라계열)

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
            tmp.text = trait.DisplayName ?? trait.TraitId;

            var badge = go.AddComponent<TraitBadge>();
            badge._trait = trait;
            badge._labelTmp = tmp;

            // 툴팁
            var tooltip = go.AddComponent<TooltipTarget>();
            tooltip.SetContent(trait.DisplayName, trait.Description);

            return badge;
        }

        /// <summary>장착 특성 변경 시 갱신.</summary>
        public void Refresh(CharacterTraitData trait)
        {
            if (trait == null)
            {
                gameObject.SetActive(false);
                return;
            }
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            _trait = trait;
            if (_labelTmp != null)
                _labelTmp.text = trait.DisplayName ?? trait.TraitId;
            var tooltip = GetComponent<TooltipTarget>();
            if (tooltip != null)
                tooltip.SetContent(trait.DisplayName, trait.Description);
        }
    }
}
