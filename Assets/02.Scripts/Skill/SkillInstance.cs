namespace TeamLog.Characters
{
    /// <summary>
    /// 스킬 인스턴스 — SkillData(SO 템플릿) + 업그레이드 레벨
    /// 캐릭터별 독립 상태를 관리하여 공유 SO를 수정하지 않음
    /// </summary>
    public class SkillInstance
    {
        public SkillData Data { get; }
        public int UpgradeLevel { get; private set; }

        public const int MaxUpgradeLevel = 3;

        public SkillInstance(SkillData data, int upgradeLevel = 0)
        {
            Data = data;
            UpgradeLevel = upgradeLevel;
        }

        /// <summary>업그레이드 반영 위력</summary>
        public int EffectivePower => Data.Power + GetPowerBonus();

        /// <summary>업그레이드 반영 비용 (+3 시 -1)</summary>
        public int EffectiveCost => UpgradeLevel >= 3 ? System.Math.Max(0, Data.Cost - 1) : Data.Cost;

        /// <summary>업그레이드 반영 가중치 (+1당 +3)</summary>
        public int EffectiveWeight => Data.Weight + UpgradeLevel * 3;

        /// <summary>업그레이드 가능 여부</summary>
        public bool CanUpgrade => UpgradeLevel < MaxUpgradeLevel;

        /// <summary>업그레이드 수행 — 성공 시 true</summary>
        public bool Upgrade()
        {
            if (!CanUpgrade) return false;
            UpgradeLevel++;
            return true;
        }

        private int GetPowerBonus()
        {
            return Data.Type switch
            {
                SkillType.Attack => UpgradeLevel * 5,
                SkillType.Heal => UpgradeLevel * 4,
                SkillType.Shield => UpgradeLevel * 3,
                _ => UpgradeLevel * 2
            };
        }
    }
}
