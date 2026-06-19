#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TeamLog.Characters;
using TeamLog.Skill;

namespace TeamLog.Editor
{
    /// <summary>
    /// DataGenerator — 캐릭터 장착형 특성 데이터 생성 (Phase 8A)
    /// 진입점/스킬/캐릭터/유틸리티: DataGenerator.cs
    /// 증강 데이터/스폰 패턴: DataGenerator.Augments.cs
    /// 이벤트 데이터: DataGenerator.Events.cs
    /// 유물 데이터: DataGenerator.Relics.cs
    /// 팔레트 (UI/오디오/VFX): DataGenerator.Palettes.cs
    /// 스테이지 테마: DataGenerator.Stages.cs
    /// 캐릭터 특성: DataGenerator.Traits.cs
    /// 메타 강화: DataGenerator.MetaUpgrades.cs
    /// </summary>
    public static partial class DataGenerator
    {
        private const string TRAIT_PATH = "Assets/03.Data/CharacterTraits";

        [MenuItem("TeamLog/Generate Character Traits", false, 110)]
        public static void GenerateTraitData()
        {
            EnsureFolder(TRAIT_PATH);

            // 전사 (Warrior)
            CreateTrait("Trait_Warrior_ShieldMastery", "warrior_shield_mastery", "방패 숙련",
                "매 턴 시작 시 쉴드 +2 획득",
                CharacterClass.Warrior, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] { Kw(KeywordType.ShieldPerTurn, 2, KeywordTrigger.OnTurnStart) });

