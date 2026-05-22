using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Combat.Turn;
using TeamLog.Combat.AI;
using TeamLog.UI.Battle;

namespace TeamLog.Combat
{
    /// <summary>
    /// 전투 씬 초기화 - UI 초기화, 테스트 데이터 생성, 시스템 연결
    /// </summary>
    public class BattleSceneSetup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private BattleUIManager _battleUIManager;
        [SerializeField] private ActionBarUI _actionBar;

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

        // 외부 데이터 주입용 (맵 시스템에서 전투 시작 시 사용)
        private static List<Character> _pendingParty;
        private static List<Character> _pendingEnemies;

        /// <summary>
        /// 맵 시스템에서 전투 시작 시 파티와 적 데이터를 설정
        /// </summary>
        public static void SetBattleData(List<Character> party, List<Character> enemies)
        {
            _pendingParty = party;
            _pendingEnemies = enemies;
        }

        private void Start()
        {
            // 외부 데이터가 있으면 사용, 없으면 테스트 모드
            if (_pendingParty != null && _pendingEnemies != null)
            {
                _playerParty = new List<Character>(_pendingParty);
                _enemies = new List<Character>(_pendingEnemies);
                _pendingParty = null;
                _pendingEnemies = null;
            }
            else if (_useTestData)
            {
                CreateTestData();
            }

            InitializeBattle();
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

        private void InitializeBattle()
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
            _turnManager = new TurnManager(_playerParty, _enemies, _enemyControllers, maxRerolls: 2);
            _turnManager.OnPhaseChanged += OnPhaseChanged;
            _turnManager.OnTurnStarted += OnTurnStarted;
            _turnManager.OnBattleEnded += OnBattleEnded;

            // ActionBar 초기화
            if (_actionBar != null)
                _actionBar.Initialize(_turnManager);

            // BattleUIManager 초기화
            if (_battleUIManager != null)
                _battleUIManager.Initialize(_turnManager, _playerParty, _enemies);

            // PlayerActionController 생성 및 연결
            _actionController = new PlayerActionController(
                _turnManager, _actionBar, _battleUIManager, _playerParty, _enemies);
            _actionController.Initialize();

            // HP/쉴드 변경 이벤트 구독
            foreach (var c in _playerParty)
            {
                c.Health.OnHPChanged += (hp, max) => OnCharacterHealthChanged(c);
                c.Health.OnShieldChanged += (shield) => OnCharacterHealthChanged(c);
            }
            foreach (var c in _enemies)
            {
                c.Health.OnHPChanged += (hp, max) => OnCharacterHealthChanged(c);
                c.Health.OnShieldChanged += (shield) => OnCharacterHealthChanged(c);
            }

            // 전투 시작
            _turnManager.StartBattle();
        }

        #region Events

        private void OnPhaseChanged(TurnPhase oldPhase, TurnPhase newPhase)
        {
            _battleUIManager?.UpdateAllPanels();
        }

        private void OnTurnStarted(int turnNumber)
        {
            _battleUIManager?.UpdateAllPanels();

            foreach (var controller in _enemyControllers)
            {
                if (controller.Owner.IsAlive)
                    controller.PrepareNextAction();
                else
                {
                    int idx = _enemyControllers.IndexOf(controller);
                    _battleUIManager?.SetEnemyIntent(idx, null);
                }
            }
        }

        private void OnBattleEnded()
        {
            _actionController?.Shutdown();
            _battleUIManager?.UpdateAllPanels();

            bool victory = _enemies.TrueForAll(e => e.IsDead);
            _battleUIManager?.AddLog(victory ? "전투 승리!" : "전투 패배...");

            BattleResult.SetResult(victory);

            StartCoroutine(BattleEndTransition());
        }

        private IEnumerator BattleEndTransition()
        {
            _battleUIManager?.AddLog("3초 후 맵으로 돌아갑니다...");
            yield return new WaitForSecondsRealtime(3f);
            SceneManager.LoadScene("MapScene");
        }

        private void OnCharacterHealthChanged(Character character)
        {
            _battleUIManager?.UpdateAllPanels();

            if (character.Health.IsDead)
            {
                _battleUIManager?.AddLog($"{character.Name}이(가) 쓰러졌습니다.");

                // 적 사망 시 의도 텍스트 제거
                int enemyIdx = _enemies.IndexOf(character);
                if (enemyIdx >= 0)
                    _battleUIManager?.SetEnemyIntent(enemyIdx, null);
            }
        }

        private EnemyActionPattern LoadEnemyPattern(int enemyIndex, Character enemy)
        {
            // 1. 인스펙터에 할당된 패턴 배열에서 매칭 시도
            if (_enemyPatternData != null && enemyIndex < _enemyPatternData.Length && _enemyPatternData[enemyIndex] != null)
                return _enemyPatternData[enemyIndex].CreateRuntimePattern();

            // 2. 캐릭터 Data의 에셋 이름으로 패턴 에셋 자동 탐색 (에디터 전용)
#if UNITY_EDITOR
            string assetName = enemy.Data != null ? enemy.Data.name : "";
            if (!string.IsNullOrEmpty(assetName))
            {
                var patternAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyPatternData>(
                    $"Assets/03.Data/Patterns/Pattern_{assetName}.asset");
                if (patternAsset != null)
                    return patternAsset.CreateRuntimePattern();
            }
#endif

            // 3. 폴백: 캐릭터 Data의 스킬 목록에서 패턴 생성
            if (enemy.Data != null && enemy.Data.Skills.Count > 0)
                return new EnemyActionPattern(enemy.Data.Skills);

            // 4. 최종 폴백: 빈 패턴 (아무 행동도 안 함)
            return new EnemyActionPattern(new Characters.SkillData[0]);
        }

        private void OnEnemyIntentChanged(int enemyIndex, EnemyIntent intent)
        {
            _battleUIManager?.SetEnemyIntent(enemyIndex, intent);
        }

        #endregion

        private void OnDestroy()
        {
            if (_turnManager != null)
            {
                _turnManager.OnPhaseChanged -= OnPhaseChanged;
                _turnManager.OnTurnStarted -= OnTurnStarted;
                _turnManager.OnBattleEnded -= OnBattleEnded;
            }

            _actionController?.Shutdown();
            _playerParty.Clear();
            _enemies.Clear();
        }
    }
}
