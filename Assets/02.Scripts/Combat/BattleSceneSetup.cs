using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Combat.Turn;
using SkillExecutor = TeamLog.Combat.Turn.SkillExecutor;
using TeamLog.Combat.AI;
using TeamLog.UI;
using TeamLog.UI.Battle;
using TeamLog.Map;
using TeamLog.Reward;

namespace TeamLog.Combat
{
    /// <summary>
    /// 전투 씬 초기화 - UI 초기화, 테스트 데이터 생성, 시스템 연결
    /// 이벤트 핸들러/사운드/VFX: BattleSceneSetup.Events.cs
    /// </summary>
    public partial class BattleSceneSetup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private BattleUIManager _battleUIManager;
        [SerializeField] private ActionBarUI _actionBar;
        [SerializeField] private BattleEndOverlay _battleEndOverlay;
        [SerializeField] private RectTransform _mainCanvasRect;
        [SerializeField] private BattleRelicBarUI _relicBarUI;
        [SerializeField] private BattleTitleManager _titleManager;

        [Header("Test Mode")]
        [SerializeField] private bool _useTestData = true;

        [Header("Test Party Data")]
        [SerializeField] private CharacterData _testWarriorData;
        [SerializeField] private CharacterData _testMageData;
        [SerializeField] private CharacterData _testHealerData;
        [SerializeField] private CharacterData _testRogueData;

        [Header("Test Enemy Data")]
        [SerializeField] private CharacterData[] _testEnemyData;

        [Header("Enemy Pattern Data")]
        [SerializeField] private EnemyPatternData[] _enemyPatternData;

        private TurnManager _turnManager;
        private PlayerActionController _actionController;
        private List<EnemyAIController> _enemyControllers;
        private List<Character> _playerParty = new();
        private List<Character> _enemies = new();
        private VFXManager _vfxManager;
        private BattleScreenFlash _screenFlash;

        // 전투 속도
        public enum BattleSpeed { Normal = 1, Fast = 2 }
        private BattleSpeed _currentSpeed = BattleSpeed.Normal;
        public BattleSpeed CurrentBattleSpeed => _currentSpeed;

        public event System.Action<BattleSpeed> OnBattleSpeedChanged;

        // 외부 데이터 주입용 (맵 시스템에서 전투 시작 시 사용)
        private static List<Character> _pendingParty;
        private static List<Character> _pendingEnemies;
        private static int _pendingBonusAP;

        /// <summary>
        /// 맵 시스템에서 전투 시작 시 파티와 적 데이터를 설정
        /// </summary>
        public static void SetBattleData(List<Character> party, List<Character> enemies, int bonusAP = 0)
        {
            _pendingParty = party;
            _pendingEnemies = enemies;
            _pendingBonusAP = bonusAP;
        }

        private void Start()
        {
            int bonusAP = 0;

            // 외부 데이터가 있으면 사용, 없으면 테스트 모드
            if (_pendingParty != null && _pendingEnemies != null)
            {
                _playerParty = new List<Character>(_pendingParty);
                _enemies = new List<Character>(_pendingEnemies);
                bonusAP = _pendingBonusAP;
                _pendingParty = null;
                _pendingEnemies = null;
                _pendingBonusAP = 0;
            }
            else if (_useTestData)
            {
                CreateTestData();
            }

            InitializeBattle(bonusAP);
        }

        private void CreateTestData()
        {
            // 파티 생성 - 에셋이 있으면 사용, 없으면 기본값
            AddPartyMember(_testWarriorData, "전사", CharacterClass.Warrior, 20, 1, 0);
            AddPartyMember(_testMageData, "마법사", CharacterClass.Mage, 12, 2, 0);
            AddPartyMember(_testHealerData, "힐러", CharacterClass.Healer, 14, 1, 0);
            AddPartyMember(_testRogueData, "도적", CharacterClass.Rogue, 13, 2, 0);

            // 적 생성 - 에셋 배열이 있으면 사용, 없으면 기본 고블린 2마리
            if (_testEnemyData != null && _testEnemyData.Length > 0)
            {
                foreach (var data in _testEnemyData)
                    _enemies.Add(new Character(data));
            }
            else
            {
                _enemies.Add(CreateDefaultCharacter("슬라임1", CharacterClass.Warrior, 10, 1, 0));
                _enemies.Add(CreateDefaultCharacter("고블린1", CharacterClass.Rogue, 12, 2, 0));
            }
        }

        private void AddPartyMember(CharacterData data, string fallbackName,
            CharacterClass fallbackClass, int hp, int atk, int def)
        {
            if (data != null)
                _playerParty.Add(new Character(data));
            else
                _playerParty.Add(CreateDefaultCharacter(fallbackName, fallbackClass, hp, atk, def));
        }

