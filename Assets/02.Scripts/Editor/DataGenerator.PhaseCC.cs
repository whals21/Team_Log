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
            // Ashe (Pyromancer) — Ember 자해 폭딜 스킬 4종 (Phase CC-2G-1 리워크)
            // ════════════════════════════════════════════
            // 1. Cinder Accretion — 셋업: 단일 2 + Burn, Ember +2.
            // Phase CC-2G-1: TargetFullHP(rank=3) 추가 — 풀피 적에게 2+3=5 위력 셋업 강화.
            CreatePhaseCCSkill("Ashe_CinderAccretion", "잿빛 응축", SkillType.Attack, TargetType.SingleEnemy,
                power: 2, cost: 1, statusEffect: StatusEffectType.Burn, effectDuration: 2, effectValue: 2,
                gainType: ResourceType.Ember, gainAmount: 2,
                behaviors: new BehaviorTag(BehaviorKeyword.TargetFullHP, 3));

            // 2. Phoenix Renewal — 아군 힐 3 + Ember×1 추가 힐, Ember +1
            // 강화 조건: 대상 HP 50%- 시 Burn/Poison 정화 추가 (CleanseLowTarget).
            // ★ 통합 파이프라인 검증: Heal 타입이 PostApply Phase 거쳐서 정화 자동 작동.
            CreatePhaseCCSkill("Ashe_PhoenixRenewal", "불사조 갱생", SkillType.Heal, TargetType.SingleAlly,
                power: 3, cost: 1,
                gainType: ResourceType.Ember, gainAmount: 1,
                resourcePowerPerStack: 1, // Ember×1 추가 힐
                behaviors: new BehaviorTag(BehaviorKeyword.CleanseLowTarget, 0));

            // 3. Brand of Ash — 단일 3 + Ember×1 데미지, Ember -2 소모.
            // 강화 조건: 자신 HP 50%- 시 데미지 2배 (Berserk). Ashe는 자해 메카닉이라 임계 도달 잦음.
            // Phase CC-2G-1: Desperation(rank=1) 추가 — 잃은 HP 10당 +1 위력 (Ember 자해와 시너지).
            CreatePhaseCCSkill("Ashe_BrandOfAsh", "잿더미 낙인", SkillType.Attack, TargetType.SingleEnemy,
                power: 3, cost: 2,
                costType: ResourceType.Ember, costAmount: 2,
                resourcePowerPerStack: 1, // Ember×1 추가 위력
                behaviors: new BehaviorTag[] {
                    new(BehaviorKeyword.Berserk, 0),       // 자신 HP 50%- 시 2배
                    new(BehaviorKeyword.Desperation, 1)    // 잃은 HP 10당 +1
                });

            // 4. Embrace of Cinders — 단일 11 + Ember×5, Ember 5 소모 (궁극기).
            // Phase CC-2G-1: AllIn(rank=10) 추가 — AP 0일 때 +10 위력 (풀충전 후 마무리 폭딜).
            CreatePhaseCCSkill("Ashe_EmbraceOfCinders", "잔불의 포옹", SkillType.Attack, TargetType.SingleEnemy,
                power: 11, cost: 3,
                costType: ResourceType.Ember, costAmount: 5,
                resourcePowerPerStack: 5, // Ember×5 추가 위력 — 풀충전 시 11+25=36
                behaviors: new BehaviorTag(BehaviorKeyword.AllIn, 10));

            // ════════════════════════════════════════════
            // Duran (Warrior) — Vengeance 복수 게이지 스킬 4종
            // ════════════════════════════════════════════
            // 1. Shield Wall — 아군 쉴드 3, AP 1 (셋업)
            // 강화 조건: Vengeance 5+ 시 썰드 +5 (ResourceThresholdShield).
            // ★ 통합 파이프라인 검증: Shield 타입이 ApplyMain Phase 거쳐서 임계값 가산 자동 작동.
            CreatePhaseCCSkill("Duran_ShieldWall", "방패벽", SkillType.Shield, TargetType.SingleAlly,
                power: 3, cost: 1,
                behaviors: new BehaviorTag(BehaviorKeyword.ResourceThresholdShield, 5));

            // 2. Provoking Shield — 자신 도발 부여, AP 1 (적이 Duran을 우선 공격)
            CreatePhaseCCSkill("Duran_ProvokingShield", "도발 방패", SkillType.Buff, TargetType.Self,
                power: 2, cost: 1, statusEffect: StatusEffectType.Taunt, effectDuration: 1, effectValue: 1);

            // 3. Revenge Strike — 단일 4 + Vengeance×1, Vengeance 전량 소모.
            // Phase CC-2G-2: Bulwank(5) 추가 — 쉴드 보유 시 +5 (Shield Wall 콤보).
            CreatePhaseCCSkill("Duran_RevengeStrike", "복수의 일격", SkillType.Attack, TargetType.SingleEnemy,
                power: 4, cost: 2,
                costType: ResourceType.Vengeance, costAmount: 0,
                resourcePowerPerStack: 1, consumeAllResource: true, // Vengeance 전량 소모
                behaviors: new BehaviorTag(BehaviorKeyword.Bulwark, 5));

            // 4. Last Bastion — 자신 쉴드 8 + Vengeance 15 소모 (궁극기).
            // Phase CC-2G-2: Desperation(1) 추가 — 잃은 HP 10당 +1 (탱커 궁극기 강화).
            CreatePhaseCCSkill("Duran_LastBastion", "최후의 보루", SkillType.Shield, TargetType.Self,
                power: 8, cost: 3,
                costType: ResourceType.Vengeance, costAmount: 15,
                behaviors: new BehaviorTag(BehaviorKeyword.Desperation, 1));

            // ════════════════════════════════════════════
            // Lumi (Cryomancer) — Frost 통제 스킬 4종
            // ════════════════════════════════════════════
            // 1. Frostbolt — 단일 2 + Freeze 1, Frost +1, AP 1
            // 강화 조건: 대상 이미 Freeze 상태 시 +3 위력 (총 5). Lumi 콤보: Frost Armor/Blizzard로 Freeze 건 뒤 사용.
            CreatePhaseCCSkill("Lumi_Frostbolt", "서리 화살", SkillType.Attack, TargetType.SingleEnemy,
                power: 2, cost: 1, statusEffect: StatusEffectType.Freeze, effectDuration: 1, effectValue: 1,
                gainType: ResourceType.Frost, gainAmount: 1,
                behaviors: new BehaviorTag(BehaviorKeyword.TargetFreeze, 3)); // Freeze 적 +3

            // 2. Frost Armor — 아군 쉴드 3, Frost +1, AP 1
            CreatePhaseCCSkill("Lumi_FrostArmor", "서리 갑옷", SkillType.Shield, TargetType.SingleAlly,
                power: 3, cost: 1,
                gainType: ResourceType.Frost, gainAmount: 1);

            // 3. Blizzard — 광역 1 + Freeze 1, Frost +1, AP 2
            CreatePhaseCCSkill("Lumi_Blizzard", "눈보라", SkillType.Attack, TargetType.AllEnemies,
                power: 1, cost: 2, statusEffect: StatusEffectType.Freeze, effectDuration: 1, effectValue: 1,
                gainType: ResourceType.Frost, gainAmount: 1);

            // 4. Glacial Spike — 단일 4, Frost 3 소모 (폭딜).
            // Phase CC-2G-3: Bulwark(4) + GiantSlayer(5) 추가 — Frost Armor 콤보 + 보스전 강화.
            CreatePhaseCCSkill("Lumi_GlacialSpike", "빙하 창", SkillType.Attack, TargetType.SingleEnemy,
                power: 4, cost: 2,
                costType: ResourceType.Frost, costAmount: 3,
                behaviors: new BehaviorTag[] {
                    new(BehaviorKeyword.Bulwark, 4),       // 쉴드 보유 시 +4 (Frost Armor 콤보)
                    new(BehaviorKeyword.GiantSlayer, 5)    // 적 MaxHP 100+ 시 +5 (보스전)
                });

            // ── Taranis (Stormcaller) Charge 메커니즘 (★ 2026-07-22 단순화 재설계) ──
            // 핵심 규칙 1개: Charge 보유 적이 매 턴 종료 시 고정 3 데미지를 자기 자신에게 받고 Charge -1.
            // Charge 스택 = 지속 턴 수. 데미지는 항상 3 고정.
            // 즉발 데미지 없음 → 타이밍이 늦지만 지속딜+광역딜로 총량 높음.

            // Wire (cost 1): 단일 적에게 Charge 3 → 3턴 × 3 = 총 9 도트
            CreatePhaseCCSkill("Taranis_Wire", "와이어", SkillType.Debuff, TargetType.SingleEnemy,
                power: 0, cost: 1, statusEffect: StatusEffectType.Charge, effectDuration: 3, effectValue: 3);

            // Branch (cost 2): 모든 적에게 Charge 1 → 각 1턴 × 3 = 총 3 도트/적
            CreatePhaseCCSkill("Taranis_Branch", "브랜치", SkillType.Debuff, TargetType.AllEnemies,
                power: 0, cost: 2, statusEffect: StatusEffectType.Charge, effectDuration: 3, effectValue: 1);

            // Grounding Field (cost 2): 아군 전체 쉴드 3
            CreatePhaseCCSkill("Taranis_GroundingField", "접지 장벽", SkillType.Shield, TargetType.AllAllies,
                power: 3, cost: 2);

            // Thunderstorm (cost 3): 모든 적에게 Charge 2 → 각 2턴 × 3 = 총 6 도트/적
            CreatePhaseCCSkill("Taranis_Thunderstorm", "뇌우", SkillType.Debuff, TargetType.AllEnemies,
                power: 0, cost: 3, statusEffect: StatusEffectType.Charge, effectDuration: 3, effectValue: 2);

            // ── Sibyl (Oracle) Prophecy 연동 스킬 (간소화 — 일반 힐/딜로 처리, Prophecy 메카닉은 추후) ──
            // Phase CC-2G-5: BehaviorTag 3종 추가로 조립식 시너지 혜택.
            // 1. Death Prophecy — FollowUp(4): 이번 턴 이미 맞은 적 +4. 파티 일점사 시너지.
            CreatePhaseCCSkill("Sibyl_DeathProphecy", "죽음의 예언", SkillType.Attack, TargetType.SingleEnemy,
                power: 5, cost: 1,
                behaviors: new BehaviorTag(BehaviorKeyword.FollowUp, 4));

            // 2. Vision of Renewal — LimitBreak(8): 전투당 첫 사용 시 힐 +8 (4→12).
            // "갱생" 컨셉 — 첫 사용은 강력한 회생이나, 전투가 길어질수록 효과 감소.
            CreatePhaseCCSkill("Sibyl_VisionOfRenewal", "갱생의 환영", SkillType.Heal, TargetType.SingleAlly,
                power: 4, cost: 1,
                behaviors: new BehaviorTag(BehaviorKeyword.LimitBreak, 8));

            // 3. Borrowed Future — Buff Self (AttackUp). 행운의 시간 차용.
            CreatePhaseCCSkill("Sibyl_BorrowedFuture", "미래 차용", SkillType.Buff, TargetType.Self,
                power: 0, cost: 1, statusEffect: StatusEffectType.AttackUp, effectDuration: 2, effectValue: 2);

            // 4. Déjà Vu — Echo(0): 위력 절반 추가 데미지. "데자부" 컨셉 — 같은 공격이 반복됨.
            CreatePhaseCCSkill("Sibyl_DéjàVu", "데자부", SkillType.Attack, TargetType.SingleEnemy,
                power: 4, cost: 1,
                behaviors: new BehaviorTag(BehaviorKeyword.Echo, 0));

            // Ashe — Pyromancer (Ember 자해 폭딜).
            // Cinder Accretion(충전) + Brand of Ash(폭딜) + 기존 Mage 스킬 2종
            CreatePhaseCCCharacter("Char_Ashe", "아셰", CharacterClass.Pyromancer,
                "화염 마법사. Ember 자원을 축적하여 자해 위험을 감수하고 폭딜을 낸다",
                12, 0, 0, new[] { "Ashe_CinderAccretion", "Ashe_BrandOfAsh", "Ashe_PhoenixRenewal", "Ashe_EmbraceOfCinders" },
                EnemyTrait.None, true, "", ResourceType.Ember);

            // Duran — Warrior (Vengeance 복수 게이지).
            CreatePhaseCCCharacter("Char_Duran", "듀란", CharacterClass.Warrior,
                "불멸의 성벽. 피격 시 Vengeance가 축적되며 소비 스킬로 버스트 딜",
                30, 0, 0, new[] { "Duran_RevengeStrike", "Duran_ShieldWall", "Duran_ProvokingShield", "Duran_LastBastion" },
                EnemyTrait.None, true, "", ResourceType.Vengeance);

            // Lumi — Cryomancer (Frost 통제).
            CreatePhaseCCCharacter("Char_Lumi", "루미", CharacterClass.Cryomancer,
                "냉기 마법사. Frost를 축적하여 적을 얼린다",
                16, 0, 0, new[] { "Lumi_Frostbolt", "Lumi_GlacialSpike", "Lumi_FrostArmor", "Lumi_Blizzard" },
                EnemyTrait.None, true, "", ResourceType.Frost);

            // Sibyl — Oracle (Prophecy 지연 발동 — 스킬이 1턴 뒤 발동)
            CreatePhaseCCCharacter("Char_Sibyl", "시빌", CharacterClass.Oracle,
                "예언자. 미래에 투자하는 서포터 — 스킬이 1턴 뒤 발동",
                18, 0, 0, new[] { "Sibyl_DeathProphecy", "Sibyl_VisionOfRenewal", "Sibyl_BorrowedFuture", "Sibyl_DéjàVu" },
                EnemyTrait.None, true, "", ResourceType.Prophecy);

            // Taranis — Stormcaller (Charge Network)
            CreatePhaseCCCharacter("Char_Taranis", "타라니스", CharacterClass.Stormcaller,
                "폭풍 소환사. 적에게 전하를 부여하여 매 턴 연쇄 도트 데미지",
                22, 0, 0, new[] { "Taranis_Wire", "Taranis_Branch", "Taranis_GroundingField", "Taranis_Thunderstorm" },
                EnemyTrait.None, true, "", ResourceType.None);

            // ════════════════════════════════════════════
            // Umbra (Rogue) — Shadows 그림자 자원 스킬 4종 (Phase CC-2A)
            // ════════════════════════════════════════════
            // 기획: ReworkDrafts/02_Rogue.md
            // 핵심: 안 맞을수록 치명타 강화. Eviscerate는 Shadows 3 필수 + 사용 후 -1.

            // 1. Poison Blade — 단일 1 + Poison 1턴, AP 1 (셋업). 항상 안전 사용.
            CreatePhaseCCSkill("Umbra_PoisonBlade", "독 바르기", SkillType.Attack, TargetType.SingleEnemy,
                power: 1, cost: 1, statusEffect: StatusEffectType.Poison, effectDuration: 2, effectValue: 1);

            // 2. Backstab — 단일 2, AP 2. 디버프 적 2배 (StrongVsDebuff BehaviorTag).
            // 대상이 Poison/Burn/Bleed/Freeze/Stun 중 하나라도 있으면 위력 ×2 (2 → 4).
            // Phase CC-2G-6: FollowUp(3) 추가 — 이미 맞은 적 +3 (Poison Blade/Rupture 콤보).
            CreatePhaseCCSkill("Umbra_Backstab", "기습 찌르기", SkillType.Attack, TargetType.SingleEnemy,
                power: 2, cost: 2,
                behaviors: new BehaviorTag[] {
                    new(BehaviorKeyword.StrongVsDebuff, 0),
                    new(BehaviorKeyword.FollowUp, 3)
                });

            // 3. Rupture — 단일 1 + Bleed 2턴, AP 1. HP 50%- 적 도트 +2턴 (Cull).
            CreatePhaseCCSkill("Umbra_Rupture", "할퀴기", SkillType.Attack, TargetType.SingleEnemy,
                power: 1, cost: 1, statusEffect: StatusEffectType.Bleed, effectDuration: 2, effectValue: 2,
                behaviors: new BehaviorTag(BehaviorKeyword.Cull, 0)); // HP 50%- 적 보너스

            // 4. Eviscerate — 단일 5, AP 3. Shadows 3 필수 + 사용 후 -1 (3→2).
            // minResourceRequired=3 (Shadows 최대치에서만 사용 가능).
            // costAmount=1 (사용 후 1 소모 — 매 턴 연속 Eviscerate 허용, 파티 보호 시).
            CreatePhaseCCSkill("Umbra_Eviscerate", "결정타", SkillType.Attack, TargetType.SingleEnemy,
                power: 5, cost: 3,
                costType: ResourceType.Shadows, costAmount: 1,
                minResourceRequired: 3); // Shadows 3 필수, 사용 후 -1

            // Umbra — Rogue (Shadows 그림자).
            // 안 맞을수록 치명타 강화. 파티 보호(도발/쉴드/일점사) 시너지 핵심.
            CreatePhaseCCCharacter("Char_Umbra", "움브라", CharacterClass.Rogue,
                "그림자 암살자. 안 맞을수록 치명타가 강해진다 — 동료가 그녀의 그림자를 지켜줄 때, 완벽한 암살이 완성된다",
                14, 0, 0, new[] { "Umbra_PoisonBlade", "Umbra_Backstab", "Umbra_Rupture", "Umbra_Eviscerate" },
                EnemyTrait.None, true, "", ResourceType.Shadows);

            // ════════════════════════════════════════════
            // Aster (Archer) — Combo 연속 사격 스킬 4종 (Phase CC-2B)
            // ════════════════════════════════════════════
            // 기획: ReworkDrafts/03_Archer.md
            // 핵심: 매 턴 스킬을 쏠수록 Combo 강화. Umbra(Shadows)와 정반대 축.

            // 1. Quick Shot — 단일 1, AP 1 (셋업). Combo +1은 ComboResourceComponent가 매 턴 스킬 사용 시 자동 처리.
            // Phase CC-2G-7: Momentum(1) 추가 — 매 사용 시 위력 +1 (UsesThisBattle 기반, 누적).
            // 쉴 새 없이 쏠수록 화살이 강해지는 "폭우의 사수" 컨셉 강화.
            CreatePhaseCCSkill("Aster_QuickShot", "빠른 사격", SkillType.Attack, TargetType.SingleEnemy,
                power: 1, cost: 1,
                behaviors: new BehaviorTag(BehaviorKeyword.Momentum, 1));

            // 2. Multi-Shot — 단일 1, AP 2. Combo 1 소모당 +1타격 (ComboMultiHit Behavior).
            // costType=Combo, costAmount=1 → TurnManager 기본 자원 소모. minResourceRequired=1 (Combo 1+ 필수).
            CreatePhaseCCSkill("Aster_MultiShot", "다중 사격", SkillType.Attack, TargetType.SingleEnemy,
                power: 1, cost: 2,
                costType: ResourceType.Combo, costAmount: 1,
                minResourceRequired: 1,
                behaviors: new BehaviorTag(BehaviorKeyword.ComboMultiHit, 0));

            // 3. Hunter's Mark — 단일 Mark 부여 (2턴), AP 1. Archer 본인이 Mark 적 공격 시 +3 (PowerAddVsMark 특성).
            CreatePhaseCCSkill("Aster_HuntersMark", "사냥표식", SkillType.Debuff, TargetType.SingleEnemy,
                power: 0, cost: 1, statusEffect: StatusEffectType.Mark, effectDuration: 2, effectValue: 1);

            // 4. Execute Shot — 단일 3 + Combo×2, AP 3. 모든 Combo 소모. 킬 시 Combo 3 복구 (ComboFinisher Behavior).
            // resourcePowerPerStack=2 → Combo 1당 +2 위력 (3스택 시 3+6=9).
            // consumeAllResource=true → TurnManager가 전량 소모. minResourceRequired=1.
            CreatePhaseCCSkill("Aster_ExecuteShot", "처형 사격", SkillType.Attack, TargetType.SingleEnemy,
                power: 3, cost: 3,
                costType: ResourceType.Combo, costAmount: 0,
                resourcePowerPerStack: 2, consumeAllResource: true,
                minResourceRequired: 1,
                behaviors: new BehaviorTag(BehaviorKeyword.ComboFinisher, 0));

            // Aster — Archer (Combo 연속 사격).
            // 매 턴 스킬을 쏠수록 Combo가 강해진다. 스킬을 안 쓰면 Combo 리셋.
            CreatePhaseCCCharacter("Char_Archer", "아스테르", CharacterClass.Archer,
                "폭우의 사수. 쉴 새 없이 쏠수록 화살이 강해진다 — 멈추지 않는 공격이 곧 완벽한 사냥이다",
                16, 0, 0, new[] { "Aster_QuickShot", "Aster_MultiShot", "Aster_HuntersMark", "Aster_ExecuteShot" },
                EnemyTrait.None, true, "", ResourceType.Combo);

            // ════════════════════════════════════════════
            // Elara (Healer) — Mercy 회복의 연결고리 스킬 4종 (Phase CC-2C)
            // ════════════════════════════════════════════
            // 기획: ReworkDrafts/01_Healer.md
            // 핵심: 매 턴 자동 힐 + Mercy 축전 → 15 도달 시 자동 ATK+3 버스트. 힐과 버프의 순환.

            // 1. Bond Link — 단일 BondBoost 부여 (2턴). 자동 힐 1→2 강화. AP 1 (셋업).
            CreatePhaseCCSkill("Elara_BondLink", "연결의 끈", SkillType.Buff, TargetType.SingleAlly,
                power: 0, cost: 1, statusEffect: StatusEffectType.BondBoost, effectDuration: 2, effectValue: 1,
                behaviors: new BehaviorTag(BehaviorKeyword.BondLinkBoost, 0));

            // 2. Mend Wounds — 단일 힐 3 + 도트 정화 (CleanseLowTarget) + Mercy +10 축전 (MercyAccumulate).
            // CleanseLowTarget은 HP 50%- 시 정화이므로, 일반적으로는 힐만. 추후 별도 정화 Behavior 검토.
            CreatePhaseCCSkill("Elara_MendWounds", "상처 치유", SkillType.Heal, TargetType.SingleAlly,
                power: 3, cost: 2,
                behaviors: new BehaviorTag[] {
                    new(BehaviorKeyword.CleanseLowTarget, 0),
                    new(BehaviorKeyword.MercyAccumulate, 0)
                });

            // 3. Blessing of Mercy — 단일 ATK+2 (3턴), Mercy 5 소모.
            CreatePhaseCCSkill("Elara_BlessingOfMercy", "자비의 축복", SkillType.Buff, TargetType.SingleAlly,
                power: 0, cost: 2, statusEffect: StatusEffectType.AttackUp, effectDuration: 3, effectValue: 2,
                costType: ResourceType.Mercy, costAmount: 5,
                behaviors: new BehaviorTag(BehaviorKeyword.MercyConsume, 0));

            // 4. Sanctuary — 광역 힐 2 + Mercy 8 소모 (자원 임계). MercyAccumulate로 Mercy +8 축전 후 소모.
            // (PostApply에서 Mercy +8 → 자동 버스트 가능성 → 이후 costAmount 8 소모)
            CreatePhaseCCSkill("Elara_Sanctuary", "성소", SkillType.Heal, TargetType.AllAllies,
                power: 2, cost: 3,
                costType: ResourceType.Mercy, costAmount: 8,
                behaviors: new BehaviorTag[] {
                    new(BehaviorKeyword.MercyAccumulate, 0),
                    new(BehaviorKeyword.MercyConsume, 0)
                });

            // Elara — Healer (Mercy 회복의 연결고리).
            // 매 턴 연결된 파티원에게 자동 힐. 회복이 쌓이면 자동으로 파티원에게 축복이 내려진다.
            CreatePhaseCCCharacter("Char_Healer", "엘라라", CharacterClass.Healer,
                "빛의 여신. 파티에 회복의 연결고리를 만들어, 동료가 행동할 때마다 상처를 어루만진다 — 그리고 그 회복이 쌓이면, 축복이 내려진다",
                20, 0, 0, new[] { "Elara_BondLink", "Elara_MendWounds", "Elara_BlessingOfMercy", "Elara_Sanctuary" },
                EnemyTrait.None, true, "", ResourceType.Mercy);

            // ════════════════════════════════════════════
            // Calliope (Bard) — Melody 주/부 선율 메아리 스킬 4종 (Phase CC-2D)
            // ════════════════════════════════════════════
            // 기획: ReworkDrafts/06_Bard.md
            // 핵심: 매 턴 다른 선율 연주 → 주 선율(100%) + 직전 턴 부 선율(50%) 자동 발동.

            // 1. Mending Song — 단일 힐 8, AP 2. 회복 영역. CurrentMelody=Healing.
            CreatePhaseCCSkill("Calliope_MendingSong", "치유의 노래", SkillType.Heal, TargetType.SingleAlly,
                power: 2, cost: 2,
                behaviors: new BehaviorTag[] { new(BehaviorKeyword.MelodyHealing, 0) });

            // 2. Anthem of Valor — 광역 ATK+3 (2턴), AP 2. 버프 영역. CurrentMelody=Valor.
            CreatePhaseCCSkill("Calliope_AnthemOfValor", "용기의 찬가", SkillType.Buff, TargetType.AllAllies,
                power: 0, cost: 2, statusEffect: StatusEffectType.AttackUp, effectDuration: 2, effectValue: 2,
                behaviors: new BehaviorTag[] { new(BehaviorKeyword.MelodyValor, 0) });

            // 3. Dissonant Chord — 광역 적 ATK-3 (2턴), AP 2. 디버프 영역. CurrentMelody=Dissonance.
            CreatePhaseCCSkill("Calliope_DissonantChord", "불협화음", SkillType.Debuff, TargetType.AllEnemies,
                power: 0, cost: 2, statusEffect: StatusEffectType.AttackDown, effectDuration: 2, effectValue: 2,
                behaviors: new BehaviorTag[] { new(BehaviorKeyword.MelodyDissonance, 0) });

            // 4. Inspiring Refrain — 광역 쉴드 5, AP 2. 유틸리티 영역. CurrentMelody=Inspiration.
            // (TODO: AP+1 효과는 별도 Behavior/인프라 필요 — 현재 쉴드만. 추후 확장)
            CreatePhaseCCSkill("Calliope_InspiringRefrain", "영감의 후렴", SkillType.Shield, TargetType.AllAllies,
                power: 2, cost: 2,
                behaviors: new BehaviorTag[] { new(BehaviorKeyword.MelodyInspiration, 0) });

            // Calliope — Bard (Melody 주/부 선율 메아리).
            // 매 턴 다른 선율을 연주하며, 직전 곡이 부 선율로 메아리쳐 두 효과가 겹친다.
            CreatePhaseCCCharacter("Char_Bard", "칼리오페", CharacterClass.Bard,
                "서사시의 여신. 매 턴 다른 곡을 연주하며, 직전의 곡이 부 선율로 메아리쳐 울려 퍼진다 — 두 선율이 겹칠 때, 비로소 교향곡이 완성된다",
                18, 0, 0, new[] { "Calliope_MendingSong", "Calliope_AnthemOfValor", "Calliope_DissonantChord", "Calliope_InspiringRefrain" },
                EnemyTrait.None, true, "", ResourceType.Melody);

            Debug.Log("[DataGenerator] Phase CC 캐릭터 9종 생성 완료 (Ashe/Duran/Lumi/Sibyl/Taranis/Umbra/Aster/Elara/Calliope)");

            // ════════════════════════════════════════════
            // Cael (Alchemist) — Discover 발견 메커니즘 (Phase CC-2E)
            // ════════════════════════════════════════════
            // 기획: ReworkDrafts/05_Alchemist.md
            // 핵심: 매 스킬 3개 선택지 모달 팝업 (하스스톤 발견). 자원 없음, 순수 선택 기반.
            // 발견 본체 스킬 4개 + 각 풀 5-7개 효과 스킬.

            GenerateCaelDiscoverPools();

            // 4개 발견 본체 스킬 (IsDiscover=true, DiscoverPool 연결)
            CreatePhaseCCSkill("Cael_MendingBrew", "회복 물약", SkillType.Heal, TargetType.SingleAlly,
                power: 0, cost: 2, isDiscover: true,
                discoverPool: LoadDiscoverPool("Pool_Mending"));
            CreatePhaseCCSkill("Cael_StrengtheningBrew", "강화 물약", SkillType.Buff, TargetType.SingleAlly,
                power: 0, cost: 2, isDiscover: true,
                discoverPool: LoadDiscoverPool("Pool_Strengthening"));
            CreatePhaseCCSkill("Cael_CripplingBrew", "약화 물약", SkillType.Debuff, TargetType.SingleEnemy,
                power: 0, cost: 2, isDiscover: true,
                discoverPool: LoadDiscoverPool("Pool_Crippling"));
            CreatePhaseCCSkill("Cael_CatalyticBrew", "촉매 물약", SkillType.Attack, TargetType.SingleEnemy,
                power: 0, cost: 3, isDiscover: true,
                discoverPool: LoadDiscoverPool("Pool_Catalytic"));

            // Cael — Alchemist (Discover 발견 — 자원 없음, 매 스킬 3-4개 선택지).
            // 발견 스킬 4개만 CharacterData._skills에 등록 (24개 풀 스킬은 드로우 풀 오염 방지로 등록 안 함).
            CreatePhaseCCCharacter("Char_Alchemist", "켈", CharacterClass.Alchemist,
                "연금술사. 매 스킬이 새로운 발견의 순간 — 풀에서 3개의 선택지를 무작위로 뽑아, 상황에 맞는 효과를 선택해 발동한다. 같은 스킬도 매번 다른 결과를 만든다",
                22, 0, 0, new[] { "Cael_MendingBrew", "Cael_StrengtheningBrew", "Cael_CripplingBrew", "Cael_CatalyticBrew" },
                EnemyTrait.None, true, "", ResourceType.None);

            // ════════════════════════════════════════════
            // Mortis (Necromancer) — Summoned Corpse 시체 메커니즘 (Phase CC-2F)
            // ════════════════════════════════════════════
            // 기획: ReworkDrafts/04_Necromancer.md
            // 핵심: 매 턴 종료 후 시체가 4스킬 중 무작위 1개 자동 시전. 적 처치 시 스킬 교체.

            // 시체 기본 스킬 4종 (전투 시작 시 시체 슬롯 초기화용)
            CreatePhaseCCSkill("Corpse_Scratch", "할퀴기", SkillType.Attack, TargetType.SingleEnemy,
                power: 1, cost: 0, isCorpseSkill: true);
            CreatePhaseCCSkill("Corpse_PoisonBite", "독 물기", SkillType.Attack, TargetType.SingleEnemy,
                power: 1, cost: 0, isCorpseSkill: true,
                statusEffect: StatusEffectType.Poison, effectDuration: 2, effectValue: 2);
            CreatePhaseCCSkill("Corpse_BoneToss", "뼈 던지기", SkillType.Attack, TargetType.SingleEnemy,
                power: 1, cost: 0, isCorpseSkill: true,
                statusEffect: StatusEffectType.Bleed, effectDuration: 2, effectValue: 2);
            CreatePhaseCCSkill("Corpse_StunStrike", "기절 타격", SkillType.Attack, TargetType.SingleEnemy,
                power: 1, cost: 0, isCorpseSkill: true,
                statusEffect: StatusEffectType.Stun, effectDuration: 1, effectValue: 1);

            // Mortis 본인 스킬 4종 (시체 강화 마법)
            // 1. Empower Undead — Buff, Self, AP 1. 다음 시체 스킬 위력 +2.
            CreatePhaseCCSkill("Mortis_EmpowerUndead", "강령술 강화", SkillType.Buff, TargetType.Self,
                power: 0, cost: 1, corpseAction: CorpseActionType.EmpowerNext, corpseActionValue: 2);
            // 2. Soul Link — Buff, Self, AP 2. 2턴간 시체가 준 데미지 50% 회복.
            CreatePhaseCCSkill("Mortis_SoulLink", "영혼 결속", SkillType.Buff, TargetType.Self,
                power: 0, cost: 2, corpseAction: CorpseActionType.SoulLink, corpseActionValue: 50);
            // 3. Curse of Weakness — Debuff, SingleEnemy, AP 2. ATK-2 (2턴). DEF down은 일반 인프라 제약으로 생략.
            CreatePhaseCCSkill("Mortis_CurseOfWeakness", "약화 저주", SkillType.Debuff, TargetType.SingleEnemy,
                power: 0, cost: 2, statusEffect: StatusEffectType.AttackDown, effectDuration: 2, effectValue: 2);
            // 4. Mass Empower — Buff, Self, AP 3. 시체 모든 스킬 영구 위력 +1.
            CreatePhaseCCSkill("Mortis_MassEmpower", "대량 강화", SkillType.Buff, TargetType.Self,
                power: 0, cost: 3, corpseAction: CorpseActionType.MassEmpower, corpseActionValue: 1);

            // Mortis — Necromancer (Summoned Corpse — 시체 자동 전투).
            // 매 턴 종료 후 시체가 4스킬 중 무작위 1개 자동 시전. 적 처치 시 스킬 교체.
            CreatePhaseCCCharacter("Char_Necromancer", "모티스", CharacterClass.Necromancer,
                "죽음의 스승. 전투마다 시체를 일으켜 세우고, 적을 쓰러뜨려 그 기술을 시체에게 먹여 성장시킨다 — 죽음은 끝이 아니라, 새로운 시작이다",
                20, 0, 0, new[] { "Mortis_EmpowerUndead", "Mortis_SoulLink", "Mortis_CurseOfWeakness", "Mortis_MassEmpower" },
                EnemyTrait.None, true, "", ResourceType.None,
                corpseSkills: new[] { "Corpse_Scratch", "Corpse_PoisonBite", "Corpse_BoneToss", "Corpse_StunStrike" });

            Debug.Log("[DataGenerator] Phase CC 캐릭터 11종 생성 완료 (Ashe/Duran/Lumi/Sibyl/Taranis/Umbra/Aster/Elara/Calliope/Cael/Mortis)");
        }

        /// <summary>Cael 발견 풀 4종 + 효과 스킬 24개 생성 (Phase CC-2E).</summary>
        private static void GenerateCaelDiscoverPools()
        {
            EnsureFolder(DISCOVER_POOL_PATH);

            // ════ Mending (회복) — 5개 효과 ════
            CreateDiscoverEffectSkill("DM_Heal10", "소회복", SkillType.Heal, TargetType.SingleAlly, 3);
            CreateDiscoverEffectSkill("DM_Heal15", "대회복", SkillType.Heal, TargetType.SingleAlly, 5);
            CreateDiscoverEffectSkill("DM_Shield10", "보호막", SkillType.Shield, TargetType.SingleAlly, 3);
            CreateDiscoverEffectSkill("DM_Purify", "정화", SkillType.Purify, TargetType.SingleAlly, 0);
            CreateDiscoverEffectSkill("DM_Heal8Regen", "재생 물약", SkillType.Heal, TargetType.SingleAlly, 2,
                statusEffect: StatusEffectType.Regeneration, effectDuration: 2, effectValue: 2);

            CreateDiscoverPool("Pool_Mending", "회복 물약 풀", DiscoverCategory.Mending,
                "회복/쉴드/정화 영역 — 대상 상태에 따라 선택",
                new[] { ("DM_Heal10", 30), ("DM_Heal15", 20), ("DM_Shield10", 20),
                        ("DM_Purify", 15), ("DM_Heal8Regen", 15) });

            // ════ Strengthening (버프) — 6개 효과 ════
            CreateDiscoverEffectSkill("DS_Atk3", "공격 강화", SkillType.Buff, TargetType.SingleAlly, 0,
                statusEffect: StatusEffectType.AttackUp, effectDuration: 2, effectValue: 2);
            CreateDiscoverEffectSkill("DS_Def3", "방어 강화", SkillType.Buff, TargetType.SingleAlly, 0,
                statusEffect: StatusEffectType.DefenseUp, effectDuration: 2, effectValue: 2);
            CreateDiscoverEffectSkill("DS_Atk2", "속성 강화", SkillType.Buff, TargetType.SingleAlly, 0,
                statusEffect: StatusEffectType.AttackUp, effectDuration: 2, effectValue: 1);
            CreateDiscoverEffectSkill("DS_Def2", "보호 강화", SkillType.Buff, TargetType.SingleAlly, 0,
                statusEffect: StatusEffectType.DefenseUp, effectDuration: 2, effectValue: 1);
            CreateDiscoverEffectSkill("DS_AtkAll2", "전체 공격 강화", SkillType.Buff, TargetType.AllAllies, 0,
                statusEffect: StatusEffectType.AttackUp, effectDuration: 1, effectValue: 1);
            CreateDiscoverEffectSkill("DS_DefAll2", "전체 보호 강화", SkillType.Buff, TargetType.AllAllies, 0,
                statusEffect: StatusEffectType.DefenseUp, effectDuration: 1, effectValue: 1);

            CreateDiscoverPool("Pool_Strengthening", "강화 물약 풀", DiscoverCategory.Strengthening,
                "버프 영역 — 현재 부족한 스탯 보충",
                new[] { ("DS_Atk3", 25), ("DS_Def3", 20), ("DS_Atk2", 20),
                        ("DS_Def2", 15), ("DS_AtkAll2", 10), ("DS_DefAll2", 10) });

            // ════ Crippling (디버프) — 6개 효과 ════
            CreateDiscoverEffectSkill("DC_AtkDown3", "약화 곰팡", SkillType.Debuff, TargetType.SingleEnemy, 0,
                statusEffect: StatusEffectType.AttackDown, effectDuration: 2, effectValue: 2);
            CreateDiscoverEffectSkill("DC_DefDown3", "부식 액체", SkillType.Debuff, TargetType.SingleEnemy, 0,
                statusEffect: StatusEffectType.DefenseDown, effectDuration: 2, effectValue: 2);
            CreateDiscoverEffectSkill("DC_Stun", "마비 독", SkillType.Debuff, TargetType.SingleEnemy, 0,
                statusEffect: StatusEffectType.Stun, effectDuration: 1, effectValue: 1);
            CreateDiscoverEffectSkill("DC_Poison", "맹독", SkillType.Debuff, TargetType.SingleEnemy, 0,
                statusEffect: StatusEffectType.Poison, effectDuration: 3, effectValue: 2);
            CreateDiscoverEffectSkill("DC_Burn", "발화제", SkillType.Debuff, TargetType.SingleEnemy, 0,
                statusEffect: StatusEffectType.Burn, effectDuration: 3, effectValue: 2);
            CreateDiscoverEffectSkill("DC_Bleed", "혈액 응고제", SkillType.Debuff, TargetType.SingleEnemy, 0,
                statusEffect: StatusEffectType.Bleed, effectDuration: 3, effectValue: 2);

            CreateDiscoverPool("Pool_Crippling", "약화 물약 풀", DiscoverCategory.Crippling,
                "디버프 영역 — 적 약화 방식 선택 (독성 폭발 특성 시 가중치 2배)",
                new[] { ("DC_AtkDown3", 25), ("DC_DefDown3", 20), ("DC_Stun", 15),
                        ("DC_Poison", 15), ("DC_Burn", 15), ("DC_Bleed", 10) });

            // ════ Catalytic (유틸리티) — 7개 효과 ════
            CreateDiscoverEffectSkill("DX_Aoe6", "연쇄 폭발", SkillType.Attack, TargetType.AllEnemies, 2);
            CreateDiscoverEffectSkill("DX_Single15", "고농도 폭약", SkillType.Attack, TargetType.SingleEnemy, 5);
            CreateDiscoverEffectSkill("DX_Single8", "신속한 투척", SkillType.Attack, TargetType.SingleEnemy, 3);
            CreateDiscoverEffectSkill("DX_PartyShield5", "진동 보호막", SkillType.Shield, TargetType.AllAllies, 2);
            CreateDiscoverEffectSkill("DX_Charge", "전하 주입", SkillType.Debuff, TargetType.SingleEnemy, 0,
                statusEffect: StatusEffectType.Charge, effectDuration: 3, effectValue: 1);
            CreateDiscoverEffectSkill("DX_PartyHeal5", "치유 가스", SkillType.Heal, TargetType.AllAllies, 2);
            CreateDiscoverEffectSkill("DX_Single10", "농축 타격", SkillType.Attack, TargetType.SingleEnemy, 4);

            CreateDiscoverPool("Pool_Catalytic", "촉매 물약 풀", DiscoverCategory.Catalytic,
                "유틸리티 영역 — 상황 특수 효과 (광역/폭딜/특수)",
                new[] { ("DX_Aoe6", 20), ("DX_Single15", 20), ("DX_Single8", 15),
                        ("DX_PartyShield5", 15), ("DX_Charge", 10),
                        ("DX_PartyHeal5", 10), ("DX_Single10", 10) });

            Debug.Log("[DataGenerator] Cael 발견 풀 4종 + 효과 스킬 24개 생성 완료");
        }

        /// <summary>발견 풀의 개별 효과 스킬 생성 — 모두 Cost=0 (본체에서 AP 청구).</summary>
        private static void CreateDiscoverEffectSkill(string fileName, string name, SkillType type, TargetType target,
            int power, StatusEffectType statusEffect = StatusEffectType.None,
            int effectDuration = 0, int effectValue = 0)
        {
            CreatePhaseCCSkill(fileName, name, type, target,
                power: power, cost: 0, // 발견 선택 스킬은 Cost=0 (본체에서 청구)
                statusEffect: statusEffect, effectDuration: effectDuration, effectValue: effectValue);
        }

        /// <summary>발견 풀 데이터 .asset 생성.</summary>
        private static void CreateDiscoverPool(string fileName, string poolName, DiscoverCategory category,
            string description, (string skillFile, int weight)[] entries)
        {
            var path = $"{DISCOVER_POOL_PATH}/{fileName}.asset";
            var pool = GetOrCreateAsset<DiscoverPoolData>(path);
            pool.name = fileName;

            SetPrivateField(pool, "_poolName", poolName);
            SetPrivateField(pool, "_category", category);
            SetPrivateField(pool, "_description", description);

            var entryList = new List<DiscoverEntry>();
            foreach (var (skillFile, weight) in entries)
            {
                var skill = AssetDatabase.LoadAssetAtPath<SkillData>($"{SKILL_PATH}/{skillFile}.asset");
                if (skill != null)
                    entryList.Add(new DiscoverEntry(skill, weight));
                else
                    Debug.LogWarning($"[DiscoverPool] 스킬 {skillFile} 로드 실패 — 풀 {fileName}");
            }
            SetPrivateField(pool, "_entries", entryList.ToArray());

            EditorUtility.SetDirty(pool);
        }

        /// <summary>발견 풀 .asset 로드 헬퍼.</summary>
        private static DiscoverPoolData LoadDiscoverPool(string fileName)
            => AssetDatabase.LoadAssetAtPath<DiscoverPoolData>($"{DISCOVER_POOL_PATH}/{fileName}.asset");

        /// <summary>Phase CC 캐릭터 생성 헬퍼 — ResourceType + corpseSkills 포함.</summary>
        private static void CreatePhaseCCCharacter(string fileName, string name, CharacterClass charClass,
            string desc, int hp, int atk, int def, string[] skills, EnemyTrait trait,
            bool isDefault, string unlockCondition, ResourceType resourceType,
            string[] corpseSkills = null)
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

            // Phase CC-2F: Necromancer 시체 기본 스킬 등록
            if (corpseSkills != null && corpseSkills.Length > 0)
            {
                var corpseList = new List<SkillData>();
                foreach (var skillName in corpseSkills)
                {
                    var skill = AssetDatabase.LoadAssetAtPath<SkillData>($"{SKILL_PATH}/{skillName}.asset");
                    if (skill != null) corpseList.Add(skill);
                }
                SetPrivateField(character, "_corpseBaseSkills", corpseList);
            }

            EditorUtility.SetDirty(character);
        }

        /// <summary>Phase CC 스킬 생성 헬퍼 — 자원 획득/소모/비례 위력 + 쉴드 속성 + 발견 스킬 + 시체 스킬 포함.</summary>
        private static void CreatePhaseCCSkill(string fileName, string name, SkillType type, TargetType target,
            int power, int cost, StatusEffectType statusEffect = StatusEffectType.None,
            int effectDuration = 0, int effectValue = 0,
            ResourceType gainType = ResourceType.None, int gainAmount = 0,
            ResourceType costType = ResourceType.None, int costAmount = 0,
            int resourcePowerPerStack = 0, bool consumeAllResource = false,
            ShieldFlag shieldFlags = ShieldFlag.None,
            int minResourceRequired = 0,
            bool isDiscover = false, DiscoverPoolData discoverPool = null,
            bool isCorpseSkill = false,
            CorpseActionType corpseAction = CorpseActionType.None, int corpseActionValue = 0,
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
            SetPrivateField(skill, "_minResourceRequired", minResourceRequired);
            SetPrivateField(skill, "_isDiscover", isDiscover);
            SetPrivateField(skill, "_discoverPool", discoverPool);
            SetPrivateField(skill, "_isCorpseSkill", isCorpseSkill);
            SetPrivateField(skill, "_corpseAction", corpseAction);
            SetPrivateField(skill, "_corpseActionValue", corpseActionValue);
            SetPrivateField(skill, "_behaviors", behaviors ?? new BehaviorTag[0]);

            EditorUtility.SetDirty(skill);
        }
    }
}
#endif
