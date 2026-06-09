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
        /// 선택지 실행 — 결과를 GameRunState와 파티에 적용
        /// </summary>
        public EventOutcome ProcessChoice(EventData eventData, int choiceIndex, GameRunState runState)
        {
            if (choiceIndex < 0 || choiceIndex >= eventData.Choices.Count) return null;

            var choice = eventData.Choices[choiceIndex];
            var outcome = choice.Outcome;

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

            // 증강 획득 (스킬 대신)
            if (outcome.GiveRandomSkill)
            {
                var augment = runState.PeekRandomAugment();
                if (augment != null)
                {
                    // 첫 번째 생존 파티원의 첫 번째 스킬에 부착
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

            return outcome;
        }
    }
}
