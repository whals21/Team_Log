using System.Collections.Generic;
using UnityEngine;

namespace TeamLog.Map
{
    /// <summary>
    /// 맵 생성 설정
    /// </summary>
    public class MapGenerationConfig
    {
        public int LayerCount { get; set; } = 6;
        public int MinNodesPerLayer { get; set; } = 2;
        public int MaxNodesPerLayer { get; set; } = 4;
        public int MaxConnectionsPerNode { get; set; } = 3;
        public int EliteCount { get; set; } = 0;
        public int ShopCount { get; set; } = 1;
        public int RestCount { get; set; } = 1;
        public int EventCount { get; set; } = 1;
        /// <summary>
        /// 분기 레이어 인덱스 목록 — 이 레이어에는 Battle+Elite 노드 쌍이 생성됨.
        /// 엘리트 풀이 비어 있으면 일반 Battle 노드로 폴백.
        /// </summary>
        public HashSet<int> BranchingLayers { get; set; } = new HashSet<int>();
    }

    /// <summary>
    /// 층별 기본 설정 — 4스테이지 확장 (StageDesign.md 기준)
    /// </summary>
    public static class FloorConfigs
    {
        public static MapGenerationConfig GetConfig(int floorNumber)
        {
            // 4스테이지 모두 동일한 6레이어 구조 (Start + 4 전투 + Boss)
            // 분기 레이어: 2, 4 (각각 Battle/Elite 선택지)
            var branching = new HashSet<int> { 2, 4 };
            return floorNumber switch
            {
                1 => new MapGenerationConfig
                {
                    LayerCount = 6,
                    MinNodesPerLayer = 2,
                    MaxNodesPerLayer = 3,
                    EliteCount = 0, // 엘리트는 분기 레이어에서만
                    ShopCount = 1,
                    RestCount = 1,
                    EventCount = 1,
                    BranchingLayers = branching
                },
                2 => new MapGenerationConfig
                {
                    LayerCount = 6,
                    MinNodesPerLayer = 2,
                    MaxNodesPerLayer = 3,
                    EliteCount = 0,
                    ShopCount = 1,
                    RestCount = 1,
                    EventCount = 1,
                    BranchingLayers = branching
                },
                3 => new MapGenerationConfig
                {
                    LayerCount = 6,
                    MinNodesPerLayer = 2,
                    MaxNodesPerLayer = 4,
                    EliteCount = 0,
                    ShopCount = 1,
                    RestCount = 1,
                    EventCount = 2,
                    BranchingLayers = branching
                },
                4 => new MapGenerationConfig
                {
                    LayerCount = 6,
                    MinNodesPerLayer = 2,
                    MaxNodesPerLayer = 4,
                    EliteCount = 0,
                    ShopCount = 1,
                    RestCount = 1,
                    EventCount = 2,
                    BranchingLayers = branching
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
                else
                {
                    // 중간 단계: 2~4개 노드
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
        /// 특수 노드 타입 배정 (Shop, Rest, Event) — 분기 레이어는 스킵
        /// </summary>
        private void AssignSpecialNodeTypes(List<List<MapNode>> nodesByLayer, MapGenerationConfig config)
        {
            // 배정 가능한 중간 노드 수집 (Start, Boss, 분기 레이어 제외)
            var candidates = new List<MapNode>();
            for (int layer = 1; layer < nodesByLayer.Count - 1; layer++)
            {
                if (config.BranchingLayers.Contains(layer)) continue;
                candidates.AddRange(nodesByLayer[layer]);
            }

            Shuffle(candidates);

            int assigned = 0;

            // 엘리트 배정 (분기 외에 추가 엘리트가 있는 경우만 — 현재는 0)
            for (int i = 0; i < config.EliteCount && assigned < candidates.Count; i++, assigned++)
            {
                OverrideNodeType(candidates[assigned], MapNodeType.Elite);
            }

            // 상점 배정
            for (int i = 0; i < config.ShopCount && assigned < candidates.Count; i++, assigned++)
            {
                OverrideNodeType(candidates[assigned], MapNodeType.Shop);
            }

            // 휴식 배정
            for (int i = 0; i < config.RestCount && assigned < candidates.Count; i++, assigned++)
            {
                OverrideNodeType(candidates[assigned], MapNodeType.Rest);
            }

            // 이벤트 배정
            for (int i = 0; i < config.EventCount && assigned < candidates.Count; i++, assigned++)
            {
                OverrideNodeType(candidates[assigned], MapNodeType.Event);
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
