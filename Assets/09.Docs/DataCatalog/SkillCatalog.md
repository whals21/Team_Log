# 스킬 데이터 카탈로그

> 최종 갱신: 2026-06-04
> 데이터 소스: `Assets/03.Data/Tables/SkillTable.csv`
> 데미지 공식: `max(1, ATK + Power - DEF)` (Attack 타입)
> AP 시스템: 적 스킬은 Cost=0 (AP 미사용), 플레이어 스킬은 Cost 1~3

---

## 스킬 타입 정의

| SkillType | 설명 | 동작 |
|-----------|------|------|
| Attack | 공격 | 대상에게 데미지 |
| Heal | 치유 | 대상의 HP 회복 |
| Shield | 쉴드 | 대상에게 보호막 부여 |
| Buff | 강화 | 대상에게 긍정 상태이상 부여 |
| Debuff | 약화 | 대상에게 부정 상태이상 부여 |
| Purify | 정화 | 대상의 모든 상태이상 제거 |

## 타겟 타입 정의

| TargetType | 설명 |
|------------|------|
| SingleEnemy | 적 1명 |
| AllEnemies | 모든 적 |
| Self | 자신 |
| SingleAlly | 아군 1명 |
| AllAllies | 모든 아군 |

## 상태이상 정의

| StatusEffectType | 분류 | 효과 |
|------------------|------|------|
| Poison | 도트 | 매 턴 시작 시 value 피해 |
| Burn | 도트 | 매 턴 시작 시 value 피해 |
| Bleed | 도트 | 매 턴 시작 시 value×stacks 피해 |
| Regeneration | 도트 | 매 턴 시작 시 value×stacks 회복 |
| Stun | 제어 | 행동 불가 (1턴) |
| Freeze | 제어 | 행동 불가 + 받는 피해 증가 |
| Sleep | 제어 | 행동 불가, 피격 시 해제 |
| Taunt | 행동 | 적의 공격 대상 고정 |
| AttackUp | 버프 | ATK +value |
| AttackDown | 디버프 | ATK -value |
| DefenseUp | 버프 | DEF +value |
| DefenseDown | 디버프 | DEF -value |

---

## 1. 플레이어 스킬 (16종)

### 전사 (Warrior)

| ID | 이름 | 타입 | 타겟 | Power | Cost | Weight | 상태이상 |
|----|------|------|------|-------|------|--------|---------|
| Warrior_Strike | 강타 | Attack | 단일적 | 5 | 1 | 40 | — |
| Warrior_Shield | 방패 방어 | Shield | 자신 | 15 | 1 | 30 | — |
| Warrior_Taunt | 도발 | Buff | 자신 | 0 | 1 | 20 | Taunt 1턴 |
| Warrior_Rage | 분노 | Buff | 자신 | 4 | 2 | 10 | AttackUp 1턴 (+4) |

**최대 위력 계산** (ATK 8 기준):
- 강타: 8 + 5 = 13 데미지
- 분노 후 강타: 12 + 5 = 17 데미지

---

### 마법사 (Mage)

| ID | 이름 | 타입 | 타겟 | Power | Cost | Weight | 상태이상 |
|----|------|------|------|-------|------|--------|---------|
| Mage_Fireball | 파이어볼 | Attack | 단일적 | 8 | 2 | 35 | Burn 2턴 (3/턴) |
| Mage_IceSpear | 얼음창 | Attack | 단일적 | 6 | 1 | 35 | — |
| Mage_MagicShield | 마법 방어막 | Shield | 자신 | 12 | 1 | 20 | — |
| Mage_Meteor | 메테오 | Attack | 전체적 | 10 | 3 | 10 | — |

**최대 위력 계산** (ATK 12 기준):
- 파이어볼: 12 + 8 = 20 데미지 + Burn 6 (총 26)
- 메테오: 12 + 10 = 22 데미지 × 전체

---

### 힐러 (Healer)

| ID | 이름 | 타입 | 타겟 | Power | Cost | Weight | 상태이상 |
|----|------|------|------|-------|------|--------|---------|
| Healer_Heal | 치유 | Heal | 단일아군 | 15 | 2 | 40 | — |
| Healer_Barrier | 보호막 | Shield | 단일아군 | 10 | 1 | 25 | — |
| Healer_Purify | 정화 | Purify | 단일아군 | 0 | 1 | 20 | — |
| Healer_Blessing | 축복 | Buff | 단일아군 | 2 | 2 | 15 | AttackUp 2턴 (+3) |

---

### 도적 (Rogue)

