using UnityEngine;
using UnityEngine.UI;
using TeamLog.Map;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 맵 상의 플레이어 위치 마커
    /// </summary>
    public class MapPlayerMarker : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Image _markerImage;
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _pulseMinScale = 0.9f;
        [SerializeField] private float _pulseMaxScale = 1.1f;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void MoveToNode(MapNodeButton nodeButton)
        {
            if (nodeButton == null || _rectTransform == null) return;

            var nodeRect = nodeButton.GetComponent<RectTransform>();
            if (nodeRect != null)
            {
                _rectTransform.SetParent(nodeRect, false);
                _rectTransform.anchoredPosition = Vector2.zero;
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_markerImage == null) return;

            // 펄스 애니메이션
            float t = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f;
            float scale = Mathf.Lerp(_pulseMinScale, _pulseMaxScale, t);
            transform.localScale = Vector3.one * scale;

            // 알파 펄스
            Color color = _markerImage.color;
            color.a = Mathf.Lerp(0.7f, 1f, t);
            _markerImage.color = color;
        }
    }
}
