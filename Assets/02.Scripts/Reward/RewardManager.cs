using System.Collections.Generic;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Map;

namespace TeamLog.Reward
{
    /// <summary>
    /// 보상 생성 및 선택 관리 — 순수 C# 클래스
    /// </summary>
    public class RewardManager
    {
        private readonly System.Random _rng = new();

        /// <summary>
        /// 전투 결과에 따라 보상 3개 생성
        /// </summary>
        public List<RewardOffer> GenerateRewards(MapNodeType battleType, GameRunState runState)
        {
            var rewards = new List<RewardOffer>();
            int goldMin, goldMax;

            switch (battleType)
            {
                case MapNodeType.Elite:
                    goldMin = 50; goldMax = 100;
                    break;
                case MapNodeType.Boss:
                    goldMin = 100; goldMax = 200;
                    break;
                default: // Battle
                    goldMin = 20; goldMax = 50;
                    break;
            }

            // 보상 3개 생성: 항상 골드 1개 + 나머지는 가중치 랜덤
            rewards.Add(CreateGoldReward(goldMin, goldMax, RewardRarity.Common));

            for (int i = 0; i < 2; i++)
            {
                rewards.Add(GenerateRandomReward(battleType, runState));
            }

            return rewards;
        }

        private RewardOffer GenerateRandomReward(MapNodeType battleType, GameRunState runState)
        {
            // 가중치: 골드 40%, 스킬 35%, 아이템 25%
            double roll = _rng.NextDouble();

            if (roll < 0.40)
            {
                int goldMin = battleType == MapNodeType.Elite ? 50 : 20;
                int goldMax = battleType == MapNodeType.Boss ? 80 : 50;
                return CreateGoldReward(goldMin, goldMax, RewardRarity.Common);
            }
            else if (roll < 0.75)
            {
                var skill = runState.PeekRandomSkill();
                return new RewardOffer
                {
                    Type = RewardType.Skill,
                    Rarity = battleType == MapNodeType.Boss ? RewardRarity.Unique : RewardRarity.Common,
                    Description = skill != null ? $"스킬: {skill.SkillName}" : "스킬 보상 (풀 없음)",
                    Skill = skill
                };
            }
            else
            {
                var item = runState.PeekRandomItem();
                return new RewardOffer
                {
                    Type = RewardType.Item,
                    Rarity = battleType == MapNodeType.Elite ? RewardRarity.Rare : RewardRarity.Common,
                    Description = item != null ? $"아이템: {item.ItemName}" : "아이템 보상 (풀 없음)",
                    Item = item
                };
            }
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
        /// </summary>
        public void ApplyReward(RewardOffer reward, GameRunState runState)
        {
            switch (reward.Type)
            {
                case RewardType.Gold:
                    runState.AddGold(reward.GoldAmount);
                    break;
                case RewardType.Skill:
                    runState.AcquireSkill(reward.Skill);
                    break;
                case RewardType.Item:
                    runState.AcquireItem(reward.Item);
                    break;
            }
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
        public SkillData Skill;
        public ItemData Item;

        // 희귀도별 색상
        public Color GetRarityColor()
        {
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
