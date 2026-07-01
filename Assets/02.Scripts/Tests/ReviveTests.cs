using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Combat.Turn;
using TeamLog.Map;

namespace TeamLog.Tests
{
    /// <summary>
    /// Phase CC-0: 부활 시스템 단위 테스트.
    /// HealthComponent.Revive / ApplyMaxHpModifier / HealToFull,
    /// GameRunState.ProcessBattleEnd (승리 시 부활, 전멸 시 런 종료),
    /// CombatEventBus.OnPartyMemberRevived 발생 검증.
    /// </summary>
    [TestFixture]
    public class ReviveTests
    {
        [SetUp]
        public void SetUp()
        {
            CombatEventBus.Clear();
            DamageCalculator.ClearEvents();
            SkillExecutor.ClearEvents();
            GameRunState.Destroy();
        }

        [TearDown]
        public void TearDown()
        {
            CombatEventBus.Clear();
            DamageCalculator.ClearEvents();
            SkillExecutor.ClearEvents();
            GameRunState.Destroy();
        }

        // ── HealthComponent ──

        [Test]
        public void Revive_RestoresHP_ByPercentageOfMax()
        {
            var health = new HealthComponent();
            health.Initialize(100);
            health.TakeDamage(100);
            Assert.IsTrue(health.IsDead);

            health.Revive(0.5f);

            Assert.IsTrue(health.IsAlive);
            Assert.AreEqual(50, health.CurrentHP);
        }

        [Test]
        public void Revive_ClearsDeathState()
        {
            var health = new HealthComponent();
            health.Initialize(60);
            health.TakeDamage(60);
            Assert.IsTrue(health.IsDead);

            health.Revive(0.5f);

            Assert.IsFalse(health.IsDead);
            Assert.AreEqual(30, health.CurrentHP);
        }

        [Test]
        public void ApplyMaxHpModifier_ReducesMax_Proportionally()
        {
            var health = new HealthComponent();
            health.Initialize(100);
            health.TakeDamage(20); // HP 80

            health.ApplyMaxHpModifier(0.9f);

            Assert.AreEqual(90, health.MaxHP);
            Assert.AreEqual(80, health.CurrentHP); // 현재 HP는 클램프만 (비례 감소 X)
        }

        [Test]
        public void ApplyMaxHpModifier_MultipleAccumulates_Multiplicatively()
        {
            var health = new HealthComponent();
            health.Initialize(100);

            // 6회 누적: 100 × 0.9^6 = 53.14 → 53
            for (int i = 0; i < 6; i++)
                health.ApplyMaxHpModifier(0.9f);

            Assert.AreEqual(53, health.MaxHP);
        }

        [Test]
        public void HealToFull_RestoresCurrentHP_ToMax()
        {
            var health = new HealthComponent();
            health.Initialize(80);
            health.TakeDamage(30); // HP 50

            health.HealToFull();

            Assert.AreEqual(80, health.CurrentHP);
        }

        [Test]
        public void HealToFull_DoesNothing_WhenDead()
        {
            var health = new HealthComponent();
            health.Initialize(50);
            health.TakeDamage(50);
            Assert.IsTrue(health.IsDead);

            health.HealToFull();

            Assert.IsTrue(health.IsDead);
            Assert.AreEqual(0, health.CurrentHP);
        }

        // ── GameRunState.ProcessBattleEnd ──

        [Test]
        public void ProcessBattleEnd_Victory_HealsSurvivorsToFull()
        {
            var c1 = CreateCharacter(100);
            c1.Health.TakeDamage(30); // HP 70 (생존)
            var c2 = CreateCharacter(80);
            c2.Health.TakeDamage(20); // HP 60 (생존)
            var party = new List<Character> { c1, c2 };

            var runState = GameRunState.Create(party, 0);
            runState.StartRun();

            bool ended = runState.ProcessBattleEnd(victory: true);

            Assert.IsFalse(ended, "승리 시 런 종료 아님");
            Assert.AreEqual(100, c1.Health.CurrentHP);
            Assert.AreEqual(80, c2.Health.CurrentHP);
        }

