using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Map;

namespace TeamLog.Tests
{
    [TestFixture]
    public class GameRunStateTests
    {
        // ── 골드 ──

        [Test]
        public void AddGold_IncreasesGold()
        {
            var state = CreateRunState(50);
            state.AddGold(30);
            Assert.AreEqual(80, state.Gold);
        }

        [Test]
        public void AddGold_FiresOnGoldChanged()
        {
            var state = CreateRunState(50);
            int newGold = -1;
            state.OnGoldChanged += g => newGold = g;
            state.AddGold(30);
            Assert.AreEqual(80, newGold);
        }

        [Test]
        public void SpendGold_DecreasesGold()
        {
            var state = CreateRunState(50);
            bool result = state.SpendGold(20);
            Assert.IsTrue(result);
            Assert.AreEqual(30, state.Gold);
        }

        [Test]
        public void SpendGold_Insufficient_ReturnsFalse()
        {
            var state = CreateRunState(50);
            bool result = state.SpendGold(100);
            Assert.IsFalse(result);
            Assert.AreEqual(50, state.Gold); // 변동 없음
        }

        // ── 보너스 AP ──

        [Test]
        public void Meditate_SetsBonusAP()
        {
            var state = CreateRunState();
            state.MeditateAtCampfire();
            Assert.AreEqual(1, state.BonusAP);
        }

        [Test]
        public void ConsumeBonusAP_ReturnsAndResets()
        {
            var state = CreateRunState();
            state.MeditateAtCampfire();
            int bonus = state.ConsumeBonusAP();
            Assert.AreEqual(1, bonus);
            Assert.AreEqual(0, state.BonusAP);
        }

        [Test]
        public void ConsumeBonusAP_WhenZero_ReturnsZero()
        {
            var state = CreateRunState();
            int bonus = state.ConsumeBonusAP();
            Assert.AreEqual(0, bonus);
        }

        // ── 층 이동 ──

        [Test]
        public void CurrentFloor_StartsAtOne()
        {
            var state = CreateRunState();
            Assert.AreEqual(1, state.CurrentFloor);
        }

        [Test]
        public void AdvanceToNextFloor_IncrementsFloor()
        {
            var state = CreateRunState();
            state.AdvanceToNextFloor();
            Assert.AreEqual(2, state.CurrentFloor);
        }

        [Test]
        public void AdvanceToNextFloor_FiresOnMapChanged()
        {
            var state = CreateRunState();
            bool mapChanged = false;
            state.OnMapChanged += _ => mapChanged = true;
            state.AdvanceToNextFloor();
            Assert.IsTrue(mapChanged);
        }

        // ── 휴식 ──

        [Test]
        public void TrainAtCampfire_IncreasesATK()
        {
            var state = CreateRunState();
            int atkBefore = state.PlayerParty[0].Stats.GetStat(StatType.ATK);
            state.TrainAtCampfire();
            Assert.Greater(state.PlayerParty[0].Stats.GetStat(StatType.ATK), atkBefore);
        }

        // ── 보조 ──

        private static GameRunState CreateRunState(int gold = 50)
        {
            var party = new List<Character>();
            var data = ScriptableObject.CreateInstance<CharacterData>();
            var c = new Character(data);
            c.Health.Initialize(50);
            c.Stats.Initialize(5, 2);
            party.Add(c);

            return GameRunState.Create(party, startingGold: gold);
        }
    }
}
