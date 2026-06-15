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

        public event System.Action<TurnPhase, TurnPhase> OnPhaseChanged;
        public event System.Action<int> OnTurnStarted;
        public event System.Action OnBattleEnded;
        public event System.Action<int, int> OnAPChanged;

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

            // AP 리셋: 기본 1 + 생존 파티원 수 (+ 첫 턴 보너스 + 유물 ExtraAP)
            int aliveCount = _playerParty.FindAll(p => p.IsAlive).Count;
            int bonus = (_context.TurnNumber == 1) ? _bonusFirstTurnAP : 0;
            int relicAP = GameRunState.Instance?.RelicHandler.GetExtraAP() ?? 0;
            _context.ResetAP(1 + aliveCount + bonus + relicAP);

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

            // AP 체크 — 증강 반영 코스트 사용
            int effectiveCost = instance != null ? instance.EffectiveCost : skill.Cost;
            if (!_context.CanAfford(effectiveCost))
                return false;

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
                        // Spread 증강: 단일→광역, 위력 70%
                        if (instance != null && instance.HasAugment(AugmentType.Spread))
                        {
                            foreach (var enemy in _enemies)
                                if (enemy.IsAlive) _skillExecutor.ExecuteSkillInternal(caster, skill, enemy, instance, 0.7f);
                        }
                        // 저주 증강: AOEAuto — 자동 광역, 위력 65%
                        else if (instance != null && instance.HasAugment(AugmentType.AOEAuto))
                        {
                            foreach (var enemy in _enemies)
                                if (enemy.IsAlive) _skillExecutor.ExecuteSkillInternal(caster, skill, enemy, instance, 0.65f);
                        }
                        else
                        {
                            _skillExecutor.ExecuteSkillInternal(caster, skill, target, instance);
                        }
                    }
                    break;
                case TargetType.AllEnemies:
                    foreach (var enemy in _enemies)
                        if (enemy.IsAlive) _skillExecutor.ExecuteSkillInternal(caster, skill, enemy, instance);
                    break;
                case TargetType.AllAllies:
                    foreach (var ally in _playerParty)
                        if (ally.IsAlive) _skillExecutor.ExecuteSkillInternal(caster, skill, ally, instance);
                    break;
            }

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
            ExecuteEnemyActions();
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
