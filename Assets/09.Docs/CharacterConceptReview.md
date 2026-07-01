# 캐릭터 컨셉 개편안 — 조건부 메카닉 중심 설계

> **작성일**: 2026-06-20
> **상태**: 기획 설계 (구현 전)
> **배경**: 기존 8캐릭터가 "역할 분담은 명확하지만 고유 메카닉 부재" 상태. 다른 턴제 로그라이크(Slay the Spire / Darkest Dungeon / Into the Breach / Monster Train)가 채택한 **조건부 스킬·특성·유물** 패턴을 도입해 "턴 설계의 재미"를 끌어올린다.

---

## 1. 비전 및 설계 목표

### 핵심 가설
> **"조건부 보상은 턴 설계를 게임으로 만든다."**

무조건 강한 스킬은 AP 계산으로 끝난다. 하지만 *"대상이 도트 디버프 상태일 때 +50%"*, *"이번 턴 힐을 받은 아군의 다음 공격에 +3"* 같은 조건부는 **플레이어가 매 턴 조건을 채우기 위해 순서·타겟·타이밍을 고민하게 만든다.** 이것이 StS·DD·ItB가 공유하는 핵심 재미 요소다.

### 4가지 설계 목표
1. **캐릭터 정체성 부여** — 각 캐릭터가 "이 캐릭터만의 플레이 루프"를 갖는다 (Warrior=맞을수록 강해지는 루프, Rogue=콤보 적재 루프 등)
2. **턴 순서 설계 재미** — 단순 "가장 강한 스킬 누르기" → "조건 채우기 퍼즐"로 전환
3. **파티 시너지 자연 발생** — 한 캐릭터가 만든 상태(독/저주/Mark)를 다른 캐릭터가 소비하는 연쇄
4. **AP 환급/재시전 메카닉** — 고비용 스킬도 조건 채우면 "무료"가 되는 폭발력 추가

### 비젼(Non-Goal)
- 원소 상성(불>물) 같은 **가위바위보** 도입 안 함 — 턴 설계의 재미가 목표이지 데미지 계산기 게임이 아님
- 캐릭터를 **완전히 새로 만들지** 않음 — 기존 8캐릭터框架를 유지하되 스킬 4종을 조건부로 리워크 + 고유 메카닉 1개씩 추가

---

## 2. 타 게임 조건부 메카닉 사례 분석

### 2.1 Slay the Spire — "조건부 카드"의 교과서

| 카드 | 조건 | 보상 |
|------|------|------|
| **Heavy Blade** | Strength 버프 보유 시 | 위력 3배 스케일 |
| **Searing Blow** | 업그레이드 불가 대신 | 무한 업그레이드 가능 |
| **Reaper** | 턴 첫 번째 공격 | 흡혈량 = 준 데미지 |
| **Limit Break** | Strength 보유 시에만 | Strength 2배 ( otherwise 실패 ) |
| **Whirlwind + Flash Strike** | 에너지 다 쓸 때 | 추가 발동 |
| **Feed / Reaper / Metallicize** | 적 처치 시 / 첫 턴 / 상시 | 영구 강화 / 흡혈 / 가Armor |
| **Predatory Instincts** (Watcher) | Divine Stance 유지 | 에너지 +2 |

**StS의 핵심 통찰**: 조건부 카드는 "덱 안에서 서로 시너지"를 만든다. Strength 버프 카드 3장 + Heavy Blade가 한 덱에서 만나면 폭발.

### 2.2 Darkest Dungeon — "위치 기반 조건부"

| 스킬 | 조건 | 효과 |
|------|------|------|
| **Crusader - Holy Lance** | 캐릭터가 뒤에 위치할 때 | 앞으로 1칸 이동하며 공격 |
| **Highwayman - Point Blank Shot** | 1번 위치에서만 | 200% 데미지 + 후퇴 |
| **Vestal - Divine Comfort** | 파티 전체 부상 시 | 추가 힐 |
| **Plague Doctor - Battlefield Medicine** | 대상이 상태이상 | 정화 + 힐 동시 |
| **Houndmaster - Hound's Rush** | 적이 Marked | 크리티컬 확률 폭증 |

**DD의 핵심 통찰**: 위치 자체가 "자원"이다. 위치 이동 스킬이 캐릭터 정체성.

### 2.3 Into the Breach — "정보 기반 조건부"

| 파일럿/메크 | 조건 | 효과 |
|------------|------|------|
| **Time Traveler** | 한 번 죽은 후 | 타임 리와인드 |
| **Siege Bot** | 건물 옆에 위치 | 방어 보너스 |
| **Blitzkrieg** | 적을 밀어서 다른 적과 충돌 | 연쇄 데미지 |
| **Flame Bot** | 적이 이미 불에 탐 | 추가 데미지 |

**ItB의 핵심 통찰**: "적의 다음 행동이 보인다"는 정보 자체가 조건부 판단의 재료. Team Log는 적 intent 시스템이 있으므로 이 패턴 차용 가능.

### 2.4 Monster Train — "층/유닛 조건부"

| 메카닉 | 조건 | 효과 |
|--------|------|------|
| **Spell Buff** | 유닛이 특정 챔피언 종족 | 영구 강화 |
| **Train Steward** | 보스층에서만 | 데미지 2배 |
| **Consumable** | 사용 후 파티에 유닛 | 소환 |

**MT의 핵심 통찰**: "다음 층에서 발동" 같은 **지연 조건부**도 재미. Team Log의 휴식지/상점 방문 트리거와 연계 가능.

---

## 3. 조건부 메카닉 분류 — 설계 어휘집

> 8가지 **조건 유형** × 4가지 **보상 유형** = 32가지 조합. 본 설계의 모든 카드/특성/유물은 이 어휘집의 조합으로 표현한다.

### 3.1 조건 유형 (8종)

| 유형 | 코드 | 예시 |
|------|------|------|
| **상태 기반** | `OnStatus` | 대상이 Burn/Poison/Freeze/Stun 중일 때 |
| **HP 임계** | `OnHpThreshold` | 대상/자신 HP 30%/50%/100% |
| **행동 기반** | `OnActionPerformed` | 이번 턴에 X 스킬 타입 사용 / 공격 안 함 / 힐 받음 |
| **위치/순서 기반** | `OnTurnOrder` | 첫 번째로 행동 / 마지막 / 두 번째 스킬 |
| **적 수 기반** | `OnEnemyCount` | 적 3마리 이상 / 1마리 남음 |
| **스택 기반** | `OnStack` | 자신의 분노/콤보/원소 공명 스택이 N 이상 |
| **시점 기반** | `OnTurnPhase` | 첫 턴 / 보스 턴 / 홀수/짝수 |
| **사건 기반** | `OnEvent` | 적 처치 / 아군 사망 / 쉴드 파괴 / 도트 데미지 받음 |

### 3.2 보상 유형 (4종)

| 유형 | 코드 | 예시 |
|------|------|------|
| **데미지 배율** | `DamageMultiplier` | +50% / 2배 / 치명타 |
| **AP 환급** | `RefundAP` | 사용 후 AP +1 / 다음 스킬 AP -1 |
| **재시전 / 추가 발동** | `Replay` | 같은 스킬 한 번 더 / 효과 2회 적용 |
| **영구 강화 / 부여** | `PermanentBuff` | Strength +1 / 상태이상 부여 / 쿨다운 리셋 |

### 3.3 Team Log 우선 적용 매트릭스

| | DamageMul | RefundAP | Replay | Permanent |
|---|---|---|---|---|
| **OnStatus** | ★★★ (Rogue) | ★★ | ★ | ★★ |
| **OnHpThreshold** | ★★★ (Warrior) | ★ | ★ | ★ |
| **OnActionPerformed** | ★★ | ★★★ (마법사 3종) | ★★ | ★ |
| **OnTurnOrder** | ★ | ★★ | ★ | ★★ (Bard) |
| **OnEnemyCount** | ★★ (Stormcaller/Pyromancer AOE) | ★ | ★ | ★ |
| **OnStack** | ★★★ (콤보) | ★★ | ★ | ★★ |
| **OnTurnPhase** | ★★ | ★ | ★ | ★ |
| **OnEvent** | ★★ (Necro) | ★★★ (처치 시) | ★ | ★★ |

---

## 4. 현재 캐릭터 평가 (요약)

> 상세 평가는 본 문서 도입부의 컨셉 리뷰에서 이미 다룸. 여기는 핵심 결함만 재요약.

### 4개 핵심 결함
1. **고유 자원/메카닉 부재** — 모두 동일 AP 프레임, "그 캐릭터만의 룰" 없음
2. **단일 대상 과잉** — Rogue·Archer 4스킬 전부 SingleEnemy
3. **역할 중복 + 하위 호환** — Alch.HealPotion(AP1) > Healer.Heal(AP2), Bard.BattleSong(AP2 AllAllies) > Warrior.Rage(AP2 Self)
4. **원소·컨셉 색칠만 다름** — IceSpear에 Freeze 부재, Necro 소환이 그냥 광역 공격

---

## 4.5 스킬 4개 설계 원칙 (★모든 캐릭터 공통 규칙)

> 본 개편의 모든 스킬 설계는 이 원칙을 따른다. 위반 시 설계 재검토.

### 4.5.1 핵심 철학 — 드로우 운이 전략이 되려면

Team Log는 **매 턴 4슬롯에 가중치 랜덤**으로 스킬이 등장. 플레이어는 등장한 스킬 중에서만 선택 가능. 이 제약이 **"뽑은 스킬로 고민하기"** 라는 핵심 재미.

→ 스킬 설계가 이 철학을 훼손하면 안 됨. 특히 두 가지 반패턴 주의:

| 반패턴 | 설명 | 예시 |
|--------|------|------|
| **만능 스킬** | 하나의 스킬이 모든 상황에 대응 | "적 수에 따라 자동으로 범위 전환" — 드로우 운이 의미 없어짐 |
| **변형 스킬 분리** | 비슷한 효과의 변형을 2개 스킬로 | "전체 도발" + "단일 도발" — 원하는 쪽이 안 뽑히면 좌절 |

