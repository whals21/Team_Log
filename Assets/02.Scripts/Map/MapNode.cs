using System.Collections.Generic;

namespace TeamLog.Map
{
    /// <summary>
    /// 맵 상의 단일 노드 — 순수 C# 클래스
    /// </summary>
    public class MapNode
    {
        private readonly List<MapNode> _connections = new();

        // 기본 정보
        public MapNodeType NodeType { get; set; }
        public int Layer { get; }       // 소속 단계 (0부터 시작)
        public int Index { get; }       // 단계 내 인덱스

        // 상태
        public bool IsVisited { get; private set; }
        public bool IsActive { get; private set; }

        // 연결 정보
        public IReadOnlyList<MapNode> Connections => _connections;

        // 노드별 추가 데이터 (전투용 적 캐릭터 데이터 참조 등)
        public object NodeData { get; set; }

        public MapNode(MapNodeType nodeType, int layer, int index)
        {
            NodeType = nodeType;
            Layer = layer;
            Index = index;
            IsActive = false;
            IsVisited = false;
        }

        public void AddConnection(MapNode target)
        {
            if (!_connections.Contains(target))
                _connections.Add(target);
        }

        public void SetActive(bool active)
        {
            IsActive = active;
        }

        public void MarkVisited()
        {
            IsVisited = true;
            IsActive = false;
        }

        public void Reset()
        {
            IsVisited = false;
            IsActive = false;
            NodeData = null;
        }

        public override string ToString()
        {
            return $"[{NodeType}] Layer {Layer} Index {Index}";
        }
    }
}
