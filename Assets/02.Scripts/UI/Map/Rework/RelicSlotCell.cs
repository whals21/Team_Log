using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Reward;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// 단일 유물 슬롯 (UIBestPractices §1 준거 — 별도 파일).
    /// </summary>
    public class RelicSlotCell : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _frame;
        [SerializeField] private Image _synergyDot;       // 우상단 글로우 점

        private RelicData _relic;

        private void Awake()
        {
            if (_icon == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "Icon");
                if (go != null) _icon = go.GetComponent<Image>();
            }
            if (_frame == null) _frame = GetComponent<Image>();
            if (_synergyDot != null) _synergyDot.gameObject.SetActive(false);

            // 자식 Image raycastTarget=false (슬롯 클릭 가로채기 방지)
            UIAutoBindHelper.DisableChildRaycastsExcept(transform);
        }

        public void SetRelic(RelicData relic)
        {
            _relic = relic;
            if (_icon != null) _icon.gameObject.SetActive(true);

            // ★ 플레이스홀더: 이름 첫 글자 표시 (실제 아트는 추후 연동)
            if (_icon != null)
            {
                _icon.color = UIPalette.Default.GradeCursed; // 임시 색상
                var label = _icon.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    string n = relic.RelicName;
                    label.text = string.IsNullOrEmpty(n) ? "?" : n.Substring(0, 1);
                    label.gameObject.SetActive(true);
                }
            }

            // 시너지 발동 여부 — 향후 RelicHandler 연동
            if (_synergyDot != null) _synergyDot.gameObject.SetActive(false);
        }

        public void SetEmpty()
        {
            _relic = null;
            if (_icon != null) _icon.gameObject.SetActive(false);
            if (_synergyDot != null) _synergyDot.gameObject.SetActive(false);
        }
    }
}
