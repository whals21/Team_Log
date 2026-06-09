using System.Collections.Generic;
using TeamLog.Map;

// 네임스페이스 충돌 해결
using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;
using SkillInstance = TeamLog.Characters.SkillInstance;

namespace TeamLog.Combat.Draw
{
    /// <summary>
    /// 전투 중 스킬 드로우 시스템
    /// 각 캐릭터에서 드로우된 스킬들을 통합 관리
    /// </summary>
    public class SkillDrawSystem
    {
        private readonly List<Character> _party;
        private readonly List<DrawnSkillSlot> _drawnSlots = new();

        private int _rerollCount;
        private int _maxRerolls = 1;

        public IReadOnlyList<DrawnSkillSlot> DrawnSlots => _drawnSlots;
        public int RerollsRemaining => _maxRerolls - _rerollCount;
        public int MaxRerolls => _maxRerolls;
        public bool CanReroll => _rerollCount < _maxRerolls;

        public event System.Action<IReadOnlyList<DrawnSkillSlot>> OnDrawComplete;
        public event System.Action OnSlotRerolled;

        public SkillDrawSystem(List<Character> party, int maxRerolls = 1)
        {
            _party = party;
            _maxRerolls = maxRerolls;
        }

        /// <summary>
        /// 새 턴 드로우 실행
        /// </summary>
        public void ExecuteDraw()
        {
            _drawnSlots.Clear();
            _rerollCount = 0;

            int bonusWeight = GameRunState.Instance?.RelicHandler.GetDrawWeightBonus() ?? 0;

            foreach (var character in _party)
            {
                if (character.IsAlive)
                {
                    var instance = character.SkillInventory.DrawSkillInstance(bonusWeight);
                    if (instance != null)
                    {
                        var slot = new DrawnSkillSlot(character, instance, _drawnSlots.Count);
                        _drawnSlots.Add(slot);
                    }
                }
            }

            OnDrawComplete?.Invoke(_drawnSlots);
        }

        /// <summary>
        /// 특정 슬롯 리롤
        /// </summary>
        public bool RerollSlot(int slotIndex)
        {
            if (!CanReroll || slotIndex < 0 || slotIndex >= _drawnSlots.Count)
                return false;

            var slot = _drawnSlots[slotIndex];
            var newInstance = slot.Caster.SkillInventory.DrawSkillInstance();

            if (newInstance != null)
            {
                slot.SetInstance(newInstance);
                _rerollCount++;
                OnSlotRerolled?.Invoke();
                return true;
            }

            return false;
        }

        public DrawnSkillSlot GetSlot(int index)
        {
            if (index >= 0 && index < _drawnSlots.Count)
                return _drawnSlots[index];
            return null;
        }
    }

    /// <summary>
    /// 드로우된 스킬 슬롯 — SkillInstance 기반
    /// </summary>
    public class DrawnSkillSlot
    {
        public Character Caster { get; }
        public SkillInstance Instance { get; private set; }
        public int SlotIndex { get; }
        public bool IsSelected { get; set; }
        public int ExecutionOrder { get; set; } = -1;
        public Character AssignedTarget { get; set; }
        public bool IsAssigned => AssignedTarget != null;

        /// <summary>하위 호환: SkillData</summary>
        public SkillData Skill => Instance?.Data;

        public DrawnSkillSlot(Character caster, SkillInstance instance, int slotIndex)
        {
            Caster = caster;
            Instance = instance;
            SlotIndex = slotIndex;
        }

        /// <summary>SkillInstance 직접 설정</summary>
        public void SetInstance(SkillInstance newInstance)
        {
            Instance = newInstance;
        }

        public void Reset()
        {
            IsSelected = false;
            ExecutionOrder = -1;
            AssignedTarget = null;
        }
    }
}
