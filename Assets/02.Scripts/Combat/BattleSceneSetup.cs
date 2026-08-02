using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using TeamLog.Characters;
using TeamLog.Combat.Turn;
using SkillExecutor = TeamLog.Combat.Turn.SkillExecutor;
using TeamLog.Combat.AI;
using TeamLog.UI;
using TeamLog.UI.Battle;
using TeamLog.UI.Battle.Direction;  // ★ Phase GF: BattleDirectionController
using TeamLog.Map;
using TeamLog.Meta;
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

        // ★ Phase GF (2026-07-21): 전투 연출 컨트롤러 (Tier S+A+B)
        // InitializeBattle에서 자동 생성. null이면 모든 연출 no-op.
        private BattleDirectionController _directionController;

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
        private static bool _pendingIsBossBattle;  // 어센션 BossHpPercent 적용 트리거

        // 전투 종료 후 돌아갈 씬 이름 — BattleTestSceneSetup이 변경 가능. 기본값 MapScene.
        private static string _returnSceneName = "MapScene";

        /// <summary>
        /// 맵 시스템에서 전투 시작 시 파티와 적 데이터를 설정.
        /// isBossBattle=true면 어센션 BossHpPercent modifier가 추가 적용됨.
        /// </summary>
        public static void SetBattleData(List<Character> party, List<Character> enemies, int bonusAP = 0, bool isBossBattle = false)
        {
            _pendingParty = party;
            _pendingEnemies = enemies;
            _pendingBonusAP = bonusAP;
            _pendingIsBossBattle = isBossBattle;
        }

        /// <summary>
        /// 전투 종료 후 돌아갈 씬 이름 설정. 기본값 "MapScene".
        /// BattleTestScene 등 다른 씬에서 전투를 시작할 때 사용.
        /// </summary>
        public static void SetReturnScene(string sceneName)
        {
            _returnSceneName = string.IsNullOrEmpty(sceneName) ? "MapScene" : sceneName;
        }

        private void Start()
        {
            int bonusAP = 0;
            bool isBossBattle = false;

            // 외부 데이터가 있으면 사용, 없으면 테스트 모드
            if (_pendingParty != null && _pendingEnemies != null)
            {
                _playerParty = new List<Character>(_pendingParty);
                _enemies = new List<Character>(_pendingEnemies);
                bonusAP = _pendingBonusAP;
                isBossBattle = _pendingIsBossBattle;
                _pendingParty = null;
                _pendingEnemies = null;
                _pendingBonusAP = 0;
                _pendingIsBossBattle = false;
            }
            else if (_useTestData)
            {
                CreateTestData();
            }

            // 어센션 modifier 적용 — GameRunState.SelectedAscensionLevel 기반.
            // 시뮬레이터/BattleTestScene(런 미진행)은 SelectedAscensionLevel=0 → no-op.
            ApplyAscensionModifiers(isBossBattle);

            InitializeBattle(bonusAP);
        }

        /// <summary>
        /// 어센션 modifier를 적 전투 인스턴스에 적용.
        /// - 적 HP: 모든 적 (mul)
        /// - 보스 HP: isBossBattle=true일 때 추가 mul
        /// 리롤 delta는 InitializeBattle의 maxRerolls 계산에서 적용.
        /// 참고: EnemyAtkPercent는 제거됨 — 시스템 전체 ATK=0 구조에서 무의미 (2026-06-30).
        /// </summary>
        private void ApplyAscensionModifiers(bool isBossBattle)
        {
            var state = GameRunState.Instance;
            if (state == null) return;
            int asc = state.SelectedAscensionLevel;
            if (asc <= 0) return;

            float hpMul = AscensionManager.GetEnemyHpMulByLevel(asc);
            float bossMul = isBossBattle ? AscensionManager.GetBossHpMulByLevel(asc) : 1f;

            foreach (var enemy in _enemies)
            {
                if (enemy == null) continue;
                // HP — Initialize 재호출로 MaxHP 재설정. 현재 HP도 같이 스케일.
                int scaledMax = System.Math.Max(1, (int)(enemy.Health.MaxHP * hpMul * bossMul));
                enemy.Health.Initialize(scaledMax);
            }
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
                // ★ Phase GF (2026-07-22): SingleAlly 스킬이 자신 또는 랜덤 아군 선택하도록 아군 리스트 주입.
                controller.SetAllies(_enemies);
                int index = i; // 클로저 캡처
                controller.OnIntentChanged += intent => OnEnemyIntentChanged(index, intent);
                _enemyControllers.Add(controller);
            }

            // TurnManager 생성 — AI 컨트롤러 전달
            // Phase 8E: 메타 강화 ExtraReroll 구매 시 턴당 리롤 +1
            // Ascension: 런 선택 어센션 레벨에 따라 리롤 -1~-3 (최소 1 보장)
            int extraReroll = MetaProgressionManager.GetExtraRerollCount(SaveManager.Meta);
            int ascRerollDelta = 0;
            var runState = GameRunState.Instance;
            if (runState != null)
                ascRerollDelta = AscensionManager.GetRerollDeltaByLevel(runState.SelectedAscensionLevel);
            int maxRerolls = System.Math.Max(1, 2 + extraReroll + ascRerollDelta);
            _turnManager = new TurnManager(_playerParty, _enemies, _enemyControllers, maxRerolls: maxRerolls, bonusFirstTurnAP: bonusAP);
            _turnManager.OnPhaseChanged += OnPhaseChanged;
            _turnManager.OnTurnStarted += OnTurnStarted;
            _turnManager.OnBattleEnded += OnBattleEnded;
            _turnManager.OnEnemyTurnSequenceStarted += OnEnemyTurnSequenceStarted;
            _turnManager.OnEnemyActing += OnEnemyActing;
            // 순차 적 턴 모드 — 코루틴 주도로 적을 한 명씩 행동시켜 시각적 인지 향상
            _turnManager.EnableSequentialEnemyTurn();

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

            // ★ 2026-08-02 P1-4: 크리티컬 히트 시 금색 CRIT! 플로팅 텍스트
            DamageCalculator.OnCriticalHit += HandleCriticalHit;

            // 유물 이벤트 구독 — 전투 시작 전에 연결
            var relicHandler = GameRunState.Instance?.RelicHandler;
            if (relicHandler != null)
            {
                relicHandler.SetPlayerParty(_playerParty);
                relicHandler.SubscribeEvents();
            }

            // ★ Phase GF (2026-07-21): 전투 연출 컨트롤러 초기화 — StartBattle "이전"에 호출해야
            // 첫 드로우(StartBattle 안에서 발생) 시점에 DirectionController가 준비됨.
            InitializeDirectionController();

            // 전투 시작
            _turnManager.StartBattle();

            // 유물 바 새로고침
            if (_relicBarUI != null)
                _relicBarUI.Refresh();

            // 전투 시작 타이틀
            if (_titleManager != null)
                _titleManager.ShowBattleStart();

            // Phase GF (2026-07-20): 보스 페이즈 관리자 초기화 — 보스 있으면 임계값(75%/50%) 모니터링 시작
            InitializeBossPhaseManager();

            // ★ 2026-08-02 P0-3: 첫 전투 시 튜토리얼 가이드 활성화 (BattlesWon==0 && Floor==1)
            // BattleTestScene(_useTestData=true)에서는 스킵 — 매 전투마다 튜토리얼 뜨는 것 방지
            if (!_useTestData)
                BattleTutorialGuide.TryActivate(_mainCanvasRect);
        }

        /// <summary>★ Phase GF: 전투 연출 컨트롤러 생성 및 초기화.</summary>
        private void InitializeDirectionController()
        {
            if (_mainCanvasRect == null) return;

            // 기존 컴포넌트가 있으면 재사용 (씬에서 인스펙터 할당한 경우)
            if (_directionController == null)
                _directionController = GetComponentInChildren<BattleDirectionController>(true);

            if (_directionController == null)
            {
                var go = new GameObject("BattleDirection");
                go.transform.SetParent(transform);
                _directionController = go.AddComponent<BattleDirectionController>();
            }

            // 한국어 폰트는 BattleTitleManager에서 가져오거나 null 허용
            TMP_FontAsset koreanFont = null;
            if (_titleManager != null)
            {
                var field = typeof(BattleTitleManager).GetField("_koreanFont",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                    koreanFont = field.GetValue(_titleManager) as TMP_FontAsset;
            }

            _directionController.Initialize(_mainCanvasRect, _battleUIManager, koreanFont);

            // ★ ActionBarUI에 DirectionController 주입 (S2 순차 등장 트리거용)
            if (_actionBar != null)
                _actionBar.SetDirectionController(_directionController);
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
