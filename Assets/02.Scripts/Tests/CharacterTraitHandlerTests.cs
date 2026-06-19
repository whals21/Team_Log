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
    /// 캐릭터 장착 특성(CharacterTraitHandler) 런타임 적용 검증 (Phase 8F).
    /// Character.PlayerTraitHandler가 CombatEventBus/SkillExecutor/DamageCalculator에
    /// 정상 반영되는지 각 키워드 카테고리별로 확인.
    /// </summary>
    [TestFixture]
    public class CharacterTraitHandlerTests
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
        // 1. ShieldPerTurn OnTurnStart — 쉴드 획득
        // ═══════════════════════════════════════════

        [Test]
        public void ShieldPerTurn_OnTurnStart_AppliesShieldToOwner()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(100, 5, 0);
            var party = new List<Character> { player };
            var enemies = new List<Character> { enemy };

            // 전사 기본 특성 — ShieldPerTurn +2 OnTurnStart
            var trait = CreateTrait(
                (KeywordType.ShieldPerTurn, 2, KeywordTrigger.OnTurnStart, 0f));
            player.EquipTrait(trait);
            player.PlayerTraitHandler.SubscribeEvents();

            int shieldBefore = player.Health.CurrentShield;
            CombatEventBus.FireTurnStart(1);

            Assert.AreEqual(shieldBefore + 2, player.Health.CurrentShield,
                $"OnTurnStart ShieldPerTurn +2 미적용 — 기대 {shieldBefore + 2}, 실제 {player.Health.CurrentShield}");
        }

        // ═══════════════════════════════════════════
        // 2. PowerAdd Passive — 공격 위력 증가
        // ═══════════════════════════════════════════

        [Test]
        public void PowerAdd_Passive_IncreasesAttackDamage()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(100, 5, 0);
            var party = new List<Character> { player };
            var enemies = new List<Character> { enemy };

            // 궁수 명사수 — PowerAdd +2 Passive
            var trait = CreateTrait(
                (KeywordType.PowerAdd, 2, KeywordTrigger.Passive, 0f));
            player.EquipTrait(trait);
            SetupRunStateWithTraitSubscription(party);

            var attack = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, power: 10);
            int hpBefore = enemy.Health.CurrentHP;
            var executor = new SkillExecutor(party, enemies);
            executor.ExecuteSkillInternal(player, attack, enemy);

            // ATK 10 + power 10 + PowerAdd 2 - DEF 0 = 22 (PowerAdd 없으면 20)
            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(22, damage,
                $"PowerAdd +2 미적용 — 기대 22, 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 3. DamageReduction Passive — 받는 데미지 감소
        // ═══════════════════════════════════════════

        [Test]
        public void DamageReduction_Passive_ReducesIncomingDamage()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(100, 20, 0); // ATK 20
            var party = new List<Character> { player };
            var enemies = new List<Character> { enemy };

            // 도적 회피의 대가 — DamageReduction 2 Passive
            var trait = CreateTrait(
                (KeywordType.DamageReduction, 2, KeywordTrigger.Passive, 0f));
            player.EquipTrait(trait);
            SetupRunStateWithTraitSubscription(party);

            int hpBefore = player.Health.CurrentHP;
            DamageCalculator.DealDamage(enemy, player, bonusPower: 5);
            // base: ATK 20 + power 5 - DEF 0 = 25, 특성 -2 = 23
            int damage = hpBefore - player.Health.CurrentHP;
            Assert.AreEqual(23, damage,
                $"DamageReduction 2 미적용 — 기대 23, 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 4. PowerMul OnEnemyLowHP — 대상 HP 낮을 시 위력 증폭
        // ═══════════════════════════════════════════

        [Test]
        public void PowerMul_OnEnemyLowHP_TriggersAtThreshold()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(100, 5, 0);
            enemy.Health.TakeDamage(60); // HP 40 → 40% (임계 0.6 이하)
            var party = new List<Character> { player };
            var enemies = new List<Character> { enemy };

            // 궁수 약점 포착 — PowerMul 2.0 OnEnemyLowHP(0.6) (float 정밀도 회피 위해 2.0 사용)
            var trait = CreateTrait(
                (KeywordType.PowerMul, 2.0f, KeywordTrigger.OnEnemyLowHP, 0.6f));
            player.EquipTrait(trait);
            SetupRunStateWithTraitSubscription(party);

            var attack = CreateSkill(SkillType.Attack, TargetType.SingleEnemy, power: 10);
            int hpBefore = enemy.Health.CurrentHP;
            var executor = new SkillExecutor(party, enemies);
            executor.ExecuteSkillInternal(player, attack, enemy);

            // power = (int)(10 * 2.0) = 20, ATK 10 + 20 - DEF 0 = 30
            int damage = hpBefore - enemy.Health.CurrentHP;
            Assert.AreEqual(30, damage,
                $"PowerMul 2.0 OnEnemyLowHP 미적용 — 기대 30, 실제 {damage}");
        }

        // ═══════════════════════════════════════════
        // 5. ExtraAP Passive — 매 턴 시작 시 AP 추가
        // ═══════════════════════════════════════════

        [Test]
        public void ExtraAP_Passive_AddsToTurnAP()
        {
            var player = CreateCharacter(100, 10, 0);
            var enemy = CreateCharacter(100, 5, 0);
            var party = new List<Character> { player };
            var enemies = new List<Character> { enemy };

            // 음유시인 전투 노래 — ExtraAP 1 Passive
            var trait = CreateTrait(
                (KeywordType.ExtraAP, 1, KeywordTrigger.Passive, 0f));
            player.EquipTrait(trait);
            SetupRunStateWithTraitSubscription(party);

            int extra = player.PlayerTraitHandler.GetExtraAP();
            Assert.AreEqual(1, extra,
                $"ExtraAP 1 쿼리 — 기대 1, 실제 {extra}");
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
            field?.SetValue(obj, value);
        }

        /// <summary>
        /// GameRunState 생성 + 파티 전체 특성 구독 + 유물 핸들러 구독.
        /// SkillExecutor/DamageCalculator가 Character.PlayerTraitHandler를 참조할 수 있도록
        /// runState.RelicHandler.SetPlayerParty로 party 연결 필수.
        /// </summary>
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
