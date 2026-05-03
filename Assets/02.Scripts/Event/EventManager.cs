using System.Collections.Generic;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Reward;

namespace TeamLog.Event
{
    /// <summary>
    /// 이벤트 처리 관리자 — 순수 C# 클래스
    /// </summary>
    public class EventManager
    {
        private readonly System.Random _rng = new();

        public event System.Action<string> OnEventText;
        public event System.Action<EventChoice> OnChoiceMade;

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

            // 스킬 획득
            if (outcome.GiveRandomSkill)
            {
                var skill = runState.AcquireRandomSkill();
                if (skill != null && outcome.ResultText != null)
                    outcome.ResultText += $" ({skill.SkillName} 획득!)";
            }

            // 아이템 획득
            if (outcome.GiveRandomItem)
            {
                var item = runState.AcquireRandomItem();
                if (item != null && outcome.ResultText != null)
                    outcome.ResultText += $" ({item.ItemName} 획득!)";
            }

            OnChoiceMade?.Invoke(choice);
            return outcome;
        }
    }
}
