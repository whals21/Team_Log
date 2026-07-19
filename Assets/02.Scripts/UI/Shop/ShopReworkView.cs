using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;  // ★ SkillInstance (AutoAssignAugmentFallback)
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.Shop;
using TeamLog.Skill;
using TeamLog.UI;

namespace TeamLog.UI.Shop
{
    /// <summary>
    /// ★ Stained Glass Shop UI — 메인 View 컴포넌트 (B 시안).
    /// 기존 ShopUI 로직(OpenShop / Buy/Sell 탭 / 골드 / ConfirmationDialog)을 그대로 유지하되,
    /// 시각적 구조를 "스테인드글라스 유물함(Reliquary)"로 재설계.
    ///
    /// 자식 구조 (ShopSceneReworkBuilder가 생성):
    /// - ShopReworkView (CanvasGroup)
    ///   - DimBackground
    ///   - ReliquaryFrame (900x760)
    ///     - GlassCrown (상단 80px — 스테인드글라스 장식)
    ///     - ReliquaryPanel (본문)
    ///       - TopBar (HLG: TitleBlock + BuyTab/SellTab 버튼)
    ///       - GoldBar (좌측 "GOLD" + 우측 goldValue)
    ///       - BuyContainer
    ///         - SlotContainer (GridLayout 3×2)
    ///       - SellContainer (초기 비활성)
    ///         - SellSlotContainer (VLG)
    ///       - Footer (HLG: hint + Leave 버튼)
    ///
    /// ★ 기존 ShopUI와 호환: Initialize / SetAugmentPool / OpenShop 시그니처 동일.
    /// </summary>
    public class ShopReworkView : MonoBehaviour
    {
        [Header("Refs — Frame")]
        [SerializeField] private Image _glassCrownImage;
        [SerializeField] private Image _reliquaryPanelImage;

        [Header("Refs — Top Bar")]
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _subtitleLabel;
        [SerializeField] private Button _buyTabButton;
        [SerializeField] private Button _sellTabButton;
        [SerializeField] private Image _buyTabBackground;
        [SerializeField] private Image _sellTabBackground;

        [Header("Refs — Gold")]
        [SerializeField] private TextMeshProUGUI _goldValueText;

        [Header("Refs — Buy")]
        [SerializeField] private GameObject _buyContainer;
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private GameObject _shopSlotPrefab;

        [Header("Refs — Sell")]
        [SerializeField] private GameObject _sellContainer;
        [SerializeField] private Transform _sellSlotContainer;
        [SerializeField] private GameObject _sellRowPrefab;

        [Header("Refs — Footer")]
        [SerializeField] private Button _leaveButton;
        [SerializeField] private TextMeshProUGUI _hintLabel;

        [Header("External Dialogs")]
        [SerializeField] private ConfirmationDialog _confirmationDialog;
        [SerializeField] private AugmentSelectPanel _augmentSelectPanel;

        private ShopManager _shopManager;
        private GameRunState _runState;
        private System.Action _onShopExit;
        private IReadOnlyList<AugmentData> _augmentPool;
        private IReadOnlyList<RelicData> _relicPool;

        private readonly List<ShopSlot> _currentSlots = new();
        private readonly List<ShopItemRowRework> _spawnedRows = new();
        private ShopSlot _pendingPurchase;
        private bool _isSellMode;
        private int _currentFloorNumber;

        private CanvasGroup _canvasGroup;
        private bool _autoBound;

        private void Awake()
        {
            AutoBindMissingFields();
            EnsureCanvasGroup();

            if (_leaveButton != null) _leaveButton.onClick.AddListener(OnExit);
            if (_buyTabButton != null) _buyTabButton.onClick.AddListener(() => SetTab(false));
            if (_sellTabButton != null) _sellTabButton.onClick.AddListener(() => SetTab(true));

            // ★ CLAUDE.md #2 — Awake에서 SetActive(false) 금지 → CanvasGroup으로 비활성
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
        }

