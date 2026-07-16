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
using TeamLog.Skill.Behaviors;

using SkillData = TeamLog.Characters.SkillData;
using SkillType = TeamLog.Characters.SkillType;
using TargetType = TeamLog.Characters.TargetType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;

namespace TeamLog.Tests
{
    /// <summary>
    /// Phase CC-2A Umbra, the Rogue 핵심 메카닉 검증.
    /// - ShadowsResourceComponent: 안 맞을수록 치명타 강화 (0/1/2/3/4 스택)
    /// - ShadowsMaxUp 특성: MaxStacksBonus +1 (3→4), 4스택 시 치명타 피해 3.5배
    /// - StrongVsDebuffBehavior: 도트 디버프 적 위력 ×2 (Backstab)
    /// - PowerAddVsDebuff 특성: 도트 디버프 적 +N 위력 (약점 포착)
    /// - Eviscerate: MinResourceRequired=3 검사 + 사용 후 -1
    ///
    /// 기획: Assets/09.Docs/Characters/ReworkDrafts/02_Rogue.md
    /// 작업 일지: Assets/09.Docs/WorkLog/2026-07-14.md
    /// </summary>
    [TestFixture]
    public class PhaseCC2ATests
    {
        [SetUp]
        public void SetUp()
        {
            CombatEventBus.Clear();
            DamageCalculator.ClearEvents();
            SkillExecutor.ClearEvents();
            BehaviorRegistry.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            CombatEventBus.Clear();
            DamageCalculator.ClearEvents();
            SkillExecutor.ClearEvents();
            GameRunState.Destroy();
            BehaviorRegistry.Reset();
        }

        // ═══════════════════════════════════════════
        // 1. ShadowsResourceComponent — 치명타 갱신
        // ═══════════════════════════════════════════

        [Test]
        public void Shadows_InitialState_ZeroStacks_ZeroCritChance()
        {
            var umbra = CreateShadowsCharacter();
            Assert.AreEqual(0, umbra.Resource.CurrentStacks, "초기 스택 0");
            Assert.AreEqual(0f, umbra.CritChance, "초기 치명타 확률 0%");
            Assert.AreEqual(1.5f, umbra.CritDamageMul, "초기 치명타 피해 1.5배");
        }

        [Test]
        public void Shadows_Stack1_CritChance50Percent()
        {
            var umbra = CreateShadowsCharacter();
            PassTurns(umbra, 1); // 무피해 1턴 → 1스택

            Assert.AreEqual(1, umbra.Resource.CurrentStacks, "1스택 도달");
            Assert.AreEqual(0.50f, umbra.CritChance, "1스택 - 치명타 확률 50%");
            Assert.AreEqual(1.5f, umbra.CritDamageMul, "1스택 - 치명타 피해 1.5배");
        }

        [Test]
        public void Shadows_Stack2_CritChance75Percent()
        {
            var umbra = CreateShadowsCharacter();
            PassTurns(umbra, 2); // 무피해 2턴 → 2스택

            Assert.AreEqual(2, umbra.Resource.CurrentStacks, "2스택 도달");
            Assert.AreEqual(0.75f, umbra.CritChance, "2스택 - 치명타 확률 75%");
            Assert.AreEqual(1.5f, umbra.CritDamageMul, "2스택 - 치명타 피해 1.5배");
        }

        [Test]
        public void Shadows_Stack3_CritChance100Percent_2xDamage()
        {
            var umbra = CreateShadowsCharacter();
            PassTurns(umbra, 3); // 무피해 3턴 → 3스택 (최대치)

            Assert.AreEqual(3, umbra.Resource.CurrentStacks, "3스택 도달 (최대치)");
            Assert.AreEqual(1.0f, umbra.CritChance, "3스택 - 치명타 확률 100%");
            Assert.AreEqual(ShadowsResourceComponent.Shadows3CritDamageMul, umbra.CritDamageMul,
                "3스택 - 치명타 피해 2.0배 (Shadows3CritDamageMul 상수)");
            Assert.AreEqual(2.0f, umbra.CritDamageMul, "3스택 - 치명타 피해 2.0배");
        }

