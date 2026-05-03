# 기능 추가 프롬프트 템플릿

## 사용법
새 기능을 추가할 때 아래 형식으로 AI에게 요청하세요.

---

## 입력 양식

```
### 기능명
[기능 이름]

### 설명
[어떤 기능인지 상세 설명]

### 관련 시스템
[Character / Combat / Draw / AI / StatusEffect / UI / Editor 중 선택]

### 디자인 문서 참조 (선택)
[GameDesign.md의 어떤 섹션과 관련되는지]
```

## AI 작업 프로세스

### Step 1: 기존 코드 분석
1. 관련 시스템의 모든 기존 코드 읽기
2. 기존 패턴/컨벤션 파악
3. 새 기능과 상호작용할 클래스 식별
4. 기존 이벤트 구조 파악

### Step 2: 데이터 계층 설계
1. ScriptableObject 정의가 필요한지 확인
   - 필요 시 `TeamLog` 메뉴 경로로 `[CreateAssetMenu]` 추가
   - `Assets/03.Data/` 적절한 하위 폴더에 배치
2. enum 확장이 필요한지 확인
   - 기존 enum: `TurnPhase`, `SkillType`, `TargetType`, `StatusEffectType`, `CharacterClass`
3. 기존 데이터와의 호환성 확인

### Step 3: 로직 구현
1. 순수 C# 클래스로 핵심 로직 구현 (MonoBehaviour 불필요 시)
2. 적절한 네임스페이스 적용
3. 이벤트 정의 (C# event/Action)
4. 기존 시스템과 연동 지점 구현

### Step 4: UI 구현
1. UI가 필요한 경우 `02.Scripts/UI/Battle/`에 추가
2. BattleEventManager에 필요한 이벤트 추가
3. BattleSceneSetup에서 초기화 코드 추가
4. 다크 테마 유지, NanumGothic 폰트 사용
5. 스킬 타입 색상 규칙 준수 (빨강=공격, 초록=치유, 노랑=버프, 보라=디버프)

### Step 5: 에디터 도구 업데이트
1. DataGenerator에 새 ScriptableObject 생성 코드 추가
2. 필요시 SceneBuilder 업데이트

### Step 6: 검증
1. Unity 컴파일 에러 확인
2. 기존 기능 회귀 테스트
3. 새 기능 정상 동작 확인

## 아키텍처 패턴 참고

### 새 상태이상 추가 시
```
1. StatusEffectType enum에 추가 (StatusEffectComponent.cs)
2. EffectProcessor에 로직 구현 (TurnStart/TurnEnd/OnDamage/OnHeal)
3. SkillData에 상태이상 설정 가능하도록 (이미 지원됨)
4. UI: CharacterPopupUI 상태이상 탭에 자동 표시
```

### 새 적 행동 추가 시
```
1. EnemyAIController에 액션 타입 추가
2. EnemyIntentType에 의도 표시 텍스트 추가
3. TurnManager의 ExecuteEnemyTurn에서 처리 로직 추가
```

### 새 캐릭터 클래스 추가 시
```
1. CharacterData의 CharacterClass enum 확장
2. SkillData 4개 이상 생성 (클래스별 기본 4스킬)
3. CharacterData ScriptableObject 생성
4. DataGenerator에 추가
5. UI 색상 매핑 업데이트 (CharacterUIPanel 등)
```
