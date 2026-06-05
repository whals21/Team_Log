using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace TeamLog.UI
{
    /// <summary>
    /// UI 애니메이션 DOTween 헬퍼 — 코루틴 기반에서 DOTween으로 전환
    /// SetUpdate(true)로 unscaledTime 동작 유지
    /// Extension method 의존성을 피하기 위해 DOTween.To() 직접 사용
    /// </summary>
    public static class UIAnimationHelper
    {
        public static Tween FadeIn(CanvasGroup cg, float duration = 0.3f)
        {
            if (cg == null) return null;
            cg.alpha = 0f;
            cg.gameObject.SetActive(true);
            return DOTween.To(() => cg.alpha, x => cg.alpha = x, 1f, duration).SetUpdate(true);
        }

        public static Tween FadeOut(CanvasGroup cg, float duration = 0.3f)
        {
            if (cg == null) return null;
            return DOTween.To(() => cg.alpha, x => cg.alpha = x, 0f, duration)
                .SetUpdate(true)
                .OnComplete(() => cg.gameObject.SetActive(false));
        }

        public static Tween ScaleFromZero(Transform target, float duration = 0.3f, float targetScale = 1f)
        {
            if (target == null) return null;
            target.localScale = Vector3.zero;
            return DOTween.To(() => target.localScale.x,
                x => target.localScale = new Vector3(x, x, x),
                targetScale, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
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
        public static Tween TweenAnchorMaxX(RectTransform rt, float targetX, float duration = 0.3f)
        {
            if (rt == null) return null;
            return DOTween.To(
                () => rt.anchorMax.x,
                x => rt.anchorMax = new Vector2(x, rt.anchorMax.y),
                targetX,
                duration
            ).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        /// <summary>
        /// 피격 플래시 — Image를 지정 색상으로 깜빡임
        /// </summary>
        public static Tween FlashColor(Image img, Color flashColor, float duration = 0.15f)
        {
            if (img == null) return null;
            Color original = img.color;
            img.color = flashColor;
            float progress = 0f;
            return DOTween.To(() => progress, p =>
            {
                progress = p;
                img.color = Color.Lerp(flashColor, original, p);
            }, 1f, duration).SetUpdate(true);
        }

        /// <summary>
        /// CanvasGroup의 alpha를 서서히 변경 (사망 페이드아웃용)
        /// </summary>
        public static Tween FadeToAlpha(CanvasGroup cg, float targetAlpha, float duration = 0.5f)
        {
            if (cg == null) return null;
            return DOTween.To(() => cg.alpha, x => cg.alpha = x, targetAlpha, duration)
                .SetUpdate(true);
        }
    }
}
