#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat.AI;
using TeamLog.Combat.Turn;
using TeamLog.Map;

using TeamLog.Combat;
using TeamLog.Combat.Draw;

using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;
using SkillInstance = TeamLog.Characters.SkillInstance;
using SkillType = TeamLog.Characters.SkillType;
using StatType = TeamLog.Characters.StatType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;
using TargetType = TeamLog.Characters.TargetType;

namespace TeamLog.Editor
{
    /// <summary>
    /// BalanceSimulator partial — 단일 전투 시뮬레이션 + SimulatedPlayerAI + Quick Combat 매트릭스.
    /// </summary>
    public static partial class BalanceSimulator
    {
        // ═══════════════════════════════════════════
        // 단일 전투 결과
        // ═══════════════════════════════════════════

        public class CombatResult
        {
            public string ScenarioName;
            public string Category; // "F1_Normal", "F1_Elite", "F1_Boss", ...
            public int Floor;
            public string EnemyName;
            public bool Victory;
            public int TurnCount;
            public float AvgRemainingHP; // 0~1
            public int Survivors;
        }

        // ═══════════════════════════════════════════
        // Quick Combat 시뮬레이션 (1000팩)
        // ═══════════════════════════════════════════

        private static List<CombatResult> RunQuickCombatSimulation()
        {
            var results = new List<CombatResult>(QuickCombatPacks);

            // 시나리오 매트릭스 (카테고리, 층, 팩 수, 생성 방식)
            var matrix = new (string category, int floor, int count, bool isElite, bool isBoss)[]
            {
                ("F1_Normal", 1, 200, false, false),
                ("F1_Elite",  1, 100, true,  false),
                ("F1_Boss",   1, 100, false, true),
                ("F2_Normal", 2, 150, false, false),
                ("F2_Elite",  2, 100, true,  false),
                ("F2_Boss",   2, 100, false, true),
                ("F3_Normal", 3, 100, false, false),
                ("F3_Elite",  3, 50,  true,  false),
                ("F3_Boss",   3, 100, false, true),
            };

            int totalDone = 0;
            try
            {
                foreach (var entry in matrix)
                {
                    var floorScaling = GetFloorScalingFor(entry.floor);
                    for (int i = 0; i < entry.count; i++)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar(
                            "Quick Combat 시뮬레이션",
                            $"{entry.category} ({i + 1}/{entry.count}) — 전체 {totalDone}/{QuickCombatPacks}",
                            (float)totalDone / QuickCombatPacks))
                        {
                            Debug.Log("[BalanceSimulator] 사용자 취소 — 부분 결과만 출력합니다.");
                            ClearProgress();
                            return results;
                        }

                        // 매 팩 새 파티 + 새 적
                        var party = CreateDefaultParty();
                        var enemies = CreateEnemiesFor(entry.floor, entry.isElite, entry.isBoss, floorScaling);
                        string scenario = $"{entry.category}_{i + 1:000}";

                        var result = RunSingleCombat(party, enemies, entry.isElite ? MapNodeType.Elite : MapNodeType.Boss, entry.floor, scenario, entry.category);
                        result.EnemyName = DescribeEnemies(enemies);
                        results.Add(result);

                        totalDone++;
                    }
                }
            }
            finally
            {
                ClearProgress();
            }

