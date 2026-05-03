using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Map;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 맵 전체 뷰 — 노드 배치, 연결선, 플레이어 마커 관리
    /// </summary>
    public class MapView : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private RectTransform _nodeContainer;
        [SerializeField] private RectTransform _lineContainer;
        [SerializeField] private float _layerSpacing = 180f;
        [SerializeField] private float _nodeSpacing = 120f;

        [Header("Prefabs")]
        [SerializeField] private GameObject _nodeButtonPrefab;
        [SerializeField] private GameObject _connectionLinePrefab;
        [SerializeField] private GameObject _playerMarkerPrefab;

        [Header("Info Panel")]
        [SerializeField] private TextMeshProUGUI _floorLabel;
        [SerializeField] private TextMeshProUGUI _goldLabel;

        private MapFloor _currentMap;
        private MapPlayerMarker _playerMarker;
        private readonly List<MapNodeButton> _nodeButtons = new();
        private readonly List<MapConnectionLine> _connectionLines = new();
        private System.Action<MapNode> _onNodeClicked;

        /// <summary>
        /// 맵 데이터 바인딩 및 시각화
        /// </summary>
        public void Initialize(MapFloor mapFloor, int gold, System.Action<MapNode> onNodeClicked)
        {
            _currentMap = mapFloor;
            _onNodeClicked = onNodeClicked;

            ClearAll();
            CreateNodeButtons();
            CreateConnectionLines();
            CreatePlayerMarker();

            UpdateInfoPanel(mapFloor.FloorNumber, gold);
            UpdatePlayerPosition();
        }

        /// <summary>
        /// 플레이어 이동 후 UI 업데이트
        /// </summary>
        public void Refresh(int gold)
        {
            foreach (var button in _nodeButtons)
                button.UpdateVisuals();

            foreach (var line in _connectionLines)
                line.UpdateVisual();

            UpdatePlayerPosition();
            UpdateInfoPanel(_currentMap.FloorNumber, gold);
        }

        private void CreateNodeButtons()
        {
            if (_nodeContainer == null || _nodeButtonPrefab == null) return;

            for (int layer = 0; layer < _currentMap.Layers.Count; layer++)
            {
                var layerNodes = _currentMap.Layers[layer];
                int nodeCount = layerNodes.Count;
                float totalWidth = (nodeCount - 1) * _nodeSpacing;
                float startX = -totalWidth / 2f;

                for (int i = 0; i < nodeCount; i++)
                {
                    var nodeObj = Instantiate(_nodeButtonPrefab, _nodeContainer);
                    var rectTransform = nodeObj.GetComponent<RectTransform>();

                    if (rectTransform != null)
                    {
                        float x = startX + i * _nodeSpacing;
                        float y = -layer * _layerSpacing;
                        rectTransform.anchoredPosition = new Vector2(x, y);
                    }

                    var nodeButton = nodeObj.GetComponent<MapNodeButton>();
                    if (nodeButton != null)
                    {
                        nodeButton.Setup(layerNodes[i], OnNodeButtonClicked);
                        _nodeButtons.Add(nodeButton);
                    }
                }
            }
        }

        private void CreateConnectionLines()
        {
            if (_lineContainer == null || _connectionLinePrefab == null) return;

            // 모든 노드의 연결에 대해 선 생성
            var allNodes = new List<MapNode>();
            foreach (var layer in _currentMap.Layers)
                allNodes.AddRange(layer);

            foreach (var node in allNodes)
            {
                var fromButton = FindNodeButton(node);
                if (fromButton == null) continue;

                foreach (var connected in node.Connections)
                {
                    var toButton = FindNodeButton(connected);
                    if (toButton == null) continue;

                    var lineObj = Instantiate(_connectionLinePrefab, _lineContainer);
                    var line = lineObj.GetComponent<MapConnectionLine>();

                    if (line != null)
                    {
                        line.Setup(node, connected);
                        line.DrawLine(
                            fromButton.GetComponent<RectTransform>(),
                            toButton.GetComponent<RectTransform>(),
                            _lineContainer);
                        _connectionLines.Add(line);
                    }
                }
            }
        }

        private void CreatePlayerMarker()
        {
            if (_playerMarkerPrefab == null || _nodeContainer == null) return;

            var markerObj = Instantiate(_playerMarkerPrefab, _nodeContainer);
            _playerMarker = markerObj.GetComponent<MapPlayerMarker>();
        }

        private void UpdatePlayerPosition()
        {
            if (_playerMarker == null || _currentMap.CurrentNode == null) return;

            var currentNodeButton = FindNodeButton(_currentMap.CurrentNode);
            if (currentNodeButton != null)
                _playerMarker.MoveToNode(currentNodeButton);
        }

        private void UpdateInfoPanel(int floorNumber, int gold)
        {
            if (_floorLabel != null)
                _floorLabel.text = $"층 {floorNumber}";
            if (_goldLabel != null)
                _goldLabel.text = $"{gold} G";
        }

        private MapNodeButton FindNodeButton(MapNode node)
        {
            foreach (var button in _nodeButtons)
            {
                if (button != null && ReferenceEquals(GetNodeFromButton(button), node))
                    return button;
            }
            return null;
        }

        private MapNode GetNodeFromButton(MapNodeButton button)
        {
            // MapNodeButton의 private _node에 접근할 수 없으므로
            // Setup에서 전달한 노드를 다시 찾는 방식 필요
            // 간단한 해결: 버튼에 노드 참조 저장
            var nodeRef = button.GetComponent<MapNodeReference>();
            return nodeRef != null ? nodeRef.Node : null;
        }

        private void OnNodeButtonClicked(MapNode node)
        {
            _onNodeClicked?.Invoke(node);
        }

        private void ClearAll()
        {
            foreach (var button in _nodeButtons)
            {
                if (button != null) Destroy(button.gameObject);
            }
            _nodeButtons.Clear();

            foreach (var line in _connectionLines)
            {
                if (line != null) Destroy(line.gameObject);
            }
            _connectionLines.Clear();

            if (_playerMarker != null)
            {
                Destroy(_playerMarker.gameObject);
                _playerMarker = null;
            }
        }

        private void OnDestroy()
        {
            ClearAll();
        }
    }
}
