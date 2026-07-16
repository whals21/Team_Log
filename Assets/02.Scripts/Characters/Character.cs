using System.Collections.Generic;
using UnityEngine;

namespace TeamLog.Characters
{
    /// <summary>
    /// 캐릭터 코디네이터 - 컴포넌트들을 조립하여 관리
    /// </summary>
    public class Character
    {
        private readonly CharacterData _data;

        // 컴포넌트들
        public HealthComponent Health { get; }
        public StatComponent Stats { get; }
        public StatusEffectComponent StatusEffects { get; }
        public SkillInventoryComponent SkillInventory { get; }
        public EnemyTraitHandler TraitHandler { get; }

        // 플레이어 장착 특성 처리기 (Phase 8C) — 적 캐릭터는 사용하지 않음 (null로 남음)
        public CharacterTraitHandler PlayerTraitHandler { get; private set; }

        // Phase CC: 캐릭터 고유 자원 (Ashe=Ember, Duran=Vengeance 등). null이면 자원 없는 캐릭터.
        public CharacterResourceComponent Resource { get; private set; }

        // Phase CC-2F: Mortis(Necromancer) 시체 컴포넌트. null이면 시체 없는 캐릭터.
        public CorpseComponent Corpse { get; private set; }

        // Phase CC-2A: 치명타 시스템 (Umbra Shadows용 기반). 모든 캐릭터 기본 0%/1.5배.
        // CharacterResourceComponent가 런타임에 동적 갱신 가능 (Umbra의 ShadowsResourceComponent).
        public float CritChance { get; set; }
        public float CritDamageMul { get; set; }

        /// <summary>Phase CC: 자원 컴포넌트 장착 (캐릭터 생성 시 또는 런 시작 시).</summary>
        public void SetResource(CharacterResourceComponent resource)
        {
            Resource = resource;
        }

        // 프로퍼티
        public CharacterData Data => _data;
        public string Name => _data.CharacterName;
        public CharacterClass Class => _data.Class;
        public bool IsDead => Health.IsDead;
        public bool IsAlive => Health.IsAlive;

        public Character(CharacterData data)
        {
            _data = data;

            // 컴포넌트 생성 및 초기화
            Health = new HealthComponent();
            Health.SetOwner(this);  // Phase CC P1: 쉴드 흡수 이벤트에서 owner 전달용
            Stats = new StatComponent();
            StatusEffects = new StatusEffectComponent();
            SkillInventory = new SkillInventoryComponent();
            TraitHandler = new EnemyTraitHandler(data.Trait, this);
            PlayerTraitHandler = new CharacterTraitHandler(this);

            // 특성 사망 방지 훅 연결
            if (TraitHandler.HasTrait)
                Health.OnPreDeath += () => TraitHandler.PreventDeath();

            // Phase CC P1: 쉴드 흡수 훅 — 부여자 기반 자원/상태이상 처리
            // (caster가 부여한 쉴드가 attacker에 의해 흡수되었을 때)
            // - caster의 자원이 Vengeance면 흡수량만큼 축적 (Duran 원격 Vengeance)
            // - ShieldFlag.GivesChargeOnAbsorb 시 attacker에게 Charge 부여 (Taranis Grounding Field)
            Health.OnShieldAbsorbed += OnShieldAbsorbedInternal;

            InitializeComponents();

            // Phase CC: CharacterData.ResourceType에 따라 고유 자원 컴포넌트 인스턴스화
            Resource = CreateResource(data.ResourceType);
            // Phase CC-2C: Owner 설정 (MercyResourceComponent 등이 OnTurnStart 없이도 Owner 접근)
            Resource?.InitializeOwner(this);

            // Phase CC-2F: Necromancer 시체 컴포넌트 초기화 (corpseBaseSkills가 있으면)
            InitializeCorpseFromData(data);
        }

        /// <summary>Phase CC-2F: CharacterData의 corpseBaseSkills가 있으면 시체 컴포넌트 생성.</summary>
        private void InitializeCorpseFromData(CharacterData data)
        {
            var corpseSkills = data.CorpseBaseSkills;
            if (corpseSkills == null || corpseSkills.Count < CorpseComponent.CORPSE_SLOT_COUNT) return;

            var skills = new SkillData[CorpseComponent.CORPSE_SLOT_COUNT];
            for (int i = 0; i < CorpseComponent.CORPSE_SLOT_COUNT && i < corpseSkills.Count; i++)
                skills[i] = corpseSkills[i];
            Corpse = new CorpseComponent(this, skills);

            // Necromancer 사망 시 시체 자동 비활성화 (기획: "Necromancer 사망 = 시체도 사망")
            Health.OnDeath += () =>
            {
                if (Corpse != null && Corpse.IsActive)
                    Corpse.Deactivate();
            };
        }

