# Phase CC-2G 아카이브 — 기존 8종 스킬 리워크 완료 (2026-07-17)

> **작업 기간**: 2026-07-17 단일 세션
> **범위**: Ashe/Duran/Lumi/Taranis/Sibyl/Umbra/Aster 7캐릭터 + Elara 생략
> **목적**: 이미 구축된 BehaviorTag 시스템(24종+)의 "조립식 시너지"를 기존 스킬에 부여하여, 각 캐릭터 컨셉을 강화하고 Pipeline 통합 혜택 제공

---

## 1. 작업 요약

| 항목 | 수치 |
|------|------|
| 커릭터 리워크 | 7개 (Ashe/Duran/Lumi/Taranis/Sibyl/Umbra/Aster) |
| 신규 Behavior 구현체 | **5종** (TargetFullHP, Explosion, FollowUp, Echo, LimitBreak) |
| 신규 인프라 | Character.HitThisTurn + 매 턴 리셋 + HealthComponent 훅 |
| 수정된 스크립트 | 8개 파일 |
| 신규 테스트 | 24개 (CC-2G-1: 11 + CC-2G-2~7: 13) |
| 전체 테스트 | **297/297 통과** (기존 273 + 24 신규, 회귀 0) |
| 컴파일 | **0 에러 / 0 경고** (새 코드 관련) |

---

## 2. 캐릭터별 리워크 상세 (Before → After)

### 2.1 Ashe (Pyromancer) — CC-2G-1
> **컨셉**: Ember 자해-폭딜 루프. HP 낮을수록 위력 급증.

| 스킬 | Before | After |
|------|--------|-------|
| Cinder Accretion | (BehaviorTag 없음) | `TargetFullHP(3)` — 풀피 적 +3 (첫 턴 5→8) |
| Phoenix Renewal | CleanseLowTarget | **변경 없음** |
| Brand of Ash | Berserk(0) | `Berserk(0) + Desperation(1)` — 잃은 HP 10당 +1 |
| Embrace of Cinders | (BehaviorTag 없음) | `AllIn(10)` — AP 0일 때 +10 |

**콤보 예시**: HP 30인 Ashe가 Brand of Ash 시전 → Berserk(8×2) + Desperation(40/1) = **56 위력**

### 2.2 Duran (Warrior) — CC-2G-2
> **컨셉**: 복수-탱커 루프. 쉴드/잃은 HP 기반 위력.

| 스킬 | Before | After |
|------|--------|-------|
| Shield Wall | ResourceThresholdShield(5) | **변경 없음** |
| Provoking Shield | (BehaviorTag 없음) | **변경 없음** |
| Revenge Strike | (BehaviorTag 없음) | `Bulwark(5)` — 쉴드 보유 시 +5 (Shield Wall 콤보) |
| Last Bastion | (BehaviorTag 없음) | `Desperation(1)` — 잃은 HP 10당 +1 Shield |

### 2.3 Lumi (Cryomancer) — CC-2G-3
> **컨셉**: 쉴드-빙결 콤보. Frost Armor 후 빙하 창 폭딜.

| 스킬 | Before | After |
|------|--------|-------|
| Frostbolt | TargetFreeze(3) | **변경 없음** |
| Frost Armor | (BehaviorTag 없음) | **변경 없음** |
| Blizzard | (BehaviorTag 없음) | **변경 없음** |
| Glacial Spike | (BehaviorTag 없음) | `Bulwark(4) + GiantSlayer(5)` — 쉴드+4 / 보스전+5 |

**콤보 예시**: Frost Armor → Glacial Spike (보스전) → 12+4+5 = **21 위력**

### 2.4 Taranis (Stormcaller) — CC-2G-4
> **컨셉**: 네트워크 연쇄. Charge 축적된 적에게 폭발.

| 스킬 | Before | After |
|------|--------|-------|
| Wire | Propagate(1) | **변경 없음** |
| Branch | (BehaviorTag 없음) | **변경 없음** |
| Grounding Field | (ShieldFlag) | **변경 없음** |
| Thunderstorm | (BehaviorTag 없음) | `Explosion(3)` — Charge 3스택+ 적에게 스택×3 추가 |

**콤보 예시**: Wire로 Charge 축적 → Thunderstorm 시전 → 10 + 9(폭발) = **19 위력**