| ID | 이름 | 타입 | 타겟 | Power | Cost | Weight | 상태이상 |
|----|------|------|------|-------|------|--------|---------|
| Rogue_Backstab | 급소 공격 | Attack | 단일적 | 8 | 2 | 35 | — |
| Rogue_PoisonBlade | 독 바르기 | Debuff | 단일적 | 3 | 1 | 25 | Poison 3턴 (3/턴) |
| Rogue_Weaken | 약화 | Debuff | 단일적 | 0 | 1 | 20 | DefenseDown 2턴 (-3) |
| Rogue_DoubleStrike | 이중 타격 | Attack | 단일적 | 4 | 1 | 20 | — |

**최대 위력 계산** (ATK 10 기준):
- 급소 공격: 10 + 8 = 18 데미지
- 독 바르기: 10 + 3 = 13 데미지 + Poison 9 (총 22)

---

## 2-1. 기존 일반 적 스킬 (24종)

### 슬라임

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Slime_Tackle | 몸통박치기 | Attack | 단일 | 3 | — | 50 |
| Slime_AcidSpit | 산성 침 | Attack | 단일 | 2 | Poison 2턴 (2/턴) | 30 |
| Slime_Split | 분열 준비 | Shield | 자신 | 5 | — | 20 |
| Slime_Jiggle | 출렁임 | Buff | 자신 | 0 | — | 10 |

---

### 고블린

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Goblin_Scratch | 긁기 | Attack | 단일 | 4 | — | 40 |
| Goblin_Bite | 물기 | Attack | 단일 | 6 | — | 30 |
| Goblin_Steal | 약화 공격 | Attack | 단일 | 3 | AttackDown 1턴 (-2) | 20 |
| Goblin_Hide | 은신 | Shield | 자신 | 4 | — | 10 |

---

### 해골 전사

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Skeleton_Slash | 베기 | Attack | 단일 | 4 | — | 35 |
| Skeleton_BoneThrow | 뼈 투척 | Attack | 단일 | 3 | — | 25 |
| Skeleton_DefensiveStance | 방어 태세 | Shield | 자신 | 6 | — | 20 |
| Skeleton_Rattle | 뼈 울림 | Debuff | 단일 | 0 | DefenseDown 1턴 (-2) | 20 |

---

### 늑대

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Wolf_Bite | 물기 | Attack | 단일 | 5 | — | 35 |
| Wolf_Howl | 울부짖기 | Buff | 자신 | 0 | AttackUp 1턴 (+3) | 25 |
| Wolf_Lunge | 돌진 | Attack | 단일 | 7 | — | 25 |
| Wolf_Sniff | 냄새 맡기 | Buff | 자신 | 0 | — | 15 |

---

### 독버섯

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Mushroom_Spore | 포자 확산 | Debuff | 전체 | 0 | Poison 2턴 (2/턴) | 30 |
| Mushroom_Headbutt | 박치기 | Attack | 단일 | 3 | — | 25 |
| Mushroom_Regrow | 재생 | Heal | 자신 | 5 | — | 20 |
| Mushroom_ToxicMist | 맹독 안개 | Debuff | 단일 | 0 | Poison 2턴 (3/턴) | 25 |

---

### 박쥐

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Bat_Screech | 초음파 | Attack | 전체 | 2 | — | 30 |
| Bat_DrainLife | 흡혈 | Attack | 단일 | 3 | — | 25 |
| Bat_Evasion | 회피 비행 | Shield | 자신 | 4 | — | 20 |
| Bat_Curse | 저주 소리 | Debuff | 단일 | 0 | AttackDown 1턴 (-2) | 25 |

---

## 3-1. 기존 엘리트 적 스킬 (12종)

### 정예 기사

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| EliteKnight_Slash | 참격 | Attack | 단일 | 8 | — | 35 |
| EliteKnight_ShieldBash | 방패 들이밀기 | Shield | 자신 | 12 | Stun 1턴 | 25 |
| EliteKnight_Taunt | 도발 | Buff | 자신 | 0 | Taunt 1턴 | 20 |
| EliteKnight_Warcry | 전투 함성 | Buff | 자신 | 0 | AttackUp 2턴 (+4) | 20 |

> ShieldBash는 Shield 타입이지만 Stun 부여. 쉴드 12 획득 + 타겟 기절.

---

### 마법사 고블린

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| EliteMage_Fireball | 화염구 | Attack | 단일 | 10 | Burn 2턴 (4/턴) | 30 |
| EliteMage_Blizzard | 눈보라 | Attack | 전체 | 6 | Freeze 1턴 | 25 |
| EliteMage_Barrier | 마법 장벽 | Shield | 자신 | 10 | — | 20 |
| EliteMage_Drain | 흡수 | Attack | 단일 | 6 | — | 25 |

