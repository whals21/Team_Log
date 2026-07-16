# [DRAFT → 확정 예정] Mortis, the Necromancer — "시체의 스승"

> **상태**: 🟢 확정 (2026-07-17 Corpse 컨셉 + 핵심 결정 완료 — 코드 구현 대기, 별도 Phase CC-2F)
> **슬롯**: Necromancer (기존 Char_Necromancer 리워크)
> **상위 문서**: [INDEX.md](INDEX.md), [CharacterConceptReview.md 5.9](../../CharacterConceptReview.md)
> **기존 특성 파일**: `Trait_Necro_LifeLeech/CursePrice/DeathHarvest.asset`

### 2026-07-17 확정 사항 (사용자 제안 + 결정)
- **이름**: Mortis (라틴 "죽음")
- **자원**: 없음 (시체 자체가 메카닉)
- **시체**: 1체. HP 없음 (적의 대상 안 됨). Necromancer 사망 시 동반 사망
- **시체 행동**: 매 턴 플레이어 종료 후 4개 스킬 중 무작위 1개 자동 시전
- **스킬 교체**: 적 처치 시 Discover UI (Alchemist 재사용)로 스킬 선택 → 시체 슬롯 1개 교체
- **리셋**: 전투 종료 시 시체 사라지고 다음 전투에서 기본 4스킬로 재소환

---

## 1. 정체성 (한 문장)

> **"전투마다 시체를 일으켜 세운다 — 그리고 쓰러뜨린 적의 기술을 시체에게 먹여 성장시킨다. 죽음은 끝이 아니라, 새로운 시작이다."**

## 2. 이름

**Mortis** (라틴 "죽음", rigor mortis) — Necromancer 정체성과 1:1 대응. "죽음 그 자체"

## 3. 역할군

- **주 역할군**: 간접 딜러 (시체가 매 턴 자동 딜)
- **부 역할군**: 적 약화 (Curse로 시체 처치 유도) + 시체 강화 (버퍼)

## 4. 강점 / 약점

| 강점 | 약점 |
|------|------|
| 매 턴 자동 딜 (시체가 플레이어 개입 없이 행동) | **시체 스킬이 랜덤** — 원하는 효과 안 나올 수 있음 |
| 적 처치로 시체 강화 (동적 성장) | Necromancer 본인 딜은 약함 (시체에 의존) |
| Curse로 적 약화 → 시체가 쉽게 처치 | **Necromancer 사망 = 시체도 사망** (CC-0 부활 시 시체 리셋) |
| Soul Link로 시체 딜 → Necromancer 회복 | 시체 강화에 AP 소모 (본인 스킬 부담) |

**DesignPillars 약점 유형**: **간접 딜러** (본인 딜 약함, 시체 의존)

## 5. 고유 메카닉: Summoned Corpse (동적 스킬 풀)

### 시체 구조
- **별도 엔티티가 아님** — Necromancer 캐릭터에 종속된 데이터 컨테이너
- `CorpseSkillSlots[4]` — 시체가 사용 가능한 4개 스킬 슬롯
- 매 턴 플레이어 종료 후, **무작위 1개 슬롯의 스킬 자동 시전**
- 시체는 HP/StatusEffects 없음 (적의 대상이 안 됨)
- Necromancer 사망 시 시체도 자동 사망 (비활성화)

### 매 전투 흐름
```
[전투 시작]
  ↓
[자동 시체 소환] — CorpseSkillSlots = 기본 4스킬로 초기화
  ↓
[매 턴 플레이어 종료 후]
  ↓
[시체가 4개 슬롯 중 무작위 1개 자동 시전]
  ↓
[적 처치 이벤트 발생 시]
  ↓
[Discover Modal 팝업 (Alchemist UI 재사용)]
  - 방금 처치한 적의 스킬 4개 표시
  - 플레이어가 1개 선택
  - 시체 슬롯 4개 중 교체할 슬롯 선택 (또는 자동)
  ↓
[전투 반복... 적이 강할수록 시체 스킬도 강해짐]
  ↓
[전투 종료]
  ↓
[시체 사라지고 다음 전투에서 기본 4스킬로 리셋]
```

