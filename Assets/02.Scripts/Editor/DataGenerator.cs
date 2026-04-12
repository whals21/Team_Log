#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TeamLog.Characters;

namespace TeamLog.Editor
{
    /// <summary>
    /// 테스트용 데이터 자동 생성 에디터
    /// </summary>
    public static class DataGenerator
    {
        private const string SKILL_PATH = "Assets/03.Data/Skills";
        private const string CHAR_PATH = "Assets/03.Data/Characters";

        [MenuItem("TeamLog/Generate Test Data", false, 100)]
        public static void GenerateAllTestData()
        {
            GenerateSkillData();
            GenerateCharacterData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DataGenerator] 테스트 데이터 생성 완료!");
        }

        #region Skill Data

        private static void GenerateSkillData()
        {
            // 전사 스킬
            CreateSkill("Warrior_Strike", "강타", "적에게 물리 데미지를 입힙니다.",
                SkillType.Attack, TargetType.SingleEnemy, power: 15, weight: 40);

            CreateSkill("Warrior_Shield", "방패 방어", "이번 턴 동안 방어력이 증가합니다.",
                SkillType.Buff, TargetType.Self, power: 10, weight: 30,
                effect: StatusEffect.DefenseUp, duration: 1);

            CreateSkill("Warrior_Taunt", "도발", "적의 공격을 자신에게 유도합니다.",
                SkillType.Debuff, TargetType.SingleEnemy, power: 0, weight: 20);

            CreateSkill("Warrior_Rage", "분노", "다음 공격의 데미지가 증가합니다.",
                SkillType.Buff, TargetType.Self, power: 20, weight: 10,
                effect: StatusEffect.AttackUp, duration: 1);

            // 마법사 스킬
            CreateSkill("Mage_Fireball", "파이어볼", "적에게 불꽃 데미지를 입히고 화상을 입힙니다.",
                SkillType.Attack, TargetType.SingleEnemy, power: 20, weight: 35,
                effect: StatusEffect.Burn, duration: 2, effectValue: 5);

            CreateSkill("Mage_IceSpear", "얼음창", "적에게 얼음 데미지를 입힙니다.",
                SkillType.Attack, TargetType.SingleEnemy, power: 15, weight: 35);

            CreateSkill("Mage_MagicShield", "마법 방어막", "자신의 방어력을 증가시킵니다.",
                SkillType.Buff, TargetType.Self, power: 15, weight: 20,
                effect: StatusEffect.DefenseUp, duration: 1);

            CreateSkill("Mage_Meteor", "메테오", "모든 적에게 강력한 데미지를 입힙니다.",
                SkillType.Attack, TargetType.AllEnemies, power: 30, weight: 10);

            // 힐러 스킬
            CreateSkill("Healer_Heal", "치유", "아군 한 명의 체력을 회복합니다.",
                SkillType.Heal, TargetType.SingleAlly, power: 25, weight: 40);

            CreateSkill("Healer_Barrier", "보호막", "아군 한 명의 방어력을 증가시킵니다.",
                SkillType.Buff, TargetType.SingleAlly, power: 15, weight: 25,
                effect: StatusEffect.DefenseUp, duration: 2);

            CreateSkill("Healer_Purify", "정화", "아군의 약화 효과를 제거합니다.",
                SkillType.Buff, TargetType.SingleAlly, power: 0, weight: 20);

            CreateSkill("Healer_Blessing", "축복", "아군의 공격력을 증가시킵니다.",
                SkillType.Buff, TargetType.SingleAlly, power: 10, weight: 15,
                effect: StatusEffect.AttackUp, duration: 2);

            // 도적 스킬
            CreateSkill("Rogue_Backstab", "급소 공격", "적에게 치명타 데미지를 입힙니다.",
                SkillType.Attack, TargetType.SingleEnemy, power: 25, weight: 35);

            CreateSkill("Rogue_PoisonBlade", "독 바르기", "적에게 독 효과를 부여합니다.",
                SkillType.Debuff, TargetType.SingleEnemy, power: 5, weight: 25,
                effect: StatusEffect.Poison, duration: 3, effectValue: 8);

            CreateSkill("Rogue_Weaken", "약화", "적의 방어력을 감소시킵니다.",
                SkillType.Debuff, TargetType.SingleEnemy, power: 0, weight: 20,
                effect: StatusEffect.DefenseDown, duration: 2);

            CreateSkill("Rogue_DoubleStrike", "이중 타격", "적에게 2번 공격합니다.",
                SkillType.Attack, TargetType.SingleEnemy, power: 12, weight: 20);

            // 적 슬라임 스킬
            CreateSkill("Slime_Tackle", "몸통박치기", "기본 공격입니다.",
                SkillType.Attack, TargetType.SingleEnemy, power: 8, weight: 50);

            CreateSkill("Slime_AcidSpit", "산성 침", "독이 섞인 공격입니다.",
                SkillType.Attack, TargetType.SingleEnemy, power: 6, weight: 30,
                effect: StatusEffect.Poison, duration: 2, effectValue: 3);

            CreateSkill("Slime_Split", "분열 준비", "방어력이 증가합니다.",
                SkillType.Buff, TargetType.Self, power: 10, weight: 20,
                effect: StatusEffect.DefenseUp, duration: 1);

            CreateSkill("Slime_Jiggle", "출렁임", "아무 일도 일어나지 않습니다.",
                SkillType.Buff, TargetType.Self, power: 0, weight: 10);

            // 적 고블린 스킬
            CreateSkill("Goblin_Scratch", "긁기", "빠른 공격입니다.",
                SkillType.Attack, TargetType.SingleEnemy, power: 10, weight: 40);

            CreateSkill("Goblin_Bite", "물기", "강한 공격입니다.",
                SkillType.Attack, TargetType.SingleEnemy, power: 15, weight: 30);

            CreateSkill("Goblin_Steal", "약화 공격", "적의 공격력을 감소시킵니다.",
                SkillType.Attack, TargetType.SingleEnemy, power: 8, weight: 20,
                effect: StatusEffect.AttackDown, duration: 1);

            CreateSkill("Goblin_Hide", "은신", "방어력을 증가시킵니다.",
                SkillType.Buff, TargetType.Self, power: 8, weight: 10,
                effect: StatusEffect.DefenseUp, duration: 1);
        }

