#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Event;

namespace TeamLog.Editor
{
    /// <summary>
    /// DataGenerator.Stages — 스테이지 테마 에셋 자동 생성 (StageDesign.md 기반)
    /// 진입점/스킬/캐릭터/패턴/유틸리티: DataGenerator.cs
    /// 증강 데이터/스폰 패턴: DataGenerator.Augments.cs
    /// 이벤트 데이터: DataGenerator.Events.cs
    /// 유물 데이터: DataGenerator.Relics.cs
    /// 팔레트 (UI/오디오/VFX): DataGenerator.Palettes.cs
    /// 스테이지 테마: DataGenerator.Stages.cs (Phase E3: 테마별 이벤트 24개 포함)
    /// </summary>
    public static partial class DataGenerator
    {
        private const string STAGE_PATH = "Assets/03.Data/Stages";

        /// <summary>
        /// 12개 스테이지 테마 에셋 생성 — StageDesign.md 기준
        /// 4스테이지 × 3테마 = 81가지 조합 (런 시작 시 스테이지마다 무작위 1개 채택).
        ///
        /// 적 풀 전략 (Phase 7D):
        /// - 신규 적 에셋 생성 없이, 기존 F1/F2/F3 적 풀을 재조합하여 테마별 차별화
        /// - 스테이지 4는 GetFloorScaling(2.0f)으로 자동 난이도 상승
        /// - 테마 키워드/설명은 StageDesign 그대로 반영 (UI 노출용)
        ///
        /// 테마별 이벤트 (Phase E3):
        /// - 12테마 × 2개 = 24개 전용 이벤트
        /// - 각 테마의 키워드와 연계된 딜레마
        /// - ExclusiveThemeId로 해당 테마에서만 등장
        /// </summary>
        [MenuItem("TeamLog/Generate Stage Themes", false, 110)]
        public static void GenerateStageThemes()
        {
            EnsureFolder(STAGE_PATH);

            // Phase E3: 테마별 전용 이벤트 24개 먼저 생성
            GenerateThemeSpecificEvents();

            // ── Stage 1: 튜토리얼 (F1 적 풀 기반) ──
            // ★ Phase GF (2026-07-20): 잿빛 숲 테마 고유 적 풀로 재정의.
            // 기존 Slime/Goblin/Wolf/Mushroom 공통 풀 → 잿빛 숲 전용 4종으로 교체.
            // 기존 EliteKnight/EliteMage/EliteDarkSlime → Witherwarden/CompostKing 신규 엘리트.
            // 키워드 "재생 + 독"이 일반/엘리트/보스 전체에 관통.
            CreateTheme(
                themeId: "GreyForest",
                displayName: "잿빛 숲",
                stageNumber: 1,
                normals: new[] { "Enemy_AshwoodWisp", "Enemy_BlightbedCrawler", "Enemy_Mossbulwark", "Enemy_Sporecaller" },
                elites: new[] { "Enemy_EliteWitherwarden", "Enemy_EliteCompostKing" },
                boss: "Enemy_BossVerdantTerror",
                spawnTable: "SpawnPatterns_GreyForest",
                keywords: new[] { "재생", "독" },
                desc: "재생과 독의 악순환. 매 런 다른 공략법 요구 — 한 방에 잡거나 정화가 필수.",
                themeEventIds: new[] { "Event_T_GF_MistMerchant", "Event_T_GF_RegenSpring" });

            CreateTheme(
                themeId: "FrostedPass",
                displayName: "서리 고개",
                stageNumber: 1,
                normals: new[] { "Enemy_Wolf", "Enemy_Mushroom", "Enemy_Slime", "Enemy_Goblin" },
                elites: new[] { "Enemy_EliteKnight", "Enemy_EliteMage", "Enemy_EliteDarkSlime" },
                boss: "Enemy_BossFrostMonarch",
                spawnTable: "SpawnPatterns_F1",
                keywords: new[] { "둔화", "빙결" },
                desc: "빙결과 둔화로 AP를 압박하는 튜토리얼 변형.",
                themeEventIds: new[] { "Event_T_FP_FrozenTraveler", "Event_T_FP_IceShardTrade" });

            CreateTheme(
                themeId: "SunscorchedPlains",
                displayName: "모래 평원",
                stageNumber: 1,
                normals: new[] { "Enemy_Goblin", "Enemy_Wolf", "Enemy_Mushroom", "Enemy_Slime" },
                elites: new[] { "Enemy_EliteKnight", "Enemy_EliteMage", "Enemy_EliteDarkSlime" },
                boss: "Enemy_BossSandLeviathan",
                spawnTable: "SpawnPatterns_F1",
                keywords: new[] { "은폐", "회피" },
                desc: "회피와 은폐로 명중 관리를 요구하는 튜토리얼 변형.",
                themeEventIds: new[] { "Event_T_SP_Mirage", "Event_T_SP_SandstormShelter" });

            // ── Stage 2: 체력 관리 (F2 적 풀 기반) ──
            CreateTheme(
                themeId: "CrimsonChapel",
                displayName: "혈련 예배당",
                stageNumber: 2,
                normals: new[] { "Enemy_Bat", "Enemy_Mummy", "Enemy_Skeleton", "Enemy_SkeletonArcher" },
                elites: new[] { "Enemy_EliteKnight", "Enemy_EliteMage", "Enemy_EliteDarkSlime" },
                boss: "Enemy_BossBloodQueen",
                spawnTable: "SpawnPatterns_F2",
                keywords: new[] { "흡혈", "부활" },
                desc: "흡혈과 부활로 HP를 뺏기는 체력 관리 스테이지.",
                themeEventIds: new[] { "Event_T_CC_BloodFountain", "Event_T_CC_VampireDeal" });

            CreateTheme(
                themeId: "RotbloomBog",
                displayName: "부패 늪",
                stageNumber: 2,
                normals: new[] { "Enemy_Mushroom", "Enemy_Slime", "Enemy_Bat", "Enemy_Mummy" },
                elites: new[] { "Enemy_EliteKnight", "Enemy_EliteMage", "Enemy_EliteDarkSlime" },
                boss: "Enemy_BossPlagueLord",
                spawnTable: "SpawnPatterns_F2",
                keywords: new[] { "독", "전염" },
                desc: "독과 전염으로 도트 데미지를 입히는 늪지대.",
                themeEventIds: new[] { "Event_T_RB_PlagueDoctor", "Event_T_RB_Bogwitch" });

            CreateTheme(
                themeId: "RuinedTemple",
                displayName: "유적 잔해",
                stageNumber: 2,
                normals: new[] { "Enemy_Skeleton", "Enemy_Bat", "Enemy_Mummy", "Enemy_SkeletonArcher" },
                elites: new[] { "Enemy_EliteKnight", "Enemy_EliteMage", "Enemy_EliteDarkSlime" },
                boss: "Enemy_BossLichKing",
                spawnTable: "SpawnPatterns_F2",
                keywords: new[] { "언데드", "저주" },
                desc: "언데드와 저주로 상태이상 정화의 가치를 학습.",
                themeEventIds: new[] { "Event_T_RT_CursedSarcophagus", "Event_T_RT_LichLibrary" });

            // ── Stage 3: 자원 압박 (F3 적 풀 기반) ──
            CreateTheme(
                themeId: "AbyssalTrench",
                displayName: "심연 해구",
                stageNumber: 3,
                normals: new[] { "Enemy_Wraith", "Enemy_Gargoyle", "Enemy_Shadow", "Enemy_DemonSoldier" },
                elites: new[] { "Enemy_EliteGoblinShaman", "Enemy_EliteSkeletonCaptain", "Enemy_EliteDemonMage" },
                boss: "Enemy_BossKraken",
                spawnTable: "SpawnPatterns_F3",
                keywords: new[] { "흡수", "속박" },
                desc: "흡수와 속박으로 쉴드 운영을 압박하는 심연.",
                themeEventIds: new[] { "Event_T_AT_DrownedChest", "Event_T_AT_KrakenTentacle" });

            CreateTheme(
                themeId: "Stormpeak",
                displayName: "번개 봉우리",
                stageNumber: 3,
                normals: new[] { "Enemy_Gargoyle", "Enemy_Shadow", "Enemy_Wraith", "Enemy_DemonSoldier" },
                elites: new[] { "Enemy_EliteGoblinShaman", "Enemy_EliteSkeletonCaptain", "Enemy_EliteDemonMage" },
                boss: "Enemy_BossStormLord",
                spawnTable: "SpawnPatterns_F3",
                keywords: new[] { "기절", "연쇄" },
                desc: "기절과 연쇄 공격으로 행동 차단을 시도하는 봉우리.",
                themeEventIds: new[] { "Event_T_ST_StruckByLightning", "Event_T_ST_StormRitual" });

            CreateTheme(
                themeId: "ShadowsGlade",
                displayName: "그림자 골짜기",
                stageNumber: 3,
                normals: new[] { "Enemy_Shadow", "Enemy_Bat", "Enemy_Wraith", "Enemy_Gargoyle" },
                elites: new[] { "Enemy_EliteGoblinShaman", "Enemy_EliteSkeletonCaptain", "Enemy_EliteDemonMage" },
                boss: "Enemy_BossVoidWalker",
                spawnTable: "SpawnPatterns_F3",
                keywords: new[] { "은신", "회피" },
                desc: "은신과 회피로 예측을 어렵게 만드는 골짜기.",
                themeEventIds: new[] { "Event_T_SG_FadeEcho", "Event_T_SG_BlindSeer" });

            // ── Stage 4: 클라이맥스 (F3 적 풀 + 마왕, GetFloorScaling 2.0) ──
            CreateTheme(
                themeId: "EmberThrone",
                displayName: "불꽃왕좌",
                stageNumber: 4,
                normals: new[] { "Enemy_DemonSoldier", "Enemy_Mummy", "Enemy_Wraith", "Enemy_Gargoyle" },
                elites: new[] { "Enemy_EliteDemonMage", "Enemy_EliteSkeletonCaptain", "Enemy_EliteGoblinShaman" },
                boss: "Enemy_BossFlameEmperor",
                spawnTable: "SpawnPatterns_F3",
                keywords: new[] { "화염", "폭발" },
                desc: "화염과 폭발로 고데미지를 입히는 클라이맥스. 모든 시스템 통합 운영 필요.",
                themeEventIds: new[] { "Event_T_ET_SalamanderPact", "Event_T_ET_EmberForge" });

            CreateTheme(
                themeId: "EternalTundra",
                displayName: "영원동토",
                stageNumber: 4,
                normals: new[] { "Enemy_Wraith", "Enemy_Gargoyle", "Enemy_DemonSoldier", "Enemy_Shadow" },
                elites: new[] { "Enemy_EliteDemonMage", "Enemy_EliteSkeletonCaptain", "Enemy_EliteGoblinShaman" },
                boss: "Enemy_BossIceGoddess",
                spawnTable: "SpawnPatterns_F3",
                keywords: new[] { "빙결", "봉쇄" },
                desc: "빙결과 행동 봉쇄로 파티를 굳히는 영구 동토.",
                themeEventIds: new[] { "Event_T_ETu_FrozenHero", "Event_T_ETu_IceQueenRiddle" });

            CreateTheme(
                themeId: "DemonCitadel",
                displayName: "마왕성 심장",
                stageNumber: 4,
                normals: new[] { "Enemy_DemonSoldier", "Enemy_Shadow", "Enemy_Wraith", "Enemy_Gargoyle" },
                elites: new[] { "Enemy_EliteDemonMage", "Enemy_EliteSkeletonCaptain", "Enemy_EliteGoblinShaman" },
                boss: "Enemy_BossArchdemon",
                spawnTable: "SpawnPatterns_F3",
                keywords: new[] { "소환", "다중페이즈" },
                desc: "소환과 다중 페이즈로 지속적인 전멸 위협을 주는 마왕성 심장.",
                themeEventIds: new[] { "Event_T_DC_DemonContract", "Event_T_DC_LegionAmbush" });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DataGenerator] 스테이지 테마 12종 + 테마별 이벤트 24개 생성 완료 (Phase E3)");
        }

