# Team Log — UI 디자인 산출물 매니페스트 (UI-A.5)

> **작성일**: 2026-07-17
> **목적**: Party Selection UI (다크 판타지 고딕) 제작에 필요한 모든 비주얼 에셋의 출처/스펙/우선순위 정리.
> **원본 디자인**: `UI_Mockup/PartySelection_Mockup.html` (다크 판타지 고딕 + 캐러셀 중앙 포커스)

---

## 1. 에셋 분류 체계

각 에셋은 3가지 제작 방식 중 하나로 분류:

| 분류 | 설명 | 작업 주체 |
|------|------|----------|
| **PROCEDURAL** | 코드로 생성 (PartySelectionSpriteGenerator.cs) | 자동 |
| **BAKED** | 웹 목업/디자인 도구에서 이미지로 베이킹 → Sprite Import | 자동/반자동 |
| **EXTERNAL** | 외부 아티스트/AI 생성/에셋 스토어 구매 | 사용자 |

---

## 2. PROCEDURAL 에셋 (자동 생성됨)

> 메뉴 `TeamLog/UI/Generate Party Selection Sprites` 실행 시 자동 생성.
> 출력 폴더: `Assets/03.Data/UI/PartySelection/`

| 에셋 | 해상도 | 9-slice Border | 용도 | 비고 |
|------|--------|----------------|------|------|
| `ResourceBadge_EMBER.png` | 128×128 | — | Ember 자원 배지 | Ashe |
| `ResourceBadge_VENGEANCE.png` | 128×128 | — | Vengeance 배지 | Duran |
| `ResourceBadge_FROST.png` | 128×128 | — | Frost 배지 | Lumi |
| `ResourceBadge_PROPHECY.png` | 128×128 | — | Prophecy 배지 | Sibyl |
| `ResourceBadge_CHARGE.png` | 128×128 | — | Charge 배지 | Taranis |
| `ResourceBadge_SHADOWS.png` | 128×128 | — | Shadows 배지 | Umbra |
| `ResourceBadge_COMBO.png` | 128×128 | — | Combo 배지 | Aster |
| `ResourceBadge_CORPSE.png` | 128×128 | — | Corpse 배지 | Mortis |
| `ResourceBadge_DISCOVER.png` | 128×128 | — | Discover 배지 | Cael |
| `ResourceBadge_MELODY.png` | 128×128 | — | Melody 배지 | Calliope |
| `ResourceBadge_MERCY.png` | 128×128 | — | Mercy 배지 | Elara |
| `GoldBorder_9Slice.png` | 48×48 | 12,12,12,12 | 두꺼운 골드 테두리 (메인 패널) | 4px 외곽 |
| `GoldBorderThin_9Slice.png` | 48×48 | 6,6,6,6 | 얇은 골드 테두리 (서브 패널) | 2px 외곽 |
| `ParchmentPanel_9Slice.png` | 64×64 | 8,8,8,8 | 양피지 밝은 패널 | 강점/약점 박스 배경 |
| `ParchmentDark_9Slice.png` | 64×64 | 8,8,8,8 | 양피지 어두운 패널 | 자원 메커니즘 박스 |
| `SlatePanel_9Slice.png` | 64×64 | 8,8,8,8 | 남색 기본 패널 | 캐릭터 카드 배경 |
| `SlatePanelLight_9Slice.png` | 64×64 | 8,8,8,8 | 남색 밝은 패널 | hover/selected 상태 |
| `BloodButton_Normal.png` | 48×48 | 8,8,8,8 | 핏빛 버튼 기본 | EMBARK, 주요 CTA |
| `BloodButton_Hover.png` | 48×48 | 8,8,8,8 | 핏빛 버튼 hover | 마우스 오버 |
| `BloodButton_Pressed.png` | 48×48 | 8,8,8,8 | 핏빛 버튼 pressed | 클릭 순간 |
| `RuneOverlay_Tile.png` | 200×200 | — | 룬 문양 타일 배경 | 화면 전체 오버레이, Repeat |
| `Crest_Logo.png` | 128×128 | — | 팀 로고 (문장) | 헤더 우측 또는 타이틀 |
| `Shadow_Vignette.png` | 256×256 | — | 코너 비네팅 | 화면 가장자리 어둠 |

총 **23종 자동 생성**

---

## 3. BAKED 에셋 (웹 목업에서 추출, 반자동)

이 에셋들은 procedural로 만들면 퀄리티가 떨어지므로, 웹 목업을 브라우저에서 렌더링 한 뒤 고품질 PNG로 추출하는 방식을 권장.

### 3.1 추출 절차

1. **브라우저에서 웹 목업 열기**
   - `UI_Mockup/PartySelection_Mockup.html` Chrome/Firefox로 열기
2. **개발자 도구(F12) → 디바이스 툴바(Ctrl+Shift+M)**
   - 해상도 1280×820 고정
3. **스크린샷 캡처 도구 사용**
   - 권장: Chrome 확장 `GoFullPage` 또는 Firefox 개발자 도구의 "스크린샷 찍기" (전체 화면)
   - 또는 `Windows + Shift + S` 부분 캡처
