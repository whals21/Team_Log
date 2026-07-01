namespace TeamLog.Characters
{
    /// <summary>
    /// Frost 자원 — Lumi (Cryomancer) 고유 메카닉 (Phase CC).
    ///
    /// 핵심 루프 (기획: Characters/Lumi_the_Cryomancer.md):
    /// - 냉기 스킬 사용 시 +1 (별도 훅 필요 — 스킬 실행 시)
    /// - 최대 3스택
    /// - 턴 종료 시 절반 소실 (MaxStacks/2, 최소 1)
    /// - 3스택 도달 시 다음 냉기 마법 강화 (Freeze 1→2턴) — 정식 구현 시
    ///
    /// 이 버전: 스택 관리 + 절반 소실만 구현. 3스택 강화는 별도 작업.
    /// </summary>
    public class FrostResourceComponent : CharacterResourceComponent
    {
        public override ResourceType Resource => ResourceType.Frost;
        public override int MaxStacks => 3;

        /// <summary>매 턴 종료 시 절반 소실. Lumi의 균형추 — 방치하면 Frost가 녹아내림.</summary>
        public override void OnTurnEnd(Character owner)
        {
            if (CurrentStacks > 0)
            {
                int loss = System.Math.Max(1, CurrentStacks / 2);
                CurrentStacks = System.Math.Max(0, CurrentStacks - loss);
            }
        }
    }
}
