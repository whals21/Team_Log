# Party Selection Scene — UI 배치 수동 조정 설명서

> **작성일**: 2026-07-18
> **목적**: SceneBuilder가 자동 생성한 씬의 레이아웃이 깨져서, 사용자가 인스펙터에서 수동으로 수정할 수 있도록 정확한 수치를 제공.
> **기준**: 웹 목업 `UI_Mockup/PartySelection_Mockup.html` (1280×820 다크 판타지 고딕)
> **해상도 기준**: 1280×820 (Canvas Scaler 기준)

---

## 0. 현재 문제 진단

스크린샷에서 확인된 문제:
1. "TEAM LOG" 텍스트만 보이고 나머지 UI가 안 보임
2. 여러 빈 패널이 흩어져 있음
3. 초상화 / 정보 패널 / 스킬 카드 / 특성 카드가 안 보임

**근본 원인**:
- `VerticalLayoutGroup`/`HorizontalLayoutGroup`의 `childForceExpandHeight=false`인데 자식들이 `LayoutElement.preferredHeight`이 없어서 0 높이로 붕괴
- Anchor가 0.5,0.5 중심으로 설정되어 부모 크기를 안 채움
- 일부 Image가 LayoutGroup 안에 없어서 부모 안에서 자기 마음대로 위치함

**해결 전략**: LayoutGroup 의존을 줄이고, **Anchor + 절대 좌표(AnchoredPosition) + SizeDelta** 기반으로 직접 배치. 이 설명서대로 인스펙터에서 수정.

---

## 1. Canvas / Camera 설정

### Camera (Main Camera)
- **Clear Flags**: Solid Color
- **Background**: `#050509` (R5 G5 B9)
- **Projection**: Orthographic
- **Size**: 5 (가로 1280 / 세로 820 비율에 맞추려면 약 5)
- **Position**: (0, 0, -10)

### Canvas
- **Render Mode**: Screen Space - Overlay
- **CanvasScaler**:
  - **UI Scale Mode**: Scale With Screen Size
  - **Reference Resolution**: X=1280, Y=820
  - **Screen Match Mode**: Expand
  - **Match Width Or Height**: 0.5

---

## 2. 전체 계층 트리 (목표 구조)

```
Canvas (1280×820)
├── Background (전체)
│   ├── RuneOverlay (전체, Tile)
│   ├── Vignette_TL/TR/BL/BR (코너 4개)
├── MainFrame (좌우하상 20px padding)
│   ├── Header (56px 높이, 상단)
│   ├── Stage (중앙, 1fr)
│   ├── Carousel (110px 높이)
│   └── Footer (86px 높이, 하단)
└── PartySelectionController (빈 GO)
```

---

## 3. Background 설정

### Background (Image)
- **Anchor Min**: (0, 0)
- **Anchor Max**: (1, 1)
- **Pivot**: (0.5, 0.5)
- **Size Delta**: (0, 0)
- **Anchored Position**: (0, 0)
- **Color**: `#0a0a14` (R10 G10 B20 / 0.039, 0.039, 0.078)

### RuneOverlay (Background의 자식)
- **Anchor Min**: (0, 0)
- **Anchor Max**: (1, 1)
- **Pivot**: (0.5, 0.5)
- **Size Delta**: (0, 0)
- **Image Type**: Tiled
- **Sprite**: `Assets/03.Data/UI/PartySelection/RuneOverlay_Tile.png`
- **Color**: R=1 G=1 B=1 A=**0.4**

### Vignette_TL (Background의 자식)
- **Anchor Min**: (0, 0.65)
- **Anchor Max**: (0.35, 1)
- **Pivot**: (0.5, 0.5)
- **Size Delta**: (0, 0)
- **Sprite**: `Shadow_Vignette.png`
- **Color**: R=0 G=0 B=0 A=**0.7**

Vignette_TR / BL / BR 도 각 코너에 맞춰 Anchor만 변경 (TR: 0.65,1 ~ 1,0.65 / BL: 0,0 ~ 0.35,0.35 / BR: 0.65,0 ~ 1,0.35).

---

## 4. MainFrame (전체 컨테이너)