4. **이미지 편집기(GIMP/Krita/Photoshop)로 부품별 잘라내기**
   - 저장: `Assets/03.Data/UI/PartySelection/Baked/`

### 3.2 추출 대상 부품

| 부품 | 스펙 | 용도 |
|------|------|------|
| 초상화 프레임 배경 (원형) | 256×320 PNG, 투명 배경 | 캐릭터 초상화 뒤 프레임 |
| 자원 메커니즘 박스 배경 | 256×80 PNG | 자원 설명 박스 |
| EMBARK 버튼 완성본 | 240×56 PNG (3-state) | 시작 버튼 최종 |
| 정체성 인용구 배경 | 600×60 PNG | 양피지 텍스처 포함 |
| 캐릭터 카드 (선택됨) | 280×360 PNG | 캐러셀 active 상태 테두리 |
| 캐릭터 카드 (파티 소속) | 80×80 PNG | 체크 배지 |

### 3.3 웹 목업 → Unity Sprite 변환 스크립트 (선택)

원하면 `puppeteer` 또는 `html2canvas-pro`로 자동화 가능:

```bash
# Node.js 환경에서 (선택적 자동화)
npm install puppeteer
node scripts/render_mockup_to_png.js PartySelection_Mockup.html
```

이 단계는 사용자가 Node 환경이 익숙할 때만 권장. 기본은 수동 캡처.

---

## 4. EXTERNAL 에셋 (외부 제작 필요)

### 4.1 ★ 캐릭터 초상화 (11종) — 최우선

현재는 **플레이스홀더** (자원 색상 + 거대 이니셜)로 시작 → 추후 실제 아트로 교체.

