# Team Log - AI Harness

> 턴제 카드 드로우 로그라이크 RPG (Unity C#)

## 프로젝트 구조

```
Assets/
├── 01.Scenes/          # TitleScene, BattleScene, BattleTestScene, MapScene
├── 02.Scripts/
│   ├── Characters/     # Character, CharacterData, Components/
│   ├── Combat/         # AI/, Draw/, Turn/
│   ├── Skill/          # SkillData
│   ├── Map/            # MapNode, MapFloor, MapGenerator, GameRunState
│   ├── Reward/         # RewardData, RewardManager, AugmentOfferGenerator
│   ├── Shop/           # ShopData, ShopManager
│   ├── Event/          # EventData, EventManager
│   ├── UI/
│   │   ├── Battle/     # 모든 전투 UI
│   │   ├── Map/        # MapView, MapNodeButton, MapSceneSetup
│   │   ├── Reward/     # RewardUI, RewardCard
│   │   ├── Shop/       # ShopUI, ShopItemSlot
│   │   └── Event/      # EventUI
│   ├── Debug/          # GameDebugOptions (SRDebugger 인게임 디버그)
│   ├── EditorDebug/    # BattleTestSceneSetup, BattleTestConfig, BattleTestTemplateStore (인터랙티브 전투 테스트 씬)
│   ├── Editor/         # DataGenerator, SceneBuilder, MapSceneBuilder, BattleTestSceneBuilder
│   └── Tests/          # Editor-mode 단위 테스트 (73개)
├── 03.Data/            # ScriptableObject 에셋
├── 08.Resource/        # 폰트, 이미지
└── 09.Docs/            # 기획서, 가이드, 작업일지
```

## 아키텍처

### 핵심 클래스 관계
```
맵 시스템 (Phase 3 / 7A):
MapSceneSetup (진입점, SaveManager.HasSave 분기, OnRunEnded → RunEndOverlay, 자동 저장, StageThemeCandidateList[] 보유)
    ├── GameRunState (런 전체 상태: 4스테이지, 골드, 파티 HP 유지, BonusAP 명상 보너스, RerollTokens, BattlesWon/TotalGoldEarned 통계, AugmentGenerator 위임, SelectedThemes 리스트 + CurrentStageTheme 프로퍼티, 런 시작 시 테마 무작위 채택, ApplyEliteBonus/ApplyStageClearBonus + PendingShopDiscount/ExtraRelics/ExtraAugments 생명주기)
    ├── StageThemeData (ScriptableObject: themeId/displayName/stageNumber/normalEnemies/eliteEnemies/boss/spawnPatternTable/themeKeywords/description, DataGenerator.Stages.cs에서 자동 생성)
    ├── StageThemeCandidateList (스테이지별 3개 후보 배열, 런 시작 시 1개 무작위 채택)
    ├── MapFloor (단일 스테이지 맵, 6 레이어: Start + 4 전투 + Boss, BranchingLayers={2,4}에서 Battle/Elite 선택)
    │   └── MapNode (노드: 타입, 위치, 연결, 방문 상태, NodeData 객체 참조)
    ├── MapView (맵 시각화 UI)
    ├── EventUI / ShopUI / RewardUI / RestUI / RunEndOverlay / StageBonusUI (노드별 서브 UI + 런 종료 오버레이 + 엘리트/스테이지클리어 보상)
    └── MapGenerator (프로시저럴 맵 생성, BranchingLayers에서 Battle+Elite 노드 쌍 생성, StageThemeData.eliteEnemies 존재 여부로 분기 폴백)

전투 시스템 (Phase 1-2):
BattleSceneSetup (진입점, SetBattleData로 외부 데이터 수신, bonusAP 명상 보너스 전달, 순차 적 턴 코루틴 ExecuteEnemyTurnSequence — EnableSequentialEnemyTurn 모드에서 OnEnemyTurnSequenceStarted 이벤트 수신 시 적 한 명씩 행동 + 하이라이트/의도 클리어/VFX 지연, 매 행동 후 IsBattleEndedEarly 전멸 체크)
    ├── TurnManager (턴 사이클 오케스트레이터, AP 관리, 대상 결정, bonusFirstTurnAP 첫 턴 AP 보너스, 순차 적 턴 모드: EnableSequentialEnemyTurn + ExecuteSingleEnemyAction/CompleteEnemyTurn/IsBattleEndedEarly + OnEnemyTurnSequenceStarted/OnEnemyActing 이벤트, 기본값은 동기 모드로 시뮬레이터 호환)
    │   ├── SkillExecutor (인스턴스, 타겟별 스킬 실행 + 증강 해석 + 키워드 해석, OnSkillApplied 이벤트)
    │   ├── DamageCalculator (static, 데미지 공식 + 특성 훅 + 유물 훅 + 카운터 데미지, TurnManager/EnemyAIController에서 호출)
    │   ├── SkillDrawSystem (가중치 랜덤 드로우)
    │   └── TurnContext (턴 상태: phase, AP)
    │       └── AP: 파티 공유, 매 턴 1+생존수 전량 회복, 첫 턴 bonusAP 추가, OnAPChanged 이벤트
    ├── PlayerActionController (UI ↔ 전투 로직 중재자, AP 부족 차단, 리롤 중계)
    ├── EnemyAIController (상황인식 가중치 AI, 의도 표시, Taunt 타겟 우선, DamageCalculator.DealDamage 호출)
    └── BattleUIManager (UI 패널 생성/관리, AP/리롤 이벤트 구독, GetPanelTransform)
        ├── TopBarUI (턴 카운터, AP 표시, 리롤 카운트)
        ├── ActionBarUI → ActionSlotUI (AP 부족 시 회색 처리, 리롤 버튼, 툴팁)
        ├── BattleEndOverlay (승리/패배 오버레이 + ScaleFromZero/FadeIn 애니메이션)
        ├── BattleLogUI (LogEntryType 색상 코딩, ScrollRect 자동 스크롤)
        ├── PlayerSidebarPanel / EnemyDetailPanel (HP 트윈 애니메이션, 피격 플래시, 사망 페이드아웃, ButtonArea에 특성 표시)
        ├── FloatingTextUI (데미지/힐/쉴드/MISS 플로팅 텍스트, UIPalette 색상 참조)
        ├── StatusEffectBadge (한국어 이니셜 + 툴팁)
        ├── TooltipUI / TooltipTarget (호버 툴팁 시스템)
        ├── PartyStatusWidget (총 HP, 골드 표시)
        └── BattleScreenFlash (크리티컬 히트 시 전체 화면 점멸, DOTween.To 기반)

UI 시스템 (Phase 4):
TitleSceneSetup (타이틀 화면: 새 게임/이어하기, SaveManager.Meta 통계 표시)
SceneTransition (씬 트랜지션 페이드, DontDestroyOnLoad 싱글톤)
ToastUI (토스트 알림, 큐 기반, ShopUI 골드 부족/구매 성공에 활용)
UIAnimationHelper (DOTween 기반, FadeIn/FadeOut/ScaleFromZero/TweenAnchorMaxX/FlashColor/FadeToAlpha, 모두 Tween 반환)
ConfirmationDialog (ShopUI 구매 확인, MapSceneSetup 보스/엘리트 전투 확인)
UIPalette (색상 설계 토큰 SO, Default 정적 프로퍼티, 배경/강조/HP/쉴드/AP/스킬타입/로그/의도/슬롯/상태이상색/특성색/등급색 토큰)
AudioManager (DontDestroyOnLoad 사운드 싱글톤, 마스터 볼륨, 40개 편의 메서드, 스킬 타입별 사운드 분기)
AudioPalette (오디오 클립 매핑 SO, Resources/ 저장, DataGenerator에서 42개 클립 자동 할당 — CombatMagicSpellsVIISFX 활용)
VFXManager (전투 이펙트 매니저, URP Camera Stacking 방식, 15종 VFX: Hit/Critical/Heal/Shield/Death/Buff/Debuff/Burn/Poison/Freeze/Purify/Slash/Stun/Victory/Defeat)
VFXPalette (VFX 프리팹 매핑 SO, Resources/ 저장, 15개 Epic Toon FX 프리팹 자동 할당)
CameraShake (DontDestroyOnLoad 캔버스 흔들림 싱글톤, DOTween.To() 기반, 데미지 비례 강도, 크리티컬 시 강한 흔들림)
BattleTitleManager (전투 타이틀 애니메이션, Motion Titles Pack 활용, ShowBattleStart/ShowVictory/ShowDefeat)
SaveManager (저장/불러오기, JsonUtility + 파일 I/O, RunSaveData/CharacterSaveData DTO, MetaSaveData 런 간 영구 통계, RecordRunEnd 자동 정리)
GameDebugOptions (SRDebugger 인게임 디버그 옵션, Run State 조회 + 치트 메서드, TeamLog.EditorDebug 네임스페이스)

DeckViewerUI (파티 전체 스킬/아이템/유물 뷰어 오버레이, 캐릭터별 그룹화, MapSceneSetup._deckViewerUI, 탑 패널 "덱" 버튼)
TutorialUI (인터랙티브 튜토리얼 오버레이, TutorialStep 진행 상태, MetaSaveData.HasCompletedTutorial 플래그, 4단계: MapNavigation/BattleBasics/ShopBasics/RestBasics)
BattleSpeed (전투 속도 enum: Normal=1, Fast=2, BattleSceneSetup.ToggleBattleSpeed → Time.timeScale, TopBarUI 속도 버튼 "1x"/"2x")
ShopUI.Sell (상점 판매 탭 partial, ShopManager.SellSkill/SellItem, 구매가 50% 환불, ConfirmationDialog 재사용)
StageBonusUI (엘리트/스테이지클리어 보상 3택1 듀얼모드 — ShowEliteBonus/ShowStageClearBonus, 버튼 인덱스==enum값 매핑, UIAnimationHelper.FadeIn/FadeOut)
EliteBonusType (enum: BonusRelic/PartyUpgrade/ShopDiscount — GameRunState.ApplyEliteBonus 처리)
StageClearBonusType (enum: BurstReady/Recharge/IntelAdvantage — GameRunState.ApplyStageClearBonus 처리)
BalanceSimulator (자동 밸런스 시뮬레이터, Editor 전용 partial class 5분할, TeamLog.Editor 네임스페이스)
    ├── Quick Combat 1000팩: F1~F3 일반/엘리트/보스 9카테고리 매트릭스 + SimulatedPlayerAI 휴리스틱 (Heal/Shield/Attack/Buff/Debuff/Purify 점수 평가)
    ├── Full Run 100회: GameRunState.Create/Destroy로 매 런 격리, 맵 경로 자동 선택(위기시 Rest>Shop, 여유시 Elite>Battle), 노드별 자동 처리
    ├── Relic Synergy Test: 7카테고리 3-세트 유물 강제 지급 후 F2 전투 (일반30+보스15팩), 카테고리별 승률 비교
    ├── 리포트: Assets/09.Docs/BalanceReports/{QuickCombat,FullRun,RelicSynergy}_타임스탬프.csv + 콘솔 요약
    └── 안전장치: MAX_TURNS=50 무한루프 방지, CombatEventBus.Clear/SkillExecutor.ClearEvents 매 팩 정리, Application.isPlaying 가드
BattleTestSceneSetup (`TeamLog.EditorDebug` MonoBehaviour, 인터랙티브 전투 테스트 씬 진입점)
    ├── 씬: BattleTestScene.unity (BattleScene.unity 복제 — 모든 인스펙터 참조 보존, ConfigCanvas + BattleTestConfigPanel 추가)
    ├── 사이클: 단일 씬 자기 리로드 (설정 → 전투 → 설정). static 필드로 드롭다운 인덱스 보존
    ├── 드롭다운 14개: 파티 4 (Char_ 8종), 유물 6 (Relic_ 42종), 적 4 (Enemy_ 18종=일반12+엘리트6), 층 1 (F1~F4), 보스 토글
    ├── 전투 시작 시: BuildParty/BuildEnemies → GameRunState 생명주기 (Destroy→Create→SetDataPools→SetPlayerParty→SubscribeEvents→AcquireRelic) → BattleSceneSetup.SetBattleData + SetReturnScene("BattleTestScene") → FadeToScene 자기 리로드
    ├── _pendingTestBattle static 분기: 씬 리로드 후 BattleSceneSetup GO를 SetActive(true)하여 Awake/Start 유도
    ├── 빌더: BattleTestSceneBuilder (TeamLog/Scene/Build Battle Test Scene 메뉴, AssetDatabase.CopyAsset으로 BattleScene 복제, 에셋 배열 자동 바인딩. Phase GC 정화: partial 2파일 분할 — BattleTestSceneBuilder.cs(323줄, 진입점/오케스트레이션/바인딩/에셋로드) + BattleTestSceneBuilder.UI.cs(506줄, UI 생성 헬퍼))
    ├── BattleTestConfig (순수 C# static 헬퍼, BuildParty/BuildEnemies + FloorScaling 로컬 복제, 런타임 호환 AssetDatabase 미사용)
    ├── 템플릿 시스템: 파티/유물/적 조합 각각 독립 저장·불러오기·삭제 (BattleTestTemplateStore — JSON 파일 persistentDataPath 영속화, TemplateCategory enum 3종, 15개 SerializeField UI 바인딩)
    └── 하위 호환: BattleSceneSetup.SetReturnScene("BattleTestScene") API 추가, 아무도 호출 안 하면 기존 MapScene 복귀 100% 보존

Character (순수 C# 클래스, MonoBehaviour 아님)
    ├── HealthComponent (HP/쉴드 관리, OnHPChanged/OnShieldChanged/OnDeath + delta: OnDamageTaken/OnHealApplied/OnShieldAdded, OnPreDeath 사망 방지)
    ├── StatComponent (ATK/DEF, base + modifier + permanent base 증가)
    ├── StatusEffectComponent (14종 상태이상 관리: Taunt 추가)
    ├── SkillInventoryComponent (SkillInstance 목록, DrawSkillInstance 가중치 뽑기, 업그레이드 상태 관리)
    └── EnemyTraitHandler (적 패시브 특성 처리기: ShieldPrep/Agile/Sturdy/ArcaneFury/Corrosive/Enrage/ScaleArmor/Immortal)

SkillInstance (순수 C# 클래스, SkillData + UpgradeLevel, EffectivePower/Cost/Weight 계산 프로퍼티)

CombatEventBus (static 전투 중앙 이벤트 버스 — 13종 이벤트: OnBattleStart/End, OnTurnStart/End, OnDamageDealt/Received, OnKill, OnHealApplied, OnShieldGained, OnGoldEarned, OnSkillUsed, OnRerollUsed, OnRelicTriggered)
EnemyActionPattern (순수 C# 클래스, 상황인식 가중치 기반 스킬 선택 — 기본 가중치에 5규칙 배수 곱산: HP<30% 힐/쉴드 x3.0, HP<50% 힐/쉴드 x2.0, 약한 적 존재 공격 x2.5, 첫 턴 버프 x2.0, HP<50% 디버프 x1.5)
RelicHandler (순수 C# 클래스, GameRunState 소속, 유물 트리거 매칭 → 효과 적용, _nextAttackBonusDamage 일시적 버프 시스템, _triggerDepth 무한루프 방지(MAX 5))

AugmentOfferGenerator (순수 C# 클래스, 등급 가중치 증강 선택 + 호환성 체크 + 제안 조합, GameRunState.AugmentGenerator로 접근)

ItemEffectApplier (순수 C# static, 아이템 효과 런타임 적용)

메타프로세션 시스템 (Phase 8):
MetaProgressionManager (순수 C# static, TeamLog.Meta — 런 보상 계산 CalculateRunReward / 특성 해금 TryPurchaseTrait / 강화 구매 TryPurchaseUpgrade / 장착 바인딩 TryEquipTrait+GetEquippedTraitId / 유물 풀 필터링 FilterRelicPool+RollRelics / 시작 유물 대수 GetStartingRelicGrantCount / ExtraReroll+PartyHealBoost 강화 조회. DefaultRelicIds HashSet 16종 기본 해금 유물 관리)
AscensionManager (순수 C# static, TeamLog.Meta — 어센션 시스템 중앙 관리자. Phase ASC-A. 15레벨 매핑: GetStackCountByLevel(type, level)로 스택 수 조회, GetEnemyHpMulByLevel/GetEnemyAtkMulByLevel/GetBossHpMulByLevel/GetPlayerMaxHpMulByLevel/GetHealMulByLevel/GetStartGoldDeltaByLevel/GetRerollDeltaByLevel로 누적 값. GetActiveModifiers(level)로 활성 modifier 리스트. GetAscensionLevel(meta) + ClampSelectedLevel(selected, meta). MetaSaveData 버전 GetXxxMul(meta) 헬퍼 포함. 레벨 임계값 — EnemyHp{1,7,13}, StartGold{2,8}, Reroll{3,9,14}, PlayerHp{4,10}, Heal{5,11}, EnemyAtk{6,12}, BossHp{15}. MaxLevel=15)
AscensionModifierData (ScriptableObject, TeamLog.Meta — 어센션 modifier 정적 데이터. AscensionModifierType enum 7종: EnemyHpPercent/PlayerMaxHpPercent/RerollCount/StartGold/HealPercent/EnemyAtkPercent/BossHpPercent. 필드: modifierId/displayName/description/modifierType/intValue/floatValue. DataGenerator.Ascension.cs 10번째 partial에서 7종 자동 생성)
MetaUpgradeData (ScriptableObject, TeamLog.Meta — 일회성 메타 강화. MetaUpgradeType enum: RelicUnlock/StartingRelicSlot/StartingRelicChoice/ExtraReroll/PartyHealBoost. 30종 DataGenerator.MetaUpgrades.cs에서 생성)
CharacterTraitData (ScriptableObject, TeamLog.Characters — 캐릭터 장착형 특성 Loadout. KeywordEntry[] 기반, 8캐릭터 × 3특성 = 24종 DataGenerator.Traits.cs에서 생성. isDefault/unlockCost/soulUnlockCost 해금 정책)
CharacterTraitHandler (순수 C# 클래스, TeamLog.Characters — Character 1명당 1개 소유. Character.PlayerTraitHandler. CombatEventBus 구독 + Owner 자신에게만 효과 적용. RelicHandler(파티 전체)와 달리 개인 한정. Phase 8C에 Character.cs에 추가 — EnemyTraitHandler 회귀 제로)
MetaShopUI (MonoBehaviour, TeamLog.UI.Meta — 타이틀 화면 메타 상점 3탭: 특성/유물/강화. 잔고 표시 + 카드 동적 생성 + 구매 처리. TitleSceneSetup._metaShopUI)
CharacterTraitSelectUI (MonoBehaviour, TeamLog.UI.Map — CharacterSelectUI 이후 표시되는 캐릭터별 장착 특성 선택 패널. 파티원 각각에 대해 해금된 특성 중 1개 선택. MapSceneSetup.OnCharacterSelectConfirmed → OnTraitSelectConfirmed 파이프라인)
TraitBindingEntry ([Serializable] TeamLog.Map — CharacterName+TraitId 쌍. MetaSaveData.EquippedTraitBindings List로 저장)
```

### 턴 사이클
`Draw → PlayerAction(AP 관리) → Execution → EnemyTurn → BattleEnd`

**적 턴 실행 모드**:
- **순차 모드 (런타임, 기본)**: BattleSceneSetup이 `EnableSequentialEnemyTurn()` 호출 → StartEnemyTurn이 `OnEnemyTurnSequenceStarted` 이벤트 발행 → 코루틴이 적 한 명씩 `ExecuteSingleEnemyAction()` 실행 (하이라이트 → 행동 → 의도 클리어 → VFX 대기 → 전멸 체크) → `CompleteEnemyTurn()`. WaitForSeconds는 Time.timeScale 영향으로 1x/2x 자동 적용
- **동기 모드 (시뮬레이터)**: `EnableSequentialEnemyTurn()` 미호출 → StartEnemyTurn이 `ExecuteEnemyActions()` 동기 실행 (모든 적 같은 프레임). BalanceSimulator는 수정 없이 동기 모드 유지

### VFX/임팩트 시스템
스킬 사용 시 시각 효과는 두 경로로 분기:
- **Health 이벤트 기반** (플레이어/적 공통): OnDamageTaken → PlayDamageVFX (Hit 또는 Critical + CameraShake + 화면 플래시 + 히트스톱), OnHealApplied → Heal VFX, OnShieldAdded → Shield VFX, OnDeath → Death VFX
- **OnSkillApplied 기반** (플레이어 스킬 전용): 스킬 타입별 VFX — Attack(속성별 Burn/Poison/Freeze/Slash), Buff, Debuff, Purify. 사운드 분기와 동일 구조
- **크리티컬 히트**: 데미지 ≥ 최대 HP의 35% 시 Critical VFX + 강한 흔들림(12px) + 백색 화면 플래시 + 0.04초 히트스톱(Time.timeScale)
- **상태이상 적용**: Stun 시 StunStarExplosion VFX, OnEffectApplied 이벤트에서 처리

### 자원 시스템
- **AP (Action Point)**: 파티 공유 자원, 매 턴 시작 시 `1 + 생존 파티원 수` 전량 회복
- **스킬 Cost**: 0~3 (SkillData.Cost), 사용 시 AP 차감, 부족 시 스킬 사용 불가
- **쉴드 (Shield)**: 일시적 보호막, HP 바 위 갈색 바로 표시, 데미지를 HP보다 먼저 흡수, 턴 시작 시 리셋
- **리롤 (Reroll)**: 턴당 2회, 개별 슬롯 리롤만 지원, 이미 사용한 슬롯은 리롤 불가
- **드로우 가중치**: 모든 플레이어 스킬 weight=25 균등 (SkillDrawSystem 가중치 랜덤)
- **적 행동 가중치**: EnemyActionPattern — 기본 가중치(EnemyPatternTable weight)에 상황 배수(5규칙)를 곱해 매 턴 동적 선택. 의도는 공개되지만 행동은 매번 달라짐
- 적은 AP/리롤 시스템에서 제외 (EnemyAIController가 독립적으로 행동 결정)

### 메타 재화 (Phase 8)
- **기억의 조각 (MemoryFragments)**: 일반 메타 재화. 패배/승리 모두 획득. 공식: `floor*5(패배)/floor*10(승리) + battlesWon + (승리 시 +50) + gold/100`
- **영혼 (Souls)**: 희귀 메타 재화. 승리 시만 획득. 공식: `1 + floor/2` (F1=1, F4=3)
- **기본 해금 유물 16종**: Phase 5C 원본 유물 (DefaultRelicIds HashSet)
- **메타 해금 유물 26종**: Phase 6A 시너지 유물 — RelicUnlock 강화 구매 필요
- **캐릭터 특성**: 캐릭터당 3개 (1 기본 무료 + 2 메타 해금). 런 시작 시 1개 장착 선택

### 데이터 계층
- **CharacterData** (ScriptableObject): 이름, 클래스, 기본 스탯, 스킬 목록, 적 특성(EnemyTrait)
- **ItemData** (ScriptableObject): 이름, 효과타입, 값, 아이콘(Sprite, DataGenerator에서 ItemEffectType 기반 자동 할당)
- **SkillData** (ScriptableObject): 이름, 타입(Attack/Heal/Buff/Debuff/Shield/Purify), 타겟타입, 위력, 비용, 가중치(플레이어 스킬 모두 25 균등), 상태이상, 아이콘(Sprite, DataGenerator에서 SkillType+StatusEffect 기반 자동 할당)
- **EnemyPatternData** (ScriptableObject): enemyId, 스킬 목록, 스킬별 기본 가중치(_weights), EnemyPatternTable.csv(enemyId,order,skillId,weight)에서 생성
- 모든 데이터는 `Assets/03.Data/`에 `TeamLog/` 메뉴로 생성
- **DataGenerator 규칙**:
  - `GetOrCreateAsset<T>`로 기존 에셋 로드 우선 (GUID 보존, 참조 끊김 방지)
  - `Object.name = fileName` (에셋 파일명과 일치, Unity 경고 방지)
  - 한국어 표시명은 `_skillName`/`_characterName` 등 private 필드에 저장
  - 스킬 Cost 포함하여 모든 필드를 명시적으로 설정
  - 파일 구조: DataGenerator.cs (진입점+스킬/캐릭터/유틸), DataGenerator.Augments.cs (증강+스폰패턴), DataGenerator.Events.cs (이벤트), DataGenerator.Relics.cs (유물), DataGenerator.Palettes.cs (UI/오디오/VFX 팔레트), DataGenerator.Stages.cs (스테이지 테마)
- **MapSceneBuilder 규칙**:
  - 에셋 필터링은 `Object.name`이 아닌 파일 경로 기반 (`namePrefix` 파라미터)
  - 스테이지 테마 후보: `StageThemeCandidateList[]`로 4스테이지 × 3후보 관리 (Phase 7D: 12테마 정식 분화)
  - 테마 데이터는 StageThemeData SO에 적/엘리트/보스/스폰테이블 통합 (FloorEnemyPool은 레거시 호환용)
  - 테마 배정 (Phase 7D 정식): Stage1=GreyForest/FrostedPass/SunscorchedPlains, Stage2=CrimsonChapel/RotbloomBog/RuinedTemple, Stage3=AbyssalTrench/Stormpeak/ShadowsGlade, Stage4=EmberThrone/EternalTundra/DemonCitadel
  - 적 풀 재조합 전략: 기존 F1/F2/F3 적 에셋을 테마별로 재조합 (신규 적 에셋 생성 없이 차별화), F4는 F3 풀 + GetFloorScaling(2.0f)으로 자동 강화
  - 엘리트 보상 (Phase 7B): EliteBonusType 3택1 (BonusRelic / PartyUpgrade / ShopDiscount+100G)
  - 스테이지 클리어 보상 (Phase 7C): StageClearBonusType 3택1 (BurstReady AP+2 / Recharge HP50% / IntelAdvantage 상점 진열 추가)

## 코딩 규칙

### 필수
- **네임스페이스**: `TeamLog` 최상위, 하위는 폴더 구조 따름
  - `TeamLog.Characters` — Character, CharacterData, Components, SkillData, CharacterTraitData, CharacterTraitHandler
  - `TeamLog.Combat.Turn` — TurnManager, TurnPhase, TurnContext, SkillExecutor
  - `TeamLog.Combat.Draw` — SkillDrawSystem (SkillInstance 기반)
  - `TeamLog.Combat` — CombatEventBus (static 전투 이벤트 버스), DamageCalculator (static 데미지 계산)
  - `TeamLog.Combat.AI` — EnemyAIController, EnemyActionPattern
  - `TeamLog.Map` — MapNode, MapFloor, MapGenerator, GameRunState
  - `TeamLog.Meta` — MetaProgressionManager (런 보상/해금/필터링 순수 C# static), MetaUpgradeData
  - `TeamLog.Reward` — RewardData, RewardManager, AugmentOfferGenerator
  - `TeamLog.Shop` — ShopData, ShopManager
  - `TeamLog.Event` — EventData, EventManager
  - `TeamLog.UI.Battle` — 전투 UI 클래스
  - `TeamLog.UI.Map` — 맵 UI 클래스, CharacterTraitSelectUI
  - `TeamLog.UI.Title` — 타이틀 UI 클래스
  - `TeamLog.UI.Meta` — MetaShopUI (메타 상점 3탭)
  - `TeamLog.UI.Reward` — 보상 UI 클래스
  - `TeamLog.UI.Shop` — 상점 UI 클래스
  - `TeamLog.UI.Event` — 이벤트 UI 클래스
  - `TeamLog.Editor` — 에디터 도구
  - `TeamLog.EditorDebug` — SRDebugger 인게임 디버그 옵션 (TeamLog.Debug 사용 금지: UnityEngine.Debug와 충돌)
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

DataGenerator/
  DataGenerator.cs           — 진입점 + 상수 + 스킬/캐릭터/패턴 CSV 생성 + 유틸리티
  DataGenerator.Augments.cs  — 증강 데이터 + 스폰 패턴 테이블
  DataGenerator.Events.cs    — 이벤트 데이터 생성
  DataGenerator.Relics.cs    — 유물 데이터 생성
  DataGenerator.Palettes.cs  — UI/오디오/VFX 팔레트 생성

BalanceSimulator (자동 밸런스 시뮬레이터, `#if UNITY_EDITOR`):
  BalanceSimulator.cs          — 진입점 + 상수 + 메뉴 + 에셋 로드 + Configuration
  BalanceSimulator.Combat.cs   — SimulatedPlayerAI (private 중첩 클래스) + Quick Combat 1000팩 매트릭스
  BalanceSimulator.Run.cs      — Full Run 100회 + 맵 노드 자동 결정 (Battle/Elite/Shop/Event/Rest)
  BalanceSimulator.Report.cs   — ReportUtils 중첩 클래스 + 통계 집계 + CSV 출력 + 콘솔 요약
  BalanceSimulator.Synergy.cs  — 유물 3-세트 시너지 테스트 (7카테고리 × F2 일반30+보스15팩)
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

### 4. DOTween 확장 메서드 + asmdef 경계

**문제**: DOTween의 UI 확장 메서드(`CanvasGroup.DOFade()`, `Image.DOColor()`, `Transform.DOScale()` 등)는 `DOTweenModuleUI.cs` 소스 파일에 정의되어 있고, 이 파일은 asmdef가 없어 글로벌 어셈블리에 컴파일됨. `TeamLog.Runtime`(asmdef)에서는 글로벌 어셈블리의 소스 코드 타입을 볼 수 없어 컴파일 에러 발생.

**해결**: `DOTween.To()` (코어 DLL, 확장 메서드 아님)만 사용. getter/setter 람다로 수동 트윈 구현.

```csharp
// BAD — asmdef 경계에서 보이지 않음
cg.DOFade(0f, 0.3f);
img.DOColor(Color.red, 0.15f);

// GOOD — DOTween.To() 직접 사용
DOTween.To(() => cg.alpha, x => cg.alpha = x, 0f, 0.3f);
```

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
  - 4I: 버그 수정 2건 + 자동화 테스트 인프라 완료 (BattleEndOverlay/RewardUI 버그, 단위 테스트 인프라, 어셈블리 분리)
  - 4K: UI 종합 개선 완료 (UIPalette 토큰, HP 트윈/플래시/페이드 애니메이션, 색맹 이니셜, 툴팁 시스템, 로그 색상 코딩+스크롤, GUI 에셋 스프라이트, 베지에 곡선 연결선, 파티 상태 위젯, 사운드 시스템)
  - 4L: 사운드 시스템 완료 (42개 SFX 자동 할당 — CombatMagicSpellsVIISFX, 스킬 타입별 사운드 분기, SkillExecutor.OnSkillApplied 이벤트)
  - 4M: 에셋 활용 5단계 완료 (스킬 아이콘 ✅, DOTween 전환 ✅, SRDebugger ✅, CameraShake ✅, VFXManager URP Camera Stacking ✅)
  - 4N: 아이템 아이콘 시스템 완료 (ItemData._icon, DataGenerator.GetItemIconPath(), ShopItemSlot/RewardCard 아이콘 표시)
  - 4O: 밸런싱 패스 완료 (플레이어 HP 증가, 보스 쉴드 너프, 적 스탯 층별 스케일링 개선)
  - 4P: Motion Titles Pack 통합 완료 (BattleTitleManager, 전투 시작/승리/패배 타이틀, MotionTitlesPack.Runtime.asmdef)
  - 4Q: 저장 시스템 기반 완료 (SaveManager — JsonUtility + 파일 I/O, RunSaveData/CharacterSaveData DTO, 파티/아이템/스킬 복원)
- **Phase 5 (메타프로그레션 + 심화)**: 완료
  - 5A: 타이틀 + 런 라이프사이클 완료 (TitleScene/TitleSceneSetup, MetaSaveData 영구 통계, RunEndOverlay 승리/패배 화면, SaveManager.HasSave 이어하기, 자동 저장)
  - 5B: 스킬 성장 시스템 완료 (SkillInstance 래퍼 + 업그레이드 레벨, SkillDrawSystem/EfficientCost/Power, SkillUpgradeUI 휴식지 강화 선택지, 저장/로드 업그레이드 레벨 유지)
  - 5C: 유물 시스템 완료 (CombatEventBus 전투 중앙 이벤트 버스, RelicData SO 트리거+효과, RelicHandler 구독/적용, 12종 유물 DataGenerator 생성, DamageCalculator DealDamage/OnKill 이벤트 발행)
  - 5D: 폴리싱 완료 (ItemEffectApplier HealPerTurn/BonusGold/DrawWeight 처리, EventData 저주/상태이상 필드, EventManager 상태이상 적용)
  - 5E: 전투 밸런스 재설계 완료 (드로우 가중치 25 통일, 적 AI 상황인식 가중치 시스템 5규칙, 적 스킬 위력 +20%, 스폰 패턴 6건 +1마리, 보스 HP +15%)
  - 5F: 자동 밸런스 시뮬레이터 완료 (BalanceSimulator 4파일 분할 — Quick Combat 1000팩 + Full Run 100회, SimulatedPlayerAI 휴리스틱, CSV 리포트 + 콘솔 요약, MAX_TURNS 무한루프 방지, CombatEventBus 매 팩 정리)
  - **6A**: 유물 시너지 시스템 (신규 26종 유물 — 총 42종, 9카테고리 시너지 설계, 신규 키워드/트리거: OnEnemyLowHP/OnRerollUsed, _nextAttackBonusDamage 일시적 버프, 트리거 체인 시스템, RelicSynergy 테스트 모드, F2 보스 HP 184→145 조정)
- **Phase 7 (스테이지 시스템 재설계)**: 완료
  - 7A: 4스테이지 인프라 완료 (StageThemeData SO 신규 정의, GameRunState.TotalFloors 3→4, SelectedThemes 리스트 + 무작위 채택 로직, MapGenerator BranchingLayers={2,4} Battle/Elite 노드 쌍 생성, FloorConfigs 4스테이지 6레이어 표준화, MapSceneSetup _stageThemeCandidates 필드 교체, SaveManager SelectedThemePaths 저장/로드)
  - 7B: 엘리트 분기 보상 완료 (EliteBonusType 3택1 — BonusRelic/PartyUpgrade/ShopDiscount+100G, GameRunState.ApplyEliteBonus, StageBonusUI.ShowEliteBonus, PendingShopDiscount 생명주기: Peek→Save→Restore→Consume)
  - 7C: 스테이지 클리어 보상 완료 (StageClearBonusType 3택1 — BurstReady AP+2/Recharge HP50%/IntelAdvantage 진열추가, GameRunState.ApplyStageClearBonus, StageBonusUI.ShowStageClearBonus, ShopManager.GenerateShopSlots extraAugments/extraRelics/discount 파라미터 확장)
  - 7D: 테마 콘텐츠 확충 완료 (DataGenerator.Stages.cs 12테마 분화 — 4스테이지 × 3테마, 기존 적 에셋 재조합으로 차별화, 테마 키워드/설명 StageDesign.md 반영)
  - 7E: BalanceSimulator 4스테이지 대응 완료 (FloorBossIds 4개 확장, _spawnTables F4 F3 폴백, Quick Combat 매트릭스 F4 추가 + 1000팩 유지, Report.cs 카테고리/사망분포/도달률 F4 포함)
  - **7F**: 유물 검증 + 임계 버그 2건 수정 (단위 테스트 12개 추가 — 총 73개, OnKill 트리거 키워드 집계 누락 수정: VampireFang/SlayerSigil 작동, Full Run 시뮬레이터 RelicHandler 재구독 수정: 0%→1% 클리어율, F2 도달 20% 신규)
  - **7G**: 인터랙티브 전투 테스트 씬 완료 (BattleTestScene.unity — 씬 복제 전략, 자기 리로드 사이클, SetReturnScene API 하위 호환 보존, 드롭다운 14개로 파티/유물/적/층/보스 세팅, 런타임 BattleSceneSetup 그대로 재사용)
  - **잔여**: 런타임 엔드투엔드 런 클리어 검증 (F1→F2→F3→F4, 분기/엘리트/스테이지클리어 보상 정상 동작), BattleTestScene Play 모드 7가지 시나리오 검증
- **Phase 8 (메타프로세션 + 캐릭터 특성 Loadout)**: 완료
  - 8A: 데이터 계층 + DataGenerator 완료 (CharacterTraitData SO 신규 — 8 캐릭터 × 3 특성 = 24종, MetaUpgradeData SO 신규 — RelicUnlock 26 + 글로벌 4 = 30종, DataGenerator.Traits.cs/DataGenerator.MetaUpgrades.cs 8번째 9번째 partial 분할)
  - 8B: 메타 재화 + 저장 확장 완료 (MetaSaveData 필드 추가 — MemoryFragments/Souls/UnlockedTraitIds/UnlockedRelicIds/PurchasedUpgradeIds/EquippedTraitBindings+TraitBindingEntry, MetaProgressionManager 신규 — CalculateRunReward/TryPurchaseTrait/TryPurchaseUpgrade/TryEquipTrait, SaveManager.RecordRunEnd 시그니처 확장 + 메타 재화 적립, RunEndOverlay.Show 메모리/영혼 표시, TitleSceneSetup 잔고 표시)
  - 8C: 캐릭터 특성 런타임 적용 완료 (CharacterTraitHandler 신규 — Character 1명당 1개 소유, CombatEventBus 구독 Owner 한정 적용, Character.PlayerTraitHandler 프로퍼티 + EquipTrait 메서드 — EnemyTraitHandler 회귀 제로, SkillExecutor.GetAllKeywordSum/Mul + ExecuteAttack + ApplyEffect(caster) 시그니처 확장으로 특성 키워드 집계, DamageCalculator.DealDamage attacker/target 양쪽 훅, TurnManager StartBattle/StartNewTurn/ExecuteSkillImmediately/CheckBattleEnd 4곳 훅)
  - 8D: 특성 선택 UI + 메타 상점 UI 완료 (CharacterTraitSelectUI 신규 — CharacterSelectUI 이후 캐릭터별 장착 특성 1개 선택, MetaShopUI 신규 — 타이틀 3탭 구조 특성/유물/강화, MapSceneSetup.OnCharacterSelectConfirmed → OnTraitSelectConfirmed 파이프라인, TitleSceneSetup 메타 상점 버튼 + _allCharacters 동적 계산, MapSceneBuilder.Panels.cs BuildCharacterTraitSelectPanel/BuildMetaShopPanel 신규, MapSceneBuilder.Helpers.cs WireMetaShopDataPools 헬퍼)
  - 8E: 유물 해금 풀 + 시작 유물 지급 완료 (MetaProgressionManager.FilterRelicPool/RollRelics/GetStartingRelicGrantCount/GetExtraRerollCount/GetPartyHealBoost 추가, DefaultRelicIds HashSet 16종 기본 해금, MapSceneSetup.StartRunWithParty 필터링 + ApplyStartingRelics, BattleSceneSetup maxRerolls 메타 강화 반영, MapSceneSetup.Nodes.cs RestAtCampfire PartyHealBoost 가산)
  - 8F: 회귀 버그 수정 + 단위 테스트 + 검증 완료 (SaveManager.CheckCharacterUnlocks Phase 7A 회귀 수정 — victory && floor >= N 기반, TitleSceneSetup.RefreshUI 하드코딩 4 → _allCharacters 동적 계산, MetaProgressionTests 10개 + CharacterTraitHandlerTests 5개 신규 — 총 88/88 통과, TitleScene/MapScene 리빌드 + 전 필드 와이어링 검증 완료)
- **Phase E (이벤트 퀄리티 향상)**: 완료
  - E1: 데이터 구조 확장 완료 (EventData/EventManager/EventUI 개편 — EventOutcome에 PermanentAtk/Def/RerollTokens/RandomOutcomes/NextEventId 추가, EventChoice에 MinGold/HP/AliveMembers 조건부, EventRiskLevel enum + GetRiskLevel 자동 분류. EventManager 영구 강화 처리 + 확률 Outcome 추첨 + ResultText 오염 방지(Clone 반환) + CanChoose 헬퍼. EventUI ChoiceDescription 표시 + 위험도 색상 코딩 + 조건부 비활성화 + 연쇄 이벤트)
  - E2: 공통 이벤트 15개 신규 완료 (도박 4 / 저주 3 / 영구 강화 3 / Story 3 / 조건부 2 — 기존 10개 포함 총 25개 공통 이벤트)
  - E3: 스테이지 테마별 전용 이벤트 24개 완료 (12테마 × 2개, StageThemeData.themeEvents 필드 추가, ExclusiveThemeId로 해당 테마에서만 등장. DataGenerator.Stages.cs GenerateThemeSpecificEvents + CreateTheme themeEventIds 파라미터)
  - E4: 맵 노드 처리 개선 완료 (MapSceneSetup.Nodes.cs PickRandomEvent 헬퍼 — 테마 풀 70% / 공통 풀 30% 가중치, 폴백 체인)
  - 테스트: EventManagerTests 12개 신규 (영구 강화/확률 Outcome/ResultText 복사본/가중치 분포/조건부 CanChoose/골드·HP·상태이상 회귀) — 총 100/100 통과
- **Phase ASC (어센션 + 보스 12종 확장)**: 코드/데이터 완료, 런타임 검증 잔여
  - ASC-A: 어센션 시스템 데이터+로직 완료 (AscensionModifierData SO + AscensionModifierType enum 7종 + BossHpPercent, AscensionManager 순수 C# static — GetStackCountByLevel/GetXxxByLevel/GetActiveModifiers, 15레벨 매핑: 1=EnemyHp+5% / 2=StartGold-10 / 3=Reroll-1 / 4=PlayerHp-5% / 5=Heal-10% / 6=EnemyAtk+5% / 7-12=동일 modifier 2스택 / 13=EnemyHp 3스택 / 14=Reroll 3스택 / 15=BossHp+20%. DataGenerator.Ascension.cs 10번째 partial — modifier 7종 자동 생성. MetaSaveData.AscensionLevel(달성)+SelectedAscensionLevel(선택) 필드 추가. SaveManager.RecordRunEnd 승리 시 +1(최대 15)+LoadOrCreateMeta 호환 마이그레이션. GameRunState.SelectedAscensionLevel 캐시. BattleSceneSetup.SetBattleData isBossBattle 매개변수 추가 + ApplyAscensionModifiers(적 HP/ATK/BossHp 스케일링, 리롤 delta). MapSceneSetup.StartRunWithParty 어센션 적용(시작 골드/MaxHP), Nodes.cs RestAtCampfire heal mul, StartBattle 보스 노드 isBossBattle=true. TitleSceneSetup 어센션 표시+선택 버튼. RunEndOverlay.Show ascensionNote 매개변수)
  - ASC-B: 보스 12종 완전 교체 완료 (기존 보스 3종 제거 — GoblinKing/Dragon/DemonLord. 신규 12종 테마별 전용 보스 — Stage1: VerdantTerror/FrostMonarch/SandLeviathan, Stage2: BloodQueen/PlagueLord/LichKing, Stage3: Kraken/StormLord/VoidWalker, Stage4: FlameEmperor/IceGoddess/Archdemon. HP 130~320, ATK/DEF 층별 부여. 보스별 4스킬 × 12 = 48종 신규 스킬. CharacterTable/SkillTable/EnemyPatternTable CSV 교체. DataGenerator.Stages.cs 12 테마에 각 보스 연결. BalanceSimulator FloorBossIds 4종 대표 보스로 교체 + FloorBossCandidates 12종 후보 배열)
  - 테스트: AscensionManagerTests 25개 신규 (스택 카운트/누적 값/활성 modifier/MetaSaveData 기반) — 총 130/130 통과. 0에러.

### 미구현 항목
- VFXManager 런타임 시각 검증 (URP Camera Stacking 코드 완료, VFXPalette.asset에 15개 프리팹 할당, 스킬 타입별 VFX 분기+크리티컬 임팩트 연결 완료, 실제 파티클 표시 확인 필요)
- 전투 밸런스 튜닝 (Quick Combat/Full Run 결과 기반 — 2026-06-19 조치 전: F1 사망 80%, F2 도달 20%, F3/F4 도달 2%, Full Run 1% 클리어율. **2026-06-19 조치 후(시뮬레이터 캐릭터 특성 반영 + F1 적 HP -12~14%)**: 클리어율 1%→**9%** (9배), F2 도달 20%→**72%** (+52%p), F3 도달 2%→**19%**, F4 도달 2%→**11%**. F2 보스 승률 20%→58.7%, F3 일반 40%→88%. 잔존 과제: F3 보스 9.3%, F4 보스 1.3% — 보스 자체 HP/위력 너프 필요. **Phase ASC-B 보스 12종 교체 후에는 재측정 필요** — BalanceSimulator FloorBossIds가 신규 4종 대표 보스(FrostMonarch/PlagueLord/Kraken/Archdemon)로 교체됨. 상세: `Assets/09.Docs/BalanceReports/Balance_Diagnostic_Report_2026-06-19.md`)
- BattleTestScene 런타임 검증 (빌드 완료 — Play 모드 7가지 시나리오로 사용자 직접 검증 필요: 기본 전투/유물 효과/보스/재테스트/세팅 변경/층 스케일/일반 플레이 회귀)
- Phase E 이벤트 런타임 검증 (Play 모드 — 테마별 이벤트 등장 / 확률 이벤트 결과 분포 / 영구 강화 적용 / ChoiceDescription 표시 / 위험도 색상 / 조건부 비활성화 / 연쇄 이벤트 NextEventId)
- Phase E 밸런스 시뮬레이터 연계 (BalanceSimulator에 이벤트 효과 반영 — 현재 시뮬레이터는 RunSingleRunWithState에서 이벤트를 Choices[0] 고정 선택. 영구 강화/저주/확률이 승률에 미치는 영향 측정 필요)
- **Phase ASC 데이터 생성 (사용자 실행 필요)** — `TeamLog/Generate Test Data` (보스 12종 + 스킬 48종 + 패턴 12종 생성), `TeamLog/Generate Ascension Data` (modifier 7종 생성), `TeamLog/Generate Stage Themes` (12 테마에 신규 보스 연결). CSV는 이미 교체됨 → 메뉴 실행 시 .asset 파일 자동 생성
- **Phase ASC 어센션 런타임 검증 (Play 모드)** — 타이틀 어센션 표시/선택 UI(버튼 SerializeField 연결 필요), 런 시작 시 modifier 적용(리롤 감소/MaxHP 감소/시작 골드 감소 확인), F4 보스 클리어 시 어센션 +1 상승 확인, 어센션 15에서 보스 HP +20% 확인
- **Phase ASC 보스 12종 런타임 검증 (Play 모드)** — 12 테마 각각 보스 다르게 등장 확인, 보스 스킬 4종 정상 작동, 보스 trait(Regenerate/Sturdy/PhaseShift/Immortal/Corrosive/ArcaneFury/Counter/Rampage) 정상 적용
- **가비지 컬렉션 대상** — 기존 보스 3종 에셋(orphan): `Enemy_BossGoblinKing.asset` / `Enemy_BossDragon.asset` / `Enemy_BossDemonLord.asset` + 관련 스킬 12종 + 패턴 3종. CSV에서 제거됨 → DataGenerator 재실행 시 orphan. 사용자 승인 후 삭제 권장
- **Phase CC (캐릭터 컨셉 개편 — 조건부 메카닉)**: 기획 설계 문서 완료 `Assets/09.Docs/CharacterConceptReview.md`. 10캐릭터(Warrior/Pyromancer/Cryomancer/Stormcaller/Healer/Rogue/Archer/Necromancer/Alchemist/Bard) 고유 메카닉 + 조건부 스킬 리워크 + 특성 30종 조건부화 + 신규 유물 10종. **★스킬 4개 설계 원칙** (단일 조건/만능 금지/서로 다른 조건/셋업-소비 분해) 확립. **★부활 시스템 정책** 확정 — 전투 종료 시 살아남은 자 HP 100%, 사망자 50% 부활 + MaxHP 영구 -10% 누적. **★Mage → 3원소 마법사 분할** (8→10 캐릭터, Mage 제거 + Pyromancer/Cryomancer/Stormcaller 신규). Warrior Bodyblock 재설계 (ForcedTarget 단일 도발 부여). SkillData에 SkillConditionType/ConditionalBonusType 필드 + StatusEffectType.ForcedTarget 신규 항목 확장 필요. 구현 로드맵 CC-0(부활 시스템 선행) → CC-1 → CC-1B(마법사 2종 추가) → CC-2 → CC-3 → CC-4. StS/DD/ItB 벤치마크. 상세: `Assets/09.Docs/WorkLog/2026-06-20.md`

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
- **에셋**: GUI Pro-CasualGame (Layer Lab), Epic Toon FX, SRDebugger, DOTween, CombatMagicSpellsVIISFX, Motion Titles Pack
