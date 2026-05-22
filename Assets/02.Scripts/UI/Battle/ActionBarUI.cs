using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using TeamLog.Combat.Turn;
using TeamLog.Combat.Draw;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 하단 액션 바 (스킬 선택, 상세정보, End Turn 버튼)
    /// </summary>
    public class ActionBarUI : MonoBehaviour
    {
        [Header("Action Menu")]
        [SerializeField] private Transform _actionMenuContainer;
        [SerializeField] private ActionSlotUI _actionSlotPrefab;
        [SerializeField] private int _maxActionSlots = 6;

        [Header("Action Detail")]
        [SerializeField] private GameObject _actionDetailPanel;
        [SerializeField] private TextMeshProUGUI _actionTitleText;
        [SerializeField] private TextMeshProUGUI _actionDescText;
        [SerializeField] private Transform _effectsContainer;
        [SerializeField] private TextMeshProUGUI _targetTypeText;
        [SerializeField] private Button _cancelButton;

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

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(OnCancelSelection);
        }

        public void UpdateActionSlots(IReadOnlyList<DrawnSkillSlot> slots)
        {
            _nextExecutionOrder = 0;

            for (int i = 0; i < _actionSlots.Count; i++)
            {
                if (i < slots.Count)
                {
                    var slot = slots[i];
                    _actionSlots[i].SetSkill(slot.Skill, slot.Caster);
                    _actionSlots[i].SetAssigned(slot.IsAssigned);
                    _actionSlots[i].SetExecutionOrder(slot.ExecutionOrder);
                    _actionSlots[i].SetAffordable(slot.Skill == null || slot.IsSelected || _currentAP >= slot.Skill.Cost);
                }
                else
                {
                    _actionSlots[i].Clear();
                }
            }
        }

        public void SelectSlot(int slotIndex)
        {
            _selectedSlotIndex = slotIndex;

            for (int i = 0; i < _actionSlots.Count; i++)
            {
                _actionSlots[i].SetSelected(i == slotIndex);
            }

            if (slotIndex >= 0 && slotIndex < _actionSlots.Count)
            {
                ShowActionDetail(_actionSlots[slotIndex]);
            }

            OnSlotSelected?.Invoke(slotIndex);
        }

        private void ShowActionDetail(ActionSlotUI slot)
        {
            if (_actionDetailPanel == null) return;

            _actionDetailPanel.SetActive(true);

            if (_actionTitleText != null && slot.Skill != null)
                _actionTitleText.text = slot.Skill.SkillName;

            if (_actionDescText != null && slot.Skill != null)
                _actionDescText.text = BuildActionDescription(slot.Skill, slot.Caster);
        }

        private string BuildActionDescription(SkillData skill, Character caster)
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(skill.Description))
                parts.Add(skill.Description);

            string summary = BattleDisplayUtil.BuildSkillDescription(skill, caster, "  |  ");
            if (!string.IsNullOrEmpty(summary))
                parts.Add(summary);

            return string.Join("  |  ", parts);
        }

        private void HideActionDetail()
        {
            if (_actionDetailPanel != null)
                _actionDetailPanel.SetActive(false);

            _selectedSlotIndex = -1;

            foreach (var slot in _actionSlots)
                slot.SetSelected(false);
        }

        private void OnCancelSelection()
        {
            HideActionDetail();
            OnSlotSelectionCancelled?.Invoke();
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
            foreach (var slot in _actionSlots)
                slot.SetRerollAvailable(canReroll);
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
                var skill = slots[i].Skill;
                _actionSlots[i].SetAffordable(skill == null || slots[i].IsSelected || _currentAP >= skill.Cost);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
