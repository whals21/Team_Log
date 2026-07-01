using System.Collections.Generic;
using TeamLog.Combat.AI;
using TeamLog.Combat.Draw;
using TeamLog.Map;
using TeamLog.Skill;

// 네임스페이스 충돌 해결
using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;
using SkillInstance = TeamLog.Characters.SkillInstance;
using StatusEffectType = TeamLog.Characters.StatusEffectType;
using TargetType = TeamLog.Characters.TargetType;
using ResourceType = TeamLog.Characters.ResourceType;

// Phase BK: 행동 키워드 타입 별칭
using BehaviorKeyword = TeamLog.Skill.BehaviorKeyword;

namespace TeamLog.Combat.Turn
{
    /// <summary>
    /// 턴 진행 오케스트레이터 — 턴 라이프사이클, AP 관리, 대상 분해만 담당
    /// 데미지 계산: DamageCalculator, 스킬 실행: SkillExecutor
    /// </summary>
    public class TurnManager
    {
        private readonly TurnContext _context;
        private readonly List<Character> _playerParty;
        private readonly List<Character> _enemies;
        private readonly List<EnemyAIController> _enemyControllers;
        private readonly SkillDrawSystem _drawSystem;
        private readonly SkillExecutor _skillExecutor;
        private readonly int _bonusFirstTurnAP;

        public TurnContext Context => _context;
        public SkillDrawSystem DrawSystem => _drawSystem;
        public SkillExecutor SkillExecutor => _skillExecutor;
        public TurnPhase CurrentPhase => _context.CurrentPhase;
        public int TurnNumber => _context.TurnNumber;

        /// <summary>코루틴이 순회할 적 컨트롤러 목록 (읽기 전용).</summary>
        public IReadOnlyList<EnemyAIController> EnemyControllers => _enemyControllers;

        public event System.Action<TurnPhase, TurnPhase> OnPhaseChanged;
        public event System.Action<int> OnTurnStarted;
        public event System.Action OnBattleEnded;
        public event System.Action<int, int> OnAPChanged;

        // 순차 적 턴 모드 — 코루틴 주도 실행 시 사용 (BattleSceneSetup)
        public event System.Action OnEnemyTurnSequenceStarted; // 코루틴 시작 알림
        public event System.Action<Character> OnEnemyActing;   // 개별 적 행동 시작 (하이라이트용)

        private bool _sequentialEnemyTurn;

        public int CurrentAP => _context.CurrentAP;
        public int MaxAP => _context.MaxAP;

        public TurnManager(List<Character> playerParty, List<Character> enemies,
            List<EnemyAIController> enemyControllers = null, int maxRerolls = 1, int bonusFirstTurnAP = 0)
        {
            _playerParty = playerParty;
            _enemies = enemies;
            _enemyControllers = enemyControllers ?? new List<EnemyAIController>();
            _context = new TurnContext();
            _drawSystem = new SkillDrawSystem(playerParty, maxRerolls);
            _skillExecutor = new SkillExecutor(playerParty, enemies);
            _bonusFirstTurnAP = bonusFirstTurnAP;

            _context.OnPhaseChanged += (old, newPhase) => OnPhaseChanged?.Invoke(old, newPhase);
            _context.OnTurnStarted += turn => OnTurnStarted?.Invoke(turn);
            _context.OnAPChanged += (current, max) => OnAPChanged?.Invoke(current, max);
        }

        public void StartBattle()
        {
            // Phase 8C: 플레이어 장착 특성 처리기 구독
            foreach (var c in _playerParty)
            {
                if (c.PlayerTraitHandler != null && c.PlayerTraitHandler.HasTrait)
                    c.PlayerTraitHandler.SubscribeEvents();

                // Phase ARCH-5: 모든 스킬의 UsesThisBattle 리셋 (Fatigue/Momentum/Escalation/Mastery)
                foreach (var inst in c.SkillInventory.SkillInstances)
                    inst.ResetUsesThisBattle();
            }

            CombatEventBus.FireBattleStart();
            StartNewTurn();
        }

