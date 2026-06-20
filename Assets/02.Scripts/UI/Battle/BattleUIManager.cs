using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Combat.AI;
using TeamLog.Combat.Turn;
using TeamLog.Combat;

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

        [Header("Player Strip")]
        [SerializeField] private Transform _playerPanelContainer;
        [SerializeField] private PlayerSidebarPanel _playerPanelPrefab;
        [SerializeField] private int _maxPlayerPanels = 4;

        [Header("Center - Enemy Panels")]
        [SerializeField] private Transform _enemyPanelContainer;
        [SerializeField] private EnemyDetailPanel _enemyPanelPrefab;

        [Header("Right Sidebar")]
        [SerializeField] private BattleLogUI _battleLog;

        [Header("Bottom Bar")]
        [SerializeField] private ActionBarUI _actionBar;

        [Header("Character Popup")]
        [SerializeField] private CharacterPopupUI _characterPopup;

        private TurnManager _turnManager;
        private List<PlayerSidebarPanel> _playerPanels = new List<PlayerSidebarPanel>();
        private List<EnemyDetailPanel> _enemyPanels = new List<EnemyDetailPanel>();
        private List<Character> _playerParty;
        private List<Character> _enemies;

        public void Initialize(TurnManager turnManager, List<Character> playerParty, List<Character> enemies,
            BattleSceneSetup battleSetup = null)
        {
            _turnManager = turnManager;
            _playerParty = playerParty;
            _enemies = enemies;

            _turnManager.OnPhaseChanged += OnPhaseChanged;
            _turnManager.OnTurnStarted += OnTurnStarted;
            _turnManager.OnAPChanged += OnAPChanged;

            CreatePlayerPanels();
            CreateEnemyPanels();

            // TopBar 속도 토글 초기화
            if (_topBar != null && battleSetup != null)
                _topBar.Initialize(battleSetup);

            AddLog("전투가 시작되었습니다.");
        }

        #region Panel Creation

        private void CreatePlayerPanels()
        {
            ClearPanels(_playerPanels);
            UIAnimationHelper.ClearContainerChildren(_playerPanelContainer);

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

        private const int ENEMIES_PER_ROW = 5;

        private void CreateEnemyPanels()
        {
            ClearPanels(_enemyPanels);

            if (_enemyPanelContainer == null)
            {
                Debug.LogError("[BattleUIManager] _enemyPanelContainer is null! Cannot create enemy panels.");
                return;
            }
            UIAnimationHelper.ClearContainerChildren(_enemyPanelContainer);

            int count = _enemies.Count;
            bool multiRow = count > ENEMIES_PER_ROW;

            if (multiRow)
                SetupMultiRowContainer(count);
            else
                SetupSingleRowContainer();

            float panelWidth = multiRow ? 120f : 180f;
            float panelHeight = multiRow ? 280f : 320f;

            foreach (var enemy in _enemies)
            {
                var panel = Instantiate(_enemyPanelPrefab, _enemyPanelContainer);
                var le = panel.GetComponent<LayoutElement>();
                if (le == null) le = panel.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = panelWidth;
                le.minWidth = panelWidth;
                le.preferredHeight = panelHeight;
                le.minHeight = panelHeight;
                le.flexibleWidth = 0;
                panel.Setup(_enemyPanels.Count, enemy.Name, character: enemy, uiManager: this);
                panel.UpdateHP(enemy.Health.CurrentHP, enemy.Health.MaxHP);
                panel.OnPanelClicked += OnEnemyPanelClicked;
                _enemyPanels.Add(panel);
            }
        }

        private void SetupSingleRowContainer()
        {
            // 기존 레이아웃 유지 (HorizontalLayoutGroup)
            var existing = _enemyPanelContainer.GetComponent<HorizontalLayoutGroup>();
            if (existing == null)
            {
                var hlg = _enemyPanelContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 12;
                hlg.padding = new RectOffset(12, 12, 12, 12);
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
            }

            // GridLayoutGroup이 있으면 즉시 제거
            var grid = _enemyPanelContainer.GetComponent<GridLayoutGroup>();
            if (grid != null) DestroyImmediate(grid);
        }

        private void SetupMultiRowContainer(int count)
        {
            // HorizontalLayoutGroup 즉시 제거
            var hlg = _enemyPanelContainer.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null) DestroyImmediate(hlg);

            // GridLayoutGroup으로 2줄 배치
            var grid = _enemyPanelContainer.GetComponent<GridLayoutGroup>();
            if (grid == null) grid = _enemyPanelContainer.gameObject.AddComponent<GridLayoutGroup>();

            int columns = Mathf.CeilToInt(count / 2f);
            if (columns > ENEMIES_PER_ROW) columns = ENEMIES_PER_ROW;

            grid.cellSize = new Vector2(120f, 280f);
            grid.spacing = new Vector2(10, 28);
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
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
            AddLog($"--- 턴 {turnNumber} 시작 ---");
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
            {
                var c = _playerParty[i];
                _playerPanels[i].UpdateHP(c.Health.CurrentHP, c.Health.MaxHP, c.Health.CurrentShield);
                _playerPanels[i].SetDead(c.IsDead);
                _playerPanels[i].UpdateStats(c.Stats.GetStat(StatType.ATK), c.Stats.GetStat(StatType.DEF));
                _playerPanels[i].UpdateStatusEffects(c.StatusEffects.GetAllEffects());
            }

            for (int i = 0; i < _enemyPanels.Count && i < _enemies.Count; i++)
            {
                var e = _enemies[i];
                _enemyPanels[i].UpdateHP(e.Health.CurrentHP, e.Health.MaxHP, e.Health.CurrentShield);
                _enemyPanels[i].SetDead(e.IsDead);
                _enemyPanels[i].UpdateStats(e.Stats.GetStat(StatType.ATK), e.Stats.GetStat(StatType.DEF));
                _enemyPanels[i].UpdateStatusEffects(e.StatusEffects.GetAllEffects());
            }
        }

        public void AddLog(string message, LogEntryType type = LogEntryType.System)
        {
            if (_battleLog != null)
                _battleLog.AddLog(message, type);
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

        /// <summary>순차 적 턴 — 행동 중인 적 패널 하이라이트.</summary>
        public void HighlightActingEnemy(Character enemy)
        {
            var panel = GetEnemyPanelFor(enemy);
            if (panel != null) panel.HighlightActing();
        }

        /// <summary>순차 적 턴 — 행동 종료 후 하이라이트 해제.</summary>
        public void ClearActingEnemyHighlight(Character enemy)
        {
            var panel = GetEnemyPanelFor(enemy);
            if (panel != null) panel.ClearActingHighlight();
        }

        /// <summary>순차 적 턴 — 행동을 마친 적의 의도를 즉시 비움.</summary>
        public void ClearEnemyIntentFor(Character enemy)
        {
            if (_enemyPanels == null) return;
            int idx = _enemyPanels.FindIndex(p => p != null && p.Target == enemy);
            if (idx >= 0) SetEnemyIntent(idx, null);
        }

        private EnemyDetailPanel GetEnemyPanelFor(Character enemy)
        {
            if (_enemyPanels == null) return null;
            foreach (var p in _enemyPanels)
                if (p != null && p.Target == enemy) return p;
            return null;
        }

        public void UpdateRerollCount(int remaining, int max)
        {
            _actionBar?.SetRerollState(remaining, max);
        }

        public void ShowTraitInfo(string title, string description)
        {
            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Show(title, description);
        }

        public void HideTraitInfo()
        {
            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Hide();
        }

        public void FlashPanelForCharacter(Character character)
        {
            if (_playerParty != null)
            {
                int idx = _playerParty.IndexOf(character);
                if (idx >= 0 && idx < _playerPanels.Count)
                {
                    _playerPanels[idx].FlashHit();
                    return;
                }
            }
            if (_enemies != null)
            {
                int idx = _enemies.IndexOf(character);
                if (idx >= 0 && idx < _enemyPanels.Count)
                    _enemyPanels[idx].FlashHit();
            }
        }

        public Transform GetPanelTransform(Character character)
        {
            if (_playerParty != null)
            {
                int idx = _playerParty.IndexOf(character);
                if (idx >= 0 && idx < _playerPanels.Count)
                    return _playerPanels[idx].transform;
            }
            if (_enemies != null)
            {
                int idx = _enemies.IndexOf(character);
                if (idx >= 0 && idx < _enemyPanels.Count)
                    return _enemyPanels[idx].transform;
            }
            return null;
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