### 4.5.2 4가지 설계 규칙

#### 규칙 1: 각 스킬은 **단일하고 단순한 조건** (다중 조건 금지)

- 각 스킬의 조건부는 **한 가지 상태만** 체크
- "A이고 B일 때" 같은 복합 조건은 학습 비용 high → 금지
- 예: `Strike` 조건 = "분노 3+" (O) / "분노 3+이고 도발 상태일 때" (X)

#### 규칙 2: 스킬 4개는 **서로 다른 조건** (매 턴 다른 퍼즐)

- 4개 스킬이 4개의 다른 조건을 가져야 매 턴 다른 퍼즐 등장
- 비슷한 조건 중복 시 "운 좋게 둘 다 뽑으면 강함" → 드로우 운 결정
- 예 (Warrior): Strike=분노 / Bodyblock=도발 / Bloodlust=피격 / Taunt=셋업 (무조건)

#### 규칙 3: **만능 스킬 금지** — 상황 자동 대응 X

- 하나의 스킬이 조건에 따라 완전히 다른 효과로 전환되면 안 됨
- "상황별로 다르게 작동"이 필요하면 → **별도 스킬이 아니라 다른 캐릭터의 역할**로 해결
- 드로우 운이 중요하지 않게 되므로 철학 위반

#### 규칙 4: **셋업 스킬과 소비 스킬**의 분리

- 한 스킬은 다른 스킬의 조건을 만들어주는 "셋업" 역할 가능 (조건부 없음)
- 다른 스킬들은 그 셋업을 소비하는 조건부
- 예: `Taunt` (셋업, 조건 없음) → `Bodyblock` (소비, "도발 상태일 때")

#### 규칙 5: **조건의 두 종류** — 강화 조건 vs 사용 제약 조건 (★핵심)

> 스킬의 "조건"은 두 가지로 명확히 구분한다. 혼용 시 플레이어 좌절(기다림 지옥) 발생.

**A. 강화 조건 (기본 원칙 — 모든 스킬의 디폴트)**
- 스킬은 **항상 사용 가능**
- 조건 충족 시 **추가 보너스/강화** 적용
- 기본 효과는 그 자체로 의미 있어야 (Ember 0/조건 미충족이어도 최소 역할 수행)

```
스킬 사용 → 기본 효과 발동 (항상)
         → 조건 충족 확인 → 충족 시 보너스 효과 추가
```

**B. 사용 제약 조건 (예외 — 엄격한 기준 통과 시에만)**
- 조건 미충족 시 **스킬 자체를 사용 불가** (또는 효과 0)
- 예외 허용 기준 (3가지 모두 충족 권장):
  1. **게임 체인저급 메카닉** — 자살, 원킬, 파티 전체 부활, 영구 강화
  2. **위력이 일반 스킬의 3배 이상**
  3. **셋업-소비 루프의 명확한 종착지** — 자원 완충 등 특수 상태

**왜 이 원칙이 필요한가**:
- 강화 조건 스킬은 매 턴 의미 있게 사용 가능 → 드로우 운이 전략으로 작동
- 사용 제약 조건 스킬이 많으면 "특정 조건 기다리기" 게임이 됨 → 좌절
- 기획서의 모든 스킬은 **기본적으로 강화 조건**으로 설계. 사용 제약 조건은 스킬 4개 중 **최대 1개** (보통 궁극기) 권장.

### 4.5.3 조건의 두 종류 — 적용 예시

**강화 조건 (올바른 예)**:
- `Brand of Ash`: 항상 사용 가능 (기본 8 + Ember×3 데미지). 자신 HP 50% 이하 시 데미지 2배
- `Frost Armor`: 항상 사용 가능 (기본 쉴드 10). 대상 Freeze 시 쉴드 +50%
- `Backstab`: 항상 사용 가능 (기본 7 데미지). 대상 디버프 시 데미지 2배

**사용 제약 조건 (예외 허용 예)**:
- `Embrace of Cinders`: Ember 5에서만 사용 가능. 자살 메카닉 + 105 데미지 (게임 체인저 + 3배 위력 + 루프 종착지)
- `Eviscerate`: 콤보 5에서만 사용 가능. finisher 메카닉 (데미지 3배 + 치명타)

**사용 제약 조건 (금지 예)**:
- `Spark`: "대상이 도트 상태"일 때만 사용 가능 → 조건 기다려야 함, 기본 역할 불가 → **강화 조건으로 전환** (항상 사용, 도트 시 +50%)

### 4.5.4 검증 체크리스트 (각 스킬 설계 시)

- [ ] 이 스킬의 조건은 한 가지 상태만 체크하는가? (규칙 1)
- [ ] 4개 스킬의 조건이 서로 다른가? (규칙 2)
- [ ] 이 스킬이 만능(모든 상황에 강함)은 아닌가? (규칙 3)
- [ ] 셋업-소비 관계가 형성되는가? (규칙 4)
- [ ] 이 스킬은 **항상 사용 가능**한가? (강화 조건, 규칙 5A)
- [ ] 만약 사용 제약 조건(규칙 5B)이라면 — 게임 체인저급/3배 위력/루프 종착지 3가지 기준 충족하는가?
- [ ] 조건을 못 채웠을 때 명확한 약점이 있는가?
- [ ] 기본 수치(조건 미충족 시)가 그 자체로 의미가 있는가?

### 4.5.5 반례 vs 올바른 설계

**반례 1 (X)** — 만능 도발:
> "Warrior_Taunt: 적 1~2마리일 때 전체 도발, 3+마리일 때 상위 2마리만 도발, 분노 5+일 때 지속 +1턴"
> → 하나의 스킬이 모든 상황에 자동 대응. 드로우 운이 무의미. (규칙 3 위반)

**반례 2 (X)** — 사용 제약 조건 남용:
> "Pyromancer_Spark: 대상이 도트 상태일 때만 사용 가능, 데미지 +50%"
> → 매 턴 도트 묻힌 적 기다려야 함. 드로우 운이 이중 작용 → 좌절. (규칙 5 위반)

**올바른 설계 (O)** — 강화 조건 4종 (매 턴 사용 가능, 보너스로 다양성):
> - `Cinder Accretion`: 항상 사용 (기본 5 데미지 + Burn). 조건 없음 (셋업)
> - `Brand of Ash`: 항상 사용 (기본 8 + Ember×3). 자신 HP 50% 이하 시 데미지 2배
> - `Phoenix Renewal`: 항상 사용 (기본 8 힐). 대상 HP 50% 이하 시 정화 추가
> - `Embrace of Cinders`: 사용 제약 (Ember 5 필수). 궁극기 자살 메카닉 — 예외 인정

매 턴 뽑힌 스킬의 조건을 채우는 퍼즐이 다름 → 드로우 운이 전략적 의미를 가짐.

---

## 4.6 캐릭터 명명법 및 문서 구조 (★원칙)

### 4.6.1 캐릭터 명명법 — "[이름], the [칭호]"

모든 캐릭터는 **`[이름], the [칭호/클래스]`** 형식으로 부른다. 클래스명만으로 부르지 않는다.

| 클래스 | 영문 명칭 | 한국어 | 이름 의미/어원 |
|--------|---------|--------|--------------|
| Pyromancer | **Ashe, the Pyromancer** | 아셰, 화염 마법사 | "잿더미(ash)"에서 파생. 화재의 생존자라는 배경과 부합 |
| Cryomancer | TBD, the Cryomancer | TBD, 냉기 마법사 | 후속 정의 |
| Stormcaller | TBD, the Stormcaller | TBD, 번개 마법사 | 후속 정의 |
| Warrior | **Duran, the Warrior** | 듀란, 전사 | 라틴어 *durandus* (견고한) — "불멸의 성벽" 컨셉 |
| Healer | TBD, the Healer | TBD, 힐러 | 후속 정의 |
| Rogue | TBD, the Rogue | TBD, 도적 | 후속 정의 |
| Archer | TBD, the Archer | TBD, 궁수 | 후속 정의 |
| Necromancer | TBD, the Necromancer | TBD, 네크로맨서 | 후속 정의 |
| Alchemist | TBD, the Alchemist | TBD, 연금술사 | 후속 정의 |
| Bard | TBD, the Bard | TBD, 음유시인 | 후속 정의 |
| **Oracle** ⭐신규 | **Sibyl, the Oracle** | 시빌, 예언자 | 그리스 *sibulla* (예언자). "보되 바꿀 수 없는 자" |
| **Stormcaller** ⭐신규 | **Taranis, the Stormcaller** | 타라니스, 폭풍 소환사 | 켈트 번개 신. "때리지 않는 자, 길만 만드는 자" |

**규칙**:
- 본 문서/대화/코드 주석에서 클래스명만으로 부르지 않는다 — "Pyromancer가..." (X) → **"Ashe가..." (O)**
- 개인 이름(Ashe)은 캐릭터 정체성·배경 스토리와 연결되는 의미 있는 이름이어야 함
- 칭호(the Pyromancer)는 직업/역할을 나타내는 서사적 호칭
- 각 캐릭터 개인 이름은 후속 작업에서 개별 정의 (현재는 Ashe만 확정)

### 4.6.2 캐릭터별 문서 분리 정책

각 캐릭터의 상세 설계(스토리/메카닉/스킬/밸런스/시너지/연출)는 **별도 문서로 분리**하여 관리한다.

- **경로**: `Assets/09.Docs/Characters/[Name]_the_[Class].md`
  - 예: `Assets/09.Docs/Characters/Ashe_the_Pyromancer.md`
- **본 문서(CharacterConceptReview.md)의 각 캐릭터 섹션(5.x)에는 요약만 기재**:
  - 정체성 한 문장
  - 스킬 4종 간결 표
  - 상세 문서 링크
- **본 문서는 횡단 관심사에 집중**: 아키텍처 개요 / 설계 원칙 / 캐릭터 비교 / 시너지 매트릭스 / 로드맵
- **캐릭터 개별 디테일은 개별 문서에서 관리** — 본 문서 팽창 방지 + 개별 캐릭터 작업 시 충돌 최소화

---

## 5. 캐릭터별 개편안

