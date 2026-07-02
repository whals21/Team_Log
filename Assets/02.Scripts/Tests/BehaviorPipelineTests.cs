using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Combat.Turn;
using TeamLog.Map;
using TeamLog.Skill;
using TeamLog.Skill.Behaviors;
using TeamLog.Skill.Behaviors.Implementations;

using SkillData = TeamLog.Characters.SkillData;
using SkillType = TeamLog.Characters.SkillType;
using TargetType = TeamLog.Characters.TargetType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;
using StatType = TeamLog.Characters.StatType;

namespace TeamLog.Tests
{
    /// <summary>
    /// Phase ARCH-2: 조립식 스킬 파이프라인(BehaviorRegistry + SkillExecutionPipeline) 검증.
    /// 핵심 5종 Behavior(Berserk/Pierce/Execution/Lifesteal/Chain)이
    /// 기존 SkillExecutor.ExecuteAttack과 동일한 결과를 내는지 확인.
    ///
    /// 병행 구조: 이 테스트들은 SkillExecutionPipeline.ExecuteAttack을 직접 호출.
    /// 기존 SkillExecutor는 그대로 유지되며 BehaviorSkillExecutionTests가 별도 검증.
    /// </summary>
    [TestFixture]
    public class BehaviorPipelineTests
    {
        [SetUp]
        public void SetUp()
        {
            CombatEventBus.Clear();
            DamageCalculator.ClearEvents();
            SkillExecutor.ClearEvents();
            GameRunState.Destroy();
            BehaviorRegistry.Reset(); // 각 테스트 격리 — 매 테스트마다 Registry 재초기화
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
        // BehaviorRegistry 기본 동작
        // ═══════════════════════════════════════════

        [Test]
        public void Registry_Get_ReturnsRegisteredBehavior()
        {
            var berserk = BehaviorRegistry.Get(BehaviorKeyword.Berserk);
            Assert.IsNotNull(berserk, "Berserk Behavior가 등록되어 있어야 함");
            Assert.AreEqual(BehaviorKeyword.Berserk, berserk.Keyword);
        }

        [Test]
        public void Registry_GetForPhase_FiltersByPhase()
        {
            var tags = new List<BehaviorTag>
            {
                new(BehaviorKeyword.Berserk, 0),    // PowerModify
                new(BehaviorKeyword.Pierce, 0),     // DamageApply
                new(BehaviorKeyword.Lifesteal, 0),  // PostDamage
            };

            var powerBehaviors = BehaviorRegistry.GetForPhase(tags, ExecutionPhase.PowerModify);
            Assert.AreEqual(1, powerBehaviors.Count, "PowerModify Phase에 Berserk 1개만 해당");
            Assert.AreEqual(BehaviorKeyword.Berserk, powerBehaviors[0].Keyword);

            var damageBehaviors = BehaviorRegistry.GetForPhase(tags, ExecutionPhase.ApplyMain);
            Assert.AreEqual(1, damageBehaviors.Count);
            Assert.AreEqual(BehaviorKeyword.Pierce, damageBehaviors[0].Keyword);
        }

        [Test]
        public void Registry_GetForPhase_SortsByOrder()
        {
            var tags = new List<BehaviorTag>
            {
                new(BehaviorKeyword.Chain, 1),       // Order=200
                new(BehaviorKeyword.Execution, 5),   // Order=10
                new(BehaviorKeyword.Lifesteal, 0),   // Order=50
            };

            var postDamage = BehaviorRegistry.GetForPhase(tags, ExecutionPhase.PostApply);
            Assert.AreEqual(3, postDamage.Count);
            // Order 오름차순: Execution(10) → Lifesteal(50) → Chain(200)
            Assert.AreEqual(BehaviorKeyword.Execution, postDamage[0].Keyword);
            Assert.AreEqual(BehaviorKeyword.Lifesteal, postDamage[1].Keyword);
            Assert.AreEqual(BehaviorKeyword.Chain, postDamage[2].Keyword);
        }

        // ═══════════════════════════════════════════
        // Berserk — HP 50% 이하 시 위력 2배
        // ═══════════════════════════════════════════

        [Test]
        public void Berserk_HPBelowHalf_DoublesDamage()
        {
            // MaxHP 100, 현재 HP 50 (50% 이하) → 위력 2배
            var player = CreateCharacter(100, 0, 0);
            player.Health.Initialize(100);
            player.Health.TakeDamage(50); // HP 50으로

            var enemy = CreateCharacter(500, 0, 0);
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Berserk, 0));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // ATK 0, power 10 (×2 배) - DEF 0 = 20
            Assert.AreEqual(20, damage, $"Berserk(HP<=50%) 위력 2배 — 기대 20, 실제 {damage}");
        }

