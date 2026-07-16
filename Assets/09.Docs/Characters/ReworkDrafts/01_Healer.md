# [DRAFT → 확정 예정] Elara, the Healer — "회복의 연결고리"

> **상태**: 🟢 확정 (2026-07-17 Mercy 컨셉 + 핵심 결정 완료 — 코드 구현 대기)
> **슬롯**: Healer (기존 Char_Healer 리워크)
> **상위 문서**: [INDEX.md](INDEX.md), [CharacterConceptReview.md 5.6](../../CharacterConceptReview.md)
> **기존 특성 파일**: `Trait_Healer_Blessing/PureHeal/DivineShield.asset`
> **기존 스킬 에셋**: `Healer_Heal/Barrier/Purify/Blessing.asset`

### 2026-07-17 확정 사항 (사용자 제안 + 결정)
- **이름**: Elara (켈트 "빛의 여신")
- **자원**: Mercy (회복의 연결고리) — 파티원별 회복량 추적
- **자동 힐 위력**: 3 (매 턴 시작 시 연결된 파티원에게)
- **버스트 임계값**: 15 누적 회복 시 자동 발동
- **버스트 버프**: 고정 ATK+3 (3턴 지속)
- **버스트 대상**: 가장 많이 회복받은 파티원 (기여도 보상)

---

## 1. 정체성 (한 문장)

> **"파티에 회복의 연결고리를 만들고, 파티원이 행동할 때마다 연결고리에서 힐이 흘러나온다 — 그 회복이 쌓이면 축복이 내려진다. 힐과 버프의 영원한 순환."**

## 2. 이름

**Elara** (켈트 "빛의 여신", 그리스 "따뜻한 빛") — 서구권 힐러 이름으로 친숙, 한국어 "엘라라" 발음 부드러움

## 3. 역할군

- **주 역할군**: 파티 서포터 (연결고리 기반 지속 힐 + 자동 버프)
- **부 역할군**: 단일 위기 치유 (Mend Wounds 즉시 힐)

## 4. 강점 / 약점

| 강점 | 약점 |
|------|------|
| 매 턴 자동 힐 (파티원 행동 보상) | **연결고리 유지 시 Healer 본인 AP 소모** (AP 1 소비) |
| Mercy 자동 버스트 (파티 ATK+3 영구 순환) | 단일 딜 능력 0 (순수 서포터) |
| Umbra/Duran/Ashe/Aster 모두와 시너지 | **적 도트 폭격에 취약** (자동 힐 3으로 부족) |
| 부활 페널티 완화 (CC-0 자동 힐로 사망자 회복 보조) | 보스전 단일 딜러 처치 느림 (딜 보조 필요) |

**DesignPillars 약점 유형**: **순수 서포터 (딜 0)** — 딜러 파티 구성 필수

## 5. 고유 메카닉: Mercy (회복의 연결고리)

### 자원 구조
- **Mercy (Healer 본인)**: 전투 총 누적 회복량. MaxStacks=15
- **파티원별 회복량 추적**: MercyResourceComponent 내부 `Dictionary<Character, int>`

### 핵심 루프
```
[전투 시작] → Healer가 파티 전체에 연결고리 부여 (자동)
[매 턴 시작] → 연결된 파티원 각자에게 자동 힐 3
              → Mercy +3 (모든 자동 힐이 Mercy 축전)
[Healer 힐 스킬 시전] → 대상 큰 힐 + Mercy +N (힐량만큼)
[Mercy 15 도달] → 자동 버스트: 가장 많이 회복받은 파티원에게 ATK+3 (3턴)
                → Mercy 0 리셋, 해당 파티원 회복량도 리셋
[버프 받은 파티원] → 더 강해짐 → 더 활동적 → 더 많은 자동 힐 → 순환
```

### 기존 7종 자원과의 차별화
| 자원 | 축전 패턴 | 본질 |
|------|----------|------|
| Ember/Vengeance/Shadows/Combo | 개인 행동/피해 기반 | 딜러/탱커 |
| Prophecy/Charge | 시간/공간축 | 서포터/제어 |
| **Mercy** | **"누구를 위해 행동했나" (파티원별 추적)** | **보호 서포터** ⭐ |

→ 완전히 새로운 축. "파티원 한 명 한 명에게 얼마나 도움을 주었나"를 자원화

## 6. 스킬 4종 (4개 다른 조건)

