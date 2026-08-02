using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TeamLog.Characters;

namespace TeamLog.UI.Battle.Direction
{
    /// <summary>
    /// ★ Phase GF (2026-07-21): 전투 연출 중앙 오케스트레이터.
    /// TurnManager/SkillExecutor에서 발생한 이벤트를 받아 연출 서브시스템에 분배.
    ///
    /// Tier 구조:
    ///   S1 (TurnBannerView) — 턴 시작 배너 ("아군 턴"/"적 턴")
    ///   S2 (SlotEntranceAnimator) — 슬롯 순차 등장
    ///   A1 (SlotEntranceAnimator) — 슬롯 사용 글로우
    ///   A2 (SkillNamePopup) — 스킬 이름 팝업
    ///   B1 (ProjectileSystem) — 투사체 시스템
    ///   B2 (CharacterReactionAnimator) — 시전자/타겟 반응
    ///
    /// 설계 원칙:
    ///   - 이벤트 "구독만" (발행 안 함) — CombatEventBus 정적 이벤트 누수 회피
    ///   - 모든 서브시스템 null 허용 — 누락 시 no-op (안전 장치)
    ///   - MonoBehaviour — BattleSceneSetup.InitializeBattle에서 생성
    /// </summary>
    public class BattleDirectionController : MonoBehaviour
    {
        [Header("Sub-Systems (auto-created if null)")]
        [SerializeField] private TurnBannerView _turnBanner;
        [SerializeField] private SkillNamePopup _skillNamePopup;
        [SerializeField] private ProjectileSystem _projectileSystem;
        [SerializeField] private CharacterReactionAnimator _reactionAnimator;

        [Header("References")]
        [SerializeField] private RectTransform _directionLayer;

        private bool _initialized;
        private RectTransform _parentCanvas;
        private BattleUIManager _uiManager;

        public bool IsInitialized => _initialized;

        /// <summary>전투 시작 시 BattleSceneSetup이 호출. 서브시스템 자동 생성.</summary>
        public void Initialize(RectTransform parentCanvas, BattleUIManager uiManager, TMP_FontAsset koreanFont)
        {
            _parentCanvas = parentCanvas;
            _uiManager = uiManager;

            EnsureDirectionLayer();
            EnsureSubSystems(koreanFont);

            _initialized = true;
            Debug.Log("[BattleDirection] Initialized — Tier S+A+B active");
        }

        // ════════════════════════════════════════════════════════════════
        // 진입점 (Entry Points) — BattleSceneSetup/ActionBarUI에서 호출
        // ════════════════════════════════════════════════════════════════

        /// <summary>S1: 턴 시작 배너. BattleSceneSetup.OnTurnStarted에서 호출.</summary>
        public void PlayTurnStartDirection(int turnNumber, bool isPlayerTurn)
        {
            if (!_initialized) return;
            _turnBanner?.Show(
                isPlayerTurn ? "아군 턴" : "적 턴",
                isPlayerTurn ? TurnBannerView.BannerColorAlly : TurnBannerView.BannerColorEnemy);
        }

        /// <summary>S2: 슬롯 순차 등장. ActionBarUI.UpdateActionSlots에서 호출.</summary>
        public void PlaySlotDrawEntrance(IReadOnlyList<ActionSlotUI> slots)
        {
            if (!_initialized) return;
            SlotEntranceAnimator.TriggerSequentialEntrance(slots);
        }

        /// <summary>A1: 슬롯 사용 글로우. PlayerActionController 또는 ActionBarUI에서 호출.</summary>
        public void PlaySlotUseGlow(ActionSlotUI slot)
        {
            if (!_initialized) return;
            SlotEntranceAnimator.TriggerUseGlow(slot);
        }