        public void StartNewTurn()
        {
            _context.StartNewTurn();

            // 턴 시작 시 모든 캐릭터의 스탯 수정자 재계산
            foreach (var c in _playerParty) if (c.IsAlive) c.ApplyStatModifiers();
            foreach (var c in _enemies) if (c.IsAlive) c.ApplyStatModifiers();

            // 턴 시작 시 모든 캐릭터의 쉴드 리셋
            foreach (var c in _playerParty) if (c.IsAlive) c.Health.ResetShield();
            foreach (var c in _enemies) if (c.IsAlive) c.Health.ResetShield();

            // 적 특성: 턴 시작 훅 (Regenerate, Sturdy, PhaseShift, Rampage, Shell)
            foreach (var c in _enemies) if (c.IsAlive) c.TraitHandler.OnTurnStart(_context.TurnNumber);

            // 키워드: HPPerTurn — 매턴 HP 변화 (저주: 감소, 재생: 회복)
            foreach (var c in _playerParty)
            {
                if (!c.IsAlive) continue;
                int hpChange = SkillExecutor.GetKeywordSumForCharacter(c, KeywordType.HPPerTurn);
                if (hpChange > 0) c.Health.Heal(hpChange);
                else if (hpChange < 0) c.Health.TakeDamage(-hpChange);
            }

            // Phase CC: 캐릭터 고유 자원 턴 시작 훅 (Ashe Ember +1, Lumi Frost 유지 등)
            foreach (var c in _playerParty)
                if (c.IsAlive && c.Resource != null) c.Resource.OnTurnStart(c);

            // AP 리셋: 기본 1 + 생존 파티원 수 (+ 첫 턴 보너스 + 유물 ExtraAP + 장착 특성 ExtraAP)
            int aliveCount = _playerParty.FindAll(p => p.IsAlive).Count;
            int bonus = (_context.TurnNumber == 1) ? _bonusFirstTurnAP : 0;
            int relicAP = GameRunState.Instance?.RelicHandler.GetExtraAP() ?? 0;
            int traitAP = 0;
            foreach (var c in _playerParty)
            {
                if (c.IsAlive && c.PlayerTraitHandler != null && c.PlayerTraitHandler.HasTrait)
                    traitAP += c.PlayerTraitHandler.GetExtraAP();
            }
            _context.ResetAP(1 + aliveCount + bonus + relicAP + traitAP);

            ExecuteDrawPhase();

            CombatEventBus.FireTurnStart(_context.TurnNumber);
        }

        private void ExecuteDrawPhase()
        {
            _context.SetPhase(TurnPhase.Draw);
            _drawSystem.ExecuteDraw();
            _context.SetPhase(TurnPhase.PlayerAction);
        }

        public bool RerollSlot(int slotIndex)
        {
            if (CurrentPhase != TurnPhase.PlayerAction) return false;
            return _drawSystem.RerollSlot(slotIndex);
        }

        /// <summary>
        /// 스킬 즉시 시전 — 대상 분해 + AP 관리 후 SkillExecutor에 위임
        /// </summary>
        public bool ExecuteSkillImmediately(Character caster, SkillData skill, Character target)
        {
            return ExecuteSkillImmediately(caster, skill, target, null);
        }