| 스킬 | AP | 기본 효과 | 조건 | Mercy 효과 |
|-----|----|---------|------|-----------|
| **Bond Link** (연결의 끈) | 1 | 단일 연결고리 강화 — 대상은 이번 턴 자동 힐 3 → 6으로 강화 | 셋업 | (셋업 전용) |
| **Mend Wounds** (상처 치유) | 2 | 단일 즉시 힐 10 + 도트 정화 | 대상 부상 (HP<70%) | Mercy +10 (힐량만큼) |
| **Blessing of Mercy** (자비의 축복) | 2 | 단일 즉시 ATK+3 버프 (3턴) | 자원 소모 (Mercy 5+) | Mercy -5 |
| **Sanctuary** (성소) | 3 | 광역 힐 8 + 모든 파티원 연결고리 강화 (다음 턴 자동 힐 6) | 자원 임계 (Mercy 8+) | Mercy -8, 각 파티원 +8 누적 |

### 조건 다양성 검증 (4.5 원칙 2)
- Bond Link → 셋업 (조건 없음)
- Mend Wounds → **대상 상태** (부상 HP<70%)
- Blessing of Mercy → **자원 소모** (Mercy 5+)
- Sanctuary → **자원 임계** (Mercy 8+)

→ 4개 모두 다른 조건. 매 턴 다른 퍼즐. ✅

### Mercy 버스트 (자동, 별도 스킬 아님)
- Mercy 15 도달 시 자동 발동
- 대상: **이번 전투에서 가장 많이 회복받은 파티원** (기여도 보상)
- 효과: ATK+3 버스트 (3턴)
- 이후: Mercy 0 리셋 + 해당 파티원 회복량 추적 0 리셋
- **하이브리드 모드**: 가장 많이 회복받은 파티원이 이미 ATK+3 활성 중이면 → 다음 우선순위 파티원에게 버스트

### 기존 스킬 매핑
| 기존 | 신규 | 변경 |
|------|------|------|
| Healer_Heal (12, AP2) | Mend Wounds | 12→10, 조건부 정화 추가 (Purify 통합) |
| Healer_Barrier (8, AP1) | Bond Link | 쉴드 제거 → 연결고리 강화 (자동 힐 3→6) |
| Healer_Purify (0, AP1) | (제거 — Mend Wounds에 통합) | 정화가 힐과 결합 |
| Healer_Blessing (ATK+2, AP2) | Blessing of Mercy | Mercy 5 소모 추가, 위력 +2→+3 |
| (신규) | Sanctuary | 광역 힐 + 연결고리 강화 |

## 7. BehaviorTag 활용

| BehaviorTag | 적용 스킬 | 효과 | 상태 |
|------------|----------|------|------|
| **`CleanseLowTarget`** | Mend Wounds | 대상 HP 50%- 시 Burn/Poison 정화 | **이미 구현됨** (Ashe Phoenix Renewal 재사용) |
| **신규: `MercyAccumulate`** | Mend Wounds/Sanctuary | 시전자 Mercy +N (힐량만큼) | **신규 구현 필요** (PostApply Phase, ~10줄) |
| **신규: `MercyConsume`** | Blessing of Mercy/Sanctuary | Mercy N 소모, 소모 실패 시 스킬 실패 | **신규 구현 필요** (PowerModify Phase, ~15줄) |
| **신규: `BondLinkBoost`** | Bond Link | 대상의 자동 힐 위력 증가 (상태이상 형태) | **신규 구현 필요** (StatusEffectType.BondBoost 추가) |

### 자동 힐 + 자동 버스트는 MercyResourceComponent에서 처리
- `OnTurnStart`: 연결된 파티원에게 Health.Heal(3). 각 파티원 회복량 추적 + Mercy 축전
- Mercy 15 도달 시 자동 버스트: `CombatEventBus.FireHealApplied` 후 버스트 로직

## 8. 장착 특성 3종 리워크

| 특성 | 기존 효과 | 리워크 효과 | 해금 |
|------|---------|------------|------|
| **축복** (기본) | 힐 효과 +15% | **자동 힐 위력 +2 (3→5)** (연결고리 효율 증가) | 기본 |
| **순수 치유** | 적 처치 시 HP +3 | **Mend Wounds 정화 시 Mercy +3 추가** (정화 인센티브) | 30 조각 |
| **신성 방패** | 힐 적용 시 쉴드 +2 | **Mercy 버스트 범위 확장** (단일 → 부상자 2명에게 버스트) | 60 조각 + 1 영혼 |

