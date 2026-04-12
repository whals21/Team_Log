using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Reward;
using TeamLog.Map;

namespace TeamLog.UI.Reward
{
    /// <summary>
    /// 보상 카드 하나
    /// </summary>
    public class RewardCard : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _descLabel;
        [SerializeField] private Button _button;

        private RewardOffer _offer;
        private System.Action<RewardOffer> _onSelected;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        public void Setup(RewardOffer offer, System.Action<RewardOffer> onSelected)
        {
            _offer = offer;
            _onSelected = onSelected;

            // 타이틀
            string typeLabel = offer.Type switch
            {
                RewardType.Gold => "골드",
                RewardType.Skill => "스킬",
                RewardType.Item => "아이템",
                _ => "보상"
            };

            if (_titleLabel != null)
            {
                _titleLabel.text = typeLabel;
                _titleLabel.color = offer.GetRarityColor();
            }

            // 설명
            if (_descLabel != null)
                _descLabel.text = offer.Description;

            // 배경색
            if (_backgroundImage != null)
            {
                Color baseColor = offer.Type switch
                {
                    RewardType.Gold => new Color(0.2f, 0.18f, 0.08f),
                    RewardType.Skill => new Color(0.12f, 0.15f, 0.22f),
                    RewardType.Item => new Color(0.15f, 0.12f, 0.2f),
                    _ => new Color(0.15f, 0.15f, 0.2f)
                };
                _backgroundImage.color = baseColor;
            }
        }

        private void OnClicked()
        {
            _onSelected?.Invoke(_offer);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClicked);
        }
    }

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
            var rewards = _rewardManager.GenerateRewards(battleType);
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
