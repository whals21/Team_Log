using TeamLog.Characters;
using TeamLog.Skill;

namespace TeamLog.Reward
{
    /// <summary>
    /// 증강 보상 제안 — 전투 보상에서 제시되는 [캐릭터 + 스킬 + 증강] 조합
    /// </summary>
    public class AugmentOffer
    {
        public AugmentData Augment { get; }
        public Character TargetCharacter { get; }
        public SkillInstance TargetSkill { get; }
        public int Tier { get; }
        public bool IsCursed { get; }

        public AugmentOffer(AugmentData augment, Character targetCharacter, SkillInstance targetSkill)
        {
            Augment = augment;
            TargetCharacter = targetCharacter;
            TargetSkill = targetSkill;
            Tier = augment != null ? augment.Tier : 1;
            IsCursed = augment != null && augment.IsCursed;
        }

        /// <summary>"전사의 강타 → 비용 감소"</summary>
        public string GetDisplayText()
        {
            if (Augment == null || TargetCharacter == null || TargetSkill == null)
                return "증강 보상";

            return $"{TargetCharacter.Name}의 {TargetSkill.Data.SkillName} → {Augment.AugmentName}";
        }

        /// <summary>증강 설명 + 저주 설명</summary>
        public string GetDetailText()
        {
            if (Augment == null) return "";

            string detail = Augment.Description;
            if (IsCursed && !string.IsNullOrEmpty(Augment.CurseDescription))
                detail += $"\n<color=#ff4444>[저주] {Augment.CurseDescription}</color>";

            return detail;
        }
    }
}
