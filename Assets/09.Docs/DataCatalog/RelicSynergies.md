# 유물 시너지 설계서 (Relic Synergy Catalog)

> 최종 갱신: 2026-06-16
> 기반 카탈로그: `RelicCatalog.md` (기존 16종 유물 데이터)
> 구현 소스: `Assets/02.Scripts/Reward/RelicData.cs`, `RelicHandler.cs`, `KeywordResolver.cs`
> 관련 에디터: `Assets/02.Scripts/Editor/DataGenerator.Relics.cs`

---

## 1. 설계 철학

### 1.1 핵심 원칙 (사용자 합의사항)

| 원칙 | 설명 | 위반 사례 (금지) |
|------|------|------------------|
| **확정 효과 only** | 발동 조건이 충족되면 100% 발동. 확률(RNG) 기반 효과 금지 | "30% 확률로 크리티컬", "20% 확률로 AP 환급" |
| **개별 약화, 조합 폭발** | 유물 1개 = +1~3 수준의 미미한 효과. 3종 세트 결합 시 시너지 루프로 폭발적 강화 | 단독으로 "+10 데미지", "매 턴 5 쉴드" |
| **트리거 체인** | 한 유물의 발동이 다른 유물의 트리거를 다시 건드리는 구조 | 서로 독립적인 효과 병렬 나열 |
| **조건 ≠ 확률** | "HP 30% 이하일 때"는 조건(100% 발동), "30% 확률"은 확률(금지) | 혼동 금지 |

### 1.2 시너지 루프 패턴 3가지

모든 카테고리는 아래 3가지 루프 패턴 중 하나를 따른다.

#### 패턴 A: 이벤트 폭포 (Event Cascade)
```
[행동 X] → 트리거 T1 → 효과 E1 + 트리거 T2 → 효과 E2 + 트리거 T3 → 효과 E3
```
한 행동이 연쇄적으로 효과를 발생시키는 구조.

#### 패턴 B: 자원 순환 (Resource Cycle)
```
자원 A 소모 → 자원 B 획득 → 자원 B로 자원 A 회복 → 반복
```
자원(AP/골드/쉴드/HP)이 순환하며 누적되는 구조.

#### 패턴 C: 조건부 배율 (Conditional Multiplier)
```
조건 C 충족 → 효과 X 발동
효과 X가 있으면 조건 C 더 쉽게 충족
```
피드백 루프로 조건이 점점 쉽게 충족되는 구조.

---

## 2. 시스템 요구사항 (새 기능)

### 2.1 기존 시스템으로 커버되는 것

아래 키워드/트리거는 `KeywordEntry.cs`에 이미 존재하므로 데이터만 추가하면 즉시 구현 가능.

- **KeywordType**: PowerAdd, PowerMul, CostAdd, ShieldMul, HealMul, EffectMul, DurationAdd, ShieldPerTurn, DamageTakenMul, BonusOutgoingDamage, DamageReduction, CounterDamage, OnKillHeal, DamageDealtHealPercent, StackingPowerOnKill, MaxHPUp, ATKUp, DEFUp, ExtraAP, BonusGold
- **KeywordTrigger**: Passive, OnTurnStart, OnTurnEnd, OnBattleStart, OnDamageDealt, OnDamageReceived, OnKill, OnHealApplied, OnShieldGained, OnSkillUsed, OnGoldEarned, HPBelow
- **RelicTrigger**: BattleStart, TurnStart, TurnEnd, OnDamageDealt, OnDamageReceived, OnKill, OnHealApplied, OnShieldGained, OnSkillUsed, OnGoldEarned

### 2.2 신규 확장 필요 (구현 우선순위순)

일부 시너지 루프는 현재 시스템으로 표현 불가. 아래 확장이 필요하다.

#### A. KeywordType 3종 추가

|新增 타입| 의미 | 사용 카테고리 |
|--------|------|---------------|
| `HealPerTurn` | 매 턴 시작 시 HP 회복 (ShieldPerTurn과 대칭) | C(힐), H(팀워크) |
| `PowerAddOnCondition` | 조건부 위력 가산 (조건 파라미터 별도) | F(집중사격) |
| `CostRefundOnCondition` | 조건 충족 시 코스트 환급 (AP 회복 아님) | E(비전) |

#### B. KeywordTrigger 2종 추가

|新增 트리거| 의미 | 사용 카테고리 |
|----------|------|---------------|
| `OnCasterLowHP` | 시전자 HP 조건 (HPBelow와 달리 캐스터 기준) | I(리스크) |
| `OnEnemyLowHP` | 대상 적 HP 조건 | F(집중사격) |

#### C. RelicTrigger 1종 추가 (선택)

|新增 트리거| 의미 | 사용 카테고리 |
|----------|------|---------------|
| `OnRerollUsed` | 리롤 1회 소비 시 | G(운명조작) |

