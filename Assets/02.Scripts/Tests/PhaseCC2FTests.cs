using System;
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
    /// Phase CC-2F Mortis, the Necromancer 핵심 메카닉 검증.
    /// - CorpseComponent: 기본 4스킬 초기화, 자동 행동, Necromancer 사망 시 비활성화
    /// - EmpowerNext/MassEmpower/KillEmpower 가산
    /// - Soul Link 회복, TickSoulLink 턴 감소
    /// - 특성 SoulLinkMul 적용 (생명력 흡수)
    /// - 시체 슬롯 교체
    ///
    /// 기획: Assets/09.Docs/Characters/ReworkDrafts/04_Necromancer.md
    /// </summary>
    [TestFixture]
    public class PhaseCC2FTests
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
        // 1. CorpseComponent 기본 초기화
        // ═══════════════════════════════════════════

        [Test]
        public void Corpse_Initial_FourSlotsIsActive()
        {
            var (mortis, party) = CreateNecromancerParty();

            Assert.IsNotNull(mortis.Corpse, "Necromancer는 시체 보유");
            Assert.IsTrue(mortis.Corpse.IsActive, "초기 시체 활성 상태");
            Assert.AreEqual(4, mortis.Corpse.Slots.Count, "시체 슬롯 4개");
            Assert.IsNotNull(mortis.Corpse.Slots[0], "슬롯 0 스킬 존재");
            Assert.IsNotNull(mortis.Corpse.Slots[3], "슬롯 3 스킬 존재");
        }

        [Test]
        public void Corpse_GetRandomSkill_ReturnsSlotSkill()
        {
            var (mortis, party) = CreateNecromancerParty();

            // 100회 시도 — 항상 슬롯 내 스킬 중 하나여야 함
            var slotSkills = new HashSet<SkillData>();
            for (int i = 0; i < 4; i++) slotSkills.Add(mortis.Corpse.Slots[i]);

            for (int i = 0; i < 100; i++)
            {
                var skill = mortis.Corpse.GetRandomSkill();
                Assert.IsNotNull(skill, "무작위 스킬 null 아님");
                CollectionAssert.Contains(slotSkills, skill, "무작위 스킬은 슬롯 내 스킬 중 하나");
            }
        }

        // ═══════════════════════════════════════════
        // 2. Necromancer 사망 → 시체 비활성화
        // ═══════════════════════════════════════════

        [Test]
        public void Corpse_DeactivateOnNecromancerDeath()
        {
            var (mortis, party) = CreateNecromancerParty();
            Assert.IsTrue(mortis.Corpse.IsActive, "사망 전 시체 활성");

            // Necromancer 사망 — Health의 OnDeath 이벤트로 자동 Deactivate
            mortis.Health.TakeDamage(mortis.Health.CurrentHP);

            Assert.IsTrue(mortis.IsDead, "Necromancer 사망 상태");
            Assert.IsFalse(mortis.Corpse.IsActive, "Necromancer 사망 시 시체 비활성화");
        }

        // ═══════════════════════════════════════════
        // 3. EmpowerNext / MassEmpower / KillEmpower
        // ═══════════════════════════════════════════

        [Test]
        public void Corpse_EmpowerNext_ConsumedOnGet()
        {
            var (mortis, party) = CreateNecromancerParty();

            mortis.Corpse.ApplyEmpowerNext(5);
            Assert.AreEqual(5, mortis.Corpse.EmpowerBonusNext, "EmpowerNext 설정");

            int consumed = mortis.Corpse.ConsumeEmpowerNext();
            Assert.AreEqual(5, consumed, "Consume 반환값");
            Assert.AreEqual(0, mortis.Corpse.EmpowerBonusNext, "Consume 후 0");
        }

        [Test]
        public void Corpse_MassEmpower_PersistentBonus()
        {
            var (mortis, party) = CreateNecromancerParty();

            mortis.Corpse.ApplyMassEmpower(3);
            Assert.AreEqual(3, mortis.Corpse.MassEmpowerBonus, "MassEmpower 설정");

            // GetEffectivePower에 반영되는지 확인
            var skill = mortis.Corpse.Slots[0];
            int expected = skill.Power + 3;
            Assert.AreEqual(expected, mortis.Corpse.GetEffectivePower(skill),
                "GetEffectivePower에 MassEmpower 반영");
        }

        [Test]
        public void Corpse_KillEmpower_AppliedToEffectivePower()
        {
            var (mortis, party) = CreateNecromancerParty();

            mortis.Corpse.ApplyKillEmpower(2);
            Assert.AreEqual(2, mortis.Corpse.KillEmpowerBonus, "KillEmpower 설정");

            var skill = mortis.Corpse.Slots[0];
            int expected = skill.Power + 2;
            Assert.AreEqual(expected, mortis.Corpse.GetEffectivePower(skill),
                "GetEffectivePower에 KillEmpower 반영");
        }

        // ═══════════════════════════════════════════
        // 4. Soul Link — 회복 비율 + TickSoulLink 턴 감소
        // ═══════════════════════════════════════════

        [Test]
        public void SoulLink_TickSoulLink_DecrementsTurns()
        {
            var (mortis, party) = CreateNecromancerParty();
            Assert.AreEqual(0, mortis.Corpse.SoulLinkRemainingTurns, "초기 SoulLink 0턴");

            mortis.Corpse.SoulLinkRemainingTurns = 2;
            Assert.AreEqual(2, mortis.Corpse.SoulLinkRemainingTurns, "SoulLink 2턴 설정");

            mortis.Corpse.TickSoulLink();
            Assert.AreEqual(1, mortis.Corpse.SoulLinkRemainingTurns, "TickSoulLink → 1턴 감소");

            mortis.Corpse.TickSoulLink();
            Assert.AreEqual(0, mortis.Corpse.SoulLinkRemainingTurns, "TickSoulLink → 0턴");

            mortis.Corpse.TickSoulLink();
            Assert.AreEqual(0, mortis.Corpse.SoulLinkRemainingTurns, "0턴에서 추가 Tick → 음수 아님");
        }

        [Test]
        public void SoulLinkMul_Trait_AppliedToCorpse()
        {
            var (mortis, party) = CreateNecromancerParty();
            Assert.AreEqual(0.5f, mortis.Corpse.SoulLinkMul, 0.001f, "기본 SoulLink 배율 0.5");

            var trait = CreateTrait((KeywordType.SoulLinkMul, 0.75f, KeywordTrigger.Passive, 0f));
            mortis.EquipTrait(trait);

            Assert.AreEqual(0.75f, mortis.Corpse.SoulLinkMul, 0.001f,
                "생명력 흡수 특성 — SoulLink 배율 0.75");
        }

        // ═══════════════════════════════════════════
        // 5. 시체 슬롯 교체
        // ═══════════════════════════════════════════

        [Test]
        public void Corpse_ReplaceSlot_SwapsSkill()
        {
            var (mortis, party) = CreateNecromancerParty();
            var original = mortis.Corpse.Slots[1];

            var newSkill = CreateTestSkill("빼앗은 스킬", SkillType.Attack, 8);
            mortis.Corpse.ReplaceSlot(1, newSkill);

            Assert.AreSame(newSkill, mortis.Corpse.Slots[1], "슬롯 1이 새 스킬로 교체됨");
            Assert.AreNotSame(original, mortis.Corpse.Slots[1], "원래 스킬과 다름");
        }

        [Test]
        public void Corpse_ResetToBaseSkills_RestoresOriginalSlots()
        {
            var (mortis, party) = CreateNecromancerParty();
            var originalSlot0 = mortis.Corpse.Slots[0];

            // 슬롯 교체
            var newSkill = CreateTestSkill("빼앗은 스킬", SkillType.Attack, 8);
            mortis.Corpse.ReplaceSlot(0, newSkill);
            Assert.AreNotSame(originalSlot0, mortis.Corpse.Slots[0], "교체 후 슬롯 0 변경됨");

            // 리셋
            mortis.Corpse.ResetToBaseSkills();
            Assert.AreSame(originalSlot0, mortis.Corpse.Slots[0], "리셋 후 원래 스킬로 복원");
            Assert.AreEqual(0, mortis.Corpse.EmpowerBonusNext, "리셋 후 EmpowerNext 0");
            Assert.AreEqual(0, mortis.Corpse.MassEmpowerBonus, "리셋 후 MassEmpower 0");
            Assert.AreEqual(0, mortis.Corpse.SoulLinkRemainingTurns, "리셋 후 SoulLink 0턴");
            Assert.IsTrue(mortis.Corpse.IsActive, "리셋 후 시체 활성");
        }

        // ═══════════════════════════════════════════
        // 헬퍼
        // ═══════════════════════════════════════════

        /// <summary>Mortis(Necromancer) + 더미 파티 3명 + 더미 적 2명 생성.</summary>
        private static (Character necromancer, List<Character> party) CreateNecromancerParty()
        {
            var necromancer = CreateNecromancerCharacter();
            var member1 = CreateCharacter(120, 0, 0); // Duran equivalent
            var member2 = CreateCharacter(75, 0, 0);  // Ashe equivalent
            var member3 = CreateCharacter(70, 0, 0);  // 일반

            var party = new List<Character> { necromancer, member1, member2, member3 };
            var runState = GameRunState.Create(party, 0);
            runState.RelicHandler.SetPlayerParty(party);
            runState.RelicHandler.SubscribeEvents();

            return (necromancer, party);
        }

        private static Character CreateNecromancerCharacter()
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            SetPrivateField(data, "_resourceType", ResourceType.None);
            // 시체 기본 스킬 4종 세팅
            var corpseSkills = new List<SkillData>
            {
                CreateTestSkill("할퀴기", SkillType.Attack, 4),
                CreateTestSkill("독 물기", SkillType.Attack, 3),
                CreateTestSkill("뼈 던지기", SkillType.Attack, 4),
                CreateTestSkill("기절 타격", SkillType.Attack, 2),
            };
            SetPrivateField(data, "_corpseBaseSkills", corpseSkills);

            var character = new Character(data);
            character.Health.Initialize(75);
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

        private static SkillData CreateTestSkill(string name, SkillType type, int power)
        {
            var skill = ScriptableObject.CreateInstance<SkillData>();
            SetPrivateField(skill, "_skillName", name);
            SetPrivateField(skill, "_skillType", type);
            SetPrivateField(skill, "_targetType", TargetType.SingleEnemy);
            SetPrivateField(skill, "_power", power);
            SetPrivateField(skill, "_cost", 0);
            return skill;
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