### MainFrame (RectTransform)
- **Anchor Min**: (0, 0)
- **Anchor Max**: (1, 1)
- **Pivot**: (0.5, 0.5)
- **Left/Right**: 20 / -20 (또는 Size Delta X=−40)
- **Top/Bottom**: -20 / 20 (또는 Size Delta Y=−40)
- **Anchored Position**: (0, 0)
- **VerticalLayoutGroup**:
  - **Padding**: Left=20, Right=20, Top=20, Bottom=20
  - **Spacing**: 12
  - **Child Alignment**: Upper Center
  - **Child Control Width**: ☑
  - **Child Control Height**: ☐ (중요 — 체크 해제)
  - **Child Force Expand Width**: ☑
  - **Child Force Expand Height**: ☐ (중요 — 체크 해제)

> **★ 핵심**: Child Control Height와 Force Expand Height를 모두 **해제**하면, 각 자식이 자신의 LayoutElement.preferredHeight만큼 차지합니다. Header/Carousel/Footer는 명시적 높이, Stage는 flexibleHeight=1로.

### Header / Stage / Carousel / Footer 공통
모두 MainFrame의 직계 자식으로, Anchor는 다음과 같이 동일:
- **Anchor Min**: (0, 0)
- **Anchor Max**: (1, 1)
- **Pivot**: (0.5, 0.5)
- **Size Delta**: (0, 0)

각각 LayoutElement로 크기 지정:

### Header LayoutElement
- **Preferred Height**: 56
- **Flexible Height**: 0

### Stage LayoutElement
- **Preferred Height**: 100 (최소, 무시됨)
- **Flexible Height**: 1 (★ 남은 공간 모두 차지)

### Carousel LayoutElement
- **Preferred Height**: 110
- **Flexible Height**: 0

### Footer LayoutElement
- **Preferred Height**: 86
- **Flexible Height**: 0

---

## 5. Header 상세 배치 (56px 높이)

### HeaderPanel (Header 자리의 루트)
- LayoutElement: Preferred Height=56
- Anchor: 부모 영역 전체 (0,0)-(1,1), Size Delta (0,0)
- **Image (BG)**: Sprite=SlatePanel_9Slice, Color=White, Type=Sliced
- **HorizontalLayoutGroup**:
  - Padding: L=36, R=36, T=8, B=8
  - Spacing: 24
  - Child Alignment: Middle Center
  - Control Width/Height: ☑/☑
  - Force Expand Width: ☑
  - Force Expand Height: ☑

### 좌측 (Left) — 로고 + 타이틀
- LayoutElement: Flexible Width=1
- HorizontalLayoutGroup: Spacing=12, Alignment=MiddleLeft

#### Crest (문장 로고)
- LayoutElement: Preferred Width=34, Preferred Height=34
- Image: Sprite=`Crest_Logo.png`, Color=White

#### TitleGroup
- VerticalLayoutGroup: Spacing=0, Alignment=MiddleLeft
- Control Width: ☑, Force Expand Width: ☑

**Title (TextMeshProUGUI)**:
- Text: `TEAM LOG`
- Font: Cinzel-Black SDF (없으면 NanumGothic SDF)
- Font Size: 22
- Color: `#f4d35e` (R244 G211 B94)
- Alignment: Left
- Character Spacing: 4
- Style: Bold

**Subtitle (TextMeshProUGUI)**:
- Text: `A ROGUELIKE CHRONICLE`
- Font Size: 10
- Color: `#6b5e44`
- Alignment: Left
- Character Spacing: 4

### 우측 (Right) — 메타 pill + 설정 버튼
- HorizontalLayoutGroup: Spacing=12, Alignment=MiddleRight
- Control Width/Height: ☐/☑ (중요 — Width는 자식이 결정)
- Force Expand Width: ☐

#### Pill_STAGE / Pill_ASCENSION / Pill_GOLD
- LayoutElement: Preferred Height=28
- Image: Sprite=SlatePanelLight_9Slice, Color=White, Type=Sliced
- HorizontalLayoutGroup: Padding=L12 R12, Spacing=8, Alignment=MiddleCenter

