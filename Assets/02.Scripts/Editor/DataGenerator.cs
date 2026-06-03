#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using TeamLog.Characters;
using TeamLog.Combat.AI;
using TeamLog.Event;
using TeamLog.Reward;

namespace TeamLog.Editor
{
    /// <summary>
    /// CSV 테이블 → ScriptableObject 에셋 자동 생성 에디터
    /// </summary>
    public static class DataGenerator
    {
        private const string TABLE_PATH = "Assets/03.Data/Tables";
        private const string SKILL_PATH = "Assets/03.Data/Skills";
        private const string CHAR_PATH = "Assets/03.Data/Characters";
        private const string PATTERN_PATH = "Assets/03.Data/Patterns";
        private const string ITEM_PATH = "Assets/03.Data/Items";
        private const string EVENT_PATH = "Assets/03.Data/Events";

        [MenuItem("TeamLog/Generate Test Data", false, 100)]
        public static void GenerateAllTestData()
        {
            EnsureFolder(SKILL_PATH);
            EnsureFolder(CHAR_PATH);
            EnsureFolder(PATTERN_PATH);
            EnsureFolder(ITEM_PATH);
            EnsureFolder(EVENT_PATH);

            GenerateSkillData();
            GenerateCharacterData();
            GenerateEnemyPatternData();
            GenerateItemData();
            GenerateEventData();

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

                CreateCharacter(id, displayName, charClass, desc, hp, atk, def, skills, trait);
            }
        }

        private static void CreateCharacter(string fileName, string name, CharacterClass charClass,
            string desc, int hp, int atk, int def, string[] skills, EnemyTrait trait = EnemyTrait.None)
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

        #region Item Data

        private static void GenerateItemData()
        {
            CreateItem("Item_HPBoost", "생명력의 결정", "최대 HP가 20 증가합니다.",
                ItemType.PassiveBuff, ItemEffectType.MaxHPUp, 20, price: 80, RewardRarity.Common);

            CreateItem("Item_ATKBoost", "무기 강화석", "공격력이 3 증가합니다.",
                ItemType.PassiveBuff, ItemEffectType.ATKUp, 3, price: 100, RewardRarity.Rare);

            CreateItem("Item_DEFBoost", "단단한 껍질", "방어력이 3 증가합니다.",
                ItemType.PassiveBuff, ItemEffectType.DEFUp, 3, price: 90, RewardRarity.Common);

            CreateItem("Item_HealingHerb", "치유의 허브", "매 턴 5 HP를 회복합니다.",
                ItemType.Consumable, ItemEffectType.HealPerTurn, 5, price: 60, RewardRarity.Common);

            CreateItem("Item_LuckyCoin", "행운의 주화", "전투 후 추가 골드를 획득합니다.",
                ItemType.Relic, ItemEffectType.ExtraGold, 15, price: 120, RewardRarity.Rare);

            CreateItem("Item_DragonHeart", "드래곤의 심장", "최대 HP가 50 증가합니다.",
                ItemType.Relic, ItemEffectType.MaxHPUp, 50, price: 200, RewardRarity.Unique);
        }

        private static void CreateItem(string fileName, string name, string desc,
            ItemType type, ItemEffectType effectType, int effectValue, int price, RewardRarity rarity)
        {
            var path = $"{ITEM_PATH}/{fileName}.asset";
            var item = GetOrCreateAsset<ItemData>(path);
            item.name = fileName;

            SetPrivateField(item, "_itemName", name);
            SetPrivateField(item, "_description", desc);
            SetPrivateField(item, "_itemType", type);
            SetPrivateField(item, "_effectType", effectType);
            SetPrivateField(item, "_effectValue", effectValue);
            SetPrivateField(item, "_price", price);
            SetPrivateField(item, "_rarity", rarity);

            EditorUtility.SetDirty(item);
        }

        #endregion

        #region Event Data

