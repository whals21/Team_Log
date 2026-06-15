#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Skill;

namespace TeamLog.Editor
{
    /// <summary>
    /// DataGenerator — 증강 데이터 및 스폰 패턴 생성
    /// 진입점/스킬/캐릭터/유틸리티: DataGenerator.cs
    /// 이벤트 데이터: DataGenerator.Events.cs
    /// 유물 데이터: DataGenerator.Relics.cs
    /// 팔레트 (UI/오디오/VFX): DataGenerator.Palettes.cs
    /// </summary>
    public static partial class DataGenerator
    {
        #region Augment Data

        private static void GenerateAugmentData()
        {
            // (파일명, 표시명, 설명, AugmentType, 호환SkillType, 등급, 저주여부, 저주설명, 키워드[])
            var augments = new[]
            {
                Mk("Aug_CostDown", "비용 감소", "스킬 코스트가 1 감소합니다 (최소 0)", AugmentType.CostDown, SkillType.Attack, 1, false, "",
                    Kw(KeywordType.CostAdd, -1)),

                Mk("Aug_Spread", "확산", "단일 대상 스킬이 광역으로 변합니다 (위력 70%)", AugmentType.Spread, SkillType.Attack, 2, false, ""),

                Mk("Aug_Pierce", "관통", "쉴드를 무시하고 방어력의 50%를 무시합니다", AugmentType.Pierce, SkillType.Attack, 2, false, ""),

                Mk("Aug_Chain", "연쇄", "타격 후 인접한 적에게 위력 60%의 연쇄 피해를 줍니다", AugmentType.Chain, SkillType.Attack, 2, false, ""),

                Mk("Aug_Drain", "흡혈", "데미지의 30%만큼 자신의 HP를 회복합니다", AugmentType.Drain, SkillType.Attack, 1, false, "",
                    Kw(KeywordType.DamageDealtHealPercent, 0.3f)),

                Mk("Aug_HeavyHit", "중격", "위력이 1.5배가 되지만 코스트가 1 증가합니다", AugmentType.HeavyHit, SkillType.Attack, 3, false, "",
                    Kw(KeywordType.PowerMul, 1.5f),
                    Kw(KeywordType.CostAdd, 1)),

                Mk("Aug_QuickDraw", "신속", "가중치가 0이 되어 무조건 뽑힙니다 (위력 80%)", AugmentType.QuickDraw, SkillType.Attack, 1, false, "",
                    Kw(KeywordType.PowerMul, 0.8f),
                    Kw(KeywordType.DrawWeightOverride, 0f)),

                Mk("Aug_Lingering", "잔류", "상태이상 지속시간이 2턴 증가합니다", AugmentType.Lingering, SkillType.Debuff, 1, false, "",
                    Kw(KeywordType.DurationAdd, 2)),

                Mk("Aug_Intensify", "강화", "버프/디버프 효과가 1.5배가 됩니다", AugmentType.Intensify, SkillType.Buff, 2, false, "",
                    Kw(KeywordType.EffectMul, 1.5f)),

                Mk("Aug_VenomTouch", "맹독", "공격 시 중독을 추가합니다 (2턴, 위력 30%)", AugmentType.VenomTouch, SkillType.Attack, 1, false, ""),
                Mk("Aug_BurningTouch", "화염", "공격 시 화상을 추가합니다 (2턴, 위력 30%)", AugmentType.BurningTouch, SkillType.Attack, 1, false, ""),

                Mk("Aug_ShieldBonus", "철벽", "쉴드 효과가 1.5배가 됩니다", AugmentType.ShieldBonus, SkillType.Shield, 1, false, "",
                    Kw(KeywordType.ShieldMul, 1.5f)),

                Mk("Aug_HealBonus", "치유 강화", "힐 효과가 1.5배가 됩니다", AugmentType.HealBonus, SkillType.Heal, 1, false, "",
                    Kw(KeywordType.HealMul, 1.5f)),

                // 저주 증강
                Mk("Aug_BloodPact", "피의 계약", "위력이 +5 증가합니다", AugmentType.BloodPact, SkillType.Attack, 2, true, "매턴 HP 2 감소",
                    Kw(KeywordType.PowerAdd, 5),
                    Kw(KeywordType.HPPerTurn, -2, KeywordTrigger.Passive)),

                Mk("Aug_GlassCannon", "유리 대포", "위력이 +8 증가합니다", AugmentType.GlassCannon, SkillType.Attack, 3, true, "받는 피해 +50%",
                    Kw(KeywordType.PowerAdd, 8),
                    Kw(KeywordType.DamageTakenMul, 1.5f)),

                Mk("Aug_Reaper", "수확자", "적을 처치할 때 HP 10을 회복합니다", AugmentType.Reaper, SkillType.Attack, 2, true, "코스트 +1",
                    Kw(KeywordType.OnKillHeal, 10),
                    Kw(KeywordType.CostAdd, 1)),

                Mk("Aug_AOEAuto", "파동", "단일 대상 스킬이 자동으로 광역이 됩니다 (위력 65%)", AugmentType.AOEAuto, SkillType.Attack, 2, true, "코스트 +1",
                    Kw(KeywordType.CostAdd, 1)),

                Mk("Aug_Berserk", "광폭", "위력이 2배가 됩니다", AugmentType.Berserk, SkillType.Attack, 3, true, "HP 50% 이하일 때만 발동",
                    Kw(KeywordType.PowerMul, 2f, KeywordTrigger.HPBelow, 0.5f)),
            };

            foreach (var (fileName, augmentName, description, type, compatSkillType, tier, isCursed, curseDesc, keywords) in augments)
            {
                var path = $"{AUGMENT_PATH}/{fileName}.asset";
                var augment = GetOrCreateAsset<AugmentData>(path);
                augment.name = fileName;

                SetPrivateField(augment, "_augmentName", augmentName);
                SetPrivateField(augment, "_description", description);
                SetPrivateField(augment, "_type", type);
                SetPrivateField(augment, "_compatibleSkillType", compatSkillType);
                SetPrivateField(augment, "_tier", tier);
                SetPrivateField(augment, "_isCursed", isCursed);
                SetPrivateField(augment, "_curseDescription", curseDesc);
                SetPrivateField(augment, "_keywords", keywords);

                EditorUtility.SetDirty(augment);
            }

            Debug.Log($"[DataGenerator] 증강 데이터 {augments.Length}개 생성 완료");
        }

