using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Map;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// 9 레이어 맵의 단일 노드 (UIBestPractices §1 준거 — 별도 파일).
    /// </summary>
    public class MapReworkNode : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _frameGlow;
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _label;

        private MapNode _node;
        private System.Action<MapNode> _onClick;
        private Sprite _nodeSprite;

        public MapNode Node => _node;

        private void Awake()
        {
            if (_icon == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "Icon");
                if (go != null) _icon = go.GetComponent<Image>();
            }
            if (_button == null)
            {
                _button = GetComponent<Button>();
                if (_button == null) _button = gameObject.AddComponent<Button>();
            }
            if (_label == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "NodeLabel");
                if (go != null) _label = go.GetComponent<TextMeshProUGUI>();
            }
            if (_frameGlow == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "FrameGlow");
                if (go != null) _frameGlow = go.GetComponent<Image>();
            }

            // 자식 Image raycastTarget=false
            UIAutoBindHelper.DisableChildRaycastsExcept(transform, _icon);
            if (_icon != null) _icon.raycastTarget = true;
        }

        public void Setup(MapNode node, Sprite sprite, System.Action<MapNode> onClick)
        {
            _node = node;
            _onClick = onClick;
            _nodeSprite = sprite;

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnClick);
            }

            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (_node == null) return;

            // 스프라이트 적용
            if (_icon != null && _nodeSprite != null)
            {
                _icon.sprite = _nodeSprite;
            }

            // 라벨
            if (_label != null)
            {
                _label.text = GetNodeLabel(_node);
            }

            // 상태 — visited/active/locked
            bool visited = _node.IsVisited;
            bool active = _node.IsActive;
            bool locked = !visited && !active;

            if (_icon != null)
            {
                Color color = _icon.color;
                color.a = visited ? 0.4f : (locked ? 0.35f : 1f);
                _icon.color = color;
            }

            if (_frameGlow != null)
            {
                _frameGlow.gameObject.SetActive(active);
            }

            if (_button != null)
            {
                _button.interactable = active;
            }
        }

        private void OnClick()
        {
            _onClick?.Invoke(_node);
        }

        private static string GetNodeLabel(MapNode node)
        {
            return node.NodeType switch
            {
                MapNodeType.Start  => "Origin",
                MapNodeType.Battle => "Battle",
                MapNodeType.Elite  => "Elite",
                MapNodeType.Boss   => "Boss",
                MapNodeType.Event  => "Event",
                MapNodeType.Shop   => "Shop",
                MapNodeType.Rest   => "Rest",
                _ => ""
            };
        }
    }
}