        private static void GenerateEventData()
        {
            CreateEvent("Event_AbandonedChest", "버려진 상자", "길가에 낡은 상자가 놓여 있습니다. 조심스럽게 열어볼까요?",
                TeamLog.Event.EventType.Treasure,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "조심스럽게 연다",
                        ChoiceDescription = "천천히 상자를 엽니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "상자 안에 금화가 들어 있었습니다! 40 골드를 획득했습니다.",
                            GoldChange = 40, HPPercentChange = 0
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "무작정 연다",
                        ChoiceDescription = "빠르게 상자를 엽니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "함정이 발동했습니다! 약간의 데미지를 받았지만 골드를 얻었습니다.",
                            GoldChange = 25, HPPercentChange = -15
                        }
                    }
                });

            CreateEvent("Event_MysteriousShrine", "신비한 신전", "오래된 신전이 숲 속에 서 있습니다. 기분 좋은 빛이 새어나옵니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "기원한다",
                        ChoiceDescription = "신전에 기도를 올립니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "따뜻한 빛이 파티를 감쌉니다. 모든 파티원의 HP가 30% 회복되었습니다!",
                            GoldChange = 0, HPPercentChange = 30
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "제물을 바친다",
                        ChoiceDescription = "골드를 제물로 바칩니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "신전이 빛나며 반응합니다. 파티원의 HP가 50% 회복되었습니다!",
                            GoldChange = -20, HPPercentChange = 50
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "무시하고 지나간다",
                        ChoiceDescription = "그냥 지나갑니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "아무 일도 일어나지 않았습니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });

            CreateEvent("Event_WoundedTraveler", "부상당한 여행자", "길에서 다친 여행자를 만났습니다. 도와줄까요?",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "치유해 준다",
                        ChoiceDescription = "파티의 힐러가 치유합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "여행자가 고마워하며 보상으로 30 골드를 주었습니다!",
                            GoldChange = 30, HPPercentChange = -5
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "무기와 방어구를 나눠준다",
                        ChoiceDescription = "여분의 장비를 줍니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "여행자가 감동하여 귀중한 아이템을 건넵니다!",
                            GoldChange = 0, HPPercentChange = 0, GiveRandomItem = true
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "무시하고 지나간다",
                        ChoiceDescription = "바쁘니 그냥 갑니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "여행자가 실망한 표정으로 뒤를 돌아봅니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });

            CreateEvent("Event_SpiderWeb", "거미줄 함정", "거대한 거미줄이 길을 막고 있습니다. 어떻게 할까요?",
                TeamLog.Event.EventType.Trap,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "불태운다",
                        ChoiceDescription = "횃불로 거미줄을 태웁니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "거미줄이 타면서 숨겨진 보물이 드러났습니다! 35 골드를 획득했습니다.",
                            GoldChange = 35, HPPercentChange = 0
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "돌아간다",
                        ChoiceDescription = "안전하게 우회합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "우회하느라 시간이 걸렸지만 아무 일도 없었습니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });

            CreateEvent("Event_AncientLibrary", "고대 도서관", "오래된 도서관에서 반짝이는 책을 발견했습니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "책을 읽는다",
                        ChoiceDescription = "고대의 지식을 얻습니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "고대의 지식을 얻었습니다! 모든 파티원의 공격력이 영구히 증가했습니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "책을 판다",
                        ChoiceDescription = "골드로 바꿉니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "귀중한 책을 50 골드에 판매했습니다!",
                            GoldChange = 50, HPPercentChange = 0
                        }
                    }
                });

            CreateEvent("Event_FairySpring", "요정의 샘", "신비로운 빛이 나는 샘물이 있습니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "마신다",
                        ChoiceDescription = "샘물을 마십니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "따뜻한 기운이 온몸을 감쌉니다! 파티원 전원의 HP가 40% 회복되었습니다.",
                            GoldChange = 0, HPPercentChange = 40
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "동전을 던진다",
                        ChoiceDescription = "소원을 빕니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "요정이 나타나 감사 인사를 합니다. 30 골드를 선물로 받았습니다!",
                            GoldChange = 30, HPPercentChange = 0
                        }
                    }
                });

            CreateEvent("Event_FallenKnight", "쓰러진 기사", "부상당한 기사가 도움을 요청합니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "치유해 준다",
                        ChoiceDescription = "기사를 치유합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "기사가 감사하며 방어 기술을 알려주었습니다! 방어력이 영구히 증가했습니다.",
                            GoldChange = 0, HPPercentChange = -5
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "무시한다",
                        ChoiceDescription = "바쁘니 지나갑니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "기사가 실망한 표정으로 뒤를 돌아봅니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });

            CreateEvent("Event_TreasureGoblin", "보물 고블린", "보물 가방을 들고 도망치는 고블린을 발견했습니다!",
                TeamLog.Event.EventType.Treasure,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "쫓아간다",
                        ChoiceDescription = "고블린을 쫓아갑니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "고블린을 잡아 보물을 탈환했습니다! 60 골드를 획득했습니다!",
                            GoldChange = 60, HPPercentChange = -10
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "함정이다",
                        ChoiceDescription = "의심스러우니 무시합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "신중한 판단이었습니다. 안전하게 지나갑니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });

            CreateEvent("Event_CursedAltar", "저주받은 제단", "어둠의 기운이 감도는 제단이 있습니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "기도한다",
                        ChoiceDescription = "제단에 기도를 올립니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "저주가 풀리며 강력한 힘이 주입되었습니다! HP가 약간 깎였지만 공격력이 영구히 증가했습니다.",
                            GoldChange = 0, HPPercentChange = -20
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "파괴한다",
                        ChoiceDescription = "제단을 부숩니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "제단이 무너지며 숨겨진 골드가 나왔습니다! 45 골드를 획득했습니다.",
                            GoldChange = 45, HPPercentChange = 0
                        }
                    }
                });

            CreateEvent("Event_TravelingMerchant", "상인 대행", "여행 중인 상인이 특별한 거래를 제안합니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "회복약을 산다",
                        ChoiceDescription = "20 골드를 지불합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "회복약을 마셨습니다! 파티원 전원의 HP가 25% 회복되었습니다.",
                            GoldChange = -20, HPPercentChange = 25
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "정보를 산다",
                        ChoiceDescription = "15 골드를 지불합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "유용한 정보를 얻었습니다! 앞으로의 전투에서 유리할 것입니다.",
                            GoldChange = -15, HPPercentChange = 0
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "거절한다",
                        ChoiceDescription = "골드를 아낍니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "상인이 아쉬운 표정으로 떠납니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });
        }

        private static void CreateEvent(string fileName, string name, string desc,
            TeamLog.Event.EventType type, EventChoice[] choices)
        {
            var path = $"{EVENT_PATH}/{fileName}.asset";
            var eventData = GetOrCreateAsset<EventData>(path);
            eventData.name = fileName;

            SetPrivateField(eventData, "_eventName", name);
            SetPrivateField(eventData, "_description", desc);
            SetPrivateField(eventData, "_eventType", type);
            SetPrivateField(eventData, "_choices", new List<EventChoice>(choices));

            EditorUtility.SetDirty(eventData);
        }

        #endregion

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
