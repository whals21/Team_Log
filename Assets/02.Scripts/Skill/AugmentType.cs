namespace TeamLog.Skill
{
    /// <summary>
    /// 증강(Augment) 타입 — 스킬에 부착하여 효과 강화/변형
    /// </summary>
    public enum AugmentType
    {
        CostDown,       // 코스트 -1 (최소 0)
        Spread,         // 단일 → 광역 (위력 70%)
        Pierce,         // 쉴드 무시 + 방어력 50% 무시
        Chain,          // 타격 후 인접 적에게 위력 50% 연쇄
        Drain,          // 데미지의 30%를 자신 HP 회복
        HeavyHit,       // 위력 1.5배, 코스트 +1
        QuickDraw,      // 가중치 0 (무조건 뽑힘), 위력 80%
        Lingering,      // 상태이상 지속시간 +2턴
        Intensify,      // 버프/디버프 효과 1.5배
        VenomTouch,     // 공격 시 중독 추가 (2턴, 위력 30%)
        BurningTouch,   // 공격 시 화상 추가 (2턴, 위력 30%)
        ShieldBonus,    // 쉴드 효과 1.5배
        HealBonus,      // 힐 효과 1.5배

        // 저주 증강 (강력한 효과 + 페널티)
        BloodPact,      // 위력 +5, 매턴 HP 2 감소
        GlassCannon,    // 위력 +8, 받는 피해 +50%
        Reaper,         // 처치 시 HP 10 회복, 코스트 +1
        AOEAuto,        // 자동 광역 (위력 50%), 코스트 +2
        Berserk         // 위력 x2, HP 30% 이하일 때만 발동
    }
}
