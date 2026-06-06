using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Combat;

namespace TeamLog.Reward
{
    /// <summary>
    /// 유물 처리기 — GameRunState에 소속된 전체 파티 유물을 관리
    /// CombatEventBus를 구독하여 트리거 매칭 → 효과 적용
    /// </summary>
    public class RelicHandler
    {
        private readonly List<RelicData> _relics = new();
        public IReadOnlyList<RelicData> Relics => _relics;

        // 누적형 효과 상태
        private int _killStackCount;

        public void AddRelic(RelicData relic)
        {
            if (relic != null && !_relics.Contains(relic))
                _relics.Add(relic);
        }

        public void SubscribeEvents()
        {
            CombatEventBus.OnBattleStart += OnBattleStart;
            CombatEventBus.OnTurnStart += OnTurnStart;
            CombatEventBus.OnTurnEnd += OnTurnEnd;
            CombatEventBus.OnDamageDealt += OnDamageDealt;
            CombatEventBus.OnDamageReceived += OnDamageReceived;
            CombatEventBus.OnKill += OnKill;
            CombatEventBus.OnHealApplied += OnHealApplied;
            CombatEventBus.OnShieldGained += OnShieldGained;
        }

        public void UnsubscribeEvents()
        {
            CombatEventBus.OnBattleStart -= OnBattleStart;
            CombatEventBus.OnTurnStart -= OnTurnStart;
            CombatEventBus.OnTurnEnd -= OnTurnEnd;
            CombatEventBus.OnDamageDealt -= OnDamageDealt;
            CombatEventBus.OnDamageReceived -= OnDamageReceived;
            CombatEventBus.OnKill -= OnKill;
            CombatEventBus.OnHealApplied -= OnHealApplied;
            CombatEventBus.OnShieldGained -= OnShieldGained;
        }

        /// <summary>유물에 의한 추가 데미지 (ModifyOutgoingDamage 훅)</summary>
        public int GetBonusOutgoingDamage()
        {
            int bonus = 0;
            foreach (var relic in _relics)
            {
                if (relic.EffectType == RelicEffectType.BonusDamage)
                    bonus += relic.EffectValue;
                if (relic.EffectType == RelicEffectType.StackingPowerOnKill)
                    bonus += relic.EffectValue * _killStackCount;
            }
            return bonus;
        }

        /// <summary>유물에 의한 피해 감소 (ModifyIncomingDamage 훅)</summary>
        public int GetDamageReduction()
        {
            int reduction = 0;
            foreach (var relic in _relics)
            {
                if (relic.EffectType == RelicEffectType.DamageReduction)
                    reduction += relic.EffectValue;
            }
            return reduction;
        }

        /// <summary>유물에 의한 드로우 가중치 보너스</summary>
        public int GetDrawWeightBonus()
        {
            int bonus = 0;
            foreach (var relic in _relics)
            {
                if (relic.EffectType == RelicEffectType.BonusDrawWeight)
                    bonus += relic.EffectValue;
            }
            return bonus;
        }

        // ── 이벤트 핸들러 ──

        private void OnBattleStart()
        {
            _killStackCount = 0;
            foreach (var relic in _relics)
            {
                if (relic.Trigger != RelicTrigger.BattleStart) continue;
                ApplyEffect(relic, null);
            }
        }

        private void OnTurnStart(int turnNumber)
        {
            foreach (var relic in _relics)
            {
                if (relic.Trigger != RelicTrigger.TurnStart) continue;
                ApplyEffect(relic, null);
            }
        }

        private void OnTurnEnd()
        {
            foreach (var relic in _relics)
            {
                if (relic.Trigger != RelicTrigger.TurnEnd) continue;
                ApplyEffect(relic, null);
            }
        }

        private void OnDamageDealt(Character attacker, Character target, int amount)
        {
            foreach (var relic in _relics)
            {
                if (relic.Trigger != RelicTrigger.OnDamageDealt) continue;
                ApplyEffect(relic, target);
            }
        }

        private void OnDamageReceived(Character target, int amount)
        {
            foreach (var relic in _relics)
            {
                if (relic.Trigger != RelicTrigger.OnDamageReceived) continue;
                ApplyEffect(relic, target);

                // 반사 데미지 (CounterDamage)
                if (relic.EffectType == RelicEffectType.CounterDamage && target != null)
                {
                    // 적에게 반사 데미지 — 여기서는 간접 처리 불가하므로 이벤트로 위임
                }
            }
        }

        private void OnKill(Character killed)
        {
            foreach (var relic in _relics)
            {
                if (relic.Trigger != RelicTrigger.OnKill) continue;
                ApplyEffect(relic, killed);
            }
        }

        private void OnHealApplied(Character target, int amount)
        {
            foreach (var relic in _relics)
            {
                if (relic.Trigger != RelicTrigger.OnHealApplied) continue;
                ApplyEffect(relic, target);
            }
        }

        private void OnShieldGained(Character target, int amount)
        {
            foreach (var relic in _relics)
            {
                if (relic.Trigger != RelicTrigger.OnShieldGained) continue;
                ApplyEffect(relic, target);
            }
        }

        // ── 효과 적용 ──

        private void ApplyEffect(RelicData relic, Character target)
        {
            switch (relic.EffectType)
            {
                case RelicEffectType.HealPerTurn:
                    if (target != null) target.Health.Heal(relic.EffectValue);
                    break;
                case RelicEffectType.BonusGold:
                    // 골드는 GameRunState에서 처리 — 이벤트로 전달
                    CombatEventBus.FireGoldEarned(relic.EffectValue);
                    break;
                case RelicEffectType.BonusShield:
                    if (target != null) target.Health.AddShield(relic.EffectValue);
                    break;
                case RelicEffectType.ExtraAP:
                    // AP 보너스는 TurnManager에서 처리
                    break;
                case RelicEffectType.HealOnKill:
                    if (target != null) target.Health.Heal(relic.EffectValue);
                    break;
                case RelicEffectType.StackingPowerOnKill:
                    _killStackCount++;
                    break;
                // BonusDamage, DamageReduction, BonusDrawWeight는 Get 메서드에서 처리
                // CounterDamage는 전투 로직에서 별도 처리
            }
        }
    }
}
