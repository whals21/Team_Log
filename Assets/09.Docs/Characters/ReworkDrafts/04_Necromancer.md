# [DRAFT] TBD, the Necromancer — "진짜 소환 + 영혼 수확"

> **상태**: 🔴 초안 (결정 대기)
> **슬롯**: Necromancer (기존 Char_Necromancer 리워크)
> **상위 문서**: [INDEX.md](INDEX.md), [CharacterConceptReview.md 5.9](../../CharacterConceptReview.md)
> **기존 특성 파일**: `Trait_Necro_LifeLeech/CursePrice/DeathHarvest.asset`

---

## 1. 정체성 (한 문장)

> **"죽음을 자원으로 삼아 언데드를 부리는 흑마법사. 적이 죽을수록 강해진다."**

## 2. 이름 후보

| # | 이름 | 어원/뉘앙스 | 장단점 |
|---|------|-----------|--------|
| **A** | **Mortis** | 라틴 "죽음" (rigor mortis) | 직관+강렬, Necro 정체성 1:1 |
| B | Lilith | 유대 전설 아담 첫 아내 | 서사 강함, 다소 진부 |
| C | Vex | "괴롭히다" | 짧으나 의미 약함 |
| D | Morrigan | 켈트 죽음 여신 | 강렬, 발음 어려움 |
| E | Thane | "귀족/시종" | 부적합 |

**추천**: `Mortis` (A) — Necromancer 정체성과 1:1 대응. "죽음 그 자체"

## 3. 역할군

- **주 역할군**: 도트+처치 딜러 (Curse → Decay → Soul Harvest 루프)
- **부 역할군**: 소환 탱커 (미니언으로 적 공격 대신 맞음)

## 4. 강점 / 약점

| 강점 | 약점 |
|------|------|
| 도트로 적 약화 → 처치 시 자원 회수 | 빈사 적(HP 10%-)에게 약함 (과잉 딜 낭비) |
| 미니언 소환으로 추가 탱킹 | 소환 인프라 복잡 (구현 난이도 高) |
| Soul 자원 스노우볼 | 초반(영혼 0) 평범 |

**DesignPillars 약점 유형**: **자원 효율** (빈사 적 약함)

## 5. 고유 메카닉: Soul + 미니언 시스템

```
[도트 디버프로 적 처치] → Soul +1 (최대 3)
[Soul 1+ 보유 시] → Raise Dead 패시브 액션 가능 (미니언 소환)
[미니언] → HP 15, 1턴 지속, 다음 적 턴 한 대 대신 맞음 (대신 사망)
[미니언 사망] → Soul +1 (죽음의 수확 특성 시)
[영혼 소모] → Soul Harvest 위력 가산
```

**핵심 루프**: Curse → Decay(도트) → 적 약화 → Life Drain 흡혈 → Soul Harvest 처치 → Soul 획득 → 미니언 소환 → 탱킹 보조. "서서히 약화시키며 자원 회수" 사이클.

## 6. 스킬 4종 (4개 다른 조건)

| 스킬 | AP | 기본 효과 | 조건 | 조건 충족 보너스 |
|-----|----|---------|------|----------------|
| **Curse of Frailty** | 1 | 단일 AtkDown (2턴) | (셋업 — 조건 없음) | Curse 부여 |
| **Decay** | 1 | 단일 4 + Poison 3턴 | (셋업-소비 — Curse 선행 권장) | 도트 부여 |
| **Life Drain** | 2 | 단일 10 + 흡혈 50% | 대상 디버프 상태 | 흡혈량 2배 (100%) |
| **Soul Harvest** | 3 | 단일 12 + 영혼 비례 | 적 처치 시 | AP 2 환급 + Soul +1 |

> **Raise Dead**: 별도 스킬이 아닌 **Soul 1 소비 패시브 액션**으로 변경. 스킬 슬롯 4개 유지.

### 조건 다양성 검증 (4.5 원칙 2)
- Curse of Frailty → 셋업 (조건 없음)
- Decay → 셋업-소비 (Curse 선행 권장, 강제 아님)
- Life Drain → **대상 상태** (디버프)
- Soul Harvest → **이벤트** (적 처치)

→ 4개 모두 다른 조건. ✅

### 기존 스킬 매핑
| 기존 | 신규 | 변경 |
|------|------|------|
| Necro_Curse (AtkDown, AP1) | Curse of Frailty | 유지, 이름만 변경 |
| Necro_Decay (4, AP1, Poison) | Decay | 유지 |
| Necro_LifeDrain (10, AP2) | Life Drain | 디버프 조건부 흡혈 2배 추가 |
| Necro_RaiseDead (광역 7, AP3) | Soul Harvest | 광역 공격 → 단일 처치 + 자원 회수 |

> Raise Dead(광역) → Soul Harvest(단일 처치). 소환은 패시브로 이관.

## 7. BehaviorTag 활용

| BehaviorTag | 적용 스킬 | 효과 | 백로그 번호 |
|------------|----------|------|-----------|
| `Bounty` | Soul Harvest | 킬 시 AP 2 환급 + Soul +1 | 컨셉 21 (구현 필요) |
| `Wound` | Life Drain | 다칠수록 약화 (의도적 약점) | 컨셉 12 (이미 구현됨) |
| `Lifesteal` | Life Drain | 흡혈 (기존 24종) | 이미 구현됨 |
| `GiantSlayer` (후보) | Soul Harvest | 적 MaxHP 100+ 보너스 | 컨셉 15 (이미 구현됨) |

