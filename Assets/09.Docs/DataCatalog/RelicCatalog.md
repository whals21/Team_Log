# 유물 데이터 카탈로그

> 최종 갱신: 2026-06-16
> 데이터 소스: `Assets/03.Data/Relics/*.asset` (16종)
> 구현 소스: `Assets/02.Scripts/Reward/RelicData.cs`, `RelicHandler.cs`
> 키워드 시스템: `Assets/02.Scripts/Skill/KeywordEntry.cs`

---

## 설계 개요

유물은 **트리거 + 효과** 조합으로 구성된 영구 강화 아이템. 한 번 획득하면 런이 끝날 때까지 유지되며, 상점(`ShopManager`)이나 보상(`RewardManager`)으로 획득.

### 스키마

```csharp
class RelicData : ScriptableObject {
    string _relicName;          // 표시 이름
    string _description;        // 설명
    Sprite _icon;
    RelicTrigger _trigger;      // 발동 시점 (11종)
    int _effectValue;           // 기본 수치
    RewardRarity _rarity;       // 희귀도
    int _price;                 // 상점 가격
    KeywordEntry[] _keywords;   // 상세 효과 (복합 가능)
}
```

### RewardRarity (3등급)

| 값 | 이름 | 가격대 | 드랍 가중치 |
|----|------|--------|-------------|
| 0 | Common | 60~90 | 높음 |
| 1 | Rare | 100~130 | 중간 |
| 2 | Unique | 160~200 | 낮음 |

### RelicTrigger (11종)

| 값 | 이름 | 발동 시점 |
|----|------|----------|
| 0 | None | 미사용 |
| 1 | BattleStart | 전투 시작 시 (1회) |
| 2 | TurnStart | 매 턴 시작 |
| 3 | TurnEnd | 매 턴 종료 |
| 4 | OnDamageDealt | 데미지를 줄 때 |
| 5 | OnDamageReceived | 데미지를 받을 때 |
| 6 | OnKill | 적 처치 시 |
| 7 | OnHealApplied | 힐 적용 시 |
| 8 | OnShieldGained | 쉴드 획득 시 |
| 9 | OnGoldEarned | 골드 획득 시 |
| 10 | OnSkillUsed | 스킬 사용 시 |

### KeywordType (24종, 핵심 15종만 유물에 사용)

| 값 | 이름 | 의미 | 사용 유물 수 |
|----|------|------|-------------|
| 5 | DrawWeightAdd | 드로우 가중치 가산 | 1 |
| 10 | HPPerTurn | 매 턴 HP 변화 | 2 |
| 11 | ShieldPerTurn | 매 턴 쉴드 획득 | 3 |
| 13 | BonusOutgoingDamage | 추가 고정 데미지 | 1 |
| 14 | DamageReduction | 고정 피해 감소 | 1 |
| 15 | CounterDamage | 반사 피해 | 1 |
| 16 | OnKillHeal | 처치 시 HP 회복 | 1 |
| 18 | StackingPowerOnKill | 처치당 공격력 누적 | 1 |
| 19 | MaxHPUp | 최대 HP 증가 | 2 |
| 20 | ATKUp | ATK 영구 증가 | 1 |
| 21 | DEFUp | DEF 영구 증가 | 1 |
| 23 | BonusGold | 골드 획득 시 추가 골드 | 1 |

### KeywordTrigger (12종)

| 값 | 이름 | 비고 |
|----|------|------|
| 0 | Passive | 상시 적용 (EffectivePower 계산 등) |
| 1 | OnTurnStart | 턴 시작 |
| 2 | OnTurnEnd | 턴 종료 |
| 3 | OnBattleStart | 전투 시작 |
| 4 | OnDamageDealt | 데미지를 줄 때 |
| 5 | OnDamageReceived | 데미지를 받을 때 |
| 6 | OnKill | 적 처치 시 |
| 7 | OnHealApplied | 힐 적용 시 |
| 8 | OnShieldGained | 쉴드 획득 시 |
| 9 | OnSkillUsed | 스킬 사용 시 |
| 10 | OnGoldEarned | 골드 획득 시 |
| 11 | HPBelow | HP 임계치 이하 |