            return results;
        }

        /// <summary>층별 스케일 팩터 (GameRunState.GetFloorScaling와 동일).</summary>
        private static float GetFloorScalingFor(int floor)
        {
            return floor switch
            {
                1 => 1.0f,
                2 => 1.3f,
                3 => 1.6f,
                _ => 1.0f + (floor - 1) * 0.3f
            };
        }

        /// <summary>시나리오에 맞춰 적 리스트 생성. 매 팩 새 인스턴스.</summary>
        private static List<Character> CreateEnemiesFor(int floor, bool isElite, bool isBoss, float scaling)
        {
            List<Character> enemies;

            if (isBoss)
            {
                var bossData = GetBossData(floor);
                enemies = bossData != null ? new List<Character> { new Character(bossData) } : new List<Character>();
            }
            else
            {
                if (!_spawnTables.TryGetValue(floor, out var table))
                    return new List<Character>();

                enemies = isElite ? table.RollElitePattern() : table.RollNormalPattern();
            }

            // 층별 스케일 적용
            foreach (var enemy in enemies)
                enemy.ApplyFloorScaling(scaling);

            return enemies;
        }

        private static string DescribeEnemies(List<Character> enemies)
        {
            if (enemies == null || enemies.Count == 0) return "(없음)";
            if (enemies.Count == 1) return enemies[0].Data.CharacterName;
            // 동일 적이면 이름 + 수, 아니면 첫 적 + N
            return $"{enemies[0].Data.CharacterName} x{enemies.Count}";
        }

        // ═══════════════════════════════════════════
        // 단일 전투 실행
        // ═══════════════════════════════════════════

        /// <summary>
        /// 단일 전투 실행. 매 팩 새 TurnManager + 새 EnemyAIController + SimulatedPlayerAI.
        /// Quick Combat에서는 GameRunState 싱글톤을 사용하지 않음 (유물 효과 제외, null-safe).
        /// </summary>
        private static CombatResult RunSingleCombat(
            List<Character> party, List<Character> enemies,
            MapNodeType battleType, int floor, string scenarioName, string category)
        {
            // 정리: 이전 시뮬레이션 잔여 이벤트 구독 방지
            ClearCombatEventBus();

            // 적 AI 컨트롤러 생성 (캐릭터별 독립 패턴)
            var enemyControllers = new List<EnemyAIController>(enemies.Count);
            foreach (var enemy in enemies)
            {
                var pattern = CreatePatternFor(enemy);
                var controller = new EnemyAIController(enemy, pattern, party);
                enemyControllers.Add(controller);
            }

            // TurnManager 생성 — Quick Combat은 보너스 AP 없음
            var turnManager = new TurnManager(party, enemies, enemyControllers, maxRerolls: 1, bonusFirstTurnAP: 0);

            var ai = new SimulatedPlayerAI(turnManager, party, enemies, enemyControllers);

            turnManager.StartBattle();

            // 최초 적 행동 준비 (TurnManager가 안 함)
            foreach (var c in enemyControllers)
                if (c.Owner.IsAlive) c.PrepareNextAction();

            // 턴 루프
            int turns = 0;
            while (turnManager.CurrentPhase != TurnPhase.BattleEnd && turns < MaxTurnsPerCombat)
            {
                turns++;
                ai.PlayTurn(turns);

                if (turnManager.CurrentPhase == TurnPhase.BattleEnd)
                    break;

                // 다음 턴 적 행동 준비 (TurnManager.StartNewTurn 직후 상태)
                foreach (var c in enemyControllers)
                    if (c.Owner.IsAlive) c.PrepareNextAction();
            }

            bool victory = !party.TrueForAll(p => p.IsDead) && enemies.TrueForAll(e => e.IsDead);
            // 무한 루프 방지 임계치 도달 시 패배 처리
            if (turns >= MaxTurnsPerCombat && turnManager.CurrentPhase != TurnPhase.BattleEnd)
                victory = false;

            // 잔여 이벤트 정리 (메모리 누수 / 다음 팩 오염 방지)
            ClearCombatEventBus();

            int survivors = party.FindAll(p => p.IsAlive).Count;
            float avgHp = ComputeAvgRemainingHP(party);

            return new CombatResult
            {
                ScenarioName = scenarioName,
                Category = category,
                Floor = floor,
                EnemyName = DescribeEnemies(enemies),
                Victory = victory,
                TurnCount = turns,
                AvgRemainingHP = avgHp,
                Survivors = survivors
            };
        }

        private static float ComputeAvgRemainingHP(IReadOnlyList<Character> party)
        {
            if (party == null || party.Count == 0) return 0f;
            float sum = 0f;
            foreach (var c in party)
            {
                if (c.Health.MaxHP <= 0) continue;
                sum += (float)c.Health.CurrentHP / c.Health.MaxHP;
            }
            return sum / party.Count;
        }

        private static void ClearCombatEventBus()
        {
            CombatEventBus.Clear();
            DamageCalculator.ClearEvents();
            SkillExecutor.ClearEvents();
        }

        // ═══════════════════════════════════════════
        // SimulatedPlayerAI — 휴리스틱 기반 합리적 플레이어
        // ═══════════════════════════════════════════

        private class SimulatedPlayerAI
        {
            private readonly TurnManager _turnManager;
            private readonly List<Character> _party;
            private readonly List<Character> _enemies;
            private readonly List<EnemyAIController> _enemyControllers;

            public SimulatedPlayerAI(TurnManager turnManager, List<Character> party,
                List<Character> enemies, List<EnemyAIController> enemyControllers)
            {
                _turnManager = turnManager;
                _party = party;
                _enemies = enemies;
                _enemyControllers = enemyControllers;
            }

            public void PlayTurn(int turnNumber)
            {
                if (_turnManager.CurrentPhase != TurnPhase.PlayerAction) return;

                // 1) 리롤 평가 — 최악 슬롯이 20점 미만이면 리롤
                TryRerollWorstSlot();

                // 2) 최고점 슬롯 시전 루프
                int safety = 32; // 슬롯 수 + AP 합 이상의 가드
                while (safety-- > 0)
                {
                    if (_turnManager.CurrentPhase == TurnPhase.BattleEnd) return;
                    if (_turnManager.CurrentAP <= 0) break;

                    int bestIdx = FindBestUsableSlot(out int bestScore, out var bestTarget);
                    if (bestIdx < 0 || bestScore <= 0) break;

                    ExecuteSlot(bestIdx, bestTarget);
                }

                // 3) 턴 확정 — 적 턴 + 다음 턴 시작까지 동기 실행
                if (_turnManager.CurrentPhase == TurnPhase.PlayerAction)
                    _turnManager.ConfirmActions();
            }

            // ── 리롤 ──

            private void TryRerollWorstSlot()
            {
                var slots = _turnManager.DrawSystem.DrawnSlots;
                if (slots == null || slots.Count == 0) return;
                if (!_turnManager.DrawSystem.CanReroll) return;

                int worstIdx = -1;
                int worstScore = int.MaxValue;
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].IsSelected) continue;
                    int s = EvaluateSlot(slots[i]);
                    if (s < worstScore)
                    {
                        worstScore = s;
                        worstIdx = i;
                    }
                }

                if (worstIdx >= 0 && worstScore < 20)
                    _turnManager.RerollSlot(worstIdx);
            }

            // ── 슬롯 선택 ──

            private int FindBestUsableSlot(out int bestScore, out Character bestTarget)
            {
                bestScore = -1;
                bestTarget = null;

                var slots = _turnManager.DrawSystem.DrawnSlots;
                if (slots == null) return -1;

                int bestIdx = -1;
                for (int i = 0; i < slots.Count; i++)
                {
                    var slot = slots[i];
                    if (slot == null || slot.IsSelected) continue;
                    if (slot.Instance == null || slot.Skill == null) continue;
                    if (slot.Caster == null || !slot.Caster.IsAlive) continue;

                    int cost = slot.Instance.EffectiveCost;
                    if (_turnManager.CurrentAP < cost) continue;

                    int score = EvaluateSlot(slot);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIdx = i;
                        bestTarget = SelectTarget(slot);
                    }
                }
                return bestIdx;
            }

            private void ExecuteSlot(int idx, Character target)
            {
                var slots = _turnManager.DrawSystem.DrawnSlots;
                if (idx < 0 || idx >= slots.Count) return;

                var slot = slots[idx];
                if (slot == null || slot.Skill == null) return;

                slot.IsSelected = true;
                _turnManager.ExecuteSkillImmediately(slot.Caster, slot.Skill, target, slot.Instance);
            }

            // ── 슬롯 평가 ──

            private int EvaluateSlot(DrawnSkillSlot slot)
            {
                if (slot?.Skill == null || slot.Caster == null) return 0;

                var skill = slot.Skill;
                int cost = slot.Instance?.EffectiveCost ?? skill.Cost;
                if (_turnManager.CurrentAP < cost) return 0;

                int score;
                switch (skill.Type)
                {
                    case SkillType.Heal:
                        score = ScoreHeal(skill);
                        break;
                    case SkillType.Shield:
                        score = ScoreShield(skill);
                        break;
                    case SkillType.Attack:
                        score = ScoreAttack(skill);
                        break;
                    case SkillType.Buff:
                        score = ScoreBuff(skill);
                        break;
                    case SkillType.Debuff:
                        score = ScoreDebuff(skill);
                        break;
                    case SkillType.Purify:
                        score = ScorePurify(skill);
                        break;
                    default:
                        score = 30;
                        break;
                }

                // 코스트 효율 가산 — 싼 스킬에 약간 가산
                score += (3 - cost) * 2;

                return Mathf.Max(0, score);
            }

            private int ScoreHeal(SkillData skill)
            {
                var lowMember = FindLowestHPAlly();
                if (lowMember == null) return 20;

                float ratio = HP(lowMember);
                if (ratio < 0.3f) return 95;
                if (ratio < 0.5f) return 85;
                return 20;
            }

            private int ScoreShield(SkillData skill)
            {
                var threat = ComputeEnemyThreat();
                var lowMember = FindLowestHPAlly();
                float ratio = lowMember != null ? HP(lowMember) : 1f;
                if (threat > 0 && ratio < 0.5f) return 75;
                if (ratio < 0.3f) return 60;
                return 25;
            }

            private int ScoreAttack(SkillData skill)
            {
                var killable = FindKillableEnemy(skill);
                if (killable != null) return 90;

                int baseScore = 50 + skill.Power / 5;
                return Mathf.Min(89, baseScore);
            }

            private int ScoreBuff(SkillData skill)
            {
                return _turnManager.TurnNumber == 1 ? 80 : 40;
            }

            private int ScoreDebuff(SkillData skill)
            {
                foreach (var e in _enemies)
                    if (e.IsAlive && IsThreateningEnemy(e)) return 60;
                return 30;
            }

            private int ScorePurify(SkillData skill)
            {
                foreach (var p in _party)
                {
                    if (!p.IsAlive) continue;
                    if (HasHarmfulStatus(p)) return 70;
                }
                return 15;
            }

            // ── 타겟 선택 ──

            private Character SelectTarget(DrawnSkillSlot slot)
            {
                var skill = slot.Skill;
                switch (skill.Target)
                {
                    case TargetType.SingleEnemy:
                        return SelectSingleEnemyTarget(skill);
                    case TargetType.SingleAlly:
                        return skill.Type == SkillType.Heal || skill.Type == SkillType.Shield
                            ? FindLowestHPAlly()
                            : FindHighestATKAlly();
                    case TargetType.Self:
                    case TargetType.AllEnemies:
                    case TargetType.AllAllies:
                        return null;
                }
                return null;
            }

            private Character SelectSingleEnemyTarget(SkillData skill)
            {
                // 1순위: 처결 가능 적
                var killable = FindKillableEnemy(skill);
                if (killable != null) return killable;

                // 2순위: 위협적 intent 적
                Character threat = null;
                int threatVal = 0;
                foreach (var c in _enemyControllers)
                {
                    if (c?.Owner == null || !c.Owner.IsAlive) continue;
                    int v = EstimateThreatValue(c);
                    if (v > threatVal)
                    {
                        threatVal = v;
                        threat = c.Owner;
                    }
                }
                if (threat != null) return threat;

                // 3순위: 최저 HP 적
                Character lowest = null;
                int lowestHP = int.MaxValue;
                foreach (var e in _enemies)
                {
                    if (e == null || !e.IsAlive) continue;
                    if (e.Health.CurrentHP < lowestHP)
                    {
                        lowestHP = e.Health.CurrentHP;
                        lowest = e;
                    }
                }
                return lowest;
            }

            // ── 유틸리티 ──

            private Character FindLowestHPAlly()
            {
                Character result = null;
                float lowestRatio = 1.5f;
                foreach (var p in _party)
                {
                    if (p == null || !p.IsAlive) continue;
                    float r = HP(p);
                    if (r < lowestRatio)
                    {
                        lowestRatio = r;
                        result = p;
                    }
                }
                return result;
            }

            private Character FindHighestATKAlly()
            {
                Character result = null;
                int bestATK = -1;
                foreach (var p in _party)
                {
                    if (p == null || !p.IsAlive) continue;
                    int atk = p.Stats.GetStat(StatType.ATK);
                    if (atk > bestATK)
                    {
                        bestATK = atk;
                        result = p;
                    }
                }
                return result;
            }

            private Character FindKillableEnemy(SkillData skill)
            {
                if (skill.Type != SkillType.Attack) return null;
                int projectedDamage = EstimateDamage(skill);
                foreach (var e in _enemies)
                {
                    if (e == null || !e.IsAlive) continue;
                    if (e.Health.CurrentHP - e.Health.CurrentShield <= projectedDamage)
                        return e;
                }
                return null;
            }

            private int EstimateDamage(SkillData skill)
            {
                // SkillExecutor 기준: 데미지 = caster.ATK + skill.Power - target.DEF (대략)
                int totalCasterATK = 0;
                int aliveCount = 0;
                foreach (var p in _party)
                {
                    if (p != null && p.IsAlive)
                    {
                        totalCasterATK += p.Stats.GetStat(StatType.ATK);
                        aliveCount++;
                    }
                }
                int avgATK = aliveCount > 0 ? totalCasterATK / aliveCount : 0;
                // 평균 적 DEF를 약 5로 가정 (정확도보다 휴리스틱 우선)
                return Mathf.Max(1, avgATK + skill.Power - 5);
            }

            private int ComputeEnemyThreat()
            {
                int total = 0;
                foreach (var c in _enemyControllers)
                {
                    if (c?.Owner == null || !c.Owner.IsAlive) continue;
                    total += EstimateThreatValue(c);
                }
                return total;
            }

            private int EstimateThreatValue(EnemyAIController c)
            {
                var intent = c.CurrentIntent;
                if (intent == null) return 0;
                // 공격/디버프 intent에 가중치
                return intent.Type switch
                {
                    EnemyIntentType.Attack => Mathf.Max(1, intent.Value),
                    EnemyIntentType.Debuff => 5,
                    _ => 0
                };
            }

            private bool IsThreateningEnemy(Character enemy)
            {
                if (enemy == null) return false;
                // 적의 첫 스킬이 공격이면 위협으로 간주
                var skills = enemy.Data.Skills;
                if (skills == null) return false;
                foreach (var s in skills)
                {
                    if (s != null && s.Type == SkillType.Attack) return true;
                }
                return false;
            }

            private bool HasHarmfulStatus(Character c)
            {
                if (c == null || c.StatusEffects == null) return false;
                var effects = c.StatusEffects.GetAllEffects();
                if (effects == null) return false;
                foreach (var eff in effects)
                {
                    if (eff.Type == StatusEffectType.Poison ||
                        eff.Type == StatusEffectType.Burn ||
                        eff.Type == StatusEffectType.Bleed ||
                        eff.Type == StatusEffectType.AttackDown ||
                        eff.Type == StatusEffectType.DefenseDown ||
                        eff.Type == StatusEffectType.Stun)
                        return true;
                }
                return false;
            }

            private static float HP(Character c)
            {
                if (c == null || c.Health == null || c.Health.MaxHP <= 0) return 1f;
                return (float)c.Health.CurrentHP / c.Health.MaxHP;
            }
        }
    }
}
#endif
