# [DRAFT → 확정 예정] Calliope, the Bard — "주선율과 부선율의 교향곡"

> **상태**: 🟢 확정 (2026-07-17 Melody 컨셉 + 핵심 결정 완료 — 코드 구현 대기)
> **슬롯**: Bard (기존 Char_Bard 리워크)
> **상위 문서**: [INDEX.md](INDEX.md), [CharacterConceptReview.md 5.11](../../CharacterConceptReview.md)
> **기존 특성 파일**: `Trait_Bard_BattleSong/CourageChord/HealingMelody.asset`

### 2026-07-17 확정 사항 (사용자 제안 + 결정)
- **이름**: Calliope (그리스 "서사시의 여신")
- **자원**: Melody (주 선율 + 부 선율 메아리)
- **부 선율 위력**: 주 선율의 50% (힐 8→4, ATK+3→+1)
- **같은 스킬 연속 사용**: 부 선율 무효화 (주만 작동) — 매 턴 다른 스킬 강제
- **유틸리티 스킬 (Inspiring Refrain)**: 파티 AP+1 + 쉴드 5 / 부: 쉴드 3

---

## 1. 정체성 (한 문장)

> **"이번 턴 연주한 곡은 주 선율로, 직전 턴의 곡은 부 선율로 메아리친다 — 두 선율이 겹쳐 울릴 때, 진정한 교향곡이 완성된다."**

## 2. 이름

**Calliope** (그리스 "서사시의 여신") — 음악 신 서사. 한국어 "칼리오페" 발음. 약간 길지만 독특

## 3. 역할군

- **주 역할군**: 파티 서포터 (매 턴 다른 효과 — 힐/버프/디버프/유틸)
- **부 역할군**: 리듬형 메아리 (직전 행동의 50% 효과 자동 발동)

## 4. 강점 / 약점

| 강점 | 약점 |
|------|------|
| 매 턴 2개 효과 동시 작동 (주+부 선율) | **같은 스킬 연속 시 부 선율 무효** (단조 플레이 페널티) |
| 회복/버프/디버프/유틸 4영역 커버 | 단일 딜 0 (순수 서포터) |
| 매 턴 다른 스킬 자연 유도 (전략성) | AP 부족 시 부 선율도 발동 안 함 (자원 의존) |
| 파티 모든 캐릭터와 시너지 | 적 도트 폭격 시 회복 부족 (Mending 1개로는 한계) |

**DesignPillars 약점 유형**: **자연 제한 (리듬 다양성 강제)** — 같은 스킬 반복 시 효율 급감

## 5. 고유 메카닉: Melody (주 선율 + 부 선율)

### 자원 구조
- **MelodyType** enum (4종 선율): None / Healing / Valor / Dissonance / Inspiration
- **CurrentMelody**: 이번 턴 연주한 스킬의 선율 (주 선율, 100% 효과)
- **EchoMelody**: 직전 턴 연주한 스킬의 선율 (부 선율, 50% 효과)

### 매 턴 흐름
```
[턴 N 시작]
  1. 이번 턴 CurrentMelody → EchoMelody로 이동 (직전 곡이 부 선율 됨)
  2. EchoMelody 자동 발동 (50% 효과) — 같은 선율이면 무효화 (주만 작동)
[턴 N 행동]
  3. 스킬 X 시전 → X 즉시 효과 (100%) + X가 새 CurrentMelody로 설정
[턴 N+1 시작]: (다시 1번으로)
```

### 같은 스킬 연속 사용 페널티 (사용자 결정)
- 턴 N: Mending → CurrentMelody = Healing
- 턴 N+1 시작: EchoMelody = Healing (Mending 부 선율 50% 발동)
- 턴 N+1: Mending 다시 사용 → CurrentMelody = Healing
  - **EchoMelody가 CurrentMelody와 같으므로 부 선율 무효** (주 선율만 작동)
  - 즉 힐 8만 (힐 4 부 선율 사라짐) → 매 턴 다른 스킬 유도

### 기존 8종 자원과의 차별화 ⭐
| 자원 | 축전 패턴 | 본질 |
|------|---------|------|
| Ember/Vengeance/Shadows/Combo | 개인 행동 | 딜러/탱커 |
| Prophecy/Charge | 시간/공간축 | 서포터/제어 |
| Mercy | "누구를 위해" | 보호 서포터 |
| **Melody** | **"이전 행동이 현재를 강화" (기억/메아리)** | **리듬 서포터** ⭐ |

→ 완전히 새로운 축. 직전 턴 선택이 다음 턴에 약하게 이어지는 구조

