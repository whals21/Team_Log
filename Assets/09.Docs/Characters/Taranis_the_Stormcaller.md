# Taranis, the Stormcaller — "때리지 않는 자, 길만 만드는 자"

> **캐릭터 상세 설계 문서**. 전체 개편 개요는 [CharacterConceptReview.md](../CharacterConceptReview.md) 참조.
> **12번째 캐릭터 (신규 추가)** — Sibyl에 이은 두 번째 신규 캐릭터. "공간"이라는 새로운 자원 축 도입. Sibyl이 "미래에 투자"라면 Taranis는 **"네트워크에 투자"**.

---

## 1. 정체성 (한 문장)

Taranis는 번개의 혼돈을 도면으로 옮기는 **설계자**다. 그는 직접 때리지 않는다 — 적들 사이에 전하 네트워크를 깔아두고, 적이 스스로를 지지게 놔두며, 적의 공격마저 네트워크의 연료로 삼는다. 그는 싸우는 자가 아니라 **"길을 만드는 자"**다.

---

## 2. 배경 스토리

Taranis는 발명가의 아들이었다. 아버지는 번개를 연구했고, 아들은 그 연구실에서 자랐다. 소년은 어린 시절 폭풍을 관찰하며 깨달았다 — **번개는 무작위가 아니다. 패턴이 있다.** 패턴이 있다는 건, 그것을 예측할 수 있다는 뜻이고, 예측할 수 있다는 건, **그것이 흐를 길을 만들 수 있다**는 뜻이었다.

어른이 된 그는 자연을 다루는 마법사들이 대부분 "힘으로 제압"한다는 걸 보며 실망했다. 불을 크게 만들고, 얼음을 단단하게 만들고, 번개를 강하게 만드는 — 그건 다루는 게 아니라 **부풀리는 것**이었다. Taranis는 다른 길을 택했다. 그는 **번개가 스스로 흐르고 싶어 할 길**을 도면으로 그렸다. 적이 서 있는 자리에 전하의 씨앗을 심고, 적들이 서로를 향해 튀어오르게 두었다.

그는 전장에서 한 발짝도 움직이지 않는다. 그의 손에는 지팡이가 아닌 **도면**이 들려 있다.

**이름 어원**: Taranis = 켈트 신화의 번개 신. "하늘을 부르는 자". but Taranis라는 인간은 신이 아니다 — 그는 신의 힘을 **설계도로 옮긴 엔지니어**다.

**감정 키워드**: 인내, 정밀함, 거리두기. "나는 때리지 않는다. 단지 길을 만들 뿐이다." — 자연을 지배하지 않고 자연이 흐르게 두는, 엔지니어의 철학.

---

## 3. 왜 Team Log에서만 가능한 컨셉인가

| Team Log 특수성 | Taranis 결합 방식 |
|---------------|------------------|
| **적 intent 공개** | "보스가 누구를 얼만큼 때릴지" 미리 안다 → 쉴드(접지 장벽)로 그 공격을 흡수하며, **때린 적에게 전하를 역부여**. 적 intent 자체가 Taranis의 자원. ItB식 정보 활용의 극점 |
| **매 턴 종료 타이밍** | 자동 연쇄 발동이 "매 턴 종료"에 일어남. Team Log의 턴 사이클 구조가 아니면 불가능한 메카닉 |
| **다수전/단일전 차이** | 네트워크가 다수전에서만 폭발 → 자연스러운 "다수전 특화" 정체성. 보스전 약점이 시스템으로 강제 |
| **부활 시스템** (CC-0) | Taranis 자체 방어력 낮아 위험 → 부활로 "네트워크가 오래가는" 전투 운용 가능 |
| **Sibyl과의 대칭** | Sibyl = "시간에 투자" / Taranis = "공간에 투자". 둘 다 "지연 보상"이지만 방식이 정반대 |

---

## 4. 역할군 명시

- **주 역할군**: 광역 딜러 (Area Dealer) — 전하 네트워크로 매 턥 다수 적에게 도트 누적
- **부 역할군**: 서포터 (Supporter) — 접지 장벽으로 파티 보호 + 적 공격을 전하로 역이용

**다른 딜러/서포터와의 차별화**:
- **Ashe** = 자기 파괴적 폭딜 (Ember로 자해)
- **Lumi** = 통제+딜 (Freeze로 적 봉쇄)
- **Taranis** = 네트워크 딜+서포터 (**적을 감옥에 가두고, 적의 공격을 흡수**)
- **Sibyl** = 미래 투자 서포터 (예언 지연 발동)

