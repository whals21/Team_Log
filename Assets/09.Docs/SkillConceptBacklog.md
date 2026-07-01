# Skill Concept Backlog

> 사용자가 구상한 스킬 메카닉 백로그. 적합한 캐릭터에게 배정할 때까지 보관.
> 각 컨셉은 **고정 수치 + 타겟팅 제약**의 리스크/리턴 구조를 가짐. Phase BK의 BehaviorTag 시스템과 함께 작동하도록 설계.

---

## 컨셉 1 — Distribute (무작위 분배)

> **공격/힐 양쪽 모두에 적용 가능한 메카닉**. 같은 BehaviorTag `Distribute`를 스킬 타입(Attack/Heal)에 따라 자동 분기.

### 스킬 시안 A — Damage Distribute (피해 분배)

**"적들에게 총 12의 피해를 무작위로 분배한다"**

#### 작동
- 총 위력 = 12 (고정)
- 적이 3마리일 때: 12를 각 적에게 무작위로 쪼개어 분배 (예: 5/5/2, 4/4/4, 8/3/1 등 무작위)
- 단일 적이면 12 모두 들어감
- **메인 타겟 지정 불가** — 시전자가 때릴 적을 고를 수 없음

#### 리스크/리턴
- **리턴**: 단일 타겟 스킬(위력 10)보다 총 데미지 고점 +20%
- **리스크**: 원하는 적을 골라서 때릴 수 없음 — 위험한 적을 남길 수 있음, 과잉 데미지 낭비 가능 (이미 약해진 적에게 8 들어가서 낭비)
- **결론**: 다수전에서 빠른 정리에 유리, 단일 보스전에서는 단일기와 비슷

### 스킬 시안 B — Heal Distribute (힐 분배)

**"아군 파티에 총 24의 회복을 무작위로 분배한다"**

#### 작동
- 총 힐량 = 24 (고정)
- 부상 아군이 3명일 때: 24를 각 아군에게 무작위로 쪼개어 분배 (예: 10/10/4, 8/8/8, 18/4/2 등 무작위)
- **메인 타겟 지정 불가** — 시전자가 누구를 우선 치유할지 고를 수 없음
- 부상 아군이 1명이면 24 모두 들어감 (과잉 힐 낭비 가능)

#### 리스크/리턴
- **리턴**: 단일 힐 스킬(위력 20)보다 총 힐량 고점 +20%. 파티 전체 도트 회복에 강함
- **리스크**: **위기의 아군(HP 5)에게 3만 가고 풀피 아군에게 18이 가는 등 분배 운 작용**. 죽어가는 아군 살리지 못할 위험. 풀피 아군에게 힐이 배분되면 과잉 힐 낭비
- **결론**: 파티 전체가 골고루 다쳤을 때 효율 최대. 특정 아군 위기 상황에서는 신뢰성 낮음 → 단일 힐기와 보완 필요

#### 힐 분배만의 독특한 포인트
- **Death's Door 방지 불가**: "이번 턴 누가 죽을지 모름" — Darkest Dungeon식 긴장감. 단일 힐이 정확한 타겟 보장하는 것과 차별화
- **Healer 신뢰성 하락**: 힐러의 핵심 가치인 "위기 개입"이 불확실성으로 대체 → 캐릭터 정체성 트레이드오프. 대신 **총 힐량**으로 보상
- **Mass Heal vs 차별화**: Mass Heal(광역 힐)은 모든 아군에게 균등하게. Distribute Heal은 무작위 분배. "균등하지만 총량 많음" vs "고른 분배 보장"

### 필요 메카닉
- 새 BehaviorTag: **`Distribute`** (rank = 총 위력/힐량)
- **SkillType 기반 자동 분기**: SkillType == Attack → 피해 분배, SkillType == Heal → 힐 분배. 동일 BehaviorTag로 두 케이스 처리
- 또는 새 TargetType: `DistributeRandomAllies` / `DistributeRandomEnemies`
- 구현:
  - 피해: `_enemies.FindAll(alive)`에서 가중치 균등 랜덤로 1씩 N회 분배 (or 묶어서 한 번에)
  - 힐: `_playerParty.FindAll(alive)`에서 동일 로직. **풀피 아군 제외 옵션** 권장 — 과잉 힐 낭비 방지
- UI: 타겟 지정 버튼 없이 "시전" 버튼만. 분배 결과는 플로팅 텍스트/배틀로그로만 표시

### 적합 캐릭터 후보

#### 피해 분배 (시안 A)
- **Stormcaller(Taranis)**: 번개가 무작위로 튀는 이미지. Chain + Distribute 조합으로 다수전 폭딜
- **Pyromancer(Ashe)**: 불이 번지며 분산
- **Bard**: 무작위 혼란 공연 컨셉

#### 힐 분배 (시안 B)
- **Healer**: "생명 결속" 메카닉과 결합 — Distribute로 무작위 분배되는 힐이 "이번 턴 힐 받은 아군" 버프의 대상이 됨. 신뢰성 낮지만 총 힐량 높은 변형 힐러 포지션
- **Alchemist**: "시약 반응" 컨셉 — 물약을 무작위로 뿌리는 이미지. 같은 대상에게 두 번째 물약이 가면 효과 2배 특성과 시너지 (랜덤 분배로 자연 발생)
- **Bard**: 음표가 무작위로 퍼지는 이미지. 리듬 자원과 결합 가능 (Distribute Heal이 리듬 +1)
- **Sibyl(Oracle)**: "미래 차용" — 1턴 뒤 무작위 분배 힐이 발동. 시간 축 + 무작위 축 이중 불확실성 (고위험 고수익 서포터)

---

## 컨셉 2 — Target Highest HP (자동 단일)

**스킬 시안**: "체력이 가장 높은 적에게 10의 피해를 준다"

### 작동
- 위력 = 10 (고정, 단일기 대비 +0~25%)
- 체력이 가장 높은 적 자동 선택
- 동점이면 그 중 무작위 1마리
- **플레이어가 타겟 지정 불가**

### 리스크/리턴
- **리턴**: 일반 단일기(위력 8)보다 +25% 데미지
- **리스크**: 때리고 싶은 적(예: 체력이 낮아 마무리 직전인 적)이 있어도 자동으로 가장 튼튼한 적에게 낭비될 수 있음
- **결론**: 엘리트/보스 녹이기에 강함, 마무리 상황에선 비효율

### 필요 메카닉
- 새 BehaviorTag: `TargetHighestHP` (또는 TargetType 확장)
- 구현: `_enemies.MaxBy(e => e.Health.CurrentHP)` 후 단일 ExecuteSkillInternal

### 적합 캐릭터 후보
- **Warrior(Duran)**: 가장 큰 위협부터 때리는 탱커 컨셉. Vengeance 게이지 활용과 연계
- **Archer**: 저격수가 가장 튼튼한 표적을 우선 사격
- **Necromancer**: "죽음의 인식" — 생명력이 큰 자를 감지

---

## 컨셉 3 — Multi Strike Free (자유 다단)

**스킬 시안**: "3의 피해를 2번 가한다 (총 6 데미지). 때릴 적을 매 타격마다 자유롭게 선택 가능"

### 작동
- 1회 타격 위력 = 3
- 타격 횟수 = 2 (총 6)
- 적이 3마리여도 2회만 때림
- 매 타격마다 플레이어가 타겟 지정:
  - 같은 적 2번 (집중) → 한 놈을 6 데미지
  - 2명에게 1회씩 (분산) → 두 놈을 3 데미지씩
- **Phase BK의 MultiHit과 다른 점**: MultiHit은 동일 대상 고정. 이 컨셉은 매 타격마다 자유 선택

### 리스크/리턴
- **리턴**: 유연한 마무리/기믹 트리거 (체력 절반 이하 도트 리트리거, 약한 적 2마리 동시 마무리)
- **리스크**: 단일기(위력 8) 대비 총 데미지 -25%. 위력 자체가 낮아 쉴드/DEF에 의해 흡수되기 쉬움
- **결론**: 정밀 제어가 보상, 데미지 자체는 낮음

