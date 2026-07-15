# [DRAFT] TBD, the Archer — "표식 사냥꾼"

> **상태**: 🔴 초안 (결정 대기)
> **슬롯**: Archer (기존 Char_Archer 리워크)
> **상위 문서**: [INDEX.md](INDEX.md), [CharacterConceptReview.md 5.8](../../CharacterConceptReview.md)
> **기존 특성 파일**: `Trait_Archer_Marksman/WeakPoint/RapidFire.asset`

---

## 1. 정체성 (한 문장)

> **"표식으로 적을 약화시키고, 파티 전체가 그 표식을 소비하는 마무리 사냥꾼."**

## 2. 이름 후보

| # | 이름 | 어원/뉘안스 | 장단점 |
|---|------|-----------|--------|
| **A** | **Aster** | 그리스 "별" — 저격수 별빛 이미지 | 직관+우아, 짧고 강렬 |
| B | Lyra | 거문고자리 — 화살/현 이미지 | 서사 강함, Bard와 충돌 가능 |
| C | Veth | Critical Role 아쳐 | 친숙, 저작권 |
| D | Sable | "검은 색" — 암살궁수 | 강렬, 어두움 |
| E | Sienna | 주황/갈색 — 활의 색 | 우아하나 약함 |

**추천**: `Aster` (A) — "별처럼 정확한 한 발" 서사. 저작권 이슈 없음

## 3. 역할군

- **주 역할군**: 단일 마무리 딜러 (Mark → 2배 데미지)
- **부 역할군**: 군중 제압 (Volley 광역), 도트 마무리 (Crippling Shot)

## 4. 강점 / 약점

| 강점 | 약점 |
|------|------|
| Mark로 파티 +20% 버프 | 도트 없는 적에겐 Crippling Shot 빈약 |
| 2배 데미지 단일 폭딜 (Piercing Shot) | AP 2~3 고비용 |
| 광역+단일 모두 보유 (만능 아님 — 조건 다름) | Mark 의존 (Mark 없으면 딜 반감) |

**DesignPillars 약점 유형**: **역할 특화** (도트 없으면 빈약)

## 5. 고유 메카닉: Hunter's Mark (사냥꾼의 표식)

```
[Archer 디버프 스킬 사용] → 대상에게 자동 Mark 부가
[Mark 걸린 적] → 모든 파티원 +20% 데미지
[Mark 3스택 적 사망] → AP +1 환급 (파티 전체)
[Archer 본인 Mark 적 공격] → 추가 위력 +4
```

**핵심 루프**: Hunter's Mark로 표식 → 파티원(Alch/Ashe/Rogue)이 도트 묻힘 → Archer가 Mark/도트 조건으로 결정타. "지휘 + 마무리" 이중 역할.

## 6. 스킬 4종 (4개 다른 조건)

| 스킬 | AP | 기본 효과 | 조건 | 조건 충족 보너스 |
|-----|----|---------|------|----------------|
| **Hunter's Mark** | 1 | 단일 Mark + Def-2 (2턴) | (셋업 — 조건 없음) | Mark 부여 + 파티 +20% |
| **Piercing Shot** | 2 | 단일 14 | 대상 Mark 상태 | 데미지 2배 (28) |
| **Volley** | 2 | 광역 6 | 적 3마리+ | 한 발 추가 (광역 틱 2회) |
| **Crippling Shot** | 3 | 단일 12 + Stun 1 | 대상 상태이상 (Mark 아닌 다른) | 치명타 + AP 1 환급 |

### 조건 다양성 검증 (4.5 원칙 2)
- Hunter's Mark → 셋업 (조건 없음)
- Piercing Shot → **대상 상태** (Mark)
- Volley → **적 수** (3마리+)
- Crippling Shot → **대상 상태이상** (Mark 아닌 다른 상태)

→ 4개 모두 다른 조건. 매 턴 다른 퍼즐. ✅

### 기존 스킬 매핑
| 기존 | 신규 | 변경 |
|------|------|------|
| Archer_Mark (DefenseDown, AP1) | Hunter's Mark | Mark 자원 부여 추가. Def-2 유지 |
| Archer_PiercingArrow (14, AP2) | Piercing Shot | Mark 조건부 2배 |
| Archer_RapidShot (6, AP1) | Volley | 단일 → 광역, 적 수 조건부 |
| Archer_CriticalShot (22, AP3) | Crippling Shot | 22 → 12+Stun, 상태이상 조건부 치명타+AP환급 |

## 7. BehaviorTag 활용

| BehaviorTag | 적용 스킬 | 효과 | 백로그 번호 |
|------------|----------|------|-----------|
| `Dominance` | Piercing Shot | 적 HP < Archer HP 시 위력 +4 | 컨셉 17 (이미 구현됨) |
| `FirstBlood` | Hunter's Mark | 풀피 적 첫 표식 +4 위력 | 컨셉 6 (이미 구현됨) |
| `TargetHighestHP` | Crippling Shot | 자동 보스 우선 타겟 (후보) | 컨셉 2 (구현 필요, 10줄) |
| `Bounty` | Crippling Shot | 킬 시 AP 환급 (Rogue와 공유) | 컨셉 21 (구현 필요, 15줄) |

