// BattleSceneSetup.Events.cs — 이벤트 핸들러, 사운드, VFX, 클린업
// 진입점+초기화: BattleSceneSetup.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TeamLog.Characters;
using TeamLog.Combat.AI;
using TeamLog.Combat.Turn;
using SkillExecutor = TeamLog.Combat.Turn.SkillExecutor;
using TeamLog.Map;
using TeamLog.UI;
using TeamLog.UI.Battle;

namespace TeamLog.Combat
{
    public partial class BattleSceneSetup
    {
        // ── 순차 적 턴 타이밍 — Time.timeScale의 영향을 받아 1x/2x 자동 적용 ──
        private const float EnemyTurnEntryDelay = 0.30f;   // 적 턴 진입 후 대기
        private const float EnemyHighlightDelay = 0.15f;   // 하이라이트 후 행동까지
        private const float EnemyActionVfxDelay = 0.55f;   // 행동 후 VFX 대기
        private bool _isEnemyTurnRunning;                   // 코루틴 중복 실행 가드

        #region Character Event Subscription

        /// <summary>
        /// 캐릭터 HP/쉴드/상태이상 이벤트 공통 구독 헬퍼
        /// </summary>
        private void SubscribeCharacterEvents(Character c)
        {
            c.Health.OnHPChanged += (hp, max) => OnCharacterStateChanged(c);
            c.Health.OnShieldChanged += (shield) => OnCharacterStateChanged(c);
            c.Health.OnDamageTaken += amount => SpawnFloatingText(c, $"-{amount}", FloatingTextUI.DamageColor);
            c.Health.OnHealApplied += amount => SpawnFloatingText(c, $"+{amount}", FloatingTextUI.HealColor);
            c.Health.OnShieldAdded += amount => SpawnFloatingText(c, $"+{amount}", FloatingTextUI.ShieldColor);
            c.Health.OnDamageTaken += _ => _battleUIManager?.FlashPanelForCharacter(c);
            c.Health.OnDamageTaken += amount => PlayDamageVFX(c, amount);
            c.Health.OnHealApplied += _ => _vfxManager?.PlayHealEffect(
                _battleUIManager?.GetPanelTransform(c));
            c.Health.OnShieldAdded += _ => _vfxManager?.PlayShieldEffect(
                _battleUIManager?.GetPanelTransform(c));
            c.Health.OnDeath += () => AudioManager.Instance.PlayCharacterDeath();
            c.Health.OnDeath += () => _vfxManager?.PlayDeathEffect(
                _battleUIManager?.GetPanelTransform(c));
            c.StatusEffects.OnEffectsChanged += () => OnCharacterStateChanged(c);
            c.StatusEffects.OnEffectApplied += effect => OnStatusEffectApplied(effect, c);
        }

        #endregion

        #region Skill & Status Effect Handlers

        private static readonly HashSet<StatusEffectType> BuffEffects = new()
        {
            StatusEffectType.AttackUp, StatusEffectType.DefenseUp,
            StatusEffectType.Regeneration, StatusEffectType.Shield
        };

        private static readonly HashSet<StatusEffectType> DebuffEffects = new()
        {
            StatusEffectType.AttackDown, StatusEffectType.DefenseDown,
            StatusEffectType.Poison, StatusEffectType.Burn,
            StatusEffectType.Bleed, StatusEffectType.Stun,
            StatusEffectType.Freeze, StatusEffectType.Sleep
        };

        private void OnSkillApplied(SkillData skill, Character target)
        {
            var panel = _battleUIManager?.GetPanelTransform(target);

            switch (skill.Type)
            {
                case SkillType.Attack:
                    PlayAttackSound(skill);
                    PlayAttackVFX(skill, panel);
                    break;
                case SkillType.Heal:
                    AudioManager.Instance.PlayHealImpact();
                    break; // Heal VFX는 OnHealApplied에서 재생
                case SkillType.Buff:
                    AudioManager.Instance.PlayBuffCast();
                    _vfxManager?.PlayBuffEffect(panel);
                    break;
                case SkillType.Debuff:
                    AudioManager.Instance.PlayDebuffCast();
                    _vfxManager?.PlayDebuffEffect(panel);
                    break;
                case SkillType.Shield:
                    AudioManager.Instance.PlayShieldCast();
                    break; // Shield VFX는 OnShieldAdded에서 재생
                case SkillType.Purify:
                    AudioManager.Instance.PlayPurifyCast();
                    _vfxManager?.PlayPurifyEffect(panel);
                    break;
            }
        }

