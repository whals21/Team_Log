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

        /// <summary>
        /// RectTransform의 anchorMax.x를 트윈 애니메이션 (HP 바용)
        /// </summary>
        public static IEnumerator TweenAnchorMaxX(RectTransform rt, float targetX, float duration = 0.3f)
        {
            float startX = rt.anchorMax.x;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // ease-out
                t = 1f - (1f - t) * (1f - t);
                rt.anchorMax = new Vector2(Mathf.Lerp(startX, targetX, t), rt.anchorMax.y);
                yield return null;
            }
            rt.anchorMax = new Vector2(targetX, rt.anchorMax.y);
        }

        /// <summary>
        /// 피격 플래시 — Image를 지정 색상으로 깜빡임
        /// </summary>
        public static IEnumerator FlashColor(Image img, Color flashColor, float duration = 0.15f)
        {
            if (img == null) yield break;
            Color original = img.color;
            img.color = flashColor;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                img.color = Color.Lerp(flashColor, original, t);
                yield return null;
            }
            img.color = original;
        }

        /// <summary>
        /// CanvasGroup의 alpha를 서서히 변경 (사망 페이드아웃용)
        /// </summary>
        public static IEnumerator FadeToAlpha(CanvasGroup cg, float targetAlpha, float duration = 0.5f)
        {
            if (cg == null) yield break;
            float startAlpha = cg.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            cg.alpha = targetAlpha;
        }
    }
}
