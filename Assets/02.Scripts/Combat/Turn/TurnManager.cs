using System.Collections.Generic;
using TeamLog.Combat.AI;
using TeamLog.Combat.Draw;

// 네임스페이스 충돌 해결
using Character = TeamLog.Characters.Character;
using CharacterData = TeamLog.Characters.CharacterData;
using SkillData = TeamLog.Characters.SkillData;
using SkillType = TeamLog.Characters.SkillType;
using StatType = TeamLog.Characters.StatType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;
using TargetType = TeamLog.Characters.TargetType;

namespace TeamLog.Combat.Turn
{
    /// <summary>
    /// 턴 진행 관리자
    /// </summary>
    public class TurnManager
    {
        private readonly TurnContext _context;
        private readonly List<Character> _playerParty;
        private readonly List<Character> _enemies;
        private readonly List<EnemyAIController> _enemyControllers;
        private readonly SkillDrawSystem _drawSystem;

        public TurnContext Context => _context;
        public SkillDrawSystem DrawSystem => _drawSystem;
        public TurnPhase CurrentPhase => _context.CurrentPhase;
        public int TurnNumber => _context.TurnNumber;

        public event System.Action<TurnPhase, TurnPhase> OnPhaseChanged;
        public event System.Action<int> OnTurnStarted;
        public event System.Action OnBattleEnded;
        public event System.Action<int, int> OnAPChanged;

        public int CurrentAP => _context.CurrentAP;
        public int MaxAP => _context.MaxAP;

        public TurnManager(List<Character> playerParty, List<Character> enemies,
            List<EnemyAIController> enemyControllers = null, int maxRerolls = 1)
        {
            _playerParty = playerParty;
            _enemies = enemies;
            _enemyControllers = enemyControllers ?? new List<EnemyAIController>();
            _context = new TurnContext();
            _drawSystem = new SkillDrawSystem(playerParty, maxRerolls);

            _context.OnPhaseChanged += (old, newPhase) => OnPhaseChanged?.Invoke(old, newPhase);
            _context.OnTurnStarted += turn => OnTurnStarted?.Invoke(turn);
            _context.OnAPChanged += (current, max) => OnAPChanged?.Invoke(current, max);
        }

        /// <summary>
        /// 중앙화된 데미지 계산 공식
        /// </summary>
        public static int CalculateDamage(int attackPower, int defense)
        {
            return System.Math.Max(1, attackPower - defense);
        }

        /// <summary>
        /// 중앙화된 데미지 적용: 공격자 ATK + bonusPower - 대상 DEF
        /// </summary>
        public static void DealDamage(Character attacker, Character target, int bonusPower = 0)
        {
            int damage = attacker.Stats.GetStat(StatType.ATK) + bonusPower;
            int defense = target.Stats.GetStat(StatType.DEF);
            target.Health.TakeDamage(CalculateDamage(damage, defense));
        }

        public void StartBattle()
        {
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

            // AP 리셋: 기본 1 + 생존 파티원 수
            int aliveCount = _playerParty.FindAll(p => p.IsAlive).Count;
            _context.ResetAP(1 + aliveCount);

            ExecuteDrawPhase();
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
        /// 스킬 즉시 시전 — 대상 클릭 시 곧바로 실행
        /// </summary>
        public bool ExecuteSkillImmediately(Character caster, SkillData skill, Character target)
        {
            if (caster.IsDead) return false;

            // AP 체크
            if (!_context.CanAfford(skill.Cost))
                return false;

            _context.SpendAP(skill.Cost);

            switch (skill.Target)
            {
                case TargetType.Self:
                case TargetType.SingleAlly:
                    if (target != null)
                        ExecuteSkillInternal(caster, skill, target);
                    break;
                case TargetType.SingleEnemy:
                    if (target != null && target.IsAlive)
                        ExecuteSkillInternal(caster, skill, target);
                    break;
                case TargetType.AllEnemies:
                    foreach (var enemy in _enemies)
                        if (enemy.IsAlive) ExecuteSkillInternal(caster, skill, enemy);
                    break;
                case TargetType.AllAllies:
                    foreach (var ally in _playerParty)
                        if (ally.IsAlive) ExecuteSkillInternal(caster, skill, ally);
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

        private void ExecuteSkillInternal(Character caster, SkillData skill, Character target)
        {
            switch (skill.Type)
            {
                case SkillType.Attack:
                    ExecuteAttack(caster, target, skill);
                    break;
                case SkillType.Heal:
                    ExecuteHeal(target, skill);
                    break;
                case SkillType.Buff:
                    ApplyEffect(skill, target);
                    break;
                case SkillType.Debuff:
                    ApplyEffect(skill, target);
                    break;
                case SkillType.Shield:
                    ExecuteShield(target, skill);
                    break;
            }
        }

        private void ExecuteAttack(Character caster, Character target, SkillData skill)
        {
            DealDamage(caster, target, skill.Power);
        }

        private void ExecuteHeal(Character target, SkillData skill)
        {
            target.Health.Heal(skill.Power);
        }

        private void ApplyEffect(SkillData skill, Character target)
        {
            if (skill.StatusEffect != StatusEffectType.None)
            {
                target.StatusEffects.ApplyEffect(skill.StatusEffect, skill.EffectDuration, skill.EffectValue);
                target.ApplyStatModifiers();
            }
        }

        private void ExecuteShield(Character target, SkillData skill)
        {
            target.Health.AddShield(skill.Power);
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

            var target = alivePlayers[UnityEngine.Random.Range(0, alivePlayers.Count)];
            DealDamage(enemy, target);
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
        }

        private void CheckBattleEnd()
        {
            bool allPlayersDead = _playerParty.TrueForAll(p => p.IsDead);
            bool allEnemiesDead = _enemies.TrueForAll(e => e.IsDead);

            if (allPlayersDead || allEnemiesDead)
            {
                _context.SetPhase(TurnPhase.BattleEnd);
                OnBattleEnded?.Invoke();
            }
        }
    }
}
