using TeamLog.Combat;

namespace TeamLog.Characters
{
    /// <summary>
    /// Ember 자원 — Ashe (Pyromancer) 고유 메카닉 (Phase CC).
    ///
    /// 핵심 루프 (기획: Characters/Ashe_the_Pyromancer.md):
    /// - 매 턴 시작 시 Ember +1 (강제 부여 — 자원이 쌓이는 것이 컨셉)
    /// - 턴 종료 시 Ember × 2 자기 피해 (자해)
    /// - 최대 5스택. 화염 스킬로 충전/소비
    ///
    /// 전략: 자해 위험을 감수하고 폭딜. 부활 시스템과 시너지 (사망 시 50% 부활).
    /// </summary>
    public class EmberResourceComponent : CharacterResourceComponent
    {
        public override ResourceType Resource => ResourceType.Ember;
        public override int MaxStacks => 5;

        /// <summary>매 턴 시작 시 Ember +1 강제 부여. Ashe의 자원은 시간이 지나면 자연 축적.</summary>
        public override void OnTurnStart(Character owner)
        {
            AddStacks(1);
        }

        /// <summary>매 턴 종료 시 Ember × 2 자해. Ashe의 핵심 페널티 — 방치하면 HP가 녹아내림.</summary>
        public override void OnTurnEnd(Character owner)
        {
            if (owner == null || !owner.IsAlive) return;
            int selfDamage = CurrentStacks * 2;
            if (selfDamage > 0)
            {
                owner.Health.TakeDamage(selfDamage);
                CombatEventBus.FireDamageReceived(owner, selfDamage);
            }
        }
    }
}