> **주의**: 신규 트리거/키워드는 `KeywordResolver`, `RelicHandler`, `DamageCalculator`의 이벤트 발행 지점 수정이 필요. 구현 난이도: A < B < C 순.

### 2.3 구현 로드맵

| 단계 | 범위 | 예상 유물 수 |
|------|------|--------------|
| **Phase 6A** | 기존 키워드만 사용 (20종) | A, B, C, D, I 완전 + E, F, G, H 일부 |
| **Phase 6B** | 신규 KeywordType 3종 추가 | C, E, F, H 완전 구현 |
| **Phase 6C** | 신규 KeywordTrigger 2종 + RelicTrigger 1종 | G(운명조작) 완전 구현 |
| **Phase 6D** | 밸런스 시뮬레이터 3-세트 파티 모드 추가 | 전체 검증 |

---

## 3. 카테고리별 유물 설계 (9개 카테고리 × 3종 = 27종)

---

### 카테고리 A: 성전의 루프 (Holy Crusade) — 골드/처치

**루프 패턴**: B (자원 순환) — 스킬 사용 → 골드 획득 → 공격력 강화 → 처치 → 추가 골드

**시너지 다이어그램**:
```
[스킬 사용] ──A3──> 골드 +1 ──A2──> 이번 턴 ATK +1 ──> 공격
                                                         │
[다음 스킬 사용] <──A3── 골드 +2 <──A1── 적 처치 <────────┘
```

| ID | 이름 | Rare | Trigger | Keywords | 단독 효과 | 시너지 효과 |
|----|------|------|---------|----------|-----------|-------------|
| A1 | **Reliquary Cross** (성유물 십자가) | Rare | OnKill | `BonusGold(2, OnKill)` | 처치 시 2G | 골드가 풍부해 A2 발동 빈도 증가 |
| A2 | **Tithe Chalice** (십일잔) | Rare | OnGoldEarned | `PowerAdd(1, OnGoldEarned)` | 골드 획득 시 이번 턴 공격력 +1 | A1+A3가 골드를 계속 주어 매 턴 ATK 누적 |
| A3 | **Indulgence Coin** (면죄부) | Common | OnSkillUsed | `BonusGold(1, OnSkillUsed)` | 스킬 사용 시 1G | 스킬을 많이 쓸수록 A2 발동 횟수 증가 |

**개별 유물 효과**: 매우 약함 (골드 1~2, 일시적 ATK +1). 경제 빌드가 아닌 한 거의 무의미.

**3종 세트 시너지**:
- 스킬 1개 사용 → 골드 +1 → ATK +1 → 적 처치 → 골드 +2 → ATK +1 추가 → 다음 스킬更强
- 매 턴 평균 4~6 스킬 사용 → 자연스럽게 매 턴 +4~6 ATK 누적
- 골드 또한 매 턴 5~8 획득 → 상점 활용 극대화

**구현 난이도**: ★☆☆ (기존 키워드만 사용)

**권장 파티**: 모든 캐릭터 (범용)

---

### 카테고리 B: 쉴드 공명 (Aegis Resonance) — 쉴드 루프

**루프 패턴**: A (이벤트 폭포) — 턴 시작 쉴드 → 공격 강화 → 공격 시 쉴드 재획득 → 다시 공격 강화

**시너지 다이어그램**:
```
[턴 시작] ──B1──> 쉴드 +3 ──B2──> 다음 공격 +2 데미지
                                       │
                                       ▼
                                    [공격]
                                       │
[다음 공격 강화] <──B2── 쉴드 +1 <──B3── 데미지 부여 <──┘
```

| ID | 이름 | Rare | Trigger | Keywords | 단독 효과 | 시너지 효과 |
|----|------|------|---------|----------|-----------|-------------|
| B1 | **Aegis Charm** (이지스 부적) | Common | TurnStart | `ShieldPerTurn(3, OnTurnStart)` | 턴 시작 쉴드 3 | B2 발동의 시동기 |
| B2 | **Aegis Strike** (이지스 일격) | Rare | OnShieldGained | `BonusOutgoingDamage(2, OnShieldGained)` | 쉴드 얻을 때 다음 공격 +2 | B1+B3가 쉴드를 계속 주어 매 공격 강화 |
| B3 | **Aegis Counter** (이지스 반격) | Rare | OnDamageDealt | `ShieldPerTurn(1, OnDamageDealt)` (재해석: 데미지 시 쉴드 획득) | 공격 성공 시 쉴드 1 | 공격할 때마다 B2 재발동 |

**개별 유물 효과**: B1은 방어용으로 무난, B2/B3는 단독 시 거의 효과 없음.

