namespace TeamLog.Combat.AI
{
    /// <summary>
    /// 적 행동 의도 타입
    /// </summary>
    public enum EnemyIntentType
    {
        None,
        Attack,         // 공격 예정
        HeavyAttack,    // 강력한 공격 예정
        MultiAttack,    // 다중 공격 예정
        Defend,         // 방어 예정
        Buff,           // 버프 예정
        Debuff,         // 디버프 예정
        Heal,           // 치유 예정
        Summon,         // 소환 예정
        Charge,         // 차지 (다음 턴 강화)
        Unknown         // 알 수 없음
    }

    /// <summary>
    /// 적 행동 의도 정보
    /// </summary>
    public class EnemyIntent
    {
        public EnemyIntentType Type { get; }
        public int Value { get; }
        public int TargetCount { get; }
        public string Description { get; }

        public EnemyIntent(EnemyIntentType type, int value = 0, int targetCount = 1, string description = "")
        {
            Type = type;
            Value = value;
            TargetCount = targetCount;
            Description = description;
        }

        public string GetDisplayText()
        {
            return Type switch
            {
                EnemyIntentType.Attack => $"공격 {Value}",
                EnemyIntentType.HeavyAttack => $"강공격 {Value}",
                EnemyIntentType.MultiAttack => $"다중공격 x{TargetCount}",
                EnemyIntentType.Defend => "방어",
                EnemyIntentType.Buff => "강화",
                EnemyIntentType.Debuff => "약화",
                EnemyIntentType.Heal => $"치유 {Value}",
                EnemyIntentType.Summon => "소환",
                EnemyIntentType.Charge => "충전",
                EnemyIntentType.Unknown => "???",
                _ => ""
            };
        }
    }
}
