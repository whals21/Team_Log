using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TeamLog.UI;  // ★ UIKoreanFont

namespace TeamLog.UI.Battle.Direction
{
    /// <summary>
    /// ★ Phase GF (2026-07-21): S1 — 턴 시작 배너 ("아군 턴"/"적 턴").
    /// 1.6초 표시 후 자동 페이드아웃. 다크 판타지 고딕 톤 (Cinzel 폰트 흉내).
    /// 기존 BattleTitleManager(Motion Titles Pack)와 병행 운영.
    ///
    /// 연출 흐름:
    ///   0.0s — 페이드인 시작 (alpha 0→1, scale 1.5→1.0, letter-spacing 24→8)
    ///   0.2s — 완전 표시
    ///   1.3s — 페이드아웃 시작 (alpha 1→0, scale 1.0→0.9)
    ///   1.6s — 자동 Destroy
    /// </summary>
    public class TurnBannerView : MonoBehaviour
    {
        public static readonly Color BannerColorAlly = new Color(0.35f, 0.65f, 1.0f);   // 청록
        public static readonly Color BannerColorEnemy = new Color(0.85f, 0.20f, 0.20f); // 크림슨

        [SerializeField] private float _showDuration = 1.6f;
        [SerializeField] private float _fontSize = 56f;

        private RectTransform _container;
        private GameObject _currentBanner;
        private TextMeshProUGUI _currentText;
        private Tween _lifetimeTween;

        private void Awake()
        {
            // 배너 컨테이너 자식으로 배치 (ScreenSpaceOverlay 상단)
            _container = transform as RectTransform;
            if (_container != null)
            {
                _container.anchorMin = new Vector2(0.5f, 0.5f);
                _container.anchorMax = new Vector2(0.5f, 0.5f);
                _container.pivot = new Vector2(0.5f, 0.5f);
                _container.anchoredPosition = Vector2.zero;
                _container.sizeDelta = new Vector2(800, 120);
            }
        }

        public void Show(string text, Color tint)
        {
            if (_container == null) return;

            // 기존 배너 정리
            KillAndClear();

            // 새 배너 GameObject 생성 (자식에만)
            _currentBanner = new GameObject("TurnBanner");
            _currentBanner.transform.SetParent(_container, false);

            var rt = _currentBanner.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _currentText = _currentBanner.AddComponent<TextMeshProUGUI>();
            _currentText.text = text;
            _currentText.fontSize = _fontSize;
            _currentText.fontStyle = FontStyles.Bold;
            _currentText.alignment = TextAlignmentOptions.Center;
            _currentText.color = tint;
            _currentText.raycastTarget = false;
            _currentText.characterSpacing = 24; // 시작은 넓게

            // 한국어 폰트 보장 (기존 FloatingTextUI/BattleTitleManager 패턴)
            UIKoreanFont.EnsureFont(_currentText);

            // 캔버스 그룹 추가 (alpha 제어)
            var cg = _currentBanner.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;

            PlayEntranceAnimation(rt, cg, tint);
        }

        private void PlayEntranceAnimation(RectTransform rt, CanvasGroup cg, Color tint)
        {
            // Phase 1: 입장 (0~0.25s)
            // alpha 0→1, scale 1.5→1.0, letter-spacing 24→8
            rt.localScale = Vector3.one * 1.5f;

            DOTween.To(() => cg.alpha, a => cg.alpha = a, 1f, 0.25f)
                .SetUpdate(true);

            DOTween.To(() => rt.localScale.x,
                x => rt.localScale = Vector3.one * x, 1f, 0.25f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);

            DOTween.To(() => _currentText.characterSpacing,
                cs => _currentText.characterSpacing = cs, 8f, 0.25f)
                .SetUpdate(true);

            // ★ outline 효과는 폰트 매터리얼이 있어야 작동.
            // fontSharedMaterial = null 제거 (이게 null source 에러 원인).
            // 매터리얼 있을 때만 outline 설정.
            if (_currentText.fontSharedMaterial != null)
            {
                _currentText.outlineWidth = 0.2f;
                _currentText.outlineColor = tint;
            }

            // Phase 2: 유지 후 퇴장 (1.3~1.6s)
            _lifetimeTween = DOVirtual.DelayedCall(1.3f, () =>
            {
                if (_currentBanner == null) return;

                DOTween.To(() => cg.alpha, a => cg.alpha = a, 0f, 0.3f)
                    .SetUpdate(true);

                DOTween.To(() => rt.localScale.x,
                    x => rt.localScale = Vector3.one * x, 0.9f, 0.3f)
                    .SetEase(Ease.InCubic)
                    .SetUpdate(true)
                    .OnComplete(() => KillAndClear());
            }).SetUpdate(true);
        }

        private void KillAndClear()
        {
            _lifetimeTween?.Kill();
            _lifetimeTween = null;

            if (_currentBanner != null)
            {
                Destroy(_currentBanner);
                _currentBanner = null;
                _currentText = null;
            }
        }

        private void OnDestroy()
        {
            KillAndClear();
        }
    }
}
