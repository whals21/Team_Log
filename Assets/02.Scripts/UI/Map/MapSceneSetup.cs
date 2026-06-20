using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Event;
using TeamLog.Map;
using TeamLog.Meta;
using TeamLog.Reward;
using TeamLog.Skill;
using TeamLog.UI;
using TeamLog.UI.Event;
using TeamLog.UI.Reward;
using TeamLog.UI.Shop;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 층별 적 풀 — 각 층의 일반/엘리트/보스 적 데이터를 보관 (레거시 호환)
    /// </summary>
    [Serializable]
    public class FloorEnemyPool
    {
        public CharacterData[] normalEnemies;
        public CharacterData[] eliteEnemies;
        public CharacterData boss;
    }

    /// <summary>
    /// 스테이지 테마 후보 목록 — StageDesign.md 기준 각 스테이지마다 3개 테마 후보 보관.
    /// 런 시작 시 이 중 1개가 무작위 채택됨.
    /// </summary>
    [Serializable]
    public class StageThemeCandidateList
    {
        public StageThemeData[] candidates;
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
        [SerializeField] private StageBonusUI _stageBonusUI;
        [SerializeField] private CharacterTraitSelectUI _characterTraitSelectUI;

        [Header("All Characters")]
        [SerializeField] private CharacterData[] _allCharacters;

        [Header("Character Traits Pool (Phase 8D)")]
        [SerializeField] private CharacterTraitData[] _allCharacterTraits;

        [Header("Test Mode")]
        [SerializeField] private bool _useTestData = true;
        [SerializeField] private CharacterData _testWarriorData;
        [SerializeField] private CharacterData _testMageData;
        [SerializeField] private CharacterData _testHealerData;
        [SerializeField] private CharacterData _testRogueData;

        [Header("Test Events")]
        [SerializeField] private EventData[] _testEvents;

        [Header("Stage Theme Candidates (per stage, 3 candidates each)")]
        [SerializeField] private StageThemeCandidateList[] _stageThemeCandidates;

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
        /// 캐릭터 선택 완료 → 특성 선택 단계로 (Phase 8D)
        /// </summary>
        private void OnCharacterSelectConfirmed(List<CharacterData> selectedCharacters)
        {
            _playerParty = new List<Character>();
            foreach (var data in selectedCharacters)
                _playerParty.Add(new Character(data));

            // Phase 8D: 특성 선택 UI 표시 — _allCharacterTraits가 비활성이면 바로 런 시작
            if (_characterTraitSelectUI != null && _allCharacterTraits != null && _allCharacterTraits.Length > 0)
            {
                _characterTraitSelectUI.Initialize(
                    selectedCharacters.ToArray(),
                    _allCharacterTraits,
                    SaveManager.Meta,
                    OnTraitSelectConfirmed);
                _characterTraitSelectUI.Show();
            }
            else
            {
                StartRunWithParty();
            }
        }

        /// <summary>
        /// 특성 선택 완료 → meta 저장 + 파티에 특성 장착 + 런 시작
        /// </summary>
        private void OnTraitSelectConfirmed(List<CharacterTraitSelectUI.TraitSelection> selections)
        {
            var meta = SaveManager.Meta;
            // meta.EquippedTraitBindings 업데이트
            foreach (var sel in selections)
            {
                if (sel?.Character == null) continue;
                string traitId = sel.Trait != null ? sel.Trait.TraitId : "";
                MetaProgressionManager.TryEquipTrait(meta, sel.Character.CharacterName, traitId, requireUnlocked: false);
            }
            SaveManager.SaveMeta();

            // 파티에 장착 적용
            ApplyEquippedTraitsFromMeta(_playerParty);

            StartRunWithParty();
        }

        /// <summary>
        /// meta에 저장된 장착 특성을 파티원에 적용 (Phase 8D).
        /// 런 시작(OnTraitSelectConfirmed) 및 세이브 로드(ContinueFromSave) 양쪽에서 사용.
        /// </summary>
        private void ApplyEquippedTraitsFromMeta(List<Character> party)
        {
            if (_allCharacterTraits == null || party == null) return;
            var meta = SaveManager.Meta;
            foreach (var c in party)
            {
                if (c == null) continue;
                string traitId = MetaProgressionManager.GetEquippedTraitId(meta, c.Data.CharacterName);
                if (string.IsNullOrEmpty(traitId)) continue;
                var trait = System.Array.Find(_allCharacterTraits, t => t != null && t.TraitId == traitId);
                if (trait != null) c.EquipTrait(trait);
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

            StartRunWithParty();
        }

        /// <summary>
        /// 파티가 확정된 후 런 초기화
        /// </summary>
        private void StartRunWithParty()
        {
            var meta = SaveManager.Meta;

            // Ascension: 메타에 저장된 선택 레벨을 런에 복사.
            // 파티 MaxHP / 시작 골드 modifier는 런 시작 1회 적용.
            int ascLevel = AscensionManager.ClampSelectedLevel(meta.SelectedAscensionLevel, meta);
            int startGold = System.Math.Max(0, 50 + AscensionManager.GetStartGoldDeltaByLevel(ascLevel));

            _runState = GameRunState.Create(_playerParty, startingGold: startGold);
            _runState.SetSelectedAscensionLevel(ascLevel);
            ApplyAscensionToPartyMaxHp(ascLevel);

            _runState.OnMapChanged += OnMapChanged;
            _runState.OnRunEnded += OnRunEnded;

            // Phase 8E: 유물 풀 필터링 (잠긴 유물 제외)
            var filteredRelicPool = MetaProgressionManager.FilterRelicPool(_relicPool, meta);
            _runState.SetDataPools(
                filteredRelicPool,
                _augmentPool != null ? new List<AugmentData>(_augmentPool) : new List<AugmentData>());

            // 스테이지 테마 후보 주입
            _runState.SetThemeCandidates(BuildThemeCandidates());

            InitializeSubUIs();

            _runState.StartRun();

            // Phase 8E: 시작 유물 지급 (메타 강화로 해금 시)
            ApplyStartingRelics(filteredRelicPool, meta);

            // 메타 데이터에 런 시작 기록
            meta.HasPendingRun = true;
            SaveManager.SaveMeta();
        }

        /// <summary>
        /// 어센션 PlayerMaxHpPercent modifier를 파티 MaxHP에 적용 (런 시작 1회).
        /// asc 4 = -5%, asc 10 = -10%. 현재 HP도 MaxHP에 비례하여 감소.
        /// </summary>
        private void ApplyAscensionToPartyMaxHp(int ascLevel)
        {
            float mul = AscensionManager.GetPlayerMaxHpMulByLevel(ascLevel);
            if (mul >= 1f) return;
            foreach (var member in _playerParty)
            {
                if (member == null) continue;
                int newMax = System.Math.Max(1, (int)(member.Health.MaxHP * mul));
                member.Health.Initialize(newMax);
            }
        }

        /// <summary>
        /// 메타 강화 기반 시작 유물 지급 (Phase 8E).
        /// StartingRelicSlot: 랜덤 1개, StartingRelicChoice: 추가 랜덤 1개 (단순화 — 본래는 3选1 UI).
        /// </summary>
        private void ApplyStartingRelics(List<RelicData> filteredPool, MetaSaveData meta)
        {
            if (_runState == null || filteredPool == null || filteredPool.Count == 0) return;
            int grantCount = MetaProgressionManager.GetStartingRelicGrantCount(meta);
            if (grantCount <= 0) return;
            var picks = MetaProgressionManager.RollRelics(filteredPool, grantCount);
            foreach (var relic in picks)
                _runState.AcquireRelic(relic);
        }

        /// <summary>
        /// 인스펙터에 설정된 _stageThemeCandidates를 List<List<StageThemeData>>로 변환
        /// </summary>
        private List<List<StageThemeData>> BuildThemeCandidates()
        {
            var result = new List<List<StageThemeData>>();
            if (_stageThemeCandidates == null) return result;

            foreach (var entry in _stageThemeCandidates)
            {
                var pool = new List<StageThemeData>();
                if (entry != null && entry.candidates != null)
                {
                    foreach (var theme in entry.candidates)
                        if (theme != null) pool.Add(theme);
                }
                result.Add(pool);
            }
            return result;
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

            // 테마 후보 주입 — 저장 데이터에 이미 선택된 테마가 있으면 그대로 유지됨
            _runState.SetThemeCandidates(BuildThemeCandidates());

            // Phase 8D: 저장된 장착 특성 복원
            ApplyEquippedTraitsFromMeta(_playerParty);

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
            if (_stageBonusUI != null)
                _stageBonusUI.Initialize(_runState, OnStageBonusComplete);
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

            // 메타 재화 보상 계산 — RecordRunEnd 이전에 산출하여 오버레이에 전달
            var reward = TeamLog.Meta.MetaProgressionManager.CalculateRunReward(
                victory, floor, gold, battlesWon);

            // 어센션 상승 정보 — RecordRunEnd 이전 레벨 캡처
            int ascBefore = SaveManager.Meta.AscensionLevel;

            SaveManager.RecordRunEnd(victory, floor, gold, battlesWon);
            GameRunState.Destroy();

            string ascensionNote = null;
            if (victory && floor >= 4)
            {
                int ascAfter = SaveManager.Meta.AscensionLevel;
                if (ascAfter > ascBefore)
                    ascensionNote = $"어센션 상승! {ascBefore} → {ascAfter}";
            }

            if (_runEndOverlay != null)
            {
                _runEndOverlay.Show(victory, floor, gold, battlesWon, reward.memory, reward.souls, ascensionNote);
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
