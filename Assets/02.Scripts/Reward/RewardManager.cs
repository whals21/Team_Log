using System.Collections.Generic;
using UnityEngine;
using TeamLog.Map;
using TeamLog.Skill;

namespace TeamLog.Reward
{
    /// <summary>
    /// 보상 생성 및 선택 관리 — 순수 C# 클래스
    /// </summary>
    public class RewardManager
    {
        private readonly System.Random _rng = new();

        private MapNodeType _lastBattleType;
        private GameRunState _lastRunState;

        /// <summary>
        /// 전투 결과에 따라 보상 3개 생성
        /// 새 흐름: 증강 제안 3개 + 유물 (보스/엘리트)
        /// </summary>
        public List<RewardOffer> GenerateRewards(MapNodeType battleType, GameRunState runState)
        {
            _lastBattleType = battleType;
            _lastRunState = runState;

            var rewards = new List<RewardOffer>();

            // 증강 제안 3개 생성
            for (int i = 0; i < 3; i++)
            {
                var offer = runState.AugmentGenerator.GenerateAugmentOffer(battleType);
                if (offer != null)
                {
                    rewards.Add(new RewardOffer
                    {
                        Type = RewardType.AugmentOffer,
                        Rarity = offer.Tier switch
                        {
                            3 => RewardRarity.Unique,
                            2 => RewardRarity.Rare,
                            _ => RewardRarity.Common
                        },
                        Description = offer.GetDisplayText(),
                        AugmentOfferData = offer
                    });
                }
            }

            // 증강 제안이 3개 미만이면 골드로 채움
            int goldMin, goldMax;
            switch (battleType)
            {
                case MapNodeType.Elite:
                    goldMin = 30; goldMax = 60;
                    break;
                case MapNodeType.Boss:
                    goldMin = 50; goldMax = 100;
                    break;
                default:
                    goldMin = 10; goldMax = 25;
                    break;
            }

            while (rewards.Count < 3)
            {
                rewards.Add(CreateGoldReward(goldMin, goldMax, RewardRarity.Common));
            }

            // 보스: 항상 유물 보상 추가
            if (battleType == MapNodeType.Boss)
            {
                var relic = runState.PeekRandomRelic();
                if (relic != null)
                {
                    rewards.Add(new RewardOffer
                    {
                        Type = RewardType.Relic,
                        Rarity = RewardRarity.Unique,
                        Description = $"유물: {relic.RelicName}",
                        Relic = relic
                    });
                }
            }
            // 엘리트: 50% 확률로 유물 보상 추가
            else if (battleType == MapNodeType.Elite && _rng.NextDouble() < 0.50)
            {
                var relic = runState.PeekRandomRelic();
                if (relic != null)
                {
                    rewards.Add(new RewardOffer
                    {
                        Type = RewardType.Relic,
                        Rarity = RewardRarity.Rare,
                        Description = $"유물: {relic.RelicName}",
                        Relic = relic
                    });
                }
            }

            return rewards;
        }

        /// <summary>
        /// 리롤 — 토큰 소모 후 보상 재생성
        /// </summary>
        public List<RewardOffer> RerollRewards(MapNodeType battleType, GameRunState runState)
        {
            return GenerateRewards(battleType, runState);
        }

        private RewardOffer CreateGoldReward(int min, int max, RewardRarity rarity)
        {
            int amount = _rng.Next(min, max + 1);
            return new RewardOffer
            {
                Type = RewardType.Gold,
                Rarity = rarity,
                GoldAmount = amount,
                Description = $"{amount} 골드"
            };
        }

        /// <summary>
        /// 선택된 보상을 GameRunState에 적용
        /// AugmentOffer: 즉시 증강 부착
        /// </summary>
        public void ApplyReward(RewardOffer reward, GameRunState runState)
        {
            switch (reward.Type)
            {
                case RewardType.Gold:
                    runState.AddGold(reward.GoldAmount);
                    break;
                case RewardType.Augment:
                    // 상점용 — AugmentSelectPanel에서 처리
                    break;
                case RewardType.AugmentOffer:
                    if (reward.AugmentOfferData != null)
                    {
                        var offer = reward.AugmentOfferData;
                        runState.AcquireAugment(offer.Augment, offer.TargetCharacter, offer.TargetSkill);
                    }
                    break;
                case RewardType.Relic:
                    runState.AcquireRelic(reward.Relic);
                    break;
            }
        }

        /// <summary>
        /// 스킵 시 골드 보상 계산
        /// </summary>
        public static int GetSkipGold(MapNodeType battleType)
        {
            return battleType switch
            {
                MapNodeType.Elite => 30,
                MapNodeType.Boss => 50,
                _ => 15
            };
        }
    }

    /// <summary>
    /// 보상 선택지 하나 (런타임 데이터)
    /// </summary>
    public class RewardOffer
    {
        public RewardType Type;
        public RewardRarity Rarity;
        public int GoldAmount;
        public string Description;
        public RelicData Relic;
        public AugmentData Augment;
        public AugmentOffer AugmentOfferData;

        // 희귀도별 색상
        public Color GetRarityColor()
        {
            // 저주 증강은 암적색
            if (Type == RewardType.AugmentOffer && AugmentOfferData != null && AugmentOfferData.IsCursed)
                return new Color(0.8f, 0.15f, 0.15f);

            return Rarity switch
            {
                RewardRarity.Common => Color.white,
                RewardRarity.Rare => new Color(0.3f, 0.6f, 1f),
                RewardRarity.Unique => new Color(0.7f, 0.3f, 0.9f),
                _ => Color.white
            };
        }
    }
}