        private Character CreateDefaultCharacter(string name, CharacterClass charClass, int hp, int atk, int def)
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            var character = new Character(data);
            character.Health.Initialize(hp);
            character.Stats.Initialize(atk, def);
            return character;
        }

        private void InitializeBattle(int bonusAP = 0)
        {
            if (_playerParty.Count == 0 || _enemies.Count == 0)
            {
                Debug.LogError("[BattleSceneSetup] 파티/적 데이터가 없습니다!");
                return;
            }

            // 적 AI 컨트롤러 생성 — EnemyPatternData 에셋에서 패턴 로드
            _enemyControllers = new List<EnemyAIController>();
            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                var pattern = LoadEnemyPattern(i, enemy);
                var controller = new EnemyAIController(enemy, pattern, _playerParty);
                int index = i; // 클로저 캡처
                controller.OnIntentChanged += intent => OnEnemyIntentChanged(index, intent);
                _enemyControllers.Add(controller);
            }

            // TurnManager 생성 — AI 컨트롤러 전달
            _turnManager = new TurnManager(_playerParty, _enemies, _enemyControllers, maxRerolls: 2, bonusFirstTurnAP: bonusAP);
            _turnManager.OnPhaseChanged += OnPhaseChanged;
            _turnManager.OnTurnStarted += OnTurnStarted;
            _turnManager.OnBattleEnded += OnBattleEnded;

            // ActionBar 초기화
            if (_actionBar != null)
                _actionBar.Initialize(_turnManager);

            // BattleUIManager 초기화
            if (_battleUIManager != null)
                _battleUIManager.Initialize(_turnManager, _playerParty, _enemies, this);

            // PlayerActionController 생성 및 연결
            _actionController = new PlayerActionController(
                _turnManager, _actionBar, _battleUIManager, _playerParty, _enemies);
            _actionController.Initialize();

            // VFXManager 초기화 — BattleUICanvas 아래에 VFX Canvas 생성
            if (_mainCanvasRect != null)
            {
                var vfxGO = new GameObject("VFXManager");
                vfxGO.transform.SetParent(transform);
                _vfxManager = vfxGO.AddComponent<VFXManager>();
                _vfxManager.Initialize(_mainCanvasRect);

                // 화면 플래시 초기화 — 크리티컬 히트 시 순간 점멸
                var flashGO = new GameObject("BattleScreenFlash");
                flashGO.transform.SetParent(transform);
                _screenFlash = flashGO.AddComponent<BattleScreenFlash>();
                _screenFlash.Initialize(_mainCanvasRect);
            }

            // HP/쉴드 변경 이벤트 구독 — 플레이어/적 공통 헬퍼 사용
            foreach (var c in _playerParty)
                SubscribeCharacterEvents(c);
            foreach (var c in _enemies)
                SubscribeCharacterEvents(c);

            // 스킬 타입별 사운드 분기
            SkillExecutor.OnSkillApplied += OnSkillApplied;

            // 드로우/리롤 사운드
            _turnManager.DrawSystem.OnDrawComplete += _ => AudioManager.Instance.PlaySkillDraw();
            _turnManager.DrawSystem.OnSlotRerolled += () => AudioManager.Instance.PlaySkillReroll();

            // 턴 시작 사운드 (별도 람다 — OnTurnStarted는 이미 명명 메서드로 구독 중)
            _turnManager.OnTurnStarted += _ => AudioManager.Instance.PlayTurnStart();

            // 특성: 회피 시 MISS 플로팅 텍스트 + 사운드
            DamageCalculator.OnAttackMissed += OnAttackMissed;

            // 유물 이벤트 구독 — 전투 시작 전에 연결
            var relicHandler = GameRunState.Instance?.RelicHandler;
            if (relicHandler != null)
            {
                relicHandler.SetPlayerParty(_playerParty);
                relicHandler.SubscribeEvents();
            }

            // 전투 시작
            _turnManager.StartBattle();

            // 유물 바 새로고침
            if (_relicBarUI != null)
                _relicBarUI.Refresh();

            // 전투 시작 타이틀
            if (_titleManager != null)
                _titleManager.ShowBattleStart();
        }

        /// <summary>
        /// 전투 속도 토글 — Normal(1x) ↔ Fast(2x)
        /// </summary>
        public void ToggleBattleSpeed()
        {
            _currentSpeed = _currentSpeed == BattleSpeed.Normal ? BattleSpeed.Fast : BattleSpeed.Normal;
            Time.timeScale = (int)_currentSpeed;
            OnBattleSpeedChanged?.Invoke(_currentSpeed);
        }
    }
}
