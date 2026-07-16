using System;
using System.Collections.Generic;
using TeamLog.Skill;

namespace TeamLog.Characters
{
    /// <summary>
    /// Phase CC-2F: Mortis(Necromancer) 전용 — 시체(Summoned Corpse) 데이터 컨테이너.
    /// 시체는 별도 Character가 아닌 Necromancer에 종속된 데이터. HP/StatusEffects 없음 (적의 대상 안 됨).
    ///
    /// 핵심 메카닉:
    /// 1. 전투 시작 시 4개 기본 스킬로 초기화 (Scratch/Poison Bite/Bone Toss/Stun Strike)
    /// 2. 매 턴 플레이어 종료 후 무작위 슬롯 스킬 1개 자동 시전 (TurnManager.ProcessCorpseAction)
    /// 3. 적 처치 시 Discover Modal로 처치한 적의 스킬 4개 표시 → 1개 선택 → 시체 슬롯 교체
    /// 4. Necromancer 사망 시 시체도 자동 비활성화 (IsActive=false)
    /// 5. 전투 종료 시 리셋 — 다음 전투에서 기본 4스킬로 재소환
    ///
    /// 강화 상태:
    /// - EmpowerBonusNext: 다음 시체 스킬 위력 가산 (1회 후 소모)
    /// - MassEmpowerBonus: 시체 모든 스킬 영구 위력 가산
    /// - SoulLinkRemainingTurns: Soul Link 지속 턴 수 (시체 딜 → Necromancer 회복)
    /// </summary>
    public class CorpseComponent
    {
        public const int CORPSE_SLOT_COUNT = 4;

        private readonly Character _owner;
        private readonly SkillData[] _baseSkills; // 전투 시작 시 리셋용 기본 스킬
        private readonly SkillData[] _slots = new SkillData[CORPSE_SLOT_COUNT];
        private readonly System.Random _rng = new System.Random();

        /// <summary>시체 슬롯 4개 (현재 스킬 풀).</summary>
        public IReadOnlyList<SkillData> Slots => _slots;

        /// <summary>시체 활성화 여부 — Necromancer 생존 시 true. 사망 시 false.</summary>
        public bool IsActive { get; private set; }

        /// <summary>다음 시체 스킬 위력 가산 (1회 후 소모). Empower Undead용.</summary>
        public int EmpowerBonusNext { get; set; }

        /// <summary>시체 모든 스킬 영구 위력 가산. Mass Empower용.</summary>
        public int MassEmpowerBonus { get; private set; }

        /// <summary>Soul Link 남은 턴 수 (0=비활성). 매 턴 종료 시 -1.</summary>
        public int SoulLinkRemainingTurns { get; set; }

        /// <summary>Soul Link 회복 비율 (0.5=50%). 특성(생명력 흡수) 적용 시 0.75.</summary>
        public float SoulLinkMul { get; set; } = 0.5f;

        /// <summary>적 처치 시 시체 영구 강화 가산 (죽음의 수확 특성).</summary>
        public int KillEmpowerBonus { get; private set; }

        public Character Owner => _owner;

        public CorpseComponent(Character owner, SkillData[] baseSkills)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            if (baseSkills == null || baseSkills.Length != CORPSE_SLOT_COUNT)
                throw new ArgumentException($"시체 기본 스킬은 {CORPSE_SLOT_COUNT}개여야 함", nameof(baseSkills));
            _baseSkills = baseSkills;
            ResetToBaseSkills();
            IsActive = true;
        }

        /// <summary>기본 4스킬로 리셋 (전투 시작/재소환 시).</summary>
        public void ResetToBaseSkills()
        {
            for (int i = 0; i < CORPSE_SLOT_COUNT; i++)
                _slots[i] = _baseSkills[i];
            EmpowerBonusNext = 0;
            MassEmpowerBonus = 0;
            SoulLinkRemainingTurns = 0;
            KillEmpowerBonus = 0;
            IsActive = true;
        }

        /// <summary>시체 비활성화 — Necromancer 사망 시 호출.</summary>
        public void Deactivate()
        {
            IsActive = false;
            EmpowerBonusNext = 0;
            SoulLinkRemainingTurns = 0;
        }

        /// <summary>시체 슬롯 교체 — 적 처치 시 선택한 스킬로.</summary>
        public void ReplaceSlot(int slotIndex, SkillData newSkill)
        {
            if (slotIndex < 0 || slotIndex >= CORPSE_SLOT_COUNT) return;
            if (newSkill == null) return;
            _slots[slotIndex] = newSkill;
        }

        /// <summary>무작위 슬롯의 스킬 반환 (매 턴 자동 시전용).</summary>
        public SkillData GetRandomSkill()
        {
            int idx = _rng.Next(CORPSE_SLOT_COUNT);
            return _slots[idx];
        }

        /// <summary>무작위 슬롯 인덱스 + 스킬을 함께 반환 (UI 하이라이트용).</summary>
        public (int slotIndex, SkillData skill) GetRandomSkillWithIndex()
        {
            int idx = _rng.Next(CORPSE_SLOT_COUNT);
            return (idx, _slots[idx]);
        }

        /// <summary>시체 스킬의 최종 위력 — 본래 Power + EmpowerNext + MassEmpower + KillEmpower.</summary>
        public int GetEffectivePower(SkillData skill)
        {
            if (skill == null) return 0;
            int power = skill.Power;
            power += MassEmpowerBonus;
            power += KillEmpowerBonus;
            return power;
        }

        /// <summary>Empower Undead 스킬 효과 — 다음 시체 스킬 위력 가산.</summary>
        public void ApplyEmpowerNext(int bonus)
        {
            if (bonus > 0) EmpowerBonusNext += bonus;
        }

        /// <summary>Mass Empower 스킬 효과 — 모든 시체 스킬 영구 가산.</summary>
        public void ApplyMassEmpower(int bonus)
        {
            if (bonus > 0) MassEmpowerBonus += bonus;
        }

        /// <summary>적 처치 시 영구 강화 — 죽음의 수확 특성 발동 시 호출.</summary>
        public void ApplyKillEmpower(int bonus)
        {
            if (bonus > 0) KillEmpowerBonus += bonus;
        }

        /// <summary>Soul Link 회복 비율 getter — 특성(생명력 흡수)이 설정.</summary>
        public float GetSoulLinkMultiplier() => SoulLinkMul;

        /// <summary>매 턴 종료 후 SoulLink 턴 수 감소.</summary>
        public void TickSoulLink()
        {
            if (SoulLinkRemainingTurns > 0) SoulLinkRemainingTurns--;
        }

        /// <summary>시체가 소모한 EmpowerNext 반환 (시전 후 소모).</summary>
        public int ConsumeEmpowerNext()
        {
            int bonus = EmpowerBonusNext;
            EmpowerBonusNext = 0;
            return bonus;
        }
    }
}
