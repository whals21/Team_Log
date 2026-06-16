# 스테이지 테마별 배경 이미지 — AI 생성 프롬프트

> 턴제 카드 드로우 로그라이크 RPG **Team Log** 의 12개 스테이지 테마별 전투 배경 이미지 생성용 프롬프트.
> AI 이미지 생성기(Midjourney / Stable Diffusion / DALL-E)에 그대로 복사해서 사용.

---

## 공통 가이드

### 아트 스타일 목표
- **장르**: 다크 판타지 로그라이크 RPG 배경 아트
- **참고작**: Slay the Spire, Darkest Dungeon, Darkest Dungeon II, Slay the Spire II
- **톤**: 어둡고 묵직한 분위기. 현재 게임 배경색 `#141428`(암남색)과 조화
- **절대 원칙**: 인물/캐릭터/텍스트/UI 요소 **불포함**. 순수 환경 배경만.

### 기술 사양
| 항목 | 값 |
|------|-----|
| 해상도 | 1920×1080 (풀HD 16:9). Retina 대응 시 2560×1440 권장 |
| 포맷 | PNG (무손실) |
| 구도 | 중앙은 비교적 단순/어둡게 (전투 UI 패널이 중앙에 오므로). 시각적 포인트는 **상단(하늘/천장)과 하단(바닥)**에 배치 |
| 밝기 | UI 가독성 확보를 위해 전체적으로 어두운 톤. 밝은 하이라이트는 최소화 |
| 가장자리 | 화면 테두리는 더 어둡게 (UI 패널이 도드라지도록) |

### UI 레이아웃 주의점 (배경이 가려지는 영역)
```
┌─────────────────────────────────────────────┐
│ [상단] 하늘/천장 — 비교적 잘 보임            │
├──────────┬──────────────────┬───────────────┤
│ 플레이어  │   적 패널 (중앙)  │  전투 로그    │
│ 패널 좌측 │   가려짐          │  우측 가려짐  │
├──────────┴──────────────────┴───────────────┤
│ [하단] 액션바 — 가려짐 / 바닥 텍스처 부분 보임 │
└─────────────────────────────────────────────┘
```
→ **중앙과 좌우는 UI에 가려지므로, 중요한 디테일은 상단 30%와 하단 15%에 배치할 것.**

---

## 공통 스타일 프롬픽스 (모든 프롬프트 앞에 붙임)

```
dark fantasy game background art, digital painting, atmospheric, cinematic lighting, moody, desaturated color palette, painterly illustration, no characters, no people, no creatures, no text, no UI, wide establishing shot
```

## 공통 네거티브 프롬프트 (Stable Diffusion용)

```
characters, people, person, creatures, monsters, animals, text, letters, UI elements, interface, buttons, icons, watermarks, signatures, bright cheerful colors, cartoon style, anime style, chibi, flat colors, low detail, blurry, noisy, grainy, oversaturated
```

## Midjourney 파라미터 (각 프롬프트 끝에 추가)

```
--ar 16:9 --style raw --v 6 --s 250 --q 2
```

---

# Stage 1 — 튜토리얼 (학습: AP 관리, 타겟 우선순위)

## 1. 잿빛 숲 (GreyForest)

**테마 키워드**: 재생, 독 | **보스**: 고블린왕

```
dark fantasy game background art, digital painting, atmospheric, cinematic lighting, moody, desaturated color palette, painterly illustration, no characters, no people, no creatures, no text, no UI, wide establishing shot, a misty dead forest at grey dawn, ancient gnarled trees with bare twisted branches, thick grey fog rolling between mossy tree trunks, poisonous mushrooms glowing faintly purple-green on the forest floor, damp decaying leaves, overgrown thorny vines, a narrow dirt path winding into the haze, somber muted grey-green tones with hints of sickly violet, cold diffused light filtering through dense canopy, eerie unsettling atmosphere, depth and atmospheric perspective --ar 16:9 --style raw --v 6 --s 250 --q 2
```

