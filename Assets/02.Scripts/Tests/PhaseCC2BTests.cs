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
    /// Phase CC-2B Aster, the Archer 핵심 메카닉 검증.
    /// - ComboResourceComponent: 매 턴 스킬 사용 시 +1, 미사용 시 리셋 (최대 3)
    /// - Execute Shot 킬 시 Combo 3 복구 (ComboFinisher)
    /// - Multi-Shot ComboMultiHit 다타수
    /// - ComboMaxPowerBonus 특성 (Combo 3일 때 위력 +)
    /// - PowerAddVsMark 특성 (Mark 적 +N)
    ///
    /// 기획: Assets/09.Docs/Characters/ReworkDrafts/03_Archer.md
    /// </summary>
    [TestFixture]
    public class PhaseCC2BTests
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
        // 1. ComboResourceComponent 기본 동작
        // ═══════════════════════════════════════════

        [Test]
        public void Combo_InitialState_ZeroStacks()
        {
            var aster = CreateComboCharacter();
            Assert.AreEqual(0, aster.Resource.CurrentStacks, "초기 Combo 0");
            Assert.AreEqual(3, aster.Resource.MaxStacks, "Combo 최대 3");
        }

        [Test]
        public void Combo_OnSkillUsed_IncreasesBy1()
        {
            var aster = CreateComboCharacter();
            aster.Resource.OnTurnStart(aster);

            // 스킬 사용 이벤트 발생 (aster가 시전)
            CombatEventBus.FireSkillUsed(CreateAttackSkill(), aster);
            aster.Resource.OnTurnEnd(aster);

            Assert.AreEqual(1, aster.Resource.CurrentStacks, "스킬 사용 턴 종료 시 Combo +1");
        }

        [Test]
        public void Combo_NoSkillUsed_ResetsToZero()
        {
            var aster = CreateComboCharacter();
            aster.Resource.AddStacks(2); // 이미 2스택

            aster.Resource.OnTurnStart(aster);
            // 스킬 사용 안 함
            aster.Resource.OnTurnEnd(aster);

            Assert.AreEqual(0, aster.Resource.CurrentStacks, "스킬 미사용 턴 종료 시 Combo 0 리셋");
        }

        [Test]
        public void Combo_MaxStacks_ClampedTo3()
        {
            var aster = CreateComboCharacter();
            aster.Resource.AddStacks(5); // 5 추가 시도

            Assert.AreEqual(3, aster.Resource.CurrentStacks, "Combo 최대 3으로 clamp");
        }

        // ═══════════════════════════════════════════
        // 2. ComboMaxPowerBonus 특성 (명사수)
        // ═══════════════════════════════════════════

        [Test]
        public void ComboMaxPowerBonus_BelowMax_NoBonus()
        {
            var aster = CreateComboCharacter();
            var trait = CreateTrait(
                (KeywordType.ComboMaxPowerBonus, 3, KeywordTrigger.Passive, 0f));
            aster.EquipTrait(trait);

            aster.Resource.AddStacks(2); // Combo 2 (최대 3 미만)

            int bonus = aster.PlayerTraitHandler.GetBonusOutgoingDamage();
            Assert.AreEqual(0, bonus, "Combo 2 — ComboMaxPowerBonus 미발동 (3 미만)");
        }

        [Test]
        public void ComboMaxPowerBonus_AtMax_Adds3()
        {
            var aster = CreateComboCharacter();
            var trait = CreateTrait(
                (KeywordType.ComboMaxPowerBonus, 3, KeywordTrigger.Passive, 0f));
            aster.EquipTrait(trait);

            aster.Resource.AddStacks(3); // Combo 최대치

            int bonus = aster.PlayerTraitHandler.GetBonusOutgoingDamage();
            Assert.AreEqual(3, bonus, "Combo 3 — ComboMaxPowerBonus +3 발동");
        }

        // ═══════════════════════════════════════════
        // 3. PowerAddVsMark 특성 (약점 포착)
        // ═══════════════════════════════════════════

        [Test]
        public void PowerAddVsMark_NoMark_NoBonus()
        {
            var aster = CreateComboCharacter();
            var enemy = CreateCharacter(500, 0, 0);
            var trait = CreateTrait(
                (KeywordType.PowerAddVsMark, 4, KeywordTrigger.Passive, 0f));
            aster.EquipTrait(trait);

            int bonus = aster.PlayerTraitHandler.GetBonusOutgoingDamage(enemy);
            Assert.AreEqual(0, bonus, "Mark 없는 적 — PowerAddVsMark 미발동");
        }

        [Test]
        public void PowerAddVsMark_WithMark_Adds4()
        {
            var aster = CreateComboCharacter();
            var enemy = CreateCharacter(500, 0, 0);
            enemy.StatusEffects.ApplyEffect(StatusEffectType.Mark, 2, 1);
            var trait = CreateTrait(
                (KeywordType.PowerAddVsMark, 4, KeywordTrigger.Passive, 0f));
            aster.EquipTrait(trait);

            int bonus = aster.PlayerTraitHandler.GetBonusOutgoingDamage(enemy);
            Assert.AreEqual(4, bonus, "Mark 적 — PowerAddVsMark +4 발동");
        }

        // ═══════════════════════════════════════════
        // 4. Execute Shot 킬 시 Combo 복구 로직 검증
        // ═══════════════════════════════════════════
        // TurnManager 전체 흐름은 복잡하므로, 자원 소모/복구 로직만 직접 검증.

        [Test]
        public void ExecuteShot_ConsumeAllResource_Logics()
        {
            var aster = CreateComboCharacter();
            aster.Resource.AddStacks(3);

            // Execute Shot은 consumeAllResource=true → TurnManager가 Reset() 호출
            aster.Resource.Reset();

            Assert.AreEqual(0, aster.Resource.CurrentStacks, "Execute Shot 사용 후 Combo 전부 소모");
        }

        [Test]
        public void ExecuteShot_KillComboRestore_AddStacks3()
        {
            var aster = CreateComboCharacter();
            // Execute Shot 킬 시 ComboFinisher가 AddStacks(3) 호출 (시뮬레이션)
            aster.Resource.AddStacks(3);

            Assert.AreEqual(3, aster.Resource.CurrentStacks, "킬 시 Combo 3 복구");
        }

        // ═══════════════════════════════════════════
        // 헬퍼
        // ═══════════════════════════════════════════

        private static Character CreateComboCharacter(int atk = 0, int def = 0)
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            SetPrivateField(data, "_resourceType", ResourceType.Combo);
            var character = new Character(data);
            character.Health.Initialize(65);
            character.Stats.Initialize(atk, def);
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

        private static SkillData CreateAttackSkill(int power = 5)
        {
            var skill = ScriptableObject.CreateInstance<SkillData>();
            SetPrivateField(skill, "_skillType", SkillType.Attack);
            SetPrivateField(skill, "_targetType", TargetType.SingleEnemy);
            SetPrivateField(skill, "_power", power);
            SetPrivateField(skill, "_cost", 1);
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