Taranis는 직접 딜 스킬이 거의 없다는 점에서 **"간접 딜러"**라는 완전히 새로운 축. 마법사 3종 삼각(Ashe/Lumi/Taranis)이 딜/통제/네트워크로 완성.

---

## 5. 핵심 메카닉: Charge Network (전하 네트워크)

### 정의

> **전하(Charge)** 는 적에게 부여되는 "번개 충전 상태". 전하를 가진 적들은 **매 턴 종료 시 서로에게 번개를 쏘며 도트 데미지**를 준다. Taranis는 이 네트워크를 설계하고, 유지하고, 때로 수확한다.

### 네 가지 핵심 규칙

| 규칙 | 설명 |
|------|------|
| **부여** | 번개 스킬 적중 or 접지 장벽 쉴드 흡수 시 적에게 전하 부여. 한 적당 최대 3스택 |
| **전파** | 전하 부여 스킬 사용 시, **자동으로 다른 적 1명(전하 보유 적 우선)에게도 전하 1스택 전파** |
| **자동 연쇄** | 매 턴 종료 시, 각 전하 적이 **자신의 스택 수만큼** 다른 전하 적에게 번개를 쏨 (1스택당 도트 1, 고정값) |
| **자연 소멸** | 2턴마다 모든 적의 전하 -1스택 (느린 소멸) |

### 핵심 긴장감

> **"네트워크를 얼마나 키울까, 언제 수확할까"**

네트워크가 클수록 매 턴 도트가 폭발하지만, 자연 소멸이 있어 영원히 못 키움. 또한 네트워크를 유지하려면 적을 살려둬야 하지만, 살려둔 적은 파티를 때린다. 이게 Taranis의 핵심 딜레마.

---

## 6. 스킬 4종 — "직접 때리지 않는 4가지 방식"

> 모든 스킬이 **"네트워크 설계 / 유지 / 보호"**에 집중. Taranis가 직접 데미지를 주는 스킬은 없다시피 함.

| # | 스킬 | AP | 메커니즘 |
|---|------|----|---------|
| 1 | **Wire (와이어)** | 1 | 단일 적 전하 2스택 부여 + 전파 |
| 2 | **Branch (브랜치)** | 2 | 광역(모든 적) 전하 1스택 부여 + 전파 |
| 3 | **Grounding Field (접지 장벽)** | 2 | 파티 전체 쉴드 + 쉴드를 때린 적에게 전하 부여 |
| 4 | **Thunderstorm (뇌우)** | 3 | 광역 데미지 + 모든 적 전하 3스택(풀충전) 부여 |

---

## 7. 스킬 상세

### 1번 — Wire (와이어)

**"네트워크의 씨앗"**

단일 적에게 전하 2스택을 부여합니다. 전파 메카닉이 자동으로 다른 적 1명(전하 보유 적 우선)에게도 전하 1스택을 옮깁니다. 즉 **1AP로 두 마리를 네트워크에 편입**. Taranis 플레이의 시작점이자 매 턥 쓰는 기본 도구.

이 스킬의 핵심은 **"한 번만 써도 네트워크가 저절로 시작된다"**는 것. Taranis는 Wire 한 번으로 감옥의 씨앗을 심고, 나머지는 자동 연쇄와 자연 소멸이 알아서 갉아먹힙니다.

### 2번 — Branch (브랜치)

**"네트워크 확장"**

광역(모든 생존 적)에게 전하 1스택을 부여합니다. 전파 메카닉이 기존 전하 적을 우선하므로, Wire로 형성된 네트워크에 새 적이 매끄럽게 편입됩니다. **다수전에서 한 번에 전하를 넓게 까는 핵심 스킬**.

Branch가 빛나는 순간은 **Wire를 1-2턴 미리 써둔 뒤**입니다. 기존 네트워크가 있으면 전파가 그 적들을 우선해서 강화하고, 남은 전파가 새 적을 편입시킵니다. 즉 Branch는 단독보다 **기존 네트워크 위에서 훨씬 강력**. 이게 "네트워크에 투자할수록 다음 스킬이 강해진다"는 Taranis의 리듬을 만듭니다.

### 3번 — Grounding Field (접지 장벽)

**"적의 공격을 역이용" — Taranis만의 고유한 역발상 메카닉**

파티 전체에 쉴드를 부여합니다. **이 쉴드가 적의 공격을 흡수할 때마다, 그 공격자에게 전하 1스택이 자동 부여**됩니다. 즉 적이 아군을 때릴수록 — Taranis의 네트워크에 편입됩니다.