## 2. 서리 고개 (FrostedPass)

**테마 키워드**: 둔화, 빙결 | **보스**: 고블린왕

```
dark fantasy game background art, digital painting, atmospheric, cinematic lighting, moody, desaturated color palette, painterly illustration, no characters, no people, no creatures, no text, no UI, wide establishing shot, a frozen mountain pass at blue twilight, jagged ice-covered cliffs on both sides, snow-covered pine trees bent under frost, frozen waterfall caught mid-cascade, glittering ice crystals floating in the cold air, a snow-covered trail leading upward, pale blue and cyan dominant with dark grey rock accents, cold moonlight casting long blue shadows, biting frost atmosphere, visible breath mist, desolate and bone-chilling mood --ar 16:9 --style raw --v 6 --s 250 --q 2
```

## 3. 모래 평원 (SunscorchedPlains)

**테마 키워드**: 은폐, 회피 | **보스**: 고블린왕

```
dark fantasy game background art, digital painting, atmospheric, cinematic lighting, moody, desaturated color palette, painterly illustration, no characters, no people, no creatures, no text, no UI, wide establishing shot, a vast harsh desert plain under a hazy bleached sky, rippling sand dunes stretching to the horizon, scattered jagged rock formations and dead twisted trees, a faint sandstorm swirling in the distance, cracked parched earth in the foreground, bones half-buried in sand, heat shimmer distorting the air, muted sandy ochre and dusty brown tones with pale washed-out sky, harsh blinding sunlight partially obscured by blowing sand, desolate unforgiving wasteland atmosphere --ar 16:9 --style raw --v 6 --s 250 --q 2
```

---

# Stage 2 — 체력 관리 (학습: 정화, 치명타, 순차 처결)

## 4. 혈련 예배당 (CrimsonChapel)

**테마 키워드**: 흡혈, 부활 | **보스**: 드래곤

```
dark fantasy game background art, digital painting, atmospheric, cinematic lighting, moody, desaturated color palette, painterly illustration, no characters, no people, no creatures, no text, no UI, wide establishing shot, interior of a ruined gothic cathedral, towering stone pillars with cracked arches, shattered stained glass windows with deep crimson and dark red panes, blood-red candle flames flickering on ornate iron candelabras, a decrepit stone altar in the center background, dried bloodstains on the cracked marble floor, tattered banners hanging from walls, creeping red vines growing through cracks, deep crimson and dark obsidian dominant with warm bloody highlights from candles, ominous sacrificial atmosphere, shafts of dim red light from broken windows --ar 16:9 --style raw --v 6 --s 250 --q 2
```

## 5. 부패 늪 (RotbloomBog)

**테마 키워드**: 독, 전염 | **보스**: 드래곤

```
dark fantasy game background art, digital painting, atmospheric, cinematic lighting, moody, desaturated color palette, painterly illustration, no characters, no people, no creatures, no text, no UI, wide establishing shot, a decaying swamp bog shrouded in toxic green fog, twisted dead trees rising from murky stagnant water, thick green algae and slime covering the water surface, glowing poisonous swamp flowers in sickly yellow-green, bubbling pools of murky liquid, rotting fallen logs half-submerged, hanging moss and dead vines dangling from branches, gnats and spores floating in the thick air, murky dark green and brown tones with toxic luminescent green accents, oppressive miasma, decay and disease atmosphere --ar 16:9 --style raw --v 6 --s 250 --q 2
```

## 6. 유적 잔해 (RuinedTemple)

**테마 키워드**: 언데드, 저주 | **보스**: 드래곤

