# 스킬 조립식 아키텍처 설계안 (Skill Architecture Proposal)

> **작성일**: 2026-07-01
> **문서 위상**: Phase ARCH 설계 사양서 (구현 가이드)
> **배경**: `SkillConceptBacklog.md`의 21개 신규 컨셉 + 기존 24종 BehaviorKeyword = 45종이 되면 기존 하드코딩 방식(`SkillExecutor.ExecuteAttack` if문)이 한계. 조립식(composable) 파이프라인으로 아키텍처 전환.
> **관련 문서**: `CLAUDE.md` (아키텍처 규칙), `SkillConceptBacklog.md` (컨셉 목록), `DesignPillars.md` (설계 원칙)

---

## 0. 문서의 목적

1. **조립식 전환의 필요성 명확화** — 현재 구조의 한계와 개선 목표
2. **구체적인 아키텍처 사양 제공** — 인터페이스/클래스/파이프라인 구조
3. **전환 로드맵 수립** — Phase ARCH-1 ~ 5 단계별 작업 정의
4. **기존 시스템과의 호환성 보장** — 회귀 위험 최소화 전략

본 문서는 **구현 가이드**이며, 코드 리뷰와 단위 테스트의 기준으로 사용.

---

## 1. 배경 — 현재 구조의 진단

### 1.1 현재 구조 (Phase BK 기준)

```
SkillData (ScriptableObject)
├── SkillType, TargetType, Power, Cost, Weight
├── StatusEffect + Duration + Value
└── BehaviorTag[] _behaviors  ← ★ 데이터 계층은 완전 조립식
```

`SkillData`의 `_behaviors` 배열은 **인스펙터에서 드래그로 자유롭게 조립 가능**. `{BurningTouch(2), Bounce(3), Lifesteal(0)}` 같은 조합이 데이터 레벨에서 완벽히 작동. **데이터 계층은 이미 훌륭한 조립식**.

### 1.2 문제 — 로직 계층의 하드코딩

`SkillExecutor.ExecuteAttack()` (line 99~257)의 구조:

```csharp
// 현재: BehaviorKeyword마다 if문으로 하드코딩
private void ExecuteAttack(...) {
    if (Has(BehaviorKeyword.Berserk) && ...) basePower *= 2;
    if (Has(BehaviorKeyword.Pierce)) TakeDirectDamage(...);
    if (First(BehaviorKeyword.Execution)...) target 즉사;
    if (Has(BehaviorKeyword.Lifesteal)...) caster.Heal(...);
    int chainCount = RankSum(BehaviorKeyword.Chain); ...
    // 45종이 되면 약 400~800줄의 if문 늪 예상
}
```

**근본적 한계 4가지**:
1. **Open-Closed 원칙 위반** — 새 키워드 추가 시 기존 SkillExecutor 코드 수정 필수
2. **단위 테스트 어려움** — 개별 키워드 테스트가 SkillExecutor 통째로 필요
3. **코드 응집도 저하** — 한 키워드의 로직이 여러 줄에 흩어짐
4. **조합 폭발 시 디버깅 어려움** — Behavior 간 순서 의존성이 한 함수에 경직

---

## 2. 설계 목표

### 2.1 핵심 목표 (Success Criteria)

| 목표 | 측정 기준 |
|------|---------|
| **조립성 (Composability)** | 인스펙터에서 Behavior 배열 조합만으로 새 스킬이 코드 수정 없이 작동 |
| **개방-폐쇄 (Open-Closed)** | 새 BehaviorKeyword 추가 시 SkillExecutor 코드 수정 0줄 |
| **독립 테스트 가능성** | 각 Behavior가 별도 클래스로 독립 단위 테스트 가능 |
| **회귀 보장** | 기존 172개 테스트 모두 통과 유지 |
| **성능 유지** | 파이프라인 호출 오버헤드 < 0.1ms / 스킬 실행 |

### 2.2 비목표 (Non-Goals)

