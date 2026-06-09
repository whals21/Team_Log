#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using TeamLog.Characters;
using TeamLog.Combat.AI;
using TeamLog.Event;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.Skill;
using TeamLog.UI;

namespace TeamLog.Editor
{
    /// <summary>
    /// CSV 테이블 → ScriptableObject 에셋 자동 생성 에디터
    /// 진입점/스킬/캐릭터/패턴/유틸리티: DataGenerator.cs
    /// 증강 데이터/스폰 패턴: DataGenerator.Augments.cs
    /// 콘텐츠 데이터 (이벤트/유물/팔레트): DataGenerator.Content.cs
    /// </summary>
    public static partial class DataGenerator
    {
        private const string TABLE_PATH = "Assets/03.Data/Tables";
        private const string SKILL_PATH = "Assets/03.Data/Skills";
        private const string CHAR_PATH = "Assets/03.Data/Characters";
        private const string PATTERN_PATH = "Assets/03.Data/Patterns";
        private const string EVENT_PATH = "Assets/03.Data/Events";
        private const string AUGMENT_PATH = "Assets/03.Data/Augments";
        private const string SPAWN_PATTERN_PATH = "Assets/03.Data/SpawnPatterns";

        // Icon base paths
        private const string PICTO_BASE = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_PictoIcons/128";
        private const string ITEM_BASE = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128";
        private const string RUNE_BASE = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_RuneIcons/256";

        [MenuItem("TeamLog/Generate Test Data", false, 100)]
        public static void GenerateAllTestData()
        {
            EnsureFolder(SKILL_PATH);
            EnsureFolder(CHAR_PATH);
            EnsureFolder(PATTERN_PATH);
            EnsureFolder(EVENT_PATH);
            EnsureFolder(AUGMENT_PATH);
            EnsureFolder(SPAWN_PATTERN_PATH);

            GenerateSkillData();
            GenerateCharacterData();
            GenerateEnemyPatternData();
            GenerateEventData();
            GenerateRelicData();
            GenerateAugmentData();
            GenerateSpawnPatternTables();
            GenerateUIPalette();
            GenerateAudioPalette();
            GenerateVFXPalette();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DataGenerator] CSV → SO 데이터 생성 완료!");
        }

        #region Skill Data

        private static void GenerateSkillData()
        {
            var csv = new CsvParser($"{TABLE_PATH}/SkillTable.csv");
            if (csv.RowCount == 0)
            {
                Debug.LogWarning("[DataGenerator] SkillTable.csv 가 비어있습니다.");
                return;
            }

            for (int i = 0; i < csv.RowCount; i++)
            {
                string id = csv.Get(i, "id");
                string displayName = csv.Get(i, "displayName");
                string desc = csv.Get(i, "description");
                var type = ParseEnum<SkillType>(csv.Get(i, "type"));
                var target = ParseEnum<TargetType>(csv.Get(i, "target"));
                int power = csv.GetInt(i, "power");
                int cost = csv.GetInt(i, "cost");
                int weight = csv.GetInt(i, "weight");
                var effect = ParseEnum<StatusEffectType>(csv.Get(i, "statusEffect"));
                int duration = csv.GetInt(i, "effectDuration");
                int effectValue = csv.GetInt(i, "effectValue");

                CreateSkill(id, displayName, desc, type, target, power, weight, cost, effect, duration, effectValue);
            }
        }

        private static void CreateSkill(string fileName, string name, string desc,
            SkillType type, TargetType target, int power, int weight, int cost,
            StatusEffectType effect, int duration, int effectValue)
        {
            var path = $"{SKILL_PATH}/{fileName}.asset";
            var skill = GetOrCreateAsset<SkillData>(path);
            skill.name = fileName;

            SetPrivateField(skill, "_skillName", name);
            SetPrivateField(skill, "_description", desc);
            SetPrivateField(skill, "_skillType", type);
            SetPrivateField(skill, "_targetType", target);
            SetPrivateField(skill, "_power", power);
            SetPrivateField(skill, "_cost", cost);
            SetPrivateField(skill, "_weight", weight);
            SetPrivateField(skill, "_statusEffect", effect);
            SetPrivateField(skill, "_effectDuration", duration);
            SetPrivateField(skill, "_effectValue", effectValue);

            // 아이콘 자동 할당
            string iconPath = GetSkillIconPath(type, effect, target);
            var icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (icon != null)
                SetPrivateField(skill, "_icon", icon);
            else
                Debug.LogWarning($"[DataGenerator] 스킬 아이콘 없음: {iconPath} ({fileName})");

            EditorUtility.SetDirty(skill);
        }

        #endregion

        #region Character Data

        private static void GenerateCharacterData()
        {
            var csv = new CsvParser($"{TABLE_PATH}/CharacterTable.csv");
            if (csv.RowCount == 0)
            {
                Debug.LogWarning("[DataGenerator] CharacterTable.csv 가 비어있습니다.");
                return;
            }

            for (int i = 0; i < csv.RowCount; i++)
            {
                string id = csv.Get(i, "id");
                string displayName = csv.Get(i, "displayName");
                var charClass = ParseEnum<CharacterClass>(csv.Get(i, "class"));
                string desc = csv.Get(i, "description");
                int hp = csv.GetInt(i, "hp");
                int atk = csv.GetInt(i, "atk");
                int def = csv.GetInt(i, "def");
                string skillsRaw = csv.Get(i, "skills");
                string[] skills = string.IsNullOrEmpty(skillsRaw) ? new string[0] : skillsRaw.Split(';');
                var trait = ParseEnum<EnemyTrait>(csv.Get(i, "trait"));
                bool isDefault = csv.GetInt(i, "isDefault") == 1;
                string unlockCondition = csv.Get(i, "unlockCondition");

                CreateCharacter(id, displayName, charClass, desc, hp, atk, def, skills, trait, isDefault, unlockCondition);
            }
        }