        private void PlayAttackSound(SkillData skill)
        {
            if (skill.StatusEffect == StatusEffectType.Burn)
                AudioManager.Instance.PlayBurnImpact();
            else if (skill.StatusEffect == StatusEffectType.Poison)
                AudioManager.Instance.PlayPoisonImpact();
            else if (skill.StatusEffect == StatusEffectType.Freeze)
                AudioManager.Instance.PlayFreezeImpact();
            else
                AudioManager.Instance.PlayAttackHit();
        }

        /// <summary>
        /// 공격 스킬 속성별 VFX. 기본 Hit/Critical VFX는 OnDamageTaken → PlayDamageVFX에서 재생됨.
        /// 여기서는 원소별 추가 이펙트(불/독/얼음) 또는 물리 베기 궤적을 겹쳐 재생.
        /// </summary>
        private void PlayAttackVFX(SkillData skill, Transform panel)
        {
            if (skill.StatusEffect == StatusEffectType.Burn)
                _vfxManager?.PlayBurnEffect(panel);
            else if (skill.StatusEffect == StatusEffectType.Poison)
                _vfxManager?.PlayPoisonEffect(panel);
            else if (skill.StatusEffect == StatusEffectType.Freeze)
                _vfxManager?.PlayFreezeEffect(panel);
            else
                _vfxManager?.PlaySlashEffect(panel); // 일반 물리 공격 — 검 궤적
        }

        private void OnStatusEffectApplied(StatusEffectType effect, Character target)
        {
            var panel = _battleUIManager?.GetPanelTransform(target);

            if (effect == StatusEffectType.Stun)
            {
                _vfxManager?.PlayStunEffect(panel);
                AudioManager.Instance.PlayDebuffApply();
                return;
            }

            if (BuffEffects.Contains(effect))
                AudioManager.Instance.PlayBuffApply();
            else if (DebuffEffects.Contains(effect))
                AudioManager.Instance.PlayDebuffApply();
            else
                AudioManager.Instance.PlayStatusEffectApply();
        }

        private void OnAttackMissed(Character target)
        {
            SpawnFloatingText(target, "MISS", FloatingTextUI.DamageColor);
            AudioManager.Instance.PlayMiss();
        }

        #endregion

        #region Turn & Battle Lifecycle Handlers

        private void OnPhaseChanged(TurnPhase oldPhase, TurnPhase newPhase)
        {
            _battleUIManager?.UpdateAllPanels();
        }

        private void OnTurnStarted(int turnNumber)
        {
            _battleUIManager?.UpdateAllPanels();

            foreach (var controller in _enemyControllers)
            {
                if (controller.Owner.IsAlive)
                    controller.PrepareNextAction();
                else
                {
                    int idx = _enemyControllers.IndexOf(controller);
                    _battleUIManager?.SetEnemyIntent(idx, null);
                }
            }
        }

        // ── 순차 적 턴 — 코루틴 주도 실행 ──

        private void OnEnemyTurnSequenceStarted()
        {
            if (_isEnemyTurnRunning) return; // 중복 실행 방지
            StartCoroutine(ExecuteEnemyTurnSequence());
        }

        private void OnEnemyActing(Character enemy)
        {
            _battleUIManager?.HighlightActingEnemy(enemy);
        }

        /// <summary>
        /// 순차 적 턴 코루틴 — 적 한 명씩 행동, 행동 사이 시각적 지연으로 "누가 무엇을 하는지" 인지.
        /// 매 행동 후 전멸 체크 → 파티 전멸 시 즉시 종료.
        /// </summary>
        private IEnumerator ExecuteEnemyTurnSequence()
        {
            _isEnemyTurnRunning = true;
            try
            {
                yield return new WaitForSeconds(EnemyTurnEntryDelay);

                var controllers = _turnManager.EnemyControllers;
                foreach (var controller in controllers)
                {
                    if (controller == null || !controller.Owner.IsAlive) continue;

                    _turnManager.ExecuteSingleEnemyAction(controller);
                    yield return new WaitForSeconds(EnemyHighlightDelay);

                    // 행동한 적의 의도를 즉시 비워서 "이미 행동함" 시각화
                    _battleUIManager?.ClearEnemyIntentFor(controller.Owner);
                    yield return new WaitForSeconds(EnemyActionVfxDelay);

                    _battleUIManager?.ClearActingEnemyHighlight(controller.Owner);

                    // 중간 전멸 체크 → 조기 종료
                    if (_turnManager.IsBattleEndedEarly())
                    {
                        _turnManager.CompleteEnemyTurn();
                        yield break;
                    }
                }

                _turnManager.CompleteEnemyTurn();
            }
            finally
            {
                _isEnemyTurnRunning = false;
            }
        }

