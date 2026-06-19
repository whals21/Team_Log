# 이벤트 씬 퀄리티 분석 보고서

> **분석 일자**: 2026-06-19
> **분석 대상**: 이벤트 시스템(EventData / EventManager / EventUI / DataGenerator.Events)
> **비교 기준**: Slay the Spire, Inscryption, Balatro 등 상용 턴제 로그라이크
> **목적**: 현재 이벤트 씬의 퀄리티 수준을 객관적으로 평가하고 개선 우선순위를 도출

---

## 핵심 결론

**현재 퀄리티는 '프로토타입 기본기' 수준으로, 상용 턴제 로그라이크와 비교하면 현저히 부족합니다.**
시스템 뼈대는 갖췄으나 콘텐츠 깊이와 연출이 크게 부족하며, 일부는 기획-구현 불일치 버그입니다.

---

## 정량 비교 (타 게임 대비)

| 항목 | StS | Inscryption | Balatro | **Team Log** | 평가 |
|------|-----|-------------|---------|--------------|------|
| 이벤트 수 | ~70+ | ~40+ | ~30+ | **10** | 🔴 10~30% 수준 |
| 분기/연쇄 구조 | 일부 | 핵심 특징 | 없음 | **없음** | 🟡 평균 이하 |
| 확률 기반 결과 | 있음 | 있음 | 없음 | **불가** | 🔴 구조적 한계 |
| 부정 결과 다양성 | 매우 높음 | 높음 | 낮음 | **매우 낮음** | 🔴 HP/골드만 |
| 상태이상/저주 | 있음 | 있음 | - | **필드만 있고 0건 사용** | 🔴 기획-구현 불일치 |
| 이벤트 일러스트 | 있음 | 핵심 | - | **없음** | 🔴 텍스트 전부 |
| 결과 시각 피드백 | 애니메이션 | 카드 연출 | - | **텍스트만** | 🔴 |

---

## 시스템 구조 분석

### 이벤트 데이터 구조 (`EventData.cs`, 71줄)

3단계 직렬화 클래스 구조:

```
EventData (ScriptableObject)
 +-- EventName: string              (이벤트 이름, 한국어)
 +-- Description: string            (상황 설명, TextArea(3,6))
 +-- EventType: enum                (5종: Story, Treasure, Trap, NPC, Shrine)
 +-- Choices: List<EventChoice>     (선택지 목록)
      +-- ChoiceText: string         (버튼에 표시될 텍스트)
      +-- ChoiceDescription: string  (선택지 상세 설명 - UI에 미표시)
      +-- Outcome: EventOutcome      (결과 객체)
           +-- ResultText: string            (결과 설명)
           +-- GoldChange: int               (골드 +/-)
           +-- HPPercentChange: int           (파티 전체 HP 비율 -100~100)
           +-- GiveRandomSkill: bool          (랜덤 증강 획득)
           +-- GiveRandomItem: bool           (랜덤 유물 획득)
           +-- ApplyStatusEffect: StatusEffectType (저주/상태이상)
           +-- StatusEffectDuration: int
           +-- StatusEffectValue: int
```

**장점**:
- ScriptableObject 기반으로 에디터에서 직접 수정 가능
- 이벤트 선택지가 2~3개로 명확한 딜레마 구조
- 결과에 골드/HP/증강/유물/상태이상 5가지 효과 타입을 지원

**한계**:
- **분기(branch) 구조 미지원**: 선택 결과가 새로운 이벤트로 연결되는 기능 없음
- **확률 기반 결과 불가**: "50% 확률로 피해 없음/피해" 구현 불가 (기획서 8.2 명시됨)
- **파티 상태 조건부 분기 없음**: 현재 HP/골드/파티원 수에 따른 선택지 출현/비활성화 없음
- **영구 스탯 증가 필드 없음**: 데이터에는 결과 텍스트가 있으나 효과 미적용
- **Weight 필드 미구현**: 기획서 8.1 명시됨
- EventType이 `Story`로 분류된 이벤트가 실제로는 0개 생성됨

### EventManager (`EventManager.cs`, 108줄)

