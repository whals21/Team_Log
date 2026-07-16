# [DRAFT → 확정 예정] Cael, the Alchemist — "매번 새로운 발견"

> **상태**: 🟢 확정 (2026-07-17 Discover 컨셉 + 핵심 결정 완료 — 코드 구현 대기, 별도 Phase CC-2E)
> **슬롯**: Alchemist (기존 Char_Alchemist 리워크)
> **상위 문서**: [INDEX.md](INDEX.md), [CharacterConceptReview.md 5.10](../../CharacterConceptReview.md)
> **기존 특성 파일**: `Trait_Alch_PotionMaster/ToxicBurst/ReinforcedPotion.asset`

### 2026-07-17 확정 사항 (사용자 제안 + 결정)
- **이름**: Cael (켈트 "약초학자")
- **자원**: 없음 (순수 발견 메커니즘)
- **선택지 수**: 3개 (하스스톤 발견 표준)
- **발견 풀**: 스킬별 독자 풀 (각 5-7개 효과 중 3개 랜덤 추출)
- **UI 방식**: 모달 팝업 (하스스톤식 — 스킬 클릭 시 중앙에 3버튼 패널)

---

## 1. 정체성 (한 문장)

> **"매 스킬이 새로운 발견의 순간 — 같은 스킬을 써도 매번 3개의 랜덤 선택지 중 상황에 맞는 것을 골라 쓴다. 연금술은 곧 예측 불가능한 가능성."**

## 2. 이름

**Cael** (켈트 "약초학자" 변형) — 짧고 발음 쉬움, Alchemist 정체성에 부합

## 3. 역할군

- **주 역할군**: 하이브리드 (매 스킬마다 회복/버프/디버프/유틸 중 하나 선택 가능)
- **부 역할군**: 적응형 서포터 (상황에 맞는 효과 즉시 제공)

## 4. 강점 / 약점

| 강점 | 약점 |
|------|------|
| 매 스킬마다 5-7가지 가능성 | **선택지가 랜덤** — 원하는 효과 안 나올 수 있음 |
| 상황에 맞는 효과 선택 (유연성) | 모달 팝업이 게임 흐름 일시정지 (빠른 플레이 방해) |
| 드로우 운 의존 감소 (한 스킬에 3 옵션) | 밸런스 튜닝 어려움 (각 효과별 위력 조정) |
| 높은 리플레이성 (매 게임 다름) | 단일 역할 부족 (회복/딜/버프 반쪽짜리) |

**DesignPillars 약점 유형**: **랜덤 의존** (원하는 효과가 안 나오면 효율 급감)

## 5. 고유 메카닉: Discover (발견)

### 핵심 메카닉
```
[스킬 클릭 (타겟 지정)]
  ↓
[모달 팝업: 3개 랜덤 효과 버튼] (별도 UI)
  ↓
[플레이어 1개 클릭]
  ↓
[선택한 효과 즉시 발동]
```

### 발견 풀 구조 (스킬별 독자 풀)
- 각 스킬은 자체 효과 풀(5-7개)을 가짐
- 스킬 시전 시 풀에서 **무작위 3개** 추출
- 플레이어가 3개 중 1개 선택

### 자원 없음 (사용자 결정)
- ResourceBadge 대신 **"마지막 발견 효과"** 표시 (UI 옵션)
- 자원 관리 부담 없이 순수 전략적 선택에 집중

### 기존 9종 자원과의 차별화 ⭐
| 자원 | 축전 패턴 | 본질 |
|------|---------|------|
| Ember/Vengeance/Shadows/Combo | 개인 행동 | 딜러/탱커 |
| Prophecy/Charge/Frost | 시간/공간/자원 축 | 서포터/제어 |
| Mercy/Melody | "누구를 위해"/"이전 메아리" | 보호/리듬 서포터 |
| **Alchemist** | **"매 행동이 결정의 순간" (선택 기반)** | **발견 서포터** ⭐ |

→ 완전히 새로운 축. 자원이 아니라 **선택지 자체가 메카닉**

## 6. 스킬 4종 (회복/버프/디버프/유틸 4영역 — 각각 발견)

### 각 스킬의 발견 풀 설계

#### 1. Mending Brew (회복 물약, AP 2)
> 풀에서 3개 랜덤 추출:

| 효과 | 확률 가중치 |
|------|-----------|
| HP 10 힐 (단일) | 30% |
| HP 15 힐 (단일, 강화) | 20% |
| 쉴드 10 부여 (단일) | 20% |
| 도트 정화 (Poison/Burn/Bleed 제거) | 15% |
| HP 8 힐 + 도트 면역 1턴 | 15% |

