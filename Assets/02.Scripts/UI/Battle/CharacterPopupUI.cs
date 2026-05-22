using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 캐릭터 상세 팝업 UI (좌클릭 시 표시)
    /// </summary>
    public class CharacterPopupUI : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Image _portraitImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _classText;
        [SerializeField] private Button _closeButton;

        [Header("HP")]
        [SerializeField] private Image _hpFillImage;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private Image _hpBarBg;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _atkText;
        [SerializeField] private TextMeshProUGUI _defText;

        [Header("Tabs")]
        [SerializeField] private Button _tabSkillButton;
        [SerializeField] private Button _tabStatusButton;
        [SerializeField] private GameObject _tabSkillIndicator;
        [SerializeField] private GameObject _tabStatusIndicator;

        [Header("Content")]
        [SerializeField] private GameObject _skillContent;
        [SerializeField] private Transform _skillEntryContainer;
        [SerializeField] private GameObject _skillEntryPrefab;
        [SerializeField] private GameObject _statusContent;
        [SerializeField] private Transform _statusEntryContainer;
        [SerializeField] private GameObject _statusEntryPrefab;

        [Header("Background")]
        [SerializeField] private Button _backgroundButton;

        [Header("Colors")]
        [SerializeField] private Color _hpColor = new Color(0.15f, 0.68f, 0.38f);
        [SerializeField] private Color _hpLowColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color _accentYellow = new Color(0.96f, 0.82f, 0.25f);

        private Character _currentCharacter;
        private List<GameObject> _spawnedEntries = new List<GameObject>();
        private int _currentTab;

        private void Awake()
        {
            // Auto-wire: Inspector에 할당되지 않은 필드를 자동으로 찾아 연결
            if (_portraitImage == null) _portraitImage = FindComponent<Image>("Panel/Header/Portrait");
            if (_nameText == null) _nameText = FindComponent<TextMeshProUGUI>("Panel/Header/Name");
            if (_classText == null) _classText = FindComponent<TextMeshProUGUI>("Panel/Header/Class");
            if (_closeButton == null) _closeButton = FindComponent<Button>("Panel/Header/CloseBtn");
            if (_hpFillImage == null) _hpFillImage = FindComponent<Image>("Panel/HPArea/HPBarBg/Fill");
            if (_hpText == null) _hpText = FindComponent<TextMeshProUGUI>("Panel/HPArea/HPText");
            if (_hpBarBg == null) _hpBarBg = FindComponent<Image>("Panel/HPArea/HPBarBg");
            if (_atkText == null) _atkText = FindComponent<TextMeshProUGUI>("Panel/StatsArea/ATK/T");
            if (_defText == null) _defText = FindComponent<TextMeshProUGUI>("Panel/StatsArea/DEF/T");
            if (_tabSkillButton == null) _tabSkillButton = FindComponent<Button>("Panel/TabArea/TabSkill");
            if (_tabStatusButton == null) _tabStatusButton = FindComponent<Button>("Panel/TabArea/TabStatus");
            if (_tabSkillIndicator == null) _tabSkillIndicator = FindChild("Panel/TabArea/TabSkill/Indicator");
            if (_tabStatusIndicator == null) _tabStatusIndicator = FindChild("Panel/TabArea/TabStatus/Indicator");
            if (_skillContent == null) _skillContent = FindChild("Panel/SkillContent");
            if (_skillEntryContainer == null) _skillEntryContainer = FindChild("Panel/SkillContent")?.transform;
            if (_statusContent == null) _statusContent = FindChild("Panel/StatusContent");
            if (_statusEntryContainer == null) _statusEntryContainer = FindChild("Panel/StatusContent")?.transform;
            if (_backgroundButton == null) _backgroundButton = GetComponent<Button>();

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);

            if (_backgroundButton != null)
                _backgroundButton.onClick.AddListener(Hide);

            if (_tabSkillButton != null)
                _tabSkillButton.onClick.AddListener(() => SwitchTab(0));

            if (_tabStatusButton != null)
                _tabStatusButton.onClick.AddListener(() => SwitchTab(1));
        }

        private T FindComponent<T>(string path) where T : Component
        {
            var t = transform.Find(path);
            return t != null ? t.GetComponent<T>() : null;
        }

        private GameObject FindChild(string path)
        {
            var t = transform.Find(path);
            return t != null ? t.gameObject : null;
        }

        /// <summary>
        /// 샘플 데이터로 팝업 표시 (Character 객체 없이)
        /// </summary>
        public void ShowSample(string name, string hp)
        {
            gameObject.SetActive(true);

            if (_nameText != null) _nameText.text = name;
            if (_classText != null) _classText.text = "전사";
            if (_hpText != null) _hpText.text = "HP " + hp;
            if (_atkText != null) _atkText.text = "ATK 10";
            if (_defText != null) _defText.text = "DEF 5";

            if (_hpFillImage != null)
            {
                _hpFillImage.rectTransform.anchorMax = new Vector2(1f, 1f);
                _hpFillImage.color = _hpColor;
            }

            SwitchTab(0);
        }

        public void Show(Character character)
        {
            _currentCharacter = character;
            gameObject.SetActive(true);

            UpdateHeader();
            UpdateHP();
            UpdateStats();
            SwitchTab(0);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _currentCharacter = null;
        }

        private void UpdateHeader()
        {
            if (_nameText != null)
                _nameText.text = _currentCharacter.Name;

            if (_classText != null)
                _classText.text = GetClassLabel(_currentCharacter.Class);
        }

        private void UpdateHP()
        {
            int current = _currentCharacter.Health.CurrentHP;
            int max = _currentCharacter.Health.MaxHP;
            float ratio = max > 0 ? (float)current / max : 0f;

            if (_hpText != null)
                _hpText.text = $"HP {current} / {max}";

            if (_hpFillImage != null)
            {
                _hpFillImage.rectTransform.anchorMax = new Vector2(ratio, 1f);
                _hpFillImage.color = ratio <= 0.3f ? _hpLowColor : _hpColor;
            }
        }

        private void UpdateStats()
        {
            if (_atkText != null)
                _atkText.text = $"ATK {_currentCharacter.Stats.GetStat(StatType.ATK)}";

            if (_defText != null)
                _defText.text = $"DEF {_currentCharacter.Stats.GetStat(StatType.DEF)}";
        }

        private void SwitchTab(int tabIndex)
        {
            _currentTab = tabIndex;

            if (_tabSkillIndicator != null)
                _tabSkillIndicator.SetActive(tabIndex == 0);
            if (_tabStatusIndicator != null)
                _tabStatusIndicator.SetActive(tabIndex == 1);

            if (_skillContent != null)
                _skillContent.SetActive(tabIndex == 0);
            if (_statusContent != null)
                _statusContent.SetActive(tabIndex == 1);

            if (tabIndex == 0)
                PopulateSkills();
            else
                PopulateStatusEffects();
        }

        private void PopulateSkills()
        {
            ClearEntries();

            if (_currentCharacter == null || _skillEntryContainer == null) return;

            var skills = _currentCharacter.SkillInventory.Skills;
            for (int i = 0; i < skills.Count; i++)
            {
                var entryObj = CreateEntry(_skillEntryPrefab, _skillEntryContainer);
                if (entryObj != null)
                {
                    var texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
                    if (texts.Length > 0)
                        texts[0].text = $"{skills[i].SkillName} ({skills[i].Type})";
                }
            }
        }

        private void PopulateStatusEffects()
        {
            ClearEntries();

            if (_currentCharacter == null || _statusEntryContainer == null) return;

            var effects = _currentCharacter.StatusEffects.GetAllEffects();
            if (effects == null) return;

            foreach (var effect in effects)
            {
                var entryObj = CreateEntry(_statusEntryPrefab, _statusEntryContainer);
                if (entryObj != null)
                {
                    var texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
                    if (texts.Length >= 2)
                    {
                        texts[0].text = GetEffectLabel(effect.Type);
                        texts[1].text = $"{effect.Value} ({effect.RemainingTurns}턴)";
                    }
                }
            }
        }

        private GameObject CreateEntry(GameObject prefab, Transform container)
        {
            if (prefab == null)
            {
                // 폴백: 빈 객체에 TMP 텍스트 생성
                var go = new GameObject("Entry");
                go.transform.SetParent(container, false);
                var rect = go.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(0, 60);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 14;
                tmp.alignment = TextAlignmentOptions.Left;
                return go;
            }

            var obj = Instantiate(prefab, container);
            _spawnedEntries.Add(obj);
            return obj;
        }

        private void ClearEntries()
        {
            foreach (var entry in _spawnedEntries)
                if (entry != null) Destroy(entry);
            _spawnedEntries.Clear();
        }

        private string GetClassLabel(CharacterClass cls) => cls switch
        {
            CharacterClass.Warrior => "전사",
            CharacterClass.Mage => "마법사",
            CharacterClass.Healer => "치유사",
            CharacterClass.Rogue => "도적",
            _ => cls.ToString()
        };

        private string GetEffectLabel(StatusEffectType type) => type switch
        {
            StatusEffectType.Poison => "독",
            StatusEffectType.Burn => "화상",
            StatusEffectType.Stun => "기절",
            StatusEffectType.Freeze => "빙결",
            StatusEffectType.Sleep => "수면",
            StatusEffectType.Bleed => "출혈",
            StatusEffectType.DefenseUp => "방어 증가",
            StatusEffectType.DefenseDown => "방어 감소",
            StatusEffectType.AttackUp => "공격 증가",
            StatusEffectType.AttackDown => "공격 감소",
            StatusEffectType.Regeneration => "재생",
            StatusEffectType.Shield => "보호막",
            _ => type.ToString()
        };
    }
}
