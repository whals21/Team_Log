using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.Shop;
using TeamLog.Skill;
using TeamLog.UI;

namespace TeamLog.UI.Shop
{
    /// <summary>
    /// 상점 UI — 구매 슬롯 목록 + 골드 표시 + 나가기 버튼
    /// 판매 탭: ShopUI.Sell.cs
    /// </summary>
    public partial class ShopUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private GameObject _shopSlotPrefab;
        [SerializeField] private TextMeshProUGUI _goldLabel;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private Button _exitButton;
        [SerializeField] private ConfirmationDialog _confirmationDialog;

        [Header("Tab References")]
        [SerializeField] private Button _buyTabButton;
        [SerializeField] private Button _sellTabButton;
        [SerializeField] private GameObject _buyContainer;
        [SerializeField] private Transform _sellContainer;

        [Header("Augment Assign")]
        [SerializeField] private AugmentSelectPanel _augmentSelectPanel;

        private ShopManager _shopManager;
        private GameRunState _runState;
        private System.Action _onShopExit;
        private readonly List<ShopSlot> _currentSlots = new();
        private IReadOnlyList<AugmentData> _augmentPool;
        private IReadOnlyList<RelicData> _relicPool;
        private ShopSlot _pendingPurchase;
        private bool _isSellMode;

        private void Awake()
        {
            if (_exitButton != null)
                _exitButton.onClick.AddListener(OnExit);
            if (_buyTabButton != null)
                _buyTabButton.onClick.AddListener(() => SetTab(false));
            if (_sellTabButton != null)
                _sellTabButton.onClick.AddListener(() => SetTab(true));
        }

        public void Initialize(GameRunState runState, System.Action onShopExit,
            IReadOnlyList<RelicData> relicPool = null)
        {
            _runState = runState;
            _onShopExit = onShopExit;
            _relicPool = relicPool;
            _shopManager = new ShopManager();
        }

        /// <summary>
        /// 증강 풀 주입 (MapSceneSetup에서 호출)
        /// </summary>
        public void SetAugmentPool(IReadOnlyList<AugmentData> augmentPool)
        {
            _augmentPool = augmentPool;
        }

        /// <summary>
        /// 상점 열기
        /// </summary>
        public void OpenShop(int floorNumber)
        {
            gameObject.SetActive(true);
            var cg = UIAnimationHelper.EnsureCanvasGroup(gameObject);
            cg.alpha = 0f;
            UIAnimationHelper.FadeIn(cg);
            AudioManager.Instance.PlayUIShopOpen();

            _currentFloorNumber = floorNumber;
            SetTab(false); // 기본: 구매 탭

            ClearSlots();

            _currentSlots.Clear();
            var slots = _shopManager.GenerateShopSlots(floorNumber, _augmentPool, _relicPool);
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
                AudioManager.Instance.PlayUIWarning();
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

                if (slot.ContentType == ShopSlot.SlotContentType.Augment && _augmentSelectPanel != null)
                {
                    _augmentSelectPanel.Show(slot.Augment, _runState.PlayerParty, _runState,
                        (applied) =>
                        {
                            AudioManager.Instance.PlayUIShopPurchase();
                            AudioManager.Instance.PlayUIGoldSpend();
                            if (applied)
                                ToastUI.Show($"{slot.Name}을(를) 구매했습니다.");
                            else
                                ToastUI.Show("증강을 적용하지 않았습니다.");
                        });
                }
                else
                {
                    AudioManager.Instance.PlayUIShopPurchase();
                    AudioManager.Instance.PlayUIGoldSpend();
                    ToastUI.Show($"{slot.Name}을(를) 구매했습니다.");
                }
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
            HideAndNotify();
        }

        private void HideAndNotify()
        {
            _onShopExit?.Invoke(); // FadeOut이 SetActive(false)하므로 콜백을 먼저 실행
            var cg = UIAnimationHelper.EnsureCanvasGroup(gameObject);
            UIAnimationHelper.FadeOut(cg);
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
            if (_buyTabButton != null)
                _buyTabButton.onClick.RemoveAllListeners();
            if (_sellTabButton != null)
                _sellTabButton.onClick.RemoveAllListeners();
        }

        private void SetTab(bool sellMode)
        {
            _isSellMode = sellMode;
            if (_buyContainer != null) _buyContainer.SetActive(!sellMode);
            if (_sellContainer != null) _sellContainer.gameObject.SetActive(sellMode);
            if (sellMode) RefreshSellList();
        }
    }
}