### 시체의 4개 기본 스킬 (전투 시작 시)
| 슬롯 | 기본 스킬 | 효과 |
|------|---------|------|
| 1 | **Scratch** | 단일 4 기본 공격 |
| 2 | **Poison Bite** | 단일 3 + Poison 2턴 |
| 3 | **Bone Toss** | 단일 4 + Bleed 2턴 |
| 4 | **Stun Strike** | 단일 2 + Stun 1턴 (저확률) |

### 스킬 교체 예시 (Soul Echo)
```
전투 1 시작: 시체 = Scratch / Poison Bite / Bone Toss / Stun Strike
  ↓ Goblin 처치 → Discover Modal: Goblin_Scratch / Bite / Steal / Hide 중 1개 선택
  ↓ Goblin_Bite 선택 → 시체 슬롯 2 (Poison Bite)와 교체
전투 1 진행: 시체 = Scratch / Goblin_Bite / Bone Toss / Stun Strike
  ↓ Slime 처치 → Slime_AcidSpit 등 선택 → 슬롯 1 교체
전투 1 종료: 시체 = Slime_AcidSpit / Goblin_Bite / Bone Toss / Stun Strike

전투 2 시작: 다시 기본 4스킬로 리셋
```

### 자원 없음 (시체 자체가 메카닉)
- 명시적 자원 대신 **"시체의 현재 스킬 풀"** 자체가 자원
- UI: 시체 스킬 4개 패널 표시 (ResourceBadge 대신)
- 강화 상태 (Empower Stack)는 별도 추적 — Necromancer 본인의 버프 형태

### 기존 11종 자원과의 차별화 ⭐
| 자원 | 본질 |
|------|------|
| Ember/Vengeance/Shadows/Combo | 개인 행동 |
| Prophecy/Charge/Frost | 시간/공간/자원 축 |
| Mercy/Melody | 파티 중심 |
| Alchemist | 발견 (선택 기반) |
| **Necromancer** | **"동적 스킬 풀 + 자동 전투" (적 처치로 시체 성장)** ⭐ |

→ 완전히 새로운 축. **"시체의 스킬 풀"** 자체가 자원

## 6. Necromancer 본인 스킬 4종 (시체 강화 마법)

| 스킬 | AP | 효과 | 조건 |
|------|----|------|------|
| **Empower Undead** (강령술 강화) | 1 | 시체 다음 스킬 위력 +5 (1회 버프) | 셋업 |
| **Soul Link** (영혼 결속) | 2 | 2턴 동안 시체가 준 데미지 50%를 Necromancer HP 회복 | 자원 효율 |
| **Curse of Weakness** (약화 저주) | 2 | 단일 적 ATK-3 + DEF-3 (2턴) → 시체 처치 유도 | 대상 상태 |
| **Mass Empower** (대량 강화) | 3 | 시체 4스킬 전부 위력 +3 (이번 전투 영구) | 자원 임계 |

### 조건 다양성 검증 (4.5 원칙 2)
- Empower Undead → 셋업 (조건 없음)
- Soul Link → 자원 효율 (지속 효과)
- Curse of Weakness → 대상 상태 (적 약화)
- Mass Empower → 자원 임계 (AP 3 고비용)

→ 4개 모두 다른 목적. 매 턴 다른 퍼즐. ✅

### 기존 스킬 매핑
| 기존 | 신규 | 변경 |
|------|------|------|
| Necro_LifeDrain (10, AP2) | Soul Link | 흡혈 → 시체 딜의 50% 회복 (간접 흡혈) |
| Necro_Curse (AtkDown, AP1) | Curse of Weakness | AtkDown → AtkDown+DefDown 이중 약화 |
| Necro_Decay (4+Poison, AP1) | Empower Undead | 도트 → 시체 강화 (역할 정규화) |
| Necro_RaiseDead (광역 7, AP3) | Mass Empower | 광역 공격 → 시체 대량 강화 |