### 필요 메카닉
- 새 BehaviorTag: `MultiStrike` (rank = 추가 타격 수, 기존 MultiHit과 구분)
- 또는 PlayerActionController에서 "타격마다 타겟 재지정" UI 모드 추가
- 구현: TurnManager가 2회의 ExecuteSkillInternal을 호출하되, 매 타격 전 player에게 타겟 선택 UI

### UI/UX 고려
- "타겟 선택 → 타격 → 다음 타겟 선택 → 타격" 순차 모드
- 또는 미리 2개 타겟 지정 (같은 적 중복 허용) 후 일괄 실행
- 기존 SingleEnemy UI를 확장하거나 별도 모드 필요

### 적합 캐릭터 후보
- **Rogue**: 빠른 연격 + 정밀 마무리 컨셉에 완벽 부합
- **Archer**: 두 발 사격
- **Warrior(Duran)**: "Provoke → 타격 분배" 전술

---

## 컨셉 4 — Target Full HP (풀피 단일)

**스킬 시안**: "체력이 가득 찬 적에게만 10의 피해를 준다"

### 작동
- 위력 = 10 (단일기 대비 +25%)
- 후보 = `CurrentHP == MaxHP`인 적
- 풀피 적이 없으면 스킬 사용 불가 (또는 자동 폴백 금지)
- 풀피 적이 여러 마리면 플레이어가 선택

### 리스크/리턴
- **리턴**: 단일기보다 +25% 데미지
- **리스크**: 풀피 적이 없으면 스킬이 무용지물. 전투 중반 이후 사용 기회 감소. 선행 타격용으로만 가치
- **결론**: 전투 개시용, 도트/쉴드로 적 HP가 깎인 후엔 봉인

### 필요 메카닉
- 새 BehaviorTag: `TargetFullHP`
- 또는 TargetType 확장 + CanUse 검사 (풀피 적 존재 여부)
- UI: 풀피 적만 클릭 가능, 나머지는 회색 처리
- 캐릭터가 이 스킬을 드로우했을 때 조건 불충족이면 플레이어가 미리 알 수 있어야 함 (슬롯에 "조건 불충족" 표시)

### 적합 캐릭터 후보
- **Cryomancer**: 얼려서 완전한 상태를 치는 "빙결 강타"
- **Archer**: 첫 발 명중 — 풀피 표적에게 치명상
- **Warrior(Duran)**: 선제 강타

---

## 컨셉 5 — Already Hit (이미 공격받은 대상 강화)

**스킬 시안**: "이미 이번 턴에 공격받은 적에게 추가 데미지를 준다" (예: 기본 8 + 보너스 +4 = 12)

### 작동
- 위력 = 8 (기본) + **+4 보너스** (조건 충족 시) = 총 12
- 조건 = **대상이 이번 턴에 이미 1회 이상 피해를 받음** (플레이어/아군이 먼저 때린 뒤 사용)
- 스킬 자체는 **항상 사용 가능** (DesignPillars 원칙 1 규칙 5A "강화 조건")
- 기본 위력(8)은 단일기와 동일 → 조건 미충족 시 그냥 평범한 단일 공격

### 리스크/리턴
- **리턴**: 다른 아군의 공격을 "셋업"으로 활용 → 같은 적에게 2연속 타격으로 빠른 마무리
- **리스크**: 셋업 없이 단독 사용하면 단일기(위력 8)와 동일 → 보너스 못 받음. 드로우 순서 의존
- **결론**: 파티 시너지 / 타겟 집중 전술에 강함. 단독으로는 평범. StS `Eviscerate`(콤보 의존)와 유사한 구조

### 필요 메카닉
- 새 BehaviorTag: **이름 후보 (아래 참조)** (rank = 보너스 위력)
- 구현: `TurnContext`에 `Dictionary<Character, int> _hitsTakenThisTurn` 추가. 매 데미지 이벤트마다 +1. 스킬 실행 시 대상의 카운트 ≥ 1이면 보너스 위력 가산
- 기존 `OnDamageDealt` 이벤트 후크 활용 가능 (CombatEventBus)

### 이름 후보 (사용자 선택)
| # | 이름 | 어원/뉘앙스 | 장단점 |
|---|------|-----------|--------|
| **A** | **FollowUp** | "후속타" — 이전 공격을 잇는다는 의미 가장 정확 | 직관적, 메카닉 설명과 1:1 대응 |
| **B** | **Pursuit** | "추격" — 흔들리는 적을 쫓는 전술적 서사 | 서사 강함, 약간 모호 |
| **C** | **GangUp** | "집단 공격" — 파티가 한 명을 몰아치는 이미지 | 파티 시너지 의도 부합, 다소 구어적 |
| D | Convergence | "수렴" — 공격이 한 대상으로 수렴 | 추상적, 일반 유저 이해 난해 |
| E | Finisher | "마무리타" — RPG 전통 용어 | 보너스 의미는 약함, Rogue Eviscerate와 충돌 |
| F | PileOn | "얹어치기" — 한국어식 직관 | 비격식적 |

**추천**: `FollowUp` (A) — 메카닉 설명("이전 공격을 잇는다")과 이름이 1:1로 대응하여 학습 비용 최소

### 적합 캐릭터 후보
- **Rogue**: 콤보 적재 → Backstab(FollowUp) 연속 마무리. 정체성과 완벽 부합
- **Archer**: Hunter's Mark → Piercing Shot(FollowUp) 표식 집중 사격
- **Warrior(Duran)**: Vengeance 축적 후 FollowUp으로 결정타 — 분노 흐름과 자연스러움

---

## 컨셉 6 — Full HP Bonus (풀피 대상 강화) — ⚠️ 컨셉 4와 비교 필수

**스킬 시안**: "체력이 가득 찬 적에게 공격 시 추가 데미지" (예: 기본 8 + 보너스 +4 = 12)

### ⚠️ 기존 컨셉 4(TargetFullHP)와의 핵심 차이

| 구분 | 컨셉 4 (TargetFullHP) | 컨셉 6 (본 컨셉) |
|------|----------------------|-----------------|
| **조건 종류** | **사용 제약 조건** (DesignPillars 규칙 5B) | **강화 조건** (DesignPillars 규칙 5A) |
| **풀피 적 없을 때** | 스킬 사용 **불가** | 스킬 사용 **가능** (기본 위력 8만 적용) |
| **기본 위력** | 10 (단일기 +25% 보상) | 8 (단일기 동일) + 보너스 +4 |
| **설계 위치** | 예외적 (게임 체인저급) | **기본 원칙 준수** (4.5.5 권장) |
| **플레이 경험** | "풀피 적 기다리기" 좌절 가능 | "항상 쓰되 보너스 노리기" 전술 |

> DesignPillars 원칙 1에 따라 **컨셉 6(강화 조건)이 기본**, 컨셉 4(사용 제약)는 예외적 게임 체인저용으로만 권장. 같은 "풀피" 상황을 다루되 사용자 경험 차이가 큼.

### 작동
- 위력 = 8 (기본) + **+4 보너스** (풀피 적 대상 시)
- 조건 = `대상.CurrentHP == 대상.MaxHP` (쉴드는 무시, HP 기준)
- 스킬 자체는 **항상 사용 가능**
- 도트/쉴드/이전 타격으로 HP가 1이라도 깎이면 보너스 미적용

### 리스크/리턴
- **리턴**: 전투 첫 턴 폭딜 — 가장 위협적인 풀피 적을 빠르게 약화
- **리스크**: 두 번째 턴부터는 거의 모든 적이 풀피가 아님 → 보너스 미적용 빈도 증가
- **결론**: 전투 개시용 / 도트 딜러와 시너지 (도트로 HP 깎으면 본 컨셉 비활성, 다른 캐릭터가 풀피 적 우선 타격)

### 필요 메카닉
- 새 BehaviorTag: **이름 후보 (아래 참조)** (rank = 보너스 위력)
- 구현: SkillExecutor에서 `target.Health.CurrentHP == target.Health.MaxHP` 체크 후 보너스 위력 가산
- 기존 컨셉 4(TargetFullHP)는 CanUse 검사가 필요하지만, 본 컨셉 6은 그냥 위력 분기만. 코드 5줄 내외

