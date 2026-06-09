using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Reward;
using TeamLog.Map;
using TeamLog.UI;
using TeamLog.Characters;
using TeamLog.Skill;

namespace TeamLog.UI.Reward
{
    /// <summary>
    /// 보상 선택 화면 — 전투 승리 후 표시
    /// 새 흐름: 무작위 3개 증강 제안 → 선택/리롤/스킵
    /// 상점에서는 AugmentSelectPanel을 그대로 사용
    /// </summary>
    public class RewardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _cardContainer;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private GameObject _rewardCardPrefab;

        [Header("Reroll / Skip")]
        [SerializeField] private Button _rerollButton;
        [SerializeField] private TextMeshProUGUI _rerollLabel;
        [SerializeField] private Button _skipButton;

        [Header("Augment Assign (상점용)")]
        [SerializeField] private AugmentSelectPanel _augmentSelectPanel;

        private RewardManager _rewardManager;
        private GameRunState _runState;
        private System.Action _onRewardComplete;

        private readonly List<RewardOffer> _currentRewards = new();
        private MapNodeType _currentBattleType;
        private RewardOffer _pendingAugmentReward;

        /// <summary>
        /// 보상 화면 초기화
        /// </summary>
        public void Initialize(GameRunState runState, System.Action onRewardComplete)
        {
            _runState = runState;
            _onRewardComplete = onRewardComplete;
            _rewardManager = new RewardManager();

            if (_rerollButton != null)
                _rerollButton.onClick.AddListener(OnRerollClicked);
            if (_skipButton != null)
                _skipButton.onClick.AddListener(OnSkipClicked);
        }

        /// <summary>
        /// 전투 승리 후 보상 화면 표시
        /// </summary>
        public void ShowRewards(MapNodeType battleType)
        {
            _currentBattleType = battleType;
            gameObject.SetActive(true);
            var cg = UIAnimationHelper.EnsureCanvasGroup(gameObject);
            cg.alpha = 0f;
            UIAnimationHelper.FadeIn(cg);
            ClearCards();

            _currentRewards.Clear();
            var rewards = _rewardManager.GenerateRewards(battleType, _runState);
            _currentRewards.AddRange(rewards);

            if (_titleLabel != null)
                _titleLabel.text = "보상을 선택하세요";

            UpdateRerollUI();
            UpdateSkipButton();

            // 보상 카드 생성
            foreach (var reward in rewards)
            {
                if (_rewardCardPrefab == null || _cardContainer == null) continue;

                var cardObj = Instantiate(_rewardCardPrefab, _cardContainer);
                var card = cardObj.GetComponent<RewardCard>();
                if (card != null)
                    card.Setup(reward, OnRewardSelected);
            }
        }

        private void OnRewardSelected(RewardOffer selected)
        {
            if (selected.Type == RewardType.AugmentOffer)
            {
                AudioManager.Instance.PlayUIConfirm();
                _rewardManager.ApplyReward(selected, _runState);

                var offer = selected.AugmentOfferData;
                if (offer != null)
                    ToastUI.Show($"{offer.GetDisplayText()} 적용!");
                HideAndNotify();
                return;
            }

            if (selected.Type == RewardType.Augment)
            {
                // 상점 플로우에서 Augment 타입이 들어온 경우 (호환성 유지)
                AudioManager.Instance.PlayUIConfirm();
                _pendingAugmentReward = selected;

                if (_augmentSelectPanel != null)
                {
                    _augmentSelectPanel.Show(selected.Augment, _runState.PlayerParty,
                        _runState,
                        (applied) =>
                        {
                            _pendingAugmentReward = null;
                            HideAndNotify();
                        });
                }
                else
                {
                    _rewardManager.ApplyReward(selected, _runState);
                    HideAndNotify();
                }
                return;
            }

            if (selected.Type == RewardType.Gold)
                AudioManager.Instance.PlayUIGoldEarn();
            else
                AudioManager.Instance.PlayUIConfirm();

            _rewardManager.ApplyReward(selected, _runState);
            HideAndNotify();
        }

        private void OnRerollClicked()
        {
            if (_runState == null || !_runState.SpendRerollToken()) return;

            AudioManager.Instance.PlayUICancel();
            ClearCards();
            _currentRewards.Clear();

            var rewards = _rewardManager.RerollRewards(_currentBattleType, _runState);
            _currentRewards.AddRange(rewards);

            foreach (var reward in rewards)
            {
                if (_rewardCardPrefab == null || _cardContainer == null) continue;

                var cardObj = Instantiate(_rewardCardPrefab, _cardContainer);
                var card = cardObj.GetComponent<RewardCard>();
                if (card != null)
                    card.Setup(reward, OnRewardSelected);
            }

            UpdateRerollUI();
        }

        private void OnSkipClicked()
        {
            int skipGold = RewardManager.GetSkipGold(_currentBattleType);
            _runState.AddGold(skipGold);
            ToastUI.Show($"+{skipGold} 골드 획득!");
            AudioManager.Instance.PlayUIGoldEarn();
            HideAndNotify();
        }

        private void UpdateRerollUI()
        {
            if (_rerollButton != null)
                _rerollButton.interactable = _runState != null && _runState.RerollTokens > 0;
            if (_rerollLabel != null)
                _rerollLabel.text = $"리롤 ({_runState?.RerollTokens ?? 0})";
        }

        private void UpdateSkipButton()
        {
            if (_skipButton != null)
            {
                int skipGold = RewardManager.GetSkipGold(_currentBattleType);
                var skipLabel = _skipButton.GetComponentInChildren<TextMeshProUGUI>();
                if (skipLabel != null)
                    skipLabel.text = $"건너뛰기 +{skipGold}G";
            }
        }

        private void HideAndNotify()
        {
            _onRewardComplete?.Invoke();

            var cg = UIAnimationHelper.EnsureCanvasGroup(gameObject);
            UIAnimationHelper.FadeOut(cg);
        }

        private void ClearCards()
        {
            if (_cardContainer == null) return;

            for (int i = _cardContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_cardContainer.GetChild(i).gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_rerollButton != null)
                _rerollButton.onClick.RemoveListener(OnRerollClicked);
            if (_skipButton != null)
                _skipButton.onClick.RemoveListener(OnSkipClicked);
        }
    }
}
