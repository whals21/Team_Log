using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Map;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// 9 레이어 맵 뷰 — Rework 버전 (2026-07-19).
    ///
    /// 기존 MapView(Assets/02.Scripts/UI/Map/MapView.cs)와의 차이점:
    ///  - 9 레이어 표준 구조 (Start + 4전투 + 보스 + 비전투 3)
    ///  - 분기 레이어 (Battle/Elite 쌍)에 "CHOICE OF PATH" 라벨 자동 표시
    ///  - 비전투 레이어에 "— between battles —" 라벨 표시
    ///  - 노드 타입별 색상/아이콘 자동 매핑
    ///  - 노드 상태별 글로우/흐림 (visited/active/locked)
    ///  - 좌측 레이어 인덱스 표시 (1~9)
    /// </summary>
    public class MapReworkView : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private RectTransform _nodeContainer;
        [SerializeField] private RectTransform _labelContainer;     // 레이어 라벨용 (좌측)
        [SerializeField] private float _layerSpacing = 110f;        // ★ Priority 6: 70→110 (노드 56px + 여유)
        [SerializeField] private float _nodeSpacing = 130f;         // ★ 110→130
        [SerializeField] private float _branchNodeSpacing = 140f;   // ★ 90→140 (분기 노드 명확히 분리)
        [SerializeField] private float _singleNodeZigzag = 50f;     // ★ 단일 노드 지그재그 x 오프셋

        [Header("Prefabs")]
        [SerializeField] private GameObject _nodePrefab;             // ReworkedMapNode
        [SerializeField] private GameObject _branchLabelPrefab;      // "CHOICE OF PATH"
        [SerializeField] private GameObject _betweenLabelPrefab;     // "— between battles —"
        [SerializeField] private GameObject _playerMarkerPrefab;

        [Header("Sprites (MapSceneSpriteGenerator 출력)")]
        [SerializeField] private Sprite _iconStart;
        [SerializeField] private Sprite _iconBattle;
        [SerializeField] private Sprite _iconElite;
        [SerializeField] private Sprite _iconBoss;
        [SerializeField] private Sprite _iconEvent;
        [SerializeField] private Sprite _iconShop;
        [SerializeField] private Sprite _iconRest;
        [SerializeField] private Sprite _frameGlow;
        [SerializeField] private Sprite _playerMarkerSprite;

        private MapFloor _currentMap;
        private readonly List<MapReworkNode> _nodes = new();
        private GameObject _playerMarker;
        private System.Action<MapNode> _onNodeClicked;

        /// <summary>
        /// 맵 데이터 바인딩 및 시각화.
        /// </summary>
        public void Initialize(MapFloor mapFloor, System.Action<MapNode> onNodeClicked)
        {
            _currentMap = mapFloor;
            _onNodeClicked = onNodeClicked;

            // ★ Priority 7 (디버그 로그 — 런타임 노드 위치 진단)
            Debug.Log($"[MapReworkView] Initialize — LayerCount:{mapFloor?.Layers?.Count} " +
                      $"NodeContainer:{(_nodeContainer != null ? _nodeContainer.rect.ToString() : "NULL")} " +
                      $"NodeContainer.sizeDelta:{(_nodeContainer != null ? _nodeContainer.sizeDelta.ToString() : "NULL")}");

            ClearAll();
            CreateNodes();
            CreateConnectionLines();
            CreatePlayerMarker();
            UpdatePlayerPosition();

            // 노드 위치 로그 출력
            Debug.Log($"[MapReworkView] 노드 {(_nodes?.Count ?? 0)}개 생성 완료. 첫 3개 위치:");
            if (_nodes != null)
            {
                for (int i = 0; i < Mathf.Min(3, _nodes.Count); i++)
                {
                    var n = _nodes[i];
                    var rt = n?.GetComponent<RectTransform>();
                    if (n?.Node != null && rt != null)
                        Debug.Log($"  [{i}] Layer{n.Node.Layer} Type={n.Node.NodeType} pos={rt.anchoredPosition} anchorMin={rt.anchorMin} sizeDelta={rt.sizeDelta}");
                }
            }
        }

        /// <summary>
        /// 플레이어 이동 후 UI 업데이트.
        /// </summary>
        public void Refresh()
        {
            foreach (var node in _nodes)
            {
                if (node != null) node.UpdateVisuals();
            }
            UpdatePlayerPosition();
        }

        private void CreateNodes()
        {
            if (_nodeContainer == null || _nodePrefab == null) return;
            if (_currentMap?.Layers == null) return;

            int layerCount = _currentMap.Layers.Count;
            if (layerCount == 0) return;

            // ★ Priority 7 (치명 수정): containerHeight가 0이면 effectiveSpacing=0이 되어 모든 노드가 y=0에 겹침.
            // 3중 폴백: (1) LayoutRebuilder 강제 평가 (2) 부모/조상 rect에서 찾기 (3) 절대값 900f.
            float containerHeight = _nodeContainer.rect.height;
            if (containerHeight < 1f)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_nodeContainer);
                containerHeight = _nodeContainer.rect.height;
            }
            if (containerHeight < 1f)
            {
                // 조상 트리 순회하며 유효한 rect 찾기
                var ancestor = _nodeContainer.parent;
                while (ancestor != null && containerHeight < 1f)
                {
                    if (ancestor is RectTransform ancestorRt && ancestorRt.rect.height > 1f)
                    {
                        containerHeight = ancestorRt.rect.height;
                        break;
                    }
                    ancestor = ancestor.parent;
                }
            }
            if (containerHeight < 1f)
            {
                // 최종 폴백: Body height 추정값 (1920x1080 기준 헤더 52 + 푸터 40 제외)
                containerHeight = 900f;
                Debug.LogWarning($"[MapReworkView] containerHeight 최종 폴백 900f 적용 (조상 rect 모두 0)");
            }
            Debug.Log($"[MapReworkView] containerHeight resolved = {containerHeight} (spacing={_layerSpacing}/{_nodeSpacing}/{_branchNodeSpacing}/{_singleNodeZigzag})");

            float effectiveSpacing = _layerSpacing;
            float totalHeight = (layerCount - 1) * effectiveSpacing;
            // containerHeight 유효하면 축소
            if (containerHeight >= 1f && totalHeight > containerHeight * 0.9f)
            {
                effectiveSpacing = containerHeight * 0.9f / (layerCount - 1);
                totalHeight = (layerCount - 1) * effectiveSpacing;
            }

            // 캔버스 중앙 정렬 (상단이 layer 0)
            float startY = totalHeight / 2f;

            Debug.Log($"[MapReworkView] CreateNodes — layerCount:{layerCount} effectiveSpacing:{effectiveSpacing} startY:{startY}");

            for (int layer = 0; layer < layerCount; layer++)
            {
                var layerNodes = _currentMap.Layers[layer];
                if (layerNodes == null || layerNodes.Count == 0) continue;

                // 분기 레이어 — Battle+Elite 쌍
                bool isBranch = layerNodes.Count == 2 &&
                                layerNodes[0].NodeType != layerNodes[1].NodeType;
                float nodeGap = isBranch ? _branchNodeSpacing : _nodeSpacing;

                // 레이어 라벨
                CreateLayerLabelIfNeeded(layer, layerNodes, startY - layer * effectiveSpacing);

                int nodeCount = layerNodes.Count;
                float totalWidth = (nodeCount - 1) * nodeGap;
                float startX = -totalWidth / 2f;

                // ★ Priority 6: 단일 노드에 자연스러운 x 오프셋 (지그재그)
                float layerXOffset = 0f;
                if (nodeCount == 1)
                {
                    var singleType = layerNodes[0].NodeType;
                    if (singleType != MapNodeType.Start && singleType != MapNodeType.Boss)
                    {
                        layerXOffset = (layer % 2 == 1) ? -_singleNodeZigzag : _singleNodeZigzag;
                    }
                }

                for (int i = 0; i < nodeCount; i++)
                {
                    var nodeObj = Instantiate(_nodePrefab, _nodeContainer);
                    var rt = nodeObj.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);

                        float x = startX + i * nodeGap + layerXOffset;
                        float y = startY - layer * effectiveSpacing;
                        rt.anchoredPosition = new Vector2(x, y);
                    }

                    var reworkedNode = nodeObj.GetComponent<MapReworkNode>();
                    if (reworkedNode == null) reworkedNode = nodeObj.AddComponent<MapReworkNode>();

                    reworkedNode.Setup(layerNodes[i], GetSpriteForType(layerNodes[i].NodeType), OnNodeClicked);
                    _nodes.Add(reworkedNode);
                }
            }
        }

        /// <summary>
        /// 분기 레이어 / 비전투 레이어 라벨 생성.
        /// </summary>
        private void CreateLayerLabelIfNeeded(int layer, IReadOnlyList<MapNode> layerNodes, float y)
        {
            if (_labelContainer == null) return;
            if (layerNodes == null || layerNodes.Count == 0) return;

            // 분기 레이어 감지: Battle + Elite 조합
            bool isBranch = layerNodes.Count == 2;
            if (isBranch)
            {
                bool hasBattle = false, hasElite = false;
                foreach (var n in layerNodes)
                {
                    if (n.NodeType == MapNodeType.Battle) hasBattle = true;
                    if (n.NodeType == MapNodeType.Elite) hasElite = true;
                }
                isBranch = hasBattle && hasElite;
            }

            // 비전투 레이어 감지: 단일 노드가 Event/Shop/Rest
            bool isBetween = layerNodes.Count == 1 &&
                             (layerNodes[0].NodeType == MapNodeType.Event ||
                              layerNodes[0].NodeType == MapNodeType.Shop ||
                              layerNodes[0].NodeType == MapNodeType.Rest);

            GameObject prefab = null;
            if (isBranch && _branchLabelPrefab != null) prefab = _branchLabelPrefab;
            else if (isBetween && _betweenLabelPrefab != null) prefab = _betweenLabelPrefab;

            if (prefab == null) return;

            var labelObj = Instantiate(prefab, _labelContainer);
            var rt = labelObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(0f, y + 30f); // 노드 위에 라벨
            }
            labelObj.gameObject.SetActive(true);
        }

        private Sprite GetSpriteForType(MapNodeType type)
        {
            return type switch
            {
                MapNodeType.Start  => _iconStart,
                MapNodeType.Battle => _iconBattle,
                MapNodeType.Elite  => _iconElite,
                MapNodeType.Boss   => _iconBoss,
                MapNodeType.Event  => _iconEvent,
                MapNodeType.Shop   => _iconShop,
                MapNodeType.Rest   => _iconRest,
                _ => _iconBattle
            };
        }

        private void CreatePlayerMarker()
        {
            if (_playerMarkerPrefab == null || _nodeContainer == null) return;
            _playerMarker = Instantiate(_playerMarkerPrefab, _nodeContainer);

            // Sprite 적용
            var image = _playerMarker.GetComponent<Image>();
            if (image != null && _playerMarkerSprite != null)
            {
                image.sprite = _playerMarkerSprite;
                image.raycastTarget = false;
            }
        }

        private void UpdatePlayerPosition()
        {
            if (_playerMarker == null || _currentMap?.CurrentNode == null) return;

            // 현재 노드 위치로 마커 이동
            foreach (var node in _nodes)
            {
                if (node != null && ReferenceEquals(node.Node, _currentMap.CurrentNode))
                {
                    var nodeRt = node.GetComponent<RectTransform>();
                    var markerRt = _playerMarker.GetComponent<RectTransform>();
                    if (nodeRt != null && markerRt != null)
                    {
                        markerRt.anchoredPosition = nodeRt.anchoredPosition;
                    }
                    break;
                }
            }
        }

        private void OnNodeClicked(MapNode node)
        {
            _onNodeClicked?.Invoke(node);
        }

        /// <summary>
        /// ★ Priority 6 — 노드 간 연결선 생성. WebMockup의 점선 스타일 흉내.
        /// 간단한 직선 Image (색상 골드 어두운 + 반투명)로 노드 쌍 연결.
        /// </summary>
        private void CreateConnectionLines()
        {
            if (_nodeContainer == null || _currentMap?.Layers == null) return;

            // 노드 위치 맵 (MapNode → anchoredPosition)
            var nodePositions = new System.Collections.Generic.Dictionary<MapNode, Vector2>();
            foreach (var reworkedNode in _nodes)
            {
                if (reworkedNode?.Node == null) continue;
                var rt = reworkedNode.GetComponent<RectTransform>();
                if (rt != null) nodePositions[reworkedNode.Node] = rt.anchoredPosition;
            }

            // 양방향 중복 방지
            var drawn = new System.Collections.Generic.HashSet<long>();
            foreach (var reworkedNode in _nodes)
            {
                if (reworkedNode?.Node == null) continue;
                var fromNode = reworkedNode.Node;
                if (!nodePositions.TryGetValue(fromNode, out var fromPos)) continue;

                foreach (var toNode in fromNode.Connections)
                {
                    if (toNode == null) continue;
                    if (!nodePositions.TryGetValue(toNode, out var toPos)) continue;

                    // 해시 기반 중복 방지
                    int fromHash = fromNode.GetHashCode();
                    int toHash = toNode.GetHashCode();
                    long key = fromHash < toHash ? ((long)fromHash << 32 | (uint)toHash) : ((long)toHash << 32 | (uint)fromHash);
                    if (!drawn.Add(key)) continue;

                    CreateLine(fromPos, toPos);
                }
            }
        }

        /// <summary>
        /// 두 점을 잇는 직선 Image 생성. 노드 가장자리에서 시작/끝나도록 28px(노드 반지름) 안쪽으로 조정.
        /// </summary>
        private void CreateLine(Vector2 from, Vector2 to)
        {
            var lineGo = new GameObject("ConnectionLine", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            lineGo.transform.SetParent(_nodeContainer, false);
            lineGo.transform.SetAsFirstSibling(); // 노드보다 뒤로 렌더링

            Vector2 delta = to - from;
            float distance = delta.magnitude;
            if (distance < 1f) return; // 너무 가까우면 스킵

            Vector2 direction = delta / distance;
            // 노드 가장자리에서 시작/끝 (노드 반지름 28px 가정)
            float nodeRadius = 28f;
            Vector2 adjustedFrom = from + direction * nodeRadius;
            Vector2 adjustedTo = to - direction * nodeRadius;
            Vector2 adjustedDelta = adjustedTo - adjustedFrom;
            float adjustedDistance = adjustedDelta.magnitude;
            if (adjustedDistance < 1f) return;

            float angle = Mathf.Atan2(adjustedDelta.y, adjustedDelta.x) * Mathf.Rad2Deg;
            Vector2 midPoint = (adjustedFrom + adjustedTo) * 0.5f;

            var rt = lineGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = midPoint;
            rt.sizeDelta = new Vector2(adjustedDistance, 2f); // 두께 2px
            rt.localRotation = Quaternion.Euler(0, 0, angle);

            var img = lineGo.GetComponent<Image>();
            img.sprite = GetWhiteSprite();
            img.color = new Color(0.545f, 0.412f, 0.078f, 0.45f); // DFGoldD + 반투명
            img.raycastTarget = false;
        }

        /// <summary>
        /// 1x1 흰색 Sprite 캐시 (선 그리기용). Sprite.Create로 매번 만들면 가비니 증가하므로 캐싱.
        /// </summary>
        private static Sprite _whiteSprite;
        private static Sprite GetWhiteSprite()
        {
            if (_whiteSprite == null)
            {
                _whiteSprite = Sprite.Create(Texture2D.whiteTexture,
                    new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 100f);
            }
            return _whiteSprite;
        }

        private void ClearAll()
        {
            foreach (var node in _nodes)
            {
                if (node != null) Destroy(node.gameObject);
            }
            _nodes.Clear();

            if (_playerMarker != null)
            {
                Destroy(_playerMarker);
                _playerMarker = null;
            }

            // ★ ConnectionLine들 제거 (노드 자식이 아닌 _nodeContainer 직접 자식 중 "ConnectionLine" 이름)
            if (_nodeContainer != null)
            {
                for (int i = _nodeContainer.childCount - 1; i >= 0; i--)
                {
                    var child = _nodeContainer.GetChild(i);
                    if (child != null && child.name.StartsWith("ConnectionLine"))
                    {
                        Destroy(child.gameObject);
                    }
                }
            }

            // 라벨 컨테이너 자식 제거
            if (_labelContainer != null)
            {
                for (int i = _labelContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(_labelContainer.GetChild(i).gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            ClearAll();
        }
    }
}
