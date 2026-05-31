using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.Shop;
using TeamLog.UI;

namespace TeamLog.UI.Shop
{
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
        [SerializeField] private ConfirmationDialog _confirmationDialog;

        private ShopManager _shopManager;
        private GameRunState _runState;
        private System.Action _onShopExit;
        private readonly List<ShopSlot> _currentSlots = new();
        private IReadOnlyList<SkillData> _skillPool;
        private IReadOnlyList<ItemData> _itemPool;
        private ShopSlot _pendingPurchase;

        private void Awake()
        {
            if (_exitButton != null)
                _exitButton.onClick.AddListener(OnExit);
        }

        public void Initialize(GameRunState runState, System.Action onShopExit,
            IReadOnlyList<SkillData> skillPool = null, IReadOnlyList<ItemData> itemPool = null)
        {
            _runState = runState;
            _onShopExit = onShopExit;
            _skillPool = skillPool;
            _itemPool = itemPool;
            _shopManager = new ShopManager();
        }

        /// <summary>
        /// 상점 열기
        /// </summary>
        public void OpenShop(int floorNumber)
        {
            gameObject.SetActive(true);
            var cg = UIAnimationHelper.EnsureCanvasGroup(gameObject);
            cg.alpha = 0f;
            StartCoroutine(UIAnimationHelper.FadeIn(cg));

            ClearSlots();

            _currentSlots.Clear();
            var slots = _shopManager.GenerateShopSlots(floorNumber, _skillPool, _itemPool);
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

            RefreshAllSlots();
        }

        private void OnBuyItem(ShopSlot slot)
        {
            if (slot == null || slot.IsSold) return;

            if (_runState.Gold < slot.Price)
            {
                ToastUI.Show("골드가 부족합니다.");
                return;
            }

            _pendingPurchase = slot;

            if (_confirmationDialog != null)
            {
                _confirmationDialog.Show(
                    $"{slot.Name}을(를) {slot.Price}G에 구매하시겠습니까?",
                    OnPurchaseConfirmed);
            }
            else
            {
                OnPurchaseConfirmed();
            }
        }

        private void OnPurchaseConfirmed()
        {
            if (_pendingPurchase == null) return;
            var slot = _pendingPurchase;
            _pendingPurchase = null;

            if (_shopManager.PurchaseItem(slot, _runState))
            {
                UpdateGoldDisplay();
                RefreshAllSlots();
                ToastUI.Show($"{slot.Name}을(를) 구매했습니다.");
            }
        }

        private void RefreshAllSlots()
        {
            if (_slotContainer == null || _runState == null) return;
            foreach (var slotUI in _slotContainer.GetComponentsInChildren<ShopItemSlot>())
                slotUI.UpdateVisual(_runState.Gold);
        }

        private void UpdateGoldDisplay()
        {
            if (_goldLabel != null && _runState != null)
                _goldLabel.text = $"{_runState.Gold} G";
        }

        private void OnExit()
        {
            StartCoroutine(HideAndNotify());
        }

        private IEnumerator HideAndNotify()
        {
            var cg = UIAnimationHelper.EnsureCanvasGroup(gameObject);
            yield return UIAnimationHelper.FadeOut(cg);
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
