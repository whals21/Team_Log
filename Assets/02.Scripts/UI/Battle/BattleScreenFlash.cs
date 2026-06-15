using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 전투 화면 플래시 효과 — 크리티컬 히트/강한 타격 시 순간 화면 점멸.
    /// BattleUICanvas 최상단에 투명 Image 배치, Flash 호출 시 알파 페이드인/아웃.
    /// DOTween.To() 사용 (asmdef 경계 안전), unscaled time 동작 (히트스톱 중에도 작동).
    /// </summary>
    public class BattleScreenFlash : MonoBehaviour
    {
        private Image _flashImage;
        private Tween _flashTween;

        /// <summary>
        /// 전체 화면을 덮는 투명 Image 생성/설정
        /// </summary>
        public void Initialize(RectTransform parentCanvas)
        {
            var go = new GameObject("ScreenFlash");
            go.transform.SetParent(parentCanvas, false);
            _flashImage = go.AddComponent<Image>();

            var rt = _flashImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _flashImage.color = Color.clear;
            _flashImage.raycastTarget = false; // UI 클릭 차단 방지
        }

        /// <summary>
        /// 지정 색상으로 화면 플래시 (알파 0.5 → 0으로 페이드아웃).
        /// </summary>
        public void Flash(Color color, float duration = 0.2f)
        {
            if (_flashImage == null) return;

            _flashTween?.Kill();

            _flashImage.rectTransform.SetAsLastSibling();
            _flashImage.color = new Color(color.r, color.g, color.b, 0.5f);

            float alpha = 0.5f;
            _flashTween = DOTween.To(
                () => alpha,
                x =>
                {
                    alpha = x;
                    var c = _flashImage.color;
                    c.a = alpha;
                    _flashImage.color = c;
                },
                0f, duration).SetUpdate(true);
        }
    }
}
