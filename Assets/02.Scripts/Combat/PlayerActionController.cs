using System.Collections.Generic;
using TeamLog.Combat.Draw;
using TeamLog.Combat.Turn;
using TeamLog.UI.Battle;
using TeamLog.Skill;

using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;
using TargetType = TeamLog.Characters.TargetType;

namespace TeamLog.Combat
{
    /// <summary>
    /// 스킬 선택 → 타겟 선택 → 즉시 시전 흐름 중재 컨트롤러.
    /// Phase CC-2E: 발견(Discover) 스킬 처리 — Cael Alchemist. 모달 팝업으로 3-4개 선택지 제공.
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

        // Phase CC-2E: 발견 스킬 진행 중 상태 — 모달에서 선택된 스킬을 타겟 선택 후 시전.
        // null이 아닐 때 OnEnemyPanelClicked/OnPlayerPanelClicked가 발견 경로로 처리.
        private SkillData _pendingDiscoverSkill;

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
            _actionBar.OnSlotRerollRequested += OnSlotRerollRequested;

            _uiManager.OnPlayerPanelClickedInternal += OnPlayerPanelClicked;
            _uiManager.OnEnemyPanelClickedInternal += OnEnemyPanelClicked;
        }

        public void Shutdown()
        {
            _drawSystem.OnDrawComplete -= OnDrawComplete;
            _actionBar.OnSlotSelected -= OnSlotSelected;
            _actionBar.OnSlotSelectionCancelled -= OnSlotSelectionCancelled;
            _actionBar.OnSlotRerollRequested -= OnSlotRerollRequested;

            _uiManager.OnPlayerPanelClickedInternal -= OnPlayerPanelClicked;
            _uiManager.OnEnemyPanelClickedInternal -= OnEnemyPanelClicked;
        }

        // ── Reroll ──────────────────────────────────────────────

        private void OnSlotRerollRequested(int slotIndex)
        {
            if (!_drawSystem.CanReroll) return;

            var slot = _drawSystem.GetSlot(slotIndex);
            if (slot == null || slot.Skill == null || slot.IsSelected) return;

            // 셔플 풀 — 같은 캐스터의 스킬 목록
            var caster = slot.Caster;
            var shufflePool = caster.SkillInventory.Skills;

            // 리롤 — 새 스킬 결정
            _turnManager.RerollSlot(slotIndex);

            // 리롤된 슬롯의 새 스킬
            var newSlot = _drawSystem.GetSlot(slotIndex);
            var finalSkill = newSlot.Skill;

            // 리롤 카운트 즉시 갱신
            UpdateRerollUI();

            // 셔플 애니메이션 (스킬이 2개 이상일 때만)
            var slotUI = _actionBar.GetSlot(slotIndex);
            if (slotUI != null && shufflePool.Count > 1)
            {
                slotUI.PlayRerollShuffle(finalSkill, caster, shufflePool, () =>
                {
                    // 애니메이션 완료 후 Affordable 상태 갱신
                    int effectiveCost = newSlot.Instance?.EffectiveCost ?? finalSkill.Cost;
                    slotUI.SetAffordable(newSlot.IsSelected || _turnManager.Context.CurrentAP >= effectiveCost);

                    // 리롤이 남아있으면 리롤 버튼 다시 표시
                    if (_drawSystem.CanReroll && !newSlot.IsSelected)
                        slotUI.SetRerollAvailable(true);
                });
            }
            else if (slotUI != null)
            {
                // 스킬이 1개뿐이면 즉시 갱신
                slotUI.SetSkill(finalSkill, caster);
            }
        }

        private void UpdateRerollUI()
        {
            _actionBar.SetRerollState(_drawSystem.RerollsRemaining, _drawSystem.MaxRerolls);
            _uiManager.UpdateRerollCount(_drawSystem.RerollsRemaining, _drawSystem.MaxRerolls);
        }

        // ── Draw Phase ──────────────────────────────────────────────

        private void OnDrawComplete(IReadOnlyList<DrawnSkillSlot> slots)
        {
            foreach (var slot in slots)
                slot.Reset();

            _actionBar.ResetAllAssignments();
            _actionBar.UpdateActionSlots(slots);
            _uiManager.ClearAllHighlights();
            UpdateRerollUI();
        }

        // ── Slot Selection ──────────────────────────────────────────

        private void OnSlotSelected(int slotIndex)
        {
            var slot = _drawSystem.GetSlot(slotIndex);
            if (slot == null || slot.Skill == null) return;

            // 이미 시전된 스킬은 무시
            if (slot.IsSelected) return;

            // Phase CC-2E: 발견 스킬 — 모달 팝업 흐름으로 분기
            if (slot.Skill.IsDiscover && slot.Skill.DiscoverPool != null)
            {
                HandleDiscoverSkill(slot);
                return;
            }

            _selectedSlotIndex = slotIndex;
            EnterTargetSelectionMode(slot);
        }

        /// <summary>발견 스킬 처리 — 모달 팝업 + 선택 후 시전 흐름.</summary>
        private void HandleDiscoverSkill(DrawnSkillSlot slot)
        {
            // AP 부족 시 모달 표시 안 함 (기존 CastImmediately와 동일한 UX)
            int effectiveCost = slot.Instance?.EffectiveCost ?? slot.Skill.Cost;
            if (!_turnManager.Context.CanAfford(effectiveCost))
            {
                _uiManager.AddLog($"AP 부족! (필요: {effectiveCost}, 잔여: {_turnManager.Context.CurrentAP})");
                return;
            }

            _selectedSlotIndex = slot.SlotIndex;

            // "강화 물약" 특성 — 전투당 1회 모달 없이 풀 전부 발동
            var caster = slot.Caster;
            if (DiscoverSystem.ShouldApplyAll(caster))
            {
                DiscoverSystem.ConsumeApplyAll(caster);
                ApplyDiscoverAll(slot);
                return;
            }

            // 모달 표시 — 가중치 추출 후 DiscoverModalUI.Show 호출
            var pool = slot.Skill.DiscoverPool;
            var options = DiscoverSystem.RollOptions(pool, caster);

            if (options == null || options.Count == 0)
            {
                _uiManager.AddLog("[발견] 풀이 비어있음 — 발동 실패");
                _selectedSlotIndex = -1;
                return;
            }

            // 모달이 없으면 즉시 첫 번째 옵션으로 폴백 (UI 미연결 안전장치)
            var modal = _uiManager?.DiscoverModal;
            if (modal == null)
            {
                _uiManager.AddLog("[발견] 모달 UI 미연결 — 첫 옵션 자동 선택");
                OnDiscoverSkillSelected(options[0].Skill);
                return;
            }

            string title = $"{slot.Skill.SkillName}";
            modal.Show(options, title, selected => OnDiscoverSkillSelected(selected), caster);
        }

        /// <summary>"강화 물약" 특성 — 모달 없이 풀 전부 발동.</summary>
        private void ApplyDiscoverAll(DrawnSkillSlot slot)
        {
            var pool = slot.Skill.DiscoverPool;
            var caster = slot.Caster;
            int count = 0;
            foreach (var entry in pool.Entries)
            {
                if (entry.Skill == null) continue;
                // Self/All류는 즉시, Single류는 첫 대상(적: 첫 적, 아군: caster 본인)에 발동
                Character target = ResolveDefaultTarget(entry.Skill, caster);
                CastDiscoverImmediately(slot, entry.Skill, target);
                count++;
                // 도중에 전투 종료 시 중단
                if (_turnManager.IsBattleEndedEarly()) break;
            }
            _uiManager.AddLog($"[강화 물약] 발견 {count}종 전부 발동!");

            // 모든 발견 스킬 발동 후 슬롯 마무리
            FinalizeDiscoverSlot(slot);
        }

        /// <summary>발견 모달에서 스킬 선택됨 — TargetType에 따라 타겟 대기 또는 즉시 시전.</summary>
        private void OnDiscoverSkillSelected(SkillData selected)
        {
            var slot = _drawSystem.GetSlot(_selectedSlotIndex);
            if (slot == null || selected == null)
            {
                // 취소/실패 시 상태 리셋
                _selectedSlotIndex = -1;
                _pendingDiscoverSkill = null;
                _targetMode = TargetMode.None;
                _uiManager.ClearAllHighlights();
                return;
            }

            _pendingDiscoverSkill = selected;

            // 선택된 스킬의 TargetType으로 모드 진입
            switch (selected.Target)
            {
                case TargetType.Self:
                    CastDiscoverImmediately(slot, selected, slot.Caster);
                    FinalizeDiscoverSlot(slot);
                    break;

                case TargetType.AllEnemies:
                    CastDiscoverImmediately(slot, selected, null);
                    FinalizeDiscoverSlot(slot);
                    break;

                case TargetType.AllAllies:
                    CastDiscoverImmediately(slot, selected, null);
                    FinalizeDiscoverSlot(slot);
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

        /// <summary>
        /// 발견 선택 스킬 시전 — 발견 본체(slot.Skill)의 AP/Cost로 소모하지만,
        /// Pipeline.ExecuteSkill에는 선택 스킬을 전달.
        /// 발견 선택 스킬들은 DataGenerator에서 Cost=0으로 설정되므로, ExecuteSkillImmediately는 0 소모.
        /// 발견 본체 Cost는 별도로 SpendAP로 청구.
        /// </summary>
        private void CastDiscoverImmediately(DrawnSkillSlot slot, SkillData selectedSkill, Character target)
        {
            // 선택 스킬을 Pipeline에 전달 (instance=null — 발견 본체의 증강은 선택 스킬에 영향 주면 안 됨)
            bool battleEnded = _turnManager.ExecuteSkillImmediately(slot.Caster, selectedSkill, target, null);

            _uiManager.AddLog($"[{slot.Caster.Name}] 발견: {selectedSkill.SkillName}" +
                (target != null ? $" → {target.Name}" : ""));

            // 발견 본체 Cost 청구 — slot.Skill.Cost에서 selectedSkill.Cost(=0)를 뺀 잔액
            int discoverBodyCost = slot.Skill.Cost;
            if (discoverBodyCost > 0)
                _turnManager.Context.SpendAP(discoverBodyCost);

            // 발견 스킬 사용 표시는 FinalizeDiscoverSlot에서 처리
            _ = battleEnded; // 사용 안 함 (FinalizeDiscoverSlot에서 IsBattleEnded 재확인)
        }

        /// <summary>발견 스킬 발동 후 슬롯 마무리 — IsSelected 마킹 + UI 갱신 + 자동 턴 종료 검사.</summary>
        private void FinalizeDiscoverSlot(DrawnSkillSlot slot)
        {
            slot.IsSelected = true;
            slot.AssignedTarget = null; // 발견은 단일 타겟이 아님

            _actionBar.MarkSlotAssigned(slot.SlotIndex, _actionBar.GetNextExecutionOrder());
            _actionBar.UpdateActionSlots(_drawSystem.DrawnSlots);

            _pendingDiscoverSkill = null;
            _targetMode = TargetMode.None;
            _selectedSlotIndex = -1;
            _uiManager.ClearAllHighlights();

            if (_turnManager.IsBattleEndedEarly()) return;

            // 모든 스킬 사용 완료 시 자동 적 턴 전환
            bool allUsed = true;
            foreach (var s in _drawSystem.DrawnSlots)
            {
                if (!s.IsSelected) { allUsed = false; break; }
            }
            if (allUsed)
                _turnManager.ConfirmActions();
        }

        /// <summary>발견 선택 스킬의 기본 타겟 결정 (ApplyAll 용) — Single 류는 첫 대상.</summary>
        private Character ResolveDefaultTarget(SkillData skill, Character caster)
        {
            switch (skill.Target)
            {
                case TargetType.Self: return caster;
                case TargetType.SingleAlly:
                    return caster; // ApplyAll 시 본인에게 폴백
                case TargetType.SingleEnemy:
                    // 살아있는 첫 적
                    if (_enemies != null)
                        foreach (var e in _enemies)
                            if (e != null && e.IsAlive) return e;
                    return null;
                default: return null; // AllEnemies/AllAllies는 어차피 null
            }
        }

        /// <summary>타겟팅 모드 진입 — 비발견 스킬용 기존 로직.</summary>
        private void EnterTargetSelectionMode(DrawnSkillSlot slot)
        {
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
            _pendingDiscoverSkill = null;
            _uiManager.ClearAllHighlights();
        }

        // ── Target Panel Click ──────────────────────────────────────

        private void OnEnemyPanelClicked(int enemyIndex)
        {
            // 발견 스킬 타겟팅 진행 중
            if (_pendingDiscoverSkill != null && _targetMode == TargetMode.SelectingEnemy)
            {
                if (enemyIndex < 0 || enemyIndex >= _enemies.Count) return;
                var enemy = _enemies[enemyIndex];
                if (!enemy.IsAlive) return;

                var slot = _drawSystem.GetSlot(_selectedSlotIndex);
                if (slot == null) return;

                CastDiscoverImmediately(slot, _pendingDiscoverSkill, enemy);
                FinalizeDiscoverSlot(slot);
                return;
            }

            if (_targetMode != TargetMode.SelectingEnemy) return;

            if (enemyIndex < 0 || enemyIndex >= _enemies.Count) return;
            var enemyTarget = _enemies[enemyIndex];
            if (!enemyTarget.IsAlive) return;

            var targetSlot = _drawSystem.GetSlot(_selectedSlotIndex);
            if (targetSlot == null) return;

            CastImmediately(targetSlot, enemyTarget);
        }

        private void OnPlayerPanelClicked(int playerIndex)
        {
            // 발견 스킬 타겟팅 진행 중
            if (_pendingDiscoverSkill != null && _targetMode == TargetMode.SelectingAlly)
            {
                if (playerIndex < 0 || playerIndex >= _playerParty.Count) return;
                var ally = _playerParty[playerIndex];
                if (!ally.IsAlive) return;

                var slot = _drawSystem.GetSlot(_selectedSlotIndex);
                if (slot == null) return;

                CastDiscoverImmediately(slot, _pendingDiscoverSkill, ally);
                FinalizeDiscoverSlot(slot);
                return;
            }

            if (_targetMode != TargetMode.SelectingAlly) return;

            if (playerIndex < 0 || playerIndex >= _playerParty.Count) return;
            var allyTarget = _playerParty[playerIndex];
            if (!allyTarget.IsAlive) return;

            var targetSlot = _drawSystem.GetSlot(_selectedSlotIndex);
            if (targetSlot == null) return;

            CastImmediately(targetSlot, allyTarget);
        }

        // ── Immediate Cast ──────────────────────────────────────────

        private void CastImmediately(DrawnSkillSlot slot, Character target)
        {
            // AP 부족 체크 — 증강 반영 비용
            int effectiveCost = slot.Instance?.EffectiveCost ?? slot.Skill.Cost;
            if (!_turnManager.Context.CanAfford(effectiveCost))
            {
                _uiManager.AddLog($"AP 부족! (필요: {effectiveCost}, 잔여: {_turnManager.Context.CurrentAP})");
                return;
            }

            // 슬롯 사용 표시
            slot.IsSelected = true;
            slot.AssignedTarget = target;

            // 즉시 시전 — SkillInstance 전달 (증강 효과 반영)
            bool battleEnded = _turnManager.ExecuteSkillImmediately(slot.Caster, slot.Skill, target, slot.Instance);

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