        private void OnBattleEnded()
        {
            _actionController?.Shutdown();
            _battleUIManager?.UpdateAllPanels();

            bool victory = _enemies.TrueForAll(e => e.IsDead);
            _battleUIManager?.AddLog(victory ? "전투 승리!" : "전투 패배...");

            BattleResult.SetResult(victory);

            // Phase CC-0: 부활 시스템 — 승리 시 사망자 부활 + MaxHP 0.9배 누적.
            // 패배(파티 전멸) 시 런 종료 플래그 설정.
            bool runEnded = false;
            if (GameRunState.Instance != null && GameRunState.Instance.IsRunActive)
            {
                runEnded = GameRunState.Instance.ProcessBattleEnd(victory);
                if (runEnded)
                    _battleUIManager?.AddLog("런 종료 — 파티 전멸");
            }

            if (_battleEndOverlay != null)
            {
                _battleEndOverlay.Show(victory);
                _battleEndOverlay.OnContinueClicked += OnBattleEndContinue;

                if (_titleManager != null)
                {
                    if (victory) _titleManager.ShowVictory();
                    else _titleManager.ShowDefeat();
                }

                if (victory)
                    _vfxManager?.PlayVictoryEffect();
                else
                    _vfxManager?.PlayDefeatEffect();
            }
            else
            {
                StartCoroutine(BattleEndTransition());
            }
        }

        private void OnBattleEndContinue()
        {
            if (_battleEndOverlay != null)
                _battleEndOverlay.OnContinueClicked -= OnBattleEndContinue;

            StartCoroutine(BattleEndTransition());
        }

        private IEnumerator BattleEndTransition()
        {
            yield return null;
            // _returnSceneName은 BattleTestSceneSetup.SetReturnScene()으로 변경 가능 (기본값 MapScene)
            SceneTransition.Instance.FadeToScene(_returnSceneName);
        }

        #endregion

        #region Character State & Floating Text

        private void OnCharacterStateChanged(Character character)
        {
            _battleUIManager?.UpdateAllPanels();

            if (character.Health.IsDead)
            {
                _battleUIManager?.AddLog($"{character.Name}이(가) 쓰러졌습니다.");

                int enemyIdx = _enemies.IndexOf(character);
                if (enemyIdx >= 0)
                    _battleUIManager?.SetEnemyIntent(enemyIdx, null);
            }
        }

        private void SpawnFloatingText(Character character, string message, Color color)
        {
            var panelTransform = _battleUIManager?.GetPanelTransform(character);
            if (panelTransform == null) return;
            FloatingTextUI.Spawn(panelTransform, message, color, new Vector2(0, 30));
        }

        // ── 데미지 VFX — 크리티컬 히트 감지 + 임팩트 연출 ──

        private bool _hitStopActive;

        /// <summary>
        /// 데미지 타격 VFX — 데미지 비례 강도 조절, 크리티컬 히트(최대 HP 35%+) 감지.
        /// 크리티컬 시: Critical VFX + 강한 흔들림 + 화면 플래시 + 히트스톱.
        /// 일반 시: Hit VFX + 데미지 비례 흔들림.
        /// </summary>
        private void PlayDamageVFX(Character c, int amount)
        {
            var panel = _battleUIManager?.GetPanelTransform(c);

            bool isCritical = c.Health.MaxHP > 0 && amount >= c.Health.MaxHP * 0.35f;

            if (isCritical)
            {
                _vfxManager?.PlayCriticalEffect(panel);
                CameraShake.Instance.Shake(_mainCanvasRect, 0.3f, 12f);
                _screenFlash?.Flash(Color.white, 0.2f);
                StartCoroutine(HitStopRoutine());
            }
            else
            {
                _vfxManager?.PlayHitEffect(panel);
                float ratio = c.Health.MaxHP > 0
                    ? Mathf.Clamp01((float)amount / c.Health.MaxHP)
                    : 0.2f;
                float strength = Mathf.Lerp(3f, 8f, ratio);
                CameraShake.Instance.Shake(_mainCanvasRect, 0.15f, strength);
            }
        }