#### 2. Strengthening Brew (버프 물약, AP 2)
> 풀에서 3개 랜덤 추출:

| 효과 | 확률 가중치 |
|------|-----------|
| 단일 ATK+3 (2턴) | 25% |
| 단일 DEF+3 (2턴) | 20% |
| 단일 AP+1 | 20% |
| 단일 쉴드 8 + ATK+2 (2턴) | 15% |
| 단일 치명타 확률 +30% (1턴) | 10% |
| 광역 ATK+1 (1턴) | 10% |

#### 3. Crippling Brew (디버프 물약, AP 2)
> 풀에서 3개 랜덤 추출 (단일 적 대상):

| 효과 | 확률 가중치 |
|------|-----------|
| 적 ATK-3 (2턴) | 25% |
| 적 DEF-3 (2턴) | 20% |
| 적 Stun 1턴 | 15% |
| 적 Poison 3턴 (매 턴 3 데미지) | 15% |
| 적 Burn 3턴 (매 턴 4 데미지) | 15% |
| 적 Bleed 3턴 (매 턴 3 데미지) | 10% |

#### 4. Catalytic Brew (유틸리티 물약, AP 3)
> 풀에서 3개 랜덤 추출:

| 효과 | 확률 가중치 |
|------|-----------|
| 광역 6 데미지 | 20% |
| 단일 15 폭딜 | 20% |
| 무작위 적 3회 타격 (각 5 데미지) | 15% |
| 파티 전체 쉴드 5 | 15% |
| 이번 턴 드로우 가중치 +50 | 10% |
| 단일 적에게 Charge 부여 (Taranis 시너지) | 10% |
| 단일 적 + 단일 아군 위치 교환 (고급) | 10% |

### 조건 다양성 검증 (4.5 원칙 2)
- Mending → 회복 영역 (대상 상태에 따라 선택지 가치 변동)
- Strengthening → 버프 영역 (현재 부족한 스탯 보충)
- Crippling → 디버프 영역 (적 약화 방식 선택)
- Catalytic → 유틸리티 영역 (상황 특수 효과)

→ 4개 모두 다른 영역/목적. 매 턴 다른 퍼즐. ✅

### 기존 스킬 매핑
| 기존 | 신규 | 변경 |
|------|------|------|
| Alch_HealPotion (12, AP1) | Mending Brew | AP 1→2, 발견 5효과 중 3 선택 |
| Alch_PoisonBomb (6+Poison, AP2) | Crippling Brew | 단일 적 → 발견 디버프 풀 |
| Alch_BoostPotion (ATK+4, AP1) | Strengthening Brew | 발견 버프 풀 |
| Alch_ShieldPotion (10, AP2) | (Strengthening에 통합) 또는 Catalytic |
| (신규) | Catalytic Brew | 유틸리티 발견 풀 (광역/폭딜/특수) |

## 7. BehaviorTag / 시스템 설계

### 신규 인프라 필요 (★ 구현 복잡도 높음)
1. **DiscoverSystem** (신규 클래스)
   - 각 스킬의 DiscoverPool 정의 (ScriptableObject 또는 인스턴스)
   - 랜덤 3개 추출 (가중치 기반)
   - 플레이어 선택 대기
2. **DiscoverEffect** (신규 데이터 구조)
   - 효과 설명, 위력, StatusEffectType, BehaviorTag 등 캡슐화
3. **DiscoverModalUI** (신규 UI 컴포넌트)
   - 스킬 클릭 시 모달 팝업
   - 3개 버튼 + 효과 설명 + 아이콘
   - 플레이어 클릭 시 발동
4. **SkillData 확장**
   - `isDiscover: bool` 플래그 (발견 스킬 여부)
   - `discoverPoolId: string` (발견 풀 참조)
5. **TurnManager/SkillExecutor 확장**
   - 발견 스킬 시전 시 모달 호출 → 선택 대기 → 효과 적용
   - 코루틴 또는 async 패턴

### 기존 BehaviorTag 활용
- 각 발견 효과는 기존 BehaviorTag/StatusEffectType으로 구성 가능 (Pierce, Stun, Poison, ATK+ 등)

## 8. 장착 특성 3종 리워크