- **ECS 패턴 도입**: Unity DOTS/ECS가 아닌 순수 C# 인터페이스 기반
- **기존 SkillData 스키마 변경**: `BehaviorTag[] _behaviors` 그대로 유지
- **모든 Behavior를 당장 전환**: 점진적 마이그레이션 (기존 24종은 ARCH-2~3에서, 신규 21종은 ARCH-4에서)
- **유물/특성 체인 통합**: RelicHandler/CharacterTraitHandler는 별도 체인 유지

---

## 3. 아키텍처 개요

### 3.1 3대 컴포넌트

```
┌──────────────────────────────────────────────────────┐
│  ISkillBehavior 인터페이스                            │
│  └─ 각 BehaviorKeyword이 하나의 클래스로 캡슐화       │
│     (자신의 로직 + 개입 타이밍 Phase를 스스로 알림)    │
├──────────────────────────────────────────────────────┤
│  BehaviorRegistry (정적 매핑)                         │
│  └─ BehaviorKeyword enum → ISkillBehavior 인스턴스    │
│     (부패 시 자동 등록, 45개 키워드)                  │
├──────────────────────────────────────────────────────┤
│  SkillExecutionPipeline                               │
│  └─ 정해진 Phase 순서대로 모든 Behavior의 훅 호출      │
└──────────────────────────────────────────────────────┘
```

### 3.2 데이터/로직 흐름

```
[1] 스킬 사용 결정 (PlayerActionController)
      ↓
[2] SkillInstance.GetCombinedBehaviors() → BehaviorTag[] 수집
      ↓
[3] BehaviorTag[] → ISkillBehavior[] 조회 (BehaviorRegistry.Get)
      ↓
[4] SkillExecutionPipeline.Run(context) 호출
   ┌── Phase: PowerModify ─────────────────────────┐
   │  Berserk.ModifyPower(), HeavyHit.ModifyPower()│
   │  Desperation.ModifyPower(), ...               │
   └───────────────────────────────────────────────┘
   ┌── Phase: TargetModify ────────────────────────┐
   │  Spread.ModifyTargets(), Bounce.ModifyTargets()│
   │  Chain.ModifyTargets(), ...                   │
   └───────────────────────────────────────────────┘
   ┌── Phase: DamageApply ─────────────────────────┐
   │  Pierce.ApplyDamage() (특수), 일반은 기본 DealDamage│
   └───────────────────────────────────────────────┘
   ┌── Phase: PostDamage ──────────────────────────┐
   │  Lifesteal.OnPostDamage(), AllIn.OnPostDamage()│
   │  FollowUp.OnPostDamage(), ...                 │
   └───────────────────────────────────────────────┘
   ┌── Phase: OnKill (대상 사망 시만) ──────────────┐
   │  Reaper.OnKill(), Bounty.OnKill()             │
   └───────────────────────────────────────────────┘
      ↓
[5] OnSkillApplied 이벤트 → 사운드/VFX/UI
      ↓
[6] CombatEventBus.FireSkillUsed → 유물/특성 트리거
```

---

## 4. 상세 설계

### 4.1 ISkillBehavior 인터페이스

**파일**: `02.Scripts/Skill/Behaviors/ISkillBehavior.cs`
**네임스페이스**: `TeamLog.Skill.Behaviors`

