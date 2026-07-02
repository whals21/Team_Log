using System;

namespace TeamLog.Characters
{
    /// <summary>
    /// 쉴드 속성 플래그 — 흡수 시 부가 효과 (Phase CC P1).
    /// Taranis Grounding Field는 GivesChargeOnAbsorb — 흡수 시 공격자에게 Charge 부여.
    /// </summary>
    [Flags]
    public enum ShieldFlag
    {
        None = 0,
        GivesChargeOnAbsorb = 1,  // 흡수 시 공격자에게 Charge 1스택 부여 (Taranis Grounding Field)
    }

    /// <summary>
    /// 개별 쉴드 인스턴스 — 부여자(Caster) + 양 + 속성 플래그 추적.
    /// HealthComponent._shields 리스트의 원소.
    /// 흡수 시 부여자에게 알림 → Duran Vengeance 원격 축적, Taranis Charge 역부여.
    /// </summary>
    public class ShieldInstance
    {
        public Character Caster;       // 쉴드를 부여한 캐릭터 (null = 부여자 불명, 기본 취급)
        public int Amount;             // 남은 쉴드 양
        public ShieldFlag Flags;       // 속성 (None / GivesChargeOnAbsorb)
    }
}
