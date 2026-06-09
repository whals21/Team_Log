using System.Collections.Generic;
using TeamLog.Characters;
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

        // 맵 진행
        public int CurrentFloor { get; private set; } = 1;
        public MapFloor CurrentMap { get; private set; }
        public int TotalFloors { get; } = 3;

        // 리소스
        public int Gold { get; private set; }
        public int RerollTokens { get; private set; } = 1;

        // 통계
        public int BattlesWon { get; private set; }
        public int TotalGoldEarned { get; private set; }

        // 파티
        public IReadOnlyList<Character> PlayerParty => _playerParty;

        // 유물
        public RelicHandler RelicHandler { get; } = new();

        // 증강 보상 생성기
        public AugmentOfferGenerator AugmentGenerator { get; private set; }

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
            GenerateCurrentFloorMap();
        }

        public void GenerateCurrentFloorMap()
        {
            var generator = new MapGenerator();
            CurrentMap = generator.GenerateFloor(CurrentFloor);
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
            AddLog($"층 {CurrentFloor}(으)로 진입");
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

        public void OnBattleVictory()
        {
            BattlesWon++;
            AddLog($"층 {CurrentFloor} — 전투 승리");
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

        private void AddLog(string entry)
        {
            _runHistory.Add($"[층 {CurrentFloor}] {entry}");
        }
    }
}
