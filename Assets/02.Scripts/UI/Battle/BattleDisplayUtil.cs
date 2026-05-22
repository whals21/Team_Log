using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 전투 UI 공통 표시 유틸리티 — 중복 로직 통합
    /// </summary>
    public static class BattleDisplayUtil
    {
        /// <summary>
        /// 상태이상 타입 → 한국어 라벨
        /// </summary>
        public static string GetEffectLabel(StatusEffectType type) => type switch
        {
            StatusEffectType.Poison => "독",
            StatusEffectType.Burn => "화상",
            StatusEffectType.Stun => "기절",
            StatusEffectType.Freeze => "빙결",
            StatusEffectType.Sleep => "수면",
            StatusEffectType.Bleed => "출혈",
            StatusEffectType.DefenseUp => "방어 증가",
            StatusEffectType.DefenseDown => "방어 감소",
            StatusEffectType.AttackUp => "공격 증가",
            StatusEffectType.AttackDown => "공격 감소",
            StatusEffectType.Regeneration => "재생",
            StatusEffectType.Shield => "보호막",
            _ => type.ToString()
        };

        /// <summary>
        /// 쉴드 바 앵커 갱신 — HP 바 끝점부터 겹쳐서 표시
        /// </summary>
        public static void UpdateShieldBar(Image shieldFill, float hpRatio, int shield, int maxHP)
        {
            if (shieldFill == null) return;

            if (shield > 0 && maxHP > 0)
            {
                float shieldEnd = Mathf.Min(1f, hpRatio + (float)shield / maxHP);
                shieldFill.rectTransform.anchorMin = new Vector2(hpRatio, 0f);
                shieldFill.rectTransform.anchorMax = new Vector2(shieldEnd, 1f);
                shieldFill.gameObject.SetActive(true);
            }
            else
            {
                shieldFill.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 스킬 수치 요약 문자열 생성 (위력, 상태이상 등)
        /// 공격 스킬은 캐릭터 ATK 포함하여 최종 위력 계산
        /// </summary>
        public static string BuildSkillDescription(SkillData skill, Character caster, string separator = " | ")
        {
            var parts = new List<string>();

            if (skill.Power > 0)
            {
                int displayPower = skill.Type == SkillType.Attack && caster != null
                    ? caster.Stats.GetStat(StatType.ATK) + skill.Power
                    : skill.Power;

                string label = skill.Type switch
                {
                    SkillType.Attack => "위력",
                    SkillType.Shield => "쉴드",
                    SkillType.Heal => "회복",
                    SkillType.Buff => "수치",
                    SkillType.Debuff => "수치",
                    _ => "수치"
                };
                parts.Add($"{label} {displayPower}");
            }

            if (skill.StatusEffect != StatusEffectType.None)
            {
                string effectName = GetEffectLabel(skill.StatusEffect);
                string duration = skill.EffectDuration > 0 ? $" ({skill.EffectDuration}턴)" : "";
                string value = skill.EffectValue > 0 ? $" {skill.EffectValue}" : "";
                parts.Add($"{effectName}{value}{duration}");
            }

            return string.Join(separator, parts);
        }
    }
}
