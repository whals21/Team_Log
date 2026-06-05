using System;
using System.Collections.Generic;

namespace TeamLog.Characters
{
    /// <summary>
    /// 상태이상 관리 컴포넌트
    /// </summary>
    public class StatusEffectComponent
    {
        private readonly Dictionary<StatusEffectType, ActiveEffect> _activeEffects = new();

        public event Action OnEffectsChanged;
        public event Action<StatusEffectType> OnEffectApplied;
        public event Action<StatusEffectType> OnEffectExpired;

        public bool HasEffect(StatusEffectType type) => _activeEffects.ContainsKey(type);

        public void ApplyEffect(StatusEffectType type, int duration, int value)
        {
            if (_activeEffects.TryGetValue(type, out var existing))
            {
                existing.RemainingTurns = duration;
                existing.Value = value;
            }
            else
            {
                _activeEffects[type] = new ActiveEffect(type, duration, value);
            }

            OnEffectApplied?.Invoke(type);
            OnEffectsChanged?.Invoke();
        }

        public void RemoveEffect(StatusEffectType type)
        {
            if (_activeEffects.Remove(type))
            {
                OnEffectsChanged?.Invoke();
            }
        }

        /// <summary>
        /// 턴 종료 시 호출 - 지속시간 감소
        /// </summary>
        public List<StatusEffectType> TickTurnEnd()
        {
            var expiredEffects = new List<StatusEffectType>();

            foreach (var kvp in _activeEffects)
            {
                kvp.Value.RemainingTurns--;
                if (kvp.Value.RemainingTurns <= 0)
                    expiredEffects.Add(kvp.Key);
            }

            foreach (var type in expiredEffects)
            {
                OnEffectExpired?.Invoke(type);
                RemoveEffect(type);
            }

            return expiredEffects;
        }

        public void ClearAllEffects()
        {
            if (_activeEffects.Count == 0) return;
            foreach (var type in new List<StatusEffectType>(_activeEffects.Keys))
                _activeEffects.Remove(type);
            OnEffectsChanged?.Invoke();
        }

        public IEnumerable<ActiveEffect> GetAllEffects() => _activeEffects.Values;
    }

    public enum StatusEffectType
    {
        None,
        Poison,
        Burn,
        Stun,
        Freeze,
        Sleep,
        Bleed,
        DefenseUp,
        DefenseDown,
        AttackUp,
        AttackDown,
        Regeneration,
        Shield,
        Taunt
    }

    public class ActiveEffect
    {
        public StatusEffectType Type { get; }
        public int RemainingTurns { get; set; }
        public int Value { get; set; }
        public int Stacks { get; set; } = 1;

        public ActiveEffect(StatusEffectType type, int duration, int value)
        {
            Type = type;
            RemainingTurns = duration;
            Value = value;
        }
    }
}
