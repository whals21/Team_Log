using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TeamLog.Characters;

namespace TeamLog.UI.Battle.Direction
{
    /// <summary>
    /// ★ Phase GF (2026-07-21): B2 — 캐릭터 반응 애니메이터.
    /// 시전자(앞으로 점프), 타겟(넉백), 힐 수신자(위로 뜸).
    /// 기존 EnemyDetailPanel.HighlightActing 패턴 일반화 (DOTween.To + Sin 곡선).
    ///
    /// 주의:
    ///   - 모든 트윈은 SetUpdate(true) — TimeScale 2x Fast 모드 대응
    ///   - 겹침 방지: 같은 패널에 새 트윈 시 기존 Kill
    ///   - 원위치 보장: 트윈 종료 시 anchoredPosition/scale 복귀
    /// </summary>
    public class CharacterReactionAnimator : MonoBehaviour
    {
        [Header("Cast (시전자)")]
        [SerializeField] private float _castJumpHeight = 18f;
        [SerializeField] private float _castScaleBonus = 0.10f;
        [SerializeField] private float _castDuration = 0.35f;

        [Header("Hit (피격자 — Attack/Debuff)")]
        [SerializeField] private float _hitKnockback = 15f;
        [SerializeField] private float _hitDuration = 0.30f;

        [Header("Heal/Buff 받은 대상")]
        [SerializeField] private float _healFloatHeight = 12f;
        [SerializeField] private float _healDuration = 0.50f;

        // 진행 중인 트윈 추적 — 패널당 1개
        private readonly Dictionary<Transform, Tween> _activeTweens = new();
        // 원위치 백업 (트윈 종료 후 복귀용)
        private readonly Dictionary<Transform, (Vector2 pos, Vector3 scale)> _originals = new();

        /// <summary>시전자 반응: 위로 점프 + 확대 → 원위치.</summary>
        public void PlayCastReaction(Transform panel)
        {
            if (panel == null) return;
            KillExisting(panel);
            BackupOriginal(panel);

            var rt = panel as RectTransform;
            var original = _originals[panel];

            float elapsed = 0f;
            var tween = DOTween.To(() => elapsed, t =>
            {
                elapsed = t;
                // 0→1→0 사인 곡선
                float curve = Mathf.Sin(Mathf.PI * Mathf.Clamp01(elapsed / _castDuration));
                if (rt != null)
                    rt.anchoredPosition = original.pos + Vector2.up * (_castJumpHeight * curve);
                panel.localScale = original.scale * (1f + _castScaleBonus * curve);
            }, _castDuration, _castDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (rt != null) rt.anchoredPosition = original.pos;
                panel.localScale = original.scale;
                _activeTweens.Remove(panel);
            });

            _activeTweens[panel] = tween;
        }

        /// <summary>타겟 반응: Attack/Debuff → 넉백(아래+흔들림), Heal/Buff → 위로 뜸.</summary>
        public void PlayHitReaction(Transform panel, SkillType type)
        {
            if (panel == null) return;
            KillExisting(panel);
            BackupOriginal(panel);

            bool upward = type == SkillType.Heal || type == SkillType.Buff || type == SkillType.Shield;
            float distance = upward ? _healFloatHeight : _hitKnockback;
            float duration = upward ? _healDuration : _hitDuration;
            int dir = upward ? 1 : -1; // 위로(+1) 또는 아래로(-1)

            var rt = panel as RectTransform;
            var original = _originals[panel];

            float elapsed = 0f;
            var tween = DOTween.To(() => elapsed, t =>
            {
                elapsed = t;
                float curve = Mathf.Sin(Mathf.PI * Mathf.Clamp01(elapsed / duration));
                if (rt != null)
                    rt.anchoredPosition = original.pos + Vector2.up * (distance * curve * dir);
            }, duration, duration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (rt != null) rt.anchoredPosition = original.pos;
                _activeTweens.Remove(panel);
            });

            _activeTweens[panel] = tween;
        }

        private void BackupOriginal(Transform panel)
        {
            if (_originals.ContainsKey(panel)) return;
            var rt = panel as RectTransform;
            var pos = rt != null ? rt.anchoredPosition : Vector2.zero;
            _originals[panel] = (pos, panel.localScale);
        }

        private void KillExisting(Transform panel)
        {
            if (_activeTweens.TryGetValue(panel, out var t))
            {
                t.Kill(true); // complete=true로 OnComplete 호출 보장 → 원위치 복귀
                _activeTweens.Remove(panel);
            }
            _originals.Remove(panel);
        }

        private void OnDestroy()
        {
            foreach (var kvp in _activeTweens)
            {
                kvp.Value?.Kill();
            }
            _activeTweens.Clear();
            _originals.Clear();
        }
    }
}
