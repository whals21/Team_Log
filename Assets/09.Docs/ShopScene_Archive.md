# Shop UI — 향후 후보군 아카이브

> 2026-07-20 기준, 상점 UI는 **B 시안 (Stained Glass Reliquary)** 로 채택됨.
> 본 문서는 향후 **상점 종류가 다양화**될 때 적용을 고려할 수 있는 **C/D 시안**의 디자인 의도와 활용 시나리오를 보존.

---

## 원본 목업

- `UI_Mockup/ShopScene_Mockup.html` — 4종 시안 전체 비교 (A/B/C/D)
- B 시안은 `ShopReworkView` 구현에 반영 완료 (Assets/02.Scripts/UI/Shop/ShopReworkView.cs)

---

## C 시안 — Alchemist's Laboratory (연금술사 실험실)

### 분위기
- **컨셉**: 마법사의 실험실. 유리병(시약관)이 진열된 5×2 선반.
- **색상 톤**: 청록(venom #6ed5b2) + 보라(shadow #b388ff) + 어두운 녹회 배경
- **폰트 강조**: 마법사적 신비감 — Cormorant Italic이 잘 어울림
- **레이아웃**: 세로형 선반 2줄 (한 줄에 5슬롯). 각 슬롯은 유리병 + 라벨 + 가격.

### 적용 시나리오
- **연금술사 NPC (Cael)** 전용 상점 — Alchemist 캐릭터가 운영하는 특수 상점
- **이벤트 보상**: 연금술 이벤트(Phase E)에서 "비약"을 구매하는 특수 상점
- **마법사 타워 스테이지**: S3 Shadows Glade 또는 S2 Ruined Temple에서 등장
- **포션/소모품 시스템 도입 시**: 현재는 증강/유물만 있지만, 향후 포션 아이템이 추가되면 자연스럽게 매칭

### 구현 시 필요한 수정
1. **LabVialSpriteGenerator** 신규 — 유리병 5종 (Rarity별 액체 색상: 회/파랑/보라/황금/핏빛)
2. **ShopLayoutMode** enum 추가 — `Reliquary`(B) vs `Laboratory`(C) 전환. ShopReworkView가 enum 기반으로 프레임 Sprite/레이아웃 교체
3. **ShopSceneReworkBuilder.Parts.cs**에 `BuildLaboratoryFrame()` 별도 추가 (GlassCrown 대신 실험실 선반 헤더)
4. **이벤트/NPC 데이터에 shopLayout 필드 추가** — StageThemeData 또는 EventData에서 특정 상점의 레이아웃 지정

### 시각적 포인트
- 유리병 내 액체가 Rarity별로 다른 색/발광
- 병 마개(코르크) 작은 디테일
- 선반의 가로선 (박스 테두리) — 빈 슬롯과 점유 슬롯 구분
- 마법진 펜던트 장식 (헤더 좌/우)

---

## D 시안 — Merchant Caravan (떠돌이 상인 캐러밴)

### 분위기
- **컨셉**: 야외에서 천막을 치고 물건을 파는 떠돌이 상인. 가죽, 나무, 천.
- **색상 톤**: 따뜻한 호박(type-augment #c98a3a) + 갈색 + 황금빛 하이라이트
- **폰트 강조**: 친근하고 거친 모험 느낌 — Cinzel Bold가 잘 어울림
- **레이아웃**: 3×2 그리드. 가죽 테두리 + 천막 지붕 무늬(상단) + 짚/가죽 바닥(하단).

### 적용 시나리오
- **일반 전투 층 상점** — S1 GreyForest / S1 Sunscorched Plains 같은 야외 스테이지의 기본 상점
- **유목민 NPC** — Wandering Merchant 이벤트에서 등장
- **보스 전 야영지** — Floor 보스 클리어 후 쉴 때 딸려오는 야영 상점
- ** 거래/모험 강조**: 신성(Event B) ↔ 세속(Shop D) 대비로 장소 다양성 극대화

### 구현 시 필요한 수정
1. **CaravanSpriteGenerator** 신규 — 천막 지붕 패턴, 가죽 테두리, 짚 바닥 패턴 3종
2. **ShopLayoutMode** enum에 `Caravan` 추가 (위와 동일한 메커니즘)
3. **ShopSceneReworkBuilder.Parts.cs**에 `BuildCaravanTentFrame()` 별도 추가 (천막 지붕 + 가죽 + 짚 바닥)
4. **이벤트/NPC 데이터에 shopLayout 필드** (C와 동일)

### 시각적 포인트
- 상단 천막 지붕 — 빨강/갈색 가로 줄무늬 (repeating-linear-gradient)
- 하단 짚/가죽 바닥 — 미세한 사선 패턴
- 슬롯 좌측 4px 띠 — Type/Rarity 구분 (황금/핏빛/파랑/보라)
- 골드 표시를 "purse(주머니)" 아이콘과 함께 — 캐러밴 컨셉 강조

---

## 통합 아키텍처 (향후 확장 시)

```
ShopReworkView.cs (메인 로직 — 공통)
  ├─ ShopLayoutMode { Reliquary, Laboratory, Caravan }  // 신규 enum
  ├─ ApplyLayout(mode)  // 프레임 Sprite + 헤더 + 배경 교체
  └─ 자식 구조는 동일 (TopBar/GoldBar/SlotContainer/Tabs)

ShopSceneReworkBuilder.Parts.cs
  ├─ BuildReliquaryFrame()   // 현재(B)
  ├─ BuildLaboratoryFrame()  // 후보(C)
  └─ BuildCaravanTentFrame() // 후보(D)

ShopSceneSpriteGenerator.cs
  ├─ (현재) Reliquary용 7 Sprite
  ├─ (후보) Laboratory용 5 Sprite — LabVial_*.png + LabShelf.png
  └─ (후보) Caravan용 5 Sprite — TentRoof.png + LeatherBorder.png + StrawFloor.png
```

## 채택 우선순위 (사용자 피드백 시)

1. **1순위**: C (Laboratory) — 연금술사 캐릭터(Cael) 추가 시 자연스럽게 매칭
2. **2순위**: D (Caravan) — S1 같은 야외 스테이지 분위기 다양화
3. **3순위**: A (Parchment Ledger) — 가장 텍스트 중심이지만 B와 톤이 안 맞아 보류

## 메모

- B 시안 먼저 안정화(C/A/D 시안은 B 구현 완료 후)
- 각 시안을 선택할 때마다 별도 SpriteGenerator/Builder 추가 — EventSceneReworkBuilder 패턴 그대로
- ShopLayoutMode enum을 미리 도입해두면 향후 확장 시 코드 수정 최소화 (선택적)
