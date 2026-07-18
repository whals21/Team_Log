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

using SkillData = TeamLog.Characters.SkillData;
using SkillType = TeamLog.Characters.SkillType;
using TargetType = TeamLog.Characters.TargetType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;

namespace TeamLog.Tests
{
    /// <summary>
    /// Phase CC-2G-1 Ashe (Pyromancer) 리워크 검증.
    /// - TargetFullHPBehavior: 풀피 적 +N 위력 (Cinder Accretion)
    /// - DesperationBehavior: 잃은 HP 10당 +1 (Brand of Ash)
    /// - AllInBehavior: AP 0 시 +N 추가 데미지 (Embrace of Cinders)
    ///
    /// 기획: Assets/09.Docs/Characters/ReworkDrafts/INDEX.md (Phase CC-2G 로드맵)
    /// </summary>
    [TestFixture]
    public class PhaseCC2GTests
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
        // 1. TargetFullHPBehavior — 풀피 적 +N 위력
        // ═══════════════════════════════════════════

        [Test]
        public void TargetFullHP_FullHPEnemy_AddsBonusDamage()
        {
            var ashe = CreateCharacter(70, 0, 0);
            var enemy = CreateCharacter(500, 0, 0); // 풀피 상태
            var party = new List<Character> { ashe };
            var enemies = new List<Character> { enemy };

            // Cinder Accretion: power 5 + TargetFullHP rank 3 → 풀피 적에게 8 기대
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 5,
                new BehaviorTag(BehaviorKeyword.TargetFullHP, 3));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(ashe, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(8, damage,
                $"풀피 적 — 위력 5 + TargetFullHP 3 = 8. 실제 {damage}");
        }