### 2.5 Sibyl (Oracle) — CC-2G-5 (신규 Behavior 3종)
> **컨셉**: Prophecy-지연 딜. 파티 일점사/시간 축 강화.

| 스킬 | Before | After |
|------|--------|-------|
| Death Prophecy | (BehaviorTag 없음) | `FollowUp(4)` — 이미 맞은 적 +4 |
| Vision of Renewal | (BehaviorTag 없음) | `LimitBreak(8)` — 전투당 첫 힐 +8 (12→20) |
| Borrowed Future | (BehaviorTag 없음) | **변경 없음** (Buff라 PowerModify 무의미) |
| Déjà Vu | (BehaviorTag 없음) | `Echo(0)` — 위력 절반 2회 (10+5=15) |

### 2.6 Umbra (Rogue) — CC-2G-6
> **컨셉**: 암살 콤보 강화. 독/할퀴기 후 기습.

| 스킬 | Before | After |
|------|--------|-------|
| Poison Blade | (BehaviorTag 없음) | **변경 없음** |
| Backstab | StrongVsDebuff(0) | `StrongVsDebuff(0) + FollowUp(3)` — 이미 맞은 적 +3 |
| Rupture | Cull(0) | **변경 없음** |
| Eviscerate | (Shadows 1 소모, MinReq 3) | **변경 없음** |

### 2.7 Aster (Archer) — CC-2G-7
> **컨셉**: 연사 스노우볼. 쏠수록 강해짐.

| 스킬 | Before | After |
|------|--------|-------|
| Quick Shot | (BehaviorTag 없음) | `Momentum(1)` — 매 사용 시 +1 누적 |
| Multi-Shot | ComboMultiHit | **변경 없음** |
| Hunter's Mark | (BehaviorTag 없음) | **변경 없음** |
| Execute Shot | ComboFinisher | **변경 없음** |

**★ 핵심 발견**: Momentum/Fatigue/Escalation/Mastery 4종은 SkillInstance.EffectivePower/EffectiveCost에 이미 구현되어 있음. 별도 Behavior 클래스 없이 BehaviorTag만 추가하면 자동 작동.

### 2.8 Elara (Healer) — CC-2G-8 (생략)
이미 BehaviorTag **4/4 보유** (BondLinkBoost, MercyAccumulate × 2, MercyConsume × 2, CleanseLowTarget). 추가 변경 없음.

---

## 3. 신규 Behavior 구현체 5종 (ConditionalBehaviors.cs)

### 3.1 TargetFullHPBehavior — CC-2G-1
```csharp
Keyword: TargetFullHP | Phases: PowerModify | Order: 55
로직: target.Health.CurrentHP >= MaxHP 시 power + rank
용도: Ashe Cinder Accretion — 풀피 적 셋업 강화
```

### 3.2 ExplosionBehavior — CC-2G-4
```csharp
Keyword: Explosion | Phases: PostApply | Order: 40
로직: target의 Charge 스택이 rank+일 때 (스택 × 3) 추가 데미지
상수: DamagePerStack = 3 (Taranis Charge 최대 3스택이라 항상 +9)
용도: Taranis Thunderstorm — Charge 네트워크 폭발
```

### 3.3 FollowUpBehavior — CC-2G-5
```csharp
Keyword: FollowUp | Phases: PowerModify | Order: 65
로직: target.HitThisTurn == true 시 power + rank
의존: Character.HitThisTurn (신규 인프라)
용도: Sibyl Death Prophecy, Umbra Backstab — 일점사 시너지
```

### 3.4 EchoBehavior — CC-2G-5
```csharp
Keyword: Echo | Phases: PostApply | Order: 50
로직: 메인 타겟에게 CurrentPower / 2 추가 데미지 (최소 1)
용도: Sibyl Déjà Vu — "데자부" 컨셉 (위력 절반 2회)
```

### 3.5 LimitBreakBehavior — CC-2G-5
```csharp
Keyword: LimitBreak | Phases: PowerModify | Order: 80
로직: ctx.Instance.UsesThisBattle == 0 (전투당 첫 사용) 시 power + rank
의존: SkillInstance.UsesThisBattle (ARCH-5 인프라)
용도: Sibyl Vision of Renewal — 첫 힐 강화
```

---

## 4. 인프라 변경 사항

