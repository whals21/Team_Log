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

        public SkillData Data { get; }
        public IReadOnlyList<AugmentInstance> Augments => _augments;

        public SkillInstance(SkillData data)
        {
            Data = data;
        }

        /// <summary>증강 추가 — 성공 시 true, 슬롯 가득 차거나 중복 시 false</summary>
        public bool AddAugment(AugmentData augment)
        {
            if (augment == null) return false;
            if (_augments.Count >= MaxAugments) return false;
            if (HasAugment(augment.Type)) return false;
            _augments.Add(new AugmentInstance(augment));
            _keywordCacheVersion++;
            return true;
        }

        /// <summary>특정 증강 타입 보유 여부</summary>
        public bool HasAugment(AugmentType type)
        {
            return _augments.Any(a => a.Data.Type == type);
        }

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

        /// <summary>기본 위력 — 키워드 기반 계산</summary>
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

                return System.Math.Max(1, (int)power);
            }
        }

        /// <summary>증강 반영 비용 — 키워드 기반 계산</summary>
        public int EffectiveCost
        {
            get
            {
                float cost = Data.Cost;
                var kw = GetAllKeywords();

                // 키워드 합산 (CostDown=-1, HeavyHit=+1, Reaper=+1, AOEAuto=+2 모두 여기서 처리)
                cost += KeywordResolver.SumKeyword(kw, KeywordType.CostAdd);

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
