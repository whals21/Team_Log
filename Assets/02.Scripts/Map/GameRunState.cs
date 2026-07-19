using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Reward;
using TeamLog.Skill;

using StatType = TeamLog.Characters.StatType;

namespace TeamLog.Map
{
    /// <summary>
    /// 로그라이크 런 상태 — 정적 싱글톤 순수 C# 클래스
    /// 전체 플레이 세션의 진행 상태를 관리, 씬 전환 시에도 유지
    /// 보상 생성: AugmentOfferGenerator, 데미지: DamageCalculator
    /// </summary>
    public class GameRunState
    {
        private static GameRunState _instance;

        public static GameRunState Instance => _instance;

        public static GameRunState Create(List<Character> playerParty, int startingGold = 0)
        {
            if (_instance != null)
                _instance.Cleanup();
            _instance = new GameRunState(playerParty, startingGold);
            return _instance;
        }

        public static void Destroy()
        {
            if (_instance != null)
            {
                _instance.Cleanup();
                _instance = null;
            }
        }

        private readonly List<Character> _playerParty;
        private readonly List<string> _runHistory = new();
        private readonly System.Random _rng = new();

        // 데이터 풀 (MapSceneSetup에서 주입)
        private List<RelicData> _relicPool;
        private List<AugmentData> _augmentPool;

        // 테마 후보 (MapSceneSetup에서 주입) — 스테이지별 3개 후보
        private List<List<StageThemeData>> _themeCandidates;

        // 보류된 상점 보너스 (엘리트 보상/스테이지 클리어 보상에서 적립, 상점 방문 시 소비)
        private float _pendingShopDiscount;       // 0~1, 다음 상점 할인율
        private int _pendingShopExtraRelics;      // 다음 상점 추가 유물 진열 수
        private int _pendingShopExtraAugments;    // 다음 상점 추가 증강 진열 수

        // 맵 진행
        public int CurrentFloor { get; private set; } = 1;
        public MapFloor CurrentMap { get; private set; }
        public int TotalFloors { get; } = 4;

        // 리소스
        public int Gold { get; private set; }
        public int RerollTokens { get; private set; } = 1;

        // 통계
        public int BattlesWon { get; private set; }
        public int TotalGoldEarned { get; private set; }

        // 파티
        public IReadOnlyList<Character> PlayerParty => _playerParty;

        // ★ Phase CC-2D: 현재 전투 중인 적 목록 — BattleSceneSetup/TurnManager에서 매 전투 시작 시 설정.
        // MelodyResourceComponent(부 선율 Dissonance) 등이 접근. null이면 해당 효과 스킵.
        public IReadOnlyList<Character> CurrentEnemies { get; private set; }

        /// <summary>현재 전투 적 목록 설정 — BattleSceneSetup.SetupBattle에서 호출.</summary>
        public void SetCurrentEnemies(List<Character> enemies) => CurrentEnemies = enemies;

        /// <summary>전투 종료 시 적 목록 클리어.</summary>
        public void ClearCurrentEnemies() => CurrentEnemies = null;

        // 스테이지 테마 — 런 시작 시 각 스테이지별 1개씩 무작위 채택
        public List<StageThemeData> SelectedThemes { get; } = new();
        public StageThemeData CurrentStageTheme
        {
            get
            {
                int idx = CurrentFloor - 1;
                return (idx >= 0 && idx < SelectedThemes.Count) ? SelectedThemes[idx] : null;
            }
        }

        // 유물
        public RelicHandler RelicHandler { get; } = new();

        // 증강 보상 생성기
        public AugmentOfferGenerator AugmentGenerator { get; private set; }

        // 어센션 — 현재 런에서 플레이 중인 어센션 레벨 (런 시작 시 MetaSaveData.SelectedAscensionLevel 복사).
        // 0 = 어센션 없음. BattleSceneSetup/MapSceneSetup.Nodes.cs에서 AscensionManager로 값 계산.
        public int SelectedAscensionLevel { get; private set; }

        // 이력
        public IReadOnlyList<string> RunHistory => _runHistory;

        public bool IsRunActive { get; private set; }
        public bool IsRunComplete { get; private set; }

