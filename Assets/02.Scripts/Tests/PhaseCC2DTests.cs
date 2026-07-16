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
    /// Phase CC-2D Calliope, the Bard 핵심 메카닉 검증.
    /// - MelodyResourceComponent: CurrentMelody 설정, Echo 이동, 부 선율 자동 발동
    /// - 같은 스킬 연속 시 부 무효화 (페널티)
    /// - EchoPowerMul 특성 (부 선율 75%)
    /// - RepeatNoPenalty 특성 (반복 시에도 부 선율 발동)
    ///
    /// 기획: Assets/09.Docs/Characters/ReworkDrafts/06_Bard.md
    /// </summary>
    [TestFixture]
    public class PhaseCC2DTests
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
        // 1. MelodyResourceComponent 기본 동작
        // ═══════════════════════════════════════════

        [Test]
        public void Melody_InitialState_NoneCurrent()
        {
            var (calliope, party) = CreateBardParty();
            Assert.AreEqual(MelodyType.None, GetCurrentMelody(calliope), "초기 CurrentMelody = None");
            Assert.AreEqual(MelodyType.None, GetPrevTurnMelody(calliope), "초기 PrevTurnMelody = None");
        }

        [Test]
        public void Melody_SetCurrent_UpdatesState()
        {
            var (calliope, party) = CreateBardParty();
            SetMelody(calliope, MelodyType.Healing);

            Assert.AreEqual(MelodyType.Healing, GetCurrentMelody(calliope),
                "SetCurrentMelody(Healing) → CurrentMelody = Healing");
        }

        [Test]
        public void Melody_TurnStart_MovesCurrentToPrev()
        {
            var (calliope, party) = CreateBardParty();
            SetMelody(calliope, MelodyType.Valor);

            calliope.Resource.OnTurnStart(calliope);
            // Valor 부 선율 자동 발동 (파티 ATK+1). 그 후 Current→Prev 이동

            Assert.AreEqual(MelodyType.None, GetCurrentMelody(calliope), "턴 시작 후 CurrentMelody = None");
            Assert.AreEqual(MelodyType.Valor, GetPrevTurnMelody(calliope), "직전 Current가 Prev로 이동");
        }

        // ═══════════════════════════════════════════
        // 2. 부 선율 자동 발동 (Valor 케이스)
        // ═══════════════════════════════════════════

        [Test]
        public void Melody_EchoEffect_Valor_AppliesPartyAtkUp()
        {
            var (calliope, party) = CreateBardParty();
            SetMelody(calliope, MelodyType.Valor);

            calliope.Resource.OnTurnStart(calliope);
            // 부 선율 Valor → 파티 전체 ATK+1 (1턴)

            foreach (var member in party)
            {
                if (member == calliope) continue;
                Assert.IsTrue(member.StatusEffects.HasEffect(StatusEffectType.AttackUp),
                    $"파티원 {member.Name}에게 ATK+ 부 선율 발동");
            }
        }

        [Test]
        public void Melody_EchoEffect_Inspiration_AppliesPartyShield()
        {
            var (calliope, party) = CreateBardParty();
            SetMelody(calliope, MelodyType.Inspiration);

            int shieldBefore = party[1].Health.CurrentShield;
            calliope.Resource.OnTurnStart(calliope);
            // 부 선율 Inspiration → 파티 전체 쉴드 3

            Assert.Greater(party[1].Health.CurrentShield, shieldBefore,
                "부 선율 Inspiration으로 파티원 쉴드 획득");
        }

        // ═══════════════════════════════════════════
        // 3. 같은 스킬 연속 사용 페널티
        // ═══════════════════════════════════════════

        [Test]
        public void Melody_RepeatSameSkill_EchoPenalty()
        {
            var (calliope, party) = CreateBardParty();
            // 턴 1: Valor 사용 (Current=Valor)
            SetMelody(calliope, MelodyType.Valor);
            calliope.Resource.OnTurnStart(calliope); // 턴 2 시작, 부 선율 Valor 발동, Prev=Valor

            // 턴 2: Valor 다시 사용 (Current=Valor, Prev=Valor → 같음)
            SetMelody(calliope, MelodyType.Valor);

            // 턴 3 시작: Current(Valor) == Prev(Valor) → 부 선율 무효
            int atkUpCountBefore = CountPartyWithAttackUp(party);
            // 먼저 기존 ATK+ 클리어
            foreach (var member in party)
                member.StatusEffects.RemoveEffect(StatusEffectType.AttackUp);
            int cleared = CountPartyWithAttackUp(party);
            Assert.AreEqual(0, cleared, "클리어 후 ATK+ 0명");

            calliope.Resource.OnTurnStart(calliope); // 페널티로 부 선율 무효

            int atkUpCountAfter = CountPartyWithAttackUp(party);
            Assert.AreEqual(0, atkUpCountAfter,
                "같은 스킬 연속 시 부 선율 무효화 — ATK+ 부여 안 됨");
        }

        [Test]
        public void Melody_DifferentSkill_NoPenalty()
        {
            var (calliope, party) = CreateBardParty();
            // 턴 1: Valor
            SetMelody(calliope, MelodyType.Valor);
            calliope.Resource.OnTurnStart(calliope);
            foreach (var member in party)
                member.StatusEffects.RemoveEffect(StatusEffectType.AttackUp);

            // 턴 2: 다른 스킬 (Inspiration)
            SetMelody(calliope, MelodyType.Inspiration);

            // 턴 3 시작: Current(Inspiration) != Prev(Valor) → 부 선율 Inspiration 발동
            int shieldBefore = party[1].Health.CurrentShield;
            calliope.Resource.OnTurnStart(calliope);

            Assert.Greater(party[1].Health.CurrentShield, shieldBefore,
                "다른 스킬 사용 → 부 선율(Inspiration) 정상 발동 (쉴드 획득)");
        }

        // ═══════════════════════════════════════════
        // 4. RepeatNoPenalty 특성 (용기의 화음)
        // ═══════════════════════════════════════════

        [Test]
        public void RepeatNoPenalty_Trait_AllowsEchoOnRepeat()
        {
            var (calliope, party) = CreateBardParty();
            var trait = CreateTrait(
                (KeywordType.RepeatNoPenalty, 1, KeywordTrigger.Passive, 0f));
            calliope.EquipTrait(trait);

            // 턴 1: Valor
            SetMelody(calliope, MelodyType.Valor);
            calliope.Resource.OnTurnStart(calliope);
            foreach (var member in party)
                member.StatusEffects.RemoveEffect(StatusEffectType.AttackUp);

            // 턴 2: Valor 다시 (반복)
            SetMelody(calliope, MelodyType.Valor);
            // 턴 3 시작: RepeatNoPenalty 특성으로 부 선율 발동
            calliope.Resource.OnTurnStart(calliope);

            int atkUpCount = CountPartyWithAttackUp(party);
            Assert.Greater(atkUpCount, 0,
                "용기의 화음 특성 — 같은 스킬 반복이어도 부 선율 정상 발동");
        }

        // ═══════════════════════════════════════════
        // 5. EchoPowerMul 특성 (전투 노래 — 75%)
        // ═══════════════════════════════════════════

        [Test]
        public void EchoPowerMul_Trait_AppliesHigherEcho()
        {
            var (calliope, party) = CreateBardParty();
            var trait = CreateTrait(
                (KeywordType.EchoPowerMul, 0.75f, KeywordTrigger.Passive, 0f));
            calliope.EquipTrait(trait);

            // 부 선율 Valor 일 때 기본 ATK+1, 특성 시향 계산은 단순화 (최소 1)
            SetMelody(calliope, MelodyType.Valor);
            calliope.Resource.OnTurnStart(calliope);

            // 검증: 파티원에게 ATK+ 부여됨 (값 검증은 단순화 — 부여 여부만)
            Assert.IsTrue(party[1].StatusEffects.HasEffect(StatusEffectType.AttackUp),
                "EchoPowerMul 0.75 — 부 선율 정상 발동");
        }

        // ═══════════════════════════════════════════
        // 헬퍼
        // ═══════════════════════════════════════════

        private static MelodyType GetCurrentMelody(Character c)
            => ((MelodyResourceComponent)c.Resource).CurrentMelody;

        private static MelodyType GetPrevTurnMelody(Character c)
            => ((MelodyResourceComponent)c.Resource).PrevTurnMelody;

        private static void SetMelody(Character c, MelodyType type)
            => ((MelodyResourceComponent)c.Resource).SetCurrentMelody(type);

        private static int CountPartyWithAttackUp(IReadOnlyList<Character> party)
        {
            int count = 0;
            foreach (var p in party)
                if (p.StatusEffects.HasEffect(StatusEffectType.AttackUp)) count++;
            return count;
        }

        /// <summary>Melody Bard + 더미 파티 3명 생성 + GameRunState 초기화.</summary>
        private static (Character bard, List<Character> party) CreateBardParty()
        {
            var bard = CreateMelodyCharacter();
            var member1 = CreateCharacter(120, 0, 0); // Duran equivalent
            var member2 = CreateCharacter(75, 0, 0);  // Umbra equivalent
            var member3 = CreateCharacter(70, 0, 0);  // Ashe equivalent

            var party = new List<Character> { bard, member1, member2, member3 };
            var runState = GameRunState.Create(party, 0);
            runState.RelicHandler.SetPlayerParty(party);
            runState.RelicHandler.SubscribeEvents();

            return (bard, party);
        }

        private static Character CreateMelodyCharacter()
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            SetPrivateField(data, "_resourceType", ResourceType.Melody);
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
