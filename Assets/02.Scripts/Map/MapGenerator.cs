using System.Collections.Generic;
using UnityEngine;

namespace TeamLog.Map
{
    /// <summary>
    /// 맵 생성 설정
    /// </summary>
    public class MapGenerationConfig
    {
        public int LayerCount { get; set; } = 9;
        public int MinNodesPerLayer { get; set; } = 2;
        public int MaxNodesPerLayer { get; set; } = 4;
        public int MaxConnectionsPerNode { get; set; } = 3;
        public int EliteCount { get; set; } = 0;
        public int ShopCount { get; set; } = 1;
        public int RestCount { get; set; } = 1;
        public int EventCount { get; set; } = 2;
        /// <summary>
        /// 분기 레이어 인덱스 목록 — 이 레이어에는 Battle+Elite 노드 쌍이 생성됨.
        /// 엘리트 풀이 비어 있으면 일반 Battle 노드로 폴백.
        /// </summary>
        public HashSet<int> BranchingLayers { get; set; } = new HashSet<int>();

        /// <summary>
        /// ★ 비전투 우선 레이어 (Phase 9-Layer Rework).
        /// 전투 사이마다 이벤트/상점/휴식이 들어가도록 보장.
        /// 이 레이어의 노드는 일반 Battle로 시작하지 않고, AssignSpecialNodeTypes가
        /// Event/Shop/Rest 중 하나로 배정 (부족하면 Battle으로 폴백).
        /// 기본: 분기 레이어 사이사이 (레이어 2, 4, 6).
        /// </summary>
        public HashSet<int> NonCombatLayers { get; set; } = new HashSet<int>();
    }

    /// <summary>
    /// 층별 기본 설정 — 4스테이지 9레이어 표준 (Start + 4전투 + 보스 + 비전투 3)
    ///
    /// ★ 9레이어 표준 구조 (2026-07-19 Rework):
    /// <code>
    ///   L0  Start
    ///   L1  Battle #1
    ///   L2  Event / Shop / Rest   (비전투 — 전투 사이 #1)
    ///   L3  Battle #2 [분기: Battle or Elite]
    ///   L4  Event / Shop / Rest   (비전투 — 전투 사이 #2)
    ///   L5  Battle #3
    ///   L6  Event / Shop / Rest   (비전투 — 전투 사이 #3)
    ///   L7  Battle #4 [분기: Battle or Elite]
    ///   L8  Boss
    /// </code>
    /// 사용자 요구사항: "한 플로어에 일반/엘리트 4번 + 보스 1번 + 전투 사이마다 이벤트"
    /// </summary>
    public static class FloorConfigs
    {
        public static MapGenerationConfig GetConfig(int floorNumber)
        {
            // 4스테이지 모두 동일한 9레이어 구조
            // 분기 레이어: 3, 7 (각각 Battle/Elite 선택지)
            // 비전투 레이어: 2, 4, 6 (전투 사이마다 Event/Shop/Rest)
            var branching = new HashSet<int> { 3, 7 };
            var nonCombat = new HashSet<int> { 2, 4, 6 };
            return floorNumber switch
            {
                1 => new MapGenerationConfig
                {
                    LayerCount = 9,
                    MinNodesPerLayer = 2,
                    MaxNodesPerLayer = 3,
                    EliteCount = 0,
                    ShopCount = 1,
                    RestCount = 1,
                    EventCount = 2, // 3개 비전투 레이어 중 2개는 Event, 1개는 Shop/Rest
                    BranchingLayers = branching,
                    NonCombatLayers = nonCombat
                },
                2 => new MapGenerationConfig
                {
                    LayerCount = 9,
                    MinNodesPerLayer = 2,
                    MaxNodesPerLayer = 3,
                    EliteCount = 0,
                    ShopCount = 1,
                    RestCount = 1,
                    EventCount = 2,
                    BranchingLayers = branching,
                    NonCombatLayers = nonCombat
                },
                3 => new MapGenerationConfig
                {
                    LayerCount = 9,
                    MinNodesPerLayer = 2,
                    MaxNodesPerLayer = 4,
                    EliteCount = 0,
                    ShopCount = 1,
                    RestCount = 1,
                    EventCount = 2,
                    BranchingLayers = branching,
                    NonCombatLayers = nonCombat
                },
                4 => new MapGenerationConfig
                {
                    LayerCount = 9,
                    MinNodesPerLayer = 2,
                    MaxNodesPerLayer = 4,
                    EliteCount = 0,
                    ShopCount = 1,
                    RestCount = 1,
                    EventCount = 2,
                    BranchingLayers = branching,
                    NonCombatLayers = nonCombat
                },
                _ => new MapGenerationConfig()
            };
        }
    }

