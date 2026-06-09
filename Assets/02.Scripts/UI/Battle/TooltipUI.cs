using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 싱글톤 툴팁 패널 — 마우스 커서를 따라다니며 툴팁 표시
    /// Title / Subtitle(비용·타입·타겟) / Description 구조
    /// </summary>
    public class TooltipUI : MonoBehaviour
    {
        [SerializeField] private float _offsetX = 15f;
        [SerializeField] private float _offsetY = -15f;

        private static TooltipUI _instance;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _subtitleText;
        private TextMeshProUGUI _descText;
        private RectTransform _rectTransform;
        private Canvas _parentCanvas;

        public static TooltipUI Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var all = Resources.FindObjectsOfTypeAll<TooltipUI>();
                if (all != null && all.Length > 0)
                    _instance = all[0];
                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
            _rectTransform = GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>();

            var title = transform.Find("Title");
            if (title != null) _titleText = title.GetComponent<TextMeshProUGUI>();

            var subtitle = transform.Find("Subtitle");
            if (subtitle != null) _subtitleText = subtitle.GetComponent<TextMeshProUGUI>();

            var desc = transform.Find("Desc");
            if (desc != null) _descText = desc.GetComponent<TextMeshProUGUI>();

            gameObject.SetActive(false);
        }

        public void Show(string title, string description)
        {
            Show(title, null, description);
        }

        public void Show(string title, string subtitle, string description)
        {
            if (_titleText != null) _titleText.text = title;

            if (_subtitleText != null)
            {
                _subtitleText.gameObject.SetActive(!string.IsNullOrEmpty(subtitle));
                _subtitleText.text = subtitle ?? "";
            }

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

            float offsetX = _offsetX;
            float offsetY = _offsetY;

            // 1) 기본 위치 (마우스 우측 하단)
            _rectTransform.anchoredPosition = mousePos + new Vector2(offsetX, offsetY);
            _rectTransform.ForceUpdateRectTransforms();

            var corners = new Vector3[4];
            _rectTransform.GetWorldCorners(corners);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // 2) 우측 넘침 → 좌측으로 이동
            if (corners[2].x > screenWidth)
                offsetX = -_offsetX - _rectTransform.rect.width;

            // 3) 좌측 넘침 → 다시 우측으로 (최소한 마우스 옆)
            // (좌측 반전 후에도 넘치면 화면 좌측에 붙임)

            // 4) 하단 넘침 → 마우스 위쪽으로
            if (corners[0].y < 0)
                offsetY = -_offsetY + _rectTransform.rect.height;

            // 5) 상단 넘침 → 마우스 아래쪽으로
            if (corners[1].y > screenHeight)
                offsetY = _offsetY;

            _rectTransform.anchoredPosition = mousePos + new Vector2(offsetX, offsetY);
            _rectTransform.ForceUpdateRectTransforms();
            _rectTransform.GetWorldCorners(corners);

            // 6) 최종 clamp — 그래도 넘치면 화면 경계에 맞춤
            var parentRect = _rectTransform.parent as RectTransform;
            Vector2 adjusted = _rectTransform.anchoredPosition;

            // 좌측
            if (corners[0].x < 0)
            {
                Vector2 screenLeft;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, Vector2.zero, null, out screenLeft);
                adjusted.x = screenLeft.x + _rectTransform.rect.width * (1f - _rectTransform.pivot.x);
            }
            // 우측
            if (corners[2].x > screenWidth)
            {
                Vector2 screenRight;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, new Vector2(screenWidth, 0), null, out screenRight);
                adjusted.x = screenRight.x - _rectTransform.rect.width * _rectTransform.pivot.x;
            }
            // 하단
            if (corners[0].y < 0)
            {
                Vector2 screenBottom;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, new Vector2(0, 0), null, out screenBottom);
                adjusted.y = screenBottom.y + _rectTransform.rect.height * (1f - _rectTransform.pivot.y);
            }
            // 상단
            if (corners[1].y > screenHeight)
            {
                Vector2 screenTop;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, new Vector2(0, screenHeight), null, out screenTop);
                adjusted.y = screenTop.y - _rectTransform.rect.height * _rectTransform.pivot.y;
            }

            _rectTransform.anchoredPosition = adjusted;
        }
    }
}
