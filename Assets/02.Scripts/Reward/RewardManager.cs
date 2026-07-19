using System.Collections.Generic;
using UnityEngine;
using TeamLog.Map;
using TeamLog.Skill;
using TeamLog.UI;
using TeamLog.UI.Map.Rework;  // ★ Node Detail Preview 파이프 — RewardPreviewInfo 참조

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

        /// <summary>
        /// ★ Node Detail Preview 파이프 — 전투 노드 미리보기용 보상 정보 생성.
        /// 기존 GenerateRewards와 분리된 정적 헬퍼 (실제 보상은 전투 후 동일 로직으로 생성됨).
        /// 골드 범위는 GenerateRewards의 goldMin/goldMax와 동일.
        /// </summary>
        public static RewardPreviewInfo GetPreview(MapNodeType type, GameRunState runState)
        {
            // using TeamLog.UI.Map.Rework가 안 되어 있어도 타입은 같은 어셈블리라 참조 가능.
            // (RewardManager는 TeamLog.Reward 네임스페이스 — NodePreviewData는 TeamLog.UI.Map.Rework)
            return type switch
            {
                MapNodeType.Battle => new RewardPreviewInfo
                {
                    GoldMin = 10, GoldMax = 25,
                    AugmentCount = 3,
                    RelicChance = 0f,
                    IncludesSouls = false,
                    Summary = "10-25G · 3 Augments"
                },
                MapNodeType.Elite => new RewardPreviewInfo
                {
                    GoldMin = 30, GoldMax = 60,
                    AugmentCount = 3,
                    RelicChance = 0.5f,
                    IncludesSouls = false,
                    Summary = "30-60G · 3 Augments · 50% Relic"
                },
                MapNodeType.Boss => new RewardPreviewInfo
                {
                    GoldMin = 50, GoldMax = 100,
                    AugmentCount = 3,
                    RelicChance = 1f,
                    IncludesSouls = true,
                    Summary = "50-100G · 3 Augments · Relic · Souls"
                },
                _ => new RewardPreviewInfo { Summary = "—" }
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

        // 희귀도별 색상 (UIPalette 토큰 참조)
        public Color GetRarityColor()
        {
            var p = UIPalette.Default;

            // 저주 증강은 암적색
            if (Type == RewardType.AugmentOffer && AugmentOfferData != null && AugmentOfferData.IsCursed)
                return p.GradeCursed;

            return Rarity switch
            {
                RewardRarity.Common => p.RarityCommon,
                RewardRarity.Rare => p.RarityRare,
                RewardRarity.Unique => p.RarityUnique,
                _ => p.RarityCommon
            };
        }
    }
}
