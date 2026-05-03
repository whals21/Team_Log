using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Reward;
using TeamLog.Map;

namespace TeamLog.UI.Reward
{
    /// <summary>
    /// 보상 선택 화면 — 전투 승리 후 표시
    /// </summary>
    public class RewardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _cardContainer;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private GameObject _rewardCardPrefab;

        private RewardManager _rewardManager;
        private GameRunState _runState;
        private System.Action _onRewardComplete;

        private readonly List<RewardOffer> _currentRewards = new();

        /// <summary>
        /// 보상 화면 초기화
        /// </summary>
        public void Initialize(GameRunState runState, System.Action onRewardComplete)
        {
            _runState = runState;
            _onRewardComplete = onRewardComplete;
            _rewardManager = new RewardManager();
        }

        /// <summary>
        /// 전투 승리 후 보상 화면 표시
        /// </summary>
        public void ShowRewards(MapNodeType battleType)
        {
            gameObject.SetActive(true);
            ClearCards();

            _currentRewards.Clear();
            var rewards = _rewardManager.GenerateRewards(battleType, _runState);
            _currentRewards.AddRange(rewards);

            if (_titleLabel != null)
                _titleLabel.text = "보상을 선택하세요";

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
            _rewardManager.ApplyReward(selected, _runState);

            // 보상 화면 닫기
            gameObject.SetActive(false);
            _onRewardComplete?.Invoke();
        }

        private void ClearCards()
        {
            if (_cardContainer == null) return;

            for (int i = _cardContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_cardContainer.GetChild(i).gameObject);
            }
        }
    }
}
