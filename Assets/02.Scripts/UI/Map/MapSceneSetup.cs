using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Event;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.UI.Event;
using TeamLog.UI.Reward;
using TeamLog.UI.Shop;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 맵 씬의 진입점 — 맵 UI, GameRunState, 노드 이벤트 처리를 연결
    /// </summary>
    public class MapSceneSetup : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private MapView _mapView;
        [SerializeField] private EventUI _eventUI;
        [SerializeField] private ShopUI _shopUI;
        [SerializeField] private RewardUI _rewardUI;

        [Header("Test Mode")]
        [SerializeField] private bool _useTestData = true;
        [SerializeField] private CharacterData _testWarriorData;
        [SerializeField] private CharacterData _testMageData;
        [SerializeField] private CharacterData _testHealerData;
        [SerializeField] private CharacterData _testRogueData;

        [Header("Test Events")]
        [SerializeField] private EventData[] _testEvents;

        [Header("Enemy Data Pools")]
        [SerializeField] private CharacterData[] _normalEnemyPool;
        [SerializeField] private CharacterData[] _eliteEnemyPool;
        [SerializeField] private CharacterData[] _bossEnemyPool;

        [Header("Data Pools")]
        [SerializeField] private SkillData[] _skillPool;
        [SerializeField] private ItemData[] _itemPool;

        private GameRunState _runState;
        private List<Character> _playerParty;

        private const string BattleSceneName = "BattleScene";

        private void Start()
        {
            if (GameRunState.Instance != null)
            {
                RestoreExistingRun();
            }
            else
            {
                InitializeTestRun();
            }
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

            _runState = GameRunState.Create(_playerParty, startingGold: 50);
            _runState.OnMapChanged += OnMapChanged;
            _runState.SetDataPools(
                _skillPool != null ? new List<SkillData>(_skillPool) : new List<SkillData>(),
                _itemPool != null ? new List<ItemData>(_itemPool) : new List<ItemData>());

            InitializeSubUIs();

            _runState.StartRun();
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

        private void InitializeSubUIs()
        {
            if (_eventUI != null)
                _eventUI.Initialize(_runState, OnEventComplete);
            if (_shopUI != null)
                _shopUI.Initialize(_runState, OnShopExit, _skillPool, _itemPool);
            if (_rewardUI != null)
                _rewardUI.Initialize(_runState, OnRewardComplete);
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

        private void OnMapChanged(MapFloor mapFloor)
        {
            if (_mapView != null)
                _mapView.Initialize(mapFloor, _runState.Gold, OnNodeClicked);
        }

        private void OnNodeClicked(MapNode node)
        {
            if (!_runState.IsRunActive) return;

            bool moved = _runState.CurrentMap.MoveToNode(node);
            if (!moved) return;

            // UI 갱신
            if (_mapView != null)
                _mapView.Refresh(_runState.Gold);

            // 노드 타입별 처리
            switch (node.NodeType)
            {
                case MapNodeType.Battle:
                case MapNodeType.Elite:
                case MapNodeType.Boss:
                    StartBattle(node);
                    break;
                case MapNodeType.Rest:
                    _runState.RestAtCampfire();
                    break;
                case MapNodeType.Event:
                    OpenEvent();
                    break;
                case MapNodeType.Shop:
                    OpenShop();
                    break;
            }
        }

        private void StartBattle(MapNode node)
        {
            EnsureEnemyPoolsLoaded();
            var enemies = new List<Character>();

            switch (node.NodeType)
            {
                case MapNodeType.Boss:
                    if (_bossEnemyPool != null && _bossEnemyPool.Length > 0)
                        enemies.Add(new Character(_bossEnemyPool[Random.Range(0, _bossEnemyPool.Length)]));
                    break;
                case MapNodeType.Elite:
                    if (_eliteEnemyPool != null && _eliteEnemyPool.Length > 0)
                        enemies.Add(new Character(_eliteEnemyPool[Random.Range(0, _eliteEnemyPool.Length)]));
                    break;
                default: // 일반 전투
                    if (_normalEnemyPool != null && _normalEnemyPool.Length > 0)
                    {
                        int count = Random.Range(1, 3); // 1~2마리
                        for (int i = 0; i < count; i++)
                            enemies.Add(new Character(_normalEnemyPool[Random.Range(0, _normalEnemyPool.Length)]));
                    }
                    break;
            }

            if (enemies.Count == 0)
            {
                Debug.LogWarning("[MapSceneSetup] 적 데이터가 없어 전투를 시작할 수 없습니다.");
                return;
            }

            BattleSceneSetup.SetBattleData(_playerParty, enemies);
            BattleResult.SetBattleType(node.NodeType);
            SceneManager.LoadScene(BattleSceneName);
        }

        /// <summary>
        /// 인스펙터 연결 누락 시 경고 로그
        /// </summary>
        private void EnsureEnemyPoolsLoaded()
        {
            bool allEmpty = (_normalEnemyPool == null || _normalEnemyPool.Length == 0)
                && (_eliteEnemyPool == null || _eliteEnemyPool.Length == 0)
                && (_bossEnemyPool == null || _bossEnemyPool.Length == 0);

            if (allEmpty)
                Debug.LogWarning("[MapSceneSetup] 적 풀이 비어 있습니다. 인스펙터에 Enemy_ 에셋을 연결하세요.");
        }

        private void OpenEvent()
        {
            if (_eventUI == null) return;

            // 테스트 이벤트 데이터 있으면 사용, 없으면 스킵
            if (_testEvents != null && _testEvents.Length > 0)
            {
                int index = Random.Range(0, _testEvents.Length);
                _eventUI.ShowEvent(_testEvents[index]);
            }
        }

        private void OpenShop()
        {
            if (_shopUI != null)
                _shopUI.OpenShop(_runState.CurrentFloor);
        }

        private void OnEventComplete()
        {
            if (_mapView != null)
                _mapView.Refresh(_runState.Gold);
        }

        private void OnShopExit()
        {
            if (_mapView != null)
                _mapView.Refresh(_runState.Gold);
        }

        private void OnRewardComplete()
        {
            // 보스 클리어 시 다음 층으로 이동 (보상 선택 이후)
            if (_runState.CurrentMap.IsCleared)
            {
                _runState.AdvanceToNextFloor();
            }

            if (_mapView != null)
                _mapView.Refresh(_runState.Gold);
        }

        private void OnDestroy()
        {
            if (_runState != null)
                _runState.OnMapChanged -= OnMapChanged;
            // GameRunState.Instance는 파괴하지 않음 — 씬 전환 간 유지
        }
    }
}
