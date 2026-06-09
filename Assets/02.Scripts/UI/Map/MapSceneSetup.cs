using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Event;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.Skill;
using TeamLog.UI;
using TeamLog.UI.Event;
using TeamLog.UI.Reward;
using TeamLog.UI.Shop;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 층별 적 풀 — 각 층의 일반/엘리트/보스 적 데이터를 보관
    /// </summary>
    [Serializable]
    public class FloorEnemyPool
    {
        public CharacterData[] normalEnemies;
        public CharacterData[] eliteEnemies;
        public CharacterData boss;
    }

    /// <summary>
    /// 맵 씬의 진입점 — 맵 UI, GameRunState, 노드 이벤트 처리를 연결
    /// 노드 디스패치: MapSceneSetup.Nodes.cs
    /// </summary>
    public partial class MapSceneSetup : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private MapView _mapView;
        [SerializeField] private EventUI _eventUI;
        [SerializeField] private ShopUI _shopUI;
        [SerializeField] private RewardUI _rewardUI;
        [SerializeField] private ConfirmationDialog _confirmationDialog;
        [SerializeField] private RestUI _restUI;
        [SerializeField] private RunEndOverlay _runEndOverlay;
        [SerializeField] private RelicBarUI _relicBarUI;
        [SerializeField] private DeckViewerUI _deckViewerUI;
        [SerializeField] private Button _deckButton;
        [SerializeField] private TutorialUI _tutorialUI;
        [SerializeField] private CharacterSelectUI _characterSelectUI;

        [Header("All Characters")]
        [SerializeField] private CharacterData[] _allCharacters;

        [Header("Test Mode")]
        [SerializeField] private bool _useTestData = true;
        [SerializeField] private CharacterData _testWarriorData;
        [SerializeField] private CharacterData _testMageData;
        [SerializeField] private CharacterData _testHealerData;
        [SerializeField] private CharacterData _testRogueData;

        [Header("Test Events")]
        [SerializeField] private EventData[] _testEvents;

        [Header("Floor-based Enemy Pools")]
        [SerializeField] private FloorEnemyPool[] _floorPools;

        [Header("Spawn Pattern Tables (per floor)")]
        [SerializeField] private SpawnPatternTable[] _spawnPatternTables;

        [Header("Data Pools")]
        [SerializeField] private RelicData[] _relicPool;
        [SerializeField] private AugmentData[] _augmentPool;

        private GameRunState _runState;
        private List<Character> _playerParty;
        private MapNode _pendingBattleNode;

        private const string BattleSceneName = "BattleScene";

        private void Start()
        {
            // RunEndOverlay 이벤트 연결
            if (_runEndOverlay != null)
                _runEndOverlay.OnReturnToTitle += OnReturnToTitle;

            if (GameRunState.Instance != null)
            {
                // 전투 씬에서 복귀
                RestoreExistingRun();
            }
            else if (SaveManager.HasSave)
            {
                // 타이틀에서 이어하기 — 세이브 파일에서 복원
                ContinueFromSave();
            }
            else
            {
                // 새 런 시작 — 캐릭터 선택
                if (_characterSelectUI != null && _allCharacters != null && _allCharacters.Length > 0)
                {
                    _characterSelectUI.Initialize(_allCharacters, SaveManager.Meta, OnCharacterSelectConfirmed);
                    _characterSelectUI.Show();
                }
                else
                {
                    InitializeTestRun();
                }
            }
        }

        /// <summary>
        /// 캐릭터 선택 완료 → 런 시작
        /// </summary>
        private void OnCharacterSelectConfirmed(List<CharacterData> selectedCharacters)
        {
            _playerParty = new List<Character>();
            foreach (var data in selectedCharacters)
                _playerParty.Add(new Character(data));

            StartRunWithParty();
        }

        private void InitializeTestRun()
        {
            _playerParty = new List<Character>();

            var testData = new[]
            {
                _testWarriorData, _testMageData, _testHealerData, _testRogueData
            };

            foreach (var data in testData)
            {
                if (data != null)
                    _playerParty.Add(new Character(data));
            }

            StartRunWithParty();
        }

        /// <summary>
        /// 파티가 확정된 후 런 초기화
        /// </summary>
        private void StartRunWithParty()
        {
            _runState = GameRunState.Create(_playerParty, startingGold: 50);
            _runState.OnMapChanged += OnMapChanged;
            _runState.OnRunEnded += OnRunEnded;
            _runState.SetDataPools(
                _relicPool != null ? new List<RelicData>(_relicPool) : new List<RelicData>(),
                _augmentPool != null ? new List<AugmentData>(_augmentPool) : new List<AugmentData>());

            InitializeSubUIs();

            _runState.StartRun();

            // 메타 데이터에 런 시작 기록
            var meta = SaveManager.Meta;
            meta.HasPendingRun = true;
            SaveManager.SaveMeta();
        }

        /// <summary>
        /// 씬 복귀 시 기존 런 인스턴스 복원
        /// </summary>
        private void RestoreExistingRun()
        {
            _runState = GameRunState.Instance;
            _playerParty = new List<Character>(_runState.PlayerParty);

            // 이벤트 재구독
            _runState.OnMapChanged += OnMapChanged;
            _runState.OnRunEnded += OnRunEnded;

            // 서브 UI 재초기화
            InitializeSubUIs();

            // MapView 복원
            if (_mapView != null && _runState.CurrentMap != null)
                _mapView.Initialize(_runState.CurrentMap, _runState.Gold, OnNodeClicked);

            // 전투 결과 처리
            if (BattleResult.HasPendingResult)
            {
                HandleBattleResult();
            }
        }

        /// <summary>
        /// 타이틀에서 이어하기 — 세이브 파일에서 런 복원
        /// </summary>
        private void ContinueFromSave()
        {
            _runState = SaveManager.Load();
            if (_runState == null)
            {
                if (_characterSelectUI != null && _allCharacters != null && _allCharacters.Length > 0)
                {
                    _characterSelectUI.Initialize(_allCharacters, SaveManager.Meta, OnCharacterSelectConfirmed);
                    _characterSelectUI.Show();
                }
                else
                {
                    InitializeTestRun();
                }
                return;
            }

            _playerParty = new List<Character>(_runState.PlayerParty);
            _runState.OnMapChanged += OnMapChanged;
            _runState.OnRunEnded += OnRunEnded;
            _runState.SetDataPools(
                _relicPool != null ? new List<RelicData>(_relicPool) : new List<RelicData>(),
                _augmentPool != null ? new List<AugmentData>(_augmentPool) : new List<AugmentData>());

            InitializeSubUIs();

            // 현재 층 맵 재생성
            _runState.GenerateCurrentFloorMap();

            if (_mapView != null && _runState.CurrentMap != null)
                _mapView.Initialize(_runState.CurrentMap, _runState.Gold, OnNodeClicked);

            // 메타 데이터에 대기 중 런 표시
            var meta = SaveManager.Meta;
            meta.HasPendingRun = true;
            SaveManager.SaveMeta();
        }

        private void InitializeSubUIs()
        {
            if (_eventUI != null)
                _eventUI.Initialize(_runState, OnEventComplete);
            if (_shopUI != null)
            {
                _shopUI.Initialize(_runState, OnShopExit, _relicPool);
                _shopUI.SetAugmentPool(_augmentPool);
            }
            if (_rewardUI != null)
                _rewardUI.Initialize(_runState, OnRewardComplete);
            if (_restUI != null)
                _restUI.Initialize(OnRestChoiceSelected);
            if (_relicBarUI != null)
                _relicBarUI.Initialize(_runState);
            if (_deckViewerUI != null)
                _deckViewerUI.Initialize(_runState);
            if (_deckButton != null)
                _deckButton.onClick.AddListener(OnDeckButtonClicked);
            if (_tutorialUI != null)
                _tutorialUI.Initialize(_runState);
        }

        private void OnMapChanged(MapFloor mapFloor)
        {
            if (_mapView != null)
                _mapView.Initialize(mapFloor, _runState.Gold, OnNodeClicked);
        }

        /// <summary>
        /// 전투 결과에 따라 보상 또는 패배 처리
        /// </summary>
        private void HandleBattleResult()
        {
            if (BattleResult.WasVictory)
            {
                _runState.OnBattleVictory();
                OnBattleVictory();
            }
            else
            {
                _runState.EndRunDefeat();
            }
            BattleResult.Clear();
        }

        /// <summary>
        /// 전투 승리 시 보상 표시
        /// </summary>
        private void OnBattleVictory()
        {
            if (_rewardUI != null)
            {
                _rewardUI.ShowRewards(BattleResult.BattleType);
            }
        }

        private void OnDestroy()
        {
            if (_runState != null)
            {
                _runState.OnMapChanged -= OnMapChanged;
                _runState.OnRunEnded -= OnRunEnded;
            }
            if (_runEndOverlay != null)
                _runEndOverlay.OnReturnToTitle -= OnReturnToTitle;
            if (_deckButton != null)
                _deckButton.onClick.RemoveListener(OnDeckButtonClicked);
        }

        /// <summary>
        /// 런 종료 시 RunEndOverlay 표시 + 메타 통계 갱신
        /// </summary>
        private void OnRunEnded()
        {
            bool victory = _runState.IsRunComplete;
            int floor = _runState.CurrentFloor;
            int gold = _runState.TotalGoldEarned;
            int battlesWon = _runState.BattlesWon;

            SaveManager.RecordRunEnd(victory, floor, gold);
            GameRunState.Destroy();

            if (_runEndOverlay != null)
            {
                _runEndOverlay.Show(victory, floor, gold, battlesWon);
            }
            else
            {
                // RunEndOverlay가 없으면 바로 타이틀로
                OnReturnToTitle();
            }
        }

        /// <summary>
        /// 타이틀 씬으로 복귀
        /// </summary>
        private void OnReturnToTitle()
        {
            SceneTransition.Instance.FadeToScene("TitleScene");
        }
    }
}
