using System.Collections.Generic;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.Skill;

namespace TeamLog.Event
{
    /// <summary>
    /// 이벤트 처리 관리자 — 순수 C# 클래스
    /// </summary>
    public class EventManager
    {
        private readonly System.Random _rng = new();

        /// <summary>
        /// 선택지가 현재 파티 상태에서 선택 가능한지 검증
        /// </summary>
        public bool CanChoose(EventChoice choice, GameRunState runState)
        {
            if (choice == null || runState == null) return false;

            if (choice.MinGoldRequired > 0 && runState.Gold < choice.MinGoldRequired)
                return false;

            if (choice.MinPartyHPPercent > 0f)
            {
                float avgHp = GetAveragePartyHPRatio(runState);
                if (avgHp < choice.MinPartyHPPercent)
                    return false;
            }

            if (choice.RequiresAliveMembers > 0)
            {
                int alive = 0;
                foreach (var m in runState.PlayerParty)
                    if (m.IsAlive) alive++;
                if (alive < choice.RequiresAliveMembers)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 선택지 실행 — 결과를 GameRunState와 파티에 적용
        /// </summary>
        public EventOutcome ProcessChoice(EventData eventData, int choiceIndex, GameRunState runState)
        {
            if (choiceIndex < 0 || choiceIndex >= eventData.Choices.Count) return null;

            var choice = eventData.Choices[choiceIndex];
            var outcome = choice.Outcome;

            // 확률 기반 Outcome 추첨 (있으면 대체)
            if (outcome.RandomOutcomes != null && outcome.RandomOutcomes.Count > 0)
            {
                outcome = PickRandomOutcome(outcome);
            }

            // ★ ResultText 오염 방지: 원본 에셋 대신 복사본 반환
            var resultCopy = CloneOutcome(outcome);

            ApplyOutcome(resultCopy, runState);
            return resultCopy;
        }

        /// <summary>
        /// Outcome 효과 적용 (내부 헬퍼)
        /// </summary>
        private void ApplyOutcome(EventOutcome outcome, GameRunState runState)
        {
            // 골드 변화
            if (outcome.GoldChange != 0)
            {
                if (outcome.GoldChange > 0)
                    runState.AddGold(outcome.GoldChange);
                else
                    runState.SpendGold(-outcome.GoldChange);
            }

            // HP 변화 (파티 전체)
            if (outcome.HPPercentChange != 0)
            {
                foreach (var member in runState.PlayerParty)
                {
                    if (member.IsAlive)
                    {
                        int hpChange = member.Health.MaxHP * outcome.HPPercentChange / 100;
                        if (hpChange > 0)
                            member.Health.Heal(hpChange);
                        else
                            member.Health.TakeDamage(-hpChange);
                    }
                }
            }

            // 영구 ATK 강화
            if (outcome.PermanentAtkBonus != 0)
            {
                foreach (var member in runState.PlayerParty)
                {
                    if (member.IsAlive)
                        member.Stats.AddPermanentBase(StatType.ATK, outcome.PermanentAtkBonus);
                }
            }

            // 영구 DEF 강화
            if (outcome.PermanentDefBonus != 0)
            {
                foreach (var member in runState.PlayerParty)
                {
                    if (member.IsAlive)
                        member.Stats.AddPermanentBase(StatType.DEF, outcome.PermanentDefBonus);
                }
            }

            // 리롤 토큰
            if (outcome.RerollTokensBonus != 0)
            {
                runState.AddRerollTokens(outcome.RerollTokensBonus);
            }

            // 증강 획득 (스킬 대신)
            if (outcome.GiveRandomSkill)
            {
                var augment = runState.PeekRandomAugment();
                if (augment != null)
                {
                    foreach (var member in runState.PlayerParty)
                    {
                        if (!member.IsAlive) continue;
                        foreach (var inst in member.SkillInventory.SkillInstances)
                        {
                            if (inst.Augments.Count < SkillInstance.MaxAugments)
                            {
                                runState.AcquireAugment(augment, member, inst);
                                if (outcome.ResultText != null)
                                    outcome.ResultText += $" ({augment.AugmentName} 획득!)";
                                goto DoneAugment;
                            }
                        }
                    }
                DoneAugment:;
                }
            }

            // 유물 획득
            if (outcome.GiveRandomItem)
            {
                var relic = runState.PeekRandomRelic();
                if (relic != null)
                {
                    runState.AcquireRelic(relic);
                    if (outcome.ResultText != null)
                        outcome.ResultText += $" ({relic.RelicName} 획득!)";
                }
            }

            // 저주 / 상태이상 적용
            if (outcome.ApplyStatusEffect != StatusEffectType.None)
            {
                foreach (var member in runState.PlayerParty)
                {
                    if (member.IsAlive)
                    {
                        member.StatusEffects.ApplyEffect(
                            outcome.ApplyStatusEffect,
                            outcome.StatusEffectDuration,
                            outcome.StatusEffectValue);
                        member.ApplyStatModifiers();
                    }
                }
            }
        }

        /// <summary>
        /// 확률 기반 Outcome 추첨
        /// </summary>
        private EventOutcome PickRandomOutcome(EventOutcome parent)
        {
            var outcomes = parent.RandomOutcomes;
            var weights = parent.OutcomeWeights;

            if (outcomes == null || outcomes.Count == 0)
                return parent;

            // 가중치 정규화: weights가 부족하거나 비어있으면 균등
            float total = 0f;
            var weightList = new List<float>();
            for (int i = 0; i < outcomes.Count; i++)
            {
                float w = (i < weights.Count && weights[i] > 0f) ? weights[i] : 1f;
                weightList.Add(w);
                total += w;
            }

            float roll = (float)_rng.NextDouble() * total;
            float cumulative = 0f;
            for (int i = 0; i < outcomes.Count; i++)
            {
                cumulative += weightList[i];
                if (roll <= cumulative)
                    return outcomes[i];
            }

            return outcomes[outcomes.Count - 1];
        }

        /// <summary>
        /// Outcome 얕은 복사 (ResultText 누적 방지)
        /// </summary>
        private EventOutcome CloneOutcome(EventOutcome src)
        {
            var copy = new EventOutcome
            {
                ResultText = src.ResultText,
                GoldChange = src.GoldChange,
                HPPercentChange = src.HPPercentChange,
                GiveRandomSkill = src.GiveRandomSkill,
                GiveRandomItem = src.GiveRandomItem,
                PermanentAtkBonus = src.PermanentAtkBonus,
                PermanentDefBonus = src.PermanentDefBonus,
                RerollTokensBonus = src.RerollTokensBonus,
                ApplyStatusEffect = src.ApplyStatusEffect,
                StatusEffectDuration = src.StatusEffectDuration,
                StatusEffectValue = src.StatusEffectValue,
                NextEventId = src.NextEventId
                // RandomOutcomes는 복사하지 않음 — 이미 추첨 완료된 상태
            };
            return copy;
        }

        private float GetAveragePartyHPRatio(GameRunState runState)
        {
            int alive = 0;
            int totalRatio = 0;
            foreach (var m in runState.PlayerParty)
            {
                if (!m.IsAlive) continue;
                alive++;
                if (m.Health.MaxHP > 0)
                    totalRatio += m.Health.CurrentHP * 100 / m.Health.MaxHP;
            }
            return alive == 0 ? 0f : totalRatio / (alive * 100f);
        }
    }
}
