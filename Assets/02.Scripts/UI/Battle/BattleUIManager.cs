using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Combat.AI;
using TeamLog.Combat.Turn;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 전투 UI 총괄 관리자 (스크린샷 레이아웃 기반)
    /// </summary>
    public class BattleUIManager : MonoBehaviour
    {
        public event Action<int> OnPlayerPanelClickedInternal;
        public event Action<int> OnEnemyPanelClickedInternal;
        [Header("Top Bar")]
        [SerializeField] private TopBarUI _topBar;

        [Header("Left Sidebar - Player Panels")]
        [SerializeField] private Transform _playerPanelContainer;
        [SerializeField] private PlayerSidebarPanel _playerPanelPrefab;
        [SerializeField] private int _maxPlayerPanels = 4;

        [Header("Center - Enemy Panels")]
        [SerializeField] private Transform _enemyPanelContainer;
        [SerializeField] private EnemyDetailPanel _enemyPanelPrefab;

        [Header("Right Sidebar")]
        [SerializeField] private BattleLogUI _battleLog;

        [Header("Bottom Bar")]
        [SerializeField] private TextMeshProUGUI _currentTurnText;
        [SerializeField] private ActionBarUI _actionBar;

        [Header("Character Popup")]
        [SerializeField] private CharacterPopupUI _characterPopup;

        private TurnManager _turnManager;
        private List<PlayerSidebarPanel> _playerPanels = new List<PlayerSidebarPanel>();
        private List<EnemyDetailPanel> _enemyPanels = new List<EnemyDetailPanel>();
        private List<Character> _playerParty;
        private List<Character> _enemies;

        public void Initialize(TurnManager turnManager, List<Character> playerParty, List<Character> enemies)
        {
            _turnManager = turnManager;
            _playerParty = playerParty;
            _enemies = enemies;

            _turnManager.OnPhaseChanged += OnPhaseChanged;
            _turnManager.OnTurnStarted += OnTurnStarted;
            _turnManager.OnAPChanged += OnAPChanged;

            CreatePlayerPanels();
            CreateEnemyPanels();

            AddLog("전투가 시작되었습니다.");
        }

        #region Panel Creation

        private void CreatePlayerPanels()
        {
            ClearPanels(_playerPanels);
            ClearContainerChildren(_playerPanelContainer);

            for (int i = 0; i < _playerParty.Count && i < _maxPlayerPanels; i++)
            {
                var character = _playerParty[i];
                var firstSkill = character.SkillInventory.Skills.Count > 0
                    ? character.SkillInventory.Skills[0] : null;
                string skillName = firstSkill != null ? firstSkill.SkillName : "-";

                var panel = Instantiate(_playerPanelPrefab, _playerPanelContainer);
                panel.Setup(i, character.Name, skillName, character, this);
                panel.UpdateHP(character.Health.CurrentHP, character.Health.MaxHP);
                panel.OnPanelClicked += OnPlayerPanelClicked;
                _playerPanels.Add(panel);
            }
        }

        private void CreateEnemyPanels()
        {
            ClearPanels(_enemyPanels);
            ClearContainerChildren(_enemyPanelContainer);

            foreach (var enemy in _enemies)
            {
                var panel = Instantiate(_enemyPanelPrefab, _enemyPanelContainer);
                // 적 패널 고정 크기 보장 (프리팹에 없을 경우 대비)
                var le = panel.GetComponent<LayoutElement>();
                if (le == null) le = panel.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = 180;
                le.minWidth = 120;
                le.preferredHeight = 280;
                le.minHeight = 200;
                panel.Setup(_enemyPanels.Count, enemy.Name, character: enemy, uiManager: this);
                panel.UpdateHP(enemy.Health.CurrentHP, enemy.Health.MaxHP);
                panel.OnPanelClicked += OnEnemyPanelClicked;
                _enemyPanels.Add(panel);
            }
        }

        private void ClearPanels<T>(List<T> panels) where T : Component
        {
            foreach (var panel in panels)
            {
                if (panel is PlayerSidebarPanel psp)
                    psp.OnPanelClicked -= OnPlayerPanelClicked;
                else if (panel is EnemyDetailPanel edp)
                    edp.OnPanelClicked -= OnEnemyPanelClicked;
                if (panel != null) Destroy(panel.gameObject);
            }
            panels.Clear();
        }

        private void ClearContainerChildren(Transform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        #endregion

        #region Popup

        private void OnPlayerPanelClicked(int index)
        {
            OnPlayerPanelClickedInternal?.Invoke(index);
        }

        private void OnEnemyPanelClicked(int index)
        {
            OnEnemyPanelClickedInternal?.Invoke(index);
        }

        #endregion

        #region Events

        private void OnPhaseChanged(TurnPhase oldPhase, TurnPhase newPhase)
        {
            UpdateAllPanels();

            string phaseText = newPhase switch
            {
                TurnPhase.PlayerAction => "플레이어 액션 페이즈",
                TurnPhase.Execution => "실행 페이즈",
                TurnPhase.EnemyTurn => "적 턴",
                _ => newPhase.ToString()
            };
            AddLog($"페이즈 변경: {phaseText}");
        }

        private void OnTurnStarted(int turnNumber)
        {
            _topBar.SetTurnCounter(turnNumber, _playerParty?.Count ?? 1);
            AddLog($"--- 턴 {turnNumber} 시작 ---");

            if (_currentTurnText != null && _playerParty != null && _playerParty.Count > 0)
            {
                var currentChar = _playerParty[turnNumber % _playerParty.Count];
                _currentTurnText.text = $"{currentChar.Name}, 턴";
            }
        }

        private void OnAPChanged(int current, int max)
        {
            _topBar.SetAP(current, max);
            if (_actionBar != null)
                _actionBar.SetAPState(current);
        }

        #endregion

        #region Public Methods

        public void UpdateAllPanels()
        {
            for (int i = 0; i < _playerPanels.Count && i < _playerParty.Count; i++)
                _playerPanels[i].UpdateHP(_playerParty[i].Health.CurrentHP, _playerParty[i].Health.MaxHP, _playerParty[i].Health.CurrentShield);

            for (int i = 0; i < _enemyPanels.Count && i < _enemies.Count; i++)
                _enemyPanels[i].UpdateHP(_enemies[i].Health.CurrentHP, _enemies[i].Health.MaxHP, _enemies[i].Health.CurrentShield);
        }

        public void AddLog(string message)
        {
            if (_battleLog != null)
                _battleLog.AddLog(message);
        }

        public EnemyDetailPanel GetEnemyPanel(int index)
        {
            return index >= 0 && index < _enemyPanels.Count ? _enemyPanels[index] : null;
        }

        public CharacterPopupUI CharacterPopup => _characterPopup;

        public void HighlightEnemyPanels(bool highlight)
        {
            foreach (var panel in _enemyPanels)
            {
                var enemy = _enemies != null && panel.EnemyIndex < _enemies.Count
                    ? _enemies[panel.EnemyIndex] : null;
                panel.SetTargetMode(highlight && enemy != null && enemy.IsAlive);
            }
        }

        public void HighlightPlayerPanels(bool highlight)
        {
            foreach (var panel in _playerPanels)
            {
                var player = _playerParty != null && panel.PanelIndex < _playerParty.Count
                    ? _playerParty[panel.PanelIndex] : null;
                panel.SetSelected(highlight && player != null && player.IsAlive);
            }
        }

        public void ClearAllHighlights()
        {
            foreach (var panel in _enemyPanels)
                panel.SetTargetMode(false);

            foreach (var panel in _playerPanels)
                panel.SetSelected(false);
        }

        public void SetEnemyIntent(int enemyIndex, EnemyIntent intent)
        {
            var panel = GetEnemyPanel(enemyIndex);
            if (panel != null)
                panel.SetIntent(intent);
        }

        public void UpdateRerollCount(int remaining, int max)
        {
            _topBar?.SetRerollCount(remaining, max);
        }

        #endregion

        private void OnDestroy()
        {
            if (_turnManager != null)
            {
                _turnManager.OnPhaseChanged -= OnPhaseChanged;
                _turnManager.OnTurnStarted -= OnTurnStarted;
                _turnManager.OnAPChanged -= OnAPChanged;
            }
        }
    }
}
