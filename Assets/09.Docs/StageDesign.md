# Team Log - Stage Design Document

> 런 구조 / 스테이지 테마 / 맵 노드 / 엘리트 분기 / 보상 설계
> GameDesign.md의 "3. 맵 시스템" 상위 문서

---

## 1. 개요

### 핵심 지표
| 항목 | 수치 |
|------|------|
| **스테이지 수** | 4 |
| **스테이지당 전투** | 5 (일반 4 + 보스 1) |
| **런당 총 전투** | 20 ± 2 (엘리트 분기에 따른 편차) |
| **스테이지당 테마 후보** | 3 → **런 시작 시 랜덤 1개 채택** |
| **엘리트 분기** | 스테이지당 2회 (2번째, 4번째 전투) |
| **보스** | 스테이지마다 1 + 런 종료 시 최종보스 페이즈 2 |

### 설계 원칙
1. **스테이지 = 학습 커리큘럼** — 각 스테이지는 플레이어가 배워야 할 공략 패턴을 가짐
2. **테마 후보군으로 반복 플레이 유지** — 4스테이지 × 3테마 = 81가지 조합
3. **엘리트 분기로 리스크/리턴 의사결정** — 자원 관리의 핵심 지점
4. **기존 콘텐츠 재사용 + 테마별 1~2개 신규 적** — 콘텐츠 분량 최적화

---

## 2. 스테이지 노드 구조

### 2.1 레이어 구성 (스테이지당 6 레이어)

```
Layer 0: [Start]
   ↓
Layer 1: [Battle #1]                          ← 일반 전투 (필수)
   ↓
Layer 2: [Battle #2]  또는  [Elite #2]        ← 분기 (리스크/리턴)
   ↓
Layer 3: [Battle #3] + [Shop] 또는 [Event]    ← 메인 전투 + 보조 노드
   ↓
Layer 4: [Battle #4]  또는  [Elite #4]        ← 분기 (보스 전 마지막 선택)
   ↓
Layer 5: [Boss]                               ← 스테이지 보스 (필수)
```

### 2.2 분기 노드 상세

- Layer 2, Layer 4는 **두 노드(Battle, Elite)가 항상 함께 존재**
- 이전 레이어 노드에서 어느 쪽으로 연결할지 **플레이어가 선택**
- Elite 노드는 시각적으로 강조 (빨강 테두리, 스켈레톤 아이콘 등)
- 선택은 **이전 노드 클리어 시점**에 확정 (되돌릴 수 없음)

### 2.3 레이어 3 보조 노드

- Shop, Event, Rest 중 1~2개가 Layer 3에 배치
- 플레이어 경로에 따라 0~2개 방문 가능
- 스테이지당 평균 상점 1회 + 이벤트 1회 + 휴식 0~1회

---

## 3. 스테이지 테마 후보군

> 각 스테이지는 3개의 테마 후보 중 하나가 런 시작 시 무작위 채택.
> 테마가 결정하면 **적 풀 + 엘리트 + 보스 + 키워드 + 기믹**이 함께 고정.
> 4스테이지 × 3테마 = 12개 테마 풀.

### Stage 1 — 튜토리얼 (학습: AP 관리, 타겟 우선순위)

| 테마 | 핵심 기믹 | 적 풀 (재사용 + 신규) | 엘리트 | 보스 |
|------|-----------|----------------------|--------|------|
| **잿빛 숲** (Grey Forest) | 재생/도적 — 장기전 불리 | 슬라임, 고블린, 늑대, 독버섯 | 고블린 족장 | 고블린왕 |
| **서리 고개** (Frosted Pass) | 둔화/빙결 — AP 압박 | 늑대, 독버섯, **빙석 골렘★**, **서리 정령★** | 얼음 군주 | 서리 거인 |
| **모래 평원** (Sunscorched Plains) | 회피/은폐 —命中 관리 | 고블린, 늑대, **모래뱀★**, **스콜피언★** | 사막의 암살자 | 모래폭풍 군주 |

★ = 신규 적 (DataGenerator에서 자동 생성)

**공통 학습 목표**: 스킬 우선순위, 리롤 활용, 집중 딜의 중요성

