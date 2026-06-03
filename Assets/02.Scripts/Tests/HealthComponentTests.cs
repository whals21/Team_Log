using NUnit.Framework;
using TeamLog.Characters;

namespace TeamLog.Tests
{
    [TestFixture]
    public class HealthComponentTests
    {
        private HealthComponent health;

        [SetUp]
        public void SetUp()
        {
            health = new HealthComponent();
            health.Initialize(30);
        }

        // ── 기본 상태 ──

        [Test]
        public void Initialize_SetsHPAndMaxHP()
        {
            Assert.AreEqual(30, health.CurrentHP);
            Assert.AreEqual(30, health.MaxHP);
            Assert.IsFalse(health.IsDead);
            Assert.IsTrue(health.IsAlive);
            Assert.AreEqual(0, health.CurrentShield);
        }

        // ── TakeDamage ──

        [Test]
        public void TakeDamage_ReducesHP()
        {
            health.TakeDamage(10);
            Assert.AreEqual(20, health.CurrentHP);
            Assert.IsFalse(health.IsDead);
        }

        [Test]
        public void TakeDamage_KillsWhenHPReachesZero()
        {
            health.TakeDamage(30);
            Assert.AreEqual(0, health.CurrentHP);
            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void TakeDamage_KillsWhenHPGoesBelowZero()
        {
            health.TakeDamage(50);
            Assert.AreEqual(0, health.CurrentHP);
            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void TakeDamage_DoesNothingWhenAlreadyDead()
        {
            health.TakeDamage(30);
            Assert.IsTrue(health.IsDead);

            bool eventFired = false;
            health.OnDamageTaken += _ => eventFired = true;
            health.TakeDamage(10);
            Assert.IsFalse(eventFired);
        }

        // ── 쉴드 ──

        [Test]
        public void TakeDamage_ShieldAbsorbsAllDamage()
        {
            health.AddShield(10);
            health.TakeDamage(5);
            Assert.AreEqual(30, health.CurrentHP);
            Assert.AreEqual(5, health.CurrentShield);
        }

        [Test]
        public void TakeDamage_ShieldAbsorbsPartialDamage()
        {
            health.AddShield(5);
            health.TakeDamage(10);
            Assert.AreEqual(25, health.CurrentHP);
            Assert.AreEqual(0, health.CurrentShield);
        }

        // ── OnPreDeath 훅 ──

        [Test]
        public void TakeDamage_OnPreDeath_True_PreventsDeath()
        {
            health.OnPreDeath += () => true;
            health.TakeDamage(50);
            Assert.IsFalse(health.IsDead);
            Assert.AreEqual(1, health.CurrentHP); // HP=1로 생존
        }

        [Test]
        public void TakeDamage_OnPreDeath_False_AllowsDeath()
        {
            health.OnPreDeath += () => false;
            health.TakeDamage(50);
            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void TakeDamage_NoPreDeathHook_AllowsDeath()
        {
            // OnPreDeath에 아무것도 구독하지 않음
            health.TakeDamage(50);
            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void TakeDamage_Immortal_OnceOnly()
        {
            bool used = false;
            health.OnPreDeath += () =>
            {
                if (used) return false;
                used = true;
                return true;
            };

            // 첫 번째 치명적 데미지: 생존
            health.TakeDamage(50);
            Assert.IsFalse(health.IsDead);
            Assert.AreEqual(1, health.CurrentHP);

            // 두 번째 데미지: 사망
            health.TakeDamage(1);
            Assert.IsTrue(health.IsDead);
        }

        // ── Heal ──

        [Test]
        public void Heal_IncreasesHP()
        {
            health.TakeDamage(10);
            health.Heal(5);
            Assert.AreEqual(25, health.CurrentHP);
        }

        [Test]
        public void Heal_CappedAtMaxHP()
        {
            health.TakeDamage(5);
            health.Heal(100);
            Assert.AreEqual(30, health.CurrentHP);
        }

        [Test]
        public void Heal_DoesNothingWhenDead()
        {
            health.TakeDamage(30);
            health.Heal(10);
            Assert.AreEqual(0, health.CurrentHP);
            Assert.IsTrue(health.IsDead);
        }

        // ── 이벤트 ──

        [Test]
        public void OnDeath_FiresWhenKilled()
        {
            bool deathFired = false;
            health.OnDeath += () => deathFired = true;
            health.TakeDamage(30);
            Assert.IsTrue(deathFired);
        }

        [Test]
        public void OnDeath_DoesNotFireWhenPrevented()
        {
            bool deathFired = false;
            health.OnPreDeath += () => true;
            health.OnDeath += () => deathFired = true;
            health.TakeDamage(50);
            Assert.IsFalse(deathFired);
        }
    }
}
