using System.Collections.Generic;
using TeamLog.Map;
using TeamLog.Reward;

namespace TeamLog.Meta
{
    /// <summary>
    /// 메타프로세션 중앙 관리자 — 순수 C# static.
    /// 런 보상 계산, 특성/강화 해금, 장착 바인딩 조회를 담당.
    /// 유물 풀 필터링/시작 유물 지급은 Phase 8E에서 확장 예정.
    /// </summary>
    public static class MetaProgressionManager
    {
        // ── 런 보상 계산 ──

        /// <summary>
        /// 런 종료 시 메타 재화 보상 계산.
        /// 패배: memory = floor*5 + battlesWon + gold/100; souls = 0
        /// 승리: memory = floor*10 + battlesWon + 50 + gold/100; souls = 1 + floor/2
        /// </summary>
        public static (int memory, int souls) CalculateRunReward(
            bool victory, int floor, int gold, int battlesWon)
        {
            int safeFloor = floor < 1 ? 1 : floor;
            int goldBonus = gold / 100;

            if (victory)
            {
                int memory = safeFloor * 10 + battlesWon + 50 + goldBonus;
                int souls = 1 + safeFloor / 2;
                return (memory, souls);
            }
            else
            {
                int memory = safeFloor * 5 + battlesWon + goldBonus;
                return (memory, 0);
            }
        }

        // ── 특성 해금/장착 ──

        /// <summary>
        /// 메모리/영혼으로 특성 해금 시도. 성공 시 재화 차감 + UnlockedTraitIds에 추가.
        /// </summary>
        public static bool TryPurchaseTrait(MetaSaveData meta, string traitId, int memoryCost, int soulCost)
        {
            if (meta == null || string.IsNullOrEmpty(traitId)) return false;
            if (IsTraitUnlocked(meta, traitId)) return false;
            if (meta.MemoryFragments < memoryCost) return false;
            if (meta.Souls < soulCost) return false;

            meta.MemoryFragments -= memoryCost;
            meta.Souls -= soulCost;
            if (meta.UnlockedTraitIds == null)
                meta.UnlockedTraitIds = new List<string>();
            meta.UnlockedTraitIds.Add(traitId);
            return true;
        }

        /// <summary>
        /// 메모리/영혼으로 일회성 메타 강화 구매 시도.
        /// </summary>
        public static bool TryPurchaseUpgrade(MetaSaveData meta, string upgradeId, int memoryCost, int soulCost)
        {
            if (meta == null || string.IsNullOrEmpty(upgradeId)) return false;
            if (IsUpgradePurchased(meta, upgradeId)) return false;
            if (meta.MemoryFragments < memoryCost) return false;
            if (meta.Souls < soulCost) return false;

            meta.MemoryFragments -= memoryCost;
            meta.Souls -= soulCost;
            if (meta.PurchasedUpgradeIds == null)
                meta.PurchasedUpgradeIds = new List<string>();
            meta.PurchasedUpgradeIds.Add(upgradeId);
            return true;
        }

        /// <summary>
        /// 캐릭터에 특성 장착 — 기존 바인딩 교체. traitId가 null/빈 값이면 장착 해제.
        /// </summary>
        public static bool TryEquipTrait(MetaSaveData meta, string characterName, string traitId,
            bool requireUnlocked = true)
        {
            if (meta == null || string.IsNullOrEmpty(characterName)) return false;
            if (requireUnlocked && !string.IsNullOrEmpty(traitId) && !IsTraitUnlocked(meta, traitId))
                return false;

            if (meta.EquippedTraitBindings == null)
                meta.EquippedTraitBindings = new List<TraitBindingEntry>();

            // 기존 바인딩 제거
            meta.EquippedTraitBindings.RemoveAll(b => b.CharacterName == characterName);

            // 새 바인딩 추가 (traitId가 비어있지 않은 경우만)
            if (!string.IsNullOrEmpty(traitId))
                meta.EquippedTraitBindings.Add(new TraitBindingEntry(characterName, traitId));
            return true;
        }

        /// <summary>캐릭터에 장착된 특성 TraitId 조회. 없으면 null/빈 문자열.</summary>
        public static string GetEquippedTraitId(MetaSaveData meta, string characterName)
        {
            if (meta == null || meta.EquippedTraitBindings == null || string.IsNullOrEmpty(characterName))
                return null;
            foreach (var entry in meta.EquippedTraitBindings)
            {
                if (entry.CharacterName == characterName)
                    return entry.TraitId;
            }
            return null;
        }

