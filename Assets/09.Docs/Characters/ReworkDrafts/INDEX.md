# 기존 캐릭터 리워크 후보군 — 인덱스

> **목적**: Phase CC-2 (기존 캐릭터 리워크)의 결정 대기 항목을 한 캐릭터씩 집중 분석하기 위한 초안 모음
> **작성일**: 2026-07-14
> **상위 문서**: [CharacterConceptReview.md](../../CharacterConceptReview.md), [DesignPillars.md](../../DesignPillars.md)
> **진행 방식**: 1캐릭터씩 순차 분석 → 결정 → 확정 시 [Characters] 폴더로 승격

---

## 리워크 대상 6종 요약

| # | 슬롯 | 이름 (TBD) | 핵심 메카닉 | 약점 유형 | 문서 | 상태 |
|---|------|-----------|------------|---------|------|------|
| 1 | Healer | **Elara, the Healer** | **Mercy** (회복의 연결고리, 힐→버프 순환) | 순수 서포터 (딜 0) | [01_Healer.md](01_Healer.md) | 🟢 확정 |
| 2 | Rogue | **Umbra, the Rogue** | **Shadows** (파티 보호형) | 자원 의존 + 파티 의존 | [02_Rogue.md](02_Rogue.md) | 🟢 확정 |
| 3 | Archer | **Aster, the Archer** | **Combo** (연속 사격, Umbra 정반대) | 자원 의존 + 자원 획득 조건 엄격 | [03_Archer.md](03_Archer.md) | 🟢 확정 |
| 4 | Necromancer | **Mortis, the Necromancer** | **Summoned Corpse** (동적 스킬 풀 + 자동 전투) | 간접 딜러 (본인 딜 약함) | [04_Necromancer.md](04_Necromancer.md) | 🟢 확정 |
| 5 | Alchemist | **Cael, the Alchemist** | **Discover** (하스스톤 발견 — 스킬별 3개 랜덤 선택지) | 랜덤 의존 + UI 복잡 | [05_Alchemist.md](05_Alchemist.md) | 🟢 확정 |
| 6 | Bard | **Calliope, the Bard** | **Melody** (주/부 선율 메아리, 매 턴 2효과 동시) | 자연 제한 (리듬 다양성 강제) | [06_Bard.md](06_Bard.md) | 🟢 확정 |

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
| 2026-07-16 | Archer 컨셉 후보 논의 → **D. Combo 채택** (Umbra 정반대 축). A(Mark 단독)/B(Focus+Mark)/C(Focus 단독) 후보 탈락. Quiver 백로그 추가 (아래) |

---

## 자원 메카닉 백로그 (참고 — 향후 신규 캐릭터/리워크 시 검토)

> 기획 단계에서 검토했으나 채택되지 않은 자원 메카닉 아이디어. 새 캐릭터 설계 시 참조.

### Quiver (한정 화살) — 2026-07-16 Archer 후보 E에서 보류
> "한 전투당 N발의 완벽한 화살 — 매 발이 전략적 선택"

- **컨셉**: 전투 시작 시 화살 7개 (단일 3 / 관통 2 / 광역 2처럼 종류별). 스킬 사용 시 화살 1 소모. 화살 0이면 기본 공격만 가능
- **게임감**: StS 아이리스 독, 발라토 매치 카드와 유사 — 매 전투가 퍼즐 (언제 퍼부을지 아낄지)
- **독창성**: 매우 높음 (로그라이크 자원 관리 극대화)
- **구현 복잡도**: ★★★★☆ — UI(화살 카운터), 화살 종류 관리, 매 전투 리셋 로직
- **사용 후보**: 신규 캐릭터 (예: Bombardier/Area Denial 컨셉) 또는 Necromancer 리워크 (미니언을 화살처럼 한정 자원화)
- **검증 필요**: 화살 고갈 시 플레이어 좌절감, 전투당 7개 수치 밸런스

### Predator (추적) — 2026-07-16 Archer 후보 F에서 보류
> "상처 입힌 적은 끝까지 쫓는다"

- **컨셉**: 적 처치 시 Hunt +1 (영구, 최대 5). 한 번 때린 적에게 추적 표식 → 매 턴 자동 추가 도트
- **차별화**: Taranis(Charge 광역 연쇄) vs Predator(단일 지속 추적)
- **사용 후보**: 신규 캐릭터 (Bounty Hunter/Tracker 컨셉)

### Trick Arrows (함정 화살) — 2026-07-16 Archer 후보 G에서 보류
> "화살이 박힌 자리가 곧 죽음의 자리"

- **컨셉**: 빗나간 화살이 바닥에 박혀 지뢰처럼 작동. 적이 지나갈 때 폭발
- **구현 복잡도**: ★★★★★ — Unity 타일/필드 시스템 재설계 필요 (현재 전투 필드는 절대좌표 기반 아님)
- **판정**: 현재 인프라로 구현 불가. **부결**
