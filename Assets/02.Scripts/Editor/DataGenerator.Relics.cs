#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TeamLog.Reward;
using TeamLog.Skill;

namespace TeamLog.Editor
{
    /// <summary>
    /// DataGenerator — 유물 데이터 생성 (GenerateRelicData + CreateRelic + SetField 헬퍼)
    /// 진입점/스킬/캐릭터/유틸리티: DataGenerator.cs
    /// 증강 데이터/스폰 패턴: DataGenerator.Augments.cs
    /// 이벤트 데이터: DataGenerator.Events.cs
    /// 팔레트 (UI/오디오/VFX): DataGenerator.Palettes.cs
    /// </summary>
    public static partial class DataGenerator
    {
        #region Relic Data

        private const string RELIC_PATH = "Assets/03.Data/Relics";

        private static void GenerateRelicData()
        {
            EnsureFolder(RELIC_PATH);

            CreateRelic("Relic_BurningSword", "불타는 검", "공격 시 추가 데미지 +3",
                RelicTrigger.OnSkillUsed, 3, RewardRarity.Common, price: 80,
                new[] { Kw(KeywordType.BonusOutgoingDamage, 3) });

            CreateRelic("Relic_IronHide", "철가죽", "받는 피해 -2",
                RelicTrigger.OnDamageReceived, 2, RewardRarity.Common, price: 90,
                new[] { Kw(KeywordType.DamageReduction, 2) });

            CreateRelic("Relic_RegenRing", "재생의 반지", "매 턴 3 HP 회복",
                RelicTrigger.TurnEnd, 3, RewardRarity.Common, price: 60,
                new[] { Kw(KeywordType.HPPerTurn, 3, KeywordTrigger.OnTurnEnd) });

            CreateRelic("Relic_GoldCharm", "황금 부적", "골드 획득 시 +15 골드",
                RelicTrigger.OnGoldEarned, 15, RewardRarity.Rare, price: 120,
                new[] { Kw(KeywordType.BonusGold, 15, KeywordTrigger.OnGoldEarned) });

            CreateRelic("Relic_ShieldAmulet", "방패 부적", "방패 스킬 사용 시 +3 쉴드",
                RelicTrigger.OnShieldGained, 3, RewardRarity.Common, price: 70,
                new[] { Kw(KeywordType.ShieldPerTurn, 3, KeywordTrigger.OnShieldGained) });

            CreateRelic("Relic_VampireFang", "흡혈 송곳니", "적 처치 시 +5 HP 회복",
                RelicTrigger.OnKill, 5, RewardRarity.Rare, price: 100,
                new[] { Kw(KeywordType.OnKillHeal, 5, KeywordTrigger.OnKill) });

            CreateRelic("Relic_BerserkerMark", "광전사 인장", "적 처치당 공격력 +2 누적",
                RelicTrigger.OnKill, 2, RewardRarity.Unique, price: 180,
                new[] { Kw(KeywordType.StackingPowerOnKill, 2, KeywordTrigger.OnKill) });

            CreateRelic("Relic_LuckyClover", "네잎클로버", "드로우 가중치 +5",
                RelicTrigger.BattleStart, 5, RewardRarity.Rare, price: 110,
                new[] { Kw(KeywordType.DrawWeightAdd, 5) });

            CreateRelic("Relic_ThornArmor", "가시 갑옷", "피격 시 반사 데미지 2",
                RelicTrigger.OnDamageReceived, 2, RewardRarity.Rare, price: 130,
                new[] { Kw(KeywordType.CounterDamage, 2) });

            CreateRelic("Relic_SwiftBoots", "질풍 부츠", "매 턴 쉴드 +2",
                RelicTrigger.TurnStart, 2, RewardRarity.Rare, price: 100,
                new[] { Kw(KeywordType.ShieldPerTurn, 2, KeywordTrigger.OnTurnStart) });

            CreateRelic("Relic_WarBanner", "전투 깃발", "전투 시작 시 쉴드 +5 (전체)",
                RelicTrigger.BattleStart, 5, RewardRarity.Unique, price: 160,
                new[] { Kw(KeywordType.ShieldPerTurn, 5, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_HealingHerb", "치유 허브", "전투 시작 시 파티 HP 10 회복",
                RelicTrigger.BattleStart, 10, RewardRarity.Common, price: 60,
                new[] { Kw(KeywordType.HPPerTurn, 10, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_LifeCrystal", "생명력의 결정", "전투 시작 시 최대 HP +20",
                RelicTrigger.BattleStart, 20, RewardRarity.Common, price: 80,
                new[] { Kw(KeywordType.MaxHPUp, 20, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_WeaponStone", "무기 강화석", "전투 시작 시 공격력 +3",
                RelicTrigger.BattleStart, 3, RewardRarity.Rare, price: 100,
                new[] { Kw(KeywordType.ATKUp, 3, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_HardShell", "단단한 껍질", "전투 시작 시 방어력 +3",
                RelicTrigger.BattleStart, 3, RewardRarity.Common, price: 90,
                new[] { Kw(KeywordType.DEFUp, 3, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_DragonHeart", "드래곤의 심장", "전투 시작 시 최대 HP +50",
                RelicTrigger.BattleStart, 50, RewardRarity.Unique, price: 200,
                new[] { Kw(KeywordType.MaxHPUp, 50, KeywordTrigger.OnBattleStart) });

            Debug.Log($"[DataGenerator] 유물 데이터 생성 완료");
        }

        private static void CreateRelic(string fileName, string relicName, string desc,
            RelicTrigger trigger, int effectValue, RewardRarity rarity, int price = 0,
            KeywordEntry[] keywords = null)
        {
            var path = $"{RELIC_PATH}/{fileName}.asset";
            var relic = GetOrCreateAsset<RelicData>(path);

            // 필드 설정 via SerializedObject
            var so = new SerializedObject(relic);
            SetField(so, "_relicName", relicName);
            SetField(so, "_description", desc);
            SetField(so, "_trigger", (int)trigger);
            SetField(so, "_effectValue", effectValue);
            SetField(so, "_rarity", (int)rarity);
            SetField(so, "_price", price);
            so.ApplyModifiedProperties();

            // 키워드 설정 via SetPrivateField (SerializedObject 배열은 복잡하므로)
            if (keywords != null && keywords.Length > 0)
                SetPrivateField(relic, "_keywords", keywords);

            EditorUtility.SetDirty(relic);
        }

        private static void SetField(SerializedObject so, string fieldName, int value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null) prop.intValue = value;
        }

        private static void SetField(SerializedObject so, string fieldName, string value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null) prop.stringValue = value;
        }

        #endregion
    }
}
#endif
