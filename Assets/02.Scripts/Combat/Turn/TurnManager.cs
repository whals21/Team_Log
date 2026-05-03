using System.Collections.Generic;
using TeamLog.Combat.Draw;

// 네임스페이스 충돌 해결
using Character = TeamLog.Characters.Character;
using CharacterData = TeamLog.Characters.CharacterData;
using SkillData = TeamLog.Characters.SkillData;
using SkillType = TeamLog.Characters.SkillType;
using StatType = TeamLog.Characters.StatType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;
using StatusEffect = TeamLog.Characters.StatusEffect;
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
        private readonly SkillDrawSystem _drawSystem;

        public TurnContext Context => _context;
        public SkillDrawSystem DrawSystem => _drawSystem;
        public TurnPhase CurrentPhase => _context.CurrentPhase;
        public int TurnNumber => _context.TurnNumber;

        public event System.Action<TurnPhase, TurnPhase> OnPhaseChanged;
        public event System.Action<int> OnTurnStarted;
        public event System.Action OnBattleEnded;

        public TurnManager(List<Character> playerParty, List<Character> enemies, int maxRerolls = 1)
        {
            _playerParty = playerParty;
            _enemies = enemies;
            _context = new TurnContext();
            _drawSystem = new SkillDrawSystem(playerParty, maxRerolls);

            _context.OnPhaseChanged += (old, newPhase) => OnPhaseChanged?.Invoke(old, newPhase);
            _context.OnTurnStarted += turn => OnTurnStarted?.Invoke(turn);
        }

        public void StartBattle()
        {
            StartNewTurn();
        }

        public void StartNewTurn()
        {
            _context.StartNewTurn();
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

        public bool RerollAll()
        {
            if (CurrentPhase != TurnPhase.PlayerAction) return false;
            return _drawSystem.RerollAll();
        }

        /// <summary>
        /// 스킬 즉시 시전 — 대상 클릭 시 곧바로 실행
        /// </summary>
        public bool ExecuteSkillImmediately(Character caster, SkillData skill, Character target)
        {
            if (caster.IsDead) return false;

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
            }
        }

        private void ExecuteAttack(Character caster, Character target, SkillData skill)
        {
            int damage = caster.Stats.GetStat(StatType.ATK) + skill.Power;
            int defense = target.Stats.GetStat(StatType.DEF);
            int finalDamage = System.Math.Max(1, damage - defense);
            target.Health.TakeDamage(finalDamage);
        }

        private void ExecuteHeal(Character target, SkillData skill)
        {
            target.Health.Heal(skill.Power);
        }

        private void ApplyEffect(SkillData skill, Character target)
        {
            if (skill.StatusEffect != global::TeamLog.Characters.StatusEffect.None)
            {
                var effectType = (StatusEffectType)(int)skill.StatusEffect;
                target.StatusEffects.ApplyEffect(effectType, skill.EffectDuration, skill.EffectValue);
            }
        }

        public void StartEnemyTurn()
        {
            _context.SetPhase(TurnPhase.EnemyTurn);
            ExecuteEnemyActions();
        }

        private void ExecuteEnemyActions()
        {
            foreach (var enemy in _enemies)
            {
                if (enemy.IsAlive)
                    ExecuteEnemyAction(enemy);
            }

            CheckBattleEnd();

            if (CurrentPhase != TurnPhase.BattleEnd)
                StartNewTurn();
        }

        private void ExecuteEnemyAction(Character enemy)
        {
            var alivePlayers = _playerParty.FindAll(p => p.IsAlive);
            if (alivePlayers.Count == 0) return;

            var target = alivePlayers[UnityEngine.Random.Range(0, alivePlayers.Count)];
            int damage = enemy.Stats.GetStat(StatType.ATK);
            int defense = target.Stats.GetStat(StatType.DEF);
            int finalDamage = System.Math.Max(1, damage - defense);
            target.Health.TakeDamage(finalDamage);
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
