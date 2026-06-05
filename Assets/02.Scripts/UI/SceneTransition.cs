using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TeamLog.UI
{
    /// <summary>
    /// 씬 트랜지션 — 페이드 인/아웃 오버레이
    /// DontDestroyOnLoad 싱글톤. 코루틴이 이 객체에서 실행되므로 씬 전환 후에도 살아있음.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        private static SceneTransition _instance;
        public static SceneTransition Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("SceneTransition");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<SceneTransition>();
                    _instance.CreateOverlay();
                }
                return _instance;
            }
        }

        private Image _fadeImage;
        private const float DefaultDuration = 0.3f;

        private void CreateOverlay()
        {
            var canvas = new GameObject("FadeCanvas");
            canvas.transform.SetParent(transform);
            var c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 9999;
            canvas.AddComponent<CanvasScaler>();

            // GraphicRaycaster 추가하지 않음 — 입력을 차단하지 않기 위해
            // 대신 페이드 중에만 Image의 raycastTarget을 켬

            var image = new GameObject("FadeImage");
            image.transform.SetParent(canvas.transform, false);
            _fadeImage = image.AddComponent<Image>();
            _fadeImage.color = new Color(0, 0, 0, 0);
            _fadeImage.raycastTarget = false;
            var rect = image.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        /// <summary>
        /// 페이드 아웃 → 씬 로드 → 페이드 인. 이 객체(=DontDestroyOnLoad)에서 코루틴이 실행됨.
        /// </summary>
        public void FadeToScene(string sceneName, float duration = DefaultDuration)
        {
            AudioManager.Instance.PlayUITransition();
            StartCoroutine(FadeRoutine(sceneName, duration));
        }

        private IEnumerator FadeRoutine(string sceneName, float duration)
        {
            // 페이드 아웃 중에는 입력 차단
            _fadeImage.raycastTarget = true;

            yield return FadeAlpha(0f, 1f, duration);

            SceneManager.LoadScene(sceneName);

            yield return FadeAlpha(1f, 0f, duration);

            // 완전히 투명해지면 입력 차단 해제
            _fadeImage.raycastTarget = false;
        }

        private IEnumerator FadeAlpha(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(from, to, t));
                yield return null;
            }
            _fadeImage.color = new Color(0, 0, 0, to);
        }
    }
}
