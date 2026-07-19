# Team Log — UI 작업 베스트 프랙티스 (★ 다음 UI 작업 필수 참조)

> **작성일**: 2026-07-18
> **목적**: Party Selection Scene UI 작업에서 학습한 노하우를 체계화하여, 다음 UI 작업에서 동일한 실수를 반복하지 않도록 장치 제공.
> **활용**: 새 UI 컴포넌트/씬 작성 전 반드시 이 문서의 체크리스트를 점검. `UIAutoBindHelper.cs` 헬퍼 적극 활용.

---

## 0. 작업 플로우 (★ 필수)

1. **기존 코드 읽고 패턴 파악** — `UI/PartySelection/` 폴더의 컴포넌트들을 참조.
2. **UIAutoBindHelper 활용** — 새 컴포넌트는 헬퍼의 `EnsureButton`/`AutoBindField`/`DisableChildRaycastsExcept` 사용.
3. **SceneBuilder 패턴 준수** — `PartySelectionSceneBuilder` 구조 참조. CreateSectionSlot + ControllerSetup 조합.
4. **체크리스트 점검** — 작업 완료 후 하단의 "UI 작업 완료 체크리스트" 실행.
5. **가비지 컬렉터** — 디버그 로그, 미사용 메서드 정리.

---

## 1. ★★ MonoBehaviour는 한 파일에 하나만 (가장 중요)

**문제**: 한 .cs 파일에 여러 MonoBehaviour 클래스를 정의하면 Unity 직렬화 시 컴포넌트 참조가 깨짐. "Missing Script" warning 발생. `Instantiate` 시 깨진 컴포넌트가 복사되어 런타임 에러.

**사례**: PartySlotItem이 PartySlotPanel.cs 안에 정의되어 있어 씬 저장 후 로드 시 참조 깨짐. 한나절 디버깅.

**규칙**:
```csharp
// ❌ 절대 금지
public class PartySlotPanel : MonoBehaviour { ... }
public class PartySlotItem : MonoBehaviour { ... }  // 같은 파일

// ✓ 올바름
// PartySlotPanel.cs
public class PartySlotPanel : MonoBehaviour { ... }
// PartySlotItem.cs (별도 파일)
public class PartySlotItem : MonoBehaviour { ... }
```

**검증**: 콘솔에 `The referenced script on this Behaviour is missing!` 경고가 뜨면 즉시 파일 분리 점검.

---

## 2. ★★ LayoutGroup 설정 표준

LayoutGroup은 3가지 설정의 조합으로 동작. 잘못되면 자식이 0 크기로 붕괴.

### HorizontalLayoutGroup / VerticalLayoutGroup 기본값
```csharp
childControlWidth = true;      // 자식 LayoutElement.preferredWidth/flexibleWidth 존중
childControlHeight = true;     // 동일
childForceExpandWidth = false; // 강제 늘어남 금지 (자식이 자기 크기 유지)
childForceExpandHeight = false;
childScaleWidth = false;
childScaleHeight = false;
```

### 자식 GameObject는 반드시 LayoutElement 명시
```csharp
// 빈 부모 GameObject는 LayoutElement 없으면 0 크기로 붕괴
var le = go.AddComponent<LayoutElement>();
le.preferredWidth = 320;  // 명시적 크기
le.flexibleWidth = 1;     // 또는 남은 공간 차지
le.minWidth = 100;        // 최소 보장
```

### Image의 자동 preferredSize 주의
Image는 ILayoutElement를 구현하여 Sprite 크기 기반으로 preferredWidth를 자동 보고. 이걸 override하려면:
- LayoutElement.preferredWidth 명시 + 부모 LayoutGroup의 `childControlWidth=true` 필수.

### CreateLayoutChild 헬퍼 활용
```csharp
// UIAutoBindHelper 또는 SceneBuilder의 CreateLayoutChild 사용
var child = CreateLayoutChild("Name", parent, prefW: 320, prefH: 50, flexW: 1);
```

---

## 3. ★ 인스펙터 바인딩은 불안정 — 런타임 자동 보완 필수

SceneBuilder의 `BindField`/`FindDescendant`가 씬 저장/로드 사이클에서 실패할 수 있음 (직렬화 타이밍 이슈).

