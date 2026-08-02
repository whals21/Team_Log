using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TeamLog.Characters;
using TeamLog.UI;  // ★ UIKoreanFont

namespace TeamLog.UI.Battle.Direction
{
    /// <summary>
    /// ★ Phase GF (2026-07-21): A2 — 스킬 이름 팝업.
    /// 화면 중앙 상단 (TopBar 아래)에 스킬 이름 + 위력을 스킬 속성별 색상으로 표시.
    /// 약 1.0초 표시 후 페이드아웃.
    ///
    /// 연출 흐름:
    ///   0.00s — 페이드인 시작 (alpha 0→1, scale 0.7→1.1→1.0, y offset +20→0)
    ///   0.25s — 완전 표시
    ///   0.80s — 유지 종료
    ///   0.80~1.10s — 페이드아웃 (alpha 1→0, y 0→-20)
    ///   1.10s — 자동 Destroy
    /// </summary>
    public class SkillNamePopup : MonoBehaviour
    {
        [SerializeField] private float _showDuration = 1.1f;
        [SerializeField] private float _fontSize = 28f;
        [SerializeField] private float _yOffset = 200f; // 상단에서 떨어진 거리

        private RectTransform _container;
        private GameObject _currentPopup;
        private TextMeshProUGUI _currentText;
        private Tween _lifetimeTween;

        // 스킬 타입별 기본 색상
        private static readonly Color AttackColor = new Color(1.0f, 0.45f, 0.10f); // 주황 (기본 Attack)
        private static readonly Color HealColor = new Color(1.0f, 0.85f, 0.20f);   // 금빛
        private static readonly Color ShieldColor = new Color(0.55f, 0.30f, 0.85f); // 보라
        private static readonly Color BuffColor = new Color(0.95f, 0.75f, 0.20f);   // 황금
        private static readonly Color DebuffColor = new Color(0.55f, 0.30f, 0.85f); // 보라
        private static readonly Color PurifyColor = new Color(0.20f, 0.85f, 0.90f); // 청록

        // 속성별 override (StatusEffect 기반)
        private static Color GetElementColor(StatusEffectType effect)
        {
            switch (effect)
            {
                case StatusEffectType.Burn:   return new Color(1.0f, 0.40f, 0.05f); // 불 주황
                case StatusEffectType.Freeze: return new Color(0.30f, 0.65f, 1.0f); // 얼음 파랑
                case StatusEffectType.Poison: return new Color(0.45f, 0.85f, 0.20f); // 독 녹색
                case StatusEffectType.Stun:   return new Color(1.0f, 0.85f, 0.10f); // 번개 노랑
                case StatusEffectType.Bleed:  return new Color(0.95f, 0.20f, 0.20f); // 출혈 빨강
                default: return Color.white;
            }
        }

        private static Color ResolveColor(SkillType type, StatusEffectType element)
        {
            if (element != StatusEffectType.None) return GetElementColor(element);
            switch (type)
            {
                case SkillType.Attack: return AttackColor;
                case SkillType.Heal:   return HealColor;
                case SkillType.Shield: return ShieldColor;
                case SkillType.Buff:   return BuffColor;
                case SkillType.Debuff: return DebuffColor;
                case SkillType.Purify: return PurifyColor;
                default: return Color.white;
            }
        }

        private void Awake()
        {
            _container = transform as RectTransform;
            if (_container != null)
            {
                _container.anchorMin = new Vector2(0.5f, 1f);
                _container.anchorMax = new Vector2(0.5f, 1f);
                _container.pivot = new Vector2(0.5f, 0.5f);
                _container.anchoredPosition = new Vector2(0, -_yOffset);
                _container.sizeDelta = new Vector2(600, 80);
            }
        }

        public void Show(string skillName, int power, SkillType type,
            StatusEffectType element = StatusEffectType.None)
        {
            if (_container == null) return;

            // 기존 팝업 즉시 제거 (연속 시전 시)
            KillAndClear();

            var color = ResolveColor(type, element);

            // 새 팝업 GameObject 생성
            _currentPopup = new GameObject("SkillNamePopup");
            _currentPopup.transform.SetParent(_container, false);

            var rt = _currentPopup.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _currentText = _currentPopup.AddComponent<TextMeshProUGUI>();
            _currentText.fontSize = _fontSize;
            _currentText.fontStyle = FontStyles.Bold;
            _currentText.alignment = TextAlignmentOptions.Center;
            _currentText.color = color;
            _currentText.raycastTarget = false;
            _currentText.characterSpacing = 4;

            // 텍스트 조립: "Ember Strike  +5"
            string powerPart = type == SkillType.Heal ? $"+{power}" : (power > 0 ? $"+{power}" : "");
            _currentText.text = power > 0 ? $"{skillName}  <size=130%>{powerPart}</size>" : skillName;

            // 한국어 폰트 보장 (기존 FloatingTextUI 패턴)
            UIKoreanFont.EnsureFont(_currentText);

            // ★ outline 효과는 폰트 매터리얼이 있어야 작동.
            // 매터리얼 없을 때 outlineWidth setter 호출하면 ArgumentNullException 발생.
            if (_currentText.fontSharedMaterial != null)
            {
                _currentText.outlineWidth = 0.15f;
                _currentText.outlineColor = color;
            }

            var cg = _currentPopup.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;

            PlayEntranceAnimation(rt, cg);
        }

        private void PlayEntranceAnimation(RectTransform rt, CanvasGroup cg)
        {
            // 초기 상태
            rt.localScale = Vector3.one * 0.7f;
            var originalY = rt.anchoredPosition.y;

            // Phase 1: 입장 (0~0.25s)
            DOTween.To(() => cg.alpha, a => cg.alpha = a, 1f, 0.20f)
                .SetEase(Ease.OutQuart)
                .SetUpdate(true);

            DOTween.To(() => rt.localScale.x,
                x => rt.localScale = Vector3.one * x, 1f, 0.30f)
                .SetEase(Ease.OutBack) // 1.15 피크 후 1.0 안착
                .SetUpdate(true);

            DOTween.To(() => rt.anchoredPosition.y,
                y => rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y),
                originalY, 0.30f)
                .From(originalY + 20f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);

            // Phase 2: 유지 후 퇴장 (0.80~1.10s)
            _lifetimeTween = DOVirtual.DelayedCall(0.80f, () =>
            {
                if (_currentPopup == null) return;

                DOTween.To(() => cg.alpha, a => cg.alpha = a, 0f, 0.30f)
                    .SetUpdate(true);

                DOTween.To(() => rt.anchoredPosition.y,
                    y => rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y),
                    originalY - 30f, 0.30f)
                    .SetEase(Ease.InCubic)
                    .SetUpdate(true)
                    .OnComplete(() => KillAndClear());
            }).SetUpdate(true);
        }

        private void KillAndClear()
        {
            _lifetimeTween?.Kill();
            _lifetimeTween = null;

            if (_currentPopup != null)
            {
                Destroy(_currentPopup);
                _currentPopup = null;
                _currentText = null;
            }
        }

        private void OnDestroy()
        {
            KillAndClear();
        }
    }
}
