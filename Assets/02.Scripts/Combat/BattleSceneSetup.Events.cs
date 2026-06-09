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
            c.Health.OnDamageTaken += _ => _vfxManager?.PlayHitEffect(
                _battleUIManager?.GetPanelTransform(c));
            c.Health.OnDamageTaken += _ => CameraShake.Instance.Shake(_mainCanvasRect, 0.15f, 5f);
            c.Health.OnHealApplied += _ => _vfxManager?.PlayHealEffect(
                _battleUIManager?.GetPanelTransform(c));
            c.Health.OnShieldAdded += _ => _vfxManager?.PlayShieldEffect(
                _battleUIManager?.GetPanelTransform(c));
            c.Health.OnDeath += () => AudioManager.Instance.PlayCharacterDeath();
            c.Health.OnDeath += () => _vfxManager?.PlayDeathEffect(
                _battleUIManager?.GetPanelTransform(c));
            c.StatusEffects.OnEffectsChanged += () => OnCharacterStateChanged(c);
            c.StatusEffects.OnEffectApplied += effect => OnStatusEffectApplied(effect);
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
            switch (skill.Type)
            {
                case SkillType.Attack:
                    PlayAttackSound(skill);
                    break;
                case SkillType.Heal:
                    AudioManager.Instance.PlayHealImpact();
                    break;
                case SkillType.Buff:
                    AudioManager.Instance.PlayBuffCast();
                    break;
                case SkillType.Debuff:
                    AudioManager.Instance.PlayDebuffCast();
                    break;
                case SkillType.Shield:
                    AudioManager.Instance.PlayShieldCast();
                    break;
                case SkillType.Purify:
                    AudioManager.Instance.PlayPurifyCast();
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

        private void OnStatusEffectApplied(StatusEffectType effect)
        {
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

        private void OnBattleEnded()
        {
            _actionController?.Shutdown();
            _battleUIManager?.UpdateAllPanels();

            bool victory = _enemies.TrueForAll(e => e.IsDead);
            _battleUIManager?.AddLog(victory ? "전투 승리!" : "전투 패배...");

            BattleResult.SetResult(victory);

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
            SceneTransition.Instance.FadeToScene("MapScene");
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
        }

        #endregion
    }
}