### 특성 키워드 매핑 (CharacterTraitHandler에서 처리)
| 특성 | KeywordType | Trigger | Value |
|------|------------|---------|-------|
| 축복 | **`AutoHealBonus`** (신규) | Passive | 2 |
| 순수 치유 | **`MercyCleanseBonus`** (신규) | Passive | 3 |
| 신성 방패 | **`MercyBurstTargets`** (신규) | Passive | 2 (버스트 대상 수) |

## 9. 밸런스 시나리오 (보스전 5턴 — 파티: Elara/Ashe/Duran/Umbra)

```
턴 1: Bond Link (Duran 강화) → 자동 힐 6 (Duran). Mercy 0→6
턴 2: Mend Wounds (Ashe, 자해 후) → 10 힐 + 정화. Mercy 6→16 → 버스트!
       가장 많이 회복받은 Ashe에게 ATK+3 (3턴). Mercy 0
턴 3: Sanctuary (광역) → 8 힐×4명=32 + Mercy +32 → 버스트 2회 (32/15=2)
       Duran, Umbra에게 각각 ATK+3
턴 4: Blessing of Mercy (Duran, Mercy 17 소모 5) → ATK+3 (이미 버스트 中 → 갱신)
턴 5: Mend Wounds (Umbra) → 10 힐. Mercy 12→22 → 버스트! Ashe에게 ATK+3 재갱신
```

**비교 — Umbra 5턴 평균**:
```
Umbra (Healer 없음): 안 맞으며 Poison Blade/Backstab/Rupture/Eviscerate → 52 데미지
Umbra (Healer 있음): Healer의 자동 힐로 안전 + ATK+3 버스트로 데미지 +30% → 68 데미지
```

→ Healer가 직접 딜은 0이지만, 파티 딜량 증대 + 생존력 향상으로 **실제 파티 DPS는 약 30~50% 증가**

## 10. 파티 시너지 (자원 메카닉 자체가 시너지 창출)

| 조합 | 시너지 | 핵심 |
|------|-------|------|
| **Elara + Umbra** | ★★★ | Umbra 매 턴 스킬 쓸 때 자동 힐 3 → Umbra는 "안 맞음" 유지 + Mercy 축전 → Umbra에게 ATK+3 버스트. **쌍방 강화 루프** ⭐ |
| **Elara + Duran** | ★★★ | Duran 도발 후 매 턴 자동 힐 → 탱킹 보조 + Mercy 축전 → Duran ATK 버스트 (적 도발 중에도 딜 가능) |
| **Elara + Ashe** | ★★★ | Ashe 자해 후 자동 힐로 상쇄 + Mercy 축전 가속 → 자해 하이리스크 하이리턴 극대화 |
| **Elara + Aster** | ★★★ | Aster 매 턴 스킬 쏨 → 자동 힐 + Mercy → Aster의 Combo와 Mercy ATK 버프 이중 효과 |
| **Elara + Sibyl** | ★★ | Sibyl Prophecy 스킬도 행동으로 카운트 → Mercy 축전. 시간축+보호축 시너지 |

## 11. ✅ 결정 항목 (2026-07-17 확정)

- [x] **이름**: Elara (켈트 "빛의 여신")
- [x] **자원 이름**: Mercy (회복의 연결고리)
- [x] **자원 구조**: 단일 Mercy (15 도달 시 버스트) + 파티원별 회복량 추적 (Dictionary)
- [x] **자동 힐 위력**: 3 (매 턴 시작 시 연결된 파티원)
- [x] **버스트 임계값**: 15 회복 누적
- [x] **버스트 버프**: 고정 ATK+3 (3턴)
- [x] **버스트 대상**: 가장 많이 회복받은 파티원
- [x] **연결고리 부여**: 전투 시작 시 자동 (파티 전체). Bond Link 스킬로 강화
- [x] **Purify 제거**: Mend Wounds에 통합
- [x] **특성 3종**: 축복(자동 힐 +2)/순수 치유(정화 시 Mercy +3)/신성 방패(버스트 2명)

## 12. 리스크와 검증

