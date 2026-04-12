using System.Collections.Generic;
using TeamLog.Characters;

namespace TeamLog.Map
{
    /// <summary>
    /// 로그라이크 런 상태 — 순수 C# 클래스
    /// 전체 플레이 세션의 진행 상태를 관리
    /// </summary>
    public class GameRunState
    {
        private readonly List<Character> _playerParty;
        private readonly List<Character> _inventory = new();
        private readonly List<string> _runHistory = new();

        // 맵 진행
        public int CurrentFloor { get; private set; } = 1;
        public MapFloor CurrentMap { get; private set; }
        public int TotalFloors { get; } = 3;

        // 리소스
        public int Gold { get; private set; }

        // 파티
        public IReadOnlyList<Character> PlayerParty => _playerParty;

        // 인벤토리
        public IReadOnlyList<Character> Inventory => _inventory;

        // 이력
        public IReadOnlyList<string> RunHistory => _runHistory;

        public bool IsRunActive { get; private set; }
        public bool IsRunComplete { get; private set; }

        public event System.Action<int> OnGoldChanged;
        public event System.Action<MapFloor> OnMapChanged;
        public event System.Action OnRunEnded;

        public GameRunState(List<Character> playerParty, int startingGold = 0)
        {
            _playerParty = new List<Character>(playerParty);
            Gold = startingGold;
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
    }
}