        public event System.Action<int> OnGoldChanged;
        public event System.Action<MapFloor> OnMapChanged;
        public event System.Action OnRunEnded;

        private GameRunState(List<Character> playerParty, int startingGold = 0)
        {
            _playerParty = new List<Character>(playerParty);
            Gold = startingGold;
        }

        private void Cleanup()
        {
            OnGoldChanged = null;
            OnMapChanged = null;
            OnRunEnded = null;
        }

        // ── 런 라이프사이클 ──

        public void StartRun()
        {
            IsRunActive = true;
            IsRunComplete = false;
            CurrentFloor = 1;
            SelectThemes();
            AddLog($"런 시작 — 총 {TotalFloors}스테이지" + (SelectedAscensionLevel > 0 ? $" (어센션 {SelectedAscensionLevel})" : ""));
            GenerateCurrentFloorMap();
        }

        /// <summary>런 시작 전 어센션 레벨 지정 (MapSceneSetup이 SaveManager.Meta에서 복사).</summary>
        public void SetSelectedAscensionLevel(int level)
        {
            SelectedAscensionLevel = level < 0 ? 0 : (level > 15 ? 15 : level);
        }

        /// <summary>
        /// 각 스테이지별 테마 무작위 채택 — 시작 시 1회 호출
        /// </summary>
        private void SelectThemes()
        {
            SelectedThemes.Clear();
            if (_themeCandidates == null || _themeCandidates.Count == 0)
            {
                for (int i = 0; i < TotalFloors; i++)
                    SelectedThemes.Add(null);
                return;
            }

            for (int stage = 1; stage <= TotalFloors; stage++)
            {
                if (stage <= _themeCandidates.Count)
                {
                    var pool = _themeCandidates[stage - 1];
                    if (pool != null && pool.Count > 0)
                    {
                        SelectedThemes.Add(pool[_rng.Next(pool.Count)]);
                        continue;
                    }
                }
                SelectedThemes.Add(null);
            }
        }

        /// <summary>
        /// 테마 후보 풀 주입 — MapSceneSetup에서 런 시작 전 호출
        /// </summary>
        public void SetThemeCandidates(List<List<StageThemeData>> candidates)
        {
            _themeCandidates = candidates;
        }

        /// <summary>
        /// 저장 로드용 — 이미 선택된 테마를 직접 복원
        /// </summary>
        public void RestoreSelectedThemes(List<StageThemeData> themes)
        {
            SelectedThemes.Clear();
            if (themes != null)
                SelectedThemes.AddRange(themes);
        }

        public void GenerateCurrentFloorMap()
        {
            var generator = new MapGenerator();
            CurrentMap = generator.GenerateFloor(CurrentFloor, CurrentStageTheme);
            CurrentMap.StartFloor();
            OnMapChanged?.Invoke(CurrentMap);
        }

        public void AdvanceToNextFloor()
        {
            if (CurrentFloor >= TotalFloors)
            {
                IsRunComplete = true;
                IsRunActive = false;
                AddLog("런 완료! 모든 층 클리어!");
                OnRunEnded?.Invoke();
                return;
            }

            CurrentFloor++;
            AddLog($"스테이지 {CurrentFloor}(으)로 진입");
            GenerateCurrentFloorMap();
            SaveManager.Save();
        }

        public void EndRunDefeat()
        {
            IsRunActive = false;
            AddLog("런 패배 — 파티 전멸");
            OnRunEnded?.Invoke();
        }

        public bool IsPartyWiped() => _playerParty.TrueForAll(p => p.IsDead);