### AutoBindMissingFields 패턴
모든 UI 컨트롤러의 `Awake()`에서 인스펙터 필드가 null이면 자동 검색:
```csharp
private void Awake()
{
    LoadData();
    AutoBindMissingFields();  // ← 필수
}

private void AutoBindMissingFields()
{
    var root = transform.root;
    if (_someField == null)
        _someField = GetComponentInChildren<SomeType>(true);
    if (_someField == null)
    {
        var go = UIAutoBindHelper.FindDescendantByName(root, "SomeGameObject");
        if (go != null)
        {
            _someField = go.GetComponent<SomeType>();
            if (_someField == null) _someField = go.AddComponent<SomeType>();
        }
    }
}
```

### UIAutoBindHelper 활용 (권장)
```csharp
// 헬퍼 메서드로 간결하게
UIAutoBindHelper.EnsureButton(this, ref _button, _targetGraphic);
UIAutoBindHelper.AutoBindField(this, "_portraitBig", root, "PortraitFrame");
UIAutoBindHelper.DisableChildRaycastsExcept(transform, _clickableGraphic);
```

---

## 4. ★ 자식 Image raycastTarget 가로채기 방지

부모 Button의 클릭을 자식 Image가 가로채면 버튼이 안 눌림. 특히 전체 영역을 덮는 오버레이(ActiveRing/LockOverlay/InPartyBadge)가 위험.

### Initialize에서 강제 설정
```csharp
public void Initialize(...)
{
    // ...
    UIAutoBindHelper.DisableChildRaycastsExcept(transform, _portraitImage);
}
```

### SceneBuilder에서 raycastTarget 명시
```csharp
// 클릭 감지용 Image만 raycastTarget=true, 나머지는 false
img.raycastTarget = false;  // 기본값
// Button.targetGraphic으로 쓸 Image만 true
buttonTargetGraphic.raycastTarget = true;
```

---

## 5. ★ GameObject 이름 충돌 주의

`FindDescendantByName`이 재귀 검색으로 첫 번째 발견한 것을 반환. "Content" 같은 범용 이름이 여러 군데 있으면 잘못된 참조.

### 이름 규칙
- 범용 이름 대신 컨텍스트 포함: `Content` → `CarouselContent` / `BadgeContent` / `PlateContent` / `TraitContent`
- 접두사로 영역 구분: `Btn` (버튼), `Label` (텍스트), `Bg` (배경)

### SceneBuilder에서 명명
```csharp
// ❌ 위험
var contentGo = new GameObject("Content", ...);

// ✓ 안전
var contentGo = new GameObject("CarouselContent", ...);
```

---

## 6. 디버그 로그 전략

### 개발 중 (적극적 로그)
- 각 단계별 진입/종료 로그
- null 체크 결과
- 메서드 호출 체인 추적

### 완료 후 (제거)
- `Debug.Log` 제거
- `Debug.LogWarning`/`LogError`는 진단 가치 있으면 유지
- `[Conditional("UNITY_EDITOR")]` 래핑으로 에디터 전용 로그 (선택)

### 가비지 컬렉터 정기 실행
- Phase 전환 시 디버그 로그 정리
- 미사용 메서드/필드 제거
- 컴파일 warning 해결

---

## 7. SceneBuilder 패턴 (코드로 씬 생성)

### 구조
```
SceneBuilder.cs          — 진입점 + 메뉴 + 씬 생성 + Canvas + 헬퍼
SceneBuilder.Parts.cs    — 각 영역(Header/Stage/Carousel/Footer) 빌드
```

### CreateSectionSlot 헬퍼
MainFrame의 VerticalLayoutGroup 안에서 각 섹션(Header/Stage/Carousel/Footer)을 LayoutElement와 함께 생성:
```csharp
private static RectTransform CreateSectionSlot(string name, Transform parent, float height, float flexibleHeight = 0)
{
    var go = new GameObject(name, typeof(RectTransform), typeof(Image));
    go.transform.SetParent(parent, false);
    StretchToParent(go.GetComponent<RectTransform>());
    var le = go.AddComponent<LayoutElement>();
    le.preferredHeight = height;
    le.flexibleHeight = flexibleHeight;
    le.flexibleWidth = 1;
    le.minWidth = 1000;
    return go.GetComponent<RectTransform>();
}
```

### ControllerSetup — 컨트롤러 필드 자동 바인딩
씬 생성 마지막에 `FindDescendant`로 모든 UI 참조를 컨트롤러에 할당:
```csharp
private static void ControllerSetup(Canvas canvas, Controller controller)
{
    var root = canvas.transform;
    BindField(controller, "_portraitBig", FindDescendant(root, "PortraitFrame")?.GetComponent<PortraitBig>());
    // ... 각 필드
}
```

