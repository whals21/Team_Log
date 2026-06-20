using NUnit.Framework;
using TeamLog.Map;
using TeamLog.Meta;

namespace TeamLog.Tests
{
    /// <summary>
    /// 어센션 시스템 단위 테스트 (Phase A).
    /// GetStackCountByLevel / GetXxxByLevel / GetActiveModifiers / GetAscensionLevel 핵심 검증.
    /// </summary>
    [TestFixture]
    public class AscensionManagerTests
    {
        // ═══════════════════════════════════════════
        // 1. GetStackCountByLevel — 레벨 임계값 누적
        // ═══════════════════════════════════════════

        [Test]
        public void StackCount_EnemyHp_Level0_ReturnsZero()
        {
            Assert.AreEqual(0, AscensionManager.GetStackCountByLevel(AscensionModifierType.EnemyHpPercent, 0));
        }

        [Test]
        public void StackCount_EnemyHp_Level1_ReturnsOne()
        {
            Assert.AreEqual(1, AscensionManager.GetStackCountByLevel(AscensionModifierType.EnemyHpPercent, 1));
        }

        [Test]
        public void StackCount_EnemyHp_Level7_ReturnsTwo()
        {
            Assert.AreEqual(2, AscensionManager.GetStackCountByLevel(AscensionModifierType.EnemyHpPercent, 7));
        }

        [Test]
        public void StackCount_EnemyHp_Level13_ReturnsThree()
        {
            Assert.AreEqual(3, AscensionManager.GetStackCountByLevel(AscensionModifierType.EnemyHpPercent, 13));
        }

        [Test]
        public void StackCount_Reroll_Level14_ReturnsThree()
        {
            // Reroll 임계값: 3, 9, 14
            Assert.AreEqual(3, AscensionManager.GetStackCountByLevel(AscensionModifierType.RerollCount, 14));
        }

        [Test]
        public void StackCount_BossHp_Level14_ReturnsZero()
        {
            // BossHp는 레벨 15 전용
            Assert.AreEqual(0, AscensionManager.GetStackCountByLevel(AscensionModifierType.BossHpPercent, 14));
        }

        [Test]
        public void StackCount_BossHp_Level15_ReturnsOne()
        {
            Assert.AreEqual(1, AscensionManager.GetStackCountByLevel(AscensionModifierType.BossHpPercent, 15));
        }

        // ═══════════════════════════════════════════
        // 2. 누적 값 (GetXxxByLevel)
        // ═══════════════════════════════════════════

        [Test]
        public void EnemyHpMul_Level0_ReturnsOne()
        {
            Assert.AreEqual(1f, AscensionManager.GetEnemyHpMulByLevel(0));
        }

        [Test]
        public void EnemyHpMul_Level1_Returns1_05()
        {
            Assert.AreEqual(1.05f, AscensionManager.GetEnemyHpMulByLevel(1));
        }

        [Test]
        public void EnemyHpMul_Level13_Returns1_15()
        {
            // 스택 3: 1 + 0.05*3 = 1.15
            Assert.AreEqual(1.15f, AscensionManager.GetEnemyHpMulByLevel(13));
        }

        [Test]
        public void RerollDelta_Level0_ReturnsZero()
        {
            Assert.AreEqual(0, AscensionManager.GetRerollDeltaByLevel(0));
        }

        [Test]
        public void RerollDelta_Level3_ReturnsMinus1()
        {
            Assert.AreEqual(-1, AscensionManager.GetRerollDeltaByLevel(3));
        }

        [Test]
        public void RerollDelta_Level14_ReturnsMinus3()
        {
            Assert.AreEqual(-3, AscensionManager.GetRerollDeltaByLevel(14));
        }

        [Test]
        public void StartGoldDelta_Level8_ReturnsMinus20()
        {
            Assert.AreEqual(-20, AscensionManager.GetStartGoldDeltaByLevel(8));
        }

        [Test]
        public void BossHpMul_Level15_Returns1_20()
        {
            Assert.AreEqual(1.20f, AscensionManager.GetBossHpMulByLevel(15));
        }