```
dark fantasy game background art, digital painting, atmospheric, cinematic lighting, moody, desaturated color palette, painterly illustration, no characters, no people, no creatures, no text, no UI, wide establishing shot, ancient ruined temple interior, crumbling stone columns lying broken on the ground, cracked stone floor with overgrown weeds pushing through, a collapsed roof open to a grey sky, weathered carved statues of forgotten gods missing heads and limbs, dust and debris scattered across the floor, faint ghostly blue mist lingering in the shadows, cryptic runes carved into walls faintly glowing, grey stone and dusty brown tones with ethereal pale blue accents, solemn cursed and forgotten atmosphere, shafts of dim dusty light from above --ar 16:9 --style raw --v 6 --s 250 --q 2
```

---

# Stage 3 — 자원 압박 (학습: 쉴드 활용, 다중 타겟, 버스트 딜)

## 7. 심연 해구 (AbyssalTrench)

**테마 키워드**: 흡수, 속박 | **보스**: 마왕

```
dark fantasy game background art, digital painting, atmospheric, cinematic lighting, moody, desaturated color palette, painterly illustration, no characters, no people, no creatures, no text, no UI, wide establishing shot, deep ocean abyss floor, pitch black underwater void with bioluminescent creatures and plants glowing in the distance, towering rock formations rising from the darkness, strange alien coral structures emitting faint teal and cyan light, floating particles and marine snow drifting downward, ancient submerged ruins barely visible in the gloom, crushing darkness with pockets of eerie bioluminescent glow, deep abyssal black-blue dominant with glowing teal and violet accents, crushing oppressive pressure, primordial terrifying depth atmosphere --ar 16:9 --style raw --v 6 --s 250 --q 2
```

## 8. 번개 봉우리 (Stormpeak)

**테마 키워드**: 기절, 연쇄 | **보스**: 마왕

```
dark fantasy game background art, digital painting, atmospheric, cinematic lighting, moody, desaturated color palette, painterly illustration, no characters, no people, no creatures, no text, no UI, wide establishing shot, a jagged mountain peak summit during a violent thunderstorm, dark swirling storm clouds filling the sky, a massive lightning bolt striking in the background illuminating sharp rocky cliffs, rain lashing against wet stone surfaces, a narrow rocky ridge leading to the peak, churning dark purple-grey clouds, flickering electric light from constant lightning, dark grey rock and storm cloud tones with bright electric white-violet lightning accents, chaotic violent energy, overwhelming raw power of nature atmosphere --ar 16:9 --style raw --v 6 --s 250 --q 2
```

## 9. 그림자 골짜기 (ShadowsGlade)

**테마 키워드**: 은신, 회피 | **보스**: 마왕

```
dark fantasy game background art, digital painting, atmospheric, cinematic lighting, moody, desaturated color palette, painterly illustration, no characters, no people, no creatures, no text, no UI, wide establishing shot, a dark shadowy forest clearing at midnight, impossibly dark trees with black bark, deep shadows pooling between trunks, wisps of dark purple mist drifting low across the ground, faint moonlight barely penetrating the dense canopy creating isolated shafts of cold light, dark flowers with subtle glow, twisted roots breaking through the dark soil, an unsettling stillness, deep blacks and dark purples dominant with faint cold moonlight accents, mysterious hidden and dangerous atmosphere, things lurking just beyond sight --ar 16:9 --style raw --v 6 --s 250 --q 2
```

---

# Stage 4 — 클라이맥스 (학습: 통합 운영, 페이즈 대비)

## 10. 불꽃왕좌 (EmberThrone)

**테마 키워드**: 화염, 폭발 | **보스**: 마왕

```
dark fantasy game background art, digital painting, atmospheric, cinematic lighting, moody, desaturated color palette, painterly illustration, no characters, no people, no creatures, no text, no UI, wide establishing shot, a massive volcanic throne room, rivers of glowing orange lava flowing through cracks in the black obsidian floor, a towering empty stone throne on a raised platform in the background, walls of dark volcanic rock with veins of molten magma, burning embers and ash floating in the hot air, jagged volcanic pillars framing the throne, intense orange-red glow from below contrasting with black charred stone, billowing dark smoke near the ceiling, molten orange and deep black dominant with bright ember highlights, overwhelming heat and menace, apocalyptic power atmosphere --ar 16:9 --style raw --v 6 --s 250 --q 2
```

