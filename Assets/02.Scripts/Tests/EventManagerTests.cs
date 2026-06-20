using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Event;
using TeamLog.Map;

using StatType = TeamLog.Characters.StatType;

namespace TeamLog.Tests
{
    /// <summary>
    /// EventManager 단위 테스트 (Phase E1/E2).
    /// 영구 강화, 확률 Outcome, ResultText 오염 방지, 조건부 검증을 확인한다.
    /// </summary>
    [TestFixture]
    public class EventManagerTests
    {
        private EventManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new EventManager();
            GameRunState.Destroy();
        }

        [TearDown]
        public void TearDown()
        {
            GameRunState.Destroy();
        }

        // ═══════════════════════════════════════════
        // 1. 영구 ATK 강화
        // ═══════════════════════════════════════════

        [Test]
        public void PermanentAtkBonus_AppliesToAllPartyMembers()
        {
            var party = new List<Character>
            {
                CreateCharacter(100, 10, 0),
                CreateCharacter(100, 15, 0)
            };
            var runState = CreateRunState(party);

            var eventData = CreateEvent(new EventChoice
            {
                ChoiceText = "선택",
                Outcome = new EventOutcome { PermanentAtkBonus = 3 }
            });

            _manager.ProcessChoice(eventData, 0, runState);

            Assert.AreEqual(13, party[0].Stats.GetStat(StatType.ATK), "파티원 1 ATK +3");
            Assert.AreEqual(18, party[1].Stats.GetStat(StatType.ATK), "파티원 2 ATK +3");
        }

        // ═══════════════════════════════════════════
        // 2. 영구 DEF 강화
        // ═══════════════════════════════════════════

        [Test]
        public void PermanentDefBonus_AppliesToAllPartyMembers()
        {
            var party = new List<Character>
            {
                CreateCharacter(100, 10, 5)
            };
            var runState = CreateRunState(party);

            var eventData = CreateEvent(new EventChoice
            {
                ChoiceText = "선택",
                Outcome = new EventOutcome { PermanentDefBonus = 2 }
            });

            _manager.ProcessChoice(eventData, 0, runState);

            Assert.AreEqual(7, party[0].Stats.GetStat(StatType.DEF), "파티원 DEF +2");
        }

        // ═══════════════════════════════════════════
        // 3. 리롤 토큰
        // ═══════════════════════════════════════════

        [Test]
        public void RerollTokensBonus_AddsToRunState()
        {
            var party = new List<Character> { CreateCharacter(100, 10, 0) };
            var runState = CreateRunState(party);
            int tokensBefore = runState.RerollTokens;

            var eventData = CreateEvent(new EventChoice
            {
                ChoiceText = "선택",
                Outcome = new EventOutcome { RerollTokensBonus = 2 }
            });

            _manager.ProcessChoice(eventData, 0, runState);

            Assert.AreEqual(tokensBefore + 2, runState.RerollTokens, "리롤 토큰 +2");
        }

        // ═══════════════════════════════════════════
        // 4. ResultText 오염 방지 — 복사본 반환
        // ═══════════════════════════════════════════

        [Test]
        public void ProcessChoice_ReturnsClone_DoesNotMutateOriginalAsset()
        {
            var party = new List<Character> { CreateCharacter(100, 10, 0) };
            var runState = CreateRunState(party);

            var originalOutcome = new EventOutcome
            {
                ResultText = "원본 텍스트",
                GoldChange = 10,
                GiveRandomItem = false
            };
            var eventData = CreateEvent(new EventChoice
            {
                ChoiceText = "선택",
                Outcome = originalOutcome
            });

            var returned = _manager.ProcessChoice(eventData, 0, runState);

            Assert.AreNotSame(originalOutcome, returned, "반환된 Outcome은 복사본이어야 함");
            Assert.AreEqual("원본 텍스트", originalOutcome.ResultText, "원본 ResultText 보존");
        }

        // ═══════════════════════════════════════════
        // 5. 확률 기반 Outcome — RandomOutcomes 중 하나 반환
        // ═══════════════════════════════════════════

        [Test]
        public void RandomOutcomes_PicksOneOfCandidates()
        {
            var party = new List<Character> { CreateCharacter(100, 10, 0) };
            var runState = CreateRunState(party);

            var outcomeA = new EventOutcome { ResultText = "A", GoldChange = 100 };
            var outcomeB = new EventOutcome { ResultText = "B", GoldChange = 200 };
            var parent = new EventOutcome
            {
                ResultText = "추첨...",
                RandomOutcomes = new List<EventOutcome> { outcomeA, outcomeB },
                OutcomeWeights = new List<float> { 1f, 1f }
            };

            var eventData = CreateEvent(new EventChoice
            {
                ChoiceText = "도박",
                Outcome = parent
            });

            // 100회 반복 추첨 — 둘 중 하나가 선택되는지 확인
            bool gotA = false, gotB = false;
            for (int i = 0; i < 100; i++)
            {
                var result = _manager.ProcessChoice(eventData, 0, runState);
                if (result.ResultText == "A") gotA = true;
                if (result.ResultText == "B") gotB = true;
                if (gotA && gotB) break;
            }

            Assert.IsTrue(gotA && gotB, "두 결과 모두 최소 1회 이상 추첨되어야 함");
        }

