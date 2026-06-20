// MapSceneSetup.Nodes.cs — 노드 디스패치 + 전투 시작 + 서브 UI 핸들러
// 진입점+초기화: MapSceneSetup.cs

using System.Collections.Generic;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Event;
using TeamLog.Map;
using TeamLog.Meta;
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
                        float healBoost = MetaProgressionManager.GetPartyHealBoost(SaveManager.Meta);
                        float ascMul = AscensionManager.GetHealMulByLevel(_runState.SelectedAscensionLevel);
                        _runState.RestAtCampfire((0.3f + healBoost) * ascMul);
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
            var theme = _runState.CurrentStageTheme;
            if (theme == null)
            {
                Debug.LogWarning("[MapSceneSetup] 현재 스테이지 테마가 없습니다.");
                return;
            }

            var enemies = new List<Character>();
            var patternTable = theme.spawnPatternTable;

            switch (node.NodeType)
            {
                case MapNodeType.Boss:
                    if (theme.boss != null)
                        enemies.Add(new Character(theme.boss));
                    break;
                case MapNodeType.Elite:
                    if (patternTable != null)
                        enemies = patternTable.RollElitePattern();
                    // 폴백: 패턴 테이블이 없거나 비었으면 테마 엘리트 풀에서 무작위
                    if (enemies.Count == 0 && theme.eliteEnemies != null && theme.eliteEnemies.Count > 0)
                        enemies.Add(new Character(theme.eliteEnemies[UnityEngine.Random.Range(0, theme.eliteEnemies.Count)]));
                    break;
                default: // 일반 전투
                    if (patternTable != null)
                        enemies = patternTable.RollNormalPattern();
                    // 폴백: 패턴 테이블이 없거나 비었으면 테마 일반 풀에서 무작위 1~3마리
                    if (enemies.Count == 0 && theme.normalEnemies != null && theme.normalEnemies.Count > 0)
                    {
                        int count = UnityEngine.Random.Range(1, 4);
                        for (int i = 0; i < count; i++)
                            enemies.Add(new Character(theme.normalEnemies[UnityEngine.Random.Range(0, theme.normalEnemies.Count)]));
                    }
                    break;
            }

            if (enemies.Count == 0)
            {
                Debug.LogWarning("[MapSceneSetup] 적 데이터가 없어 전투를 시작할 수 없습니다.");
                return;
            }

            // 스테이지별 적 스케일링 적용
            float scaling = _runState.GetFloorScaling();
            foreach (var enemy in enemies)
                enemy.ApplyFloorScaling(scaling);

            int bonusAP = _runState.ConsumeBonusAP();
            bool isBossBattle = node.NodeType == MapNodeType.Boss;
            BattleSceneSetup.SetBattleData(_playerParty, enemies, bonusAP, isBossBattle: isBossBattle);
            BattleResult.SetBattleType(node.NodeType);
            SceneTransition.Instance.FadeToScene(BattleSceneName);
        }

        #region Sub-UI Handlers

        private void OnRestChoiceSelected(int choice)
        {
            AudioManager.Instance.PlayUIConfirm();
            switch (choice)
            {
                case 0: // 휴식
                    float boost = MetaProgressionManager.GetPartyHealBoost(SaveManager.Meta);
                    float ascMul = AscensionManager.GetHealMulByLevel(_runState.SelectedAscensionLevel);
                    _runState.RestAtCampfire((0.3f + boost) * ascMul);
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

            // Phase E3: 테마별 이벤트 우선 사용, 폴백으로 공통 풀
            EventData selected = PickRandomEvent();
            if (selected != null)
                _eventUI.ShowEvent(selected);
        }

        /// <summary>
        /// Phase E3: 이벤트 추첨 — 테마 전용 이벤트 70% / 공통 이벤트 30%
        /// </summary>
        private EventData PickRandomEvent()
        {
            var theme = _runState?.CurrentStageTheme;
            bool hasThemeEvents = theme != null && theme.themeEvents != null && theme.themeEvents.Count > 0;
            bool hasCommonEvents = _testEvents != null && _testEvents.Length > 0;

            // 테마 이벤트 풀이 있으면 70% 확률로 우선 사용
            if (hasThemeEvents && (UnityEngine.Random.value < 0.7f || !hasCommonEvents))
            {
                int idx = UnityEngine.Random.Range(0, theme.themeEvents.Count);
                return theme.themeEvents[idx];
            }

            // 공통 이벤트 풀
            if (hasCommonEvents)
            {
                int index = UnityEngine.Random.Range(0, _testEvents.Length);
                return _testEvents[index];
            }

            // 폴백: 테마 전용이라도
            if (hasThemeEvents)
            {
                int idx = UnityEngine.Random.Range(0, theme.themeEvents.Count);
                return theme.themeEvents[idx];
            }

            return null;
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
            // 엘리트 전투 후 엘리트 보너스 표시
            if (BattleResult.BattleType == MapNodeType.Elite && _stageBonusUI != null)
            {
                _stageBonusUI.ShowEliteBonus();
                return;
            }

            // 보스 클리어 후 (런 미완료 시) 스테이지 클리어 보너스 표시
            if (BattleResult.BattleType == MapNodeType.Boss
                && _runState.CurrentMap != null
                && _runState.CurrentMap.IsCleared
                && !_runState.IsRunComplete
                && _stageBonusUI != null)
            {
                _stageBonusUI.ShowStageClearBonus();
                return;
            }

            ProceedAfterBonus();
        }

        /// <summary>
        /// 엘리트/스테이지 클리어 보너스 선택 완료 후 진행
        /// </summary>
        private void OnStageBonusComplete()
        {
            ProceedAfterBonus();
        }

        private void ProceedAfterBonus()
        {
            // 보스 클리어 시 다음 층으로 이동
            if (_runState.CurrentMap != null && _runState.CurrentMap.IsCleared)
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