각 Pill 안:
- **Label** (TMP): Font Size=9, Color=`#6b5e44`, Character Spacing=3
- **Value** (TMP): Font Size=11, Bold, Color=필러별 상이
  - STAGE: `#f4d35e` / ASCENSION: `#c0392b` / GOLD: `#f4d35e`

#### BtnSettings
- LayoutElement: Preferred Width=32, Preferred Height=32
- Button + Image: Sprite=SlatePanelLight_9Slice, Color=White
- 자식 Label TMP: `⚙`, Font Size=14, Color=`#d4af37`

### 하단 골드 라인 (HeaderPanel의 자식, 가장 마지막)
- **Anchor Min**: (0, 0)
- **Anchor Max**: (1, 0)
- **Pivot**: (0.5, 0)
- **Size Delta**: (0, 3)
- **Anchored Position**: (0, 0)
- **Image**: Sprite=GoldBorderThin_9Slice, Color=White, Type=Sliced

---

## 6. Stage 상세 배치 (메인 디스플레이)

### StageContainer
- LayoutElement: Preferred Height=100, Flexible Height=1
- **HorizontalLayoutGroup**:
  - Padding: 8
  - Spacing: 8
  - Control Width/Height: ☑/☑
  - Force Expand Width: ☑, Force Expand Height: ☑

### BtnPrev (왼쪽 네비게이션 ‹)
- LayoutElement: Preferred Width=50, Flexible Height=1
- Button + Image: Sprite=SlatePanel_9Slice, Color=`#1a1a2e`
- 자식 Label TMP: `‹`, Font Size=36, Color=`#d4af37`, Font=Cinzel-Black SDF

### MainArea (중앙 — 초상화 + 정보)
- LayoutElement: Flexible Width=1, Flexible Height=1
- **HorizontalLayoutGroup**: Padding=8, Spacing=20, Control Width/Height=☑/☑, Force Expand Width/Height=☑/☑

### BtnNext (오른쪽 네비게이션 ›)
- BtnPrev와 동일 (Width=50)

---

## 7. PortraitArea (320px)

PortraitArea는 MainArea의 자식. 메커니즘 박스까지 포함한 영역.

### PortraitArea
- LayoutElement: Preferred Width=320, Flexible Height=1
- **VerticalLayoutGroup**: Spacing=8, Alignment=UpperCenter, Control Width: ☐, Force Expand: ☐/☑

### PortraitFrame (초상화 메인 — 280×440)
- LayoutElement: Preferred Width=280, Preferred Height=440
- **CharacterPortraitBig** 컴포넌트

내부 자식 구성:

#### Frame (외곽 골드)
- Anchor: (0,0)-(1,1), Size Delta=(0,0)
- Image: Sprite=GoldBorder_9Slice, Type=Sliced

#### InnerBG (내부 보이드)
- Anchor: (0.05, 0.05)-(0.95, 0.95)
- Image: Color=`#050509`

#### Placeholder > Glow
- Anchor: (0,0)-(1,1)
- Image: Sprite=ParchmentDark_9Slice, Color=(자원색, A=0.3)

#### Placeholder > Initial (거대 이니셜)
- Anchor: (0,0)-(1,1)
- TextMeshProUGUI: Font Size=180, Color=(자원색, A=0.25), Font=Cinzel-Black SDF, Bold

#### PortraitImage (실제 초상화 — 비활성)
- Anchor: (0,0)-(1,1)
- Image: Color=White (플레이스홀더 모드에서는 비활성)

#### ResourceBadge (우상단)
- Anchor: (1, 1), Pivot: (1, 1)
- Anchored Position: (8, -8)
- Size Delta: (56, 56)
- Image: Sprite=GoldBorderThin_9Slice, Color=White

내부:
- **ResInitial** TMP: Font Size=18, Bold, Color=White
- **ResLabel** TMP: Font Size=7, Color=`#a89878`

#### NamePlate (하단 이름 각인)
- Anchor: (0.5, 0), Pivot: (0.5, 0)
- Anchored Position: (0, -12)
- Size Delta: (240, 50)
- Image: Sprite=SlatePanelLight_9Slice, Color=White