---

### Stage 2 — 체력 관리 (학습: 정화, 치명타, 순차 처결)

| 테마 | 핵심 기믹 | 적 풀 (재사용 + 신규) | 엘리트 | 보스 |
|------|-----------|----------------------|--------|------|
| **혈련 예배당** (Crimson Chapel) | 흡혈/부활 — HP 뺏기 | 박쥐, 미라, **뱀파이어★**, **구울★** | 흡혈 귀부인 | 뱀파이어 로드 |
| **부패 늪** (Rotbloom Bog) | 독/전염 — 도트 데미지 | 독버섯, 슬라임, **늪괴물★**, **역병술사★** | 역병 숙주 | 부패의 요정 |
| **유적 잔해** (Ruined Temple) | 언데드/저주 — 상태이상 | 해골, 박쥐, **리치★**, **스켈레톤 메이지★** | 해골 대장 | 고대 리치 |

**공통 학습 목표**: 상태이상 정화의 가치, 순차적 처결(한 놈 끝내고 다음), 힐/쉴드 밸런스

---

### Stage 3 — 자원 압박 (학습: 쉴드 활용, 다중 타겟, 버스트 딜)

| 테마 | 핵심 기믹 | 적 풀 (재사용 + 신규) | 엘리트 | 보스 |
|------|-----------|----------------------|--------|------|
| **심연 해구** (Abyssal Trench) | 흡수/속박 — 쉴드 셔터 | 망령, 가고일, **심해인★**, **크라켄 유생★** | 심해 사도 | 크라켄 |
| **번개 봉우리** (Stormpeak Summit) | 기절/연쇄 — 행동 차단 | 가고일, **뇌전 정령★**, **템페스트★**, 그림자 | 폭풍 소환사 | 천둥군주 |
| **그림자 골짜기** (Shadows Glade) | 은신/회피 — 예측 불가 | 그림자, 박쥐, **페이드★**, **눈 먼 자★** | 그림자 암살자 | 공허의 지배자 |

**공통 학습 목표**: 쉴드/히업 운영, 다중 타겟 우선순위, 적 intent 예측

---

### Stage 4 — 클라이맥스 (학습: 통합 운영, 페이즈 대비)

| 테마 | 핵심 기믹 | 적 풀 (재사용 + 신규) | 엘리트 | 최종보스 |
|------|-----------|----------------------|--------|---------|
| **불꽃왕좌** (Ember Throne) | 화염/폭발 — 고데미지 | 악마병사, 악마마법사, **화염정령★**, **살라만더★** | 화염 군단장 | 마왕 (불꽃 형상) |
| **영원동토** (Eternal Tundra) | 빙결/셧다운 — 행동 봉쇄 | **서리 악마★**, **빙결 정령★**, 가고일, 망령 | 빙결 여왕 | 빙하 마왕 |
| **마왕성 심장** (Demon Citadel) | 소환/다중 페이즈 — 지속 전멸 위협 | 악마병사, 악마마법사, **악마 기사★**, **군단장★** | 근위 대장 | 대마왕 (3페이즈) |

**공통 학습 목표**: 페이즈 전환 대비 자원 비축, 최종보스전 아드레날린, 지금까지 배운 모든 시스템 통합

---

## 4. 테마 키워드 시스템

### 4.1 키워드 표시

적 intent 툴팁과 특성 배지에 **테마 키워드**를 명시하여 플레이어가 기믹을 직관적으로 인지:

```
[흡혈] 뱀파이어 — 위력 12 (단일 적)
 HP의 50%를 회복합니다.
```

### 4.2 테마별 키워드 사전