> **참고**: `RelicTrigger`와 `KeywordTrigger`는 비슷하지만 별개 enum. RelicTrigger는 유물 전체 발동 시점, KeywordTrigger는 개별 키워드의 적용 시점. 보통 둘이 일치하지만, **패시브 효과**는 RelicTrigger=이벤트 + KeywordTrigger=Passive 조합으로 작동.

---

## 1. 유물 카탈로그 (16종)

### Common (공통, 7종)

#### Relic_HealingHerb — 치유 허브
| 항목 | 내용 |
|------|------|
| 트리거 | BattleStart |
| 키워드 | `HPPerTurn(10, OnBattleStart)` |
| 가격 | 60 |
| 효과 | **전투 시작 시 파티 HP 10 회복** |
| 시너지 | DragonHeart(MaxHP 증가)와 결합 시 회복 가치 상승 |

---

#### Relic_LifeCrystal — 생명력의 결정
| 항목 | 내용 |
|------|------|
| 트리거 | BattleStart |
| 키워드 | `MaxHPUp(20, OnBattleStart)` |
| 가격 | 80 |
| 효과 | **전투 시작 시 파티원 모두 최대 HP +20** |
| 시너지 | HealingHerb/RegenRing과 결합 시 회복 효율 증가 |

---

#### Relic_BurningSword — 불타는 검
| 항목 | 내용 |
|------|------|
| 트리거 | OnSkillUsed |
| 키워드 | `BonusOutgoingDamage(3, Passive)` |
| 가격 | 80 |
| 효과 | **모든 공격 스킬 사용 시 추가 고정 데미지 +3** |
| 시너지 | 도적 DoubleStrike(2회 타격)와 결합 시 +6 데미지 |

---

#### Relic_RegenRing — 재생의 반지
| 항목 | 내용 |
|------|------|
| 트리거 | TurnEnd |
| 키워드 | `HPPerTurn(3, OnTurnEnd)` |
| 가격 | 60 |
| 효과 | **매 턴 종료 시 파티 HP 3 회복** |
| 시너지 | 장기전에서 누적 효과. 10턴 = 30 HP 회복 |

---

#### Relic_ShieldAmulet — 방패 부적
| 항목 | 내용 |
|------|------|
| 트리거 | OnShieldGained |
| 키워드 | `ShieldPerTurn(3, OnShieldGained)` |
| 가격 | 70 |
| 효과 | **쉴드 스킬 사용 시 추가 쉴드 +3** |
| 시너지 | 전사 방패 방어(Power 5) → 실제 8 쉴드. WarBanner와 중첩 가능 |

---

#### Relic_HardShell — 단단한 껍질
| 항목 | 내용 |
|------|------|
| 트리거 | BattleStart |
| 키워드 | `DEFUp(3, OnBattleStart)` |
| 가격 | 90 |
| 효과 | **전투 시작 시 파티원 모두 DEF +3** |
| 시너지 | IronHide(받는 피해 -2)와 결합 시 물리 탱킹 극대화 |

---

#### Relic_IronHide — 철가죽
| 항목 | 내용 |
|------|------|
| 트리거 | OnDamageReceived |
| 키워드 | `DamageReduction(2, Passive)` |
| 가격 | 90 |
| 효과 | **받는 모든 피해 -2** (최소 1) |
| 시너지 | HardShell(DEF +3)과 결합 시 매우 단단한 탱커 구축 |

---

### Rare (희귀, 6종)

#### Relic_SwiftBoots — 질풍 부츠
| 항목 | 내용 |
|------|------|
| 트리거 | TurnStart |
| 키워드 | `ShieldPerTurn(2, OnTurnStart)` |
| 가격 | 100 |
| 효과 | **매 턴 시작 시 파티 쉴드 +2** (자동) |
| 시너지 | ShieldAmulet + WarBanner와 결합 시 쉴드가 복리로 쌓임 |

