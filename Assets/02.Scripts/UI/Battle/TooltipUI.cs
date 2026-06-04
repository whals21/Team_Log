using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 싱글톤 툴팁 패널 — 마우스 커서를 따라다니며 툴팁 표시
    /// </summary>
    public class TooltipUI : MonoBehaviour
    {
        [SerializeField] private float _offsetX = 15f;
        [SerializeField] private float _offsetY = -15f;

        private static TooltipUI _instance;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _descText;
        private RectTransform _rectTransform;
        private Canvas _parentCanvas;

        public static TooltipUI Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindFirstObjectByType<TooltipUI>();
                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
            _rectTransform = GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>();

            // 자동으로 자식 요소 찾기
            var title = transform.Find("Title");
            if (title != null) _titleText = title.GetComponent<TextMeshProUGUI>();
            var desc = transform.Find("Desc");
            if (desc != null) _descText = desc.GetComponent<TextMeshProUGUI>();

            gameObject.SetActive(false);
        }

        public void Show(string title, string description)
        {
            if (_titleText != null) _titleText.text = title;
            if (_descText != null) _descText.text = description;
            gameObject.SetActive(true);
            UpdatePosition();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (gameObject.activeSelf)
                UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (_parentCanvas == null || _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                return;

            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform.parent as RectTransform,
                Input.mousePosition,
                null,
                out mousePos);

            _rectTransform.anchoredPosition = mousePos + new Vector2(_offsetX, _offsetY);

            // 화면 밖으로 나가지 않도록 보정
            _rectTransform.ForceUpdateRectTransforms();
            var corners = new Vector3[4];
            _rectTransform.GetWorldCorners(corners);

            var canvasRect = (_parentCanvas.transform as RectTransform).rect;
            bool needsFlip = false;
            foreach (var corner in corners)
            {
                if (corner.x < 0 || corner.x > Screen.width || corner.y < 0 || corner.y > Screen.height)
                {
                    needsFlip = true;
                    break;
                }
            }
            if (needsFlip)
                _rectTransform.anchoredPosition = mousePos + new Vector2(-_offsetX - _rectTransform.rect.width, _offsetY);
        }
    }
}