| 키워드 | 의미 | 등장 스테이지 |
|--------|------|--------------|
| 재생 | 매 턴 HP 회복 | S1 잿빛 숲 |
| 둔화 | AP/시전 속도 감소 | S1 서리 고개 |
| 은폐 | 회피 확률 증가 | S1 모래 평원 |
| 흡혈 | 데미지의 %를 HP 회복 | S2 혈련 예배당 |
| 부활 | 1회 사망 시 부활 | S2 혈련 예배당 |
| 독 | 매 턴 도트 데미지 | S2 부패 늪 |
| 저주 | 디버프 효율 증가 | S2 유적 잔해 |
| 속박 | 타겟 행동 제한 | S3 심연 해구 |
| 기절 | 턴 스킵 | S3 번개 봉우리 |
| 화염 | 화염 데미지 + 전이 | S4 불꽃왕좌 |
| 빙결 | 행동 봉쇄 누적 | S4 영원동토 |
| 소환 | 적 추가 소환 | S4 마왕성 심장 |

---

## 5. 엘리트 분기 보상

### 5.1 일반 전투 vs 엘리트 보상 비교

| 항목 | 일반 전투 | 엘리트 전투 |
|------|----------|------------|
| 골드 | 15 ~ 25 | 40 ~ 60 |
| 증강 | 1개 (T1~T2, 랜덤) | 2개 (T2+, 보장) |
| 유물 | 5% 확률 | **100% 보장** |
| 추가 보너스 | 없음 | **승리 시 3택 1 팝업** |

### 5.2 엘리트 승리 보너스 (3택 1)

```
┌─────────────────────────────────────┐
│  엘리트 처치 보너스 — 하나를 선택하세요  │
├─────────────────────────────────────┤
│  □ 추가 유물 수령 (일반 등급 1개)       │
│  □ 파티 영구 강화                      │
│      → 전원 HP +15 / 전원 ATK +2 / 전원 DEF +2 (내부 랜덤) │
│  □ 다음 상점 50% 할인 + 골드 +100       │
└─────────────────────────────────────┘
```

- RewardUI를 확장하여 `EliteBonusUI` 신규 컴포넌트로 구현
- 엘리트 격파 직후 자동 팝업 (보상 수령 전 다음 노드로 이동 불가)
- 선택은 `GameRunState.PendingEliteBonus`에 임시 저장 후 다음 상점/보상 적용 시 소비

---

## 6. 스테이지 클리어 보상

### 6.1 보스 격파 후 (스테이지 종료)

1. **유물 1개 보장** (보스 보상, 등급 가중치 적용)
2. **스테이지 선택 보너스** (3택 1):
   - **버스트 준비**: 다음 스테이지 첫 전투 AP +2
   - **재충전**: 파티 전원 HP 50% 회복
   - **정보 우위**: 다음 상점에서 유물 1개 추가 진열 + 증강 1개 추가 진열

### 6.2 최종 스테이지(S4) 보스 격파

- **런 클리어 연출** (BattleTitleManager + RunEndOverlay)
- 메타 영구 해금: 새 캐릭터/도전과제/하드 모드 (추후 구현)
- `MetaSaveData`에 클리어 기록 저장

---

## 7. 테마 선택 메커니즘

### 7.1 런 시작 시 테마 랜덤 채택

```
GameRunState.StartRun()
    ↓
for each stage (1..4):
    candidates = StageThemes[stage]   // 3개 테마 후보
    selected = candidates.Random(rng)
    SelectedThemes[stage] = selected
    ↓
맵 생성 시 SelectedThemes[floorNumber]의 적 풀 사용
```

- 런 시작 시점에 4스테이지 분의 테마가 모두 결정됨
- `RunSaveData`에 저장/로드 지원 (이어하기 호환)
- 테마별 시드 사용으로 **같은 테마여도 매 런 다른 적 조합**

### 7.2 테마 노출 (UI)

- 맵 화면 상단에 현재 스테이지 테마명 표시: `Stage 2 — 혈련 예배당`
- 테마별 배경색/아이콘으로 시각적 차별화 (추후 자산 확보 시)

---

## 8. 데이터 구조

### 8.1 신규 ScriptableObject

```csharp
// StageThemeData.cs
[CreateAssetMenu(menuName = "TeamLog/Stage Theme")]
public class StageThemeData : ScriptableObject
{
    public string ThemeId;           // "S1_GreyForest"
    public string DisplayName;       // "잿빛 숲"
    public int StageNumber;          // 1~4
    public List<CharacterData> NormalEnemies;   // 일반 전투 풀
    public List<CharacterData> EliteEnemies;    // 엘리트 풀
    public CharacterData Boss;       // 스테이지 보스
    public List<string> ThemeKeywords; // ["재생", "도적"]
    public string Description;       // 테마 소개 (UI 표시용)
}
```

