using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 캐릭터 고유 자원 큰 원형 배지 (Phase CC — StS/Hearthstone 스타일).
    /// 캐릭터 초상화 옆에 큰 원형 아이콘으로 표시. 중앙 큰 숫자 + 자원 이니셜.
    /// 자원별 고유 색상 테두리. 변화 시 펄스 스케일.
    /// ★ 임계값(WarningThreshold) 도달 시 빨강 깜빡임/글로우 — 위험도 직관 전달.
    /// Slay the Spire 에너지 구슬 + Hearthstone 마나 크리스탈 + Darkest Dungeon Torch 경고 결합.
    /// </summary>
    public class ResourceBadge : MonoBehaviour
    {
        // 원형 배지 — Avatar(48x48) 영역 내 위계, 패널 좌상단
        private static readonly Vector2 BadgeSize = new Vector2(44, 44);
        private static readonly float NumberFontSize = 18f;
        private static readonly float InitialFontSize = 9f;
        private static readonly float PulseScale = 0.3f;
        private static readonly float PulseDuration = 0.3f;
        private static readonly float WarningBlinkPeriod = 0.6f;

        private CharacterResourceComponent _resource;
        private Image _ring;           // 외곽 원형 테두리 (자원색)
        private Image _bg;              // 내부 배경 (어두운 색)
        private TextMeshProUGUI _numberTmp;
        private TextMeshProUGUI _initialTmp;
        private Color _resourceColor;
        private bool _warningActive;
        private Tween _warningTween;

        /// <summary>부모 아래 자원 배지 동적 생성. 자원 null이면 null 반환.</summary>
        public static ResourceBadge Create(Transform parent, CharacterResourceComponent resource)
        {
            if (resource == null) return null;

            var go = new GameObject("ResourceBadge");
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = BadgeSize;
            // 앵커: 부모 좌상단 기준, 살짝 우하단 오프셋 (초상화 옆)
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(2, -2);

            // 외곽 원형 링 (자원색)
            var ringGo = new GameObject("Ring");
            var ringRect = ringGo.AddComponent<RectTransform>();
            ringRect.SetParent(rect, false);
            ringRect.anchorMin = Vector2.zero;
            ringRect.anchorMax = Vector2.one;
            ringRect.offsetMin = Vector2.zero;
            ringRect.offsetMax = Vector2.zero;
            var ring = ringGo.AddComponent<Image>();
            ring.sprite = BattleDisplayUtil.WhiteSprite;
            ring.color = BattleDisplayUtil.GetResourceColor(resource.Resource);

            // 내부 배경 (어두운)
            var bgGo = new GameObject("BG");
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.SetParent(ringRect, false);
            bgRect.anchorMin = new Vector2(0.12f, 0.12f);
            bgRect.anchorMax = new Vector2(0.88f, 0.88f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bg = bgGo.AddComponent<Image>();
            bg.sprite = BattleDisplayUtil.WhiteSprite;
            bg.color = new Color(0.04f, 0.04f, 0.1f, 0.95f); // BgPanel 색

            // 중앙 큰 숫자
            var numGo = new GameObject("Number");
            var numRect = numGo.AddComponent<RectTransform>();
            numRect.SetParent(bgRect, false);
            numRect.anchorMin = new Vector2(0f, 0.25f);
            numRect.anchorMax = new Vector2(1f, 1f);
            numRect.offsetMin = Vector2.zero;
            numRect.offsetMax = Vector2.zero;
            var numTmp = numGo.AddComponent<TextMeshProUGUI>();
            UIKoreanFont.EnsureFont(numTmp);
            numTmp.fontSize = NumberFontSize;
            numTmp.fontStyle = FontStyles.Bold;
            numTmp.alignment = TextAlignmentOptions.Center;
            numTmp.color = Color.white;
            numTmp.enableWordWrapping = false;
            numTmp.raycastTarget = false;

            // 하단 작은 이니셜 (자원 라벨)
            var initGo = new GameObject("Initial");
            var initRect = initGo.AddComponent<RectTransform>();
            initRect.SetParent(bgRect, false);
            initRect.anchorMin = new Vector2(0f, 0f);
            initRect.anchorMax = new Vector2(1f, 0.3f);
            initRect.offsetMin = Vector2.zero;
            initRect.offsetMax = Vector2.zero;
            var initTmp = initGo.AddComponent<TextMeshProUGUI>();
            UIKoreanFont.EnsureFont(initTmp);
            initTmp.fontSize = InitialFontSize;
            initTmp.fontStyle = FontStyles.Bold;
            initTmp.alignment = TextAlignmentOptions.Center;
            initTmp.color = new Color(0.85f, 0.85f, 0.85f);
            initTmp.enableWordWrapping = false;
            initTmp.raycastTarget = false;
            initTmp.text = BattleDisplayUtil.GetResourceInitial(resource.Resource);

            var badge = go.AddComponent<ResourceBadge>();
            badge._resource = resource;
            badge._ring = ring;
            badge._bg = bg;
            badge._numberTmp = numTmp;
            badge._initialTmp = initTmp;
            badge._resourceColor = ring.color;

            // 툴팁
            var tooltip = go.AddComponent<TooltipTarget>();
            string label = BattleDisplayUtil.GetResourceLabel(resource.Resource);
            string desc = BattleDisplayUtil.GetResourceDescription(resource.Resource);
            tooltip.SetContent(label, desc);

            badge.Refresh();
            return badge;
        }

        /// <summary>외부에서 매 턴/스킬 사용/자원 변화 후 갱신 호출.</summary>
        public void Refresh()
        {
            if (_resource == null || _numberTmp == null) return;

            int current = _resource.CurrentStacks;
            int max = _resource.MaxStacks;
            _numberTmp.text = current.ToString();

            // 임계값 도달 시 경고 (빨강 깜빡임)
            bool shouldWarn = current >= _resource.WarningThreshold && current > 0;
            if (shouldWarn && !_warningActive)
                StartWarning();
            else if (!shouldWarn && _warningActive)
                StopWarning();
        }

        /// <summary>자원 변화 시 펄스 애니 (스택 증가=확대, 감소=축소).</summary>
        public void OnStacksChanged(int delta)
        {
            if (delta == 0) return;
            Refresh();

            // 펄스 스케일
            float dir = delta > 0 ? 1f : -0.4f;
            PunchScale(dir);
        }

        private void PunchScale(float direction)
        {
            var originalScale = transform.localScale;
            var target = originalScale + new Vector3(PulseScale * direction, PulseScale * direction, 0f);
            DOTween.To(
                () => transform.localScale,
                x => transform.localScale = x,
                target,
                PulseDuration * 0.4f)
                .OnComplete(() =>
                {
                    DOTween.To(
                        () => transform.localScale,
                        x => transform.localScale = x,
                        originalScale,
                        PulseDuration * 0.6f);
                });
        }

        private void StartWarning()
        {
            _warningActive = true;
            // 테두리 색을 빨강으로 변경 + 주기적 깜빡임
            _ring.color = UIPalette.Default.AccentRed;
            _warningTween?.Kill();
            _warningTween = DOTween.To(
                () => _ring.color.a,
                a => _ring.color = new Color(_ring.color.r, _ring.color.g, _ring.color.b, a),
                0.4f,
                WarningBlinkPeriod * 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .From(1f);
        }

        private void StopWarning()
        {
            _warningActive = false;
            _warningTween?.Kill();
            _ring.color = new Color(_resourceColor.r, _resourceColor.g, _resourceColor.b, 1f);
        }

        private void OnDestroy()
        {
            _warningTween?.Kill();
        }
    }
}
