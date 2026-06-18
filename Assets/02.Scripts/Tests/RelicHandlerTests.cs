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
using StatType = TeamLog.Characters.StatType;

namespace TeamLog.Tests
{
    /// <summary>
    /// 유물(Relic) 효과가 실제로 게임 로직에 반영되는지 검증하는 단위 테스트.
    /// 각 트리거 카테고리(Passive/OnTurnStart/OnKill/OnEnemyLowHP/OnShieldGained/OnRerollUsed)
    /// 별로 핵심 유물의 효과 적용을 확인한다.
    /// 추가: SubscribeEvents 누락 시 트리거 미동작 입증 (Full Run 시뮬레이터 버그 재현).
    /// </summary>
    [TestFixture]
    public class RelicHandlerTests
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
        // 1. BurningSword — Passive BonusOutgoingDamage +3
        // ═══════════════════════════════════════════

        [Test]
        public void PassiveBonusOutgoingDamage_AddsToEveryAttack()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(100, 5, 0);
            var party = new List<Character> { player };
            var enemies = new List<Character> { enemy };

            var attackSkill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, power: 5);
            var burningSword = CreateRelic(
                (KeywordType.BonusOutgoingDamage, 3, KeywordTrigger.Passive, 0f));

            SetupRunState(party, burningSword);

            int enemyHpBefore = enemy.Health.CurrentHP;
            var executor = new SkillExecutor(party, enemies);
            executor.ExecuteSkillInternal(player, attackSkill, enemy);

            // ATK 10 + power 5 - DEF 0 = 15, + BurningSword 3 = 18
            int damage = enemyHpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(18, damage,
                $"BurningSword(Passive BonusOutgoingDamage +3) 미적용 — 기대 18, 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 2. ExecutionerBlade — OnEnemyLowHP PowerMul 1.5
        // ═══════════════════════════════════════════

        [Test]
        public void PowerMul_OnEnemyLowHP_AppliesWhenTargetBelowThreshold()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(100, 5, 0);
            enemy.Health.TakeDamage(60); // HP 40 (40%)
            var party = new List<Character> { player };
            var enemies = new List<Character> { enemy };

            var attackSkill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, power: 10);
            var executioner = CreateRelic(
                (KeywordType.PowerMul, 1.5f, KeywordTrigger.OnEnemyLowHP, 0.5f));

            SetupRunState(party, executioner);

            int enemyHpBefore = enemy.Health.CurrentHP;
            var executor = new SkillExecutor(party, enemies);
            executor.ExecuteSkillInternal(player, attackSkill, enemy);

            // basePower 10 * 1.5 = 15, ATK 10 + 15 - DEF 0 = 25
            int damage = enemyHpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(25, damage,
                $"ExecutionerBlade(OnEnemyLowHP PowerMul 1.5) 미적용 — 기대 25, 실제 {damage}");
        }

        [Test]
        public void PowerMul_OnEnemyLowHP_DoesNotApplyWhenAboveThreshold()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(100, 5, 0); // HP 100 (100%)
            var party = new List<Character> { player };
            var enemies = new List<Character> { enemy };

            var attackSkill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, power: 10);
            var executioner = CreateRelic(
                (KeywordType.PowerMul, 1.5f, KeywordTrigger.OnEnemyLowHP, 0.5f));

            SetupRunState(party, executioner);

            int enemyHpBefore = enemy.Health.CurrentHP;
            var executor = new SkillExecutor(party, enemies);
            executor.ExecuteSkillInternal(player, attackSkill, enemy);

            // 임계 조건 불충족 — power 10 그대로, ATK 10 + 10 - 0 = 20
            int damage = enemyHpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(20, damage,
                $"임계 미충족 시 PowerMul 미적용 — 기대 20, 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 3. VampireFang — OnKillHeal +5 (SkillExecutor 경로)
        // ═══════════════════════════════════════════

        [Test]
        public void OnKillHeal_HealsCaster_OnKill()
        {
            var player = CreateCharacter(100, 10, 0);
            player.Health.TakeDamage(50); // HP 50
            var enemy = CreateCharacter(20, 5, 0); // HP 20 — 한 방에 사망
            var party = new List<Character> { player };
            var enemies = new List<Character> { enemy };

            var attackSkill = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, power: 100);
            var vampireFang = CreateRelic(
                (KeywordType.OnKillHeal, 5, KeywordTrigger.OnKill, 0f));

            SetupRunState(party, vampireFang);

            int playerHpBefore = player.Health.CurrentHP;
            var executor = new SkillExecutor(party, enemies);
            executor.ExecuteSkillInternal(player, attackSkill, enemy);

            Assert.IsTrue(enemy.IsDead, "적이 사망해야 함");
            int healAmount = player.Health.CurrentHP - playerHpBefore;
            Assert.AreEqual(5, healAmount,
                $"VampireFang(OnKillHeal +5) 미적용 — 기대 +5, 실제 +{healAmount}");
        }

        // ═══════════════════════════════════════════
        // 4. AegisCharm — OnTurnStart ShieldPerTurn +10
        // ═══════════════════════════════════════════

        [Test]
        public void ShieldPerTurn_OnTurnStart_AddsShieldToParty()
        {
            var p1 = CreateCharacter(100, 10, 0);
            var p2 = CreateCharacter(100, 10, 0);
            var party = new List<Character> { p1, p2 };

            var aegisCharm = CreateRelic(
                (KeywordType.ShieldPerTurn, 10, KeywordTrigger.OnTurnStart, 0f));

            SetupRunState(party, aegisCharm);

            Assert.AreEqual(0, p1.Health.CurrentShield);
            Assert.AreEqual(0, p2.Health.CurrentShield);

            CombatEventBus.FireTurnStart(1);

            Assert.AreEqual(10, p1.Health.CurrentShield, "파티원 1 쉴드 +10 예상");
            Assert.AreEqual(10, p2.Health.CurrentShield, "파티원 2 쉴드 +10 예상");
        }

        // ═══════════════════════════════════════════
        // 5. VerdantSeed — OnTurnStart HPPerTurn +3
        // ═══════════════════════════════════════════

        [Test]
        public void HPPerTurn_OnTurnStart_HealsParty()
        {
            var player = CreateCharacter(100, 10, 0);
            player.Health.TakeDamage(50); // HP 50
            var party = new List<Character> { player };

            var verdantSeed = CreateRelic(
                (KeywordType.HPPerTurn, 3, KeywordTrigger.OnTurnStart, 0f));

            SetupRunState(party, verdantSeed);

            CombatEventBus.FireTurnStart(1);
            Assert.AreEqual(53, player.Health.CurrentHP, "턴 시작 HP +3 예상");
        }

        // ═══════════════════════════════════════════
        // 6. CardShark — OnRerollUsed ShieldPerTurn +5
        // ═══════════════════════════════════════════

        [Test]
        public void ShieldPerTurn_OnRerollUsed_AddsShieldOnReroll()
        {
            var player = CreateCharacter(100, 10, 0);
            var party = new List<Character> { player };

            var cardShark = CreateRelic(
                (KeywordType.ShieldPerTurn, 5, KeywordTrigger.OnRerollUsed, 0f));

            SetupRunState(party, cardShark);

            Assert.AreEqual(0, player.Health.CurrentShield);
            CombatEventBus.FireRerollUsed();
            Assert.AreEqual(5, player.Health.CurrentShield, "리롤 시 쉴드 +5 예상");
        }

        // ═══════════════════════════════════════════
        // 7. SlayerSigil — StackingPowerOnKill (처치당 누적)
        // ═══════════════════════════════════════════

        [Test]
        public void StackingPowerOnKill_AccumulatesPerKill()
        {
            var player = CreateCharacter(100, 10, 0);
            var party = new List<Character> { player };

            var slayerSigil = CreateRelic(
                (KeywordType.StackingPowerOnKill, 2, KeywordTrigger.OnKill, 0f));

            SetupRunState(party, slayerSigil);

            Assert.AreEqual(0, GameRunState.Instance.RelicHandler.GetBonusOutgoingDamage(),
                "처치 0회 시 보너스 0");

            CombatEventBus.FireKill(CreateCharacter(10, 0, 0));
            Assert.AreEqual(2, GameRunState.Instance.RelicHandler.GetBonusOutgoingDamage(),
                "처치 1회 후 보너스 +2 예상");

            CombatEventBus.FireKill(CreateCharacter(10, 0, 0));
            Assert.AreEqual(4, GameRunState.Instance.RelicHandler.GetBonusOutgoingDamage(),
                "처치 2회 후 보너스 +4 예상");
        }

        // ═══════════════════════════════════════════
        // 8. AegisStrike — 트리거 체인 (OnShieldGained → BonusOutgoingDamage)
        // ═══════════════════════════════════════════

        [Test]
        public void BonusOutgoingDamage_OnShieldGained_AccumulatesAndConsumes()
        {
            var player = CreateCharacter(100, 10, 0);
            var party = new List<Character> { player };

            var aegisStrike = CreateRelic(
                (KeywordType.BonusOutgoingDamage, 4, KeywordTrigger.OnShieldGained, 0f));

            SetupRunState(party, aegisStrike);

            Assert.AreEqual(0, GameRunState.Instance.RelicHandler.PeekNextAttackBonus(),
                "초기 버프 0");

            CombatEventBus.FireShieldGained(player, 10);
            Assert.AreEqual(4, GameRunState.Instance.RelicHandler.PeekNextAttackBonus(),
                "쉴드 획득 시 다음 공격 강화 +4 누적 예상");

            // 추가 쉴드 획득 → 누적
            CombatEventBus.FireShieldGained(player, 5);
            Assert.AreEqual(8, GameRunState.Instance.RelicHandler.PeekNextAttackBonus(),
                "추가 쉴드 획득 시 누적 +8 예상");

            // 소비 — 1회 공격 후 리셋
            int consumed = GameRunState.Instance.RelicHandler.ConsumeNextAttackBonus();
            Assert.AreEqual(8, consumed);
            Assert.AreEqual(0, GameRunState.Instance.RelicHandler.PeekNextAttackBonus(),
                "소비 후 버프 0으로 리셋");
        }

        // ═══════════════════════════════════════════
        // 9. SubscribeEvents 누락 시 트리거 미동작 입증 (Full Run 시뮬레이터 버그)
        // ═══════════════════════════════════════════

        [Test]
        public void Relics_DoNotTrigger_WhenSubscribeEventsNotCalled()
        {
            var player = CreateCharacter(100, 10, 0);
            var party = new List<Character> { player };

            var runState = GameRunState.Create(party, 0);
            // ★ 의도적으로 SetPlayerParty / SubscribeEvents 호출 생략
            // (BalanceSimulator.Run.cs 가 동일한 패턴으로 유물 효과 누락 중)
            var aegisCharm = CreateRelic(
                (KeywordType.ShieldPerTurn, 10, KeywordTrigger.OnTurnStart, 0f));
            runState.AcquireRelic(aegisCharm);

            CombatEventBus.FireTurnStart(1);

            Assert.AreEqual(0, player.Health.CurrentShield,
                "SubscribeEvents 미호출 시 이벤트 구독 자체가 안 되어 유물 트리거가 동작하지 않아야 함. " +
                "이 테스트가 통과하면 Full Run 시뮬레이터의 RelicHandler가 동일한 이유로 무효임을 입증.");
        }

        // ═══════════════════════════════════════════
        // 10. 전투 종료 시 킬 스택 리셋
        // ═══════════════════════════════════════════

        [Test]
        public void KillStack_Resets_OnBattleEnd()
        {
            var player = CreateCharacter(100, 10, 0);
            var party = new List<Character> { player };

            var slayerSigil = CreateRelic(
                (KeywordType.StackingPowerOnKill, 2, KeywordTrigger.OnKill, 0f));

            SetupRunState(party, slayerSigil);

            CombatEventBus.FireKill(CreateCharacter(10, 0, 0));
            Assert.AreEqual(2, GameRunState.Instance.RelicHandler.GetBonusOutgoingDamage(),
                "처치 후 보너스 누적");

            CombatEventBus.FireBattleEnd(true);
            Assert.AreEqual(0, GameRunState.Instance.RelicHandler.GetBonusOutgoingDamage(),
                "전투 종료 시 킬 스택 리셋 예상");
        }

        // ═══════════════════════════════════════════
        // 11. DamageCalculator가 Passive 키워드를 직접 조회
        //     (SkillExecutor를 거치지 않는 경로 — DealDamage 호출)
        // ═══════════════════════════════════════════

        [Test]
        public void DamageCalculator_AppliesPassiveBonusOutgoingDamage()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(100, 5, 0);
            var party = new List<Character> { player };

            var burningSword = CreateRelic(
                (KeywordType.BonusOutgoingDamage, 3, KeywordTrigger.Passive, 0f));

            SetupRunState(party, burningSword);

            int enemyHpBefore = enemy.Health.CurrentHP;
            DamageCalculator.DealDamage(player, enemy, bonusPower: 0);

            // ATK 10 - DEF 0 = 10, + BurningSword 3 = 13
            int damage = enemyHpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(13, damage,
                $"DamageCalculator 직접 호출 시 Passive 보너스 적용 — 기대 13, 실제 {damage}");
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

        private static SkillData CreateSkill(SkillType type, TargetType target, int power, int cost = 0)
        {
            var skill = ScriptableObject.CreateInstance<SkillData>();
            SetPrivateField(skill, "_skillType", type);
            SetPrivateField(skill, "_targetType", target);
            SetPrivateField(skill, "_power", power);
            SetPrivateField(skill, "_cost", cost);
            return skill;
        }

        private static RelicData CreateRelic(
            params (KeywordType type, float value, KeywordTrigger trigger, float cond)[] keywords)
        {
            var relic = ScriptableObject.CreateInstance<RelicData>();
            var entries = new KeywordEntry[keywords.Length];
            for (int i = 0; i < keywords.Length; i++)
            {
                entries[i] = new KeywordEntry(
                    keywords[i].type, keywords[i].value, keywords[i].trigger, keywords[i].cond);
            }
            SetPrivateField(relic, "_keywords", entries);
            return relic;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        private static GameRunState SetupRunState(List<Character> party, params RelicData[] relics)
        {
            var runState = GameRunState.Create(party, 0);
            runState.RelicHandler.SetPlayerParty(party);
            runState.RelicHandler.SubscribeEvents();
            foreach (var relic in relics)
                runState.AcquireRelic(relic);
            return runState;
        }
    }
}
