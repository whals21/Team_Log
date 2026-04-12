using System.Collections.Generic;

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
            Stats = new StatComponent();
            StatusEffects = new StatusEffectComponent();
            SkillInventory = new SkillInventoryComponent();

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Health.Initialize(_data.BaseHP);
            Stats.Initialize(_data.BaseATK, _data.BaseDEF);
            SkillInventory.Initialize(_data.Skills);
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
    }
}
