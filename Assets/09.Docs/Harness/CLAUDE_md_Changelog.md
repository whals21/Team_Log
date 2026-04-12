# CLAUDE.md 변경 이력

**파일 경로**: 프로젝트 루트 `/CLAUDE.md`

---

## v1.0 — 2026-04-11 (최초 생성)

### 배경
프로젝트에 AI 하네스(Harness) 방법론을 도입하기 위해 루트 레벨 CLAUDE.md를 최초 생성.
기존에는 프로젝트 루트에 CLAUDE.md가 존재하지 않았음.

### 생성된 파일
- `CLAUDE.md` (프로젝트 루트, 173줄)

### 포함된 섹션

| # | 섹션 | 내용 | 줄 수 |
|---|------|------|-------|
| 1 | **프로젝트 구조** | Assets/ 폴더 전체 맵핑 (01.Scenes ~ 09.Docs) | 5-19 |
| 2 | **아키텍처** | 핵심 클래스 관계도, 턴 사이클, 데이터 계층 | 21-47 |
| 3 | **코딩 규칙** | 필수 규칙, 네이밍 컨벤션, 파일 배치 | 49-76 |
| 4 | **가드레일** | 8가지 금지 사항 | 78-87 |
| 5 | **워크플로우** | 새 기능 추가 7단계 프로세스 | 89-97 |
| 6 | **현재 개발 상태** | Phase 1-4 현황, 미구현 항목 | 99-113 |
| 7 | **가비지 컬렉터** | 오염 감지/분류/정화 규칙 | 115-164 |
| 8 | **기술 스택** | Unity 6, URP, TMP, Input System, 에셋 | 166-172 |

### 섹션별 상세 내용

#### 프로젝트 구조 (line 5-19)
```
Assets/
├── 01.Scenes/          # BattleScene, BattleUI, TestCombatScene
├── 02.Scripts/
│   ├── Characters/     # Character, CharacterData, Components/
│   ├── Combat/         # AI/, Draw/, StatusEffect/, Turn/
│   ├── Skill/          # SkillData
│   ├── UI/Battle/      # 모든 전투 UI
│   └── Editor/         # DataGenerator, SceneBuilder
├── 03.Data/            # ScriptableObject 에셋
├── 08.Resource/        # 폰트, 이미지
└── 09.Docs/            # 문서
```

#### 아키텍처 (line 21-47)
- BattleSceneSetup → TurnManager → SkillDrawSystem / TurnContext 계층 구조
- Character 합성 구조: HealthComponent, StatComponent, StatusEffectComponent, SkillInventoryComponent
- 턴 사이클: `Draw → PlayerAction → Execution → EnemyTurn → BattleEnd`
- 데이터 계층: CharacterData, SkillData ScriptableObject 정의

#### 코딩 규칙 (line 49-76)
- **네임스페이스 8종 정의**: TeamLog.Characters, Combat.Turn, Combat.Draw, Combat.AI, Combat.StatusEffect, UI.Battle, Editor
- **이벤트 기반 통신**: 직접 참조 최소화
- **UI-로직 분리**: UI는 표시만
- **순수 C# 우선**: MonoBehaviour 최소화
- **ScriptableObject 데이터 관리**: 하드코딩 금지
- **네이밍**: PascalCase(클래스), camelCase(필드), On접두사(이벤트), Class_Name(SO)
- **파일 배치**: 시스템 폴더 준수

#### 가드레일 (line 78-87)
```
8가지 금지 사항:
1. Library/, Temp/, obj/ 수정 금지
2. .meta 파일 수동 조작 금지
3. FindObjectOfType / Find / GameObject.Find 금지
4. PlayerPrefs 금지
5. 불필요한 MonoBehaviour 금지
6. 02.Scripts/ 외부 스크립트 금지
7. UI에서 게임 로직 금지
8. public API 변경 시 호환성 확인
```

#### 워크플로우 (line 89-97)
```
7단계: 기존 코드 읽기 → 데이터 정의 → 로직 구현 → 이벤트 연결 → UI 구현 → 에디터 도구 → 테스트
```

#### 개발 상태 (line 99-113)
```
Phase 1 (코어 전투): 완료
Phase 2 (전투 완성): 완료
Phase 3 (로그라이크): 미착수
Phase 4 (폴리싱): 미착수
```

#### 가비지 컬렉터 (line 115-164)
- 수집 대상 7가지: 죽은 코드, 미사용 에셋, 기획 불일치, 아키텍처 위반, 중복 로직, 유령 참조, 스텁/TODO 방치
- 수집 주기: 경량(매 세션), 심층(Phase 전환)
- 수집 절차: 스캔 → 분류 → 보고 → 정화 → 검증
- 삭제 분류: 즉시 삭제 / 사용자 확인 / 보류
- 실행 규칙 5가지: 보고 후 삭제, 삭제 순서, 검증, git 확인, 복구 가능성

#### 기술 스택 (line 166-172)
```
Unity 6000.0 (Unity 6), URP, TextMesh Pro (NanumGothic), New Input System, GUI Pro-CasualGame
```

---

## 함께 생성된 파일

CLAUDE.md와 함께 다음 파일들이 생성됨:

| 파일 | 경로 | 목적 |
|------|------|------|
| bug-fix.md | `.claude/prompts/` | 버그 수정 프롬프트 템플릿 |
| feature-add.md | `.claude/prompts/` | 기능 추가 프롬프트 템플릿 |
| refactor.md | `.claude/prompts/` | 리팩토링 프롬프트 템플릿 |
| data-design.md | `.claude/prompts/` | 데이터 설계 프롬프트 템플릿 |
| garbage-collect.md | `.claude/prompts/` | 가비지 컬렉터 프롬프트 템플릿 |

---

## 변경 기록 템플릿

이후 CLAUDE.md 수정 시 아래 형식으로 기록하세요:

```
## vX.X — YYYY-MM-DD (변경 유형)

### 변경 사유
[왜 변경했는지]

### 변경 내용
| 섹션 | 변경 유형 | 내용 |
|------|-----------|------|
| 섹션명 | 추가/수정/삭제 | 구체적 변경 내용 |

### 영향 범위
[이 변경이 어떤 작업에 영향을 미치는지]
```
