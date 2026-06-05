# 서드파티 에셋 카탈로그

> 최종 갱신: 2026-06-06
> 총 에셋: 20종 (VFX 3, SFX 3, UI/스프라이트 7, 프레임워크 6)

---

## 1. 이펙트/VFX 에셋 (4종)

### 1.1 Epic Toon FX

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/Epic Toon FX/` |
| 규모 | 1000+ 프리팹, ~40 WAV 효과음 |
| 버전 | v1.8 |
| 도큐먼트 | `ETFX v1.8 - Documentation.pdf` |

#### 폴더 구조

```
Epic Toon FX/
├── Demo/              — 24개 데모 씬 (etfx_combat, etfx_magic, etfx_healing 등)
├── Prefabs/           — 메인 3D VFX 프리팹
│   ├── Combat/        — 전투 이펙트 (Blood, Death, Explosions, Magic, Missiles, Shield, Sword)
│   ├── Environment/   — 환경 이펙트 (Confetti, Fire, Lightning, Smoke, Weather)
│   └── Interactive/   — 인터랙티브 (Cards, Healing, LevelUp, Loot, Money, Portals)
├── Prefabs 2D/        — 2D 스프라이트 기반 VFX
├── Materials/         — 공유 머티리얼
├── Models/            — 3D 모델
├── Scripts/           — 런타임 스크립트 (ETFXProjectileScript 등)
├── Shaders/           — 커스텀 셰이더
├── Sound/             — 효과음
└── Textures/          — 소스 텍스처 (Emojis, Powerbox, Symbols, Text)
```

#### Team Log 활용 매핑

| 게임 이벤트 | 추천 프리팹 경로 | 변형 수 |
|-------------|------------------|---------|
| **물리 공격 히트** | `Combat/Sword/Hit/` SwordHit, SwordHitCritical, SwordHitMagic | 4종 |
| **마법 히트** | `Combat/Explosions (Misc)/TargetHitExplosion`, `Combat/Brawling/` | 다수 |
| **화염 스킬** | `Combat/Missiles/Fireball/` + `Combat/Explosions/FireballRoundExplosion/` | Blue/Fire/Green/Pink |
| **빙결 스킬** | `Combat/Missiles/Frost/` + `Combat/Explosions/FrostExplosion/` | Frost |
| **번개 스킬** | `Combat/Missiles/Lightning/` + `Combat/Explosions/LightningExplosion/` | Blue/Green/Pink/Yellow |
| **암흑 스킬** | `Combat/Missiles/Shadow/` + `Combat/Explosions/ShadowExplosion/` | 3종 |
| **힐** | `Interactive/Healing/` HealBig, HealField, HealNova, HealOnce, HealStream 등 | 10종 |
| **쉴드** | `Combat/Magic/Shield/` + `Combat/Shield/` | Blue/Green/Purple/Yellow × 2 |
| **버프** | `Combat/Magic/Buff/` + `Combat/Magic/Aura/` | Blue/Green/Yellow |
| **디버프/독** | `Combat/Explosions (Misc)/PoisonExplosion`, PoisonSkullExplosion | 3종 |
| **기절** | `Combat/Explosions (Misc)/StunStarExplosion` + `Combat/Brawling/Stun/` | 3종 |
| **사망** | `Combat/Death/Skulls/` (10종) + `Combat/Death/Souls/` (10종) | Fire/Frost/Electric/Poison/Mystic/Evil/Generic/Cute |
| **승리** | `Environment/Confetti/` + `Interactive/Level Up/Nova/` (6색상) | 다수 |
| **골드 획득** | `Interactive/Money/Coins/` GoldCoinBlast/Directional/Fountain/Shower | 8종 |
| **보상 드롭** | `Interactive/Loot/` TreasureChestGlowRays, ItemSparkle, UnboxExplosion | 다수 |
| **카드 드로우** | `Interactive/Cards/` Cardglow 4종 | 4종 |
| **텍스트 이펙트** | `Combat/Explosions (Text)/` Bang, Boom, Critical, Hit, Miss, Pow, Zap 등 | 24종 |

#### 특이사항
- 대부분의 이펙트가 Blue/Green/Pink/Yellow 색상 변형 제공 → 속성별 색상 코딩 가능
- Missile(투사체) → Explosion(폭발) → Nova(광역) 3단계 구성 → 단일/광역 스킬 모두 커버
- 텍스트 이펙트("Critical!", "Miss!", "Pow!")는 턴제 RPG에 특히 적합
- 3D 프리팹이 주력이며, 2D 프리팹은 제한적. UI Canvas에서 사용 시 렌더 모드 조정 필요

---

### 1.2 Break Items Toon VFX

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/Break_Items_Toon_VFX/` |
| 규모 | 7 VFX 프리팹, ~40 머티리얼, ~20 메시 |
| 스타일 | 툰 렌더링 |