> 각 캐릭터: (a) **고유 메카닉** 1개 + (b) **스킬 4종 리워크** (조건부 포함). 기존 스키마(SkillData + 새 SkillCondition 필드)에 맞춤.
> **캐릭터 수 변경**: 기존 8종 → **12종**. Mage(4원소 혼합) 제거, **Pyromancer/Cryomancer/Stormcaller 3종 추가** (각 원소 전문 마법사로 컨셉 통일). **Sibyl, the Oracle 11번째 신규 캐릭터** (2026-06-30, "미래에 투자"하는 시간 축) + **Taranis, the Stormcaller 12번째 신규 캐릭터** (2026-06-30, "네트워크에 투자"하는 공간 축) 추가. 마법사 3종 삼각(Ashe/Lumi/Taranis) 완성. 상세: [Characters/Sibyl_the_Oracle.md](Characters/Sibyl_the_Oracle.md) / [Characters/Taranis_the_Stormcaller.md](Characters/Taranis_the_Stormcaller.md)

### 5.1 Duran, the Warrior (전사) — "불멸의 성벽"

**정체성**: 충격을 흡수하여 역석으로 전환하는 불멸의 성벽. 받은 고통을 되갚으며, 부서지는 대신 더 단단해진다.

**역할군**: 주=메인 탱커 / 부=버스트 딜러

**고유 메카닉**: **Vengeance (복수 게이지)** — Duran이 받는 데미지(쉴드 흡수 + HP 직접) 또는 **Duran이 부여한 쉴드**가 다른 아군에서 흡수한 데미지가 1:1로 축적. 최대 20. **자연 감소 없음** (Duran이 안 맞으면 자연히 쌓이지 않음). 소비 스킬로만 감소.

**스킬 4종 (2:2 구조 — 충전 2 + 소비 2)**:

| 스킬 | AP | 기본 효과 | 조건 | 충족 보너스 / 비고 |
|-----|-----|---------|------|----------------|
| Shield Wall (방패벽) | 1 | 아군 1명(자신 포함) 쉴드 +10 | Vengeance 10+ | 쉴드 **+10 + Vengeance/2** |
| Provoking Shield (도발 방패) | 1 | 단일 적 ForcedTarget 1턴 + 자신 쉴드 +6 | Vengeance 5+ | 쉴드 **+6 + Vengeance/2** |
| Revenge Strike (복수의 일격) | 2 | 단일 (10 + Vengeance) 데미지, Vengeance 전량 소모 | (조건 없음) | — |
| Last Bastion (최후의 보루) | 3 | 모든 적 도발 1턴 + 본인 HP 25 회복 + 본인 쉴드 25 | ⚠️ 사용 제약: Vengeance 15+ 필수 | Vengeance 15 소모 |

📖 **상세 설계 (스토리/밸런스 곡선/시너지/연출/구현 명세)**: [Characters/Duran_the_Warrior.md](Characters/Duran_the_Warrior.md)

### 5.2 Ashe, the Pyromancer (화염 마법사) — "태우는 자가 아니라, 타는 자" ⭐신규 캐릭터

**정체성**: 자신이 타고 있는 자. 매 턴 스스로를 재로 만들며, 그 화력을 적/아군에게 전이.

**역할군**: 주=단일 폭딜러 / 부=지정 아군 힐

**고유 메카닉**: **Ember (자기 연소)** — 매 턴 +1 자동 부여(강제), 턴 종료 시 Ember×2 자기 피해, 최대 5.

**스킬 4종 (2:2 구조 — 충전 2 + 소비 2)**:

| 스킬 | AP | 기본 효과 (항상 발동) | Ember | 강화 조건 | 충족 보너스 |
|-----|-----|---------------------|-------|---------|-----------|
| Cinder Accretion (잿빛 응축) | 1 | 단일 5 + Burn 1스택 | +2 | 셋업 (조건 없음) | — |
| Phoenix Renewal (불사조 갱생) | 1 | 아군 1명 8 + Ember×3 힐 | +1 | 대상 HP 50% 이하 | 정화 추가 |
| Brand of Ash (잿더미 낙인) | 2 | 단일 8 + Ember×3 데미지 | -2 | 자신 HP 50% 이하 | 데미지 2배 |
| Embrace of Cinders (잔불의 포옹) | 3 | 단일 30 + Ember×15 + **자살** | -5 | ⚠️ Ember 5 필수 | 사망 패널티 면제 |

📖 **상세 설계 (스토리/밸런스/시너지/연출/시스템 명세)**: [Characters/Ashe_the_Pyromancer.md](Characters/Ashe_the_Pyromancer.md)

### 5.3 Cryomancer (냉기 마법사) — "통제형, Freeze 봉쇄" ⭐신규 캐릭터

**정체성**: 냉기 전문 마법사. Freeze로 적 행동을 차단하고, Glacial Spike로 결정타. 서포터-딜러 하이브리드.

**고유 메카닉: Frost (냉기 축적)**
- 냉기 스킬 사용 시마다 Frost +1 (최대 3)
- 3스택 도달 시 다음 냉기 마법이 **Freeze 1 → 2턴** 으로 강화 (자동 소비)
- 턴 종료 시 절반 소실

**스킬 4종 (전부 냉기) — 각각 다른 단일 조건**:

| 스킬 | 구성 (Frost 획득/소비) | 단일 조건 | 보상 | 역할 |
|------|------------------------|----------|------|------|
| **Frostbolt** | 단일 5 Freeze 1 AP1 (Frost +1) | (셋업 — 조건 없음) | Freeze 부여 | 셋업 |
| **Frost Armor** | 단일 쉴드 10 AP1 (Frost +1) | 대상이 이미 Freeze 상태 | 쉴드 +50% | 통제 정합 |
| **Blizzard** | 광역 4 Freeze 1 AP2 (Frost +1) | 적 3마리+ | Freeze 1턴 추가 | 군중 봉쇄 |
| **Glacial Spike** | 단일 12 AP3 (**Frost 전부 소비 → 0**) | 대상이 Freeze | 치명타 + AP 1 환급 (자동 강화 Freeze 1→2턴 병합) | Freeze 소비 |

**조건 다양성**:
- Frostbolt → 셋업 (조건 없음)
- Frost Armor → 대상 Freeze 상태 (자신/아군)
- Blizzard → 적 수
- Glacial Spike → 대상 Freeze
- → 셋업 / 대상 상태 / 적 수 / 대상 상태(Freeze 소비)

**파티 시너지**: Frostbolt → Glacial Spike 자가 콤보 가능 (같은 적 Freeze 후 치명타). Healer와 협업하여 파티 보호. Healer Mass Heal과 안 겹침.

### 5.4 Stormcaller (번개 마법사) — "연쇄 딜러, 다수전 특화" ⭐신규 캐릭터

**정체성**: 번개 전문 마법사. 다수 적에게 연쇄 데미지를 입히며, 궁극으로 폭풍을 소환. 광역 딜의 스페셜리스트.

**고유 메카닉: Storm (전하 축적)**
- 번개 스킬 사용 시마다 Storm +1 (최대 3)
- 3스택 도달 시 다음 번개 마법이 **추가 타겟 +1** (자동 소비)
- 턴 종료 시 절반 소실

**스킬 4종 (전부 번개) — 각각 다른 단일 조건**:

| 스킬 | 구성 (Storm 획득/소비) | 단일 조건 | 보상 | 역할 |
|------|------------------------|----------|------|------|
| **Lightning Bolt** | 단일 7 Stun 1 AP2 (Storm +1) | (셋업 — 조건 없음) | Stun 부여 | 셋업 |
| **Static Field** | 광역 AtkDown AP2 (Storm +1) | 적 3마리+ | 추가 DefDown | 군중 약화 |
| **Chain Lightning** | 광역 5 AP2 (Storm +1) | 대상이 상태이상 | 연쇄 데미지 +3 | 약점 exploited |
| **Thunderstorm** | 광역 10 AP3 (**Storm 전부 소비 → 0**) | Storm 3스택 | AP 2 환급 + Stun 1 추가 (자동 강화 추가 타겟 +1 병합) | 자원 소비 |

**조건 다양성**:
- Lightning Bolt → 셋업 (조건 없음)
- Static Field → 적 수
- Chain Lightning → 대상 상태이상 (다른 마법사/도트가 만든 상태)
- Thunderstorm → 자원 (Storm)
- → 셋업 / 적 수 / 대상 상태 / 자원. 4개 다름.

**파티 시너지**: Pyromancer/Cryomancer가 만든 Burn/Freeze를 Chain Lightning이 활용 (연쇄 데미지). 다수전에서 Archer Volley와 부분 중복 but 차별화 (Archer는 단일 중심, Stormcaller는 광역 전문).

### 5.5 마법사 3종 비교 요약

| 마법사 | 컨셉 | 핵심 메카닉 | 강점 | 약점 |
|--------|------|------------|------|------|
| **Pyromancer** | 도트 딜러 | Pyre | 지속 데미지, Alch 시너지 | 단일 폭딸 약함 |
| **Cryomancer** | 통제 | Frost | 행동 차단, 자가 콤보 | 도트 약함 |
| **Stormcaller** | 다수 광역 | Storm | 광역 연쇄, 약점 exploited | 단일전 약함 |

세 마법사는 서로 **다른 상황에서 빛남**:
- 다수전 + 도트 → Pyromancer
- 보스 통제 → Cryomancer
- 다수 광역 + 약점 exploited → Stormcaller

파티 조합에 따른 시너지:
- **Pyromancer + Stormcaller**: Burn 도트를 Stormcaller Chain이 활용
- **Cryomancer + Stormcaller**: Freeze를 Stormcaller chain이 치명타로 exploited
- **Pyromancer + Cryomancer**: 도트 + 통제, 다방면 대응

### 5.6 Healer (힐러) — "생명 순환 서포터"

**고유 메카닉: 생명 결속(Life Bond)**
- 이번 턴 힐을 받은 아군의 다음 공격 +3
- Healer 본인이 힐을 줄 때마다 자신에게 "신성 에너지" +1 (최대 3)
- 신성 에너지 3 도달 시 다음 힐량 2배

