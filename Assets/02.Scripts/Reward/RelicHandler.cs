using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Map;
using TeamLog.Skill;

namespace TeamLog.Reward
{
    /// <summary>
    /// 유물 처리기 — GameRunState에 소속된 전체 파티 유물을 관리
    /// CombatEventBus를 구독하여 트리거 매칭 → 키워드 기반 효과 적용
    /// </summary>
    public class RelicHandler
    {
        private readonly List<RelicData> _relics = new();
        public IReadOnlyList<RelicData> Relics => _relics;

        // 누적형 효과 상태
        private int _killStackCount;

        // 파티 참조
        private List<Character> _playerParty;

        public void SetPlayerParty(List<Character> party)
        {
            _playerParty = party;
        }

        public void AddRelic(RelicData relic)
        {
            if (relic != null && !_relics.Contains(relic))
                _relics.Add(relic);
        }

        public bool RemoveRelic(RelicData relic)
        {
            if (relic == null) return false;
            return _relics.Remove(relic);
        }

        public void SubscribeEvents()
        {
            CombatEventBus.OnBattleStart += OnBattleStart;
            CombatEventBus.OnBattleEnd += OnBattleEnd;
            CombatEventBus.OnTurnStart += OnTurnStart;
            CombatEventBus.OnTurnEnd += OnTurnEnd;
            CombatEventBus.OnDamageDealt += OnDamageDealt;
            CombatEventBus.OnDamageReceived += OnDamageReceived;
            CombatEventBus.OnKill += OnKill;
            CombatEventBus.OnHealApplied += OnHealApplied;
            CombatEventBus.OnShieldGained += OnShieldGained;
            CombatEventBus.OnGoldEarned += OnGoldEarned;
            CombatEventBus.OnSkillUsed += OnSkillUsed;
        }

        public void UnsubscribeEvents()
        {
            CombatEventBus.OnBattleStart -= OnBattleStart;
            CombatEventBus.OnBattleEnd -= OnBattleEnd;
            CombatEventBus.OnTurnStart -= OnTurnStart;
            CombatEventBus.OnTurnEnd -= OnTurnEnd;
            CombatEventBus.OnDamageDealt -= OnDamageDealt;
            CombatEventBus.OnDamageReceived -= OnDamageReceived;
            CombatEventBus.OnKill -= OnKill;
            CombatEventBus.OnHealApplied -= OnHealApplied;
            CombatEventBus.OnShieldGained -= OnShieldGained;
            CombatEventBus.OnGoldEarned -= OnGoldEarned;
            CombatEventBus.OnSkillUsed -= OnSkillUsed;
        }

        // ── 키워드 기반 쿼리 메서드 ──

        /// <summary>유물 키워드에서 지정 타입의 합산 값 (Passive 트리거만)</summary>
        private int QueryKeywordSum(KeywordType type)
        {
            int total = 0;
            foreach (var relic in _relics)
            {
                if (relic.Keywords == null) continue;
                foreach (var kw in relic.Keywords)
                {
                    if (kw.Type == type && kw.Trigger == KeywordTrigger.Passive)
                        total += (int)kw.Value;
                }
            }
            return total;
        }

        /// <summary>유물에 의한 추가 데미지</summary>
        public int GetBonusOutgoingDamage()
        {
            int bonus = QueryKeywordSum(KeywordType.BonusOutgoingDamage);
            // 누적형: StackingPowerOnKill
            bonus += QueryKeywordSum(KeywordType.StackingPowerOnKill) * _killStackCount;
            return bonus;
        }

        /// <summary>유물에 의한 피해 감소</summary>
        public int GetDamageReduction() => QueryKeywordSum(KeywordType.DamageReduction);

        /// <summary>유물에 의한 드로우 가중치 보너스</summary>
        public int GetDrawWeightBonus() => QueryKeywordSum(KeywordType.DrawWeightAdd);

        /// <summary>유물에 의한 매 턴 추가 AP</summary>
        public int GetExtraAP() => QueryKeywordSum(KeywordType.ExtraAP);

        /// <summary>유물에 의한 반사 피해량</summary>
        public int GetCounterDamage() => QueryKeywordSum(KeywordType.CounterDamage);