## 6. 스킬 4종 (회복/버프/디버프/유틸 4영역)

| 스킬 | AP | 타입 | 주 선율 (100%) | 부 선율 (50%) |
|------|----|------|---------------|---------------|
| **Mending Song** (치유의 노래) | 2 | 회복 | 단일 힐 8 | 단일 힐 4 (가장 부상당한 아군 자동) |
| **Anthem of Valor** (용기의 찬가) | 2 | 버프 | 광역 ATK+3 (2턴) | 광역 ATK+1 (1턴) |
| **Dissonant Chord** (불협화음) | 2 | 디버프 | 광역 적 ATK-3 (2턴) | 광역 적 ATK-1 (1턴) |
| **Inspiring Refrain** (영감의 후렴) | 2 | 유틸리티 | 파티 AP+1 + 쉴드 5 | 쉴드 3 (파티 전체 자동) |

### 조건 다양성 검증 (4.5 원칙 2 — 스킬 타입 자체가 4개 다른 목적)
- Mending Song → 회복 (대상 부상 HP<70% 시 추가 효과 검토)
- Anthem of Valor → 버프 (이미 버프 받은 아군 대상 추가 위력)
- Dissonant Chord → 디버프 (적 3마리+ 시 추가 효과)
- Inspiring Refrain → 유틸리티 (자원 없이 즉시 발동)

→ 4개 모두 다른 목적. 매 턴 다른 퍼즐. ✅

### 부 선율 자동 발동 상세
- **Mending 부**: 가장 부상당한 파티원 자동 힐 4 (Bard 안 함)
- **Anthem 부**: 파티 전체 ATK+1 (1턴)
- **Dissonant 부**: 적 전체 ATK-1 (1턴)
- **Inspiring 부**: 파티 전체 쉴드 3

### 기존 스킬 매핑
| 기존 | 신규 | 변경 |
|------|------|------|
| Bard_BattleSong (광역 AtkUp, AP2) | Anthem of Valor | ATK+3 (위력 상향), Melody 시스템 통합 |
| Bard_WeakenMelody (광역 AtkDown, AP2) | Dissonant Chord | ATK-3, Melody 시스템 통합 |
| Bard_CourageChord (힐 8, AP2) | Mending Song | Courage(단일 버프) → Mending(단일 힐)으로 재설계 |
| Bard_Blessing (광역 DefUp, AP3) | Inspiring Refrain | DefUp → AP+1+쉴드 (유틸로 재설계) |

## 7. BehaviorTag 활용

| BehaviorTag | 적용 스킬 | 효과 | 상태 |
|------------|----------|------|------|
| **신규: `MelodyHealing`** | Mending Song | CurrentMelody=Healing 설정 | **신규 구현 필요** (ApplyMain Phase, ~5줄) |
| **신규: `MelodyValor`** | Anthem of Valor | CurrentMelody=Valor 설정 | **신규 구현 필요** (~5줄) |
| **신규: `MelodyDissonance`** | Dissonant Chord | CurrentMelody=Dissonance 설정 | **신규 구현 필요** (~5줄) |
| **신규: `MelodyInspiration`** | Inspiring Refrain | CurrentMelody=Inspiration 설정 | **신규 구현 필요** (~5줄) |

### 자동 부 선율 발동은 MelodyResourceComponent에서 처리
- `OnTurnStart`: 이번 턴 CurrentMelody → EchoMelody 이동 + EchoMelody 50% 효과 자동 발동 (같은 선율이면 스킵)

## 8. 장착 특성 3종 리워크

| 특성 | 기존 효과 | 리워크 효과 | 해금 |
|------|---------|------------|------|
| **전투 노래** (기본) | 매 턴 AP +1 | **부 선율 위력 50% → 75%** (메아리 강화) | 기본 |
| **용기의 화음** | 전투 시작 ATK +2 | **같은 스킬 연속 사용 시 부 선율 무효화 페널티 제거** (자유 연주) | 30 조각 |
| **치유 멜로디** | 매 턴 종료 HP +2 | **EchoMelody 발동 시 추가 효과** (Anthem이면 쉴드 5, Dissonant이면 추가 도트 등) | 60 조각 + 1 영혼 |

### 특성 키워드 매핑
| 특성 | KeywordType | Trigger | Value |
|------|------------|---------|-------|
| 전투 노래 | **`EchoPowerMul`** (신규) | Passive | 0.75 (부 선율 배율) |
| 용기의 화음 | **`RepeatNoPenalty`** (신규) | Passive | 1 (페널티 무시 플래그) |
| 치유 멜로디 | **`EchoBonusEffect`** (신규) | Passive | 1 (추가 효과 플래그) |

