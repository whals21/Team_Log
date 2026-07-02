using TeamLog.Combat;
using StatType = TeamLog.Characters.StatType;

namespace TeamLog.Skill.Behaviors.Implementations
{
    /// <summary>
    /// Pierce — 쉴드 + DEF 완전 무시 (Phase ARCH-2 추출).
    /// TakeDirectDamage를 사용해 쉴드를 우회하고, 위력 + 시전자 ATK를 직접 HP에 적용.
    /// </summary>
    public class PierceBehavior : ISkillBehavior
    {
        public BehaviorKeyword Keyword => BehaviorKeyword.Pierce;
        public ExecutionPhase Phases => ExecutionPhase.ApplyMain;
        public int Order => 10; // ApplyMain 중 가장 먼저 (기본 DealDamage 스킵)

        public void ApplyMain(SkillExecContext ctx)
        {
            var caster = ctx.Caster;
            var target = ctx.InitialTarget;
            if (caster == null || target == null || !target.IsAlive) return;

            // Pierce 데미지 = 시전자 ATK + 위력 (DEF/쉴드 무시)
            int pierceDamage = System.Math.Max(1, caster.Stats.GetStat(StatType.ATK) + ctx.CurrentPower);

            int hpBefore = target.Health.CurrentHP;
            target.Health.TakeDirectDamage(pierceDamage);
            int actualDealt = hpBefore - target.Health.CurrentHP;

            // 이벤트 발생
            CombatEventBus.FireDamageDealt(caster, target, actualDealt);
            CombatEventBus.FireDamageReceived(target, actualDealt);

            // Lifesteal 등 후속 Behavior가 참조할 수 있도록 기록
            ctx.LastActualDamage = actualDealt;

            // 기본 본 효과를 스킵 — Pierce가 모든 데미지 처리를 담당
            ctx.SkipDefaultApply = true;

            // Phase CC: Pierce에도 피격 훅 작동 (Duran Vengeance 축적 등)
            if (target.Resource != null && actualDealt > 0)
                target.Resource.OnDamageTaken(target, actualDealt);
        }
    }
}
