# Team Log — 이벤트 파트 퀄리티 향상 기획서

> **기획자 시각**: EventSceneQualityAnalysis.md의 D+ 종합 평가를 **B+ 이상**으로 끌어올리기 위한 실행 계획
> **작성일**: 2026-06-19
> **목표**: "10회 반복 후 정답을 외우는" 현재 구조 → **"매 런마다 다른 딜레마와 후회"**를 주는 상용 로그라이크급 이벤트 경험
> **비교 기준**: Slay the Spire, Inscryption, Balatro

---

## 1. 현재 상태 진단 (기획 관점)

### 1-1. 정량 비교

| 항목 | StS | Inscryption | Balatro | **Team Log (현재)** | **목표** |
|------|-----|-------------|---------|---------------------|----------|
| 총 이벤트 수 | 70+ | 40+ | 30+ | 10 | **49+** (공통 25 + 테마 24) |
| 분기/연쇄 구조 | 일부 | 핵심 | 없음 | 없음 | **기반 구축** |
| 확률 기반 결과 | 있음 | 있음 | 없음 | 불가 | **4개 이상** |
| 상태이상/저주 이벤트 | 다수 | 있음 | - | **0건** | **8개 이상** |
| Story 타입 | 다수 | 핵심 | - | **0건** | **3개 이상** |
| 스테이지 테마 연계 | 부분 | 핵심 | - | 없음 | **12테마 전용** |
| ChoiceDescription UI 표시 | - | - | - | **미표시** | **전 이벤트** |
| 위험도 시각 단서 | 있음 | 있음 | - | 없음 | **색상 코딩** |

### 1-2. 구조적 한계점 (코드 기반)

| 한계 | 기획적 영향 |
|------|------------|
| **확률 기반 Outcome 불가** | 단일 결과만 가능 → 도박/리스크 이벤트 설계 불가 |
| **분기(연쇄) 구조 미지원** | 선택이 곧바로 결과로 직결 → 스토리텔링·복합 딜레마 불가 |
| **영구 스탯 증가 필드 부재** | 3개 이벤트(AncientLibrary/FallenKnight/CursedAltar)가 "영구 증가" 텍스트만 표시 → **플레이어 기대 배신** (기획-구현 불일치 버그) |
| **Weight / 조건부 분기 없음** | 완전 균등 랜덤 → 스테이지 분위기와 무관한 이벤트 등장 |
| **ChoiceDescription UI 미표시** | 데이터에 존재하지만 버튼 텍스트만 노출 → 위험 감수 판단 정보 부족 |
| **스테이지 테마 분리 안 됨** | 12개 테마(잿빛숲/크림슨예배당 등)가 있는데 이벤트 풀은 공통 1개 |
| **outcome.ResultText += 오염 버그** | 원본 에셋 오염 → 재사용 시 텍스트 누적 |

---

## 2. 기획 비전 (Design Pillars)

이벤트 파트가 플레이어에게 전달할 **3가지 감정**을 설계 기둥으로 삼는다.

### Pillar 1: 긴장감 (Tension)
"이 선택이 런을 망칠 수도 있다"는 위험-보상 딜레마. 현재는 정답이 명확하여 긴장 제로.

### Pillar 2: 서사 (Narrative)
스테이지 테마와 연결된 단편 스토리. 12개 테마 각각의 분위기를 이벤트로 전달.

### Pillar 3: 기억 (Memorability)
"그때 그 저주받은 제단에서 흥했던 런" 같은 회상. 영구 강화/저주가 런 정체성을 만듦.

---

## 3. 단계별 로드맵 (Phase E1 ~ E4)

| Phase | 작업 | 기간 | 효과 |
|-------|------|------|------|
| **E1 버그 수정 + 데이터 구조 확장** | EventData/EventManager/EventUI 코드 개선 | 즉시 | D+ → C |
| **E2 공통 이벤트 2.5배 확충** | 도박/저주/영구/스토리/조건부 15개 신규 | 1일 | C → C+ |
| **E3 스테이지 테마 전용 이벤트** | 12테마 × 2개 = 24개 신규 + 테마 연결 | 2일 | C+ → B |
| **E4 연출 + 밸런스 검증** | ChoiceDescription/위험도 색상/시뮬레이터 연계 | 1일 | B → B+ |