### 8.2 GameRunState 확장

```csharp
// 기존
public int CurrentFloor { get; } = 1;
public int TotalFloors { get; } = 3;

// 변경
public int CurrentFloor { get; } = 1;
public int TotalFloors { get; } = 4;  // 3 → 4
public List<StageThemeData> SelectedThemes { get; } = new();  // 런 시작 시 4개 채택
public StageThemeData CurrentStageTheme => SelectedThemes[CurrentFloor - 1];
```

### 8.3 FloorConfigs 확장 (스테이지 분기 노드 지원)

```csharp
// 기존: LayerCount, EliteCount, ShopCount, ...
// 변경: 분기 레이어 명시
public class MapGenerationConfig
{
    public int LayerCount = 6;               // 시작 + 4 전투 + 보스
    public List<int> BranchingLayers = new() { 2, 4 };  // 분기 레이어 인덱스
    public int EliteCount = 2;               // 분기당 1 → 총 2
    // ... ShopCount, RestCount, EventCount는 Stage별 설정
}

public static class FloorConfigs
{
    public static MapGenerationConfig GetConfig(int stage)
    {
        return stage switch
        {
            1 => new() { LayerCount = 6, BranchingLayers = {2,4}, ShopCount=1, RestCount=1, EventCount=1 },
            2 => new() { LayerCount = 6, BranchingLayers = {2,4}, ShopCount=1, RestCount=1, EventCount=1 },
            3 => new() { LayerCount = 6, BranchingLayers = {2,4}, ShopCount=1, RestCount=1, EventCount=2 },
            4 => new() { LayerCount = 6, BranchingLayers = {2,4}, ShopCount=1, RestCount=1, EventCount=2 },
            _ => new()
        };
    }
}
```

### 8.4 에셋 디렉토리 구조

```
Assets/03.Data/
├── Stages/
│   ├── Stage1/
│   │   ├── Theme_GreyForest.asset
│   │   ├── Theme_FrostedPass.asset
│   │   └── Theme_SunscorchedPlains.asset
│   ├── Stage2/
│   │   ├── Theme_CrimsonChapel.asset
│   │   ├── Theme_RotbloomBog.asset
│   │   └── Theme_RuinedTemple.asset
│   ├── Stage3/
│   │   ├── Theme_AbyssalTrench.asset
│   │   ├── Theme_StormpeakSummit.asset
│   │   └── Theme_ShadowsGlade.asset
│   └── Stage4/
│       ├── Theme_EmberThrone.asset
│       ├── Theme_EternalTundra.asset
│       └── Theme_DemonCitadel.asset
├── Characters/   (기존 + 신규 적)
└── Patterns/     (기존 + 신규 패턴)
```

---

## 9. 구현 로드맵

### Phase 7A — 스테이지 인프라 (우선)
1. `StageThemeData` SO 신규 정의
2. `GameRunState.TotalFloors = 4`, `SelectedThemes` 추가, 테마 랜덤 채택 로직
3. `FloorConfigs` 4스테이지 분기 레이어 확장
4. `MapGenerator` 분기 노드 생성 지원 (BranchingLayers)
5. `MapSceneBuilder` StageThemeData 기반 적 풀 와이어링
6. **기존 3스테이지 테마(숲/유적/심연)를 Stage 1의 3개 후보로 임시 매핑** (검증 우선)
7. 런 정상 클리어까지 엔드투엔드 검증

### Phase 7B — 엘리트 분기 + 보상
1. 엘리트 노드 시각적 강조 (MapNodeButton 스킨)
2. `EliteBonusUI` 신규 — 엘리트 격파 시 3택 1 팝업
3. `GameRunState.PendingEliteBonus` / `PendingShopDiscount` 플래그
4. `ShopManager` 할인 적용 로직
5. 보상 선택지 데이터 (RewardData 확장 또는 별도 EliteBonusData)

