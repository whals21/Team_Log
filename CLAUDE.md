# Team Log - AI Harness

> 턴제 카드 드로우 로그라이크 RPG (Unity C#)

## 프로젝트 구조

```
Assets/
├── 01.Scenes/          # BattleScene, TestCombatScene, MapScene
├── 02.Scripts/
│   ├── Characters/     # Character, CharacterData, Components/
│   ├── Combat/         # AI/, Draw/, Turn/
│   ├── Skill/          # SkillData
│   ├── Map/            # MapNode, MapFloor, MapGenerator, GameRunState
│   ├── Reward/         # RewardData, ItemData, RewardManager
│   ├── Shop/           # ShopData, ShopManager
│   ├── Event/          # EventData, EventManager
│   ├── UI/
│   │   ├── Battle/     # 모든 전투 UI
│   │   ├── Map/        # MapView, MapNodeButton, MapSceneSetup
│   │   ├── Reward/     # RewardUI, RewardCard
│   │   ├── Shop/       # ShopUI, ShopItemSlot
│   │   └── Event/      # EventUI
│   ├── Editor/         # DataGenerator, SceneBuilder, MapSceneBuilder
│   └── Tests/          # Editor-mode 단위 테스트 (61개)
├── 03.Data/            # ScriptableObject 에셋
├── 08.Resource/        # 폰트, 이미지
└── 09.Docs/            # 기획서, 가이드, 작업일지
```

## 아키텍처

### 핵심 클래스 관계
```
맵 시스템 (Phase 3):
MapSceneSetup (진입점)
    ├── GameRunState (런 전체 상태: 층, 골드, 파티 HP 유지, BonusAP 명상 보너스, Train/Rest/Meditate 캠프파이어 선택)
    ├── FloorEnemyPool (층별 적 풀: normalEnemies/eliteEnemies/boss, MapSceneBuilder에서 와이어링)
    ├── MapFloor (단일 층 맵)
    │   └── MapNode (노드: 타입, 위치, 연결, 방문 상태)
    ├── MapView (맵 시각화 UI)
    ├── EventUI / ShopUI / RewardUI / RestUI (노드별 서브 UI)
    └── MapGenerator (프록시럴 맵 생성)

전투 시스템 (Phase 1-2):
BattleSceneSetup (진입점, SetBattleData로 외부 데이터 수신, bonusAP 명상 보너스 전달)
    ├── TurnManager (턴 사이클 오케스트레이터, DealDamage 중앙화, 특성 훅 연동, bonusFirstTurnAP 첫 턴 AP 보너스)
    │   ├── SkillDrawSystem (가중치 랜덤 드로우)
    │   └── TurnContext (턴 상태: phase, AP)
    │       └── AP: 파티 공유, 매 턴 1+생존수 전량 회복, 첫 턴 bonusAP 추가, OnAPChanged 이벤트
    ├── PlayerActionController (UI ↔ 전투 로직 중재자, AP 부족 차단, 리롤 중계)
    ├── EnemyAIController (패턴 기반 AI, 의도 표시, Taunt 타겟 우선)
    └── BattleUIManager (UI 패널 생성/관리, AP/리롤 이벤트 구독, GetPanelTransform)
        ├── TopBarUI (턴 카운터, AP 표시, 리롤 카운트)
        ├── ActionBarUI → ActionSlotUI (AP 부족 시 회색 처리, 리롤 버튼)
        ├── BattleEndOverlay (승리/패배 오버레이 + 계속하기 버튼)
        ├── BattleLogUI (전투 로그 표시)
        ├── PlayerSidebarPanel / EnemyDetailPanel (사망 시 alpha=0.4, ButtonArea에 특성 표시)
        ├── FloatingTextUI (데미지/힐/쉴드/MISS 플로팅 텍스트, BattleSceneSetup에서 delta 이벤트로 연동)
        └── StatusEffectBadge (상태이상 뱃지, 클릭 시 Info 영역에 설명 표시)

UI 시스템 (Phase 4):
SceneTransition (씬 트랜지션 페이드, DontDestroyOnLoad 싱글톤)
ToastUI (토스트 알림, 큐 기반, ShopUI 골드 부족/구매 성공에 활용)
UIAnimationHelper (FadeIn/FadeOut/ScaleFromZero, EventUI/ShopUI/RewardUI 패널 전환에 적용)
ConfirmationDialog (ShopUI 구매 확인, MapSceneSetup 보스/엘리트 전투 확인)

Character (순수 C# 클래스, MonoBehaviour 아님)
    ├── HealthComponent (HP/쉴드 관리, OnHPChanged/OnShieldChanged/OnDeath + delta: OnDamageTaken/OnHealApplied/OnShieldAdded, OnPreDeath 사망 방지)
    ├── StatComponent (ATK/DEF, base + modifier + permanent base 증가)
    ├── StatusEffectComponent (14종 상태이상 관리: Taunt 추가)
    ├── SkillInventoryComponent (스킬 목록, DrawSkill 가중치 뽑기)
    └── EnemyTraitHandler (적 패시브 특성 처리기: ShieldPrep/Agile/Sturdy/ArcaneFury/Corrosive/Enrage/ScaleArmor/Immortal)

ItemEffectApplier (순수 C# static, 아이템 효과 런타임 적용)
```

### 턴 사이클
`Draw → PlayerAction(AP 관리) → Execution → EnemyTurn → BattleEnd`

### 자원 시스템
- **AP (Action Point)**: 파티 공유 자원, 매 턴 시작 시 `1 + 생존 파티원 수` 전량 회복
- **스킬 Cost**: 0~3 (SkillData.Cost), 사용 시 AP 차감, 부족 시 스킬 사용 불가
- **쉴드 (Shield)**: 일시적 보호막, HP 바 위 갈색 바로 표시, 데미지를 HP보다 먼저 흡수, 턴 시작 시 리셋
- **리롤 (Reroll)**: 턴당 2회, 개별 슬롯 리롤만 지원, 이미 사용한 슬롯은 리롤 불가
- 적은 AP/리롤 시스템에서 제외 (EnemyAIController가 독립적으로 행동 결정)

### 데이터 계층
- **CharacterData** (ScriptableObject): 이름, 클래스, 기본 스탯, 스킬 목록, 적 특성(EnemyTrait)
- **SkillData** (ScriptableObject): 이름, 타입(Attack/Heal/Buff/Debuff/Shield/Purify), 타겟타입, 위력, 비용, 가중치, 상태이상
- 모든 데이터는 `Assets/03.Data/`에 `TeamLog/` 메뉴로 생성
- **DataGenerator 규칙**:
  - `GetOrCreateAsset<T>`로 기존 에셋 로드 우선 (GUID 보존, 참조 끊김 방지)
  - `Object.name = fileName` (에셋 파일명과 일치, Unity 경고 방지)
  - 한국어 표시명은 `_skillName`/`_characterName` 등 private 필드에 저장
  - 스킬 Cost 포함하여 모든 필드를 명시적으로 설정
- **MapSceneBuilder 규칙**:
  - 에셋 필터링은 `Object.name`이 아닌 파일 경로 기반 (`namePrefix` 파라미터)
  - 층별 적 풀 분리: `FloorEnemyPool` 구조체로 층별(normal/elite/boss) 독립 관리
  - 층 배정: F1=숲(슬라임/고블린/늑대/독버섯), F2=유적(해골/박쥐/미라/궁수), F3=심연(망령/그림자/악마병사/가고일)
  - 엘리트 배정: F2=기존3종, F3=신규3종(주술사/대장/악마마법사)
  - 보스 배정: F1=고블린왕, F2=드래곤, F3=마왕

## 코딩 규칙

### 필수
- **네임스페이스**: `TeamLog` 최상위, 하위는 폴더 구조 따름
  - `TeamLog.Characters` — Character, CharacterData, Components, SkillData
  - `TeamLog.Combat.Turn` — TurnManager, TurnPhase, TurnContext
  - `TeamLog.Combat.Draw` — SkillDrawSystem
  - `TeamLog.Combat.AI` — EnemyAIController, EnemyActionPattern
  - `TeamLog.Map` — MapNode, MapFloor, MapGenerator, GameRunState
  - `TeamLog.Reward` — RewardData, ItemData, RewardManager
  - `TeamLog.Shop` — ShopData, ShopManager
  - `TeamLog.Event` — EventData, EventManager
  - `TeamLog.UI.Battle` — 전투 UI 클래스
  - `TeamLog.UI.Map` — 맵 UI 클래스
  - `TeamLog.UI.Reward` — 보상 UI 클래스
  - `TeamLog.UI.Shop` — 상점 UI 클래스
  - `TeamLog.UI.Event` — 이벤트 UI 클래스
  - `TeamLog.Editor` — 에디터 도구
- **이벤트 기반 통신**: 클래스 간 직접 참조 최소화, C# event/Action 사용
- **UI-로직 분리**: UI 클래스는 표시만, 게임 로직은 Combat/Characters 계층에
- **순수 C# 우선**: MonoBehaviour는 Unity 라이프사이클이 필요한 경우만 사용
- **ScriptableObject로 데이터 관리**: 하드코딩 금지

### 네이밍 컨벤션
- 클래스: PascalCase (`TurnManager`, `SkillDrawSystem`)
- private 필드: camelCase (`turnNumber`, `drawnSkills`)
- 이벤트: `On` 접두사 (`OnPhaseChanged`, `OnHPChanged`)
- ScriptableObject 에셋: `Class_Name` 형식 (`Char_Warrior`, `Mage_Fireball`)
- enum: PascalCase, 값도 PascalCase

### 파일 배치
- 새 스크립트는 해당 시스템 폴더에 배치
- UI 스크립트는 항상 `02.Scripts/UI/{시스템명}/` (Battle, Map, Reward, Shop, Event)
- Editor 스크립트는 항상 `02.Scripts/Editor/`

## 가드레일 (금지 사항)

- `Library/`, `Temp/`, `obj/` 폴더 수정 절대 금지
- `.meta` 파일 수동 생성/수정 금지 (Unity가 자동 관리)
- `FindObjectOfType`, `Find`, `GameObject.Find` 사용 금지
- `PlayerPrefs` 사용 금지 (데이터는 ScriptableObject)
- MonoBehaviour가 불필요한 클래스에 MonoBehaviour 상속 금지
- `Assets/02.Scripts/` 외부에 게임 스크립트 배치 금지
- UI 스크립트에서 직접 게임 로직(데미지 계산, 상태이상 적용 등) 구현 금지
- 기존 public API(이벤트, 메서드 시그니처) 변경 시 하위 호환성 확인

## 클래스 크기 관리 (God Class 방지)

### 파일 크기 기준

| 줄 수 | 조치 |
|--------|------|
| ~300줄 이하 | 양호 |
| 300~400줄 | 책임 분리 검토 |
| 400~600줄 | partial class 분할 필수 |
| 600줄 초과 | 클래스 자체의 설계 재검토 |

### 단일 책임 분리 원칙

하나의 클래스가 서로 다른 성격의 책임을 동시에 가지면 안 된다. 특히:

- **"생성"과 "연결"의 분리**: UI 계층을 생성하는 코드와 컴포넌트를 연결(wire)하는 코드는 별개 파일로 분리
- **"로직"과 "표시"의 분리**: 게임 로직(데미지 계산, 상태 변경)과 UI 표시(텍스트 갱신, 색상 변경)는 별개 클래스로 분리
- **"데이터"와 "처리"의 분리**: 상태 보관(TurnContext)과 상태 변경 오케스트레이션(TurnManager)은 별개 클래스

### partial class 분할 컨벤션

에디터 도구 등 부득이하게 큰 클래스가 필요한 경우 partial class로 분할:

```
Editor/
  XxxSceneBuilder.cs         — 진입점 + 상수 + 오케스트레이션
  XxxSceneBuilder.UI.cs      — UI 계층 생성 (Create* 메서드)
  XxxSceneBuilder.Setup.cs   — 컴포넌트 부착 + 참조 연결 (AutoWire*, Setup*)
```

- 각 partial 파일의 `namespace`와 `class` 선언은 동일하게 유지
- 파일 상단에 해당 파일의 역할을 한 줄로 주석 명시
- 진입점 파일의 클래스 주석에 분할 파일 목록 참조 표기

## Unity 함정 체크리스트 (반드시 숙지)

> 이 프로젝트에서 실제로 발생한 버그를 기반으로 작성된 체크리스트.
> 새 코드 작성 시 반드시 아래 패턴이 해당되는지 확인.

### 1. 코루틴 + SetActive(false) = 코루틴 즉시 종료

**문제**: MonoBehaviour의 `gameObject.SetActive(false)`를 호출하면 해당 객체의 **모든 코루틴이 즉시 종료**됨.

**실제 사례**: `RewardUI.HideAndNotify()`에서 `FadeOut()`이 `gameObject.SetActive(false)` 후 콜백이 실행되지 않아 보스 클리어 후 층 이동 불가.

**해결**: 코루틴 내에서 `SetActive(false)` 이후에 실행해야 할 코드가 있으면, `SetActive(false)` **이전**에 호출.

```csharp
// BAD — FadeOut이 SetActive(false)하면 다음 줄 실행 안 됨
private IEnumerator HideAndNotify()
{
    yield return UIAnimationHelper.FadeOut(cg); // 내부에서 SetActive(false)
    _callback?.Invoke(); // 절대 도달하지 않음!
}

// GOOD — 콜백을 먼저 호출
private IEnumerator HideAndNotify()
{
    _callback?.Invoke(); // 먼저 실행
    yield return UIAnimationHelper.FadeOut(cg); // 그 후에 페이드아웃
}
```

### 2. 비활성 객체의 Awake() 지연 호출

**문제**: 씬 로드 시 비활성 상태인 gameObject는 `Awake()`가 호출되지 않음. **첫 활성화 시점에 `Awake()`가 호출됨**.

**실제 사례**: `BattleEndOverlay.Awake()`에서 `gameObject.SetActive(false)`를 호출하여, `Show()`로 활성화 → `Awake()` 호출 → 즉시 다시 비활성화됨.

**해결**: `Awake()`에서 `gameObject.SetActive(false)` 호출 금지. 초기 비활성화는 씬 빌더나 인스펙터에서 처리.

### 3. FadeOut/FadeIn + 콜백 패턴

**규칙**: `UIAnimationHelper.FadeOut()`은 마지막에 `gameObject.SetActive(false)` 함. 코루틴 안에서 FadeOut **이후**에 실행해야 할 로직이 있으면 콜백을 FadeOut **이전**에 실행하거나, 별도의 비동기 패턴 사용.

## 새 기능 추가 워크플로우

1. **기존 코드 읽기** — 관련 클래스 먼저 읽고 패턴 파악
2. **데이터 정의** — ScriptableObject부터 설계 (`03.Data/`)
3. **로직 구현** — 순수 C# 클래스로 핵심 로직 작성
4. **이벤트 연결** — BattleEventManager 또는 클래스 이벤트로 UI 연동
5. **UI 구현** — `02.Scripts/UI/Battle/`에 UI 컴포넌트 작성
6. **에디터 도구** — DataGenerator 업데이트, 필요시 SceneBuilder 수정
7. **데이터-로직 연동 검증** — ScriptableObject 필드값이 로직에 실제로 반영되는지 확인
8. **통합 테스트** — 씬 리빌드 후 엔드투엔드 검증

## 현재 개발 상태

- **Phase 1 (코어 전투)**: 완료
- **Phase 2 (전투 완성)**: 완료 (상태이상, 적 AI, UI)
- **Phase 3 (로그라이크 요소)**: 완료 (맵 시스템, 보상/상점, 이벤트)
- **Phase 4 (폴리싱)**: 진행 중
  - 4A: 버그 수정 완료 (턴 카운터/배틀로그 와이어링, CanvasScaler)
  - 4B: 사망 상태 + 핵심 데이터 완료 (사망 시각, 스탯 스케일업, Taunt/Purify)
  - 4C: 데이터 확충 완료 (엘리트/보스 6종, 이벤트 6종, 층별 스케일링, 아이템 효과)
  - 4D: UI 폴리싱 기반 완료 (씬 트랜지션, 토스트, 승리/패배 오버레이)
  - 4E: UI 디테일 완료 (플로팅 텍스트, 패널 애니메이션, 확인 다이얼로그)
  - 4F: UI 연동 완료 (FloatingText delta 이벤트, FadeIn/FadeOut 전환, ConfirmationDialog 활성화, 골드 부족 피드백)
  - 4G: 적 특성 시스템 완료 (8종 패시브 특성, TraitHandler 훅, TraitBadge UI, MISS 플로팅 텍스트)
  - 4H: 층별 적 풀 + 신규 적 9종 + 휴식 선택지 완료 (FloorEnemyPool 3층 분리, RestUI 3선택지, BonusAP 파이프라인)
  - 4I: 버그 수정 2건 + 자동화 테스트 인프라 완료 (BattleEndOverlay/RewardUI 버그, 61개 단위 테스트, 어셈블리 분리)
  - **잔여**: 사운드, 이펙트, 밸런싱

### 미구현 항목
- 스킬 레벨/업그레이드
- EnemyDetailPanel 가디언/아크카 버튼 실제 로직 (TODO 스텁 상태)
- 사운드/이펙트 시스템

### 세션 종료 체크리스트

매 작업 세션 종료 전 반드시 수행:
- [ ] CLAUDE.md 아키텍처 섹션 업데이트 (새 클래스/관계/이벤트 추가 시)
- [ ] CLAUDE.md 미구현 항목 업데이트 (완료/변경/추가 항목 반영)
- [ ] 작업 일지 기록 (`09.Docs/WorkLog/YYYY-MM-DD.md`)
- [ ] 커밋 & 푸시

## 가비지 컬렉터 (프로젝트 청소)

> 아무리 좋은 설계라도 개발 과정에서 오염이 누적되면 프로젝트가 부패한다.
> 가비지 컬렉터는 정기적으로 오염을 감지하고 제거하여 프로젝트 토대를 건강하게 유지한다.

### 수집 대상 (무엇이 오염인가)

| 카테고리 | 감지 기준 | 예시 |
|----------|-----------|------|
| **죽은 코드** | 어디서도 참조되지 않는 메서드, 클래스, 변수 | 사용되지 않는 private 메서드 |
| **미사용 에셋** | 씬/스크립트에서 참조되지 않는 ScriptableObject, 프리팹 | 테스트용으로 만든 후 잊힌 데이터 |
| **기획 불일치** | GameDesign.md와 충돌하는 구현 | 기획에 없는 임의 추가 시스템 |
| **아키텍처 위반** | CLAUDE.md 규칙을 위반한 코드 | UI 스크립트에 하드코딩된 데미지 계산 |
| **중복 로직** | 동일한 로직이 여러 위치에 산재 | 데미지 계산이 TurnManager와 UI에 모두 존재 |
| **유령 참조** | 존재하지 않는 클래스/메서드/에셋 참조 | 삭제된 스크립트를 참조하는 씬 오브젝트 |
| **스텁/TODO 방치** | 미구현 상태로 장기 방치된 코드 | 빈 메서드, TODO 주석만 있는 클래스 |

### 수집 주기

```
경량 수집 — 매 작업 세션 종료 전
  → 해당 세션에서 수정한 파일에 한해 즉시 검사

심층 수집 — Phase 전환 시 또는 사용자 요청 시
  → 전체 프로젝트 스캔
```

### 수집 절차

```
1. 스캔     — 오염 후보 탐지
2. 분류     — 즉시 삭제 / 사용자 확인 필요 / 보류
3. 보고     — 발견된 오염 항목을 사용자에게 목록화하여 보고
4. 정화     — 사용자 승인 후 삭제
5. 검증     — 삭제 후 컴파일/런타임 정상 확인
```

### 삭제 분류 기준

- **즉시 삭제 가능**: 명백한 죽은 코드, 주석 처리된 코드 블록, 미사용 using문
- **사용자 확인 필요**: 미사용 ScriptableObject/프리팹, 기획과 불일치하는 기능, 스텁 코드
- **보류**: 현재 미사용이나 Phase 3/4에서 사용 예정인 코드 (명시적 TODO 있는 경우)

### 가비지 컬렉터 실행 시 따라야 할 규칙

1. **삭제 전 반드시 사용자에게 보고** — 자동 삭제 금지, 항상 승인 요청
2. **삭제 순서**: 데이터(03.Data) → 스크립트(02.Scripts) → 씬(01.Scenes)
3. **삭제 후 검증**: Unity 콘솔 에러, 깨진 참조, 누락된 의존성 확인
4. **git 상태 확인**: 삭제 전 `git status`로 보호해야 할 미커밋 작업 확인
5. **복구 가능성 확보**: 삭제는 git으로 복구 가능한 상태에서만 수행

## 기술 스택

- **Unity**: 6000.0 (Unity 6)
- **렌더파이프라인**: URP
- **UI**: TextMesh Pro (NanumGothic SDF 한국어 폰트)
- **입력**: New Input System
- **에셋**: GUI Pro-CasualGame (Layer Lab)
