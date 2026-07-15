# Umbra, the Rogue — "그림자 속에 숨은 자" 🟢 확정

> **상태**: 🟢 기획 확정 (2026-07-14)
> **슬롯**: Rogue (기존 Char_Rogue 리워크)
> **상위 문서**: [INDEX.md](INDEX.md), [CharacterConceptReview.md 5.7](../../CharacterConceptReview.md)
> **기존 특성 파일**: `Trait_Rogue_AssassinInstinct/PoisonMaster/EvasionMaster.asset`
> **선행 작업**: **치명타(critical) 시스템 신규 구현 필수** (현재 프로젝트에 없음)

---

## 1. 정체성 (한 문장)

> **"그림자 속에 숨은 자만이 완벽한 암살을 완성한다. 맞지 마라 — 그것이 유일한 규칙이다."**

Umbra는 빛을 거부한다. 한 번이라도 피를 흘리면 그림자가 깨지고, 암살자는 평범한 도적으로 돌아간다. 동료가 그녀의 그림자를 지켜줄 때 — 도발로 적을 돌리거나, 쉴드로 상처를 막거나, 위협을 일점사할 때 — Umbra는 마침내 죽음의 칼날이 된다.

## 2. 이름

**Umbra, the Rogue** (움브라, 도적)

- **어원**: 라틴어 *umbra* (그림자). 일식 핵심 그림자 (eclipse umbra)에서 차용
- **서사**: "그림자 그 자체". 어둠 속에서만 완벽해지는 암살자
- **대안 후보** (기록용): Nyx / Shade / Vax / Wraith

## 3. 역할군

- **주 역할군**: 단일 암살 딜러 (Shadows 3 도달 시 치명타 폭딜)
- **부 역할군**: 도트 디버거 (Poison/Bleed 부여)

## 4. 강점 / 약점

| 강점 | 약점 |
|------|------|
| Shadows 3 도달 시 치명타 확정 + 3배 피해 | **한 번이라도 맞으면 Shadows 전부 상실** |
| Backstab 2배 + 치명타 = 단일 최강 딜 | 단일전 특화 (광역 없음) |
| 쉴드/도발로 보호받으면 매 턴 폭딜 가능 | 파티 보호 실패 시 일반 도적으로 전락 |

**DesignPillars 약점 유형**: **자원 의존** (Shadows 유지가 곧 화력). 추가로 **파티 의존** (혼자서는 Shadows 유지 불가 — 적 공격을 돌릴 수단 없음).

## 5. 고유 메카닉: Shadows (그림자)

### 작동 규칙

```
[매 턴 종료 시 평가]
    ├── Umbra가 이번 턴 피해를 1도 받지 않음
    │   (쉴드 흡수로 HP 손상 0인 경우도 "안 맞음" 인정)
    │   → Shadows +1 (최대 3)
    │
    └── Umbra가 피해를 받음
        (HP 직접 손상 OR 도트 틱 Poison/Burn/Bleed)
        → Shadows = 0 (즉시 리셋)
```

### "피해"의 정의 (중요)

| 상황 | 판정 | 비고 |
|------|------|------|
| 적 직접 공격 → HP 감소 | 🔴 맞음 (리셋) | 기본 |
| 적 직접 공격 → 쉴드 흡수 (HP 0 손상) | 🟢 안 맞음 | **Healer Holy Shield 가치 폭발** |
| 도트 디버프 틱 (Poison/Burn/Bleed) | 🔴 맞음 (리셋) | "완전 무상태" 요구 → Purify 가치 상승 |
| 자해 (Ashe Ember 등) | 🔴 맞음 (리셋) | Umbra 본인 자해 시 리셋 (Umbra 자해 스킬 없지만 향후 추가 시) |
| 힐/버프 받음 | 🟢 영향 없음 | 회복은 "피해" 아님 |

### Shadows 보너스 (치명타 시스템 연동)

