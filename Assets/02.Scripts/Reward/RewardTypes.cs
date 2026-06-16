namespace TeamLog.Reward
{
    public enum RewardRarity
    {
        Common,
        Rare,
        Unique
    }

    public enum RewardType
    {
        Gold,
        Relic,
        Augment,
        AugmentOffer
    }

    /// <summary>
    /// 엘리트 전투 승리 시 3택 1 보너스 — StageDesign 5.2
    /// </summary>
    public enum EliteBonusType
    {
        BonusRelic,        // 추가 유물 수령 (일반 등급 1개)
        PartyUpgrade,      // 전원 HP+15 / ATK+2 / DEF+2 (내부 랜덤)
        ShopDiscount       // 다음 상점 50% 할인 + 골드 +100
    }

    /// <summary>
    /// 스테이지 클리어(보스 격파) 시 3택 1 보너스 — StageDesign 6.1
    /// </summary>
    public enum StageClearBonusType
    {
        BurstReady,        // 다음 스테이지 첫 전투 AP +2
        Recharge,          // 파티 전원 HP 50% 회복
        IntelAdvantage     // 다음 상점 진열 추가 (유물+1, 증강+1)
    }
}
