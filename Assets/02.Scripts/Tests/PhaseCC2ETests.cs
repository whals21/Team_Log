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

using Random = System.Random;
using SkillData = TeamLog.Characters.SkillData;
using SkillType = TeamLog.Characters.SkillType;
using TargetType = TeamLog.Characters.TargetType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;

namespace TeamLog.Tests
{
    /// <summary>
    /// Phase CC-2E Cael, the Alchemist 핵심 메카닉 검증.
    /// - DiscoverSystem: 가중치 기반 무작위 추출, 중복 방지, 풀 크기 폴백
    /// - GetChoiceCount: 기본 3, "물약 명인" 특성 시 4
    /// - GetWeightMultiplier: "독성 폭발" 특성 — Crippling 카테고리 배수
    /// - ShouldApplyAll: "강화 물약" 특성 — 전투당 1회
    ///
    /// 기획: Assets/09.Docs/Characters/ReworkDrafts/05_Alchemist.md
    /// </summary>
    [TestFixture]
    public class PhaseCC2ETests
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
        // 1. RollOptions — 기본 가중치 추출 (중복 방지 + 시드 고정)
        // ═══════════════════════════════════════════

        [Test]
        public void RollOptions_BasicWeighted_ReturnsRequestedCount()
        {
            var pool = CreateTestPool(DiscoverCategory.Mending, 5);
            var rng = new Random(42);

            var options = DiscoverSystem.RollOptions(pool, null, rng);

            Assert.AreEqual(3, options.Count, "기본 선택지 수 = 3");
            Assert.NotNull(options[0].Skill, "첫 옵션 스킬 null 아님");
            Assert.NotNull(options[1].Skill, "둘째 옵션 스킬 null 아님");
            Assert.NotNull(options[2].Skill, "셋째 옵션 스킬 null 아님");
        }

        [Test]
        public void RollOptions_NoDuplicateSkills()
        {
            var pool = CreateTestPool(DiscoverCategory.Mending, 5);
            var rng = new Random(123);

            // 100회 반복 — 중복 미발생 검증
            for (int i = 0; i < 100; i++)
            {
                var options = DiscoverSystem.RollOptions(pool, null, new Random(i + 1000));
                var skillSet = new HashSet<SkillData>();
                foreach (var opt in options)
                {
                    CollectionAssert.DoesNotContain(skillSet, opt.Skill,
                        $"중복 스킬 발견: {opt.Skill.SkillName}");
                    skillSet.Add(opt.Skill);
                }
            }
            Assert.Pass("100회 시드 — 중복 없음");
        }

        [Test]
        public void RollOptions_PoolSmallerThanChoiceCount_ReturnsAll()
        {
            // 풀이 2개뿐이면 3개 요청해도 2개만 반환 (비복원 추출)
            var pool = CreateTestPool(DiscoverCategory.Mending, 2);
            var rng = new Random(42);

            var options = DiscoverSystem.RollOptions(pool, null, rng);

            Assert.AreEqual(2, options.Count, "풀이 2개면 2개만 반환");
        }

        // ═══════════════════════════════════════════
        // 2. GetChoiceCount — 특성 반영
        // ═══════════════════════════════════════════

        [Test]
        public void GetChoiceCount_Default_IsThree()
        {
            var cael = CreateAlchemistCharacter();

            int count = DiscoverSystem.GetChoiceCount(cael);

            Assert.AreEqual(3, count, "기본 발견 선택지 수 = 3");
        }

        [Test]
        public void GetChoiceCount_WithPotionMasterTrait_IsFour()
        {
            var cael = CreateAlchemistCharacter();
            var trait = CreateTrait((KeywordType.DiscoverChoicesAdd, 1, KeywordTrigger.Passive, 0f));
            cael.EquipTrait(trait);

            int count = DiscoverSystem.GetChoiceCount(cael);

            Assert.AreEqual(4, count, "물약 명인 특성 — 선택지 4");
        }

        // ═══════════════════════════════════════════
        // 3. GetWeightMultiplier — "독성 폭발" 특성
        // ═══════════════════════════════════════════