#### 프리팹 목록

| 프리팹 | 용도 |
|--------|------|
| `VFX_Ceramic_Hit` | 도자기 파괴 — 일반 적 피격 |
| `VFX_Coins` | 코인 분출 — 골드 획득 |
| `VFX_Confetti_Hit` | 컨페티 — 승리/보상 |
| `VFX_Diamond_Hit` | 다이아몬드 파괴 — 레어 아이템 |
| `VFX_Pumpkin_Hit` | 호박 파괴 — 특수 이벤트 |
| `VFX_Toxic_Hit` | 독성 파괴 — 독 상태이상 |
| `VFX_WoodBox_Hit` | 나무상자 파괴 — 일반 오브젝트 |

#### Team Log 활용 매핑

| 게임 이벤트 | 추천 프리팹 |
|-------------|-------------|
| 골드 획득 | VFX_Coins |
| 독 상태이상 | VFX_Toxic_Hit |
| 레어 보상 | VFX_Diamond_Hit |

---

### 1.3 Master Stylized Projectiles

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/MasterStylizedProjectiles/` |
| 규모 | 26종 투사체 (각 Muzzle + Bullet + Hit 프리팹) |
| 구성 | 75개 프리팹 + 머티리얼 + 텍스처 |

#### 투사체 종류

| 속성 | 투사체 |
|------|--------|
| **화염** | Fireball, SmallFireBullet |
| **빙결** | SmallIceBullet |
| **번개** | LightningExplosion, SmallLightBullet |
| **물리** | Arrow, GreenArrow, Shuriken, Shurikens |
| **마법** | BlueShoot, CyanBlueBullet, EnergyExplosion, SmallEnergyBullet, PurpleStar, Star |
| **암흑** | Shoot_Purple, Shoot_Red |
| **기타** | Missile, OrangeGunShot, OrangeSparkleShoot, RedSwordBeam, YellowSwordBeam, TornadoShoots, WindShoot |

#### Team Log 활용 매핑

| 스킬 타입 | 추천 투사체 |
|-----------|-------------|
| 전사 물리 공격 | RedSwordBeam, YellowSwordBeam, Arrow |
| 마법사 화염 | Fireball, SmallFireBullet |
| 마법사 빙결 | SmallIceBullet |
| 마법사 번개 | LightningExplosion |
| 도적 공격 | Shuriken, Shurikens |
| 힐러 마법 | BlueShoot, Star |

---

---

## 2. 사운드 에셋 (3종)

### 2.1 Combat Magic Spells VII SFX

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/CombatMagicSpellsVIISFX/` |
| 규모 | 342개 WAV 파일 |
| 구성 | 7속성 × 10변종 |

#### 속성별 오디오

| 속성 | 폴더 | 파일 수 | 파일명 패턴 |
|------|------|---------|-------------|
| **Fire** | `Fire1`~`Fire10` | 61 | SFX_Spell{NN}Fire/Cast/Layer/Impact |
| **Ice** | `Ice1`~`Ice10` | 65 | SFX_Spell{NN}Ice/Cast/Layer |
| **Thunder** | `Thunder1`~`Thunder10` | 42 | SFX_Spell{NN}Thunder/Cast/Charge/Impact |
| **Heal** | `Heal1`~`Heal10` | 31 | SFX_Spell{NN}Heal/Cast/Swoosh/Designed |
| **Dark** | `Dark1`~`Dark10` | 51 | SFX_Spell{NN}Dark/Cast/Layer/Impact |
| **Earth** | `Earth1`~`Earth10` | 43 | SFX_Spell{NN}Earth/Cast/Layer |
| **Water** | `Water1`~`Water10` | 49 | SFX_Spell{NN}Water/Cast/Layer |

