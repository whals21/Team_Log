using System.Collections.Generic;

namespace TeamLog.Skill.Behaviors
{
    /// <summary>
    /// BehaviorKeyword enum → ISkillBehavior 인스턴스 정적 매핑 (Phase ARCH).
    /// 부패 시(lazy init) 모든 Behavior 구현체를 등록.
    /// SkillExecutor/Pipeline은 이 Registry를 통해 키워드를 로직으로 변환한다.
    ///
    /// 등록 가이드:
    /// - 새 Behavior 추가 시 Register(new XxxBehavior()) 한 줄 추가.
    /// - 기존 코드 수정 없이 확장 가능 (Open-Closed 원칙).
    /// </summary>
    public static class BehaviorRegistry
    {
        private static readonly Dictionary<BehaviorKeyword, ISkillBehavior> _map = new();
        private static bool _initialized = false;
        private static readonly object _lock = new();

        /// <summary>Registry 초기화 — 모든 표준 Behavior 등록. 한 번만 실행.</summary>
        public static void Initialize()
        {
            lock (_lock)
            {
                if (_initialized) return;

                // ── Phase ARCH-2: 핵심 5종 (기존 SkillExecutor.ExecuteAttack에서 추출) ──
                Register(new Implementations.BerserkBehavior());
                Register(new Implementations.PierceBehavior());
                Register(new Implementations.ExecutionBehavior());
                Register(new Implementations.LifestealBehavior());
                Register(new Implementations.ChainBehavior());

                // ── Phase ARCH-3: Touch 계열 3종 (기존 SkillExecutor.ApplyTouchEffects에서 추출) ──
                Register(new Implementations.VenomTouchBehavior());
                Register(new Implementations.BurningTouchBehavior());
                Register(new Implementations.FreezeTouchBehavior());

                // ── Phase ARCH-4: 신규 9종 (상태 추적 불필요 — 컨셉 6/7/11/12/15/16/17/18/21) ──
                Register(new Implementations.FirstBloodBehavior());    // 6 — 풀피 대상
                Register(new Implementations.CullBehavior());          // 7 — 절반 이하 대상
                Register(new Implementations.DesperationBehavior());   // 11 — 잃은 HP당 +
                Register(new Implementations.WoundBehavior());         // 12 — 잃은 HP당 -
                Register(new Implementations.GiantSlayerBehavior());   // 15 — 적 MaxHP 임계+
                Register(new Implementations.AllInBehavior());         // 16 — AP 0 시
                Register(new Implementations.DominanceBehavior());     // 17 — 적 HP < 나 HP
                Register(new Implementations.BulwarkBehavior());       // 18 — 쉴드 보유 시
                Register(new Implementations.BountyBehavior());        // 21 — 킬 시 자원

                // ── Phase CC: 캐릭터 고유 메카닉 ──
                Register(new Implementations.PropagateBehavior());     // Taranis — Wire 전파
                Register(new Implementations.TargetFreezeBehavior());  // Lumi — Frost Bite 강화

                // ── 통합 파이프라인 검증 (2026-07-02): Pipeline 수정 0줄로 추가 ──
                Register(new Implementations.CleanseLowTargetBehavior());        // Phoenix Renewal용 — Heal에 정화
                Register(new Implementations.ResourceThresholdShieldBehavior()); // Shield Wall용 — Shield에 임계값 가산

                // ── Phase ARCH-4 보류 (상태 추적/TurnManager 수정 필요) ──
                // FollowUp(5) — hitsTakenThisTurn 인프라 필요
                // Fatigue(8)/Momentum(9) — usesThisBattle 추적 필요
                // Escalation(13)/Mastery(14) — usesThisBattle + EffectiveCost 파이프라인 (ARCH-5)
                // LimitBreak(19) — usedThisBattle 추적 + 드로우 풀 필터링
                // Echo(10)/Distribute(1)/TargetHighestHP(2)/MultiStrike(3)/TargetFullHP(4)/Flank(20)
                //   — TurnManager 타겟팅 수정 또는 순차 타겟팅 UI 필요

                // ── Phase ARCH-3 잔여 (TurnManager가 계속 처리 — 회귀 안전) ──
                // Spread, Bounce, MultiHit, Explosion, AOEAuto — TurnManager.ExecuteSkillImmediately에서
                // 타겟팅 분해 담당. TargetModify Phase로의 이관은 추후 검토.

                // ── Phase ARCH-3 키워드 기반 (EffectivePower/Cost/Weight에서 간접 처리) ──
                // HeavyHit, BloodPact, GlassCannon, PowerUp, Reaper, AOEAuto, CostDown, QuickDraw
                // — 이들은 Behavior로 추출 불필요 (SkillInstance.EffectivePower/Cost/Weight가 키워드 기반 처리)

                // ── Phase ARCH-3 Heal/Shield/Buff 계열 (ExecuteHeal/ExecuteShield/ApplyEffect에서 처리) ──
                // ShieldBonus, HealBonus, Intensify, Lingering — Attack 외 스킬 타입 처리. ARCH-5 이후 검토

                // ── Phase ARCH-4 (예정): 신규 21종 후보 ──
                // FollowUp, FirstBlood, Cull, Fatigue, Momentum, Echo, Desperation, Wound,
                // Escalation, Mastery, GiantSlayer, AllIn, Dominance, Bulwark, LimitBreak,
                // Flank, Bounty, Distribute, TargetHighestHP, MultiStrike, TargetFullHP

                _initialized = true;
            }
        }

        /// <summary>Behavior 등록. 같은 Keyword에 다시 등록 시 덮어씀 (테스트용).</summary>
        public static void Register(ISkillBehavior behavior)
        {
            if (behavior == null) return;
            _map[behavior.Keyword] = behavior;
        }

        /// <summary>Registry 초기화 해제 — 테스트용 (커스텀 Behavior 임시 등록 시).</summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _map.Clear();
                _initialized = false;
            }
        }

        /// <summary>지정 Keyword의 Behavior 조회. 없으면 null.</summary>
        public static ISkillBehavior Get(BehaviorKeyword keyword)
        {
            Initialize();
            return _map.TryGetValue(keyword, out var b) ? b : null;
        }

        /// <summary>주어진 태그 목록에서 특정 Phase에 해당하는 Behavior들을 Order 오름차순으로 반환.</summary>
        /// <param name="tags">스킬 본체 + 증강의 평탄화된 BehaviorTag 목록.</param>
        /// <param name="phase">조회할 Phase.</param>
        /// <returns>해당 Phase에 개입하는 Behavior 리스트 (Order 순).</returns>
        public static List<ISkillBehavior> GetForPhase(IReadOnlyList<BehaviorTag> tags, ExecutionPhase phase)
        {
            Initialize();
            var result = new List<ISkillBehavior>();
            if (tags == null) return result;

            // 중복 방지 (같은 Behavior가 여러 태그에서 참조되더라도 한 번만)
            var seen = new HashSet<BehaviorKeyword>();
            foreach (var tag in tags)
            {
                if (!seen.Add(tag.Keyword)) continue;
                if (_map.TryGetValue(tag.Keyword, out var b) && (b.Phases & phase) != 0)
                    result.Add(b);
            }

            result.Sort((a, b) => a.Order.CompareTo(b.Order));
            return result;
        }
    }
}