| 리스크 | 완화 |
|-------|-------|
| 자동 힐 3 + 자동 버스트가 사기일 수 있음 | Healer 본인 AP 소모 (Bond Link 1, Mend Wounds 2 등). 딜 0으로 파티 딜 비중 낮춤 |
| Mercy 15 도달 너무 빠를 위험 (3턴만) | 자동 힐 3×4명=12/턴은 이론상이고, 실제로는 모든 파티원이 매 턴 행동하지 않음. 도트/사망 시 회복량 제한 |
| 복잡한 파티원별 추적이 구현 까다로움 | MercyResourceComponent 내부 Dictionary로 단순화. UI는 총 Mercy만 표시 |
| 단일 딜 0인데 보스전에서 시간 부족 | BalanceSimulator로 Healer 포함 파티 승률 측정 (Quick Combat 1000팩) |
| 부활 페널티(CC-0)와 시너지 | 사망자 부활 후 자동 힐 3×N턴으로 회복 보조. CC-0 페널티 완화에 기여 |
| 고정 ATK+3 버스트가 상황 안 바뀌어 단조 | "신성 방패" 특성으로 부상자 2명 버스트로 변형. 전략적 선택지 |

## 13. 구현 메모 (코드 구현 시 — 별도 Phase CC-2C)

### 신규 코드 필요
1. **MercyResourceComponent** (`Characters/Components/`)
   - `OnTurnStart`: 연결된 파티원에게 Health.Heal(3). 파티원별 추적 + Mercy 축전
   - Mercy 15 도달 시 자동 버스트: 가장 많이 회복받은 파티원에게 ATK+3 (3턴)
   - `Dictionary<Character, int> _healingReceivedByMember` 파티원별 추적
   - `OnHealApplied` 이벤트 구독 — Healer 본인이 힐 시전 시 Mercy +힐량
2. **StatusEffectType.BondBoost** 신규 — Bond Link 강화 대상 표식 (자동 힐 3→6)
3. **BehaviorKeyword.MercyAccumulate / MercyConsume / BondLinkBoost** 신규
4. **KeywordType.AutoHealBonus / MercyCleanseBonus / MercyBurstTargets** 신규
5. **ResourceType.Mercy** enum 추가
6. **Character.CreateResource** Mercy 분기 추가
7. **CharacterTraitHandler** 확장 — AutoHealBonus(OnTurnStart에서 자동 힐 위력 가산), MercyCleanseBonus(정화 시 Mercy 추가), MercyBurstTargets(버스트 대상 수)

### 기존 코드 수정
- `DataGenerator.PhaseCC.cs` — Elara 스킬 4종 + Char_Healer 재생성 (ResourceType.Mercy)
- `DataGenerator.Traits.cs` — Healer 특성 3종 리워크 키워드 적용
- `TurnManager.StartNewTurn` — 매 턴 시작 시 Healer의 MercyResourceComponent.OnTurnStart 호출 (이미 일반적 OnTurnStart 루프 있음)
- `BattleDisplayUtil.cs` — Mercy 자원 색상 토큰 (따뜻한 노랑/황금 계열) + 라벨/설명 추가
- `UIPalette` — Mercy 색상 (0.95, 0.85, 0.30 황금)

### 구현 난이도 추정
- MercyResourceComponent: **중상** (~80줄, Dictionary 추적 + 자동 버스트 로직)
- Behavior 3종: 중간 (~40줄)
- 특성 키워드 3종: 낮음 (~25줄)
- DataGenerator/UI: 낮음 (~40줄)
- **총합**: 약 185줄 + 4 스킬 .asset 재생성
- Umbra/Aster보다 약간 큰 규모 (Dictionary 추적 + 자동 버스트가 복잡)

### 테스트 계획 (PhaseCC2CTests.cs 신규)
1. MercyResourceComponent: 턴 시작 자동 힐, Mercy 축전, 15 도달 자동 버스트
2. 파티원별 추적: 가장 많이 회복받은 파티원에게 버스트
3. Mend Wounds: 도트 정화 + Mercy 가산
4. Blessing of Mercy: Mercy 소모, 부족 시 실패
5. AutoHealBonus 특성: 자동 힐 3→5
6. MercyBurstTargets 특성: 버스트 대상 1→2

---

## 변경 이력

| 날짜 | 변경 |
|------|------|
| 2026-07-14 | 최초 작성 (Life Bond 컨셉, 🔴 초안) |
| 2026-07-17 | **Mercy(회복의 연결고리) 컨셉으로 전면 재작성** (사용자 제안). 힐→버프 순환 메카닉. 🟢 확정 |
