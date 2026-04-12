using System.Collections.Generic;
using System.Linq;

namespace TeamLog.Map
{
    /// <summary>
    /// 단일 층(Floor)의 맵 — 순수 C# 클래스
    /// 단계(Layer)별 노드 목록과 플레이어 위치를 관리
    /// </summary>
    public class MapFloor
    {
        private readonly List<List<MapNode>> _layers = new();
        private MapNode _currentNode;

        public int FloorNumber { get; }
        public IReadOnlyList<IReadOnlyList<MapNode>> Layers => _layers;
        public MapNode CurrentNode => _currentNode;
        public bool IsCleared { get; private set; }

        public event System.Action<MapNode> OnPlayerMoved;
        public event System.Action OnFloorCleared;

        public MapFloor(int floorNumber)
        {
            FloorNumber = floorNumber;
        }

        /// <summary>
        /// 층에 단계를 추가하고 해당 단계의 노드 목록 반환
        /// </summary>
        public List<MapNode> AddLayer()
        {
            var layer = new List<MapNode>();
            _layers.Add(layer);
            return layer;
        }

        /// <summary>
        /// 층의 시작 노드에서 플레이어 시작
        /// </summary>
        public void StartFloor()
        {
            if (_layers.Count == 0 || _layers[0].Count == 0) return;

            _currentNode = _layers[0][0];
            _currentNode.MarkVisited();

            // 첫 번째 연결 노드들을 활성화
            ActivateConnectedNodes(_currentNode);
        }

        /// <summary>
        /// 지정한 노드로 플레이어 이동
        /// </summary>
        public bool MoveToNode(MapNode target)
        {
            if (target == null || !target.IsActive) return false;
            if (_currentNode != null && !_currentNode.Connections.Contains(target)) return false;

            _currentNode = target;
            _currentNode.MarkVisited();

            OnPlayerMoved?.Invoke(_currentNode);

            // 보스 노드 도달 시 층 클리어 확인
            if (_currentNode.NodeType == MapNodeType.Boss)
            {
                IsCleared = true;
                OnFloorCleared?.Invoke();
                return true;
            }

            ActivateConnectedNodes(_currentNode);
            return true;
        }

        /// <summary>
        /// 현재 노드에서 이동 가능한 다음 노드 목록 반환
        /// </summary>
        public IReadOnlyList<MapNode> GetAvailableNodes()
        {
            if (_currentNode == null) return new List<MapNode>();
            return _currentNode.Connections;
        }

        /// <summary>
        /// 모든 노드 초기화
        /// </summary>
        public void Reset()
        {
            foreach (var layer in _layers)
            {
                foreach (var node in layer)
                    node.Reset();
            }
            _currentNode = null;
            IsCleared = false;
        }

        private void ActivateConnectedNodes(MapNode node)
        {
            foreach (var connected in node.Connections)
            {
                if (!connected.IsVisited)
                    connected.SetActive(true);
            }
        }
    }
}