순수 C# 클래스 (MonoBehaviour 아님).

**이벤트 선택 로직**: 없음. 이벤트 선택은 `MapSceneSetup.Nodes.cs`의 `OpenEvent()`에서 완전 균등 랜덤으로 처리:
```csharp
int index = UnityEngine.Random.Range(0, _testEvents.Length);
```

- 가중치 없음, 스테이지/층별 필터링 없음, EventType 기반 필터링 없음

**효과 적용** (`ProcessChoice`):

| 효과 | 처리 방식 | 비고 |
|------|-----------|------|
| 골드 +/- | `runState.AddGold()` / `SpendGold()` | 정상 |
| HP 비율 변화 | 파티 전원 `Heal()` / `TakeDamage()` | 사망 처리 명시 없음 |
| 랜덤 증강 | `PeekRandomAugment()` → 첫 생존자 첫 슬롯 | 플레이어 선택 불가 |
| 랜덤 유물 | `PeekRandomRelic()` → `AcquireRelic()` | 정상 |
| 상태이상 | 파티 전원 `ApplyEffect()` | 정상 |

**취약점**:
- 증강 부착이 첫 생존자 첫 빈 슬롯 고정 (플레이어 선택 불가)
- HP 비율 데미지 시 사망 체크 없음
- outcome.ResultText를 직접 수정(`+=`)하여 동일 에셋 재사용 시 텍스트 누적 위험

### EventUI (`EventUI.cs`, 129줄)

7개 SerializeField로 구성된 깔끔한 2단계 UI (선택 → 결과).

**사용자 경험 흐름**:
```
1. ShowEvent() → FadeIn + 제목/설명/선택지 버튼 생성
2. 플레이어 클릭 → OnChoiceSelected() → 효과 적용 + 결과 패널
3. 확인 클릭 → HideAndNotify() (콜백 먼저 → FadeOut)
```

코루틴 + SetActive(false) 함정을 정확히 회피 (콜백을 FadeOut 이전에 실행).

**시각적 연출 분석**:

| 연출 요소 | 구현 여부 |
|-----------|-----------|
| FadeIn/FadeOut | ✅ CanvasGroup alpha |
| 배경 오버레이 | ✅ 반투명 |
| 버튼 클릭음/확인음 | ✅ 2종 |
| 이벤트 이미지/일러스트 | ❌ |
| 선택지 호버 애니메이션 | ❌ |
| 결과 플로팅 텍스트 | ❌ |
| 토스트 알림 | ❌ |
| 이벤트 전용 VFX | ❌ |
| 이벤트 타입별 아이콘 | ❌ |
| 타이핑 효과 | ❌ |
| 위험도 표시 | ❌ |

### DataGenerator.Events.cs (331줄)

**생성된 이벤트: 10개**

| # | 파일명 | 표시명 | 타입 | 선택지 | 딜레마 |
|---|--------|--------|------|--------|--------|
| 1 | Event_AbandonedChest | 버려진 상자 | Treasure | 2 | 안전(+40G) vs 위험(+25G, -15%HP) |
| 2 | Event_MysteriousShrine | 신비한 신전 | Shrine | 3 | HP회복 / 골드 지불+대HP회복 / 무시 |
| 3 | Event_WoundedTraveler | 부상당한 여행자 | NPC | 3 | 치유(+30G, -5%HP) / 장비증정(랜덤유물) / 무시 |
| 4 | Event_SpiderWeb | 거미줄 함정 | Trap | 2 | 불태우기(+35G) / 우회(아무일 없음) |
| 5 | Event_AncientLibrary | 고대 도서관 | Shrine | 2 | 책 읽기(ATK영구증가-미구현) / 판매(+50G) |
| 6 | Event_FairySpring | 요정의 샘 | Shrine | 2 | 마신다(+40%HP) / 동전 던지기(+30G) |
| 7 | Event_FallenKnight | 쓰러진 기사 | NPC | 2 | 치유(DEF영구증가-미구현, -5%HP) / 무시 |
| 8 | Event_TreasureGoblin | 보물 고블린 | Treasure | 2 | 추격(+60G, -10%HP) / 무시 |
| 9 | Event_CursedAltar | 저주받은 제단 | Shrine | 2 | 기도(ATK영구증가-미구현, -20%HP) / 파괴(+45G) |
| 10 | Event_TravelingMerchant | 상인 대행 | NPC | 3 | 회복약 구매(-20G, +25%HP) / 정보 구매(-15G) / 거절 |