**스킬 리워크**:

| 스킬 | 구성 | 조건 | 보상 |
|------|------|------|------|
| **Heal** | 단일 힐 12 AP2 | 대상이 도트 디버프 | 정화 동시 (무료 Purify) |
| **Holy Shield** | 단일 쉴드 10 AP1 | 대상이 도발 중 | 쉴드 +50% |
| **Mass Heal** | 광역 힐 8 AP3 | 아군 3명+ 부상 | AP 1 환급 |
| **Benediction** | 단일 ATK+3 영구 AP2 | 대상이 이번 턴 힐 받음 | 효과 2배 (ATK+6) |

**플레이 루프**: 도트 디버프 걸린 아군에게 Heal로 정화+힐 동시 → 다음 턴 그 아군 Benediction으로 영구 강화. Healer가 "지우개+버퍼"로 다층 역할.

### 5.7 Rogue (도적) — "콤보 적재 암살자"

**고유 메카닉: 콤보 포인트(Combo Point)**
- 스킬 사용 시 +1 콤보 (최대 5)
- 콤보 5에서 finisher 스킬 사용 시 데미지 3배 + 콤보 소모
- 대상이 교체되면 콤보 절반 유지 (디버프 걸린 대상 유지 시 전부 유지)

**스킬 4종 — 각각 다른 단일 조건** (섹션 4.5 원칙 준수):

| 스킬 | 구성 (콤보 획득/소모) | 단일 조건 | 보상 | 역할 |
|------|----------------------|----------|------|------|
| **Backstab** | 단일 7 AP2 (콤보 +1) | 대상 디버프 상태 | 데미지 2배 | 디버프 보상 |
| **Poison Blade** | 단일 3 Poison AP1 (콤보 +1) | (셋업 — 조건 없음) | Poison 부여 | 셋업 |
| **Rupture** | 단일 4 Bleed AP1 (콤보 +1) | 대상 HP 50%- | 도트 지속 +2턴 | 위기 가속 |
| **Eviscerate** (finisher) | 단일 10 AP3 (**콤보 전부 소모 → 0**) | 콤보 5 | 데미지 3배 + 치명타 | 자원 소비 |

**조건 다양성 검증**:
- Backstab → 대상 상태 (디버프)
- Poison Blade → 셋업 (조건 없음)
- Rupture → 대상 HP 임계
- Eviscerate → 자원 (콤보)
- → 4개 모두 다른 조건. 암살 루프 안에서도 매 턴 다른 퍼즐.

**플레이 루프**: Poison Blade로 디버프+콤보 축적 → Backstab 2타 (디버프 보너스) → 콤보 5 도달 → Eviscerate 결정타. Rupture는 적 약해질 때 가속.

### 5.8 Archer (궁수) — "표식 사냥꾼"

