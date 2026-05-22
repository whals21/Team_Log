using System.Collections.Generic;
using TeamLog.Combat.Turn;

// 네임스페이스 충돌 해결
using Character = TeamLog.Characters.Character;
using StatusEffectType = TeamLog.Characters.StatusEffectType;

namespace TeamLog.Combat.AI
{
    /// <summary>
    /// 적 AI 컨트롤러
    /// </summary>
    public class EnemyAIController
    {
        private readonly Character _owner;
        private readonly EnemyActionPattern _pattern;
        private readonly List<Character> _players;

        private EnemyIntent _currentIntent;
        private EnemyActionNode _nextAction;

        public Character Owner => _owner;
        public EnemyIntent CurrentIntent => _currentIntent;

        public event System.Action<EnemyIntent> OnIntentChanged;

        public EnemyAIController(Character owner, EnemyActionPattern pattern, List<Character> players)
        {
            _owner = owner;
            _pattern = pattern;
            _players = players;
        }

        /// <summary>
        /// 턴 시작 시 다음 행동 결정
        /// </summary>
        public void PrepareNextAction()
        {
            _nextAction = _pattern.GetNextAction();
            _currentIntent = CreateIntent(_nextAction);
            OnIntentChanged?.Invoke(_currentIntent);
        }

        /// <summary>
        /// 준비된 행동 실행
        /// </summary>
        public void ExecuteAction()
        {
            if (_nextAction == null) return;

            var targets = SelectTargets(_nextAction.TargetCount);
            ExecuteActionOnTargets(_nextAction, targets);
        }

        private EnemyIntent CreateIntent(EnemyActionNode action)
        {
            if (action == null) return new EnemyIntent(EnemyIntentType.None);

            return action.ActionType switch
            {
                EnemyActionType.Attack => new EnemyIntent(EnemyIntentType.Attack, action.Value),
                EnemyActionType.HeavyAttack => new EnemyIntent(EnemyIntentType.HeavyAttack, action.Value),
                EnemyActionType.MultiAttack => new EnemyIntent(EnemyIntentType.MultiAttack, action.Value, action.TargetCount),
                EnemyActionType.Defend => new EnemyIntent(EnemyIntentType.Defend),
                EnemyActionType.Buff => new EnemyIntent(EnemyIntentType.Buff, description: "강화"),
                EnemyActionType.Debuff => new EnemyIntent(EnemyIntentType.Debuff, description: "약화"),
                EnemyActionType.Heal => new EnemyIntent(EnemyIntentType.Heal, action.Value),
                EnemyActionType.Summon => new EnemyIntent(EnemyIntentType.Summon),
                EnemyActionType.Charge => new EnemyIntent(EnemyIntentType.Charge),
                _ => new EnemyIntent(EnemyIntentType.Unknown)
            };
        }

        private List<Character> SelectTargets(int count)
        {
            var alivePlayers = _players.FindAll(p => p.IsAlive);
            var targets = new List<Character>();

            // 랜덤 타겟팅
            for (int i = 0; i < count && alivePlayers.Count > 0; i++)
            {
                int index = UnityEngine.Random.Range(0, alivePlayers.Count);
                targets.Add(alivePlayers[index]);
            }

            return targets;
        }

        private void ExecuteActionOnTargets(EnemyActionNode action, List<Character> targets)
        {
            switch (action.ActionType)
            {
                case EnemyActionType.Attack:
                case EnemyActionType.HeavyAttack:
                    foreach (var target in targets)
                        AttackTarget(target, action.Value);
                    break;

                case EnemyActionType.MultiAttack:
                    foreach (var target in targets)
                        AttackTarget(target, action.Value);
                    break;

                case EnemyActionType.Defend:
                    ApplyDefenseBuff(action.Value);
                    break;

                case EnemyActionType.Buff:
                    ApplySelfBuff(action.ApplyEffect, action.EffectDuration, action.Value);
                    break;

                case EnemyActionType.Debuff:
                    foreach (var target in targets)
                        ApplyDebuff(target, action.ApplyEffect, action.EffectDuration, action.Value);
                    break;

                case EnemyActionType.Heal:
                    _owner.Health.Heal(action.Value);
                    break;

                case EnemyActionType.Charge:
                    // 다음 공격 강화
                    ApplySelfBuff(StatusEffectType.AttackUp, 1, action.Value);
                    break;
            }
        }

        private void AttackTarget(Character target, int damage)
        {
            TurnManager.DealDamage(_owner, target, damage);
        }

        private void ApplyDefenseBuff(int value)
        {
            _owner.StatusEffects.ApplyEffect(StatusEffectType.DefenseUp, 1, value);
            _owner.ApplyStatModifiers();
        }

        private void ApplySelfBuff(StatusEffectType? effect, int duration, int value)
        {
            if (effect.HasValue && effect.Value != StatusEffectType.None)
            {
                _owner.StatusEffects.ApplyEffect(effect.Value, duration, value);
                _owner.ApplyStatModifiers();
            }
        }

        private void ApplyDebuff(Character target, StatusEffectType? effect, int duration, int value)
        {
            if (effect.HasValue && effect.Value != StatusEffectType.None)
            {
                target.StatusEffects.ApplyEffect(effect.Value, duration, value);
                target.ApplyStatModifiers();
            }
        }
    }
}