### LoadAssetData — 에셋 자동 로드
```csharp
private static void LoadCharacterData(Controller controller)
{
    var guids = AssetDatabase.FindAssets("Char_ t:CharacterData", new[] { "Assets/03.Data/Characters" });
    // ...
}
```

---

## 8. UI 컴포넌트 설계 원칙

### Initialize(data) 패턴
모든 UI 컴포넌트는 `Initialize(data)` 메서드로 데이터 주입:
```csharp
public class MyComponent : MonoBehaviour
{
    public void Initialize(MyData data)
    {
        // 1. 데이터 저장
        // 2. Button/Graphic 보완 (UIAutoBindHelper)
        // 3. 자식 raycast 비활성화
        // 4. Render()
    }
}
```

### StretchToParent — 부모 영역 채우기
```csharp
public static void StretchToParent(RectTransform rect)
{
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;
    rect.pivot = new Vector2(0.5f, 0.5f);
}
```

### CreateText — 자동 LayoutElement + SafeText
```csharp
public static TextMeshProUGUI CreateText(string name, Transform parent, string content, ...)
{
    // ...
    tmp.text = SafeText(content);  // 특수 기호 ASCII 변환
    var le = go.AddComponent<LayoutElement>();
    le.flexibleWidth = 1;
    le.flexibleHeight = 1;
    le.minWidth = 10;
    le.minHeight = 10;
    return tmp;
}
```

---

## 9. Sprite 생성 전략

### PROCEDURAL (코드 생성)
- 자원별 배지, 그라디언트 패널, 단색 버튼
- `PartySelectionSpriteGenerator` 패턴 — `Texture2D.SetPixel` → PNG 저장 → 9-slice border 자동 설정

### BAKED (디자인 도구)
- 복잡한 텍스처, 양피지 질감, 룬 문양
- 브라우저 목업 캡처 → 잘라내기 → 9-slice Sprite

### EXTERNAL (외부 제작)
- 캐릭터 초상화, 스킬 아이콘
- 플레이스홀더(자원색 + 이니셜)로 시작 → 실제 아트로 교체

---

## 10. 폰트 Fallback 체인

TMP 폰트가 특정 Unicode를 지원하지 않으면 □로 표시. Fallback 체인 필수:
- Cinzel SDF (영문 전용) → NanumGothic SDF (한국어 fallback)
- 특수 기호(⚡⚙⚜ 등)는 폰트가 지원 안 하면 `SafeText()`로 ASCII 변환

### UIFontSetupGuide.md 참조
- Cinzel 4종 (Regular/Medium/Bold/Black) + Cormorant Garamond Italic
- 각 SDF에 NanumGothic SDF를 Fallback Font Assets에 연결

---

## 11. 씬 전환 파이프

### 정적 필드로 데이터 전달
씬 경계를 넘을 때는 static 필드 사용 (PlayerPrefs 금지):
```csharp
public class PartySelectionController : MonoBehaviour
{
    public static List<CharacterData> SelectedParty { get; private set; }

    private void OnEmbark(...)
    {
        SelectedParty = party.Select(d => d.CharacterData).ToList();
        SceneManager.LoadScene("MapScene");
    }
}

// MapScene 측
var selectedParty = PartySelectionController.SelectedParty;
if (selectedParty != null && selectedParty.Count > 0)
{
    // 파티 사용
}
```

### SceneTransition.FadeToScene
페이드 효과와 함께 씬 전환:
```csharp
SceneTransition.Instance.FadeToScene("PartySelectionScene");
```

---

## ★ UI 작업 완료 체크리스트

새 UI 컴포넌트/씬 작성 후 반드시 점검:

- [ ] **MonoBehaviour별 별도 .cs 파일** — 한 파일에 여러 MonoBehaviour 정의 없는지
- [ ] **LayoutGroup 설정** — `childControlWidth/Height=true`, `childForceExpand=false` (기본값)
- [ ] **자식 LayoutElement 명시** — 빈 부모 GameObject에 LayoutElement 없는지
- [ ] **AutoBindMissingFields** — 컨트롤러 Awake에서 인스펙터 바인딩 실패 시 자동 보완
- [ ] **자식 raycastTarget=false** — Initialize에서 `DisableChildRaycastsExcept` 호출
- [ ] **GameObject 이름 고유** — "Content" 등 범용 이름 피하기
- [ ] **폰트 Fallback** — Cinzel SDF에 NanumGothic SDF 연결
- [ ] **SafeText** — 특수 기호 ⚡⚙⚜ 등은 ASCII 변환
- [ ] **씬 빌드 후 Play 검증** — 코드 수정 후 반드시 씬 재빌드
- [ ] **콘솔 에러/경고 확인** — "Missing Script" 경고 없는지
- [ ] **디버그 로그 정리** — 완료 후 불필요한 Debug.Log 제거
- [ ] **Build Settings 등록** — 새 씬이 Scenes In Build에 있는지