    /// <summary>
    /// 프록시럴 맵 생성기 — 순수 C# 클래스
    /// 층별 맵을 규칙에 따라 자동 생성. StageThemeData로 분기 레이어 엘리트 가용성 체크.
    /// </summary>
    public class MapGenerator
    {
        private readonly System.Random _rng;

        public MapGenerator(int? seed = null)
        {
            _rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        /// <summary>
        /// 지정한 층 번호에 맞는 맵 생성 (테마 미지정 시 분기 레이어가 일반 Battle으로 폴백)
        /// </summary>
        public MapFloor GenerateFloor(int floorNumber)
        {
            var config = FloorConfigs.GetConfig(floorNumber);
            return GenerateFloor(floorNumber, config, null);
        }

        /// <summary>
        /// 테마 지정 — 분기 레이어에 엘리트 생성 여부 결정
        /// </summary>
        public MapFloor GenerateFloor(int floorNumber, StageThemeData theme)
        {
            var config = FloorConfigs.GetConfig(floorNumber);
            return GenerateFloor(floorNumber, config, theme);
        }

        /// <summary>
        /// 커스텀 설정으로 맵 생성
        /// </summary>
        public MapFloor GenerateFloor(int floorNumber, MapGenerationConfig config, StageThemeData theme = null)
        {
            var floor = new MapFloor(floorNumber);
            var nodesByLayer = new List<List<MapNode>>();

            bool hasElite = theme != null && theme.eliteEnemies != null && theme.eliteEnemies.Count > 0;

            // 1. 각 단계별 노드 생성
            for (int layer = 0; layer < config.LayerCount; layer++)
            {
                var layerNodes = floor.AddLayer();

                if (layer == 0)
                {
                    // 첫 단계: 시작 노드 1개
                    layerNodes.Add(new MapNode(MapNodeType.Start, layer, 0));
                }
                else if (layer == config.LayerCount - 1)
                {
                    // 마지막 단계: 보스 노드 1개
                    layerNodes.Add(new MapNode(MapNodeType.Boss, layer, 0));
                }
                else if (config.BranchingLayers.Contains(layer) && hasElite)
                {
                    // 분기 레이어: Battle 1개 + Elite 1개 (플레이어가 선택)
                    layerNodes.Add(new MapNode(MapNodeType.Battle, layer, 0));
                    layerNodes.Add(new MapNode(MapNodeType.Elite, layer, 1));
                }
                else if (config.NonCombatLayers.Contains(layer))
                {
                    // ★ 비전투 우선 레이어 — 단일 노드로 시작 (타입은 AssignSpecialNodeTypes가 배정).
                    // Shop/Rest/Event 중 하나로 변환되며, 정원 초과 시 Battle으로 폴백.
                    layerNodes.Add(new MapNode(MapNodeType.Battle, layer, 0));
                }
                else
                {
                    // 일반 전투 레이어: 2~4개 노드 (Battle)
                    int nodeCount = _rng.Next(config.MinNodesPerLayer, config.MaxNodesPerLayer + 1);
                    for (int i = 0; i < nodeCount; i++)
                        layerNodes.Add(new MapNode(MapNodeType.Battle, layer, i));
                }

                nodesByLayer.Add(layerNodes);
            }

            // 2. 단계 간 연결 생성
            for (int layer = 0; layer < nodesByLayer.Count - 1; layer++)
            {
                ConnectLayers(nodesByLayer[layer], nodesByLayer[layer + 1], config.MaxConnectionsPerNode);
            }

            // 3. 중간 노드 타입 배정 (분기 레이어는 이미 배정됨)
            AssignSpecialNodeTypes(nodesByLayer, config);

            return floor;
        }

        /// <summary>
        /// 두 단계 간 연결 생성 — 상향 분기 구조
        /// </summary>
        private void ConnectLayers(List<MapNode> lower, List<MapNode> upper, int maxConnections)
        {
            // 1. 모든 상위 노드가 최소 1개 하위 노드에서 도달 가능하도록 보장
            foreach (var upperNode in upper)
            {
                int lowerIndex = _rng.Next(0, lower.Count);
                lower[lowerIndex].AddConnection(upperNode);
            }

            // 2. 모든 하위 노드가 최소 1개 상위 노드로 연결되도록 보장 (막힘 방지)
            foreach (var lowerNode in lower)
            {
                if (lowerNode.Connections.Count == 0)
                {
                    var target = upper[_rng.Next(0, upper.Count)];
                    lowerNode.AddConnection(target);
                }
            }

            // 3. 하위 노드에 추가 연결 분배
            foreach (var lowerNode in lower)
            {
                int existingConnections = lowerNode.Connections.Count;
                int additionalConnections = _rng.Next(0, Mathf.Min(maxConnections - existingConnections, upper.Count) + 1);

                for (int i = 0; i < additionalConnections; i++)
                {
                    var target = upper[_rng.Next(0, upper.Count)];
                    lowerNode.AddConnection(target);
                }
            }
        }

        /// <summary>
        /// 특수 노드 타입 배정 (Shop, Rest, Event).
        ///
        /// ★ 9레이어 Rework (2026-07-19):
        ///   1. NonCombatLayers 노드를 최우선으로 Event/Shop/Rest 배정
        ///      (전투 사이마다 비전투 노드 보장)
        ///   2. 정원 초과 시 폴백 — 남은 비전투 레이어 노드는 Battle 유지
        ///   3. 분기 레이어 (BranchingLayers)는 항상 스킵 — Battle+Elite 쌍 유지
        /// </summary>
        private void AssignSpecialNodeTypes(List<List<MapNode>> nodesByLayer, MapGenerationConfig config)
        {
            // 1순위 후보: NonCombatLayers에 있는 노드들 (비전투 보장 영역)
            var nonCombatCandidates = new List<MapNode>();
            var fallbackCandidates = new List<MapNode>();

            for (int layer = 1; layer < nodesByLayer.Count - 1; layer++)
            {
                if (config.BranchingLayers.Contains(layer)) continue;

                if (config.NonCombatLayers.Contains(layer))
                {
                    // 비전투 우선 영역 — Event/Shop/Rest 배정 대상
                    foreach (var n in nodesByLayer[layer])
                        nonCombatCandidates.Add(n);
                }
                else
                {
                    // 일반 전투 레이어 — 정원 초과 시에만 후보 (현재 config에서는 EventCount=2,
                    // ShopCount=1, RestCount=1 = 총 4개 비전투 → 3개 비전투 레이어에 부족분 발생 가능)
                    foreach (var n in nodesByLayer[layer])
                        fallbackCandidates.Add(n);
                }
            }

            Shuffle(nonCombatCandidates);
            Shuffle(fallbackCandidates);

            // 비전투 타입별 요청 큐 (우선순위: Shop > Rest > Event — 희소성 역순)
            var requestQueue = new List<MapNodeType>();
            for (int i = 0; i < config.ShopCount; i++) requestQueue.Add(MapNodeType.Shop);
            for (int i = 0; i < config.RestCount; i++) requestQueue.Add(MapNodeType.Rest);
            for (int i = 0; i < config.EventCount; i++) requestQueue.Add(MapNodeType.Event);
            // EliteCount (분기 외 추가 엘리트) — 일반적으로 0이지만 호환성 유지
            for (int i = 0; i < config.EliteCount; i++) requestQueue.Add(MapNodeType.Elite);

            // 1단계: 비전투 레이어 우선 배정
            int ncIdx = 0;
            var assigned = new HashSet<MapNode>();
            foreach (var type in requestQueue)
            {
                while (ncIdx < nonCombatCandidates.Count && assigned.Contains(nonCombatCandidates[ncIdx]))
                    ncIdx++;
                if (ncIdx >= nonCombatCandidates.Count) break;

                var target = nonCombatCandidates[ncIdx];
                OverrideNodeType(target, type);
                assigned.Add(target);
                ncIdx++;
            }

            // 2단계: 남은 요청은 일반 전투 레이어에서 폴백 배정
            int remaining = requestQueue.Count - assigned.Count;
            int fbIdx = 0;
            for (int i = 0; i < remaining; i++)
            {
                while (fbIdx < fallbackCandidates.Count && assigned.Contains(fallbackCandidates[fbIdx]))
                    fbIdx++;
                if (fbIdx >= fallbackCandidates.Count) break;

                // 요청에서 아직 배정 안 된 타입 찾기
                var type = requestQueue[assigned.Count]; // 단순화 — 순서대로
                var target = fallbackCandidates[fbIdx];
                OverrideNodeType(target, type);
                assigned.Add(target);
                fbIdx++;
            }

            // ★ 비전투 레이어 중 배정 못 받은 노드는 자동으로 Event로 폴백
            // (Battle으로 남기면 비전투 보장이 깨짐)
            foreach (var node in nonCombatCandidates)
            {
                if (!assigned.Contains(node))
                {
                    OverrideNodeType(node, MapNodeType.Event);
                    assigned.Add(node);
                }
            }
        }

        private void OverrideNodeType(MapNode node, MapNodeType newType)
        {
            node.NodeType = newType;
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
