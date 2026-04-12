using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Map;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 맵 노드 버튼 — 클릭 가능한 단일 노드 UI
    /// </summary>
    public class MapNodeButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TextMeshProUGUI _labelText;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _visitedOverlay;
        [SerializeField] private GameObject _activeHighlight;

        private MapNode _node;
        private System.Action<MapNode> _onClicked;

        // 노드 타입별 색상
        private static readonly Color BattleColor = new Color(0.8f, 0.3f, 0.3f);
        private static readonly Color EliteColor = new Color(0.9f, 0.5f, 0.1f);
        private static readonly Color BossColor = new Color(0.7f, 0.2f, 0.7f);
        private static readonly Color EventColor = new Color(0.3f, 0.6f, 0.9f);
        private static readonly Color ShopColor = new Color(0.9f, 0.8f, 0.2f);
        private static readonly Color RestColor = new Color(0.3f, 0.8f, 0.4f);
        private static readonly Color StartColor = new Color(0.6f, 0.6f, 0.6f);
        private static readonly Color DisabledColor = new Color(0.25f, 0.25f, 0.3f);

        // 노드 타입별 라벨
        private static readonly string[] NodeLabels =
        {
            "시작", "전투", "엘리트", "보스", "이벤트", "상점", "휴식"
        };

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnButtonClicked);
        }

        public void Setup(MapNode node, System.Action<MapNode> onClicked)
        {
            _node = node;
            _onClicked = onClicked;

            // MapNodeReference 컴포넌트로 노드 참조 저장 (MapView에서 검색용)
            var nodeRef = gameObject.GetComponent<MapNodeReference>();
            if (nodeRef == null)
                nodeRef = gameObject.AddComponent<MapNodeReference>();
            nodeRef.Node = node;

            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (_node == null) return;

            // 라벨
            int typeIndex = (int)_node.NodeType;
            if (_labelText != null)
                _labelText.text = typeIndex < NodeLabels.Length ? NodeLabels[typeIndex] : "?";

            // 색상
            Color nodeColor = GetNodeColor(_node.NodeType);

            if (_backgroundImage != null)
            {
                _backgroundImage.color = _node.IsActive ? nodeColor :
                    _node.IsVisited ? DisabledColor : DisabledColor;
            }

            // 방문 오버레이
            if (_visitedOverlay != null)
                _visitedOverlay.SetActive(_node.IsVisited);

            // 활성 하이라이트
            if (_activeHighlight != null)
                _activeHighlight.SetActive(_node.IsActive);

            // 버튼 상호작용
            if (_button != null)
                _button.interactable = _node.IsActive;
        }

        private void OnButtonClicked()
        {
            _onClicked?.Invoke(_node);
        }

        private Color GetNodeColor(MapNodeType type)
        {
            return type switch
            {
                MapNodeType.Start => StartColor,
                MapNodeType.Battle => BattleColor,
                MapNodeType.Elite => EliteColor,
                MapNodeType.Boss => BossColor,
                MapNodeType.Event => EventColor,
                MapNodeType.Shop => ShopColor,
                MapNodeType.Rest => RestColor,
                _ => Color.white
            };
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