---

## 4. Phase E1: 데이터 구조 확장

### 4-1. EventOutcome 필드 확장

```csharp
[System.Serializable]
public class EventOutcome
{
    [TextArea(2, 3)] public string ResultText;
    public int GoldChange;
    [Range(-100, 100)] public int HPPercentChange;
    public bool GiveRandomSkill;
    public bool GiveRandomItem;

    // === E1: 신규 필드 ===
    [Header("영구 강화 (런 내 영구)")]
    public int PermanentAtkBonus;     // 파티 전원 ATK 영구 +
    public int PermanentDefBonus;     // 파티 전원 DEF 영구 +
    public int RerollTokensBonus;     // GameRunState.RerollTokens +=

    [Header("확률 기반 결과 (선택)")]
    [Tooltip("비어있으면 단일 결과. 있으면 가중치 기반 추첨")]
    public List<EventOutcome> RandomOutcomes = new();
    public List<float> OutcomeWeights = new();

    [Header("연쇄 (분기) 이벤트")]
    [Tooltip("비어있으면 종료. 있으면 다음 이벤트 ID 로드")]
    public string NextEventId;

    // 기존 상태이상 필드 유지
    public StatusEffectType ApplyStatusEffect;
    public int StatusEffectDuration;
    public int StatusEffectValue;
}
```

**하위 호환**: 새 필드 default 값으로 기존 10개 에셋 100% 호환

### 4-2. EventChoice 조건부 선택지

```csharp
[System.Serializable]
public class EventChoice
{
    public string ChoiceText;
    [TextArea(2, 3)] public string ChoiceDescription;

    // === E1: 조건부 (비활성화/회색 처리) ===
    [Header("등장 조건 (0=제한 없음)")]
    public int MinGoldRequired;
    [Range(0f, 1f)] public float MinPartyHPPercent;   // 0=제한 없음
    public int RequiresAliveMembers;                   // 0=제한 없음

    public EventOutcome Outcome;
}
```

### 4-3. EventData Weight + 테마 연계

```csharp
public class EventData : ScriptableObject
{
    // 기존 필드...

    // === E1: 신규 ===
    [Header("등장 제어")]
    [Tooltip("높을수록 자주 등장. 기본 10")]
    public int Weight = 10;
    [Tooltip("비어있으면 공통. 있으면 해당 테마에서만 등장")]
    public string ExclusiveThemeId = "";   // 예: "S1_GreyForest"
}
```

---

## 5. Phase E2: 공통 이벤트 15개 신규 컨셉

### 5-1. 도박/확률 이벤트 (4개)

| ID | 표시명 | 컨셉 | EV 분석 |
|----|--------|------|---------|
| `Event_GoldenIdol` | 황금 우상 | 60%: 80G / 40%: 영구 ATK -2 | EV +40G价值, 분산 large |
| `Event_DiceGame` | 도박꾼의 주사위 | 33%: 유물 / 33%: 50G / 33%: HP -25% | EV 약 50G 상당 |
| `Event_CursedFountain` | 저주받은 분수 | 마신다: 50% 풀회복 / 50% 독 5턴 | 도박 힐 |
| `Event_AmnesiaHerb` | 망각의 허브 | 50%: 리롤 토큰 +2 / 50%: 증강 1개 랜덤 삭제 | 메타 도박 |

### 5-2. 저주 이벤트 (3개) — 강력한 보상 + 대가

| ID | 표시명 | 컨셉 |
|----|--------|------|
| `Event_BloodPact` | 피의 계약 | 영구 ATK +8 / 화상 5턴 (파티 전원) |
| `Event_ShadowMark` | 그림자의 각인 | 영구 DEF +5 / 파티 HP -20% |
| `Event_FrozenHeart` | 얼어붙은 심장 | 영혼 +1 (있을 경우) / 빙결 3턴 |

### 5-3. 영구 강화 투자 (3개)

| ID | 표시명 | 컨셉 |
|----|--------|------|
| `Event_TrainingDummy` | 훈련용 허수아비 | 골드 -60 / 파티 영구 ATK +2 |
| `Event_MeditationPeak` | 명상의 봉우리 | HP -20% / 파티 영구 DEF +3 |
| `Event_AncientBlacksmith` | 고대 대장장이 | 골드 -80 / 파티 영구 ATK +1, DEF +1 |