| 특성 | 기존 효과 | 리워크 효과 | 해금 |
|------|---------|------------|------|
| **물약 명인** (기본) | 힐/쉴드 +10% | **발견 선택지 3 → 4개** (더 많은 옵션) | 기본 |
| **독성 폭발** | 도트 지속 +2턴 | **발견 풀에서 독 계열 효과 가중치 +2배** (독 specialize) | 30 조각 |
| **강화 물약** | 전투 시작 HP +15 | **전투당 1회, 발견 선택지 "모두 적용" (3개 전부 발동)** | 60 조각 + 1 영혼 |

### 특성 키워드 매핑 (신규)
| 특성 | KeywordType | Trigger | Value |
|------|------------|---------|-------|
| 물약 명인 | **`DiscoverChoicesAdd`** (신규) | Passive | 1 (선택지 +1) |
| 독성 폭발 | **`DiscoverWeightBonus`** (신규) | Passive | 특정 카테고리 가중치 배수 |
| 강화 물약 | **`DiscoverApplyAll`** (신규) | Passive | 1 (전투당 1회 플래그) |

## 9. 밸런스 시나리오 (다수전 5턴)

```
턴 1: Crippling Brew 클릭 → 모달에서 (Poison / Burn / ATK-3) 중 Poison 선택 → 적 3명 Poison 부여
턴 2: Strengthening Brew 클릭 → (ATK+3 / DEF+3 / AP+1) 중 ATK+3 선택 → 아군 딜러 강화
턴 3: Mending Brew 클릭 → (HP 15 / 쉴드 10 / 정화) 중 정화 선택 → 도트 아군 구출
턴 4: Catalytic Brew 클릭 → (광역 6 / 단일 15 / 3회 타격) 중 광역 6 선택 → 적 잔당 정리
턴 5: Crippling Brew 클릭 → (Stun / Bleed / DEF-3) 중 Stun 선택 → 보스 통제
```

→ 매 턴 다른 선택지. 같은 스킬도 매번 다른 효과. 리플레이성 극대화

## 10. 파티 시너지

| 조합 | 시너지 | 핵심 |
|------|-------|------|
| **Cael + Ashe** | ★★★ | Crippling에서 Burn 선택 → Ashe Ember 시너지 + Burn 중복 |
| **Cael + Umbra** | ★★★ | Crippling에서 Poison/Bleed → Umbra StrongVsDebuff 자동 트리거 |
| **Cael + Lumi** | ★★ | Crippling에서 Freeze/Stun → Lumi Frost 가속 |
| **Cael + Elara** | ★★ | Mending에서 정화/힐 → Elara Mercy와 이중 회복 |
| **Cael + Taranis** | ★★ | Catalytic에서 Charge 부여 → Taranis 네트워크 가속 |
| **Cael + Calliope** | ★★ | Strengthening에서 ATK+ → Calliope Melody Valor와 중첩 |

## 11. ✅ 결정 항목 (2026-07-17 확정)

- [x] **이름**: Cael (켈트 "약초학자")
- [x] **자원**: 없음 (순수 발견 메커니즘)
- [x] **선택지 수**: 3개 (하스스톤 발견 표준)
- [x] **발견 풀**: 스킬별 독자 풀 (각 5-7개 효과)
- [x] **UI 방식**: 모달 팝업 (하스스톤식)
- [x] **4스킬 4영역**: 회복(Mending) / 버프(Strengthening) / 디버프(Crippling) / 유틸(Catalytic)
- [x] **확률 가중치**: 각 효과별 가중치 적용 (동일 확률 아님)
- [x] **특성 3종**: 물약 명인(선택지+1) / 독성 폭발(독 가중치) / 강화 물약(전투당 1회 전부 적용)

## 12. 리스크와 검증

| 리스크 | 완화 |
|-------|-------|
| 모달 UI가 게임 흐름 방해 | 빠른 애니메이션 + 키보드 단축키(1/2/3) 지원 |
| 랜덤으로 원하는 효과 안 나옴 | "물약 명인" 특성으로 선택지 4개 확장. 가중치로 자주 나오는 효과 조정 |
| 4스킬 모두 AP 2-3 = AP 부족 | Catalytic의 AP+1 옵션으로 보충 가능 |
| 밸런스 튜닝 어려움 (20개 효과) | 각 효과 위력을 80% 수준으로 설정 (동일 AP 대비 약간 약하게) |
| 멀티플레이어 시 동기화 문제 | 현재 게임이 싱글플레이어이므로 이슈 없음 |
| 모달이 BattleUI 구조와 충돌 | 최상위 캔버스에 별도 sortingOrder로 배치 |
| 발견 풀 데이터 관리 | ScriptableObject(DiscRecipePool)로 분리 — 에셋 기반 관리 |