### 4.1 Character.HitThisTurn (FollowUp 추적)
**Character.cs** (+5줄)
```csharp
public bool HitThisTurn { get; private set; }
public void MarkHitThisTurn() => HitThisTurn = true;
public void ResetHitThisTurn() => HitThisTurn = false;
```

### 4.2 HealthComponent 훅
**HealthComponent.cs** (+2줄)
- `TakeDamage` HP 감소 블록 끝에 `_owner?.MarkHitThisTurn()` 추가
- `TakeDirectDamage`에도 동일 추가 (Pierce/Execution용)

### 4.3 TurnManager 매 턴 리셋
**TurnManager.StartNewTurn** (+3줄)
```csharp
foreach (var c in _playerParty) c.ResetHitThisTurn();
foreach (var c in _enemies) c.ResetHitThisTurn();
```

---

## 5. BehaviorRegistry 등록 현황

`Assets/02.Scripts/Skill/Behaviors/BehaviorRegistry.cs`에 5종 추가:
```csharp
// ── Phase CC-2G: 기존 8종 스킬 리워크 ──
Register(new Implementations.ExplosionBehavior());
Register(new Implementations.FollowUpBehavior());
Register(new Implementations.EchoBehavior());
Register(new Implementations.LimitBreakBehavior());
// (CC-2G-1은 TargetFullHPBehavior가 위쪽 Phase ARCH-4 그룹에 등록됨)
```

---

## 6. BattleDisplayUtil.GetBehaviorLabel — 신규 라벨 6종

```csharp
TargetFullHP: "풀피 적 +{rank}"
FollowUp:     "이미 맞은 적 +{rank}"
Echo:         "위력 절반 2회"
LimitBreak:   "전투당 1회 +{rank}"
Explosion:    "전하 폭발 (스택×3)"
Momentum:     "사용 시 위력 +{rank} 누적"
Fatigue:      "사용 시 위력 -{rank} 누적"  // 백로그 대응
```

---

## 7. 테스트 매트릭스 (24개 신규)

### PhaseCC2GTests — CC-2G-1 (Ashe, 11개)
| 섹션 | 테스트 수 | 검증 항목 |
|------|----------|----------|
| TargetFullHP | 3 | 풀피 적 보너스/비-풀피 무효/rank 합산 |
| Desperation | 3 | 풀피 무효/잃은 HP 비례/rank 비례 |
| AllIn | 3 | AP=0 보너스/AP>0 무효/TurnCtx=null 안전 |
| Ashe 콤보 | 2 | Berserk+Desperation 연산 순서 (56)/Cinder Accretion 풀피 |

### PhaseCC2GTests — CC-2G-2~7 (13개)
| 섹션 | 테스트 수 | 검증 항목 |
|------|----------|----------|
| Duran | 2 | Bulwark 쉴드 보유 시 +5, Desperation Shield 증가 |
| Lumi | 1 | Glacial Spike 보스전 12+4+5=21 |
| Taranis | 2 | Explosion Charge 보너스/Charge 없을 때 무효 |
| Sibyl | 5 | FollowUp 발동/미발동, Echo 절반, LimitBreak 첫/두 번째 사용 |
| Umbra | 1 | Backstab FollowUp 콤보 |
| Aster | 2 | Momentum 첫 사용 기본/세 번째 사용 +2 |

---

## 8. 수정된 파일 목록

### 스크립트 (8개 파일)
1. `Assets/02.Scripts/Skill/BehaviorKeyword.cs` — TargetFullHP 주석 재정의
2. `Assets/02.Scripts/Skill/Behaviors/Implementations/ConditionalBehaviors.cs` — 신규 Behavior 5종 + using TeamLog.Characters 추가
3. `Assets/02.Scripts/Skill/Behaviors/BehaviorRegistry.cs` — 5종 Register 호출
4. `Assets/02.Scripts/Characters/Character.cs` — HitThisTurn 인프라
5. `Assets/02.Scripts/Characters/Components/HealthComponent.cs` — MarkHitThisTurn 훅 2곳
6. `Assets/02.Scripts/Combat/Turn/TurnManager.cs` — 매 턴 시작 HitThisTurn 리셋
7. `Assets/02.Scripts/UI/Battle/BattleDisplayUtil.cs` — 신규 라벨 6종
8. `Assets/02.Scripts/Editor/DataGenerator.PhaseCC.cs` — 6캐릭터 19스킬 업데이트

