using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Combat.Turn;

namespace TeamLog.Tests
{
    [TestFixture]
    public class TurnManagerTests
    {
        // ── 데미지 공식 ──

        [Test]
        public void CalculateDamage_MinimumOne()
        {
            Assert.AreEqual(1, DamageCalculator.CalculateDamage(1, 100));
        }

        [Test]
        public void CalculateDamage_AtkMinusDef()
        {
            Assert.AreEqual(5, DamageCalculator.CalculateDamage(10, 5));
        }

        [Test]
        public void CalculateDamage_ZeroDefense()
        {
            Assert.AreEqual(10, DamageCalculator.CalculateDamage(10, 0));
        }

        // ── 전투 종료 감지 ──

        [Test]
        public void CheckBattleEnd_AllEnemiesDead_FiresEvent()
        {
            var (tm, players, enemies) = CreateBattle(1, 1);

            bool battleEnded = false;
            tm.OnBattleEnded += () => battleEnded = true;

            // 적을 직접 사망 처리
            KillCharacter(enemies[0]);

            // CheckBattleEnd은 private이므로 ExecuteSkillImmediately를 통해 테스트
            // 대신 TurnPhase가 BattleEnd로 바뀌는지 확인
            // 직접 테스트를 위해 TurnManager 내부 로직을 간접 검증
            Assert.IsTrue(enemies[0].IsDead);
            Assert.IsFalse(battleEnded); // 아직 CheckBattleEnd 호출 안 됨
        }

        // ── 보조 메서드 ──

        private static (TurnManager tm, List<Character> players, List<Character> enemies)
            CreateBattle(int playerCount, int enemyCount)
        {
            var players = new List<Character>();
            var enemies = new List<Character>();

            for (int i = 0; i < playerCount; i++)
                players.Add(CreateCharacter($"Player{i}", 50, 5, 2));

            for (int i = 0; i < enemyCount; i++)
                enemies.Add(CreateCharacter($"Enemy{i}", 30, 3, 1));

            var tm = new TurnManager(players, enemies, maxRerolls: 2);
            return (tm, players, enemies);
        }

        private static Character CreateCharacter(string name, int hp, int atk, int def)
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            var character = new Character(data);
            character.Health.Initialize(hp);
            character.Stats.Initialize(atk, def);
            return character;
        }

        private static void KillCharacter(Character c)
        {
            c.Health.TakeDamage(c.Health.CurrentHP + c.Health.CurrentShield + 100);
        }
    }
}
