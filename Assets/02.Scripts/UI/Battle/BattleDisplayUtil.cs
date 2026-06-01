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
            StatusEffectType.Taunt => "도발",
            _ => type.ToString()
        };

        /// <summary>
        /// 상태이상 타입 → 뱃지 배경색 (버프: 녹색계, 디버프: 빨간계, 특수: 노란계)
        /// </summary>
        public static Color GetEffectColor(StatusEffectType type) => type switch
        {
            // 디버프 (빨간/보라 계열)
            StatusEffectType.Poison => new Color(0.55f, 0.1f, 0.55f),   // 보라
            StatusEffectType.Burn => new Color(0.8f, 0.3f, 0.05f),     // 주황
            StatusEffectType.Stun => new Color(0.6f, 0.6f, 0.1f),     // 어두운 노랑
            StatusEffectType.Freeze => new Color(0.2f, 0.5f, 0.8f),   // 파랑
            StatusEffectType.Sleep => new Color(0.4f, 0.3f, 0.6f),    // 연보라
            StatusEffectType.Bleed => new Color(0.7f, 0.05f, 0.05f),  // 진빨강
            StatusEffectType.DefenseDown => new Color(0.7f, 0.2f, 0.1f),
            StatusEffectType.AttackDown => new Color(0.6f, 0.15f, 0.15f),
            // 버프 (녹색/청록 계열)
            StatusEffectType.DefenseUp => new Color(0.1f, 0.5f, 0.3f),
            StatusEffectType.AttackUp => new Color(0.15f, 0.55f, 0.2f),
            StatusEffectType.Regeneration => new Color(0.1f, 0.5f, 0.5f),
            StatusEffectType.Shield => new Color(0.5f, 0.35f, 0.15f), // 갈색 (쉴드 색)
            // 특수
            StatusEffectType.Taunt => new Color(0.6f, 0.45f, 0.1f),
            _ => new Color(0.4f, 0.4f, 0.4f)
        };

        /// <summary>
        /// 상태이상 타입 → 설명 문구
        /// </summary>
        public static string GetEffectDescription(StatusEffectType type) => type switch
        {
            StatusEffectType.Poison => "매 턴 시작 시 수치만큼 피해를 받습니다.",
            StatusEffectType.Burn => "매 턴 시작 시 수치만큼 화염 피해를 받습니다.",
            StatusEffectType.Stun => "행동할 수 없습니다. 턴을 건너뜁니다.",
            StatusEffectType.Freeze => "행동할 수 없으며, 받는 피해가 증가합니다.",
            StatusEffectType.Sleep => "피해를 받으면 깨어납니다. 그 전까지 행동 불가.",
            StatusEffectType.Bleed => "매 턴 시작 시 수치만큼 물리 피해를 받습니다.",
            StatusEffectType.DefenseUp => "방어력이 수치만큼 증가합니다.",
            StatusEffectType.DefenseDown => "방어력이 수치만큼 감소합니다.",
            StatusEffectType.AttackUp => "공격력이 수치만큼 증가합니다.",
            StatusEffectType.AttackDown => "공격력이 수치만큼 감소합니다.",
            StatusEffectType.Regeneration => "매 턴 시작 시 수치만큼 HP를 회복합니다.",
            StatusEffectType.Shield => "수치만큼 피해를 흡수하는 보호막이 생성됩니다.",
            StatusEffectType.Taunt => "적의 공격 대상이 이 캐릭터로 고정됩니다.",
            _ => "알 수 없는 상태 효과입니다."
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
                    SkillType.Purify => "수치",
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