이 스킬이 Taranis의 영혼입니다. **적이 많이 때릴수록 네트워크가 자동 확장**됩니다. 다수전에서 빛나는 진짜 이유 — 적 수가 많으면 맞는 횟수도 많고, 그만큼 전하도 많이 부여됩니다. 적 intent를 보고 "이번 턴 적 3마리가 때린다" → Grounding Field로 전부 흡수하며 3마리 모두 네트워크에 편입. 적의 공격이 곧 Taranis의 자원입니다.

반대로 보스전에서는 보스 단일이라 전하 공급이 적음 → 자연스러운 약점. Taranis는 "다수전 특화"라는 정체성이 이 스킬 하나로 시스템 레벨에서 강제됩니다.

### 4번 — Thunderstorm (뇌우)

**"네트워크 총력전" — 단순한 궁극기**

광역 데미지 + **모든 적에게 전하 3스택(풀충전)** 부여. 복잡한 기믹은 뺐습니다. 핵심은 **한 번에 모든 적을 네트워크에 최대 강도로 편입**시킨다는 것.

이번 턥 도트는 폭발 — 모든 적이 3스택이니 매 턴 종료 시 연쇄 횟수가 극대화. 또한 다음 턥 Grounding Field의 효율도 극대화되는 셋업이기도 함 (네트워크가 이미 최대니 쉴드 흡수 전하가 중첩). 단순하지만 **"이번 턥 다수전 정리"** 한 가지 임팩트에 집중한 궁극기.

---

## 8. 자동 연쇄 메카닉 상세

**언제**: 매 턴 종료 시 (한 턴 사이클의 끝). Taranis가 스킬을 안 써도, 아군이 다른 짓을 해도, **전하가 살아있으면 무조건 발동**.

**누가 때리나**: 전하를 보유한 적 각자가 다른 전하 적에게 번개를 쏩니다. Taranis가 아니라 **적이 적을 때립니다**.

**얼마나 자주**: 각 전하 적은 **자신이 가진 전하 스택 수만큼** 번개를 쏩니다.

**도트 위력**: 1스택당 **고정값 1** (TBD — 밸런스 튜닝 시 조정, but 낮은 단위값 의도적. 네트워크가 클 때 폭발하도록 설계).

### 작동 예시

적 A(전하 2스택), B(1스택), C(1스택)가 네트워크에 있다고 합시다. Taranis는 이번 턴에 아무 스킬도 안 썼어도 —

```
[매 턴 종료 시 자동 처리]

  A (2스택) → B에게 1발, C에게 1발 발사  →  각각 도트 1
  B (1스택) → A에게 1발 발사              →  도트 1
  C (1스택) → A에게 1발 발사              →  도트 1

  결과: A는 도트 2, B는 도트 1, C는 도트 1
  총 4회 연쇄. Taranis는 가만히 있었음.

[연쇄 후]
  2턴마다 -1스택 (이번 턴은 해당 없으면 유지)
```

### 시각적으로 화면에 보이는 것

매 턴 종료 시 파란 번개 선들이 전하 적들 사이를 무작위로 오갑니다. 각 선이 도달할 때마다 작은 도트 숫자가 뜹니다. 3~4마리 네트워크가 형성된 턴에는 화면이 번개 선으로 가득찬 채 적들이 서로에게 데미지를 받습니다. **Taranis는 가만히 서서 이 꼴을 보고만 있습니다** — "간접 딜러" 정체성의 시각적 구현.

### 왜 이 방식인가 — 세 가지 이유

1. **"전하가 쌓일수록 더 잔인해진다"가 직관적**. 각 적이 스택 수만큼 쏘니, 고스택 적일수록 위험.
2. **N으로 스케일 (N²이 아님)**. 4마리 1스택씩이면 4회로 안정. but 한 놈이 3스택이면 그 놈이 3발을 쏘니 "고스택 위험 적"이 시스템으로 강조.
3. **보스전 자연 약점 강제**. 보스 1마리면 연쇄 쏠 대상 없음 → 자동 연쇄 0회. Taranis는 부역할(서포터)로 자연 전환.

---

## 9. 조건 다양성 검증 (4.5 규칙 2)

| 스킬 | 기본 조건 | 강화 조건 (후보) | 유형 |
|------|---------|----------------|------|
| Wire | 셋업 (조건 없음) | (강화 없음 — 기본 도구) | 셋업 |
| Branch | 셋업 (조건 없음) | 기존 전하 적 수만큼 위력 ↑ (권장) | 자원 (네트워크 크기) |
| Grounding Field | 셋업 (조건 없음) | 쉴드 흡수 횟수에 비례 (자동) | 행동 이력 (적 공격) |
| Thunderstorm | ⚠️ 사용 제약 (AP 3 + 자원) | — | 자원 |