| Shadows | 치명타 확률 | 치명타 피해 배율 | 비고 |
|---------|-----------|----------------|------|
| 0 | 0% (기본) | 1.5× (기본) | 일반 도적. 약함 |
| 1 | +30% | 2.0× | 가벼운 그림자 |
| 2 | +60% | 2.5× | 깊은 그림자 |
| 3 | **100% (확정)** | **3.0×** | 완벽한 암살 기회 |

**적용 범위**: Umbra의 모든 Attack 스킬에 적용 (Poison Blade/Backstab/Rupture/Eviscerate 전부).

### 서사-메카닉 정합성

- "오래 숨을수록 깊은 그림자" → 연속 보호가 보상
- "한 번 피를 흘리면 그림자가 깨진다" → 1회 피격 = 전부 상실
- "동료가 그림자를 지켜준다" → 파티 보호가 Umbra의 화력

### Ashe와의 서사적 대칭

- **Ashe (Ember)**: 자해할수록 강해진다 (능동적 자원)
- **Umbra (Shadows)**: 안 맞을수록 강해진다 (수동적 + 파티 의존 자원)
- 같은 파티에 두면 힐러/탱커 자원 분배 퍼즐 — DesignPillars 원칙 1(드로우 운 전략)의 핵심 사례

## 6. 스킬 4종 (4개 다른 조건)

| 스킬 | AP | 기본 효과 | 조건 | 조건 충족 보너스 |
|-----|----|---------|------|----------------|
| **Poison Blade** | 1 | 단일 3 + Poison 2턴 | (셋업 — 조건 없음) | Poison 부여 |
| **Backstab** | 2 | 단일 7 | 대상 디버프 상태 | 데미지 2배 (14) |
| **Rupture** | 1 | 단일 4 + Bleed 2턴 | 대상 HP 50% 이하 | 도트 지속 +2턴 (4턴) |
| **Eviscerate** | 3 | 단일 15 | ⚠️ **Shadows 3 필수** | 사용 후 **Shadows -1** (즉 2로 감소) |

### Shadows 보너스 적용 시 데미지 기대값

| 스킬 | 기본 | Shadows 1 (30%×2.0) | Shadows 2 (60%×2.5) | Shadows 3 (100%×3.0) |
|------|------|-------------------|---------------------|----------------------|
| Poison Blade | 3 | 3.5 | 4.5 | **9** |
| Backstab | 7 | 8.1 | 10.4 | **21** |
| Backstab (디버프) | 14 | 16.1 | 20.8 | **42** |
| Rupture | 4 | 4.6 | 6.0 | **12** (+도트) |
| **Eviscerate** | 15 | (사용 불가) | (사용 불가) | **45** |

> Eviscerate는 Shadows 3에서만 사용 가능하므로 항상 45 데미지 (치명타 확정). Backstab 디버프 시 42가 밸런스 상한.

### 조건 다양성 검증 (4.5 원칙 2)
- Poison Blade → 셋업 (조건 없음)
- Backstab → **대상 상태** (디버프)
- Rupture → **대상 HP 임계** (50%-)
- Eviscerate → **자원** (Shadows 3)

→ 4개 모두 다른 조건 ✅

### 사용 제약 조건 (Eviscerate) — 4.5 원칙 5B 예외
예외 허용 기준 3가지 모두 충족:
1. **게임 체인저급** — 45 데미지 + 치명타 = 보스 1킬 가능 (보스 HP 150-200 기준 25-30%)
2. **위력**: 일반 단일기(8) 대비 5.6배 (치명타 포함)
3. **루프 종착지** — Shadows 3 완충 = Umbra 정체성의 피크

### Eviscerate 후 Shadows -1의 의미

- Shadows 3 → Eviscerate → Shadows 2
- 다음 턴 안 맞으면 → Shadows 3 (다시 Eviscerate 가능)
- 즉 **지속적 파티 보호가 되면 매 턴 Eviscerate 가능** — 강력하지만 리스크(1회 피격 = 전부 상실)로 밸런스

