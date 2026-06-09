using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Reward;

namespace TeamLog.UI.Reward
{
    /// <summary>
    /// 보상 카드 하나 — 골드/유물/증강제안 표시
    /// </summary>
    public class RewardCard : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
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

            if (offer.Type == RewardType.AugmentOffer)
            {
                SetupAugmentOffer(offer);
                return;
            }

            string typeLabel = offer.Type switch
            {
                RewardType.Gold => "골드",
                RewardType.Augment => "증강",
                RewardType.Relic => "유물",
                _ => "보상"
            };

            // 아이콘 표시
            if (_iconImage != null)
            {
                Sprite icon = offer.Type switch
                {
                    RewardType.Augment => offer.Augment?.Icon,
                    RewardType.Relic => offer.Relic?.Icon,
                    _ => null
                };
                _iconImage.sprite = icon;
                _iconImage.enabled = icon != null;
            }

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
                    RewardType.Augment => new Color(0.1f, 0.2f, 0.15f),
                    RewardType.Relic => new Color(0.22f, 0.12f, 0.22f),
                    _ => new Color(0.15f, 0.15f, 0.2f)
                };
                _backgroundImage.color = baseColor;
            }
        }

        private void SetupAugmentOffer(RewardOffer offer)
        {
            var augOffer = offer.AugmentOfferData;
            if (augOffer == null) return;

            bool isCursed = augOffer.IsCursed;

            // 아이콘
            if (_iconImage != null)
            {
                _iconImage.sprite = augOffer.Augment?.Icon;
                _iconImage.enabled = _iconImage.sprite != null;
            }

            // 타이틀: 캐릭터명 + 스킬명
            if (_titleLabel != null)
            {
                string tierLabel = augOffer.Tier switch
                {
                    3 => "[전설] ",
                    2 => "[희귀] ",
                    _ => ""
                };
                string curseTag = isCursed ? " <color=#ff4444>[저주]</color>" : "";

                _titleLabel.text = $"{tierLabel}{augOffer.GetDisplayText()}{curseTag}";
                _titleLabel.color = offer.GetRarityColor();
                _titleLabel.enableWordWrapping = true;
                _titleLabel.fontSize = 16;
            }

            // 설명: 증강 설명 + 저주 설명
            if (_descLabel != null)
            {
                _descLabel.text = augOffer.GetDetailText();
                _descLabel.enableWordWrapping = true;
                _descLabel.fontSize = 13;
            }

            // 배경색: 등급 + 저주
            if (_backgroundImage != null)
            {
                Color baseColor = isCursed
                    ? new Color(0.25f, 0.05f, 0.1f) // 저주: 어두운 보라빨강
                    : augOffer.Tier switch
                    {
                        3 => new Color(0.18f, 0.08f, 0.28f), // 전설: 보라
                        2 => new Color(0.06f, 0.12f, 0.25f), // 희귀: 파랑
                        _ => new Color(0.12f, 0.12f, 0.16f)  // 일반: 회색
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
