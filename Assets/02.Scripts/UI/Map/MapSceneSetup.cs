using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Event;
using TeamLog.Map;
using TeamLog.Reward;
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
    /// </summary>
    public class MapSceneSetup : MonoBehaviour
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

        [Header("Data Pools")]
        [SerializeField] private SkillData[] _skillPool;
        [SerializeField] private ItemData[] _itemPool;
        [SerializeField] private RelicData[] _relicPool;

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
            _runState.OnRunEnded += OnRunEnded;
            _runState.SetDataPools(
                _skillPool != null ? new List<SkillData>(_skillPool) : new List<SkillData>(),
                _itemPool != null ? new List<ItemData>(_itemPool) : new List<ItemData>(),
                _relicPool != null ? new List<RelicData>(_relicPool) : new List<RelicData>());

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
                InitializeTestRun();
                return;
            }

            _playerParty = new List<Character>(_runState.PlayerParty);
            _runState.OnMapChanged += OnMapChanged;
            _runState.OnRunEnded += OnRunEnded;
            _runState.SetDataPools(
                _skillPool != null ? new List<SkillData>(_skillPool) : new List<SkillData>(),
                _itemPool != null ? new List<ItemData>(_itemPool) : new List<ItemData>(),
                _relicPool != null ? new List<RelicData>(_relicPool) : new List<RelicData>());

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
                _shopUI.Initialize(_runState, OnShopExit, _skillPool, _itemPool);
            if (_rewardUI != null)
                _rewardUI.Initialize(_runState, OnRewardComplete);
            if (_restUI != null)
                _restUI.Initialize(OnRestChoiceSelected);
            if (_relicBarUI != null)
                _relicBarUI.Initialize(_runState);
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

            // 보스/엘리트: 이동 전 확인 다이얼로그
            if (node.NodeType == MapNodeType.Boss || node.NodeType == MapNodeType.Elite)
            {
                string label = node.NodeType == MapNodeType.Boss ? "보스" : "엘리트";
                _pendingBattleNode = node;
                if (_confirmationDialog != null)
                {
                    _confirmationDialog.Show(
                        $"강력한 {label} 적이 기다리고 있습니다.\n전투를 시작하시겠습니까?",
                        OnBattleConfirmed,
                        () => { _pendingBattleNode = null; });
                }
                else
                {
                    OnBattleConfirmed();
                }
                return;
            }

            MoveToNode(node);
        }

        private void OnBattleConfirmed()
        {
            if (_pendingBattleNode == null) return;
            MoveToNode(_pendingBattleNode);
            _pendingBattleNode = null;
        }

        private void MoveToNode(MapNode node)
        {
            bool moved = _runState.CurrentMap.MoveToNode(node);
            if (!moved) return;

            AudioManager.Instance.PlayUINodeClick();

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
                    if (_restUI != null)
                        _restUI.Show();
                    else
                    {
                        _runState.RestAtCampfire();
                        ToastUI.Show("파티가 휴식했습니다.");
                    }
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
            var pool = GetFloorPool();
            if (pool == null)
            {
                Debug.LogWarning("[MapSceneSetup] 층별 적 풀이 비어 있습니다.");
                return;
            }

            var enemies = new List<Character>();

            switch (node.NodeType)
            {
                case MapNodeType.Boss:
                    if (pool.boss != null)
                        enemies.Add(new Character(pool.boss));
                    break;
                case MapNodeType.Elite:
                    if (pool.eliteEnemies != null && pool.eliteEnemies.Length > 0)
                        enemies.Add(new Character(pool.eliteEnemies[UnityEngine.Random.Range(0, pool.eliteEnemies.Length)]));
                    break;
                default: // 일반 전투
                    if (pool.normalEnemies != null && pool.normalEnemies.Length > 0)
                    {
                        int count = UnityEngine.Random.Range(1, 4); // 1~3마리
                        for (int i = 0; i < count; i++)
                            enemies.Add(new Character(pool.normalEnemies[UnityEngine.Random.Range(0, pool.normalEnemies.Length)]));
                    }
                    break;
            }

            if (enemies.Count == 0)
            {
                Debug.LogWarning("[MapSceneSetup] 적 데이터가 없어 전투를 시작할 수 없습니다.");
                return;
            }

            // 층별 적 스케일링 적용
            float scaling = _runState.GetFloorScaling();
            foreach (var enemy in enemies)
                enemy.ApplyFloorScaling(scaling);

            int bonusAP = _runState.ConsumeBonusAP();
            BattleSceneSetup.SetBattleData(_playerParty, enemies, bonusAP);
            BattleResult.SetBattleType(node.NodeType);
            SceneTransition.Instance.FadeToScene(BattleSceneName);
        }

        private FloorEnemyPool GetFloorPool()
        {
            if (_floorPools == null || _floorPools.Length == 0) return null;
            int index = System.Math.Clamp(_runState.CurrentFloor - 1, 0, _floorPools.Length - 1);
            return _floorPools[index];
        }

        private void OnRestChoiceSelected(int choice)
        {
            AudioManager.Instance.PlayUIConfirm();
            switch (choice)
            {
                case 0: // 휴식
                    _runState.RestAtCampfire();
                    ToastUI.Show("파티가 휴식하여 HP를 회복했습니다.");
                    break;
                case 1: // 수련
                    _runState.TrainAtCampfire();
                    ToastUI.Show("파티가 수련하여 공격력이 영구 증가했습니다.");
                    break;
                case 2: // 명상
                    _runState.MeditateAtCampfire();
                    ToastUI.Show("파티가 명상하여 다음 전투 AP 보너스를 얻었습니다.");
                    break;
            }

            if (_mapView != null)
                _mapView.Refresh(_runState.Gold);

            SaveManager.Save();
        }

        private void OpenEvent()
        {
            if (_eventUI == null) return;

            // 테스트 이벤트 데이터 있으면 사용, 없으면 스킵
            if (_testEvents != null && _testEvents.Length > 0)
            {
                int index = UnityEngine.Random.Range(0, _testEvents.Length);
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
            RefreshRelicBar();
            SaveManager.Save();
        }

        private void OnShopExit()
        {
            if (_mapView != null)
                _mapView.Refresh(_runState.Gold);
            RefreshRelicBar();
            SaveManager.Save();
        }

        private void OnRewardComplete()
        {
            // 보스 클리어 시 다음 층으로 이동 (보상 선택 이후)
            if (_runState.CurrentMap.IsCleared)
            {
                _runState.AdvanceToNextFloor();
            }
            else
            {
                SaveManager.Save();
            }

            if (_mapView != null)
                _mapView.Refresh(_runState.Gold);
            RefreshRelicBar();
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
            // GameRunState.Instance는 파괴하지 않음 — 씬 전환 간 유지
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

        private void RefreshRelicBar()
        {
            if (_relicBarUI != null && _runState != null)
                _relicBarUI.Refresh();
        }
    }
}