**EventType 분포**: Treasure 2 / Trap 1 / NPC 3 / Shrine 4 / **Story 0**

**딜레마 다양성 분석** (선택지 기준, "무시" 제외):
- 골드 획득: 8개 선택지
- HP 회복: 5개 선택지
- HP 손실: 5개 선택지
- 골드 소모: 3개 선택지
- 랜덤 유물: 1개 선택지
- 랜덤 증강: 0개 (GiveRandomSkill=true인 선택지 없음)
- 영구 스탯 증가: 3개 (모두 미구현)
- 상태이상 부여: 0개

### 맵 노드 처리 흐름

```
MapView (노드 클릭)
  → MapSceneSetup.OnNodeClicked
    → switch(Event) → OpenEvent()
      → Random.Range(0, _testEvents.Length)
      → EventUI.ShowEvent()
        → 선택 → ProcessChoice() → 결과 패널
        → 확인 → OnEventComplete() → SaveManager.Save()
```

**이벤트 노드 비율** (MapGenerator):
- Stage 1: Event 1 + Shop 1 + Rest 1 = 특수 3
- Stage 2: Event 1 + Shop 1 + Rest 1 = 특수 3
- Stage 3: Event 2 + Shop 1 + Rest 1 = 특수 4
- Stage 4: Event 2 + Shop 1 + Rest 1 = 특수 4

한 런에 방문하는 이벤트 노드: ~8개 (10개 풀에서 반복 심함)

---

## 주요 갭 (3가지 카테고리)

### 🔴 1. 명백한 버그 — 기획-구현 불일치 (즉시 수정 필요)

**3개 이벤트가 영구 스탯 증가를 표시하지만 실제 효과 미적용**:
- `Event_AncientLibrary`: "공격력이 영구히 증가했습니다" → 실제 효과 없음
- `Event_FallenKnight`: "방어력이 영구히 증가했습니다" → 실제 효과 없음
- `Event_CursedAltar`: "공격력이 영구히 증가했습니다" → 실제 효과 없음

원인: `EventOutcome` 클래스에 영구 스탯 증가 필드가 없음. `StatComponent.AddPermanentBaseAtk/Def` API는 존재하나 `EventManager.ProcessChoice`에서 호출하지 않음.

### 🔴 2. 콘텐츠 빈곤

- **이벤트 풀 10개**: 한 런에 방문하는 이벤트 노드 ~8개 → **80%가 반복 이벤트** 경험
- **상태이상/저주 이벤트 0건**: `ApplyStatusEffect` 필드가 정의되어 있지만 단 한 선택지도 사용 안 함
- **확률 기반 결과 불가**: 기획서 예시("50% 확률")가 구현 불가능 (Outcome 구조가 단일 결과)
- **ChoiceDescription 필드 미사용**: 선택지 상세 설명이 데이터에 있으나 UI에 표시 안 함 → 플레이어가 위험을 사전에 파악 불가

### 🟡 3. 연출 부재 (UI/연출 측면)

EventUI의 전체 연출 요소:
- ✅ FadeIn/FadeOut (CanvasGroup alpha)
- ✅ 버튼 클릭음/확인음 (2종)
- ❌ 이벤트 일러스트/이미지
- ❌ 이벤트 타입별 아이콘 (Story/Treasure/Trap/NPC/Shrine)
- ❌ 선택지 호버 애니메이션
- ❌ 결과 플로팅 텍스트 (골드/HP 변화)
- ❌ 결과 토스트 알림
- ❌ 이벤트 전용 VFX
- ❌ 위험도 표시 (안전/위험 색상 구분)

---

## 사용자 경험 관점

