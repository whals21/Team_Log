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
        Shadows,      // Umbra (Rogue) — 그림자 (파티 보호형 치명타 자원) — Phase CC-2A
        Combo,        // Aster (Archer) — 연속 사격 (매 턴 스킬 사용 시 +1) — Phase CC-2B
        Mercy,        // Elara (Healer) — 회복의 연결고리 (파티원별 회복량 추적) — Phase CC-2C
        Melody,       // Calliope (Bard) — 주 선율 + 부 선율 메아리 — Phase CC-2D
    }

    /// <summary>
    /// ★ Phase CC-2D: Bard 선율 종류 — 주 선율(CurrentMelody) / 부 선율(EchoMelody).
    /// 매 턴 시작 시 Current → Echo로 이동, Echo 자동 발동 (주 선율의 50%).
    /// 같은 선율 연속 시 부 선율 무효화 (매 턴 다른 스킬 유도).
    /// </summary>
    public enum MelodyType
    {
        None,         // 선율 없음 (초기/리셋)
        Healing,      // Mending Song — 힐
        Valor,        // Anthem of Valor — 파티 ATK+
        Dissonance,   // Dissonant Chord — 적 ATK-
        Inspiration,  // Inspiring Refrain — AP+ / 쉴드
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
    /// - ★ UI 갱신: OnStacksChanged(delta) 이벤트로 스택 변화 알림 (ResourceBadge 갱신 + 플로팅 텍스트).
    /// </summary>
    public abstract class CharacterResourceComponent
    {
        /// <summary>자원 종류.</summary>
        public abstract ResourceType Resource { get; }

        /// <summary>현재 스택.</summary>
        public int CurrentStacks { get; protected set; }

        /// <summary>최대 스택 (자원별 상이).</summary>
        public abstract int MaxStacks { get; }

        /// <summary>
        /// ★ Phase CC-2A: 자원 최대치 보너스 — 특성/유물 등 외부 요소로 MaxStacks 증가.
        /// 기본 0. CharacterTraitHandler 등에서 설정. "그림자 심화" 특성 = Shadows MaxStacksBonus 1.
        /// </summary>
        public int MaxStacksBonus { get; set; }

        /// <summary>실제 최대 스택 (기본 MaxStacks + 보너스).</summary>
        public int EffectiveMaxStacks => MaxStacks + MaxStacksBonus;

        /// <summary>
        /// ★ 위험 임계값 — 이 값 이상 시 UI 경고(빨강 글로우/깜빡임).
        /// 기본: EffectiveMaxStacks - 1 (자원별 override 가능). Ember 4/5, Vengeance 19/20 등.
        /// </summary>
        public virtual int WarningThreshold => System.Math.Max(1, EffectiveMaxStacks - 1);

        /// <summary>★ 스택 변화 이벤트 — 양수=획득, 음수=소모. UI 갱신/플로팅 텍스트용.</summary>
        public event System.Action<int> OnStacksChanged;

        /// <summary>
        /// ★ Phase CC-2C: Owner 캐릭터 참조 — Character 생성 시 설정.
        /// MercyResourceComponent 등이 OnTurnStart 없이도 Owner에 접근 가능.
        /// 기존 컴포넌트는 OnTurnStart(owner) 매개변수를 계속 사용 (회귀 0).
        /// </summary>
        public Character Owner { get; private set; }

        /// <summary>Character 생성자에서 호출 — Owner 설정.</summary>
        public void InitializeOwner(Character owner) => Owner = owner;

        /// <summary>현재 스택을 최대치로 제한.</summary>
        protected void ClampStacks()
        {
            if (CurrentStacks < 0) CurrentStacks = 0;
            if (CurrentStacks > EffectiveMaxStacks) CurrentStacks = EffectiveMaxStacks;
        }

        /// <summary>스택 추가 (Clamp 적용).</summary>
        public virtual void AddStacks(int amount)
        {
            if (amount == 0) return;
            CurrentStacks += amount;
            ClampStacks();
            OnStacksChanged?.Invoke(amount);
        }

        /// <summary>스택 소모 — 성공 시 true, 부족 시 false (소모 안 함).</summary>
        public virtual bool ConsumeStacks(int amount)
        {
            if (amount <= 0) return true;
            if (CurrentStacks < amount) return false;
            CurrentStacks -= amount;
            OnStacksChanged?.Invoke(-amount);
            return true;
        }

        /// <summary>소모 가능 여부만 체크 (실제 소모 안 함). CanUse 검사용.</summary>
        public bool CanConsume(int amount) => amount <= 0 || CurrentStacks >= amount;

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