### 이름 후보 (사용자 선택)
| # | 이름 | 어원/뉘앙스 | 장단점 |
|---|------|-----------|--------|
| **A** | **FirstBlood** | "첫 피" — 전투/대상 첫 타격 보상. 직관적이고 강렬 | 가장 직관, "첫 타격 보너스" 의도 부합 |
| **B** | **Opener** | "개막타" — 전투 시작 타격 | 간결, 다소 밋밋 |
| **C** | **Vanguard** | "선봉" — 전술적 선두 타격 | 서사 강함, 메카닉과 약간 거리 |
| D | FirstStrike | "선제 타격" | 명확하나 보상 뉘앙스 약함 |
| E | Initiate | "개시자" | 모호 |
| F | Pristine | "원형/손상 없는" | 상태 묘사, 어려움 |

**추천**: `FirstBlood` (A) — "전투의 첫 피를 선언한다"는 서사가 강렬하고, 풀피 적 첫 타격 보상이라는 메카닉과 1:1 대응

### 적합 캐릭터 후보
- **Warrior(Duran)**: 선제 강타로 큰 위협을 먼저 약화. 탱커의 "먼저 때려서 위협 차단" 전술과 부합
- **Archer**: 저격수가 풀피 표적에게 치명상. Hunter's Mark 없이도 첫 타강화
- **Ashe(Pyromancer)**: Ember 축적 전 셋업으로 첫 턴 폭딜 — Cinder Accretion(FirstBlood)로 Burn + 보너스 동시

---

## 컨셉 7 — Half HP Bonus (절반 이하 대상 강화)

**스킬 시안**: "체력이 절반 이하인 적에게 공격 시 추가 데미지" (예: 기본 8 + 보너스 +6 = 14)

### 작동
- 위력 = 8 (기본) + **+6 보너스** (절반 이하 대상 시) — 보너스를 크게 줘 "마무리 역할" 강조
- 조건 = `대상.CurrentHP * 2 <= 대상.MaxHP` (즉 HP 50% 이하)
- 스킬 자체는 **항상 사용 가능** (강화 조건)
- 기존 24종 `Execution`(절대 HP 임계값 처형)과 다름 — Execution은 "임계값 이하 즉사", 본 컨셉은 "보너스 데미지"

### 리스크/리턴
- **리턴**: 빈사 적 확실 마무리 — 과잉 데미지 낭비 없이 다음 적으로 전환
- **리스크**: 절반 이하 적 없으면 단일기(8)와 동일. 도트 딜러가 깎아줘야 진가 발휘
- **결론**: 마무리/처형 전문 역할. Ashe(Rogue) Eviscerate, Necromancer Soul Harvest와 카테고리 겹침 주의

### 필요 메카닉
- 새 BehaviorTag: **이름 후보 (아래 참조)** (rank = 보너스 위력)
- 구현: SkillExecutor에서 `target.Health.CurrentHP * 2 <= target.Health.MaxHP` 체크 후 보너스 위력 가산
- 기존 `Execution`(BehaviorTagResolver.Has)과 명확한 분리 필요 — Execution은 TakeDirectDamage 즉사, 본 컨셉은 보너스 위력 가산

### 이름 후보 (사용자 선택)
| # | 이름 | 어원/뉘앙스 | 장단점 |
|---|------|-----------|--------|
| **A** | **Cull** | "도축/도태" — 약해진 개체를 골라 도축하는 서사 | 직관+강렬, "약한 적 골라 처치" 의도 부합 |
| **B** | **Predator** | "포식자" — 약한 사냥감을 노리는 서사 | 서사 강함, 행동보다 이미지에 치중 |
| **C** | **CoupDeGrace** | "쿠드그라스" — 빈사 적 마지막 일격 (프랑스어) | 전통 RPG 용어, 다소 길고 외래어 |
| D | Mercy | "자비" — 빨리 끝내주는 자비 | 부드러운 서사, 위력 보너스와 충돌 |
| E | KillShot | "킬 샷" | 직관적, 다소 평범 |
| F | Subdue | "제압" | 좋으나 약함 |
| G | Deathblow | "치명타" | Execution과 뉘앙스 충돌 |

**추천**: `Cull` (A) — 짧고 강렬하며 "약해진 개체를 골라 도축한다"는 메카닉 의도와 1:1 대응. 기존 24종과 어감 충돌도 없음

### 적합 캐릭터 후보
- **Rogue**: 빈사 적 마무리 전문. 콤보 5 + Eviscerate → 절반 이하 적 확정 처치
- **Necromancer**: Soul Harvest와 자연스러운 연계 — 도트로 약화 → 본 컨셉으로 마무리 → 영혼 획득
- **Ashe(Pyromancer)**: Burn 도트로 적을 절반 이하로 깎은 뒤 Brand of Ash(Cull)로 결정타. Ember 자해 위험을 감수할 만한 보상

---

## 컨셉 8 — Fatigue (피로) — 매 사용 시 위력 감소

**스킬 시안**: "이 전투에서 매 사용 시 위력 -2" (예: 10 → 8 → 6 → 4 → 2)

**이름**: `Fatigue` — "피로". 사용할수록 지쳐 약해진다는 직관. D&D 5판 Fatigued 상태, WoW 피로도 시스템에서 공통 어휘

### 작동
- 위력 = `basePower - (usesThisBattle × rank)` (rank=감소량, 예: 2)
- 전투 종료 시 리셋 (다음 전투는 다시 basePower부터)
- 최소 위력 1 보장 (0/음수 방지)

### 리스크/리턴
- **리턴**: 보통 스킬보다 **초반 위력 높음** (예: 첫 사용 12). 강력한 오프닝
- **리스크**: 3회 사용부터 단일기(8) 이하로 약해짐 → 드로우 운으로 자주 뽑히면 딜 손실
- **결론**: 전투 길이 짧을수록 유리 (일반전). 보스전 장기전에서는 드로우 안 뽑히게 운영 필요

### 필요 메카닉
- `BehaviorTag`: `Fatigue` (rank = 사용당 위력 감소량)
- 구현: `SkillInstance.usesThisBattle` 필드 추가. 전투 시작 시 리셋. SkillExecutor에서 `EffectivePower = Max(1, basePower - uses × rank)` 계산

### 적합 캐릭터 후보
- **Warrior(Duran)**: 강력한 오프닝 일격 후 Vengeance로 전환. "첫 충격은 강하다" 전술
- **Ashe(Pyromancer)**: Ember 충전 전 첫 폭딜용. Cinder Accretion(Fatigue)으로 빠른 Burn 부여

---

## 컨셉 9 — Momentum (관성) — 매 사용 시 위력 증가

**스킬 시안**: "이 전투에서 매 사용 시 위력 +2" (예: 6 → 8 → 10 → 12 → 14)

**이름**: `Momentum` — "관성/가속". 사용할수록 관성이 붙어 강해진다는 직관. StS `Searing Blow`(업그레이드 누적)의 자동 버전, LoL Momentum 패시브에서 공통 어휘

### 작동
- 위력 = `basePower + (usesThisBattle × rank)` (rank=증가량, 예: 2)
- Fatigue와 정반대. 전투 종료 시 리셋
- 상한선 권장 (예: 최대 +8) — 무한 스노우볼 방지

### 리스크/리턴
- **리턴**: 장기전 보상 — 보스전에서 5턴째부터 단일기 2배 위력
- **리스크**: 초반(1~2회 사용)은 단일기(8) 이하. 드로우 안 뽑히면 가속 안 됨
- **결론**: 보스전/엘리트전 강력. 일반전 짧은 전투에서는 효과 제한

### 필요 메카닉
- `BehaviorTag`: `Momentum` (rank = 사용당 위력 증가량)
- 구현: Fatigue와 동일 (`usesThisBattle` 추적). SkillExecutor에서 `EffectivePower = basePower + min(uses × rank, cap)`

### 적합 캐릭터 후보
- **Bard**: 리듬 자원과 완벽 부합 — 곡을 반복할수록 가속
- **Rogue**: 콤보 적재와 함께 매 턴 사용 → 후반 폭딜
- **Taranis**: Charge Network 유지되는 장기전에서 네트워크 + Momentum 이중 스노우볼

---

## 컨셉 10 — Echo (메아리) — 위력 절반으로 2회 시전