StS/Inscryption의 이벤트는 **"잘 읽힌 단편 소설 + 시각적 몰입"**을 제공합니다. 일러스트 한 장 + 분위기 있는 설명 텍스트 + 명확한 딜레마로 플레이어가 스토리에 감정적으로 몰입할 수 있습니다.

반면 Team Log의 이벤트는:
- 텍스트 3줄 + 버튼 2~3개가 전부
- 10회 반복 후에는 "어떤 선택이 좋은지" 외워버리는 정답 게임이 됨
- 위험 감수(reward vs risk)의 긴장감이 약함 — 정답이 명확
- 스테이지 분위기(Forest/Chapel/Trench)와 이벤트 테마가 무관

---

## 개선 우선순위 (비용 대비 효과)

### 🥇 1순위: 버그 수정 + 콘텐츠 확충 (저비용 고효과)
1. **영구 스탯 증가 구현** — EventOutcome 필드 2개 + ProcessChoice 10줄 추가
2. **상태이상/저주 이벤트 3~5개 추가** — 기존 `ApplyStatusEffect` 필드 활용
3. **ChoiceDescription UI 표시** — EventUI에 1줄 추가, 위험도 시각적 단서 제공
4. **이벤트 풀 10→25개 확대** — DataGenerator.Events.cs 패턴 그대로 복제

### 🥈 2순위: 연출 개선 (중비용)
5. **EventType 아이콘 시스템** — 5종 아이콘 에셋 매핑 (GUI Pro-CasualGame 활용)
6. **결과 플로팅 텍스트** — FloatingTextUI 재사용 (골드 +/- 표시)
7. **위험도 색상 코딩** — 빨강(위험)/회색(안전) 버튼 배경

### 🥉 3순위: 구조적 개선 (고비용)
8. **확률 기반 Outcome** — EventOutcome에 `Outcomes[]` + `Weights[]` 배열 도입
9. **스테이지별 이벤트 풀 분리** — StageThemeData에 이벤트 리스트 임베드
10. **연쇄 이벤트** — EventOutcome에 `NextEventId` 필드 (데이터 구조 대폭 수정)

---

## 최종 평가

| 평가 항목 | 점수 | 코멘트 |
|----------|------|--------|
| 시스템 아키텍처 | **B+** | ScriptableObject + 순수 C# + UI 분리 규칙 잘 준수 |
| 코드 품질 | **A-** | 71+108+129줄로 적절, 코루틴 함정 회피 패턴 정확 |
| 콘텐츠 깊이 | **D** | 10개 이벤트, 상태이상 0건, 확률 불가, 반복 심함 |
| UI 연출 | **D-** | 텍스트 + 버튼 리스트가 전부, 일러스트/아이콘/VFX 전무 |
| 기획-구현 일치도 | **C** | 영구 스탯 3건 미구현, ChoiceDescription 미사용 |
| **종합 퀄리티** | **D+** | **상용 출시 기준 부적합. 프로토타입 수준.** |

**한줄평**: 뼈대는 튼튼하나 살이 없는 상태. 1순위 버그 수정 + 콘텐츠 2배 확충만으로도 C+ 등급까지 올릴 수 있음. StS/Balatro급 딜레마 경험을 제공하려면 연출 보강과 확률 기반 Outcome 구조 개편이 필수적입니다.

---

## 관련 문서

- `Assets/09.Docs/GameDesign.md` 섹션 8 (이벤트 기획서)
- `Assets/09.Docs/GapAnalysis.md` (기존에 이벤트 부족을 스스로 지적한 문서)
- `Assets/02.Scripts/Event/EventData.cs`
- `Assets/02.Scripts/Event/EventManager.cs`
- `Assets/02.Scripts/UI/Event/EventUI.cs`
- `Assets/02.Scripts/Editor/DataGenerator.Events.cs`
- `Assets/02.Scripts/UI/Map/MapSceneSetup.Nodes.cs` (이벤트 노드 처리)
- `Assets/02.Scripts/Editor/MapSceneBuilder.Panels.cs` (BuildEventPanel)
