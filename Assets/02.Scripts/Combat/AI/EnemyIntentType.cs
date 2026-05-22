using System.Collections.Generic;
using TeamLog.Characters;

namespace TeamLog.Combat.AI
{
    /// <summary>
    /// 적 행동 의도 타입
    /// </summary>
    public enum EnemyIntentType
    {
        None,
        Attack,     // 공격 예정
        Shield,     // 쉴드 예정
        Heal,       // 치유 예정
        Buff,       // 버프 예정
        Debuff,     // 디버프 예정
        Unknown     // 알 수 없음
    }

    /// <summary>
    /// 적 행동 의도 정보 — SkillData + 타겟 정보 포함
    /// </summary>
    public class EnemyIntent
    {
        public EnemyIntentType Type { get; }
        public int Value { get; }
        public string Description { get; }
        public SkillData Skill { get; }
        public List<Character> Targets { get; }
        public string TargetDisplay { get; }

        public EnemyIntent(EnemyIntentType type, int value = 0, string description = "",
            SkillData skill = null, List<Character> targets = null, string targetDisplay = "")
        {
            Type = type;
            Value = value;
            Description = description;
            Skill = skill;
            Targets = targets ?? new List<Character>();
            TargetDisplay = targetDisplay;
        }

        /// <summary>
        /// SkillData와 선택된 타겟으로부터 EnemyIntent 생성
        /// 공격 스킬은 ATK + Power를 최종 위력으로 저장
        /// </summary>
        public static EnemyIntent FromSkill(SkillData skill, List<Character> targets, Character owner)
        {
            if (skill == null) return new EnemyIntent(EnemyIntentType.None);

            string targetDisplay = BuildTargetDisplay(skill, targets, owner);
            var intentType = MapToIntentType(skill.Type);

            // 공격 타입은 ATK + Power가 최종 위력
            int displayPower = skill.Type == SkillType.Attack && owner != null
                ? owner.Stats.GetStat(StatType.ATK) + skill.Power
                : skill.Power;

            return new EnemyIntent(intentType, displayPower, "", skill, targets, targetDisplay);
        }

        /// <summary>
        /// Info 영역에 표시할 텍스트: "스킬명 위력 → 타겟" (위력은 ATK + Power)
        /// </summary>
        public string GetDisplayText()
        {
            if (Type == EnemyIntentType.None || Skill == null) return "";

            string skillLabel = Skill.SkillName;
            string powerText = Value > 0 ? $" {Value}" : "";
            return $"{skillLabel}{powerText}{TargetDisplay}";
        }

        private static EnemyIntentType MapToIntentType(SkillType skillType)
        {
            return skillType switch
            {
                SkillType.Attack => EnemyIntentType.Attack,
                SkillType.Shield => EnemyIntentType.Shield,
                SkillType.Heal => EnemyIntentType.Heal,
                SkillType.Buff => EnemyIntentType.Buff,
                SkillType.Debuff => EnemyIntentType.Debuff,
                _ => EnemyIntentType.Unknown
            };
        }

        private static string BuildTargetDisplay(SkillData skill, List<Character> targets, Character owner)
        {
            return skill.Target switch
            {
                TargetType.SingleEnemy => targets.Count > 0 ? $"→ {targets[0].Name}" : "",
                TargetType.AllEnemies => "→ 전체",
                TargetType.Self => "",
                TargetType.SingleAlly => targets.Count > 0 ? $"→ {targets[0].Name}" : "",
                TargetType.AllAllies => "→ 전체아군",
                _ => ""
            };
        }
    }
}