내부:
- **Name** TMP: Font Size=19, Bold, Color=`#f4d35e`, Font=Cinzel-Bold SDF
- **Title** TMP: Font Size=11, Italic, Color=`#a89878`, Font=CormorantGaramond-Italic SDF

### MechanicBox (초상화 아래 — 280×90)
- LayoutElement: Preferred Width=280, Preferred Height=90
- Image: Sprite=ParchmentDark_9Slice, Color=White, Type=Sliced
- VerticalLayoutGroup: Padding=10, Spacing=4, Control Width: ☑, Force Expand Width: ☑

내부:
- **Title** TMP: `◈  RESOURCE  MECHANIC`, Font Size=9, Color=`#d4af37`, Character Spacing=3, Font=Cinzel-Regular SDF
- **Desc** TMP: Font Size=10.5, Color=`#c9b485`, Alignment=Left

---

## 8. InfoArea (정보 패널 — 메인 스크롤 영역)

InfoArea는 MainArea의 자식. 모든 정보가 들어감.

### InfoArea
- LayoutElement: Flexible Width=1, Flexible Height=1
- **VerticalLayoutGroup**: Padding=4, Spacing=8, Control Width: ☑, Force Expand Width: ☑, Force Expand Height: ☐

> ★ Force Expand Height를 **해제**하여 자식들이 preferredHeight만큼 차지. ContentSizeFitter는 **넣지 말 것** (대신 VerticalLayoutGroup가 정렬).

### IdentityQuote (정체성 인용구 — 48px)
- LayoutElement: Preferred Height=48, Flexible Width=1
- Image: Sprite=ParchmentDark_9Slice, Color=(1,1,1,0.4), Type=Sliced
- 자식 Text TMP:
  - Anchor: (0,0)-(1,1)
  - Offset Min: (28, 8) / Offset Max: (-12, -8) (좌측 padding 28, 우측 12)
  - Font Size: 13, Italic, Color=`#c9b485`, Font=CormorantGaramond-Italic SDF
  - Alignment: Left

### StatsRow (스탯 3종 — 56px)
- LayoutElement: Preferred Height=56
- **HorizontalLayoutGroup**: Spacing=8, Force Expand Width: ☑, Force Expand Height: ☐

각 StatCell (Vigor / Resource / Role) 동일 구조:
- LayoutElement: Preferred Height=56, Flexible Width=1
- Image: Sprite=SlatePanel_9Slice, Color=White, Type=Sliced
- VerticalLayoutGroup: Padding=4, Spacing=1, Alignment=MiddleCenter, Control Width/Height: ☑/☑, Force Expand: ☑/☑

내부 (3개 TMP):
- **Label**: Font Size=9, Color=`#6b5e44`, Character Spacing=3, Font=Cinzel-Regular SDF
- **Value**: Font Size=18, Bold, Color=`#f4d35e`, Font=Cinzel-Bold SDF
- **Sub**: Font Size=9, Color=`#a89878`

### StrengthWeaknessRow (강점/약점 — 52px)
- LayoutElement: Preferred Height=52
- HorizontalLayoutGroup: Spacing=8, Force Expand Width: ☑

#### StrengthBox
- LayoutElement: Preferred Height=52, Flexible Width=1
- Image: Sprite=ParchmentDark_9Slice, Color=(0.15, 0.30, 0.15, 0.9), Type=Sliced (녹색 틴트)
- VerticalLayoutGroup: Padding=8, Alignment=UpperLeft

내부:
- **Label** TMP: `◆ STRENGTH`, Font Size=9, Bold, Color=`#7da34a`, Character Spacing=3
- **Desc** TMP: Font Size=10.5, Color=`#c9b485`, Alignment=Left

#### WeaknessBox
- StrengthBox와 동일하되 색상만 다름:
  - Background Color: (0.40, 0.15, 0.15, 0.9) (핏빛 틴트)
  - Label: `✕ VULNERABILITY`, Color=`#c0392b`

### Label_SKILLS (섹션 라벨 — 18px)
- LayoutElement: Preferred Height=18
- HorizontalLayoutGroup: Spacing=10, Alignment=MiddleCenter, Control Width: ☑