        // ═══════════════════════════════════════════
        // 2. ShadowsResourceComponent — 피해 시 리셋
        // ═══════════════════════════════════════════

        [Test]
        public void Shadows_TookDamage_ResetsToZero()
        {
            var umbra = CreateShadowsCharacter();
            PassTurns(umbra, 3); // 치명타 100% 도달

            // 턴 시작 → 피해 받음 → 턴 종료 시 리셋
            umbra.Resource.OnTurnStart(umbra);
            umbra.Health.TakeDamage(10); // Health.OnDamageTaken → _tookDamageThisTurn = true
            umbra.Resource.OnTurnEnd(umbra);

            Assert.AreEqual(0, umbra.Resource.CurrentStacks, "피해 받은 턴 종료 시 스택 0 리셋");
            Assert.AreEqual(0f, umbra.CritChance, "리셋 후 치명타 0%");
        }

        [Test]
        public void Shadows_NoDamage_IncreasesStacks()
        {
            var umbra = CreateShadowsCharacter();
            // 3턴 연속 무피해 → 스택 3 도달
            umbra.Resource.OnTurnStart(umbra);
            umbra.Resource.OnTurnEnd(umbra);
            Assert.AreEqual(1, umbra.Resource.CurrentStacks, "1턴 무피해 - 스택 1");

            umbra.Resource.OnTurnStart(umbra);
            umbra.Resource.OnTurnEnd(umbra);
            Assert.AreEqual(2, umbra.Resource.CurrentStacks, "2턴 무피해 - 스택 2");

            umbra.Resource.OnTurnStart(umbra);
            umbra.Resource.OnTurnEnd(umbra);
            Assert.AreEqual(3, umbra.Resource.CurrentStacks, "3턴 무피해 - 스택 3 (최대치)");
        }

        /// <summary>N턴 무피해 통과 헬퍼 — OnTurnStart/End N회 호출로 자연스럽게 스택 적축.</summary>
        private static void PassTurns(Character c, int turns)
        {
            for (int i = 0; i < turns; i++)
            {
                c.Resource.OnTurnStart(c);
                c.Resource.OnTurnEnd(c);
            }
        }

        // ═══════════════════════════════════════════
        // 3. ShadowsMaxUp 특성 — MaxStacksBonus +1
        // ═══════════════════════════════════════════

        [Test]
        public void ShadowsMaxUp_Trait_IncreasesMaxStacks_To4()
        {
            var umbra = CreateShadowsCharacter();
            Assert.AreEqual(3, umbra.Resource.MaxStacks, "기본 최대 스택 3");
            Assert.AreEqual(3, umbra.Resource.EffectiveMaxStacks, "특성 없을 때 EffectiveMaxStacks = 3");

            // "그림자 심화" 특성 — ShadowsMaxUp 1
            var trait = CreateTrait(
                (KeywordType.ShadowsMaxUp, 1, KeywordTrigger.Passive, 0f));
            umbra.EquipTrait(trait);

            Assert.AreEqual(1, umbra.Resource.MaxStacksBonus, "MaxStacksBonus = 1 설정됨");
            Assert.AreEqual(4, umbra.Resource.EffectiveMaxStacks, "특성 장착 후 EffectiveMaxStacks = 4");
        }

        [Test]
        public void ShadowsMaxUp_Stack4_CritDamage35x()
        {
            var umbra = CreateShadowsCharacter();
            var trait = CreateTrait(
                (KeywordType.ShadowsMaxUp, 1, KeywordTrigger.Passive, 0f));
            umbra.EquipTrait(trait);

            PassTurns(umbra, 4); // 무피해 4턴 → EffectiveMaxStacks=4 도달

            Assert.AreEqual(4, umbra.Resource.CurrentStacks, "4스택 도달 (특성으로 확장)");
            Assert.AreEqual(1.0f, umbra.CritChance, "4스택 - 치명타 확률 100%");
            Assert.AreEqual(3.5f, umbra.CritDamageMul, "4스택 - 치명타 피해 3.5배 (ShadowsMaxUp 보상)");
        }

