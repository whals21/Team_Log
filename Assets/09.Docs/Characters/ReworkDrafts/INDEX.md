# 기존 캐릭터 리워크 후보군 — 인덱스

> **목적**: Phase CC-2 (기존 캐릭터 리워크)의 결정 대기 항목을 한 캐릭터씩 집중 분석하기 위한 초안 모음
> **작성일**: 2026-07-14
> **상위 문서**: [CharacterConceptReview.md](../../CharacterConceptReview.md), [DesignPillars.md](../../DesignPillars.md)
> **진행 방식**: 1캐릭터씩 순차 분석 → 결정 → 확정 시 [Characters] 폴더로 승격

---

## 리워크 대상 6종 요약

| # | 슬롯 | 이름 (TBD) | 핵심 메카닉 | 약점 유형 | 문서 | 상태 |
|---|------|-----------|------------|---------|------|------|
| 1 | Healer | TBD, the Healer | Life Bond (신성 에너지) | 자기 위험 | [01_Healer.md](01_Healer.md) | 🔴 초안 |
| 2 | Rogue | **Umbra, the Rogue** | **Shadows** (파티 보호형) | 자원 의존 + 파티 의존 | [02_Rogue.md](02_Rogue.md) | 🟢 확정 |
| 3 | Archer | TBD, the Archer | Hunter's Mark | 역할 특화 | [03_Archer.md](03_Archer.md) | 🔴 초안 |
| 4 | Necromancer | TBD, the Necromancer | Soul + 미니언 | 자원 효율 | [04_Necromancer.md](04_Necromancer.md) | 🔴 초안 |
| 5 | Alchemist | TBD, the Alchemist | Reagent Reaction | 자원 효율 | [05_Alchemist.md](05_Alchemist.md) | 🔴 초안 |
| 6 | Bard | TBD, the Bard | Rhythm | 자연 제한 | [06_Bard.md](06_Bard.md) | 🔴 초안 |

**상태 범례**: 🔴 초안 → 🟡 논의 중 → 🟢 확정 → ⭐ [Characters/] 승격

---

## 제외 대상 (이미 대체 완료)

| 기존 슬롯 | 대체 캐릭터 | 비고 |
|----------|-----------|------|
| Warrior | **Duran, the Warrior** | Phase CC 코드 구현 완료. 구 `Char_Warrior.asset` 제거 대상 |
| Mage | **Ashe, the Pyromancer** + **Lumi, the Cryomancer** | Mage 만능 컨셉 4.5 원칙 위반 → 화염/냉기 전문화 분할 |

> Warrior/Mage는 본 리워크 대상에서 **제외**. 구 에셋은 CC-2E(가비지 컬렉션)에서 정리.

---

## 진행 워크플로우

```
[1캐릭터 선택]
    ↓
[초안 문서 검토 — 사용자]
    ↓
[결정 대기 항목 논의 — 사용자 + AI]
    ↓
[기획 확정 — 사용자 승인]
    ↓
[문서 승격 — ReworkDrafts → Characters/[Name]_the_[Class].md]
    ↓
[코드 구현 — 별도 Phase CC-2X]
```

---

## 공통 설계 원칙 (모든 캐릭터 준수)

### DesignPillars 3대 원칙
1. **드로우 운을 전략으로** — 만능 스킬 금지, 4스킬은 서로 다른 조건
2. **약점-보조 구조** — 명확한 약점 1+ 보조 수단 2+ (완전 제거 금지)
3. **강점 명확성** — 정체성 한 문장 정의

### CharacterConceptReview 4.5 원칙
1. 단일 조건 (다중 조건 금지)
2. 4개 스킬 = 4개 다른 조건 (매 턴 다른 퍼즐)
3. 만능 스킬 금지
4. 셋업-소비 분리
5. **강화 조건 기본** (항상 사용 가능 + 조건 시 보너스) / 사용 제약 조건은 예외 (게임 체인저급 + 3배 위력 + 루프 종착지)

### 자원 메카닉 설계 기준
- 자원 획득/소비 루프 명확 (충전 스킬 2 + 소비 스킬 2 권장)
- 자원 없어도 스킬은 기본 역할 수행 (강화 조건)
- 자원 비례 위력은 `ResourcePowerPerStack` 필드로 처리
- 최대 스택 캡 필수 (무한 스노우볼 방지)

---

## 캐릭터별 우선순위 (권장)

| 순서 | 캐릭터 | 사유 |
|------|--------|------|
| 1 | **Rogue** | BehaviorTag 기반 (FollowUp/Cull/Bounty). 자원 인프라 최소. 가장 구현 용이 |
| 2 | **Archer** | Mark 상태이상 기반. BehaviorTag 다수 활용. Rogue 다음으로 간단 |
| 3 | **Healer** | 신규 자원 1종 (신성 에너지). CharacterResourceComponent 확장 필요 |
| 4 | **Bard** | 신규 자원 1종 (Rhythm). usesThisBattle 인프라 활용 |
| 5 | **Alchemist** | 시약 반응 추적 인프라 (행동 이력). UI 순차 타겟 (Echo) 필요 |
| 6 | **Necromancer** | 미니언 소환 인프라 (별도 시스템). 가장 복잡 — 마지막 |

---

## 각 캐릭터별 결정 대기 항목 요약

각 문서의 "🔴 결정 대기 항목" 섹션 참조. 주요 공통 결정 항목:

1. **이름 확정** (각 캐릭터 TBD)
2. **자원 수치 밸런스** (최대 스택, 획득/소모량)
3. **스킬 위력** (기본 vs 보너스 비율)
4. **장착 특성 재설계** (기존 특성 → 자원 메카닉 연동)
5. **BehaviorTag 우선순위** (신규 구현 vs 기존 24종 활용)
6. **시너지 매트릭스 검증** (신규 5종과 중복 회피)

---

## 완성된 캐릭터 (참고용 — 이미 구현됨)

- [Ashe, the Pyromancer](../Ashe_the_Pyromancer.md) — Ember 자해 폭딜
- [Duran, the Warrior](../Duran_the_Warrior.md) — Vengeance 복수 게이지
- [Lumi, the Cryomancer](../Lumi_the_Cryomancer.md) — Frost 통제
- [Sibyl, the Oracle](../Sibyl_the_Oracle.md) — Prophecy 1턴 뒤 발동
- [Taranis, the Stormcaller](../Taranis_the_Stormcaller.md) — Charge 네트워크

---

## 변경 이력

| 날짜 | 변경 |
|------|------|
| 2026-07-14 | 최초 작성. 6종 초안 동시 등록 |