#### 파일 유형

| 유형 | 설명 | 활용 |
|------|------|------|
| Cast | 시전/차징 사운드 | 스킬 사용 준비음 |
| Fire/Ice/Thunder/... | 속성별 메인 공격음 | 스킬 타격음 |
| Impact | 피격/명중음 | 데미지 적중음 |
| Layer | 레이어드 사운드 (믹싱용) | 고급 사운드 믹싱 |
| Charge | 차징 사운드 | 강력한 스킬 준비음 |
| Swoosh | 바람/휘두르는 소리 | 물리 공격 |
| Designed | 완성형 사운드 | 단독 사용 가능 |

#### Team Log → AudioPalette 매핑

| AudioPalette 키 | 추천 소스 |
|-----------------|-----------|
| `AttackHit` | `Fire1/SFX_Spell01Fire.wav` 또는 `Thunder1/SFX_Spell01Impact01.wav` |
| `Heal` | `Heal1/SFX_SpellDesigned01Heal.wav` |
| `Shield` | `Earth1/SFX_Spell01Earth.wav` (둔탁한 방어음) |
| `Victory` | `Heal3/SFX_Spell01Cast01.wav` (밝은 사운드) |
| `Defeat` | `Dark1/SFX_Spell01Dark.wav` |

---

### 2.2 Fantasy UI SFX - Lite Edition

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/Fantasy UI SFX - Lite Edition/` |
| 규모 | 210개 WAV 파일 (플랫 구조, 서브폴더 없음) |
| 특성 | RPG 특화 UI 사운드 |

#### 카테고리별 오디오

| 카테고리 | 파일 수 | 활용 |
|----------|---------|------|
| **Card Draw** | 7 | 스킬 드로우 — ActionSlot 리롤 |
| **Card Place** | 5 | 스킬 사용 — ActionSlot 클릭 |
| **Coins** | 14 | 골드 획득 — 보상/상점 |
| **Coin Bag** | 6 | 골드 지불 — 상점 구매 |
| **Potion Item** | 3 | 포션 사용 — 힐/버프 아이템 |
| **Alchemy** | 1 | 연금술 — 아이템 제작 |
| **Food Eat** | 4 | 음식 섭취 — 휴식/캠프파이어 |
| **Armor** | 5 | 방어구 장착 — 쉴드 효과 |
| **Blacksmith** | 12 | 대장간 — 장비 강화/수련 |
| **Book Page** | 5 | 책 넘기기 — 배틀로그/이벤트 |
| **Book Handle** | 4 | 책 열기/닫기 — 상점/이벤트 |
| **Interface** | 19 | 일반 UI 클릭 |
| **Building Interface** | 21 | 빌딩/구조물 UI — 맵 노드 |
| **Magical Interface** | 25 | 마법 UI — 스킬 선택/버프 |
| **Magical Texture Chimes** | 4 | 차임벨 — 알림/토스트 |
| **Special Interface** | 3 | 특수 UI — 레벨업/보스 경고 |
| **Arrow & Bow** | 6 | 화살 — 궁수 스킬 |
| **Bag Handle** | 25 | 가방 — 인벤토리 열기 |
| **Dice** | 12 | 주사위 — 랜덤 이벤트 |
| **Key & Lock** | 6 | 열쇠 — 잠금 해제 |
| **Weapon** | 3 | 무기 휘두름 — 전사 공격 |

#### Team Log → AudioPalette 매핑 (UI 사운드)

| 용도 | 추천 파일 |
|------|-----------|
| 스킬 드로우 | `Card Draw 1-1.wav` ~ `Card Draw 3-2.wav` (7종) |
| 스킬 사용 | `Card Place 1-1.wav` ~ `Card Place 2-3.wav` (5종) |
| 골드 획득 | `Coins 1-5.wav` ~ `Coins 4-06.wav` (14종) |
| 골드 지불 | `Coin Bag 1-1.wav` ~ `Coin Bag 3-1.wav` (6종) |
| 힐/포션 | `Potion Item 1-1.wav` ~ `Potion Item 1-3.wav` (3종) |
| 쉴드 | `Armor 1-1.wav` ~ `Armor 1-5.wav` (5종) |
| UI 버튼 | `Interface 1-1.wav` ~ `Interface 6-5.wav` (19종) |
| 마법 UI | `Magical Interface 1-1.wav` ~ `Magical Interface 10-4.wav` (25종) |
| 토스트/알림 | `Magical Texture Chimes 1-1.wav` ~ `1-4.wav` (4종) |
| 책/로그 | `Book Page 1-1.wav` ~ `Book Page 1-5.wav` (5종) |
| 휴식 | `Food Eat 02.wav` ~ `Food Eat 05.wav` (4종) |
| 수련 | `Blacksmith 1-1.wav` ~ `Blacksmithing 4-3.wav` (12종) |

---

### 2.3 UI SFX Mega Pack

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/UI SFX Mega Pack/` |
| 규모 | 5,300+ WAV 파일 |
| 구성 | 9개 카테고리 |

