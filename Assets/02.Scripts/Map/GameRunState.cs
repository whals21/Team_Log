using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Reward;

namespace TeamLog.Map
{
    /// <summary>
    /// 로그라이크 런 상태 — 정적 싱글톤 순수 C# 클래스
    /// 전체 플레이 세션의 진행 상태를 관리, 씬 전환 시에도 유지
    /// </summary>
    public class GameRunState
    {
        private static GameRunState _instance;

        /// <summary>
        /// 현재 런 인스턴스 (씬 전환 시에도 유지)
        /// </summary>
        public static GameRunState Instance => _instance;

        /// <summary>
        /// 새 런 생성 — 기존 인스턴스가 있으면 파괴 후 생성
        /// </summary>
        public static GameRunState Create(List<Character> playerParty, int startingGold = 0)
        {
            if (_instance != null)
                _instance.Cleanup();
            _instance = new GameRunState(playerParty, startingGold);
            return _instance;
        }

        /// <summary>
        /// 런 종료 시 인스턴스 파괴
        /// </summary>
        public static void Destroy()
        {
            if (_instance != null)
            {
                _instance.Cleanup();
                _instance = null;
            }
        }

        private readonly List<Character> _playerParty;
        private readonly List<ItemData> _acquiredItems = new();
        private readonly List<string> _runHistory = new();
        private readonly System.Random _rng = new();

        // 데이터 풀 (MapSceneSetup에서 주입)
        private List<SkillData> _skillPool;
        private List<ItemData> _itemPool;

        // 맵 진행
        public int CurrentFloor { get; private set; } = 1;
        public MapFloor CurrentMap { get; private set; }
        public int TotalFloors { get; } = 3;

        // 리소스
        public int Gold { get; private set; }

        // 파티
        public IReadOnlyList<Character> PlayerParty => _playerParty;

        // 인벤토리
        public IReadOnlyList<ItemData> AcquiredItems => _acquiredItems;

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

        /// <summary>
        /// 런 시작 — 첫 층 맵 생성
        /// </summary>
        public void StartRun()
        {
            IsRunActive = true;
            IsRunComplete = false;
            CurrentFloor = 1;
            GenerateCurrentFloorMap();
        }

        /// <summary>
        /// 현재 층의 맵 생성
        /// </summary>
        public void GenerateCurrentFloorMap()
        {
            var generator = new MapGenerator();
            CurrentMap = generator.GenerateFloor(CurrentFloor);
            CurrentMap.StartFloor();
            OnMapChanged?.Invoke(CurrentMap);
        }

        /// <summary>
        /// 골드 추가
        /// </summary>
        public void AddGold(int amount)
        {
            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        /// <summary>
        /// 골드 사용 — 성공 시 true, 부족 시 false
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        /// <summary>
        /// 전투 승리 처리
        /// </summary>
        public void OnBattleVictory()
        {
            AddLog($"층 {CurrentFloor} — 전투 승리");
        }

        /// <summary>
        /// 보스 격파 시 다음 층으로 이동
        /// </summary>
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
        }

        /// <summary>
        /// 파티 전체 HP 비율 (0.0 ~ 1.0)
        /// </summary>
        public float GetPartyHealthRatio()
        {
            if (_playerParty.Count == 0) return 0f;

            float totalRatio = 0f;
            foreach (var member in _playerParty)
            {
                if (member.IsAlive)
                    totalRatio += (float)member.Health.CurrentHP / member.Health.MaxHP;
            }
            return totalRatio / _playerParty.Count;
        }

        /// <summary>
        /// 파티 전원 사망 여부
        /// </summary>
        public bool IsPartyWiped()
        {
            return _playerParty.TrueForAll(p => p.IsDead);
        }

        /// <summary>
        /// 런 종료 (패배)
        /// </summary>
        public void EndRunDefeat()
        {
            IsRunActive = false;
            AddLog("런 패배 — 파티 전멸");
            OnRunEnded?.Invoke();
        }

        /// <summary>
        /// 휴식 노드 — 파티 전체 HP 회복
        /// </summary>
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

        private void AddLog(string entry)
        {
            _runHistory.Add($"[층 {CurrentFloor}] {entry}");
        }

        /// <summary>
        /// 스킬/아이템 데이터 풀 주입 (MapSceneSetup에서 호출)
        /// </summary>
        public void SetDataPools(List<SkillData> skillPool, List<ItemData> itemPool)
        {
            _skillPool = skillPool;
            _itemPool = itemPool;
        }

        /// <summary>
        /// 풀에서 랜덤 스킬 조회 (실제 획득하지 않음 — 보상 미리보기용)
        /// </summary>
        public SkillData PeekRandomSkill()
        {
            if (_skillPool == null || _skillPool.Count == 0) return null;
            return _skillPool[_rng.Next(_skillPool.Count)];
        }

        /// <summary>
        /// 풀에서 랜덤 아이템 조회 (실제 획득하지 않음 — 보상 미리보기용)
        /// </summary>
        public ItemData PeekRandomItem()
        {
            if (_itemPool == null || _itemPool.Count == 0) return null;
            return _itemPool[_rng.Next(_itemPool.Count)];
        }

        /// <summary>
        /// 풀에서 랜덤 스킬 획득 — 첫 번째 생존 파티원에게 추가
        /// </summary>
        public SkillData AcquireRandomSkill()
        {
            if (_skillPool == null || _skillPool.Count == 0) return null;
            var skill = _skillPool[_rng.Next(_skillPool.Count)];
            foreach (var member in _playerParty)
            {
                if (member.IsAlive) { member.SkillInventory.AddSkill(skill); break; }
            }
            AddLog($"스킬 획득: {skill.SkillName}");
            return skill;
        }

        /// <summary>
        /// 풀에서 랜덤 아이템 획득
        /// </summary>
        public ItemData AcquireRandomItem()
        {
            if (_itemPool == null || _itemPool.Count == 0) return null;
            var item = _itemPool[_rng.Next(_itemPool.Count)];
            _acquiredItems.Add(item);
            AddLog($"아이템 획득: {item.ItemName}");
            return item;
        }

        /// <summary>
        /// 특정 스킬 획득
        /// </summary>
        public void AcquireSkill(SkillData skill)
        {
            if (skill == null) return;
            foreach (var member in _playerParty)
            {
                if (member.IsAlive) { member.SkillInventory.AddSkill(skill); break; }
            }
            AddLog($"스킬 획득: {skill.SkillName}");
        }

        /// <summary>
        /// 특정 아이템 획득
        /// </summary>
        public void AcquireItem(ItemData item)
        {
            if (item == null) return;
            _acquiredItems.Add(item);
            AddLog($"아이템 획득: {item.ItemName}");
        }
    }
}
