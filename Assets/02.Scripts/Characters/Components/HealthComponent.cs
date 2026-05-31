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
        private int _currentShield;
        private bool _isDead;

        public int CurrentHP => _currentHP;
        public int MaxHP => _maxHP;
        public int CurrentShield => _currentShield;
        public bool IsDead => _isDead;
        public bool IsAlive => !_isDead;

        public event System.Action<int, int> OnHPChanged;    // current, max
        public event System.Action<int> OnShieldChanged;     // currentShield
        public event System.Action OnDeath;

        public event System.Action<int> OnDamageTaken;       // HP 실제 손실량 (쉴드 흡수 후)
        public event System.Action<int> OnHealApplied;       // 실제 회복량
        public event System.Action<int> OnShieldAdded;       // 쉴드 획득량

        public void Initialize(int maxHP)
        {
            _maxHP = maxHP;
            _currentHP = maxHP;
            _currentShield = 0;
            _isDead = false;
        }

        public void TakeDamage(int damage)
        {
            if (_isDead) return;

            // 쉴드가 먼저 데미지를 흡수
            if (_currentShield > 0)
            {
                if (damage <= _currentShield)
                {
                    _currentShield -= damage;
                    damage = 0;
                }
                else
                {
                    damage -= _currentShield;
                    _currentShield = 0;
                }
                OnShieldChanged?.Invoke(_currentShield);
            }

            if (damage > 0)
            {
                _currentHP = Mathf.Max(0, _currentHP - damage);
                OnHPChanged?.Invoke(_currentHP, _maxHP);
                OnDamageTaken?.Invoke(damage);
            }

            if (_currentHP <= 0)
            {
                _isDead = true;
                OnDeath?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (_isDead) return;

            int previousHP = _currentHP;
            _currentHP = Mathf.Min(_maxHP, _currentHP + amount);
            int actualHeal = _currentHP - previousHP;
            OnHPChanged?.Invoke(_currentHP, _maxHP);
            if (actualHeal > 0)
                OnHealApplied?.Invoke(actualHeal);
        }

        public void SetMaxHP(int newMaxHP, bool healToFull = false)
        {
            _maxHP = newMaxHP;
            _currentHP = Mathf.Min(_currentHP, _maxHP);

            if (healToFull)
                _currentHP = _maxHP;

            OnHPChanged?.Invoke(_currentHP, _maxHP);
        }

        public void AddShield(int amount)
        {
            if (_isDead) return;

            _currentShield += amount;
            OnShieldChanged?.Invoke(_currentShield);
            OnShieldAdded?.Invoke(amount);
        }

        public void ResetShield()
        {
            if (_currentShield == 0) return;

            _currentShield = 0;
            OnShieldChanged?.Invoke(_currentShield);
        }
    }
}