```csharp
public interface ISkillBehavior
{
    /// <summary>이 Behavior가 나타내는 BehaviorKeyword.</summary>
    BehaviorKeyword Keyword { get; }

    /// <summary>개입할 Phase (복수 지정 가능, Flags enum).</summary>
    ExecutionPhase Phases { get; }

    /// <summary>Phase 내 세부 순서 (낮을수록 먼저, 기본 100).</summary>
    int Order => 100;

    // ── Phase별 훅 (기본 구현: 아무 것도 안 함) ──

    /// <summary>PowerModify: 위력 수정.</summary>
    int ModifyPower(int power, SkillExecContext ctx) => power;

    /// <summary>TargetModify: 타겟 리스트 변경.</summary>
    List<Character> ModifyTargets(List<Character> targets, SkillExecContext ctx) => targets;

    /// <summary>DamageApply: 데미지 적용 방식 (Pierce 등 특수 처리).</summary>
    void ApplyDamage(SkillExecContext ctx) { }

    /// <summary>PostDamage: 데미지 후처리.</summary>
    void OnPostDamage(SkillExecContext ctx) { }

    /// <summary>OnKill: 대상 사망 시.</summary>
    void OnKill(SkillExecContext ctx) { }
}
```

### 4.2 ExecutionPhase enum (Flags)

**파일**: `02.Scripts/Skill/Behaviors/ExecutionPhase.cs`

```csharp
[Flags]
public enum ExecutionPhase
{
    None          = 0,
    PowerModify   = 1 << 0,   // 위력 계산 (Berserk, HeavyHit, Desperation...)
    TargetModify  = 1 << 1,   // 타겟 결정 (Spread, Bounce, Chain, MultiHit, Distribute...)
    DamageApply   = 1 << 2,   // 데미지 적용 (Pierce 특수 처리)
    PostDamage    = 1 << 3,   // 후처리 (Lifesteal, AllIn, FollowUp, Touch 계열)
    OnKill        = 1 << 4,   // 킬 시 (Reaper, Bounty)
    TurnEnd       = 1 << 5,   // 턴 종료 (Lingering 등 지속 효과)
}
```

### 4.3 SkillExecContext (공유 상태)

**파일**: `02.Scripts/Skill/Behaviors/SkillExecContext.cs`

```csharp
public class SkillExecContext
{
    // 입력 (불변)
    public Character Caster;
    public Character InitialTarget;
    public SkillData Skill;
    public SkillInstance Instance;
    public TurnContext TurnCtx;
    public IReadOnlyList<Character> PlayerParty;
    public IReadOnlyList<Character> Enemies;

    // 진행 중 상태 (Behavior들이 갱신)
    public List<Character> CurrentTargets;
    public int CurrentPower;
    public bool BypassShield;        // Pierce가 true로 설정
    public bool SkipDefaultDamage;   // DamageApply에서 기본 DealDamage 스킵 (Pierce/Execution이 사용)

    // 결과 기록 (후속 Behavior가 참조)
    public int LastActualDamage;
    public List<Character> KilledTargets = new();
}
```

### 4.4 BehaviorRegistry

**파일**: `02.Scripts/Skill/Behaviors/BehaviorRegistry.cs`

```csharp
public static class BehaviorRegistry
{
    private static readonly Dictionary<BehaviorKeyword, ISkillBehavior> _map = new();
    private static bool _initialized = false;

    public static void Initialize()
    {
        if (_initialized) return;
        Register(new BerserkBehavior());
        Register(new PierceBehavior());
        Register(new ExecutionBehavior());
        Register(new LifestealBehavior());
        Register(new ChainBehavior());
        // ... 추후 40종 더 (점진적 마이그레이션)
        _initialized = true;
    }

    public static void Register(ISkillBehavior behavior)
    {
        if (behavior == null) return;
        _map[behavior.Keyword] = behavior;
    }

    public static ISkillBehavior Get(BehaviorKeyword keyword)
    {
        Initialize();
        return _map.TryGetValue(keyword, out var b) ? b : null;
    }

    /// <summary>주어진 태그 목록에서 특정 Phase에 해당하는 Behavior들을 Order순으로 반환.</summary>
    public static List<ISkillBehavior> GetForPhase(IReadOnlyList<BehaviorTag> tags, ExecutionPhase phase)
    {
        Initialize();
        var result = new List<ISkillBehavior>();
        if (tags == null) return result;
        foreach (var tag in tags)
        {
            if (_map.TryGetValue(tag.Keyword, out var b) && (b.Phases & phase) != 0)
                result.Add(b);
        }
        result.Sort((a, b) => a.Order.CompareTo(b.Order));
        return result;
    }
}
```