### 테스트 (1개 파일)
- `Assets/02.Scripts/Tests/PhaseCC2GTests.cs` — 24개 테스트

### 문서 (3개 파일)
- `Assets/09.Docs/WorkLog/2026-07-17-CC-2G-1.md`
- `Assets/09.Docs/WorkLog/2026-07-17-CC-2G-2-7.md`
- `Assets/09.Docs/WorkLog/2026-07-17-CC-2G-COMPLETE.md` (본 문서)
- `CLAUDE.md` (Phase 표 + 진행 상태 + 테스트 수)
- `MEMORY.md` (CC-2G 완료 요약)

### .asset (DataGenerator 메뉴 재실행 시 자동 갱신, 사용자 실행 필요)
- Ashe 4스킬 + Duran 2스킬 + Lumi 1스킬 + Taranis 1스킬 + Sibyl 3스킬 + Umbra 1스킬 + Aster 1스킬 = **13개 .asset**

---

## 9. 핵심 설계 결정 요약

1. **HitThisTurn은 Character 자체 필드**: CombatEventBus나 별도 매니저 대신 Character에 두어 캡슐화. 매 턴 시작 시 리셋, HP 감소 시 true.
2. **LimitBreak는 Heal에 재배치**: Buff 스킬은 CurrentPower가 무의미 (statusEffect.value 고정). Vision of Renewal(Heal)에 배치하여 힐량 증가로 직관적 효과.
3. **Momentum은 별도 Behavior 없이 작동**: SkillInstance.EffectivePower에서 이미 `power += UsesThisBattle × momentumRank` 처리됨 (ARCH-5). BehaviorTag만 추가하면 자동 반영.
4. **Explosion DamagePerStack=3**: Taranis Charge 최대 3스택이므로 항상 +9. 밸런스 조정 시 상수 1줄 변경으로 간단.
5. **Echo는 PostApply에서 CurrentPower/2**: PowerModify로 증가된 최종 위력 기준. 재귀 호출 대신 DamageCalculator.DealDamage 직접 호출로 단순화.

---

## 10. 사용자 잔여 작업

### 10.1 DataGenerator 메뉴 재실행 (필수)
Unity Editor 메뉴: `TeamLog/Generate Test Data`
- 13개 .asset에 새 BehaviorTag 반영
- 코드-에셋 동기화 (현재는 코드만 수정된 상태)

### 10.2 Play 모드 검증 (선택)
BattleTestScene에서 각 캐릭터 스킬 사용 시:
- 스킬 설명 툴팁에 BehaviorTag 한국어 라벨 표시 확인
- 데미지 로그로 조건부 보너스 발동 확인
- 추천 검증 시나리오:
  - Ashe: 풀피 적 → Cinder Accretion → HP 낮아진 후 Brand of Ash
  - Sibyl: 동료가 적 때린 후 → Death Prophecy FollowUp 발동
  - Taranis: Wire 2회 → Thunderstorm Explosion 폭발

### 10.3 밸런스 시뮬레이션 (후속 작업)
`BalanceSimulator`로 Quick Combat 1000팩 실행:
- 캐릭터별 승률 곡선 측정 (특히 Taranis/Sibyl — 딜링 패턴 변화)
- HitThisTurn/Momentum 누적이 시뮬레이터에 제대로 반영되는지 점검
- BalanceSimulator에 CharacterTraitHandler 통합 여부 확인 (CC-2G 이전부터 대기 과제)

---

## 11. Phase CC-2G 완료 상태 (최종)

| Phase | 캐릭터 | 상태 | 신규 Behavior |
|-------|--------|------|--------------|
| CC-2G-1 | Ashe | **완료** | TargetFullHP |
| CC-2G-2 | Duran | **완료** | (기존 만족) |
| CC-2G-3 | Lumi | **완료** | (기존 만족) |
| CC-2G-4 | Taranis | **완료** | Explosion |
| CC-2G-5 | Sibyl | **완료** | FollowUp, Echo, LimitBreak |
| CC-2G-6 | Umbra | **완료** | (FollowUp 재사용) |
| CC-2G-7 | Aster | **완료** | (Momentum은 SkillInstance에 구현) |
| CC-2G-8 | Elara | **생략** | (이미 4/4 보유) |

**총 신규 Behavior 구현체**: 5종
**총 테스트**: 297/297 통과
**로드맵 완료일**: 2026-07-17
