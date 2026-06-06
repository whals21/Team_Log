using TeamLog.Characters;

namespace TeamLog.Combat
{
    /// <summary>
    /// 전투 중앙 이벤트 버스 — 유물 트리거를 위한 전투 이벤트 브로드캐스트
    /// TurnManager에서 발생시키고 RelicHandler에서 구독
    /// </summary>
    public static class CombatEventBus
    {
        public static event System.Action OnBattleStart;
        public static event System.Action<bool> OnBattleEnd; // victory
        public static event System.Action<int> OnTurnStart;  // turnNumber
        public static event System.Action OnTurnEnd;
        public static event System.Action<Character, Character, int> OnDamageDealt;    // attacker, target, amount
        public static event System.Action<Character, int> OnDamageReceived;              // target, amount
        public static event System.Action<Character> OnKill;                             // killed
        public static event System.Action<Character, int> OnHealApplied;                 // target, amount
        public static event System.Action<Character, int> OnShieldGained;                // target, amount
        public static event System.Action<int> OnGoldEarned;                             // amount
        public static event System.Action<SkillData, Character> OnSkillUsed;             // skill, caster

        public static void FireBattleStart() => OnBattleStart?.Invoke();
        public static void FireBattleEnd(bool victory) => OnBattleEnd?.Invoke(victory);
        public static void FireTurnStart(int turnNumber) => OnTurnStart?.Invoke(turnNumber);
        public static void FireTurnEnd() => OnTurnEnd?.Invoke();
        public static void FireDamageDealt(Character attacker, Character target, int amount) => OnDamageDealt?.Invoke(attacker, target, amount);
        public static void FireDamageReceived(Character target, int amount) => OnDamageReceived?.Invoke(target, amount);
        public static void FireKill(Character killed) => OnKill?.Invoke(killed);
        public static void FireHealApplied(Character target, int amount) => OnHealApplied?.Invoke(target, amount);
        public static void FireShieldGained(Character target, int amount) => OnShieldGained?.Invoke(target, amount);
        public static void FireGoldEarned(int amount) => OnGoldEarned?.Invoke(amount);
        public static void FireSkillUsed(SkillData skill, Character caster) => OnSkillUsed?.Invoke(skill, caster);

        /// <summary>
        /// 전투 종료 시 모든 이벤트 구독 해제 — 이벤트 누수 방지
        /// </summary>
        public static void Clear()
        {
            OnBattleStart = null;
            OnBattleEnd = null;
            OnTurnStart = null;
            OnTurnEnd = null;
            OnDamageDealt = null;
            OnDamageReceived = null;
            OnKill = null;
            OnHealApplied = null;
            OnShieldGained = null;
            OnGoldEarned = null;
            OnSkillUsed = null;
        }
    }
}
