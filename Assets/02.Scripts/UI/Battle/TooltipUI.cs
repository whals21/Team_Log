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
            if (_parentCanvas == null) return;
            // ScreenSpaceOverlay → worldCamera null, ScreenSpaceCamera → worldCamera 사용
            Camera uiCamera = _parentCanvas.worldCamera;
            var parentRT = _rectTransform.parent as RectTransform;
            if (parentRT == null) return;

            Vector3 mouseScreen = Input.mousePosition;

            // 툴팁의 실제 스크린 픽셀 크기 (CanvasScaler / pivot 무관)
            var worldCorners = new Vector3[4];
            _rectTransform.GetWorldCorners(worldCorners);
            Vector2 scrMin = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCorners[0]);
            Vector2 scrMax = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCorners[2]);
            float tooltipW = Mathf.Max(10f, scrMax.x - scrMin.x);
            float tooltipH = Mathf.Max(10f, scrMax.y - scrMin.y);

            // 좌하단 기준 위치 — 기본은 마우스 우측 하단
            float left = mouseScreen.x + _offsetX;
            float bottom = mouseScreen.y + _offsetY;

            // 우측 넘침 → 마우스 좌측으로
            if (left + tooltipW > Screen.width)
                left = mouseScreen.x - _offsetX - tooltipW;
            // 그래도 우측 넘침 → 화면 우측 끝에 붙임
            if (left + tooltipW > Screen.width)
                left = Screen.width - tooltipW - 4f;
            // 좌측 넘침 → 화면 좌측 끝에 붙임 (마우스와 떨어지더라도 최소한 화면 안)
            if (left < 0f)
                left = 4f;

            // 하단 넘침 → 마우스 위쪽으로
            if (bottom < 0f)
                bottom = mouseScreen.y - _offsetY + tooltipH;
            // 상단 넘침 → 화면 상단 끝에 붙임
            if (bottom + tooltipH > Screen.height)
                bottom = Screen.height - tooltipH - 4f;
            // 그래도 하단 넘침 → 화면 하단 끝에 붙임
            if (bottom < 0f)
                bottom = 4f;

            // pivot 보정 → 툴팁 중심의 스크린 좌표
            Vector2 tooltipCenterScreen = new Vector2(
                left + tooltipW * _rectTransform.pivot.x,
                bottom + tooltipH * _rectTransform.pivot.y);

            // 스크린 좌표 → 부모 RectTransform 로컬 좌표
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRT, tooltipCenterScreen, uiCamera, out localPos);
            _rectTransform.anchoredPosition = localPos;
        }
    }
}
