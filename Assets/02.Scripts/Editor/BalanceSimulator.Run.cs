#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat.AI;
using TeamLog.Combat.Turn;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.Shop;
using TeamLog.Event;
using TeamLog.Skill;

using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;

namespace TeamLog.Editor
{
    /// <summary>
    /// BalanceSimulator partial — Full Run 시뮬레이션 (100회).
    /// 매 런마다 GameRunState.Create/Destroy로 런타임 상태를 격리하고,
    /// 맵 노드를 자동으로 선택/처리하여 런 클리어율과 사망 분포를 측정한다.
    /// </summary>
    public static partial class BalanceSimulator
    {
        // ═══════════════════════════════════════════
        // 런 결과
        // ═══════════════════════════════════════════

        public class FullRunResult
        {
            public int RunIndex;
            public bool RunCompleted;
            public int FloorReached;
            public string DeathLocation; // "F1_Battle", "F2_Elite", "Cleared"
            public int TotalBattles;
            public int TotalTurns;
            public float FinalPartyHP; // 0~1
            public int Survivors;
            public int GoldEarned;
            public int RelicsAcquired;
            public int AugmentsAcquired;
        }

        // ═══════════════════════════════════════════
        // Full Run 시뮬레이션 (100회)
        // ═══════════════════════════════════════════

        private static List<FullRunResult> RunFullRunSimulation()
        {
            var results = new List<FullRunResult>(FullRunCount);

            try
            {
                for (int i = 0; i < FullRunCount; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                        "Full Run 시뮬레이션",
                        $"Run {i + 1}/{FullRunCount}",
                        (float)i / FullRunCount))
                    {
                        Debug.Log("[BalanceSimulator] 사용자 취소 — 부분 결과만 출력합니다.");
                        ClearProgress();
                        return results;
                    }

                    var result = RunSingleRun(i + 1);
                    results.Add(result);

                    // 안전장치: 매 런 종료 후 싱글톤 정리
                    GameRunState.Destroy();
                    ClearCombatEventBus();
                }
            }
            finally
            {
                ClearProgress();
                // 최종 정리
                GameRunState.Destroy();
                ClearCombatEventBus();
            }