        private static void CreateSkill(string fileName, string name, string desc,
            SkillType type, TargetType target, int power, int weight,
            StatusEffect effect = StatusEffect.None, int duration = 0, int effectValue = 0)
        {
            var skill = ScriptableObject.CreateInstance<SkillData>();
            skill.name = name;

            SetPrivateField(skill, "_skillName", name);
            SetPrivateField(skill, "_description", desc);
            SetPrivateField(skill, "_skillType", type);
            SetPrivateField(skill, "_targetType", target);
            SetPrivateField(skill, "_power", power);
            SetPrivateField(skill, "_weight", weight);
            SetPrivateField(skill, "_statusEffect", effect);
            SetPrivateField(skill, "_effectDuration", duration);
            SetPrivateField(skill, "_effectValue", effectValue);

            AssetDatabase.CreateAsset(skill, $"{SKILL_PATH}/{fileName}.asset");
        }

        #endregion

        #region Character Data

        private static void GenerateCharacterData()
        {
            // 파티 캐릭터
            CreateCharacter("Char_Warrior", "전사", CharacterClass.Warrior,
                "근접 전투 전문가. 높은 체력과 방어력을 가집니다.",
                hp: 120, atk: 12, def: 8,
                skills: new[] { "Warrior_Strike", "Warrior_Shield", "Warrior_Taunt", "Warrior_Rage" });

            CreateCharacter("Char_Mage", "마법사", CharacterClass.Mage,
                "원소 마법의 달인. 강력한 마법 공격을 사용합니다.",
                hp: 70, atk: 18, def: 3,
                skills: new[] { "Mage_Fireball", "Mage_IceSpear", "Mage_MagicShield", "Mage_Meteor" });

            CreateCharacter("Char_Healer", "힐러", CharacterClass.Healer,
                "치유와 보조 마법의 전문가. 파티의 생존을 돕습니다.",
                hp: 80, atk: 8, def: 5,
                skills: new[] { "Healer_Heal", "Healer_Barrier", "Healer_Purify", "Healer_Blessing" });

            CreateCharacter("Char_Rogue", "도적", CharacterClass.Rogue,
                "민첩한 암살자. 높은 치명타와 상태이상 공격을 사용합니다.",
                hp: 75, atk: 15, def: 4,
                skills: new[] { "Rogue_Backstab", "Rogue_PoisonBlade", "Rogue_Weaken", "Rogue_DoubleStrike" });

            // 적 캐릭터
            CreateCharacter("Enemy_Slime", "슬라임", CharacterClass.Warrior,
                "끈적끈적한 젤리 형태의 몬스터.",
                hp: 40, atk: 8, def: 2,
                skills: new[] { "Slime_Tackle", "Slime_AcidSpit", "Slime_Split", "Slime_Jiggle" });

            CreateCharacter("Enemy_Goblin", "고블린", CharacterClass.Rogue,
                "교활하고 빠른 작은 몬스터.",
                hp: 50, atk: 12, def: 3,
                skills: new[] { "Goblin_Scratch", "Goblin_Bite", "Goblin_Steal", "Goblin_Hide" });
        }

        private static void CreateCharacter(string fileName, string name, CharacterClass charClass,
            string desc, int hp, int atk, int def, string[] skills)
        {
            var character = ScriptableObject.CreateInstance<CharacterData>();
            character.name = name;

            SetPrivateField(character, "_characterName", name);
            SetPrivateField(character, "_characterClass", charClass);
            SetPrivateField(character, "_description", desc);
            SetPrivateField(character, "_baseHP", hp);
            SetPrivateField(character, "_baseATK", atk);
            SetPrivateField(character, "_baseDEF", def);

            // 스킬 리스트 설정
            var skillList = new List<SkillData>();
            foreach (var skillName in skills)
            {
                var skill = AssetDatabase.LoadAssetAtPath<SkillData>($"{SKILL_PATH}/{skillName}.asset");
                if (skill != null)
                    skillList.Add(skill);
            }
            SetPrivateField(character, "_skills", skillList);

            AssetDatabase.CreateAsset(character, $"{CHAR_PATH}/{fileName}.asset");
        }

        #endregion

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}
#endif