## 9. 밸런스 시나리오 (보스전 5턴 — Calliope/Duran/Ashe/Umbra)

```
턴 1: Anthem of Valor (광역 ATK+3 2턴). CurrentMelody=Valor
턴 2 시작: EchoMelody=Valor → 파티 ATK+1 (1턴)
턴 2: Dissonant Chord (광역 적 ATK-3). CurrentMelody=Dissonance
턴 3 시작: EchoMelody=Dissonance → 적 ATK-1 (1턴)
턴 3: Mending Song (Ashe 힐 8, 자해 회복). CurrentMelody=Healing
턴 4 시작: EchoMelody=Healing → 가장 부상당한 아군 힐 4
턴 4: Inspiring Refrain (파티 AP+1 + 쉴드 5). CurrentMelody=Inspiration
턴 5 시작: EchoMelody=Inspiration → 파티 쉴드 3
턴 5: Mending Song (위기 아군 힐 8)
       (부 선율 Healing == 이전 Mending → 같은 선율 페널티로 부 무효)
```

**비교 — 매 턴 다른 스킬 vs 같은 스킬 반복**:
```
매 턴 다른 스킬 (5턴):
  힐 8+0 / 버프 / 디버프 / 힐 8+4 / 버프
  총 힐: 20, 버프: 2회, 디버프: 1회
같은 스킬 반복 (Mending 5회 — 페널티):
  힐 8+4 / 힐 8+0(무효) / 힐 8+0 / 힐 8+0 / 힐 8+0
  총 힐: 40, 버프: 0, 디버프: 0 — 전략성 0
```

→ 다른 스킬 사용이 다양한 효과로 보상. 같은 스킬 반복은 힐만 많고 다른 기능 0

## 10. 파티 시너지 (메카닉 자체가 시너지 창출)

| 조합 | 시너지 | 핵심 |
|------|-------|------|
| **Calliope + Elara** | ★★★ | 이중 힐 — Mending 주 선율 + Elara Mercy 자동 힐. 회복량 폭발 |
| **Calliope + Aster** | ★★★ | Inspiring Refrain AP+ → Aster Combo 유지 + Anthem ATK+ → 다타수 강화 |
| **Calliope + Umbra** | ★★★ | Dissonant Chord 적 디버프 → Umbra StrongVsDebuff 자동 트리거 (위력 2배) |
| **Calliope + Ashe** | ★★ | Mending Echo로 자해 회복 보조 + Anthem ATK+로 폭딜 강화 |
| **Calliope + Duran** | ★★ | Anthem ATK+로 Vengeance 딜 강화 + Mending 회복으로 탱킹 보조 |

## 11. ✅ 결정 항목 (2026-07-17 확정)

- [x] **이름**: Calliope (그리스 "서사시의 여신")
- [x] **자원 이름**: Melody (주 선율 + 부 선율)
- [x] **부 선율 위력**: 주 선율의 50%
- [x] **같은 스킬 연속 사용**: 부 선율 무효화 (주만 작동) — 매 턴 다른 스킬 유도
- [x] **유틸리티 (Inspiring Refrain)**: 파티 AP+1 + 쉴드 5 / 부: 쉴드 3
- [x] **MelodyType 4종**: Healing / Valor / Dissonance / Inspiration
- [x] **부 선율 자동 발동**: 매 턴 시작 시 EchoMelody 자동 (50% 효과)
- [x] **기존 스킬 매핑**: BattleSong→Anthem, WeakenMelody→Dissonant, CourageChord→Mending, Blessing→Inspiring
- [x] **특성 3종**: 전투 노래(부 선율 75%)/용기의 화음(반복 페널티 제거)/치유 멜로디(Echo 추가 효과)

## 12. 리스크와 검증

| 리스크 | 완화 |
|-------|-------|
| 매 턴 다른 스킬이 너무 엄격 (원하는 스킬 못 씀) | "용기의 화음" 특성(60+1영혼)으로 반복 페널티 제거 가능 |
| 부 선율 50%가 너무 약해서 의미 없음 | "전투 노래" 특성으로 75%까지 강화 가능 |
| 파티 AP+1 매 턴 = 사기 | Inspiring Refrain은 AP 2 소모 → 순이익 +1. 다른 스킬 안 쓰면 부 선율 페널티 |
| Mending 부 선율이 매번 가장 부상당한 아군 자동 힐 → AI 의존 | 단순 로직 (가장 HP 낮은 파티원). Elara Mercy와 시너지 |
| 4스킬 모두 AP 2 = AP 부족 시도 | Inspiring Refrain으로 AP 보충 가능. 부 선율은 AP 0으로 자동 발동 |
| 기존 Bard_Blessing(DefUp) 제거 영향 | 세이브 호환 — 가비지 컬렉션 대상 (이전 리워크 패턴과 동일) |