#### 스펙
- **해상도**: 512×640 (세로형, 초상화 비율 4:5)
- **포맷**: PNG, 투명 배경 또는 어두운 배경 (#0a0a14)
- **스타일**: 다크 판타지 일러스트 (Hades, Darkest Dungeon, Hearthstone 카드 참고)
- **색상**: 각 캐릭터 자원 색상을 메인 톤으로 (Ember=주황, Vengeance=핏빛, ...)
- **구도**: 상반신 중심, 얼굴 가까이, 캐릭터 정체성 시각화

#### 캐릭터별 컨셉 (아티스트 브리핑용)

| 캐릭터 | 핵심 시각 요소 | 분위기 |
|--------|----------------|--------|
| **Ashe** (Pyromancer) | 화염에 휩싸인 여성, 한쪽 눈 빛남, 재가스피 | 고통+결의 |
| **Duran** (Warrior) | 무거운 갑옷, 거대 방패, 핏빛 룬 검 | 묵직함+복수 |
| **Lumi** (Cryomancer) | 푸른 로브, 얼음 결정 떠다님, 창백한 피부 | 차가움+통제 |
| **Sibyl** (Oracle) | 눈가리개, 모래시계 들고 있음, 청록 빛 | 신비+시간 |
| **Taranis** (Stormcaller) | 번개 항아리, 전류 흐르는 팔, 노란 망토 | 역동+연쇄 |
| **Umbra** (Rogue) | 두건, 그림자 속 단검, 보라 안개 | 은밀+치명 |
| **Aster** (Archer) | 활과 화살통, 황금 퀴버, 날카로운 눈 | 집중+정확 |
| **Mortis** (Necromancer) | 해골 지팡이, 독녹 두건, 시체 아우라 | 죽음+수확 |
| **Cael** (Alchemist) | 연보라 로브, 물약들, 거대 플라스크 | 실험+반응 |
| **Calliope** (Bard) | 리륨, 분홍빛 악보, 화려한 모자 | 리듬+피날레 |
| **Elara** (Healer) | 은금 로브, 빛나는 지팡이, 온화한 미소 | 구원+희생 |

#### 플레이스홀더 전략
- UI-B에서는 procedural로 생성한 "원형 + 자원색 gradient + 거대 이니셜" 초상화 사용
- 초상화 영역에 `<자원 배지 색상>` + `<캐릭터 이니셜>` 텍스트
- 추후 실제 아트가 준비되면 같은 위치에 Sprite 교체만 하면 됨 (코드 수정 불필요)

### 4.2 스킬 아이콘 (44종) — 차순위

각 캐릭터 4스킬 × 11캐릭터 = 44종.

#### 스펙
- **해상도**: 96×96 PNG, 투명 배경
- **포맷**: PNG
- **스타일**: 톤 일관성 중요 (같은 캐릭터 스킬은 같은 컬러 팔레트)
- **대체**: 현재는 이모지/유니코드 기호(◈ ⚚ ✦ ※) 사용 → 추후 아이콘 에셋으로 교체

#### 우선순위
- Phase UI-B: 이모지로 시작 (목업 HTML 참고)
- Phase UI-F (선택): 전문 아이콘 에셋으로 교체 (Game-icons.net, CC0 라이선스 권장)

### 4.3 특성 아이콘 (33종 = 3 × 11) — 후순위

현재는 특성 카드에 텍스트만. 추후 시각적 강조용 아이콘 추가 가능.

### 4.4 정교한 양피지 텍스처 (선택)

procedural 양피지는 단조로울 수 있음. 실제 양피지/종이 질감 PNG로 교체하면 퀄리티 향상.

- **스펙**: 512×512 PNG, 타일 가능
- **소스**: TextureHaven, PolyHaven (CC0), 또는 직접 촬영 (스캔한 종이)

### 4.5 정교한 룬 문양 패턴 (선택)

procedural 룬은 단순한 원+다이아몬드. 더 풍부한 룬/마법진 디자인은 외부 SVG/PNG로.

- **스펙**: 400×400 PNG, 투명 배경, 양피지 알파 0.04
- **참고**: Game-icons.net의 Rune 시리즈, Freepik 마법진 벡터

---

## 5. 우선순위 로드맵

### Phase UI-A (자원 준비 — 현재)
- ✅ Procedural 23종 (PartySelectionSpriteGenerator)
- ⏳ Cinzel 폰트 다운로드 + SDF 생성 (UIFontSetupGuide.md 참고)
- ⏳ Cormorant Garamond Italic SDF
- ⏳ (선택) NanumMyeongjo SDF

### Phase UI-B (컴포넌트 작성)
- Procedural 배지/패널 사용
- 캐릭터 초상화는 **플레이스홀더** (자원색 gradient + 이니셜)
- 스킬 아이콘은 이모지/유니코드 기호

### Phase UI-C (씬 빌더)
- 모든 procedural 에셋 연결
- 폰트 TMP 에셋 참조

### Phase UI-D (통합)
- 기존 CharacterData와 연동
- 데이터 흐름 검증

### Phase UI-F (선택, 마무리)
- 실제 캐릭터 초상화 11종 외부 제작
- 스킬 아이콘 44종 외부 제작
- 정교한 양피지/룬 텍스처
- 이때까지는 플레이스홀더로도 충분히 플레이 가능

---

## 6. 에셋 라이선스 가이드

외부 에셋 사용 시 라이선스 주의:

| 소스 | 라이선스 | 비고 |
|------|----------|------|
| **Google Fonts** (Cinzel, Cormorant) | OFL 1.1 | 상업적 사용 가능, 저작권 표시 권장 |
| **Game-icons.net** | CC BY 4.0 | 저작권 표시 필수 |
| **PolyHaven / TextureHaven** | CC0 | 완전 자유 |
| **Freepik** | 무료/유료 혼합 | 무료 버전은 저작권 표시 필수 |
| **Hire 아티스트** | 계약서 명시 | 독점 라이선스 권장 |

---

## 7. 파일 구조 (최종)

```
Assets/
├── 03.Data/
│   └── UI/
│       └── PartySelection/
│           ├── ResourceBadge_EMBER.png          ← PROCEDURAL
│           ├── ResourceBadge_VENGEANCE.png
│           ├── ... (총 11종 배지)
│           ├── GoldBorder_9Slice.png
│           ├── GoldBorderThin_9Slice.png
│           ├── ParchmentPanel_9Slice.png
│           ├── ParchmentDark_9Slice.png
│           ├── SlatePanel_9Slice.png
│           ├── SlatePanelLight_9Slice.png
│           ├── BloodButton_Normal.png
│           ├── BloodButton_Hover.png
│           ├── BloodButton_Pressed.png
│           ├── RuneOverlay_Tile.png
│           ├── Crest_Logo.png
│           ├── Shadow_Vignette.png
│           ├── Baked/                            ← BAKED (사용자 추출)
│           │   ├── PortraitFrame.png
│           │   └── ...
│           └── Characters/                       ← EXTERNAL (아티스트)
│               ├── Ashe_Portrait.png
│               ├── Duran_Portrait.png
│               └── ... (총 11종)
├── 08.Resource/
│   └── Fonts/
│       ├── Cinzel-Regular.ttf                   ← 사용자 다운로드
│       ├── Cinzel-Bold.ttf
│       ├── Cinzel-Black.ttf
│       ├── CormorantGaramond-Italic.ttf
│       ├── Cinzel-Regular SDF.asset             ← TMP Font Asset Creator
│       ├── Cinzel-Bold SDF.asset
│       ├── Cinzel-Black SDF.asset
│       └── CormorantGaramond-Italic SDF.asset
└── 09.Docs/
    ├── UIFontSetupGuide.md                      ← UI-A.4
    └── UIDesignAssetManifest.md                 ← 본 문서
```

---

## 8. 다음 단계

1. **(사용자)** `UIFontSetupGuide.md` 따라 폰트 다운로드 + SDF 생성
2. **(사용자)** Unity 에디터에서 메뉴 실행:
   - `TeamLog/UI/Generate Party Selection Sprites` → 23종 Sprite 자동 생성
   - `TeamLog/Generate Test Data` (기존 메뉴 — UIPalette 갱신 포함)
3. **(자동)** UI-B 착수 — Sprite 에셋 활용하여 UI 컴포넌트 작성

---

> **진행 상태**: UI-A.1~A.5 코드 작업 완료. 사용자 작업(폰트 다운로드, 메뉴 실행) 후 UI-B 진입.