        #region Phase E3: 테마별 전용 이벤트 24개

        /// <summary>
        /// 12테마 × 2개 = 24개 테마 전용 이벤트 생성.
        /// 각 이벤트는 ExclusiveThemeId로 해당 테마에서만 등장.
        /// 키워드와 연계된 딜레마로 테마 분위기를 이벤트로 전달.
        /// </summary>
        private static void GenerateThemeSpecificEvents()
        {
            // ===== Stage 1 — 튜토리얼 =====

            // GreyForest (잿빛 숲) — 재생, 도적
            CreateEvent("Event_T_GF_MistMerchant", "안개 속 상인",
                "짙은 안개 속에서 정체불명의 상인이 나타납니다. 유물을 싸게 팔지만 어딘가 수상합니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "싼 유물을 산다 (30 골드)",
                        ChoiceDescription = "70% 일반 유물 / 30% 저주받은 유물",
                        MinGoldRequired = 30,
                        Outcome = new EventOutcome
                        {
                            ResultText = "상인이 미소 짓습니다...",
                            GoldChange = -30,
                            GiveRandomItem = true
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "거절한다",
                        ChoiceDescription = "안전을 선택합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "상인이 안개 속으로 사라졌습니다. 그 자리에 15 골드가 떨어져 있었습니다.",
                            GoldChange = 15
                        }
                    }
                },
                exclusiveThemeId: "GreyForest");

            CreateEvent("Event_T_GF_RegenSpring", "재생의 샘",
                "오래된 정령이 빛나는 샘물을 가꾸고 있습니다. 깊은 마실수록 더 큰 치유를 얻을 수 있습니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "깊이 마신다",
                        ChoiceDescription = "HP 30% 회복 + 재생 3턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "정령의 축복을 받았습니다! HP 30% 회복, 3턴간 재생 효과.",
                            HPPercentChange = 30,
                            ApplyStatusEffect = StatusEffectType.Regeneration,
                            StatusEffectDuration = 3,
                            StatusEffectValue = 5
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "조금만 마신다",
                        ChoiceDescription = "HP 10% 회복 (안전)",
                        Outcome = new EventOutcome
                        {
                            ResultText = "가볍게 한 모금 마셨습니다. HP 10% 회복.",
                            HPPercentChange = 10
                        }
                    }
                },
                exclusiveThemeId: "GreyForest");

            // FrostedPass (서리 고개) — 둔화, 빙결
            CreateEvent("Event_T_FP_FrozenTraveler", "얼어붙은 여행자",
                "길가에 동사 직전의 여행자가 쓰러져 있습니다. 도와주면 보상을 줄 것 같지만, 체온을 뺏길 수도 있습니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "도와준다",
                        ChoiceDescription = "HP -10%, 파티 영구 DEF +1",
                        Outcome = new EventOutcome
                        {
                            ResultText = "여행자를 구했지만 추위에 데미지를 입었습니다. 감사의 의미로 방어 기술을 배웠습니다!",
                            HPPercentChange = -10,
                            PermanentDefBonus = 1
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "무시한다",
                        ChoiceDescription = "25 골드를 발견합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "차갑게 지나칩니다. 여행자의 주머니에서 25 골드가 떨어졌습니다.",
                            GoldChange = 25
                        }
                    }
                },
                exclusiveThemeId: "FrostedPass");

            CreateEvent("Event_T_FP_IceShardTrade", "얼음 조각 거래",
                "빙석 골렘의 파편이 반짝입니다. 보석상으로 가치가 있지만, 녹이면 정령의 힘을 얻을 수도 있습니다.",
                TeamLog.Event.EventType.Treasure,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "보석으로 판다",
                        ChoiceDescription = "50 골드 획득",
                        Outcome = new EventOutcome
                        {
                            ResultText = "아름다운 보석으로 50 골드를 받았습니다!",
                            GoldChange = 50
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "녹여 마신다",
                        ChoiceDescription = "HP 20% 회복, 빙결 1턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "차가운 정령의 힘이 스며듭니다. HP 20% 회복되었지만 잠시 얼어붙었습니다.",
                            HPPercentChange = 20,
                            ApplyStatusEffect = StatusEffectType.Freeze,
                            StatusEffectDuration = 1,
                            StatusEffectValue = 0
                        }
                    }
                },
                exclusiveThemeId: "FrostedPass");

            // SunscorchedPlains (모래 평원) — 은폐, 회피
            CreateEvent("Event_T_SP_Mirage", "신기루",
                "잠깐 오아시스가 보이다가 사라집니다. 다가가면 진짜일지도 모르지만, 위험할 수도 있습니다.",
                TeamLog.Event.EventType.Treasure,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "다가간다",
                        ChoiceDescription = "50% 파티 풀회복 / 50% HP -20%",
                        Outcome = new EventOutcome
                        {
                            ResultText = "신기루에 다가섭니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "진짜 오아시스였습니다! 파티 HP가 가득 찼습니다.",
                                    HPPercentChange = 100
                                },
                                new EventOutcome
                                {
                                    ResultText = "환상이었습니다! 타는 듯한 더위에 HP 20% 감소.",
                                    HPPercentChange = -20
                                }
                            }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "무시한다",
                        ChoiceDescription = "안전하게 지나갑니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "신중하게 지나칩니다. 작은 동전을 발견했습니다.",
                            GoldChange = 10
                        }
                    }
                },
                exclusiveThemeId: "SunscorchedPlains");

            CreateEvent("Event_T_SP_SandstormShelter", "모래폭풍 대피소",
                "거친 모래폭풍이 다가옵니다. 근처 작은 동굴에 숨을 수 있지만, 누군가 이미 먼저 숨어있을지 모릅니다.",
                TeamLog.Event.EventType.Trap,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "동굴에 숨는다",
                        ChoiceDescription = "50% 생존자 무기 획득 / 50% 도적 습격 (HP -15%)",
                        Outcome = new EventOutcome
                        {
                            ResultText = "동굴에 들어갑니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "친절한 사막의 암살자가 무기를 공유해줬습니다! 30 골드 획득.",
                                    GoldChange = 30
                                },
                                new EventOutcome
                                {
                                    ResultText = "도적이 이미 숨어있었습니다! 습격당해 HP 15% 감소.",
                                    HPPercentChange = -15
                                }
                            }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "폭풍을 뚫고간다",
                        ChoiceDescription = "HP -10%, 25 골드 발견",
                        Outcome = new EventOutcome
                        {
                            ResultText = "모래폭풍을 견뎠습니다. HP 10% 감소했지만 숨겨진 보물을 발견했습니다.",
                            HPPercentChange = -10,
                            GoldChange = 25
                        }
                    }
                },
                exclusiveThemeId: "SunscorchedPlains");

            // ===== Stage 2 — 체력 관리 =====

            // CrimsonChapel (혈련 예배당) — 흡혈, 부활
            CreateEvent("Event_T_CC_BloodFountain", "피의 분수",
                "예배당 중앙에 붉은 분수가 솟습니다. 마시면 힘이 넘치지만, 저주에 빠질 수도 있습니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "피를 마신다",
                        ChoiceDescription = "50% 영구 ATK +3 / 50% AttackDown 3턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "피를 들이킵니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "강력한 힘이 끓어올랐습니다! 파티 ATK 영구 +3.",
                                    PermanentAtkBonus = 3
                                },
                                new EventOutcome
                                {
                                    ResultText = "저주가 뿌리내렸습니다! 파티 AttackDown 3턴.",
                                    ApplyStatusEffect = StatusEffectType.AttackDown,
                                    StatusEffectDuration = 3,
                                    StatusEffectValue = 5
                                }
                            }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "기도만 한다",
                        ChoiceDescription = "HP 10% 회복",
                        Outcome = new EventOutcome
                        {
                            ResultText = "경건하게 기도드립니다. HP 10% 회복.",
                            HPPercentChange = 10
                        }
                    }
                },
                exclusiveThemeId: "CrimsonChapel");

            CreateEvent("Event_T_CC_VampireDeal", "뱀파이어의 거래",
                "고풍스러운 뱀파이어 로드가 거래를 제안합니다. 골드를 받고 강력한 힘을 주겠다고 합니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "거래를 수락한다 (70 골드)",
                        ChoiceDescription = "70 골드 받고 파티 영구 ATK +4",
                        MinGoldRequired = 0,
                        Outcome = new EventOutcome
                        {
                            ResultText = "강력한 흡혈의 힘이 주입됩니다! 70 골드를 받고 파티 ATK 영구 +4.",
                            GoldChange = 70,
                            PermanentAtkBonus = 4
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "거절한다",
                        ChoiceDescription = "강제로 HP -15% (뱀파이어의 분노)",
                        Outcome = new EventOutcome
                        {
                            ResultText = "거절하자 분노한 뱀파이어가 공격했습니다! HP 15% 감소.",
                            HPPercentChange = -15
                        }
                    }
                },
                exclusiveThemeId: "CrimsonChapel");

            // RotbloomBog (부패 늪) — 독, 전염
            CreateEvent("Event_T_RB_PlagueDoctor", "역병 의사",
                "가면을 쓴 의사가 해독제를 판매합니다. 정가는 30 골드지만, 무료 샘플은 50% 확률로 역병을 퍼뜨립니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "정가에 구매 (30 골드)",
                        ChoiceDescription = "독/화상 정화 + 힐 15%",
                        MinGoldRequired = 30,
                        Outcome = new EventOutcome
                        {
                            ResultText = "진짜 해독제였습니다! 독과 화상이 정화되고 HP 15% 회복.",
                            GoldChange = -30,
                            HPPercentChange = 15
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "무료 샘플을 받는다",
                        ChoiceDescription = "50% HP 25% 회복 / 50% 역병 5턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "샘플을 마십니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "효과가 있었습니다! HP 25% 회복.",
                                    HPPercentChange = 25
                                },
                                new EventOutcome
                                {
                                    ResultText = "역병이었습니다! 파티 전원 독 5턴.",
                                    ApplyStatusEffect = StatusEffectType.Poison,
                                    StatusEffectDuration = 5,
                                    StatusEffectValue = 3
                                }
                            }
                        }
                    }
                },
                exclusiveThemeId: "RotbloomBog");

            CreateEvent("Event_T_RB_Bogwitch", "늪 마녀",
                "늪 마녀가 거래를 제안합니다. 유물을 주거나 골드를 줄 수 있지만, 둘 다 대가가 있습니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "유물을 받는다",
                        ChoiceDescription = "유물 획득, 대가로 파티 독 5턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "마녀가 유물을 건넵니다. 저주로 파티 전원 독 5턴.",
                            GiveRandomItem = true,
                            ApplyStatusEffect = StatusEffectType.Poison,
                            StatusEffectDuration = 5,
                            StatusEffectValue = 2
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "골드를 받는다",
                        ChoiceDescription = "50 골드, 대가로 HP -10%",
                        Outcome = new EventOutcome
                        {
                            ResultText = "마녀가 50 골드를 줍니다. 체력을 조금 빼앗겼습니다.",
                            GoldChange = 50,
                            HPPercentChange = -10
                        }
                    }
                },
                exclusiveThemeId: "RotbloomBog");

            // RuinedTemple (유적 잔해) — 언데드, 저주
            CreateEvent("Event_T_RT_CursedSarcophagus", "저주받은 석관",
                "고대 석관이 반짝입니다. 열면 보물이 있을 수도 있지만, 저주가 깃들어 있을 수도 있습니다.",
                TeamLog.Event.EventType.Treasure,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "석관을 연다",
                        ChoiceDescription = "50% 유물 / 50% AttackDown 5턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "석관을 엽니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "고대 유물이었습니다! 유물 획득.",
                                    GiveRandomItem = true
                                },
                                new EventOutcome
                                {
                                    ResultText = "저주가 발동했습니다! 파티 AttackDown 5턴.",
                                    ApplyStatusEffect = StatusEffectType.AttackDown,
                                    StatusEffectDuration = 5,
                                    StatusEffectValue = 5
                                }
                            }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "그대로 둔다",
                        ChoiceDescription = "경건하게 떠납니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "석관을 그대로 둡니다. 작은 공물을 발견했습니다.",
                            GoldChange = 20
                        }
                    }
                },
                exclusiveThemeId: "RuinedTemple");

            CreateEvent("Event_T_RT_LichLibrary", "리치의 도서관",
                "리치의 금서가 보관된 도서관을 발견했습니다. 읽으면 강력한 지식을 얻을 수 있지만 위험합니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "금서를 읽는다",
                        ChoiceDescription = "영구 ATK +2, 대가로 화상 3턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "금서의 지식이 스며듭니다! 파티 ATK 영구 +2, 하지만 영혼이 타들어갑니다.",
                            PermanentAtkBonus = 2,
                            ApplyStatusEffect = StatusEffectType.Burn,
                            StatusEffectDuration = 3,
                            StatusEffectValue = 2
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "금서를 태운다",
                        ChoiceDescription = "40 골드 발견",
                        Outcome = new EventOutcome
                        {
                            ResultText = "금서를 태웁니다. 잿더미에서 40 골드가 나왔습니다.",
                            GoldChange = 40
                        }
                    }
                },
                exclusiveThemeId: "RuinedTemple");

            // ===== Stage 3 — 자원 압박 =====

            // AbyssalTrench (심연 해구) — 흡수, 속박
            CreateEvent("Event_T_AT_DrownedChest", "익사자의 상자",
                "심해에 가라앉은 상자를 건집니다. 열면 보물이 있지만, 익사자의 원한도 함께 나올 수 있습니다.",
                TeamLog.Event.EventType.Treasure,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "상자를 연다",
                        ChoiceDescription = "쉴드 50 + 60 골드 / AttackDown 3턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "쉴드 50과 60 골드를 얻었지만, 익사자의 원한이 파티를 짓누릅니다. AttackDown 3턴.",
                            GoldChange = 60,
                            ApplyStatusEffect = StatusEffectType.Shield,
                            StatusEffectDuration = 1,
                            StatusEffectValue = 50
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "도로 던진다",
                        ChoiceDescription = "안전하게 20 골드 발견",
                        Outcome = new EventOutcome
                        {
                            ResultText = "상자를 도로 던집니다. 해변에서 20 골드를 발견했습니다.",
                            GoldChange = 20
                        }
                    }
                },
                exclusiveThemeId: "AbyssalTrench");

            CreateEvent("Event_T_AT_KrakenTentacle", "크라케의 촉수",
                "떨어진 크라케 촉수가 아직 생명력을 가지고 있습니다. 먹으면 강력한 힘을 얻을 수 있을지도 모릅니다.",
                TeamLog.Event.EventType.Treasure,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "촉수를 먹는다",
                        ChoiceDescription = "HP -25%, 파티 영구 ATK +4",
                        Outcome = new EventOutcome
                        {
                            ResultText = "강렬한 맛과 힘이 폭발합니다! HP 25% 감소, 파티 ATK 영구 +4.",
                            HPPercentChange = -25,
                            PermanentAtkBonus = 4
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "무시한다",
                        ChoiceDescription = "35 골드에 판매",
                        Outcome = new EventOutcome
                        {
                            ResultText = "근처 어부에게 35 골드에 팔았습니다.",
                            GoldChange = 35
                        }
                    }
                },
                exclusiveThemeId: "AbyssalTrench");

            // Stormpeak (번개 봉우리) — 기절, 연쇄
            CreateEvent("Event_T_ST_StruckByLightning", "벼락 맞은 무덤",
                "벼락에 맞은 석상이 에너지를 뿜어내고 있습니다. 만지면 힘을 얻거나, 벼락을 맞을 수도 있습니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "석상을 만진다",
                        ChoiceDescription = "50% 영구 ATK +5 / 50% 파티 기절 2턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "에너지가 전해집니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "벼락의 힘이 스며듭니다! 파티 ATK 영구 +5.",
                                    PermanentAtkBonus = 5
                                },
                                new EventOutcome
                                {
                                    ResultText = "벼락이 떨어졌습니다! 파티 전원 기절 2턴.",
                                    ApplyStatusEffect = StatusEffectType.Stun,
                                    StatusEffectDuration = 2,
                                    StatusEffectValue = 0
                                }
                            }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "멀리한다",
                        ChoiceDescription = "25 골드 발견",
                        Outcome = new EventOutcome
                        {
                            ResultText = "거리를 둡니다. 주변에서 25 골드를 발견했습니다.",
                            GoldChange = 25
                        }
                    }
                },
                exclusiveThemeId: "Stormpeak");

            CreateEvent("Event_T_ST_StormRitual", "폭풍 의식",
                "고대 폭풍 소환사의 의식 유적을 발견했습니다. 참여하면 힘을 얻을 수 있지만 큰 대가가 필요합니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "의식에 참여한다",
                        ChoiceDescription = "HP -30%, 리롤 토큰 +3",
                        Outcome = new EventOutcome
                        {
                            ResultText = "폭풍의 힘을 흡수했습니다! HP 30% 감소, 리롤 토큰 3개 획득.",
                            HPPercentChange = -30,
                            RerollTokensBonus = 3
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "구경만 한다",
                        ChoiceDescription = "25 골드 발견",
                        Outcome = new EventOutcome
                        {
                            ResultText = "관찰만 합니다. 유적에서 25 골드를 발견했습니다.",
                            GoldChange = 25
                        }
                    }
                },
                exclusiveThemeId: "Stormpeak");

            // ShadowsGlade (그림자 골짜기) — 은신, 회피
            CreateEvent("Event_T_SG_FadeEcho", "페이드의 메아리",
                "그림자 존재가 다가와 속삭입니다. 받아들이면 골드를 주지만, 공격력이 일시적으로 약해집니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "메아리를 받아들인다",
                        ChoiceDescription = "40 골드, AttackDown 3턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "그림자가 스며듭니다. 40 골드를 받았지만 AttackDown 3턴.",
                            GoldChange = 40,
                            ApplyStatusEffect = StatusEffectType.AttackDown,
                            StatusEffectDuration = 3,
                            StatusEffectValue = 3
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "메아리를 거부한다",
                        ChoiceDescription = "HP -15%",
                        Outcome = new EventOutcome
                        {
                            ResultText = "그림자가 분노하며 떠납니다. HP 15% 감소.",
                            HPPercentChange = -15
                        }
                    }
                },
                exclusiveThemeId: "ShadowsGlade");

            CreateEvent("Event_T_SG_BlindSeer", "눈 먼 예언자",
                "눈 먼 노인이 다가오는 보스전에 대한 예언을 해줍니다. 골드를 내면 정확한 정보를 줄 수 있습니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "예언을 듣는다 (40 골드)",
                        ChoiceDescription = "리롤 토큰 +3 (다음 보스전 대비)",
                        MinGoldRequired = 40,
                        Outcome = new EventOutcome
                        {
                            ResultText = "보스의 약점을 예언받았습니다! 리롤 토큰 3개 획득.",
                            GoldChange = -40,
                            RerollTokensBonus = 3
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "무시한다",
                        ChoiceDescription = "10 골드를 도둑질",
                        Outcome = new EventOutcome
                        {
                            ResultText = "예언자의 지갑에서 10 골드를 훔쳤습니다.",
                            GoldChange = 10
                        }
                    }
                },
                exclusiveThemeId: "ShadowsGlade");

            // ===== Stage 4 — 클라이맥스 =====

            // EmberThrone (불꽃왕좌) — 화염, 폭발
            CreateEvent("Event_T_ET_SalamanderPact", "살라만더의 계약",
                "화염 정령 살라만더가 강력한 계약을 제안합니다. 영구 화염 저항과 함께 공격력이 크게 오릅니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "계약을 맺는다",
                        ChoiceDescription = "영구 ATK +6, 대가로 파티 화상 5턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "화염의 힘이 영혼에 새겨집니다! 파티 ATK 영구 +6, 화상 5턴.",
                            PermanentAtkBonus = 6,
                            ApplyStatusEffect = StatusEffectType.Burn,
                            StatusEffectDuration = 5,
                            StatusEffectValue = 3
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "거절한다",
                        ChoiceDescription = "HP 15% 회복",
                        Outcome = new EventOutcome
                        {
                            ResultText = "거절하자 살라만더가 존경의 눈빛을 보냅니다. HP 15% 회복.",
                            HPPercentChange = 15
                        }
                    }
                },
                exclusiveThemeId: "EmberThrone");

            CreateEvent("Event_T_ET_EmberForge", "잔불의 대장간",
                "잔불 속에서 정령 대장장이가 작업 중입니다. 골드를 내면 최종 결전을 위한 강력한 강화를 해줍니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "강화를 의뢰한다 (100 골드)",
                        ChoiceDescription = "영구 ATK +3, DEF +2",
                        MinGoldRequired = 100,
                        Outcome = new EventOutcome
                        {
                            ResultText = "정령의 불꽃이 장비를 강화했습니다! 파티 ATK 영구 +3, DEF 영구 +2.",
                            GoldChange = -100,
                            PermanentAtkBonus = 3,
                            PermanentDefBonus = 2
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "구경만 한다",
                        ChoiceDescription = "30 골드 받고 떠남",
                        Outcome = new EventOutcome
                        {
                            ResultText = "정령이 감탄하며 30 골드를 줍니다.",
                            GoldChange = 30
                        }
                    }
                },
                exclusiveThemeId: "EmberThrone");

            // EternalTundra (영원동토) — 빙결, 봉쇄
            CreateEvent("Event_T_ETu_FrozenHero", "빙결된 영웅",
                "수백 년간 얼어붙은 영웅의 시체를 발견했습니다. 해동하면 영웅의 힘을 얻지만, 흡수하면 체력을 얻습니다.",
                TeamLog.Event.EventType.Treasure,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "해동한다",
                        ChoiceDescription = "HP -25%, 영구 ATK +5",
                        Outcome = new EventOutcome
                        {
                            ResultText = "영웅이 마지막 힘을 전해줍니다! HP 25% 감소, 파티 ATK 영구 +5.",
                            HPPercentChange = -25,
                            PermanentAtkBonus = 5
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "에너지를 흡수한다",
                        ChoiceDescription = "HP 30% 회복",
                        Outcome = new EventOutcome
                        {
                            ResultText = "영웅의 생명력을 흡수했습니다. HP 30% 회복.",
                            HPPercentChange = 30
                        }
                    }
                },
                exclusiveThemeId: "EternalTundra");

            CreateEvent("Event_T_ETu_IceQueenRiddle", "빙결 여왕의 수수께끼",
                "빙결 여왕이 수수께끼를 냅니다. 맞히면 유물을 주고, 틀리면 얼려버립니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "정답을 자신한다",
                        ChoiceDescription = "50% 유물 획득 / 50% 파티 빙결 3턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "여왕이 정답을 기다립니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "정답이었습니다! 여왕이 감동하여 유물을 줍니다.",
                                    GiveRandomItem = true
                                },
                                new EventOutcome
                                {
                                    ResultText = "틀렸습니다! 여왕이 분노하여 얼려버립니다. 빙결 3턴.",
                                    ApplyStatusEffect = StatusEffectType.Freeze,
                                    StatusEffectDuration = 3,
                                    StatusEffectValue = 0
                                }
                            }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "포기한다",
                        ChoiceDescription = "20 골드 받고 떠남",
                        Outcome = new EventOutcome
                        {
                            ResultText = "여왕이 관대하게 20 골드를 줍니다.",
                            GoldChange = 20
                        }
                    }
                },
                exclusiveThemeId: "EternalTundra");

            // DemonCitadel (마왕성 심장) — 소환, 다중페이즈
            CreateEvent("Event_T_DC_DemonContract", "악마의 계약",
                "대마왕의 사자가 최종 계약을 제안합니다. 영혼을 담보로 하는 가장 강력한 거래입니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "계약에 서명한다",
                        ChoiceDescription = "영구 ATK +8, 영구 DEF -3",
                        Outcome = new EventOutcome
                        {
                            ResultText = "영혼의 일부를 바칩니다! 파티 ATK 영구 +8, DEF 영구 -3.",
                            PermanentAtkBonus = 8,
                            PermanentDefBonus = -3
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "계약을 찢는다",
                        ChoiceDescription = "HP 20% 회복 + 리롤 +2",
                        Outcome = new EventOutcome
                        {
                            ResultText = "계약을 찢자 악마가 놀랍니다. HP 20% 회복, 리롤 토큰 2개.",
                            HPPercentChange = 20,
                            RerollTokensBonus = 2
                        }
                    }
                },
                exclusiveThemeId: "DemonCitadel");

            CreateEvent("Event_T_DC_LegionAmbush", "군단의 매복",
                "악마 군단이 매복해 있습니다. 싸우면 대유물을, 도망치면 골드를 잃습니다.",
                TeamLog.Event.EventType.Trap,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "맞서 싸운다",
                        ChoiceDescription = "50% 대유물 / 50% 파티 HP -40%",
                        Outcome = new EventOutcome
                        {
                            ResultText = "군단과 전투를 벌입니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "승리했습니다! 대유물을 획득했습니다!",
                                    GiveRandomItem = true
                                },
                                new EventOutcome
                                {
                                    ResultText = "패배했습니다! 파티가 큰 데미지를 입었습니다. HP 40% 감소.",
                                    HPPercentChange = -40
                                }
                            }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "도망친다",
                        ChoiceDescription = "50 골드 잃음",
                        MinGoldRequired = 50,
                        Outcome = new EventOutcome
                        {
                            ResultText = "겨우 도망쳤습니다. 도망치며 50 골드를 잃었습니다.",
                            GoldChange = -50
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "은인다 (골드 없으면)",
                        ChoiceDescription = "HP -20%",
                        Outcome = new EventOutcome
                        {
                            ResultText = "그림자 속에 숨었습니다. 추격 중 HP 20% 감소.",
                            HPPercentChange = -20
                        }
                    }
                },
                exclusiveThemeId: "DemonCitadel");

            AssetDatabase.SaveAssets();
            Debug.Log("[DataGenerator.Stages] 테마별 전용 이벤트 24개 생성 완료");
        }

        #endregion

        private static void CreateTheme(
            string themeId, string displayName, int stageNumber,
            string[] normals, string[] elites, string boss,
            string spawnTable, string[] keywords, string desc,
            string[] themeEventIds = null)
        {
            string path = $"{STAGE_PATH}/Theme_{themeId}.asset";
            var theme = GetOrCreateAsset<StageThemeData>(path);
            theme.name = $"Theme_{themeId}";

            theme.themeId = themeId;
            theme.displayName = displayName;
            theme.stageNumber = stageNumber;
            theme.description = desc;

            // Normal enemies
            theme.normalEnemies = LoadCharactersByNames(normals);

            // Elite enemies
            theme.eliteEnemies = LoadCharactersByNames(elites);

            // Boss
            var bossAsset = AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_PATH}/{boss}.asset");
            theme.boss = bossAsset;

            // Phase BK: 보스 에셋에 _isBoss=true 설정 (Execution 즉사 제외용)
            if (bossAsset != null)
            {
                SetPrivateField(bossAsset, "_isBoss", true);
                EditorUtility.SetDirty(bossAsset);
            }
            else
            {
                Debug.LogWarning($"[DataGenerator.Stages] 보스 에셋 누락: {boss}");
            }

            // Spawn pattern table
            var table = AssetDatabase.LoadAssetAtPath<SpawnPatternTable>($"{SPAWN_PATTERN_PATH}/{spawnTable}.asset");
            theme.spawnPatternTable = table;

            // Keywords
            theme.themeKeywords = keywords.ToList();

            // Phase E3: 테마별 전용 이벤트 연결
            theme.themeEvents = LoadEventsByNames(themeEventIds);

            EditorUtility.SetDirty(theme);
        }

        private static List<CharacterData> LoadCharactersByNames(string[] names)
        {
            var list = new List<CharacterData>();
            if (names == null) return list;
            foreach (var n in names)
            {
                var asset = AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_PATH}/{n}.asset");
                if (asset != null)
                    list.Add(asset);
                else
                    Debug.LogWarning($"[DataGenerator.Stages] 적 에셋 누락: {n}");
            }
            return list;
        }

        /// <summary>
        /// Phase E3: 이벤트 이름 배열로 EventData 리스트 로드
        /// </summary>
        private static List<EventData> LoadEventsByNames(string[] eventIds)
        {
            var list = new List<EventData>();
            if (eventIds == null) return list;
            foreach (var id in eventIds)
            {
                var asset = AssetDatabase.LoadAssetAtPath<EventData>($"{EVENT_PATH}/{id}.asset");
                if (asset != null)
                    list.Add(asset);
                else
                    Debug.LogWarning($"[DataGenerator.Stages] 이벤트 에셋 누락: {id}");
            }
            return list;
        }
    }
}
#endif
