using System.Collections.Generic;
using System.Linq;
using TeamLog.Skill;

namespace TeamLog.Characters
{
    /// <summary>
    /// 스킬 인스턴스 — SkillData(SO 템플릿) + 증강(Augment) 슬롯
    /// 캐릭터별 독립 상태를 관리하여 공유 SO를 수정하지 않음
    /// </summary>
    public class SkillInstance
    {
        public const int MaxAugments = 3;

        private readonly List<AugmentInstance> _augments = new();

        // 캐시: 키워드가 변경될 때만 재계산
        private List<KeywordEntry> _keywordCache;
        private int _keywordCacheVersion;

        // 캐시: 행동 키워드가 변경될 때만 재계산 (Phase BK)
        private List<BehaviorTag> _behaviorCache;
        private int _behaviorCacheVersion = -1;

        public SkillData Data { get; }
        public IReadOnlyList<AugmentInstance> Augments => _augments;

        /// <summary>이번 전투에서 사용한 횟수 (Phase ARCH-5 — Fatigue/Momentum/Escalation/Mastery/LimitBreak용).</summary>
        public int UsesThisBattle { get; private set; }

        /// <summary>이번 전투에서 사용했는지 여부 (Phase ARCH-5 — LimitBreak용 flag).</summary>
        public bool UsedThisBattle => UsesThisBattle > 0;

        public SkillInstance(SkillData data)
        {
            Data = data;
        }

        /// <summary>이번 전투 사용 횟수 증가 (TurnManager.ExecuteSkillImmediately에서 호출).</summary>
        public void IncrementUsesThisBattle() => UsesThisBattle++;

        /// <summary>이번 전투 사용 횟수 리셋 (전투 시작 시 호출).</summary>
        public void ResetUsesThisBattle() => UsesThisBattle = 0;

        /// <summary>증강 추가 — 성공 시 true, 슬롯 가득 차거나 중복 시 false</summary>
        public bool AddAugment(AugmentData augment)
        {
            if (augment == null) return false;
            if (_augments.Count >= MaxAugments) return false;
            // Phase BK: 이미 부착된 "증강" 중 동일 BehaviorKeyword를 가진 것이 있으면 거부.
            // (스킬 본체 Behavior와의 충돌은 허용 — rank가 합산되므로)
            if (HasConflictingAugment(augment)) return false;
            _augments.Add(new AugmentInstance(augment));
            _keywordCacheVersion++;
            _behaviorCacheVersion = -1;
            return true;
        }

        /// <summary>이미 부착된 증강 중 augment와 동일 BehaviorKeyword를 가진 것이 있는지 검사.</summary>
        private bool HasConflictingAugment(AugmentData augment)
        {
            if (augment.Behaviors.Count == 0) return false;
            foreach (var existing in _augments)
            {
                if (existing.Data.Behaviors == null) continue;
                foreach (var b in augment.Behaviors)
                    if (BehaviorTagResolver.Has(existing.Data.Behaviors, b.Keyword)) return true;
            }
            return false;
        }

        /// <summary>
        /// 스킬 본체 + 모든 증강의 BehaviorTag를 평탄화하여 반환 (캐시됨, Phase BK).
        /// </summary>
        public List<BehaviorTag> GetCombinedBehaviors()
        {
            // 스킬 본체 + 증강 개수가 바뀌면 캐시 무효화
            int version = 1 + _augments.Count;
            if (_behaviorCache != null && _behaviorCacheVersion == version)
                return _behaviorCache;

            _behaviorCache = new List<BehaviorTag>();
            // 스킬 본체 BehaviorTag 우선 추가
            if (Data != null && Data.Behaviors != null)
                _behaviorCache.AddRange(Data.Behaviors);
            foreach (var aug in _augments)
            {
                if (aug.Data.Behaviors != null)
                    _behaviorCache.AddRange(aug.Data.Behaviors);
            }
            _behaviorCacheVersion = version;
            return _behaviorCache;
        }

        /// <summary>지정 행동 키워드 보유 여부 (스킬 본체 + 증강).</summary>
        public bool HasBehavior(BehaviorKeyword keyword)
            => BehaviorTagResolver.Has(GetCombinedBehaviors(), keyword);

