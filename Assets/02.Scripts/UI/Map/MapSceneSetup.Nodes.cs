// MapSceneSetup.Nodes.cs — 노드 디스패치 + 전투 시작 + 서브 UI 핸들러
// 진입점+초기화: MapSceneSetup.cs
//
// ★ Node Detail Preview 파이프 (2단계 흐름):
//   노드 클릭 → OnNodeClicked → PrepareNodePreview (적 샘플링 + 보상 계산)
//                                  → NodeDetailPanel.Initialize(node, preview, OnConfirmAction)
//   "Enter Battle" 버튼 → OnConfirmAction → MoveToNode → StartBattle (캐싱된 적 사용)
//
// 무작위 일관성 보장: 노드 클릭 시 1회 샘플링 → _previewedEnemies 캐싱 → StartBattle에서 재사용.
// 다른 노드 클릭 시 자동으로 새 샘플링.

using System.Collections.Generic;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Event;
using TeamLog.Map;
using TeamLog.Meta;
using TeamLog.UI;
using TeamLog.UI.Event;
using TeamLog.UI.Map.Rework;  // ★ NodePreviewData / EnemyPreviewInfo / RewardPreviewInfo
using TeamLog.UI.Shop;

namespace TeamLog.UI.Map
{
    public partial class MapSceneSetup
    {
        // ★ Preview 파이프 상태 — _previewedNode != null인 동안 _previewedEnemies가 유효.
        private MapNode _previewedNode;
        private List<CharacterData> _previewedEnemies;

        private void OnNodeClicked(MapNode node)
        {
            if (!_runState.IsRunActive) return;
            if (!node.IsActive) return;  // 잠긴 노드 무시

            // ★ 새 흐름 (MapSceneRework) — 2단계: 클릭 → 상세 패널 → 버튼 → 액션
            if (_mapReworkView != null && _nodeDetailPanel != null)
            {
                PrepareNodePreview(node);
                return;
            }

            // 레거시 흐름 (기존 MapView) — 즉시 이동
            MoveToNode(node);
        }

        /// <summary>
        /// ★ Preview 빌더 — 노드에 대한 미리보기 데이터 생성 + NodeDetailPanel 갱신.
        /// 1회 적 샘플링 → _previewedEnemies 캐싱 (StartBattle이 동일 적 사용).
        /// </summary>
        private void PrepareNodePreview(MapNode node)
        {
            _previewedNode = node;
            _previewedEnemies = SampleEnemiesForPreview(node);

            var preview = BuildPreviewData(node, _previewedEnemies);
            if (_nodeDetailPanel != null)
                _nodeDetailPanel.Initialize(node, preview, OnConfirmAction);

            Debug.Log($"[MapSceneSetup] PrepareNodePreview — node:{node.NodeType} Layer:{node.Layer} " +
                      $"enemies:{(_previewedEnemies?.Count ?? 0)} gold:{preview?.Rewards?.Summary ?? "—"}");
        }

        /// <summary>
        /// "Enter Battle" 버튼 클릭 시 호출 — 캐시된 노드로 실제 액션 수행.
        /// </summary>
        private void OnConfirmAction(MapNode node)
        {
            // 다른 노드가 선택된 상태면 무시 (UI 경쟁 상태 방지)
            if (_previewedNode != node) return;
            MoveToNode(node);
        }

