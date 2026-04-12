# 전투 시스템 테스트 씬 제작 가이드

## 1. 개요

이 문서는 현재 구현된 전투 시스템을 테스트하기 위한 씬 제작 가이드입니다.

### 구현된 시스템

| 시스템 | 위치 |
|--------|------|
| Character | `02.Scripts/Characters/` |
| Skill | `02.Scripts/Skill/` |
| Turn | `02.Scripts/Combat/Turn/` |
| Draw | `02.Scripts/Combat/Draw/` |
| StatusEffect | `02.Scripts/Combat/StatusEffect/` |
| AI | `02.Scripts/Combat/AI/` |

---

## 2. ScriptableObject 생성 순서

### 2.1 스킬 데이터 생성

**경로**: `Assets/03.Data/Skills/`

1. 우클릭 → Create → TeamLog → Skill Data
2. 파일명: `Skill_Fireball` 등

**설정 예시**:
```
Skill Name: 파이어볼
Description: 적에게 불꽃 데미지를 입힙니다.
Type: Attack
Target: SingleEnemy
Power: 15
Cost: 0
Weight: 50
Status Effect: Burn
Effect Duration: 2
Effect Value: 5
```

### 2.2 캐릭터 데이터 생성

**경로**: `Assets/03.Data/Characters/`

1. 우클릭 → Create → TeamLog → Character Data
2. 파일명: `Char_Warrior` 등

**설정 예시**:
```
Character Name: 전사
Class: Warrior
Description: 근접 전투 전문가
Base HP: 120
Base ATK: 15
Base DEF: 10
Skills: [강타, 방패, 도발, 분노]
```

---

## 3. 테스트 씬 구성

### 3.1 씬 구조

```
TestCombatScene.unity
├── Main Camera
├── EventSystem
├── Canvas
│   ├── TopBar (턴 정보, 리롤 버튼)
│   ├── PartyArea (플레이어 캐릭터 4명)
│   ├── EnemyArea (적 캐릭터)
│   ├── SkillDrawArea (드로우된 스킬 4개)
│   ├── ActionQueueArea (실행 순서)
│   └── LogArea (전투 로그)
└── Managers
    └── TestCombatManager
```

### 3.2 필수 컴포넌트

| GameObject | 컴포넌트 |
|------------|----------|
| Canvas | Canvas, Canvas Scaler |
| TestCombatManager | TestCombatManager (스크립트) |

---

## 4. 테스트 매니저 코드

### 4.1 기본 구조

```csharp
// TestCombatManager.cs
using UnityEngine;
using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Combat.Turn;
using TeamLog.Combat.AI;

public class TestCombatManager : MonoBehaviour
{
    [Header("캐릭터 데이터")]
    [SerializeField] private CharacterData[] _partyData;
    [SerializeField] private CharacterData[] _enemyData;

    [Header("참조")]
    [SerializeField] private Transform _partyArea;
    [SerializeField] private Transform _enemyArea;

    private List<Character> _party;
    private List<Character> _enemies;
    private TurnManager _turnManager;
    private List<EnemyAIController> _enemyControllers;

    private void Start()
    {
        InitializeBattle();
    }

    private void InitializeBattle()
    {
        // 1. 파티 생성
        _party = new List<Character>();
        foreach (var data in _partyData)
            _party.Add(new Character(data));

        // 2. 적 생성
        _enemies = new List<Character>();
        foreach (var data in _enemyData)
            _enemies.Add(new Character(data));

        // 3. 턴 매니저 생성
        _turnManager = new TurnManager(_party, _enemies);
        _turnManager.OnPhaseChanged += OnPhaseChanged;
        _turnManager.OnTurnStarted += OnTurnStarted;

        // 4. 적 AI 컨트롤러 생성
        _enemyControllers = new List<EnemyAIController>();
        foreach (var enemy in _enemies)
        {
            var pattern = CreateDefaultPattern();
            var controller = new EnemyAIController(enemy, pattern, _party);
            _enemyControllers.Add(controller);
        }

        // 5. 전투 시작
        _turnManager.StartBattle();
    }

    private EnemyActionPattern CreateDefaultPattern()
    {
        return EnemyActionPattern.CreateSimpleLoop(
            new EnemyActionNode(EnemyActionType.Attack, 10),
            new EnemyActionNode(EnemyActionType.Attack, 10),
            new EnemyActionNode(EnemyActionType.Defend, 5)
        );
    }

    private void OnPhaseChanged(TurnPhase oldPhase, TurnPhase newPhase)
    {
        Debug.Log($"[Phase] {oldPhase} → {newPhase}");
    }

    private void OnTurnStarted(int turnNumber)
    {
        Debug.Log($"[Turn] {turnNumber}턴 시작");
    }
}
```

