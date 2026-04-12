using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Map;
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

    /// <summary>
    /// 상점 UI — 구매 슬롯 목록 + 골드 표시 + 나가기 버튼
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private GameObject _shopSlotPrefab;
        [SerializeField] private TextMeshProUGUI _goldLabel;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private Button _exitButton;

        private ShopManager _shopManager;
        private GameRunState _runState;
        private System.Action _onShopExit;
        private readonly List<ShopSlot> _currentSlots = new();

        private void Awake()
        {
            if (_exitButton != null)
                _exitButton.onClick.AddListener(OnExit);
        }

        public void Initialize(GameRunState runState, System.Action onShopExit)
        {
            _runState = runState;
            _onShopExit = onShopExit;
            _shopManager = new ShopManager();
        }

        /// <summary>
        /// 상점 열기
        /// </summary>
        public void OpenShop(int floorNumber)
        {
            gameObject.SetActive(true);
            ClearSlots();

            _currentSlots.Clear();
            var slots = _shopManager.GenerateShopSlots(floorNumber);
            _currentSlots.AddRange(slots);

            if (_titleLabel != null)
                _titleLabel.text = "상점";

            UpdateGoldDisplay();

            foreach (var slot in slots)
            {
                if (_shopSlotPrefab == null || _slotContainer == null) continue;

                var slotObj = Instantiate(_shopSlotPrefab, _slotContainer);
                var shopSlot = slotObj.GetComponent<ShopItemSlot>();
                if (shopSlot != null)
                    shopSlot.Setup(slot, OnBuyItem);
            }
        }

        private void OnBuyItem(ShopSlot slot)
        {
            if (_shopManager.PurchaseItem(slot, _runState))
            {
                UpdateGoldDisplay();

                // 해당 슬롯 UI 갱신
                foreach (var slotUI in _slotContainer.GetComponentsInChildren<ShopItemSlot>())
                {
                    slotUI.UpdateVisual();
                }
            }
        }

        private void UpdateGoldDisplay()
        {
            if (_goldLabel != null && _runState != null)
                _goldLabel.text = $"{_runState.Gold} G";
        }

        private void OnExit()
        {
            gameObject.SetActive(false);
            _onShopExit?.Invoke();
        }

        private void ClearSlots()
        {
            if (_slotContainer == null) return;
            for (int i = _slotContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_slotContainer.GetChild(i).gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_exitButton != null)
                _exitButton.onClick.RemoveListener(OnExit);
        }
    }
}