#### 카테고리

| 카테고리 | 폴더 | 파일 수 | 활용 |
|----------|------|---------|------|
| **Buttons** | `Audio/Buttons/` | 1,598 | 모든 UI 버튼 |
| **High_Pitched** | `Audio/High_Pitched/` | 1,560 | 경쾌한 UI 피드백 |
| **Purchase** | `Audio/Purchase/` | 540 | 상점 구매 |
| **Cancel** | `Audio/Cancel/` | 400 | 취소/뒤로 |
| **Repair** | `Audio/Repair/` | 400 | 수리/복구 |
| **Ok** | `Audio/Ok/` | 300 | 확인/승인 |
| **Warning_Popup** | `Audio/Warning_Popup/` | 310 | 경고/주의 |
| **Extras** | `Audio/Extras/` | 130 | 기타 |
| **Sliders** | `Audio/Sliders/` | 126 | 슬라이더/스크롤 |

#### Team Log 활용 매핑

| 용도 | 추천 카테고리 |
|------|---------------|
| 일반 버튼 | Buttons (1,598종에서 선택) |
| 상점 구매 | Purchase (540종) |
| 구매 취소 | Cancel (400종) |
| 확인 다이얼로그 | Ok (300종) |
| 경고 (골드 부족 등) | Warning_Popup (310종) |

---

## 3. UI/스프라이트 에셋 (7종)

