using System.Collections.Generic;
using UnityEngine;
using TeamLog.Map;
using TeamLog.Reward;

namespace TeamLog.Shop
{
    /// <summary>
    /// 상점 관리 — 구매/판매 로직, 상품 생성
    /// </summary>
    public class ShopManager
    {
        private readonly System.Random _rng = new();

        public event System.Action<int> OnGoldChanged;

        /// <summary>
        /// 랜덤 상점 슬롯 생성 (상점 노드 방문 시)
        /// </summary>
        public List<ShopSlot> GenerateShopSlots(int floorNumber)
        {
            var slots = new List<ShopSlot>();

            // 스킬 3개
            for (int i = 0; i < 3; i++)
            {
                slots.Add(new ShopSlot
                {
                    ContentType = ShopSlot.SlotContentType.Skill,
                    Price = GetSkillPrice(floorNumber),
                    IsSold = false
                });
            }

            // 아이템 2개
            for (int i = 0; i < 2; i++)
            {
                slots.Add(new ShopSlot
                {
                    ContentType = ShopSlot.SlotContentType.Item,
                    Price = GetItemPrice(floorNumber),
                    IsSold = false
                });
            }

            return slots;
        }

        /// <summary>
        /// 아이템 구매 — 성공 시 true
        /// </summary>
        public bool PurchaseItem(ShopSlot slot, GameRunState runState)
        {
            if (slot.IsSold) return false;
            if (!runState.SpendGold(slot.Price)) return false;

            slot.IsSold = true;
            OnGoldChanged?.Invoke(runState.Gold);

            if (slot.ContentType == ShopSlot.SlotContentType.Skill)
            {
                Debug.Log($"[ShopManager] 스킬 구매: {slot.Name}");
            }
            else
            {
                Debug.Log($"[ShopManager] 아이템 구매: {slot.Name}");
            }

            return true;
        }

        /// <summary>
        /// 스킬 판매 (판매가 = 50%)
        /// </summary>
        public void SellSkill(int sellPrice, GameRunState runState)
        {
            runState.AddGold(sellPrice);
            OnGoldChanged?.Invoke(runState.Gold);
        }

        private int GetSkillPrice(int floorNumber)
        {
            int basePrice = 30 + (floorNumber - 1) * 15;
            return _rng.Next(basePrice, basePrice + 30);
        }

        private int GetItemPrice(int floorNumber)
        {
            int basePrice = 40 + (floorNumber - 1) * 20;
            return _rng.Next(basePrice, basePrice + 40);
        }
    }
}
