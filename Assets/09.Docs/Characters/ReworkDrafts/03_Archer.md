# [DRAFT → 확정 예정] Aster, the Archer — "폭우의 사수"

> **상태**: 🟢 확정 (2026-07-16 Combo 컨셉 + 핵심 결정 완료 — 코드 구현 대기)
> **슬롯**: Archer (기존 Char_Archer 리워크)
> **상위 문서**: [INDEX.md](INDEX.md), [CharacterConceptReview.md 5.8](../../CharacterConceptReview.md)
> **기존 특성 파일**: `Trait_Archer_Marksman/WeakPoint/RapidFire.asset`
> **기존 스킬 에셋**: `Archer_PiercingArrow/RapidShot/Mark/CriticalShot.asset`

### 2026-07-16 확정 사항
- **이름**: Aster (그리스 "별")
- **Combo 최대치**: 3 (Umbra와 대칭 일관성)
- **Execute Shot 킬 시 Combo 복구량**: 3 완전 복구 (스노우볼은 킬 난이도로 자연 밸런스)
- **Combo 리셋 조건**: 스킬 미사용 턴 종료 시 (직관적)
- **Mark 상태이상**: `StatusEffectType.Mark` 신규 항목 (Hunter's Mark 전용)
- **Multi-Shot 다타수**: Combo 소모 1당 +1타격 (최대 4타격=12)
- **속사 특성**: Quick Shot만 AP 0 (모든 스킬 AP 0는 과도)

---

## 1. 정체성 (한 문장)

> **"쉴 새 없이 쏘는 폭우의 사수 — 한 발이 끝나기 전에 다음 화살이 날아간다."**

## 2. 이름 후보

| # | 이름 | 어원/뉘안스 | 장단점 |
|---|------|-----------|--------|
| **A** | **Aster** | 그리스 "별" — 별빛처럼 쏟아지는 화살 | 직관+우아, 기존 초안 추천 유지 |
| B | Arrowynn | Arrow + -wynn (기쁨) | 독창적이나 어색 |
| C | Sagitta | 라틴 "화살" (궁수자리 Sagittarius) | 강렬, 다소 어려움 |
| D | Vesper | "저녁 별" — 저녁의 사냥꾼 | 서사 좋으나 Umbra(그림자)와 톤 겹침 |

**추천**: `Aster` (A) — Combo 컨셉(별빛 폭우)에 가장 부합. 기존 초안과 일관성 유지

## 3. 역할군

- **주 역할군**: 지속 딜러 (다단계 히트 + Combo 축적)
- **부 역할군**: 마무리 (Execute Shot 콤보 소모 폭딜), 표식 부여 (Hunter's Mark 파티 버프)

## 4. 강점 / 약점

| 강점 | 약점 |
|------|------|
| 매 턴 스킬만 쓰면 Combo 영구 축전 | **스킬 못 쓰는 턴 = Combo 전부 리셋** |
| 다타수로 Taranis Charge/Lumi Frost 도트와 시너지 | AP 부족 시 딜 급감 (Combo 유지 불가) |
| Execute Shot로 킬 시 Combo 복구 (스노우볼) | 단일 폭딜은 Umbra/Ashe에 밀림 (지속 딜 특화) |
| Hunter's Mark로 파티 버프 (서포터 부역할) | 광역 딜 부족 (Volley 없음 — Combo는 단일 지향) |

**DesignPillars 약점 유형**: **자원 의존 + 자원 획득 조건 엄격** (매 턴 스킬 사용 강제)

## 5. 고유 메카닉: Combo (연속 사격)

```
[매 턴 스킬 사용] → Combo +1 (최대 3)
[이번 턴 스킬 미사용 시 턴 종료] → Combo 전부 리셋 (=0)
[Combo 소모 스킬 사용] → Combo 따라 위력/타수 증폭
[Execute Shot로 적 킬] → Combo 3 복구 (스노우볼)
```

**핵심 루프**: Quick Shot(축전) → Multi-Shot(소모+다타수) → Hunter's Mark(축전+서포트) → Execute Shot(전부 소모+킬 시 복구). 매 턴 스킬을 쏘며 Combo 유지 → Execute Shot로 마무리.

### Umbra(Shadows)와의 정반대 대칭

| 캐릭터 | 자원 | 축전 조건 | 리셋 조건 | 보상 유형 | 플레이 스타일 |
|--------|------|----------|----------|----------|------------|
| **Umbra** | Shadows | 안 **맞을** 때 +1 | 맞으면 리셋 | 치명타 **확률** | 회피 기동전 |
| **Aster** | Combo | **계속 쏠** 때 +1 | 스킬 안 쓰면 리셋 | 다타수 **확정** | 정지 진지사격 |

**두 캐릭터가 한 파티에 있을 때**:
- Umbra는 적 공격을 회피하며 뒤에서 암살
- Aster는 앞장서서 쉴 새 없이 화살을 퍼부어야 함 — 완전히 다른 포지션 요구
- Duran(도발) + Lumi(Freeze)가 적을 묶어두면 둘 다 안전하게 역할 수행

## 6. 스킬 4종 (4개 다른 조건)

| 스킬 | AP | 기본 효과 | 조건 | Combo 상호작용 |
|-----|----|---------|------|---------------|
| **Quick Shot** (빠른 사격) | 1 | 단일 4 | (셋업 — 조건 없음) | **Combo +1** (축전 전용) |
| **Multi-Shot** (다중 사격) | 2 | 단일 3×N타격 | **자원 소모** (Combo 1+ 필요) | Combo 1 소모당 **+1타격** (Combo 3→4타격, 위력 12) |
| **Hunter's Mark** (사냥표식) | 1 | 단일 Mark + Def-2 (2턴) | **대상 상태** (Mark 부여) | Combo +1 (축전 겸용). Archer 본인이 Mark 적 공격 시 +3 위력 |
| **Execute Shot** (처형 사격) | 3 | 단일 8 + Combo×5 | **자원 전량 소모** (Combo 0 불가) | 모든 Combo 소모. 킬 시 **Combo 3 복구** |

### 조건 다양성 검증 (4.5 원칙 2)
- Quick Shot → 셋업 (조건 없음)
- Multi-Shot → **자원 소모** (Combo 1+)
- Hunter's Mark → **대상 상태** (Mark 부여 자체)
- Execute Shot → **자원 전량 소모** (Combo 1+)

→ 4개 모두 다른 조건. 매 턴 다른 퍼즐. ✅

### Combo 상호작용 매트릭스
| 스킬 | Combo 0 | Combo 1 | Combo 2 | Combo 3 |
|------|---------|---------|---------|---------|
| Quick Shot | → 1 | → 2 | → 3 | 3 유지 |
| Multi-Shot | 사용 불가 | 3×2=6 (1 소모) | 3×3=9 (1 소모, 2 잔여) | 3×4=12 (1 소모, 2 잔여) |
| Hunter's Mark | → 1 | → 2 | → 3 | 3 유지 |
| Execute Shot | 사용 불가 | 8+5=13 (1 소모) | 8+10=18 (2 소모) | 8+15=23 (3 소모) |

### 기존 스킬 매핑
| 기존 | 신규 | 변경 |
|------|------|------|
| Archer_RapidShot (6, AP1) | Quick Shot | 6→4 (Combo +1 보상 감안 너프) |
| Archer_PiercingArrow (14, AP2) | Multi-Shot | 단일 14 → 3×N타격 (Combo 따라 6/9/12) |
| Archer_Mark (DefenseDown, AP1) | Hunter's Mark | Mark 상태이상 유지 + Def-2 유지 |
| Archer_CriticalShot (22, AP3) | Execute Shot | 22 → 8+Combo×5 (최대 23, 킬 시 Combo 복구) |

## 7. BehaviorTag 활용

| BehaviorTag | 적용 스킬 | 효과 | 백로그/구현 상태 |
|------------|----------|------|----------------|
| `FirstBlood` | Hunter's Mark | 풀피 적 첫 표식 +4 위력 | 컨셉 6 (이미 구현됨) |
| `Dominance` | Execute Shot | 적 HP < Aster HP 시 위력 +4 | 컨셉 17 (이미 구현됨) |
| **신규 Behavior: `ComboFinisher`** | Execute Shot | 킬 시 Combo 3 볩구 (Archer 고유) | **신규 구현 필요** (~15줄) |
| **신규 Behavior: `ComboMultiHit`** | Multi-Shot | Combo 1 소모당 추가 타격 | **신규 구현 필요** (~20줄) |

### 신규 BehaviorTag 설계 노트
- `ComboFinisher` (ExecutionPhase.PostApply): 스킬 킬 발생 시 caster.Resource.AddStacks(3) 호출. 일반 BehaviorTag vs 자원 특화 헬퍼 둘 중 선택 검토
- `ComboMultiHit` (ExecutionPhase.ApplyMain): caster.Resource.CurrentStacks 만큼追加 ExecuteSkillInternal 호출, 이후 ConsumeStacks(1). **다중 타격 인프라**(ARCH 백로그) 활용

## 8. 장착 특성 3종 리워크

| 특성 | 기존 효과 | 리워크 효과 | 해금 |
|------|---------|------------|------|
| **명사수** (기본) | 위력 +2 가산 | **Combo가 3일 때 모든 스킬 위력 +3** (Combo 유지 보상) | 기본 |
| **약점 포착** | 적 HP 60%- 시 ×1.4 | **Hunter's Mark 적에게 위력 +4** (Mark 의존 강화) | 30 조각 |
| **속사** | 스킬 코스트 -1 | **Quick Shot AP 0** (매 턴 무료 Combo 축전 → Combo 유지 용이) | 60 조각 + 1 영혼 |

### 특성 키워드 매핑 (CharacterTraitHandler에서 처리)
| 특성 | KeywordType | Trigger | Value |
|------|------------|---------|-------|
| 명사수 | **`ComboMaxPowerBonus`** (신규) | Passive | 3 |
| 약점 포착 | **`PowerAddVsMark`** (신규, PowerAddVsDebuff와 유사) | Passive | 4 |
| 속사 | `CostAdd` | Passive | -1 (Quick Shot에만 적용되도록 제한 검토) |

## 9. 밸런스 시나리오 (단일 보스전 5턴)

```
턴 1: Quick Shot (Combo 0→1) → 4 데미지
턴 2: Quick Shot (Combo 1→2) → 4 데미지 (누적 8)
턴 3: Hunter's Mark (Combo 2→3, Mark 부여) → 0 + 셋업
턴 4: Multi-Shot (Combo 3 소모 1 → 2, 4타격) → 12 데미지 (누적 20)
턴 5: Execute Shot (Combo 2 전부 소모) → 8+10=18 데미지 (누적 38)
       킬 시 Combo 3 복구 → 다음 전투 시작부터 유리
```

**비교 — Umbra 5턴 평균**:
```
턴 1-3: 안 맞으며 Poison Blade/Backstab/Rupture → 약 22 데미지
턴 4: Eviscerate (Shadows 3) → 30 데미지 (치명타)
총합: 약 52 데미지 (폭딜형)
```

→ Aster는 지속 딜 (38), Umbra는 폭딜 (52). 역할 차별화 명확

## 10. 파티 시너지

| 조합 | 시너지 | 핵심 |
|------|-------|------|
| **Aster + Umbra** | ★★★ | 완전 보완 — Aster(지속 딜) + Umbra(폭딜). Duran이 도발하면 둘 다 안전 |
| **Aster + Lumi** | ★★★ | Lumi Blizzard 광역 Freeze → Aster Multi-Shot 다타수로 도트 촉발 |
| **Aster + Ashe** | ★★ | Ashe Burn → Aster Hunter's Mark. 이중 디버프로 파티 전체 딜 증폭 |
| **Aster + Taranis** | ★★★ | Taranis Charge 광역 + Aster Multi-Shot 다타수 = 도트 폭발 |
| **Aster + Healer** | ★★ | AP 회복으로 Combo 유지 (스킬 못 쓰면 리셋이므로 AP가 생명) |

## 11. ✅ 결정 항목 (2026-07-16 확정)

- [x] **이름**: **Aster** (그리스 "별")
- [x] **자원 이름**: **Combo** (간결, 직관)
- [x] **Combo 최대치**: **3** (Umbra 대칭)
- [x] **Combo 리셋 조건**: 스킬 미사용 턴 종료 시
- [x] **Mark 상태이상 구현**: **StatusEffectType.Mark 신규** (DefenseDown과 별개)
- [x] **Multi-Shot 다타수**: **Combo 소모 1당 +1타격** (최대 4타격=12)
- [x] **Execute Shot 킬 시 Combo 복구량**: **3 완전 복구**
- [x] **명사수 특성**: ComboMaxPowerBonus 신규 키워드 (Combo 3일 때 위력 +3)
- [x] **속사 특성 AP 0**: Quick Shot에만 적용
- [x] **기존 Archer_CriticalShot**: 가비지 컬렉션 대상 (Umbra CC-2A 패턴과 동일)
- [x] **ArcherUI ResourceBadge**: Combo 카운터 (0/3), 색상 = UIPalette 토큰 추가 (빨강/주황 계열 제안)

## 12. 리스크와 검증

| 리스크 | 완화 |
|-------|-------|
| Combo 리셋이 플레이어에게 가혹 (AP 부족 시 멘붕) | Quick Shot을 AP 1 저렴화 (속사 특성 시 AP 0). 매 턴 1개는 무조건 쏠 수 있게 |
| Multi-Shot 4타격=12 위력 사기 (Ashe Brand of Ash 23과 비슷) | AP 2 + Combo 1 소모 비용 명확. Umbra Eviscerate 15(치명타 30)보다 약함 |
| Execute Shot 킬 시 Combo 3 복구 스노우볼 | 약한 적에게는 과치, 보스전에서는 킬 어려움 → 자연 밸런스 |
| Hunter's Mark가 Umbra StrongVsDebuff와 중복 | Archer Mark는 "방어 감소 + 서포트", Umbra StrongVsDebuff는 "디버프 시 2배" — 다른 축 |
| Combo 0일 때 Multi/Execute 사용 불가 → 플레이어 갇힘 | Quick Shot은 Combo 0에서 사용 가능 (셋업 역할) |
| 다타수가 Lumi Frost/Freeze 도트 과도 촉발 | 도트 밸런스 별도 검증 (BalanceSimulator 1000팩) |

## 13. 구현 메모 (코드 구현 시 참조 — 별도 Phase)

### 신규 코드 필요
1. **ComboResourceComponent** (`Characters/Components/`)
   - OnTurnEnd에서 _usedSkillThisTurn=false 시 Reset(). OnSkillUsed 이벤트 구독
   - MaxStacks = 3. WarningThreshold = 2 (2스택부터 주의)
2. **StatusEffectType.Mark** 신규 항목 (Hunter's Mark 표식용)
3. **ComboMultiHitBehavior** (`Skill/Behaviors/Implementations/`)
   - ApplyMain Phase. caster의 CurrentStacks만큼 추가 ExecuteSkillInternal. 이후 ConsumeStacks(1)
4. **ComboFinisherBehavior** (`Skill/Behaviors/Implementations/`)
   - OnKill Phase. 킬 시 caster.Resource.AddStacks(3)
5. **KeywordType.ComboMaxPowerBonus** + **KeywordType.PowerAddVsMark** 신규
6. **CharacterTraitHandler.ApplyPassiveEffects / GetBonusOutgoingDamage** 확장
   - ComboMaxPowerBonus: owner.Resource.CurrentStacks >= MaxStacks 시 위력 +
   - PowerAddVsMark: target이 Mark 상태일 시 위력 +N (HasDotDebuff와 유사 패턴)
7. **ResourceType.Combo** enum 추가
8. **Character.CreateResource** Combo 분기 추가

### 기존 코드 수정
- `DataGenerator.PhaseCC.cs` (또는 신규 DataGenerator.ArcherRework.cs) — Archer 스킬 4종 재생성 + Char_Archer ResourceType.Combo 설정
- `DataGenerator.Traits.cs` — Archer 특성 3종 리워크 키워드 적용
- `UIPalette.cs` — Combo 자원 색상 토큰 (빨강/주황 계열 제안)
- `BattleDisplayUtil.cs` — BuildSkillDescription에 Combo 소모/다타수 설명 추가

### 구현 난이도 추정
- 신규 Behavior 2종: 중간 (~35줄)
- ComboResourceComponent: 중간 (~50줄, ShadowsResourceComponent 패턴 차용)
- 특성 키워드 2종: 낮음 (~20줄)
- DataGenerator/UI: 낮음 (~30줄)
- **총합**: 약 130줄 + 4 스킬 .asset 재생성

### 테스트 계획 (PhaseCC2BTests.cs 신규)
1. ComboResourceComponent: 스킬 사용 시 +1, 미사용 시 리셋, 최대 3
2. ComboMultiHitBehavior: Combo 0/1/2/3에서 타격 수
3. ComboFinisherBehavior: 킬 시 Combo 3 복구
4. ComboMaxPowerBonus 특성: Combo 3일 때 위력 +3
5. PowerAddVsMark 특성: Mark 적 +4
6. Execute Shot 전체 흐름: Combo 3→0 + 킬 시 복구

---

## 변경 이력

| 날짜 | 변경 |
|------|------|
| 2026-07-14 | 최초 작성 (Hunter's Mark 단독 컨셉, 🔴 초안) |
| 2026-07-16 | **D. Combo 컨셉으로 전면 재작성**. Umbra 정반대 축. ResourceBadge 일관성 확보. 🟡 논의 중 |
