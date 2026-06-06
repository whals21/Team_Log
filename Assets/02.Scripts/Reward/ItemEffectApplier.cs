using TeamLog.Characters;
using TeamLog.Map;

namespace TeamLog.Reward
{
    /// <summary>
    /// 아이템 효과를 캐릭터에 적용하는 순수 C# static 클래스
    /// </summary>
    public static class ItemEffectApplier
    {
        public static void Apply(Character character, ItemData item)
        {
            if (character == null || item == null) return;

            switch (item.EffectType)
            {
                case ItemEffectType.MaxHPUp:
                    character.Health.SetMaxHP(character.Health.MaxHP + item.EffectValue);
                    break;
                case ItemEffectType.ATKUp:
                    character.Stats.AddPermanentBase(StatType.ATK, item.EffectValue);
                    break;
                case ItemEffectType.DEFUp:
                    character.Stats.AddPermanentBase(StatType.DEF, item.EffectValue);
                    break;
                // HealPerTurn, ExtraGold, DrawWeightBonus는 GameRunState 플래그로 처리
            }
        }

        /// <summary>
        /// 턴 시작 시 HealtPerTurn 아이템 효과 적용
        /// </summary>
        public static void ApplyHealPerTurn(Character character, int healAmount)
        {
            if (character != null && character.IsAlive && healAmount > 0)
                character.Health.Heal(healAmount);
        }

        /// <summary>
        /// ExtraGold 아이템 보너스 골드 계산
        /// </summary>
        public static int CalculateBonusGold(int baseGold, int bonusPercent)
        {
            return baseGold * bonusPercent / 100;
        }
    }
}
