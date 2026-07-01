using System.Collections.Generic;

namespace TeamLog.Characters
{
    /// <summary>
    /// Prophecy 자원 — Sibyl (Oracle) 고유 메카닉 (Phase CC).
    ///
    /// 핵심: 스킬 사용 시 1턴 뒤 발동으로 "예약". 매 턴 시작 시 예약된 스킬 발동.
    /// CurrentStacks = 예약된 스킬 개수.
    /// </summary>
    public class ProphecyResourceComponent : CharacterResourceComponent
    {
        public override ResourceType Resource => ResourceType.Prophecy;
        public override int MaxStacks => 99;

        private struct PendingProphecy
        {
            public SkillData Skill;
            public SkillInstance Instance;
            public Character InitialTarget;
        }

        private readonly List<PendingProphecy> _pending = new();
        public int PendingCount => _pending.Count;

        /// <summary>스킬을 1턴 뒤 발동으로 예약.</summary>
        public void Reserve(SkillData skill, Character target, SkillInstance instance = null)
        {
            _pending.Add(new PendingProphecy { Skill = skill, Instance = instance, InitialTarget = target });
            CurrentStacks = _pending.Count;
        }

        /// <summary>예약된 스킬 목록을 소비(반환 후 클리어). TurnManager가 발동 시 호출.</summary>
        public List<(SkillData skill, SkillInstance instance, Character target)> ConsumePending()
        {
            var result = new List<(SkillData, SkillInstance, Character)>();
            foreach (var p in _pending)
                result.Add((p.Skill, p.Instance, p.InitialTarget));
            _pending.Clear();
            CurrentStacks = 0;
            return result;
        }

        public override void Reset()
        {
            base.Reset();
            _pending.Clear();
        }
    }
}
