using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Map;
using TeamLog.Skill;

namespace TeamLog.Tests
{
    /// <summary>
    /// Phase CC 자원 컴포넌트 검증 (Ember/Vengeance/Frost).
    /// </summary>
    [TestFixture]
    public class ResourceComponentTests
    {
        [SetUp]
        public void SetUp()
        {
            CombatEventBus.Clear();
            GameRunState.Destroy();
        }

        [TearDown]
        public void TearDown()
        {
            CombatEventBus.Clear();
            GameRunState.Destroy();
        }

        // ═══════════════════════════════════════════
        // Ember (Ashe) — 매 턴 +1, 턴 종료 시 Ember×2 자해
        // ═══════════════════════════════════════════

        [Test]
        public void Ember_OnTurnStart_AddsOneStack()
        {
            var ashe = CreateCharacter(ResourceType.Ember);
            Assert.AreEqual(0, ashe.Resource.CurrentStacks, "초기 Ember=0");

            ashe.Resource.OnTurnStart(ashe);
            Assert.AreEqual(1, ashe.Resource.CurrentStacks, "턴 시작 후 Ember=1");

            ashe.Resource.OnTurnStart(ashe);
            Assert.AreEqual(2, ashe.Resource.CurrentStacks, "2턴 후 Ember=2");
        }

        [Test]
        public void Ember_OnTurnEnd_DealsSelfDamage()
        {
            var ashe = CreateCharacter(ResourceType.Ember);
            // Ember 3 세팅
            ashe.Resource.AddStacks(3);
            int hpBefore = ashe.Health.CurrentHP;

            ashe.Resource.OnTurnEnd(ashe);

            int damage = hpBefore - ashe.Health.CurrentHP;
            Assert.AreEqual(6, damage, "Ember 3 → 자해 6 (3×2)");
        }

        [Test]
        public void Ember_MaxStacks_ClampedTo5()
        {
            var ashe = CreateCharacter(ResourceType.Ember);
            ashe.Resource.AddStacks(10);
            Assert.AreEqual(5, ashe.Resource.CurrentStacks, "Ember 최대 5스택");
        }

        // ═══════════════════════════════════════════
        // Vengeance (Duran) — 피격 시 데미지 1:1 축적
        // ═══════════════════════════════════════════

        [Test]
        public void Vengeance_OnDamageTaken_Accumulates()
        {
            var duran = CreateCharacter(ResourceType.Vengeance);
            Assert.AreEqual(0, duran.Resource.CurrentStacks);

            duran.Resource.OnDamageTaken(duran, 15);
            Assert.AreEqual(15, duran.Resource.CurrentStacks, "15 데미지 → Vengeance 15");

            duran.Resource.OnDamageTaken(duran, 5);
            Assert.AreEqual(20, duran.Resource.CurrentStacks, "+5 데미지 → Vengeance 20 (최대)");
        }

        [Test]
        public void Vengeance_MaxStacks_ClampedTo20()
        {
            var duran = CreateCharacter(ResourceType.Vengeance);
            duran.Resource.OnDamageTaken(duran, 100);
            Assert.AreEqual(20, duran.Resource.CurrentStacks, "Vengeance 최대 20스택");
        }

        // ═══════════════════════════════════════════
        // Frost (Lumi) — 턴 종료 시 절반 소실
        // ═══════════════════════════════════════════

        [Test]
        public void Frost_OnTurnEnd_HalvesStacks()
        {
            var lumi = CreateCharacter(ResourceType.Frost);
            lumi.Resource.AddStacks(3); // 최대 3
            Assert.AreEqual(3, lumi.Resource.CurrentStacks);

            lumi.Resource.OnTurnEnd(lumi);
            // "절반 소실": loss = Max(1, 3/2) = Max(1, 1) = 1. 남은 스택 = 3 - 1 = 2.
            Assert.AreEqual(2, lumi.Resource.CurrentStacks, "Frost 3 → 절반(1) 소실 → 2 남음");
        }

        [Test]
        public void Frost_MaxStacks_ClampedTo3()
        {
            var lumi = CreateCharacter(ResourceType.Frost);
            lumi.Resource.AddStacks(10);
            Assert.AreEqual(3, lumi.Resource.CurrentStacks, "Frost 최대 3스택");
        }

        // ═══════════════════════════════════════════
        // Character.CreateResource 자동 인스턴스화
        // ═══════════════════════════════════════════

        [Test]
        public void Character_EmberResourceType_CreatesEmberComponent()
        {
            var ashe = CreateCharacter(ResourceType.Ember);
            Assert.IsNotNull(ashe.Resource, "ResourceType.Ember → Resource null 아님");
            Assert.AreEqual(ResourceType.Ember, ashe.Resource.Resource);
            Assert.IsInstanceOf<EmberResourceComponent>(ashe.Resource);
        }

        [Test]
        public void Character_NoneResourceType_CreatesNullResource()
        {
            var warrior = CreateCharacter(ResourceType.None);
            Assert.IsNull(warrior.Resource, "ResourceType.None → Resource null");
        }

        // ═══════════════════════════════════════════
        // 헬퍼
        // ═══════════════════════════════════════════

        private static Character CreateCharacter(ResourceType resourceType)
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            SetPrivateField(data, "_resourceType", resourceType);
            var character = new Character(data);
            character.Health.Initialize(100);
            character.Stats.Initialize(0, 0);
            return character;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"필드 '{fieldName}'을 찾을 수 없음");
            field.SetValue(obj, value);
        }
    }
}