**3종 세트 시너지**:
- 턴 시작: 쉴드 3 (B1) → 다음 공격 +2 (B2)
- 첫 공격: 데미지 +2 → 쉴드 1 획득 (B3) → 다음 공격 +2 (B2 재발동)
- 2번째 공격: 데미지 +2 → 쉴드 1 획득 → 다시 +2 ...
- **파티 전체 매 턴 4~6회 공격 = +8~12 추가 데미지 + 쉴드 4~6**

**구현 난이도**: ★★☆ (B3의 OnDamageDealt → ShieldPerTurn 변환은 `RelicHandler` 수정 필요: 쉴드 획득 이벤트를 다시 발행해야 함)

**권장 파티**: Warrior (탱커), Healer (보조)

**자연 시너지**: 기존 `ShieldAmulet`/`HardShell`과 추가 조합 가능 (방어 극대화 빌드)

---

### 카테고리 C: 생명 순환 (Life Cycle) — 힐/지속력

**루프 패턴**: C (조건부 배율) — 힐 강화 → 힐 효과가 쉴드 변환 → 쉴드가 데미지 흡수 → 생존 연장 → 힐 기회 증가

**시너지 다이어그램**:
```
[힐 시전] ──C1(1.3x)──> 힐량 증가 ──C2──> 쉴드 +2
                                         │
                                         ▼
                                   [피해 흡수]
                                         │
[다음 힐 기회] <── 생존 연장 <──────────┘
```

| ID | 이름 | Rare | Trigger | Keywords | 단독 효과 | 시너지 효과 |
|----|------|------|---------|----------|-----------|-------------|
| C1 | **Verdant Seed** (신록의 씨앗) | Rare | None (Passive) | `HealMul(1.3, Passive)` | 힐 효과 +30% | C2로 변환되는 쉴드량 증가 |
| C2 | **Sanguine Bond** (혈연의 결속) | Rare | OnHealApplied | `ShieldPerTurn(2, OnHealApplied)` (재해석: 힐 시 쉴드 획득) | 힐 받은 캐릭터 쉴드 +2 | C1과 결합 시 강력한 보호막 |
| C3 | **Mercy Blade** (자비의 칼날) | Unique | OnShieldGained | `BonusOutgoingDamage(1, OnShieldGained)` | 쉴드 획득 시 다음 공격 +1 | 힐 → 쉴드 → 공격 강화 루프 |

**개별 유물 효과**: C1은 Healer 파티에 무난, C2/C3는 단독 시 미미.

**3종 세트 시너지**:
- 힐 시전 → 힐량 1.3배 (C1) → 받는 이 쉴드 2 획득 (C2) → 다음 공격 +1 (C3)
- Healer가 매 턴 2~3회 힐 → 매 턴 쉴드 4~6, 파티 전체 공격 +2~3
- C2 획득 쉴드가 B카테고리와도 자연 연쇄

**구현 난이도**: ★★☆ (C2의 OnHealApplied → ShieldPerTurn 변환 필요, `RelicHandler` 수정)

**권장 파티**: Healer 필수, Warrior/Rogue 보조

**자연 시너지**: `HealingHerb`/`RegenRing`/`LifeCrystal`과 극강 시너지

---

### 카테고리 D: 학살자의 춤 (Slayer's Dance) — 처치 강화

**루프 패턴**: A (이벤트 폭포) — 공격 → 다음 공격 강화 → 처치 → 영구 강화 + 비용 감소 → 다음 스킬更强

**시너지 다이어그램**:
```
[공격 1] ──D2──> 다음 공격 +1
              │
              ▼
          [공격 2] ──D2──> 다음 공격 +1
                          │
                          ▼
                      [적 처치]
                          │
        ┌─────────────────┼─────────────────┐
        ▼                                   ▼
[D1: ATK +2 영구]              [D3: 다음 스킬 코스트 -1]
        │                                   │
        └──────── 다음 공격 더 강해짐 <──────┘
```

| ID | 이름 | Rare | Trigger | Keywords | 단독 효과 | 시너지 효과 |
|----|------|------|---------|----------|-----------|-------------|
| D1 | **Slayer Sigil** (도살자 인장) | Unique | OnKill | `StackingPowerOnKill(2, OnKill)` | 처치당 영구 ATK +2 | D2 누적으로 처치 속도 증가 |
| D2 | **Bloodhound Mark** (사냥개 표식) | Rare | OnDamageDealt | `StackingPowerOnKill(1, OnDamageDealt)` (재해석: 데미지 시 다음 공격 +1) | 공격 성공 시 다음 공격 +1 | 연속 공격 시 누적 가산 |
| D3 | **Executioner Axe** (처형인 도끼) | Rare | OnKill | `CostAdd(-1, OnKill)` (재해석: 다음 스킬 비용 -1) | 처치 시 다음 스킬 -1 AP | 처치 후 추가 스킬 사용 가능 |

