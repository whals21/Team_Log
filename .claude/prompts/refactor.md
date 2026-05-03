# 리팩토링 프롬프트 템플릿

## 사용법
기존 코드를 개선할 때 아래 형식으로 AI에게 요청하세요.

---

## 입력 양식

```
### 리팩토링 대상
[파일 경로 또는 시스템 이름]

### 목적
[성능 개선 / 가독성 향상 / 중복 제거 / 확장성 확보 중 선택]

### 상세 설명
[무엇을 어떻게 개선하고 싶은지]
```

## AI 작업 프로세스

### Step 1: 영향 범위 분석
1. 리팩토링 대상 파일의 모든 참조 검색
2. public API 변경 여부 확인
3. 이벤트 구독자 파악
4. 하위 호환성 영향 평가

### Step 2: 리팩토링 원칙
- **동작 변경 없음**: 외부에서 관찰 가능한 동작은 동일하게 유지
- **점진적 변경**: 한 번에 하나의 관심사만 수정
- **테스트 보장**: 수정 전후 동일한 시나리오로 검증
- **기존 패턴 준수**: 프로젝트 컨벤션에서 벗어나지 않음

### Step 3: 일반적인 리팩토링 패턴

#### 컴포넌트 분리
```
Before: 하나의 큰 MonoBehaviour
After: 순수 C# 로직 클래스 + MonoBehaviour 래퍼

예시 패턴 (Character.cs 참고):
- Character (순수 C#) ← 로직
- CharacterData (ScriptableObject) ← 데이터
- CharacterUIPanel (MonoBehaviour) ← 표시
```

#### 이벤트 도입
```
Before: A가 B를 직접 호출
After: A가 이벤트 발생 → B가 구독

// 패턴
public event Action<int> OnHPChanged;
public event Action OnDeath;
```

#### 매직넘버 제거
```
Before: if (hp <= 0)
After:  if (healthComponent.IsDead)

Before: damage = atk + 15 - def;
After:  damage = statComponent.ATK + skill.Power - target.StatComponent.DEF;
```

### Step 4: 검증
1. 컴파일 에러 없는지 확인
2. 기존 씬에서 정상 동작 확인
3. 이벤트 연결이 끊어지지 않았는지 확인
4. ScriptableObject 참조가 유효한지 확인

## 주의사항

- BattleUISceneBuilder (~1700줄) 리팩토링 시 특별히 주의
  - 프로그래박틱 UI 구성이므로 구조 변경이 씬 전체에 영향
- EffectProcessor는 static 클래스 — 확장 시 인스턴스화 필요성 검토
- BattleEventManager는 싱글톤 — 다른 싱글톤과의 의존성 주의