---

## 참고 자료

- `Assets/09.Docs/WorkLog/2026-07-18.md` — PartySelection 상세 작업 이력
- `Assets/09.Docs/WorkLog/2026-07-19.md` — ★ MapScene Rework 상세 작업 이력
- `Assets/09.Docs/UIFontSetupGuide.md` — 폰트 설정
- `Assets/09.Docs/UIDesignAssetManifest.md` — 에셋 분류
- `Assets/09.Docs/PartySelectionUILayoutGuide.md` — 레이아웃 수동 조정 가이드
- `Assets/02.Scripts/UI/UIAutoBindHelper.cs` — ★ 재사용 가능한 UI 헬퍼
- `UI_Mockup/PartySelection_Mockup.html` — PartySelection 웹 목업
- `UI_Mockup/MapScene_Mockup.html` — ★ MapScene Rework 웹 목업 (v2 — 9 레이어)

---

## 12. ★ MapScene Rework 노하우 (2026-07-19 추가)

PartySelection 노하우를 그대로 적용하여 MapScene을 전면 리워크하면서 학습한 추가 교훈.

### 12.1 다른 씬 리워크 시 체크리스트
- [ ] 기존 씬(MapScene.unity)은 보존, 새 Rework 씬(MapSceneRework.unity)으로 빌드 → 검증 후 교체
- [ ] 새 폴더(Rework/)에 컴포넌트 배치, namespace도 별도(Rework)로 분리 — 기존 코드와 충돌 회피
- [ ] SpriteGenerator는 씬 전용으로 별도 작성 (`MapSceneSpriteGenerator.cs`) — 공통 Sprite는 기존 것 재사용
- [ ] 씬 빌더는 main + .Parts partial로 분할 — 헬퍼(CreateSection/CreatePanelHeader)는 main, 각 섹션은 .Parts

### 12.2 ★ 다른 시스템의 데이터 API 사전 확인
MapScene은 GameRunState/Character/SkillInventoryComponent 등 다양한 데이터 소스에 의존. 컴포넌트 작성 전 반드시:
- `GameRunState.RelicHandler.Relics` (IReadOnlyList<RelicData>) — 보유 유물
- `Character.Resource.CurrentStacks` — 자원 수치 (Stacks 아님)
- `Character.Health.IsDead/CurrentHP/MaxHP` — HP 상태
- `CharacterData.ResourceType` — 자원 타입 (UIPalette 매핑용)
- `SkillInventoryComponent` — 증강 추출 API가 불명확하면 안전 폴백(빈 리스트)으로 두고 TODO 표시

### 12.3 NonCombatLayers 명시적 분리 원칙
"전투 사이마다 비전투 노드" 같은 요구사항은 무작위 배정으로는 보장 불가. MapGenerationConfig에 `NonCombatLayers` HashSet을 명시적으로 두고:
1. GenerateFloor에서 비전투 레이어는 단일 노드(타입 미정)로 시작
2. AssignSpecialNodeTypes가 1순위로 비전투 레이어에 Event/Shop/Rest 분배
3. 폴백: 비전투 레이어 중 배정 못 받은 노드는 자동 Event 강제 (Battle으로 남기면 보장 깨짐)

### 12.4 SpriteGenerator 재사용 패턴
- `HexColor()`, `SaveSprite()`, `EnsureOutputDirectory()` 헬퍼는 거의 모든 UI 작업에 동일 패턴 적용 가능
- 노드 아이콘의 "중앙 문양"은 폰트로 처리하는 것보다 픽셀 단위 직접 그리기가 더 선명 (폰트 fallback 이슈 회피)
- 9-slice border는 Sprite 전체 영역 사용 시 `new Vector4(0,0,0,0)`으로 설정, 패널처럼 늘어나야 하면 명시적 border 값

### 12.5 ★ PartySelectionSpriteGenerator와의 협력
공통 에셋(골드 테두리/양피지 패널/남색 패널/핏빛 버튼)은 PartySelectionSpriteGenerator 출력을 그대로 재사용. 씬 전용 SpriteGenerator는 진짜 그 씬만의 고유 Sprite(노드 아이콘 등)만 생성. 중복 생성 방지.

---

> 이 문서는 살아있는 문서입니다. 새로운 UI 함정을 발견하면 즉시 추가할 것.