**고유 메카닉: 표식(Hunter's Mark) 강화**
- Archer가 디버프를 걸면 자동으로 Mark 부가 (이미 Mark 시 위력 50%)
- Mark 걸린 적은 모든 파티원이 +20% 데미지
- 적 1명에게 Mark 3스택 시 그 적 사망 시 AP +1

**스킬 4종 — 각각 다른 단일 조건** (섹션 4.5 원칙 준수):
- 기존 설계는 3개 스킬이 모두 "대상 Mark" 조건 중복 → 매 턴 같은 퍼즐 문제. 재설계.

| 스킬 | 구성 | 단일 조건 | 보상 | 역할 |
|------|------|----------|------|------|
| **Hunter's Mark** | 단일 Mark + Def-2 AP1 | (셋업 — 조건 없음) | Mark 부여 + 파티 +20% | 셋업 |
| **Piercing Shot** | 단일 14 AP2 | 대상이 Mark | 데미지 2배 | Mark 소비 |
| **Volley** | 광역 6 AP2 | 적 3마리+ | 한 발 추가 (광역 틱) | 군중 제압 |
| **Crippling Shot** | 단일 12 Stun 1 AP3 | 대상 상태이상(Burn/Poison/Freeze/Stun 중 하나) | 치명타 + AP 1 환급 | 도트 마무리 |

**조건 다양성 검증**:
- Hunter's Mark → 셋업 (조건 없음)
- Piercing Shot → 대상 상태 (Mark)
- Volley → 적 수
- Crippling Shot → 대상 상태이상 (Mark 아닌 다른 상태)
- → 4개 모두 다른 조건. 매 턴 다른 퍼즐.

**매 턴 다른 퍼즐 예시**:
- `Hunter's Mark + Piercing Shot` 뽑힘 → Mark 걸고 2배 데미지 콤보
- `Volley + Crippling Shot` 뽑힘 → 적 3마리+일 때 Volley로 상태이상 묻히고 Crippling으로 치명타
- `Piercing Shot 단독` 뽑힘 → Mark 안 걸려 있으면 기본 데미지 (약함)
- `Crippling Shot 단독` 뽑힘 → 도트가 없으면 치명타/환급 없음 (단순 스턴)

**플레이 루프**: Hunter's Mark로 표식 → 파티원(Alch/Pyromancer)이 도트 묻힘 → Piercing이나 Crippling으로 마무리. 궁수가 "지휘 + 마무리" 이중 역할.

### 5.9 Necromancer (네크로맨서) — "진짜 소환 + 영혼 수확"

**고유 메카닉: 미니언(Minion) 시스템**
- Raise Dead 사용 시 미니얼 1마리 소환 (HP 15, 한 턴 지속)
- 미니언은 다음 적 턴에 한 대 맞아줌 (대신 사망)
- 도트 데미지로 적 처치 시 "영혼" +1 (최대 3), 소환 비용 -1에 사용 가능

**스킬 리워크**:

| 스킬 | 구성 | 조건 | 보상 |
|------|------|------|------|
| **Life Drain** | 단일 10 AP2 | 대상이 디버프 | 흡혈량 2배 |
| **Curse of Frailty** | 단일 AtkDown AP1 | 대상이 이미 Curse | 추가 DefDown |
| **Soul Harvest** | 단일 12 AP3 | 적 처치 시 | AP 2 환급 + 영혼 +1 |
| **Raise Dead** | 미니언 소환 AP3 | 영혼 1+ 보유 | AP 1 환급 |

**플레이 루프**: Curse → Decay(도트) → Life Drain 회복 → 적 약해지면 Soul Harvest로 처치+AP 회수 → 회수한 AP/영혼으로 Raise Dead. "서서히 약화시키며 자원 회수" 사이클.

### 5.10 Alchemist (연금술사) — "시약 반응 촉매자"

**고유 메카닉: 시약 반응(Reagent Reaction)**
- 같은 대상에게 두 번째 물약 사용 시 효과 2배
- 도트 디버프 2종+ 적에게 "연쇄 반응" 추가 데미지
- 한 턴에 3물약 사용 시 다음 턴 AP +1

**스킬 4종 — 각각 다른 단일 조건** (섹션 4.5 원칙 준수):
- 기존 Poison Bomb "Poison+Burn 동시"는 규칙 1(단일 조건) 위반 → 재설계.

| 스킬 | 구성 | 단일 조건 | 보상 | 역할 |
|------|------|----------|------|------|
| **Heal Potion** | 단일 힐 10 AP2 | 대상 도트 상태 | 힐량 2배 (해독제 겸용) | 대상 상태 |
| **Poison Bomb** | 광역 6 Poison AP2 | 적 3마리+ | 독 범위 확대 (+1마리) | 적 수 |
| **Catalyst** | 단일 버프 AP1 | 대상 물약 효과 받음 | 효과 2배 | 대상 이력 |
| **Mega Bomb** | 광역 12 Burn AP3 | 이번 턴 자신 물약 2+회 사용 | AP 1 환급 | 행동 이력 |

**조건 다양성 검증**:
- Heal Potion → 대상 상태 (도트)
- Poison Bomb → 적 수
- Catalyst → 대상 이력 (물약 받음)
- Mega Bomb → 자신 행동 이력
- → 4개 모두 다른 조건.

**플레이 루프**: Healer와 차별화 — Healer는 "정화+힐", Alch는 "도트 딜러+힐". Poison Bomb으로 도트 깔고 Catalyst로 버프 강화, Mega Bomb로 마무리+AP 회수. Pyromancer Spark와 연쇄 반응 일으키는 파티 시너지.

### 5.11 Bard (음유시인) — "리듬과 화음의 지휘자"

**고유 메카닉: 리듬(Rhythm) 시스템**
- 스킬 사용 시마다 리듬 +1 (최대 4)
- 리듬 4 도달 시 "피날레" 발동 가능 — 다음 곡 효과 3배
- 곡을 사용하지 않은 턴에 리듬 절반 유지

**스킬 4종 — 각각 다른 단일 조건** (섹션 4.5 원칙 준수):
- 기존 Battle Song(리듬 3+) / Grand Finale(리듬 4 소모) 모두 리듬 자원 의존 → 조건 중복. Dissonance "디버프 2개+" 다중 조건 → 재설계.

| 스킬 | 구성 (리듬 획득/소모) | 단일 조건 | 보상 | 역할 |
|------|----------------------|----------|------|------|
| **Battle Song** | 광역 AtkUp AP2 (리듬 +1) | (셋업 — 조건 없음) | AtkUp 부여 | 셋업 |
| **Dissonance** | 광역 AtkDown AP2 (리듬 +1) | 적 3마리+ | 광역 디버프 + 추가 DefDown | 군중 약화 |
| **Courage Chord** | 단일 힐 8 AP2 (리듬 +1) | 대상 곡 효과 받는 중 | 힐량 2배 | 버프 정합 |
| **Grand Finale** | 광역 버프+힐 AP4 (**리듬 전부 소모 → 0**) | 리듬 4 | 다음 턴 파티 전체 AtkUp+3, DefUp+3 | 자원 소비 |

**조건 다양성 검증**:
- Battle Song → 셋업 (조건 없음)
- Dissonance → 적 수
- Courage Chord → 대상 상태 (곡 효과)
- Grand Finale → 자원 (리듬)
- → 4개 모두 다른 조건.

**플레이 루프**: Battle Song으로 리듬 축적 + AtkUp → Courage Chord로 버프받은 아군 힐 강화 → Dissonance로 군중 약화 → 리듬 4 도달 시 Grand Finale로 터뜨리기. 곡 사용이 셀프 시너지.

### 5.12 Sibyl, the Oracle (예언자) — "보되 바꿀 수 없는 자" ⭐11번째 신규 캐릭터 (2026-06-30)

**정체성**: 미래를 보되 바꿀 수 없는 예언자. 모든 스킬이 **"1AP + 1턴 뒤 발동"** 패턴 — **"미래에 투자"**하는 유일한 캐릭터.

**역할군**: 주=서포터 / 부=디버퍼·finisher

**고유 메카닉: Prophecy (예언 — 지연 발동)**
- 모든 스킬이 "예언 1" (1턴 뒤, 즉 다음 턴 시작 시 발동)
- 매 턴 시작 시 보류 예언 카운트 -1, 0 도달 시 자동 발동
- 한 타겟에 여러 예언 중첩 가능. 타겟 사망 시 소멸.
- **위력 보상**: 동급 일반 스킬 대비 약 +50~100% (1턴 지연 페널티 보상)

**스킬 4종 (공통: 1AP + 1턴 뒤)**:

| 스킬 | 타겟 | 기본 효과 | 강화 조건 후보 |
|------|------|---------|---------------|
| **죽음의 예언** (Death Prophecy) | 단일 적 | HP 임계 이하 처형 | 도트 디버프 시 임계값 +N |
| **갱생의 환영** (Vision of Renewal) | 단일 아군 | 회복 | 대상 HP 50% 이하 시 힐 2배 |
| **미래 차용** (Borrowed Future) | 자신 | 다음 턴 AP +2 | ⚠️사용 제약 필요 (런 N회 또는 다른 예언 보류 시) |
| **데자부** (Déjà Vu) | 자신 | 직전 턴 예언 스킬 비용 0 재시전 | 사용 제약: 직전 턴 스킬 존재 |

**특성 3종**:

| 슬롯 | 특성 | 메커니즘 |
|------|------|---------|
| **기본** | **죽음의 표식** (Mark of Doom) | 매 턴 시작 시 무작위 적 표식. 이번 턴 안에 사망 시 → 무작위 적 피해 + 무작위 아군 회복 |
| **메타1** | **운명의 손** (Hand of Fate) | 3턴마다 Sibyl 스킬 1개 무작위 자동 시전 (비용 0, 대상 무작위) |
| **메타2** | **시간 붕괴** (Time Collapse) | 3턴마다 **이번 턴 Sibyl이 AP를 사용해 시전하는 스킬이 카운트 없이 즉시 발동** (Hand of Fate와 같은 턴) |

**3턴 주기 콤보** (Sibyl 숙련도의 핵심):
```
턴 1-2: 예언 스킬 여러 개 시전 (1턴 뒤 발동 예정)
턴 3: ⚡ 시간 붕괴 + ⚡ Hand of Fate 동시 발동
      → 미래에 걸어둔 예언들 + 무작위 스킬이 한꺼번에 도래 → 폭발 턴
턴 4: 새 사이클 시작
```

**조건 다양성 검증 (규칙 2)**: 스킬 자체는 모두 "셋업"에 가까우나, 강화 조건(대상 상태/HP/자원/이력)과 특성 3종(무작위 사망/3턴 주기/3턴 주기+AP 사용)이 매 턴 다른 퍼즐 생성 → **스킬 + 특성 합산 시 조건 다양성 충족**.

**다른 서포터와의 차별화**: Healer="현재 힐", Bard="리듬 버프", Sibyl=**"미래 힐/딜"** — 타이밍 축에서만 작동하여 역할 충돌 없음.

**강력한 파티 시너지**:
- **Sibyl + Ashe (Pyromancer)** ★★★★★ — Burn 도트 → 죽음의 예언 임계값 보정 → 보스 처치 콤보
- **Sibyl + Lumi (Cryomancer)** ★★★★★ — Freeze로 적 봉쇄 → 1턴 뒤 Sibyl 예언 안전 발동
- **Sibyl + Duran (Warrior)** ★★★★ — ForcedTarget으로 적 고정 → Sibyl 처형 확정 타겟

**편입 방식**: 11번째 캐릭터 (신규). 해금 조건 추천 = **어센션 5 달성 시** (숙련자용 메카닉). CharacterSelectUI 10→11 대응 필요.

📖 **상세 설계 (스토리/밸런스/3턴 콤보 시나리오/어센션/구현 명세)**: [Characters/Sibyl_the_Oracle.md](Characters/Sibyl_the_Oracle.md)

### 5.13 Taranis, the Stormcaller (폭풍 소환사) — "때리지 않는 자, 길만 만드는 자" ⭐12번째 신규 캐릭터 (2026-06-30)

**정체성**: 번개의 혼돈을 도면으로 옮기는 설계자. **직접 때리지 않고**, 적들 사이에 전하 네트워크를 깔아두며, 적이 스스로를 지지게 놔두고, 적의 공격마저 네트워크의 연료로 삼는 "간접 딜러".

**역할군**: 주=광역 딜러 (네트워크 기반 도트) / 부=서포터 (접지 장벽)

**고유 메카닉: Charge Network (전하 네트워크)**
- 전하(Charge) 부여 시 **자동 전파** (다른 적 1명, 전하 보유 적 우선)
- **매 턴 종료 시 자동 연쇄** — 각 전하 적이 자신의 스택 수만큼 다른 전하 적에게 번개 도트 (1스택당 고정값 1)
- **2턴마다 자연 소멸** (-1스택)
- 한 적당 최대 3스택. 보스전(단일)은 연쇄 대상 없어 자동 약화

**스킬 4종 (직접 딜 스킬 없음)**:

| 스킬 | AP | 메커니즘 |
|------|----|---------|
| **Wire (와이어)** | 1 | 단일 적 전하 2스택 부여 + 전파 |
| **Branch (브랜치)** | 2 | 광역(모든 적) 전하 1스택 부여 + 전파 |
| **Grounding Field (접지 장벽)** | 2 | 파티 전체 쉴드 + **쉴드를 때린 적에게 전하 부여** (적 공격 역이용) |
| **Thunderstorm (뇌우)** | 3 | 광역 데미지 + 모든 적 전하 3스택(풀충전) 부여 |

**특성 3종**:

| 슬롯 | 특성 | 메커니즘 |
|------|------|---------|
| **기본** | **Compounding (누적 가속)** | 같은 적에게 매 턥 도트가 누적될수록 위력 증가. "오래 살려둘수록 더 아프다" |
| **메타1** | **Thunder God (뇌신)** | 매 턴 시작 시 모든 적에게 전하 1스택 부여 (런 3회) |
| **메타2** | **Superconductor (초전도체)** | 연쇄 도트가 적 DEF 무시. 누적 가속과 완벽 시너지 |

**핵심 플레이 루프 — "네트워크 설계 → 유지 → 보상"**:
```
[다수전] Wire/Branch로 전하 확산 → 매 턥 자동 연쇄로 도트 누적
         + Grounding Field로 적 공격을 흡수하며 추가 전하 부여
         → Compounding 특성으로 도트가 점점 강해짐
[보스전] 단일이라 연쇄 약함 → 대신 Grounding Field로 파티 보호 (서포터 전환)
         + Superconductor 특성으로 DEF 무시 도트 보강
```

**조건 다양성 검증 (규칙 2)**: 스킬 자체는 셋업 중심이나, **Branch 강화(네트워크 크기) + Grounding Field 자동 보상(적 공격 흡수)**가 매 턥 다른 상황 생성 → 메카닉이 퍼즐 다양화.

**마법사 3종 삼각 완성**: Ashe(자해 폭딜) / Lumi(통제) / **Taranis(네트워크 딜+서포터)** — 딜/통제/네트워크 축. 파티 3인 시너지 (Ashe Burn + Lumi Freeze + Taranis Charge)의 정점.

**편입 방식**: 12번째 캐릭터 (신규). 해금 조건 추천 = **F3 보스 클리어 시** (다수전 특화라 F3부터 자연스러움, Lumi F2 해금과 연속). CharacterSelectUI 11→12 대응.

📖 **상세 설계 (스토리/밸런스/네트워크 메카닉/어센션/구현 명세)**: [Characters/Taranis_the_Stormcaller.md](Characters/Taranis_the_Stormcaller.md)

---

## 6. 조건부 특성 설계 (기존 24종 리워크)

> 현재 24 특성(8캐릭터 × 3)은 KeywordEntry 기반 단순 버프. 이를 **조건부 키워드**로 리워크한다.

### 6.1 특성 리워크 예시 (10캐릭터별 3종 = 30종)

**Warrior** (3종):
- `Trait_Berserker` (기본): HP 50% 이하 시 ATK +4
- `Trait_IronWill`: 쉴드 10+ 보유 시 첫 공격 데미지 50% 감소 (Sturdy 강화)
- `Trait_Protector`: 도발한 적이 다른 아군을 공격할 때 (도발 실패) 분노 +2 + 그 적에게 다음 공격 +50% (도발 몰이 정체성 강화)

**Pyromancer** (3종):
- `Trait_Kindling` (기본): Burn 도트 데미지 +30%
- `Trait_Pyromaniac`: Pyre 3스택 도달 시 다음 화염 마법 2회 발동 (1런 3회)
- `Trait_FlashFire`: 대상이 이미 Burn일 때 Spark 데미지 추가 +50%

**Cryomancer** (3종):
- `Trait_PackIce` (기본): Freeze 지속시간 +1턴
- `Trait_GlacierWalk`: Frost 3스택 도달 시 모든 적 Freeze 1턴 (1런 2회)
- `Trait_ColdBlooded`: Freeze 걸린 적에게 치명타 확률 +50%

**Stormcaller** (3종):
- `Trait_Voltage` (기본): Storm 스택당 번개 데미지 +10%
- `Trait_Overload`: Storm 3스택 도달 시 번개 마법이 추가 타겟 2개 (1턴)
- `Trait_Grounded`: 적 3마리+일 때 번개 마법 AP -1

**Rogue** (3종):
- `Trait_Assassin` (기본): 디버프 걸린 적에게 +25% 데미지
- `Trait_ComboMaster`: 콤보 5 도달 시 AP +1 즉시
- `Trait_Shadowstep`: 첫 턴에 은신 (1회 피격 면역)

**Archer** (3종):
- `Trait_Hunter` (기본): Mark 걸린 적에게 +30% 데미지
- `Trait_EagleEye`: 적 HP 30% 이하 시 치명타 100%
- `Trait_VolleyMaster`: 광역 스킬 사용 시 1발 추가

**Healer** (3종):
- `Trait_Mercy` (기본): 도트 디버프 걸린 아군 힐 시 정화 동시
- `Trait_DivineFury`: 이번 턴 힐 받은 아군 다음 공격 +3
- `Trait_Martyr`: 자신 HP 30% 이하 시 힐 효율 2배

**Necromancer** (3종):
- `Trait_SoulCollector` (기본): 적 처치 시 영혼 +1
- `Trait_Plaguebringer`: 도트 디버프 2개+ 적에게 +30% 데미지
- `Trait_DeathTouch`: 적 HP 20% 이하 즉시 처치 (보스 제외, 1런 3회)

**Alchemist** (3종):
- `Trait_Catalyst` (기본): 같은 대상 2번째 물약 효과 2배
- `Trait_MadScientist`: 한 턴 3물약 사용 시 다음 턴 AP +1
- `Trait_Toxicologist`: Poison 5스택+ 적에게 +50% 데미지

**Bard** (3종):
- `Trait_Virtuoso` (기본): 리듬 4 도달 시 피날레 가능
- `Trait_Encore`: 곡 효과 받은 아군이 공격 시 +2 데미지
- `Trait_Maestro`: 버프 3스택+ 아군 있을 때 AP +1/턴

### 6.2 메타 해금 특성 (신규 10종 — 1캐릭터당 1개)

> 기존 24종(3/캐릭터) 외에 **메타 해금 4번째 특성** 추가. 각 캐릭터의 "극단적 플레이 스타일" 강제.

- `Trait_Warlord` (Warrior): 분노 최대 10스택, 스택당 +3 (대신 시작 분노 0에서 회복 불가)
- `Trait_InfernoLord` (Pyromancer): Pyre 최대 5스택, 5스택 시 광역 화염 (대신 비화염 스킬 사용 시 Pyre 리셋)
- `Trait_AbsoluteZero` (Cryomancer): Freeze 2턴 기본 (Frost 0에서도), 모든 적에게 Freeze 적용 시 추가 데미지
- `Trait_LightningGod` (Stormcaller): Storm 최대 5스택, 5스택 시 번개 마법이 모든 적 타격 (대신 단일 데미지 -20%)
- `Trait_Phantom` (Rogue): 콤보 최대 10, 10스택 finisher 데미지 5배 (대신 AP +1 비용)
- `Trait_Sniper` (Archer): Mark 5스택 가능, 5스택 시 다음 공격 AP 0 (대신 Mark 안 걸면 데미지 -30%)
- `Trait_Savior` (Healer): 도트 디버프 면역 부여 가능 (대신 자신 최대 HP -20)
- `Trait_Lich` (Necromancer): 미니언 영구 지속 (대신 본인 HP -1/턴)
- `Trait_Bomber` (Alchemist): 도트 디버프 틱마다 폭발 +2 데미지 (대신 자신 도트 면역 불가)
- `Trait_Conductor` (Bard): 피날레 시 전체 버프 2턴 (대신 리듬 5 필요)

---

## 7. 조건부 유물 설계 (신규 10종)

> 기존 42종 유물(기본 16 + 시너지 26)은 트리거 기반 패시브. 신규 10종은 **명시적 조건 + 강력 보상** 조합.

### 7.1 AP 환급 / 재시전 계열 (3종)

| 유물 | 조건 | 보상 |
|------|------|------|
| **Relic_EchoChamber** | 같은 스킬 타입 3연속 사용 | 다음 스킬 AP 0 |
| **Relic_Momentum** | 한 턴에 3스킬 사용 | AP +1 (한 턴 1회) |
| **Relic_TimePiece** | 첫 턴에만 | AP +2 (보스전 강화) |

### 7.2 상황 반응 계열 (4종)

| 유물 | 조건 | 보상 |
|------|------|------|
| **Relic_Predator** | 적 HP 30% 이하 처치 | 골드 +20 |
| **Relic_Bloodlust** | 자신 HP 20% 이하 | 모든 공격 치명타 |
| **Relic_LastBreath** | 쉴드 파괴 시 | 즉시 쉴드 20 + ATK +3 (1턴) |
| **Relic_Phalanx** | 같은 턴 2회 이상 피격 | 쉴드 +15 + 분노/콤보/공명 스택 +1 |

### 7.3 스택 / 누적 계열 (3종)

| 유물 | 조건 | 보상 |
|------|------|------|
| **Relic_TrophyRack** | 적 처치 누적 | 처치당 영구 ATK +1 (최대 10) |
| **Relic_Perfectionist** | 한 턴 피격 0회 | 다음 턴 AP +1 |
| **Relic_Unbroken** | 3연속 턴 피격 0 | 영구 DefUp +2 |

---

## 8. 시스템 구현 관점

### 8.1 SkillData 확장 (필수)

```csharp
// 신규 필드 — SkillData.cs
[Header("조건부 효과")]
[SerializeField] private SkillConditionType _conditionType;     // None = 조건 없음
[SerializeField] private int _conditionValue;                  // 임계값 (HP %, 스택 수 등)
[SerializeField] private StatusEffectType _conditionStatus;    // OnStatus용
[SerializeField] private ConditionalBonusType _bonusType;      // 보상 유형
[SerializeField] private int _bonusValue;                      // 보상 수치

public enum SkillConditionType {
    None, OnStatus, OnHpThreshold, OnActionPerformed, OnTurnOrder,
    OnEnemyCount, OnStack, OnTurnPhase, OnEvent
}

public enum ConditionalBonusType {
    None, DamageMultiplier, RefundAP, Replay, PermanentBuff, AddStatusEffect
}
```

### 8.2 SkillExecutor 통합

- `EvaluateCondition(condition, caster, target, turnContext)` 헬퍼 추가
- 스킬 실행 후 조건 충족 시 `ApplyBonus(bonusType, value)` 호출
- AP 환급은 TurnManager.CurrentAP에 직접 가산
- 재시전은 재귀 호출 (무한루프 방지: 1회 only 플래그)

### 8.3 캐릭터 고유 메카닉 통합

각 메카닉은 Character 클래스에 새 컴포넌트로 분리 (EnemyTraitHandler 패턴 차용):

```csharp
// 신규 — Characters/Components/
public class WarriorRageComponent { int _stacks; ... }
public class PyromancerPyreComponent { int _stacks; ... }
public class CryomancerFrostComponent { int _stacks; ... }
public class StormcallerStormComponent { int _stacks; ... }
public class RogueComboComponent { int _stacks; ... }
public class BardRhythmComponent { int _rhythm; ... }
public class NecromancerMinionComponent { List<Minion> _minions; ... }
```

CharacterTraitHandler가 CombatEventBus 구독하듯, 각 컴포넌트도 OnDamageTaken/OnSkillUsed 등을 구독.

### 8.3.1 ForcedTarget 디버프 (Warrior Bodyblock용)

Warrior의 두 가지 도발 전략을 구현하려면 **단일 적 고정** 메카닉이 필요:

```csharp
// StatusEffectType enum 신규 항목
ForcedTarget    // 단일 적이 지정된 캐릭터만 공격하도록 강제 (effectValue = 타겟 캐릭터 ID)
```

**기존 Taunt와의 차이**:

| 상태 | 적용 대상 | 효과 | 사용 스킬 |
|------|----------|------|----------|
| `Taunt` | 플레이어 자신 | 모든 적이 그 플레이어 우선 타겟 | Warrior_Taunt (광역) |
| `ForcedTarget` | 적 1명 | 그 적만 지정된 플레이어 공격 | Warrior_Bodyblock (단일) |

**EnemyAIController.SelectRandomAlivePlayer 확장** (기존 L139-160):
```csharp
// 현재: Taunt 걸린 플레이어 우선
// 확장: 이 적이 ForcedTarget 디버프를 가지고 있으면 지정된 타겟强制
if (_owner.StatusEffects.HasEffect(StatusEffectType.ForcedTarget))
{
    int targetId = _owner.StatusEffects.GetEffectValue(StatusEffectType.ForcedTarget);
    var forced = alive.Find(p => p.Id == targetId);
    if (forced != null) return forced;
}
// 기존 Taunt 로직은 그대로 유지
```

**Opportunist 특성 정책**:
- 기존: Taunt 무시 (최저 HP 타겟)
- 확장: ForcedTarget도 무시 (일관성) — Opportunist는 어떤 어그로 효과든 관통

**게임 디자인적 의미**:
- `Taunt` = "전체 어그로" (다수 적 상대로 효율)
- `ForcedTarget` = "정밀 탱킹" (보스 1마리 상대로 효율)
- 둘은 **상호 배타적이지 않음** — 같은 턴 Taunt(자신) + Bodyblock(적에게 ForcedTarget) 둘 다 가능. 하지만 AP 부족으로 보통은 하나만 택일.

### 8.4 UI 표시

- 캐릭터 패널에 고유 메카닉 게이지 (분노/콤보/리듬 등)
- 스킬 카드에 조건부 효과 텍스트 (회색 → 충족 시 골드)
- 툴팁에 조건 설명

### 8.5 밸런스 시뮬레이터 확장

- BalanceSimulator.SimulatedPlayerAI에 "조건 의사결정" 추가
- SimulatedCharacter에 고유 메카닉 스택 시뮬레이션
- 어센션 레벨별 조건부 승률 곡선 측정

---

## 9. 구현 로드맵 (7단계 — CC-Sibyl + CC-Taranis 추가)

### Phase CC-0 (P0, 선행) — 부활 시스템 기반 (★모든 조건부 메카닉의 전제)

**목표**: 사망 트리거 디자인을 가능하게 하는 기반 시스템. Phase CC-1 이전에 완료되어야 함.

- `HealthComponent.Revive(int hp)` 메서드 추가
- `HealthComponent.ApplyMaxHpModifier(float mul)` 메서드 추가 (-10% 누적용)
- `GameRunState.ProcessBattleEnd()` — 살아남은 자 풀힐, 사망자 50% 부활 + MaxHP × 0.9
- `BattleSceneSetup.OnBattleEnded` (승리 분기) → ProcessBattleEnd 호출
- `CombatEventBus.OnPartyMemberRevived` 이벤트 추가
- SaveManager 호환성 — 기존 CurrentHP/MaxHP 저장 로직 그대로 작동 (부활 후 HP가 저장됨)
- UI: "부활!" 플로팅 텍스트 + MaxHP 감소 깜빡임 애니메이션
- **밸런스 조정**: 적 평균 데미지 +15~25% 상향 (사망 허용분량 확대)
- 단위 테스트 8개 (부활/누적 MaxHP 감소/풀힐/저장-로드 호환성)

**리스크**: 이 Phase가 완료되지 않으면 사망 트리거 특성/유물(Relic_Vengeance 등)이 활성화되지 않음.

### Phase CC-1 (P0, 1주) — 분노/콤보/Pyre 3개 메카닉 + 조건부 스킬 인프라

**목표**: 가장 영향 큰 3개 메카닉으로 "조건부 재미" 검증. Mage 분할(3종 마법사)을 이 Phase에 포함.

- SkillData에 SkillConditionType / ConditionalBonusType 필드 추가
- SkillExecutor.EvaluateCondition / ApplyBonus 구현
- Warrior 분노 스택 (RageComponent)
- Rogue 콤보 포인트 (ComboComponent)
- **Mage 제거 + Pyromancer 신규 캐릭터** (PyromancerPyreComponent, 스킬 4종, CharacterTable/DataGenerator/SaveManager 호환성 처리)
- Cryomancer/Stormcaller는 Phase CC-1B에서 추가 (씬 에셋, 해금 조건 등 부가 작업)
- 3캐릭터 스킬 4종 리워크 (CSV + DataGenerator)
- 단위 테스트 15개

### Phase CC-1B (P0, 1주) — Cryomancer + Stormcaller 추가

- Cryomancer (Frost 메카닉, 스킬 4종)
- Stormcaller (Storm 메카닉, 스킬 4종)
- 3원소 마법사간 시너지 검증 (Burn/Freeze 연쇄)
- 마법사 3종 해금 정책:
  - Pyromancer: 기본 캐릭터 (Mage 슬롯 계승)
  - Cryomancer: F2 보스 클리어 해금 (기존 Necromancer 해금 조건과 조정 필요)
  - Stormcaller: F3 보스 클리어 해금
- 캐릭터 선택 UI 8→10 대응

### Phase CC-Taranis (P0, 1주) — Taranis, the Stormcaller 12번째 캐릭터 + Charge Network 메카닉 ⭐신규 (2026-06-30 기획 완료)

**목표**: "네트워크에 투자"하는 공간 축 메카닉 도입. 마법사 3종 삼각(Ashe/Lumi/Taranis) 완성. 12번째 캐릭터 편입.

- **신규 인프라**:
  - `ChargeNetworkComponent` 신설 — 전하 부여/전파/매 턴 종료 자동 연쇄/2턴마다 자연 소멸
  - `StatusEffectType` 신규 항목: `Charge` / `GroundingShield` / `Compounding` / `ThunderGodProc`
  - `SkillData` 신규 필드: `_chargeStacksApplied` / `_triggersPropagation` / `_grantsShieldToParty` / `_chargesOnShieldAbsorb`
  - TurnManager.OnTurnEnd에 자동 연쇄 훅
  - HealthComponent.OnShieldAbsorbed 이벤트 확장 (공격자 정보 포함 → 접지 장벽 전하 역부여)
- **Taranis 캐릭터 에셋**: CharacterTable/SkillTable 신규 1+4행 (Char_Stormcaller + Storm_Wire/Branch/GroundingField/Thunderstorm)
- **특성 3종**: Trait_Compounding(기본) / Trait_ThunderGod(메타1) / Trait_Superconductor(메타2)
- **접지 장벽 특수 로직**: 쉴드 흡수 시 공격자에게 전하 자동 부여 — 적 intent 공개 시스템과 찰떡
- **해금 조건**: F3 보스 클리어 시 (다수전 특화라 F3부터 자연스러움, Lumi F2 해금과 연속)
- **씬 통합**: CharacterSelectUI 11→12, CharacterTraitSelectUI, DataGenerator 12번째 분기
- 단위 테스트 14개 (전하 부여/전파/매 턴 연쇄/자연 소멸/접지 장벽 역부여/Thunder God 자동 발동/Compounding 누적/Superconductor DEF 무시/보스전 연쇄 0 약점 등)
- **리스크**: 다수전 폭발 밸런스 (4마리 네트워크), 접지 장벽 사기 가능성 — 도트 단위 1 + 자연 소멸 2턴으로 완화

📖 **상세 기획**: [Characters/Taranis_the_Stormcaller.md](Characters/Taranis_the_Stormcaller.md)

### Phase CC-2 (P1, 1주) — 나머지 4개 메카닉 + 특성 리워크

- Healer 생명 결속, Archer 표식 강화, Necromancer 미니언/영혼, Alchemist 시약 반응, Bard 리듬
- 기존 21 특성(7캐릭터 × 3) + 마법사 3종 9특성 + **Sibyl 3특성 + Taranis 3특성 = 36특성** 조건부 키워드로 리워크
- 캐릭터 패널 메카닉 게이지 UI

### Phase CC-3 (P2, 1주) — 신규 유물 10종 + 메타 해금 특성 12종

- 조건부 유물 10종 DataGenerator 추가
- 4번째 메타 해금 특성 12종 (CharacterTraitData) — **Sibyl + Taranis 포함 12캐릭터**
- MetaShopUI 4번째 특성 탭 추가

### Phase CC-4 (P3, 1주) — 밸런스 튜닝 + 폴리싱

- BalanceSimulator 조건부 AI 확장
- 어센션 0/5/10/15에서 각 캐릭터 조합 승률 측정
- 조건부 툴팁/게이지 애니메이션
- 보스 12종과의 매칭 검증 (예: Rogue 콤보가 크라켄 Sturdy에 잘 통하는가)

---

## 10. 밸런스 검증 계획

### 10.1 핵심 KPI

- **턴 설계 보상도**: 조건부 충족률 60%+ (너무 쉬우면 의미 없음, 너무 어려우면 좌절)
- **캐릭터 픽 다양성**: 8캐릭터 픽률 분산 (특정 캐릭터 50%+集中 금지)
- **어센션 곡선**: 어센션 0/5/10/15 클리어율 70/50/30/15% (StS 기준)

### 10.2 시뮬레이터 확장 포인트

1. SimulatedCharacter에 분노/콤보/원소 공명 스택 추적
2. SimulatedPlayerAI 의사결정 트리 확장:
   - "디버프 걸린 적 우선 타겟" (Rogue Backstab 조건)
   - "같은 자원(Pyre/Frost/Storm) 3스택 후 Meteor/Thunderstorm" (마법사 3종)
   - "도발 후 Bodyblock" (Warrior)
3. 1000팩 × 4 어센션 레벨 = 4000팩 시뮬레이션

### 10.3 위험 시나리오

- **조건부 스킬이 안 쓰임**: 조건이 너무 까다로움 → 시뮬레이터 사용률 30% 미만 시 조건 완화
- **특정 조합 사기**: Pyromancer Pyre + Bard 리듬 → 1턴 킬 → 시뮬레이터 승률 90%+ 시 너프
- **AP 환급 무한 루프**: EchoChamber + 마법사 3종 자원 → AP 무한 → 시뮬레이터 턴당 스킬 7+ 시 제한

---

## 11. 부록 A — 부활 시스템 (★핵심 기반 시스템)

### 11.A.1 정책 (2026-06-20 확정)

**전투 종료 시 자동 처리**:

| 캐릭터 상태 | 처리 |
|-----------|------|
| 살아남은 캐릭터 | HP **100% 회복** (상태이상/쉴드 초기화) |
| 사망한 캐릭터 | **부활** + 현재 MaxHP의 **50%** 로 시작 + **MaxHP 영구 -10% 누적** (곱셈) |
| 연쇄 사망 | 매 전투마다 50% 리셋 + MaxHP -10% 추가 누적 |
| 보스전 | 일반전 동일 정책 |
| 런 종료 조건 | 파티 전원 동시 사망 (전멸) |

**MaxHP 누적 효과 (Warrior 120 기준, 곱셈 0.9^n)**:

| 사망 횟수 | MaxHP | 부활 시작 HP (50%) |
|----------|-------|------------------|
| 0 | 120 | — |
| 1 | 108 (-10%) | 54 |
| 2 | 97 (-19%) | 49 |
| 3 | 87 (-27%) | 44 |
| 5 | 71 (-41%) | 35 |
| 7 | 58 (-52%) | 29 |

**설계 의도**:
- 사망 1~2회는 감수 가능 (전투당 1명 희생 전략 허용)
- 사망 3회부터 누적 약화가 체감 → "사망 관리" 부담
- 사망 5회+ 사실상 전멸 위기 → 런 종료로 자연 수렴
- Healer/쉴드의 가치 유지 — 전투 중 사망 방지가 여전히 중요

### 11.A.2 왜 이 정책인가 (Darkest Dungeon / StS 비교)

| 게임 | 사망 정책 | 특징 |
|------|----------|------|
| **Slay the Spire** | 사망 = 즉시 런 종료 | 단순, 명확. 사망 트리거 설계 불가 |
| **Darkest Dungeon** | Death's Door (HP 0 도달 시 즉사 아님, 다음 공격에 사망) + Hamlet 회복 | 사망 위협 지속, 회복 비용 큼 |
| **Team Log (신규)** | 사망 = 전투 종료 시 50% 부활 + MaxHP -10% 누적 | 사망 트리거 활성화 + 점진적 약화로 런 종료로 수렴 |

**Team Log의 독창성**: 부활의 "기회비용"이 명확 (MaxHP -10%). DD처럼 회복 비용을 지불하지 않아도 되지만, 누적 약화가 자연스러운 런 종료 압력으로 작용.

### 11.A.3 트리거 우선순위 매트릭스 (부활 시스템 기반 재정의)

| 트리거 유형 | 적합성 | 비고 |
|------------|--------|------|
| **본인 피격 / 쉴드 파괴** | ★★★ | Warrior 분노 메카닉 핵심 |
| **본인 HP 임계 (30%/50%)** | ★★★ | 위기 반응, 사망 아님 |
| **적 처치** | ★★★ | 자연스러운 보상 |
| **아군 HP 30% 이하** (사망 아님) | ★★ | "위기감" 연출 |
| **도발 실패** | ★★ | Warrior Protector 특성 |
| **미니언 희생** (Necromancer) | ★★ | 매 전투 재소환 가능 |
| **아군 사망** (★신규 활성화) | ★★ | 부활 보장되므로 정당. but 런당 1~2회 자연 발생, 빈도 낮음 |
| **파티 전멸 직전** (1명 생존) | △ | 부활 직후 상황이라 의미 부족 |

### 11.A.4 사망 트리거 디자인 (부활 시스템 활성화로 복구)

**유물**:
- `Relic_Vengeance`: 아군 사망 시 다음 공격 2배 + 영구 ATK +3 (런당 3회)
- `Relic_EternalBond`: 아군 부활 시 그 캐릭터에게 쉴드 30 + ATK +5 (1턴)
- `Relic_Martyr`: 이번 런 아군 사망 누적 3회 달성 시 파티 전체 영구 ATK +5 (1회)

**특성**:
- `Trait_LastStand` (Warrior): 아군 사망 시 분노 +3 + 다음 공격 치명타 — Warrior 정체성(본인 피격)과 충돌하므로 보류, 대안 `Trait_Protector` 유지 권장
- `Trait_Grief` (Healer): 아군 사망 시 다음 힐량 2배
- `Trait_Reaper` (Necromancer): 아군 사망 시 영혼 +2 즉시

**주의**: 사망 트리거 보상은 **사망 페널티(MaxHP -10%)를 상쇄할 정도여선 안 됨**. 사망 자체는 여전히 손해. 트리거 보상은 "위안" 수준.

### 11.A.5 밸런스 임팩트 및 조정 필요사항

**기존 밸런스 대비 변화**:

| 항목 | 기존 | 신규 | 조정 필요 |
|------|------|------|----------|
| 사망 1명 의미 | 사실상 런 종료 | 다음 전투 50% + MaxHP -10% | 적 난이도 상향 가능 |
| 적 평균 데미지 | 현재 기준 | **+15~25% 상향** 권장 | 사망 허용분량 확대 |
| 보스 HP | Phase ASC-B 신규 12종 | **유지 또는 +10%** | 보스전 사망 빈도 증가 예상 |
| Healer 가치 | 매우 높음 | 여전히 높음 (전투 중 사망 방지) | 변화 적음 |
| 어센션 체감 | 강함 | **약화 가능** | 어센션 PlayerMaxHp과 사망 -10% 곱셈 누적 → 너무 가혹 시 사망 -5% 완화 검토 |
| 런 종료 시점 | 사망 1~2명 | 사망 4~6명 누적 | 평균 런 길이 약간 연장 |

**어센션 × 사망 누적 상호작용 (시뮬레이션 필요)**:

Warrior 120 HP 기준:

| 상황 | MaxHP |
|------|-------|
| 기본 | 120 |
| 어센션 10 (-10%) + 사망 0 | 108 |
| 어센션 10 + 사망 2 | 87 |
| 어센션 10 + 사망 4 | 70 |

어센션 15 (PlayerMaxHp -10%) + 사망 3회 = 120 × 0.9 × 0.9 × 0.9 × 0.9 = 72 HP. 게임 불가능 수준은 아니지만 가혹 → BalanceSimulator로 검증 후 사망 -10%를 -7% 또는 -5%로 완화 검토.

### 11.A.6 구현 관점

**HealthComponent 신규 API**:
```csharp
// 부활 — _isDead = false, HP를 지정값으로 설정
public void Revive(int reviveHP) {
    if (!_isDead) return;
    _isDead = false;
    _currentHP = Mathf.Max(1, reviveHP);
    _currentShield = 0;
    OnHPChanged?.Invoke(_currentHP, _maxHP);
}

// MaxHP 영구 감소 — 부활 시마다 -10% 누적 (곱셈)
public void ApplyMaxHpModifier(float multiplier) {
    _maxHP = Mathf.Max(1, (int)(_maxHP * multiplier));
    _currentHP = Mathf.Min(_currentHP, _maxHP);
    OnHPChanged?.Invoke(_currentHP, _maxHP);
}
```

**GameRunState 신규 — 전투 종료 처리**:
```csharp
public void ProcessBattleEnd() {
    foreach (var member in _playerParty) {
        if (member.IsDead) {
            // 부활: MaxHP -10% 누적 → 50%로 부활
            member.Health.ApplyMaxHpModifier(0.9f);
            int reviveHP = member.Health.MaxHP / 2;
            member.Health.Revive(reviveHP);
        } else {
            // 생존자: 풀힐
            member.Health.Heal(member.Health.MaxHP);
        }
        // 상태이상/쉴드는 TurnManager 종료 시 이미 클리어됨
    }
}
```

**연결 지점**:
- `BattleSceneSetup.OnBattleEnded` (승리 시) → `GameRunState.ProcessBattleEnd()` 호출
- `SaveManager.Save` — 부활 후 HP/MaxHP 저장 (기존 로직 호환, 추가 코드 불필요)
- 부활 시 `CombatEventBus.OnPartyMemberRevived` 이벤트 추가 (사망 트리거 특성/유물 구독)
- UI: 사망 캐릭터 전투 종료 시 "부활" 플로팅 텍스트 + MaxHP 감소 애니메이션

### 11.A.7 사망 트리거 디자인 가이드라인

부활 시스템이 있더라도 사망은 **여전히 손해**. 트리거 설계 원칙:

1. **보상은 손해를 상쇄하지 않는다** — 사망 페널티(MaxHP -10%)를 보상으로 완전 메우면 사망이 "전략"이 됨 → 밸런스 붕괴. 보상은 "위안" 수준 (1~2턴 버프, 1회성)
2. **사망 빈도 1~2회/런 목표** — 시뮬레이터에서 평균 사망 3회+/런이면 적이 너무 강하거나 부활 보상이 너무 미미한 것
3. **전멸은 여전히 런 종료** — 1명 생존으로 승리 시에만 부활 발동. 전멸 시 즉시 런 종료
4. **부활 직후 전투의 긴장감 유지** — 50% 시작이라 다음 전투 사망 위험 높음 → Healer/쉴드 가치 유지

---

## 12. 부록 B — 카드 디자인 템플릿

```csv
id,displayName,description,type,target,power,cost,weight,statusEffect,effectDuration,effectValue,conditionType,conditionValue,conditionStatus,bonusType,bonusValue
Rogue_Eviscerate,절개,콤보를 소모해 강력한 일격,Attack,SingleEnemy,10,3,25,None,0,0,OnStack,5,None,DamageMultiplier,300
Pyromancer_Meteor,운석,광역 화염 공격,Attack,AllEnemies,10,3,25,Burn,2,3,OnStack,3,None,RefundAP,2
Warrior_Bloodlust,피의 갈망,이번 턴 피격 시 분노 충전,Buff,Self,0,2,25,None,0,0,OnActionPerformed,0,None,RefundAP,1
```

### 조건 평가 의사코드

```csharp
public bool EvaluateCondition(SkillConditionType type, int value,
    Character caster, Character target, TurnContext ctx)
{
    switch (type)
    {
        case OnStatus:
            return target.StatusEffects.HasEffect(_conditionStatus);
        case OnHpThreshold:
            return target.Health.CurrentHP * 100 <= target.Health.MaxHP * value;
        case OnStack:
            return caster.Rage?.Stacks >= value
                || caster.Combo?.Stacks >= value
                || caster.Resonance?.Stacks >= value;
        case OnEnemyCount:
            return ctx.Enemies.Count >= value;
        case OnActionPerformed:
            return ctx.TurnStats.SkillsUsedThisTurn >= value;
        case OnTurnOrder:
            return ctx.TurnStats.ActionsThisTurn == value;
        // ...
    }
}
```

---

## 결론: 왜 이 개편이 "재미"를 만드는가

기존 Team Log의 캐릭터는 **"주어진 AP를 가장 효율적으로 쓰기"** 게임이었습니다 — 매 턴 정답이 하나. 조건부 메카닉 도입 후에는:

1. **매 턴 정답이 3~5개로 분할** — 조건 채우기 vs 지금 바로 쓰기 트레이드오프
2. **캐릭터가 "룰"을 갖는다** — StS의 오브/자세가 Team Log에는 분노/콤보/공명으로 치환
3. **파티 시너지가 설계된다** — Archer Mark → Rogue Backstab → Pyromancer Spark 연쇄
4. **어센션 의미 부여** — 고인 어센션에서 조건 충족이 더 어려워져 "기량"이 승률 격차

이것이 Slay the Spire 1000시간 플레이어가 "또 해야지" 하는 이유이고, Team Log가 장기 리텐션을 얻을 경로입니다.

**다음 단계**: Phase CC-1부터 착수 — Warrior/Rogue/Pyromancer 3개 메카닉으로 "조건부 재미" 가설 검증. 이 3개만으로 턴 설계 경험의 질적 변화를 체감할 수 있습니다. Mage 1캐릭터를 3원소 마법사로 분할한 것이 8→10 캐릭터로 파티 조합 다양성 2배 증가시키는 핵심 변경.