        [Test]
        public void Berserk_HPAboveHalf_NoEffect()
        {
            var player = CreateCharacter(100, 0, 0);
            player.Health.TakeDamage(30); // HP 70 (50% 초과)

            var enemy = CreateCharacter(500, 0, 0);
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Berserk, 0));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 위력 2배 미발동: power 10 - DEF 0 = 10
            Assert.AreEqual(10, damage, $"Berserk(HP>50%) 미발동 — 기대 10, 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // Pierce — 쉴드 우회 데미지
        // ═══════════════════════════════════════════

        [Test]
        public void Pierce_BypassesShield()
        {
            var player = CreateCharacter(100, 5, 0); // ATK 5
            var enemy = CreateCharacter(100, 0, 0);
            enemy.Health.AddShield(50); // 쉴드 50

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Pierce, 0));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int hpBefore = enemy.Health.CurrentHP;
            int shieldBefore = enemy.Health.CurrentShield;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // Pierce: ATK 5 + power 10 = 15 직접 HP 데미지. 쉴드는 그대로.
            Assert.AreEqual(15, damage, "Pierce는 쉴드를 우회하고 HP에 직접 데미지");
            Assert.AreEqual(shieldBefore, enemy.Health.CurrentShield, "Pierce 후에도 쉴드는 유지되어야 함");
        }

        // ═══════════════════════════════════════════
        // Execution — HP rank 이하 처형 (보스 제외)
        // ═══════════════════════════════════════════

        [Test]
        public void Execution_KillsLowHPNonBoss()
        {
            var player = CreateCharacter(100, 0, 0);
            var enemy = CreateCharacter(100, 0, 0);
            enemy.Health.TakeDamage(95); // HP 5

            // Execution rank=10 → HP 10 이하 즉사
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 1,
                new BehaviorTag(BehaviorKeyword.Execution, 10));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            pipeline.ExecuteSkill(player, skill, enemy, instance);

            Assert.IsTrue(enemy.IsDead, "Execution(rank 10)이 HP 5 일반 적을 즉사시켜야 함");
        }

        [Test]
        public void Execution_SkipsBoss()
        {
            var player = CreateCharacter(100, 0, 0);
            var boss = CreateCharacter(100, 0, 0, isBoss: true);
            boss.Health.TakeDamage(95); // HP 5

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 1,
                new BehaviorTag(BehaviorKeyword.Execution, 10));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { boss });

            pipeline.ExecuteSkill(player, skill, boss, instance);

            // 보스는 Execution 면역. 일반 공격 power 1만 적용
            Assert.IsTrue(boss.IsAlive, "Execution은 보스에게 작동하지 않아야 함");
        }

        // ═══════════════════════════════════════════
        // Lifesteal — 준 데미지 절반 회복
        // ═══════════════════════════════════════════

        [Test]
        public void Lifesteal_HealsHalfDamage()
        {
            var player = CreateCharacter(100, 0, 0);
            player.Health.TakeDamage(50); // HP 50 (회복 여유)

            var enemy = CreateCharacter(100, 0, 0);
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 20,
                new BehaviorTag(BehaviorKeyword.Lifesteal, 0));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int playerHpBefore = player.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            // power 20 → 적에게 20 데미지. Lifesteal 회복 = 20/2 = 10
            int heal = player.Health.CurrentHP - playerHpBefore;
            Assert.AreEqual(10, heal, $"Lifesteal 회복량 = 준 데미지/2 — 기대 10, 실제 {heal}");
        }

        // ═══════════════════════════════════════════
        // Chain — 무작위 N명 연쇄
        // ═══════════════════════════════════════════

        [Test]
        public void Chain_HitsOtherEnemies()
        {
            var player = CreateCharacter(100, 0, 0);
            var e1 = CreateCharacter(500, 0, 0);
            var e2 = CreateCharacter(500, 0, 0);
            var e3 = CreateCharacter(500, 0, 0);

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Chain, 2));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { e1, e2, e3 });

            int hp1Before = e1.Health.CurrentHP;
            int hp2Before = e2.Health.CurrentHP;
            int hp3Before = e3.Health.CurrentHP;

            pipeline.ExecuteSkill(player, skill, e1, instance);

            // 메인 타겟 e1은 반드시 데미지
            int d1 = hp1Before - e1.Health.CurrentHP;
            Assert.AreEqual(10, d1, "메인 타겟은 power 10 데미지");

            // 나머지 둘 중 최소 하나는 맞아야 함 (Chain rank 2)
            int d2 = hp2Before - e2.Health.CurrentHP;
            int d3 = hp3Before - e3.Health.CurrentHP;
            Assert.Greater(d2 + d3, 0, "Chain(2)은 메인 타겟 외 적 중 최소 1명 이상에게 데미지");
        }

        [Test]
        public void Chain_SingleEnemy_NoChain()
        {
            var player = CreateCharacter(100, 0, 0);
            var enemy = CreateCharacter(500, 0, 0);

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Chain, 3));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 단일 적 → 연쇄 대상 없음. 메인 타겟에게만 power 10
            Assert.AreEqual(10, damage, "Chain(3) 단일 적 = 연쇄 없이 메인 타겟에게만 power 10");
        }

        // ═══════════════════════════════════════════
        // 복합 조합 — 다중 Behavior가 순서대로 작동
        // ═══════════════════════════════════════════

        [Test]
        public void Pipeline_CombinesBerserkAndLifesteal()
        {
            // Berserk(위력 2배) + Lifesteal(회복) 조합
            var player = CreateCharacter(100, 0, 0);
            player.Health.TakeDamage(60); // HP 40 (50% 이하 → Berserk 발동)

            var enemy = CreateCharacter(100, 0, 0);
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Berserk, 0),
                new BehaviorTag(BehaviorKeyword.Lifesteal, 0));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int playerHpBefore = player.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            // Berserk 발동: power 10 × 2 = 20 데미지
            // Lifesteal: 20 / 2 = 10 회복
            int heal = player.Health.CurrentHP - playerHpBefore;
            Assert.AreEqual(10, heal, "Berserk+Lifesteal: 20 데미지 → 10 회복");
        }

        [Test]
        public void Pipeline_NoBehaviors_DefaultDamage()
        {
            // BehaviorTag 없는 순수 공격 — 기본 DealDamage 작동 확인
            var player = CreateCharacter(100, 0, 0);
            var enemy = CreateCharacter(100, 0, 0);
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10);
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(10, damage, "Behavior 없는 스킬은 기본 DealDamage로 power 10 적용");
        }

        // ═══════════════════════════════════════════
        // Phase ARCH-4 신규 Behavior 9종 검증
        // ═══════════════════════════════════════════

        [Test]
        public void FirstBlood_FullHP_AddsBonusDamage()
        {
            var player = CreateCharacter(100, 0, 0);
            var enemy = CreateCharacter(100, 0, 0); // 풀피
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.FirstBlood, 4));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 풀피 적 → power 10 + 4 = 14
            Assert.AreEqual(14, damage, "FirstBlood(4) 풀피 적 — 기대 14, 실제 " + damage);
        }

        [Test]
        public void FirstBlood_Damaged_NoBonus()
        {
            var player = CreateCharacter(100, 0, 0);
            var enemy = CreateCharacter(100, 0, 0);
            enemy.Health.TakeDamage(10); // HP 90
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.FirstBlood, 4));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 풀피 아님 → power 10
            Assert.AreEqual(10, damage, "FirstBlood 풀피 아님 → 보너스 없음");
        }

        [Test]
        public void Cull_HalfHP_AddsBonusDamage()
        {
            var player = CreateCharacter(100, 0, 0);
            var enemy = CreateCharacter(100, 0, 0);
            enemy.Health.TakeDamage(60); // HP 40 (50% 이하)
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 8,
                new BehaviorTag(BehaviorKeyword.Cull, 6));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 절반 이하 → power 8 + 6 = 14
            Assert.AreEqual(14, damage, "Cull(6) 절반 이하 — 기대 14, 실제 " + damage);
        }

        [Test]
        public void Desperation_HighLostHP_AddsPowerPerRank()
        {
            // rank 5 → 잃은 HP 5당 위력 +1. 잃은 HP 50 → +10
            var player = CreateCharacter(100, 0, 0);
            player.Health.TakeDamage(50); // HP 50, 잃은 HP 50
            var enemy = CreateCharacter(200, 0, 0);
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Desperation, 5));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // power 10 + (50/5) = 20
            Assert.AreEqual(20, damage, "Desperation(5) 잃은 HP 50 — 기대 20, 실제 " + damage);
        }

        [Test]
        public void Wound_HighLostHP_ReducesPower()
        {
            // rank 5 → 잃은 HP 5당 위력 -1. 잃은 HP 20 → -4
            var player = CreateCharacter(100, 0, 0);
            player.Health.TakeDamage(20); // HP 80, 잃은 HP 20
            var enemy = CreateCharacter(200, 0, 0);
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Wound, 5));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // power 10 - (20/5) = 6
            Assert.AreEqual(6, damage, "Wound(5) 잃은 HP 20 — 기대 6, 실제 " + damage);
        }

        [Test]
        public void Bulwark_HasShield_AddsBonusDamage()
        {
            var player = CreateCharacter(100, 0, 0);
            player.Health.AddShield(20);
            var enemy = CreateCharacter(100, 0, 0);
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Bulwark, 5));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 쉴드 보유 → power 10 + 5 = 15
            Assert.AreEqual(15, damage, "Bulwark(5) 쉴드 보유 — 기대 15, 실제 " + damage);
        }

        [Test]
        public void Dominance_EnemyLowerHP_AddsBonusDamage()
        {
            var player = CreateCharacter(100, 0, 0); // HP 100
            var enemy = CreateCharacter(100, 0, 0);
            enemy.Health.TakeDamage(60); // HP 40 < 100
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Dominance, 4));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 적 HP 40 < 나 HP 100 → power 10 + 4 = 14
            Assert.AreEqual(14, damage, "Dominance(4) 적 HP < 나 HP — 기대 14, 실제 " + damage);
        }

        [Test]
        public void GiantSlayer_HighMaxHP_AddsBonusDamage()
        {
            var player = CreateCharacter(100, 0, 0);
            var enemy = CreateCharacter(150, 0, 0); // MaxHP 100+ (엘리트 기준)
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 8,
                new BehaviorTag(BehaviorKeyword.GiantSlayer, 6));
            var instance = new SkillInstance(skill);

            var pipeline = new SkillExecutionPipeline(
                new List<Character> { player }, new List<Character> { enemy });

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(player, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // MaxHP 150 >= 100 → power 8 + 6 = 14
            Assert.AreEqual(14, damage, "GiantSlayer(6) MaxHP 150 — 기대 14, 실제 " + damage);
        }

        // ═══════════════════════════════════════════
        // Phase ARCH-5: Fatigue/Momentum/Escalation/Mastery
        // — UsesThisBattle 기반 EffectivePower/Cost 변동 검증
        // ═══════════════════════════════════════════

        [Test]
        public void Fatigue_ReducesPowerOnRepeatedUse()
        {
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Fatigue, 2));
            var instance = new SkillInstance(skill);

            // 첫 사용 (UsesThisBattle=0): power 10
            Assert.AreEqual(10, instance.EffectivePower, "Fatigue 첫 사용 — power 10");

            instance.IncrementUsesThisBattle();
            // 두 번째 사용 (UsesThisBattle=1): power 10 - 2 = 8
            Assert.AreEqual(8, instance.EffectivePower, "Fatigue 두 번째 — power 8");

            instance.IncrementUsesThisBattle();
            // 세 번째 (UsesThisBattle=2): power 10 - 4 = 6
            Assert.AreEqual(6, instance.EffectivePower, "Fatigue 세 번째 — power 6");
        }

        [Test]
        public void Momentum_IncreasesPowerOnRepeatedUse()
        {
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 6,
                new BehaviorTag(BehaviorKeyword.Momentum, 2));
            var instance = new SkillInstance(skill);

            Assert.AreEqual(6, instance.EffectivePower, "Momentum 첫 사용 — power 6");

            instance.IncrementUsesThisBattle();
            Assert.AreEqual(8, instance.EffectivePower, "Momentum 두 번째 — power 8");

            instance.IncrementUsesThisBattle();
            Assert.AreEqual(10, instance.EffectivePower, "Momentum 세 번째 — power 10");
        }

        [Test]
        public void Escalation_IncreasesCostOnRepeatedUse()
        {
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Escalation, 1));
            // 기본 cost 설정
            SetPrivateField(skill, "_cost", 1);
            var instance = new SkillInstance(skill);

            // 첫 사용 (UsesThisBattle=0): cost 1
            Assert.AreEqual(1, instance.EffectiveCost, "Escalation 첫 사용 — cost 1");

            instance.IncrementUsesThisBattle();
            // 두 번째 (UsesThisBattle=1): cost 1 + 1 = 2
            Assert.AreEqual(2, instance.EffectiveCost, "Escalation 두 번째 — cost 2");

            instance.IncrementUsesThisBattle();
            // 세 번째 (UsesThisBattle=2): cost 1 + 2 = 3
            Assert.AreEqual(3, instance.EffectiveCost, "Escalation 세 번째 — cost 3");
        }

        [Test]
        public void Mastery_DecreasesCostOnRepeatedUse()
        {
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Mastery, 1));
            SetPrivateField(skill, "_cost", 3);
            var instance = new SkillInstance(skill);

            // 첫 사용 (UsesThisBattle=0): cost 3
            Assert.AreEqual(3, instance.EffectiveCost, "Mastery 첫 사용 — cost 3");

            instance.IncrementUsesThisBattle();
            // 두 번째 (UsesThisBattle=1): cost 3 - 1 = 2
            Assert.AreEqual(2, instance.EffectiveCost, "Mastery 두 번째 — cost 2");

            instance.IncrementUsesThisBattle();
            instance.IncrementUsesThisBattle();
            // 네 번째 (UsesThisBattle=3): cost 3 - 3 = 0 (최소 0)
            Assert.AreEqual(0, instance.EffectiveCost, "Mastery 네 번째 — cost 0 (최소)");
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

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"필드 '{fieldName}'을 찾을 수 없음 — SkillData/CharacterData 스키마 변경 확인 필요");
            field.SetValue(obj, value);
        }
    }
}