        [Test]
        public void ProcessBattleEnd_Victory_RevivesDeadMembers_WithHalfHp()
        {
            var alive = CreateCharacter(100);
            var dead = CreateCharacter(80);
            dead.Health.TakeDamage(80); // 사망
            var party = new List<Character> { alive, dead };

            var runState = GameRunState.Create(party, 0);
            runState.StartRun();

            bool ended = runState.ProcessBattleEnd(victory: true);

            Assert.IsFalse(ended);
            Assert.IsTrue(dead.IsAlive, "사망자 부활해야 함");
            Assert.AreEqual(36, dead.Health.CurrentHP, "80 × 0.9 × 0.5 = 36"); // MaxHP 0.9배 후 50%
            Assert.AreEqual(72, dead.Health.MaxHP, "MaxHP 0.9배 누적");
        }

        [Test]
        public void ProcessBattleEnd_Victory_WithAllDead_FallsBackToDefeat()
        {
            // victory=true이지만 파티 전멸 상태 — 안전장치
            var c1 = CreateCharacter(50);
            c1.Health.TakeDamage(50);
            var c2 = CreateCharacter(60);
            c2.Health.TakeDamage(60);
            var party = new List<Character> { c1, c2 };

            var runState = GameRunState.Create(party, 0);
            runState.StartRun();

            bool ended = runState.ProcessBattleEnd(victory: true);

            Assert.IsTrue(ended, "파티 전멸 시 victory=true여도 런 종료");
            Assert.IsFalse(runState.IsRunActive);
        }

        [Test]
        public void ProcessBattleEnd_Defeat_EndsRun()
        {
            var c1 = CreateCharacter(100);
            var party = new List<Character> { c1 };

            var runState = GameRunState.Create(party, 0);
            runState.StartRun();

            bool ended = runState.ProcessBattleEnd(victory: false);

            Assert.IsTrue(ended);
            Assert.IsFalse(runState.IsRunActive);
        }

        [Test]
        public void ProcessBattleEnd_ReviveAccumulates_MaxHpAcrossMultipleBattles()
        {
            // 같은 캐릭터가 연속 2회 사망/부활 — MaxHP 0.9배씩 2회 누적.
            // 생존자 1명을 항상 유지해야 "전멸" 분기에 걸리지 않음.
            var survivor = CreateCharacter(200); // 생존자 (절대 죽지 않음)
            var c = CreateCharacter(100);
            var party = new List<Character> { survivor, c };

            var runState = GameRunState.Create(party, 0);
            runState.StartRun();

            // 1차 사망/부활
            c.Health.TakeDamage(100);
            runState.ProcessBattleEnd(victory: true);
            Assert.AreEqual(90, c.Health.MaxHP, "1회: 100 × 0.9 = 90");

            // 2차 사망/부활
            c.Health.TakeDamage(100);
            runState.ProcessBattleEnd(victory: true);
            Assert.AreEqual(81, c.Health.MaxHP, "2회: 90 × 0.9 = 81");
        }

        // ── CombatEventBus.OnPartyMemberRevived ──

        [Test]
        public void ProcessBattleEnd_FiresOnPartyMemberRevived_PerDeadMember()
        {
            var alive = CreateCharacter(100);
            var dead1 = CreateCharacter(80);
            dead1.Health.TakeDamage(80);
            var dead2 = CreateCharacter(60);
            dead2.Health.TakeDamage(60);
            var party = new List<Character> { alive, dead1, dead2 };

            var runState = GameRunState.Create(party, 0);
            runState.StartRun();

            int fired = 0;
            CombatEventBus.OnPartyMemberRevived += _ => fired++;

            runState.ProcessBattleEnd(victory: true);

            Assert.AreEqual(2, fired, "사망자 2명 각각 부활 이벤트 발생");
        }

        // ── 헬퍼 ──

        private static Character CreateCharacter(int hp)
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            var c = new Character(data);
            c.Health.Initialize(hp);
            c.Stats.Initialize(10, 0);
            return c;
        }
    }
}