        /// <summary>
        /// NodePreviewData 빌더 — 노드 타입별 헤더/적/보상 데이터 조립.
        /// </summary>
        private NodePreviewData BuildPreviewData(MapNode node, List<CharacterData> enemies)
        {
            var typeInfo = NodeDetailPanel.GetStaticNodeTypeInfo(node.NodeType);
            var preview = new NodePreviewData
            {
                NodeType = node.NodeType,
                Title = typeInfo.DisplayName,
                Subtitle = typeInfo.Subtitle,
                Description = typeInfo.Description,
                ActionLabel = typeInfo.ActionLabel,
                ThemeColor = typeInfo.Color,
                IconSymbol = typeInfo.IconSymbol,
            };

            // 전투 노드 — 적 목록 + 보상
            bool isCombat = node.NodeType == MapNodeType.Battle
                          || node.NodeType == MapNodeType.Elite
                          || node.NodeType == MapNodeType.Boss;
            if (isCombat)
            {
                preview.Enemies = BuildEnemyPreviewList(node, enemies);
                preview.Rewards = TeamLog.Reward.RewardManager.GetPreview(node.NodeType, _runState);
            }
            else
            {
                // 비전투 노드 — 샘플링된 적이 없을 수도 있음. 빈 리스트.
                preview.Enemies = new List<EnemyPreviewInfo>();
            }

            return preview;
        }

        /// <summary>
        /// EnemyPreviewInfo 리스트 빌더 — 스케일링 후 추정 HP 포함.
        /// </summary>
        private List<EnemyPreviewInfo> BuildEnemyPreviewList(MapNode node, List<CharacterData> enemies)
        {
            var result = new List<EnemyPreviewInfo>();
            if (enemies == null) return result;

            float scaling = _runState.GetFloorScaling();
            Color tint = node.NodeType switch
            {
                MapNodeType.Boss => new Color(0.55f, 0.06f, 0.06f, 1f),
                MapNodeType.Elite => new Color(0.96f, 0.83f, 0.37f, 1f),
                _ => new Color(0.75f, 0.22f, 0.17f, 1f)
            };

            foreach (var data in enemies)
            {
                if (data == null) continue;
                int estimatedHP = System.Math.Max(1, (int)(data.BaseHP * scaling));
                result.Add(new EnemyPreviewInfo
                {
                    Name = data.CharacterName,
                    EstimatedHP = estimatedHP,
                    Tint = tint
                });
            }
            return result;
        }