**스킬 시안**: "위력 5로 2회 시전 (총 10). 두 번째는 같은 적 or 다른 적 선택 가능"

**이름**: `Echo` — "메아리". 같은 효과가 메아리처럼 한 번 더 울린다는 직관. StS `Echo Form`(매 턴 첫 공격 2회), `Double Tap`(이번 턴 공격 2회)에서 정통 용어

### 작동
- 위력 = `basePower × 2회` (basePower는 일반 단일기의 절반, 예: 5)
- 첫 타격 → 타겟 재지정 → 두 번째 타격
- 기존 `MultiHit`과 다름 — **MultiHit은 동일 대상 고정**, Echo는 **2회째 타겟 자유 지정**
- 기존 `Bounce`와 다름 — Bounce는 **무작위 N명**, Echo는 **플레이어가 지정**

### 리스크/리턴
- **리턴**: 같은 적 2연타(집중) or 다른 적 1타씩(분산) 자유 선택
- **리스크**: 위력 5가 쉴드/DEF에 막히면 0데미지 위험. 타겟 지정 2번 = UI 클릭 2회
- **결론**: 유연한 마무리/기믹 트리거. MultiStrike(컨셉 3)와 비슷하나 위력 절반 구조

### 필요 메카닉
- `BehaviorTag`: `Echo` (rank = 추가 시전 횟수, 기본 1)
- 구현: PlayerActionController에서 "첫 타겟 → 실행 → 두 번째 타겟 UI → 실행" 순차 모드. MultiStrike(컨셉 3) UI와 공유 가능

### 적합 캐릭터 후보
- **Cryomancer**: Frostbolt(Echo)로 같은 적 2연타 Freeze → 빠른 Frost 3스택
- **Healer**: Heal(Echo)로 위기 시 2명 연속 힐
- **Sibyl**: 예언 1턴 뒤 + Echo = 2턴 뒤 2회 발동 (시간 축 시너지 극대화)

---

## 컨셉 11 — Desperation (절박) — 잃은 HP당 위력 +

**스킬 시안**: "잃은 체력 5당 위력 +1" (예: MaxHP 100, 현재 HP 50 → +10 위력)

**이름**: `Desperation` — "절박함". 다칠수록 필사적으로 강해진다는 직관. 기존 `Berserk`(HP% 임계값 boolean)과 다름 — **Desperation은 선형 스케일** (잃은 HP × N)

### 작동
- 위력 = `basePower + (lostHP / rank)` (rank=위력당 필요 잃은 HP, 예: 5)
- %가 아닌 **절대 HP당 +N** (사용자 명시). 어센션 PlayerMaxHp 축소 시 효과 감소 — 자연 밸런스
- 기존 `Berserk`와 비교: Berserk는 "HP 50% 이하" 1회 보너스. Desperation은 선형 비례

### 리스크/리턴
- **리턴**: 위기일수록 폭딜 — 빈사 상태(잃은 HP 80)에서 +16 위력 가산
- **리스크**: 풀피일 때 보너스 0 → 첫 턴 약함. 자해/도트 페널티 캐릭터와 시너지 but 사망 위험
- **결론**: "역전의 핵심 메카닉". StS `Reckless Charge`, `Offering`의 자해 딜러 포지션

### 필요 메카닉
- `BehaviorTag`: `Desperation` (rank = 위력 1당 필요 잃은 HP)
- 구현: `caster.Health.MaxHP - caster.Health.CurrentHP` 계산 후 `bonusPower = lostHP / rank`

### 적합 캐릭터 후보
- **Ashe(Pyromancer)**: Ember 자해와 완벽 시너지 — 자해로 잃은 HP 축적 → Desperation 보너스 가산
- **Warrior(Duran)**: Vengeance(피격 축적)와 병행 — 맞으면서 강해지는 탱커 딜러 하이브리드
- **Healer(Martyr 특성)**: 자신 HP 30% 이하 힐 2배 특성과 연쇄

---

## 컨셉 12 — Wound (상처) — 잃은 HP당 위력 −

**스킬 시안**: "잃은 체력 5당 위력 -1" (예: MaxHP 100, 현재 HP 50 → -10 위력)

**이름**: `Wound` — "상처". 다칠수록 약해진다는 직관. StS `Wound` 카드(데크에 쓰레기 카드 추가)에서 따왔으나 다른 메카닉. **Desperation과 정반대** — 양수/음수만 다른 구조

### 작동
- 위력 = `Max(1, basePower - (lostHP / rank))` (rank=위력 감소당 잃은 HP)
- 최소 위력 1 보장. %가 아닌 절대 HP 스케일 (사용자 명시)
- **의도적 약점 부여용** — 특정 스킬에만 Wound를 달아 "다칠수록 약해지는 페널티" 부여

### 리스크/리턴
- **리턴**: 풀피일 때 단일기 +25% 강력 (예: 위력 10). 오프닝 특화
- **리스크**: 다치면 급격히 약화 → 캐릭터가 "녹아내림" → 부활 MaxHP 누적과 시너지 페널티
- **결론**: **DesignPillars 원칙 2(약점-보조)의 핵심 도구**. 약점을 명시적으로 부여하고 특성/증강으로 보조 설계 가능

### 필요 메카닉
- `BehaviorTag`: `Wound` (rank = 위력 1 감소당 필요 잃은 HP)
- 구현: Desperation과 동일 구조, 부호만 반대. `bonusPower = -(lostHP / rank)`

### 적합 캐릭터 후보
- **GlassCannon형 스킬**: 강력하지만 다치면 급격히 약화 → "풀피일 때 결정타" 전술 강제
- **Necromancer**: 영혼 자원을 가진 스킬에 Wound 부여 — "다치면 영혼과 연결 약화" 서사
- **Healer**: 자기 생존 약점을 Wound로 명시 → 보조 특성(Martyr 등)과 트레이드오프

---

## 컨셉 13 — Escalation (에스컬레이션) — 매 사용 시 AP cost 증가

**스킬 시안**: "이 전투에서 매 사용 시 AP cost +1" (예: 1 → 2 → 3 → 4)

**이름**: `Escalation` — "에스컬레이션/점증". 사용할수록 비용이 에스컬레이션된다는 직관. 정치/경제 용어에서 차용, StS `Blasphemy`(Divinity 1턴)와 유사한 자원 증가 패턴

### 작동
- AP cost = `baseCost + (usesThisBattle × rank)` (rank=사용당 cost 증가량, 예: 1)
- Fatigue(위력 감소)와 같은 구조, **위력이 아닌 cost**가 변동
- 최대 cost 상한선 권장 (예: 5 이하) — AP 시스템 붕괴 방지

### 리스크/리턴
- **리턴**: 초반 cheap burst — 첫 사용 cost 1로 폭딜 가능
- **리스크**: 3회 사용부터 cost 4+ → AP 부족으로 다른 스킬 사용 제약
- **결론**: "초반 올인, 후반 봉인" 전술 강제. Fatigue와 쌍둥이 메카닉

### 필요 메카닉
- `BehaviorTag`: `Escalation` (rank = 사용당 cost 증가량)
- 구현: `SkillInstance.usesThisBattle` 재사용 (Fatigue/Momentum과 공유). `EffectiveCost = baseCost + min(uses × rank, cap)`

### 적합 캐릭터 후보
- **Ashe(Pyromancer)**: Ember 충전 전 초반 폭딜용. Cinder Accretion(Escalation)로 cost 1에 Burn 부여
- **Alchemist**: 시약 반응과 결합 — 물약 3개 사용 후 Escalation 발동으로 효율 극대화

---

## 컨셉 14 — Mastery (숙련) — 매 사용 시 AP cost 감소

**스킬 시안**: "이 전투에서 매 사용 시 AP cost -1 (최소 0)" (예: 3 → 2 → 1 → 0)

**이름**: `Mastery` — "숙련". 자주 쓸수록 숙련되어 비용이 줄어든다는 직관. RPG 전통 용어 (Mastery 레벨, Weapon Mastery 등). **Escalation과 정반대**