### 4.5 SkillExecutionPipeline

**파일**: `02.Scripts/Combat/Turn/SkillExecutionPipeline.cs`
**네임스페이스**: `TeamLog.Combat.Turn`

```csharp
public class SkillExecutionPipeline
{
    private readonly List<Character> _playerParty;
    private readonly List<Character> _enemies;

    public SkillExecutionPipeline(List<Character> playerParty, List<Character> enemies) { ... }

    /// <summary>단일 대상 스킬 실행 (병행 구조에서 호출). 아직 사용 안 함 — 플래그로 전환.</summary>
    public void ExecuteSkill(Character caster, SkillData skill, Character target,
        SkillInstance instance, float powerMultiplier = 1f)
    {
        var ctx = new SkillExecContext { ... };
        var tags = instance?.GetCombinedBehaviors() ?? skill.Behaviors;

        // Phase 1: PowerModify
        ctx.CurrentPower = ComputeBasePower(caster, skill, instance, powerMultiplier);
        foreach (var b in BehaviorRegistry.GetForPhase(tags, ExecutionPhase.PowerModify))
            ctx.CurrentPower = b.ModifyPower(ctx.CurrentPower, ctx);

        // Phase 2: TargetModify
        ctx.CurrentTargets = new List<Character> { target };
        foreach (var b in BehaviorRegistry.GetForPhase(tags, ExecutionPhase.TargetModify))
            ctx.CurrentTargets = b.ModifyTargets(ctx.CurrentTargets, ctx);

        // 각 타겟에 대해 데미지 적용
        foreach (var t in ctx.CurrentTargets)
        {
            ctx.InitialTarget = t;
            // Phase 3: DamageApply
            bool handledByBehavior = false;
            foreach (var b in BehaviorRegistry.GetForPhase(tags, ExecutionPhase.DamageApply))
            {
                b.ApplyDamage(ctx);
                handledByBehavior = true;
            }
            if (!handledByBehavior)
                DefaultApplyDamage(ctx);

            // Phase 4: PostDamage
            foreach (var b in BehaviorRegistry.GetForPhase(tags, ExecutionPhase.PostDamage))
                b.OnPostDamage(ctx);

            // Phase 5: OnKill
            if (t.IsDead)
            {
                ctx.KilledTargets.Add(t);
                foreach (var b in BehaviorRegistry.GetForPhase(tags, ExecutionPhase.OnKill))
                    b.OnKill(ctx);
            }
        }
    }
}
```

---

## 5. Phase ARCH 전환 로드맵

### Phase ARCH-1 (기반) — 인터페이스 + Registry + Pipeline 뼈대
**목표**: 새 아키텍처 뼈대 구축, 기존 코드는 그대로 유지 (병행 구조)
**산출물**:
- `02.Scripts/Skill/Behaviors/ISkillBehavior.cs`
- `02.Scripts/Skill/Behaviors/ExecutionPhase.cs`
- `02.Scripts/Skill/Behaviors/SkillExecContext.cs`
- `02.Scripts/Skill/Behaviors/BehaviorRegistry.cs`
- `02.Scripts/Combat/Turn/SkillExecutionPipeline.cs`

**검증**: 컴파일 0에러 + 기존 172 테스트 통과 (새 코드는 아직 호출 안 됨)

### Phase ARCH-2 (핵심 5종 마이그레이션)
**목표**: 기존 24종 중 가장 복잡한 5종을 Behavior 클래스로 추출
**대상**: Berserk, Pierce, Execution, Lifesteal, Chain
**산출물**:
- `02.Scripts/Skill/Behaviors/Implementations/BerserkBehavior.cs`
- `02.Scripts/Skill/Behaviors/Implementations/PierceBehavior.cs`
- `02.Scripts/Skill/Behaviors/Implementations/ExecutionBehavior.cs`
- `02.Scripts/Skill/Behaviors/Implementations/LifestealBehavior.cs`
- `02.Scripts/Skill/Behaviors/Implementations/ChainBehavior.cs`
- `02.Scripts/Tests/BehaviorPipelineTests.cs` (새 단위 테스트)

