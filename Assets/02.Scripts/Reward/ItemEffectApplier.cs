using TeamLog.Characters;

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
            }
        }
    }
}