### 3.1 GUI Pro-CasualGame (Layer Lab)

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/Layer Lab/GUI Pro-CasualGame/` |
| 규모 | 1,200+ 스프라이트, 386개 프리팹 |
| 현재 활용 | 12개 스프라이트 (PanelFrame, ButtonFrame, CardFrame 등) |

#### 스프라이트 카테고리

| 카테고리 | 수량 | Team Log 활용 |
|----------|------|---------------|
| **Icon_PictoIcons** | 324종 × 5사이즈 | 스킬 아이콘, 상태이상 배지, UI 버튼 |
| **Icon_ItemIcons** | ~100종 × 6사이즈 | 인벤토리, 상점, 보상 아이콘 |
| **Icon_RuneIcons** | 67종 × 6사이즈 | 캐릭터 스탯, 장비 보너스 |
| **Button** | 75 | UI 버튼 전반 |
| **Frame** | 174 | 카드 프레임, 패널 배경 |
| **Slider** | 103 | HP 바, 경험치 바 |
| **Popup** | 28 | 다이얼로그 배경 |
| **Label** | 34 | 리본, 타이틀 배너 |
| **UI_Etc** | 52 | 토글, 탭바, 체크박스 |
| **IconMisc** | 167 | 보물상자, 메뉴, 보상 패스, 스킬 |
| **Icon_ShopItems** | 23 | 코인(5티어), 젬(3종×5티어), 체스트 |
| **Demo_Character** | 41 | 캐릭터 초상샘플 |

#### PictoIcons 주요 아이콘 (RPG 활용)

| 분류 | 아이콘 목록 |
|------|-------------|
| **전투** | Attack, Battle, Boss, Critical, Damage, Defense, Defense_Weak, Fight, Fist, Health, Passive, Tank |
| **무기** | Sword, Axe, Hammer, Hatchet, Wand_0/1/2, Gun, Missile, Shuriken |
| **방어** | Shield, Armor, Helmet, Viking_Helmet, Glove |
| **마법** | Magic, Magic_Ball, Magic_Bomb, Magic_Drop, Magic_Square, Rune, Crystal |
| **상태이상** | Buff, Posion (sic), Bad_Immune, Sleep, Energy, Life, Life_Add, Life_Break |
| **아이템** | Potion, Key, Key_Crown, Chest_0/1, Coin_Crown/Star/Skull, Gem_Diamond/Hexagon/Rhombus/Triangle, Mana, Ring, Scroll, Bag, Pouch, Book_0, Crown, Laurel, Medal, Trophy |
| **UI** | Arrow (8방향), Plus/Minus, Check, Close, Refresh, Reload, Search, Filter, Setting, Star, Info, Help, Warning, Alert, Lock/Unlock |
| **직업** | Priest, Witchhat, Ankh, Yinyang, Angel |

#### ItemIcons 주요 아이콘

| 분류 | 아이콘 목록 |
|------|-------------|
| **포션** | Potion01 (Blue/Green/Orange/Purple/Red/Yellow), Potion02 (Blue/Green/Pink/Purple/Red/Yellow/YellowGreen) — 13종 |
| **젬** | Gem01~04 (Blue/Green/Purple/Red/Yellow), Gem_Pearl — 13종 |
| **열쇠** | Key_Bronze, Key_Silver, Key_Gold — 3티어 |
| **무기** | Sword, Hammer, Missile, Boxing Gloves |
| **방어구** | Shield, Boots, Flippers, GearWheels |
| **소비품** | Food_Can, Food_Meat, Food_Pizza, Egg, Nut |
| **폭탄** | Bomb_Bomb, Bomb_Dynamite, Bomb_LandMine |
| **통화** | Gold, Golds, Clover, Star, Star_Red |
| **기타** | Chest, Chest_Open, Bag, Book, Cards, Compass, Crown, Dice, Hourglass, Map, Trophy |

#### RuneIcons 구성

6개 룬 세트 (RuneIcon0~5), 각 세트마다 동일한 11개 스탯:
`Ball_Count, Ball_Health, Buff, Critical_Chance, Critical_Damage, Damage, Debuff, Get_Coin, Get_Score, Passive` + 프레임 6종

---

### 3.2 Classic RPG GUI

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/Classic_RPG_GUI/` |
| 규모 | 160+ 스프라이트 PNG |
| 스타일 | WoW 스타일 RPG UI |

#### 스프라이트 카테고리

