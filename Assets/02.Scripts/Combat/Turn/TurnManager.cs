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
        private readonly int _bonusFirstTurnAP;

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
            List<EnemyAIController> enemyControllers = null, int maxRerolls = 1, int bonusFirstTurnAP = 0)
        {
            _playerParty = playerParty;
            _enemies = enemies;
            _enemyControllers = enemyControllers ?? new List<EnemyAIController>();
            _context = new TurnContext();
            _drawSystem = new SkillDrawSystem(playerParty, maxRerolls);
            _bonusFirstTurnAP = bonusFirstTurnAP;

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
        /// 중앙화된 데미지 적용: 공격자 ATK + bonusPower - 대상 DEF + 특성 훅
        /// </summary>
        public static void DealDamage(Character attacker, Character target, int bonusPower = 0)
        {
            int damage = attacker.Stats.GetStat(StatType.ATK) + bonusPower;
            int defense = target.Stats.GetStat(StatType.DEF);
            int calculatedDamage = CalculateDamage(damage, defense);

            // 대상 특성: 들어오는 데미지 수정 (Sturdy 절반)
            calculatedDamage = target.TraitHandler.ModifyIncomingDamage(calculatedDamage);

            // 회피 시 MISS 처리
            if (calculatedDamage == 0)
            {
                OnAttackMissed?.Invoke(target);
                return;
            }

            target.Health.TakeDamage(calculatedDamage);

            // 공격자 특성: 피해를 입혔을 때 (Corrosive 방어감소)
            attacker.TraitHandler.OnDamageDealtTo(target);

            // 대상 특성: 피해를 받은 후 (Counter/Thorns/Rampage/ArcaneFury/Rally)
            target.TraitHandler.OnDamageReceived(attacker, calculatedDamage);

            // CombatEventBus: 유물 트리거
            CombatEventBus.FireDamageDealt(attacker, target, calculatedDamage);
            CombatEventBus.FireDamageReceived(target, calculatedDamage);

            // 사망 시 Kill 이벤트
            if (target.IsDead)
                CombatEventBus.FireKill(target);
        }

        /// <summary>
        /// 회피 발생 시 이벤트 (FloatingText "MISS" 표시용)
        /// </summary>
        public static event System.Action<Character> OnAttackMissed;

        /// <summary>
        /// 스킬 효과 적용 후 이벤트 — 스킬 타입별 사운드/VFX 분기용
        /// </summary>
        public static event System.Action<SkillData, Character> OnSkillApplied;

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

            // AP 리셋: 기본 1 + 생존 파티원 수 (+ 첫 턴 보너스)
            int aliveCount = _playerParty.FindAll(p => p.IsAlive).Count;
            int bonus = (_context.TurnNumber == 1) ? _bonusFirstTurnAP : 0;
            _context.ResetAP(1 + aliveCount + bonus);

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
        /// 스킬 즉시 시전 — 대상 클릭 시 곧바로 실행
        /// </summary>
        public bool ExecuteSkillImmediately(Character caster, SkillData skill, Character target)
        {
            return ExecuteSkillImmediately(caster, skill, target, 0);
        }

        /// <summary>
        /// 스킬 즉시 시전 — 업그레이드 보너스 포함
        /// </summary>
        public bool ExecuteSkillImmediately(Character caster, SkillData skill, Character target, int bonusPower)
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
                        ExecuteSkillInternal(caster, skill, target, bonusPower);
                    break;
                case TargetType.SingleEnemy:
                    if (target != null && target.IsAlive)
                        ExecuteSkillInternal(caster, skill, target, bonusPower);
                    break;
                case TargetType.AllEnemies:
                    foreach (var enemy in _enemies)
                        if (enemy.IsAlive) ExecuteSkillInternal(caster, skill, enemy, bonusPower);
                    break;
                case TargetType.AllAllies:
                    foreach (var ally in _playerParty)
                        if (ally.IsAlive) ExecuteSkillInternal(caster, skill, ally, bonusPower);
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

        private void ExecuteSkillInternal(Character caster, SkillData skill, Character target, int bonusPower = 0)
        {
            switch (skill.Type)
            {
                case SkillType.Attack:
                    ExecuteAttack(caster, target, skill, bonusPower);
                    break;
                case SkillType.Heal:
                    ExecuteHeal(target, skill, bonusPower);
                    break;
                case SkillType.Buff:
                    ApplyEffect(skill, target);
                    break;
                case SkillType.Debuff:
                    ApplyEffect(skill, target);
                    break;
                case SkillType.Shield:
                    ExecuteShield(target, skill, bonusPower);
                    break;
                case SkillType.Purify:
                    ExecutePurify(target);
                    break;
            }

            OnSkillApplied?.Invoke(skill, target);
            CombatEventBus.FireSkillUsed(skill, caster);
        }

        private void ExecuteAttack(Character caster, Character target, SkillData skill, int bonusPower = 0)
        {
            DealDamage(caster, target, skill.Power + bonusPower);
        }

        private void ExecuteHeal(Character target, SkillData skill, int bonusPower = 0)
        {
            target.Health.Heal(skill.Power + bonusPower);
        }

        private void ApplyEffect(SkillData skill, Character target)
        {
            if (skill.StatusEffect != StatusEffectType.None)
            {
                // Shell 특성: 매 턴 첫 상태이상 무효화
                if (target.TraitHandler.ShouldBlockEffect())
                    return;

                target.StatusEffects.ApplyEffect(skill.StatusEffect, skill.EffectDuration, skill.EffectValue);
                target.ApplyStatModifiers();
            }
        }

        private void ExecuteShield(Character target, SkillData skill, int bonusPower = 0)
        {
            target.Health.AddShield(skill.Power + bonusPower);
        }

        private void ExecutePurify(Character target)
        {
            target.StatusEffects.ClearAllEffects();
            target.ApplyStatModifiers();
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
                OnBattleEnded?.Invoke();
            }
        }
    }
}
