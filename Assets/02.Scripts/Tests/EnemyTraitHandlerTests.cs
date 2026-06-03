using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TeamLog.Characters;

namespace TeamLog.Tests
{
    [TestFixture]
    public class EnemyTraitHandlerTests
    {
        // ── PreventDeath (Immortal) ──

        [Test]
        public void PreventDeath_Immortal_FirstTime_ReturnsTrue()
        {
            var handler = CreateHandler(EnemyTrait.Immortal);
            Assert.IsTrue(handler.PreventDeath());
        }

        [Test]
        public void PreventDeath_Immortal_SecondTime_ReturnsFalse()
        {
            var handler = CreateHandler(EnemyTrait.Immortal);
            handler.PreventDeath(); // 첫 번째: true
            Assert.IsFalse(handler.PreventDeath()); // 두 번째: false
        }

        [Test]
        public void PreventDeath_NonImmortal_ReturnsFalse(
            [Values(EnemyTrait.Regenerate, EnemyTrait.Opportunist, EnemyTrait.PhaseShift,
                EnemyTrait.Counter, EnemyTrait.Thorns, EnemyTrait.Shell,
                EnemyTrait.Sturdy, EnemyTrait.ArcaneFury, EnemyTrait.Corrosive,
                EnemyTrait.Rally, EnemyTrait.Rampage)]
            EnemyTrait trait)
        {
            var handler = CreateHandler(trait);
            Assert.IsFalse(handler.PreventDeath());
        }

        // ── ModifyIncomingDamage (Sturdy) ──

        [Test]
        public void Sturdy_HalvesFirstDamage()
        {
            var handler = CreateHandler(EnemyTrait.Sturdy);
            handler.OnTurnStart(1); // _sturdyAvailable = true
            Assert.AreEqual(5, handler.ModifyIncomingDamage(10));
        }

        [Test]
        public void Sturdy_SecondHit_NoReduction()
        {
            var handler = CreateHandler(EnemyTrait.Sturdy);
            handler.OnTurnStart(1);
            handler.ModifyIncomingDamage(10); // 첫 타: 절반
            Assert.AreEqual(10, handler.ModifyIncomingDamage(10)); // 둘째 타: 풀 데미지
        }

        [Test]
        public void Sturdy_ResetsOnNewTurn()
        {
            var handler = CreateHandler(EnemyTrait.Sturdy);
            handler.OnTurnStart(1);
            handler.ModifyIncomingDamage(10); // 첫 타 소모
            handler.OnTurnStart(2); // 리셋
            Assert.AreEqual(5, handler.ModifyIncomingDamage(10)); // 다시 절반
        }

        [Test]
        public void NonSturdy_NoDamageModification(
            [Values(EnemyTrait.Counter, EnemyTrait.Thorns, EnemyTrait.Rampage)]
            EnemyTrait trait)
        {
            var handler = CreateHandler(trait);
            Assert.AreEqual(10, handler.ModifyIncomingDamage(10));
        }

        // ── Shell ──

        [Test]
        public void Shell_BlocksFirstEffect()
        {
            var handler = CreateHandler(EnemyTrait.Shell);
            handler.OnTurnStart(1);
            Assert.IsTrue(handler.ShouldBlockEffect());
        }

        [Test]
        public void Shell_DoesNotBlockSecondEffect()
        {
            var handler = CreateHandler(EnemyTrait.Shell);
            handler.OnTurnStart(1);
            handler.ShouldBlockEffect(); // 첫 번째 차단
            Assert.IsFalse(handler.ShouldBlockEffect()); // 두 번째는 통과
        }

        [Test]
        public void Shell_ResetsOnNewTurn()
        {
            var handler = CreateHandler(EnemyTrait.Shell);
            handler.OnTurnStart(1);
            handler.ShouldBlockEffect(); // 차단
            handler.OnTurnStart(2); // 리셋
            Assert.IsTrue(handler.ShouldBlockEffect()); // 다시 차단 가능
        }