        /// <summary>
        /// Phase CC-0: 전투 종료 처리.
        /// - victory=true: 생존자 HP 100% 회복, 사망자 50% 부활 + MaxHP 0.9배 누적.
        ///   파티가 완전히 전멸한 상태라면(이론적으로 victory=false와 동일) 패배 처리.
        /// - victory=false: 파티 전멸 → 런 종료.
        /// 반환값 true = 런 종료, false = 계속 진행.
        /// </summary>
        public bool ProcessBattleEnd(bool victory)
        {
            if (!victory || IsPartyWiped())
            {
                EndRunDefeat();
                return true;
            }

            int revived = 0;
            foreach (var c in _playerParty)
            {
                if (c.IsAlive)
                {
                    c.Health.HealToFull();
                }
                else
                {
                    // 사망자: MaxHP 0.9배 누적 후 50% HP 부활
                    c.Health.ApplyMaxHpModifier(0.9f);
                    c.Health.Revive(0.5f);
                    CombatEventBus.FirePartyMemberRevived(c);
                    revived++;
                }
            }

            if (revived > 0)
                AddLog($"사망자 {revived}명 부활 (MaxHP -10% 누적)");

            return false;
        }

        public void OnBattleVictory()
        {
            BattlesWon++;
            AddLog($"스테이지 {CurrentFloor} — 전투 승리");
        }

        // ── 경제 ──