---

### 암흑 슬라임

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| DarkSlime_CorruptStrike | 오염 공격 | Attack | 단일 | 7 | — | 30 |
| DarkSlime_PoisonCloud | 독 구름 | Debuff | 전체 | 0 | Poison 3턴 (4/턴) | 25 |
| DarkSlime_DarkShield | 어둠의 방패 | Shield | 자신 | 8 | — | 20 |
| DarkSlime_Absorb | 흡수 | Heal | 자신 | 8 | — | 25 |

---

## 4. 보스 적 스킬 (12종)

### 고블린 왕

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| BossGKing_CrownStrike | 왕관 강타 | Attack | 단일 | 10 | — | 30 |
| BossGKing_Rally | 소집 | Buff | 자신 | 0 | AttackUp 2턴 (+5) | 20 |
| BossGKing_Warcry | 전쟁 함성 | Buff | 자신 | 0 | AttackUp 2턴 (+4) | 20 |
| BossGKing_RoyalGuard | 왕실 근위대 | Shield | 자신 | 18 | — | 30 |

---

### 드래곤

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| BossDragon_Breath | 드래곤 브레스 | Attack | 전체 | 12 | Burn 2턴 (5/턴) | 30 |
| BossDragon_TailSwipe | 꼬리 휘두르기 | Attack | 단일 | 14 | — | 25 |
| BossDragon_FireStorm | 화염 폭풍 | Attack | 전체 | 10 | Burn 2턴 (4/턴) | 20 |
| BossDragon_DragonFear | 드래곤 공포 | Debuff | 전체 | 0 | DefenseDown 2턴 (-4) | 25 |

---

### 마왕

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| BossDL_DarkBlade | 암흑의 검 | Attack | 단일 | 16 | — | 30 |
| BossDL_Curse | 저주 | Debuff | 전체 | 0 | AttackDown 2턴 (-5) | 25 |
| BossDL_Meteor | 마왕의 메테오 | Attack | 전체 | 14 | — | 20 |
| BossDL_DarkBarrier | 암흑 방어막 | Shield | 자신 | 20 | — | 25 |

---

## 2-2. 신규 일반 적 스킬 (24종)

### 미라

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Mummy_Wrap | 붕대 감기 | Attack | 단일 | 4 | Poison 2턴 (2/턴) | 35 |
| Mummy_Curse | 저주 | Debuff | 단일 | 0 | DefenseDown 2턴 (-2) | 25 |
| Mummy_Bandage | 붕대 보호 | Shield | 자신 | 6 | — | 20 |
| Mummy_Sandstorm | 모래 폭풍 | Debuff | 전체 | 0 | AttackDown 1턴 (-1) | 20 |

---

### 해골 궁수

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Archer_Precision | 정밀 사격 | Attack | 단일 | 5 | — | 35 |
| Archer_PoisonArrow | 독화살 | Attack | 단일 | 3 | Poison 3턴 (3/턴) | 25 |
| Archer_Retreat | 후퇴 | Shield | 자신 | 4 | — | 20 |
| Archer_Aim | 조준 | Buff | 자신 | 0 | AttackUp 1턴 (+3) | 20 |

---

### 망령

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Wraith_Drain | 영혼 흡수 | Attack | 단일 | 5 | — | 35 |
| Wraith_Chill | 차가운 손길 | Attack | 단일 | 4 | — | 25 |
| Wraith_Phase | 위상 이동 | Shield | 자신 | 5 | — | 20 |
| Wraith_Wail | 죽음의 울부짖음 | Debuff | 전체 | 0 | DefenseDown 1턴 (-2) | 20 |

---

### 그림자

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Shadow_Blade | 그림자 칼날 | Attack | 단일 | 6 | — | 35 |
| Shadow_Smoke | 연막 | Shield | 자신 | 5 | — | 20 |
| Shadow_Hide | 은신 | Buff | 자신 | 0 | — | 15 |
| Shadow_Strike | 암습 | Attack | 단일 | 4 | — | 30 |

---

### 악마 병사

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Demon_Sword | 마도검 | Attack | 단일 | 5 | — | 35 |
| Demon_FireSlash | 화염 베기 | Attack | 단일 | 4 | Burn 2턴 (3/턴) | 25 |
| Demon_Shield | 방패 막기 | Shield | 자신 | 7 | — | 20 |
| Demon_Warcry | 전투 함성 | Buff | 자신 | 0 | AttackUp 1턴 (+3) | 20 |