            return results;
        }

        // ═══════════════════════════════════════════
        // 단일 런 실행
        // ═══════════════════════════════════════════

        private static FullRunResult RunSingleRun(int runIndex)
        {
            // 매 런 새 파티
            var party = CreateDefaultParty();

            // GameRunState 생성 — 골드 50 시작
            var runState = GameRunState.Create(party, DefaultStartingGold);
            runState.SetDataPools(_relicPool, _augmentPool);
            runState.StartRun();

            var result = new FullRunResult
            {
                RunIndex = runIndex,
                FloorReached = 1
            };

            int maxNodesPerRun = 200; // 안전 가드
            while (runState.IsRunActive && maxNodesPerRun-- > 0)
            {
                // 다음 노드 후보
                var available = runState.CurrentMap.GetAvailableNodes();
                if (available == null || available.Count == 0)
                {
                    // 폴백: 다음 층 강제 진입 (비정상 상황)
                    if (runState.CurrentFloor < runState.TotalFloors)
                        runState.AdvanceToNextFloor();
                    else
                        break;
                    continue;
                }

                var nextNode = SelectNextNode(available, runState);
                if (nextNode == null) break;

                runState.CurrentMap.MoveToNode(nextNode);

                // 노드 처리
                bool partyWiped = ProcessNode(nextNode, runState, result);
                if (partyWiped)
                {
                    result.RunCompleted = false;
                    result.DeathLocation = $"F{runState.CurrentFloor}_{nextNode.NodeType}";
                    runState.EndRunDefeat();
                    break;
                }

                result.FloorReached = runState.CurrentFloor;

                // 런 클리어 체크 — 보스 클리어 시 AdvanceToNextFloor가 IsRunComplete=true로 설정
                if (runState.IsRunComplete)
                {
                    result.RunCompleted = true;
                    result.DeathLocation = "Cleared";
                    break;
                }
            }

            // 런 종료 통계 수집
            result.FinalPartyHP = ComputeAvgRemainingHP(runState.PlayerParty);
            result.Survivors = CountSurvivors(runState.PlayerParty);
            result.GoldEarned = runState.TotalGoldEarned;
            result.RelicsAcquired = runState.RelicHandler.Relics.Count;

            int augCount = 0;
            foreach (var member in runState.PlayerParty)
                if (member != null)
                    foreach (var inst in member.SkillInventory.SkillInstances)
                        augCount += inst.Augments.Count;
            result.AugmentsAcquired = augCount;

            return result;
        }

        // ═══════════════════════════════════════════
        // 노드 선택 — 휴리스틱
        // ═══════════════════════════════════════════

        private static MapNode SelectNextNode(IReadOnlyList<MapNode> nodes, GameRunState runState)
        {
            if (nodes == null || nodes.Count == 0) return null;
            if (nodes.Count == 1) return nodes[0];

            float avgHP = ComputeAvgRemainingHP(runState.PlayerParty);
            bool crisis = avgHP < 0.4f;

            // 위기 시: Rest > Shop > Event > Battle > Elite
            // 여유 시: Elite > Battle > Event > Shop > Rest
            int[] priority = crisis
                ? new[] { (int)MapNodeType.Rest, (int)MapNodeType.Shop, (int)MapNodeType.Event, (int)MapNodeType.Battle, (int)MapNodeType.Elite }
                : new[] { (int)MapNodeType.Elite, (int)MapNodeType.Battle, (int)MapNodeType.Event, (int)MapNodeType.Shop, (int)MapNodeType.Rest };

            foreach (int typeInt in priority)
            {
                var type = (MapNodeType)typeInt;
                foreach (var n in nodes)
                    if (n.NodeType == type) return n;
            }

            // Boss는 마지막 우선순위 (강제 진입)
            foreach (var n in nodes)
                if (n.NodeType == MapNodeType.Boss) return n;

            return nodes[0];
        }

        // ═══════════════════════════════════════════
        // 노드 처리
        // ═══════════════════════════════════════════

        /// <summary>노드 처리 후 파티 전멸 여부 반환.</summary>
        private static bool ProcessNode(MapNode node, GameRunState runState, FullRunResult result)
        {
            switch (node.NodeType)
            {
                case MapNodeType.Battle:
                    return ProcessCombatNode(MapNodeType.Battle, runState, result);
                case MapNodeType.Elite:
                    return ProcessCombatNode(MapNodeType.Elite, runState, result);
                case MapNodeType.Boss:
                    return ProcessBossNode(runState, result);
                case MapNodeType.Shop:
                    ProcessShopNode(runState);
                    return false;
                case MapNodeType.Event:
                    ProcessEventNode(runState);
                    return false;
                case MapNodeType.Rest:
                    ProcessRestNode(runState);
                    return false;
            }
            return false;
        }

        // ── 전투 노드 (일반/엘리트) ──

        private static bool ProcessCombatNode(MapNodeType battleType, GameRunState runState, FullRunResult result)
        {
            // TurnManager는 List<Character>를 요구하므로 복사본 생성 (캐릭터 인스턴스는 동일 — HP 유지)
            var party = new List<Character>(runState.PlayerParty);
            var enemies = CreateEnemiesFor(runState.CurrentFloor, battleType == MapNodeType.Elite, false, runState.GetFloorScaling());
            if (enemies.Count == 0) return false;

            result.TotalBattles++;

            // 명상 보너스 AP (있으면)
            int bonusAP = runState.ConsumeBonusAP();

            var combatResult = RunSingleRunWithState(party, enemies, battleType, runState.CurrentFloor, bonusAP);
            result.TotalTurns += combatResult.TurnCount;

            // 전투 후 상태이상/쉴드 정리 (파티만)
            CleanupAfterCombat(party);

            if (combatResult.Victory)
            {
                runState.OnBattleVictory();
                var rewardMgr = new RewardManager();
                var rewards = rewardMgr.GenerateRewards(battleType, runState);
                if (rewards != null && rewards.Count > 0)
                {
                    var chosen = ChooseBestReward(rewards);
                    if (chosen != null) rewardMgr.ApplyReward(chosen, runState);
                }
                return false;
            }

            // 패배 → 파티 전멸 체크
            return IsPartyWiped(party);
        }

        // ── 보스 노드 ──

        private static bool ProcessBossNode(GameRunState runState, FullRunResult result)
        {
            var party = new List<Character>(runState.PlayerParty);
            var bossData = GetBossData(runState.CurrentFloor);
            if (bossData == null) return false;

            var enemies = new List<Character> { new Character(bossData) };
            foreach (var e in enemies) e.ApplyFloorScaling(runState.GetFloorScaling());

            result.TotalBattles++;

            int bonusAP = runState.ConsumeBonusAP();
            var combatResult = RunSingleRunWithState(party, enemies, MapNodeType.Boss, runState.CurrentFloor, bonusAP);
            result.TotalTurns += combatResult.TurnCount;

            CleanupAfterCombat(party);

            if (combatResult.Victory)
            {
                runState.OnBattleVictory();
                // 보스 보상
                var rewardMgr = new RewardManager();
                var rewards = rewardMgr.GenerateRewards(MapNodeType.Boss, runState);
                if (rewards != null && rewards.Count > 0)
                {
                    var chosen = ChooseBestReward(rewards);
                    if (chosen != null) rewardMgr.ApplyReward(chosen, runState);
                }
                // 다음 층 진입 (F3 보스면 런 클리어)
                runState.AdvanceToNextFloor();
                return false;
            }

            return IsPartyWiped(party);
        }

        private static bool IsPartyWiped(IReadOnlyList<Character> party)
        {
            if (party == null || party.Count == 0) return true;
            foreach (var p in party)
                if (p != null && p.IsAlive) return false;
            return true;
        }

        // ── 상점 노드 ──

        private static void ProcessShopNode(GameRunState runState)
        {
            var shopMgr = new ShopManager();
            var slots = shopMgr.GenerateShopSlots(runState.CurrentFloor, _augmentPool, _relicPool);
            if (slots == null || slots.Count == 0) return;

            int budget = Mathf.RoundToInt(runState.Gold * 0.7f);

            // 유물 우선 구매 (증강은 복잡도 제외)
            foreach (var slot in slots)
            {
                if (slot.IsSold) continue;
                if (slot.ContentType != ShopSlot.SlotContentType.Relic) continue;
                if (slot.Price > budget) continue;

                if (shopMgr.PurchaseItem(slot, runState))
                    budget -= slot.Price;
            }
        }

        // ── 이벤트 노드 ──

        private static void ProcessEventNode(GameRunState runState)
        {
            if (_eventPool == null || _eventPool.Count == 0) return;
            var eventData = _eventPool[Random.Range(0, _eventPool.Count)];
            if (eventData == null || eventData.Choices.Count == 0) return;

            var eventMgr = new EventManager();
            // 가장 무난한 옵션(첫 선택지) 가정
            eventMgr.ProcessChoice(eventData, 0, runState);
        }

        // ── 휴식 노드 ──

        private static void ProcessRestNode(GameRunState runState)
        {
            float avgHP = ComputeAvgRemainingHP(runState.PlayerParty as List<Character>);
            int floor = runState.CurrentFloor;

            if (avgHP < 0.5f)
                runState.RestAtCampfire(0.3f);
            else if (floor == 1)
                runState.TrainAtCampfire();
            else
                runState.MeditateAtCampfire();
        }

        // ═══════════════════════════════════════════
        // 보상 선택 — 휴리스틱
        // ═══════════════════════════════════════════

        private static RewardOffer ChooseBestReward(List<RewardOffer> rewards)
        {
            if (rewards == null || rewards.Count == 0) return null;

            // 우선순위: 유물 > T2+ 증강(저주 아님) > 일반 증강(저주 아님) > 골드 > 스킵(null)
            RewardOffer relic = null;
            RewardOffer augmentHigh = null;
            RewardOffer augmentLow = null;
            RewardOffer gold = null;

            foreach (var r in rewards)
            {
                if (r == null) continue;
                switch (r.Type)
                {
                    case RewardType.Relic:
                        if (relic == null) relic = r;
                        break;
                    case RewardType.AugmentOffer:
                        if (r.AugmentOfferData == null) break;
                        if (r.AugmentOfferData.IsCursed) break;
                        if (r.AugmentOfferData.Tier >= 2)
                        {
                            if (augmentHigh == null) augmentHigh = r;
                        }
                        else
                        {
                            if (augmentLow == null) augmentLow = r;
                        }
                        break;
                    case RewardType.Gold:
                        if (gold == null) gold = r;
                        break;
                }
            }

            if (relic != null) return relic;
            if (augmentHigh != null) return augmentHigh;
            if (augmentLow != null) return augmentLow;
            if (gold != null) return gold;
            return null;
        }

        // ═══════════════════════════════════════════
        // 헬퍼
        // ═══════════════════════════════════════════

        /// <summary>GameRunState 컨텍스트(유물/AP 보너스)를 반영한 단일 전투.</summary>
        private static CombatResult RunSingleRunWithState(
            List<Character> party, List<Character> enemies,
            MapNodeType battleType, int floor, int bonusFirstTurnAP)
        {
            ClearCombatEventBus();

            var enemyControllers = new List<EnemyAIController>(enemies.Count);
            foreach (var enemy in enemies)
            {
                var pattern = CreatePatternFor(enemy);
                var controller = new EnemyAIController(enemy, pattern, party);
                enemyControllers.Add(controller);
            }

            var turnManager = new TurnManager(party, enemies, enemyControllers, maxRerolls: 1, bonusFirstTurnAP: bonusFirstTurnAP);

            // 시뮬레이터용 AI — partial Combat.cs의 SimulatedPlayerAI
            var ai = new SimulatedPlayerAI(turnManager, party, enemies, enemyControllers);

            turnManager.StartBattle();
            foreach (var c in enemyControllers)
                if (c.Owner.IsAlive) c.PrepareNextAction();

            int turns = 0;
            while (turnManager.CurrentPhase != TurnPhase.BattleEnd && turns < MaxTurnsPerCombat)
            {
                turns++;
                ai.PlayTurn(turns);

                if (turnManager.CurrentPhase == TurnPhase.BattleEnd)
                    break;

                foreach (var c in enemyControllers)
                    if (c.Owner.IsAlive) c.PrepareNextAction();
            }

            bool victory = !party.TrueForAll(p => p.IsDead) && enemies.TrueForAll(e => e.IsDead);
            if (turns >= MaxTurnsPerCombat && turnManager.CurrentPhase != TurnPhase.BattleEnd)
                victory = false;

            ClearCombatEventBus();

            return new CombatResult
            {
                ScenarioName = $"Run_F{floor}_{battleType}",
                Category = $"F{floor}_{battleType}",
                Floor = floor,
                EnemyName = DescribeEnemies(enemies),
                Victory = victory,
                TurnCount = turns,
                AvgRemainingHP = ComputeAvgRemainingHP(party),
                Survivors = party.FindAll(p => p.IsAlive).Count
            };
        }

        private static void CleanupAfterCombat(IReadOnlyList<Character> party)
        {
            if (party == null) return;
            foreach (var member in party)
            {
                if (member == null || !member.IsAlive) continue;
                member.StatusEffects?.ClearAllEffects();
                member.Health?.ResetShield();
                member.ApplyStatModifiers();
            }
        }

        private static int CountSurvivors(IReadOnlyList<Character> party)
        {
            if (party == null) return 0;
            int count = 0;
            foreach (var p in party)
                if (p != null && p.IsAlive) count++;
            return count;
        }
    }
}
#endif
