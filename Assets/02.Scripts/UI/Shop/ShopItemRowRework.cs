using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Shop;
using TeamLog.UI;

namespace TeamLog.UI.Shop
{
    /// <summary>
    /// ★ Stained Glass Shop UI — 상점 슬롯 1개 행.
    /// ShopReworkView가 ShopSlot마다 인스턴스화.
    ///
    /// 자식 구조 (Builder가 생성):
    /// - ShopItemRow (Image 배경 + Button + HLG)
    ///   - TypeBar (Image, 상단 2px 띠 — Augment=호박 / Relic=황금)
    ///   - SlotTop (HLG)
    ///     - IconFrame (Image + Icon 자식)
    ///     - NameAndDesc (VLG: Name TMP + Desc TMP)
    ///     - Price (TMP 우측)
    ///   - RarityBadge (TMP — 상단 우측)
    ///   - SoldOverlay (중앙 — 비활성 시 노출, "SOLD" 텍스트)
    ///
    /// CLAUDE.md 가드레일 #17 준수 — 단일 MonoBehaviour.
    /// </summary>
    public class ShopItemRowRework : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _typeBar;
        [SerializeField] private Button _button;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _iconFallbackText;  // Icon sprite 없을 때 기호
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private TextMeshProUGUI _rarityBadge;
        [SerializeField] private Image _rarityBadgeBackground;
        [SerializeField] private GameObject _soldOverlay;
        [SerializeField] private GameObject _cursedWarning;

        private ShopSlot _slot;
        private bool _autoBound;

        private static readonly Color CantAffordColor = new Color(0.85f, 0.35f, 0.35f);
        private static readonly Color NormalPriceColor = new Color(0.95f, 0.78f, 0.31f); // gold-light

        private void Awake()
        {
            AutoBindMissingFields();
            if (_button != null)
                _button.onClick.AddListener(OnClickInternal);
        }

        private void AutoBindMissingFields()
        {
            if (_autoBound) return;

            if (_background == null) _background = GetComponent<Image>();
            if (_button == null) _button = GetComponent<Button>();

            var root = transform;
            if (_typeBar == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "TypeBar");
                if (go != null) _typeBar = go.GetComponent<Image>();
            }
            if (_iconImage == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "IconFrame");
                if (go != null) _iconImage = go.GetComponent<Image>();
            }
            if (_iconFallbackText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "IconFrame");
                if (go != null) _iconFallbackText = go.GetComponentInChildren<TextMeshProUGUI>(true);
            }
            if (_nameText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "Name");
                if (go != null) _nameText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_descText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "Desc");
                if (go != null) _descText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_priceText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "Price");
                if (go != null) _priceText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_rarityBadge == null || _rarityBadgeBackground == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "RarityBadge");
                if (go != null)
                {
                    if (_rarityBadge == null) _rarityBadge = go.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (_rarityBadgeBackground == null) _rarityBadgeBackground = go.GetComponent<Image>();
                }
            }
            if (_soldOverlay == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "SoldOverlay");
                if (go != null) _soldOverlay = go;
            }
            if (_cursedWarning == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "CursedWarning");
                if (go != null) _cursedWarning = go;
            }

            _autoBound = true;
        }

        /// <summary>
        /// 슬롯 데이터 바인딩 (최초 1회).
        /// </summary>
        public void Setup(ShopSlot slot, System.Action<ShopSlot> onClick)
        {
            AutoBindMissingFields();
            _slot = slot;
            _onClickCallback = onClick;
            RenderStatic();
            UpdateVisual(int.MaxValue);  // 초기 — 골드 무한으로 시작, View가 UpdateVisual로 갱신
        }

        private System.Action<ShopSlot> _onClickCallback;

        /// <summary>
        /// 골드 변화 / 구매 완료 시 갱신.
        /// </summary>
        public void UpdateVisual(int currentGold)
        {
            if (_slot == null) return;

            bool sold = _slot.IsSold;
            bool canAfford = !sold && currentGold >= _slot.Price;

            // 가격 색상
            if (_priceText != null)
            {
                _priceText.text = $"{_slot.Price} G";
                _priceText.color = sold ? new Color(0.5f, 0.5f, 0.5f, 0.6f)
                                       : (canAfford ? NormalPriceColor : CantAffordColor);
            }

            // 버튼 상태
            if (_button != null)
                _button.interactable = canAfford;

            // SOLD 오버레이
            if (_soldOverlay != null)
                _soldOverlay.SetActive(sold);

            // 전체 알파 — sold 시 흐림
            if (_background != null)
            {
                var c = _background.color;
                c.a = sold ? 0.3f : 1f;
                _background.color = c;
            }
        }

        /// <summary>
        /// 정적 렌더링 — 슬롯 데이터가 바뀌지 않는 부분 (TypeBar/Icon/Name/Desc/Rarity).
        /// </summary>
        private void RenderStatic()
        {
            if (_slot == null) return;

            // TypeBar — Augment(호박) / Relic(황금)
            var typeVisual = ShopSlotTypeStyle.Get(_slot.ContentType);
            if (_typeBar != null)
                _typeBar.color = typeVisual.AccentColor;

            // Icon — sprite 우선, 없으면 기호
            if (_iconImage != null)
            {
                bool hasSprite = _slot.Icon != null;
                _iconImage.enabled = hasSprite;
                if (hasSprite) _iconImage.sprite = _slot.Icon;
            }
            if (_iconFallbackText != null)
            {
                bool useFallback = _slot.Icon == null;
                _iconFallbackText.gameObject.SetActive(useFallback);
                if (useFallback) _iconFallbackText.text = typeVisual.DefaultEmblem;
            }

            // Name / Desc
            if (_nameText != null) _nameText.text = _slot.Name;
            if (_descText != null) _descText.text = _slot.Desc;

            // Rarity 배지
            var rarity = ShopRarityStyle.EstimateFromSlot(_slot);
            var rarityVisual = ShopRarityStyle.Get(rarity);
            if (_rarityBadge != null)
            {
                _rarityBadge.text = $"{rarityVisual.EmblemSymbol}  {rarityVisual.DisplayName}";
                _rarityBadge.color = rarityVisual.TextColor;
            }
            if (_rarityBadgeBackground != null)
            {
                var c = rarityVisual.BorderColor;
                c.a = 0.25f;
                _rarityBadgeBackground.color = c;
            }

            // 저주 경고
            if (_cursedWarning != null)
                _cursedWarning.SetActive(rarityVisual.IsCursed);
        }

        private void OnClickInternal()
        {
            if (_slot == null) return;
            _onClickCallback?.Invoke(_slot);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClickInternal);
        }
    }
}
