using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// HP 바 UI 컴포넌트 (녹색: 플레이어, 빨강: 적)
    /// </summary>
    public class HPBarUI : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private TextMeshProUGUI _hpText;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = new Color(0.15f, 0.68f, 0.38f); // 녹색
        [SerializeField] private Color _lowColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private float _lowThreshold = 0.3f;

        private int _currentHP;
        private int _maxHP;

        public void SetColor(Color color)
        {
            _normalColor = color;
            UpdateVisual();
        }

        public void UpdateHP(int current, int max)
        {
            _currentHP = current;
            _maxHP = max;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            float ratio = _maxHP > 0 ? (float)_currentHP / _maxHP : 0f;

            if (_fillImage != null)
            {
                _fillImage.fillAmount = ratio;
                _fillImage.color = ratio <= _lowThreshold ? _lowColor : _normalColor;

                // anchor 기반 fill (fillAmount 대신)
                var rect = _fillImage.rectTransform;
                rect.anchorMax = new Vector2(ratio, 1f);
            }

            if (_hpText != null)
                _hpText.text = $"{_currentHP}/{_maxHP}";
        }
    }
}
