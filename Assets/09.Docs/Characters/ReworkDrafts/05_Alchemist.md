# [DRAFT] TBD, the Alchemist — "시약 반응 촉매자"

> **상태**: 🔴 초안 (결정 대기)
> **슬롯**: Alchemist (기존 Char_Alchemist 리워크)
> **상위 문서**: [INDEX.md](INDEX.md), [CharacterConceptReview.md 5.10](../../CharacterConceptReview.md)
> **기존 특성 파일**: `Trait_Alch_PotionMaster/ToxicBurst/ReinforcedPotion.asset`

---

## 1. 정체성 (한 문장)

> **"물약으로 전장을 조작하는 연금술사. 같은 대상에 연속 투여가 폭발한다."**

## 2. 이름 후보

| # | 이름 | 어원/뉘앙스 | 장단점 |
|---|------|-----------|--------|
| **A** | **Cael** | 켈트 "약초학자" 변형 | 직관, 짧고 우아 |
| B | Poppet | "인형/물약병" (고어) | 귀여움, 약함 |
| C | Wes | "서쪽" — 연금술 서사 | 짧으나 의미 약함 |
| D | Arsen | 비소 (독) | 강렬, 어두움 |
| E | Saffron | 사프란 (향신료/약재) | 우아, 길다 |

**추천**: `Cael` (A) — 약초학 서사. 짧고 발음 쉬움

## 3. 역할군

- **주 역할군**: 도트 딜러 (Poison/Burn) + 힐러 하이브리드
- **부 역할군**: 파티 버퍼 (Catalyst)

## 4. 강점 / 약점

| 강점 | 약점 |
|------|------|
| 힐+도트+버프 다기능 | AP 2~3 고비용 (자원 효율 약점) |
| 같은 대상 연속 투여 시 효과 3배 (시약 반응) | 단일 힐이라 광역 회복 약함 |
| 한 턴 물약 3회 사용 시 다음 턴 AP +1 | 드로우 운 의존 (물약 안 뽑히면 시너지 X) |

**DesignPillars 약점 유형**: **자원 효율** (고AP 비용)

## 5. 고유 메카닉: Reagent Reaction (시약 반응)

```
[같은 대상에게 두 번째 물약 사용] → 효과 2배 (기본) / 3배 (물약 명인 특성)
[한 턴에 자신 물약 3회 사용] → 다음 턴 AP +1 (자원 회수)
[도트 디버프 2종+ 적] → "연쇄 반응" 추가 데미지 (Mega Bomb 조건)
```

> **자원(게이지) 없음** — 행동 이력 기반 메카닉. Heal Potion/Alch_PoisonBomb/Alch_BoostPotion/Alch_ShieldPotion 전부 "물약" 카테고리.

**핵심 루프**: Heal Potion(아군) → Poison Bomb(적) → Catalyst(아군) → Mega Bomb(적, 2+ 물약 사용 시 AP 환급). 같은 대상에 2번 투여가 핵심 보너스.

## 6. 스킬 4종 (4개 다른 조건)

| 스킬 | AP | 기본 효과 | 조건 | 조건 충족 보너스 |
|-----|----|---------|------|----------------|
| **Heal Potion** | 2 | 단일 힐 10 | 대상 도트 상태 | 힐량 2배 (20, 해독제 겸용) |
| **Poison Bomb** | 2 | 광역 6 + Poison | 적 3마리+ | 독 범위 +1마리 (4마리) |
| **Catalyst** | 1 | 단일 ATK+4 버프 | 대상 물약 효과 받음 (이번 턴) | 효과 2배 (ATK+8) |
| **Mega Bomb** | 3 | 광역 12 + Burn | 이번 턴 자신 물약 2+회 사용 | AP 1 환급 |

### 조건 다양성 검증 (4.5 원칙 2)
- Heal Potion → **대상 상태** (도트 디버프)
- Poison Bomb → **적 수** (3마리+)
- Catalyst → **대상 행동 이력** (물약 받음)
- Mega Bomb → **자기 행동 이력** (물약 2+회 사용)

→ 4개 모두 다른 조건. ✅

### 기존 스킬 매핑
| 기존 | 신규 | 변경 |
|------|------|------|
| Alch_HealPotion (12, AP1) | Heal Potion | AP 1→2, 도트 조건부 2배 |
| Alch_PoisonBomb (6, AP2, Poison) | Poison Bomb | 적 수 조건부 범위 확장 |
| Alch_BoostPotion (ATK+4, AP1) | Catalyst | 대상 이력 조건부 2배 |
| Alch_ShieldPotion (10, AP2) | (제거 또는 Catalyst에 통합) | Shield → ATK 버프로 역할 정규화 |
| (신규) | Mega Bomb | 광역 Burn finisher 추가 |

## 7. BehaviorTag 활용

