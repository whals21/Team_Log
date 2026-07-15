# [DRAFT] TBD, the Bard — "리듬과 화음의 지휘자"

> **상태**: 🔴 초안 (결정 대기)
> **슬롯**: Bard (기존 Char_Bard 리워크)
> **상위 문서**: [INDEX.md](INDEX.md), [CharacterConceptReview.md 5.11](../../CharacterConceptReview.md)
> **기존 특성 파일**: `Trait_Bard_BattleSong/CourageChord/HealingMelody.asset`

---

## 1. 정체성 (한 문장)

> **"곡이 쌓여 피날레로 터지는 지휘자. 파티 버프의 정점."**

## 2. 이름 후보

| # | 이름 | 어원/뉘앙스 | 장단점 |
|---|------|-----------|--------|
| **A** | **Calliope** | 그리스 무신 "서사시의 여신" | 서사 강함, 길다 |
| B | Orpheus | 그리스 음악 영웅 | 강렬, 남성형 |
| C | Lyra | 거문고자리 | 짧고 우아, Archer 후보와 충돌 |
| D | Aria | "독창곡" | 직관, 평범 |
| E | Melody | "선율" | 직관, 진부 |

**추천**: `Calliope` (A) — 음악 신 서사. 한국어 "칼리오페" 발음. 약간 길지만 독특

## 3. 역할군

- **주 역할군**: 파티 버퍼 (광역 AtkUp/DefUp/힐)
- **부 역할군**: 광역 디버퍼 (Dissonance 적 AtkDown+DefDown)

## 4. 강점 / 약점

| 강점 | 약점 |
|------|------|
| 광역 버프/디버프로 파티 전체 강화 | 턴당 스킬 밀도 부족 (자연 제한 약점) |
| 리듬 4 피날레 = 다음 곡 3배 | AP 2~4 고비용 (특히 Grand Finale AP 4) |
| Courage Chord로 힐+곡 효과 동시 | 단일 힐이라 광역 회복 약함 |

**DesignPillars 약점 유형**: **자연 제한** (턴당 스킬 밀도 부족)

## 5. 고유 메카닉: Rhythm (리듬)

```
[Bard 스킬(곡) 사용] → 리듬 +1 (최대 4)
[리듬 4 도달] → "피날레" 모드 (다음 곡 효과 3배, 자동 소모)
[곡 미사용 턴] → 리듬 절반 유지 (반올림)
[리듬 유지 중] → 턴 종료 시 파티 HP +N 회복 (치유 멜로디 특성)
```

**핵심 루프**: Battle Song(리듬+1) → Dissonance(리듬+1) → Courage Chord(리듬+1) → Battle Song(리듬 4+1 = 피날레) → Grand Finale(리듬 4 소모, 다음 턴 파티 버프). 곡 사용이 셀프 시너지.

## 6. 스킬 4종 (4개 다른 조건)

| 스킬 | AP | 기본 효과 | 리듬 | 조건 | 조건 충족 보너스 |
|-----|----|---------|------|------|----------------|
| **Battle Song** | 2 | 광역 AtkUp (2턴) | +1 | (셋업 — 조건 없음) | AtkUp 부여 |
| **Dissonance** | 2 | 광역 AtkDown (2턴) | +1 | 적 3마리+ | 추가 DefDown |
| **Courage Chord** | 2 | 단일 힐 8 | +1 | 대상 곡 효과 받는 중 | 힐량 2배 (16) |
| **Grand Finale** | 4 | 광역 버프+힐 | **전부 소모 → 0** | ⚠️ 리듬 4 필수 | 다음 턴 파티 AtkUp+3, DefUp+3 (영구? 1턴?) |

### 조건 다양성 검증 (4.5 원칙 2)
- Battle Song → 셋업 (조건 없음)
- Dissonance → **적 수** (3마리+)
- Courage Chord → **대상 상태** (곡 효과 받는 중)
- Grand Finale → **자원** (리듬 4)

→ 4개 모두 다른 조건. ✅

### 사용 제약 조건 (Grand Finale)
예외 허용 기준 3가지 모두 충족:
1. **게임 체인저급** — 다음 턴 파티 AtkUp+3+DefUp+3 = 파티 전체 강화
2. **다중 강화** — 버프+힐+디버프 다중 효과
3. **루프 종착지** — 리듬 4 완충이라는 Bard 정체성의 피크

### 기존 스킬 매핑
| 기존 | 신규 | 변경 |
|------|------|------|
| Bard_BattleSong (광역 AtkUp, AP2) | Battle Song | 리듬 +1 추가 |
| Bard_WeakenMelody (광역 AtkDown, AP2) | Dissonance | 적 수 조건부 DefDown 추가 |
| Bard_CourageChord (힐 8, AP2) | Courage Chord | 곡 효과 조건부 2배 힐 |
| Bard_Blessing (광역 DefUp, AP3) | Grand Finale | DefUp → 버프+힔 다중 / AP 3→4 / 리듬 4 필요 |

## 7. BehaviorTag 활용

