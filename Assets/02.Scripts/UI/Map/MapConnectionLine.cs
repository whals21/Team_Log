using UnityEngine;
using UnityEngine.UI;
using TeamLog.Map;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 노드 간 연결선 UI
    /// </summary>
    public class MapConnectionLine : MonoBehaviour
    {
        [SerializeField] private Image _lineImage;
        [SerializeField] private Color _normalColor = new Color(0.5f, 0.5f, 0.6f, 0.5f);
        [SerializeField] private Color _activeColor = new Color(0.8f, 0.8f, 0.3f, 0.8f);
        [SerializeField] private Color _visitedColor = new Color(0.3f, 0.3f, 0.35f, 0.3f);

        private MapNode _fromNode;
        private MapNode _toNode;

        public void Setup(MapNode fromNode, MapNode toNode)
        {
            _fromNode = fromNode;
            _toNode = toNode;
            UpdateVisual();
        }

        public void UpdateVisual()
        {
            if (_lineImage == null) return;

            // 연결선 색상: 출발 노드가 방문되었고 목적지가 활성이면 activeColor
            bool isActivePath = _fromNode != null && _fromNode.IsVisited &&
                                _toNode != null && _toNode.IsActive;
            bool isVisitedPath = _fromNode != null && _fromNode.IsVisited &&
                                 _toNode != null && _toNode.IsVisited;

            _lineImage.color = isActivePath ? _activeColor :
                               isVisitedPath ? _visitedColor : _normalColor;
        }

        /// <summary>
        /// 두 RectTransform 사이에 선 그리기
        /// </summary>
        public void DrawLine(RectTransform fromRect, RectTransform toRect, RectTransform container)
        {
            if (fromRect == null || toRect == null) return;

            var lineRect = GetComponent<RectTransform>();
            if (lineRect == null) return;

            // 시작/끝 위치 계산
            Vector2 fromPos = GetLocalPosition(fromRect, container);
            Vector2 toPos = GetLocalPosition(toRect, container);

            // 선 위치 및 크기 설정
            Vector2 direction = toPos - fromPos;
            float distance = direction.magnitude;

            lineRect.sizeDelta = new Vector2(distance, 3f);
            lineRect.pivot = new Vector2(0f, 0.5f);
            lineRect.anchoredPosition = fromPos;

            // 회전
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            lineRect.localRotation = Quaternion.Euler(0, 0, angle);
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