        private static void CreateCharacter(string fileName, string name, CharacterClass charClass,
            string desc, int hp, int atk, int def, string[] skills, EnemyTrait trait = EnemyTrait.None,
            bool isDefault = true, string unlockCondition = "")
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

            var skillList = new List<SkillData>();
            foreach (var skillName in skills)
            {
                var skill = AssetDatabase.LoadAssetAtPath<SkillData>($"{SKILL_PATH}/{skillName}.asset");
                if (skill != null)
                    skillList.Add(skill);
            }
            SetPrivateField(character, "_skills", skillList);

            EditorUtility.SetDirty(character);
        }

        #endregion

        #region Enemy Pattern Data

        private static void GenerateEnemyPatternData()
        {
            var csv = new CsvParser($"{TABLE_PATH}/EnemyPatternTable.csv");
            if (csv.RowCount == 0)
            {
                Debug.LogWarning("[DataGenerator] EnemyPatternTable.csv 가 비어있습니다.");
                return;
            }

            // enemyId별로 스킬 ID 순서 그룹핑
            var grouped = new Dictionary<string, List<(int order, string skillId)>>();

            for (int i = 0; i < csv.RowCount; i++)
            {
                string enemyId = csv.Get(i, "enemyId");
                int order = csv.GetInt(i, "order");
                string skillId = csv.Get(i, "skillId");

                if (!grouped.ContainsKey(enemyId))
                    grouped[enemyId] = new List<(int, string)>();

                grouped[enemyId].Add((order, skillId));
            }

            foreach (var kv in grouped)
            {
                string enemyId = kv.Key;
                var entries = kv.Value.OrderBy(e => e.order).ToList();

                var path = $"{PATTERN_PATH}/Pattern_{enemyId}.asset";
                var patternData = GetOrCreateAsset<EnemyPatternData>(path);
                patternData.name = $"Pattern_{enemyId}";

                SetPrivateField(patternData, "_enemyId", enemyId);

                var skillList = new List<SkillData>();
                foreach (var entry in entries)
                {
                    var skill = AssetDatabase.LoadAssetAtPath<SkillData>($"{SKILL_PATH}/{entry.skillId}.asset");
                    if (skill != null)
                        skillList.Add(skill);
                    else
                        Debug.LogWarning($"[DataGenerator] 패턴 스킬을 찾을 수 없음: {entry.skillId}");
                }
                SetPrivateField(patternData, "_skills", skillList);

                EditorUtility.SetDirty(patternData);
            }
        }

        #endregion

        /// <summary>
        /// SkillType + StatusEffectType + TargetType 조합으로 아이콘 경로 반환
        /// </summary>
        private static string GetSkillIconPath(SkillType type, StatusEffectType effect, TargetType target)
        {
            // AoE (전체 대상) 공격은 폭발 아이콘
            bool isAoE = target == TargetType.AllEnemies;

            if (type == SkillType.Attack)
            {
                if (effect == StatusEffectType.Burn)   return $"{PICTO_BASE}/Pictoicon_Fire.Png";
                if (effect == StatusEffectType.Poison) return $"{PICTO_BASE}/Pictoicon_Posion.Png";
                if (effect == StatusEffectType.Freeze) return $"{PICTO_BASE}/Pictoicon_Water.Png";
                if (effect == StatusEffectType.Bleed)  return $"{PICTO_BASE}/Pictoicon_Skull.Png";
                if (isAoE)                             return $"{PICTO_BASE}/Pictoicon_Boom.Png";
                return $"{ITEM_BASE}/Icon_Sword.png";
            }

            if (type == SkillType.Heal)    return $"{ITEM_BASE}/Icon_Heart.png";
            if (type == SkillType.Shield)  return $"{ITEM_BASE}/Icon_Shield.png";
            if (type == SkillType.Purify)  return $"{PICTO_BASE}/Pictoicon_Angel.Png";

            if (type == SkillType.Buff)
            {
                if (effect == StatusEffectType.AttackUp)  return $"{PICTO_BASE}/Pictoicon_Buff.Png";
                if (effect == StatusEffectType.DefenseUp) return $"{PICTO_BASE}/Pictoicon_Defense.Png";
                if (effect == StatusEffectType.Taunt)     return $"{PICTO_BASE}/Pictoicon_Shield.Png";
                return $"{PICTO_BASE}/Pictoicon_Magic.Png";
            }

            if (type == SkillType.Debuff)
            {
                if (effect == StatusEffectType.Poison)      return $"{PICTO_BASE}/Pictoicon_Posion.Png";
                if (effect == StatusEffectType.DefenseDown) return $"{PICTO_BASE}/Pictoicon_Defense_Weak.Png";
                if (effect == StatusEffectType.AttackDown)  return $"{RUNE_BASE}/RuneIcon0_Debuff.png";
                return $"{PICTO_BASE}/Pictoicon_Attack.Png";
            }

            return $"{ITEM_BASE}/Icon_Sword.png";
        }

        #region Utilities

        private static T ParseEnum<T>(string value) where T : struct
        {
            if (System.Enum.TryParse<T>(value, ignoreCase: true, out var result))
                return result;
            return default;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        private static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
                return existing;

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(current + "/" + parts[i]))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current += "/" + parts[i];
            }
        }

        #endregion
    }
}
#endif