        /// <summary>
        /// A2 + B1 + B2 통합 — 스킬 시전 시 연출.
        /// BattleSceneSetup.OnSkillApplied에서 호출.
        /// </summary>
        public void PlaySkillCastDirection(Character caster, SkillData skill, Character target)
        {
            if (!_initialized || caster == null || skill == null) return;

            var casterPanel = _uiManager != null ? _uiManager.GetPanelTransform(caster) : null;
            var targetPanel = target != null && _uiManager != null ? _uiManager.GetPanelTransform(target) : null;

            // A2: 스킬 이름 팝업
            _skillNamePopup?.Show(skill.SkillName, skill.Power, skill.Type, skill.StatusEffect);

            // B2: 시전자 앞으로 점프
            if (casterPanel != null)
                _reactionAnimator?.PlayCastReaction(casterPanel);

            // B1: 투사체 (시전자 → 타겟). 도착 후 타겟 반응(넉백/힐) 트리거
            if (targetPanel != null && _projectileSystem != null)
            {
                _projectileSystem.SpawnProjectile(casterPanel, targetPanel, skill.Type, skill.StatusEffect,
                    onArrive: () =>
                    {
                        if (_reactionAnimator != null)
                            _reactionAnimator.PlayHitReaction(targetPanel, skill.Type);
                    });
            }
            else if (targetPanel != null && _reactionAnimator != null)
            {
                // 투사체 시스템이 없으면 즉시 반응만
                _reactionAnimator.PlayHitReaction(targetPanel, skill.Type);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // 내부 — 서브시스템 자동 생성 (인스펙터 할당 누락 폴백)
        // ════════════════════════════════════════════════════════════════

        private void EnsureDirectionLayer()
        {
            if (_directionLayer != null) return;
            if (_parentCanvas == null) return;

            var layerGo = new GameObject("DirectionLayer");
            layerGo.transform.SetParent(_parentCanvas, false);
            _directionLayer = layerGo.AddComponent<RectTransform>();
            _directionLayer.anchorMin = Vector2.zero;
            _directionLayer.anchorMax = Vector2.one;
            _directionLayer.offsetMin = Vector2.zero;
            _directionLayer.offsetMax = Vector2.zero;
        }

        private void EnsureSubSystems(TMP_FontAsset koreanFont)
        {
            // 각 서브시스템이 인스펙터 할당 안 되어 있으면 자동 생성
            // 이 시점에서는 Tier S/A/B 서브시스템들이 구현되어 있다고 가정.
            // (Phase 2~4에서 실제 클래스 작성 후 자동 연결)

            if (_turnBanner == null)
                _turnBanner = EnsureComponent<TurnBannerView>("TurnBanner", koreanFont);
            if (_skillNamePopup == null)
                _skillNamePopup = EnsureComponent<SkillNamePopup>("SkillNamePopup", koreanFont);
            if (_projectileSystem == null)
                _projectileSystem = EnsureComponent<ProjectileSystem>("ProjectileSystem", koreanFont);
            if (_reactionAnimator == null)
                _reactionAnimator = EnsureComponent<CharacterReactionAnimator>("ReactionAnimator", koreanFont);
        }

        private T EnsureComponent<T>(string goName, TMP_FontAsset font) where T : Component
        {
            // 자식에서 먼저 검색, 없으면 신규 생성
            var existing = GetComponentInChildren<T>(true);
            if (existing != null) return existing;

            // ★ RectTransform 명시적 추가 — UI 자식용 좌표계 보장
            // (GameObject.AddComponent<T>가 MonoBehaviour인 경우 자동 RectTransform 안 붙음)
            var go = new GameObject(goName, typeof(RectTransform));
            go.transform.SetParent(_directionLayer ?? _parentCanvas, false);
            return go.AddComponent<T>();
        }

        private void OnDestroy()
        {
            // MonoBehaviour 파괴 시 서브시스템 트윈 정리는 각 컴포넌트 OnDestroy에서 담당.
            // 여기서는 참조만 해제.
            _turnBanner = null;
            _skillNamePopup = null;
            _projectileSystem = null;
            _reactionAnimator = null;
        }
    }
}