**평가**:
- 스킬 자체는 셋업 중심이나, **Branch 강화 조건(네트워크 크기)과 Grounding Field의 자동 보상(적 공격 흡수)이 매 턴 다른 상황**을 만듦
- Taranis는 "스킬 자체보다 메카닉(전파/연쇄/흡수)이 퍼즐의 핵심" — Sibyl과 마찬가지로 스킬 구조보다 시스템이 플레이를 다양화

---

## 10. 핵심 플레이 루프 — "네트워크 설계 → 유지 → 수확"

### 시나리오 A: 다수전 (정석 사이클)

```
적 intent: "적 3마리가 각각 다른 아군 공격"
턴 1: Wire (적 A 전하 2스택 + 전파로 B 1스택) + Wire (적 C 전하 2스택 + 전파로 A 추가)
      → 네트워크: A 3스택, B 1스택, C 2스택
[매 턴 종료]: A가 3발, B가 1발, C가 2발 → 총 6회 연쇄 도트
턴 2: Branch (모든 적 전하 1스택 + 전파) → 네트워크 확장
      + Grounding Field (파티 쉴드) → 적 턴에 3마리 때리면 전하 3개 추가
[매 턴 종료]: 네트워크 최대 강도. 폭발적 도트
턴 3+: Thunderstorm로 마무리 or 계속 유지
```

### 시나리오 B: 보스전 (자연 약점 극복)

```
적 intent: "보스가 Healer를 30 데미지로 공격"
턴 1: Grounding Field (파티 쉴드) → 보스 때리면 전하 1 부여
      + Wire (보스 전하 2스택)
[매 턴 종료]: 보스 단일이라 연쇄 0회 (자동 연쇄 대상 없음)
→ Taranis의 딜 효율 급락. but Grounding Field로 파티 보호 + 매 턥 전하 1씩 누적
→ "딜러 → 서포터" 자연 전환. 부역할이 서포터인 이유가 여기서 설득력
```

### 시나리오 C: 혼합전 (소환수 있는 보스)

```
보스 + 소환수 2마리 (이상적)
Wire + Branch로 소환수에 전하 깔고 → 자동 연쇄로 소환수 정리
보스는 Grounding Field로 매 턥 전하 1씩 누적 → 천천히 약화
→ 다수전 + 단일전 동시 대응. Taranis가 가장 빛나는 상황
```

---

## 11. 파티 시너지 매트릭스

| 조합 | 시너지 | 핵심 |
|------|-------|------|
| **Taranis + Lumi (Cryomancer)** | ★★★★★ | Lumi Freeze로 적 봉쇄 → Taranis 네트워크 안전하게 확장. Freeze 걸린 적이 네트워크에 갇히면 "완벽한 감옥". 마법사 3종 시너지의 정점 |
| **Taranis + Ashe (Pyromancer)** | ★★★★★ | Ashe Burn 도트 + Taranis 전하 도트 = 이중 도트 누적. Ashe가 자해할수록 Taranis의 Grounding Field로 보호. "화염+번개 이중 감옥" |
| **Taranis + Sibyl (Oracle)** | ★★★★ | Sibyl "미래 투자" + Taranis "네트워크 투자" = 두 축의 지연 보상. 시너지는 아니지만 역할 충돌 없이 공존 |
| **Taranis + Duran (Warrior)** | ★★★★ | Duran ForcedTarget으로 적 고정 → Taranis가 안전하게 네트워크 설계. Duran이 맞는 동안 Taranis는 Grounding Field로 보조 |
| **Taranis + Healer** | ★★★ | Taranis 자체 방어 낮음 → Healer 힐로 유지. but 역할 부분 중복 (둘 다 서포터) |
| **Taranis + Rogue** | ★★★★ | Rogue 단일 딜 + Taranis 광역 도트. Rogue가 보스 처리, Taranis가 잡몹 정리 — 역할 분담 |
| **Taranis + Archer** | ★★ | 둘 다 다수전 특화 → 역할 중복. but Archer 단일 집중과 Taranis 광역 도트는 차별화 |
| **Taranis + Necromancer** | ★★★ | 둘 다 "지연 딜" 메카닉 → 페이스 느림 위험. but 네크로 미니언이 때리는 적에 전하 부여 가능? (TBD) |
| **Taranis + Bard** | ★★ | 둘 다 서포터. 역할 중복 |

