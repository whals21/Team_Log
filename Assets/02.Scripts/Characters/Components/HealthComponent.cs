using System.Collections.Generic;
using UnityEngine;

namespace TeamLog.Characters
{
    /// <summary>
    /// 체력 관리 컴포넌트
    /// Phase CC P1: 쉴드를 List<ShieldInstance>로 관리. 부여자(caster) 추적 + 흡수 시 부여자에게 알림.
    /// </summary>
    public class HealthComponent
    {
        private int _currentHP;
        private int _maxHP;
        private readonly List<ShieldInstance> _shields = new();
        private int _cachedTotalShield;  // 매번 Sum 호출 방지용 캐시
        private bool _shieldCacheDirty = true;
        private bool _isDead;

        public int CurrentHP => _currentHP;
        public int MaxHP => _maxHP;
        public int CurrentShield
        {
            get
            {
                if (_shieldCacheDirty)
                {
                    _cachedTotalShield = 0;
                    foreach (var s in _shields) _cachedTotalShield += s.Amount;
                    _shieldCacheDirty = false;
                }
                return _cachedTotalShield;
            }
        }
        public bool IsDead => _isDead;
        public bool IsAlive => !_isDead;

        public event System.Action<int, int> OnHPChanged;    // current, max
        public event System.Action<int> OnShieldChanged;     // currentShield
        public event System.Action OnDeath;

        public event System.Action<int> OnDamageTaken;       // HP 실제 손실량 (쉴드 흡수 후)
        public event System.Action<int> OnHealApplied;       // 실제 회복량
        public event System.Action<int> OnShieldAdded;       // 쉴드 획득량

        /// <summary>
        /// Phase CC P1: 쉴드 흡수 발생 — (caster, owner, absorbed, attacker, flags).
        /// caster가 부여한 쉴드가 attacker에 의해 owner(이 캐릭터)에서 흡수됨.
        /// CombatEventBus나 외부 처리기가 구독 → Vengeance 축적 / Charge 부여.
        /// </summary>
        public event System.Action<Character, Character, int, Character, ShieldFlag> OnShieldAbsorbed;

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
            _shields.Clear();
            _shieldCacheDirty = true;
            _isDead = false;
        }

        public void TakeDamage(int damage, Character attacker = null)
        {
            if (_isDead) return;

            // 쉴드가 먼저 데미지를 흡수 — 리스트 순회하며 각 쉴드에서 차감
            if (_shields.Count > 0 && damage > 0)
            {
                for (int i = _shields.Count - 1; i >= 0 && damage > 0; i--)
                {
                    var shield = _shields[i];
                    int absorbed = Mathf.Min(damage, shield.Amount);
                    shield.Amount -= absorbed;
                    damage -= absorbed;

                    // 부여자에게 흡수 알림 (Duran Vengeance 축적 / Taranis Charge 역부여)
                    if (absorbed > 0)
                        OnShieldAbsorbed?.Invoke(shield.Caster, _owner, absorbed, attacker, shield.Flags);
                }

                // 소진된 쉴드 제거
                _shields.RemoveAll(s => s.Amount <= 0);
                _shieldCacheDirty = true;
                OnShieldChanged?.Invoke(CurrentShield);
            }

            if (damage > 0)
            {
                _currentHP = Mathf.Max(0, _currentHP - damage);
                OnHPChanged?.Invoke(_currentHP, _maxHP);
                OnDamageTaken?.Invoke(damage);
                _owner?.MarkHitThisTurn(); // Phase CC-2G-5: FollowUp 추적
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
            _owner?.MarkHitThisTurn(); // Phase CC-2G-5: FollowUp 추적

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
            _shields.Clear();
            _shieldCacheDirty = true;
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

        // ═══════════════════════════════════════════
        // Phase CC P1: Shield API (부여자 추적)
        // ═══════════════════════════════════════════

        /// <summary>기본 AddShield — 부여자 불명 (null) + 속성 없음.</summary>
        public void AddShield(int amount) => AddShield(null, amount, ShieldFlag.None);

        /// <summary>부여자 명시 AddShield — Phase CC P1.</summary>
        public void AddShield(Character caster, int amount, ShieldFlag flags = ShieldFlag.None)
        {
            if (_isDead || amount <= 0) return;

            _shields.Add(new ShieldInstance
            {
                Caster = caster,
                Amount = amount,
                Flags = flags,
            });
            _shieldCacheDirty = true;
            OnShieldChanged?.Invoke(CurrentShield);
            OnShieldAdded?.Invoke(amount);
        }

        public void ResetShield()
        {
            if (_shields.Count == 0) return;

            _shields.Clear();
            _shieldCacheDirty = true;
            OnShieldChanged?.Invoke(CurrentShield);
        }

        /// <summary>외부에서 OnShieldAbsorbed 구독용 (Character 생성 시 사용).</summary>
        internal void SetOwner(Character owner) => _owner = owner;
        private Character _owner;

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
            OnShieldAbsorbed = null;
        }
    }
}
