using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Map;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 런 시작 캐릭터 선택 UI — 잠금해제된 캐릭터 중 3~4명 선택
    /// </summary>
    public class CharacterSelectUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _characterContainer;
        [SerializeField] private Button _startButton;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _countLabel;

        private readonly HashSet<int> _selectedIndices = new();
        private List<CharacterData> _availableCharacters;
        private System.Action<List<CharacterData>> _onConfirmed;
        private MetaSaveData _meta;

        private const int MinPartySize = 3;
        private const int MaxPartySize = 4;

        private void Awake()
        {
            if (_startButton != null)
                _startButton.onClick.AddListener(OnStartClicked);
        }

        /// <summary>
        /// 캐릭터 선택 UI 초기화
        /// </summary>
        public void Initialize(CharacterData[] allCharacters, MetaSaveData meta,
            System.Action<List<CharacterData>> onConfirmed)
        {
            _meta = meta;
            _onConfirmed = onConfirmed;

            // 잠금해제된 캐릭터 필터링
            _availableCharacters = new List<CharacterData>();
            foreach (var c in allCharacters)
            {
                if (c == null) continue;
                if (c.IsDefault || IsUnlocked(c))
                    _availableCharacters.Add(c);
            }

            _startButton.interactable = false;
        }

        /// <summary>
        /// 캐릭터 선택 화면 표시
        /// </summary>
        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
            _selectedIndices.Clear();

            if (_titleLabel != null)
                _titleLabel.text = "파티 구성";
            UpdateCountLabel();

            BuildCharacterCards();
        }

        private bool IsUnlocked(CharacterData c)
        {
            if (_meta == null) return false;
            return _meta.UnlockedCharacterIds.Contains(c.CharacterName);
        }

        private void BuildCharacterCards()
        {
            if (_characterContainer == null) return;

            // 기존 카드 제거
            for (int i = _characterContainer.childCount - 1; i >= 0; i--)
                Destroy(_characterContainer.GetChild(i).gameObject);

            for (int i = 0; i < _availableCharacters.Count; i++)
            {
                var charData = _availableCharacters[i];
                var idx = i;
                CreateCharacterCard(charData, idx);
            }
        }

        private void CreateCharacterCard(CharacterData charData, int index)
        {
            var cardObj = new GameObject($"Char_{charData.CharacterName}");
            cardObj.transform.SetParent(_characterContainer, false);
            var cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(180, 220);

            var bg = cardObj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.22f);

            var button = cardObj.AddComponent<Button>();
            button.targetGraphic = bg;
            button.onClick.AddListener(() => OnCharacterClicked(index));

            // 세로 레이아웃
            var vlg = cardObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // 클래스 이니셜
            var classObj = new GameObject("Class");
            classObj.transform.SetParent(cardObj.transform, false);
            var classTmp = classObj.AddComponent<TextMeshProUGUI>();
            classTmp.text = GetClassInitial(charData.Class);
            classTmp.fontSize = 32;
            classTmp.fontStyle = FontStyles.Bold;
            classTmp.color = Color.white;
            classTmp.alignment = TextAlignmentOptions.Center;
            UIKoreanFont.EnsureFont(classTmp);

            // 이름
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(cardObj.transform, false);
            var nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.text = charData.CharacterName;
            nameTmp.fontSize = 16;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.color = Color.white;
            nameTmp.alignment = TextAlignmentOptions.Center;
            UIKoreanFont.EnsureFont(nameTmp);

            // 스탯
            var statObj = new GameObject("Stats");
            statObj.transform.SetParent(cardObj.transform, false);
            var statTmp = statObj.AddComponent<TextMeshProUGUI>();
            statTmp.text = $"HP:{charData.BaseHP} ATK:{charData.BaseATK}\nDEF:{charData.BaseDEF}";
            statTmp.fontSize = 12;
            statTmp.color = new Color(0.7f, 0.7f, 0.7f);
            statTmp.alignment = TextAlignmentOptions.Center;
            UIKoreanFont.EnsureFont(statTmp);

            // 스킬 목록
            var skillObj = new GameObject("Skills");
            skillObj.transform.SetParent(cardObj.transform, false);
            var skillTmp = skillObj.AddComponent<TextMeshProUGUI>();
            var skillNames = new List<string>();
            foreach (var s in charData.Skills)
                skillNames.Add(s.SkillName);
            skillTmp.text = string.Join(", ", skillNames);
            skillTmp.fontSize = 11;
            skillTmp.color = new Color(0.6f, 0.8f, 1f);
            skillTmp.alignment = TextAlignmentOptions.Center;
            skillTmp.enableWordWrapping = true;
            UIKoreanFont.EnsureFont(skillTmp);

            // 선택 표시용 컴포넌트
            var selectIndicator = cardObj.AddComponent<CharacterSelectCard>();
            selectIndicator.Initialize(bg, button, index, this);
        }

        private void OnCharacterClicked(int index)
        {
            AudioManager.Instance?.PlayUIConfirm();

            if (_selectedIndices.Contains(index))
            {
                _selectedIndices.Remove(index);
            }
            else
            {
                if (_selectedIndices.Count >= MaxPartySize) return;
                _selectedIndices.Add(index);
            }

            UpdateVisuals();
            UpdateCountLabel();
            _startButton.interactable = _selectedIndices.Count >= MinPartySize;
        }

        internal void UpdateVisuals()
        {
            foreach (var card in _characterContainer.GetComponentsInChildren<CharacterSelectCard>())
            {
                bool selected = _selectedIndices.Contains(card.Index);
                card.SetSelected(selected);
            }
        }

        private void UpdateCountLabel()
        {
            if (_countLabel != null)
                _countLabel.text = $"선택: {_selectedIndices.Count}/{MaxPartySize} (최소 {MinPartySize}명)";
        }

        private void OnStartClicked()
        {
            if (_selectedIndices.Count < MinPartySize) return;

            AudioManager.Instance?.PlayUIConfirm();

            var selected = new List<CharacterData>();
            foreach (var idx in _selectedIndices)
                selected.Add(_availableCharacters[idx]);

            _onConfirmed?.Invoke(selected);

            if (_panel != null) _panel.SetActive(false);
        }

        private static string GetClassInitial(CharacterClass cls)
        {
            return cls switch
            {
                CharacterClass.Warrior => "전",
                CharacterClass.Mage => "마",
                CharacterClass.Healer => "힐",
                CharacterClass.Rogue => "도",
                CharacterClass.Archer => "궁",
                CharacterClass.Necromancer => "사",
                CharacterClass.Alchemist => "연",
                CharacterClass.Bard => "음",
                _ => "?"
            };
        }

        private void OnDestroy()
        {
            if (_startButton != null)
                _startButton.onClick.RemoveListener(OnStartClicked);
        }

        /// <summary>
        /// 선택 카드 내부 컴포넌트 — 선택 상태 시각화
        /// </summary>
        private class CharacterSelectCard : MonoBehaviour
        {
            private Image _bg;
            private Button _button;
            private int _index;
            private CharacterSelectUI _parent;
            private bool _selected;

            public int Index => _index;

            public void Initialize(Image bg, Button button, int index, CharacterSelectUI parent)
            {
                _bg = bg;
                _button = button;
                _index = index;
                _parent = parent;
            }

            public void SetSelected(bool selected)
            {
                _selected = selected;
                _bg.color = selected
                    ? new Color(0.2f, 0.4f, 0.7f)
                    : new Color(0.12f, 0.12f, 0.22f);
            }
        }
    }
}