---

#### Relic_WeaponStone — 무기 강화석
| 항목 | 내용 |
|------|------|
| 트리거 | BattleStart |
| 키워드 | `ATKUp(3, OnBattleStart)` |
| 가격 | 100 |
| 효과 | **전투 시작 시 파티원 모두 ATK +3** |
| 시너지 | BerserkerMark(처치당 +2)와 결합 시 스노우볼 딜 |

---

#### Relic_LuckyClover — 네잎클로버
| 항목 | 내용 |
|------|------|
| 트리거 | BattleStart |
| 키워드 | `DrawWeightAdd(5, Passive)` |
| 가격 | 110 |
| 효과 | **모든 스킬 드로우 가중치 +5** |
| 시너지 | 기본 가중치 25 → 30으로 상향. 잘 안 나오는 스킬의 등장률 증가 |

---

#### Relic_ThornArmor — 가시 갑옷
| 항목 | 내용 |
|------|------|
| 트리거 | OnDamageReceived |
| 키워드 | `CounterDamage(2, Passive)` |
| 가격 | 130 |
| 효과 | **피격 시 공격자에게 2 고정 반사 데미지** |
| 시너지 | 다수전에서 누적 반사. 다수 적 러시 카운터 |

---

#### Relic_VampireFang — 흡혈 송곳니
| 항목 | 내용 |
|------|------|
| 트리거 | OnKill |
| 키워드 | `OnKillHeal(5, OnKill)` |
| 가격 | 100 |
| 효과 | **적 처치 시 처치자 HP +5 회복** |
| 시너지 | 다수전에서 연쇄 처결 시 자원 회복 루프 |

---

#### Relic_GoldCharm — 황금 부적
| 항목 | 내용 |
|------|------|
| 트리거 | OnGoldEarned |
| 키워드 | `BonusGold(15, OnGoldEarned)` |
| 가격 | 120 |
| 효과 | **골드 획득 시마다 +15 골드 추가** |
| 시너지 | 상점/보상/이벤트 모든 골드에 적용. 경제 빌드의 핵심 |

---

### Unique (고유, 3종)

#### Relic_BerserkerMark — 광전사 인장
| 항목 | 내용 |
|------|------|
| 트리거 | OnKill |
| 키워드 | `StackingPowerOnKill(2, OnKill)` |
| 가격 | 180 |
| 효과 | **적 처치 시마다 처치자 ATK +2 영구 누적** (전투 중) |
| 시너지 | WeaponStone과 결합 시 다수전 폭딸. 단일 보스전에서는 효과 제한적 |

---

#### Relic_WarBanner — 전투 깃발
| 항목 | 내용 |
|------|------|
| 트리거 | BattleStart |
| 키워드 | `ShieldPerTurn(5, OnBattleStart)` |
| 가격 | 160 |
| 효과 | **전투 시작 시 파티 전원 쉴드 +5** |
| 시너지 | SwiftBoots + ShieldAmulet과 결합 시 쉴드가 매 턴 자가 증식 |

---

#### Relic_DragonHeart — 드래곤의 심장
| 항목 | 내용 |
|------|------|
| 트리거 | BattleStart |
| 키워드 | `MaxHPUp(50, OnBattleStart)` |
| 가격 | 200 |
| 효과 | **전투 시작 시 파티원 모두 최대 HP +50** |
| 시너지 | LifeCrystal(+20)과 중첩 시 총 +70 HP. 흡혈/재생 효율 폭증 |

---

## 2. 스탯 비교표

### 전체 유물 비교

