using System;
using TeamLog.Characters;

namespace TeamLog.Combat
{
    /// <summary>
    /// Phase GF (2026-07-20): 보스 HP 임계값 기반 페이즈 전환 관리자.
    /// 순수 C# 로직 — MonoBehaviour 아님. BattleSceneSetup이 인스턴스 보유.
    ///
    /// 기본 임계값 (Verdant Terror — 잿빛 숲 보스):
    /// - 75%: 일반 몹 소환 (Ashwood Wisp + Sporecaller)
    /// - 50%: 보스 제거 + 분신 2체 스폰 (Left Half + Right Half)
    ///
    /// 설계 원칙:
    /// - 이 클래스는 "임계값 감지 + 이벤트 발생"만 담당 (Single Responsibility)
    /// - 실제 스폰/제거 로직은 BattleSceneSetup이 Action 콜백으로 처리
    /// - 확장성: 다른 테마 보스도 임계값/콜백만 교체하여 재사용 가능
    /// </summary>
    public class BossPhaseManager
    {
        private Character _boss;
        private bool _summonTriggered;
        private bool _splitTriggered;
        private readonly float _summonThreshold;
        private readonly float _splitThreshold;

        /// <summary>HP 75% 도달 시 발생. BattleSceneSetup이 구독하여 일반 몹 스폰.</summary>
        public event Action OnSummonPhase;

        /// <summary>HP 50% 도달 시 발생. BattleSceneSetup이 구독하여 보스 제거 + 분신 스폰.</summary>
        public event Action OnSplitPhase;

        /// <summary>활성 여부 — 보스 등록 후, 분열 트리거 전까지 true.</summary>
        public bool IsActive => _boss != null && !_splitTriggered;

        /// <summary>등록된 보스 캐릭터 (없으면 null).</summary>
        public Character Boss => _boss;

        public BossPhaseManager(float summonThreshold = 0.75f, float splitThreshold = 0.50f)
        {
            _summonThreshold = summonThreshold;
            _splitThreshold = splitThreshold;
        }

        /// <summary>
        /// 전투 시작 시 보스 등록. 보스가 없으면 null 전달 (비활성).
        /// 동일 인스턴스 재사용 시 이전 상태 초기화됨.
        /// </summary>
        public void RegisterBoss(Character boss)
        {
            _boss = boss;
            _summonTriggered = false;
            _splitTriggered = false;
        }

        /// <summary>
        /// 매 데미지 후 호출 — HP 임계값 체크.
        /// 권장 구독: CombatEventBus.OnDamageReceived += (target, amount) => { if (target == boss) CheckPhaseTransitions(); };
        /// </summary>
        public void CheckPhaseTransitions()
        {
            if (_boss == null || _boss.IsDead) return;

            float ratio = (float)_boss.Health.CurrentHP / _boss.Health.MaxHP;

            if (!_summonTriggered && ratio <= _summonThreshold)
            {
                _summonTriggered = true;
                OnSummonPhase?.Invoke();
            }

            if (!_splitTriggered && ratio <= _splitThreshold)
            {
                _splitTriggered = true;
                OnSplitPhase?.Invoke();
            }
        }

        /// <summary>전투 종료 시 리셋 — 이벤트 구독 해제와 함께.</summary>
        public void Clear()
        {
            _boss = null;
            _summonTriggered = false;
            _splitTriggered = false;
            OnSummonPhase = null;
            OnSplitPhase = null;
        }
    }
}