### 기존 스킬 매핑
| 기존 | 신규 | 변경 |
|------|------|------|
| Rogue_Backstab (7, AP2) | Backstab | 디버프 조건부 2배 (유지) |
| Rogue_PoisonBlade (3, AP1, Poison) | Poison Blade | 거의 동일 (Poison 유지) |
| Rogue_Weaken (DefenseDown, AP1) | Rupture | Weaken → Bleed 도트로 변경. HP 임계 조건 추가 |
| Rogue_DoubleStrike (3, AP1) | Eviscerate | 단순 2회 타격 → Shadows 3 소비 finisher로 전면 재설계 |

## 7. BehaviorTag 활용

| BehaviorTag | 적용 스킬 | 효과 | 백로그 번호 | 구현 상태 |
|------------|----------|------|-----------|----------|
| `Cull` | Rupture | HP 50%- 적 위력 보너스 | 컨셉 7 | 🟢 이미 구현됨 |
| `HeavyHit` | Eviscerate | 치명타 확률 가산 | 기존 24종 | 🟢 이미 구현됨 |
| (신규) `ShadowsConsumedByUse` | Eviscerate | 사용 후 Shadows -1 | - | 🔴 신규 구현 필요 (5줄) |

> 대부분의 메카닉은 ShadowsResourceComponent + 치명타 시스템에서 처리되므로, 신규 BehaviorTag는 거의 불필요.

## 8. 장착 특성 3종 리워크

| 특성 | 기존 효과 | 리워크 효과 | 해금 |
|------|---------|------------|------|
| **암살자 본능** (기본) | 추가 고정 데미지 +2 | **Shadows 최대치 +1 (3→4)**. Shadows 4 = 치명타 피해 3.5배 | 기본 |
| **독 마스터** | 상태이상 지속 +1턴 | **도트 디버프 적에게 위력 +3** (Backstab 조건 강화) | 30 조각 |
| **회피의 대가** | 받는 피해 -2 | **Shadows 1+일 때 받는 피해 -3** (Shadows 유지 중 생존. 리셋 방지) | 60 조각 + 1 영혼 |

### 특성 시너지 (조합 가치)
- **암살자 본능** (Shadows 4) + **회피의 대가** (피해 -3) = 강력한 유지형 암살자. 단 각 특성만으로는 약점 보완 안 됨
- **독 마스터** + Backstab 디버프 2배 = 단일 딜 극대화. 다만 Poison 부여 스킬(Poison Blade 1개뿐) 의존

## 9. 밸런스 시나리오

### 시나리오 A: Duran 도발 루프 (이상적)
```
턴 1: Duran Provoking Shield (도발) → Umbra Poison Blade (3). 적이 Duran 공격. Umbra 안 맞음 → Shadows 1
턴 2: Duran 도발 유지 → Umbra Backstab (7, 디버프 14). 안 맞음 → Shadows 2
턴 3: Duran 도발 + Duran 쉴드 → Umbra Rupture (4 + Bleed). 안 맞음 → Shadows 3
턴 4: Umbra Eviscerate → 45 데미지 (치명타 확정). Shadows 2로 감소
턴 5: Duran 도발 → 안 맞음 → Shadows 3. 다시 Eviscerate 가능
```
→ 4턴째부터 매 턴 45 데미지. 보스 150 HP 기준 4턴 처치.

### 시나리오 B: 보호 실패 (현실적)
```
턴 1: Umbra Poison Blade. 적이 Umbra 공격 → 맞음 → Shadows 0
턴 2: Umbra Backstab (7). 또 맞음 → Shadows 0
턴 3: Umbra Rupture. Healer 쉴드 → 쉴드 흡수 → 안 맞음 → Shadows 1
턴 4: Umbra Backstab (치명타 30% 미발동 → 14). 또 쉴드 흡수 → Shadows 2
턴 5: 적이 Umbra 공격 → 쉴드 없음 → 맞음 → Shadows 0. Eviscerate 봉인
```
→ 보호가 불안정하면 Eviscerate 도달 불가. Backstab/Rupture로 버티는 게 현실적.

