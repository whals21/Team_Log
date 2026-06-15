using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using TeamLog.Combat.Turn;
using TeamLog.Combat.Draw;
using TeamLog.Characters;
using TeamLog.UI;

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
                    _actionSlots[i].gameObject.SetActive(true);
                    _actionSlots[i].SetSkill(slot.Skill, slot.Caster);
                    _actionSlots[i].SetAssigned(slot.IsAssigned);
                    _actionSlots[i].SetExecutionOrder(slot.ExecutionOrder);
                    _actionSlots[i].SetAffordable(slot.Skill == null || slot.IsSelected || _currentAP >= slot.Skill.Cost);
                }
                else
                {
                    _actionSlots[i].Clear();
                    _actionSlots[i].gameObject.SetActive(false);
                }
            }
        }

        public void SelectSlot(int slotIndex)
        {
            _selectedSlotIndex = slotIndex;

            for (int i = 0; i < _actionSlots.Count; i++)
                _actionSlots[i].SetSelected(i == slotIndex);

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
                var skill = slots[i].Skill;
                _actionSlots[i].SetAffordable(skill == null || slots[i].IsSelected || _currentAP >= skill.Cost);
            }
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
            if (Input.GetKeyDown(KeyCode.Escape) && _selectedSlotIndex >= 0)
            {
                _selectedSlotIndex = -1;
                foreach (var slot in _actionSlots)
                    slot.SetSelected(false);
                OnSlotSelectionCancelled?.Invoke();
            }
        }
    }
}