### 5-4. 스토리/세계관 (Story 타입 3개)

| ID | 표시명 | 컨셉 |
|----|--------|------|
| `Event_FallenHeroLog` | 쓰러진 영웅의 일지 | 읽는다: 다음 보스 보상 +50% (구현 시 EventManager 플래그) / 태운다: 30G |
| `Event_VisionOfPast` | 과거의 환영 | (스토리 텍스트) / 선택: 다음 치명적 데미지 1회 방어 버프 |
| `Event_MysteriousLetter` | 정체불명의 편지 | 받는다: 20G / 버린다: HP 5% 회복 |

### 5-5. 상태 기반 조건부 (2개)

| ID | 표시명 | 조건 | 컨셉 |
|----|--------|------|------|
| `Event_DesperateGamble` | 절박한 도박 | 파티 HP < 40% | 30%: 풀회복 / 70%: 사망 1명 |
| `Event_RichMerchant` | 부유한 상인 | 골드 150+ | 레어 유물 150G 고정 구매 |

---

## 6. Phase E3: 스테이지 테마 전용 이벤트 (24개)

> 각 테마의 `themeKeywords`와 `description`을 기반으로 **그 테마에서만 등장**하는 이벤트 설계.
> `EventData.ExclusiveThemeId` 필드로 등장 제어.

### 6-1. Stage 1 — 튜토리얼 (학습: AP 관리, 타겟 우선순위)

#### 🌲 S1_GreyForest (잿빛 숲) — 키워드: 재생, 도적

| ID | 표시명 | 컨셉 | 딜레마 |
|----|--------|------|--------|
| `Event_GF_MistMerchant` | 안개 속 상인 | 정체불명 상인 | 저렴 유물 (40G) / 30% 확률 저주받은 유물 |
| `Event_GF_RegenSpring` | 재생의 샘 | 오래된 정령의 샘 | HP 30% 회복 + 재생 3턴 / HP 10% 회복만 |

**컨셉**: "재생" 키워드를 이벤트에서도 체감. 안개의 불확실성이 이 스테이지의 정체성.

#### ❄️ S1_FrostedPass (서리 고개) — 키워드: 둔화, 빙결

| ID | 표시명 | 컨셉 | 딜레마 |
|----|--------|------|--------|
| `Event_FP_FrozenTraveler` | 얼어붙은 여행자 | 동사 직전의 여행자 | 도와준다: HP -10%, 영구 DEF +1 / 무시: 25G |
| `Event_FP_IceShardTrade` | 얼음 조각 거래 | 빙석 골렘 조각 | 산다: 50G / 녹인다: 힐 20% (빙결 1턴) |

**컨셉**: "둔화/빙결" 기믹을 저렴한 비용의 일시적 상태이상으로 체험.

#### 🏜️ S1_SunscorchedPlains (모래 평원) — 키워드: 은폐, 회피

| ID | 표시명 | 컨셉 | 딜레마 |
|----|--------|------|--------|
| `Event_SP_Mirage` | 신기루 | 잠깐 나타난 오아시스 | 다가간다: 50% 풀회복 / 50% HP -20% |
| `Event_SP_SandstormShelter` | 모래폭풍 대피소 | 숨어있는 사막의 암살자 | 숨는다: 30G / 싸운다: 50% 승리 시 70G |

**컨셉**: "은폐/회피" = 예측 불가. 모든 결과가 확률 기반.

---

### 6-2. Stage 2 — 체력 관리 (학습: 정화, 치명타, 순차 처결)

#### 🩸 S2_CrimsonChapel (혈련 예배당) — 키워드: 흡혈, 부활

| ID | 표시명 | 컨셉 | 딜레마 |
|----|--------|------|--------|
| `Event_CC_BloodFountain` | 피의 분수 | 붉은 분수 | 마신다: 50% 영구 ATK +3 / 50% 매혹 3턴 |
| `Event_CC_VampireDeal` | 뱀파이어의 거래 | 로드와 계약 | 골드 -70, 영구 ATK +4 / 거절: HP -15% (강제) |

**컨셉**: "흡혈" = HP가 화폐. 강력하지만 위험한 거래.

