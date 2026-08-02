using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TeamLog.UI.Battle; // ActionSlotUI

namespace TeamLog.UI.Battle.Direction
{
    /// <summary>
    /// ★ Phase GF (2026-07-21): S2 + A1 — 슬롯 연출 헬퍼 (static).
    ///
    /// S2: 턴 시작 시 슬롯 순차 등장
    ///   - 각 슬롯 0.11초 간격 순차 등장 (좌→우)
    ///   - scale 0.4 → 1.15 → 1.0 (Ease.OutBack)
    ///   - alpha 0 → 1
    ///   - y offset +30px → 0
    ///
    /// A1: 슬롯 사용 시 테두리 글로우 점화
    ///   - 배경 Image에 금빛 점멸 (FlashColor 패턴)
    ///
    /// DOTween.To 직접 사용 + SetUpdate(true) — 기존 패턴 일관성.
    /// </summary>
    public static class SlotEntranceAnimator
    {
        // === S2 상수 ===
        private const float SlotDelay = 0.11f;
        private const float SlotDuration = 0.55f;
        private const float InitialScale = 0.4f;
        private const float PeakScale = 1.15f;
        private const float FinalScale = 1.0f;
        private const float FadeInDuration = 0.2f;
        // ★ Phase GF (2026-07-21): InitialYOffset 제거 — anchoredPosition 조작이 LayoutGroup과
        // 충돌하여 슬롯이 원래 자리로 안 돌아오고 위로 올라가 캐릭터를 가리는 버그 발생.

        // === A1 상수 ===
        private const float GlowDuration = 0.4f;
        private static readonly Color GlowColor = new Color(1.0f, 0.85f, 0.30f, 1f); // 금빛

        /// <summary>
        /// S2: 슬롯 순차 등장. ActionBarUI.UpdateActionSlots에서 호출.
        /// 기존 RerollShuffle 중인 슬롯은 스킵.
        /// </summary>
        public static void TriggerSequentialEntrance(IReadOnlyList<ActionSlotUI> slots)
        {
            if (slots == null) return;

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot == null) continue;
                if (!slot.gameObject.activeSelf) continue;
                if (slot.IsShuffling) continue;

                PlaySingleEntrance(slot.transform, i * SlotDelay);
            }
        }

        /// <summary>A1: 슬롯 사용 글로우. PlayerActionController 또는 ActionBarUI에서 호출.</summary>
        public static void TriggerUseGlow(ActionSlotUI slot)
        {
            if (slot == null) return;

            // 슬롯 배경 Image 찾기 (slot 자체 또는 자식)
            var bg = slot.GetComponent<Image>();
            if (bg == null) bg = slot.GetComponentInChildren<Image>(true);
            if (bg == null) return;

            // FlashColor 패턴 — 원 색상 → 금빛 → 원 색상
            var original = bg.color;
            float elapsed = 0f;

            DOTween.To(() => elapsed, t =>
            {
                elapsed = t;
                float phase = Mathf.Clamp01(elapsed / GlowDuration);
                // 0~0.2: 금빛으로, 0.2~0.4: 원래로 복귀
                float lerp = phase < 0.5f
                    ? Mathf.Sin(phase * Mathf.PI * 2) * 0.5f + 0.5f
                    : 1f - (phase - 0.5f) * 2f;
                bg.color = Color.Lerp(original, GlowColor, lerp * 0.7f);
            }, GlowDuration, GlowDuration)
            .SetUpdate(true)
            .OnComplete(() => bg.color = original);
        }

        // ════════════════════════════════════════════════════════════════
        // 내부 — 단일 슬롯 입장 애니메이션
        // ════════════════════════════════════════════════════════════════

        private static void PlaySingleEntrance(Transform slot, float delay)
        {
            var rt = slot as RectTransform;
            if (rt == null) return;

            var cg = EnsureCanvasGroup(slot.gameObject);

            // ★ Phase GF (2026-07-21): anchoredPosition은 건드리지 않음 (LayoutGroup 충돌 회피).
            // scale + alpha만으로 entrance 연출. 자연스러운 펀치 등장.
            cg.alpha = 0f;
            slot.localScale = Vector3.one * InitialScale;

            // Phase 1: alpha 0→1 (0.2초)
            DOTween.To(() => cg.alpha, a => cg.alpha = a, 1f, FadeInDuration)
                .SetDelay(delay)
                .SetUpdate(true);

            // Phase 2: scale 0.4 → 1.0 (Ease.OutBack가 1.15 피크 자동 생성, 0.55초)
            DOTween.To(() => slot.localScale.x,
                x => slot.localScale = Vector3.one * x, FinalScale, SlotDuration)
                .SetDelay(delay)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        private static CanvasGroup EnsureCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
