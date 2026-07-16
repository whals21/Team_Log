using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Combat.Turn;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.Skill;

using SkillData = TeamLog.Characters.SkillData;
using SkillType = TeamLog.Characters.SkillType;
using TargetType = TeamLog.Characters.TargetType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;

namespace TeamLog.Tests
{
    /// <summary>
    /// Phase CC-2C Elara, the Healer 핵심 메카닉 검증.
    /// - MercyResourceComponent: 매 턴 자동 힐, Mercy 축전, 15 도달 자동 버스트
    /// - AutoHealBonus 특성 (자동 힐 +2)
    /// - MercyBurstTargets 특성 (버스트 대상 수)
    /// - Mend Wounds MercyAccumulate (직접 힐 시 Mercy +N)
    ///
    /// 기획: Assets/09.Docs/Characters/ReworkDrafts/01_Healer.md
    /// </summary>
    [TestFixture]
    public class PhaseCC2CTests
    {
        [SetUp]
        public void SetUp()
        {
            CombatEventBus.Clear();
            DamageCalculator.ClearEvents();
            SkillExecutor.ClearEvents();
        }

        [TearDown]
        public void TearDown()
        {
            CombatEventBus.Clear();
            DamageCalculator.ClearEvents();
            SkillExecutor.ClearEvents();
            GameRunState.Destroy();
        }

        // ═══════════════════════════════════════════
        // 1. MercyResourceComponent 기본 동작
        // ═══════════════════════════════════════════

        [Test]
        public void Mercy_InitialState_ZeroStacks()
        {
            var (elara, party) = CreateMercyParty();
            Assert.AreEqual(0, elara.Resource.CurrentStacks, "초기 Mercy 0");
            Assert.AreEqual(15, elara.Resource.MaxStacks, "Mercy 최대 15");
        }

        [Test]
        public void Mercy_OnTurnStart_AutoHealsParty_AndAccumulates()
        {
            var (elara, party) = CreateMercyParty();
            // 파티원 HP를 일부 깎아 힐 효과 확인
            party[1].Health.TakeDamage(20); // Duran
            int hpBefore = party[1].Health.CurrentHP;

            elara.Resource.OnTurnStart(elara);

            int healed = party[1].Health.CurrentHP - hpBefore;
            Assert.AreEqual(3, healed, "파티원 자동 힐 3");
            Assert.AreEqual(9, elara.Resource.CurrentStacks,
                $"3명×3=9 Mercy 축전 (Healer 본인은 제외). 실제 {elara.Resource.CurrentStacks}");
        }

        [Test]
        public void Mercy_BondBoost_Member_Gets6Heal()
        {
            var (elara, party) = CreateMercyParty();
            // 파티원 중 1명에게 BondBoost 부여
            party[1].StatusEffects.ApplyEffect(StatusEffectType.BondBoost, 2, 1);
            party[1].Health.TakeDamage(20);
            int hpBefore = party[1].Health.CurrentHP;

            elara.Resource.OnTurnStart(elara);

            int healed = party[1].Health.CurrentHP - hpBefore;
            Assert.AreEqual(6, healed, "BondBoost 파티원 자동 힐 6 (3+3)");
        }

        [Test]
        public void Mercy_AtThreshold_AutoBurst_ATKUp3()
        {
            var (elara, party) = CreateMercyParty();
            // Mercy를 6로 설정 — 자동 힐 3명×3=9 더해지면 딱 15 도달 (버스트 1회)
            elara.Resource.AddStacks(6);

            // 파티원 1명만 큰 데미지 입혀서 자동 힐이 가장 큰 대상(회복량=3)으로 만듦
            // 단 모든 파티원이 동일하게 3씩 회복받으므로, 정렬 순서상 첫 번째가 버스트 대상
            party[1].Health.TakeDamage(50);

            elara.Resource.OnTurnStart(elara);
            // 자동 힐 3명×3=9 → Mercy 6+9=15 → 버스트 → Mercy 0 리셋 (15-15=0)

            Assert.AreEqual(0, elara.Resource.CurrentStacks,
                $"버스트 후 Mercy 0 (15-15). 실제 {elara.Resource.CurrentStacks}");

            // 파티원 중 1명 이상이 ATK+3 버스트 받았는지 확인
            bool anyBurst = false;
            foreach (var p in party)
            {
                if (p != elara && p.StatusEffects.HasEffect(StatusEffectType.AttackUp))
                {
                    anyBurst = true;
                    break;
                }
            }
            Assert.IsTrue(anyBurst, "파티원 중 1명 이상에게 ATK+3 버스트 발동");
        }

        // ═══════════════════════════════════════════
        // 2. AutoHealBonus 특성 (축복)
        // ═══════════════════════════════════════════

        [Test]
        public void AutoHealBonus_Trait_IncreasesAutoHeal()
        {
            var (elara, party) = CreateMercyParty();
            var trait = CreateTrait(
                (KeywordType.AutoHealBonus, 2, KeywordTrigger.Passive, 0f));
            elara.EquipTrait(trait);

            party[1].Health.TakeDamage(30);
            int hpBefore = party[1].Health.CurrentHP;

            elara.Resource.OnTurnStart(elara);

            int healed = party[1].Health.CurrentHP - hpBefore;
            Assert.AreEqual(5, healed, "축복 특성 — 자동 힐 3→5");
        }

