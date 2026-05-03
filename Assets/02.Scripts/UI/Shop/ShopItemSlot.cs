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
        [SerializeField] private Button _buyButton;
        [SerializeField] private GameObject _soldOverlay;

        private ShopSlot _slot;
        private System.Action<ShopSlot> _onBuyClicked;

        private void Awake()
        {
            if (_buyButton != null)
                _buyButton.onClick.AddListener(OnBuy);
        }

        public void Setup(ShopSlot slot, System.Action<ShopSlot> onBuyClicked)
        {
            _slot = slot;
            _onBuyClicked = onBuyClicked;
            UpdateVisual();
        }

        public void UpdateVisual()
        {
            if (_slot == null) return;

            if (_nameLabel != null)
                _nameLabel.text = _slot.Name;
            if (_descLabel != null)
                _descLabel.text = _slot.Desc;
            if (_priceLabel != null)
                _priceLabel.text = $"{_slot.Price} G";
            if (_buyButton != null)
                _buyButton.interactable = !_slot.IsSold;
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
