using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// MP 바 UI 컴포넌트
    /// </summary>
    public class MPBarUI : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private TextMeshProUGUI _mpText;
        [SerializeField] private Color _mpColor = new Color(0.2f, 0.5f, 0.9f);

        public void UpdateMP(int current, int max)
        {
            float ratio = max > 0 ? (float)current / max : 0f;

            if (_fillImage != null)
            {
                _fillImage.fillAmount = ratio;
                _fillImage.color = _mpColor;
            }

            if (_mpText != null)
            {
                _mpText.text = $"{current}/{max}";
            }
        }
    }
}