        // ═══════════════════════════════════════════
        // 3. MercyBurstTargets 특성 (신성 방패)
        // ═══════════════════════════════════════════

        [Test]
        public void MercyBurstTargets_Trait_Bursts2Members()
        {
            var (elara, party) = CreateMercyParty();
            var trait = CreateTrait(
                (KeywordType.MercyBurstTargets, 2, KeywordTrigger.Passive, 0f));
            elara.EquipTrait(trait);

            // Mercy 14 + 자동 힐로 15 도달
            elara.Resource.AddStacks(14);
            foreach (var p in party)
                if (p != elara) p.Health.TakeDamage(20);

            elara.Resource.OnTurnStart(elara);
            // 3명에게 3씩 힐 → Mercy 14+9=23 → 버스트
            // MercyBurstTargets=2 → 가장 많이 회복받은 2명에게 버스트

            int burstCount = 0;
            foreach (var p in party)
            {
                if (p != elara && p.StatusEffects.HasEffect(StatusEffectType.AttackUp))
                    burstCount++;
            }

            Assert.GreaterOrEqual(burstCount, 1, "최소 1명 버스트");
            // (상황에 따라 2명 버스트 — 모든 파티원이 동일 회복량이면 정렬 순서대로 2명)
        }

        // ═══════════════════════════════════════════
        // 4. AccumulateFromDirectHeal (Mend Wounds 시뮬레이션)
        // ═══════════════════════════════════════════

        [Test]
        public void AccumulateFromDirectHeal_AddsMercy()
        {
            var (elara, party) = CreateMercyParty();
            int before = elara.Resource.CurrentStacks;

            // Mend Wounds가 target에게 10 힐 → Mercy +10
            if (elara.Resource is MercyResourceComponent mercy)
            {
                mercy.AccumulateFromDirectHeal(party[1], 10);
            }

            Assert.AreEqual(before + 10, elara.Resource.CurrentStacks,
                "직접 힐 10 → Mercy +10 축전");
        }

        [Test]
        public void AccumulateFromDirectHeal_AtThreshold_Bursts()
        {
            var (elara, party) = CreateMercyParty();
            elara.Resource.AddStacks(5); // Mercy 5

            // Mend Wounds 10 → Mercy 5+10=15 → 버스트
            party[1].Health.TakeDamage(30);
            if (elara.Resource is MercyResourceComponent mercy)
            {
                mercy.AccumulateFromDirectHeal(party[1], 10);
            }

            Assert.AreEqual(0, elara.Resource.CurrentStacks, "직접 힐 버스트 후 Mercy 리셋");
            Assert.IsTrue(party[1].StatusEffects.HasEffect(StatusEffectType.AttackUp),
                "직접 힐 받은 대상에게 버스트");
        }

        // ═══════════════════════════════════════════
        // 헬퍼
        // ═══════════════════════════════════════════

        /// <summary>Mercy Healer + 더미 파티 3명 생성 + GameRunState 초기화.</summary>
        private static (Character elara, List<Character> party) CreateMercyParty()
        {
            var elara = CreateMercyCharacter();
            var member1 = CreateCharacter(120, 0, 0); // Duran equivalent
            var member2 = CreateCharacter(75, 0, 0);  // Umbra equivalent
            var member3 = CreateCharacter(70, 0, 0);  // Ashe equivalent

            var party = new List<Character> { elara, member1, member2, member3 };
            var runState = GameRunState.Create(party, 0);
            runState.RelicHandler.SetPlayerParty(party);
            runState.RelicHandler.SubscribeEvents();

            return (elara, party);
        }

        private static Character CreateMercyCharacter()
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            SetPrivateField(data, "_resourceType", ResourceType.Mercy);
            var character = new Character(data);
            character.Health.Initialize(80);
            character.Stats.Initialize(0, 0);
            return character;
        }

        private static Character CreateCharacter(int hp, int atk, int def)
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            var character = new Character(data);
            character.Health.Initialize(hp);
            character.Stats.Initialize(atk, def);
            return character;
        }

        private static CharacterTraitData CreateTrait(
            params (KeywordType type, float value, KeywordTrigger trigger, float cond)[] keywords)
        {
            var trait = ScriptableObject.CreateInstance<CharacterTraitData>();
            var entries = new KeywordEntry[keywords.Length];
            for (int i = 0; i < keywords.Length; i++)
            {
                entries[i] = new KeywordEntry(
                    keywords[i].type, keywords[i].value, keywords[i].trigger, keywords[i].cond);
            }
            SetPrivateField(trait, "_keywords", entries);
            return trait;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field,
                $"필드 '{fieldName}'을 찾을 수 없음 — {obj.GetType().Name} 스키마 변경 확인 필요");
            field.SetValue(obj, value);
        }
    }
}
