using System.Collections.Generic;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.Skill;

namespace TeamLog.Shop
{
    /// <summary>
    /// 상점 관리 — 구매/판매 로직, 상품 생성
    /// </summary>
    public class ShopManager
    {
        private readonly System.Random _rng = new();

        /// <summary>
        /// 랜덤 상점 슬롯 생성 (상점 노드 방문 시)
        /// </summary>
        public List<ShopSlot> GenerateShopSlots(int floorNumber,
            IReadOnlyList<AugmentData> augmentPool, IReadOnlyList<RelicData> relicPool)
        {
            var slots = new List<ShopSlot>();

            // 증강 3개
            for (int i = 0; i < 3; i++)
            {
                var slot = new ShopSlot
                {
                    ContentType = ShopSlot.SlotContentType.Augment,
                    Price = GetAugmentPrice(floorNumber),
                    IsSold = false
                };

                if (augmentPool != null && augmentPool.Count > 0)
                    slot.Augment = augmentPool[_rng.Next(augmentPool.Count)];

                slots.Add(slot);
            }

            // 유물 2개
            for (int i = 0; i < 2; i++)
            {
                var slot = new ShopSlot
                {
                    ContentType = ShopSlot.SlotContentType.Relic,
                    Price = GetRelicPrice(floorNumber),
                    IsSold = false
                };

                if (relicPool != null && relicPool.Count > 0)
                    slot.Relic = relicPool[_rng.Next(relicPool.Count)];

                slots.Add(slot);
            }

            return slots;
        }

        /// <summary>
        /// 구매 — 성공 시 true
        /// </summary>
        public bool PurchaseItem(ShopSlot slot, GameRunState runState)
        {
            if (slot.IsSold) return false;
            if (!runState.SpendGold(slot.Price)) return false;

            slot.IsSold = true;

            if (slot.ContentType == ShopSlot.SlotContentType.Relic)
            {
                runState.AcquireRelic(slot.Relic);
            }
            // 증강 배정은 호출자에서 AugmentSelectPanel로 처리

            return true;
        }

        /// <summary>
        /// 유물 판매 — 유물 목록에서 제거 + 골드 획득
        /// </summary>
        public bool SellRelic(RelicData relic, GameRunState runState, int floorNumber)
        {
            if (relic == null) return false;
            if (!runState.RemoveRelic(relic)) return false;
            int sellPrice = GetRelicSellPrice(floorNumber);
            runState.AddGold(sellPrice);
            return true;
        }

        public int GetRelicSellPrice(int floorNumber) => GetRelicPrice(floorNumber) / 2;

        private int GetAugmentPrice(int floorNumber)
        {
            int basePrice = 30 + (floorNumber - 1) * 15;
            return _rng.Next(basePrice, basePrice + 30);
        }

        private int GetRelicPrice(int floorNumber)
        {
            int basePrice = 40 + (floorNumber - 1) * 20;
            return _rng.Next(basePrice, basePrice + 40);
        }
    }
}