내부:
- **Line1** Image: Color=`#8b6914`, LayoutElement Flexible Width=1, Height=1
- **Text** TMP: `SKILLS`, Font Size=10, Color=`#d4af37`, Character Spacing=4, Font=Cinzel-Regular SDF
- **Line2** Image: 동일

### SkillGrid (2×2 그리드 — 200px)
- LayoutElement: Preferred Height=200
- **GridLayoutGroup**:
  - Cell Size: (200, 95)
  - Spacing: (8, 8)
  - Constraint: Fixed Column Count = 2
  - Child Alignment: Upper Center

각 SkillCard (4개 동일 구조, `Skill1`~`Skill4`):

#### Skill1/Skill2/Skill3/Skill4
- SkillDetailCard 컴포넌트
- 자식들:
  - **BG** Image: Sprite=SlatePanel_9Slice, Color=White, Type=Sliced
  - **TypeBar** Image: Anchor (0,0)-(0,1), Pivot (0, 0.5), Size Delta (4, 0)
  - **Head** HLG (좌상단, Height=44): 아이콘 + 타이틀
    - **Icon** Image: Size=(32,32), Color=자원색
    - **TitleGroup** VLG:
      - **Name** TMP: Font Size=12, Bold, Color=`#f4d35e`, Font=Cinzel-Bold SDF
      - **Cost** TMP: `AP 1`, Font Size=9, Color=`#f4d35e`
  - **Badges** HLG (중간, Height=16): 3개 TMP 배지
    - **TypeBadge**: Font Size=8.5, Color=`#c0392b`
    - **TargetBadge**: Font Size=8.5, Color=`#e74c3c`
    - **PowerBadge**: Font Size=8.5, Color=`#f4d35e`
  - **Desc** TMP: Anchor (0,0)-(1,1), Offset Min (10, 26), Offset Max (-10, -70), Font Size=10.5, Color=`#f0e6d0`, Alignment=TopLeft
  - **BonusBox** Image (하단, Height=22): Color=(0.83, 0.69, 0.22, 0.15)
    - **Text** TMP: `⚡ —`, Font Size=9.5, Color=`#c9b485`, Alignment=Left

### Label_TRAIT (섹션 라벨)
- Label_SKILLS와 동일. 텍스트만 `EQUIPPED TRAIT`.

### TraitGrid (3열 — 90px)
- LayoutElement: Preferred Height=90
- HorizontalLayoutGroup: Spacing=7, Force Expand Width: ☑

각 TraitCard (3개 동일, `Trait1`~`Trait3`):

- LayoutElement: Preferred Height=90, Flexible Width=1
- TraitDetailCard 컴포넌트
- Button + Image (BG): SlatePanel_9Slice

내부:
- **Highlight** (우상단 ✦): Anchor (1,1), Size=(16,16), TMP `✦` Color=`#f4d35e` — **비활성**
- **Content** VLG (Padding 10/6/10/6, Spacing 3, Alignment UpperLeft)
  - **Head** HLG: Name TMP + Tag TMP
    - **Name**: Font Size=10.5, Bold, Color=`#f4d35e`, Font=Cinzel-Bold SDF
    - **Tag**: `BASE` 또는 `META`, Font Size=8, Bold, Color=White
  - **Desc** TMP: Font Size=10, Color=`#f0e6d0`, Alignment=Left
  - **UnlockRow** HLG — **비활성**:
    - **Icon** TMP: `🔒`, Color=`#c0392b`
    - **Text** TMP: Font Size=9, Color=`#c0392b`

---

## 9. Carousel 상세 배치 (110px)

### CarouselPanel
- LayoutElement: Preferred Height=110
- Image (BG): Color=`#050509`
- 상단 골드 라인 (GoldLine): Anchor (0,1)-(1,1), Pivot (0.5,1), Size (0, 2), Image=GoldBorderThin_9Slice

### ScrollView
- Anchor: (0,0)-(1,1)
- Offset Min: (24, 10) / Offset Max: (-24, -10)
- ScrollRect 컴포넌트 (Horizontal만)

### Content (HorizontalLayoutGroup)
- Anchor: (0, 0)-(0, 1)
- Pivot: (0, 0.5)
- **HorizontalLayoutGroup**: Spacing=12, Alignment=MiddleCenter, Control Width: ☐
- **ContentSizeFitter**: Horizontal Fit=Preferred Size

