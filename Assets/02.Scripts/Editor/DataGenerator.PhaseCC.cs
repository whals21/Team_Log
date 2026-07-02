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
            // ════════════════════════════════════════════
            // Ashe (Pyromancer) — Ember 자해 폭딜 스킬 4종
            // ════════════════════════════════════════════
            // 1. Cinder Accretion — 셋업: 단일 5 + Burn, Ember +2
            CreatePhaseCCSkill("Ashe_CinderAccretion", "잿빛 응축", SkillType.Attack, TargetType.SingleEnemy,
                power: 5, cost: 1, statusEffect: StatusEffectType.Burn, effectDuration: 2, effectValue: 3,
                gainType: ResourceType.Ember, gainAmount: 2);

            // 2. Phoenix Renewal — 아군 힐 8 + Ember×3 추가 힐, Ember +1
            // 강화 조건: 대상 HP 50%- 시 Burn/Poison 정화 추가 (CleanseLowTarget).
            // ★ 통합 파이프라인 검증: Heal 타입이 PostApply Phase 거쳐서 정화 자동 작동.
            CreatePhaseCCSkill("Ashe_PhoenixRenewal", "불사조 갱생", SkillType.Heal, TargetType.SingleAlly,
                power: 8, cost: 1,
                gainType: ResourceType.Ember, gainAmount: 1,
                resourcePowerPerStack: 3, // Ember×3 추가 힐
                behaviors: new BehaviorTag(BehaviorKeyword.CleanseLowTarget, 0));

            // 3. Brand of Ash — 단일 8 + Ember×3 데미지, Ember -2 소모
            // 강화 조건: 자신 HP 50%- 시 데미지 2배 (Berserk). Ashe는 자해 메카닉이라 임계 도달 잦음.
            CreatePhaseCCSkill("Ashe_BrandOfAsh", "잿더미 낙인", SkillType.Attack, TargetType.SingleEnemy,
                power: 8, cost: 2,
                costType: ResourceType.Ember, costAmount: 2,
                resourcePowerPerStack: 3, // Ember×3 추가 위력
                behaviors: new BehaviorTag(BehaviorKeyword.Berserk, 0)); // 자신 HP 50%- 시 2배

            // 4. Embrace of Cinders — 단일 30 + Ember×15, Ember 5 소모 (궁극기)
            CreatePhaseCCSkill("Ashe_EmbraceOfCinders", "잔불의 포옹", SkillType.Attack, TargetType.SingleEnemy,
                power: 30, cost: 3,
                costType: ResourceType.Ember, costAmount: 5,
                resourcePowerPerStack: 15); // Ember×15 추가 위력 — 풀충전 시 30+75=105

            // ════════════════════════════════════════════
            // Duran (Warrior) — Vengeance 복수 게이지 스킬 4종
            // ════════════════════════════════════════════
            // 1. Shield Wall — 아군 쉴드 10, AP 1 (셋업)
            // 강화 조건: Vengeance 5+ 시 썰드 +5 (ResourceThresholdShield).
            // ★ 통합 파이프라인 검증: Shield 타입이 ApplyMain Phase 거쳐서 임계값 가산 자동 작동.
            CreatePhaseCCSkill("Duran_ShieldWall", "방패벽", SkillType.Shield, TargetType.SingleAlly,
                power: 10, cost: 1,
                behaviors: new BehaviorTag(BehaviorKeyword.ResourceThresholdShield, 5));

            // 2. Provoking Shield — 자신 도발 부여, AP 1 (적이 Duran을 우선 공격)
            CreatePhaseCCSkill("Duran_ProvokingShield", "도발 방패", SkillType.Buff, TargetType.Self,
                power: 6, cost: 1, statusEffect: StatusEffectType.Taunt, effectDuration: 1, effectValue: 1);

            // 3. Revenge Strike — 단일 10 + Vengeance×1, Vengeance 전량 소모
            CreatePhaseCCSkill("Duran_RevengeStrike", "복수의 일격", SkillType.Attack, TargetType.SingleEnemy,
                power: 10, cost: 2,
                costType: ResourceType.Vengeance, costAmount: 0,
                resourcePowerPerStack: 1, consumeAllResource: true); // Vengeance 전량 소모

            // 4. Last Bastion — 자신 HP 25 회복 + 쉴드 25, Vengeance 15 소모 (궁극기)
            CreatePhaseCCSkill("Duran_LastBastion", "최후의 보루", SkillType.Shield, TargetType.Self,
                power: 25, cost: 3,
                costType: ResourceType.Vengeance, costAmount: 15);

            // ════════════════════════════════════════════
            // Lumi (Cryomancer) — Frost 통제 스킬 4종
            // ════════════════════════════════════════════
            // 1. Frostbolt — 단일 5 + Freeze 1, Frost +1, AP 1
            // 강화 조건: 대상 이미 Freeze 상태 시 +3 위력 (총 8). Lumi 콤보: Frost Armor/Blizzard로 Freeze 건 뒤 사용.
            CreatePhaseCCSkill("Lumi_Frostbolt", "서리 화살", SkillType.Attack, TargetType.SingleEnemy,
                power: 5, cost: 1, statusEffect: StatusEffectType.Freeze, effectDuration: 1, effectValue: 1,
                gainType: ResourceType.Frost, gainAmount: 1,
                behaviors: new BehaviorTag(BehaviorKeyword.TargetFreeze, 3)); // Freeze 적 +3

            // 2. Frost Armor — 아군 쉴드 10, Frost +1, AP 1
            CreatePhaseCCSkill("Lumi_FrostArmor", "서리 갑옷", SkillType.Shield, TargetType.SingleAlly,
                power: 10, cost: 1,
                gainType: ResourceType.Frost, gainAmount: 1);

            // 3. Blizzard — 광역 4 + Freeze 1, Frost +1, AP 2
            CreatePhaseCCSkill("Lumi_Blizzard", "눈보라", SkillType.Attack, TargetType.AllEnemies,
                power: 4, cost: 2, statusEffect: StatusEffectType.Freeze, effectDuration: 1, effectValue: 1,
                gainType: ResourceType.Frost, gainAmount: 1);

            // 4. Glacial Spike — 단일 12, Frost 3 소모 (폭딜)
            CreatePhaseCCSkill("Lumi_GlacialSpike", "빙하 창", SkillType.Attack, TargetType.SingleEnemy,
                power: 12, cost: 2,
                costType: ResourceType.Frost, costAmount: 3);

            // ── Taranis (Stormcaller) Charge Network 연동 스킬 ──
            // Charge 상태이상(value=스택수, duration=3 — 자연 소멄은 value 기반으로 매 턴 -1, StatusEffectComponent에서 duration 소멸 스킵).
            // 매 턴 종료 시 다른 Charge 적에게 자신의 스택 수만큼 도트 데미지 (TurnManager.ProcessTurnEnd).
            CreatePhaseCCSkill("Taranis_Wire", "와이어", SkillType.Attack, TargetType.SingleEnemy,
                power: 3, cost: 1, statusEffect: StatusEffectType.Charge, effectDuration: 3, effectValue: 2,
                behaviors: new BehaviorTag(BehaviorKeyword.Propagate, 1)); // Propagate=전파 (메인 타겟 + 다른 적 1명 추가 Charge 1스택)

            CreatePhaseCCSkill("Taranis_Branch", "브랜치", SkillType.Attack, TargetType.AllEnemies,
                power: 2, cost: 2, statusEffect: StatusEffectType.Charge, effectDuration: 3, effectValue: 1);

            CreatePhaseCCSkill("Taranis_GroundingField", "접지 장벽", SkillType.Shield, TargetType.AllAllies,
                power: 10, cost: 2, shieldFlags: ShieldFlag.GivesChargeOnAbsorb);

            CreatePhaseCCSkill("Taranis_Thunderstorm", "뇌우", SkillType.Attack, TargetType.AllEnemies,
                power: 10, cost: 3, statusEffect: StatusEffectType.Charge, effectDuration: 3, effectValue: 3);

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
                70, 0, 0, new[] { "Ashe_CinderAccretion", "Ashe_BrandOfAsh", "Ashe_PhoenixRenewal", "Ashe_EmbraceOfCinders" },
                EnemyTrait.None, true, "", ResourceType.Ember);

            // Duran — Warrior (Vengeance 복수 게이지).
            CreatePhaseCCCharacter("Char_Duran", "듀란", CharacterClass.Warrior,
                "불멸의 성벽. 피격 시 Vengeance가 축적되며 소비 스킬로 버스트 딜",
                120, 0, 0, new[] { "Duran_RevengeStrike", "Duran_ShieldWall", "Duran_ProvokingShield", "Duran_LastBastion" },
                EnemyTrait.None, true, "", ResourceType.Vengeance);

            // Lumi — Cryomancer (Frost 통제).
            CreatePhaseCCCharacter("Char_Lumi", "루미", CharacterClass.Cryomancer,
                "냉기 마법사. Frost를 축적하여 적을 얼린다",
                75, 0, 0, new[] { "Lumi_Frostbolt", "Lumi_GlacialSpike", "Lumi_FrostArmor", "Lumi_Blizzard" },
                EnemyTrait.None, true, "", ResourceType.Frost);

            // Sibyl — Oracle (Prophecy 지연 발동 — 스킬이 1턴 뒤 발동)
            CreatePhaseCCCharacter("Char_Sibyl", "시빌", CharacterClass.Oracle,
                "예언자. 미래에 투자하는 서포터 — 스킬이 1턴 뒤 발동",
                80, 0, 0, new[] { "Sibyl_DeathProphecy", "Sibyl_VisionOfRenewal", "Sibyl_BorrowedFuture", "Sibyl_DéjàVu" },
                EnemyTrait.None, true, "", ResourceType.Prophecy);

            // Taranis — Stormcaller (Charge Network)
            CreatePhaseCCCharacter("Char_Taranis", "타라니스", CharacterClass.Stormcaller,
                "폭풍 소환사. 적에게 전하를 부여하여 매 턴 연쇄 도트 데미지",
                85, 0, 0, new[] { "Taranis_Wire", "Taranis_Branch", "Taranis_GroundingField", "Taranis_Thunderstorm" },
                EnemyTrait.None, true, "", ResourceType.None);

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

        /// <summary>Phase CC 스킬 생성 헬퍼 — 자원 획득/소모/비례 위력 + 쉴드 속성 포함.</summary>
        private static void CreatePhaseCCSkill(string fileName, string name, SkillType type, TargetType target,
            int power, int cost, StatusEffectType statusEffect = StatusEffectType.None,
            int effectDuration = 0, int effectValue = 0,
            ResourceType gainType = ResourceType.None, int gainAmount = 0,
            ResourceType costType = ResourceType.None, int costAmount = 0,
            int resourcePowerPerStack = 0, bool consumeAllResource = false,
            ShieldFlag shieldFlags = ShieldFlag.None,
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
            SetPrivateField(skill, "_resourcePowerPerStack", resourcePowerPerStack);
            SetPrivateField(skill, "_consumeAllResource", consumeAllResource);
            SetPrivateField(skill, "_shieldFlags", shieldFlags);
            SetPrivateField(skill, "_behaviors", behaviors ?? new BehaviorTag[0]);

            EditorUtility.SetDirty(skill);
        }
    }
}
#endif