## 7. BehaviorTag / 시스템 설계

### 신규 인프라 필요
1. **CorpseComponent** (`Characters/Components/`)
   - `SkillData[] CorpseSkillSlots` (4 슬롯)
   - `bool IsActive` — Necromancer 생존 시 true
   - `int EmpowerBonus` — 강화 위력 가산 (일시적/영구)
   - 매 턴 종료 후 무작위 슬롯 스킬 시전 메서드
2. **CorpseSkillExecutor** (서비스)
   - 시체 자동 행동 — TurnManager가 매 턴 종료 후 호출
   - SkillExecutor.ExecuteSkillInternal 재사용 (시체가 caster)
3. **DiscoverModalUI 재사용** (Alchemist와 공유)
   - 적 처치 시: 처치한 적의 스킬 4개 표시 → 플레이어 1개 선택
   - 시체 슬롯 4개 중 어느 것 교체할지 선택 (또는 자동)
4. **SkillData 확장**
   - `isCorpseSkill: bool` 플래그 (시체 기본 스킬 식별)
   - `sourceEnemyId: string` (어떤 적에게서 빼앗은 스킬인지 추적)
5. **TurnManager 확장**
   - 매 턴 종료 후 `CorpseComponent.ExecuteRandomSkill()` 호출
   - Necromancer 사망 감지 → 시체 비활성화

### 기존 BehaviorTag 활용
- Lifesteal Behavior (Soul Link 회복 처리)
- 각 시체 스킬은 기존 SkillData 구조 그대로 (Scratch=Attack, Poison Bite=Attack+Poison 등)

## 8. 장착 특성 3종 리워크

| 특성 | 기존 효과 | 리워크 효과 | 해금 |
|------|---------|------------|------|
| **생명력 흡수** (기본) | 준 데미지 15% 회복 | **Soul Link 회복 50% → 75%** (강화 흡혈) | 기본 |
| **저주의 대가** | 버프/디버프 ×1.3 | **Curse of Weakness 적이 받는 추가 데미지 +3** (시체 처치 가속) | 30 조각 |
| **죽음의 수확** | 킬당 ATK +1 누적 | **적 처치 시 시체 스킬 교체 + 영구 강화 +2** (시체 스노우볼) | 60 조각 + 1 영혼 |

### 특성 키워드 매핑 (신규)
| 특성 | KeywordType | Trigger | Value |
|------|------------|---------|-------|
| 생명력 흡수 | **`SoulLinkMul`** (신규) | Passive | 0.75 (회복 배율) |
| 저주의 대가 | **`CurseExtraDamage`** (신규) | Passive | 3 (추가 데미지) |
| 죽음의 수확 | **`CorpseKillEmpower`** (신규) | Passive | 2 (처치 시 강화 +) |

## 9. 밸런스 시나리오 (다수전 5턴 — Mortis + 시체)

```
전투 시작: 시체 소환 (Scratch/Poison Bite/Bone Toss/Stun Strike)
턴 1: Empower Undead → 시체 다음 스킬 위력 +5
       플레이어 종료 → 시체 무작위 스킬 (Poison Bite +5 = 위력 8) → 적 A에게 8 데미지 + Poison
턴 2: Curse of Weakness (적 A) → ATK-3, DEF-3
       시체 Bone Toss → 적 A에게 4 + Bleed (DEF-3로 인해 추가 데미지)
       → 적 A 사망 → Discover Modal: Goblin_Scratch/Bite/Steal/Hide → Goblin_Bite 선택
       → 시체 슬롯 2 (Poison Bite)를 Goblin_Bite로 교체
턴 3: Soul Link → 시체 딜 50% 회복 설정
       시체 Goblin_Bite → 적 B에게 강한 데미지 → Mortis 회복
턴 4: Mass Empower → 시체 4스킬 전부 위력 +3 (이번 전투 영구)
       시체 Scratch (위력 4+3=7) → 적 B 처치 → 또 스킬 교체
턴 5: 시체 강화 스킬들로 적 잔당 정리
```

