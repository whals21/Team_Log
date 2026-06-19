using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Meta;
using TeamLog.Reward;

namespace TeamLog.Tests
{
    /// <summary>
    /// 메타프로세션 보상/해금/필터링 단위 테스트 (Phase 8F).
    /// CalculateRunReward / FilterRelicPool / TryPurchaseTrait / TryEquipTrait 핵심 검증.
    /// </summary>
    [TestFixture]
    public class MetaProgressionTests
    {
        // ═══════════════════════════════════════════
        // 1. CalculateRunReward — 패배 시 기본 공식
        // ═══════════════════════════════════════════

        [Test]
        public void CalculateRunReward_Defeat_F1_ReturnsExpectedMemory()
        {
            // 패배 F1: memory = floor*5 + battlesWon + gold/100 = 5*1 + 5 + 50/100(0) = 10
            var reward = MetaProgressionManager.CalculateRunReward(
                victory: false, floor: 1, gold: 50, battlesWon: 5);
            Assert.AreEqual(10, reward.memory, "F1 패배 memory = 5+5+0 = 10");
            Assert.AreEqual(0, reward.souls, "패배 시 영혼 0");
        }

        // ═══════════════════════════════════════════
        // 2. CalculateRunReward — 승리 시 메모리 + 영혼
        // ═══════════════════════════════════════════

        [Test]
        public void CalculateRunReward_Victory_F4_ReturnsExpectedValues()
        {
            // 승리 F4: memory = 4*10 + 20 + 50 + 400/100(4) = 114, souls = 1 + 4/2 = 3
            var reward = MetaProgressionManager.CalculateRunReward(
                victory: true, floor: 4, gold: 400, battlesWon: 20);
            Assert.AreEqual(114, reward.memory, "F4 승리 memory = 40+20+50+4 = 114");
            Assert.AreEqual(3, reward.souls, "F4 승리 souls = 1+2 = 3");
        }

        // ═══════════════════════════════════════════
        // 3. 패배 시 영혼 0 / 승리 시 영혼 지급
        // ═══════════════════════════════════════════

        [Test]
        public void CalculateRunReward_VictoryGrantsSouls_DefeatGrantsNone()
        {
            var defeat = MetaProgressionManager.CalculateRunReward(false, 2, 100, 5);
            var victory = MetaProgressionManager.CalculateRunReward(true, 2, 100, 5);

            Assert.AreEqual(0, defeat.souls, "패배 시 영혼 0");
            Assert.Greater(victory.souls, 0, "승리 시 영혼 > 0");
            Assert.AreEqual(2, victory.souls, "F2 승리 영혼 = 1+1 = 2");
        }

        // ═══════════════════════════════════════════
        // 4. 골드 보너스 — 100골드당 +1 memory
        // ═══════════════════════════════════════════

        [Test]
        public void CalculateRunReward_GoldBonus_Every100Gold()
        {
            var r1 = MetaProgressionManager.CalculateRunReward(false, 1, 99, 0);
            var r2 = MetaProgressionManager.CalculateRunReward(false, 1, 100, 0);
            var r3 = MetaProgressionManager.CalculateRunReward(false, 1, 250, 0);

            Assert.AreEqual(5, r1.memory, "99골드 — 보너스 0");
            Assert.AreEqual(6, r2.memory, "100골드 — 보너스 +1");
            Assert.AreEqual(7, r3.memory, "250골드 — 보너스 +2 (250/100=2)");
        }

        // ═══════════════════════════════════════════
        // 5. FilterRelicPool — 잠긴 유물 제거
        // ═══════════════════════════════════════════

        [Test]
        public void FilterRelicPool_ExcludesLockedRelics()
        {
            var defaultRelic = CreateRelic("Relic_BurningSword");
            var lockedRelic = CreateRelic("Relic_SynergyTest_Locked001");
            var pool = new List<RelicData> { defaultRelic, lockedRelic };

            var meta = new MetaSaveData();
            meta.UnlockedRelicIds = new List<string>();

            var filtered = MetaProgressionManager.FilterRelicPool(pool, meta);
            Assert.AreEqual(1, filtered.Count, "기본 1개만 남음 (잠긴 유물 제거)");
            Assert.AreSame(defaultRelic, filtered[0], "BurningSword(기본 해금)만 필터 통과");
        }

        // ═══════════════════════════════════════════
        // 6. FilterRelicPool — 기본 16종 유물 포함
        // ═══════════════════════════════════════════