        /// <summary>
        /// 크리티컬 히트 시 순간 정지 (0.04초) — 타격감 강조.
        /// 중첩 방지: 이미 활성화 중이면 스킵.
        /// </summary>
        private IEnumerator HitStopRoutine()
        {
            if (_hitStopActive) yield break;
            _hitStopActive = true;

            float original = Time.timeScale;
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(0.04f);
            Time.timeScale = original;

            _hitStopActive = false;
        }

        #endregion

        #region Enemy Pattern Loading

        private EnemyActionPattern LoadEnemyPattern(int enemyIndex, Character enemy)
        {
            // 1. 인스펙터에 할당된 패턴 배열에서 매칭 시도
            if (_enemyPatternData != null && enemyIndex < _enemyPatternData.Length && _enemyPatternData[enemyIndex] != null)
                return _enemyPatternData[enemyIndex].CreateRuntimePattern();

            // 2. 캐릭터 Data의 에셋 이름으로 패턴 에셋 자동 탐색 (에디터 전용)
#if UNITY_EDITOR
            string assetName = enemy.Data != null ? enemy.Data.name : "";
            if (!string.IsNullOrEmpty(assetName))
            {
                var patternAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyPatternData>(
                    $"Assets/03.Data/Patterns/Pattern_{assetName}.asset");
                if (patternAsset != null)
                    return patternAsset.CreateRuntimePattern();
            }
#endif

            // 3. 폴백: 캐릭터 Data의 스킬 목록에서 패턴 생성
            if (enemy.Data != null && enemy.Data.Skills.Count > 0)
                return new EnemyActionPattern(enemy.Data.Skills);

            // 4. 최종 폴백: 빈 패턴
            return new EnemyActionPattern(new SkillData[0]);
        }

        private void OnEnemyIntentChanged(int enemyIndex, EnemyIntent intent)
        {
            _battleUIManager?.SetEnemyIntent(enemyIndex, intent);
        }

        #endregion

        #region Delayed Actions

        /// <summary>
        /// 파괴된 MonoBehaviour에서 호출되는 것을 방지하는 지연 액션 헬퍼
        /// </summary>
        private void StartDelayedAction(float delay, System.Action action)
        {
            if (this == null) return;
            StartCoroutine(DelayedCoroutine(delay, action));
        }

        private IEnumerator DelayedCoroutine(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            if (this != null && action != null)
                action();
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // TurnManager 이벤트 해제
            if (_turnManager != null)
            {
                _turnManager.OnPhaseChanged -= OnPhaseChanged;
                _turnManager.OnTurnStarted -= OnTurnStarted;
                _turnManager.OnBattleEnded -= OnBattleEnded;
                _turnManager.OnEnemyTurnSequenceStarted -= OnEnemyTurnSequenceStarted;
                _turnManager.OnEnemyActing -= OnEnemyActing;
            }

            _actionController?.Shutdown();

            // 캐릭터 이벤트 해제 — 플레이어는 씬 전환 후에도 유지되므로 반드시 클린업
            foreach (var c in _playerParty)
            {
                c.Health.ClearEvents();
                c.StatusEffects.ClearEvents();

                // OnPreDeath 훅 재구독 — Immortal 특성이 동작하도록 유지
                c.Health.OnPreDeath += () => c.TraitHandler.PreventDeath();
            }
            foreach (var c in _enemies)
            {
                c.Health.ClearEvents();
                c.StatusEffects.ClearEvents();
            }

            // 정적 이벤트 클린업
            SkillExecutor.OnSkillApplied -= OnSkillApplied;
            DamageCalculator.OnAttackMissed -= OnAttackMissed;
            DamageCalculator.ClearEvents();
            SkillExecutor.ClearEvents();

            // 유물 이벤트 해제
            GameRunState.Instance?.RelicHandler.UnsubscribeEvents();

            _playerParty.Clear();
            _enemies.Clear();

            // 전투 속도 복원
            Time.timeScale = 1f;

            // _returnSceneName 리셋 — 다음 전투는 기본적으로 MapScene 복귀
            // (BattleTestSceneSetup이 SetReturnScene으로 바꾼 경우 다음 전투에 영향 안 가도록)
            _returnSceneName = "MapScene";
        }

        #endregion
    }
}
