namespace TeamLog.Characters
{
    /// <summary>
    /// Vengeance 자원 — Duran (Warrior) 고유 메카닉 (Phase CC).
    ///
    /// 핵심 루프 (기획: Characters/Duran_the_Warrior.md):
    /// - 피격 시 받은 데미지 1:1 축적 (쉴드 흡분 포함)
    /// - 최대 20스택. 자연 감소 없음 (안 맞으면 자연히 안 쌓임 — 자연 제한)
    /// - 소비 스킬(Revenge Strike/Last Bastion)로만 감소
    ///
    /// 전략: 매질 탱커 — 맞으면서 축적, Vengeance 소비 스킬로 버스트 딜.
    /// </summary>
    public class VengeanceResourceComponent : CharacterResourceComponent
    {
        public override ResourceType Resource => ResourceType.Vengeance;
        public override int MaxStacks => 20;

        /// <summary>피격 시 받은 데미지 1:1 축적. Duran의 핵심 — 맞을수록 강해진다.</summary>
        public override void OnDamageTaken(Character owner, int damage)
        {
            if (damage > 0)
                AddStacks(damage);
        }
    }
}