        /// <summary>지정 행동 키워드의 첫 태그 (없으면 null).</summary>
        public BehaviorTag? GetBehavior(BehaviorKeyword keyword)
            => BehaviorTagResolver.First(GetCombinedBehaviors(), keyword);

        /// <summary>지정 행동 키워드의 rank 합산 값.</summary>
        public int GetBehaviorRank(BehaviorKeyword keyword)
            => BehaviorTagResolver.RankSum(GetCombinedBehaviors(), keyword);

        /// <summary>지정 행동 키워드의 모든 태그.</summary>
        public List<BehaviorTag> GetAllBehaviors(BehaviorKeyword keyword)
            => BehaviorTagResolver.All(GetCombinedBehaviors(), keyword);

        /// <summary>모든 증강의 키워드를 평탄화하여 반환 (캐시됨)</summary>
        public List<KeywordEntry> GetAllKeywords()
        {
            if (_keywordCache != null && _keywordCacheVersion == _augments.Count)
                return _keywordCache;

            _keywordCache = new List<KeywordEntry>();
            foreach (var aug in _augments)
            {
                if (aug.Data.Keywords != null)
                    _keywordCache.AddRange(aug.Data.Keywords);
            }
            _keywordCacheVersion = _augments.Count;
            return _keywordCache;
        }

        /// <summary>기본 위력 — 키워드 기반 계산 + Phase ARCH-5 Fatigue/Momentum</summary>
        public int EffectivePower
        {
            get
            {
                float power = Data.Power;
                var kw = GetAllKeywords();

                // 배율 (곱셈) — Passive 트리거만
                power *= KeywordResolver.MulKeyword(kw, KeywordType.PowerMul);

                // 가산
                power += KeywordResolver.SumKeyword(kw, KeywordType.PowerAdd);

                // Phase ARCH-5: Fatigue/Momentum — usesThisBattle 기반 위력 변동
                // Fatigue rank N: 매 사용마다 위력 -N (누적, 최소 1)
                // Momentum rank N: 매 사용마다 위력 +N (누적)
                var behaviors = GetCombinedBehaviors();
                int fatigueRank = BehaviorTagResolver.RankSum(behaviors, BehaviorKeyword.Fatigue);
                int momentumRank = BehaviorTagResolver.RankSum(behaviors, BehaviorKeyword.Momentum);
                if (fatigueRank > 0)
                    power -= UsesThisBattle * fatigueRank;
                if (momentumRank > 0)
                    power += UsesThisBattle * momentumRank;

                return System.Math.Max(1, (int)power);
            }
        }

        /// <summary>증강 반영 비용 — 키워드 기반 계산 + Phase ARCH-5 Escalation/Mastery</summary>
        public int EffectiveCost
        {
            get
            {
                float cost = Data.Cost;
                var kw = GetAllKeywords();

                // 키워드 합산 (CostDown=-1, HeavyHit=+1, Reaper=+1, AOEAuto=+2 모두 여기서 처리)
                cost += KeywordResolver.SumKeyword(kw, KeywordType.CostAdd);

                // Phase ARCH-5: Escalation/Mastery — usesThisBattle 기반 cost 변동
                // Escalation rank N: 매 사용마다 cost +N (누적)
                // Mastery rank N: 매 사용마다 cost -N (누적, 최소 0)
                var behaviors = GetCombinedBehaviors();
                int escalationRank = BehaviorTagResolver.RankSum(behaviors, BehaviorKeyword.Escalation);
                int masteryRank = BehaviorTagResolver.RankSum(behaviors, BehaviorKeyword.Mastery);
                if (escalationRank > 0)
                    cost += UsesThisBattle * escalationRank;
                if (masteryRank > 0)
                    cost -= UsesThisBattle * masteryRank;

                return System.Math.Max(0, (int)cost);
            }
        }

        /// <summary>증강 반영 가중치 — 키워드 기반 계산</summary>
        public int EffectiveWeight
        {
            get
            {
                var kw = GetAllKeywords();

                // 덮어쓰기 키워드 (QuickDraw → 가중치 0)
                if (KeywordResolver.HasKeyword(kw, KeywordType.DrawWeightOverride))
                    return 0;

                return Data.Weight;
            }
        }
    }
}