        /// <summary>Phase CC: 자원 종류에 따른 컴포넌트 생성. 각 캐릭터 Phase에서 케이스 추가.</summary>
        private static CharacterResourceComponent CreateResource(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Ember: return new EmberResourceComponent();
                case ResourceType.Vengeance: return new VengeanceResourceComponent();
                case ResourceType.Frost: return new FrostResourceComponent();
                case ResourceType.Prophecy: return new ProphecyResourceComponent();
                case ResourceType.Shadows: return new ShadowsResourceComponent(); // Phase CC-2A Umbra
                case ResourceType.Combo: return new ComboResourceComponent(); // Phase CC-2B Aster
                case ResourceType.Mercy: return new MercyResourceComponent(); // Phase CC-2C Elara
                case ResourceType.Melody: return new MelodyResourceComponent(); // Phase CC-2D Calliope
                // Charge는 각 캐릭터 Phase에서 추가 (현재 None 처리)
                default: return null;
            }
        }

        /// <summary>
        /// 플레이어 장착 특성 설정 (Phase 8C). null 전달 시 해제.
        /// </summary>
        public void EquipTrait(CharacterTraitData trait)
        {
            PlayerTraitHandler.EquipTrait(trait);
        }

        private void InitializeComponents()
        {
            Health.Initialize(_data.BaseHP);
            Stats.Initialize(_data.BaseATK, _data.BaseDEF);
            SkillInventory.Initialize(_data.Skills);
            // Phase CC-2A: 치명타 기본값 초기화
            CritChance = _data.BaseCritChance;
            CritDamageMul = _data.BaseCritDamageMul;
        }

        /// <summary>
        /// 턴 종료 시 처리
        /// </summary>
        public void OnTurnEnd()
        {
            ProcessStatusEffects();
        }

        private void ProcessStatusEffects()
        {
            foreach (var effect in StatusEffects.GetAllEffects())
            {
                ApplyEffectDamage(effect);
            }
            StatusEffects.TickTurnEnd();
        }

        private void ApplyEffectDamage(ActiveEffect effect)
        {
            switch (effect.Type)
            {
                case StatusEffectType.Poison:
                    Health.TakeDamage(effect.Value * effect.Stacks);
                    break;
                case StatusEffectType.Burn:
                    Health.TakeDamage(effect.Value);
                    break;
                case StatusEffectType.Bleed:
                    Health.TakeDamage(effect.Value * effect.Stacks);
                    break;
                case StatusEffectType.Regeneration:
                    Health.Heal(effect.Value * effect.Stacks);
                    break;
            }
        }

        /// <summary>
        /// 스탯에 상태이상 효과 적용
        /// </summary>
        public void ApplyStatModifiers()
        {
            Stats.ClearModifiers();

            foreach (var effect in StatusEffects.GetAllEffects())
            {
                switch (effect.Type)
                {
                    case StatusEffectType.AttackUp:
                        Stats.AddModifier(StatType.ATK, effect.Value);
                        break;
                    case StatusEffectType.AttackDown:
                        Stats.AddModifier(StatType.ATK, -effect.Value);
                        break;
                    case StatusEffectType.DefenseUp:
                        Stats.AddModifier(StatType.DEF, effect.Value);
                        break;
                    case StatusEffectType.DefenseDown:
                        Stats.AddModifier(StatType.DEF, -effect.Value);
                        break;
                }
            }
        }

        /// <summary>
        /// 층별 적 스케일링 적용
        /// </summary>
        public void ApplyFloorScaling(float multiplier)
        {
            int scaledHP = System.Math.Max(1, (int)(Health.MaxHP * multiplier));
            Health.SetMaxHP(scaledHP, healToFull: true);

            int baseATK = Stats.GetBaseStat(StatType.ATK);
            int baseDEF = Stats.GetBaseStat(StatType.DEF);
            Stats.Initialize((int)(baseATK * multiplier), (int)(baseDEF * multiplier));
        }

        // ═══════════════════════════════════════════
        // Phase CC P1: 쉴드 흡수 핸들러
        // ═══════════════════════════════════════════

        /// <summary>
        /// 이 캐릭터의 쉴드가 흡수되었을 때 호출. 부여자 기반 자원/상태이상 처리.
        /// - caster의 자원이 Vengeance면 흡수량 축적 (Duran 원격 Vengeance)
        /// - ShieldFlag.GivesChargeOnAbsorb 시 attacker에게 Charge 부여 (Taranis Grounding Field)
        /// </summary>
        private void OnShieldAbsorbedInternal(Character caster, Character owner, int absorbed,
            Character attacker, ShieldFlag flags)
        {
            // Duran Vengeance 원격 축적: caster가 Vengeance 자원 보유 시
            if (caster != null && caster.Resource != null
                && caster.Resource.Resource == ResourceType.Vengeance)
            {
                caster.Resource.AddStacks(absorbed);
            }

            // Taranis Grounding Field: 썰드 흡수 시 공격자에게 Charge 부여
            if ((flags & ShieldFlag.GivesChargeOnAbsorb) != 0 && attacker != null && attacker.IsAlive)
            {
                attacker.StatusEffects.ApplyEffect(StatusEffectType.Charge, 3, 1);
            }
        }
    }
}