**개별 유물 효과**: D1은 약하고, D2/D3는 단돌시 거의 효과 없음.

**3종 세트 시너지**:
- 적 4마리 파티 기준:
  - 공격 1 → +1 (D2) → 공격 2 → +1 → 적 처치 → 영구 +2 (D1), 다음 스킬 -1 (D3)
  - 공격 3 (비용 -1) → +1 → 공격 4 → +1 → 적 처치 → 영구 +2, 다음 스킬 -1
- 전투 종료 시: 영구 ATK +8~12, 추가 스킬 4~6회
- 다음 전투에서 D1 효과가 누적 유지 (StackingPowerOnKill 특성)

**구현 난이도**: ★★☆ (D2의 OnDamageDealt → 일시적 PowerAdd, D3의 OnKill → CostRefundOnCondition 필요)

**권장 파티**: Warrior (다수 공격), Mage (AoE로 다수 처치), Rogue (빠른 처치)

**자연 시너지**: 기존 `BerserkerMark`/`WarBanner`/`WeaponStone`과 극강 시너지

---

### 카테고리 E: 비전 공명 (Arcane Resonance) — 스킬/AP 루프

**루프 패턴**: B (자원 순환) — 스킬 사용 → 쉴드 획득 → 코스트 감소 → 더 많은 스킬 사용

**시너지 다이어그램**:
```
[스킬 사용] ──E2──> 쉴드 +1 ──E3──> 다음 스킬 비용 -1
                                         │
                                         ▼
                                    [다음 스킬]
                                         │
[지속 스킬 사용] <──E1────────── 매 턴 AP +1 <──┘
```

| ID | 이름 | Rare | Trigger | Keywords | 단독 효과 | 시너지 효과 |
|----|------|------|---------|----------|-----------|-------------|
| E1 | **Arcane Cell** (비전 전지) | Rare | None (Passive) | `ExtraAP(1, Passive)` | 매 턴 AP +1 | 더 많은 스킬 사용 가능 |
| E2 | **Spell Weaver** (주술사) | Common | OnSkillUsed | `ShieldPerTurn(1, OnSkillUsed)` (재해석: 스킬 시 쉴드 획득) | 스킬 사용 시 쉴드 +1 | E3 발동의 시동기 |
| E3 | **Battle Mage** (전투 마법사) | Unique | OnShieldGained | `CostAdd(-1, OnShieldGained)` (재해석: 쉴드 시 다음 스킬 -1) | 쉴드 획득 시 다음 스킬 -1 AP | E2 쉴드로 스킬 비용 회수 |

**개별 유물 효과**: E1은 AP 빌드용, E2/E3는 단독시 거의 효과 없음.

**3종 세트 시너지**:
- 매 턴 AP 6 (기본 5 + E1)
- 스킬 사용 → 쉴드 1 획득 → 다음 스킬 -1 → 스킬 사용 → 쉴드 1 획득 → -1 ...
- 평균 6 AP로 7~8개 스킬 사용 가능 (원래 4~5개)
- 쉴드 또한 매 턴 7~8 획득 (사실상 무적)

**구현 난이도**: ★★★ (E3의 OnShieldGained → CostRefundOnCondition 새 키워드 필요)

**권장 파티**: Mage (고비용 스킬), Healer (힐 스킬 다수)

**자연 시너지**: `SwiftBoots`와 시너지

---

### 카테고리 F: 집중 사격 (Marksman's Trinity) — 단일 데미지 극대화

**루프 패턴**: C (조건부 배율) — 적 HP 낮음 → 데미지 증가 → 처치 → 다음 적에게 데미지 증가 이월

**시너지 다이어그램**:
```
[적 HP 30% 이하] ──F3──> 데미지 +50%
                          │
                          ▼
                      [적 처치] ──F2──> 다음 공격 +2
                                       │
                                       ▼
                                  [다음 적]
                                       │
                                  [F1: 단일 +3]
```

| ID | 이름 | Rare | Trigger | Keywords | 단독 효과 | 시너지 효과 |
|----|------|------|---------|----------|-----------|-------------|
| F1 | **Deadeye Lens** (명사수 렌즈) | Rare | None (Passive) | `PowerAdd(3, Passive, Condition: SingleTarget)` | 단일 타겟 스킬 +3 데미지 | F2/F3와 결합 시 단일 폭딜 |
| F2 | **Critical Focus** (치명적 집중) | Rare | OnKill | `StackingPowerOnKill(2, OnKill)` | 처치 시 다음 공격 +2 | F3로 처치 후 이월 |
| F3 | **Executioner Blade** (처형인의 검) | Unique | None (Passive) | `PowerMul(1.5, HPBelow, 0.3)` (대상 적 기준) | HP 30% 이하 적에게 +50% 데미지 | F1과 결합 시 처결 보장 |

