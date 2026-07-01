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
            // (파일명, 표시명, 설명, BehaviorTag[], 호환SkillType, 등급, 저주여부, 저주설명, 키워드[])
            // Phase BK: 행동 키워드 + 수치 키워드 분리. 빈 BehaviorTag[]는 일반 수치형 증강.
            var augments = new[]
            {
                // ── 일반 (Tier 1) ──
                Mk("Aug_CostDown", "비용 감소", "스킬 코스트가 1 감소합니다 (최소 0)",
                    BTags(BTag(BehaviorKeyword.CostDown, 1)), SkillType.Attack, 1, false, "",
                    Kw(KeywordType.CostAdd, -1)),

                Mk("Aug_PowerUp", "위력 증폭", "스킬 위력이 +3 증가합니다",
                    BTags(BTag(BehaviorKeyword.PowerUp, 3)), SkillType.Attack, 1, false, "",
                    Kw(KeywordType.PowerAdd, 3)),

                Mk("Aug_Lifesteal", "흡혈", "준 데미지의 절반만큼 자신의 HP를 회복합니다",
                    BTags(BTag(BehaviorKeyword.Lifesteal, 1)), SkillType.Attack, 1, false, ""),

                Mk("Aug_QuickDraw", "신속", "가중치가 0이 되어 무조건 뽑힙니다 (위력 절반)",
                    BTags(BTag(BehaviorKeyword.QuickDraw, 1)), SkillType.Attack, 1, false, "",
                    Kw(KeywordType.PowerMul, 0.5f),
                    Kw(KeywordType.DrawWeightOverride, 0f)),

                Mk("Aug_Lingering", "잔류", "상태이상 지속시간이 +2턴 증가합니다",
                    BTags(BTag(BehaviorKeyword.Lingering, 2)), SkillType.Debuff, 1, false, "",
                    Kw(KeywordType.DurationAdd, 2)),

                Mk("Aug_VenomTouch", "맹독", "공격 시 중독 1스택을 추가로 부여합니다",
                    BTags(BTag(BehaviorKeyword.VenomTouch, 1)), SkillType.Attack, 1, false, ""),

                Mk("Aug_BurningTouch", "화염", "공격 시 화상 1스택을 추가로 부여합니다",
                    BTags(BTag(BehaviorKeyword.BurningTouch, 1)), SkillType.Attack, 1, false, ""),

                Mk("Aug_FreezeTouch", "빙결", "공격 시 빙결 1스택을 추가로 부여합니다",
                    BTags(BTag(BehaviorKeyword.FreezeTouch, 1)), SkillType.Attack, 1, false, ""),

                Mk("Aug_ShieldBonus", "철벽", "쉴드 효과가 2배가 됩니다",
                    BTags(BTag(BehaviorKeyword.ShieldBonus, 1)), SkillType.Shield, 1, false, "",
                    Kw(KeywordType.ShieldMul, 2f)),

                Mk("Aug_HealBonus", "치유 강화", "힐 효과가 2배가 됩니다",
                    BTags(BTag(BehaviorKeyword.HealBonus, 1)), SkillType.Heal, 1, false, "",
                    Kw(KeywordType.HealMul, 2f)),

                // ── 희귀 (Tier 2) ──
                Mk("Aug_Spread", "확산", "단일 대상 스킬이 광역으로 변합니다 (위력 유지)",
                    BTags(BTag(BehaviorKeyword.Spread, 1)), SkillType.Attack, 2, false, ""),

                Mk("Aug_Pierce", "관통", "쉴드와 방어력을 완전히 무시합니다",
                    BTags(BTag(BehaviorKeyword.Pierce, 1)), SkillType.Attack, 2, false, ""),

                Mk("Aug_Chain", "연쇄", "타격 후 무작위 적 1명에게 위력 100%로 연쇄합니다",
                    BTags(BTag(BehaviorKeyword.Chain, 1)), SkillType.Attack, 2, false, ""),

                Mk("Aug_Bounce", "바운스", "무작위 적에게 2회 추가 타격합니다 (중복 허용, 위력 유지)",
                    BTags(BTag(BehaviorKeyword.Bounce, 2)), SkillType.Attack, 2, false, ""),

                Mk("Aug_MultiHit", "연타", "동일 대상에게 2회 추가 타격합니다 (위력 유지)",
                    BTags(BTag(BehaviorKeyword.MultiHit, 2)), SkillType.Attack, 2, false, ""),

                Mk("Aug_Explosion", "폭발", "광역 타격 후 무작위 2명에게 추가 타격합니다 (위력 유지)",
                    BTags(BTag(BehaviorKeyword.Explosion, 2)), SkillType.Attack, 2, false, ""),

                Mk("Aug_Intensify", "강화", "버프/디버프 효과가 2배가 됩니다",
                    BTags(BTag(BehaviorKeyword.Intensify, 1)), SkillType.Buff, 2, false, "",
                    Kw(KeywordType.EffectMul, 2f)),

                // ── 전설 (Tier 3) ──
                Mk("Aug_HeavyHit", "중격", "위력이 2배가 되지만 코스트가 +1 증가합니다",
                    BTags(BTag(BehaviorKeyword.HeavyHit, 1)), SkillType.Attack, 3, false, "",
                    Kw(KeywordType.PowerMul, 2f),
                    Kw(KeywordType.CostAdd, 1)),

                Mk("Aug_Execution", "사형 선고", "HP 10 이하의 적을 즉사시킵니다 (보스 제외)",
                    BTags(BTag(BehaviorKeyword.Execution, 10)), SkillType.Attack, 3, false, ""),

                // ── 저주 (강력 + 페널티) ──
                Mk("Aug_BloodPact", "피의 계약", "위력이 +5 증가합니다",
                    BTags(BTag(BehaviorKeyword.BloodPact, 1)), SkillType.Attack, 2, true, "매턴 HP 2 감소",
                    Kw(KeywordType.PowerAdd, 5),
                    Kw(KeywordType.HPPerTurn, -2, KeywordTrigger.Passive)),

                Mk("Aug_GlassCannon", "유리 대포", "위력이 +8 증가합니다",
                    BTags(BTag(BehaviorKeyword.GlassCannon, 1)), SkillType.Attack, 3, true, "받는 피해 2배",
                    Kw(KeywordType.PowerAdd, 8),
                    Kw(KeywordType.DamageTakenMul, 2f)),

                Mk("Aug_Reaper", "수확자", "적을 처치할 때 HP 10을 회복합니다",
                    BTags(BTag(BehaviorKeyword.Reaper, 10)), SkillType.Attack, 2, true, "코스트 +1",
                    Kw(KeywordType.OnKillHeal, 10),
                    Kw(KeywordType.CostAdd, 1)),

                Mk("Aug_AOEAuto", "파동", "단일 대상 스킬이 자동으로 광역이 됩니다 (위력 유지)",
                    BTags(BTag(BehaviorKeyword.AOEAuto, 1)), SkillType.Attack, 2, true, "코스트 +2",
                    Kw(KeywordType.CostAdd, 2)),

                Mk("Aug_Berserk", "광폭", "HP 절반 이하일 때 위력이 2배가 됩니다",
                    BTags(BTag(BehaviorKeyword.Berserk, 1)), SkillType.Attack, 3, true, "HP 50% 이하일 때만 발동"),

                // ── Phase ARCH-4 신규 (상황/상태 기반 위력 보상) ──
                Mk("Aug_Bulwark", "방패벽", "쉴드 보유 시 위력 +5",
                    BTags(BTag(BehaviorKeyword.Bulwark, 5)), SkillType.Attack, 1, false, ""),

                Mk("Aug_Dominance", "지배", "적 현재체력이 나보다 낮을 때 위력 +4",
                    BTags(BTag(BehaviorKeyword.Dominance, 4)), SkillType.Attack, 1, false, ""),

                Mk("Aug_FirstBlood", "첫 피", "풀피 적에게 위력 +4",
                    BTags(BTag(BehaviorKeyword.FirstBlood, 4)), SkillType.Attack, 2, false, ""),

                Mk("Aug_Cull", "도축", "체력이 절반 이하인 적에게 위력 +6",
                    BTags(BTag(BehaviorKeyword.Cull, 6)), SkillType.Attack, 2, false, ""),

                Mk("Aug_Desperation", "절박", "잃은 체력 5당 위력 +1 (다칠수록 강해짐)",
                    BTags(BTag(BehaviorKeyword.Desperation, 5)), SkillType.Attack, 2, false, ""),

                Mk("Aug_GiantSlayer", "거인살해자", "최대체력 100 이상인 적에게 위력 +6 (엘리트/보스 특화)",
                    BTags(BTag(BehaviorKeyword.GiantSlayer, 6)), SkillType.Attack, 3, false, ""),

                Mk("Aug_AllIn", "올인", "사용 후 AP가 0이면 추가 위력 +8 (마지막 스킬 보상)",
                    BTags(BTag(BehaviorKeyword.AllIn, 8)), SkillType.Attack, 3, false, ""),

                Mk("Aug_Bounty", "현상금", "이 스킬로 적 처치 시 HP 회복 보상 (rank=3)",
                    BTags(BTag(BehaviorKeyword.Bounty, 3)), SkillType.Attack, 2, false, ""),

                // ── Phase ARCH-5 신규 (사용 누적 — UsesThisBattle 기반) ──
                Mk("Aug_Momentum", "관성", "이 전투에서 매 사용 시 위력 +2 (누적)",
                    BTags(BTag(BehaviorKeyword.Momentum, 2)), SkillType.Attack, 2, false, ""),

                Mk("Aug_Mastery", "숙련", "이 전투에서 매 사용 시 코스트 -1 (누적, 최소 0)",
                    BTags(BTag(BehaviorKeyword.Mastery, 1)), SkillType.Attack, 3, false, ""),

                // ── Phase ARCH-4/5 저주 (페널티 동반) ──
                Mk("Aug_Wound", "상처", "기본 위력은 높으나 잃은 체력 5당 위력 -1 (다치면 약해짐)",
                    BTags(BTag(BehaviorKeyword.Wound, 5)), SkillType.Attack, 2, true, "잃은 체력 비례 위력 감소",
                    Kw(KeywordType.PowerAdd, 3)),

                Mk("Aug_Fatigue", "피로", "매 사용 시 위력 -2 (누적). 대신 기본 위력 +3",
                    BTags(BTag(BehaviorKeyword.Fatigue, 2)), SkillType.Attack, 2, true, "반복 사용 시 약화",
                    Kw(KeywordType.PowerAdd, 3)),

                Mk("Aug_Escalation", "에스컬레이션", "매 사용 시 코스트 +1 (누적). 대신 기본 위력 +3",
                    BTags(BTag(BehaviorKeyword.Escalation, 1)), SkillType.Attack, 2, true, "반복 사용 시 비싸짐",
                    Kw(KeywordType.PowerAdd, 3)),
            };

            foreach (var (fileName, augmentName, description, behaviors, compatSkillType, tier, isCursed, curseDesc, keywords) in augments)
            {
                var path = $"{AUGMENT_PATH}/{fileName}.asset";
                var augment = GetOrCreateAsset<AugmentData>(path);
                augment.name = fileName;

                SetPrivateField(augment, "_augmentName", augmentName);
                SetPrivateField(augment, "_description", description);
                SetPrivateField(augment, "_behaviors", behaviors);
                SetPrivateField(augment, "_compatibleSkillType", compatSkillType);
                SetPrivateField(augment, "_tier", tier);
                SetPrivateField(augment, "_isCursed", isCursed);
                SetPrivateField(augment, "_curseDescription", curseDesc);
                SetPrivateField(augment, "_keywords", keywords);

                EditorUtility.SetDirty(augment);
            }

            // Phase BK: 기존 Aug_Drain.asset 제거 (Aug_Lifesteal로 교체됨, GUID 재발급 감수)
            DeleteLegacyAsset("Aug_Drain");

            Debug.Log($"[DataGenerator] 증강 데이터 {augments.Length}개 생성 완료 (Phase BK BehaviorKeyword)");
        }

        /// <summary>Phase BK: 구식 에셋 삭제 헬퍼 (Aug_Drain 등).</summary>
        private static void DeleteLegacyAsset(string fileName)
        {
            string assetPath = $"{AUGMENT_PATH}/{fileName}.asset";
            string metaPath = assetPath + ".meta";
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null)
            {
                bool ok = AssetDatabase.DeleteAsset(assetPath);
                if (ok) Debug.Log($"[DataGenerator] 레거시 에셋 삭제: {fileName}");
                else Debug.LogWarning($"[DataGenerator] 레거시 에셋 삭제 실패: {fileName}");
            }
            // .meta 파일이 남아있으면 수동 제거
            if (System.IO.File.Exists(metaPath))
            {
                System.IO.File.Delete(metaPath);
            }
        }

        /// <summary>증강 데이터 튜플 생성 헬퍼 (Phase BK)</summary>
        private static (string, string, string, BehaviorTag[], SkillType, int, bool, string, KeywordEntry[])
            Mk(string fileName, string name, string desc, BehaviorTag[] behaviors, SkillType compat, int tier, bool cursed, string curseDesc,
               params KeywordEntry[] keywords)
        {
            return (fileName, name, desc, behaviors, compat, tier, cursed, curseDesc, keywords);
        }

        /// <summary>BehaviorTag 배열 생성 헬퍼 (params 단일 래퍼)</summary>
        private static BehaviorTag[] BTags(params BehaviorTag[] tags) => tags;

        /// <summary>BehaviorTag 생성 헬퍼</summary>
        private static BehaviorTag BTag(BehaviorKeyword keyword, int rank = 0)
        {
            return new BehaviorTag(keyword, rank);
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