**마법사 3종 삼각 시너지 (Ashe + Lumi + Taranis)**:
- Ashe Burn + Lumi Freeze + Taranis Charge = **3중 도트/통제/네트워크**
- 가장 이상적인 마법사 3인 파티. 하나 빠지면 다른 하나로 커버 가능

---

## 12. 12번째 캐릭터 편입 방식

### 해금 조건 (후보 — TBD)

| 옵션 | 조건 | 장단점 |
|------|------|--------|
| (A) | F3 보스 클리어 시 해금 | 다수전 특화라 F3(다수전 많음) 이후 자연스러움 |
| (B) | 총 승리 7회 달성 | Sibyl(5회)과 차별화. 늦은 해금 |
| (C) | 어센션 7 달성 시 해금 | Sibyl(어센션 5)과 준별 |
| (D) | "3개층 동안 적 30마리 처치" 업적 | 다수전 컨셉과 정합 |

**내 추천**: **(A) F3 보스 클리어 시 해금**. Taranis는 다수전 특화라 F3부터 다수전이 본격화하는 흐름과 맞음. 또한 Lumi(Cryomancer) F2 해금과 자연스러운 연속.

### 씬/데이터 통합

- **CharacterSelectUI**: 캐릭터 수 11→12. Sibyl + Taranis 둘 다 잠금 아이콘
- **CharacterTable.csv**: 신규 1행 추가 (`Char_Stormcaller`)
- **SkillTable.csv**: 신규 4행 추가 (`Storm_Wire / Branch / GroundingField / Thunderstorm`)
- **CharacterTraitData**: 신규 3종 특성 에셋 (`Trait_Compounding / ThunderGod / Superconductor`)
- **DataGenerator.Characters.cs / Traits.cs**: Taranis 생성 분기 추가
- **CharacterConceptReview.md**: 섹션 5.13 Taranis 요약 추가 (Sibyl 5.12 뒤)

---

## 13. 어센션 시나리오

### 어센션 5 (적 HP +10%, 적 ATK +5%)

- Taranis의 Grounding Field 쉴드가 조금 더 빨리 깨짐 → but 깨진 만큼 전하 부여도 많이 됨 → **자연 밸런스**
- 네트워크 유지 비용 증가 but 매 턥 도트도 증가

### 어센션 10 (적 ATK +10%, 파티 MaxHP -10%, 힐 -20%)

- 적 ATK 증가 → Grounding Field 흡수량 ↑ → 전하 부여 ↑ → **네트워크 자동 강화**
- but 파티 MaxHP 낮아져서 한 방에 위험 → Grounding Field(쉴드) 가치 ↑
- Taranis의 부역할(서포터)이 더 중요해짐

### 어센션 15 (적 ATK +10%, 보스 HP +20%, 파티 MaxHP -10%)

- 보스전에서 Taranis 딜 약함 → Grounding Field + Conduit(폐기됨, 대신 서포터 역할)로 전환
- 다수전(소환수 있는 보스)에서는 여전히 강력
- 보스 HP +20% → 매 턥 누적 도트가 더 중요 → "오래 살려두는 딜레마" 가속

---

## 14. UI/연출

### 전하 게이지 / 네트워크 시각화

- **적 캐릭터 패널**: 전하 스택 표시 (파란 번개 아이콘 + 숫자 1/2/3)
- **네트워크 연결선**: 전하 가진 적들 사이에 얇은 파란 점선 (항상 표시)
- **매 턴 종료 연출**: 점선이 두꺼운 번개 선으로 변하며 스파크, 각 적에게 도트 숫자 플로팅
- **전파 연출**: 전하 부여 시 다른 적에게 파란 잔상이 "튕겨감" (0.3초)

### Grounding Field 연출

- 파티 전체에 반투명 파란 보호막 (기존 Shield VFX에 파란 색조)
- 적이 때려서 쉴드 흡수 시 → 때린 적에게서 파란 입자가 Taranis를 거쳐 그 적으로 되돌아감 (전하 부여 시각화)
- "공격이 역류한다"는 느낌 강조

### 스킬 연출

- **Wire**: Taranis가 손가락으로 적을 가리키며 파란 선이 뻗음. 도달 시 전하 아이콘 형성
- **Branch**: Taranis가 두 손을 펼치며 모든 적에게 파란 선이 방사형으로 뻗음
- **Grounding Field**: Taranis가 지팡이를 땅에 박으면서 파란 돔이 파티를 감쌈
- **Thunderstorm**: 하늘에서 번개가 떨어지며 모든 적이 파란 전하로 충전. 각 적이 번쩍임