## 8. 장착 특성 3종 리워크

| 특성 | 기존 효과 | 리워크 효과 | 해금 |
|------|---------|------------|------|
| **명사수** (기본) | 위력 +2 가산 | **Mark 적 대상 위력 +4** (Mark 의존 강화) | 기본 |
| **약점 포착** | 적 HP 60% 미만 ×1.4 | **Mark 3스택 적 치명타 확정** (마무리 특화) | 30 조각 |
| **속사** | 스킬 코스트 -1 | **Hunter's Mark 사용 시 AP 1 환급** (셋업 무료화) | 60 조각 + 1 영혼 |

## 9. 밸런스 시나리오 (다수전 예시)

```
턴 1: Hunter's Mark (보스) → Mark + Def-2. 파티 +20%
턴 2: Volley (적 3마리+) → 6×2회 = 12 광역
턴 3: Piercing Shot (Mark 적) → 28 데미지 (2배)
턴 4: Crippling Shot (Ashe Burn 적) → 12 + Stun + 치명타 + AP 1
턴 5: Mark 3스택 적 처치 → 파티 AP +1 환급
```

## 10. 파티 시너지

| 조합 | 시너지 | 핵심 |
|------|-------|------|
| **Archer + Rogue** | ★★★ | Archer Mark + Rogue 도트 → 둘 다 마무리. Crippling+Eviscerate 콤보 |
| **Archer + Ashe** | ★★★ | Ashe Burn → Archer Crippling Shot 치명타. Mark+Ember 이중 폭딜 |
| **Archer + Alchemist** | ★★ | Alch 도트 폭격 → Archer 마무리. 시너지 자연 |
| **Archer + Taranis** | ★★ | Taranis Charge + Archer Mark. 다수전 마무리 극대화 |
| **Archer + Necromancer** | ★★ | Necro Curse/Decay → Archer Crippling. 저주-마무리 루프 |

## 11. 🔴 결정 대기 항목

- [ ] **이름 확정** (Aster 추천)
- [ ] **Mark 자원 구현 방식** (StatusEffectType.Mark 신규 vs 기존 DefenseDown 재활용)
- [ ] **Mark 3스택 적용 방식** (누적 vs 갱신) — 3스택이 사망 시 AP 환급의 핵심
- [ ] **파티 +20% 데미지 버프** (Mark 1스록 vs 3스록부터) — 1스택이면 사기 가능
- [ ] **Hunter's Mark AP 환급** (속사 특성) — 무료 셋업 사기인지 검증
- [ ] **Volley 광역 위력** (6 vs 조정) — 적 3마리+ 조건 2회 vs 단순 위력 상향
- [ ] **Crippling Shot 상태이상 종류** (Mark 제외 전부 vs 특정 상태만)
- [ ] **Crippling Shot 치명타** (확정 vs 확률) — 확정이면 사기 가능
- [ ] **Piercing Shot 2배 밸런스** (28 데미지 사기인지)
- [ ] **기존 CriticalShot 22데미지 폐지** 영향 — 세이브 호환

## 12. 리스크와 검증

| 리스크 | 완화 |
|-------|------|
| Mark 1스택에 파티 +20% = 과도 | Mark 1스택은 Archer 본인만 +4. 3스택부터 파티 +20% |
| Piercing Shot 28 = 단일 최강 (Ashe Brand of Ash 23보다 강함) | 조건(Mark) 명확. Mark 1스택 소모 여부 검토 |
| Hunter's Mark AP 0 (속사 특성) = 무한 Mark | 전투당 3회 제한 OR 마크 1스택 유지 시간 2턴 제한 |
| Volley 2회 (적 3마리+) = 광역 12 = Taranis Chain과 중복 | Archer는 "자유 타겟 광역", Taranis는 "무작위 연쇄"로 차별화 |
| Mark 3스택 적 사망 AP 환급 과도 (파티 전체 +1) | 환급을 Archer 본인만으로 제한 검토 |

## 13. 구현 메모

- `StatusEffectType.Mark` 신규 항목 추가 (이미 4.6 원칙에는 언급)
- `CharacterResourceComponent` 불필요 — Mark는 상태이상으로 처리 (Charge와 동일 패턴)
- `OnStatusApplied` 이벤트에서 Mark 부여 시 파티 버프 적용
- `OnCharacterDied` 이벤트에서 Mark 3스택 적 사망 시 AP 환급
- BehaviorTag: Dominance/FirstBlood 이미 구현됨. TargetHighestHP/Bounty 신규 구현 필요
- 파티 +20% 데미지 버프: `DamageCalculator.DealDamage`에서 target의 Mark 스택 체크