        private void EnsureCanvasGroup()
        {
            if (_canvasGroup == null)
                _canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void AutoBindMissingFields()
        {
            if (_autoBound) return;
            var root = transform;

            if (_glassCrownImage == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "GlassCrown");
                if (go != null) _glassCrownImage = go.GetComponent<Image>();
            }
            if (_reliquaryPanelImage == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "ReliquaryPanel");
                if (go != null) _reliquaryPanelImage = go.GetComponent<Image>();
            }
            if (_titleLabel == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "Title");
                if (go != null) _titleLabel = go.GetComponent<TextMeshProUGUI>();
            }
            if (_subtitleLabel == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "Subtitle");
                if (go != null) _subtitleLabel = go.GetComponent<TextMeshProUGUI>();
            }
            if (_buyTabButton == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "BuyTab");
                if (go != null) _buyTabButton = go.GetComponent<Button>();
            }
            if (_sellTabButton == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "SellTab");
                if (go != null) _sellTabButton = go.GetComponent<Button>();
            }
            if (_buyTabBackground == null && _buyTabButton != null)
                _buyTabBackground = _buyTabButton.GetComponent<Image>();
            if (_sellTabBackground == null && _sellTabButton != null)
                _sellTabBackground = _sellTabButton.GetComponent<Image>();
            if (_goldValueText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "GoldValue");
                if (go != null) _goldValueText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_buyContainer == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "BuyContainer");
                if (go != null) _buyContainer = go;
            }
            if (_slotContainer == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "SlotContainer");
                if (go != null) _slotContainer = go.transform;
            }
            if (_sellContainer == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "SellContainer");
                if (go != null) _sellContainer = go;
            }
            if (_sellSlotContainer == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "SellSlotContainer");
                if (go != null) _sellSlotContainer = go.transform;
            }
            if (_leaveButton == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "LeaveButton");
                if (go != null) _leaveButton = go.GetComponent<Button>();
            }
            if (_hintLabel == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "Hint");
                if (go != null) _hintLabel = go.GetComponent<TextMeshProUGUI>();
            }

            _autoBound = true;
        }

        // =========================================================
        // Public API (기존 ShopUI와 동일 — 하위 호환)
        // =========================================================

        public void Initialize(GameRunState runState, System.Action onShopExit,
            IReadOnlyList<RelicData> relicPool = null)
        {
            _runState = runState;
            _onShopExit = onShopExit;
            _relicPool = relicPool;
            _shopManager = new ShopManager();
        }

        public void SetAugmentPool(IReadOnlyList<AugmentData> augmentPool)
        {
            _augmentPool = augmentPool;
        }

        public void OpenShop(int floorNumber)
        {
            EnsureCanvasGroup();
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            _canvasGroup.alpha = 0f;
            UIAnimationHelper.FadeIn(_canvasGroup);
            AudioManager.Instance?.PlayUIShopOpen();

            _currentFloorNumber = floorNumber;
            SetTab(false);

            ClearSlots();
            _currentSlots.Clear();

            // 보류 상점 보너스 소비 (Phase 7B/7C)
            float discount = _runState != null ? _runState.ConsumeShopDiscount() : 0f;
            int extraRelics = _runState != null ? _runState.ConsumePendingShopExtraRelics() : 0;
            int extraAugments = _runState != null ? _runState.ConsumePendingShopExtraAugments() : 0;

            var slots = _shopManager.GenerateShopSlots(floorNumber, _augmentPool, _relicPool,
                extraAugments, extraRelics, discount);
            _currentSlots.AddRange(slots);

            // 타이틀
            if (_titleLabel != null)
            {
                string title = "RELICS OF THE FOLD";
                _titleLabel.text = title;
            }
            if (_subtitleLabel != null)
            {
                string sub = $"— Floor {floorNumber} · Sanctum —";
                if (discount > 0f)
                    sub += $"  ·  Discount {Mathf.RoundToInt(discount * 100)}%";
                if (extraRelics > 0 || extraAugments > 0)
                    sub += "  ·  Extra Wares";
                _subtitleLabel.text = sub;
            }

            UpdateGoldDisplay();

            foreach (var slot in slots)
            {
                if (_shopSlotPrefab == null || _slotContainer == null) continue;
                var slotObj = Instantiate(_shopSlotPrefab, _slotContainer);
                var row = slotObj.GetComponent<ShopItemRowRework>();
                if (row == null) row = slotObj.AddComponent<ShopItemRowRework>();
                row.Setup(slot, OnBuyItem);
                _spawnedRows.Add(row);
            }

            RefreshAllSlots();
        }

        // =========================================================
        // Buy 로직
        // =========================================================

        private void OnBuyItem(ShopSlot slot)
        {
            if (slot == null || slot.IsSold) return;

            if (_runState.Gold < slot.Price)
            {
                ToastUI.Show("골드가 부족합니다.");
                AudioManager.Instance?.PlayUIWarning();
                return;
            }

            _pendingPurchase = slot;

            // 저주 증강 — 특별 경고
            var rarity = ShopRarityStyle.EstimateFromSlot(slot);
            if (rarity == ShopRarity.Cursed && _confirmationDialog != null)
            {
                _confirmationDialog.Show(
                    $"⚠ 저주 증강 '{slot.Name}'은(는) 강력하지만 부작용이 있습니다.\n{slot.Price}G에 구매하시겠습니까?",
                    OnPurchaseConfirmed);
            }
            else if (_confirmationDialog != null)
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
                            AudioManager.Instance?.PlayUIShopPurchase();
                            AudioManager.Instance?.PlayUIGoldSpend();
                            if (applied)
                                ToastUI.Show($"{slot.Name}을(를) 구매했습니다.");
                            else
                                ToastUI.Show("증강을 적용하지 않았습니다.");
                        });
                }
                else if (slot.ContentType == ShopSlot.SlotContentType.Augment)
                {
                    // ★ P0-1 폴백: AugmentSelectPanel이 씬에 없으면 자동 배정 (첫 빈 슬롯)
                    bool fallbackApplied = AutoAssignAugmentFallback(slot.Augment);
                    AudioManager.Instance?.PlayUIShopPurchase();
                    AudioManager.Instance?.PlayUIGoldSpend();
                    ToastUI.Show(fallbackApplied
                        ? $"{slot.Name} 구매 (자동 배정)"
                        : $"{slot.Name} 구매 (빈 슬롯 없음 — 증강 영구 손실)");
                }
                else
                {
                    AudioManager.Instance?.PlayUIShopPurchase();
                    AudioManager.Instance?.PlayUIGoldSpend();
                    ToastUI.Show($"{slot.Name}을(를) 구매했습니다.");
                }
            }
        }

        /// <summary>
        /// ★ AugmentSelectPanel이 null일 때 폴백 — 첫 번째 살아있는 파티원의
        /// 첫 번째 빈 증강 슬롯에 자동 배정. 빈 슬롯 없으면 false 반환 (골드는 이미 소모됨).
        /// </summary>
        private bool AutoAssignAugmentFallback(AugmentData augment)
        {
            if (augment == null || _runState?.PlayerParty == null) return false;

            foreach (var member in _runState.PlayerParty)
            {
                if (member == null || !member.IsAlive) continue;
                foreach (var inst in member.SkillInventory.SkillInstances)
                {
                    if (inst.Augments.Count < SkillInstance.MaxAugments)
                    {
                        return _runState.AcquireAugment(augment, member, inst);
                    }
                }
            }
            return false;
        }

        private void RefreshAllSlots()
        {
            foreach (var row in _spawnedRows)
            {
                if (row != null) row.UpdateVisual(_runState != null ? _runState.Gold : 0);
            }
        }

        private void UpdateGoldDisplay()
        {
            if (_goldValueText != null && _runState != null)
                _goldValueText.text = $"{_runState.Gold} G";
        }

        // =========================================================
        // Tab 전환
        // =========================================================

        private void SetTab(bool sellMode)
        {
            _isSellMode = sellMode;
            if (_buyContainer != null) _buyContainer.SetActive(!sellMode);
            if (_sellContainer != null) _sellContainer.SetActive(sellMode);

            // 탭 시각 — 활성 쪽이 진하고 비활성 쪽이 옅음
            if (_buyTabBackground != null)
                _buyTabBackground.color = sellMode ? new Color(0.2f, 0.15f, 0.05f, 0.5f) : new Color(0.55f, 0.4f, 0.12f, 1f);
            if (_sellTabBackground != null)
                _sellTabBackground.color = sellMode ? new Color(0.55f, 0.4f, 0.12f, 1f) : new Color(0.2f, 0.15f, 0.05f, 0.5f);

            if (sellMode) RefreshSellList();
        }

        // =========================================================
        // Sell 로직 (간소화 — 유물만)
        // =========================================================

        private void RefreshSellList()
        {
            if (_sellSlotContainer == null || _runState == null) return;
            ClearSellList();

            // 빈 상태 메시지
            if (_runState.RelicHandler == null || _runState.RelicHandler.Relics.Count == 0)
            {
                if (_hintLabel != null)
                    _hintLabel.text = "판매할 수 있는 유물이 없습니다.";
                return;
            }

            foreach (var relic in _runState.RelicHandler.Relics)
            {
                if (relic == null) continue;
                var rowObj = Instantiate(_sellRowPrefab, _sellSlotContainer);
                var row = rowObj.GetComponent<ShopItemRowRework>();
                if (row == null) row = rowObj.AddComponent<ShopItemRowRework>();

                // Sell용 ShopSlot 가짜 데이터 생성
                var sellSlot = new ShopSlot
                {
                    ContentType = ShopSlot.SlotContentType.Relic,
                    Relic = relic,
                    Price = _shopManager.GetRelicSellPrice(_currentFloorNumber),
                    IsSold = false
                };
                var capturedRelic = relic;
                row.Setup(sellSlot, (s) => OnSellRelic(capturedRelic, s.Price));
            }
        }

        private void OnSellRelic(RelicData relic, int price)
        {
            if (_confirmationDialog != null)
            {
                _confirmationDialog.Show(
                    $"{relic.RelicName}을(를) {price}G에 판매하시겠습니까?",
                    () => ConfirmSellRelic(relic));
            }
            else
            {
                ConfirmSellRelic(relic);
            }
        }

        private void ConfirmSellRelic(RelicData relic)
        {
            if (_shopManager.SellRelic(relic, _runState, _currentFloorNumber))
            {
                int price = _shopManager.GetRelicSellPrice(_currentFloorNumber);
                UpdateGoldDisplay();
                RefreshSellList();
                // Buy 슬롯 가격도 갱신 (골드 변화 반영)
                RefreshAllSlots();
                AudioManager.Instance?.PlayUIGoldSpend();
                ToastUI.Show($"{relic.RelicName}을(를) {price}G에 판매했습니다.");
            }
        }

        private void ClearSellList()
        {
            if (_sellSlotContainer == null) return;
            for (int i = _sellSlotContainer.childCount - 1; i >= 0; i--)
                Destroy(_sellSlotContainer.GetChild(i).gameObject);
        }

        private void ClearSlots()
        {
            if (_slotContainer == null) return;
            for (int i = _slotContainer.childCount - 1; i >= 0; i--)
                Destroy(_slotContainer.GetChild(i).gameObject);
            _spawnedRows.Clear();
        }

        // =========================================================
        // 종료
        // =========================================================

        private void OnExit()
        {
            HideAndNotify();
        }

        private void HideAndNotify()
        {
            _onShopExit?.Invoke();
            EnsureCanvasGroup();
            UIAnimationHelper.FadeOut(_canvasGroup);
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        private void OnDestroy()
        {
            if (_leaveButton != null) _leaveButton.onClick.RemoveListener(OnExit);
            if (_buyTabButton != null) _buyTabButton.onClick.RemoveAllListeners();
            if (_sellTabButton != null) _sellTabButton.onClick.RemoveAllListeners();
        }
    }
}