| 유물 | 희귀도 | 가격 | 트리거 | 키워드 | Value |
|------|--------|------|--------|--------|-------|
| 치유 허브 | Common | 60 | BattleStart | HPPerTurn | 10 |
| 재생의 반지 | Common | 60 | TurnEnd | HPPerTurn | 3/턴 |
| 방패 부적 | Common | 70 | OnShieldGained | ShieldPerTurn | 3 |
| 생명력의 결정 | Common | 80 | BattleStart | MaxHPUp | 20 |
| 불타는 검 | Common | 80 | OnSkillUsed | BonusOutgoingDamage | 3 |
| 단단한 껍질 | Common | 90 | BattleStart | DEFUp | 3 |
| 철가죽 | Common | 90 | OnDamageReceived | DamageReduction | 2 |
| 질풍 부츠 | Rare | 100 | TurnStart | ShieldPerTurn | 2/턴 |
| 무기 강화석 | Rare | 100 | BattleStart | ATKUp | 3 |
| 흡혈 송곳니 | Rare | 100 | OnKill | OnKillHeal | 5/처치 |
| 네잎클로버 | Rare | 110 | BattleStart | DrawWeightAdd | 5 |
| 황금 부적 | Rare | 120 | OnGoldEarned | BonusGold | 15/획득 |
| 가시 갑옷 | Rare | 130 | OnDamageReceived | CounterDamage | 2/피격 |
| 전투 깃발 | Unique | 160 | BattleStart | ShieldPerTurn | 5 |
| 광전사 인장 | Unique | 180 | OnKill | StackingPowerOnKill | 2/처치 |
| 드래곤의 심장 | Unique | 200 | BattleStart | MaxHPUp | 50 |

### 트리거별 분류

| 트리거 | 유물 수 | 유물 목록 |
|--------|---------|----------|
| BattleStart | 7 | 치유 허브, 생명력의 결정, 무기 강화석, 네잎클로버, 단단한 껍질, 전투 깃발, 드래곤의 심장 |
| TurnStart | 1 | 질풍 부츠 |
| TurnEnd | 1 | 재생의 반지 |
| OnDamageReceived | 2 | 가시 갑옷, 철가죽 |
| OnKill | 2 | 흡혈 송곳니, 광전사 인장 |
| OnShieldGained | 1 | 방패 부적 |
| OnGoldEarned | 1 | 황금 부적 |
| OnSkillUsed | 1 | 불타는 검 |

> **특징**: BattleStart 트리거가 절반 가까이(7/16). 전투 시작 시 스탯 버스트가 주류.
> **부족**: OnHealApplied, OnDamageDealt 트리거 유물이 없음 → 확장 여지.

### 희귀도별 통계

| 희귀도 | 유물 수 | 평균 가격 | 평균 Value |
|--------|---------|----------|-----------|
| Common | 7 | 75.7골드 | 다양 (3~20) |
| Rare | 6 | 110골드 | 다양 (2~15) |
| Unique | 3 | 180골드 | 고수치 (5~50) |

---

## 3. 자연 시너지 조합 (구현 완료 유물만)

> 기존 16종만으로 만들 수 있는 시너지 조합.

### 시너지 A: "철벽 파티"
```
전사 + HardShell(DEF+3) + IronHide(피해-2) + WarBanner(쉴드+5) + SwiftBoots(쉴드+2/턴)
```
- **효과**: 매 턴 쉴드 자가 증식 + 높은 DEF + 피해 감소
- **강점**: 물리 다수전에서 사실상 무적
- **약점**: 마법 데미지(Burn/Poison 도트)는 DEF 무시 → 인퀴지터/마법사 적에 취약

### 시너지 B: "생명력 폭발"
```
DragonHeart(MaxHP+50) + LifeCrystal(MaxHP+20) + RegenRing(HP 3/턴) + HealingHerb(전투 시작 10)
```
- **효과**: 파티원당 최대 HP +70 + 매 턴 회복 + 시작 회복
- **강점**: 장기전 생존력 극대화
- **약점**: 딜이 부족. VampireFang과 결합하면 흡혈 루프 가능

