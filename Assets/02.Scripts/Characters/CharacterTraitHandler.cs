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

        // Phase CC-2E: Cael(Alchemist) "강화 물약" 특성 — 전투당 1회 ApplyAll 사용 가능
        private bool _discoverApplyAllAvailable;

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
            ApplyPassiveEffects(); // Phase CC-2A: Passive 키워드 즉시 적용 (ShadowsMaxUp 등)
        }

        /// <summary>
        /// Phase CC-2A: Passive 키워드 즉시 적용. EquipTrait 시 호출.
        /// ShadowsMaxUp 등 "장착 즉시 영구 적용" 키워드 처리.
        /// </summary>
        private void ApplyPassiveEffects()
        {
            if (_trait == null || _trait.Keywords == null || _owner == null) return;

            foreach (var kw in _trait.Keywords)
            {
                if (kw.Trigger != KeywordTrigger.Passive) continue;

                if (kw.Type == KeywordType.ShadowsMaxUp && _owner.Resource != null)
                {
                    _owner.Resource.MaxStacksBonus = (int)kw.Value;
                }

                // Phase CC-2F: Mortis "생명력 흡수" 특성 — Soul Link 회복 비율 설정
                if (kw.Type == KeywordType.SoulLinkMul && _owner.Corpse != null)
                {
                    _owner.Corpse.SoulLinkMul = kw.Value;
                }
            }
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

        /// <summary>장착 특성의 추가 고정 데미지 (BonusOutgoingDamage + PowerAdd + StackingPowerOnKill 누적).</summary>
        /// <param name="target">Phase CC-2A: 대상이 도트 디버프 상태일 경우 PowerAddVsDebuff 추가 가산. null이면 스킵.</param>
        public int GetBonusOutgoingDamage(Character target = null)
        {
            int bonus = QueryKeywordSum(KeywordType.BonusOutgoingDamage);
            bonus += QueryKeywordSum(KeywordType.PowerAdd);
            bonus += GetStackingPowerValue() * _killStackCount;

            // Phase CC-2A: 도트 디버프 적 대상 추가 위력 (Umbra "약점 포착" 특성)
            if (target != null && HasDotDebuff(target))
                bonus += QueryKeywordSum(KeywordType.PowerAddVsDebuff);

            // Phase CC-2B: Combo 최대치 달성 시 위력 가산 (Aster "명사수" 특성)
            if (_owner != null && _owner.Resource != null
                && _owner.Resource.Resource == ResourceType.Combo
                && _owner.Resource.CurrentStacks >= _owner.Resource.EffectiveMaxStacks)
                bonus += QueryKeywordSum(KeywordType.ComboMaxPowerBonus);

            // Phase CC-2B: Mark 상태 적 대상 추가 위력 (Aster "약점 포착" 특성)
            if (target != null && HasMark(target))
                bonus += QueryKeywordSum(KeywordType.PowerAddVsMark);

            // Phase CC-2F: AttackDown(저주) 상태 적 대상 추가 위력 (Mortis "저주의 대가" 특성)
            if (target != null && HasCurse(target))
                bonus += QueryKeywordSum(KeywordType.CurseExtraDamage);

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

        // ── Phase CC-2E: Cael(Alchemist) 발견 특성 쿼리 ──

        /// <summary>"강화 물약" 특성(DiscoverApplyAll) 보유 여부.</summary>
        public bool HasDiscoverApplyAllTrait()
            => QueryKeywordSum(KeywordType.DiscoverApplyAll) > 0;

        /// <summary>발견 ApplyAll 사용 가능 여부 — 전투당 1회.</summary>
        public bool CanUseDiscoverApplyAll() => _discoverApplyAllAvailable;

        /// <summary>발견 ApplyAll 사용 처리 — 플래그 소진.</summary>
        public void ConsumeDiscoverApplyAll() => _discoverApplyAllAvailable = false;

        /// <summary>
        /// Phase CC-2A: 대상이 도트/행동봉쇄 디버프 상태인지 확인.
        /// StrongVsDebuffBehavior.HasDotDebuff와 동일 로직 — 특성 시스템에서 재사용.
        /// </summary>
        private static bool HasDotDebuff(Character target)
        {
            if (target?.StatusEffects == null) return false;
            foreach (var effect in target.StatusEffects.GetAllEffects())
            {
                if (effect.Type == StatusEffectType.Poison ||
                    effect.Type == StatusEffectType.Burn ||
                    effect.Type == StatusEffectType.Bleed ||
                    effect.Type == StatusEffectType.Freeze ||
                    effect.Type == StatusEffectType.Stun)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Phase CC-2B: 대상이 Hunter's Mark 상태인지 확인 (Aster 자원 메카닉).
        /// </summary>
        private static bool HasMark(Character target)
        {
            return target?.StatusEffects != null && target.StatusEffects.HasEffect(StatusEffectType.Mark);
        }

        /// <summary>
        /// Phase CC-2F: 대상이 AttackDown(저주) 상태인지 확인 (Mortis "저주의 대가" 특성).
        /// </summary>
        private static bool HasCurse(Character target)
        {
            return target?.StatusEffects != null && target.StatusEffects.HasEffect(StatusEffectType.AttackDown);
        }

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
            // Phase CC-2E: "강화 물약" 특성 보유 시 매 전투마다 ApplyAll 1회 가용
            _discoverApplyAllAvailable = QueryKeywordSum(KeywordType.DiscoverApplyAll) > 0;
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

            // Phase CC-2F: Mortis "죽음의 수확" 특성 — 적 처치 시 시체 영구 강화
            if (_owner.Corpse != null && _owner.Corpse.IsActive)
            {
                int killEmpower = QueryKeywordSum(KeywordType.CorpseKillEmpower);
                if (killEmpower > 0)
                    _owner.Corpse.ApplyKillEmpower(killEmpower);
            }
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