### Taranis 캐릭터 디자인

- **외형**: 갈색 머리카락, 안경, 손에 **도면** (지팡이 아님). 발명가/엔지니어 복장 — 가죽 앞치마, 여러 주머니
- **의상**: 짙은 파랑/은색 로브 + 공학용 도구들 (나침반, 자)
- **대기 모션**: 도면을 펼치고 한참 보다가 가끔 적을 가리키며 메모
- **시전 모션**: 도면에 손가락을 대고 "선"을 그리듯 움직이면 그 궤적이 번개로 재생
- **컨셉 컬러**: 파란 번개 (Ashe 주황/빨강, Lumi 청록/하양과 대비)

---

## 15. 특성 3종

### 기본 특성 — **Compounding (누적 가속)**

> **같은 적에게 매 턥 도트가 누적될수록 위력이 증가합니다.** 첫 턴 도트는 기본값(1)이지만, 두 번째 턴에는 +1, 세 번째에는 +2, 네 번째에는 +3... 식으로 누적 가속.

**메커니즘**:
- 매 턴 종료 시, 같은 적에게 누적된 도트 횟수를 추적
- 도트 위력 = 기본값(1) + (누적 횟수 × N) — N은 밸런스 TBD (아마 1)
- 예: 한 적이 4턴 연속 도트 받음 → 4턴째에는 기본 1 + 3 = 4 (또는 1+1+2+3 = 7 — 누적 방식 TBD)

**전략성 — 이 특성이 Taranis의 영혼**:
"오래 살려둘수록 더 아프다"는 서사가 시스템으로 강제됩니다. 적을 빨리 죽이면 누적 보너스 못 받음 → **"네트워크에 갇힌 적은 가만히 두는 게 이득"**. but 살려둔 적은 파티를 때림 → 딜레마. 이게 "살려두는 딜레마"의 완벽한 시스템 구현.

**영감**: StS Combust (매 턥 누적 데미지), StS Poison stacks (누적 도트)

---

### 메타 해금 1 — **Thunder God (뇌신)**

> **매 턴 시작 시 모든 적에게 전하 1스택을 부여합니다.** (런당 3회 제한)

**메커니즘**:
- 매 턴 시작 시 자동 발동 (Taranis가 스킬 안 써도)
- 모든 생존 적에게 전하 1스택 부여
- 런당 3회 사용 제한 (게임 체인저급이라 예외 허용)

**전략성**:
- **Taranis가 직접 스킬을 안 써도 네트워크가 형성**됨 — "신의 손길" 컨셉
- 중요한 보스전(다수 소환수)에서 개막 직전에 발동 → 그 턴 즉시 네트워크 폭발
- Sibyl의 Hand of Fate(무작위 자동 시전)와 대비 — Taranis는 "자동 전하 부여"로 더 직접적

**영감**: 하스스톤 Ragnaros (매 턴 자동 공격), StS Noxious Fumes (매 턴 자동 도트)

---

### 메타 해금 2 — **Superconductor (초전도체)**

> **연쇄 도트가 적의 DEF를 무시합니다.**

**메커니즘**:
- 모든 전하 연쇄 도트가 적 DEF 무시
- Sturdy(방어 특성) 적, DEF 버프 걸린 적에게도 풀 도트
- 고정값 도트(1/스택)가 완전히 박힘

**전략성 — 누적 가속(기본)과의 완벽 시너지**:
- 기본 특성(누적 가속)은 "오래 살려둘수록 강해짐"
- Superconductor는 "DEF 무시" → 강한 적일수록 더 아픔
- 둘이 결합하면 **"강한 적을 오래 살려둘수록 끔찍이 아픔"** — 보스전에서도 Taranis가 딜러로 복귀 가능

**영감**: StS Armor Piercing, 하스스톤 Spell Damage

---

### 특성 시너지 — "네트워크 숙련자의 3단계"

| 특성 조합 | 효과 |
|----------|------|
| **기본만** | 누적 가속 — 적을 살려둘수록 강해짐 |
| **+ Thunder God** | 자동 네트워크 형성 + 누적 가속 = 매 턥 폭발 |
| **+ Superconductor** | 누적 가속 + DEF 무시 = 어떤 적이든 처치 가능 |
| **+ 둘 다** | 완벽한 네트워크 딜러 — 자동 부여 + 누적 + DEF 무시 |

Taranis 숙련도의 핵심 — **네트워크를 키우고, 유지하고, 보상받는 리듬**이 특성 3종으로 완성.

---

## 16. 구현 관점

### SkillData 확장 (Sibyl과 공유)