            CreateTrait("Trait_Warrior_Fury", "warrior_fury", "분노",
                "HP 30% 미만 시 위력 x1.3",
                CharacterClass.Warrior, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.PowerMul, 1.3f, KeywordTrigger.HPBelow, 0.3f) });

            CreateTrait("Trait_Warrior_IronWall", "warrior_iron_wall", "철벽",
                "받는 피해 -3 (영구)",
                CharacterClass.Warrior, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.DamageReduction, 3) });

            // 마법사 (Mage)
            CreateTrait("Trait_Mage_MagicArmor", "mage_magic_armor", "마법 갑옷",
                "매 턴 시작 시 쉴드 +1 획득",
                CharacterClass.Mage, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] { Kw(KeywordType.ShieldPerTurn, 1, KeywordTrigger.OnTurnStart) });

            CreateTrait("Trait_Mage_ElementalOverload", "mage_elemental_overload", "원소 폭주",
                "HP 50% 미만 시 위력 x1.25",
                CharacterClass.Mage, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.PowerMul, 1.25f, KeywordTrigger.HPBelow, 0.5f) });

            CreateTrait("Trait_Mage_ArcanePersistence", "mage_arcane_persistence", "비전 지속",
                "매 턴 AP +1",
                CharacterClass.Mage, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.ExtraAP, 1) });

            // 힐러 (Healer)
            CreateTrait("Trait_Healer_Blessing", "healer_blessing", "축복",
                "힐 효과 +15%",
                CharacterClass.Healer, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] { Kw(KeywordType.HealMul, 1.15f) });

            CreateTrait("Trait_Healer_PureHeal", "healer_pure_heal", "순수 치유",
                "적 처치 시 HP +3 회복",
                CharacterClass.Healer, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.OnKillHeal, 3, KeywordTrigger.OnKill) });

            CreateTrait("Trait_Healer_DivineShield", "healer_divine_shield", "신성 방패",
                "힐 적용 시 쉴드 +2 획득",
                CharacterClass.Healer, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.ShieldPerTurn, 2, KeywordTrigger.OnHealApplied) });

            // 도적 (Rogue)
            CreateTrait("Trait_Rogue_AssassinInstinct", "rogue_assassin_instinct", "암살자 본능",
                "추가 고정 데미지 +2",
                CharacterClass.Rogue, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] { Kw(KeywordType.BonusOutgoingDamage, 2) });

            CreateTrait("Trait_Rogue_PoisonMaster", "rogue_poison_master", "독 마스터",
                "상태이상 지속시간 +1턴",
                CharacterClass.Rogue, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.DurationAdd, 1) });

            CreateTrait("Trait_Rogue_EvasionMaster", "rogue_evasion_master", "회피의 대가",
                "받는 피해 -2 (영구)",
                CharacterClass.Rogue, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.DamageReduction, 2) });

            // 궁수 (Archer)
            CreateTrait("Trait_Archer_Marksman", "archer_marksman", "명사수",
                "위력 +2 가산",
                CharacterClass.Archer, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] { Kw(KeywordType.PowerAdd, 2) });

            CreateTrait("Trait_Archer_WeakPoint", "archer_weak_point", "약점 포착",
                "적 HP 60% 미만 시 위력 x1.4",
                CharacterClass.Archer, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.PowerMul, 1.4f, KeywordTrigger.OnEnemyLowHP, 0.6f) });

            CreateTrait("Trait_Archer_RapidFire", "archer_rapid_fire", "속사",
                "스킬 코스트 -1",
                CharacterClass.Archer, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.CostAdd, -1) });

            // 네크로맨서 (Necromancer)
            CreateTrait("Trait_Necro_LifeLeech", "necro_life_leech", "생명력 흡수",
                "준 데미지의 15% 회복",
                CharacterClass.Necromancer, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] { Kw(KeywordType.DamageDealtHealPercent, 0.15f) });

            CreateTrait("Trait_Necro_CursePrice", "necro_curse_price", "저주의 대가",
                "버프/디버프 효과 x1.3",
                CharacterClass.Necromancer, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.EffectMul, 1.3f) });

            CreateTrait("Trait_Necro_DeathHarvest", "necro_death_harvest", "죽음의 수확",
                "적 처치당 공격력 +1 누적",
                CharacterClass.Necromancer, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.StackingPowerOnKill, 1, KeywordTrigger.OnKill) });

            // 연금술사 (Alchemist)
            CreateTrait("Trait_Alch_PotionMaster", "alch_potion_master", "물약 명인",
                "힐/쉴드 효과 +10%",
                CharacterClass.Alchemist, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] {
                    Kw(KeywordType.HealMul, 1.1f),
                    Kw(KeywordType.ShieldMul, 1.1f)
                });

            CreateTrait("Trait_Alch_ToxicBurst", "alch_toxic_burst", "독성 폭발",
                "상태이상 지속시간 +2턴",
                CharacterClass.Alchemist, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.DurationAdd, 2) });

            CreateTrait("Trait_Alch_ReinforcedPotion", "alch_reinforced_potion", "강화 물약",
                "전투 시작 시 최대 HP +15",
                CharacterClass.Alchemist, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.MaxHPUp, 15, KeywordTrigger.OnBattleStart) });

            // 음유시인 (Bard)
            CreateTrait("Trait_Bard_BattleSong", "bard_battle_song", "전투 노래",
                "매 턴 AP +1",
                CharacterClass.Bard, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] { Kw(KeywordType.ExtraAP, 1) });

            CreateTrait("Trait_Bard_CourageChord", "bard_courage_chord", "용기의 화음",
                "전투 시작 시 ATK +2 (영구)",
                CharacterClass.Bard, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.ATKUp, 2, KeywordTrigger.OnBattleStart) });

            CreateTrait("Trait_Bard_HealingMelody", "bard_healing_melody", "치유 멜로디",
                "매 턴 종료 시 HP +2 회복",
                CharacterClass.Bard, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.HPPerTurn, 2, KeywordTrigger.OnTurnEnd) });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DataGenerator] 캐릭터 특성 24종 생성 완료 (8 캐릭터 × 3 특성)");
        }

        private static void CreateTrait(string fileName, string traitId, string displayName, string desc,
            CharacterClass targetClass, bool isDefault, int unlockCost, int soulCost,
            KeywordEntry[] keywords = null)
        {
            EnsureFolder(TRAIT_PATH);
            var path = $"{TRAIT_PATH}/{fileName}.asset";
            var trait = GetOrCreateAsset<CharacterTraitData>(path);
            trait.name = fileName;

            SetPrivateField(trait, "_traitId", traitId);
            SetPrivateField(trait, "_displayName", displayName);
            SetPrivateField(trait, "_description", desc);
            SetPrivateField(trait, "_targetClass", targetClass);
            SetPrivateField(trait, "_isDefault", isDefault);
            SetPrivateField(trait, "_unlockCost", unlockCost);
            SetPrivateField(trait, "_soulUnlockCost", soulCost);

            if (keywords != null && keywords.Length > 0)
                SetPrivateField(trait, "_keywords", keywords);

            EditorUtility.SetDirty(trait);
        }
    }
}
#endif