---

## 5. 테스트 시나리오

### 5.1 기본 전투 플로우 테스트

1. **드로우 페이즈**
   - [ ] 각 캐릭터마다 스킬 1개가 드로우되는가?
   - [ ] 가중치가 적용되는가?

2. **액션 페이즈**
   - [ ] 스킬 순서 지정이 가능한가?
   - [ ] 타겟 지정이 가능한가?
   - [ ] 리롤이 작동하는가?

3. **실행 페이즈**
   - [ ] 지정된 순서대로 실행되는가?
   - [ ] 데미지 계산이 올바른가?

4. **적 턴**
   - [ ] 적 AI가 행동하는가?
   - [ ] 의도 표시가 작동하는가?

5. **상태이상**
   - [ ] 독/화상 데미지가 적용되는가?
   - [ ] 버프/디버프가 스탯에 반영되는가?

### 5.2 엣지 케이스 테스트

- [ ] 캐릭터 사망 시 처리
- [ ] 전체 파티 사망 → 게임 오버
- [ ] 전체 적 처치 → 전투 승리

---

## 6. 디버그 UI 구성 (선택)

### 6.1 턴 정보 표시

```
┌─────────────────────────────┐
│ Turn: 3/10  Phase: Action   │
│ Rerolls: 1/1   [Reroll All] │
└─────────────────────────────┘
```

### 6.2 캐릭터 상태 표시

```
┌──────────────┐
│ [전사] HP: 100│
│ ATK: 15 DEF: 10│
│ Effects: 없음  │
└──────────────┘
```

### 6.3 드로우된 스킬 표시

```
┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│전사    │ │마법사  │ │힐러    │ │도적    │
│강타    │ │파이어볼│ │치유    │ │급소공격│
│[1]    │ │[2]    │ │[3]    │ │[4]    │
└────────┘ └────────┘ └────────┘ └────────┘
```

---

## 7. 빠른 테스트용 Mock 데이터

### 7.1 최소 테스트 데이터

```
파티: 전사 1명
적: 슬라임 1명
스킬: 각 2개씩만
```

### 7.2 권장 테스트 데이터

```
파티: 전사, 마법사, 힐러, 도적
적: 슬라임 x2, 고블린 x1
스킬: 각 4개
```

---

## 8. 체크리스트

### 사전 준비

- [ ] SkillData 16개 이상 생성 (4캐릭터 × 4스킬)
- [ ] CharacterData 4개 이상 생성 (파티용)
- [ ] CharacterData 2개 이상 생성 (적용)
- [ ] 테스트 씬 생성
- [ ] TestCombatManager 스크립트 작성

### 테스트 실행

- [ ] 씬 플레이
- [ ] Console에 로그 출력 확인
- [ ] 드로우 → 액션 → 실행 → 적턴 사이클 확인

---

## 9. 참고 파일 경로

```
Assets/
├── 02.Scripts/
│   ├── Characters/
│   │   ├── Character.cs
│   │   ├── CharacterData.cs
│   │   └── Components/
│   ├── Skill/
│   │   └── SkillData.cs
│   └── Combat/
│       ├── Turn/
│       ├── Draw/
│       ├── StatusEffect/
│       └── AI/
├── 03.Data/
│   ├── Characters/
│   └── Skills/
└── 01.Scenes/
    └── TestCombatScene.unity
```