        /// <summary>증강 데이터 튜플 생성 헬퍼</summary>
        private static (string, string, string, AugmentType, SkillType, int, bool, string, KeywordEntry[])
            Mk(string fileName, string name, string desc, AugmentType type, SkillType compat, int tier, bool cursed, string curseDesc,
               params KeywordEntry[] keywords)
        {
            return (fileName, name, desc, type, compat, tier, cursed, curseDesc, keywords);
        }

        /// <summary>KeywordEntry 생성 헬퍼</summary>
        private static KeywordEntry Kw(KeywordType type, float value, KeywordTrigger trigger = KeywordTrigger.Passive, float conditionParam = 0f)
        {
            return new KeywordEntry(type, value, trigger, conditionParam);
        }

        #endregion

        #region Spawn Pattern Tables

        /// <summary>
        /// 층별 스폰 패턴 테이블 생성
        /// 전투력 기준(HP+ATK*5): F1~130, F2~200, F3~280
        /// 각 층마다 6개 일반 패턴 + 3개 엘리트 패턴
        /// </summary>
        private static void GenerateSpawnPatternTables()
        {
            // 층 1 — 숲 (슬라임45, 고블린60, 늑대59, 독버섯37)
            CreateSpawnPatternTable("SpawnPatterns_F1",
                normalPatterns: new[]
                {
                    // 패턴명,        구성                                전투력
                    ("초원의 무리",   new[]{("Enemy_Slime",3),("Enemy_Goblin",2)}),         // 3*45+2*60=255
                    ("숲의 정찰대",   new[]{("Enemy_Goblin",2),("Enemy_Wolf",3)}),          // 2*60+3*59=297
                    ("독 포자 군단",  new[]{("Enemy_Mushroom",3),("Enemy_Slime",3)}),       // 3*37+3*45=246
                    ("늑대 무리",     new[]{("Enemy_Wolf",5)}),                              // 5*59=295
                    ("고블린 약탈단", new[]{("Enemy_Goblin",4),("Enemy_Slime",1)}),         // 4*60+45=285
                    ("혼성 부대",     new[]{("Enemy_Slime",2),("Enemy_Wolf",1),("Enemy_Goblin",1),("Enemy_Mushroom",1)}), // 90+59+60+37=246
                },
                elitePatterns: new[]
                {
                    ("엘리트 기사 호위", new[]{("Enemy_EliteKnight",1),("Enemy_Skeleton",2)}),   // 엘리트+정예
                    ("엘리트 마법사",    new[]{("Enemy_EliteMage",1),("Enemy_Bat",3)}),
                    ("암흑 슬라임",      new[]{("Enemy_EliteDarkSlime",1),("Enemy_Mushroom",2)}),
                });

            // 층 2 — 유적 (해골50, 박쥐49, 미라70, 해골궁수70)
            CreateSpawnPatternTable("SpawnPatterns_F2",
                normalPatterns: new[]
                {
                    ("유적 수호대",   new[]{("Enemy_Skeleton",3),("Enemy_SkeletonArcher",2)}), // 3*50+2*70=290
                    ("박쥐 떼",       new[]{("Enemy_Bat",5),("Enemy_Skeleton",1)}),              // 5*49+50=295
                    ("미라 군단",     new[]{("Enemy_Mummy",3),("Enemy_Skeleton",2)}),           // 3*70+2*50=310
                    ("궁수 부대",     new[]{("Enemy_SkeletonArcher",3),("Enemy_Bat",2)}),       // 3*70+2*49=308
                    ("혼성 유적대",   new[]{("Enemy_Mummy",1),("Enemy_SkeletonArcher",1),("Enemy_Bat",2),("Enemy_Skeleton",1)}), // 70+70+98+50=288
                    ("뼈의 행진",     new[]{("Enemy_Skeleton",4),("Enemy_Mummy",1)}),           // 4*50+70=270
                },
                elitePatterns: new[]
                {
                    ("주술사 의식",     new[]{("Enemy_EliteGoblinShaman",1),("Enemy_Skeleton",3)}),
                    ("해골 대장",       new[]{("Enemy_EliteSkeletonCaptain",1),("Enemy_SkeletonArcher",2)}),
                    ("암흑 슬라임 강화", new[]{("Enemy_EliteDarkSlime",1),("Enemy_Mummy",2)}),
                });

            // 층 3 — 심연 (망령75, 그림자67, 악마병사82, 가고일68)
            CreateSpawnPatternTable("SpawnPatterns_F3",
                normalPatterns: new[]
                {
                    ("심연의 그림자", new[]{("Enemy_Shadow",4),("Enemy_Wraith",2)}),           // 4*67+2*75=418
                    ("악마 군단",     new[]{("Enemy_DemonSoldier",4),("Enemy_Shadow",2)}),     // 4*82+2*67=462
                    ("망령 떼",       new[]{("Enemy_Wraith",4),("Enemy_Shadow",1)}),           // 4*75+67=367
                    ("석조 수호대",   new[]{("Enemy_Gargoyle",2),("Enemy_DemonSoldier",2),("Enemy_Wraith",1)}), // 2*68+2*82+75=375
                    ("혼성 마군",     new[]{("Enemy_DemonSoldier",2),("Enemy_Wraith",1),("Enemy_Gargoyle",2)}), // 164+75+136=375
                    ("어둠의 침략",   new[]{("Enemy_Shadow",2),("Enemy_DemonSoldier",2),("Enemy_Gargoyle",1)}), // 134+164+68=366
                },
                elitePatterns: new[]
                {
                    ("악마 마법사",   new[]{("Enemy_EliteDemonMage",1),("Enemy_Wraith",3)}),
                    ("해골 대장 심화", new[]{("Enemy_EliteSkeletonCaptain",1),("Enemy_DemonSoldier",2)}),
                    ("주술사 강화",    new[]{("Enemy_EliteGoblinShaman",1),("Enemy_Gargoyle",2),("Enemy_Shadow",2)}),
                });

            Debug.Log($"[DataGenerator] 스폰 패턴 테이블 3개(층별) 생성 완료");
        }

