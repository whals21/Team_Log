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

            // 힐러 (Healer) — Phase CC-2C Elara 리워크
            // 기획: ReworkDrafts/01_Healer.md
            // Phase CC-2C 키워드 구현:
            //   - AutoHealBonus → MercyResourceComponent.AutoHealPartyMembers에서 매 턴 자동 힐 +
            //   - MercyCleanseBonus → Mend Wounds 정화 시 Mercy +N (후속 Behavior 확장)
            //   - MercyBurstTargets → Mercy 버스트 대상 수 (기본 1 → N)
            CreateTrait("Trait_Healer_Blessing", "healer_blessing", "축복",
                "연결고리 자동 힐 위력 +2 (3→5)",
                CharacterClass.Healer, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] { Kw(KeywordType.AutoHealBonus, 2) });

            CreateTrait("Trait_Healer_PureHeal", "healer_pure_heal", "순수 치유",
                "Mend Wounds 정화 시 Mercy +3 추가 축전",
                CharacterClass.Healer, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.MercyCleanseBonus, 3) });

            CreateTrait("Trait_Healer_DivineShield", "healer_divine_shield", "신성 방패",
                "Mercy 버스트 대상 +1 (1명 → 2명)",
                CharacterClass.Healer, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.MercyBurstTargets, 2) });

            // 도적 (Rogue) — Phase CC-2A Umbra 리워크
            // 기획: ReworkDrafts/02_Rogue.md
            // Phase CC-2A 키워드 구현 완료:
            //   - ShadowsMaxUp → CharacterTraitHandler.ApplyPassiveEffects에서 Resource.MaxStacksBonus 적용
            //   - PowerAddVsDebuff → CharacterTraitHandler.GetBonusOutgoingDamage(target)에서 도트 적 +N
            //     (DamageCalculator.DealDamage가 target을 넘김)
            CreateTrait("Trait_Rogue_AssassinInstinct", "rogue_assassin_instinct", "그림자 심화",
                "Shadows 최대치 +1 (3→4). Shadows 4 = 치명타 피해 3.5배",
                CharacterClass.Rogue, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] { Kw(KeywordType.ShadowsMaxUp, 1) }); // Shadows MaxStacksBonus=1

            CreateTrait("Trait_Rogue_PoisonMaster", "rogue_poison_master", "약점 포착",
                "도트 디버프 적에게 위력 +3 (Backstab 조건 강화)",
                CharacterClass.Rogue, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.PowerAddVsDebuff, 3) }); // 도트 적 +3 위력

            CreateTrait("Trait_Rogue_EvasionMaster", "rogue_evasion_master", "그림자 보호",
                "Shadows 1+일 때 받는 피해 -3 (Shadows 유지 중 생존)",
                CharacterClass.Rogue, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.DamageReduction, 3) }); // 받는 피해 -3 (기존 2→3)

            // 궁수 (Archer) — Phase CC-2B Aster 리워크
            // 기획: ReworkDrafts/03_Archer.md
            // Phase CC-2B 키워드 구현:
            //   - ComboMaxPowerBonus → CharacterTraitHandler.GetBonusOutgoingDamage에서 Combo 최대치 시 +
            //   - PowerAddVsMark → Mark 상태 적 대상 +N
            CreateTrait("Trait_Archer_Marksman", "archer_marksman", "명사수",
                "Combo가 최대치(3)일 때 모든 스킬 위력 +3",
                CharacterClass.Archer, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] { Kw(KeywordType.ComboMaxPowerBonus, 3) });

            CreateTrait("Trait_Archer_WeakPoint", "archer_weak_point", "약점 포착",
                "Hunter's Mark 적에게 위력 +4 (Mark 의존 강화)",
                CharacterClass.Archer, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.PowerAddVsMark, 4) });

            CreateTrait("Trait_Archer_RapidFire", "archer_rapid_fire", "속사",
                "스킬 코스트 -1 (Quick Shot AP 1→0, 매 턴 무료 Combo 축전)",
                CharacterClass.Archer, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.CostAdd, -1) }); // TODO: Quick Shot에만 적용하도록 제한 검토 (현재 모든 스킬)

            // 네크로맨서 (Necromancer) — Phase CC-2F Mortis 리워크
            // 기획: ReworkDrafts/04_Necromancer.md
            // Phase CC-2F 키워드 구현:
            //   - SoulLinkMul → Soul Link 회복 비율 배수 (기본 0.5 → 0.75)
            //   - CurseExtraDamage → AttackDown(저주) 상태 적 대상 추가 데미지
            //   - CorpseKillEmpower → 적 처치 시 시체 영구 강화 +N
            CreateTrait("Trait_Necro_LifeLeech", "necro_life_leech", "생명력 흡수",
                "Soul Link 회복 비율 50% → 75% (강화 흡혈)",
                CharacterClass.Necromancer, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] { Kw(KeywordType.SoulLinkMul, 0.75f) });

            CreateTrait("Trait_Necro_CursePrice", "necro_curse_price", "저주의 대가",
                "AttackDown(저주) 상태 적에게 +3 추가 데미지 (시체 처치 가속)",
                CharacterClass.Necromancer, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.CurseExtraDamage, 3) });

            CreateTrait("Trait_Necro_DeathHarvest", "necro_death_harvest", "죽음의 수확",
                "적 처치 시 시체 스킬 교체 + 영구 강화 +2 (시체 스노우볼)",
                CharacterClass.Necromancer, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.CorpseKillEmpower, 2) });

            // 연금술사 (Alchemist) — Phase CC-2E Cael 리워크
            // 기획: ReworkDrafts/05_Alchemist.md
            // Phase CC-2E 키워드 구현:
            //   - DiscoverChoicesAdd → 발견 선택지 수 증가 (기본 3 → 3+N)
            //   - DiscoverWeightBonus → Crippling 카테고리 가중치 배수 (독 specialize)
            //   - DiscoverApplyAll → 전투당 1회 발견 선택지 모두 적용 (강화 물약)
            CreateTrait("Trait_Alch_PotionMaster", "alch_potion_master", "물약 명인",
                "발견 선택지 3 → 4개 (더 많은 옵션)",
                CharacterClass.Alchemist, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] { Kw(KeywordType.DiscoverChoicesAdd, 1) });

            CreateTrait("Trait_Alch_ToxicBurst", "alch_toxic_burst", "독성 폭발",
                "약화 물약 발견 풀에서 독/화상/출혈 등 독 계열 가중치 2배",
                CharacterClass.Alchemist, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.DiscoverWeightBonus, 2.0f) });

            CreateTrait("Trait_Alch_ReinforcedPotion", "alch_reinforced_potion", "강화 물약",
                "전투당 1회, 발견 선택지 모두 적용 가능 (3-4개 효과 동시 발동)",
                CharacterClass.Alchemist, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.DiscoverApplyAll, 1) });

            // 음유시인 (Bard) — Phase CC-2D Calliope 리워크
            // 기획: ReworkDrafts/06_Bard.md
            // Phase CC-2D 키워드 구현:
            //   - EchoPowerMul → 부 선율 배율 (기본 0.5 → 0.75)
            //   - RepeatNoPenalty → 같은 스킬 연속 시 부 선율 무효화 페널티 무시
            //   - EchoBonusEffect → 부 선율 추가 효과 (후속 구현)
            CreateTrait("Trait_Bard_BattleSong", "bard_battle_song", "전투 노래",
                "부 선율 위력 50% → 75% (메아리 강화)",
                CharacterClass.Bard, isDefault: true, unlockCost: 0, soulCost: 0,
                keywords: new[] { Kw(KeywordType.EchoPowerMul, 0.75f) });

            CreateTrait("Trait_Bard_CourageChord", "bard_courage_chord", "용기의 화음",
                "같은 스킬 연속 사용 시 부 선율 무효화 페널티 제거 (자유 연주)",
                CharacterClass.Bard, isDefault: false, unlockCost: 30, soulCost: 0,
                keywords: new[] { Kw(KeywordType.RepeatNoPenalty, 1) });

            CreateTrait("Trait_Bard_HealingMelody", "bard_healing_melody", "치유 멜로디",
                "EchoMelody 발동 시 추가 효과 (Valor=쉴드+5, Dissonance=추가 도트 등)",
                CharacterClass.Bard, isDefault: false, unlockCost: 60, soulCost: 1,
                keywords: new[] { Kw(KeywordType.EchoBonusEffect, 1) });

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
