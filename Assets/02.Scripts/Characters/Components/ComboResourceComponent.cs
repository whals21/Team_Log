using TeamLog.Combat;

namespace TeamLog.Characters
{
    /// <summary>
    /// Combo 자원 — Aster (Archer) 고유 메카닉 (Phase CC-2B).
    ///
    /// 핵심 루프 (기획: ReworkDrafts/03_Archer.md):
    /// - 매 턴 스킬을 1개라도 사용하면 Combo +1 (최대 3)
    /// - 이번 턴 스킬 미사용 시 턴 종료에 Combo = 0 리셋
    /// - Combo 소모 스킬 (Multi-Shot/Execute Shot) 사용 시 위력/타수 증폭
    /// - Execute Shot 킬 시 Combo 3 복구 (ComboFinisherBehavior)
    ///
    /// 전략: 매 턴 스킬을 계속 쏘며 Combo 유지 → Execute Shot로 폭딜.
    /// Umbra(Shadows, "안 맞을 때")와 정반대 — "계속 쏠 때" 축전.
    /// </summary>
    public class ComboResourceComponent : CharacterResourceComponent
    {
        public override ResourceType Resource => ResourceType.Combo;
        public override int MaxStacks => 3;

        private bool _usedSkillThisTurn;
        private bool _subscribed;

        public override void OnTurnStart(Character owner)
        {
            // 첫 턴 시작 시 OnSkillUsed 구독 (한 번만)
            if (!_subscribed)
            {
                CombatEventBus.OnSkillUsed += OnSkillUsed;
                _subscribed = true;
            }
            // 이번 턴 스킬 사용 플래그 리셋
            _usedSkillThisTurn = false;
        }

        public override void OnTurnEnd(Character owner)
        {
            if (_usedSkillThisTurn)
            {
                // 스킬 사용함 → Combo +1 (이미 최대치면 AddStacks 내부에서 clamp)
                AddStacks(1);
            }
            else
            {
                // 스킬 미사용 → Combo 전부 상실
                if (CurrentStacks > 0)
                    ConsumeStacks(CurrentStacks);
            }
        }

        private void OnSkillUsed(SkillData skill, Character caster)
        {
            // Owner가 시전한 스킬만 카운트
            if (caster == null) return;

            // owner 비교: Character 참조는 외부에서 주입되지 않으므로,
            // CombatEventBus.OnSkillUsed가 발생한 시전자가 이 컴포넌트의 owner인지
            // 확인할 방법이 필요. 여기서는 caster.Resource == this로 확인.
            if (caster.Resource == this)
                _usedSkillThisTurn = true;
        }
    }
}
