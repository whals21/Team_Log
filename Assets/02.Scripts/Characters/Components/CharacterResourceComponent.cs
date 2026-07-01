namespace TeamLog.Characters
{
    /// <summary>
    /// 캐릭터 고유 자원 종류 (Phase CC).
    /// 각 캐릭터는 하나의 자원을 가질 수 있으며, SkillData.ResourceType과 매칭.
    /// </summary>
    public enum ResourceType
    {
        None,         // 자원 없는 캐릭터 (기존 캐릭터)
        Ember,        // Ashe (Pyromancer) — 자해 폭딜 자원
        Vengeance,    // Duran (Warrior) — 복수 게이지
        Frost,        // Lumi (Cryomancer) — 냉기 축적
        Prophecy,     // Sibyl (Oracle) — 예언(지연 발동)
        Charge,       // Taranis (Stormcaller) — 전하 네트워크
    }

    /// <summary>
    /// 캐릭터 고유 자원 관리 컴포넌트 기반 (Phase CC).
    /// 각 캐릭터(Ashe/Duran/Lumi/Sibyl/Taranis)는 이 클래스를 상속한 전용 컴포넌트를 가짐.
    /// Character.Resource로 접근. null이면 자원 없는 캐릭터.
    ///
    /// 설계:
    /// - 자원은 "스택" 기반 (CurrentStacks). 자원 종류별로 최대 스택 다름.
    /// - 매 턴 시작/종료 시 TurnManager가 OnTurnStart/OnTurnEnd 호출 → 자원 특유 효과 처리.
    /// - 스킬 사용 시 SkillData.ResourceGain/Cost로 스택 증감.
    /// </summary>
    public abstract class CharacterResourceComponent
    {
        /// <summary>자원 종류.</summary>
        public abstract ResourceType Resource { get; }

        /// <summary>현재 스택.</summary>
        public int CurrentStacks { get; protected set; }

        /// <summary>최대 스택 (자원별 상이).</summary>
        public abstract int MaxStacks { get; }

        /// <summary>현재 스택을 최대치로 제한.</summary>
        protected void ClampStacks()
        {
            if (CurrentStacks < 0) CurrentStacks = 0;
            if (CurrentStacks > MaxStacks) CurrentStacks = MaxStacks;
        }

        /// <summary>스택 추가 (Clamp 적용).</summary>
        public virtual void AddStacks(int amount)
        {
            CurrentStacks += amount;
            ClampStacks();
        }

        /// <summary>스택 소모 — 성공 시 true, 부족 시 false (소모 안 함).</summary>
        public virtual bool ConsumeStacks(int amount)
        {
            if (amount <= 0) return true;
            if (CurrentStacks < amount) return false;
            CurrentStacks -= amount;
            return true;
        }

        /// <summary>스택 리셋 (전투 시작 시).</summary>
        public virtual void Reset()
        {
            CurrentStacks = 0;
        }

        /// <summary>매 턴 시작 시 호출 (자원 자연 증가 등). 기본: 아무 것도 안 함.</summary>
        public virtual void OnTurnStart(Character owner) { }

        /// <summary>매 턴 종료 시 호출 (자원 효과 발동/소실 등). 기본: 아무 것도 안 함.</summary>
        public virtual void OnTurnEnd(Character owner) { }

        /// <summary>피격 시 호출 (Vengeance 축적 등). 기본: 아무 것도 안 함.</summary>
        public virtual void OnDamageTaken(Character owner, int damage) { }

        public override string ToString() => $"{Resource}({CurrentStacks}/{MaxStacks})";
    }
}
