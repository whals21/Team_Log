using System.Collections.Generic;
using System.Linq;
using TeamLog.Combat;
using TeamLog.Map;
using TeamLog.Skill;

namespace TeamLog.Characters
{
    /// <summary>
    /// Mercy 자원 — Elara (Healer) 고유 메카닉 (Phase CC-2C).
    ///
    /// 핵심 루프 (기획: ReworkDrafts/01_Healer.md):
    /// - 매 턴 시작 시 연결된 파티원(생존자)에게 자동 힐 1 (BondBoost 시 +3 = 4)
    /// - 자동 힐 + Healer 직접 힐 시전이 Mercy 축전
    /// - Mercy 15 도달 시 자동 버스트: 가장 많이 회복받은 파티원에게 ATK+3 (3턴)
    /// - Mercy 0 리셋 + 해당 파티원 회복량 추적도 리셋
    ///
    /// 전략: 파티원 행동을 보조하며 Mercy 축전 → 자동 버스트로 파티 강화.
    /// "힐과 버프의 영원한 순환" — 사용자 제안 컨셉.
    ///
    /// ★ 2026-08-03 밸런스 조정: BaseAutoHeal 3 → 1 (자동 힐 과다로 인한 밸런스 완화)
    /// </summary>
    public class MercyResourceComponent : CharacterResourceComponent
    {
        public override ResourceType Resource => ResourceType.Mercy;
        public override int MaxStacks => 15;

        /// <summary>Mercy 버스트 임계값 (15 누적 시 자동 발동).</summary>
        public const int BurstThreshold = 15;

        /// <summary>기본 자동 힐 위력 (BondBoost 시 +3 = 4, 축복 특성 시 +2 = 3).</summary>
        public const int BaseAutoHeal = 1;

        /// <summary>BondBoost 상태일 때 추가 힐 위력.</summary>
        public const int BondBoostBonus = 3;

        private readonly Dictionary<Character, int> _healingByMember = new();
        private bool _subscribed;

        public override void OnTurnStart(Character owner)
        {
            // 매 턴 시작 시 연결된 파티원에게 자동 힐
            AutoHealPartyMembers();
        }

        public override void OnTurnEnd(Character owner)
        {
            // Mercy는 리셋 없음 (버스트 시에만 리셋)
        }

        /// <summary>매 턴 시작 시 생존 파티원에게 자동 힐. 자동 힐도 Mercy 축전. 본인(Healer)은 제외.</summary>
        private void AutoHealPartyMembers()
        {
            var owner = Owner;
            if (owner == null) return;
            var party = GetPlayerParty();
            if (party == null) return;

            // AutoHealBonus 특성 (기본 0, "축복" 특성 시 +2)
            int bonus = 0;
            if (owner.PlayerTraitHandler != null && owner.PlayerTraitHandler.HasTrait)
                bonus = owner.PlayerTraitHandler.QueryKeywordSum(KeywordType.AutoHealBonus);

            int healAmount = BaseAutoHeal + bonus;

            foreach (var member in party)
            {
                if (member == null || !member.IsAlive) continue;
                // ★ 본인(Healer)은 자동 힐 대상에서 제외 — 기획: "파티원 행동 보상"
                if (member == owner) continue;

                // BondBoost 상태 시 추가 힐
                int amount = healAmount;
                if (member.StatusEffects != null && member.StatusEffects.HasEffect(StatusEffectType.BondBoost))
                    amount += BondBoostBonus;

                member.Health?.Heal(amount);
                AccumulateMercy(member, amount);
            }
        }

        /// <summary>Mend Wounds/Sanctuary 스킬의 MercyAccumulate Behavior가 호출.</summary>
        public void AccumulateFromDirectHeal(Character target, int healAmount)
        {
            AccumulateMercy(target, healAmount);
        }

        private void AccumulateMercy(Character member, int amount)
        {
            if (amount <= 0 || member == null) return;

            // 파티원별 추적
            if (!_healingByMember.ContainsKey(member)) _healingByMember[member] = 0;
            _healingByMember[member] += amount;

            // Mercy (Healer 본인 총 스택) 축전
            int before = CurrentStacks;
            AddStacks(amount);

            // 임계값 도달 시 자동 버스트 (한 번에 여러 번 버스트 가능 — Sanctuary 같은 대형 힐 시)
            while (CurrentStacks >= MaxStacks)
            {
                bool bursted = TryAutoBurst();
                if (!bursted) break;
            }
        }

        /// <summary>Mercy 15 도달 시 자동 버스트. 가장 많이 회복받은 파티원에게 ATK+3.</summary>
        private bool TryAutoBurst()
        {
            var owner = Owner;
            if (owner == null || _healingByMember.Count == 0) return false;

            // 버스트 대상 수 (기본 1, "신성 방패" 특성 MercyBurstTargets 시 N)
            int targets = 1;
            if (owner.PlayerTraitHandler != null && owner.PlayerTraitHandler.HasTrait)
            {
                int bonusTargets = owner.PlayerTraitHandler.QueryKeywordSum(KeywordType.MercyBurstTargets);
                if (bonusTargets > 0) targets = bonusTargets;
            }

            // 가장 많이 회복받은 파티원부터 N명
            var sorted = _healingByMember.OrderByDescending(kvp => kvp.Value).ToList();
            int bursted = 0;
            foreach (var kvp in sorted)
            {
                if (bursted >= targets) break;
                var member = kvp.Key;
                if (member == null || !member.IsAlive) continue;

                ApplyBurstBuff(member);
                _healingByMember[member] = 0; // 해당 파티원 회복량 리셋
                bursted++;
            }

            // ★ 버스트 성공 시에만 Mercy 15 소모 (초과분 유지 → 다중 버스트 지원)
            // while 루프에서 CurrentStacks >= MaxStacks이면 반복 호출됨
            if (bursted > 0)
                ConsumeStacks(MaxStacks);

            return bursted > 0;
        }

        /// <summary>파티원에게 ATK+3 (3턴) 버스트. AttackUp 상태이상 활용.</summary>
        private void ApplyBurstBuff(Character member)
        {
            if (member?.StatusEffects == null) return;
            member.StatusEffects.ApplyEffect(StatusEffectType.AttackUp, 3, 3); // duration=3, value=3
        }

        /// <summary>파티 목록 조회 — GameRunState.Instance.PlayerParty.</summary>
        private IReadOnlyList<Character> GetPlayerParty()
        {
            return GameRunState.Instance?.PlayerParty;
        }
    }
}