        private static void CreateSpawnPatternTable(string fileName,
            (string name, (string enemyId, int count)[] enemies)[] normalPatterns,
            (string name, (string enemyId, int count)[] enemies)[] elitePatterns)
        {
            var path = $"{SPAWN_PATTERN_PATH}/{fileName}.asset";
            var table = GetOrCreateAsset<SpawnPatternTable>(path);
            table.name = fileName;

            SetPrivateField(table, "_normalPatterns", BuildPatterns(normalPatterns));
            SetPrivateField(table, "_elitePatterns", BuildPatterns(elitePatterns));

            EditorUtility.SetDirty(table);
        }

        private static EnemySpawnPattern[] BuildPatterns((string name, (string enemyId, int count)[] enemies)[] source)
        {
            var result = new List<EnemySpawnPattern>();

            foreach (var (patternName, entries) in source)
            {
                var pattern = new EnemySpawnPattern { patternName = patternName };
                var entryList = new List<EnemySpawnEntry>();

                foreach (var (enemyId, count) in entries)
                {
                    var enemyData = AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_PATH}/{enemyId}.asset");
                    if (enemyData == null)
                    {
                        Debug.LogWarning($"[DataGenerator] 스폰 패턴 적을 찾을 수 없음: {enemyId}");
                        continue;
                    }
                    entryList.Add(new EnemySpawnEntry { enemyData = enemyData, count = count });
                }

                pattern.enemies = entryList.ToArray();
                result.Add(pattern);
            }

            return result.ToArray();
        }

        #endregion
    }
}
#endif