**비교 — Necromancer 단독 딜 vs 시체 포함**:
```
Necromancer 본인 딜: Empower/Curse/Soul Link/Mass Empower (딜 0)
시체 자동 딜: 매 턴 4-8 데미지 × 5턴 = 20-40
시체 스노우볼: 적 처치로 강한 스킬 획득 → 후반 폭발
```

→ Necromancer는 "간접 딜러" — 본인은 버프만, 시체가 실제 딜

## 10. 파티 시너지

| 조합 | 시너지 | 핵심 |
|------|-------|------|
| **Mortis + Cael (Alchemist)** | ★★★ | Cael이 Discover UI 인프라 공유. UI 작업 중복 제거 |
| **Mortis + Ashe** | ★★★ | Ashe Burn + 시체 Poison = 다중 도트. Soul Link로 Ashe 자해 회복 |
| **Mortis + Umbra** | ★★ | Umbra 도트 + 시체 도트 → 적 약화 극대화 |
| **Mortis + Lumi** | ★★ | Lumi Freeze로 적 묶기 → 시체가 안전하게 도트 부여 |
| **Mortis + Elara** | ★★ | Elara Mercy 자동 힐 + 시체 자동 딜 = 완전 자동화 파티 |
| **Mortis + Calliope** | ★★ | Calliope Anthem ATK+ → 시체 딜 강화 |

## 11. ✅ 결정 항목 (2026-07-17 확정)

- [x] **이름**: Mortis (라틴 "죽음")
- [x] **자원**: 없음 (시체 자체가 메카닉)
- [x] **시체 수**: 1체
- [x] **시체 HP**: 없음 (적의 대상 안 됨)
- [x] **시체 사망**: Necromancer 사망 시 동반 사망
- [x] **시체 행동**: 매 턴 플레이어 종료 후, 4스킬 중 무작위 1개 자동 시전
- [x] **스킬 교체**: 적 처치 시 Discover UI (Alchemist 재사용)로 선택
- [x] **리셋**: 전투 종료 시 시체 사라지고 다음 전투 기본 4스킬로 재소환
- [x] **Necromancer 스킬 4종**: Empower / Soul Link / Curse / Mass Empower (시체 강화)
- [x] **시체 기본 스킬 4종**: Scratch / Poison Bite / Bone Toss / Stun Strike

## 12. 리스크와 검증

| 리스크 | 완화 |
|-------|-------|
| 시체 자동 행동이 매 턴 전투 흐름 느림 | 빠른 애니메이션 + 2x 속도 모드 지원 |
| 시체 스킬이 랜덤이라 전략 어려움 | Empower/Mass Empower로 강화. 교체로 커스터마이징 |
| Discover UI 두 번 (Alch + Necro) 인프라 공유 필요 | Phase CC-2E-1 인프라 먼저 구축 → Necromancer는 재사용 |
| 시체 너무 강하면 매 턴 무료 딜 사기 | Necromancer 본인 딜 0으로 밸런스. AP는 강화 스킬에 사용 |
| 시체가 처치 못 하면 스킬 교체 안 됨 | Curse of Weakness로 적 약화 유도. 기본 4스킬로도 충분 |
| 보스전 단일 적에서 스킬 교체 1회로 끝 | 다수전 강점 명확화. 보스전은 기본 4스킬 + Mass Empower로 |
| CC-0 부활 시 시체 리셋 = 페널티 | 부활 후 Raise Corpse 패시브로 자동 재소환 (기본 4스킬). 영구 강화는 손실 |
| Necromancer 사망 시 시체도 사망 → 부활 시 다시 기본 | CC-0 부활 페널티와 일관. 의도적 약점 |

## 13. 구현 메모 (코드 구현 시 — 별도 Phase CC-2F)

### ★ 구현 복잡도: Alchemist(CC-2E) 이후 진행 권장 (Discover UI 인프라 공유)