**개별 유물 효과**: F1은 Rogue에게 무난, F2/F3는 단독 시 미미.

**3종 세트 시너지**:
- 적 HP 100% → F1로 +3 데미지
- 적 HP 30% 이하 → F1(+3) × F3(1.5배) = 매우 강한 처결
- 처치 → F2로 다음 적 +2 → 첫 타부터 강한 데미지 → 또 처치
- Rogue `Backstab`/`CriticalShot`과 결합 시 1턴 킬 가능

**구현 난이도**: ★★★ (F1의 SingleTarget 조건, F3의 OnEnemyLowHP 트리거 새로 필요)

**권장 파티**: Rogue (단일 폭딜), Archer (장거리 처치)

---

### 카테고리 G: 운명 조작 (Fate Weaver) — 드로우/리롤 루프

**루프 패턴**: A (이벤트 폭포) — 리롤 → 저비용 슬롯 → 사용 → AP 절약 → 추가 리롤/스킬

**시너지 다이어그램**:
```
[리롤 사용] ──G3──> 다음 스킬 비용 -1
                     │
                     ▼
               [저비용 슬롯 사용]
                     │
[다음 슬롯 저비용화] <──G2── 다음 슬롯 가중치 조정 <──┘
                     │
[더 많은 스킬 사용] <──G1── AP 절약 누적
```

| ID | 이름 | Rare | Trigger | Keywords | 단독 효과 | 시너지 효과 |
|----|------|------|---------|----------|-----------|-------------|
| G1 | **Destiny Deck** (운명의 덱) | Rare | None (Passive) | `DrawWeightOverride(30, Passive)` | 모든 스킬 드로우 가중치 30 통일 (이미 기본값) | G2와 결합 시 특정 스킬 더 자주 |
| G2 | **Card Shark** (카드 사기꾼) | Rare | OnRerollUsed (신규) | `DrawWeightAdd(15, OnRerollUsed)` (재해석: 리롤 시 다음 슬롯 가중치 +15) | 리롰 시 다음 드로우 특정 스킬 우선 | 리롤 2회로 2개 슬롯 특화 |
| G3 | **Cheap Shot** (싸구려 샷) | Common | None (Passive) | `PowerAdd(5, Passive, Condition: Cost0)` | 코스트 0 스킬 위력 +5 | 코스트 0 스킬 자주 뽑으면 무한 강타 |

**개별 유물 효과**: G1은 효과 거의 없음 (이미 25 통일 상태), G2/G3는 단독 시 미미.

**3종 세트 시너지**:
- 리롤 2회 → 다음 2개 슬롯 가중치 +30 (G2)
- 특정 코스트 0 스킬 집중 등장 (G1+G2)
- 그 스킬 사용 → +5 데미지 (G3) + AP 1 이득
- Rogue `DoubleStrike`(Cost 0)/Warrior `Strike`(Cost 0)와 결합 시 매 턴 2~3회 +5 데미지

**구현 난이도**: ★★★ (G2의 OnRerollUsed 신규 트리거, G3의 Cost 조건부 키워드 새로 필요)

**권장 파티**: Rogue (코스트 0 스킬 다수), Warrior (코스트 0 Strike)

---

### 카테고리 H: 전우의 맹세 (Brotherhood) — 파티 시너지

**루프 패턴**: B (자원 순환) — 파티 생존 → ATK/힐 강화 → 더 잘 생존 → 시너지 유지

**시너지 다이어그램**:
```
[파티 4인 생존] ──H1──> 각자 ATK +1 (4명 = +4)
                          │
                          ▼
                     [빠른 적 처치]
                          │
[파티 생존 유지] <──H3── 매 턴 HP +1 회복 <──┘
                          │
[위기 시 추가 강화] <──H2── 아군 1명 위기 시 다른 아군 강화
```

| ID | 이름 | Rare | Trigger | Keywords | 단독 효과 | 시너지 효과 |
|----|------|------|---------|----------|-----------|-------------|
| H1 | **Brothers in Arms** (전우애) | Rare | BattleStart | `ATKUp(1, OnBattleStart)` × 파티원 수 | 전투 시작 시 파티원 수만큼 ATK +1 | 파티가 온전할 때 강력 |
| H2 | **Vow of Guardian** (수호의 맹세) | Unique | OnCasterLowHP (신규) | `PowerAdd(3, OnCasterLowHP, 0.4)` (재해석: 아군 HP<40% 시 다른 아군 +3) | 위기 시 다른 아군 강화 | H1+H3로 위기 최소화 |
| H3 | **United Front** (연대전선) | Rare | TurnEnd | `HealPerTurn(1, OnTurnEnd)` (신규 키워드) | 매 턴 종료 시 파티 전체 HP +1 | 장기전에서 강력 |