**검증**:
- 5종 Behavior 독립 단위 테스트 통과
- 기존 `BehaviorSkillExecutionTests` 18개 회귀 없음
- 기존 172 테스트 모두 통과

**병행 전략**: 이 Phase에서는 SkillExecutor.ExecuteAttack의 if문을 **제거하지 않음**. 새 Behavior 클래스는 작성만 하고 호출은 안 됨. 다음 Phase에서 플래그로 전환.

### Phase ARCH-3 (나머지 19종 마이그레이션)
**목표**: 기존 24종 중 남은 19종을 모두 Behavior로 추출
**대상**: HeavyHit, BloodPact, GlassCannon, PowerUp, Spread, Bounce, MultiHit, Explosion, AOEAuto, Reaper, CostDown, QuickDraw, Intensify, Lingering, VenomTouch, BurningTouch, FreezeTouch, ShieldBonus, HealBonus
**산출물**: 19개 Behavior 클래스 + 추가 단위 테스트
**검증**: 이 시점에서 SkillExecutor.ExecuteAttack을 Pipeline 호출로 교체. 기존 테스트 전체 통과 시 전환 완료.

### Phase ARCH-4 (신규 21종 후보 일괄 추가)
**목표**: `SkillConceptBacklog.md`의 컨셉 5~21을 모두 Behavior로 구현
**대상**: FollowUp, FirstBlood, Cull, Fatigue, Momentum, Echo, Desperation, Wound, Escalation, Mastery, GiantSlayer, AllIn, Dominance, Bulwark, LimitBreak, Flank, Bounty, Distribute, TargetHighestHP, MultiStrike, TargetFullHP
**산출물**: 21개 Behavior 클래스 + 단위 테스트 21개
**검증**: 각 Behavior 단위 테스트 + 기존 테스트 전체 통과

### Phase ARCH-5 (Cost/Weight 파이프라인 통합)
**목표**: SkillInstance.EffectiveCost/EffectiveWeight 계산도 파이프라인으로 통합
**대상**: CostDown, Escalation, Mastery, QuickDraw (CostModify Phase)
**산출물**: CostModify Phase 추가, 관련 Behavior의 `ModifyCost()` 훅 구현
**검증**: EffectiveCost가 기존과 동일한 결과 반환

---

## 6. 기존 시스템과의 호환성 매트릭스

| 기존 컴포넌트 | ARCH-1 | ARCH-2 | ARCH-3 | ARCH-4 | ARCH-5 |
|-------------|--------|--------|--------|--------|--------|
| `SkillData` (SO) | 변경 없음 | 변경 없음 | 변경 없음 | 변경 없음 | 변경 없음 |
| `BehaviorTag` struct | 변경 없음 | 변경 없음 | 변경 없음 | 변경 없음 | 변경 없음 |
| `SkillInstance` | 변경 없음 | 변경 없음 | 변경 없음 | 변경 없음 | EffectiveCost/Weight가 파이프라인 사용 |
| `BehaviorKeyword` enum | 변경 없음 | 변경 없음 | 변경 없음 | 21종 추가 | 변경 없음 |
| `SkillExecutor.ExecuteAttack` | 변경 없음 | 변경 없음 | **파이프라인 호출로 교체** | 변경 없음 | 변경 없음 |
| `CombatEventBus` 훅 | 유지 | 유지 | 유지 | 유지 | 유지 |
| `DamageCalculator.DealDamage` | 유지 | 유지 | 유지 (Pipeline이 호출) | 유지 | 유지 |
| `RelicHandler`/`CharacterTraitHandler` | 유지 | 유지 | 유지 | 유지 | 유지 |

