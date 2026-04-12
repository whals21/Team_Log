using System.Collections.Generic;

// 네임스페이스 충돌 해결
using StatusEffectType = TeamLog.Characters.StatusEffectType;

namespace TeamLog.Combat.AI
{
    /// <summary>
    /// 적 행동 패턴 정의
    /// </summary>
    public class EnemyActionPattern
    {
        private readonly List<EnemyActionNode> _nodes;
        private int _currentIndex;

        public IReadOnlyList<EnemyActionNode> Nodes => _nodes;

        public EnemyActionPattern(IEnumerable<EnemyActionNode> nodes)
        {
            _nodes = new List<EnemyActionNode>(nodes);
            _currentIndex = 0;
        }

        public EnemyActionNode GetNextAction()
        {
            if (_nodes.Count == 0) return null;

            var action = _nodes[_currentIndex];
            _currentIndex = (_currentIndex + 1) % _nodes.Count;
            return action;
        }

        public EnemyActionNode PeekNextAction()
        {
            if (_nodes.Count == 0) return null;
            return _nodes[_currentIndex];
        }

        public void Reset()
        {
            _currentIndex = 0;
        }

        public static EnemyActionPattern CreateSimpleLoop(params EnemyActionNode[] nodes)
        {
            return new EnemyActionPattern(nodes);
        }
    }

    /// <summary>
    /// 개별 행동 노드
    /// </summary>
    public class EnemyActionNode
    {
        public EnemyActionType ActionType { get; }
        public int Value { get; }
        public int TargetCount { get; }
        public StatusEffectType? ApplyEffect { get; }
        public int EffectDuration { get; }

        public EnemyActionNode(EnemyActionType actionType, int value = 0, int targetCount = 1,
            StatusEffectType? effect = null, int effectDuration = 0)
        {
            ActionType = actionType;
            Value = value;
            TargetCount = targetCount;
            ApplyEffect = effect;
            EffectDuration = effectDuration;
        }
    }

    /// <summary>
    /// 적 행동 타입
    /// </summary>
    public enum EnemyActionType
    {
        Attack,
        HeavyAttack,
        MultiAttack,
        Defend,
        Buff,
        Debuff,
        Heal,
        Summon,
        Charge
    }
}