## 13. 구현 메모 (코드 구현 시 — 별도 Phase CC-2E)

### ★ 구현 복잡도: Archer/Healer/Bard보다 **2-3배 큼** (UI 시스템 신규)

### 신규 인프라 필요
1. **DiscoverPoolData** (`Characters/` 또는 `Skill/`, ScriptableObject)
   - 각 스킬의 발견 풀 정의 (효과 리스트 + 가중치)
   - 에셋 기반 관리로 밸런스 튜닝 용이
2. **DiscoverEffect** (구조체 또는 클래스)
   - SkillType / Power / StatusEffectType / TargetType / BehaviorTag[] 캡슐화
   - 직렬화 가능해야 SO로 저장
3. **DiscoverSystem** (서비스 클래스)
   - `RollOptions(DiscoverPoolData pool, int count)` — 가중치 기반 랜덤 추출
   - `ApplyEffect(DiscoverEffect effect, Character caster, Character target)` — 선택 효과 발동
4. **DiscoverModalUI** (`UI/Battle/`)
   - 모달 패널 + 3-4개 버튼
   - DOTween fade-in/out 애니메이션
   - 키보드 단축키 1/2/3 지원
5. **SkillData 확장**
   - `_isDiscover: bool` / `_discoverPoolId: string` 필드 추가
6. **TurnManager/SkillExecutor 확장**
   - 발견 스킬 시전 시 코루틴으로 모달 대기
   - 플레이어 선택 후 효과 적용
7. **ResourceType**: 추가 안 함 (자원 없음). 대신 CharacterData에 알chemist 여부 플래그 (또는 CharacterClass.Alchemist로 판별)
8. **KeywordType 3종 신규**: DiscoverChoicesAdd / DiscoverWeightBonus / DiscoverApplyAll
9. **CharacterTraitHandler 확장**: 발견 관련 특성 처리

### 기존 코드 수정
- `DataGenerator.PhaseCC.cs` — Cael 스킬 4종 + Char_Alchemist 재생성. **DiscoverPoolData 에셋 4종도 생성**
- `DataGenerator.Traits.cs` — Alchemist 특성 3종 리워크
- `BattleDisplayUtil.cs` — 발견 스킬 설명 표시 (스킬 자체 효과 대신 "발견: 카테고리명")
- `BattleUIManager` — DiscoverModalUI 통합
- CSV: Char_Alchemist 행 + Alch_* 4행 제거

### 구현 난이도 추정
- DiscoverPoolData SO + DiscoverEffect: 중 (~60줄)
- DiscoverSystem 롤/적용 로직: 중상 (~100줄)
- DiscoverModalUI: **중상** (~150줄, UI 컴포넌트 + 애니메이션)
- TurnManager/SkillExecutor 코루틴 통합: 중 (~50줄)
- 특성 키워드 3종: 낮음 (~30줄)
- DataGenerator/UI: 낮음 (~60줄)
- **총합**: 약 450줄 + 4 스킬 .asset + 4 DiscoverPoolData .asset
- 이전 캐릭터(Umbra/Aster/Elara/Calliope)보다 **2-3배 큰 규모**

### 별도 Phase 진행 권장
- **Phase CC-2E-1**: 인프라 (DiscoverPoolData, DiscoverSystem, DiscoverModalUI) — UI 작업 메인
- **Phase CC-2E-2**: Cael 스킬 4종 + 특성 3종 + 밸런스
- **Phase CC-2E-3**: 테스트 + Play 모드 검증

### 테스트 계획 (PhaseCC2ETests.cs 신규)
1. DiscoverSystem: 가중치 기반 랜덤 추출 (시드 고정)
2. DiscoverEffect 적용: 각 효과 타입별 (힐/버프/디버프/유틸)
3. DiscoverChoicesAdd 특성: 3 → 4개 선택지
4. DiscoverApplyAll 특성: 전투당 1회 전부 적용
5. DiscoverWeightBonus 특성: 특정 카테고리 가중치 2배
6. 모달 UI 흐름 (수동 Play 모드 검증)

---

## 변경 이력

| 날짜 | 변경 |
|------|------|
| 2026-07-14 | 최초 작성 (Reagent Reaction 행동 이력 컨셉, 🔴 초안) |
| 2026-07-17 | **Discover(하스스톤 발견) 컨셉으로 전면 재작성** (사용자 제안). 매 스킬 3개 랜덤 선택지. 🟢 확정. 단 구현은 별도 Phase CC-2E 권장 (UI 규모 큼) |
