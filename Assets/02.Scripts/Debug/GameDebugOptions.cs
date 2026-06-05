using System.ComponentModel;
using SRDebugger;
using SRDebugger.Services;
using TeamLog.Characters;
using TeamLog.Map;
using UnityEngine;

namespace TeamLog.EditorDebug
{
    /// <summary>
    /// SRDebugger 인게임 디버그 옵션 — 런타임 치트/상태 확인
    /// SRDebugger 트리플 탭 또는 Ctrl+Shift+F1으로 열기
    /// AddOptionContainer(object)가 자동 리플렉션으로 속성/메서드를 발견
    /// </summary>
    public class GameDebugOptions
    {
        private static GameDebugOptions _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            _instance = new GameDebugOptions();
            global::SRDebug.Instance?.AddOptionContainer(_instance);
        }

        // ── 런 상태 ──────────────────────────────────────────

        [Category("Run State")]
        [DisplayName("현재 층")]
        [Sort(0)]
        public int CurrentFloor => GameRunState.Instance?.CurrentFloor ?? 0;

        [Category("Run State")]
        [DisplayName("골드")]
        [Sort(1)]
        public int Gold => GameRunState.Instance?.Gold ?? 0;

        [Category("Run State")]
        [DisplayName("보너스 AP")]
        [Sort(2)]
        public int BonusAP => GameRunState.Instance?.BonusAP ?? 0;

        [Category("Run State")]
        [DisplayName("런 활성")]
        [Sort(3)]
        public bool IsRunActive => GameRunState.Instance != null;

        // ── 치트: 골드 ────────────────────────────────────────

        [Category("Cheats")]
        [DisplayName("골드 +100")]
        [Sort(10)]
        public void AddGold()
        {
            if (GameRunState.Instance != null)
            {
                GameRunState.Instance.AddGold(100);
                OnPropertyChanged(nameof(Gold));
            }
        }

        [Category("Cheats")]
        [DisplayName("골드 +500")]
        [Sort(11)]
        public void AddGold500()
        {
            if (GameRunState.Instance != null)
            {
                GameRunState.Instance.AddGold(500);
                OnPropertyChanged(nameof(Gold));
            }
        }

        // ── 치트: HP 회복 ─────────────────────────────────────

        [Category("Cheats")]
        [DisplayName("파티 전원 HP 회복")]
        [Sort(20)]
        public void HealParty()
        {
            if (GameRunState.Instance == null) return;
            foreach (var c in GameRunState.Instance.PlayerParty)
            {
                if (c.IsAlive)
                    c.Health.Heal(c.Health.MaxHP);
            }
        }

        [Category("Cheats")]
        [DisplayName("파티 전원 사망")]
        [Sort(21)]
        public void KillParty()
        {
            if (GameRunState.Instance == null) return;
            foreach (var c in GameRunState.Instance.PlayerParty)
                c.Health.TakeDamage(c.Health.CurrentHP + c.Health.CurrentShield);
        }

        // ── 치트: 층 이동 ─────────────────────────────────────

        [Category("Cheats")]
        [DisplayName("다음 층으로")]
        [Sort(30)]
        public void NextFloor()
        {
            GameRunState.Instance?.AdvanceToNextFloor();
            OnPropertyChanged(nameof(CurrentFloor));
        }

        // ── IOptionContainer ──────────────────────────────────

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