### 시너지 C: "광전사의 학살"
```
WeaponStone(ATK+3) + BerserkerMark(처치당 ATK+2) + BurningSword(공격 +3) + VampireFang(처치 시 HP+5)
```
- **효과**: 처치할수록 강해지는 스노우볼 딜러
- **강점**: F2~F3 일반 다수전에서 학살
- **약점**: 보스 단일전에서는 처치 기회 부족 → BerserkerMark 효과 제한

### 시너지 D: "경제 빌드"
```
GoldCharm(골드 획득 +15) + 다수의 보상형 유물
```
- **효과**: 모든 골드 획득에 +15 추가. 상점에서 비싼 Unique 유물 조기 구매 가능
- **강점**: 런 초반에 GoldCharm 획득 시 전체 런 경제 우위
- **약점**: 전투력 직접 강화는 아님 → 딜/탱은 다른 유물로 보충 필요

---

## 4. 현재 카탈로그의 갭 분석

### 부족한 트리거 (확장 기회)

| 트리거 | 현재 유물 수 | 확장 필요성 |
|--------|-------------|-------------|
| OnHealApplied | 0 | 상승 (힐 빌드 미존재) |
| OnDamageDealt | 0 | 상승 (딜 누적 메커니즘 부재) |
| TurnStart | 1 (SwiftBoots) | 중간 |
| TurnEnd | 1 (RegenRing) | 중간 |

### 부족한 메커니즘

1. **유물-유물 연쇄**: 현재 모든 유물이 독립적으로 작동. A가 B의 트리거를 만드는 구조 없음
2. **조건부 효과**: HPBelow 트리거가 시스템에 존재하지만 사용 유물 0종
3. **캐릭터 특화**: 전사/마법사/힐러/도적 중심 유물 없음 (모두 파티 전체 적용)
4. **특수 적 카운터**: 인퀴지터/크로노맨서 등 신규 적 메커니즘 대응 유물 없음

### 권장 확장 방향

상세한 새 유물 제안은 **`RelicSynergies.md`** (별도 문서)에서 다룸. 핵심 원칙:

1. **단독 유물은 미미한 효과** (+1~3 수준)
2. **같은 카테고리 3종이 모여야 시너지 폭발**
3. **확률 기반 효과 배제** (명확한 조건/보장된 효과만)
4. **크로스 카테고리 조합이 진정한 엔드게임 빌드**

---

## 5. 구현 참고 사항

### RelicHandler 적용 흐름

```
1. GameRunState.RelicHandler가 모든 보유 유물의 KeywordEntry를 수집
2. KeywordResolver.SumKeyword/SumConditional로 타입별 합산
3. TurnManager/SkillExecutor/DamageCalculator가 합산 결과를 반영
   - EffectivePower: PowerMul × PowerAdd + BonusOutgoingDamage
   - EffectiveCost: CostAdd 합산
   - 받는 데미지: DamageReduction 합산 (최소 1)
   - 주는 데미지: CounterDamage, BonusOutgoingDamage 추가
4. 이벤트 트리거 (OnKill, OnShieldGained 등)는 CombatEventBus 경유
```

### 주의점

- **KeywordTrigger=Passive**인 키워드만 `SumKeyword`로 합산됨. 이벤트성 키워드(OnKill 등)는 별도 처리
- **RelicTrigger와 KeywordTrigger 불일치 허용**: BurningSword는 RelicTrigger=OnSkillUsed이지만 KeywordTrigger=Passive → 모든 스킬에 상시 적용
- **HPBelow 조건**: `KeywordResolver.IsHPConditionMet`으로 검증. 현재 사용 유물 없음

---

## 6. 관련 문서

- `MonsterCatalog.md` — 적 스탯/스킬 카탈로그
- `TraitCatalog.md` — 적 특성 시스템
- `SkillCatalog.md` — 플레이어/적 스킬 카탈로그
- `EncounterConcepts.md` — 전투 조합 컨셉 (특수 적 포함)
- `RelicSynergies.md` — 새 시너지형 유물 27종 제안 (작성 예정)
- `AssetCatalog.md` — 아이콘/스프라이트 매핑
