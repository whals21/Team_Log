using System.Collections.Generic;
using TeamLog.Combat.Draw;
using TeamLog.Combat.Turn;
using TeamLog.UI.Battle;

using Character = TeamLog.Characters.Character;
using TargetType = TeamLog.Characters.TargetType;

namespace TeamLog.Combat
{
    /// <summary>
    /// 스킬 선택 → 타겟 선택 → 즉시 시전 흐름 중재 컨트롤러
    /// </summary>
    public class PlayerActionController
    {
        private enum TargetMode
        {
            None,
            SelectingEnemy,
            SelectingAlly
        }

        private readonly TurnManager _turnManager;
        private readonly SkillDrawSystem _drawSystem;
        private readonly ActionBarUI _actionBar;
        private readonly BattleUIManager _uiManager;
        private readonly List<Character> _playerParty;
        private readonly List<Character> _enemies;

        private int _selectedSlotIndex = -1;
        private TargetMode _targetMode = TargetMode.None;

        public PlayerActionController(
            TurnManager turnManager,
            ActionBarUI actionBar,
            BattleUIManager uiManager,
            List<Character> playerParty,
            List<Character> enemies)
        {
            _turnManager = turnManager;
            _drawSystem = turnManager.DrawSystem;
            _actionBar = actionBar;
            _uiManager = uiManager;
            _playerParty = playerParty;
            _enemies = enemies;
        }

        public void Initialize()
        {
            _drawSystem.OnDrawComplete += OnDrawComplete;
            _actionBar.OnSlotSelected += OnSlotSelected;
            _actionBar.OnSlotSelectionCancelled += OnSlotSelectionCancelled;

            _uiManager.OnPlayerPanelClickedInternal += OnPlayerPanelClicked;
            _uiManager.OnEnemyPanelClickedInternal += OnEnemyPanelClicked;
        }

        public void Shutdown()
        {
            _drawSystem.OnDrawComplete -= OnDrawComplete;
            _actionBar.OnSlotSelected -= OnSlotSelected;
            _actionBar.OnSlotSelectionCancelled -= OnSlotSelectionCancelled;

            _uiManager.OnPlayerPanelClickedInternal -= OnPlayerPanelClicked;
            _uiManager.OnEnemyPanelClickedInternal -= OnEnemyPanelClicked;
        }

        // ── Draw Phase ──────────────────────────────────────────────

        private void OnDrawComplete(IReadOnlyList<DrawnSkillSlot> slots)
        {
            foreach (var slot in slots)
                slot.Reset();

            _actionBar.ResetAllAssignments();
            _actionBar.UpdateActionSlots(slots);
            _uiManager.ClearAllHighlights();
        }

        // ── Slot Selection ──────────────────────────────────────────

        private void OnSlotSelected(int slotIndex)
        {
            var slot = _drawSystem.GetSlot(slotIndex);
            if (slot == null || slot.Skill == null) return;

            // 이미 시전된 스킬은 무시
            if (slot.IsSelected) return;

            _selectedSlotIndex = slotIndex;

            var targetType = slot.Skill.Target;
            switch (targetType)
            {
                case TargetType.Self:
                    CastImmediately(slot, slot.Caster);
                    break;

                case TargetType.AllEnemies:
                    CastImmediately(slot, null);
                    break;

                case TargetType.AllAllies:
                    CastImmediately(slot, null);
                    break;

                case TargetType.SingleEnemy:
                    _targetMode = TargetMode.SelectingEnemy;
                    _uiManager.HighlightEnemyPanels(true);
                    break;

                case TargetType.SingleAlly:
                    _targetMode = TargetMode.SelectingAlly;
                    _uiManager.HighlightPlayerPanels(true);
                    break;
            }
        }

        private void OnSlotSelectionCancelled()
        {
            CancelTargetSelection();
        }

        private void CancelTargetSelection()
        {
            _selectedSlotIndex = -1;
            _targetMode = TargetMode.None;
            _uiManager.ClearAllHighlights();
        }

        // ── Target Panel Click ──────────────────────────────────────

        private void OnEnemyPanelClicked(int enemyIndex)
        {
            if (_targetMode != TargetMode.SelectingEnemy) return;

            if (enemyIndex < 0 || enemyIndex >= _enemies.Count) return;
            var enemy = _enemies[enemyIndex];
            if (!enemy.IsAlive) return;

            var slot = _drawSystem.GetSlot(_selectedSlotIndex);
            if (slot == null) return;

            CastImmediately(slot, enemy);
        }

        private void OnPlayerPanelClicked(int playerIndex)
        {
            if (_targetMode != TargetMode.SelectingAlly) return;

            if (playerIndex < 0 || playerIndex >= _playerParty.Count) return;
            var player = _playerParty[playerIndex];
            if (!player.IsAlive) return;

            var slot = _drawSystem.GetSlot(_selectedSlotIndex);
            if (slot == null) return;

            CastImmediately(slot, player);
        }

        // ── Immediate Cast ──────────────────────────────────────────

        private void CastImmediately(DrawnSkillSlot slot, Character target)
        {
            // 슬롯 사용 표시
            slot.IsSelected = true;
            slot.AssignedTarget = target;

            // 즉시 시전
            bool battleEnded = _turnManager.ExecuteSkillImmediately(slot.Caster, slot.Skill, target);

            // UI 갱신
            _actionBar.MarkSlotAssigned(slot.SlotIndex, _actionBar.GetNextExecutionOrder());
            _actionBar.UpdateActionSlots(_drawSystem.DrawnSlots);

            _targetMode = TargetMode.None;
            _selectedSlotIndex = -1;
            _uiManager.ClearAllHighlights();

            _uiManager.AddLog($"[{slot.Caster.Name}] {slot.Skill.SkillName}" +
                (target != null ? $" → {target.Name}" : ""));

            // 전투 종료 시 아무것도 하지 않음
            if (battleEnded) return;

            // 모든 스킬 사용 완료 시 자동으로 적 턴으로 전환
            bool allUsed = true;
            foreach (var s in _drawSystem.DrawnSlots)
            {
                if (!s.IsSelected) { allUsed = false; break; }
            }
            if (allUsed)
                _turnManager.ConfirmActions();
        }
    }
}