### 시나리오 C: 도트 걸림 (리셋 위험)
```
턴 1-3: 완벽 보호 → Shadows 3
턴 4: 이전 턴 적이 Poison 묻힘 → 턴 시작 도트 틱 → HP 2 손상 → 🔴 Shadows 0 리셋
       → Healer 정화 필요성 부각
```

## 10. 파티 시너지 매트릭스

| 조합 | 시너지 강도 | 핵심 |
|------|-----------|------|
| **Umbra + Duran** | ★★★ | Duran 도발 = Umbra Shadows 유지. 가장 강력한 시너지 (Duran 정체성과 완벽 부합) |
| **Umbra + Healer** | ★★★ | Holy Shield 쉴드 흡수 = "안 맞음". Purify로 도트 제거 = 리셋 방지 |
| **Umbra + Lumi** | ★★ | Freeze로 적 행동 봉쇄 → Umbra 안 맞음 |
| **Umbra + Taranis** | ★★ | Charge 네트워크로 위협 적 처치 |
| **Umbra + Archer** | ★★ | Archer 도트 → Umbra Backstab 디버프 조건. 이중 암살 |
| **Umbra + Ashe** | ⚠ 트레이드오프 | Ashe는 자해(Ember), Umbra는 안 맞아야(Shadows). 힐러 자원 분배 퍼즐 |
| **Umbra + Sibyl** | ★★ | Sibyl 1턴 뒤 힐 예약 = 다음 턴 Umbra 보호 보장 |

## 11. 🔴 선행 작업: 치명타 시스템 신규 구현

### 현재 상태
프로젝트에 치명타(critical) 시스템이 **존재하지 않음**. 기존 스킬 설명의 "치명타 데미지"는 단순 텍스트. DamageCalculator.cs에 crit 관련 코드 없음.

### 신규 구현 명세

**CharacterData / Character**:
```csharp
// CharacterData.cs (TeamLog.Characters)
[SerializeField] private float _baseCritChance = 0f;        // 기본 0%
[SerializeField] private float _baseCritDamageMul = 1.5f;   // 기본 1.5배

// Character.cs
public float CritChance { get; set; }      // 런타임 수정 가능 (Shadows 보너스)
public float CritDamageMul { get; set; }   // 런타임 수정 가능
```

**DamageCalculator.DealDamage 확장** (약 15줄):
```csharp
// 데미지 계산 후 치명타 판정
float finalDamage = calculatedDamage;
bool isCritical = false;

if (attacker.CritChance > 0f && UnityEngine.Random.value < attacker.CritChance)
{
    finalDamage = Mathf.RoundToInt(calculatedDamage * attacker.CritDamageMul);
    isCritical = true;
    OnCriticalHit?.Invoke(target, finalDamage); // VFX/플로팅 텍스트용
}
```

**기존 캐릭터 기본값**:
- 모든 캐릭터 CritChance = 0%, CritDamageMul = 1.5× (기본값 상속)
- Umbra만 ShadowsResourceComponent에서 CritChance/CritDamageMul 동적 수정

**기존 스킬 텍스트 정리** (선택):
- Rogue_Backstab "치명타 데미지" → "강한 데미지" (또는 신규 치명타 시스템과 연동)
- Archer_CriticalShot → Umbra 리워크 시 Archer에도 치명타 메카닉 부여 가능 (후속 검토)

### 구현 예상 분량
- CharacterData/Character 필드 추가: 10줄
- DamageCalculator 치명타 판정: 15줄
- ShadowsResourceComponent: 40줄 (신규)
- 총 약 65줄. Phase CC-2A 착수 전 선행 구현 권장

## 12. 구현 메모

### 신규 클래스
- `ShadowsResourceComponent` (TeamLog.Characters.Components)
  - `OnTurnEnd` 이벤트 구독 → 이번 턴 피해 여부 평가
  - `_tookDamageThisTurn` bool 플래그 (HealthComponent.OnDamageTaken에서 true)
  - Shadows 카운터 + 보너스 적용 (Character.CritChance/CritDamageMul 갱신)
  - 쉴드 흡소 판정: OnDamageTaken의 delta == 0 (또는 별도 이벤트)

