using UnityEngine;
using DG.Tweening;

namespace TeamLog.UI
{
    /// <summary>
    /// UI Canvas 흔들림 효과 — 피격 시 화면 흔들림
    /// ScreenSpaceOverlay Canvas에서도 작동
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        private static CameraShake _instance;
        private RectTransform _canvasRect;
        private Vector2 _originalAnchoredPosition;
        private Tween _shakeTween;

        public static CameraShake Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindFirstObjectByType<CameraShake>();
                if (_instance == null)
                {
                    var go = new GameObject("CameraShake");
                    _instance = go.AddComponent<CameraShake>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 캔버스 흔들림 효과
        /// </summary>
        /// <param name="canvas">흔들 Canvas의 RectTransform</param>
        /// <param name="duration">지속 시간</param>
        /// <param name="strength">강도 (픽셀)</param>
        /// <param name="vibrato">진동 횟수</param>
        public void Shake(RectTransform canvas, float duration = 0.2f, float strength = 8f, int vibrato = 10)
        {
            if (canvas == null) return;

            _canvasRect = canvas;
            _originalAnchoredPosition = canvas.anchoredPosition;

            _shakeTween?.Kill();
            // DOTween.To()로 흔들림 구현 (확장 메서드 의존 제거)
            float elapsed = 0f;
            float interval = duration / vibrato;
            _shakeTween = DOTween.To(() => elapsed, t =>
            {
                elapsed = t;
                var offset = new Vector2(
                    UnityEngine.Random.Range(-strength, strength) * (1f - t),
                    UnityEngine.Random.Range(-strength, strength) * (1f - t));
                canvas.anchoredPosition = _originalAnchoredPosition + offset;
            }, 1f, duration).SetUpdate(true).OnComplete(() =>
            {
                if (_canvasRect != null)
                    _canvasRect.anchoredPosition = _originalAnchoredPosition;
            });
        }
    }
}