        public void AddGold(int amount)
        {
            Gold += amount;
            TotalGoldEarned += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        public bool SpendGold(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        public float GetFloorScaling()
        {
            return CurrentFloor switch
            {
                1 => 1.0f,
                2 => 1.3f,
                3 => 1.6f,
                4 => 2.0f,
                _ => 1.0f + (CurrentFloor - 1) * 0.3f
            };
        }

        // ── 리롤 토큰 ──

        public void AddRerollTokens(int count) => RerollTokens += count;

        public bool SpendRerollToken()
        {
            if (RerollTokens <= 0) return false;
            RerollTokens--;
            return true;
        }

        public void RestoreRerollTokens(int tokens) => RerollTokens = tokens;

        // ── 휴식지 ──

        public int BonusAP { get; private set; }

        public void RestAtCampfire(float healPercent = 0.3f)
        {
            foreach (var member in _playerParty)
            {
                if (member.IsAlive)
                {
                    int healAmount = System.Math.Max(1, (int)(member.Health.MaxHP * healPercent));
                    member.Health.Heal(healAmount);
                }
            }
            AddLog("캠프파이어에서 휴식 — 파티 HP 회복");
        }

        public void TrainAtCampfire()
        {
            foreach (var member in _playerParty)
            {
                if (member.IsAlive)
                    member.Stats.AddPermanentBase(StatType.ATK, 1);
            }
            AddLog("캠프파이어에서 수련 — 파티 ATK 영구 증가");
        }

        public void MeditateAtCampfire()
        {
            BonusAP = 1;
            AddLog("캠프파이어에서 명상 — 다음 전투 AP 보너스");
        }

        public int ConsumeBonusAP()
        {
            int bonus = BonusAP;
            BonusAP = 0;
            return bonus;
        }

        public void RestoreBonusAP(int bonus) => BonusAP = bonus;

        // ── 데이터 풀 & 획득 ──

        public void SetDataPools(List<RelicData> relicPool = null, List<AugmentData> augmentPool = null)
        {
            _relicPool = relicPool ?? new List<RelicData>();
            _augmentPool = augmentPool ?? new List<AugmentData>();
            AugmentGenerator = new AugmentOfferGenerator(_augmentPool, _playerParty, _rng);
        }

        public RelicData PeekRandomRelic()
        {
            if (_relicPool == null || _relicPool.Count == 0) return null;
            return _relicPool[_rng.Next(_relicPool.Count)];
        }

        public AugmentData PeekRandomAugment()
        {
            if (_augmentPool == null || _augmentPool.Count == 0) return null;
            return _augmentPool[_rng.Next(_augmentPool.Count)];
        }

        public bool AcquireAugment(AugmentData augment, Character member, SkillInstance targetSkill)
        {
            if (augment == null || member == null || targetSkill == null) return false;
            bool applied = member.SkillInventory.ApplyAugmentToSkill(targetSkill, augment);
            if (applied)
                AddLog($"증강 획득: {augment.AugmentName} → {targetSkill.Data.SkillName}");
            return applied;
        }

        public void AcquireRelic(RelicData relic)
        {
            if (relic == null) return;
            RelicHandler.AddRelic(relic);
            AddLog($"유물 획득: {relic.RelicName}");
        }

        public bool RemoveRelic(RelicData relic)
        {
            if (relic == null) return false;
            bool removed = RelicHandler.RemoveRelic(relic);
            if (removed)
                AddLog($"유물 제거: {relic.RelicName}");
            return removed;
        }

        // ── 엘리트/스테이지 클리어 보너스 (Phase 7B/7C) ──

        /// <summary>
        /// 엘리트 격파 보너스 적용 — StageDesign 5.2
        /// </summary>
        public void ApplyEliteBonus(EliteBonusType type)
        {
            switch (type)
            {
                case EliteBonusType.BonusRelic:
                    var relic = PeekRandomRelic();
                    if (relic != null)
                    {
                        AcquireRelic(relic);
                        AddLog("엘리트 보너스: 유물 획득");
                    }
                    else
                    {
                        AddGold(50);
                        AddLog("엘리트 보너스: 유물 풀 비어 골드 +50 보상");
                    }
                    break;

                case EliteBonusType.PartyUpgrade:
                    int roll = _rng.Next(3);
                    foreach (var member in _playerParty)
                    {
                        if (!member.IsAlive) continue;
                        if (roll == 0)
                        {
                            member.Health.SetMaxHP(member.Health.MaxHP + 4);
                            member.Health.Heal(4);
                        }
                        else if (roll == 1)
                            member.Stats.AddPermanentBase(StatType.ATK, 1);
                        else
                            member.Stats.AddPermanentBase(StatType.DEF, 1);
                    }
                    string statName = roll == 0 ? "HP+4" : (roll == 1 ? "ATK+1" : "DEF+1");
                    AddLog($"엘리트 보너스: 파티 영구 강화 ({statName})");
                    break;

                case EliteBonusType.ShopDiscount:
                    if (0.5f > _pendingShopDiscount) _pendingShopDiscount = 0.5f;
                    AddGold(100);
                    AddLog("엘리트 보너스: 다음 상점 50% 할인 + 100G");
                    break;
            }
        }

        /// <summary>
        /// 스테이지 클리어 보너스 적용 — StageDesign 6.1
        /// </summary>
        public void ApplyStageClearBonus(StageClearBonusType type)
        {
            switch (type)
            {
                case StageClearBonusType.BurstReady:
                    BonusAP += 2;
                    AddLog("스테이지 보너스: 다음 스테이지 첫 전투 AP +2");
                    break;

                case StageClearBonusType.Recharge:
                    foreach (var member in _playerParty)
                    {
                        if (member.IsAlive)
                        {
                            int healAmount = member.Health.MaxHP / 2;
                            member.Health.Heal(healAmount);
                        }
                    }
                    AddLog("스테이지 보너스: 파티 HP 50% 회복");
                    break;

                case StageClearBonusType.IntelAdvantage:
                    _pendingShopExtraRelics++;
                    _pendingShopExtraAugments++;
                    AddLog("스테이지 보너스: 다음 상점 진열 추가 (유물+1, 증강+1)");
                    break;
            }
        }

        // ── 보류 상점 보너스 (상점 방문 시 소비) ──

        public float PeekShopDiscount() => _pendingShopDiscount;
        public int PeekPendingShopExtraRelics() => _pendingShopExtraRelics;
        public int PeekPendingShopExtraAugments() => _pendingShopExtraAugments;

        public float ConsumeShopDiscount()
        {
            float d = _pendingShopDiscount;
            _pendingShopDiscount = 0f;
            return d;
        }

        public int ConsumePendingShopExtraRelics()
        {
            int n = _pendingShopExtraRelics;
            _pendingShopExtraRelics = 0;
            return n;
        }

        public int ConsumePendingShopExtraAugments()
        {
            int n = _pendingShopExtraAugments;
            _pendingShopExtraAugments = 0;
            return n;
        }

        /// <summary>저장 로드용 — 보류 보너스 복원</summary>
        public void RestorePendingShopBonuses(float discount, int extraRelics, int extraAugments)
        {
            _pendingShopDiscount = discount;
            _pendingShopExtraRelics = extraRelics;
            _pendingShopExtraAugments = extraAugments;
        }

        private void AddLog(string entry)
        {
            _runHistory.Add($"[층 {CurrentFloor}] {entry}");
        }
    }
}
