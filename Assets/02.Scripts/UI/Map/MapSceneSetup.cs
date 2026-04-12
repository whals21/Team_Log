using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TeamLog.Characters;
using TeamLog.Event;
using TeamLog.Map;
using TeamLog.UI.Event;
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

        [Header("Test Mode")]
        [SerializeField] private bool _useTestData = true;
        [SerializeField] private CharacterData _testWarriorData;
        [SerializeField] private CharacterData _testMageData;
        [SerializeField] private CharacterData _testHealerData;
        [SerializeField] private CharacterData _testRogueData;

        [Header("Test Events")]
        [SerializeField] private EventData[] _testEvents;

        private GameRunState _runState;
        private List<Character> _playerParty;

        private const string BattleSceneName = "BattleScene";

        private void Start()
        {
            if (_useTestData)
            {
                InitializeTestRun();
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

            _runState = new GameRunState(_playerParty, startingGold: 50);
            _runState.OnMapChanged += OnMapChanged;

            // 서브 UI 초기화
            if (_eventUI != null)
                _eventUI.Initialize(_runState, OnEventComplete);
            if (_shopUI != null)
                _shopUI.Initialize(_runState, OnShopExit);

            _runState.StartRun();
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

            // 보스 클리어 시 다음 층
            if (_runState.CurrentMap.IsCleared)
            {
                _runState.AdvanceToNextFloor();
            }
        }

        private void StartBattle(MapNode node)
        {
            var enemies = new List<Character>();

            if (node.NodeType == MapNodeType.Boss)
            {
                var bossData = ScriptableObject.CreateInstance<CharacterData>();
                var boss = new Character(bossData);
                boss.Health.Initialize(150);
                boss.Stats.Initialize(15, 8);
                enemies.Add(boss);
            }
            else
            {
                var enemyData = ScriptableObject.CreateInstance<CharacterData>();
                var enemy = new Character(enemyData);
                enemy.Health.Initialize(50);
                enemy.Stats.Initialize(10, 3);
                enemies.Add(enemy);
            }

            BattleSceneSetup.SetBattleData(_playerParty, enemies);
            SceneManager.LoadScene(BattleSceneName);
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

        private void OnDestroy()
        {
            if (_runState != null)
                _runState.OnMapChanged -= OnMapChanged;
        }
    }
}