#### ☠️ S2_RotbloomBog (부패 늪) — 키워드: 독, 전염

| ID | 표시명 | 컨셉 | 딜레마 |
|----|--------|------|--------|
| `Event_RB_PlagueDoctor` | 역병 의사 | 독 전문가 | 해독제 구매 30G (독/화상 정화) / 무료 50% 확률 역병 5턴 |
| `Event_RB_Bogwitch` | 늪 마녀 | 정체불명 마녀 | 유물 받기: 독 5턴 / 골드 50G 받기: HP -10% |

**컨셉**: "독" = 도트 데미지로 장기전 페널티 체감.

#### ⚰️ S2_RuinedTemple (유적 잔해) — 키워드: 언데드, 저주

| ID | 표시명 | 컨셉 | 딜레마 |
|----|--------|------|--------|
| `Event_RT_CursedSarcophagus` | 저주받은 석관 | 고대 석관 | 연다: 50% 유물 / 50% 저주(AttackDown 5턴) |
| `Event_RT_LichLibrary` | 리치의 도서관 | 금서의 냄새 | 읽는다: 영구 ATK +2 / 화상 3턴 / 태운다: 40G |

**컨셉**: "저주" = 강력한 디버프로 정화 가치 체감.

---

### 6-3. Stage 3 — 자원 압박 (학습: 쉴드 활용, 다중 타겟)

#### 🌊 S3_AbyssalTrench (심연 해구) — 키워드: 흡수, 속박

| ID | 표시명 | 컨셉 | 딜레마 |
|----|--------|------|--------|
| `Event_AT_DrownedChest` | 익사자의 상자 | 심해 상자 | 연다: 쉴드 30 + 60G / 속박 3턴 |
| `Event_AT_KrakenTentacle` | 크라케의 촉수 | 떨어진 촉수 | 먹는다: HP -25%, 영구 ATK +4 / 무시: 35G |

**컨셉**: "흡수/속박" = 자원을 뺏기는 긴장. 쉴드의 가치 체감.

#### ⚡ S3_StormpeakSummit (번개 봉우리) — 키워드: 기절, 연쇄

| ID | 표시명 | 컨셉 | 딜레마 |
|----|--------|------|--------|
| `Event_ST_StruckByLightning` | 벼락 맞은 무덤 | 벼락 맞은 석상 | 만진다: 50% 영구 ATK +5 / 50% 기절 2턴 (전원) |
| `Event_ST_StormRitual` | 폭풍 의식 | 고대 폭풍 소환사 | 참여: HP -30%, 리롤 토큰 +3 / 구경: 25G |

**컨셉**: "기절" = 행동 차단의 공포. 리롤의 가치 체감.

#### 🌑 S3_ShadowsGlade (그림자 골짜기) — 키워드: 은신, 회피

| ID | 표시명 | 컨셉 | 딜레마 |
|----|--------|------|--------|
| `Event_SG_FadeEcho` | 페이드의 메아리 | 그림자 존재 | 받아들인다: 40G + AttackDown 3턴 / 거부: HP -15% |
| `Event_SG_BlindSeer` | 눈 먼 예언자 | 미래를 보는 자 | 골드 -40, 다음 보스전 보상 +50% / 무시: 10G |

**컨셉**: "은신/예측 불가" = 정보 비대칭. 장기 투자 보상.

---

### 6-4. Stage 4 — 클라이맥스 (학습: 통합 운영, 페이즈 대비)

#### 🔥 S4_EmberThrone (불꽃왕좌) — 키워드: 화염, 폭발

| ID | 표시명 | 컨셉 | 딜레마 |
|----|--------|------|--------|
| `Event_ET_SalamanderPact` | 살라만더의 계약 | 화염 정령 | 영구 ATK +6 / 화상 5턴 (전원) |
| `Event_ET_EmberForge` | 잔불의 대장간 | 마왕성 대장장이 | 골드 -100, 영구 ATK +3 + DEF +2 |

**컨셉**: "화염" = 최종 스테이지답게 강도 높은 거래. 보스전 전 투자 지점.

#### 🧊 S4_EternalTundra (영원동토) — 키워드: 빙결, 셧다운

