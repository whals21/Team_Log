using System;
using System.Collections.Generic;
using TeamLog.Characters;

namespace TeamLog.Skill
{
    /// <summary>
    /// 발견(Discover) 메커니즘 정적 서비스 — 가중치 기반 무작위 추출 + 특성 반영.
    /// Cael(Alchemist)의 4 발견 스킬 시전 시 PlayerActionController가 호출.
    ///
    /// 핵심 결정 (Phase CC-2E):
    /// 1. 시드 주입 가능 — 테스트/시뮬레이터 재현성 보장
    /// 2. 기본 선택지 수 = 3 (물약 명인 특성 시 +1 = 4)
    /// 3. 동일 스킬 중복 추출 방지 (인덱스 기반)
    /// 4. 특성(DiscoverWeightBonus): 카테고리 매칭 시 가중치 배수 적용
    /// 5. ApplyAll 특성(강화 물약): 전투당 1회 — 모든 선택지를 모두 발동
    /// </summary>
    public static class DiscoverSystem
    {
        public const int DEFAULT_CHOICE_COUNT = 3;

        /// <summary>
        /// 발견 풀에서 N개의 선택지를 가중치 기반 무작위 추출.
        /// 중복 방지 (같은 SkillData가 두 번 나오지 않음).
        /// 풀 크기가 요청 수보다 작으면 전체 반환.
        /// </summary>
        /// <param name="pool">발견 풀 데이터</param>
        /// <param name="caster">시전자 (특성 가중치 배수 적용). null 가능.</param>
        /// <param name="rng">난수 생성기. null이면 새 Random 사용.</param>
        /// <returns>추출된 DiscoverEntry 목록 (선택지)</returns>
        public static List<DiscoverEntry> RollOptions(
            DiscoverPoolData pool,
            Character caster = null,
            Random rng = null)
        {
            var result = new List<DiscoverEntry>();
            if (pool == null || pool.EntryCount == 0) return result;

            rng ??= new Random();
            int choiceCount = GetChoiceCount(caster);

            // 가중치 적용된 후보 리스트 구성 (중복 방지를 위해 인덱스 추적)
            var entries = pool.Entries;
            var candidates = new List<(int idx, double weight)>(entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Skill == null) continue;
                double w = Math.Max(1, entries[i].Weight);
                w *= GetWeightMultiplier(pool.Category, caster);
                candidates.Add((i, w));
            }

            if (candidates.Count == 0) return result;

            int toPick = Math.Min(choiceCount, candidates.Count);

            // 가중치 기반 비복원 추출
            for (int pick = 0; pick < toPick; pick++)
            {
                if (candidates.Count == 0) break;

                double totalWeight = 0;
                for (int i = 0; i < candidates.Count; i++)
                    totalWeight += candidates[i].weight;

                double roll = rng.NextDouble() * totalWeight;
                int selectedIdx = -1;
                double cumulative = 0;
                for (int i = 0; i < candidates.Count; i++)
                {
                    cumulative += candidates[i].weight;
                    if (roll < cumulative)
                    {
                        selectedIdx = i;
                        break;
                    }
                }
                if (selectedIdx < 0) selectedIdx = candidates.Count - 1;

                int entryIdx = candidates[selectedIdx].idx;
                result.Add(entries[entryIdx]);
                candidates.RemoveAt(selectedIdx);
            }

            return result;
        }

        /// <summary>
        /// 발견 선택지 수 반환 — 기본 3, "물약 명인" 특성(DiscoverChoicesAdd) 시 +N.
        /// </summary>
        public static int GetChoiceCount(Character caster)
        {
            int count = DEFAULT_CHOICE_COUNT;
            if (caster?.PlayerTraitHandler != null)
                count += caster.PlayerTraitHandler.QueryKeywordSum(KeywordType.DiscoverChoicesAdd);
            return Math.Max(1, count);
        }

        /// <summary>
        /// 카테고리별 가중치 배수 — "독성 폭발" 특성(DiscoverWeightBonus)이 Crippling 카테고리에 배수 적용.
        /// 기본 1.0. 특성 Value가 배수(예: 2.0 = 2배).
        /// </summary>
        public static float GetWeightMultiplier(DiscoverCategory category, Character caster)
        {
            if (caster?.PlayerTraitHandler == null) return 1f;

            // "독성 폭발" 특성은 Crippling 카테고리에만 적용 (Value = 배수)
            if (category == DiscoverCategory.Crippling)
                return caster.PlayerTraitHandler.QueryKeywordMul(KeywordType.DiscoverWeightBonus);

            return 1f;
        }

        /// <summary>
        /// "강화 물약" 특성(DiscoverApplyAll) — 모든 선택지를 모두 발동할지 결정.
        /// 전투당 1회 사용 가능 (CanUseDiscoverApplyAll로 사전 체크).
        /// </summary>
        public static bool ShouldApplyAll(Character caster)
        {
            if (caster?.PlayerTraitHandler == null) return false;
            return caster.PlayerTraitHandler.HasDiscoverApplyAllTrait()
                && caster.PlayerTraitHandler.CanUseDiscoverApplyAll();
        }

        /// <summary>
        /// ShouldApplyAll 사용 후 소진. PlayerActionController가 ApplyAll 분기 실행 후 호출.
        /// </summary>
        public static void ConsumeApplyAll(Character caster)
        {
            caster?.PlayerTraitHandler?.ConsumeDiscoverApplyAll();
        }
    }
}
