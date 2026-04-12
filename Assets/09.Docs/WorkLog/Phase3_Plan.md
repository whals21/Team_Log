# Phase 3 로그라이크 요소 — 실행 계획서

**작성일**: 2026-04-12
**상태**: 완료

---

## Phase 3 구성

| 단계 | 작업 | 상태 |
|------|------|------|
| Step 1 | 맵 시스템 기획 구체화 | 완료 |
| Step 2 | 맵 시스템 핵심 로직 구현 | 완료 |
| Step 3 | 맵 UI 구현 | 완료 |
| Step 4 | 보상 시스템 | 완료 |
| Step 5 | 상점 / 이벤트 시스템 | 완료 |

## 구현 결과 요약

### 생성된 스크립트 (16개)
```
02.Scripts/Map/         — MapNodeType, MapNode, MapFloor, MapGenerator, GameRunState
02.Scripts/Reward/      — RewardData, ItemData, RewardManager
02.Scripts/Shop/        — ShopData, ShopManager
02.Scripts/Event/       — EventData, EventManager
02.Scripts/UI/Map/      — MapNodeButton, MapPlayerMarker, MapConnectionLine, MapView, MapSceneSetup
02.Scripts/UI/Reward/   — RewardUI, RewardCard
02.Scripts/UI/Shop/     — ShopUI, ShopItemSlot
02.Scripts/UI/Event/    — EventUI
02.Scripts/Editor/      — MapSceneBuilder
```

### 수정된 스크립트 (1개)
```
02.Scripts/Combat/BattleSceneSetup.cs — SetBattleData API, OnBattleFinished 이벤트 추가
```

### 컴파일 상태
- 0 errors, 0 warnings

## 후속 작업 필요

| 항목 | 우선순위 |
|------|----------|
| 프리팹 생성 및 인스펙터 연결 | 높음 |
| 테스트용 EventData, ItemData SO 에셋 생성 | 높음 |
| MapScene 씬 빌드 및 런타임 테스트 | 높음 |
| 실제 스킬/아이템 풀 데이터 채우기 | 중간 |
| 심층 가비지 컬렉터 (Phase 3→4 전환) | 중간 |