### CarouselItemTemplate (비활성 — 컨트롤러가 Instantiate해서 사용)
- LayoutElement: Preferred Width=78, Preferred Height=100
- VerticalLayoutGroup: Spacing=4, Alignment=UpperCenter

내부:
- **Portrait** Image: Size=(70,70), Sprite=GoldBorderThin_9Slice, Color=자원색
  - **Initial** TMP: Font Size=28, Bold, Color=White, Font=Cinzel-Black SDF
  - **InPartyBadge** (우상단): Size=(22,22), Image=GoldBorderThin_9Slice, Color=`#c0392b`
  - **LockOverlay**: Anchor (0,0)-(1,1), Color=(0,0,0,0.7), 자식 TMP `🔒`
  - **ActiveRing**: Anchor (-0.05,-0.05)-(1.05,1.05), Image=GoldBorder_9Slice
- **Name** (78×16): TMP, Font Size=9, Color=`#a89878`, Character Spacing=1, Font=Cinzel-Regular SDF

---

## 10. Footer 상세 배치 (86px)

### FooterPanel
- LayoutElement: Preferred Height=86
- Image (BG): Color=`#0a0a14`
- GoldLine (상단): 동일 패턴
- **HorizontalLayoutGroup**: Padding L36 R36 T12 B12, Spacing=24, Force Expand Width: ☑

### Left (좌측 — 파티 슬롯)
- LayoutElement: Flexible Width=1
- HorizontalLayoutGroup: Spacing=14, Alignment=MiddleLeft

#### PartyLabel TMP
- Text: `PARTY`, Font Size=11, Bold, Color=`#d4af37`, Character Spacing=4, Font=Cinzel-Bold SDF

#### SlotsContainer
- HorizontalLayoutGroup: Spacing=12, Alignment=MiddleLeft, Control Width: ☐
- LayoutElement: Flexible Width=1

PartySlotPanel 컴포넌트는 FooterPanel에 부착됨 (빌더 설정).

### Right (우측 — 버튼 3종)
- HorizontalLayoutGroup: Spacing=14, Alignment=MiddleRight, Control Width: ☐

#### BtnRandom
- LayoutElement: Preferred Width=110, Preferred Height=36
- Button + Image: Sprite=SlatePanelLight_9Slice, Color=White
- 자식 Label TMP: `⚜ RANDOM`, Font Size=11, Color=`#f0e6d0`, Font=Cinzel-Bold SDF

#### BtnClear
- LayoutElement: Preferred Width=90, Preferred Height=36
- 동일 (텍스트만 `✕ CLEAR`)

#### BtnEmbark (핏빛 강조)
- LayoutElement: Preferred Width=180, Preferred Height=44
- Button + Image: Sprite=BloodButton_Normal.png, Color=White
- **Transition**: Sprite Swap
  - Highlighted: BloodButton_Hover
  - Pressed: BloodButton_Pressed
  - Disabled: SlatePanel_9Slice
- 자식 Label TMP: `EMBARK ▶`, Font Size=16, Bold, Color=`#f4d35e`, Character Spacing=4, Font=Cinzel-Black SDF
- **Interactable**: ☐ (파티 비어있을 때 — 컨트롤러가 자동 활성화)

---

## 11. 자주 발생하는 문제와 해결

### 문제 1: 자식이 안 보임 (0 크기)
**원인**: 부모 LayoutGroup이 `childControlHeight=true`인데 자식 LayoutElement.preferredHeight이 0.
**해결**:
- 자식에 LayoutElement 추가 + Preferred Height 명시
- 또는 부모의 Child Control Height를 해제

### 문제 2: Image가 전체 영역 안 채움
**원인**: Image RectTransform의 Anchor가 (0.5, 0.5)-(0.5, 0.5)로 되어 있음.
**해결**: Anchor Min=(0,0), Anchor Max=(1,1), Size Delta=(0,0), Pivot=(0.5, 0.5).

