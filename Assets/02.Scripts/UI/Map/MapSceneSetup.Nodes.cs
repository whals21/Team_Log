// MapSceneSetup.Nodes.cs — 노드 디스패치 + 전투 시작 + 서브 UI 핸들러
// 진입점+초기화: MapSceneSetup.cs

using System.Collections.Generic;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Map;
using TeamLog.UI;
using TeamLog.UI.Event;
using TeamLog.UI.Shop;

namespace TeamLog.UI.Map
{
    public partial class MapSceneSetup
    {
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

            var patternTable = GetSpawnPatternTable();
            var enemies = new List<Character>();

            switch (node.NodeType)
            {
                case MapNodeType.Boss:
                    if (pool.boss != null)
                        enemies.Add(new Character(pool.boss));
                    break;
                case MapNodeType.Elite:
                    if (patternTable != null)
                        enemies = patternTable.RollElitePattern();
                    // 폴백: 패턴 테이블이 없으면 기존 방식
                    if (enemies.Count == 0 && pool.eliteEnemies != null && pool.eliteEnemies.Length > 0)
                        enemies.Add(new Character(pool.eliteEnemies[UnityEngine.Random.Range(0, pool.eliteEnemies.Length)]));
                    break;
                default: // 일반 전투
                    if (patternTable != null)
                        enemies = patternTable.RollNormalPattern();
                    // 폴백: 패턴 테이블이 없으면 기존 방식
                    if (enemies.Count == 0 && pool.normalEnemies != null && pool.normalEnemies.Length > 0)
                    {
                        int count = UnityEngine.Random.Range(1, 4);
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

        private SpawnPatternTable GetSpawnPatternTable()
        {
            if (_spawnPatternTables == null || _spawnPatternTables.Length == 0) return null;
            int index = System.Math.Clamp(_runState.CurrentFloor - 1, 0, _spawnPatternTables.Length - 1);
            return _spawnPatternTables[index];
        }

        #region Sub-UI Handlers

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
                case 3: // 리롤 토큰
                    _runState.AddRerollTokens(1);
                    ToastUI.Show("리롤 토큰 +1 획득!");
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

        private void RefreshRelicBar()
        {
            if (_relicBarUI != null && _runState != null)
                _relicBarUI.Refresh();
        }

        private void OnDeckButtonClicked()
        {
            if (_deckViewerUI != null)
                _deckViewerUI.Show();
        }

        #endregion
    }
}
