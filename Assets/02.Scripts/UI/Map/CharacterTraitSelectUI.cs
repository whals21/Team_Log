using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Meta;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 캐릭터별 장착 특성 선택 UI (Phase 8D).
    /// CharacterSelectUI 이후에 표시 — 각 캐릭터에 대해 해금된 특성 중 1개 선택.
    /// </summary>
    public class CharacterTraitSelectUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _rowsContainer;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _hintLabel;

        private CharacterData[] _party;
        private List<CharacterTraitData> _allTraits;
        private MetaSaveData _meta;
        private System.Action<List<TraitSelection>> _onConfirmed;

        // 캐릭터 인덱스 → 선택된 특성 (null = 장착 없음)
        private readonly Dictionary<int, CharacterTraitData> _selections = new();

        private void Awake()
        {
            if (_startButton != null)
                _startButton.onClick.AddListener(OnStartClicked);
            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);
        }

        /// <summary>
        /// 특성 선택 화면 초기화.
        /// </summary>
        public void Initialize(
            CharacterData[] party,
            CharacterTraitData[] allTraits,
            MetaSaveData meta,
            System.Action<List<TraitSelection>> onConfirmed)
        {
            _party = party;
            _meta = meta;
            _onConfirmed = onConfirmed;

            _allTraits = new List<CharacterTraitData>();
            if (allTraits != null)
                _allTraits.AddRange(allTraits);
        }

        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
            _selections.Clear();
            BuildRows();

            if (_titleLabel != null)
                _titleLabel.text = "특성 선택";
            if (_hintLabel != null)
                _hintLabel.text = "각 캐릭터의 장착 특성을 선택하세요";
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void BuildRows()
        {
            if (_rowsContainer == null || _party == null) return;

            // 기존 행 제거
            for (int i = _rowsContainer.childCount - 1; i >= 0; i--)
                Destroy(_rowsContainer.GetChild(i).gameObject);

            for (int i = 0; i < _party.Length; i++)
            {
                if (_party[i] == null) continue;
                CreateRow(_party[i], i);
            }
        }

        private void CreateRow(CharacterData charData, int partyIndex)
        {
            var rowObj = new GameObject($"Row_{charData.CharacterName}");
            rowObj.transform.SetParent(_rowsContainer, false);
            var rowRect = rowObj.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(900, 120);

            var bg = rowObj.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.10f, 0.18f);

            var hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(12, 12, 8, 8);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // 캐릭터 이름 (좌측)
            var nameObj = new GameObject("CharName");
            nameObj.transform.SetParent(rowObj.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.sizeDelta = new Vector2(160, 100);
            var nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.text = charData.CharacterName;
            nameTmp.fontSize = 18;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.color = Color.white;
            nameTmp.alignment = TextAlignmentOptions.Left;
            UIKoreanFont.EnsureFont(nameTmp);

            // 특성 버튼 컨테이너 (우측 — HorizontalLayoutGroup)
            var traitContainerObj = new GameObject("Traits");
            traitContainerObj.transform.SetParent(rowObj.transform, false);
            var traitContainerRect = traitContainerObj.AddComponent<RectTransform>();
            traitContainerRect.sizeDelta = new Vector2(700, 100);
            var traitHlg = traitContainerObj.AddComponent<HorizontalLayoutGroup>();
            traitHlg.padding = new RectOffset(4, 4, 4, 4);
            traitHlg.spacing = 6;
            traitHlg.childAlignment = TextAnchor.MiddleLeft;
            traitHlg.childControlWidth = false;
            traitHlg.childControlHeight = false;

            // 해당 캐릭터 클래스의 사용 가능 특성 필터링
            CharacterTraitData defaultTrait = null;
            var availableTraits = new List<CharacterTraitData>();
            foreach (var trait in _allTraits)
            {
                if (trait == null || trait.TargetClass != charData.Class) continue;
                bool unlocked = trait.IsDefault || MetaProgressionManager.IsTraitUnlocked(_meta, trait.TraitId);
                if (!unlocked) continue;
                if (trait.IsDefault && defaultTrait == null)
                    defaultTrait = trait;
                availableTraits.Add(trait);
            }

            // 기본 특성 자동 선택
            if (defaultTrait != null)
                _selections[partyIndex] = defaultTrait;

            // "없음" 버튼
            CreateTraitButton("(없음)", partyIndex, null, traitContainerObj.transform);

            // 각 특성 버튼
            foreach (var trait in availableTraits)
                CreateTraitButton(trait.DisplayName, partyIndex, trait, traitContainerObj.transform);
        }

        private void CreateTraitButton(string label, int partyIndex, CharacterTraitData trait, Transform parent)
        {
            var btnObj = new GameObject(trait != null ? trait.TraitId : "None");
            btnObj.transform.SetParent(parent, false);
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(160, 80);

            var bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.22f);

            var button = btnObj.AddComponent<Button>();
            button.targetGraphic = bg;
            button.onClick.AddListener(() => OnTraitClicked(partyIndex, trait, bg));

            var tmpObj = new GameObject("Label");
            tmpObj.transform.SetParent(btnObj.transform, false);
            var tmpRect = tmpObj.AddComponent<RectTransform>();
            tmpRect.sizeDelta = new Vector2(150, 70);
            tmpRect.anchorMin = new Vector2(0.5f, 0.5f);
            tmpRect.anchorMax = new Vector2(0.5f, 0.5f);
            tmpRect.pivot = new Vector2(0.5f, 0.5f);
            var tmp = tmpObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 13;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            UIKoreanFont.EnsureFont(tmp);

            // 초기 선택 상태 시각화
            var card = btnObj.AddComponent<TraitButtonCard>();
            card.Initialize(bg, partyIndex, trait, this);
        }

        private void OnTraitClicked(int partyIndex, CharacterTraitData trait, Image bg)
        {
            AudioManager.Instance?.PlayUIConfirm();
            _selections[partyIndex] = trait;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            foreach (var card in _rowsContainer.GetComponentsInChildren<TraitButtonCard>())
            {
                bool selected = _selections.TryGetValue(card.PartyIndex, out var t) && t == card.Trait;
                card.SetSelected(selected);
            }
        }

        private void OnStartClicked()
        {
            AudioManager.Instance?.PlayUIConfirm();
            var result = new List<TraitSelection>();
            for (int i = 0; i < _party.Length; i++)
            {
                if (_party[i] == null) continue;
                _selections.TryGetValue(i, out var trait);
                result.Add(new TraitSelection { Character = _party[i], Trait = trait });
            }
            _onConfirmed?.Invoke(result);
            Hide();
        }

        private void OnBackClicked()
        {
            AudioManager.Instance?.PlayUIConfirm();
            Hide();
            // 뒤로 가기는 호출자에서 처리하지 않음 — 그냥 닫기만.
            // 필요시 별도 콜백 추가.
        }

        private void OnDestroy()
        {
            if (_startButton != null)
                _startButton.onClick.RemoveListener(OnStartClicked);
            if (_backButton != null)
                _backButton.onClick.RemoveListener(OnBackClicked);
        }

        /// <summary>특성 선택 결과 한 행</summary>
        public class TraitSelection
        {
            public CharacterData Character;
            public CharacterTraitData Trait;
        }

        /// <summary>특성 버튼 시각화용 — 선택 상태 색상 변경</summary>
        private class TraitButtonCard : MonoBehaviour
        {
            private Image _bg;
            private int _partyIndex;
            private CharacterTraitData _trait;
            private CharacterTraitSelectUI _parent;
            private bool _selected;

            public int PartyIndex => _partyIndex;
            public CharacterTraitData Trait => _trait;

            public void Initialize(Image bg, int partyIndex, CharacterTraitData trait, CharacterTraitSelectUI parent)
            {
                _bg = bg;
                _partyIndex = partyIndex;
                _trait = trait;
                _parent = parent;
                _selected = false;
            }

            public void SetSelected(bool selected)
            {
                _selected = selected;
                _bg.color = selected
                    ? new Color(0.2f, 0.45f, 0.7f)
                    : new Color(0.15f, 0.15f, 0.22f);
            }
        }
    }
}
