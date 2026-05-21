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

        public event System.Action<StatType, int> OnStatChanged;

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
            OnStatChanged?.Invoke(type, GetStat(type));
        }

        public void RemoveModifier(StatType type, int value)
        {
            _modifiers[type] = _modifiers.GetValueOrDefault(type, 0) - value;
            OnStatChanged?.Invoke(type, GetStat(type));
        }

        public void ClearModifiers()
        {
            var keys = new List<StatType>(_modifiers.Keys);
            foreach (var key in keys)
                _modifiers[key] = 0;
        }

        public void ResetToBase(StatType type)
        {
            _modifiers[type] = 0;
            OnStatChanged?.Invoke(type, GetStat(type));
        }
    }

    public enum StatType
    {
        ATK,
        DEF
    }
}