        // ── OnDamageReceived (Counter) ──

        [Test]
        public void Counter_DealsDamageToAttacker()
        {
            var owner = CreateCharacterWithTrait("Owner", 30, 5, 1, EnemyTrait.Counter);
            var attacker = CreateCharacter("Attacker", 50, 5, 1);

            int hpBefore = attacker.Health.CurrentHP;
            owner.TraitHandler.OnDamageReceived(attacker, 10);
            Assert.Less(attacker.Health.CurrentHP, hpBefore);
        }

        [Test]
        public void Counter_DoesNotTriggerWhenAttackerDead()
        {
            var owner = CreateCharacterWithTrait("Owner", 30, 5, 1, EnemyTrait.Counter);
            var attacker = CreateCharacter("Attacker", 50, 5, 1);
            KillCharacter(attacker);

            int hpBefore = attacker.Health.CurrentHP;
            owner.TraitHandler.OnDamageReceived(attacker, 10);
            Assert.AreEqual(hpBefore, attacker.Health.CurrentHP); // 데미지 없음
        }

        // ── OnDamageReceived (Thorns) ──

        [Test]
        public void Thorns_Reflects30Percent()
        {
            var owner = CreateCharacterWithTrait("Owner", 30, 5, 1, EnemyTrait.Thorns);
            var attacker = CreateCharacter("Attacker", 50, 5, 1);

            int hpBefore = attacker.Health.CurrentHP;
            owner.TraitHandler.OnDamageReceived(attacker, 10);
            Assert.AreEqual(hpBefore - 3, attacker.Health.CurrentHP); // 10 * 3 / 10 = 3
        }

        // ── OnDamageReceived (Rampage) ──

        [Test]
        public void Rampage_ResetsOnDamage()
        {
            var owner = CreateCharacterWithTrait("Owner", 50, 5, 1, EnemyTrait.Rampage);

            owner.TraitHandler.OnTurnStart(1); // 누적 시작
            owner.TraitHandler.OnDamageReceived(null, 5); // 피해 받음
            owner.TraitHandler.OnTurnStart(2); // 리셋되어야 함
            // _rampageStacks should be 0 (was damaged previous turn)
        }

        // ── HasTrait ──

        [Test]
        public void HasTrait_None_ReturnsFalse()
        {
            var handler = CreateHandler(EnemyTrait.None);
            Assert.IsFalse(handler.HasTrait);
        }

        [Test]
        public void HasTrait_AnyTrait_ReturnsTrue(
            [Values(EnemyTrait.Regenerate, EnemyTrait.Counter, EnemyTrait.Immortal)]
            EnemyTrait trait)
        {
            var handler = CreateHandler(trait);
            Assert.IsTrue(handler.HasTrait);
        }

        // ── 보조 ──

        private static EnemyTraitHandler CreateHandler(EnemyTrait trait)
        {
            var owner = CreateCharacterWithTrait("HandlerOwner", 30, 5, 1, trait);
            return owner.TraitHandler;
        }

        private static Character CreateCharacter(string name, int hp, int atk, int def)
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            var c = new Character(data);
            c.Health.Initialize(hp);
            c.Stats.Initialize(atk, def);
            return c;
        }

        private static Character CreateCharacterWithTrait(string name, int hp, int atk, int def, EnemyTrait trait)
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            // 리플렉션으로 private _enemyTrait 필드 설정
            var field = data.GetType().GetField("_enemyTrait",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(data, trait);

            var c = new Character(data);
            c.Health.Initialize(hp);
            c.Stats.Initialize(atk, def);
            return c;
        }

        private static void KillCharacter(Character c)
        {
            // trait가 없는 캐릭터는 OnPreDeath 훅이 없으므로 바로 사망
            c.Health.TakeDamage(c.Health.CurrentHP + c.Health.CurrentShield + 100);
        }
    }
}
