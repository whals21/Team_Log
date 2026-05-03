using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Reward;

namespace TeamLog.UI.Reward
{
    /// <summary>
    /// 보상 카드 하나
    /// </summary>
    public class RewardCard : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _descLabel;
        [SerializeField] private Button _button;

        private RewardOffer _offer;
        private System.Action<RewardOffer> _onSelected;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        public void Setup(RewardOffer offer, System.Action<RewardOffer> onSelected)
        {
            _offer = offer;
            _onSelected = onSelected;

            string typeLabel = offer.Type switch
            {
                RewardType.Gold => "골드",
                RewardType.Skill => "스킬",
                RewardType.Item => "아이템",
                _ => "보상"
            };

            if (_titleLabel != null)
            {
                _titleLabel.text = typeLabel;
                _titleLabel.color = offer.GetRarityColor();
            }

            if (_descLabel != null)
                _descLabel.text = offer.Description;

            if (_backgroundImage != null)
            {
                Color baseColor = offer.Type switch
                {
                    RewardType.Gold => new Color(0.2f, 0.18f, 0.08f),
                    RewardType.Skill => new Color(0.12f, 0.15f, 0.22f),
                    RewardType.Item => new Color(0.15f, 0.12f, 0.2f),
                    _ => new Color(0.15f, 0.15f, 0.2f)
                };
                _backgroundImage.color = baseColor;
            }
        }

        private void OnClicked()
        {
            _onSelected?.Invoke(_offer);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClicked);
        }
    }
}
