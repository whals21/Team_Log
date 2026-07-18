using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.UI;

namespace TeamLog.UI.PartySelection
{
    /// <summary>
    /// 특성 카드 (UI-B.3) — 웹 목업의 3열 특성 카드 유니티 재현.
    /// 특성 이름 + BASE/META 태그 + 상세 설명 + 잠금 시 해금 조건.
    /// 선택 상태 시각적 강조 (골드 테두리 + ✦). 클릭 이벤트.
    ///
    /// 레이아웃:
    /// TraitOption (Image 배경 — 9-slice Slate)
    /// ├── Head (HorizontalLayoutGroup)
    /// │   ├── Name (TMP — Cinzel Medium)
    /// │   └── Tag (TMP — 작은 배지, "BASE" 골드 / "META" 핏빛)
    /// ├── DescText (TMP — 본문)
    /// └── UnlockRow (HorizontalLayoutGroup — 잠금 시만)
    ///     ├── LockIcon (TMP — "🔒")
    ///     └── UnlockText (TMP — "기억 200" / "어센션 10 클리어")
    /// </summary>
    public class TraitDetailCard : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _background;
        [SerializeField] private Button _button;
        [SerializeField] private Image _selectedHighlight;  // 선택 시 ✦ 아이콘

        [Header("Sprites")]
        [SerializeField] private Sprite _normalSprite;     // SlatePanel
        [SerializeField] private Sprite _hoverSprite;       // SlatePanelLight
        [SerializeField] private Sprite _selectedSprite;    // SlatePanelLight (골드 테두리 효과)

        [Header("Content")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _tagText;
        [SerializeField] private TextMeshProUGUI _descText;
        [SerializeField] private GameObject _unlockRow;
        [SerializeField] private TextMeshProUGUI _unlockText;

        // 상태
        private CharacterTraitData _trait;
        private bool _locked;
        private bool _selected;
        private int _index;
        private Action<int> _onClicked;

        /// <summary>
        /// 특성 카드 초기화.
        /// </summary>
        /// <param name="trait">특성 데이터</param>
        /// <param name="index">캐릭터 특성 슬롯 인덱스 (0=기본, 1/2=메타)</param>
        /// <param name="locked">메타 해금 안 됨</param>
        /// <param name="selected">현재 선택됨</param>
        /// <param name="onClicked">클릭 콜백 (인자로 index 전달)</param>
        public void Initialize(CharacterTraitData trait, int index, bool locked, bool selected,
            Action<int> onClicked)
        {
            _trait = trait;
            _index = index;
            _locked = locked;
            _selected = selected;
            _onClicked = onClicked;

            Render();

            if (_button != null)
            {
                _button.interactable = !locked;
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnClicked);
            }
        }

        private void Render()
        {
            var palette = UIPalette.Default;

            if (_trait == null)
            {
                if (_nameText != null) _nameText.text = "(특성 없음)";
                if (_descText != null) _descText.text = "";
                if (_tagText != null) _tagText.text = "";
                if (_unlockRow != null) _unlockRow.SetActive(false);
                return;
            }

            // 이름
            if (_nameText != null)
            {
                _nameText.text = _trait.DisplayName ?? _trait.TraitId;
                _nameText.color = palette.DFGoldL;
            }

            // 태그 (BASE/META)
            bool isMeta = !_trait.IsDefault;
            if (_tagText != null)
            {
                _tagText.text = isMeta ? "META" : "BASE";
                _tagText.color = isMeta ? palette.DFBloodL : palette.DFGold;
            }

            // 설명
            if (_descText != null)
            {
                _descText.text = _trait.Description ?? "";
                _descText.color = _locked ? palette.DFInkFaint : palette.DFInk;
            }

            // 잠금 행
            if (_unlockRow != null)
            {
                bool showUnlock = _locked;
                _unlockRow.SetActive(showUnlock);
                if (showUnlock && _unlockText != null)
                {
                    _unlockText.text = ResolveUnlockText(_trait);
                    _unlockText.color = palette.DFBloodL;
                }
            }

            // 선택 강조
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            var palette = UIPalette.Default;

            // 배경 Sprite
            if (_background != null)
            {
                if (_selected && _selectedSprite != null)
                    _background.sprite = _selectedSprite;
                else if (_hoverSprite != null && IsPointerOver())
                    _background.sprite = _hoverSprite;
                else
                    _background.sprite = _normalSprite;

                // 선택 상태 — 골드 테두리 효과 (색상 오버레이)
                _background.color = _selected ? new Color(1.2f, 1.05f, 0.7f, 1f) : Color.white;
            }

            // ✦ 강조 아이콘
            if (_selectedHighlight != null)
            {
                _selectedHighlight.gameObject.SetActive(_selected);
                if (_selected) _selectedHighlight.color = palette.DFGoldL;
            }
        }

        private bool IsPointerOver()
        {
            // 간단히 false 반환 — hover 효과는 버튼 transition으로 처리
            return false;
        }

        private string ResolveUnlockText(CharacterTraitData trait)
        {
            if (trait.SoulUnlockCost > 0)
                return $"영혼 {trait.SoulUnlockCost} 필요";
            if (trait.UnlockCost > 0)
                return $"기억 {trait.UnlockCost} 필요";
            if (!trait.IsDefault)
                return "메타 해금 필요";
            return "잠김";
        }

        private void OnClicked()
        {
            if (_locked) return;
            _onClicked?.Invoke(_index);
        }

        /// <summary>
        /// 선택 상태 업데이트 (외부에서 호출).
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (_selected == selected) return;
            _selected = selected;
            UpdateVisualState();
        }
    }
}
