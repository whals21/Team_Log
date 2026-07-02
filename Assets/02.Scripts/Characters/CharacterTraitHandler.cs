using System.Collections.Generic;
using TeamLog.Combat;
using TeamLog.Skill;

using StatType = TeamLog.Characters.StatType;

namespace TeamLog.Characters
{
    /// <summary>
    /// 플레이어 캐릭터 장착 특성 처리기 — Character 1명당 1개 소유.
    /// CombatEventBus를 구독하여 트리거 매칭 → Owner 자신에게만 효과 적용.
    /// RelicHandler(파티 전체 적용)와 달리 개인 한정 적용.
    /// Phase 8C: Character.PlayerTraitHandler로 연결.
    /// </summary>
    public class CharacterTraitHandler
    {
        private readonly Character _owner;
        private CharacterTraitData _trait;

        // 누적형 효과 상태
        private int _killStackCount;

        // 트리거 체인 무한 루프 방지
        private int _triggerDepth;
        private const int MAX_TRIGGER_DEPTH = 5;

        // 일시적 "다음 공격 강화" 버프 (BonusOutgoingDamage on non-Passive 트리거)
        private int _nextAttackBonusDamage;

        public CharacterTraitData Trait => _trait;
        public bool HasTrait => _trait != null;
        public Character Owner => _owner;

        public CharacterTraitHandler(Character owner)
        {
            _owner = owner;
        }

        /// <summary>특성 장착 — trait=null이면 해제. 기존 누적 상태는 유지.</summary>
        public void EquipTrait(CharacterTraitData trait)
        {
            _trait = trait;
        }

        public void SubscribeEvents()
        {
            if (_trait == null) return;
            CombatEventBus.OnBattleStart += OnBattleStart;
            CombatEventBus.OnBattleEnd += OnBattleEnd;
            CombatEventBus.OnTurnStart += OnTurnStart;
            CombatEventBus.OnTurnEnd += OnTurnEnd;
            CombatEventBus.OnKill += OnKill;
            CombatEventBus.OnHealApplied += OnHealApplied;
            CombatEventBus.OnShieldGained += OnShieldGained;
            CombatEventBus.OnDamageDealt += OnDamageDealt;
            CombatEventBus.OnDamageReceived += OnDamageReceived;
            CombatEventBus.OnSkillUsed += OnSkillUsed;
            CombatEventBus.OnRerollUsed += OnRerollUsed;
        }

        public void UnsubscribeEvents()
        {
            CombatEventBus.OnBattleStart -= OnBattleStart;
            CombatEventBus.OnBattleEnd -= OnBattleEnd;
            CombatEventBus.OnTurnStart -= OnTurnStart;
            CombatEventBus.OnTurnEnd -= OnTurnEnd;
            CombatEventBus.OnKill -= OnKill;
            CombatEventBus.OnHealApplied -= OnHealApplied;
            CombatEventBus.OnShieldGained -= OnShieldGained;
            CombatEventBus.OnDamageDealt -= OnDamageDealt;
            CombatEventBus.OnDamageReceived -= OnDamageReceived;
            CombatEventBus.OnSkillUsed -= OnSkillUsed;
            CombatEventBus.OnRerollUsed -= OnRerollUsed;
        }

        // ── 키워드 기반 쿼리 (외부에서 호출) ──

        /// <summary>장착 특성의 Passive 키워드에서 지정 타입 합산</summary>
        public int QueryKeywordSum(KeywordType type)
        {
            if (_trait == null || _trait.Keywords == null) return 0;
            int total = 0;
            foreach (var kw in _trait.Keywords)
                if (kw.Type == type && kw.Trigger == KeywordTrigger.Passive)
                    total += (int)kw.Value;
            return total;
        }

        /// <summary>장착 특성의 Passive 키워드에서 지정 타입 곱 배율</summary>
        public float QueryKeywordMul(KeywordType type)
        {
            if (_trait == null || _trait.Keywords == null) return 1f;
            float result = 1f;
            foreach (var kw in _trait.Keywords)
                if (kw.Type == type && kw.Trigger == KeywordTrigger.Passive)
                    result *= kw.Value;
            return result;
        }

        /// <summary>장착 특성의 추가 고정 데미지 (BonusOutgoingDamage + PowerAdd + StackingPowerOnKill 누적)</summary>
        public int GetBonusOutgoingDamage()
        {
            int bonus = QueryKeywordSum(KeywordType.BonusOutgoingDamage);
            bonus += QueryKeywordSum(KeywordType.PowerAdd);
            bonus += GetStackingPowerValue() * _killStackCount;
            return bonus;
        }

        public int GetDamageReduction() => QueryKeywordSum(KeywordType.DamageReduction);

        public int GetExtraAP() => QueryKeywordSum(KeywordType.ExtraAP);

        /// <summary>OnKill 트리거의 OnKillHeal 합산</summary>
        public int GetOnKillHealValue()
        {
            if (_trait == null || _trait.Keywords == null) return 0;
            int total = 0;
            foreach (var kw in _trait.Keywords)
                if (kw.Type == KeywordType.OnKillHeal)
                    total += (int)kw.Value;
            return total;
        }

        /// <summary>적 처치당 누적 위력 가산치 (트리거 무관)</summary>
        public int GetStackingPowerValue()
        {
            if (_trait == null || _trait.Keywords == null) return 0;
            int total = 0;
            foreach (var kw in _trait.Keywords)
                if (kw.Type == KeywordType.StackingPowerOnKill)
                    total += (int)kw.Value;
            return total;
        }

