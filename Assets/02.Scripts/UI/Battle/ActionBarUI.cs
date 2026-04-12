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
        [Header("Turn Info")]
        [SerializeField] private Image _turnBadge;
        [SerializeField] private TextMeshProUGUI _turnCounterText;

        [Header("Player Stats Mini")]
        [SerializeField] private Transform _miniStatsContainer;
        [SerializeField] private CharacterMiniPanel _miniPanelPrefab;
        [SerializeField] private int _maxMiniPanels = 3;

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
        private List<CharacterMiniPanel> _miniPanels = new List<CharacterMiniPanel>();
        private int _selectedSlotIndex = -1;
        private int _nextExecutionOrder;

        public event Action<int> OnSlotSelected;
        public event Action OnSlotSelectionCancelled;

        public void Initialize(TurnManager turnManager)
        {
            _turnManager = turnManager;
            CreateActionSlots();
            CreateMiniPanels();
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
                _actionSlots.Add(slot);
            }
        }

        private void CreateMiniPanels()
        {
            if (_miniStatsContainer == null || _miniPanelPrefab == null) return;

            foreach (Transform child in _miniStatsContainer)
                Destroy(child.gameObject);

            _miniPanels.Clear();

            for (int i = 0; i < _maxMiniPanels; i++)
            {
                var panel = Instantiate(_miniPanelPrefab, _miniStatsContainer);
                _miniPanels.Add(panel);
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
                _actionDescText.text = slot.Skill.Description;
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

        public void MarkSlotAssigned(int slotIndex, int executionOrder)
        {
            if (slotIndex >= 0 && slotIndex < _actionSlots.Count)
            {
                _actionSlots[slotIndex].SetAssigned(true);
                _actionSlots[slotIndex].SetExecutionOrder(executionOrder);
            }
        }

        public void ClearSlotAssignment(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < _actionSlots.Count)
            {
                _actionSlots[slotIndex].SetAssigned(false);
                _actionSlots[slotIndex].SetExecutionOrder(-1);
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

        public void DecrementExecutionOrder()
        {
            _nextExecutionOrder = System.Math.Max(0, _nextExecutionOrder - 1);
        }

        public void SetTurnCounter(int current, int total)
        {
            if (_turnCounterText != null)
                _turnCounterText.text = $"{current}/{total}";
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
