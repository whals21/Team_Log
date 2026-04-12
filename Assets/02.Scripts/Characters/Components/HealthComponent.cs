using UnityEngine;

namespace TeamLog.Characters
{
    /// <summary>
    /// 체력 관리 컴포넌트
    /// </summary>
    public class HealthComponent
    {
        private int _currentHP;
        private int _maxHP;
        private bool _isDead;

        public int CurrentHP => _currentHP;
        public int MaxHP => _maxHP;
        public float HPPercent => _maxHP > 0 ? (float)_currentHP / _maxHP : 0f;
        public bool IsDead => _isDead;
        public bool IsAlive => !_isDead;

        public event System.Action<int, int> OnHPChanged;    // current, max
        public event System.Action OnDeath;

        public void Initialize(int maxHP)
        {
            _maxHP = maxHP;
            _currentHP = maxHP;
            _isDead = false;
        }

        public void TakeDamage(int damage)
        {
            if (_isDead) return;

            _currentHP = Mathf.Max(0, _currentHP - damage);
            OnHPChanged?.Invoke(_currentHP, _maxHP);

            if (_currentHP <= 0)
            {
                _isDead = true;
                OnDeath?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (_isDead) return;

            _currentHP = Mathf.Min(_maxHP, _currentHP + amount);
            OnHPChanged?.Invoke(_currentHP, _maxHP);
        }

        public void SetMaxHP(int newMaxHP, bool healToFull = false)
        {
            _maxHP = newMaxHP;
            _currentHP = Mathf.Min(_currentHP, _maxHP);

            if (healToFull)
                _currentHP = _maxHP;

            OnHPChanged?.Invoke(_currentHP, _maxHP);
        }
    }
}
