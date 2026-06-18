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

        // 트리거 체인 무한 루프 방지 — 최대 깊이 제한
        private int _triggerDepth;
        private const int MAX_TRIGGER_DEPTH = 5;

        // 일시적 "다음 공격 강화" 버프 — 트리거 기반 BonusOutgoingDamage가 여기 누적
        // 다음 공격 스킬 1회에만 적용되고 소비됨 (B2 AegisStrike, C3 MercyBlade, F2 CriticalFocus 등)
        private int _nextAttackBonusDamage;

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
            CombatEventBus.OnRerollUsed += OnRerollUsed;
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
            CombatEventBus.OnRerollUsed -= OnRerollUsed;
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
            // PowerAdd (Passive) — 공격력 가산
            bonus += QueryKeywordSum(KeywordType.PowerAdd);
            // 누적형: StackingPowerOnKill — OnKill 트리거이므로 QueryKeywordSum(Passive-only)에서 누락.
            // 트리거 무관하게 값 조회 후 현재 스택 곱하기.
            bonus += GetStackingPowerValue() * _killStackCount;
            return bonus;
        }

        /// <summary>
        /// 유물에 의한 처치 시 HP 회복량 합산 (OnKill 트리거).
        /// QueryKeywordSum은 Passive만 합산하므로 OnKill 트리거 키워드는 별도 조회 필요.
        /// 영향 유물: VampireFang, MercyBlade 등.
        /// </summary>
        public int GetOnKillHealValue()
        {
            int total = 0;
            foreach (var relic in _relics)
            {
                if (relic.Keywords == null) continue;
                foreach (var kw in relic.Keywords)
                {
                    if (kw.Type == KeywordType.OnKillHeal)
                        total += (int)kw.Value;
                }
            }
            return total;
        }

        /// <summary>
        /// 유물에 의한 처치당 누적 위력 가산치 합산 (트리거 무관).
        /// 영향 유물: SlayerSigil (D 카테고리 학살춤).
        /// </summary>
        public int GetStackingPowerValue()
        {
            int total = 0;
            foreach (var relic in _relics)
            {
                if (relic.Keywords == null) continue;
                foreach (var kw in relic.Keywords)
                {
                    if (kw.Type == KeywordType.StackingPowerOnKill)
                        total += (int)kw.Value;
                }
            }
            return total;
        }

        /// <summary>유물에 의한 피해 감소</summary>
        public int GetDamageReduction() => QueryKeywordSum(KeywordType.DamageReduction);

        /// <summary>유물에 의한 드로우 가중치 보너스</summary>
        public int GetDrawWeightBonus() => QueryKeywordSum(KeywordType.DrawWeightAdd);

        /// <summary>유물에 의한 매 턴 추가 AP</summary>
        public int GetExtraAP() => QueryKeywordSum(KeywordType.ExtraAP);

        /// <summary>유물에 의한 반사 피해량</summary>
        public int GetCounterDamage() => QueryKeywordSum(KeywordType.CounterDamage);

        /// <summary>일시적 "다음 공격 강화" 버프 조회 (소비 없이)</summary>
        public int PeekNextAttackBonus() => _nextAttackBonusDamage;

        /// <summary>일시적 버프 소비 — 다음 공격 1회에만 적용</summary>
        public int ConsumeNextAttackBonus()
        {
            int bonus = _nextAttackBonusDamage;
            _nextAttackBonusDamage = 0;
            return bonus;
        }

        /// <summary>유물에 의한 리롤 시 쉴드 획득 (G2 CardShark 등)</summary>
        public int GetRerollShieldBonus() => QueryTriggerKeywordSum(KeywordType.ShieldPerTurn, KeywordTrigger.OnRerollUsed);

        /// <summary>지정 트리거의 키워드 합산 (Passive 제외)</summary>
        private int QueryTriggerKeywordSum(KeywordType type, KeywordTrigger trigger)
        {
            int total = 0;
            foreach (var relic in _relics)
            {
                if (relic.Keywords == null) continue;
                foreach (var kw in relic.Keywords)
                {
                    if (kw.Type == type && kw.Trigger == trigger)
                        total += (int)kw.Value;
                }
            }
            return total;
        }

        /// <summary>적 HP 조건부 PowerMul 합산 (F3 ExecutionerBlade 등)</summary>
        public float GetEnemyLowHPPowerMul(int targetCurrentHP, int targetMaxHP)
        {
            float result = 1f;
            foreach (var relic in _relics)
            {
                if (relic.Keywords == null) continue;
                foreach (var kw in relic.Keywords)
                {
                    if (kw.Type == KeywordType.PowerMul && kw.Trigger == KeywordTrigger.OnEnemyLowHP)
                    {
                        if (targetMaxHP > 0 && (float)targetCurrentHP / targetMaxHP <= kw.ConditionParam)
                            result *= kw.Value;
                    }
                }
            }
            return result;
        }

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

        private void OnRerollUsed()
        {
            ApplyTrigger(KeywordTrigger.OnRerollUsed, null);
            // 리롤 시 쉴드 획득 (G2 CardShark 등)
            int shield = GetRerollShieldBonus();
            if (shield > 0 && _playerParty != null)
                foreach (var m in _playerParty) if (m.IsAlive) m.Health.AddShield(shield);
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
            // 무한 루프 방지 — 트리거 체인 깊이 제한
            if (_triggerDepth >= MAX_TRIGGER_DEPTH) return;
            _triggerDepth++;
            try
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
            finally
            {
                _triggerDepth--;
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
                        // 전체 파티 — 트리거 체인 미발생 (이벤트 폭주 방지)
                        if (_playerParty != null)
                            foreach (var m in _playerParty) if (m.IsAlive) m.Health.AddShield((int)kw.Value);
                    }
                    else if (context != null)
                    {
                        int shieldAmount = (int)kw.Value;
                        if (shieldAmount > 0)
                        {
                            context.Health.AddShield(shieldAmount);
                            // 트리거 체인: 쉴드 획득 이벤트 재발행 (B2/AegisStrike 등 활성화)
                            CombatEventBus.FireShieldGained(context, shieldAmount);
                        }
                    }
                    break;

                case KeywordType.OnKillHeal:
                    if (context != null)
                    {
                        int healAmount = (int)kw.Value;
                        if (healAmount > 0)
                        {
                            context.Health.Heal(healAmount);
                            // 트리거 체인: 힐 적용 이벤트 재발행 (C2/SanguineBond 등 활성화)
                            CombatEventBus.FireHealApplied(context, healAmount);
                        }
                    }
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

                case KeywordType.BonusOutgoingDamage:
                    // 비 Passive 트리거 (OnShieldGained, OnRerollUsed 등) → 일시적 버프 누적
                    if (kw.Trigger != KeywordTrigger.Passive)
                        _nextAttackBonusDamage += (int)kw.Value;
                    break;
            }
        }
    }
}