| BehaviorTag | 적용 스킬 | 효과 | 백로그 번호 |
|------------|----------|------|-----------|
| `Momentum` | Battle Song | 매 사용 AtkUp 위력 +1 (캡 +3) | 컨셉 9 (이미 구현됨) |
| `Mastery` | Courage Chord | 매 사용 cost -1 (최소 1) | 컨셉 14 (이미 구현됨) |
| `LimitBreak` (후보) | Grand Finale | 전투당 1회 (대안 조건) | 컨셉 19 (이미 구현됨) |
| `AllIn` (후보) | Grand Finale | 사용 후 AP 0 시 보너스 | 컨셉 16 (이미 구현됨) |

## 8. 장착 특성 3종 리워크

| 특성 | 기존 효과 | 리워크 효과 | 해금 |
|------|---------|------------|------|
| **전투 노래** (기본) | 매 턴 AP +1 | **리듬 획득량 +1** (스킬당 2 리듬, 빠른 피날레) | 기본 |
| **용기의 화음** | 전투 시작 ATK +2 | **피날레 효과 +1턴 연장** (피날레 가치 극대화) | 30 조각 |
| **치유 멜로디** | 매 턴 종료 HP +2 | **곡 효과 받는 아군 매 턴 HP +3** (버프받은 아군 지속 힐) | 60 조각 + 1 영혼 |

## 9. 밸런스 시나리오 (엘리트전 예시)

```
턴 1: Battle Song → 광역 AtkUp. 리듬 1
턴 2: Dissonance (적 3마리+) → AtkDown + DefDown. 리듬 2
턴 3: Courage Chord (AtkUp 받은 아군) → 16 힐 (2배). 리듬 3
턴 4: Battle Song → AtkUp 갱신. 리듬 4 (피날레 모드)
턴 5: Grand Finale (리듬 4 소모) → 광역 버프+힐. 다음 턴 파티 AtkUp+3, DefUp+3
턴 6: 다른 아군 폭딜 (Bard 버프로 스케일)
```

## 10. 파티 시너지

| 조합 | 시너지 | 핵심 |
|------|-------|------|
| **Bard + Duran** | ★★★ | Bard AtkUp → Duran Vengeance 스케일. 탱커 딜 극대화 |
| **Bard + Ashe** | ★★ | AtkUp → Ashe Brand of Ash 위력 상승. 단 Ashe 자해는 Bard 힐로 보조 |
| **Bard + Rogue** | ★★ | AtkUp → Rogue Backstab+Eviscerate 폭딜 가속 |
| **Bard + Healer** | ★★ | 이중 버프/힐. Healer 영구 + Bard 일시 = 다층 강화 |
| **Bard + Alchemist** | ★★ | Alch Catalyst (ATK+4) + Bard AtkUp = 파티 ATK 스노우볼 |

## 11. 🔴 결정 대기 항목

- [ ] **이름 확정** (Calliope 추천)
- [ ] **리듬 최대치** (4 유지 vs 5 조정)
- [ ] **피날레 배수** (3배 vs 2.5배)
- [ ] **피날레 효과 범위** (다음 곡 1개만 3배 vs 다음 턴 모든 곡 3배)
- [ ] **Grand Finale 다음 턴 버프 지속 시간** (1턴 vs 영구)
- [ ] **곡 미사용 턴 리듬 소실** (절반 유지 vs 전부 유지 vs 전부 소실)
- [ ] **리듬 4 도달 시 자동 피날레 발동** (강제 vs 선택)
- [ ] **Battle Song Momentum 캡** (+3 vs +5)
- [ ] **Courage Chord Mastery 최소 cost** (0 vs 1)
- [ ] **Grand Finale AP 4** (유지 vs 3 하향)
- [ ] **기존 Bard_Blessing(DefUp) → Grand Finale** 변경 영향 — 세이브 호환

## 12. 리스크와 검증

| 리스크 | 완화 |
|-------|------|
| Momentum+Mastery 동시 = 무료 폭딜/힐 루프 | Momentum은 AtkUp 위력(지속시간 아님), Mastery는 최소 cost 1 |
| 피날레 3배 = Mass 효과 사기 (광역 AtkUp×3) | 피날레는 다음 곡 1개만. Grand Finale과 중복 안 됨 |
| Grand Finale AP 4 = 매 5턴마다 1번 = 부족 | 리듬 4 도달이 4턴 걸리므로 자연 제한. 용기의 화음 특성으로 연장 |
| 곡 미사용 턴 소실 = 도트/적 처치 턴에 페널티 | 절반 유지로 완화. 1턴 쉬어도 2 리듬 남음 |
| Battle Song 매 턴 사용 = 단조로움 | Momentum으로 위력 변동. 다른 곡 선택 유도 |
| Courage Chord Mastery cost 0 = 무한 힐 | 최소 cost 1 강제. 매 턴 1회만 |

## 13. 구현 메모

- `CharacterResourceComponent` 서브클래스: `RhythmResourceComponent`
- `OnSkillUsed` 이벤트 훅에서 리듬 +1 (곡 스킬만 — SkillData에 `isSong` bool 또는 ID 접두사)
- 리듬 4 도달 시 자동 플래그 `_finaleReady = true`. 다음 곡 사용 시 3배 적용 후 리듬 0
- Grand Finale은 별도 — 리듬 4 소모 + 파티 버프 부여
- `usesThisBattle` 인프라 재사용 (Momentum/Mastery)
- 기존 24종 BehaviorTag 4종 모두 이미 구현됨 — 신규 구현 최소
