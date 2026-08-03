using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using TeamLog.Combat.Turn;
using TeamLog.Combat.Draw;
using TeamLog.Characters;
using TeamLog.Skill;  // ★ P1-Q2: BehaviorKeyword/LimitBreak
using TeamLog.UI;
using TeamLog.UI.Battle.Direction;  // ★ Phase GF: BattleDirectionController

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 하단 액션 바 (스킬 선택, End Turn 버튼)
    /// </summary>
    public class ActionBarUI : MonoBehaviour
    {
        [Header("Action Menu")]
        [SerializeField] private Transform _actionMenuContainer;
        [SerializeField] private ActionSlotUI _actionSlotPrefab;
        [SerializeField] private int _maxActionSlots = 6;
        [SerializeField] private TextMeshProUGUI _rerollText;

        [Header("End Turn")]
        [SerializeField] private Button _endTurnButton;

        private TurnManager _turnManager;
        private List<ActionSlotUI> _actionSlots = new List<ActionSlotUI>();
        private int _selectedSlotIndex = -1;
        private int _nextExecutionOrder;
        private int _currentAP;

        // ★ Phase GF (2026-07-21): 전투 연출 컨트롤러 (S2 슬롯 순차 등장용)
        private BattleDirectionController _directionController;

        public event Action<int> OnSlotSelected;
        public event Action OnSlotSelectionCancelled;
        public event Action<int> OnSlotRerollRequested;

        public void Initialize(TurnManager turnManager)
        {
            _turnManager = turnManager;
            CreateActionSlots();
            BindEvents();
            StatusEffectBadge.OnBadgeClicked += OnBadgeClicked;
        }

        /// <summary>★ Phase GF: BattleSceneSetup/DirectionController에서 주입.</summary>
        public void SetDirectionController(BattleDirectionController controller)
        {
            _directionController = controller;
        }

        private void CreateActionSlots()
        {
            foreach (Transform child in _actionMenuContainer)
                Destroy(child.gameObject);

            _actionSlots.Clear();

            for (int i = 0; i < _maxActionSlots; i++)
            {
                var slot = Instantiate(_actionSlotPrefab, _actionMenuContainer);
                slot.Setup(i, this);
                slot.OnSlotRerollRequested += OnSlotRerollRequestedHandler;
                _actionSlots.Add(slot);
            }
        }

        private void BindEvents()
        {
            if (_endTurnButton != null)
                _endTurnButton.onClick.AddListener(OnEndTurnClicked);
        }

        public void UpdateActionSlots(IReadOnlyList<DrawnSkillSlot> slots)
        {
            _nextExecutionOrder = 0;

            for (int i = 0; i < _actionSlots.Count; i++)
            {
                if (i < slots.Count)
                {
                    var slot = slots[i];

                    // ★ Phase GF (2026-07-21): slot.Skill이 null이면 죽은 캐릭터 슬롯 — 빈 슬롯 표시.
                    // 캐릭터 순서와 슬롯 순서 일치 (SkillDrawSystem이 죽은 캐릭터도 빈 슬롯으로 포함).
                    if (slot.Skill == null)
                    {
                        _actionSlots[i].SetEmpty();
                        continue;
                    }

                    _actionSlots[i].gameObject.SetActive(true);
                    _actionSlots[i].SetSkill(slot.Skill, slot.Caster);
                    _actionSlots[i].SetAssigned(slot.IsAssigned);
                    _actionSlots[i].SetExecutionOrder(slot.ExecutionOrder);
                    _actionSlots[i].SetAffordable(IsSlotAffordable(slot));
                }
                else
                {
                    // 파티 크기 초과 (미사용 슬롯) — 숨김
                    _actionSlots[i].Clear();
                    _actionSlots[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// ★ Phase GF: 드로우 완료 알림 — PlayerActionController.OnDrawComplete에서 호출.
        /// 이 시점에 슬롯 순차 등장 애니메이션 트리거 (매 턴 시작, 첫 턴 포함).
        /// </summary>
        public void NotifyNewDraw()
        {
            if (_directionController != null)
                _directionController.PlaySlotDrawEntrance(_actionSlots);
        }

        public void SelectSlot(int slotIndex)
        {
            _selectedSlotIndex = slotIndex;

            for (int i = 0; i < _actionSlots.Count; i++)
                _actionSlots[i].SetSelected(i == slotIndex);

            // ★ Phase GF (2026-07-21): A1 — 선택된 슬롯 글로우 점화
            if (slotIndex >= 0 && slotIndex < _actionSlots.Count && _directionController != null)
                _directionController.PlaySlotUseGlow(_actionSlots[slotIndex]);

            OnSlotSelected?.Invoke(slotIndex);
        }

        private void OnEndTurnClicked()
        {
            OnSlotSelectionCancelled?.Invoke();
            _turnManager?.ConfirmActions();
        }

        private void OnSlotRerollRequestedHandler(int slotIndex)
        {
            OnSlotRerollRequested?.Invoke(slotIndex);
        }

        public void SetRerollState(int remaining, int max)
        {
            bool canReroll = remaining > 0;
            if (_rerollText != null)
            {
                _rerollText.text = $"리롤 {remaining}/{max}";
                _rerollText.color = canReroll
                    ? UIPalette.Default.RerollNormal
                    : UIPalette.Default.RerollEmpty;
            }
            foreach (var slot in _actionSlots)
            {
                if (!slot.gameObject.activeSelf) continue;
                slot.SetRerollAvailable(canReroll);
            }
        }

        public void MarkSlotAssigned(int slotIndex, int executionOrder)
        {
            if (slotIndex >= 0 && slotIndex < _actionSlots.Count)
            {
                _actionSlots[slotIndex].SetAssigned(true);
                _actionSlots[slotIndex].SetExecutionOrder(executionOrder);
            }
        }

        public void ResetAllAssignments()
        {
            _nextExecutionOrder = 0;
            foreach (var slot in _actionSlots)
            {
                slot.SetAssigned(false);
                slot.SetExecutionOrder(-1);
                slot.SetSelected(false);
            }
        }

        public int GetNextExecutionOrder()
        {
            return _nextExecutionOrder++;
        }

        public ActionSlotUI GetSlot(int index)
        {
            if (index >= 0 && index < _actionSlots.Count)
                return _actionSlots[index];
            return null;
        }

        public void SetAPState(int currentAP)
        {
            _currentAP = currentAP;
            UpdateSlotAffordability();
        }

        private void UpdateSlotAffordability()
        {
            var slots = _turnManager?.DrawSystem?.DrawnSlots;
            if (slots == null) return;

            for (int i = 0; i < _actionSlots.Count && i < slots.Count; i++)
            {
                if (!_actionSlots[i].gameObject.activeSelf) continue;
                _actionSlots[i].SetAffordable(IsSlotAffordable(slots[i]));
            }
        }

        /// <summary>★ 2026-08-03 P0-P2: 외부에서 슬롯 활성화 상태 강제 갱신 (PlayerActionController 등).</summary>
        public void RefreshAffordability() => UpdateSlotAffordability();

        /// <summary>★ 2026-08-03 P0-Q1: AP + 자원 조건 통합 검사 (StS/Hearthstone 표준).</summary>
        /// <remarks>
        /// AP 부족 + 자원 부족(Mercy/Ember/Shadows 등) 모두 슬롯 비활성화.
        /// 사용자가 클릭 전에 "사용 불가" 상태를 시각적으로 인지하도록.
        /// ★ 2026-08-03 P1-Q2: LimitBreak(전투당 1회) 사용 시에도 비활성화.
        /// </remarks>
        private bool IsSlotAffordable(DrawnSkillSlot slot)
        {
            if (slot?.Skill == null) return true;   // 빈 슬롯 폴백
            // ★ 2026-08-03: 이미 이번 턴에 사용한 슬롯(IsSelected)은 재사용 불가 → 비활성화
            // StS/Hearthstone 표준 — 사용된 카드는 어두워지고 클릭 불가
            if (slot.IsSelected) return false;
            if (slot.Caster == null) return true;  // 캐스터 미확정 시 활성화 폴백

            // AP 검사 (EffectiveCost 반영 — Fatigue/Escalation 등)
            int effectiveCost = slot.Instance?.EffectiveCost ?? slot.Skill.Cost;
            if (_currentAP < effectiveCost) return false;

            // ★ 자원 검사 (C1 + I1 수정: TurnManager와 일치)
            // - caster 자원 타입과 스킬 비용 자원 타입이 일치할 때만 검사
            // - ConsumeAllResource=true면 자원 0이어도 통과 (위력만 낮아짐, TurnManager 정책과 일치)
            // - MinResourceRequired 명시 시 그 값 요구
            var skill = slot.Skill;
            if (skill.ResourceCostType != ResourceType.None
                && slot.Caster.Resource != null
                && skill.ResourceCostType == slot.Caster.Resource.Resource)
            {
                int current = slot.Caster.Resource.CurrentStacks;
                if (skill.MinResourceRequired > 0 && current < skill.MinResourceRequired)
                    return false;
                if (!skill.ConsumeAllResource
                    && skill.ResourceCostAmount > 0
                    && current < skill.ResourceCostAmount)
                    return false;
            }

            // ★ P1-Q2: LimitBreak(전투당 1회) 이미 사용 시 비활성화
            if (slot.Instance != null && slot.Instance.UsedThisBattle)
            {
                var behaviors = slot.Instance.GetCombinedBehaviors();
                if (behaviors != null)
                {
                    foreach (var tag in behaviors)
                    {
                        if (tag.Keyword == BehaviorKeyword.LimitBreak)
                            return false;
                    }
                }
            }

            // ★ 2026-08-03 P1-Q5: CC(Stun/Freeze/Sleep) 걸린 캐릭터 슬롯 비활성화
            // StS/Hearthstone 표준 — CC 상태 시 카드 어두워짐
            if (slot.Caster.StatusEffects != null && slot.Caster.StatusEffects.IsIncapacitated)
                return false;

            return true;
        }

        private void OnBadgeClicked(string title, string description)
        {
            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Show(title, description);
        }

        private void OnDestroy()
        {
            StatusEffectBadge.OnBadgeClicked -= OnBadgeClicked;
        }

        private void Update()
        {
            if (!isActiveAndEnabled) return;

            // ★ 2026-08-02 P2-2: 숫자 키 1-6으로 슬롯 직접 선택
            for (int i = 0; i < 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    if (i < _actionSlots.Count)
                        SelectSlot(i);
                    return;
                }
            }

            // ESC: 선택 취소
            if (Input.GetKeyDown(KeyCode.Escape) && _selectedSlotIndex >= 0)
            {
                _selectedSlotIndex = -1;
                foreach (var slot in _actionSlots)
                    slot.SetSelected(false);
                OnSlotSelectionCancelled?.Invoke();
            }

            // ★ 2026-08-02 P2-2: Space/Enter로 턴 종료
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                if (_endTurnButton != null && _endTurnButton.interactable)
                    OnEndTurnClicked();
            }
        }
    }
}