### 문제 3: 텍스트가 박스 안 안 참
**원인**: TextMeshProUGUI RectTransform이 부모 영역 안 채움.
**해결**: Anchor=(0,0)-(1,1), Size Delta=(0,0), Offset Min=(0,0), Offset Max=(0,0). 추가로 padding 원하면 Offset Min=(10,10), Offset Max=(-10,-10).

### 문제 4: LayoutGroup 자식들이 겹침
**원인**: LayoutGroup이 자식 RectTransform을 직접 조작하는데, 인스펙터에서 수동 위치를 설정한 흔적이 남음.
**해결**: 자식 RectTransform을 우클릭 → Reset. LayoutGroup가 자동 재배치.

### 문제 5: 폰트가 □ 박스로 표시
**원인**: Cinzel SDF의 Fallback에 NanumGothic SDF가 안 연결됨.
**해결**: `UIFontSetupGuide.md` 5.1절 참조 — Cinzel SDF 인스펙터의 Fallback Font Assets에 NanumGothic SDF 드래그.

### 문제 6: 9-slice Sprite가 늘어남
**원인**: Image.Type이 Simple이거나, Sprite의 Border 값이 설정 안 됨.
**해결**:
- Image.Type을 Sliced로 변경
- Sprite 에셋 클릭 → 인스펙터 → Sprite Editor → 9-slice border 설정 (보통 8,8,8,8 또는 12,12,12,12)

### 문제 7: Canvas Scaler가 작동 안 함
**원인**: Reference Resolution이 1920×1080으로 되어 있음.
**해결**: Canvas Scaler → Reference Resolution = (1280, 820).

---

## 12. 빠른 점검 체크리스트

Play 모드 진입 전 인스펙터에서 확인:

- [ ] Canvas Scaler = 1280×820, Match=0.5
- [ ] MainFrame이 화면 거의 꽉 채움 (좌우하상 20px padding)
- [ ] Header/Carousel/Footer 각각 Preferred Height(56/110/86) + Flexible Height=0
- [ ] Stage에 Flexible Height=1 설정
- [ ] PortraitFrame에 Preferred Width=280, Height=440
- [ ] MechanicBox에 Preferred Width=280, Height=90
- [ ] InfoArea 자식들이 모두 Preferred Height 가짐 (48/56/52/18/200/18/90)
- [ ] Carousel의 ScrollView가 CarouselPanel 영역 거의 채움 (24/10 padding)
- [ ] FooterPanel에 PartySlotPanel 컴포넌트 있음
- [ ] 좌/우 버튼(BtnPrev/BtnNext)이 Stage 양옆 50px

---

## 13. 수작업 시 추천 워크플로우

1. **Scene 열기** → Hierarchy 창에서 계층 구조 확인
2. **MainFrame 선택** → 인스펙터에서 VerticalLayoutGroup 설정 확인
3. **자식 각각 선택** → LayoutElement의 Preferred Height/Flexible Height 점검
4. **TextMeshProUGUI** 더블클릭 → Scene 뷰에서 텍스트 박스 위치/크기 확인
5. **Image** 클릭 → Sprite/Color/Type 점검
6. **Play 버튼** → 런타임에서 캐릭터 전환 시 자동 갱신 확인

모든 GameObject 이름은 이 설명서의 이름과 정확히 일치합니다 (예: `PortraitFrame`, `Skill1`, `Stat_Vigor`). Hierarchy에서 검색으로 찾으세요.

---

## 부록: 주요 색상 HEX 빠른 참조

| 용도 | HEX | RGB |
|------|-----|-----|
| Void (가장 어두움) | `#050509` | 5,5,9 |
| Abyss (기본 배경) | `#0a0a14` | 10,10,20 |
| Slate (패널) | `#1a1a2e` | 26,26,46 |
| GoldL (강조 골드) | `#f4d35e` | 244,211,94 |
| Gold (기본 골드) | `#d4af37` | 212,175,55 |
| GoldD (테두리) | `#8b6914` | 139,105,20 |
| BloodL (핏빛 강조) | `#c0392b` | 192,57,43 |
| Parchment (양피지 텍스트) | `#c9b485` | 201,180,133 |
| InkDim (희미 텍스트) | `#a89878` | 168,152,120 |
| InkFaint (거의 안 보임) | `#6b5e44` | 107,94,68 |
