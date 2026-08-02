using System.Collections.Generic;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Combat.AI;

namespace TeamLog.Combat
{
    /// <summary>
    /// Phase GF (2026-07-20): 보스 페이즈 시스템 연결 partial.
    /// BossPhaseManager(순수 C# 로직) ↔ BattleSceneSetup(Unity 연결) 브릿지.
    ///
    /// 기본 지원 보스: Verdant Terror (잿빛 숲)
    /// - HP 75% 도달: 일반 몹 2체 소환 (Ashwood Wisp + Sporecaller)
    /// - HP 50% 도달: 보스 사망 + 분신 2체 스폰 (Left Half + Right Half)
    ///
    /// 사용자 인스펙터 연결 필수:
    /// - _greyForestSummonPool: Enemy_AshwoodWisp, Enemy_Sporecaller 에셋 배열
    /// - _greyForestSplitPool: Enemy_BossVTLeftHalf, Enemy_BossVTRightHalf 에셋 배열
    /// </summary>
    public partial class BattleSceneSetup
    {
        [Header("Phase GF — Boss Phase Spawns")]
        [Tooltip("HP 75% 도달 시 소환할 일반 적 에셋 (잿빛 숲: AshwoodWisp, Sporecaller)")]
        [SerializeField] private CharacterData[] _greyForestSummonPool;

        [Tooltip("HP 50% 도달 시 스폰할 분신 에셋 (잿빛 숲: BossVTLeftHalf, BossVTRightHalf)")]
        [SerializeField] private CharacterData[] _greyForestSplitPool;

        private BossPhaseManager _bossPhaseManager;

        /// <summary>InitializeBattle() 마지막에 호출 — 보스 페이즈 관리자 초기화.</summary>
        private void InitializeBossPhaseManager()
        {
            // 보스 감지: _enemies 중 IsBoss인 첫 번째 적
            Character boss = null;
            foreach (var enemy in _enemies)
            {
                if (enemy != null && enemy.Data != null && enemy.Data.IsBoss)
                {
                    boss = enemy;
                    break;
                }
            }

            if (boss == null)
            {
                // 보스 아님 — 페이즈 시스템 비활성 (일반 전투)
                return;
            }

            // Manager 인스턴스 생성 (매 전투마다 신규 — 이전 구독 누수 방지)
            if (_bossPhaseManager != null)
            {
                CombatEventBus.OnDamageReceived -= OnBossDamaged;
                _bossPhaseManager.Clear();
            }

            _bossPhaseManager = new BossPhaseManager();
            _bossPhaseManager.OnSummonPhase += HandleSummonPhase;
            _bossPhaseManager.OnSplitPhase += HandleSplitPhase;
            _bossPhaseManager.RegisterBoss(boss);

            // 매 데미지 시 임계값 체크
            CombatEventBus.OnDamageReceived += OnBossDamaged;

            Debug.Log($"[BossPhaseManager] 보스 등록: {boss.Data.CharacterName} (HP {boss.Health.MaxHP})");
        }

        private void OnBossDamaged(Character target, int amount)
        {
            if (_bossPhaseManager == null || !_bossPhaseManager.IsActive) return;
            if (target != _bossPhaseManager.Boss) return;

            _bossPhaseManager.CheckPhaseTransitions();
        }

        /// <summary>HP 75% 도달 콜백 — 일반 몹 소환.</summary>
        private void HandleSummonPhase()
        {
            if (_greyForestSummonPool == null || _greyForestSummonPool.Length == 0)
            {
                Debug.LogWarning("[BossPhaseManager] _greyForestSummonPool 비어있음 — 인스펙터 연결 필요. 일반 전투로 진행.");
                return;
            }

            Debug.Log("[BossPhaseManager] ★ 75% 소환 페이즈 — 일반 몹 등장");
            foreach (var data in _greyForestSummonPool)
            {
                if (data == null) continue;
                SpawnEnemyFromData(data);
            }
        }

        /// <summary>HP 50% 도달 콜백 — 분신 스폰 + 보스 사망.</summary>
        private void HandleSplitPhase()
        {
            Debug.Log("[BossPhaseManager] ★ 50% 분열 페이즈 — 분신 스폰 + 보스 제거");

            // 1. 분신 먼저 스폰 (전투 종료 방지 — CheckBattleEnd가 모든 적 사망 체크)
            if (_greyForestSplitPool != null)
            {
                foreach (var data in _greyForestSplitPool)
                {
                    if (data == null) continue;
                    SpawnEnemyFromData(data);
                }
            }
            else
            {
                Debug.LogWarning("[BossPhaseManager] _greyForestSplitPool 비어있음 — 인스펙터 연결 필요");
            }

            // 2. 보스 사망 처리 — 분신들이 이미 _enemies에 있으므로 전투 종료 안 됨
            var boss = _bossPhaseManager.Boss;
            if (boss != null && boss.IsAlive)
            {
                boss.Health.TakeDamage(99999);  // 강제 사망
            }

            // 3. 더 이상 페이즈 전환 없음 — 구독 해제
            CombatEventBus.OnDamageReceived -= OnBossDamaged;
        }

        /// <summary>CharacterData로 새 적 생성하여 _enemies에 추가 + AI 컨트롤러 생성.</summary>
        private void SpawnEnemyFromData(CharacterData data)
        {
            var newEnemy = new Character(data);
            _enemies.Add(newEnemy);

            // AI 컨트롤러 생성 — null 패턴 허용 (기본 AI 사용)
            var controller = new EnemyAIController(newEnemy, null, _playerParty);
            // ★ Phase GF (2026-07-22): SingleAlly 스킬용 아군 리스트 주입.
            controller.SetAllies(_enemies);
            _enemyControllers?.Add(controller);

            Debug.Log($"[BossPhaseManager] 스폰: {data.CharacterName} (HP {data.BaseHP}) — 총 적 수: {_enemies.Count}");

            // ★ 주의: UI 갱신은 별도 처리 필요.
            // BattleUIManager.CreateEnemyPanels()은 전투 시작 시 한 번만 호출됨.
            // 새 적이 UI에 표시되지 않을 수 있음 — 플레이 검증 후 BattleUIManager에
            // 동적 적 패널 추가 API 추가 필요 (AddEnemyPanel(Character)).
        }

        /// <summary>전투 종료 시 정리 (OnDestroy에서 호출).</summary>
        private void CleanupBossPhaseManager()
        {
            if (_bossPhaseManager != null)
            {
                CombatEventBus.OnDamageReceived -= OnBossDamaged;
                _bossPhaseManager.Clear();
                _bossPhaseManager = null;
            }
        }
    }
}