```csharp
// SkillData.cs 신규 필드 (ConceptReview 8.1 기반)
[Header("전하 메카닉")]
[SerializeField] private int _chargeStacksApplied;    // 부여 전하 스택 (Wire=2, Branch=1, Thunderstorm=3)
[SerializeField] private bool _triggersPropagation;  // 전파 발동 여부 (기본 true)

[Header("접지 장벽 특수")]
[SerializeField] private bool _grantsShieldToParty;  // Grounding Field용
[SerializeField] private int _shieldAmount;
[SerializeField] private bool _chargesOnShieldAbsorb; // 쉴드 흡수 시 전하 부여
```

### StatusEffectType 신규 항목

```csharp
// StatusEffectType enum에 추가
Charge,           // 전하 (effectValue = 스택 수)
GroundingShield,  // 접지 장벽 (아군용, 흡수 시 전하 부여 플래그)
Compounding,      // 누적 가속 추적 (effectValue = 누적 횟수)
ThunderGodProc,   // 뇌신 특성 트리거 (런 카운트)
```

### ChargeNetworkComponent (신규 컴포넌트)

```csharp
public class ChargeNetworkComponent {
    private Dictionary<Character, int> _charges = new();  // 적별 전하 스택
    private Dictionary<Character, int> _compounding = new(); // 적별 누적 도트 횟수
    private int _turnCount = 0;

    // 전하 부여 + 전파
    public void ApplyCharge(Character target, int stacks, Character caster) {
        int current = _charges.GetValueOrDefault(target, 0);
        _charges[target] = Mathf.Min(3, current + stacks);  // 캡 3
        Propagate(caster);  // 자동 전파
    }

    // 매 턴 종료 시 자동 연쇄
    public void OnTurnEnd() {
        _turnCount++;
        var chargedEnemies = _charges.Keys.Where(e => e.IsAlive).ToList();

        // 연쇄: 각 적이 자신의 스택 수만큼 다른 전하 적에게 도트
        foreach (var attacker in chargedEnemies) {
            int stacks = _charges[attacker];
            for (int i = 0; i < stacks; i++) {
                var target = PickRandomOther(chargedEnemies, attacker);
                if (target != null) {
                    int dotDamage = CalculateDotWithCompounding(target);
                    DealDamage(target, dotDamage, ignoreDef: HasTrait("Superconductor"));
                }
            }
        }

        // 2턴마다 자연 소멸
        if (_turnCount % 2 == 0) {
            foreach (var key in _charges.Keys.ToList()) {
                _charges[key] = Mathf.Max(0, _charges[key] - 1);
                if (_charges[key] == 0) _charges.Remove(key);
            }
        }
    }

    // 누적 가속 도트 계산
    private int CalculateDotWithCompounding(Character target) {
        int baseDot = 1;  // 1스택당 1
        if (HasTrait("Compounding")) {
            int count = _compounding.GetValueOrDefault(target, 0);
            _compounding[target] = count + 1;
            return baseDot + count;  // 누적 횟수만큼 가산
        }
        return baseDot;
    }
}
```

### Grounding Field 특수 로직

```csharp
// 쉴드 흡수 시 전하 부여
아군.Health.OnShieldAbsorbed += (target, absorbed, attacker) => {
    if (target.HasEffect(StatusEffectType.GroundingShield)) {
        var taranis = FindCaster(target);
        taranis.ChargeNetwork.ApplyCharge(attacker, 1, taranis);
        // "공격이 역류한다" 연출 트리거
    }
};
```

### Thunder God 자동 발동

```csharp
// 매 턴 시작 시
if (CurrentTurnStart && HasTrait("ThunderGod") && _thunderGodUsesThisRun > 0) {
    foreach (var enemy in Enemies.Where(e => e.IsAlive)) {
        Taranis.ChargeNetwork.ApplyCharge(enemy, 1, Taranis);
    }
    _thunderGodUsesThisRun--;
}
```

### 통합 이벤트 훅

- `TurnManager.OnTurnEnd` → ChargeNetworkComponent.OnTurnEnd (자동 연쇄 + 자연 소멸)
- `Character.OnShieldAbsorbed` → Grounding Field 전하 부여 트리거
- `CombatEventBus.OnSkillUsed` → Wire/Branch/Thunderstorm 전하 부여

---

## 17. 위험 분석 / 밸런스 TBD 항목

### 밸런스 위험 시나리오