### 작동
- AP cost = `Max(0, baseCost - (usesThisBattle × rank))` (rank=사용당 cost 감소량)
- **자주 뽑혀야 발동** → 드로우 운 의존. but 발동 후에는 cost 0 무한 루프 가능
- 밸런스 리스크: cost 0 도달 후 매 턴 사용 → AP 환급 루프 위험 → 상한선 권장 (예: 최소 cost 1)

### 리스크/리턴
- **리턴**: 장기전 보상 — 3회 사용 후 cost 0 무료 스킬. AP 환급 콤보 핵심
- **리스크**: 첫 사용 cost 3 (비쌈) → 초반 드로우 시 봉인. AP 낭비 위험
- **결론**: 보스전/엘리트전에서 진가. 일반전 짧은 전투에서는 비효율

### 필요 메카닉
- `BehaviorTag`: `Mastery` (rank = 사용당 cost 감소량)
- 구현: `EffectiveCost = Max(minCost, baseCost - uses × rank)`. minCost 권장 (0 또는 1)

### 적합 캐릭터 후보
- **Bard**: 리듬 자원과 병행 — 곡을 반복할수록 숙련. Grand Finale 전 셋업
- **Healer**: 장기전에서 힐 cost 감소 → 파티 유지력 극대화
- **Taranis**: Charge Network 장기전에서 Thunderstorm 비용 절감

---

## 컨셉 15 — GiantSlayer (거인살해자) — 적 MaxHP 임계값 이상 시 보너스

**스킬 시안**: "적 최대체력이 100 이상이면 위력 +6"

**이름**: `GiantSlayer` — "거인살해자". 사용자 직접 제안. LoL 아이템 `Giant Slayer`, 스토리/게임 전통 "거인 살해자"(다윗과 골리앗, Jack and Beanstalk)에서 공통 어휘. 직관적이고 강렬

### 작동
- 위력 = `basePower + (target.MaxHP >= threshold ? rank × N : 0)`
- threshold는 보통 "보스급 HP" 기준 (예: 100 = 엘리트, 150+ = 보스)
- 임계값은 스킬별로 다를 수 있음 (예: GiantSlayer 1=100이상, GiantSlayer 2=150이상)

### 리스크/리턴
- **리턴**: 보스/엘리트 특화 — 일반 적에게 단일기, 강적에게 +75%
- **리스크**: 일반전(적 HP 30~60)에서는 보너스 0 → 보스전에서만 진가
- **결론**: **"거인 사냥 전문" 역할 강제**. CharacterConceptReview 5.4 Stormcaller "단일전 약점" 보완 핵심 도구

### 필요 메카닉
- `BehaviorTag`: `GiantSlayer` (rank = 보너스 위력, threshold는 별도 필드 or rank에서 인코딩)
- 구현: `target.Health.MaxHP >= threshold` 체크. CharacterData.MaxHP 이미 존재

### 적합 캐릭터 후보
- **Taranis**: 다수전 약점(단일 보스전 연쇄 없음)을 GiantSlayer로 보완 — 보스전 딜러 역할 확보
- **Warrior(Duran)**: 큰 위협(보스) 우선 타격 탱커 컨셉과 부합
- **Archer**: 저격수의 "큰 표적 특화" — Mark + GiantSlayer로 보스 폭딜

---

## 컨셉 16 — AllIn (올인) — 사용 후 AP 0 시 보너스

**스킬 시안**: "이 스킬 사용 후 AP가 0이 되면 위력 +8"

**이름**: `AllIn` — "올인". 모든 자원을 쥐어짜 마지막에 쓴다는 직관. 포커/도박 용어에서 차용, 강렬하고 직관적. StS `Whirlwind`(에너지 비례), `Flash Strike`(에너지 다 쓰면 추가)에서 유사 패턴

### 작동
- 위력 = `basePower + (AP remaining after this skill == 0 ? bonus : 0)`
- 조건: `(currentAP - thisCost) == 0`. 즉 마지막 AP를 이 스킬에 쓸 때 보너스
- 기존 `CostDown`/`QuickDraw`와 다름 — CostDown은 cost 자체 감소, AllIn은 **AP 잔량 0 조건부 보너스**

### 리스크/리턴
- **리턴**: 매 턴 마지막 스킬로 사용 시 단일기 2배 위력 (예: 8→16)
- **리스크**: 스킬 순서 강제 — 이 스킬을 마지막에 써야 함. 다른 스킬 드로우 운 의존
- **결론**: "마무리 일격" 전술 핵심. 순서 설계 재미와 결합 (DesignPillars 원칙 1 충족)

### 필요 메카닉
- `BehaviorTag`: `AllIn` (rank = AP 0 시 보너스 위력)
- 구현: 스킬 실행 전 `(TurnContext.CurrentAP - skill.Cost) == 0` 체크 → 보너스 위력 가산

### 적합 캐릭터 후보
- **Warrior(Duran)**: Revenge Strike(AllIn)로 매 턴 마지막 AP에 결정타. Vengeance + AllIn 이중 보너스
- **Rogue**: Eviscerate(AllIn)로 콤보 5 + AP 0 이중 조건 마무리
- **Ashe(Pyromancer)**: Brand of Ash(AllIn)로 Ember 5 + AP 0 + 자해 50% 삼중 보너스 폭딜

---

## 컨셉 17 — Dominance (지배) — 적 HP < 나 HP 시 보너스

**스킬 시안**: "적 현재체력이 나보다 낮을 때 위력 +4"

**이름**: `Dominance` — "지배/우위". 체력 우위로 적을 지배한다는 직관. 전술/전략 게임 전통 용어 (Civilization, StarCraft Dominance). StS `Predatory Instincts`(Watcher)와 유사

### 작동
- 위력 = `basePower + (target.CurrentHP < caster.CurrentHP ? rank : 0)`
- 쉴드는 무시, **현재 HP 기준** (사용자 명시에 따라)
- 쉴드 포함 여부는 밸런스 튠 시 결정 (HP만 vs HP+쉴드)

### 리스크/리턴
- **리턴**: 우위 유지 시 지속적 보너스 — 풀피 캐릭터가 약한 적 사냥
- **리스크**: 다친 캐릭터는 보너스 못 받음 → 위기 상황에서 약화. 역전 메카닉 아님
- **결론**: "강자가 약자를" 구조. Desperation(역전)과 정반대 포지션

### 필요 메카닉
- `BehaviorTag`: `Dominance` (rank = 보너스 위력)
- 구현: `target.Health.CurrentHP < caster.Health.CurrentHP` 체크

### 적합 캐릭터 후보
- **Warrior(Duran)**: 높은 HP + 쉴드로 Dominance 조건 유지 → 탱커가 딜까지. 정체성 부합
- **Healer**: 힐로 자신 HP 유지 + 적 도트로 적 HP 감소 → Dominance 발동
- **Archer**: 후열 안전 + 적 약화 후 마무리 (Mark → Dominance 연쇄)

---

## 컨셉 18 — Bulwark (방패벽) — 쉴드 존재 시 보너스

**스킬 시안**: "나에게 쉴드가 있을 때 위력 +5"

**이름**: `Bulwark` — "방패벽". 쉴드가 방패벽처럼 보호하며 동시에 공격도 강화된다는 직관. 전술/역사 용어 (로마 Scutum 방패벽, StS `Barricade`/`Body Slam` 패턴). 기존 `ShieldBonus`(쉴드 양 보너스)와 다름 — **Bulwark는 공격 위력 보너스**

### 작동
- 위력 = `basePower + (caster.Shield > 0 ? rank : 0)`
- 기존 `ShieldBonus`(쉴드 +N 부여)와 명확 분리 — ShieldBonus는 쉴드 자체 강화, Bulwark는 **쉴드 보유 시 공격 강화**
- 쉴드가 1이라도 있으면 발동 (양 무관, rank에 따라 보너스 고정)

### 리스크/리턴
- **리턴**: 쉴드 유지 시 지속 보너스 — 매 턴 쉴드 받는 캐릭터와 시너지
- **리스크**: 쉴드 없는 턴에는 단일기. 매 턴 쉴드 부여 선행 필요
- **결론**: "보호받는 딜러" 포지션. Warrior(Duran)/Healer/Bard와 자연 파티 시너지

### 필요 메카닉
- `BehaviorTag`: `Bulwark` (rank = 보너스 위력)
- 구현: `caster.Health.CurrentShield > 0` 체크. HealthComponent에 이미 Shield 프로퍼티 존재

