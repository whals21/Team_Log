using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 펼치기/접기 토글 버튼 — 연결된 GameObject의 활성/비활성을 토글.
    /// PartyStatusWidget / BattleLogUI 오버레이 패널 토글용.
    /// </summary>
    public class UIToggleButton : MonoBehaviour
    {
        [SerializeField] private GameObject _targetPanel;
        [SerializeField] private Image _buttonImage;
        [SerializeField] private Color _activeColor = new Color(0.3f, 0.5f, 0.9f, 0.95f);
        [SerializeField] private Color _inactiveColor = new Color(0.17f, 0.17f, 0.27f, 0.9f);

        private Button _button;
        private bool _isActive;

        public bool IsActive => _isActive;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_buttonImage == null) _buttonImage = GetComponent<Image>();
            if (_button != null)
                _button.onClick.AddListener(Toggle);

            // 초기 상태: 비활성
            if (_targetPanel != null)
                _targetPanel.SetActive(false);
            UpdateVisual();
        }

        public void SetTarget(GameObject panel)
        {
            _targetPanel = panel;
            if (_targetPanel != null)
                _targetPanel.SetActive(false);
            UpdateVisual();
        }

        public void Toggle()
        {
            if (_targetPanel == null) return;
            _isActive = !_isActive;
            _targetPanel.SetActive(_isActive);

            // 활성 시 패널 펼치기 애니메이션
            if (_isActive)
            {
                var cg = _targetPanel.GetComponent<CanvasGroup>();
                if (cg == null) cg = _targetPanel.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                DOTween.To(() => cg.alpha, x => cg.alpha = x, 1f, 0.2f);
            }

            UpdateVisual();
        }

        public void SetActive(bool active)
        {
            if (_targetPanel == null) return;
            _isActive = active;
            _targetPanel.SetActive(_isActive);
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_buttonImage != null)
                _buttonImage.color = _isActive ? _activeColor : _inactiveColor;
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(Toggle);
        }
    }
}