        // ── 이벤트 핸들러 ──

        private void OnBattleStart()
        {
            _killStackCount = 0;
            ApplyTrigger(KeywordTrigger.OnBattleStart, null);
        }

        private void OnTurnStart(int turnNumber)
        {
            ApplyTrigger(KeywordTrigger.OnTurnStart, null);
        }

        private void OnTurnEnd()
        {
            ApplyTrigger(KeywordTrigger.OnTurnEnd, null);
        }

        private void OnDamageDealt(Character attacker, Character target, int amount)
        {
            ApplyTrigger(KeywordTrigger.OnDamageDealt, target);
        }

        private void OnDamageReceived(Character target, int amount)
        {
            ApplyTrigger(KeywordTrigger.OnDamageReceived, target);
        }

        private void OnKill(Character killed)
        {
            ApplyTrigger(KeywordTrigger.OnKill, killed);
        }

        private void OnHealApplied(Character target, int amount)
        {
            ApplyTrigger(KeywordTrigger.OnHealApplied, target);
        }

        private void OnShieldGained(Character target, int amount)
        {
            ApplyTrigger(KeywordTrigger.OnShieldGained, target);
        }

        private void OnSkillUsed(SkillData skill, Character caster)
        {
            ApplyTrigger(KeywordTrigger.OnSkillUsed, caster);
        }

        private void OnGoldEarned(int amount)
        {
            var runState = GameRunState.Instance;
            if (runState != null)
                runState.AddGold(amount);
        }

        private void OnBattleEnd(bool victory)
        {
            _killStackCount = 0;
        }

        // ── 키워드 기반 효과 적용 ──

        private void ApplyTrigger(KeywordTrigger trigger, Character context)
        {
            foreach (var relic in _relics)
            {
                if (relic.Keywords == null) continue;

                foreach (var kw in relic.Keywords)
                {
                    if (kw.Trigger != trigger) continue;
                    ApplyKeyword(relic, kw, context);
                }
            }
        }

        private void ApplyKeyword(RelicData relic, KeywordEntry kw, Character context)
        {
            CombatEventBus.FireRelicTriggered(relic);

            switch (kw.Type)
            {
                case KeywordType.HPPerTurn:
                    if (_playerParty != null)
                    {
                        foreach (var member in _playerParty)
                        {
                            if (!member.IsAlive) continue;
                            if (kw.Value > 0) member.Health.Heal((int)kw.Value);
                            else if (kw.Value < 0) member.Health.TakeDamage((int)(-kw.Value));
                        }
                    }
                    break;

                case KeywordType.ShieldPerTurn:
                    if (kw.Trigger == KeywordTrigger.OnBattleStart || kw.Trigger == KeywordTrigger.OnTurnStart)
                    {
                        // 전체 파티
                        if (_playerParty != null)
                            foreach (var m in _playerParty) if (m.IsAlive) m.Health.AddShield((int)kw.Value);
                    }
                    else if (context != null)
                    {
                        context.Health.AddShield((int)kw.Value);
                    }
                    break;

                case KeywordType.OnKillHeal:
                    if (context != null) context.Health.Heal((int)kw.Value);
                    break;

                case KeywordType.StackingPowerOnKill:
                    _killStackCount++;
                    break;

                case KeywordType.BonusGold:
                    CombatEventBus.FireGoldEarned((int)kw.Value);
                    break;

                case KeywordType.MaxHPUp:
                    if (_playerParty != null)
                        foreach (var m in _playerParty) if (m.IsAlive) m.Health.SetMaxHP(m.Health.MaxHP + (int)kw.Value);
                    break;

                case KeywordType.ATKUp:
                    if (_playerParty != null)
                        foreach (var m in _playerParty) if (m.IsAlive) m.Stats.AddPermanentBase(StatType.ATK, (int)kw.Value);
                    break;

                case KeywordType.DEFUp:
                    if (_playerParty != null)
                        foreach (var m in _playerParty) if (m.IsAlive) m.Stats.AddPermanentBase(StatType.DEF, (int)kw.Value);
                    break;
            }
        }
    }
}