| 위험 | 원인 | 완화 방안 |
|------|------|----------|
| **다수전 폭발** | 4마리 네트워크 + 누적 가속 → 매 턥 10+ 도트 | 도트 단위 1 유지, 자연 소멄 2턴으로 밸런스 |
| **보스전 무력** | 단일이라 연쇄 0 → 딜 0 | Grounding Field로 보조 + Superconductor 특성으로 보스전 딜 보강 |
| **접지 장벽 사기** | 쉴드 + 전하 역부여가 너무 강력 | 쉴드량 낮게 TBD, 전하 부여 1회/흡수 제한 |
| **Thunder God 런 3회** | 강력 but 제한 있음 — 밸런스 양호 | 현행 유지 |
| **Taranis 자체 방어 낮음** | 마법사 HP + 부활 MaxHP 누적 → 사망 위험 | 기본 MaxHP Lumi급(80~90) 권장 |

### 수치 TBD (밸런스 튜닝 시 결정)

- 도트 위력: 현재 1/스택 (고정값) — 네트워크 크기로 폭발 유도
- 자연 소멸 주기: 2턴마다 -1스택 (사용자 명시)
- 연쇄 대상 선택: 무작위 분산 vs 교차 순환 (TBD)
- Wire/Branch/Thunderstorm 전하 부여 스택 수 (Wire=2, Branch=1, Thunderstorm=3 — 권장)
- Grounding Field 쉴드량 (TBD)
- 접지 장벽 전하 부여 방식 (매 흡수 1스택 vs 1회 1스택)
- Compounding 누적 계수 (기본 +1/누적 or +N)
- Taranis 기본 MaxHP (Ashe 70 / Lumi 80 수준이면 약함 → 80~90 권장)
- 마법사 3종 시너지 매트릭스 정확한 수치 (Burn + Charge 연계 등)

---

## 18. 후속 작업 후보

- [ ] 밸런스 튜닝 — 각 스킬 수치/연쇄 대상/Compounding 계수 결정 (Quick Combat 시뮬레이터로 검증)
- [ ] ChargeNetworkComponent 구조 설계 (TurnManager/CombatEventBus 통합)
- [ ] StatusEffectType 신규 항목 추가 (Charge / GroundingShield / Compounding / ThunderGodProc)
- [ ] SkillData 신규 필드 (_chargeStacksApplied / _triggersPropagation / _grantsShieldToParty 등)
- [ ] 캐릭터 에셋 DataGenerator 분기 (CharacterTable/SkillTable/Traits 12번째 추가)
- [ ] CharacterSelectUI 11→12 대응
- [ ] CharacterConceptReview.md 섹션 5.13 Taranis 요약 추가 + 마법사 3종 시너지 매트릭스 갱신
- [ ] 어센션 × Taranis 상호작용 시뮬레이션 (어센션 15 + 다수전 보스 검증)
- [ ] Taranis 스토리/연출 상세 (Voice line / 스프라이트 컨셉 — 아티스트 협업)
- [ ] **마법사 3종 시너지 매트릭스 별도 문서화** — Ashe/Lumi/Taranis 조합의 이중/삼중 시너지 정리

---

## 19. 핵심 디자인 철학 요약

Taranis는 **"직접 때리지 않는 딜러"**라는, Team Log에서 가장 역발상적인 컨셉입니다.

1. **네 번의 스킬 중 직접 딜 스킬은 없다시피 함** — Wire/Branch는 전하 심기, Grounding Field는 쉴드, Thunderstorm만 유일한 직접 데미지. 나머지는 전부 네트워크가 알아서 합니다.

2. **적의 공격이 곧 자원** — Grounding Field로 적이 때릴수록 전하가 부여됨. 적 intent를 보는 플레이어에게 "적이 공격할수록 이득"이라는 역설적 전략을 제공.

3. **네트워크=감옥** — 한 번 설계하면 2~3턴 유지되며, 적이 스스로를 지지게 둡니다. "감옥 설계자"라는 서사가 시스템으로 체험됩니다.

4. **마법사 3종 삼각 완성** — Ashe(자해 폭딜) / Lumi(통제) / Taranis(네트워크 딜+서포터)로 딜/통제/네트워크 축 완성. 파티 조합 다양성 극대화.

5. **"오래 살려둘수록 더 아프다"** — Compounding 특성이 이 딜레마를 시스템으로 강제. 적을 빨리 죽이면 보상 못 받고, 살려두면 파티가 위험해지는 긴장.

"나는 때리지 않는다. 단지 길을 만들 뿐이다." — 이 한 문장이 Taranis의 모든 것입니다. 그는 싸우지 않지만, 전장의 모든 번개가 그의 설계도를 따릅니다.