### Character 확장
- `Character.CritChance`, `Character.CritDamageMul` 프로퍼티 추가
- 런타임에 ShadowsResourceComponent가 동적 갱신

### 이벤트 훅
- `HealthComponent.OnDamageTaken` (delta > 0 시) → Umbra `_tookDamageThisTurn = true`
- 도트 틱 (StatusEffectComponent에서 HP 감소) → 동일하게 OnDamageTaken 발생 (이미 그렇게 구현되어 있는지 확인 필요)

### UI
- Umbra 패널에 Shadows 게이지 (0~3). 그래픽: 검은 그림자 아이콘 × N
- Shadows 3 도달 시 게이지 붉게 펄스 (Eviscerate 가능 알림)
- 치명타 발동 시 플로팅 텍스트 강조 (기존 "크리티컬 히트" VFX 재사용 — 단 기존은 "데미지 ≥ 최대 HP 35%" 기준이었으므로 별도 트리거 필요)

## 13. 리스크와 검증

| 리스크 | 완화 |
|-------|------|
| Duran+Healer+Umbra 조합 = 매 턴 Eviscerate 45 = 사기 | (1) 파티 3자리 소모 (Umbra+Duran+Healer=3/4). 다른 딜러 1명만. (2) 어센션 시 적 HP 상승으로 45가 절대치 아님. (3) Quick Combat 시뮬레이터로 클리어율 측정 후 조정 |
| 치명타 시스템 신규 도입 = 기존 밸런스 붕괴 | 모든 기존 캐릭터 CritChance=0%로 영향 0. Umbra만 예외. 단, 향후 Archer 리워크 시 재검토 |
| 쉴드 흡소 "안 맞음" 판정의 모호함 (쉴드 1 남고 HP 손상 0 vs 쉴드 0되고 HP 손상 0) | "HP 손상 0"으로 통일 (쉴드 잔량 무관). HealthComponent에서 HP 변화량으로 판정 |
| 도트 틱 리셋이 너무 가혹 (Poison 1스택에도 리셋) | (1) Umbra 자신은 도트 못 묻힘. (2) Healer Purify 우선순위 부여. (3) 필요시 "도트 1회 틱은 허용" 완화 검토 |
| Eviscerate 45 데미지 + 매 턴 가능 (Shadows -1 정책) | Quick Combat 측정. 너무 강하면 (a) Shadows -2로 변경 (b) Eviscerate AP 3→4 상향 (c) 기본 위력 15→12 하향 |
| 파티 보호 실패 시 Umbra 완전 무용 (평타 3-7) | 기본 스킬 위력(3/7/4)로 평타 보장. Backstab 디버프 2배(14)로 어느 정도 딜. 다만 Eviscerate 봉인은 명확한 약점 |

## 14. 밸런스 튠 포인트 (Quick Combat 시뮬레이션 후 조정 대상)

| 파라미터 | 기본값 | 조정 범위 | 사유 |
|---------|-------|----------|------|
| Shadows 최대치 | 3 | 2~4 | 너무 높으면 도달 어려움, 낮으면 쉬움 |
| Shadows 3 치명타 배율 | 3.0× | 2.5~3.5× | 45 데미지 vs 보스 HP 150의 비율 |
| Eviscerate 기본 위력 | 15 | 12~20 | 치명타 시 36~60 |
| Eviscerate 사용 후 Shadows | -1 | -1~-3 | -1이면 매 턴, -3이면 한 방 쓰면 끝 |
| 도트 틱 리셋 여부 | 리셋 | 허용 옵션 | 너무 가혹 시 "1틱 허용" 완화 |

## 15. 변경 이력

| 날짜 | 변경 |
|------|------|
| 2026-07-14 | 🟢 기획 확정. Combo Point → Shadows 메카닉 전환 (사용자 제안). 치명타 시스템 신규 도입 결정. 이름 Umbra 확정 |