        [Test]
        public void BossHpMul_Level14_ReturnsOne()
        {
            Assert.AreEqual(1f, AscensionManager.GetBossHpMulByLevel(14));
        }

        [Test]
        public void PlayerMaxHpMul_Level10_Returns0_90()
        {
            // 스택 2: 1 + (-0.05)*2 = 0.90
            Assert.AreEqual(0.90f, AscensionManager.GetPlayerMaxHpMulByLevel(10));
        }

        [Test]
        public void HealMul_Level11_Returns0_80()
        {
            // 스택 2: 1 + (-0.10)*2 = 0.80
            Assert.AreEqual(0.80f, AscensionManager.GetHealMulByLevel(11));
        }

        [Test]
        public void EnemyAtkMul_Level12_Returns1_10()
        {
            Assert.AreEqual(1.10f, AscensionManager.GetEnemyAtkMulByLevel(12));
        }

        // ═══════════════════════════════════════════
        // 3. GetActiveModifiers — 활성 modifier 개수
        // ═══════════════════════════════════════════

        [Test]
        public void ActiveModifiers_Level0_ReturnsEmpty()
        {
            var list = AscensionManager.GetActiveModifiers(0);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void ActiveModifiers_Level1_ReturnsOneEntry()
        {
            var list = AscensionManager.GetActiveModifiers(1);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(AscensionModifierType.EnemyHpPercent, list[0]);
        }

        [Test]
        public void ActiveModifiers_Level7_HasTwoEnemyHpStacks()
        {
            var list = AscensionManager.GetActiveModifiers(7);
            int enemyHpCount = 0;
            foreach (var m in list)
                if (m == AscensionModifierType.EnemyHpPercent) enemyHpCount++;
            Assert.AreEqual(2, enemyHpCount, "레벨 7: EnemyHp 스택 2");
        }

        [Test]
        public void ActiveModifiers_Level15_HasBossHp()
        {
            var list = AscensionManager.GetActiveModifiers(15);
            Assert.Contains(AscensionModifierType.BossHpPercent, list);
        }

        [Test]
        public void ActiveModifiers_Level15_TotalStacks_Equals15()
        {
            // 레벨 15: 모든 modifier의 스택 합 = 15
            var list = AscensionManager.GetActiveModifiers(15);
            Assert.AreEqual(15, list.Count);
        }

        // ═══════════════════════════════════════════
        // 4. GetAscensionLevel / ClampSelectedLevel — MetaSaveData 기반
        // ═══════════════════════════════════════════

        [Test]
        public void GetAscensionLevel_NullMeta_ReturnsZero()
        {
            Assert.AreEqual(0, AscensionManager.GetAscensionLevel(null));
        }

        [Test]
        public void GetAscensionLevel_NormalValue_ReturnsSame()
        {
            var meta = new MetaSaveData { AscensionLevel = 7 };
            Assert.AreEqual(7, AscensionManager.GetAscensionLevel(meta));
        }

        [Test]
        public void GetAscensionLevel_OverMax_ClampedTo15()
        {
            var meta = new MetaSaveData { AscensionLevel = 99 };
            Assert.AreEqual(15, AscensionManager.GetAscensionLevel(meta));
        }

        [Test]
        public void ClampSelectedLevel_BelowZero_ReturnsZero()
        {
            var meta = new MetaSaveData { AscensionLevel = 5 };
            Assert.AreEqual(0, AscensionManager.ClampSelectedLevel(-3, meta));
        }

        [Test]
        public void ClampSelectedLevel_AboveMax_ClampedToAscensionLevel()
        {
            var meta = new MetaSaveData { AscensionLevel = 5 };
            Assert.AreEqual(5, AscensionManager.ClampSelectedLevel(10, meta));
        }

        [Test]
        public void ClampSelectedLevel_InRange_ReturnsSame()
        {
            var meta = new MetaSaveData { AscensionLevel = 10 };
            Assert.AreEqual(7, AscensionManager.ClampSelectedLevel(7, meta));
        }
    }
}
