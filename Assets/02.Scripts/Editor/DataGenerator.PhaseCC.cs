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
            // ── 자원 연동 스킬 먼저 생성 (Ashe/Duran 전용) ──
            CreatePhaseCCSkill("Ashe_CinderAccretion", "잿빛 응축", SkillType.Attack, TargetType.SingleEnemy,
                power: 5, cost: 1, statusEffect: StatusEffectType.Burn, effectDuration: 2, effectValue: 3,
                gainType: ResourceType.Ember, gainAmount: 2); // Ember +2 획득 (충전)

            CreatePhaseCCSkill("Ashe_BrandOfAsh", "잿더미 낙인", SkillType.Attack, TargetType.SingleEnemy,
                power: 20, cost: 2,
                costType: ResourceType.Ember, costAmount: 2); // Ember 2 소모 (폭딜)

            CreatePhaseCCSkill("Duran_RevengeStrike", "복수의 일격", SkillType.Attack, TargetType.SingleEnemy,
                power: 18, cost: 2,
                costType: ResourceType.Vengeance, costAmount: 5); // Vengeance 5 소모 (폭딜)

            // ── Lumi (Cryomancer) Frost 연동 스킬 ──
            CreatePhaseCCSkill("Lumi_Frostbolt", "서리 화살", SkillType.Attack, TargetType.SingleEnemy,
                power: 5, cost: 1, statusEffect: StatusEffectType.Freeze, effectDuration: 1, effectValue: 1,
                gainType: ResourceType.Frost, gainAmount: 1); // Frost +1 (충전)

            CreatePhaseCCSkill("Lumi_GlacialSpike", "빙하 창", SkillType.Attack, TargetType.SingleEnemy,
                power: 16, cost: 2,
                costType: ResourceType.Frost, costAmount: 3); // Frost 3 소모 (폭딜, 최대치 필요)

            // ── Taranis (Stormcaller) Charge Network 연동 스킬 ──
            // Charge 상태이상(value=스택수, duration=2턴)을 적에게 부여.
            // 매 턴 종료 시 Charge 상태 적에게 value만큼 도트 데미지 (TurnManager.ProcessTurnEnd).
            CreatePhaseCCSkill("Taranis_Wire", "와이어", SkillType.Attack, TargetType.SingleEnemy,
                power: 3, cost: 1, statusEffect: StatusEffectType.Charge, effectDuration: 2, effectValue: 2);

            CreatePhaseCCSkill("Taranis_Branch", "브랜치", SkillType.Attack, TargetType.AllEnemies,
                power: 2, cost: 2, statusEffect: StatusEffectType.Charge, effectDuration: 2, effectValue: 1);

            CreatePhaseCCSkill("Taranis_GroundingField", "접지 장벽", SkillType.Shield, TargetType.AllAllies,
                power: 10, cost: 2);

            CreatePhaseCCSkill("Taranis_Thunderstorm", "뇌우", SkillType.Attack, TargetType.AllEnemies,
                power: 10, cost: 3, statusEffect: StatusEffectType.Charge, effectDuration: 2, effectValue: 3);

            // ── Sibyl (Oracle) Prophecy 연동 스킬 (간소화 — 일반 힐/딜로 처리, Prophecy 메카닉은 추후) ──
            CreatePhaseCCSkill("Sibyl_DeathProphecy", "죽음의 예언", SkillType.Attack, TargetType.SingleEnemy,
                power: 14, cost: 1); // 일반 딜 (정식 Prophecy 1턴 뒤 발동은 추후)

            CreatePhaseCCSkill("Sibyl_VisionOfRenewal", "갱생의 환영", SkillType.Heal, TargetType.SingleAlly,
                power: 12, cost: 1);

            CreatePhaseCCSkill("Sibyl_BorrowedFuture", "미래 차용", SkillType.Buff, TargetType.Self,
                power: 0, cost: 1, statusEffect: StatusEffectType.AttackUp, effectDuration: 2, effectValue: 3);

            CreatePhaseCCSkill("Sibyl_DéjàVu", "데자부", SkillType.Attack, TargetType.SingleEnemy,
                power: 10, cost: 1);

            // Ashe — Pyromancer (Ember 자해 폭딜).
            // Cinder Accretion(충전) + Brand of Ash(폭딜) + 기존 Mage 스킬 2종
            CreatePhaseCCCharacter("Char_Ashe", "아셰", CharacterClass.Pyromancer,
                "화염 마법사. Ember 자원을 축적하여 자해 위험을 감수하고 폭딜을 낸다",
                70, 0, 0, new[] { "Ashe_CinderAccretion", "Ashe_BrandOfAsh", "Mage_MagicShield", "Mage_Meteor" },
                EnemyTrait.ArcaneFury, true, "", ResourceType.Ember);

            // Duran — Warrior (Vengeance 복수 게이지).
            // Revenge Strike(소비 폭딜) + 기존 Warrior 스킬 3종
            CreatePhaseCCCharacter("Char_Duran", "듀란", CharacterClass.Warrior,
                "불멸의 성벽. 피격 시 Vengeance가 축적되며 소비 스킬로 버스트 딜",
                120, 0, 0, new[] { "Duran_RevengeStrike", "Warrior_Shield", "Warrior_Taunt", "Warrior_Strike" },
                EnemyTrait.Sturdy, true, "", ResourceType.Vengeance);

            // Lumi — Cryomancer (Frost 통제).
            // Frostbolt(충전) + GlacialSpike(폭딜) + 기존 Mage 스킬 2종
            CreatePhaseCCCharacter("Char_Lumi", "루미", CharacterClass.Cryomancer,
                "냉기 마법사. Frost를 축적하여 적을 얼린다",
                75, 0, 0, new[] { "Lumi_Frostbolt", "Lumi_GlacialSpike", "Mage_MagicShield", "Mage_IceSpear" },
                EnemyTrait.ArcaneFury, true, "", ResourceType.Frost);

            // Sibyl — Oracle (Prophecy 지연 발동 — 간소화 버전, 정식 1턴 뒤 발동은 추후)
            CreatePhaseCCCharacter("Char_Sibyl", "시빌", CharacterClass.Oracle,
                "예언자. 미래에 투자하는 서포터 (정식 Prophecy 1턴 뒤 발동은 추후 구현)",
                80, 0, 0, new[] { "Sibyl_DeathProphecy", "Sibyl_VisionOfRenewal", "Sibyl_BorrowedFuture", "Sibyl_DéjàVu" },
                EnemyTrait.Regenerate, true, "", ResourceType.None);

            // Taranis — Stormcaller (Charge Network — 적에게 Charge 상태이상 부여, 매 턴 연쇄 도트)
            CreatePhaseCCCharacter("Char_Taranis", "타라니스", CharacterClass.Stormcaller,
                "폭풍 소환사. 적에게 전하를 부여하여 매 턴 연쇄 도트 데미지",
                85, 0, 0, new[] { "Taranis_Wire", "Taranis_Branch", "Taranis_GroundingField", "Taranis_Thunderstorm" },
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

        /// <summary>Phase CC 스킬 생성 헬퍼 — 자원 획득/소모 포함.</summary>
        private static void CreatePhaseCCSkill(string fileName, string name, SkillType type, TargetType target,
            int power, int cost, StatusEffectType statusEffect = StatusEffectType.None,
            int effectDuration = 0, int effectValue = 0,
            ResourceType gainType = ResourceType.None, int gainAmount = 0,
            ResourceType costType = ResourceType.None, int costAmount = 0,
            params BehaviorTag[] behaviors)
        {
            var path = $"{SKILL_PATH}/{fileName}.asset";
            var skill = GetOrCreateAsset<SkillData>(path);
            skill.name = fileName;

            SetPrivateField(skill, "_skillName", name);
            SetPrivateField(skill, "_skillType", type);
            SetPrivateField(skill, "_targetType", target);
            SetPrivateField(skill, "_power", power);
            SetPrivateField(skill, "_cost", cost);
            SetPrivateField(skill, "_statusEffect", statusEffect);
            SetPrivateField(skill, "_effectDuration", effectDuration);
            SetPrivateField(skill, "_effectValue", effectValue);
            SetPrivateField(skill, "_resourceGainType", gainType);
            SetPrivateField(skill, "_resourceGainAmount", gainAmount);
            SetPrivateField(skill, "_resourceCostType", costType);
            SetPrivateField(skill, "_resourceCostAmount", costAmount);
            SetPrivateField(skill, "_behaviors", behaviors ?? new BehaviorTag[0]);

            EditorUtility.SetDirty(skill);
        }
    }
}
#endif