| 카테고리 | 수량 | 내용 |
|----------|------|------|
| **Parts/** | 120+ | HP/Mana/Energy 바, 프레임 (hero, inventory, skill, dialogue), 버튼 (mini, mid, long), 스크롤, 미니맵, XP 바, 화살표 |
| **frame_backgrounds/** | 22 | 장비 슬롯 배경 (head, chest, boots, gloves, ring, shield, weapon, potion, skill, rune) |
| **Icons/** | 16 | 메뉴 아이콘 (Bag, Equip, Fight, Inventory, Quest, Skills, Runes, Skull, Trade, Honor) |

#### Team Log 활용 매핑

| 용도 | 추천 리소스 |
|------|-------------|
| HP 바 | Parts/ HP/Mana 라인 스프라이트 |
| 장비 슬롯 | frame_backgrounds/ (head, chest, boots 등) |
| 스킬바 | Parts/ skill frame |
| 인벤토리 프레임 | Parts/ inventory frame |
| 대화창 | Parts/ dialogue frame |
| 메뉴 아이콘 | Icons/ (Fight, Inventory, Quest, Skills 등) |

---

### 3.3 Modern UI Pack

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/Modern UI Pack/` |
| 규모 | 100+ 프리팹, 20개 카테고리 |
| 구성 | Prefabs/ + Animations/ + Scripts/ + Textures/ |

#### 프리팹 카테고리

| 카테고리 | Team Log 활용 |
|----------|---------------|
| Modal Window | ConfirmationDialog 대체/보강 |
| Notification | ToastUI 보강 |
| Progress Bar | HP/XP 바 |
| Slider | 설정 화면 |
| Tooltip | TooltipUI 보강 |
| Switch / Toggle | 설정 토글 |
| Dropdown | 선택 메뉴 |
| List View | 아이템 목록 |
| Spinner | 로딩 표시 |

---

### 3.4 CaptainCatSparrow — 픽셀아트 스펠 아이콘

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/CaptainCatSparrow/SpellIconsVolume_1_Free/` |
| 규모 | 40개 PNG (무료판) |

#### 속성별 아이콘

| 속성 | 아이콘 수 | 범위 | 활용 |
|------|-----------|------|------|
| **Dark** | 10 | Dark_6 ~ Dark_15 | 저주/디버프 스킬 |
| **Fire** | 10 | Fire_5 ~ Fire_14, Fire_28 | 화염 공격 스킬 |
| **Holy** | 10 | Holy_5 ~ Holy_14 | 힐/버프 스킬 |
| **Nature** | 10 | Nature_2 ~ Nature_13 | 자연/쉴드 스킬 |

---

### 3.5 Tazo 2D — 판타지 스킬 아이콘

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/Tazo_2D/Icon/` |
| 규모 | 122개 PNG (플랫 구조) |

#### 주요 아이콘

| 분류 | 예시 아이콘 |
|------|-------------|
| **물리** | S_sword_hit, S_Arrow_crowd, S_rock_ball, S_Explosive |
| **화염** | S_fire_furious, S_Forward_ray, S_rubysun |
| **빙결** | S_chilly_kick, S_Forward_freeze |
| **번개** | S_Forward_thurder, S_Thunder_horn, S_Thunder_rage |
| **자연** | S_Fallbreeze, S_Green_invade, S_Cold_bark |
| **암흑** | S_devilwake, S_mistery_hit, S_stranger_light |
| **버프/힐** | S_divine, S_essence_found, S_Blue_firework |
| **일반** | S_spell01 ~ S_spell05 |

---

### 3.6 RPG Icons Pixel Art

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/RPG Icons Pixel Art/` |
| 규모 | 70+ 카테고리 (대규모 픽셀아트 아이콘 모음) |

#### 주요 카테고리

| 분류 | 카테고리 |
|------|----------|
| **클래스/스킬** | Aeromancer, Cryomancer, Lightning mage, Blood mage, Archer, Barbarian, Druid, Fairy, Goblin, Dwarf, Demon, Dark_Elves |
| **무기** | Axes, Bows, Daggers, Maces, Swords, Exotic weapons |
| **방어구** | Helmets, Cuirass, Belts, Bracers, Shields |
| **아이템** | Craft_materials, Gems, Food, Potions, Alchemy, Books |
| **몬스터** | Chaos monsters, Goblins, Demon, Fairies, Dwarf avatars |
| **상태** | Buffs, Anti-buffs, Curse |

---

### 3.7 Motion Titles Pack

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/Motion Titles Pack/` |
| 규모 | 20종 애니메이션 타이틀 (MTP-1 ~ MTP-20) |

#### 구성

각 타이틀: In/Loop/Out 애니메이션 + 컨트롤러 + 스프라이트 변형 (Flat/Icons/Radial/Rounded/Tilted)

#### Team Log 활용 매핑

| 용도 | 추천 |
|------|------|
| 전투 시작 | "Battle Start" 타이틀 |
| 승리 | "Victory" 타이틀 |
| 패배 | "Defeat" 타이틀 |
| 층 전환 | "Floor 2 — Ruins" 등 |
| 보스 등장 | 보스 이름 타이틀 |

---

## 4. 프레임워크/유틸리티 에셋 (7종)

### 4.1 DOTween + DOTween Pro

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/Plugins/DOTween/`, `Assets/Plugins/DOTweenPro/` |
| 유형 | 트윈 애니메이션 엔진 |
| 활용도 | **높음** |

**활용 방안**: UIAnimationHelper 코루틴 기반 트윈 → DOTween으로 전환 시 성능 및 코드 간결성 개선. HP 바, 페이드, 스케일, 이동 등 모든 UI 애니메이션에 적용 가능.

---

### 4.2 ProCamera2D

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/ProCamera2D/` |
| 유형 | 2D 카메라 시스템 (20개 확장) |
| 활용도 | **높음** |

**주요 확장**: Shake, Zoom, Rooms, Rails, Parallax, TransitionsFX, Boundaries, PixelPerfect

**활용 방안**: 카메라 셰이크 (피격 시), 줌 (보스 등장), 룸 전환 (맵 노드), 패럴랙스 (배경), 트랜지션 (씬 전환).

---

### 4.3 Easy Save 3

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/Plugins/Easy Save 3/` |
| 유형 | 세이브/로드 (AES 암호화) |
| 활용도 | **높음** |

**활용 방안**: GameRunState 영속화 (런 중간 저장/복원), 설정 저장, 해금 데이터 저장. CLAUDE.md에서 PlayerPrefs 금지이므로 대안으로 사용.

---

### 4.4 SRDebugger (Stompy Robot)

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/StompyRobot/SRDebugger/` |
| 유형 | 인게임 디버그 패널 |
| 활용도 | **높음** |

**기능**: 런타임 콘솔, 프로파일러, 옵션 패널, 버그 리포터

**활용 방안**: SROptions로 캐릭터 스탯/AP/골드 실시간 조정, 밸런싱 테스트. `SRDebug.Init()` 한 줄로 활성화.

---

### 4.5 Odin Inspector (Sirenix)

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/Plugins/Odin Inspector/` |
| 유형 | 인스펙터 강화 |
| 활용도 | **중간** |

**활용 방안**: ScriptableObject 편집 UX 개선 (CharacterData, SkillData 필드 그룹화, 검증, 드로어 커스텀).

---

### 4.6 BG Database (BansheeGz)

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/BansheeGz/BGDatabase/` |
| 유형 | 인메모리 DB (DLL 기반) |
| 활용도 | **낮음** |

**비고**: 현재 DataGenerator + ScriptableObject 체계로 충분. 향후 복잡한 쿼리가 필요하면 검토.

---

### 4.7 Drag & Drop Pro

| 항목 | 내용 |
|------|------|
| 경로 | `Assets/Drag & Drop Pro/` |
| 유형 | 드래그앤드롭 (5개 스크립트) |
| 활용도 | **낮음** |

**비고**: 현재 클릭 기반 UI로 충분. 향후 인벤토리/장비 드래그가 필요하면 검토.

---

## 5. 활용 우선순위 (요약)

| 순위 | 작업 | 에셋 | 난이도 | 상태 |
|------|------|------|--------|------|
| **1** | AudioPalette 오디오 클립 할당 | CombatMagicSpellsVII + Fantasy UI SFX | 낮음 | 미구현 |
| **1** | 전투 이펙트 (히트/힐/쉴드/사망) | Epic Toon FX | 낮음 | 미구현 |
| **2** | 스킬/아이템/상태이상 아이콘 | Layer Lab PictoIcons/ItemIcons/RuneIcons | 낮음 | 미구현 |
| **2** | UIAnimationHelper → DOTween 전환 | DOTween | 중간 | 미구현 |
| **3** | 카메라 연출 (셰이크/룸/바운더리) | ProCamera2D | 중간 | 미구현 |
| **4** | 스킬 아이콘 다각화 | CaptainCatSparrow + Tazo 2D | 낮음 | 미구현 |
| **4** | 캐릭터 상세/장비 패널 | Classic RPG GUI | 중간 | 미구현 |
| **5** | 인게임 디버그 패널 | SRDebugger | 매우 낮음 | 미구현 |
| **5** | 세이브/로드 시스템 | Easy Save 3 | 낮음 | 미구현 |
| **5** | 전투 타이틀 연출 | Motion Titles Pack | 낮음 | 미구현 |
