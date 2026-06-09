using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Skill;

using Character = TeamLog.Characters.Character;
using SkillInstance = TeamLog.Characters.SkillInstance;
using SkillType = TeamLog.Characters.SkillType;

namespace TeamLog.Reward
{
    /// <summary>
    /// 증강 보상 제안 생성기 — 등급 가중치 선택, 호환성 체크, 제안 조합
    /// GameRunState에서 분리된 순수 로직 클래스
    /// </summary>
    public class AugmentOfferGenerator
    {
        private readonly List<AugmentData> _augmentPool;
        private readonly List<Character> _playerParty;
        private readonly System.Random _rng;

        public AugmentOfferGenerator(List<AugmentData> augmentPool, List<Character> playerParty, System.Random rng)
        {
            _augmentPool = augmentPool ?? new List<AugmentData>();
            _playerParty = playerParty;
            _rng = rng;
        }

        /// <summary>
        /// 등급 가중치 증강 선택 — 전투 타입에 따라 등급 확률이 다름
        /// </summary>
        public AugmentData PeekTierWeightedAugment(MapNodeType battleType)
        {
            if (_augmentPool.Count == 0) return null;

            // 등급 가중치 결정
            float t1Weight, t2Weight, t3Weight;
            switch (battleType)
            {
                case MapNodeType.Elite:
                    t1Weight = 50f; t2Weight = 40f; t3Weight = 10f;
                    break;
                case MapNodeType.Boss:
                    t1Weight = 20f; t2Weight = 50f; t3Weight = 30f;
                    break;
                default: // Battle
                    t1Weight = 80f; t2Weight = 20f; t3Weight = 0f;
                    break;
            }

            // 등급 결정
            float total = t1Weight + t2Weight + t3Weight;
            float roll = (float)_rng.NextDouble() * total;
            int targetTier;
            if (roll < t3Weight)
                targetTier = 3;
            else if (roll < t3Weight + t2Weight)
                targetTier = 2;
            else
                targetTier = 1;

            // 해당 등급 증강 필터링, 없으면 다른 등급
            var candidates = _augmentPool.FindAll(a => a.Tier == targetTier);
            if (candidates.Count == 0)
                candidates = _augmentPool; // 폴백: 전체 풀

            return candidates[_rng.Next(candidates.Count)];
        }

        /// <summary>
        /// 유효한 (캐릭터, 스킬, 증강) 조합 생성
        /// </summary>
        public AugmentOffer GenerateAugmentOffer(MapNodeType battleType)
        {
            int maxAttempts = 30;
            for (int i = 0; i < maxAttempts; i++)
            {
                var augment = PeekTierWeightedAugment(battleType);
                if (augment == null) return null;

                // 살아있는 파티원 중 무작위 선택
                var aliveMembers = _playerParty.FindAll(p => p.IsAlive);
                if (aliveMembers.Count == 0) return null;

                // 증강 호환 스킬이 있는 캐릭터 수집
                var validPairs = new List<(Character, SkillInstance)>();
                foreach (var member in aliveMembers)
                {
                    foreach (var skillInst in member.SkillInventory.SkillInstances)
                    {
                        if (IsAugmentCompatible(augment, skillInst))
                            validPairs.Add((member, skillInst));
                    }
                }

                if (validPairs.Count == 0) continue;

                var (character, skill) = validPairs[_rng.Next(validPairs.Count)];
                return new AugmentOffer(augment, character, skill);
            }

            return null;
        }

        /// <summary>
        /// 증강 호환성 체크 — CompatibleSkillType, 중복, 슬롯
        /// </summary>
        private static bool IsAugmentCompatible(AugmentData augment, SkillInstance skillInst)
        {
            // 이미 같은 타입 보유 시 불가
            if (skillInst.HasAugment(augment.Type)) return false;
            // 슬롯 가득 참
            if (skillInst.Augments.Count >= SkillInstance.MaxAugments) return false;
            // 스킬 타입 호환성
            if (augment.CompatibleSkillType == SkillType.Attack)
                return true; // Attack = 모든 스킬 호환
            return skillInst.Data.Type == augment.CompatibleSkillType;
        }
    }
}