### 적합 캐릭터 후보
- **Warrior(Duran)**: Shield Wall/Bastion으로 매 턴 쉴드 유지 → Bulwark 지속 발동
- **Ashe(Pyromancer)**: Ember로 인한 자해를 쉴드로 보완 + Bulwark 보너스로 딜 상승
- **Healer**: Holy Shield로 아군에게 쉴드 + 그 아군이 Bulwark 스킬 사용

---

## 컨셉 19 — LimitBreak (리미트 브레이크) — 전투당 1회만 사용 가능

**스킬 시안**: "전투당 1회만 사용 가능. 대신 위력 25 (단일기 3배)"

**이름**: `LimitBreak` — "리미트 브레이크". 한 번의 강력한 한방. FF 시리즈 Limit Break, 오버드라이브 등 전통 RPG 용어. 한국 유저에게 친숙

### 작동
- 조건: **전투당 1회**. 사용 후 해당 전투에서는 다시 사용 불가 (드로우에서 제외 or 회색 처리)
- DesignPillars 원칙 1 **규칙 5B(사용 제약 조건)** 예외 허용 — 게임 체인저급 + 3배 위력 + 루프 종착지 3기준 충족
- 전투 종료 시 리셋 (다음 전투 사용 가능)

### 리스크/리턴
- **리턴**: 위기 탈출 / 보스 처치 원킬 메카닉 — 단일기 3배 위력
- **리스크**: 1회 낭비 시 해당 전투 봉인. 타이밍 심사숙고 필요
- **결론**: "궁극기" 포지션. 강력한 보스 1킬 or 위기 역전용

### 필요 메카닉
- `BehaviorTag`: `LimitBreak` (rank = 1, 의미상 flag)
- 구현: `SkillInstance.usedThisBattle` bool 필드 추가. 사용 시 true → 드로우 풀에서 제외 or 슬롯에 "사용됨" 표시. 전투 시작 시 false 리셋
- UI: 슬롯에 "1/1" or "0/1" 표시. 사용 후 비활성화

### 적합 캐릭터 후보
- **Ashe(Pyromancer)**: Embrace of Cinders(LimitBreak) — 자살 폭딜 궁극기 (이미 컨셉 설계됨)
- **Warrior(Duran)**: Last Bastion(LimitBreak) — Vengeance 15 소모 궁극기
- **Taranis**: Thunderstorm(LimitBreak) — 전하 네트워크 풀충전 + 광역 데미지 결정타

---

## 컨셉 20 — Flank (측면) — 행 기준 가장자리 대상만

**스킬 시안**: "적 행의 가장 왼쪽 or 가장 오른쪽 적만 타겟 가능. 대신 위력 14 (단일기 +75%)"

**이름**: `Flank` — "측면/측면 공격". 행의 양 끝(측면)만 노린다는 직관. Darkest Dungeon 위치 기반 스킬(Holy Lance, Point Blank Shot)에서 정통 용어. 전술 군사 용어

### 작동
- 타겟 = 적 리스트에서 `index == 0`(왼쪽) or `index == count-1`(오른쪽)
- 스킬별로 왼쪽 전용/오른쪽 전용/양쪽 선택 가능
- 가운데 적은 타겟 불가 → 회색 처리
- 타겟팅이 자동이 아님 — **플레이어가 가장자리 적 클릭 시에만 발동**

### 리스크/리턴
- **리턴**: 단일기 +75% 위력 보상. 가장자리 적 처치로 적 행 축소
- **리스크**: 원하는 적이 가운데 있으면 사용 불가. 적 배치 운 의존
- **결론**: Darkest Dungeon식 위치 기반 전술 도입. 향후 "적 행 시스템" 확장과 시너지

### 필요 메카닉
- `BehaviorTag`: `Flank` (rank = 보너스 위력, 별도 필드로 left/right/both 지정)
- 구현: `enemies[0]` or `enemies[count-1]`만 타겟 후보. PlayerActionController 타겟 필터링
- **선행 요건**: 현재 Team_Log는 적 리스트의 index가 의미 없음(위치 미구분). 추후 "적 행/열 시스템" 도입 시 본 컨셉 진가. **단기 구현 보류**

### 적합 캐릭터 후보
- **Archer**: 저격수 측면 사격 — 적 진형 우측(위치4) 마법사 우선 처치
- **Rogue**: 측면 기습 — 회피하는 적 측면 노림
- **Warrior(Duran)**: 방패 밀쳐내기 — 측면 적을 밀어서 진형 교란 (DD Push 메카닉)

---

## 컨셉 21 — Bounty (현상금) — 이 스킬로 적 처치 시 추가 보너스

**스킬 시안**: "이 스킬로 적 처치 시 AP +2 환급 + 골드 +20"

**이름**: `Bounty` — "현상금". 처치 시 현상금 받듯 보상받는다는 직관. 서부/해적 영화 전통 용어, LoL `Bounty Hunter` 패시브에서 공통. 기존 `Reaper`(킬 시 위력 보너스)와 다름 — **Bounty는 자원(AP/골드/영혼) 보상**

### 작동
- 조건: **이 스킬로 적 사망**. 즉 스킬의 마지막 데미지가 적 HP를 0 이하로
- 보상: rank에 따라 다양 — AP 환급 / 골드 / 영혼(Necromancer) / 쿨다운 리셋
- 기존 `Reaper`와 분리: Reaper=위력 보너스, Bounty=자원 보상. 둘 다 킬 트리거지만 보상 유형 다름

### 리스크/리턴
- **리턴**: 처치 시 자원 회수 → 연쇄 처치(Ap 환급) / 경제 이득(골드)
- **리스크": 처치 못하면 보너스 0. 위력 자체는 단일기와 비슷 (과잉 보너스 방지)
- **결론**: "마무리 전문" 포지션. Rogue Eviscerate, Necromancer Soul Harvest와 자연 융합

### 필요 메카닉
- `BehaviorTag`: `Bounty` (rank = 보상 위력, 별도 필드로 보상 유형 지정: APRefund/Gold/Soul/CooldownReset)
- 구현: 스킬 실행 후 `target.IsDead == true && lastDamageSource == thisSkill` 체크 → 보상 지급
- 기존 `Reaper` 처리 로직과 동일한 훅(OnKill)에서 분기

### 적합 캐릭터 후보
- **Rogue**: Eviscerate(Bounty)로 콤보 5 마무리 시 AP 회수 → 다음 스킬 연쇄
- **Necromancer**: Soul Harvest(Bounty)로 처치 시 영혼 +2 — 자원 회수 루프 핵심
- **Archer**: 처치 시 골드 보상 — "현상금 사냥꾼" 서사 강화

---

## 이름 사명 요약 (컨셉 5~21)

컨셉 5~7은 이름 후보 3개씩 제시 (사용자 결정), 컨셉 8~21은 단일 확정 이름 부여 (사용자 위임).

### 컨셉 5~7 (사용자 결정용 — 3후보 제시)

| 컨셉 | 1순위 추천 | 2순위 | 3순위 |
|------|-----------|------|------|
| **5. 이미 공격받은 대상** | **`FollowUp`** (후속타) | `Pursuit` (추격) | `GangUp` (집단 공격) |
| **6. 풀피 대상 강화** | **`FirstBlood`** (첫 피) | `Opener` (개막타) | `Vanguard` (선봉) |
| **7. 절반 이하 대상 강화** | **`Cull`** (도축) | `Predator` (포식자) | `CoupDeGrace` (쿠드그라스) |

### 컨셉 8~21 (이름 확정 — 사용자 위임)