        [Test]
        public void TargetFullHP_DamagedEnemy_NoBonus()
        {
            var ashe = CreateCharacter(70, 0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            enemy.Health.TakeDamage(50); // 풀피 아님
            var party = new List<Character> { ashe };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 5,
                new BehaviorTag(BehaviorKeyword.TargetFullHP, 3));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(ashe, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(5, damage,
                $"풀피 아닌 적 — 보너스 없음 (위력 5 그대로). 실제 {damage}");
        }

        [Test]
        public void TargetFullHP_RankSum_TwoTags_StackRanks()
        {
            var ashe = CreateCharacter(70, 0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { ashe };
            var enemies = new List<Character> { enemy };

            // 동일 키워드 2개 부착 — rank 합산 검증 (GetCombinedBehaviors 원칙)
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 5,
                new BehaviorTag(BehaviorKeyword.TargetFullHP, 2),
                new BehaviorTag(BehaviorKeyword.TargetFullHP, 4));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(ashe, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // rank 2 + rank 4 = 6 → 위력 5 + 6 = 11
            Assert.AreEqual(11, damage,
                $"TargetFullHP 2개 태그 rank 합산 (2+4=6) → 위력 5+6=11. 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 2. DesperationBehavior — 잃은 HP 10당 +1 위력
        // ═══════════════════════════════════════════

        [Test]
        public void Desperation_FullHP_NoBonus()
        {
            var ashe = CreateCharacter(70, 0, 0); // 풀피
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { ashe };
            var enemies = new List<Character> { enemy };

            // Brand of Ash (자해 없이 테스트): power 8 + Desperation rank 1
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 8,
                new BehaviorTag(BehaviorKeyword.Desperation, 1));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(ashe, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(8, damage,
                $"풀피 시전자 — 잃은 HP 0 → 보너스 없음 (위력 8). 실제 {damage}");
        }

        [Test]
        public void Desperation_Lost30HP_Adds3Damage()
        {
            var ashe = CreateCharacter(70, 0, 0);
            ashe.Health.TakeDamage(30); // 잃은 HP 30
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { ashe };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 8,
                new BehaviorTag(BehaviorKeyword.Desperation, 1)); // rank 1 → 잃은 HP/1
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(ashe, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 잃은 HP 30 / rank 1 = +30 → 위력 8 + 30 = 38
            Assert.AreEqual(38, damage,
                $"잃은 HP 30 — Desperation rank 1 → +30 위력 (8+30=38). 실제 {damage}");
        }

        [Test]
        public void Desperation_Rank2_HalvesBonus()
        {
            var ashe = CreateCharacter(70, 0, 0);
            ashe.Health.TakeDamage(30); // 잃은 HP 30
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { ashe };
            var enemies = new List<Character> { enemy };

            // rank 2 → 잃은 HP 2당 +1
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 8,
                new BehaviorTag(BehaviorKeyword.Desperation, 2));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(ashe, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 잃은 HP 30 / rank 2 = 15 → 위력 8 + 15 = 23
            Assert.AreEqual(23, damage,
                $"잃은 HP 30, rank 2 → +15 (8+15=23). 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 3. AllInBehavior — AP 0 시 +N 추가 데미지
        // ═══════════════════════════════════════════

        [Test]
        public void AllIn_APZero_AddsBonusDamage()
        {
            var ashe = CreateCharacter(70, 0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { ashe };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.AllIn, 10));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            // TurnContext AP=0 세팅
            var turnCtx = new TurnContext();
            turnCtx.ResetAP(5);
            turnCtx.SpendAP(5);
            Assert.AreEqual(0, turnCtx.CurrentAP, "AllIn 테스트 선행 조건 — AP=0");

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(ashe, skill, enemy, instance, turnCtx: turnCtx);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 위력 10 + AllIn 10 = 20 (PostApply에서 추가 데미지)
            Assert.AreEqual(20, damage,
                $"AP 0 시 — 위력 10 + AllIn 10 = 20. 실제 {damage}");
        }

        [Test]
        public void AllIn_APNonZero_NoBonus()
        {
            var ashe = CreateCharacter(70, 0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { ashe };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.AllIn, 10));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            var turnCtx = new TurnContext();
            turnCtx.ResetAP(5);
            // AP 소모 없음 — AP=5 유지
            Assert.AreNotEqual(0, turnCtx.CurrentAP, "AllIn 테스트 선행 조건 — AP>0");

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(ashe, skill, enemy, instance, turnCtx: turnCtx);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(10, damage,
                $"AP>0 시 — AllIn 미발동 (위력 10 그대로). 실제 {damage}");
        }

        [Test]
        public void AllIn_NoTurnCtx_NoBonus()
        {
            var ashe = CreateCharacter(70, 0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { ashe };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.AllIn, 10));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            // turnCtx=null — 일반 스킬 사용 시나리오 (파이프라인 기본값)
            pipeline.ExecuteSkill(ashe, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(10, damage,
                $"TurnCtx=null — AllIn 안전 스킵 (위력 10 그대로). 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 4. Ashe 콤보 시나리오 — 자해 폭딜 루프 (통합)
        // ═══════════════════════════════════════════

        [Test]
        public void Ashe_BrandOfAsh_BerserkAndDesperation_Stack()
        {
            // Ashe HP 70 → 30 도달 (Berserk + Desperation 동시 발동)
            var ashe = CreateCharacter(70, 0, 0);
            ashe.Health.TakeDamage(40); // 잃은 HP 40
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { ashe };
            var enemies = new List<Character> { enemy };

            // Brand of Ash: power 8 + Berserk(0) + Desperation(1)
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 8,
                new BehaviorTag(BehaviorKeyword.Berserk, 0),
                new BehaviorTag(BehaviorKeyword.Desperation, 1));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(ashe, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // Berserk(2배, HP 50%-) → (8+0)*2 = 16
            // 근데 PowerModify 순서: Berserk(50) → TargetFullHP(55) → FirstBlood(60) → ... → Desperation(70)
            // Berserk는 PowerModify 앞단에서 2배 → CurrentPower = 16
            // Desperation은 후순위 → 잃은 HP 40 / 1 = +40 → 16 + 40 = 56
            // (정밀 검증: Desperation은 power 변수 자체에 더함, Berserk와 곱연산이 아님)
            Assert.AreEqual(56, damage,
                $"Ashe HP 30 — Berserk(8→16) + Desperation(+40) = 56. 실제 {damage}");
        }

        [Test]
        public void Ashe_CinderAccretion_TargetFullHP_OnFreshEnemy()
        {
            var ashe = CreateCharacter(70, 0, 0);
            var enemy = CreateCharacter(500, 0, 0); // 풀피
            var party = new List<Character> { ashe };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 5,
                new BehaviorTag(BehaviorKeyword.TargetFullHP, 3));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(ashe, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(8, damage,
                $"Cinder Accretion 첫 턴 — 5+3=8 위력. 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 5. CC-2G-2 Duran — Bulwark + Desperation
        // ═══════════════════════════════════════════

        [Test]
        public void Duran_RevengeStrike_Bulwark_WithShield_AddsBonus()
        {
            var duran = CreateCharacter(120, 0, 0);
            duran.Health.AddShield(duran, 10, ShieldFlag.None); // 쉴드 보유
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { duran };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Bulwark, 5));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(duran, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(15, damage,
                $"Duran 쉴드 보유 시 — 10 + Bulwark 5 = 15. 실제 {damage}");
        }

        [Test]
        public void Duran_LastBastion_Desperation_BoostsShield()
        {
            var duran = CreateCharacter(120, 0, 0);
            duran.Health.TakeDamage(60); // 잃은 HP 60
            var target = CreateCharacter(100, 0, 0); // Self-cast는 무의미, 그냥 duran 스킬 자체에 집중
            var party = new List<Character> { duran };
            var enemies = new List<Character> { target };

            // Last Bastion: Shield Self 25 + Desperation(1)
            var skill = CreateSkill(SkillType.Shield, TargetType.Self, 25,
                new BehaviorTag(BehaviorKeyword.Desperation, 1));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            pipeline.ExecuteSkill(duran, skill, duran, instance);

            // Shield 25 + 잃은 HP 60/1 = 85 (shield량이 85로 적용되어야 함)
            Assert.AreEqual(85, duran.Health.CurrentShield,
                $"Last Bastion 잃은 HP 60 — Shield 25 + 60 = 85. 실제 {duran.Health.CurrentShield}");
        }

        // ═══════════════════════════════════════════
        // 6. CC-2G-3 Lumi — Bulwark + GiantSlayer
        // ═══════════════════════════════════════════

        [Test]
        public void Lumi_GlacialSpike_BulwarkAndGiantSlayer_OnBoss()
        {
            var lumi = CreateCharacter(75, 0, 0);
            lumi.Health.AddShield(lumi, 5, ShieldFlag.None); // Frost Armor 콤보
            var boss = CreateCharacter(150, 0, 0); // MaxHP 100+ (GiantSlayer)
            var party = new List<Character> { lumi };
            var enemies = new List<Character> { boss };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 12,
                new BehaviorTag(BehaviorKeyword.Bulwark, 4),
                new BehaviorTag(BehaviorKeyword.GiantSlayer, 5));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = boss.Health.CurrentHP;
            pipeline.ExecuteSkill(lumi, skill, boss, instance);

            int damage = hpBefore - boss.Health.CurrentHP;
            // 12 + Bulwark 4 (쉴드 보유) + GiantSlayer 5 (MaxHP 100+) = 21
            Assert.AreEqual(21, damage,
                $"Lumi Glacial Spike 보스전 — 12+4+5=21. 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 7. CC-2G-4 Taranis — Explosion (Charge 폭발)
        // ═══════════════════════════════════════════

        [Test]
        public void Taranis_Thunderstorm_Explosion_ChargeStackBonus()
        {
            var taranis = CreateCharacter(85, 0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            enemy.StatusEffects.ApplyEffect(StatusEffectType.Charge, 3, 3); // 기존 Charge 3스택
            var party = new List<Character> { taranis };
            var enemies = new List<Character> { enemy };

            // Thunderstorm: power 10 + Explosion(3). Charge 3스택 → 추가 9 (3×3).
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Explosion, 3));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(taranis, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 본 데미지 10 + Explosion 9 (Charge 3 × 3) = 19
            Assert.AreEqual(19, damage,
                $"Thunderstorm Charge 3스택 적 — 10 + 폭발 9 = 19. 실제 {damage}");
        }

        [Test]
        public void Taranis_Thunderstorm_Explosion_NoCharge_NoBonus()
        {
            var taranis = CreateCharacter(85, 0, 0);
            var enemy = CreateCharacter(500, 0, 0); // Charge 없음
            var party = new List<Character> { taranis };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Explosion, 3));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(taranis, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(10, damage,
                $"Charge 없는 적 — Explosion 미발동 (위력 10 그대로). 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 8. CC-2G-5 Sibyl — FollowUp / Echo / LimitBreak
        // ═══════════════════════════════════════════

        [Test]
        public void Sibyl_DeathProphecy_FollowUp_OnHitTarget_AddsBonus()
        {
            var sibyl = CreateCharacter(80, 0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            enemy.Health.TakeDamage(10); // 이미 맞은 상태 (HitThisTurn = true)
            var party = new List<Character> { sibyl };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 14,
                new BehaviorTag(BehaviorKeyword.FollowUp, 4));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(sibyl, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 14 + FollowUp 4 = 18
            Assert.AreEqual(18, damage,
                $"Sibyl Death Prophecy 이미 맞은 적 — 14+4=18. 실제 {damage}");
        }

        [Test]
        public void Sibyl_DeathProphecy_FollowUp_OnFreshTarget_NoBonus()
        {
            var sibyl = CreateCharacter(80, 0, 0);
            var enemy = CreateCharacter(500, 0, 0); // HitThisTurn = false
            var party = new List<Character> { sibyl };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 14,
                new BehaviorTag(BehaviorKeyword.FollowUp, 4));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(sibyl, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(14, damage,
                $"Sibyl 신선한 적 — FollowUp 미발동 (위력 14 그대로). 실제 {damage}");
        }

        [Test]
        public void Sibyl_DejaVu_Echo_HalfPowerSecondHit()
        {
            var sibyl = CreateCharacter(80, 0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { sibyl };
            var enemies = new List<Character> { enemy };

            // Déjà Vu: power 10 + Echo → 본 데미지 10 + Echo 5 (절반) = 15
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 10,
                new BehaviorTag(BehaviorKeyword.Echo, 0));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(sibyl, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(15, damage,
                $"Sibyl Déjà Vu — 위력 10 + Echo 절반 5 = 15. 실제 {damage}");
        }

        [Test]
        public void Sibyl_VisionOfRenewal_LimitBreak_FirstUseBoost()
        {
            var sibyl = CreateCharacter(80, 0, 0);
            var ally = CreateCharacter(100, 0, 0);
            ally.Health.TakeDamage(50); // HP 50
            var party = new List<Character> { sibyl, ally };
            var enemies = new List<Character> { CreateCharacter(100, 0, 0) };

            var skill = CreateSkill(SkillType.Heal, TargetType.SingleAlly, 12,
                new BehaviorTag(BehaviorKeyword.LimitBreak, 8));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int allyHpBefore = ally.Health.CurrentHP;
            pipeline.ExecuteSkill(sibyl, skill, ally, instance);

            int healed = ally.Health.CurrentHP - allyHpBefore;
            // 첫 사용 — 힐 12 + LimitBreak 8 = 20
            Assert.AreEqual(20, healed,
                $"Sibyl Vision 첫 사용 — 힐 12 + LimitBreak 8 = 20. 실제 {healed}");
        }

        [Test]
        public void Sibyl_VisionOfRenewal_LimitBreak_SecondUseNoBonus()
        {
            var sibyl = CreateCharacter(80, 0, 0);
            var ally = CreateCharacter(100, 0, 0);
            ally.Health.TakeDamage(70); // HP 30 (힐 받을 여유)
            var party = new List<Character> { sibyl, ally };
            var enemies = new List<Character> { CreateCharacter(100, 0, 0) };

            var skill = CreateSkill(SkillType.Heal, TargetType.SingleAlly, 12,
                new BehaviorTag(BehaviorKeyword.LimitBreak, 8));
            var instance = new SkillInstance(skill);
            instance.IncrementUsesThisBattle(); // 첫 사용 이미 했다고 가정
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int allyHpBefore = ally.Health.CurrentHP;
            pipeline.ExecuteSkill(sibyl, skill, ally, instance);

            int healed = ally.Health.CurrentHP - allyHpBefore;
            // 두 번째 사용 — LimitBreak 미발동. 힐 12만.
            Assert.AreEqual(12, healed,
                $"Sibyl Vision 두 번째 사용 — LimitBreak 미발동 (힐 12만). 실제 {healed}");
        }

        // ═══════════════════════════════════════════
        // 9. CC-2G-6 Umbra — Backstab FollowUp 콤보
        // ═══════════════════════════════════════════

        [Test]
        public void Umbra_Backstab_FollowUp_AfterPoisonBlade()
        {
            var umbra = CreateCharacter(75, 0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            enemy.Health.TakeDamage(5); // Poison Blade로 미리 때림 — HitThisTurn = true
            var party = new List<Character> { umbra };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 7,
                new BehaviorTag(BehaviorKeyword.FollowUp, 3));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(umbra, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // 7 + FollowUp 3 = 10
            Assert.AreEqual(10, damage,
                $"Umbra Backstab 이미 맞은 적 — 7+3=10. 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 10. CC-2G-7 Aster — Momentum
        // ═══════════════════════════════════════════

        [Test]
        public void Aster_QuickShot_Momentum_FirstUse_BasePower()
        {
            var aster = CreateCharacter(65, 0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { aster };
            var enemies = new List<Character> { enemy };

            // Quick Shot: power 4 + Momentum(1). UsesThisBattle=0 → 4.
            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 4,
                new BehaviorTag(BehaviorKeyword.Momentum, 1));
            var instance = new SkillInstance(skill);
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(aster, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(4, damage,
                $"Aster Quick Shot 첫 사용 (UsesThisBattle=0) — 위력 4 그대로. 실제 {damage}");
        }

        [Test]
        public void Aster_QuickShot_Momentum_ThirdUse_AddsTwo()
        {
            var aster = CreateCharacter(65, 0, 0);
            var enemy = CreateCharacter(500, 0, 0);
            var party = new List<Character> { aster };
            var enemies = new List<Character> { enemy };

            var skill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, 4,
                new BehaviorTag(BehaviorKeyword.Momentum, 1));
            var instance = new SkillInstance(skill);
            instance.IncrementUsesThisBattle();
            instance.IncrementUsesThisBattle(); // UsesThisBattle = 2
            var pipeline = new SkillExecutionPipeline(party, enemies);

            int hpBefore = enemy.Health.CurrentHP;
            pipeline.ExecuteSkill(aster, skill, enemy, instance);

            int damage = hpBefore - enemy.Health.CurrentHP;
            // Momentum: UsesThisBattle × rank = 2 × 1 = +2 → 4 + 2 = 6
            Assert.AreEqual(6, damage,
                $"Aster Quick Shot 세 번째 사용 (UsesThisBattle=2) — 4 + 2 = 6. 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 헬퍼
        // ═══════════════════════════════════════════

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
