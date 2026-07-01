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

        /// <summary>
        /// 사망 직전 호출. true 반환 시 HP=1로 생존 (Immortal 특성용)
        /// </summary>
        public event System.Func<bool> OnPreDeath;

        /// <summary>Phase CC-0: 부활 시 발생. 유물/특성 훅 확장점.</summary>
        public event System.Action<int> OnRevived;           // 부활 직후 HP

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
                // 사망 방지 특성 체크 (Immortal 등)
                if (OnPreDeath != null && OnPreDeath.Invoke())
                {
                    _currentHP = 1;
                    OnHPChanged?.Invoke(_currentHP, _maxHP);
                }
                else
                {
                    _isDead = true;
                    OnDeath?.Invoke();
                }
            }
        }

        /// <summary>
        /// Phase BK: 쉴드/이벤트를 우회하여 HP를 직접 감소시키는 순수 피해.
        /// Pierce(쉴드 무시), Execution(절대 HP 임계 즉사)에서 사용.
        /// 사망 처리와 OnHPChanged/OnDamageTaken 이벤트는 정상 발생.
        /// </summary>
        public void TakeDirectDamage(int damage)
        {
            if (_isDead || damage <= 0) return;

            _currentHP = Mathf.Max(0, _currentHP - damage);
            OnHPChanged?.Invoke(_currentHP, _maxHP);
            OnDamageTaken?.Invoke(damage);

            if (_currentHP <= 0)
            {
                if (OnPreDeath != null && OnPreDeath.Invoke())
                {
                    _currentHP = 1;
                    OnHPChanged?.Invoke(_currentHP, _maxHP);
                }
                else
                {
                    _isDead = true;
                    OnDeath?.Invoke();
                }
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

        /// <summary>
        /// Phase CC-0: MaxHP에 곱셈 modifier를 영구 적용 (0.9 = -10%).
        /// 현재 HP는 새 MaxHP를 초과하지 않도록 클램프. 부활 누적 페널티용.
        /// </summary>
        public void ApplyMaxHpModifier(float multiplier)
        {
            int newMax = Mathf.Max(1, Mathf.RoundToInt(_maxHP * multiplier));
            _maxHP = newMax;
            _currentHP = Mathf.Min(_currentHP, _maxHP);
            OnHPChanged?.Invoke(_currentHP, _maxHP);
        }

        /// <summary>
        /// Phase CC-0: 사망 상태에서 부활.
        /// hpPercentage(0~1) 비율만큼 MaxHP 기준으로 HP 회복.
        /// 부활은 사망한 캐릭터에게만 의미 있음 (이미 생존이면 Heal과 동일).
        /// </summary>
        public void Revive(float hpPercentage)
        {
            int targetHP = Mathf.Max(1, Mathf.RoundToInt(_maxHP * Mathf.Clamp01(hpPercentage)));
            _isDead = false;
            _currentHP = Mathf.Min(targetHP, _maxHP);
            _currentShield = 0;
            OnHPChanged?.Invoke(_currentHP, _maxHP);
            OnRevived?.Invoke(_currentHP);
        }

        /// <summary>
        /// Phase CC-0: HP를 MaxHP까지 완전 회복 (생존자 전투 종료 시).
        /// </summary>
        public void HealToFull()
        {
            if (_isDead) return;
            int previousHP = _currentHP;
            _currentHP = _maxHP;
            int actualHeal = _currentHP - previousHP;
            OnHPChanged?.Invoke(_currentHP, _maxHP);
            if (actualHeal > 0)
                OnHealApplied?.Invoke(actualHeal);
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

        /// <summary>
        /// 모든 이벤트 구독 해제 — 씬 전환 시 BattleSceneSetup에서 호출
        /// </summary>
        public void ClearEvents()
        {
            OnHPChanged = null;
            OnShieldChanged = null;
            OnDeath = null;
            OnDamageTaken = null;
            OnHealApplied = null;
            OnShieldAdded = null;
            OnPreDeath = null;
        }
    }
}
