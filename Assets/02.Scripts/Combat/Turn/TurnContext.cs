using System.Collections.Generic;

// 네임스페이스 충돌 해결
using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;

namespace TeamLog.Combat.Turn
{
    /// <summary>
    /// 현재 턴의 컨텍스트 정보
    /// </summary>
    public class TurnContext
    {
        public int TurnNumber { get; private set; }
        public TurnPhase CurrentPhase { get; private set; }

        // 이번 턴에 드로우된 스킬들
        private readonly List<DrawnSkill> _drawnSkills = new();
        public IReadOnlyList<DrawnSkill> DrawnSkills => _drawnSkills;

        // 실행 대기열
        private readonly List<SkillAction> _actionQueue = new();
        public IReadOnlyList<SkillAction> ActionQueue => _actionQueue;

        public event System.Action<TurnPhase, TurnPhase> OnPhaseChanged;
        public event System.Action<int> OnTurnStarted;
        public event System.Action<int, int> OnAPChanged;

        private int _currentAP;
        private int _maxAP;
        public int CurrentAP => _currentAP;
        public int MaxAP => _maxAP;

        public TurnContext()
        {
            TurnNumber = 0;
            CurrentPhase = TurnPhase.None;
        }

        public void StartNewTurn()
        {
            TurnNumber++;
            _drawnSkills.Clear();
            _actionQueue.Clear();
            OnTurnStarted?.Invoke(TurnNumber);
        }

        public void SetPhase(TurnPhase newPhase)
        {
            var oldPhase = CurrentPhase;
            CurrentPhase = newPhase;
            OnPhaseChanged?.Invoke(oldPhase, newPhase);
        }

        public void AddDrawnSkill(Character caster, SkillData skill)
        {
            _drawnSkills.Add(new DrawnSkill(caster, skill));
        }

        public void AddAction(SkillAction action)
        {
            _actionQueue.Add(action);
        }

        public void ClearActions()
        {
            _actionQueue.Clear();
        }

        public void RemoveAction(SkillAction action)
        {
            _actionQueue.Remove(action);
        }

        public void ResetAP(int maxAP)
        {
            _maxAP = maxAP;
            _currentAP = maxAP;
            OnAPChanged?.Invoke(_currentAP, _maxAP);
        }

        public bool CanAfford(int cost)
        {
            return _currentAP >= cost;
        }

        public void SpendAP(int cost)
        {
            _currentAP -= cost;
            OnAPChanged?.Invoke(_currentAP, _maxAP);
        }
    }

    /// <summary>
    /// 드로우된 스킬 정보
    /// </summary>
    public class DrawnSkill
    {
        public Character Caster { get; }
        public SkillData Skill { get; }

        public DrawnSkill(Character caster, SkillData skill)
        {
            Caster = caster;
            Skill = skill;
        }
    }

    /// <summary>
    /// 실행할 스킬 액션
    /// </summary>
    public class SkillAction
    {
        public Character Caster { get; }
        public SkillData Skill { get; }
        public Character Target { get; set; }
        public int Priority { get; set; }

        public SkillAction(Character caster, SkillData skill, Character target, int priority = 0)
        {
            Caster = caster;
            Skill = skill;
            Target = target;
            Priority = priority;
        }
    }
}
