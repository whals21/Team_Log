using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.UI;
using DG.Tweening;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 전투 패널 공통 기반 클래스 — SetDead, FlashHit, UpdateStats, UpdateStatusEffects 공유
    /// EnemyDetailPanel, PlayerSidebarPanel이 상속
    /// </summary>
    public abstract class BattlePanelBase : MonoBehaviour
    {
        protected CanvasGroup _canvasGroup;
        protected Image _panelBgImage;

        // ★ Phase CC (2026-07-22): CC 시각 연출 — Stun/Freeze/Sleep 별표/결정/z
        private CCVisualizer _ccVisualizer;

        [SerializeField] protected TextMeshProUGUI _statText;
        [SerializeField] protected Transform _statusEffectContainer;

        /// <summary>
        /// CanvasGroup과 배경 Image 초기화 — 서브클래스 Awake()에서 호출
        /// </summary>
        protected void InitPanelBase()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _panelBgImage = GetComponent<Image>();

            // ★ Phase CC: CCVisualizer 자동 부착
            _ccVisualizer = GetComponent<CCVisualizer>();
            if (_ccVisualizer == null)
                _ccVisualizer = gameObject.AddComponent<CCVisualizer>();
        }

        protected T FindComponent<T>(string path) where T : Component
        {
            var t = transform.Find(path);
            return t != null ? t.GetComponent<T>() : null;
        }

        public virtual void SetDead(bool isDead)
        {
            if (_canvasGroup != null)
            {
                if (isDead)
                {
                    UIAnimationHelper.FadeToAlpha(_canvasGroup, 0.4f, 0.5f).OnComplete(() =>
                    {
                        _canvasGroup.interactable = false;
                        _canvasGroup.blocksRaycasts = false;
                    });
                }
                else
                {
                    _canvasGroup.alpha = 1f;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }
            }
        }

        public void FlashHit()
        {
            if (_panelBgImage != null)
                UIAnimationHelper.FlashColor(_panelBgImage, Color.white, 0.15f);
        }

        public virtual void UpdateStats(int atk, int def)
        {
            if (_statText != null)
                _statText.text = $"ATK {atk}  DEF {def}";
        }

        public virtual void UpdateStatusEffects(IEnumerable<ActiveEffect> effects)
        {
            if (_statusEffectContainer == null) return;

            for (int i = _statusEffectContainer.childCount - 1; i >= 0; i--)
                Destroy(_statusEffectContainer.GetChild(i).gameObject);

            if (effects == null)
            {
                _ccVisualizer?.Hide();
                return;
            }

            // ★ Phase CC: Stun/Freeze/Sleep 체크 — CCVisualizer에 전달
            StatusEffectType activeCC = StatusEffectType.None;
            foreach (var effect in effects)
            {
                if (effect.Type == StatusEffectType.Stun) { activeCC = StatusEffectType.Stun; break; }
                if (effect.Type == StatusEffectType.Freeze) { activeCC = StatusEffectType.Freeze; break; }
                if (effect.Type == StatusEffectType.Sleep) { activeCC = StatusEffectType.Sleep; break; }
            }

            if (activeCC != StatusEffectType.None)
                _ccVisualizer?.Show(activeCC);
            else
                _ccVisualizer?.Hide();

            foreach (var effect in effects)
                StatusEffectBadge.Create(_statusEffectContainer, effect);
        }
    }
}