        [Test]
        public void FilterRelicPool_IncludesDefaultRelics()
        {
            var defaults = new[]
            {
                CreateRelic("Relic_BurningSword"),
                CreateRelic("Relic_IronHide"),
                CreateRelic("Relic_RegenRing"),
                CreateRelic("Relic_GoldCharm"),
            };
            var pool = new List<RelicData>(defaults);

            var meta = new MetaSaveData();
            meta.UnlockedRelicIds = new List<string>();

            var filtered = MetaProgressionManager.FilterRelicPool(pool, meta);
            Assert.AreEqual(4, filtered.Count, "기본 16종은 메타 해금 없이도 항상 통과");
        }

        // ═══════════════════════════════════════════
        // 7. TryPurchaseTrait — 재화 부족 시 실패
        // ═══════════════════════════════════════════

        [Test]
        public void TryPurchaseTrait_InsufficientFunds_ReturnsFalse()
        {
            var meta = new MetaSaveData
            {
                MemoryFragments = 10,
                Souls = 0,
                UnlockedTraitIds = new List<string>()
            };

            bool result = MetaProgressionManager.TryPurchaseTrait(meta, "Trait_Test", memoryCost: 30, soulCost: 0);
            Assert.IsFalse(result, "기억 부족(10<30) → 구매 실패");
            Assert.AreEqual(10, meta.MemoryFragments, "재화 차감 안 됨");
            Assert.AreEqual(0, meta.UnlockedTraitIds.Count, "해금 목록 미추가");
        }

        // ═══════════════════════════════════════════
        // 8. TryPurchaseTrait — 재화 충족 시 성공 + 차감
        // ═══════════════════════════════════════════

        [Test]
        public void TryPurchaseTrait_SufficientFunds_DeductsAndUnlocks()
        {
            var meta = new MetaSaveData
            {
                MemoryFragments = 100,
                Souls = 2,
                UnlockedTraitIds = new List<string>()
            };

            bool result = MetaProgressionManager.TryPurchaseTrait(meta, "Trait_Test", memoryCost: 30, soulCost: 1);
            Assert.IsTrue(result, "재화 충족 → 구매 성공");
            Assert.AreEqual(70, meta.MemoryFragments, "기억 30 차감");
            Assert.AreEqual(1, meta.Souls, "영혼 1 차감");
            CollectionAssert.Contains(meta.UnlockedTraitIds, "Trait_Test");
        }

        // ═══════════════════════════════════════════
        // 9. TryEquipTrait — 잠긴 특성 장착 시 실패
        // ═══════════════════════════════════════════

        [Test]
        public void TryEquipTrait_LockedTrait_ReturnsFalse()
        {
            var meta = new MetaSaveData
            {
                EquippedTraitBindings = new List<TraitBindingEntry>(),
                UnlockedTraitIds = new List<string>()
            };

            bool result = MetaProgressionManager.TryEquipTrait(
                meta, "전사", "Trait_Locked", requireUnlocked: true);
            Assert.IsFalse(result, "미해금 특성 → 장착 거부");
            Assert.AreEqual(0, meta.EquippedTraitBindings.Count, "바인딩 미추가");
        }

        // ═══════════════════════════════════════════
        // 10. TryEquipTrait — 해금된 특성 장착 + 기존 교체
        // ═══════════════════════════════════════════

        [Test]
        public void TryEquipTrait_UnlockedTrait_ReplacesExistingBinding()
        {
            var meta = new MetaSaveData
            {
                EquippedTraitBindings = new List<TraitBindingEntry>
                {
                    new TraitBindingEntry("전사", "Trait_Old")
                },
                UnlockedTraitIds = new List<string> { "Trait_New" }
            };

            bool result = MetaProgressionManager.TryEquipTrait(
                meta, "전사", "Trait_New", requireUnlocked: true);
            Assert.IsTrue(result, "해금된 특성 → 장착 성공");
            Assert.AreEqual(1, meta.EquippedTraitBindings.Count, "기존 바인딩 제거 후 새 바인딩 1개");
            Assert.AreEqual("Trait_New", meta.EquippedTraitBindings[0].TraitId, "새 특성으로 교체");
        }

        // ═══════════════════════════════════════════
        // 헬퍼
        // ═══════════════════════════════════════════

        private static RelicData CreateRelic(string fileName)
        {
            var relic = ScriptableObject.CreateInstance<RelicData>();
            relic.name = fileName; // Object.name — IsRelicUnlocked의 키
            return relic;
        }
    }
}