| # | 이름 | 한국어 | 핵심 메카닉 | 기존 24종과의 충돌 |
|---|------|--------|-----------|------------------|
| 8 | `Fatigue` | 피로 | 매 사용 시 위력 -N | 없음 (Berserk=HP% 보너스와 다름) |
| 9 | `Momentum` | 관성 | 매 사용 시 위력 +N | 없음 (Intensify=스택 보너스와 다름) |
| 10 | `Echo` | 메아리 | 위력 절반 2회 시전 | MultiHit(동일대상고정)/Bounce(무작위)와 다름 |
| 11 | `Desperation` | 절박 | 잃은 HP당 위력 +N (선형) | **Berserk(HP% 임계 boolean)와 명확 분리** |
| 12 | `Wound` | 상처 | 잃은 HP당 위력 -N (선형) | Desperation의 음수 버전, 충돌 없음 |
| 13 | `Escalation` | 에스컬레이션 | 매 사용 시 AP cost +N | 없음 (CostDown과 정반대) |
| 14 | `Mastery` | 숙련 | 매 사용 시 AP cost -N | QuickDraw(1회 cost감소)와 다름 — **누적 스케일** |
| 15 | `GiantSlayer` | 거인살해자 | 적 MaxHP 임계값 이상 시 보너스 | TargetHighestHP(자동선택)와 다름 — **수동 타겟** |
| 16 | `AllIn` | 올인 | 사용 후 AP 0 시 보너스 | 없음 (QuickDraw와 다름 — 잔AP조건) |
| 17 | `Dominance` | 지배 | 적 HP < 나 HP 시 보너스 | Desperation과 정반대 포지션 |
| 18 | `Bulwark` | 방패벽 | 쉴드 보유 시 공격 보너스 | **ShieldBonus(쉴드+N 부여)와 명확 분리** |
| 19 | `LimitBreak` | 리미트 브레이크 | 전투당 1회 | 없음 (사용 제약 조건 예외) |
| 20 | `Flank` | 측면 | 행 가장자리 대상만 | **선행: 적 행/열 시스템 미구현** |
| 21 | `Bounty` | 현상금 | 킬 시 자원(AP/골드/영혼) 보상 | **Reaper(킬 시 위력보너스)와 분리 — 자원 보상** |

### 공통 패턴 (기존 24종과 일관성)
- 모두 1~3 음절로 짧음 (HeavyHit/Bounce/Chain/Pierce/Berserk 등과 동일)
- 행동/결과를 명사형으로 표현 (BurningTouch/Execution/Lifesteal 등과 동일)
- 기존 어휘와 뉘앙스 충돌 시 별도 섹션에서 명시적 분리 (위 표 "기존 24종과의 충돌" 열)

---

## 구현 로드링 (Phase CC 진행 시 반영)

### 우선순위 높음 — 단순 조건 체크 (코드 5줄 내외, UI 변경 없음)
- **컨셉 6 (FirstBlood)**: `target.CurrentHP == target.MaxHP`
- **컨셉 7 (Cull)**: `target.CurrentHP * 2 <= target.MaxHP`
- **컨셉 11 (Desperation)**: `caster.MaxHP - caster.CurrentHP` 기반 선형 위력
- **컨셉 12 (Wound)**: Desperation 음수 버전
- **컨셉 15 (GiantSlayer)**: `target.MaxHP >= threshold`
- **컨셉 16 (AllIn)**: `currentAP - skill.Cost == 0`
- **컨셉 17 (Dominance)**: `target.CurrentHP < caster.CurrentHP`
- **컨셉 18 (Bulwark)**: `caster.Shield > 0`
- **컨셉 2 (TargetHighestHP)**: 자동 선택. 코드 10줄
- **컨셉 4 (TargetFullHP)**: 타겟 필터링 + CanUse

### 중간 — 단일 상태 필드 추가 (코드 10~20줄)
- **컨셉 5 (FollowUp)**: TurnContext에 `_hitsTakenThisTurn` 딕셔너리. 전투 시스템 약간 확장. 15줄
- **컨셉 19 (LimitBreak)**: `SkillInstance.usedThisBattle` bool. 드로우 풀 필터링. 10줄
- **컨셉 21 (Bounty)**: 기존 OnKill 훅 확장 (Reaper와 공유). 보상 유형 분기. 15줄

### 중간-높음 — usesThisBattle 추적 인프라 (1회 구축, 4종 공유)
- **컨셉 8 (Fatigue) / 9 (Momentum) / 13 (Escalation) / 14 (Mastery)**: 모두 `SkillInstance.usesThisBattle` 동일 필드 사용. 인프라 20줄 구축 후 각 컨셉은 위력/cost 공식만 5줄씩 추가. **4종 동시 구현 권장** (단일 PR)

### 우선순위 낮음 — UI 작업量大 or 선행 인프라 필요
- **컨셉 1 (Distribute)**: 무작위 분배 로직. BehaviorTag 신규
- **컨셉 3 (MultiStrike)**: PlayerActionController 순차 타겟팅 모드 필요. UI 신규 모드
- **컨셉 10 (Echo)**: MultiStrike UI(순차 타겟) 공유 가능. 위력 절반 분기 추가. 15줄
- **컨셉 20 (Flank)**: **★선행 인프라 — 적 행/열 시스템 미구현**. 추후 "적 위치 시스템" 도입 시 진가. 단기 보류

### 캐릭터 통합 시점
- **Phase CC-1** (Warrior/Rogue/Pyromancer):
  - 핵심: 컨셉 **11(Desperation) / 15(GiantSlayer) / 16(AllIn) / 21(Bounty)** — Warrior Vengeance+Desperation, Pyromancer Ember+AllIn 자해폭딜, Rogue 콤보+Bounty 마무리
  - 보조: 5(FollowUp) / 7(Cull) — 콤보 연계
- **Phase CC-1B** (Cryomancer/Stormcaller):
  - 핵심: 컨셉 **8(Fatigue) / 9(Momentum) / 10(Echo)** — 마법사 반복 시전 패턴, Cryomancer Frostbolt(Echo)로 빠른 3스택
  - 보조: 1(Distribute) / 6(FirstBlood) — Stormcaller 연쇄
- **CC-Sibyl / CC-Taranis**:
  - Sibyl: 컨셉 **8(Fatigue) / 13(Escalation)** — 3턴 주기 콤보와 자연 스케일
  - Taranis: 컨셉 **14(Mastery) / 9(Momentum) / 15(GiantSlayer)** — 장기전 네트워크 + 보스전 보완
- **Phase CC-2** (Healer/Archer/Necromancer/Bard/Alchemist):
  - Healer: 18(Bulwark) / 14(Mastery) — 장기전 힐 cost 감소
  - Archer: 17(Dominance) / 4(TargetFullHP) — 저격 선제
  - Necromancer: 21(Bounty) — 영혼 수확 루프
  - Bard: 9(Momentum) / 14(Mastery) — 리듬 자원과 병행
  - Alchemist: 13(Escalation) / 10(Echo) — 시약 반응 연쇄

---

## 수치 균열 분석 (기존 단일기 위력 8 기준)

### 1~7 (기존 — 타겟/상황 의존)

| 컨셉 | 총 데미지 | 단일기 대비 | 조건 | 원칙 1 위상 |
|------|---------|-----------|------|-----------|
| 1 분배 (데미지/힐) | 12 데미지 / 24 힐 | +20% | 무작위 분배, 과잉 위험/낭비 | 강화 (자동 타겟) |
| 2 최대체력 | 10 | +25% | 가장 튼튼한 적만 (자동) | 강화 (자동 타겟) |
| 3 다단 | 6 | -25% | 매 타격 자유 선택 | 위력 감소 (유연성 보상) |
| 4 풀피 (사용제약) | 10 | +25% | 풀피 적 존재 시만 | **사용 제약** (예외) |
| 5 추격 (FollowUp) | 8→12 | +50% | 이번 턴 공격받은 적 | 강화 |
| 6 첫피 (FirstBlood) | 8→12 | +50% | 풀피 적에게 | 강화 |
| 7 도축 (Cull) | 8→14 | +75% | 절반 이하 적에게 | 강화 |

### 8~21 (신규 — 자원/상태/사용 이력 의존)