| ID | 표시명 | 컨셉 | 딜레마 |
|----|--------|------|--------|
| `Event_ETu_FrozenHero` | 빙결된 영웅 | 얼어붙은 전사 | 해동: HP -25%, 영구 ATK +5 / 흡수: HP +30% |
| `Event_ETu_IceQueenRiddle` | 빙결 여왕의 수수께끼 | 지능 도박 | 정답: 유물 / 오답: 빙결 3턴 |

**컨셉**: "빙결" = 행동 봉쇄의 극치. 보스전 직전 마지막 도박.

#### 👹 S4_DemonCitadel (마왕성 심장) — 키워드: 소환, 다중 페이즈

| ID | 표시명 | 컨셉 | 딜레마 |
|----|--------|------|--------|
| `Event_DC_DemonContract` | 악마의 계약 | 대마왕의 사자 | 영구 ATK +8 / 영구 DEF -3 (런 종료까지) |
| `Event_DC_LegionAmbush` | 군단의 매복 | 악마 군단 | 싸운다: 50% 대유물 / 50% 파티 HP -40% / 도망: 골드 -50 |

**컨셉**: "소환/다중 페이즈" = 최종 보스전 직전 최대 긴장. 영혼 보상 가능.

---

## 7. Phase E4: UI/연출 개선

| 연출 요소 | 구현 방법 | 우선순위 |
|----------|----------|---------|
| ChoiceDescription 표시 | 버튼 하위 TMP 작은 텍스트 | **높음** |
| 위험도 색상 코딩 | 버튼 배경: 안전=회색, 도박=노랑, 위험=빨강 (Outcome 내용 기반 자동 분류) | **높음** |
| 결과 플로팅 텍스트 | FloatingTextUI 재사용 (+30G / -15% HP 등) | 높음 |
| EventType 아이콘 | 5종 (Story/Treasure/Trap/NPC/Shrine) - GUI Pro-CasualGame | 중간 |
| 결과 토스트 | ToastUI.Show("VampireFang 획득!") | 중간 |
| 일러스트 배경 | StageThemeData.backgroundImage (추후 연동) | 낮음 |

### 위험도 자동 분류 알고리즘 (기획)

```
안전 (회색): GoldChange >= 0 AND HPPercentChange >= 0
도박 (노랑): RandomOutcomes 비어있지 않음
위험 (빨강): HPPercentChange < -15 OR PermanentDebuff 있음
일반 (기본색): 그 외
```

---

## 8. 밸런스 설계 가이드라인

### 8-1. 이벤트 1회 방문 기대 가치 (EV)

| 카테고리 | EV | 비교 |
|---------|-----|------|
| 일반 전투 승리 보상 | ~30G + 가끔 증강 | 기준 |
| 상점 방문 | 70G로 유물/증강 | 기준 |
| **안전 이벤트** | 30~50G 상당 | 전투보다 약간 낮게 |
| **도박 이벤트** | EV 60G / 분산 large | 운 의존 |
| **위험 이벤트** | 영구 강화 (≈ 150G 가치) | 전투 3~4회분 |
| **저주 이벤트** | 영구 강화 + 영구 디버프 | 런 정체성 |
| **테마 전용** | 일반보다 약간 높음 (테마 학습 보상) | 테마 학습 보상 |

### 8-2. 시뮬레이터 연계 (필수)

- `BalanceSimulator.Run.cs`에 이벤트 효과 반영 로직 추가
- Full Run 100회 시뮬레이션에서 이벤트로 인한 HP 변동/영구 강화/저주 효과가 승률에 미치는 영향 측정
- **목표**: 테마 전용 이벤트 방문 런이 미방문 런보다 승률 +5~10%

---

## 9. 구현 우선순위 (Phase E1 → E4)

| 단계 | 파일 | 작업량 |
|------|------|--------|
| **E1-1** | `EventData.cs` | 필드 확장 (PermanentAtk/Def, RandomOutcomes, NextEventId, Conditions, Weight, ExclusiveThemeId) |
| **E1-2** | `EventManager.cs` | 영구 강화 처리, 확률 Outcome 추첨, ResultText 오염 방지 |
| **E1-3** | `EventUI.cs` | ChoiceDescription 표시, 조건부 비활성화 |
| **E2-1** | `DataGenerator.Events.cs` | 기존 10개 수정 + 신규 15개 |
| **E3-1** | `StageThemeData.cs` | `themeEvents` 필드 추가 |
| **E3-2** | `DataGenerator.Stages.cs` | 테마별 이벤트 24개 생성 |
| **E3-3** | `MapSceneSetup.Nodes.cs` | `OpenEvent()` 테마별 이벤트 풀 사용 |
| **E4-1** | `EventUI.cs` | 위험도 색상, 결과 플로팅 텍스트 |
| **E4-2** | `EventManagerTests.cs` | 신규 단위 테스트 |

