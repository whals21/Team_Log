# Team Log - AI Harness

> 턴제 카드 드로우 로그라이크 RPG (Unity 6, URP, C#)

## 프로젝트 구조

```
Assets/
├── 01.Scenes/          # TitleScene, MapScene, BattleScene, BattleTestScene, BattleUITestScene
├── 02.Scripts/
│   ├── Characters/     # Character, CharacterData, Components/, SkillData, CharacterTraitData
│   ├── Combat/         # AI/, Draw/, Turn/, DamageCalculator, CombatEventBus
│   ├── Skill/          # SkillData, BehaviorKeyword, Behaviors/ (Pipeline + ISkillBehavior)
│   ├── Map/            # MapNode, MapFloor, MapGenerator, GameRunState
│   ├── Reward/         # RewardData, RewardManager, AugmentOfferGenerator
│   ├── Shop/           # ShopData, ShopManager
│   ├── Event/          # EventData, EventManager
│   ├── Meta/           # MetaProgressionManager, AscensionManager, MetaUpgradeData
│   ├── UI/             # Battle/, Map/, Reward/, Shop/, Event/, Meta/
│   ├── Debug/          # GameDebugOptions (SRDebugger)
│   ├── EditorDebug/    # BattleTestSceneSetup, BattleTestConfig
│   ├── Editor/         # DataGenerator(10 partial), *SceneBuilder, BalanceSimulator(5 partial)
│   └── Tests/          # Editor-mode 단위 테스트 (211개)
├── 03.Data/            # ScriptableObject 에셋 (Characters/Skills/Relics/Items/Themes/Traits)
└── 09.Docs/            # 기획서(GameDesign, DesignPillars), 캐릭터 문서, 작업일지, 밸런스 리포트
```

## 아키텍처 핵심

### 전투 파이프라인 (★ 핵심 — 모든 스킬 통일 처리)
- **SkillExecutionPipeline.ExecuteSkill**: 단일 흐름 (PowerModify→TargetModify→ApplyMain→PostApply→OnKill)
- **ISkillBehavior 인터페이스**: 각 BehaviorKeyword가 하나의 클래스. Open-Closed 원칙 달성 (새 Behavior 추가 시 Pipeline 수정 0줄)
- **BehaviorRegistry**: Keyword→ISkillBehavior 매핑, lazy Initialize
- **SkillExecutor**: Pipeline 호출 + 글로벌 훅(유물/특성) 처리
- **SkillInstance**: GetCombinedBehaviors(스킬 본체+증강 평탄화), UpgradeLevel, UsesThisBattle

### 클래스 계층 (이름 = 역할)
- **TurnManager** (Turn 오케스트레이터, AP, 순차 적 턴 모드 지원)
- **DamageCalculator** (static, 데미지 공식 + 특성/유물 훅 + 치명타 + OnCriticalHit 이벤트)
- **EnemyAIController / EnemyActionPattern** (상황인식 가중치 AI)
- **Character** (순수 C#, HealthComponent/StatComponent/StatusEffectComponent/SkillInventoryComponent/EnemyTraitHandler/PlayerTraitHandler/Resource 소유)
- **HealthComponent** (HP/Shield 관리, List<ShieldInstance> 부여자별 추적, TakeDirectDamage 쉴드 우회, Revive 부활, ApplyMaxHpModifier)
- **CharacterResourceComponent** (Ember/Vengeance/Frost/Prophecy/Charge/Shadows 6종 자원, 매 턴 자동 작동, OnStacksChanged 이벤트)
- **CombatEventBus** (static 전투 중앙 버스 14종 이벤트)
- **RelicHandler** (파티 전체, _triggerDepth 무한루프 방지 MAX 5)
- **CharacterTraitHandler** (Character 1명당 1개, Owner 한정 적용)
- **GameRunState** (런 상태, 4스테이지, SelectedThemes 무작위 채택, ApplyEliteBonus/ApplyStageClearBonus + Pending* 생명주기)
- **StageThemeData** (12테마: 4스테이지×3후보, normalEnemies/eliteEnemies/boss/themeEvents)
- **MetaProgressionManager / AscensionManager** (순수 C# static, 런 보상/특성 해금/어센션 15레벨)
- **SaveManager** (JsonUtility + 파일 I/O, Run/Character/Meta DTO)

### 자원 시스템
- **AP**: 파티 공유, 매 턴 `1 + 생존 파티원 수` 회복
- **Shield**: 일시적 보호, HP 바 위 갈색 바, 데미지를 HP보다 먼저 흡수, 턴 시작 리셋, ShieldInstance로 부여자별 추적
- **Reroll**: 턴당 2회 (메타 강화로 +1), 개별 슬롯, 이미 사용한 슬롯은 리롤 불가
- **드로우 가중치**: 모든 플레이어 스킬 weight=25 균등
- **자원 6종**: Ember(Ashe, 자해폭딜)/Vengeance(Duran, 복수)/Frost(Lumi, 통제)/Prophecy(Sibyl, 1턴 뒤)/Charge(Taranis, 네트워크 연쇄)/Shadows(Umbra, 치명타)

### 부활 시스템 (Phase CC-0)
- 전투 종료 시 사망자 50% 부활 + MaxHP 영구 0.9배 누적
- 생존자 HP 100% 회복
- 파티 전멸만 런 종료

### 메타 재화 (Phase 8)
- **기억의 조각 (MemoryFragments)**: 일반. floor*5(패배)/floor*10(승리) + battlesWon + (승리 시 +50) + gold/100
- **영혼 (Souls)**: 희귀, 승리 시만. 1 + floor/2
- **기본 해금 유물 16종**, 메타 해금 26종
- **캐릭터 특성**: 캐릭터당 3개 (1 기본 + 2 메타 해금), 런 시작 시 1개 장착

### 어센션 (Phase ASC)
- 15레벨, 6개 modifier 유형 (EnemyHp/PlayerHp/Reroll/StartGold/Heal/BossHp)
- 레벨 6/12는 빈 레벨 (구 EnemyAtkPercent 제거)
- 승리 시 +1 (최대 15)

## 코딩 규칙 (필수)

### 네임스페이스
- 최상위 `TeamLog`, 하위는 폴더 구조 따름
  - `TeamLog.Characters`, `TeamLog.Combat.Turn`, `TeamLog.Combat.Draw`, `TeamLog.Combat`, `TeamLog.Combat.AI`, `TeamLog.Map`, `TeamLog.Meta`, `TeamLog.Reward`, `TeamLog.Shop`, `TeamLog.Event`, `TeamLog.UI.Battle/Map/Title/Meta/Reward/Shop/Event`, `TeamLog.Editor`, `TeamLog.EditorDebug`
  - **주의**: `TeamLog.Debug` 사용 금지 (UnityEngine.Debug 충돌)

### 원칙
- 이벤트 기반 통신 (클래스 간 직접 참조 최소화, C# event/Action)
- UI-로직 분리 (UI는 표시만, 게임 로직은 Combat/Characters에)
- 순수 C# 우선 (MonoBehaviour는 Unity 라이프사이클 필요 시만)
- ScriptableObject로 데이터 관리 (하드코딩 금지)

### 네이밍
- PascalCase 클래스, camelCase private 필드, `On` 접두사 이벤트
- SO 에셋: `Class_Name` (`Char_Warrior`, `Mage_Fireball`)
- enum: PascalCase (값도 PascalCase)

### 파일 배치
- 새 스크립트는 해당 시스템 폴더, UI는 `02.Scripts/UI/{시스템명}/`, Editor는 `02.Scripts/Editor/`

## 가드레일 (금지 사항)

- `Library/`, `Temp/`, `obj/` 폴더 수정 절대 금지
- `.meta` 파일 수동 생성/수정 금지
- `FindObjectOfType`, `Find`, `GameObject.Find` 사용 금지
- `PlayerPrefs` 사용 금지 (데이터는 ScriptableObject/SaveManager)
- MonoBehaviour 불필요한 클래스에 상속 금지
- `Assets/02.Scripts/` 외부에 게임 스크립트 배치 금지
- UI 스크립트에서 직접 게임 로직 구현 금지
- 기존 public API 변경 시 하위 호환성 확인

## 클래스 크기 관리 (God Class 방지)

| 줄 수 | 조치 |
|-------|------|
| ~300 | 양호 |
| 300~400 | 책임 분리 검토 |
| 400~600 | partial class 분할 필수 |
| 600+ | 설계 재검토 |

**분리 원칙**: 생성/연결, 로직/표시, 데이터/처리는 별개 파일/클래스로.

### partial class 컨벤션
- 진입점 파일에 분할 목록 참조 표기
- 각 파일 상단 역할 한 줄 주석
- namespace/class 선언 동일 유지

**주요 partial 예**:
- DataGenerator (10파일: main/Augments/Events/Relics/Palettes/Stages/Traits/MetaUpgrades/Ascension/PhaseCC)
- BalanceSimulator (5파일: main/Combat/Run/Report/Synergy)
- BattleUISceneBuilder (5파일: main/UI/UI.Sidebar/UI.Overlay/Setup)
- BattleTestSceneBuilder (2파일)

## Unity 함정 체크리스트

> 실제 발생한 버그 기반. 새 코드 작성 시 확인.

1. **코루틴 + SetActive(false)**: 코루틴 내에서 SetActive(false) **이전**에 콜백 실행 (FadeOut 패턴)
2. **비활성 객체 Awake 지연**: Awake에서 SetActive(false) 금지, 초기 비활성화는 씬 빌더/인스펙터에서
3. **DOTween asmdef 경계**: `DOTween.To()` 직접 사용 (cg.DOFade() 등 확장메서드는 TeamLog.Runtime asmdef에서 안 보임)
4. **VLG/HLG childControl=true 잠금**: 부모 LayoutGroup이 `childControlWidth/Height=true`면 자식 transform이 회색 잠금 → LayoutElement.ignoreLayout=true로 해제
5. **ScriptableObject 새 SerializeField 추가 시 using 점검**: CS0246 에러 시 using부터 확인
6. **TakeDamage vs TakeDirectDamage**: 쉴드 우회 필요 시 TakeDirectDamage 별도 사용 (Pierce/Execution)
7. **Lifesteal 회복량 = lastActualDamage/2** (power/2 아님)
8. **BehaviorTag rank 합산**: AddAugment는 "이미 부착된 증강과의 충돌"만 검사 (스킬 본체와의 합산 허용)
9. **CombatEventBus 정적 이벤트 누수**: 반복 전투 시 매 팩 `CombatEventBus.Clear()` + `DamageCalculator.ClearEvents()` + `SkillExecutor.ClearEvents()` 명시 호출
10. **RelicHandler 구독 생명주기**: 매 전투마다 `UnsubscribeEvents → ClearCombatEventBus → SetPlayerParty → SubscribeEvents` 사이클
11. **OnKill 트리거 키워드 집계**: Non-Passive 트리거 키워드는 별도 집계 헬퍼 사용
12. **TMP_Dropdown 팝업 빈칸**: Content에 VLG/CSF 배치 금지 (TMP_DefaultControls 표준)
13. **Mask.Reset() Editor 리셋**: Mask 추가 **후에** color/pivot 설정
14. **씬 자기 리로드 static 플래그**: Start에서 consume (OnDestroy premature clear 주의)
15. **MCP update_component 참조 필드 한계**: Unity Object 참조 필드 직접 설정 불가 → `SerializedObject.FindProperty.objectReferenceValue` + `ApplyModifiedProperties` 패턴 필요
16. **UI Image sprite null + raycastTarget=true**: sprite null이면 Graphic이 raycast 무시 → `Sprite.Create(Texture2D.whiteTexture, ...)`로 WhiteSprite 할당

## 현재 개발 상태 (2026-07-16 기준)

### 완료 Phase (요약)
| Phase | 핵심 산출물 |
|-------|------------|
| 1-2 | 코어 전투 / 상태이상 14종 / 적 AI / UI |
| 3 | 로그라이크 (맵/보상/상점/이벤트) |
| 4A-4Q | 폴리싱 (UIPalette, DOTween, SRDebugger, CameraShake, VFXManager URP, 사운드 42 SFX, Motion Titles, 아이콘, 저장) |
| 5 + 6A | 메타프로세션 기반, 유물 42종 (16 기본+26 시너지), CombatEventBus |
| 7A-7G | 4스테이지 인프라 + 12테마 분화 + 엘리트/스테이지 클리어 보상 3택1 + BattleTestScene 인터랙티브 |
| 8A-8F | 메타프로세션(기억/영혼) + 24 특성 + 30 강화 + CharacterTraitHandler + MetaShopUI |
| E | 이벤트 퀄리티 (공통 25 + 테마별 24, EventRiskLevel, 연쇄 NextEventId) |
| ASC | 어센션 15레벨 + 보스 12종 교체 (테마별 전용) |
| BK | BehaviorKeyword 24종 시스템 (AugmentType 통합) |
| CC-0 | 부활 시스템 (사망자 50% 부활 + MaxHP 0.9배 누적) |
| ARCH 1-5 | 스킬 조립식 파이프라인 (ISkillBehavior + 22종 구현체) |
| CC 1차 | 5캐릭터 구현 (Ashe/Duran/Lumi/Sibyl/Taranis) + 자원 4종 + Pipeline 통합 |
| UNIFIED-P | 완전 통일 파이프라인 (Attack/Heal/Shield/Buff 동일 경로, ExecutionPhase ApplyMain/PostApply 일반화) |
| CC 2차 | Taranis Wire 전파(Propagate) + Duran ShieldInstance + 조건부 보너스 + 자원/특성 UI (ResourceBadge/TraitBadge) |
| CC 3차 | 스킬 설명 갱신 + ResourceBadge 원형 디자인 + 패널 레이아웃 |

### 진행 중
- **Phase CC-2A (Umbra, the Rogue)**: 치명타 시스템 + ShadowsResourceComponent + StrongVsDebuffBehavior 코드 구현. .asset 생성/Play 검증 잔여. 상세 `2026-07-14.md`, `Characters/ReworkDrafts/02_Rogue.md`
- **기존 6종 리워크 기획**: Healer/Archer/Necromancer/Alchemist/Bard (초안만). 우선순위: Archer → Healer → Bard → Alchemist → Necromancer. `Characters/ReworkDrafts/INDEX.md`
- **BattleScene UI 개편 (2026-07-15~16)**: 5컬럼 동기화 그리드, 남색 톤, APArea 시각 분리(파란 테두리+밝은 남색), TargetBox 명시적 앵커, 캐릭터 카드 100px 확장 (ATK/DEF+자원을 HPBar 아래). Play 모드 최종 검증 잔여.

### 잔여 (주요)
- **Phase CC-2A 완성**: DataGenerator 메뉴 실행(Umbra .asset 생성), 특성 효과 적용(ShadowsMaxUp/StrongVsDebuff), 밸런스 시뮬레이션
- **밸런스 재측정**: Quick Combat 1000팩 + Full Run 100회 (최신 Phase CC/UNIFIED-P 반영). BalanceSimulator에 CharacterTraitHandler 통합 점검
- **단위 테스트 추가**: 네트워크 연쇄, Prophecy, Heal 자원 비롯
- **기존 8종 스킬 리워크**: BehaviorTag 부여로 조립식 혜택
- **Phase ARCH 잔여**: 타겟팅 Behavior(Spread/Bounce/MultiHit/Explosion/AOEAuto) TargetModify Phase 이관, 상태 추적 인프라(FollowUp/LimitBreak/Echo 등)
- **가비지 컬렉션**: 구 보스 3종(GoblinKing/Dragon/DemonLord) + Warrior/Mage orphan 에셋. 사용자 승인 후 삭제
- **런타임 검증**: F1→F4 엔드투엔드 런, BattleTestScene 7가지 시나리오, 어센션 런타임, 보스 12종

## 새 기능 추가 워크플로우

1. 기존 코드 읽고 패턴 파악
2. 데이터 정의 (ScriptableObject부터)
3. 순수 C# 클래스로 핵심 로직
4. 이벤트/Action으로 UI 연동
5. UI 컴포넌트 작성 (`UI/{시스템명}/`)
6. DataGenerator 업데이트
7. 데이터-로직 연동 검증
8. 씬 리빌드 후 엔드투엔드 검증

## 가비지 컬렉터

> 정기적 오염 감지/제거로 프로젝트 건강 유지.

**수집 대상**: 죽은 코드, 미사용 에셋, 기획 불일치, 아키텍처 위반, 중복 로직, 유령 참조, 스텁/TODO 방치

**주기**: 경량(매 세션 종료 시 해당 세션 수정 파일), 심층(Phase 전환/사용자 요청 시 전체 스캔)

**규칙**:
1. 삭제 전 반드시 사용자에게 보고 (자동 삭제 금지)
2. 삭제 순서: 데이터(03.Data) → 스크립트(02.Scripts) → 씬(01.Scenes)
3. 삭제 후 컴파일/런타임 정상 확인
4. git status로 보호할 미커밋 작업 확인
5. 복구 가능 상태(git)에서만 수행

## 세션 종료 체크리스트

- CLAUDE.md 아키텍처/미구현 항목 업데이트 (새 클래스/이벤트/완료 항목)
- 작업 일지 기록 (`09.Docs/WorkLog/YYYY-MM-DD.md`)
- 커밋 & 푸시

## 기술 스택

- **Unity**: 6000.0 (Unity 6), URP
- **UI**: TextMesh Pro (NanumGothic SDF 한국어 폰트)
- **입력**: New Input System
- **에셋**: GUI Pro-CasualGame, Epic Toon FX, SRDebugger, DOTween, CombatMagicSpellsVIISFX, Motion Titles Pack
- **테스트**: 211개 (Editor-mode 단위)

## ★사용자 설계 결정 (절대 준수)

- **CharacterTable.csv atk=0/def=0은 의도**: "스킬 위력 = 실데미지" 단순화. ATK=0/DEF=0이면 damage = bonusPower. StS/Balatro식 "카드 숫자 = 타격수치". Phase ASC-B에서 보스 atk/def 0 일관화 + Attack 스킬 위력으로 보전 (F1:+5, F2:+8, F3:+6, F4:+8).
- 어센션 EnemyAtkPercent 제거 (enum 7→6종, 레벨 6/12는 빈 레벨)
- **행동 가중치 기반 AI** (EnemyActionPattern 5규칙 배수)
- **드로우 가중치 25 균등** (모든 플레이어 스킬)
- **자원 비례 위력**: SkillData.ResourcePowerPerStack, Pipeline.ApplyMain에서 모든 타입(Attack/Heal/Shield/Buff)에 적용
- **ShieldInstance** (부여자 기반 쉴드 추적, ShieldFlag.GivesChargeOnAbsorb로 Taranis Grounding Field 통합)
