using UnityEngine;
using UnityEngine.UI;
using TeamLog.Map;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 노드 간 연결선 UI — CubicBezierLine 사용
    /// </summary>
    public class MapConnectionLine : MonoBehaviour
    {
        [SerializeField] private CubicBezierLine _bezierLine;
        [SerializeField] private Color _normalColor = new Color(0.5f, 0.5f, 0.6f, 0.5f);
        [SerializeField] private Color _activeColor = new Color(0.8f, 0.8f, 0.3f, 0.8f);
        [SerializeField] private Color _visitedColor = new Color(0.3f, 0.3f, 0.35f, 0.3f);

        private MapNode _fromNode;
        private MapNode _toNode;

        private void Awake()
        {
            if (_bezierLine == null)
                _bezierLine = GetComponent<CubicBezierLine>();
        }

        public void Setup(MapNode fromNode, MapNode toNode)
        {
            _fromNode = fromNode;
            _toNode = toNode;
            UpdateVisual();
        }

        public void UpdateVisual()
        {
            bool isActivePath = _fromNode != null && _fromNode.IsVisited &&
                                _toNode != null && _toNode.IsActive;
            bool isVisitedPath = _fromNode != null && _fromNode.IsVisited &&
                                 _toNode != null && _toNode.IsVisited;

            Color color = isActivePath ? _activeColor :
                          isVisitedPath ? _visitedColor : _normalColor;

            if (_bezierLine != null)
            {
                // BezierLine 색상 업데이트는 SetPoints에서 함께 처리
                // 단순 색상만 변경하는 경우: 이미 그려진 선의 색상 갱신
                _bezierLine.color = color;
            }
        }

        /// <summary>
        /// 두 RectTransform 사이에 베지에 곡선 그리기
        /// </summary>
        public void DrawLine(RectTransform fromRect, RectTransform toRect, RectTransform container)
        {
            if (fromRect == null || toRect == null) return;
            if (_bezierLine == null) return;

            var lineRect = GetComponent<RectTransform>();
            if (lineRect == null) return;

            // 컨테이너 전체 영역으로 설정
            lineRect.anchorMin = Vector2.zero;
            lineRect.anchorMax = Vector2.one;
            lineRect.offsetMin = Vector2.zero;
            lineRect.offsetMax = Vector2.zero;
            lineRect.pivot = new Vector2(0.5f, 0.5f);
            lineRect.anchoredPosition = Vector2.zero;

            // 시작/끝 로컬 위치 계산
            Vector2 fromPos = GetLocalPosition(fromRect, container);
            Vector2 toPos = GetLocalPosition(toRect, container);

            bool isActivePath = _fromNode != null && _fromNode.IsVisited &&
                                _toNode != null && _toNode.IsActive;
            bool isVisitedPath = _fromNode != null && _fromNode.IsVisited &&
                                 _toNode != null && _toNode.IsVisited;

            Color color = isActivePath ? _activeColor :
                          isVisitedPath ? _visitedColor : _normalColor;

            _bezierLine.SetPoints(fromPos, toPos, color);
        }

        private Vector2 GetLocalPosition(RectTransform target, RectTransform container)
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                container,
                RectTransformUtility.WorldToScreenPoint(null, target.position),
                null,
                out localPos);
            return localPos;
        }
    }
}