---

## 7. 위험과 완화 전략

| 위험 | 확률 | 영향 | 완화 |
|------|------|------|------|
| Phase ARCH-3 전환 시 기존 회귀 | 중 | 높음 | 5종만 먼저(ARCH-2) 검증 후 나머지 진행. 기존 172 테스트가 안전망 |
| Behavior 간 순서 의존성 | 중 | 중 | `Order` 프로퍼티로 세부 순서 지정. Phase 간 순서는 절대 (PowerModify → TargetModify → ...) |
| Behavior 조합 시 예외 (Pierce+Execution 동시) | 저 | 중 | `SkillInstance.ValidateBehaviors()`로 불가 조합 감지 (향후 추가) |
| SkillExecContext 거대화 (God Object) | 저 | 중 | Phase별 sub-context 분리 검토 (Phase PowerContext/TargetContext) |
| 인터페이스 호출 오버헥 | 저 | 저 | 매 턴 수십 회라 무시 가능. 프로파일러로 확인 |
| 기존 SkillExecutor와 이중 유지보수 | 중 | 중 | ARCH-2에서 플래그 도입, ARCH-3에서 완전 전환 후 기존 코드 제거 |

---

## 8. 검증 계획

### 8.1 단위 테스트 (Phase ARCH-2 기준)

`02.Scripts/Tests/BehaviorPipelineTests.cs`에 추가:

- `Berserk_HPBelowHalf_DoublesPower` — HP 50% 이하 시 위력 × 2
- `Berserk_HPAboveHalf_NoEffect` — HP 50% 초과 시 위력 그대로
- `Pierce_BypassesShield` — 쉴드 우회 데미지
- `Pierce_IgnoresDEF` — DEF 0 처리
- `Execution_KillsLowHPNonBoss` — HP 임계값 이하 일반 적 즉사
- `Execution_SkipsBoss` — 보스는 즉사 면역
- `Lifesteal_HealsHalfDamage` — 준 데미지 절반 회복
- `Lifesteal_ZeroDamage_NoHeal` — 0 데미지 시 회복 없음
- `Chain_HitsRandomNTargets` — 무작위 N명 연쇄
- `Chain_NoOtherEnemies_NoChain` — 단일 적 시 연쇄 없음
- `Pipeline_CombinesMultipleBehaviors` — 복수 Behavior 조합 시 순서대로 작동
- `Pipeline_NoBehaviors_DefaultDamage` — Behavior 0개 시 기본 DealDamage

### 8.2 회귀 테스트

기존 `BehaviorSkillExecutionTests` 18개 + `BehaviorKeywordTests` 12개 모두 통과 유지.

### 8.3 성능 검증

- 프로파일러로 스킬 실행 1회당 시간 측정
- 기존 대비 +0.1ms 이내 유지 목표

---

## 9. 구현 우선순위 (이번 세션 범위)

**이번 세션**: Phase ARCH-1 + ARCH-2 완료 (5종 Behavior 추출)

1. ✅ 설계안 문서화 (본 파일)
2. ✅ 폴더 구조 파악
3. ⏳ ARCH-1: 인터페이스/Registry/Pipeline 뼈대
4. ⏳ ARCH-2: 5종 Behavior 구현체
5. ⏳ ARCH-2 검증: 단위 테스트
6. ⏳ 컴파일/테스트 검증
7. ⏳ CLAUDE.md / MEMORY.md / 작업일지 업데이트

**다음 세션**: Phase ARCH-3 (나머지 19종) + ARCH-4 (신규 21종)

---

## 10. 개정 이력

| 날짜 | 변경 | 비고 |
|------|------|------|
| 2026-07-01 | 최초 작성 | Phase ARCH-1~5 전환 로드맵 수립. Phase CC-1 착수 전 안정기에 진행 권장이었으나, 사용자 결정으로 즉시 착수. 안전을 위해 병행 구조로 시작 |

---