## 13. 구현 메모 (코드 구현 시 — 별도 Phase CC-2D)

### 신규 코드 필요
1. **MelodyResourceComponent** (`Characters/Components/`)
   - `MelodyType` enum (None/Healing/Valor/Dissonance/Inspiration)
   - `CurrentMelody` / `EchoMelody` 프로퍼티
   - `OnTurnStart`: Current → Echo 이동 + Echo 50% 효과 자동 발동 (같은 선율이면 스킵)
   - `SetCurrentMelody(MelodyType)` — 각 스킬의 Behavior에서 호출
2. **BehaviorKeyword** 4종 신규: `MelodyHealing/MelodyValor/MelodyDissonance/MelodyInspiration`
3. **4종 Behavior** (`Skill/Behaviors/Implementations/MelodyBehaviors.cs`)
   - ApplyMain Phase. ctx.Caster.Resource.SetCurrentMelody(type) 호출
4. **KeywordType** 3종 신규: `EchoPowerMul`/`RepeatNoPenalty`/`EchoBonusEffect`
5. **ResourceType.Melody** enum 추가
6. **Character.CreateResource** Melody 분기 추가
7. **CharacterTraitHandler** 확장 — 부 선율 배율, 반복 페널티, 추가 효과 처리

### 부 선율 자동 발동 로직 (핵심 — MelodyResourceComponent.OnTurnStart)
```csharp
public override void OnTurnStart(Character owner)
{
    var party = GetPlayerParty();
    if (party == null) return;

    // 1. CurrentMelody → EchoMelody 이동
    EchoMelody = CurrentMelody;
    CurrentMelody = MelodyType.None;

    // 2. 같은 선율 페널티 체크
    bool penaltyActive = !HasTrait(RepeatNoPenalty); // 용기의 화음 특성 시 페널티 무시

    // 3. EchoMelody 자동 발동 (50%)
    if (EchoMelody != MelodyType.None)
    {
        float power = GetEchoPowerMul(); // 기본 0.5, "전투 노래" 특성 시 0.75
        ApplyEchoEffect(EchoMelody, power, party, penaltyActive);
    }
}

private void ApplyEchoEffect(MelodyType type, float power, List<Character> party, bool penalty)
{
    // Healing: 가장 부상당한 파티원 힐 (8 * 0.5 = 4)
    // Valor: 파티 전체 ATK+ (3 * 0.5 = 1)
    // Dissonance: 적 전체 ATK- (3 * 0.5 = 1)
    // Inspiration: 파티 전체 쉴드 (5 * 0.5 = 3)
    // ...
}
```

### 구현 난이도 추정
- MelodyResourceComponent: **중상** (~100줄, OnTurnStart에서 Echo 발동 로직이 핵심)
- Behavior 4종: 낮음 (~20줄, 단순 SetCurrentMelody 호출)
- 특성 키워드 3종: 중간 (~40줄, EchoPowerMul/RepeatNoPenalty/EchoBonusEffect)
- DataGenerator/UI: 낮음 (~50줄)
- **총합**: 약 210줄 + 4 스킬 .asset 재생성
- Mercy(Elara)보다 약간 큰 규모 (부 선율 자동 발동 4종류 처리)

### 테스트 계획 (PhaseCC2DTests.cs 신규)
1. MelodyResourceComponent: CurrentMelody 설정, Echo 이동
2. 부 선율 자동 발동 (4종 각각 — Healing/Valor/Dissonance/Inspiration)
3. 같은 스킬 연속 시 부 무효화
4. EchoPowerMul 특성 (부 선율 75%)
5. RepeatNoPenalty 특성 (반복 시에도 부 선율 발동)
6. 4스킬 매핑 검증

---

## 변경 이력

| 날짜 | 변경 |
|------|------|
| 2026-07-14 | 최초 작성 (Rhythm 자원 — 임계 폭발 컨셉, 🔴 초안) |
| 2026-07-17 | **Melody(주/부 선율 메아리) 컨셉으로 전면 재작성** (사용자 제안). 힐+버프+디버프+유틸 4영역. 🟢 확정 |
