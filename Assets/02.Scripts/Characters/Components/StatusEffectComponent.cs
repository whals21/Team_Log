using System.Collections.Generic;

namespace TeamLog.Characters
{
    /// <summary>
    /// 상태이상 관리 컴포넌트
    /// </summary>
    public class StatusEffectComponent
    {
        private readonly Dictionary<StatusEffectType, ActiveEffect> _activeEffects = new();

        public event System.Action<StatusEffectType, bool> OnEffectChanged; // type, applied

        public bool HasEffect(StatusEffectType type) => _activeEffects.ContainsKey(type);

        public int GetRemainingDuration(StatusEffectType type)
        {
            return _activeEffects.TryGetValue(type, out var effect) ? effect.RemainingTurns : 0;
        }

        public void ApplyEffect(StatusEffectType type, int duration, int value)
        {
            if (_activeEffects.TryGetValue(type, out var existing))
            {
                // 기존 효과 갱신 (더 긴 쪽 사용 또는 합산)
                existing.RemainingTurns = duration;
                existing.Value = value;
            }
            else
            {
                _activeEffects[type] = new ActiveEffect(type, duration, value);
                OnEffectChanged?.Invoke(type, true);
            }
        }

        public void RemoveEffect(StatusEffectType type)
        {
            if (_activeEffects.Remove(type))
                OnEffectChanged?.Invoke(type, false);
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
                RemoveEffect(type);

            return expiredEffects;
        }

        public void ClearAllEffects()
        {
            foreach (var type in new List<StatusEffectType>(_activeEffects.Keys))
                RemoveEffect(type);
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
        Shield
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
