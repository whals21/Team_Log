# 가비지 컬렉터 이력

## 수집 기록

| 일시 | 타입 | 결과 | 주요 발견 |
|------|------|------|-----------|
| 2026-04-11 | 심층 수집 | 40건 오염, 4건 보류 | [GC_Report_2026-04-11.md](./GC_Report_2026-04-11.md) |
| 2026-06-07 | 심층 수집 | 38건 오염 (DP/HC/OE/DC/SL/DL/EC/FS) | [GC_Report_2026-06-07.md](./GC_Report_2026-06-07.md) |
| 2026-06-19 | 심층 수집 (Phase E 후) | 16건 신규/잔존, 이전 38건 중 19건 해결됨 | [GC_Report_2026-06-19.md](./GC_Report_2026-06-19.md) |

---

## 정화 이력

| 일시 | 처리 건수 | 내용 |
|------|-----------|------|
| 2026-06-07 ~ 2026-06-19 | 19건 자연 해결 | Phase 7-8 작업으로 HC-01~08/OE-01~04/DC-01~04/DP-09/FS-01~02 자연 해결 (UIPalette 마이그레이션, ItemEffectApplier 제거, partial 분할 등) |
| 2026-06-19 | 5건 수동 정화 | GD-001 (GameDesign.md 3→4스테이지), EHC-03 (ClearContainerChildren → UIAnimationHelper 통합), DL-01/DL-02 (이미 #if UNITY_EDITOR 가드 확인됨 — 잘못된 잔존 등록), SL-01/SL-02 (외부 처리 확인 — DamageCalculator/TurnManager에서 GetCounterDamage/GetExtraAP 호출), FS-09 (BattleTestSceneBuilder partial 분할 807→323+506줄) |

---

## 다음 수집 권장 시점

- **경량 수집**: 다음 작업 세션 종료 전
- **심층 수집**: Phase 전환 시 (Phase 9 착수 전 또는 정식 출시 전)
- **특정 수집**: SL-01/SL-02 스텁 결정 후 (CounterDamage/ExtraAP 구현 여부)