        // ═══════════════════════════════════════════
        // 4. StrongVsDebuffBehavior — 도트 디버프 적 2배
        // ═══════════════════════════════════════════

        [Test]
        public void StrongVsDebuff_NoDebuff_BaseDamage()
        {
            var umbra = CreateShadowsCharacter(0, 0); // ATK 0
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { umbra };
            var enemies = new List<Character> { enemy };

            // Backstab — 위력 7, StrongVsDebuff
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 7,
                new BehaviorTag(BehaviorKeyword.StrongVsDebuff, 0));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(umbra, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(7, damage, $"디버프 없는 적 — 기본 위력 7 (ATK 0). 실제 {damage}");
        }

        [Test]
        public void StrongVsDebuff_WithPoison_DoublesDamage()
        {
            var umbra = CreateShadowsCharacter(0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            enemy.StatusEffects.ApplyEffect(StatusEffectType.Poison, 2, 2); // Poison 부여
            var party = new List<Character> { umbra };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 7,
                new BehaviorTag(BehaviorKeyword.StrongVsDebuff, 0));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(umbra, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(14, damage, $"Poison 적 - 위력 2배 (7×2=14). 실제 {damage}");
        }

        [Test]
        public void StrongVsDebuff_WithFreeze_DoublesDamage()
        {
            var umbra = CreateShadowsCharacter(0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            enemy.StatusEffects.ApplyEffect(StatusEffectType.Freeze, 1, 1);
            var party = new List<Character> { umbra };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 7,
                new BehaviorTag(BehaviorKeyword.StrongVsDebuff, 0));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(umbra, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(14, damage, $"Freeze 적 - 위력 2배. 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 5. PowerAddVsDebuff 특성 — 도트 적 +N 위력
        // ═══════════════════════════════════════════

        [Test]
        public void PowerAddVsDebuff_NoDebuff_NoBonus()
        {
            var umbra = CreateShadowsCharacter(0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { umbra };
            var enemies = new List<Character> { enemy };

            // "약점 포착" 특성 — PowerAddVsDebuff 3
            var trait = CreateTrait(
                (KeywordType.PowerAddVsDebuff, 3, KeywordTrigger.Passive, 0f));
            umbra.EquipTrait(trait);
            SetupRunStateWithTraitSubscription(party);

            var attack = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10);
            int hpBefore = enemy.Health.CurrentHP;
            var executor = new SkillExecutor(party, enemies);
            executor.ExecuteSkillInternal(umbra, attack, enemy);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(10, damage, $"디버프 없는 적 - PowerAddVsDebuff 미발동 (위력 10 그대로). 실제 {damage}");
        }

        [Test]
        public void PowerAddVsDebuff_WithBleed_Adds3()
        {
            var umbra = CreateShadowsCharacter(0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            enemy.StatusEffects.ApplyEffect(StatusEffectType.Bleed, 2, 3); // Bleed 부여
            var party = new List<Character> { umbra };
            var enemies = new List<Character> { enemy };

            var trait = CreateTrait(
                (KeywordType.PowerAddVsDebuff, 3, KeywordTrigger.Passive, 0f));
            umbra.EquipTrait(trait);
            SetupRunStateWithTraitSubscription(party);

            var attack = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10);
            int hpBefore = enemy.Health.CurrentHP;
            var executor = new SkillExecutor(party, enemies);
            executor.ExecuteSkillInternal(umbra, attack, enemy);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 위력 10 + PowerAddVsDebuff 3 = 13
            Assert.AreEqual(13, damage, $"Bleed 적 - PowerAddVsDebuff +3 (위력 13). 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 6. Eviscerate 자원 검사 로직 (TurnManager.ExecuteSkillImmediately L196-205 재현)
        // ═══════════════════════════════════════════
        // 참고: TurnManager 전체 흐름(StartBattle→SkillDraw)은 통합 테스트 영역.
        // 여기서는 MinResourceRequired 검사 로직과 ConsumeStacks(1) 작동을 직접 검증.

        [Test]
        public void Eviscerate_MinResourceRequired3_Blocked_WhenStacks2()
        {
            var umbra = CreateShadowsCharacter();
            umbra.Resource.AddStacks(2); // Shadows 2

            var skill = CreateEviscerateSkill();

            // TurnManager.ExecuteSkillImmediately L196-205 로직:
            // MinResourceRequired > 0 && CurrentStacks < MinResourceRequired → return false
            bool blocked = skill.MinResourceRequired > 0
                && skill.ResourceCostType == umbra.Resource.Resource
                && umbra.Resource.CurrentStacks < skill.MinResourceRequired;

            Assert.AreEqual(3, skill.MinResourceRequired, "Eviscerate MinResourceRequired=3");
            Assert.IsTrue(blocked, "Shadows 2 < MinResourceRequired 3 → 사용 차단");
        }

        [Test]
        public void Eviscerate_MinResourceRequired3_Allowed_WhenStacks3()
        {
            var umbra = CreateShadowsCharacter();
            umbra.Resource.AddStacks(3); // Shadows 3 (최대치)

            var skill = CreateEviscerateSkill();

            bool blocked = skill.MinResourceRequired > 0
                && skill.ResourceCostType == umbra.Resource.Resource
                && umbra.Resource.CurrentStacks < skill.MinResourceRequired;

            Assert.IsFalse(blocked, "Shadows 3 >= MinResourceRequired 3 → 사용 허용");
        }

        [Test]
        public void Eviscerate_ResourceCostAmount1_ConsumeStacksWorks()
        {
            var umbra = CreateShadowsCharacter();
            umbra.Resource.AddStacks(3);

            var skill = CreateEviscerateSkill();

            // TurnManager.ExecuteSkillImmediately L332-333:
            // else if (skill.ResourceCostAmount > 0) caster.Resource.ConsumeStacks(skill.ResourceCostAmount);
            Assert.AreEqual(1, skill.ResourceCostAmount, "Eviscerate ResourceCostAmount=1");
            Assert.IsTrue(umbra.Resource.ConsumeStacks(skill.ResourceCostAmount), "ConsumeStacks(1) 성공");
            Assert.AreEqual(2, umbra.Resource.CurrentStacks,
                $"Eviscerate 사용 후 Shadows 3→2. 실제 {umbra.Resource.CurrentStacks}");
        }

        // ═══════════════════════════════════════════
        // 헬퍼
        // ═══════════════════════════════════════════

        private static Character CreateShadowsCharacter(int atk = 0, int def = 0)
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            SetPrivateField(data, "_resourceType", ResourceType.Shadows);
            var character = new Character(data);
            character.Health.Initialize(75);
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

        private static SkillData CreateSkill(SkillType type, TargetType target, int power,
            params BehaviorTag[] behaviors)
        {
            var skill = ScriptableObject.CreateInstance<SkillData>();
            SetPrivateField(skill, "_skillType", type);
            SetPrivateField(skill, "_targetType", target);
            SetPrivateField(skill, "_power", power);
            SetPrivateField(skill, "_cost", 0);
            SetPrivateField(skill, "_behaviors", behaviors ?? new BehaviorTag[0]);
            return skill;
        }

        /// <summary>Eviscerate 스킬 데이터 — power 15, AP 3, Shadows 1 소모, MinResourceRequired 3.</summary>
        private static SkillData CreateEviscerateSkill()
        {
            var skill = ScriptableObject.CreateInstance<SkillData>();
            SetPrivateField(skill, "_skillType", SkillType.Attack);
            SetPrivateField(skill, "_targetType", TargetType.SingleEnemy);
            SetPrivateField(skill, "_power", 15);
            SetPrivateField(skill, "_cost", 3);
            SetPrivateField(skill, "_resourceCostType", ResourceType.Shadows);
            SetPrivateField(skill, "_resourceCostAmount", 1);
            SetPrivateField(skill, "_minResourceRequired", 3);
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

        private static GameRunState SetupRunStateWithTraitSubscription(List<Character> party)
        {
            var runState = GameRunState.Create(party, 0);
            runState.RelicHandler.SetPlayerParty(party);
            runState.RelicHandler.SubscribeEvents();
            foreach (var c in party)
                c.PlayerTraitHandler.SubscribeEvents();
            return runState;
        }
    }
}