**개별 유물 효과**: H1은 무난, H2/H3는 단독 시 미미.

**3종 세트 시너지**:
- 전투 시작: 파티원 수(4) × ATK +1 = +4 (H1)
- 매 턴 종료: 파티 전체 HP +1 (H3) → 장기전에서 누적 +10~20 HP
- 위기 상황: 다른 아군 ATK +3 (H2) → 적 처치 속도 증가
- 완전 파티 기준 매 턴 +4 ATK + +1 파티 힐

**구현 난이도**: ★★★ (H2의 OnCasterLowHP 신규 트리거, H3의 HealPerTurn 신규 키워드 필요)

**권장 파티**: 4인 풀파티 (사망 페널티 큼)

---

### 카테고리 I: 리스크/보상 (Damned Trinity) — 극단적 트레이드오프

**루프 패턴**: C (조건부 배율) - 리스크 수용 → 강력한 효과 → 리스크 상쇄 → 더 큰 리스크 수용

**시너지 다이어그램**:
```
[최대 HP -10] ──I1──> 매 턴 AP +1 ──────────────┐
                                                  │
[받는 데미지 +30%] ──I2──> 주는 데미지 +30% <─────┤
                                                  │
[턴당 HP -1] ──I3──> 처치 시 최대 HP +10 <────────┘
                          │
                          ▼
[생존 연장 → 더 많은 처치 → 최대 HP 회복]
```

| ID | 이름 | Rare | Trigger | Keywords | 단독 효과 | 시너지 효과 |
|----|------|------|---------|----------|-----------|-------------|
| I1 | **Blood Pact** (혈약) | Unique | BattleStart | `MaxHPUp(-10, OnBattleStart)`, `ExtraAP(1, Passive)` | 최대 HP -10, 매 턴 AP +1 | I3로 상쇄 가능 |
| I2 | **Reckless Fury** (무모한 분노) | Unique | None (Passive) | `DamageTakenMul(1.3, Passive)`, `PowerMul(1.3, Passive)` | 받는 피해 +30%, 주는 피해 +30% | I1과 결합 시 위험, I3로 상쇄 |
| I3 | **Cursed Doll** (저주받은 인형) | Rare | TurnStart, OnKill | `HPPerTurn(-1, OnTurnStart)`, `MaxHPUp(10, OnKill)` | 턴당 HP -1, 처치 시 최대 HP +10 | I1/I2의 리스크를 상쇄 |

**개별 유물 효과**: I1은 매우 위험, I2는 양날검, I3는 약한 디트먼트.

**3종 세트 시너지**:
- 최대 HP -10 (I1) + 받는 데미지 +30% (I2) = 매우 위험
- BUT 매 턴 AP +1 (I1) + 주는 데미지 +30% (I2) = 매우 강력
- 적 5마리 처치 시 최대 HP +50 (I3) → -10 상쇄 + net +40
- I3의 턴당 -1 HP는 평균 파티 HP 100+ 기준 무시 가능
- "고위험 고수익 → 적극적 처치로 회복" 루프

**구현 난이도**: ★☆☆ (모두 기존 키워드만 사용)

**권장 파티**: Warrior (탱커로 버팀), Mage (빠른 처치로 I3 발동)

---

## 4. 전체 통계

### 4.1 희귀도 분포

| Rare | 개수 | 카테고리 |
|------|------|----------|
| Common | 4 | A3, E2, G3, B1 |
| Rare | 15 | A1, A2, B2, B3, C1, C2, D2, D3, E1, F1, F2, G1, G2, H1, H3, I3 |
| Unique | 8 | C3, D1, E3, F3, H2, I1, I2, (없음 - 위 표 정리 필요) |

> 총 27종 (Common 4, Rare 16, Unique 7 — 분배 재조정 필요)

### 4.2 트리거 사용 빈도

| Trigger | 사용 횟수 | 비고 |
|---------|-----------|------|
| Passive | 8 | 기본 스탯 강화 |
| OnKill | 6 | D카테고리 집중 |
| OnSkillUsed | 3 | A3, E2 |
| OnShieldGained | 4 | B2, C3, E3 |
| OnDamageDealt | 2 | B3, D2 |
| OnHealApplied | 1 | C2 |
| OnGoldEarned | 1 | A2 |
| TurnStart | 3 | B1, I3 |
| TurnEnd | 1 | H3 |
| BattleStart | 2 | H1, I1 |
| **OnRerollUsed (신규)** | 1 | G2 |
| **OnCasterLowHP (신규)** | 1 | H2 |
| **OnEnemyLowHP (신규)** | 1 | F3 (재사용) |

### 4.3 신규 키워드/트리거 필요 유물 분류