        public bool ExecuteSkillImmediately(Character caster, SkillData skill, Character target, SkillInstance instance)
        {
            if (caster.IsDead) return false;

            // AP 체크 — 증강 + 장착 특성 반영 코스트 사용
            int effectiveCost = instance != null ? instance.EffectiveCost : skill.Cost;
            // Phase 8C: 장착 특성 CostAdd (궁수 속사 등)
            if (caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
                effectiveCost += caster.PlayerTraitHandler.QueryKeywordSum(KeywordType.CostAdd);
            effectiveCost = System.Math.Max(0, effectiveCost);
            if (!_context.CanAfford(effectiveCost))
                return false;

            // Phase CC: 자원 소모 체크 (Ember/Vengeance/Frost 등)
            // 스킬이 자원을 소모하고, 시전자가 해당 자원을 가졌을 때만 검사.
            // 자원 부족 시 스킬 사용 불가 (AP도 소비 안 함). 실제 소모는 스킬 실행 후.
            if (caster.Resource != null
                && skill.ResourceCostType != ResourceType.None
                && skill.ResourceCostType == caster.Resource.Resource
                && !caster.Resource.CanConsume(skill.ResourceCostAmount))
            {
                return false;
            }

            _context.SpendAP(effectiveCost);

            switch (skill.Target)
            {
                case TargetType.Self:
                case TargetType.SingleAlly:
                    if (target != null)
                        _skillExecutor.ExecuteSkillInternal(caster, skill, target, instance);
                    break;
                case TargetType.SingleEnemy:
                    if (target != null && target.IsAlive)
                    {
                        // Phase BK: Spread 행동 키워드 — 단일 → 광역 (위력 100%)
                        bool spread = instance != null && instance.HasBehavior(BehaviorKeyword.Spread);
                        // Phase BK: AOEAuto 행동 키워드 — 자동 광역 (위력 100%, 코스트 +2)
                        bool aoeAuto = instance != null && instance.HasBehavior(BehaviorKeyword.AOEAuto);

                        if (spread || aoeAuto)
                        {
                            foreach (var enemy in _enemies)
                                if (enemy.IsAlive) _skillExecutor.ExecuteSkillInternal(caster, skill, enemy, instance, 1f);

                            // Explosion: 광역 후 무작위 N명 추가 타격 (중복 허용, 위력 그대로)
                            int explosionN = instance?.GetBehaviorRank(BehaviorKeyword.Explosion) ?? 0;
                            if (explosionN > 0)
                            {
                                var aliveForExplosion = _enemies.FindAll(e => e.IsAlive);
                                if (aliveForExplosion.Count > 0)
                                {
                                    for (int i = 0; i < explosionN; i++)
                                    {
                                        int idx = UnityEngine.Random.Range(0, aliveForExplosion.Count);
                                        _skillExecutor.ExecuteSkillInternal(caster, skill, aliveForExplosion[idx], instance, 1f);
                                        aliveForExplosion.RemoveAll(e => !e.IsAlive);
                                        if (aliveForExplosion.Count == 0) break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 기본 단일 타격
                            _skillExecutor.ExecuteSkillInternal(caster, skill, target, instance, 1f);

                            // Bounce: 무작위 적 N회 추가 (같은 적 중복 허용, 위력 100%)
                            int bounceN = instance?.GetBehaviorRank(BehaviorKeyword.Bounce) ?? 0;
                            if (bounceN > 0)
                            {
                                var aliveForBounce = _enemies.FindAll(e => e.IsAlive);
                                if (aliveForBounce.Count > 0)
                                {
                                    for (int i = 0; i < bounceN; i++)
                                    {
                                        int idx = UnityEngine.Random.Range(0, aliveForBounce.Count);
                                        _skillExecutor.ExecuteSkillInternal(caster, skill, aliveForBounce[idx], instance, 1f);
                                        aliveForBounce.RemoveAll(e => !e.IsAlive);
                                        if (aliveForBounce.Count == 0) break;
                                    }
                                }
                            }

                            // MultiHit: 동일 대상 N회 추가 (위력 100%)
                            int multiHitN = instance?.GetBehaviorRank(BehaviorKeyword.MultiHit) ?? 0;
                            if (multiHitN > 0 && target.IsAlive)
                            {
                                for (int i = 0; i < multiHitN; i++)
                                {
                                    if (!target.IsAlive) break;
                                    _skillExecutor.ExecuteSkillInternal(caster, skill, target, instance, 1f);
                                }
                            }
                        }
                    }
                    break;
                case TargetType.AllEnemies:
                    // 기본 광역 타격
                    foreach (var enemy in _enemies)
                        if (enemy.IsAlive) _skillExecutor.ExecuteSkillInternal(caster, skill, enemy, instance, 1f);

                    // Explosion: 광역 후 무작위 N명 추가 타격
                    int allEnemyExplosionN = instance?.GetBehaviorRank(BehaviorKeyword.Explosion) ?? 0;
                    if (allEnemyExplosionN > 0)
                    {
                        var aliveForExplosion = _enemies.FindAll(e => e.IsAlive);
                        if (aliveForExplosion.Count > 0)
                        {
                            for (int i = 0; i < allEnemyExplosionN; i++)
                            {
                                int idx = UnityEngine.Random.Range(0, aliveForExplosion.Count);
                                _skillExecutor.ExecuteSkillInternal(caster, skill, aliveForExplosion[idx], instance, 1f);
                                aliveForExplosion.RemoveAll(e => !e.IsAlive);
                                if (aliveForExplosion.Count == 0) break;
                            }
                        }
                    }
                    break;
                case TargetType.AllAllies:
                    foreach (var ally in _playerParty)
                        if (ally.IsAlive) _skillExecutor.ExecuteSkillInternal(caster, skill, ally, instance);
                    break;
            }

            // Phase CC: 자원 획득/소모 적용 (스킬 실행 후 — 1회 사용당 1번)
            // Ember/Vengeance/Frost 스킬이 자원을 획득 또는 소모.
            if (caster.Resource != null && caster.IsAlive)
            {
                if (skill.ResourceGainType != ResourceType.None
                    && skill.ResourceGainType == caster.Resource.Resource
                    && skill.ResourceGainAmount > 0)
                    caster.Resource.AddStacks(skill.ResourceGainAmount);
                if (skill.ResourceCostType != ResourceType.None
                    && skill.ResourceCostType == caster.Resource.Resource
                    && skill.ResourceCostAmount > 0)
                    caster.Resource.ConsumeStacks(skill.ResourceCostAmount);
            }

            // Phase ARCH-5: 스킬 사용 후 UsesThisBattle 증가
            // (Fatigue/Momentum/Escalation/Mastery의 다음 사용 EffectivePower/Cost에 반영)
            instance?.IncrementUsesThisBattle();

            CheckBattleEnd();
            return CurrentPhase == TurnPhase.BattleEnd;
        }

        public void ConfirmActions()
        {
            if (CurrentPhase != TurnPhase.PlayerAction) return;
            _context.SetPhase(TurnPhase.Execution);

            if (CurrentPhase != TurnPhase.BattleEnd)
                StartEnemyTurn();
        }

        public void StartEnemyTurn()
        {
            _context.SetPhase(TurnPhase.EnemyTurn);

            if (_sequentialEnemyTurn)
            {
                // 코루틴에게 제어 위임 — 동기 실행하지 않음 (런타임 시각화용)
                OnEnemyTurnSequenceStarted?.Invoke();
                return;
            }

            ExecuteEnemyActions(); // 기존 동기 경로 (시뮬레이터)
        }

        /// <summary>순차 적 턴 모드 활성화 — BattleSceneSetup에서 호출.</summary>
        public void EnableSequentialEnemyTurn() => _sequentialEnemyTurn = true;

        /// <summary>
        /// 코루틴에서 적 한 명 행동 실행. 하이라이트 후 호출됨.
        /// </summary>
        public void ExecuteSingleEnemyAction(EnemyAIController controller)
        {
            if (controller != null && controller.Owner.IsAlive)
            {
                OnEnemyActing?.Invoke(controller.Owner);
                controller.ExecuteAction();
            }
        }

        /// <summary>
        /// 코루틴 종료 후 호출 — ProcessTurnEnd + CheckBattleEnd + StartNewTurn.
        /// ExecuteEnemyActions의 후반부와 동일한 로직.
        /// </summary>
        public void CompleteEnemyTurn()
        {
            ProcessTurnEnd();
            CheckBattleEnd();

            if (CurrentPhase != TurnPhase.BattleEnd)
                StartNewTurn();
        }

        /// <summary>
        /// 매 행동 후 전멸 조기 종료 체크 (코루틴이 사용).
        /// </summary>
        public bool IsBattleEndedEarly()
        {
            return _playerParty.TrueForAll(p => p.IsDead) || _enemies.TrueForAll(e => e.IsDead);
        }

        private void ExecuteEnemyActions()
        {
            // AI 컨트롤러가 있으면 패턴 기반 행동 사용
            if (_enemyControllers.Count > 0)
            {
                foreach (var controller in _enemyControllers)
                {
                    if (controller.Owner.IsAlive)
                        controller.ExecuteAction();
                }
            }
            else
            {
                // 폴백: AI 없으면 기본 공격
                foreach (var enemy in _enemies)
                {
                    if (enemy.IsAlive)
                        ExecuteFallbackEnemyAction(enemy);
                }
            }

            ProcessTurnEnd();
            CheckBattleEnd();

            if (CurrentPhase != TurnPhase.BattleEnd)
                StartNewTurn();
        }

        private void ExecuteFallbackEnemyAction(Character enemy)
        {
            var alivePlayers = _playerParty.FindAll(p => p.IsAlive);
            if (alivePlayers.Count == 0) return;

            // Taunt 상태인 캐릭터가 있으면 우선 타겟
            var tauntTarget = alivePlayers.Find(p => p.StatusEffects.HasEffect(StatusEffectType.Taunt));
            var target = tauntTarget ?? alivePlayers[UnityEngine.Random.Range(0, alivePlayers.Count)];
            DamageCalculator.DealDamage(enemy, target);
        }

        /// <summary>
        /// 턴 종료 처리 — 상태이상 DoT 적용, 지속시간 감소, 만료 효과 제거
        /// </summary>
        private void ProcessTurnEnd()
        {
            foreach (var c in _playerParty) if (c.IsAlive) c.OnTurnEnd();
            foreach (var c in _enemies) if (c.IsAlive) c.OnTurnEnd();

            // Phase CC: 캐릭터 고유 자원 턴 종료 훅 (Ashe Ember×2 자해, Lumi Frost 절반 소실 등)
            foreach (var c in _playerParty)
                if (c.IsAlive && c.Resource != null) c.Resource.OnTurnEnd(c);

            // Phase CC: Taranis Charge 네트워크 — Charge 상태인 적에게 매 턴 연쇄 도트 데미지
            // Charge의 Value = 전하 스택 수. 1스택당 도트 1. (적에게 부여된 상태이상 기반)
            foreach (var c in _enemies)
            {
                if (!c.IsAlive) continue;
                if (c.StatusEffects.HasEffect(StatusEffectType.Charge))
                {
                    foreach (var effect in c.StatusEffects.GetAllEffects())
                    {
                        if (effect.Type == StatusEffectType.Charge && effect.Value > 0)
                        {
                            c.Health.TakeDamage(effect.Value);
                            CombatEventBus.FireDamageReceived(c, effect.Value);
                            break;
                        }
                    }
                }
            }

            // 만료된 효과 제거 후 스탯 수정자 재계산
            foreach (var c in _playerParty) if (c.IsAlive) c.ApplyStatModifiers();
            foreach (var c in _enemies) if (c.IsAlive) c.ApplyStatModifiers();

            CombatEventBus.FireTurnEnd();
        }

        private void CheckBattleEnd()
        {
            bool allPlayersDead = _playerParty.TrueForAll(p => p.IsDead);
            bool allEnemiesDead = _enemies.TrueForAll(e => e.IsDead);

            if (allPlayersDead || allEnemiesDead)
            {
                // Phase 8C: 플레이어 장착 특성 처리기 정리
                foreach (var c in _playerParty)
                {
                    if (c.PlayerTraitHandler != null && c.PlayerTraitHandler.HasTrait)
                        c.PlayerTraitHandler.UnsubscribeEvents();
                }
                _context.SetPhase(TurnPhase.BattleEnd);
                CombatEventBus.FireBattleEnd(allEnemiesDead);
                CombatEventBus.Clear();
                DamageCalculator.ClearEvents();
                SkillExecutor.ClearEvents();
                OnBattleEnded?.Invoke();
            }
        }
    }
}