## 11. 영원동토 (EternalTundra)

**테마 키워드**: 빙결, 봉쇄 | **보스**: 마왕

```
dark fantasy game background art, digital painting, atmospheric, cinematic lighting, moody, desaturated color palette, painterly illustration, no characters, no people, no creatures, no text, no UI, wide establishing shot, a frozen wasteland under a grey overcast sky, vast flat expanse of ice and snow, frozen ruins of an ancient fortress half-buried in snowdrifts, icicles hanging from broken stone arches, a blizzard reducing visibility, ice-encrusted dead trees standing like skeletons, frozen bodies of water beneath translucent ice, pale grey-white and cold blue dominant with dark ice accents beneath the surface, biting wind and swirling snow, absolute desolation and deathly cold atmosphere, endless oppressive frozen silence --ar 16:9 --style raw --v 6 --s 250 --q 2
```

## 12. 마왕성 심장 (DemonCitadel)

**테마 키워드**: 소환, 다중페이즈 | **보스**: 마왕

```
dark fantasy game background art, digital painting, atmospheric, cinematic lighting, moody, desaturated color palette, painterly illustration, no characters, no people, no creatures, no text, no UI, wide establishing shot, the heart of a demon lord's citadel, a vast dark cathedral-like chamber, glowing red summoning circle carved into the obsidian floor, enormous dark iron chains hanging from the ceiling, pulsing red crystals embedded in the walls emitting bloody light, dark gothic arches towering into darkness above, demonic runes etched into every surface glowing faint crimson, cracks in reality leaking dark purple energy, a massive dark portal or rift in the far wall, deep crimson red and void black dominant with dark purple energy accents, overwhelming dread and infernal power, the final boss lair atmosphere --ar 16:9 --style raw --v 6 --s 250 --q 2
```

---

## 생성 후 처리 가이드

### 1. 스타일 통일 체크
12장 모두 생성 후 한 번에 검토:
- [ ] 색온도가 테마별로 차별화되는가? (Stage 1=회색/갈색, Stage 2=핏빛/독색, Stage 3=심청/번개/흑보라, Stage 4=용암/빙백/진홍)
- [ ] 화면 가장자리가 충분히 어두운가? (UI 패널 가독성)
- [ ] 중앙에 시선을 빼앗는 요소가 없는가? (전투 UI가 중앙에 위치)
- [ ] 인물/캐릭터가 없는가?

### 2. Unity 임포트 설정
- Texture Type: **Sprite (2D and UI)**
- Sprite Mode: **Single**
- Pixels Per Unit: 100
- Max Size: 2048 (또는 Original)
- Compression: High Quality
- 에셋 경로: `Assets/08.Resource/Backgrounds/Stage{N}_{ThemeId}.png`
  - 예: `Assets/08.Resource/Backgrounds/Stage1_GreyForest.png`

### 3. 코드 연동 (추후 구현)
배경 이미지를 `StageThemeData` SO에 `Sprite backgroundImage` 필드로 추가 → BattleSceneSetup에서 현재 스테이지 테마의 배경을 BG_Far에 할당. (별도 작업 항목)

---

## 파일 명명 규칙

에셋 제공 시 아래 파일명으로 저장해 주세요:

```
Assets/08.Resource/Backgrounds/
├── Stage1_GreyForest.png
├── Stage1_FrostedPass.png
├── Stage1_SunscorchedPlains.png
├── Stage2_CrimsonChapel.png
├── Stage2_RotbloomBog.png
├── Stage2_RuinedTemple.png
├── Stage3_AbyssalTrench.png
├── Stage3_Stormpeak.png
├── Stage3_ShadowsGlade.png
├── Stage4_EmberThrone.png
├── Stage4_EternalTundra.png
└── Stage4_DemonCitadel.png
```