| BehaviorTag | 적용 스킬 | 효과 | 백로그 번호 |
|------------|----------|------|-----------|
| `Distribute` | Poison Bomb | 독 무작위 분배 (대안) | 컨셉 1 (구현 필요, 20줄) |
| `Escalation` | Mega Bomb | 매 사용 cost +1 (밸런스) | 컨셉 13 (이미 구현됨) |
| `Echo` | Heal Potion | 위력 절반 2회 시전 (같은 아군 중복 투여) | 컨셉 10 (UI 필요) |
| `Intensify` | Poison Bomb | 스택 누적 도트 강화 (기존 24종) | 이미 구현됨 |

## 8. 장착 특성 3종 리워크

| 특성 | 기존 효과 | 리워크 효과 | 해금 |
|------|---------|------------|------|
| **물약 명인** (기본) | 힐/쉴드 +10% | **시약 반응 2배 → 3배 강화** (연쇄 폭딜) | 기본 |
| **독성 폭발** | 도트 지속 +2턴 | **물약 효과 받은 적 도트 +2스택** (Heal 제외) | 30 조각 |
| **강화 물약** | 전투 시작 HP +15 | **AP 1회 복구 (전투당 1회, 턴 시작 시 AP 0이면)** | 60 조각 + 1 영혼 |

## 9. 밸런스 시나리오 (다수전 예시)

```
턴 1: Poison Bomb (적 3마리+) → 6 데미지 + Poison 4마리. 물약 1
턴 2: Heal Potion (도트 아군) → 20 힐 (2배, 해독). 물약 2
턴 3: Catalyst (방금 힐받은 아군) → ATK+8 (2배). 물약 3
       → 다음 턴 AP +1 트리거
턴 4: Mega Bomb (물약 2+회 조건) → 12 광역 Burn + AP 1 환급
```

## 10. 파티 시너지

| 조합 | 시너지 | 핵심 |
|------|-------|------|
| **Alch + Necromancer** | ★★★ | 이중 도트. Alch Poison + Necro Decay → Soul Harvest 가속 |
| **Alch + Ashe** | ★★★ | 시약 반응 + Ember 연쇄. 화염 폭발 콤보 (Burn 중복) |
| **Alch + Healer** | ★★ | 힐러 이중화. Healer 정화+영구 버프, Alch 힐+도트 |
| **Alch + Bard** | ★★ | Bard 곡 + Alch Catalyst = 파티 ATK 스노우볼 |
| **Alch + Rogue** | ★★ | Alch 광역 도트 → Rogue Backstab 조건 다수 |

## 11. 🔴 결정 대기 항목

- [ ] **이름 확정** (Cael 추천)
- [ ] **ShieldPotion 제거 여부** (Catalyst에 통합 vs 스킬 5종 유지)
- [ ] **시약 반응 추적 방식** (대상별 딕셔너리 vs 간단한 카운터)
- [ ] **시약 반응 배수** (2배 기본 vs 3배 기본)
- [ ] **한 턴 물약 3회 사용 보상** (AP +1 vs 다른 보상)
- [ ] **Catalyst 2배 조건** (물약 받음 이력) — 같은 턴 vs 누적
- [ ] **Mega Bomb AP 환급** 밸런스
- [ ] **Heal Potion AP 1→2** 상향 영향 (기존 AP1과 차별화)
- [ ] **Poison Bomb 무작위 분배(Distribute) vs 범위 확장** — 둘 중 택일
- [ ] **Echo UI 구현** (순차 타겟) — 별도 인프라 필요

## 12. 리스크와 검증

| 리스크 | 완화 |
|-------|------|
| 시약 반응 3배(특성) = Heal 30 / Catalyst ATK+12 = 사기 | "물약 받음" 조건 1턴 제한. 같은 대상 연속 투여 드로우 운 의존 |
| Mega Bomb + Escalation + AP 환급 = 무한 광역 | Escalation으로 매 사용 cost +1. 3회 후 cost 6 = 사실상 봉인 |
| Heal Potion AP 2 = 기존 AP1 대비 너프 | 도트 조건부 2배(20) 보상. 해독제 겸용 가치 |
| 같은 대상 2번 투야 강제 = 드로우 운 의존 과도 | 시약 반응은 보너스. 기본 효과(10/6/4)로 평타 |
| Alch가 힐러 역할 겹침 (Healer와 차별화) | Healer=정화+영구 버프, Alch=도트+일시 버프. 명확 분리 |
| 물약 3회 사용 추적 = 전투 종료 시 클리어 필수 | TurnContext에 _potionUsesThisTurn 필드. 매 턴 시작 시 리셋 |

## 13. 구현 메모

- 자원 컴포넌트 **불필요** — 행동 이력 기반 (TurnContext 확장)
- `TurnContext._potionTargetsThisTurn` Dictionary<Character, int> — 같은 대상 투여 횟수
- `TurnContext._alchemistPotionUsesThisTurn` int — 자신 물약 사용 횟수
- 매 턴 시작 시 두 값 모두 리셋
- BehaviorTag: Escalation 이미 구현됨. Distribute/Echo 신규 (UI 인프라 필요)
- "물약" 카테고리 분류: SkillData에 `isPotion` bool 필드 추가 OR 스킬 ID 접두사로 판별