| 구현 난이도 | 유물 수 | 비고 |
|------------|---------|------|
| ★☆☆ (기존만) | 13 | A1, A2, A3, B1, C1, D1, E1, E2, F1, F2, G1, G3, I1, I2, I3 |
| ★★☆ (RelicHandler 수정) | 6 | B2, B3, C2, C3, D2, D3 |
| ★★★ (신규 키워드/트리거) | 8 | E3, F3, G2, G3, H2, H3 |

---

## 5. 자연 시너지 매트릭스 (기존 유물 + 신규 유물)

### 5.1 강력한 3-세트 조합 추천

| 조합 | 유물 | 기대 효과 |
|------|------|-----------|
| **철벽 탱크 빌드** | ShieldAmulet (기) + B1 + B2 | 매 턴 쉴드 5+, 공격 시 추가 데미지 |
| **광전사 학살 빌드** | BerserkerMark (기) + D1 + D2 | 처치 시 영구 ATK 누적 + 다음 공격 강화 |
| **경제 왕 빌드** | GoldCharm (기) + A1 + A3 | 스킬 사용마다 골드, 처치 시 추가 골드 |
| **생명력 폭발 빌드** | LifeCrystal + HealingHerb + C1 + C2 | 힐 효과 극대화 + 쉴드 변환 |
| **무한 스킬 빌드** | SwiftBoots + E1 + E2 + E3 | 매 턴 7-8 스킬 사용 가능 |
| **단일 폭딜 빌드** | WeaponStone + F1 + F2 + F3 | 단일 적 1턴 킬 가능 |

### 5.2 캐릭터 특화 조합

| 캐릭터 | 추천 유물 세트 | 이유 |
|--------|----------------|------|
| **Warrior** | B1+B2+B3 (쉴드 공명) + I3 | 쉴드 강화 + 탱킹 |
| **Mage** | E1+E2+E3 (비전 공명) + I2 | AP 효율 + 데미지 배율 |
| **Healer** | C1+C2+C3 (생명 순환) + H3 | 힐 극대화 + 파티 생존 |
| **Rogue** | F1+F2+F3 (집중 사격) + D1+D2 (처치) | 단일 폭딜 + 처치 강화 |

---

## 6. 밸런스 검증 계획

### 6.1 BalanceSimulator 확장 (Phase 6D)

`Assets/02.Scripts/Editor/BalanceSimulator.cs`에 다음 모드 추가:

```csharp
[MenuItem("TeamLog/Balance/Relic Synergy Test (3-set fixed party)")]
```

**시나리오**:
1. 기본 파티 (Warrior/Mage/Healer/Rogue)
2. 각 카테고리별 3-세트 유물 강제 지급
3. F1 일반 100팩, F1 보스 50팩, F3 보스 50팩
4. 각 카테고리별 승률/평균 턴/평균 잔여 HP 측정

**목표 승률** (3-세트 기준):
- F1 일반: 95~99% (강력한 세트는 원탑)
- F1 보스: 80~95%
- F3 보스: 60~80%

**페널티 체크**:
- 3-세트 없이 개별 유물만으로 승률이 크게 오르면 안 됨 (단독 +5% 이내)
- I(리스크) 카테고리는 3-세트여도 승률이 90% 넘으면 안 됨 (트레이드오프 유지)

### 6.2 개별 vs 세트 효과 비교

| 유물 | 단독 승률 변화 | 3-세트 승률 변화 | 시너지 배수 |
|------|----------------|-------------------|-------------|
| A 카테고리 | +1~2% | +8~12% | 5~6x |
| B 카테고리 | +2~3% | +10~15% | 4~5x |
| C 카테고리 | +1~2% | +6~10% | 4~5x |
| D 카테고리 | +2~4% | +12~18% | 4~5x |
| E 카테고리 | +1~3% | +10~15% | 4~5x |
| F 카테고리 | +2~3% | +8~12% | 3~4x |
| G 카테고리 | +0~1% | +6~10% | 6~10x |
| H 카테고리 | +1~2% | +7~11% | 4~5x |
| I 카테고리 | -3~5% | +5~10% | 리스크 상쇄 후 이익 |

> **검증 기준**: 모든 카테고리의 시너지 배수가 3x 이상이어야 함 (설계 원칙 준수)

---

## 7. 구현 순서 (DataGenerator.Relics.cs 확장)

### Phase 6A (기존 키워드만, 13종)

1. A1, A2, A3 (골드 루프)
2. B1 (쉴드 턴 시작)
3. C1 (힐 배율)
4. D1 (처치 ATK 누적)
5. E1 (AP +1), E2 (스킬 시 쉴드)
6. F1 (단일 데미지 +3), F2 (처치 시 다음 공격 +2)
7. G1 (드로우 가중치 통일), G3 (코스트 0 스킬 강화)
8. I1 (혈약), I2 (무모한 분노), I3 (저주 인형)