        // ── 상태 조회 ──

        public static bool IsTraitUnlocked(MetaSaveData meta, string traitId)
        {
            if (meta == null || meta.UnlockedTraitIds == null || string.IsNullOrEmpty(traitId))
                return false;
            return meta.UnlockedTraitIds.Contains(traitId);
        }

        public static bool IsRelicUnlocked(MetaSaveData meta, string relicFileName)
        {
            if (meta == null || string.IsNullOrEmpty(relicFileName)) return false;
            // 기본 16종 (Phase 5C 원본 유물)은 메타 해금 없이도 잠금해금 상태.
            if (IsDefaultRelic(relicFileName)) return true;
            if (meta.UnlockedRelicIds == null) return false;
            return meta.UnlockedRelicIds.Contains(relicFileName);
        }

        public static bool IsUpgradePurchased(MetaSaveData meta, string upgradeId)
        {
            if (meta == null || meta.PurchasedUpgradeIds == null || string.IsNullOrEmpty(upgradeId))
                return false;
            return meta.PurchasedUpgradeIds.Contains(upgradeId);
        }

        // ── 유물 풀 필터링 / 시작 유물 ── (Phase 8E)

        /// <summary>
        /// 유물 풀에서 잠긴 유물 제거. 런 시작 시 드롭 풀 구성.
        /// </summary>
        public static List<RelicData> FilterRelicPool(IEnumerable<RelicData> pool, MetaSaveData meta)
        {
            var result = new List<RelicData>();
            if (pool == null) return result;
            foreach (var relic in pool)
            {
                if (relic == null) continue;
                if (IsRelicUnlocked(meta, relic.name)) result.Add(relic);
            }
            return result;
        }

        /// <summary>
        /// 풀에서 count개 무작위 유물 추출 (중복 없음). 시작 유물 후보 생성.
        /// </summary>
        public static List<RelicData> RollRelics(List<RelicData> pool, int count)
        {
            var result = new List<RelicData>();
            if (pool == null || pool.Count == 0 || count <= 0) return result;
            var copy = new List<RelicData>(pool);
            for (int i = 0; i < count && copy.Count > 0; i++)
            {
                int idx = UnityEngine.Random.Range(0, copy.Count);
                result.Add(copy[idx]);
                copy.RemoveAt(idx);
            }
            return result;
        }

        /// <summary>
        /// 시작 유물 지급 대수 — StartingRelicSlot(1) + StartingRelicChoice(3) 중 큰 값.
        /// Choice는 UI 기획이 복잡하므로 Phase 8E에서는 단일 랜덤으로 단순화.
        /// </summary>
        public static int GetStartingRelicGrantCount(MetaSaveData meta)
        {
            int count = 0;
            if (IsUpgradePurchased(meta, "Meta_StartingRelicSlot")) count += 1;
            if (IsUpgradePurchased(meta, "Meta_StartingRelicChoice")) count += 1;
            return count;
        }

        /// <summary>ExtraReroll 강화 구매 시 +1</summary>
        public static int GetExtraRerollCount(MetaSaveData meta)
            => IsUpgradePurchased(meta, "Meta_ExtraReroll") ? 1 : 0;

        /// <summary>PartyHealBoost 강화 구매 시 휴식 healPercent 가산</summary>
        public static float GetPartyHealBoost(MetaSaveData meta)
            => IsUpgradePurchased(meta, "Meta_PartyHealBoost") ? 0.1f : 0f;

        /// <summary>
        /// 기본 해금 유물 16종 (Phase 5C 원본). 나머지 26종은 메타 해금 필요.
        /// 목록이 길어지면 별도 SO로 추출 검토.
        /// </summary>
        private static readonly HashSet<string> DefaultRelicIds = new()
        {
            "Relic_BurningSword", "Relic_IronHide", "Relic_RegenRing", "Relic_GoldCharm",
            "Relic_ShieldAmulet", "Relic_VampireFang", "Relic_BerserkerMark", "Relic_LuckyClover",
            "Relic_ThornArmor", "Relic_SwiftBoots", "Relic_WarBanner", "Relic_HealingHerb",
            "Relic_LifeCrystal", "Relic_WeaponStone", "Relic_HardShell", "Relic_DragonHeart"
        };

        private static bool IsDefaultRelic(string relicFileName) => DefaultRelicIds.Contains(relicFileName);
    }
}
