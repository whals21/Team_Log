using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Combat.Turn;
using TeamLog.Map;
using TeamLog.Skill;

using SkillData = TeamLog.Characters.SkillData;
using SkillType = TeamLog.Characters.SkillType;
using TargetType = TeamLog.Characters.TargetType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;
using StatType = TeamLog.Characters.StatType;

namespace TeamLog.Tests
{
    /// <summary>
    /// Phase BK: 행동 키워드 런타임 실행 검증.
    /// SkillInstance 결합, SkillExecutor 분기(Pierce/Lifesteal/Chain/Execution/Berserk/Touch),
    /// TurnManager 타겟팅 분해(Spread/Bounce/MultiHit/AOEAuto/Explosion)를 단위 테스트.
    /// </summary>
    [TestFixture]
    public class BehaviorSkillExecutionTests
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

        // ═══════════════════════════════════════════
        // SkillInstance.GetCombinedBehaviors / HasBehavior / GetBehaviorRank
        // ═══════════════════════════════════════════

        [Test]
        public void GetCombinedBehaviors_MergesSkillAndAugment()
        {
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Pierce, 1));
            var instance = new SkillInstance(skill);

            var aug = CreateAugment(new BehaviorTag(BehaviorKeyword.Chain, 2));
            Assert.IsTrue(instance.AddAugment(aug));

            Assert.IsTrue(instance.HasBehavior(BehaviorKeyword.Pierce));
            Assert.IsTrue(instance.HasBehavior(BehaviorKeyword.Chain));
        }

        [Test]
        public void GetBehaviorRank_SumsAcrossSkillAndAugments()
        {
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Bounce, 1));
            var instance = new SkillInstance(skill);

            var aug = CreateAugment(new BehaviorTag(BehaviorKeyword.Bounce, 2));
            Assert.IsTrue(instance.AddAugment(aug));

            Assert.AreEqual(3, instance.GetBehaviorRank(BehaviorKeyword.Bounce));
        }

        [Test]
        public void AddAugment_RejectsDuplicateBehavior()
        {
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10);
            var instance = new SkillInstance(skill);

            var aug1 = CreateAugment(new BehaviorTag(BehaviorKeyword.Chain, 1));
            var aug2 = CreateAugment(new BehaviorTag(BehaviorKeyword.Chain, 2));

            Assert.IsTrue(instance.AddAugment(aug1));
            Assert.IsFalse(instance.AddAugment(aug2), "동일 BehaviorKeyword 중복 부착은 거부되어야 함");
        }

        [Test]
        public void AddAugment_AllowsDifferentBehaviors()
        {
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10);
            var instance = new SkillInstance(skill);

            Assert.IsTrue(instance.AddAugment(CreateAugment(new BehaviorTag(BehaviorKeyword.Chain, 1))));
            Assert.IsTrue(instance.AddAugment(CreateAugment(new BehaviorTag(BehaviorKeyword.Pierce, 1))));
        }

        // ═══════════════════════════════════════════
        // SkillInventoryComponent.QuickDraw 우선 뽑기
        // ═══════════════════════════════════════════

        [Test]
        public void QuickDraw_IsAlwaysDrawnFirst()
        {
            var player = CreateCharacter(100, 10, 0);
            var normalSkill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10);
            var quickSkill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 8,
                new BehaviorTag(BehaviorKeyword.QuickDraw, 1));
            player.SkillInventory.Initialize(new[] { normalSkill, quickSkill });

            var drawn = player.SkillInventory.DrawSkillInstance();
            Assert.AreEqual(quickSkill, drawn.Data);
        }

        // ═══════════════════════════════════════════
        // Pierce — 쉴드 + DEF 완전 무시
        // ═══════════════════════════════════════════

        [Test]
        public void Pierce_BypassesShieldAndDefense()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(100, 0, 10); // DEF 10
            enemy.Health.AddShield(20);

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 5,
                new BehaviorTag(BehaviorKeyword.Pierce, 1));
            var instance = new SkillInstance(skill);

            int hpBefore = enemy.Health.CurrentHP;
            var executor = new SkillExecutor(new List<Character> { player }, new List<Character> { enemy });
            executor.ExecuteSkillInternal(player, skill, enemy, instance);

            // Pierce: ATK 10 + power 5 = 15 직접 데미지 (쉴드 20, DEF 10 무시)
            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(15, damage, $"Pierce는 쉴드+DEF 무시해야 함 — 기대 15, 실제 {damage}");
            Assert.IsTrue(enemy.Health.CurrentShield > 0, "Pierce는 쉴드를 우회해야 함");
        }

        // ═══════════════════════════════════════════
        // Lifesteal — 준 데미지 절반 회복
        // ═══════════════════════════════════════════

        [Test]
        public void Lifesteal_HealsCasterHalfDamage()
        {
            var player = CreateCharacter(100, 10, 0);
            player.Health.TakeDamage(50); // HP 50 (회복 여유 확보)
            var enemy = CreateCharacter(100, 0, 0);

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Lifesteal, 1));
            var instance = new SkillInstance(skill);

            int hpBefore = player.Health.CurrentHP;
            var executor = new SkillExecutor(new List<Character> { player }, new List<Character> { enemy });
            executor.ExecuteSkillInternal(player, skill, enemy, instance);

            int heal = player.Health.CurrentHP - hpBefore;
            // ATK 10 + power 10 - DEF 0 = 20 데미지 → 절반 = 10 회복
            Assert.AreEqual(10, heal, $"Lifesteal 회복량 — 기대 10, 실제 {heal}");
        }

        // ═══════════════════════════════════════════
        // Chain — 무작위 N명 연쇄 (rank=1이면 메인 외 1명)
        // ═══════════════════════════════════════════

        [Test]
        public void Chain_HitsAdditionalTarget()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy1 = CreateCharacter(100, 0, 0);
            var enemy2 = CreateCharacter(100, 0, 0);
            var enemies = new List<Character> { enemy1, enemy2 };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Chain, 1));
            var instance = new SkillInstance(skill);

            int hp2Before = enemy2.Health.CurrentHP;
            var executor = new SkillExecutor(new List<Character> { player }, enemies);
            executor.ExecuteSkillInternal(player, skill, enemy1, instance);

            // 메인(enemy1)도 데미지, chain이 enemy2를 때림 (위력 100%)
            int damage2 = hp2Before - enemy2.Health.CurrentHP;
            Assert.Greater(damage2, 0, "Chain이 메인 타겟 외의 적을 추가로 타격해야 함");
        }

        // ═══════════════════════════════════════════
        // Execution — HP rank 이하 즉사 (보스 제외)
        // ═══════════════════════════════════════════

        [Test]
        public void Execution_KillsLowHPEnemy()
        {
            var player = CreateCharacter(100, 5, 0);
            var enemy = CreateCharacter(100, 0, 0);
            enemy.Health.TakeDamage(85); // HP 15

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 1,
                new BehaviorTag(BehaviorKeyword.Execution, 10));
            var instance = new SkillInstance(skill);

            var executor = new SkillExecutor(new List<Character> { player }, new List<Character> { enemy });
            executor.ExecuteSkillInternal(player, skill, enemy, instance);

            Assert.IsTrue(enemy.IsDead, "Execution 10은 HP 15 이하 적을 즉사시켜야 함");
        }

        [Test]
        public void Execution_DoesNotKillAboveThreshold()
        {
            var player = CreateCharacter(100, 5, 0);
            var enemy = CreateCharacter(100, 0, 0);
            enemy.Health.TakeDamage(50); // HP 50 (Execution 10 임계 초과)

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 1,
                new BehaviorTag(BehaviorKeyword.Execution, 10));
            var instance = new SkillInstance(skill);

            var executor = new SkillExecutor(new List<Character> { player }, new List<Character> { enemy });
            executor.ExecuteSkillInternal(player, skill, enemy, instance);

            Assert.IsTrue(enemy.IsAlive, "Execution 10은 HP 50 적을 즉사시키지 않음");
        }

        [Test]
        public void Execution_DoesNotAffectBoss()
        {
            var player = CreateCharacter(100, 5, 0);
            var enemy = CreateCharacter(100, 0, 0, isBoss: true);
            enemy.Health.TakeDamage(91); // HP 9 (Execution 10 임계 이하, 일반 공격 6 데미지로는 사망 X)

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 1,
                new BehaviorTag(BehaviorKeyword.Execution, 10));
            var instance = new SkillInstance(skill);

            var executor = new SkillExecutor(new List<Character> { player }, new List<Character> { enemy });
            executor.ExecuteSkillInternal(player, skill, enemy, instance);

            Assert.IsTrue(enemy.IsAlive, "Execution은 보스에게 즉사 효과 없음");
        }

        // ═══════════════════════════════════════════
        // Berserk — HP 절반 이하일 때 위력 2배
        // ═══════════════════════════════════════════

        [Test]
        public void Berserk_DoublesDamage_WhenHPBelowHalf()
        {
            var player = CreateCharacter(100, 10, 0);
            player.Health.TakeDamage(55); // HP 45 (45%)
            var enemy = CreateCharacter(100, 0, 0);

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Berserk, 1));
            var instance = new SkillInstance(skill);

            int hpBefore = enemy.Health.CurrentHP;
            var executor = new SkillExecutor(new List<Character> { player }, new List<Character> { enemy });
            executor.ExecuteSkillInternal(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // ATK 10 + power 10*2 - DEF 0 = 30
            Assert.AreEqual(30, damage, $"Berserk(HP<=50%) 위력 2배 — 기대 30, 실제 {damage}");
        }

        [Test]
        public void Berserk_DoesNotTrigger_WhenHPAboveHalf()
        {
            var player = CreateCharacter(100, 10, 0);
            player.Health.TakeDamage(40); // HP 60 (60%)
            var enemy = CreateCharacter(100, 0, 0);

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Berserk, 1));
            var instance = new SkillInstance(skill);

            int hpBefore = enemy.Health.CurrentHP;
            var executor = new SkillExecutor(new List<Character> { player }, new List<Character> { enemy });
            executor.ExecuteSkillInternal(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 위력 2배 미발동: ATK 10 + power 10 - DEF 0 = 20
            Assert.AreEqual(20, damage, $"Berserk(HP>50%) 미발동 — 기대 20, 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // Touch 계열 — VenomTouch/BurningTouch/FreezeTouch 스택 부여
        // ═══════════════════════════════════════════

        [Test]
        public void VenomTouch_AppliesPoisonStacks()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(100, 0, 0);

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 5,
                new BehaviorTag(BehaviorKeyword.VenomTouch, 3));
            var instance = new SkillInstance(skill);

            var executor = new SkillExecutor(new List<Character> { player }, new List<Character> { enemy });
            executor.ExecuteSkillInternal(player, skill, enemy, instance);

            // Phase BK: ApplyTouchEffects가 공격 후 호출되어 중독 3스택 부여
            Assert.IsTrue(enemy.StatusEffects.HasEffect(StatusEffectType.Poison),
                "VenomTouch가 중독을 부여해야 함");
        }

        [Test]
        public void BurningTouch_AppliesBurnStacks()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(100, 0, 0);

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 5,
                new BehaviorTag(BehaviorKeyword.BurningTouch, 2));
            var instance = new SkillInstance(skill);

            var executor = new SkillExecutor(new List<Character> { player }, new List<Character> { enemy });
            executor.ExecuteSkillInternal(player, skill, enemy, instance);

            Assert.IsTrue(enemy.StatusEffects.HasEffect(StatusEffectType.Burn),
                "BurningTouch가 화상을 부여해야 함");
        }

        // ═══════════════════════════════════════════
        // TurnManager Spread — 단일 → 광역 (위력 100%)
        // ═══════════════════════════════════════════

        [Test]
        public void Spread_HitsAllEnemies_FullPower()
        {
            var player = CreateCharacter(100, 10, 0);
            var e1 = CreateCharacter(100, 0, 0);
            var e2 = CreateCharacter(100, 0, 0);
            var e3 = CreateCharacter(100, 0, 0);
            var party = new List<Character> { player };
            var enemies = new List<Character> { e1, e2, e3 };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Spread, 1));
            var instance = new SkillInstance(skill);

            var tm = new TurnManager(party, enemies);

            int hp1Before = e1.Health.CurrentHP;
            int hp2Before = e2.Health.CurrentHP;
            int hp3Before = e3.Health.CurrentHP;

            tm.ExecuteSkillImmediately(player, skill, e1, instance);

            // 모든 적이 같은 양의 데미지를 입어야 함 (위력 100%)
            int d1 = hp1Before - e1.Health.CurrentHP;
            int d2 = hp2Before - e2.Health.CurrentHP;
            int d3 = hp3Before - e3.Health.CurrentHP;
            Assert.Greater(d1, 0);
            Assert.AreEqual(d1, d2, "Spread는 모든 적에게 동일 위력을 적용");
            Assert.AreEqual(d1, d3, "Spread는 모든 적에게 동일 위력을 적용");
        }

        // ═══════════════════════════════════════════
        // TurnManager MultiHit — 동일 대상 N회 추가
        // ═══════════════════════════════════════════

        [Test]
        public void MultiHit_DealsMultipleStrikesToSameTarget()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(500, 0, 0); // HP 충분
            var party = new List<Character> { player };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 5,
                new BehaviorTag(BehaviorKeyword.MultiHit, 2));
            var instance = new SkillInstance(skill);

            var tm = new TurnManager(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            tm.ExecuteSkillImmediately(player, skill, enemy, instance);

            int totalDamage = hpBefore - enemy.Health.CurrentHP;
            // 메인 1회 + 추가 2회 = 총 3회. 각 회 ATK 10 + power 5 - DEF 0 = 15 → 총 45
            Assert.AreEqual(45, totalDamage, $"MultiHit(2)은 메인+2회 = 3회 타격 — 기대 45, 실제 {totalDamage}");
        }

        // ═══════════════════════════════════════════
        // TurnManager AOEAuto — 단일 자동 광역
        // ═══════════════════════════════════════════

        [Test]
        public void AOEAuto_HitsAllEnemies_FullPower()
        {
            var player = CreateCharacter(100, 10, 0);
            var e1 = CreateCharacter(100, 0, 0);
            var e2 = CreateCharacter(100, 0, 0);
            var party = new List<Character> { player };
            var enemies = new List<Character> { e1, e2 };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.AOEAuto, 1));
            var instance = new SkillInstance(skill);

            var tm = new TurnManager(party, enemies);

            int hp1Before = e1.Health.CurrentHP;
            int hp2Before = e2.Health.CurrentHP;

            tm.ExecuteSkillImmediately(player, skill, e1, instance);

            int d1 = hp1Before - e1.Health.CurrentHP;
            int d2 = hp2Before - e2.Health.CurrentHP;
            Assert.Greater(d1, 0);
            Assert.AreEqual(d1, d2, "AOEAuto는 모든 적에게 동일 위력");
        }

        // ═══════════════════════════════════════════
        // TurnManager Bounce — 무작위 N회 추가 (중복 허용)
        // ═══════════════════════════════════════════

        [Test]
        public void Bounce_DealsExtraStrikesTotalExceedsSingle()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(500, 0, 0); // HP 충분 (단일 적 → Bounce는 같은 적 반복)
            var party = new List<Character> { player };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 5,
                new BehaviorTag(BehaviorKeyword.Bounce, 2));
            var instance = new SkillInstance(skill);

            var tm = new TurnManager(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            tm.ExecuteSkillImmediately(player, skill, enemy, instance);

            int totalDamage = hpBefore - enemy.Health.CurrentHP;
            // 메인 1회 + Bounce 2회 = 3회 × 15 = 45
            Assert.AreEqual(45, totalDamage, $"Bounce(2) 단일 적 = 메인+2회 중복 타격 — 기대 45, 실제 {totalDamage}");
        }

        // ═══════════════════════════════════════════
        // 헬퍼
        // ═══════════════════════════════════════════

        private static Character CreateCharacter(int hp, int atk, int def, bool isBoss = false)
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            if (isBoss) SetPrivateField(data, "_isBoss", true);
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
            SetPrivateField(skill, "_behaviors", behaviors);
            return skill;
        }

        private static AugmentData CreateAugment(params BehaviorTag[] behaviors)
        {
            var aug = ScriptableObject.CreateInstance<AugmentData>();
            SetPrivateField(aug, "_behaviors", behaviors);
            SetPrivateField(aug, "_compatibleSkillType", SkillType.Attack);
            return aug;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}
