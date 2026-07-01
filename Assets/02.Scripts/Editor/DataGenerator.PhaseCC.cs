#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Skill;

namespace TeamLog.Editor
{
    /// <summary>
    /// DataGenerator — Phase CC 캐릭터 생성 (Ashe/Duran/Lumi/Sibyl/Taranis).
    /// 진입점/스킬/캐릭터/유틸리티: DataGenerator.cs
    /// 증강: DataGenerator.Augments.cs
    /// 이벤트: DataGenerator.Events.cs
    /// 유물: DataGenerator.Relics.cs
    /// 팔레트: DataGenerator.Palettes.cs
    /// 스테이지: DataGenerator.Stages.cs
    /// 어센션: DataGenerator.Ascension.cs
    /// </summary>
    public static partial class DataGenerator
    {
        /// <summary>Phase CC 캐릭터 5종 생성 (Ashe/Duran/Lumi/Sibyl/Taranis).</summary>
        private static void GeneratePhaseCCCharacters()
        {
            // Ashe — Pyromancer (Ember 자해 폭딜). Mage 스킬 임시 재사용.
            CreatePhaseCCCharacter("Char_Ashe", "아셰", CharacterClass.Pyromancer,
                "화염 마법사. Ember 자원을 축적하여 자해 위험을 감수하고 폭딜을 낸다",
                70, 0, 0, new[] { "Mage_Fireball", "Mage_IceSpear", "Mage_MagicShield", "Mage_Meteor" },
                EnemyTrait.ArcaneFury, true, "", ResourceType.Ember);

            // Duran — Warrior (Vengeance 복수 게이지). Warrior 스킬 재사용.
            CreatePhaseCCCharacter("Char_Duran", "듀란", CharacterClass.Warrior,
                "불멸의 성벽. 피격 시 Vengeance가 축적되며 소비 스킬로 버스트 딜",
                120, 0, 0, new[] { "Warrior_Strike", "Warrior_Shield", "Warrior_Taunt", "Warrior_Rage" },
                EnemyTrait.Sturdy, true, "", ResourceType.Vengeance);

            // Lumi — Cryomancer (Frost 통제). Mage 스킬 변형.
            CreatePhaseCCCharacter("Char_Lumi", "루미", CharacterClass.Cryomancer,
                "냉기 마법사. Frost를 축적하여 적을 얼린다",
                75, 0, 0, new[] { "Mage_IceSpear", "Mage_Fireball", "Mage_MagicShield", "Mage_Meteor" },
                EnemyTrait.ArcaneFury, true, "", ResourceType.Frost);

            // Sibyl — Oracle (Prophecy 지연 발동 — 자원 None 임시, 정식 메카닉은 추후)
            CreatePhaseCCCharacter("Char_Sibyl", "시빌", CharacterClass.Oracle,
                "예언자. 미래에 투자하는 서포터 (Prophecy 메카닉은 추후 구현)",
                80, 0, 0, new[] { "Healer_Heal", "Healer_Barrier", "Healer_Purify", "Healer_Blessing" },
                EnemyTrait.Regenerate, true, "", ResourceType.None);

            // Taranis — Stormcaller (Charge Network — 자원 None 임시, 정식 메카닉은 추후)
            CreatePhaseCCCharacter("Char_Taranis", "타라니스", CharacterClass.Stormcaller,
                "폭풍 소환사. 네트워크에 투자하는 간접 딜러 (Charge 메카닉은 추후 구현)",
                85, 0, 0, new[] { "Mage_Fireball", "Mage_Meteor", "Mage_MagicShield", "Mage_IceSpear" },
                EnemyTrait.ArcaneFury, true, "", ResourceType.None);

            Debug.Log("[DataGenerator] Phase CC 캐릭터 5종 생성 완료 (Ashe/Duran/Lumi/Sibyl/Taranis)");
        }

        /// <summary>Phase CC 캐릭터 생성 헬퍼 — ResourceType 포함.</summary>
        private static void CreatePhaseCCCharacter(string fileName, string name, CharacterClass charClass,
            string desc, int hp, int atk, int def, string[] skills, EnemyTrait trait,
            bool isDefault, string unlockCondition, ResourceType resourceType)
        {
            var path = $"{CHAR_PATH}/{fileName}.asset";
            var character = GetOrCreateAsset<CharacterData>(path);
            character.name = fileName;

            SetPrivateField(character, "_characterName", name);
            SetPrivateField(character, "_characterClass", charClass);
            SetPrivateField(character, "_description", desc);
            SetPrivateField(character, "_baseHP", hp);
            SetPrivateField(character, "_baseATK", atk);
            SetPrivateField(character, "_baseDEF", def);
            SetPrivateField(character, "_enemyTrait", trait);
            SetPrivateField(character, "_isDefault", isDefault);
            SetPrivateField(character, "_unlockCondition", unlockCondition);
            SetPrivateField(character, "_resourceType", resourceType);

            var skillList = new List<SkillData>();
            foreach (var skillName in skills)
            {
                var skill = AssetDatabase.LoadAssetAtPath<SkillData>($"{SKILL_PATH}/{skillName}.asset");
                if (skill != null) skillList.Add(skill);
            }
            SetPrivateField(character, "_skills", skillList);

            EditorUtility.SetDirty(character);
        }
    }
}
#endif