## 8. 장착 특성 3종 리워크

| 특성 | 기존 효과 | 리워크 효과 | 해금 |
|------|---------|------------|------|
| **생명력 흡수** (기본) | 준 데미지 15% 회복 | **영혼 1개당 흡혈 +5%** (영혼 스케일, 최대 30%) | 기본 |
| **저주의 대가** | 버프/디버프 ×1.3 | **Curse 2스택 적 AtkDown+DefDown 동시** (이중 저주) | 30 조각 |
| **죽음의 수확** | 킬당 ATK +1 누적 | **미니언 사망 시 Soul +1** (소환-희생 루프 강화) | 60 조각 + 1 영혼 |

## 9. 밸런스 시나리오 (엘리트전 예시)

```
턴 1: Curse of Frailty → 적 AtkDown. 영혼 0
턴 2: Decay → 4 데미지 + Poison 3턴. 영혼 0
턴 3: Life Drain (디버프 적) → 10 데미지 + 흡혈 10 (2배). 영혼 0
턴 4: Life Drain → 10 + 흡혈 10. 적 HP 20% 도달
턴 5: Soul Harvest → 12 데미지 + 킬 → AP 2 환급 + Soul 1
턴 6: Raise Dead (Soul 1 소비) → 미니언 소환. 적 공격 미니언 대신 맞음
턴 7: 미니언 사망 → Soul +1 (특성). 다시 소환 가능
```

## 10. 파티 시너지

| 조합 | 시너지 | 핵심 |
|------|-------|------|
| **Necro + Alchemist** | ★★★ | 이중 도트 (Poison). Soul Harvest + Alch Mega Bomb 연쇄 |
| **Necro + Rogue** | ★★★ | Rogue 도트 적 → Necro Life Drain 2배 흡혈. 콤보+영혼 루프 |
| **Necro + Ashe** | ★★ | Ashe Burn + Necro Poison = 다중 도트. Soul Harvest 가속 |
| **Necro + Lumi** | ★★ | Freeze로 적 묶기 → Necro 도트 누적 시간 확보 |
| **Necro + Healer** | ★★ | 미니언 + Healer Holy Shield 조합. 파티 탱킹 극대화 |

## 11. 🔴 결정 대기 항목

- [ ] **이름 확정** (Mortis 추천)
- [ ] **미니언 시스템 구현 범위** (풀 버전 vs 단순화된 "대신 맞기" 버프)
- [ ] **Soul 자원 캡** (3 vs 5) — 캡이 높으면 스노우볼, 낮으면 평범
- [ ] **Soul Harvest 영혼 비례 위력** (영혼×N, N값)
- [ ] **Life Drain 흡혈 2배 조건** (디버프 여부) — 기본이 50%인데 2배면 100%
- [ ] **미니언 HP/공격력** (15 / 0 vs 조정) — 공격 못하면 단순 탱크
- [ ] **미니언 지속 시간** (1턴 vs 2턴) — 1턴이면 매 턴 소환 필요
- [ ] **Raise Dead 패시브 액션 UX** — 버튼 클릭 vs 자동 발동
- [ ] **Wound를 Life Drain에 적용** 여부 (의도적 약점 부여)
- [ ] **기존 RaiseDead(광역 7) 폐지** 영향 — 세이브 호환

## 12. 리스크와 검증

| 리스크 | 완화 |
|-------|------|
| 미니언 소환 시스템 인프라 복잡 (별도 클래스) | CC-2C를 마지막으로 배치. "대신 맞기" 버프로 단순화 옵션 |
| Soul 3 캡 + 미니언 사망 Soul = 무한 소환 루프 | 미니언 사망 Soul 획득은 특성(60 조각)만. 기본은 미획득 |
| Life Drain 흡혈 100% = 사기적 생존 | 디버프 조건 강제. 디버프 없는 적에겐 50% |
| Soul Harvest AP 2 환급 = 연쇄 킬 | 킬 실패 시 보너스 0. 단일 처치에만 가치 |
| 빈사 적(HP 5%)에게 Soul Harvest 낭비 | Cull(Rupture)과 달리 임계값 처치가 아닌 "킬 이벤트" — 빈사 적에게도 가치 |
| 도트 디버프 없는 파티(Ashe/Rogue/Alch 없음)에서 디버프 조건 충족 어려움 | Curse of Frailty로 자체 디버프 부여 가능 |

## 13. 구현 메모

- `CharacterResourceComponent` 서브클래스: `SoulResourceComponent`
- `OnCharacterDied` 이벤트 훅에서 도트 데미지로 인한 킬 시 Soul +1
- 미니언 시스템: `MinionCharacter` 신규 클래스 (Character 서브클래스) — 가장 복잡
  - 또는 "대신 맞기" 상태이상(ForcedTarget+ShieldPrep 변형)으로 단순화 가능
- BehaviorTag: Bounty 구현 (Rogue와 공유, 15줄)
- Soul 자원 비례: ResourcePowerPerStack 필드 활용 (Ashe와 동일 패턴)
