using System.Collections.Generic;
using UnityEngine;

namespace TeamLog.Characters
{
    /// <summary>
    /// 스탯 관리 컴포넌트
    /// </summary>
    public class StatComponent
    {
        private readonly Dictionary<StatType, int> _baseStats = new();
        private readonly Dictionary<StatType, int> _modifiers = new();

        public void Initialize(int baseATK, int baseDEF)
        {
            _baseStats[StatType.ATK] = baseATK;
            _baseStats[StatType.DEF] = baseDEF;
            _modifiers[StatType.ATK] = 0;
            _modifiers[StatType.DEF] = 0;
        }

        public int GetStat(StatType type)
        {
            int baseValue = _baseStats.GetValueOrDefault(type, 0);
            int modifier = _modifiers.GetValueOrDefault(type, 0);
            return Mathf.Max(0, baseValue + modifier);
        }

        public int GetBaseStat(StatType type) => _baseStats.GetValueOrDefault(type, 0);

        public void AddModifier(StatType type, int value)
        {
            _modifiers[type] = _modifiers.GetValueOrDefault(type, 0) + value;
        }

        public void ClearModifiers()
        {
            var keys = new List<StatType>(_modifiers.Keys);
            foreach (var key in keys)
                _modifiers[key] = 0;
        }

        public void AddPermanentBase(StatType type, int value)
        {
            _baseStats[type] = _baseStats.GetValueOrDefault(type, 0) + value;
        }
    }

    public enum StatType
    {
        ATK,
        DEF
    }
}