### Phase 6B (RelicHandler 수정, +6종)

1. B2 (쉴드 → 공격 강화), B3 (공격 → 쉴드)
2. C2 (힐 → 쉴드), C3 (쉴드 → 공격)
3. D2 (공격 → 다음 공격 강화), D3 (처치 → 코스트 감소)

### Phase 6C (신규 키워드/트리거, +8종)

1. **신규 KeywordType**: `HealPerTurn`, `PowerAddOnCondition`, `CostRefundOnCondition`
2. **신규 KeywordTrigger**: `OnCasterLowHP`, `OnEnemyLowHP`
3. **신규 RelicTrigger**: `OnRerollUsed`
4. E3, F3, G2, H1, H2, H3, (B/C/D 추가분)

### Phase 6D (밸런스 검증)

- BalanceSimulator 3-세트 모드 구현
- 각 카테고리별 승률 측정
- 필요 시 effectValue 조정 (+1, +2 등 미세 튜닝)

---

## 8. 위험 및 주의사항

### 8.1 무한 루프 위험

| 카테고리 | 무한 루프 가능성 | 방지책 |
|----------|-------------------|--------|
| B (쉴드) | 공격 → 쉴드 → 공격 강화 → 공격 → 쉴드 ... | 턴당 트리거 횟수 제한 필요 (RelicHandler에 `maxTriggersPerTurn = 10`) |
| E (비전) | 스킬 → 쉴드 → 코스트 -1 → 스킬 ... | AP 자원이 자연 제한 (매 턴 AP 회복량) |
| A (골드) | 스킬 → 골드 → ATK → 스킬 ... | 스킬 코스트가 자연 제한 |

### 8.2 게임 플레이 파괴 위험

- I(리스크) 카테고리: 초반 사망 가능성. 튜토리얼에서 "리스크 유물은 숙련자용" 명시 필요
- D(처치) 카테고리 + 기존 BerserkerMark: 너무 강력해질 위험. 보스 HP +15%로 상쇄 이미 적용됨

### 8.3 DataGenerator 구현 시 주의

- 새 유물 이름은 기존 16종과 중복되지 않게 `Relic_` 접두사 사용
- `_keywords` 배열의 `ConditionParam` 필드 적극 활용 (HPBelow 임계값 등)
- 모든 신규 키워드는 `KeywordResolver`에 추가 후 `RelicHandler.ProcessKeyword` 스위치문 업데이트 필수

---

## 9. 향후 확장 아이디어

### 9.1 캐릭터 고정 유물 (Character-Locked Relics)

특정 캐릭터 클래스에서만 발동하는 유물. DataGenerator에서 `CharacterClass` 필드 추가 필요.

| 아이디어 | 효과 |
|---------|------|
| Warrior's Oath | Warrior 전용: 매 턴 첫 공격 +5 데미지 |
| Mage's Grimoire | Mage 전용: 매 턴 첫 스킬 코스트 -1 |
| Healer's Vow | Healer 전용: 힐 대상이 추가로 쉴드 3 획득 |
| Rogue's Shadow | Rogue 전용: 첫 턴 드로우 가중치 2배 |

### 9.2 저주 유물 (Cursed Relics)

강력한 효과 + 영구적 디버프. 사용자가 선택적으로 수락.

| 아이디어 | 효과 |
|---------|------|
| Crown of Thorns | 매 턴 ATK +2, 받는 데미지 +50% |
| Pact of Greed | 골드 2배 획득, 매 턴 HP -2 |

### 9.3 이벤트 전용 유물 (Event-Only Relics)

특정 이벤트 선택지만 획득 가능. 희귀도 Unique 이상.

---

## 10. 참고 자료

### 10.1 참고한 게임

| 게임 | 참고 포인트 |
|------|-------------|
| **Slay the Spire** | 유물 시너지 설계, 확정 효과 지향, 필수/상황 유물 분리 |
| **Darkest Dungeon** | 리스크/보상 트레이드오프, 큐리오 시스템 |
| **Dead Cells** | 무기 시너지 트리, 록온 효과 |
| **Hades** | Mirror of Night 영구 강화, 보스 전 특화 |
| **Loop Hero** | 자동 전투 시너지 극대화 |

### 10.2 관련 문서

- `RelicCatalog.md` — 기존 16종 유물 정확한 데이터
- `MonsterCatalog.md` — 신규 7종 특수 적 설계
- `TraitCatalog.md` — 신규 7종 특성 (Punisher, Trapper 등)
- `EncounterConcepts.md` — 전투 조합 컨셉 설계서
- `CLAUDE.md` — `Reward/RelicData.cs`, `RelicHandler.cs` 구현 규칙

---

**작성자**: Claude (Game Design Mode)
**검토 필요**: 사용자 승인 후 Phase 6A 구현 착수
