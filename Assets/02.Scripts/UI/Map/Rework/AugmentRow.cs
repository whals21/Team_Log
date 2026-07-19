using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Skill;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// 단일 증강 행 (UIBestPractices §1 준거 — 별도 파일).
    /// </summary>
    public class AugmentRow : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _ownerText;
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private Image _leftBorder;       // 좌측 보더 (자원색)

        private AugmentData _augment;

        private void Awake()
        {
            if (_icon == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "AugIcon");
                if (go != null) _icon = go.GetComponent<Image>();
            }
            if (_nameText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "AugName");
                if (go != null) _nameText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_ownerText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "AugOwner");
                if (go != null) _ownerText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_rankText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "AugRank");
                if (go != null) _rankText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_leftBorder == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "AugLeftBorder");
                if (go != null) _leftBorder = go.GetComponent<Image>();
            }
            UIAutoBindHelper.DisableChildRaycastsExcept(transform);
        }

        public void Initialize(AugmentData augment)
        {
            _augment = augment;
            Render();
        }

        private void Render()
        {
            if (_augment == null) return;

            if (_nameText != null)
                _nameText.text = _augment.AugmentName ?? _augment.name;

            if (_ownerText != null)
                _ownerText.text = "Equipped"; // ★ 실제 owner/skill 연동은 추후 — 현재 플레이스홀더

            // Rank — 첫 BehaviorTag의 Rank 표시
            var behaviors = _augment.Behaviors;
            int rank = (behaviors != null && behaviors.Count > 0) ? behaviors[0].Rank : 1;
            if (_rankText != null)
                _rankText.text = $"R{rank}";

            // 좌측 보더 색상 — 플레이스홀더 (향후 BehaviorKeyword 기반 매핑)
            Color augColor = UIPalette.Default.GradeCursed;
            if (_leftBorder != null) _leftBorder.color = augColor;
            if (_icon != null) _icon.color = augColor;
        }
    }
}