        public int PeekNextAttackBonus() => _nextAttackBonusDamage;

        public int ConsumeNextAttackBonus()
        {
            int bonus = _nextAttackBonusDamage;
            _nextAttackBonusDamage = 0;
            return bonus;
        }

        /// <summary>적 HP 조건부 PowerMul (OnEnemyLowHP 트리거)</summary>
        public float GetEnemyLowHPPowerMul(int targetCurrentHP, int targetMaxHP)
        {
            if (_trait == null || _trait.Keywords == null) return 1f;
            float result = 1f;
            foreach (var kw in _trait.Keywords)
            {
                if (kw.Type == KeywordType.PowerMul && kw.Trigger == KeywordTrigger.OnEnemyLowHP)
                {
                    if (targetMaxHP > 0 && (float)targetCurrentHP / targetMaxHP <= kw.ConditionParam)
                        result *= kw.Value;
                }
            }
            return result;
        }

        // ── 이벤트 핸들러 ──

        private void OnBattleStart()
        {
            _killStackCount = 0;
            ApplyTrigger(KeywordTrigger.OnBattleStart);
        }

        private void OnBattleEnd(bool victory)
        {
            _killStackCount = 0;
        }

        private void OnTurnStart(int turnNumber) => ApplyTrigger(KeywordTrigger.OnTurnStart);
        private void OnTurnEnd() => ApplyTrigger(KeywordTrigger.OnTurnEnd);

        private void OnDamageDealt(Character attacker, Character target, int amount)
        {
            // Owner가 입힌 데미지일 때만 처리
            if (attacker != _owner) return;
            ApplyTrigger(KeywordTrigger.OnDamageDealt);
        }

        private void OnDamageReceived(Character target, int amount)
        {
            if (target != _owner) return;
            ApplyTrigger(KeywordTrigger.OnDamageReceived);
        }

        private void OnKill(Character killed)
        {
            // Owner가 소속된 파티가 적을 처치했을 때 — owner가 살아있을 때만 처리
            if (_owner == null || !_owner.IsAlive) return;
            ApplyTrigger(KeywordTrigger.OnKill);
        }

        private void OnHealApplied(Character target, int amount)
        {
            if (target != _owner) return;
            ApplyTrigger(KeywordTrigger.OnHealApplied);
        }

        private void OnShieldGained(Character target, int amount)
        {
            if (target != _owner) return;
            ApplyTrigger(KeywordTrigger.OnShieldGained);
        }

        private void OnSkillUsed(TeamLog.Characters.SkillData skill, Character caster)
        {
            if (caster != _owner) return;
            ApplyTrigger(KeywordTrigger.OnSkillUsed);
        }

        private void OnRerollUsed() => ApplyTrigger(KeywordTrigger.OnRerollUsed);

        // ── 키워드 효과 적용 (Owner 자신에게만) ──

        private void ApplyTrigger(KeywordTrigger trigger)
        {
            if (_trait == null || _trait.Keywords == null) return;
            if (_triggerDepth >= MAX_TRIGGER_DEPTH) return;
            _triggerDepth++;
            try
            {
                foreach (var kw in _trait.Keywords)
                {
                    if (kw.Trigger != trigger) continue;
                    ApplyKeyword(kw);
                }
            }
            finally
            {
                _triggerDepth--;
            }
        }

        private void ApplyKeyword(KeywordEntry kw)
        {
            if (_owner == null || !_owner.IsAlive) return;

            switch (kw.Type)
            {
                case KeywordType.HPPerTurn:
                    if (kw.Value > 0) _owner.Health.Heal((int)kw.Value);
                    else if (kw.Value < 0) _owner.Health.TakeDamage((int)(-kw.Value));
                    break;

                case KeywordType.ShieldPerTurn:
                    int shieldAmount = (int)kw.Value;
                    if (shieldAmount > 0)
                    {
                        _owner.Health.AddShield(_owner, shieldAmount);
                        // 트리거 체인: 쉴드 획득 이벤트 재발행 (다른 특성/유물 활성화)
                        CombatEventBus.FireShieldGained(_owner, shieldAmount);
                    }
                    break;

                case KeywordType.OnKillHeal:
                    int healAmount = (int)kw.Value;
                    if (healAmount > 0)
                    {
                        _owner.Health.Heal(healAmount);
                        CombatEventBus.FireHealApplied(_owner, healAmount);
                    }
                    break;

                case KeywordType.StackingPowerOnKill:
                    _killStackCount++;
                    break;

                case KeywordType.BonusGold:
                    CombatEventBus.FireGoldEarned((int)kw.Value);
                    break;

                case KeywordType.MaxHPUp:
                    _owner.Health.SetMaxHP(_owner.Health.MaxHP + (int)kw.Value);
                    break;

                case KeywordType.ATKUp:
                    _owner.Stats.AddPermanentBase(StatType.ATK, (int)kw.Value);
                    break;

                case KeywordType.DEFUp:
                    _owner.Stats.AddPermanentBase(StatType.DEF, (int)kw.Value);
                    break;

                case KeywordType.BonusOutgoingDamage:
                    // 비 Passive 트리거 → 일시적 버프 누적
                    if (kw.Trigger != KeywordTrigger.Passive)
                        _nextAttackBonusDamage += (int)kw.Value;
                    break;
            }
        }
    }
}