---

### 가고일

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Gargoyle_Smash | 석조 일격 | Attack | 단일 | 4 | — | 35 |
| Gargoyle_Spike | 돌 가시 | Attack | 단일 | 3 | — | 25 |
| Gargoyle_StoneArmor | 석화 방어 | Shield | 자신 | 8 | — | 20 |
| Gargoyle_PoisonGas | 독가스 | Debuff | 전체 | 0 | Poison 2턴 (2/턴) | 20 |

---

## 3-2. 신규 엘리트 적 스킬 (12종)

### 고블린 주술사

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Shaman_Lightning | 번개 창 | Attack | 단일 | 9 | — | 35 |
| Shaman_HealRain | 치유의 비 | Heal | 자신 | 8 | — | 25 |
| Shaman_BloodRitual | 피의 의식 | Buff | 자신 | 0 | AttackUp 2턴 (+5) | 20 |
| Shaman_Totem | 토템 방어 | Shield | 자신 | 10 | — | 20 |

---

### 해골 대장

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| Captain_Strike | 지휘 타격 | Attack | 단일 | 7 | — | 35 |
| Captain_ShieldWall | 방패 벽 | Shield | 자신 | 14 | — | 25 |
| Captain_Warcry | 전투 함성 | Buff | 자신 | 0 | AttackUp 2턴 (+4) | 20 |
| Captain_BoneShield | 뼈 방패 | Buff | 자신 | 0 | DefenseUp 1턴 (+4) | 20 |

---

### 악마 마법사

| ID | 이름 | 타입 | 타겟 | Power | 상태이상 | Weight |
|----|------|------|------|-------|---------|--------|
| DemonMage_Hellfire | 지옥불 | Attack | 단일 | 11 | Burn 2턴 (5/턴) | 30 |
| DemonMage_DarkPulse | 암흑 파동 | Attack | 전체 | 7 | — | 25 |
| DemonMage_CurseBarrier | 저주받은 결계 | Shield | 자신 | 10 | — | 20 |
| DemonMage_SoulBurn | 영혼 불태우기 | Debuff | 단일 | 0 | DefenseDown 2턴 (-3) | 25 |

---

## 5. 스킬 통계

### 총 스킬 수

| 카테고리 | Attack | Shield | Heal | Buff | Debuff | Purify | 합계 |
|----------|--------|--------|------|------|--------|--------|------|
| 플레이어 | 6 | 3 | 1 | 3 | 2 | 1 | 16 |
| 일반 적 (기존) | 12 | 6 | 1 | 4 | 6 | 0 | 29 |
| 일반 적 (신규) | 12 | 6 | 0 | 4 | 6 | 0 | 28 |
| 엘리트 (기존) | 6 | 3 | 1 | 2 | 1 | 0 | 13 |
| 엘리트 (신규) | 4 | 3 | 1 | 3 | 1 | 0 | 12 |
| 보스 | 6 | 3 | 0 | 2 | 2 | 0 | 13 |
| **합계** | **48** | **20** | **6** | **18** | **16** | **1** | **101** |

### Power 범위

| 등급 | Attack Power | Shield Power | Heal Power |
|------|-------------|-------------|------------|
| 플레이어 | 4~10 | 10~15 | 15 |
| 일반 적 | 2~7 | 4~8 | 5 |
| 엘리트 | 6~11 | 8~14 | 8 |
| 보스 | 10~16 | 18~20 | — |

### 상태이상 빈도

| 상태이상 | 사용 스킬 수 | 주요 사용자 |
|---------|-------------|------------|
| Poison | 12 | 슬라임, 독버섯, 도적, 암흑 슬라임, 미라, 해골 궁수, 가고일 |
| Burn | 8 | 마법사, 마법사 고블린, 드래곤, 악마 병사, 악마 마법사 |
| DefenseDown | 8 | 도적, 해골, 암흑 슬라임(특성), 드래곤, 미라, 망령, 악마 마법사 |
| AttackDown | 5 | 고블린, 박쥐, 마왕, 미라 |
| AttackUp | 14 | 전사, 힐러, 늑대, 정예 기사, 마법사 고블린, 고블린 왕, 해골 궁수, 악마 병사, 고블린 주술사, 해골 대장 |
| DefenseUp | 1 | 해골 대장 |
| Stun | 1 | 정예 기사 |
| Freeze | 1 | 마법사 고블린 |
| Taunt | 2 | 전사, 정예 기사 |
