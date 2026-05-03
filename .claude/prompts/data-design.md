# 데이터/콘텐츠 설계 프롬프트 템플릿

## 사용법
새 캐릭터, 스킬, 상태이상 등 게임 콘텐츠를 추가할 때 사용하세요.

---

## 입력 양식

```
### 추가할 콘텐츠 타입
[캐릭터 / 스킬 / 상태이상 / 적]

### 컨셉
[어떤 콘텐츠인지 설명]

### 밸런스 기준 (선택)
[기존 데이터와의 비교 기준]
```

## AI 작업 프로세스

### Step 1: 기존 데이터 분석
1. `03.Data/`의 기존 에셋 읽기
2. 밸런스 범위 파악:

```
기존 캐릭터 스탯 범위:
| 클래스 | HP | ATK | DEF |
|--------|-----|-----|-----|
| Warrior | 120 | 12 | 8 |
| Mage | 70 | 18 | 3 |
| Healer | 80 | 8 | 5 |
| Rogue | 75 | 15 | 4 |

기존 스킬 위력 범위:
- 기본 공격: 8-15
- 강공격: 20-30
- 힐: 25
- 버프/디버프: 0 (효과만)
```

### Step 2: 데이터 설계 원칙

#### 캐릭터 설계
- 클래스당 스킬 4개 (필수)
- HP: 50-150 범위, 역할에 따라 차등
- ATK: 5-20 범위
- DEF: 2-10 범위
- 적 캐릭터는 일반적으로 플레이어보다 낮은 스탯

#### 스킬 설계
- Weight (드로우 가중치): 1-10, 기본값 5
- Power: 역할에 맞는 범위
- Cost: 현재 미사용, 0으로 설정
- 상태이상: 지속 턴수 1-5, 값은 효과에 따라

#### 데미지 공식
```
damage = max(1, attacker.ATK + skill.Power - target.DEF)
```

### Step 3: 구현 체크리스트

#### 새 캐릭터 추가
- [ ] CharacterData ScriptableObject 생성 (`Assets/03.Data/Characters/`)
- [ ] 스킬 4개 SkillData 생성 (`Assets/03.Data/Skills/`)
- [ ] DataGenerator.cs에 생성 코드 추가
- [ ] UI 색상 매핑 확인 (CharacterUIPanel의 클래스 색상)

#### 새 스킬 추가
- [ ] SkillData ScriptableObject 생성
- [ ] SkillType enum에 새 타입 필요한지 확인
- [ ] TargetType이 적절한지 확인
- [ ] 상태이상이 새로운 타입인 경우:
  - StatusEffectType enum 추가
  - EffectProcessor에 로직 추가
  - UI 표시 텍스트 추가
- [ ] DataGenerator에 스킬 생성 코드 추가

#### 새 적 추가
- [ ] CharacterData 생성 (적용)
- [ ] EnemyActionPattern 정의
- [ ] EnemyAIController에 패턴 등록
- [ ] EnemyIntentType에 의도 표시 텍스트
- [ ] BattleSceneSetup에 적 배치 코드

### Step 4: 밸런스 검증
- 기존 파티(전사/마법사/힐러/도적) 대비 난이도 확인
- 평균 턴당 데미지 계산
- 상태이상 지속/효과가 과도하지 않은지 확인

## 데이터 파일 네이밍

```
캐릭터: Char_{ClassName} 또는 Enemy_{Name}
         예: Char_Warrior, Enemy_Slime

스킬:   {Class}_{SkillName}
         예: Warrior_Strike, Mage_Fireball, Slime_Tackle
```
