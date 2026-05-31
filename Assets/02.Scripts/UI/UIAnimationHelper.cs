using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TeamLog.UI
{
    /// <summary>
    /// UI 애니메이션 코루틴 헬퍼
    /// </summary>
    public static class UIAnimationHelper
    {
        public static IEnumerator FadeIn(CanvasGroup cg, float duration = 0.3f)
        {
            cg.alpha = 0f;
            cg.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            cg.alpha = 1f;
        }

        public static IEnumerator FadeOut(CanvasGroup cg, float duration = 0.3f)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = 1f - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            cg.alpha = 0f;
            cg.gameObject.SetActive(false);
        }

        public static IEnumerator ScaleFromZero(Transform target, float duration = 0.3f, float targetScale = 1f)
        {
            target.localScale = Vector3.zero;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // 이징: ease-out
                t = 1f - (1f - t) * (1f - t);
                target.localScale = Vector3.one * (t * targetScale);
                yield return null;
            }
            target.localScale = Vector3.one * targetScale;
        }

        public static CanvasGroup EnsureCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