---

## 10. 검증 체크리스트 (Definition of Done)

### 10-1. 기능 검증
- [ ] 영구 스탯 증가 3개 기존 이벤트 정상 작동 (AncientLibrary/FallenKnight/CursedAltar)
- [ ] 확률 기반 Outcome 가중치 정규화, 시드 재현 가능
- [ ] 조건부 선택지 회색 처리/클릭 차단
- [ ] 연쇄 이벤트 NextEventId 정상 로드
- [ ] 12개 테마별 이벤트 풀 분리 동작

### 10-2. 기획 검증
- [ ] 이벤트 총 수 49개 이상 (공통 25 + 테마 24)
- [ ] 상태이상/저주 이벤트 8개 이상
- [ ] Story 타입 3개 이상
- [ ] 12개 테마 각각 전용 이벤트 2개
- [ ] ChoiceDescription 전 이벤트 표시
- [ ] 위험도 색상 코딩 전 적용

### 10-3. 밸런스 검증
- [ ] BalanceSimulator에 이벤트 효과 반영
- [ ] Full Run 100회 결과: 테마 이벤트 방문 런 승률 +5% 이상
- [ ] 컴파일 0에러, 기존 88 테스트 회귀 없음

---

## 11. 리스크 및 완화책

| 리스크 | 가능성 | 완화 |
|--------|--------|------|
| Outcome 구조 변경으로 기존 에셋 호환 깨짐 | 중 | 새 필드 default 값으로 하위 호환 보장 |
| 테마별 이벤트 풀 분리 후 특정 테마 사망률 급증 | 중 | 시뮬레이터로 사전 검증 |
| 연쇄 이벤트 무한 루프 | 낮 | NextEventId 깊이 제한 (MAX 3) + 순환 감지 |
| 일러스트 에셋 부족 | 높음 | StageThemeData.backgroundImage 재사용 + 텍스트 위주 분위기 연출로 우회 |
| 빌드 시 DataGenerator 메뉴 재실행 필요 | 중 | 안내 문서화 (09.Docs/WorkLog) |

---

## 12. 부록: 이벤트 분류 요약

### 12-1. EventType 분류 (49개 예상)

| Type | 기존 | 신규 공통 | 신규 테마 | 합계 |
|------|------|----------|----------|------|
| Story | 0 | 3 | 0 | 3 |
| Treasure | 2 | 4 | 6 | 12 |
| Trap | 1 | 2 | 9 | 12 |
| NPC | 3 | 4 | 3 | 10 |
| Shrine | 4 | 2 | 6 | 12 |

### 12-2. 효과 타입 분포 목표

| 효과 | 현재 | 목표 |
|------|------|------|
| 골드 변화 | 8 | 25+ |
| HP 변화 | 10 | 30+ |
| 영구 ATK 증가 | 0 (미구현) | 12+ |
| 영구 DEF 증가 | 0 (미구현) | 8+ |
| 상태이상 부여 | 0 | 12+ |
| 확률 기반 | 0 | 8+ |
| 연쇄 이벤트 | 0 | 2~3 |
| 리롤 토큰 | 0 | 3+ |

---

## 관련 문서
- `Assets/09.Docs/EventSceneQualityAnalysis.md` (분석 보고서)
- `Assets/09.Docs/StageDesign.md` (12 테마 정의)
- `Assets/09.Docs/GameDesign.md` 섹션 8 (원본 기획서)
- `Assets/02.Scripts/Event/EventData.cs`
- `Assets/02.Scripts/Event/EventManager.cs`
- `Assets/02.Scripts/UI/Event/EventUI.cs`
- `Assets/02.Scripts/Editor/DataGenerator.Events.cs`
- `Assets/02.Scripts/Map/StageThemeData.cs`
