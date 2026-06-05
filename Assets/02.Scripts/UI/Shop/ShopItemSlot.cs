using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Shop;

namespace TeamLog.UI.Shop
{
    /// <summary>
    /// 상점 아이템 슬롯 UI
    /// </summary>
    public class ShopItemSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _descLabel;
        [SerializeField] private TextMeshProUGUI _priceLabel;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _buyButton;
        [SerializeField] private GameObject _soldOverlay;

        private ShopSlot _slot;
        private System.Action<ShopSlot> _onBuyClicked;

        private static readonly Color CantAffordPriceColor = new Color(0.7f, 0.3f, 0.3f);
        private Color _normalPriceColor;
        private bool _hasNormalPriceColor;

        private void Awake()
        {
            if (_buyButton != null)
                _buyButton.onClick.AddListener(OnBuy);
        }

        public void Setup(ShopSlot slot, System.Action<ShopSlot> onBuyClicked)
        {
            _slot = slot;
            _onBuyClicked = onBuyClicked;

            if (_iconImage != null)
            {
                _iconImage.sprite = slot.Icon;
                _iconImage.enabled = slot.Icon != null;
            }

            UpdateVisual();
        }

        public void UpdateVisual(int currentGold = int.MaxValue)
        {
            if (_slot == null) return;

            if (_nameLabel != null)
                _nameLabel.text = _slot.Name;
            if (_descLabel != null)
                _descLabel.text = _slot.Desc;
            if (_priceLabel != null)
            {
                if (!_hasNormalPriceColor)
                {
                    _normalPriceColor = _priceLabel.color;
                    _hasNormalPriceColor = true;
                }
                _priceLabel.text = $"{_slot.Price} G";
                _priceLabel.color = (_slot.IsSold || currentGold >= _slot.Price)
                    ? _normalPriceColor : CantAffordPriceColor;
            }
            if (_buyButton != null)
                _buyButton.interactable = !_slot.IsSold && currentGold >= _slot.Price;
            if (_soldOverlay != null)
                _soldOverlay.SetActive(_slot.IsSold);
        }

        private void OnBuy()
        {
            _onBuyClicked?.Invoke(_slot);
        }

        private void OnDestroy()
        {
            if (_buyButton != null)
                _buyButton.onClick.RemoveListener(OnBuy);
        }
    }
}