        [Test]
        public void GetWeightMultiplier_ToxicBurstTrait_CripplingCategoryDoubled()
        {
            var cael = CreateAlchemistCharacter();
            var trait = CreateTrait((KeywordType.DiscoverWeightBonus, 2.0f, KeywordTrigger.Passive, 0f));
            cael.EquipTrait(trait);

            float cripplingMul = DiscoverSystem.GetWeightMultiplier(DiscoverCategory.Crippling, cael);
            float mendingMul = DiscoverSystem.GetWeightMultiplier(DiscoverCategory.Mending, cael);

            Assert.AreEqual(2.0f, cripplingMul, 0.001f, "Crippling 가중치 2배");
            Assert.AreEqual(1.0f, mendingMul, 0.001f, "다른 카테고리는 가중치 변화 없음");
        }

        [Test]
        public void GetWeightMultiplier_NoTrait_AllOnes()
        {
            var cael = CreateAlchemistCharacter();

            foreach (DiscoverCategory cat in Enum.GetValues(typeof(DiscoverCategory)))
            {
                if (cat == DiscoverCategory.None) continue;
                float mul = DiscoverSystem.GetWeightMultiplier(cat, cael);
                Assert.AreEqual(1.0f, mul, 0.001f, $"특성 없으면 {cat} 가중치 = 1.0");
            }
        }

        // ═══════════════════════════════════════════
        // 4. ShouldApplyAll + ConsumeApplyAll — "강화 물약" 특성
        // ═══════════════════════════════════════════

        [Test]
        public void ShouldApplyAll_NoTrait_ReturnsFalse()
        {
            var cael = CreateAlchemistCharacter();

            Assert.IsFalse(DiscoverSystem.ShouldApplyAll(cael),
                "특성 없으면 ApplyAll 비활성");
        }

        [Test]
        public void ShouldApplyAll_WithTrait_BattleStartTrueAfterConsumeFalse()
        {
            var cael = CreateAlchemistCharacter();
            var trait = CreateTrait((KeywordType.DiscoverApplyAll, 1, KeywordTrigger.Passive, 0f));
            cael.EquipTrait(trait);
            // EquipTrait 후 구독 — SubscribeEvents는 _trait=null이면 스킵하므로 장착 후 명시 호출
            cael.PlayerTraitHandler.SubscribeEvents();
            // ApplyAll 가용성 설정은 OnBattleStart에서만 이루어지므로 테스트에서 수동 트리거
            CombatEventBus.FireBattleStart();

            Assert.IsTrue(cael.PlayerTraitHandler.CanUseDiscoverApplyAll(),
                "전투 시작 후 ApplyAll 가용");
            Assert.IsTrue(DiscoverSystem.ShouldApplyAll(cael),
                "강화 물약 특성 — ShouldApplyAll = true");

            // 1회 사용 후 소진
            DiscoverSystem.ConsumeApplyAll(cael);

            Assert.IsFalse(DiscoverSystem.ShouldApplyAll(cael),
                "1회 사용 후 ShouldApplyAll = false");
            Assert.IsFalse(cael.PlayerTraitHandler.CanUseDiscoverApplyAll(),
                "1회 사용 후 가용 플래그 소진");
        }

        // ═══════════════════════════════════════════
        // 헬퍼
        // ═══════════════════════════════════════════

        /// <summary>테스트용 발견 풀 생성 — 지정 카테고리에 N개 더미 스킬.</summary>
        private static DiscoverPoolData CreateTestPool(DiscoverCategory category, int entryCount)
        {
            var pool = ScriptableObject.CreateInstance<DiscoverPoolData>();
            SetPrivateField(pool, "_poolName", $"Test{category}");
            SetPrivateField(pool, "_category", category);

            var entries = new DiscoverEntry[entryCount];
            for (int i = 0; i < entryCount; i++)
            {
                var skill = ScriptableObject.CreateInstance<SkillData>();
                SetPrivateField(skill, "_skillName", $"{category}_{i}");
                SetPrivateField(skill, "_skillType", SkillType.Heal);
                SetPrivateField(skill, "_targetType", TargetType.SingleAlly);
                SetPrivateField(skill, "_power", 10 + i);
                entries[i] = new DiscoverEntry(skill, 10 + i * 5);
            }
            SetPrivateField(pool, "_entries", entries);
            return pool;
        }

        /// <summary>Cael(Alchemist) 캐릭터 생성 — 자원 없음.</summary>
        private static Character CreateAlchemistCharacter()
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            SetPrivateField(data, "_resourceType", ResourceType.None);
            var character = new Character(data);
            character.Health.Initialize(80);
            character.Stats.Initialize(0, 0);
            // NOTE: SubscribeEvents는 EquipTrait 후에 호출 필요 (특성 장착 전엔 스킵됨)
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