### 신규 인프라 필요
1. **CorpseComponent** (`Characters/Components/`)
   - `SkillData[] CorpseSkillSlots` (4 슬롯, 기본 스킬로 초기화)
   - `bool IsActive` — Necromancer 생존 시 true, 사망 시 false
   - `int EmpowerBonusNext` (다음 스킬 위력 가산)
   - `int MassEmpowerBonus` (영구 가산)
   - 매 턴 ExecuteRandomSkill(corpse, enemies) 호출
2. **TurnManager 확장**
   - 매 턴 플레이어 종료 후 시체 자동 행동 호출
   - Necromancer 사망 감지 → CorpseComponent.IsActive = false
3. **CombatEventBus.OnKill 확장** — 적 처치 시 Discover Modal 호출
4. **DiscoverModalUI 재사용** (CC-2E에서 구축)
   - 적 스킬 4개 표시 → 선택 → 시체 슬롯 교체 (슬롯 선택 모달 추가)
5. **SkillData 확장**
   - `isCorpseSkill: bool` / `sourceEnemyId: string`
6. **Character.CreateResource** — Necromancer은 자원 없음 (Resource=null). 대신 `InitializeCorpse()` 호출
7. **KeywordType 3종 신규**: SoulLinkMul / CurseExtraDamage / CorpseKillEmpower

### 기존 코드 수정
- `DataGenerator.PhaseCC.cs` — Mortis 스킬 4종 + Char_Necromancer 재생성. **시체 기본 스킬 4종 .asset 별도 생성**
- `DataGenerator.Traits.cs` — Necromancer 특성 3종 리워크
- `BattleDisplayUtil.cs` — 시체 스킬 패널 표시 (UI 별도)
- CSV: Char_Necromancer 행 + Necro_* 4행 제거

### 구현 난이도 추정
- CorpseComponent + 자동 행동: 중상 (~120줄)
- TurnManager 시체 턴 통합: 중 (~50줄)
- Discover Modal 스킬 교체 (Alchemist UI 재사용 + 슬롯 선택): 중 (~80줄)
- 시체 스킬 4종 .asset: 낮음
- 특성 키워드 3종: 낮음 (~30줄)
- DataGenerator/UI: 낮음 (~50줄)
- **총합**: 약 330줄 + 8 스킬 .asset (본인 4 + 시체 4)
- Alchemist(CC-2E) 인프라 선구축 시 복잡도 감소

### 별도 Phase 진행 권장
- **Phase CC-2F-1**: 인프라 (CorpseComponent + TurnManager 통합)
- **Phase CC-2F-2**: Mortis 스킬 4종 + 시체 기본 스킬 4종 + 특성 3종
- **Phase CC-2F-3**: Discover Modal 스킬 교체 (CC-2E-1 이후)
- **Phase CC-2F-4**: 테스트 + Play 모드 검증

### 권장 진행 순서
1. **CC-2E (Alchemist)** — Discover UI 인프라 구축
2. **CC-2F (Necromancer)** — CC-2E의 Discover UI 재사용

### 테스트 계획 (PhaseCC2FTests.cs 신규)
1. CorpseComponent: 기본 4스킬 초기화, 자동 행동
2. 시체 자동 시전: 매 턴 종료 후 무작위 스킬
3. Necromancer 사망 → 시체 비활성화
4. 적 처치 → Discover Modal → 스킬 교체
5. Empower Undead: 다음 스킬 위력 +
6. Mass Empower: 영구 가산
7. Soul Link: 시체 딜 → Necromancer 회복

---

## 변경 이력

| 날짜 | 변경 |
|------|------|
| 2026-07-14 | 최초 작성 (Soul + 미니언 시스템 컨셉, 🔴 초안) |
| 2026-07-17 | **Summoned Corpse (동적 스킬 풀) 컨셉으로 전면 재작성** (사용자 제안). 시체 HP 없음 + Necromancer 사망 시 동반 사망. Discover UI로 스킬 교체. 🟢 확정. 단 구현은 별도 Phase CC-2F 권장 (CC-2E 인프라 공유) |