        /// <summary>
        /// ★ 적 샘플링 — StartBattle의 샘플링 로직을 발췌 (CharacterData만 반환).
        /// Boss: theme.boss / Elite: RollElitePattern or 랜덤 / Battle: RollNormalPattern or 랜덤.
        /// </summary>
        private List<CharacterData> SampleEnemiesForPreview(MapNode node)
        {
            var result = new List<CharacterData>();
            var theme = _runState.CurrentStageTheme;
            if (theme == null) return result;

            var patternTable = theme.spawnPatternTable;

            switch (node.NodeType)
            {
                case MapNodeType.Boss:
                    if (theme.boss != null)
                        result.Add(theme.boss);
                    break;

                case MapNodeType.Elite:
                    if (patternTable != null && patternTable.ElitePatterns != null && patternTable.ElitePatterns.Length > 0)
                    {
                        var pattern = patternTable.ElitePatterns[Random.Range(0, patternTable.ElitePatterns.Length)];
                        result.AddRange(ExtractDataFromPattern(pattern));
                    }
                    if (result.Count == 0 && theme.eliteEnemies != null && theme.eliteEnemies.Count > 0)
                    {
                        result.Add(theme.eliteEnemies[Random.Range(0, theme.eliteEnemies.Count)]);
                    }
                    break;

                default: // 일반 전투
                    if (patternTable != null && patternTable.NormalPatterns != null && patternTable.NormalPatterns.Length > 0)
                    {
                        var pattern = patternTable.NormalPatterns[Random.Range(0, patternTable.NormalPatterns.Length)];
                        result.AddRange(ExtractDataFromPattern(pattern));
                    }
                    if (result.Count == 0 && theme.normalEnemies != null && theme.normalEnemies.Count > 0)
                    {
                        int count = Random.Range(1, 4);
                        for (int i = 0; i < count; i++)
                            result.Add(theme.normalEnemies[Random.Range(0, theme.normalEnemies.Count)]);
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// EnemySpawnPattern에서 CharacterData 리스트 추출 (Character 인스턴스 X).
        /// </summary>
        private List<CharacterData> ExtractDataFromPattern(EnemySpawnPattern pattern)
        {
            var list = new List<CharacterData>();
            if (pattern?.enemies == null) return list;
            foreach (var entry in pattern.enemies)
            {
                if (entry.enemyData == null) continue;
                for (int i = 0; i < entry.count; i++)
                    list.Add(entry.enemyData);
            }
            return list;
        }

        private void MoveToNode(MapNode node)
        {
            bool moved = _runState.CurrentMap.MoveToNode(node);
            if (!moved) return;

            AudioManager.Instance.PlayUINodeClick();

            // ★ 노드 이동 성공 → preview 캐시 무효화 (다음 preview 시 재샘플링되도록)
            _previewedNode = null;
            // _previewedEnemies는 StartBattle이 소비할 수 있으므로 여기서 클리어하지 않음.

            // UI 갱신
            RefreshMapUI();

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

            // ★ 캐싱 우선: _previewedNode == node이고 _previewedEnemies가 있으면 그걸 사용.
            // 일관성 보장 — 미리보기에 표시된 적 = 실제 전투 적.
            var enemies = new List<Character>();
            List<CharacterData> enemiesData;
            bool useCache = _previewedNode == node && _previewedEnemies != null && _previewedEnemies.Count > 0;

            if (useCache)
            {
                enemiesData = _previewedEnemies;
            }
            else
            {
                // 레거시 경로 (또는 캐시 미스) — 그 자리에서 샘플링
                enemiesData = SampleEnemiesForPreview(node);
            }

            foreach (var data in enemiesData)
                enemies.Add(new Character(data));

            // 캐시 소비 후 클리어
            _previewedNode = null;
            _previewedEnemies = null;

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

            RefreshMapUI();

            SaveManager.Save();
        }

        private void OpenEvent()
        {
            // ★ Phase EVENT: Rework View 우선, 폴백으로 기존 _eventUI
            EventData selected = PickRandomEvent();
            if (selected == null) return;

            if (_eventReworkView != null)
                _eventReworkView.ShowEvent(selected);
            else if (_eventUI != null)
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
            // ★ Phase SHOP: Rework View 우선, 폴백으로 기존 _shopUI
            if (_shopReworkView != null)
                _shopReworkView.OpenShop(_runState.CurrentFloor);
            else if (_shopUI != null)
                _shopUI.OpenShop(_runState.CurrentFloor);
        }

        private void OnEventComplete()
        {
            RefreshMapUI();
            RefreshRelicBar();
            SaveManager.Save();
        }

        private void OnShopExit()
        {
            RefreshMapUI();
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

            RefreshMapUI();
            RefreshRelicBar();
        }

        private void RefreshRelicBar()
        {
            if (_relicBarUI != null && _runState != null)
                _relicBarUI.Refresh();
        }

        /// <summary>
        /// ★ Phase GF (2026-07-21): 맵 UI 통합 갱신 헬퍼.
        /// MapSceneRework를 사용 중일 때는 _mapReworkView.Refresh() 호출,
        /// 기존 MapView를 사용 중일 때는 _mapView.Refresh() 호출.
        /// 둘 다 없으면 no-op.
        /// ★ 버그 수정: 기존에는 _mapView.Refresh()만 호출해서 MapRework 환경에서
        /// 상점/이벤트/보상 완료 후 다음 노드가 UI에 안 열리는 현상 발생.
        /// </summary>
        private void RefreshMapUI()
        {
            if (_mapReworkView != null)
                _mapReworkView.Refresh();
            else if (_mapView != null)
                _mapView.Refresh(_runState.Gold);
        }

        private void OnDeckButtonClicked()
        {
            if (_deckViewerUI != null)
                _deckViewerUI.Show();
        }

        #endregion
    }
}