| 컨셉 | 총 데미지 | 단일기 대비 | 조건 | 원칙 1 위상 |
|------|---------|-----------|------|-----------|
| 8 Fatigue | 12→2 (사용 누적) | 초반 +50%, 후반 -75% | 매 사용 시 위력 -2 | 강화 (초반 집중) |
| 9 Momentum | 6→14+ (사용 누적) | 초반 -25%, 후반 +75% | 매 사용 시 위력 +2 | 강화 (장기전 보상) |
| 10 Echo | 5×2=10 | +25% | 2회 타겟 지정 | 강화 (유연성 보상) |
| 11 Desperation | 8 + 잃은HP/5 | 가변 | 잃은 HP 선형 비례 | 강화 (위기 보상) |
| 12 Wound | 8 - 잃은HP/5 | 가변 | 잃은 HP 선형 반비례 | **의도적 약점** |
| 13 Escalation | 8 (고정) | ±0 | 매 사용 시 cost +1 | 강화 (초반 cheap) |
| 14 Mastery | 8 (고정) | ±0 | 매 사용 시 cost -1 | 강화 (장기전 free) |
| 15 GiantSlayer | 8→14 | +75% | 적 MaxHP 임계값(예:100)+ | 강화 (보스 특화) |
| 16 AllIn | 8→16 | +100% | 사용 후 AP 0 | 강화 (터 순서 의존) |
| 17 Dominance | 8→12 | +50% | 적 HP < 나 HP | 강화 (우위 보상) |
| 18 Bulwark | 8→13 | +62% | 나에게 쉴드 존재 | 강화 (쉴드 의존) |
| 19 LimitBreak | 8→25 | +212% | 전투당 1회 | **사용 제약** (예외) |
| 20 Flank | 8→14 | +75% | 행 가장자리 적만 | 사용 제약 (위치 시스템 선행) |
| 21 Bounty | 8 (보상 별도) | ±0 + 자원 보상 | 이 스킬로 킬 시 AP/골드/영혼 | 강화 (자원 회수) |

> 단일기 위력이 현재 8~10이라고 가정. 실제 밸런스는 Quick Combat 시뮬레이터로 재측정.
> 어센션 × 부활 누적(MaxHP -10%) 환경에서 Desperation/Wound는 효과 감소 (잃은 HP 절대값 작아짐) — 자연 밸런스.
> 컨셉 5/6/7/10/11/13/14/15/16/17/18/21은 모두 **강화 조건**(DesignPillars 원칙 1 규칙 5A). 컨셉 4/19/20만 **사용 제약**(규칙 5B 예외).

---

## 메모

### 설계 패턴 분류 (21개 컨셉)

| 카테고리 | 컨셉 | 공통 원칙 |
|---------|------|----------|
| **타겟 자윤성 제한** (1~4) | Distribute / TargetHighestHP / MultiStrike / TargetFullHP | 자윤성 제한 → 위력/힐량 보상. StS Whirlwind vs Perfected Strike 전통. **Distribute는 데미지/힐 양쪽 모두 적용 가능** (SkillType 기반 자동 분기) |
| **상황 의존 보상** (5~7) | FollowUp / FirstBlood / Cull | 자율성 유지 + 특정 상황 보너스. **DesignPillars 규칙 5A(강화 조건)의 모범 사례** |
| **사용 이력 누적** (8~9, 13~14) | Fatigue / Momentum / Escalation / Mastery | 드로우 운이 의미를 갖는 핵심 메카닉 — 같은 스킬 자주 뽑힐 때 효과 변동 |
| **자원 상태 비례** (11~12) | Desperation / Wound | 잃은 HP 선형 스케일. **%가 아닌 절대값** (사용자 명시). 어센션 MaxHP 축소와 자연 밸런스 |
| **조건부 폭딜** (15~18) | GiantSlayer / AllIn / Dominance / Bulwark | 다양한 자원 상태(MaHP/AP/HP/Shield) 기반 보너스. 포지션 차별화 핵심 |
| **사용 제약 예외** (4, 19, 20) | TargetFullHP / LimitBreak / Flank | **DesignPillars 규칙 5B(사용 제약)** 예외 허용. 게임 체인저급/3배위력/루프 종착지 |
| **자원 회수 루프** (21) | Bounty | 킬 시 자원(AP/골드/영혼) 보상. 기존 Reaper(위력 보너스)와 분리 — **자원 보상** |

### 핵심 설계 결정

1. **★컨셉 4 vs 6**: 같은 "풀피 적" 상황, 다른 메카닉. 컨셉 4는 **사용 제약**(예외), 컨셉 6은 **강화 조건**(기본). DesignPillars 원칙 1의 두 조건 종류가 같은 상황에 어떻게 다르게 적용되는지 교과서적 사례. 신규 스킬 설계 시 **컨셉 6 패턴이 기본**, 컨셉 4는 게임 체인저급에만 예외 허용

2. **Desperation vs Berserk 분리**: 기존 `Berserk`는 "HP 50% 이하" 같은 boolean 임계값. `Desperation`(컨셉 11)은 잃은 HP × N의 **선형 스케일**. 둘 다 "다칠수록 강해짐"이지만 스케일 방식이 다름 → Berserk는 "임계 트리거", Desperation은 "점진 강화"

3. **Echo vs MultiHit vs MultiStrike 분리**:
   - `MultiHit`(기존 24종): 동일 대상 N회 고정
   - `MultiStrike`(컨셉 3): 매 타격마다 타겟 자유 지정 (3×2 같은 형태)
   - `Echo`(컨셉 10): 위력 절반으로 2회 시전 (같은 적 or 다른 적)
   세 메카닉 모두 "다회 타격"이지만 **타겟 지정 방식과 위력 분배**가 다름 → 명확한 용도 분리

4. **Bulwark vs ShieldBonus 분리**:
   - `ShieldBonus`(기존 24종): 쉴드 양 자체 +N 부여
   - `Bulwark`(컨셉 18): 쉴드 **보유 시** 공격 위력 보너스
   "쉴드 강화" vs "쉴드 있을 때 강화" — 명확한 분리

5. **Reaper vs Bounty 분리**: 둘 다 킬 트리거지만 **보상 유형**이 다름
   - `Reaper`(기존 24종): 킬 시 위력 보너스 (딜 증폭)
   - `Bounty`(컨셉 21): 킬 시 자원 보상 (AP/골드/영혼 회수) — 자원 루프 형성

### DesignPillars 원칙과의 연결

- **원칙 1 (드로우 운 → 전략)**: 컨셉 8~9/13~14(사용 이력)은 **같은 스킬 자주 뽑힐 때만 발동** → 드로우 운이 전략적 의미 획득. 매 턴 정답이 1개가 아니라 3~5개로 분할 (DesignPillars 핵심 가설 실현)
- **원칙 2 (약점-보조)**: 컨셉 12(Wound)는 **의도적 약점 부여 도구**. 특성/증강으로 "다칠수록 약해지는" 페널티를 보조하는 설계 공간 제공. 컨셉 7(Cull)/11(Desperation)은 "조건 미충족 시 단일기 동일 위력" → 잔존 약점으로 보조 매체 가치 유지
- **원칙 3 (강점 명확성)**: 컨셉 15(GiantSlayer)=보스 사냥꾼, 19(LimitBreak)=궁극기, 21(Bounty)=마무리 사냥꾼 등 **포지션별 차별화 도구**. 각 캐릭터 정체성과 직결

### Phase CC와의 융합

- BehaviorTag 우선순위: 기존 24종 + 새 **17종 후보** (컨셉 1~21 중 MultiStrike/Echo는 별도 UI 인프라, Flank는 위치 시스템 선행 필요 → 단기 14종, 장기 17종)
- 각 캐릭터 4스킬이 **4개 다른 컨셉** 가짐 → 매 턴 다른 퍼즐 (DesignPillars 원칙 1 + 4.5 원칙 2 준수)
- 신규 컨셉이 기존 24종과 중복/충돌 시 **"기존 어휘와의 충돌" 열에서 명시적 분리** — 어휘 확장 시 혼란 방지

### 사용자 검증 필요

- 각 컨셉의 기본/보너스 위력 비율 (예: 8/12 vs 8/14 vs 8/16)은 Quick Combat 시뮬레이터 기반 밸런스 튠 필요
- 어센션 5/10/15 + 부활 MaxHP 누적 환경에서 Desperation/Wound 효과 감소 측정
- 컨셉 14(Mastery)의 cost 0 도달 후 AP 무한 루프 가능성 검증 — min cost 1 권장
- 컨셉 16(AllIn)의 "매 턴 마지막 스킬" 강제가 드로우 운과 어떻게 상호작용하는지 시뮬레이션
