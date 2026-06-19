using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Meta;
using TeamLog.Reward;
using TeamLog.UI;

namespace TeamLog.UI.Meta
{
    /// <summary>
    /// 메타 상점 UI (Phase 8D) — 3탭 구조 (특성/유물/강화).
    /// 타이틀에서 진입. 메모리/영혼으로 해금.
    /// </summary>
    public class MetaShopUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _closeButton;

        [Header("Tabs")]
        [SerializeField] private Button _traitsTabButton;
        [SerializeField] private Button _relicsTabButton;
        [SerializeField] private Button _upgradesTabButton;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _memoryLabel;
        [SerializeField] private TextMeshProUGUI _soulLabel;

        [Header("Content")]
        [SerializeField] private Transform _contentContainer;
        [SerializeField] private GameObject _itemCardPrefab;     // 옵셔널 — 없으면 코드 생성

        [Header("Data Pools")]
        [SerializeField] private CharacterTraitData[] _allTraits;
        [SerializeField] private MetaUpgradeData[] _allUpgrades;
        [SerializeField] private RelicData[] _allRelics;        // 표시용 메타데이터 (해금 여부 확인)

        private TabType _currentTab = TabType.Traits;

        private enum TabType { Traits, Relics, Upgrades }

        private void Awake()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnCloseClicked);
            if (_traitsTabButton != null)
                _traitsTabButton.onClick.AddListener(() => SwitchTab(TabType.Traits));
            if (_relicsTabButton != null)
                _relicsTabButton.onClick.AddListener(() => SwitchTab(TabType.Relics));
            if (_upgradesTabButton != null)
                _upgradesTabButton.onClick.AddListener(() => SwitchTab(TabType.Upgrades));
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(OnCloseClicked);
            if (_traitsTabButton != null)
                _traitsTabButton.onClick.RemoveAllListeners();
            if (_relicsTabButton != null)
                _relicsTabButton.onClick.RemoveAllListeners();
            if (_upgradesTabButton != null)
                _upgradesTabButton.onClick.RemoveAllListeners();
        }

        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
            RefreshBalance();
            SwitchTab(TabType.Traits);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void SwitchTab(TabType tab)
        {
            _currentTab = tab;
            ClearContent();
            switch (tab)
            {
                case TabType.Traits: BuildTraitsTab(); break;
                case TabType.Relics: BuildRelicsTab(); break;
                case TabType.Upgrades: BuildUpgradesTab(); break;
            }
        }

        // ── 탭 내용 빌드 ──

        private void BuildTraitsTab()
        {
            if (_allTraits == null) return;
            foreach (var trait in _allTraits)
            {
                if (trait == null) continue;
                bool unlocked = trait.IsDefault || MetaProgressionManager.IsTraitUnlocked(SaveManager.Meta, trait.TraitId);
                CreateCard(
                    title: $"{trait.DisplayName} ({GetClassLabel(trait.TargetClass)})",
                    desc: trait.Description,
                    costText: unlocked ? "해금됨" : CostText(trait.UnlockCost, trait.SoulUnlockCost),
                    interactable: !unlocked && CanAfford(trait.UnlockCost, trait.SoulUnlockCost),
                    buttonText: unlocked ? "완료" : "해금",
                    onClick: () => OnPurchaseTrait(trait));
            }
        }

        private void BuildRelicsTab()
        {
            if (_allRelics == null) return;
            // RelicUnlock 메타 강화가 있는 유물만 표시 (기본 16종은 제외)
            var unlockMap = new Dictionary<string, MetaUpgradeData>();
            if (_allUpgrades != null)
            {
                foreach (var up in _allUpgrades)
                {
                    if (up != null && up.Type == MetaUpgradeType.RelicUnlock && !string.IsNullOrEmpty(up.TargetRelicId))
                        unlockMap[up.TargetRelicId] = up;
                }
            }

            foreach (var relic in _allRelics)
            {
                if (relic == null) continue;
                string fileName = relic.name;
                if (!unlockMap.ContainsKey(fileName)) continue; // 기본 16종은 표시 제외

                var upgrade = unlockMap[fileName];
                bool unlocked = MetaProgressionManager.IsRelicUnlocked(SaveManager.Meta, fileName);
                CreateCard(
                    title: relic.RelicName,
                    desc: relic.Description,
                    costText: unlocked ? "해금됨" : CostText(upgrade.MemoryCost, upgrade.SoulCost),
                    interactable: !unlocked && CanAfford(upgrade.MemoryCost, upgrade.SoulCost),
                    buttonText: unlocked ? "완료" : "해금",
                    onClick: () => OnPurchaseUpgrade(upgrade, fileName));
            }
        }

        private void BuildUpgradesTab()
        {
            if (_allUpgrades == null) return;
            foreach (var upgrade in _allUpgrades)
            {
                if (upgrade == null) continue;
                if (upgrade.Type == MetaUpgradeType.RelicUnlock) continue; // 유물 탭에서 표시

                bool purchased = MetaProgressionManager.IsUpgradePurchased(SaveManager.Meta, upgrade.UpgradeId);
                CreateCard(
                    title: upgrade.DisplayName,
                    desc: upgrade.Description,
                    costText: purchased ? "구매완료" : CostText(upgrade.MemoryCost, upgrade.SoulCost),
                    interactable: !purchased && CanAfford(upgrade.MemoryCost, upgrade.SoulCost),
                    buttonText: purchased ? "완료" : "구매",
                    onClick: () => OnPurchaseGlobalUpgrade(upgrade));
            }
        }

        // ── 카드 생성 ──

        private void CreateCard(string title, string desc, string costText,
            bool interactable, string buttonText, System.Action onClick)
        {
            if (_contentContainer == null) return;

            GameObject cardObj;
            if (_itemCardPrefab != null)
            {
                cardObj = Instantiate(_itemCardPrefab, _contentContainer);
            }
            else
            {
                cardObj = new GameObject("Card");
                cardObj.transform.SetParent(_contentContainer, false);
                var cardRect = cardObj.AddComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(820, 110);
                var bg = cardObj.AddComponent<Image>();
                bg.color = new Color(0.10f, 0.10f, 0.16f);

                var hlg = cardObj.AddComponent<HorizontalLayoutGroup>();
                hlg.padding = new RectOffset(12, 12, 8, 8);
                hlg.spacing = 8;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;

                // 텍스트 블록 (좌측)
                var textObj = new GameObject("Texts");
                textObj.transform.SetParent(cardObj.transform, false);
                var textRect = textObj.AddComponent<RectTransform>();
                textRect.sizeDelta = new Vector2(600, 90);
                var vlg = textObj.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 4;
                vlg.childAlignment = TextAnchor.UpperLeft;
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;

                var titleTmp = CreateText("Title", title, 16, Color.white, textObj.transform, 600, 26);
                titleTmp.fontStyle = FontStyles.Bold;
                CreateText("Desc", desc, 13, new Color(0.75f, 0.75f, 0.75f), textObj.transform, 600, 40);
                CreateText("Cost", costText, 13, new Color(1f, 0.85f, 0.4f), textObj.transform, 600, 18);
            }

            // 구매 버튼 (우측) — 프리팹 사용 여부와 무관하게 항상 직접 생성
            var buyBtnObj = new GameObject("BuyButton");
            buyBtnObj.transform.SetParent(cardObj.transform, false);
            var buyRect = buyBtnObj.AddComponent<RectTransform>();
            buyRect.sizeDelta = new Vector2(140, 80);
            var buyBg = buyBtnObj.AddComponent<Image>();
            buyBg.color = interactable ? new Color(0.2f, 0.4f, 0.7f) : new Color(0.18f, 0.18f, 0.22f);

            var button = buyBtnObj.AddComponent<Button>();
            button.targetGraphic = buyBg;
            button.interactable = interactable;
            button.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayUIConfirm();
                onClick?.Invoke();
            });

            var btnLabel = CreateText("Label", buttonText, 14, Color.white, buyBtnObj.transform, 130, 60);
            btnLabel.alignment = TextAlignmentOptions.Center;
        }

        private TextMeshProUGUI CreateText(string name, string text, int fontSize, Color color,
            Transform parent, int width, int height)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.enableWordWrapping = true;
            UIKoreanFont.EnsureFont(tmp);
            return tmp;
        }

        // ── 구매 처리 ──

        private void OnPurchaseTrait(CharacterTraitData trait)
        {
            if (trait == null) return;
            var meta = SaveManager.Meta;
            if (MetaProgressionManager.TryPurchaseTrait(meta, trait.TraitId, trait.UnlockCost, trait.SoulUnlockCost))
            {
                SaveManager.SaveMeta();
                RefreshBalance();
                SwitchTab(_currentTab);
            }
        }

        private void OnPurchaseUpgrade(MetaUpgradeData upgrade, string relicFileName)
        {
            if (upgrade == null) return;
            var meta = SaveManager.Meta;
            if (MetaProgressionManager.TryPurchaseUpgrade(meta, upgrade.UpgradeId, upgrade.MemoryCost, upgrade.SoulCost))
            {
                // RelicUnlock 타입은 동시에 UnlockedRelicIds에 추가
                if (upgrade.Type == MetaUpgradeType.RelicUnlock && !string.IsNullOrEmpty(relicFileName))
                {
                    if (!meta.UnlockedRelicIds.Contains(relicFileName))
                        meta.UnlockedRelicIds.Add(relicFileName);
                }
                SaveManager.SaveMeta();
                RefreshBalance();
                SwitchTab(_currentTab);
            }
        }

        private void OnPurchaseGlobalUpgrade(MetaUpgradeData upgrade)
        {
            OnPurchaseUpgrade(upgrade, null);
        }

        // ── 유틸 ──

        private void ClearContent()
        {
            if (_contentContainer == null) return;
            for (int i = _contentContainer.childCount - 1; i >= 0; i--)
                Destroy(_contentContainer.GetChild(i).gameObject);
        }

        private void RefreshBalance()
        {
            var meta = SaveManager.Meta;
            if (_memoryLabel != null)
                _memoryLabel.text = $"기억: {meta?.MemoryFragments ?? 0}";
            if (_soulLabel != null)
                _soulLabel.text = $"영혼: {meta?.Souls ?? 0}";
        }

        private bool CanAfford(int memoryCost, int soulCost)
        {
            var meta = SaveManager.Meta;
            if (meta == null) return false;
            return meta.MemoryFragments >= memoryCost && meta.Souls >= soulCost;
        }

        private static string CostText(int memoryCost, int soulCost)
        {
            if (memoryCost <= 0 && soulCost <= 0) return "무료";
            var parts = new List<string>();
            if (memoryCost > 0) parts.Add($"기억 {memoryCost}");
            if (soulCost > 0) parts.Add($"영혼 {soulCost}");
            return string.Join(" ", parts);
        }

        private static string GetClassLabel(CharacterClass cls)
        {
            return cls switch
            {
                CharacterClass.Warrior => "전사",
                CharacterClass.Mage => "마법사",
                CharacterClass.Healer => "힐러",
                CharacterClass.Rogue => "도적",
                CharacterClass.Archer => "궁수",
                CharacterClass.Necromancer => "네크로맨서",
                CharacterClass.Alchemist => "연금술사",
                CharacterClass.Bard => "음유시인",
                _ => "?"
            };
        }

        private void OnCloseClicked()
        {
            AudioManager.Instance?.PlayUIConfirm();
            Hide();
        }
    }
}