        [Test]
        public void RandomOutcomes_Weighted_ProportionalDistribution()
        {
            var party = new List<Character> { CreateCharacter(100, 10, 0) };
            var runState = CreateRunState(party);

            var rare = new EventOutcome { ResultText = "RARE", GoldChange = 1000 };
            var common = new EventOutcome { ResultText = "COMMON", GoldChange = 10 };
            var parent = new EventOutcome
            {
                ResultText = "추첨...",
                RandomOutcomes = new List<EventOutcome> { common, rare },
                OutcomeWeights = new List<float> { 90f, 10f }
            };

            var eventData = CreateEvent(new EventChoice
            {
                ChoiceText = "도박",
                Outcome = parent
            });

            int commonCount = 0;
            int total = 500;
            for (int i = 0; i < total; i++)
            {
                var result = _manager.ProcessChoice(eventData, 0, runState);
                if (result.ResultText == "COMMON") commonCount++;
            }

            // 90% 가중치이므로 500회 중 약 450회 (±여유) common이어야 함
            Assert.GreaterOrEqual(commonCount, 380, $"common이 가중치 비율(90%)로 추첨되어야 함 (실제: {commonCount}/{total})");
            Assert.LessOrEqual(commonCount, 500);
        }

        // ═══════════════════════════════════════════
        // 6. CanChoose 조건부 검증
        // ═══════════════════════════════════════════

        [Test]
        public void CanChoice_InsufficientGold_ReturnsFalse()
        {
            var party = new List<Character> { CreateCharacter(100, 10, 0) };
            var runState = CreateRunState(party);
            // 초기 골드는 0 또는 적음
            runState.AddGold(20); // 20G

            var choice = new EventChoice
            {
                ChoiceText = "구매",
                MinGoldRequired = 50
            };

            Assert.IsFalse(_manager.CanChoose(choice, runState), "골드 부족 시 false");
        }

        [Test]
        public void CanChoice_SufficientGold_ReturnsTrue()
        {
            var party = new List<Character> { CreateCharacter(100, 10, 0) };
            var runState = CreateRunState(party);
            runState.AddGold(100);

            var choice = new EventChoice
            {
                ChoiceText = "구매",
                MinGoldRequired = 50
            };

            Assert.IsTrue(_manager.CanChoose(choice, runState), "골드 충분 시 true");
        }

        [Test]
        public void CanChoice_HighPartyHP_ReturnsTrue()
        {
            var party = new List<Character> { CreateCharacter(100, 10, 0) };
            var runState = CreateRunState(party);

            var choice = new EventChoice
            {
                ChoiceText = "도박",
                MinPartyHPPercent = 0.5f
            };

            Assert.IsTrue(_manager.CanChoose(choice, runState), "HP 100%이므로 50% 조건 충족");
        }

        // ═══════════════════════════════════════════
        // 7. 골드 / HP 처리 정상 동작 (회귀 방지)
        // ═══════════════════════════════════════════

        [Test]
        public void GoldChange_Positive_AddsGold()
        {
            var party = new List<Character> { CreateCharacter(100, 10, 0) };
            var runState = CreateRunState(party);
            int before = runState.Gold;

            var eventData = CreateEvent(new EventChoice
            {
                ChoiceText = "획득",
                Outcome = new EventOutcome { GoldChange = 40 }
            });

            _manager.ProcessChoice(eventData, 0, runState);

            Assert.AreEqual(before + 40, runState.Gold);
        }

        [Test]
        public void HPPercentChange_Positive_HealsParty()
        {
            var party = new List<Character> { CreateCharacter(100, 10, 0) };
            party[0].Health.TakeDamage(50); // HP 100→50
            var runState = CreateRunState(party);

            var eventData = CreateEvent(new EventChoice
            {
                ChoiceText = "회복",
                Outcome = new EventOutcome { HPPercentChange = 30 }
            });

            _manager.ProcessChoice(eventData, 0, runState);

            // MaxHP 100 * 30% = 30 회복 → 50 + 30 = 80
            Assert.AreEqual(80, party[0].Health.CurrentHP);
        }

        [Test]
        public void StatusEffect_AppliesToAllPartyMembers()
        {
            var party = new List<Character>
            {
                CreateCharacter(100, 10, 0),
                CreateCharacter(100, 10, 0)
            };
            var runState = CreateRunState(party);

            var eventData = CreateEvent(new EventChoice
            {
                ChoiceText = "저주",
                Outcome = new EventOutcome
                {
                    ApplyStatusEffect = StatusEffectType.Poison,
                    StatusEffectDuration = 3,
                    StatusEffectValue = 2
                }
            });

            _manager.ProcessChoice(eventData, 0, runState);

            Assert.IsTrue(party[0].StatusEffects.HasEffect(StatusEffectType.Poison), "파티원 1 독 적용");
            Assert.IsTrue(party[1].StatusEffects.HasEffect(StatusEffectType.Poison), "파티원 2 독 적용");
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

        private static GameRunState CreateRunState(List<Character> party)
        {
            GameRunState.Create(party);
            return GameRunState.Instance;
        }

        private static EventData CreateEvent(params EventChoice[] choices)
        {
            var eventData = ScriptableObject.CreateInstance<EventData>();
            SetPrivateField(eventData, "_eventName", "TestEvent");
            SetPrivateField(eventData, "_description", "테스트");
            SetPrivateField(eventData, "_eventType", TeamLog.Event.EventType.NPC);
            SetPrivateField(eventData, "_choices", new List<EventChoice>(choices));
            return eventData;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (field != null)
                field.SetValue(obj, value);
        }
    }
}