### Phase 7C — 스테이지 클리어 보상
1. `StageClearBonusUI` 신규 — 보스 격파 후 3택 1
2. 다음 스테이지 첫 전투 AP +2 버스트 (BattleSceneSetup bonusFirstTurnAP 재사용)
3. `RunEndOverlay` 최종 클리어 연출 강화

### Phase 7D — 테마 콘텐츠 확충
1. **Stage 1 테마 3종**: 기존 F1 적 재사용 + 각 테마별 신규 적 2종 (총 4종 신규)
2. **Stage 2 테마 3종**: 흡혈귀/독/언데트 — 각 2종 신규 (총 6종)
3. **Stage 3 테마 3종**: 심해/번개/그림자 — 각 2종 신규 (총 6종)
4. **Stage 4 테마 3종**: 화염/빙결/악마 — 각 2종 신규 + 최종보스 3종 (총 9종)
5. 총 신규 적 약 25종 + 스킬 50종 + 특성 8종 (DataGenerator 자동 생성)
6. 테마 키워드 라벨 시스템 (BattleDisplayUtil 확장)
7. 테마별 보스 페이즈 2 / 3

### Phase 7E — 테마별 균형
1. 테마별 카운터 유물/증강 상점 비치 가중치 (`ShopManager.GenerateShopSlots(currentTheme)`)
2. BalanceSimulator로 테마별 승률 측정 (12테마 × 30팩)
3. 특정 테마가 너무 어렵/쉬울 경우 적 스탯 조정

---

## 10. 밸런스 검증 기준 (BalanceSimulator 활용)

| 스테이지 | 일반 승률 목표 | 엘리트 승률 목표 | 보스 승률 목표 |
|---------|---------------|-----------------|---------------|
| S1 | 90 ~ 95% | 75 ~ 85% | 80 ~ 90% |
| S2 | 80 ~ 90% | 60 ~ 75% | 70 ~ 80% |
| S3 | 70 ~ 85% | 50 ~ 65% | 60 ~ 75% |
| S4 | 65 ~ 80% | 45 ~ 60% | 40 ~ 60% (클라이맥스) |

- 엘리트 승률이 보스 승률보다 **더 높아야 함** (엘리트는 선택적 도전이므로)
- S4 보스 승률이 너무 높으면(>70%) 클라이맥스 실망 — 낮추거나 페이즈 강화
- S1 일반 승률이 100%면 너무 쉬운 것 — 튜토리얼이어도 95% 이하 권장

---

## 11. 리스크 / 검토 포인트

1. **12테마 × 보스 = 12개 보스 에셋** — 콘텐츠 분량 큼. Phase 7D에서 단계적 추가 가능 (초반엔 기존 보스 재사용 + 리스킨)
2. **엘리트 분기 노드 UX** — 맵 시각화에서 분기를 자연스럽게 표현해야 함 (베지에 곡선 연결선 유지)
3. **테마 키워드 라벨** — 키워드가 너무 많으면 플레이어 인지 과부하. 처음엔 5~6개 핵심 키워드부터
4. **환경 패시브 modifier는 보류** — 사용자 결정에 따라 추후 도입 검토 (변수가 더 필요할 때)
5. **기존 F2/F3 적 재배치** — 유적/심연 테마가 일부 Stage 2/3으로 이동. 밸런스 재검증 필요

---

## 부록: 테마 조합 예시 (런 시드별)

```
런 A: S1 잿빛 숲 → S2 혈련 예배당 → S3 심연 해구 → S4 불꽃왕좌
런 B: S1 서리 고개 → S2 부패 늪    → S3 번개 봉우리 → S4 영원동토
런 C: S1 모래 평원 → S2 유적 잔해   → S3 그림자 골짜기 → S4 마왕성 심장
```

- 총 3^4 = **81가지 테마 조합** + 각 테마 내 적 조합 랜덤 → 사실상 무한 재플레이성
- 플레이어가 특정 테마를 싫어할 경우 "런 시작 시 3개 후보 중 1개 채택" 구조이므로 운 요소 존재. 추후 "테마 금지" 메타 진행 옵션 검토
