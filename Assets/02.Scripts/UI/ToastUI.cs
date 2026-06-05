using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamLog.UI
{
    /// <summary>
    /// 화면 상단 임시 메시지 (토스트 알림)
    /// 큐 기반, fade-in/hold/fade-out
    /// </summary>
    public class ToastUI : MonoBehaviour
    {
        private static ToastUI _instance;
        public static ToastUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("ToastUI");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<ToastUI>();
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        private Queue<string> _messageQueue = new Queue<string>();
        private bool _isShowing;
        private TextMeshProUGUI _toastText;
        private CanvasGroup _canvasGroup;

        private void Initialize()
        {
            var canvas = new GameObject("ToastCanvas");
            canvas.transform.SetParent(transform);
            var c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 9998;
            var scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var panel = new GameObject("ToastPanel");
            panel.transform.SetParent(canvas.transform, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.3f, 0.88f);
            rect.anchorMax = new Vector2(0.7f, 0.95f);
            rect.sizeDelta = Vector2.zero;
            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.12f, 0.9f);

            _canvasGroup = panel.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(panel.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            _toastText = textObj.AddComponent<TextMeshProUGUI>();
            _toastText.alignment = TextAlignmentOptions.Center;
            _toastText.fontSize = 20;
            _toastText.color = new Color(0.96f, 0.82f, 0.25f);

            // 폰트 로드 시도
            var font = TMPro.TMP_Settings.fallbackFontAssets?.Count > 0
                ? TMPro.TMP_Settings.fallbackFontAssets[0] : null;
            if (font != null) _toastText.font = font;
        }

        public static void Show(string message)
        {
            Instance._messageQueue.Enqueue(message);
            if (!Instance._isShowing)
                Instance.StartCoroutine(Instance.ShowNext());
            AudioManager.Instance.PlayUIToast();
        }

        private IEnumerator ShowNext()
        {
            _isShowing = true;

            while (_messageQueue.Count > 0)
            {
                string msg = _messageQueue.Dequeue();
                _toastText.text = msg;

                // Fade in
                yield return FadeCanvasGroup(0f, 1f, 0.2f);

                // Hold
                yield return new WaitForSecondsRealtime(1.5f);

                // Fade out
                yield return FadeCanvasGroup(1f, 0f, 0.3f);
            }

            _isShowing = false;
        }

        private IEnumerator FadeCanvasGroup(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = to;
        }
    }
}
