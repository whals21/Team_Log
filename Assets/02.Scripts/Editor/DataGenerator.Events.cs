#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TeamLog.Event;
using TeamLog.Characters;

namespace TeamLog.Editor
{
    /// <summary>
    /// DataGenerator — 이벤트 데이터 생성 (Phase E1/E2/E3)
    /// 진입점/스킬/캐릭터/유틸리티: DataGenerator.cs
    /// 증강 데이터/스폰 패턴: DataGenerator.Augments.cs
    /// 유물 데이터: DataGenerator.Relics.cs
    /// 팔레트 (UI/오디오/VFX): DataGenerator.Palettes.cs
    /// 스테이지 테마: DataGenerator.Stages.cs (테마별 이벤트 24개)
    /// </summary>
    public static partial class DataGenerator
    {
        #region Event Data

        private static void GenerateEventData()
        {
            // ===== 기존 10개 (E1: 영구 강화 필드 적용) =====
            GenerateLegacyEvents();

            // ===== Phase E2: 공통 신규 15개 =====
            GenerateGambleEvents();        // 도박 4개
            GenerateCurseEvents();         // 저주 3개
            GeneratePermanentBuffEvents(); // 영구 강화 투자 3개
            GenerateStoryEvents();         // 스토리 3개
            GenerateConditionalEvents();   // 조건부 2개
        }

        #region Legacy 10 (영구 강화 필드 적용)

        private static void GenerateLegacyEvents()
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
                        ChoiceDescription = "빠르게 상자를 엽니다. 함정이 있을 수도 있습니다.",
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
                        ChoiceDescription = "골드 20을 제물로 바칩니다.",
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

            // ★ 영구 ATK 강화 적용 (Phase E1)
            CreateEvent("Event_AncientLibrary", "고대 도서관", "오래된 도서관에서 반짝이는 책을 발견했습니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "책을 읽는다",
                        ChoiceDescription = "고대의 지식을 얻습니다. 파티 전원 공격력 영구 +2",
                        Outcome = new EventOutcome
                        {
                            ResultText = "고대의 지식을 얻었습니다! 모든 파티원의 공격력이 영구히 2 증가했습니다.",
                            GoldChange = 0, HPPercentChange = 0,
                            PermanentAtkBonus = 2
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

            // ★ 영구 DEF 강화 적용 (Phase E1)
            CreateEvent("Event_FallenKnight", "쓰러진 기사", "부상당한 기사가 도움을 요청합니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "치유해 준다",
                        ChoiceDescription = "기사를 치유합니다. 파티 전원 방어력 영구 +2",
                        Outcome = new EventOutcome
                        {
                            ResultText = "기사가 감사하며 방어 기술을 알려주었습니다! 방어력이 영구히 2 증가했습니다.",
                            GoldChange = 0, HPPercentChange = -5,
                            PermanentDefBonus = 2
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
                        ChoiceDescription = "고블린을 쫓아갑니다. 위험할 수 있습니다.",
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

            // ★ 영구 ATK 강화 적용 (Phase E1) — 위험 비용이 크므로 +3
            CreateEvent("Event_CursedAltar", "저주받은 제단", "어둠의 기운이 감도는 제단이 있습니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "기도한다",
                        ChoiceDescription = "제단에 기도를 올립니다. HP -20%, 파티 공격력 영구 +3",
                        Outcome = new EventOutcome
                        {
                            ResultText = "저주가 풀리며 강력한 힘이 주입되었습니다! HP가 약간 깎였지만 공격력이 영구히 3 증가했습니다.",
                            GoldChange = 0, HPPercentChange = -20,
                            PermanentAtkBonus = 3
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
                        MinGoldRequired = 20,
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
                        MinGoldRequired = 15,
                        Outcome = new EventOutcome
                        {
                            ResultText = "유용한 정보를 얻었습니다! 앞으로의 전투에서 유리할 것입니다.",
                            GoldChange = -15, HPPercentChange = 0,
                            RerollTokensBonus = 1
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

        #endregion

        #region Phase E2-1: 도박 이벤트 4개

        private static void GenerateGambleEvents()
        {
            // 1. 황금 우상 — 60%: 80G / 40%: 영구 ATK -2
            CreateEvent("Event_GoldenIdol", "황금 우상",
                "동굴 깊은 곳에 황금빛 우상이 빛나고 있습니다. 손을 대면 반응할 것 같습니다.",
                TeamLog.Event.EventType.Treasure,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "우상을 만진다",
                        ChoiceDescription = "60% 확률로 80 골드 / 40% 확률로 영구 ATK -2",
                        Outcome = new EventOutcome
                        {
                            ResultText = "우상에 손을 댑니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "우상이 금화로 변했습니다! 80 골드를 획득했습니다.",
                                    GoldChange = 80
                                },
                                new EventOutcome
                                {
                                    ResultText = "저주가 발동했습니다! 파티 전원의 공격력이 영구히 2 감소했습니다.",
                                    PermanentAtkBonus = -2
                                }
                            },
                            OutcomeWeights = new List<float> { 60f, 40f }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "그냥 지나간다",
                        ChoiceDescription = "안전하게 무시합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "신중한 선택이었습니다. 우상을 남겨둡니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });

            // 2. 도박꾼의 주사위 — 33% 유물 / 33% 50G / 33% HP -25%
            CreateEvent("Event_DiceGame", "도박꾼의 주사위",
                "은둔자가 주사위 놀이를 제안합니다. 운이 좋다면 큰 보상을 얻을 수 있을지도 모릅니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "주사위를 굴린다",
                        ChoiceDescription = "33% 유물 / 33% 50 골드 / 33% HP -25%",
                        Outcome = new EventOutcome
                        {
                            ResultText = "주사위가 굴러갑니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "큰 성공! 귀중한 유물을 얻었습니다!",
                                    GiveRandomItem = true
                                },
                                new EventOutcome
                                {
                                    ResultText = "50 골드를 획득했습니다.",
                                    GoldChange = 50
                                },
                                new EventOutcome
                                {
                                    ResultText = "실패! 파티 전원이 데미지를 입었습니다.",
                                    HPPercentChange = -25
                                }
                            }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "거절한다",
                        ChoiceDescription = "도박은 신중하게.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "은둔자가 어깨를 으쓱하며 돌아섭니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });

            // 3. 저주받은 분수 — 50% 풀회복 / 50% 독 5턴
            CreateEvent("Event_CursedFountain", "저주받은 분수",
                "정체불명의 붉은 물이 솟는 분수가 있습니다. 마시면 효과가 있을 것 같지만 위험해 보입니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "분수의 물을 마신다",
                        ChoiceDescription = "50% 파티 풀회복 / 50% 파티 독 5턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "물을 마십니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "놀랍게도 파티 전원의 HP가 가득 찼습니다!",
                                    HPPercentChange = 100
                                },
                                new EventOutcome
                                {
                                    ResultText = "독이 몸을 퍼졌습니다! 파티 전원이 5턴간 독에 걸렸습니다.",
                                    ApplyStatusEffect = StatusEffectType.Poison,
                                    StatusEffectDuration = 5,
                                    StatusEffectValue = 3
                                }
                            }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "무시한다",
                        ChoiceDescription = "분수를 그대로 둡니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "신중하게 분수를 지나칩니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });

            // 4. 망각의 허브 — 50% 리롤 토큰 +2 / 50% 증강 삭제 (구현상 리롤 +0 / HP -10)
            CreateEvent("Event_AmnesiaHerb", "망각의 허브",
                "은빛 빛을 내는 허브를 발견했습니다. 전설에 따르면 무언가를 잃지만 무언가를 얻는다고 합니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "허브를 먹는다",
                        ChoiceDescription = "50% 리롤 토큰 +2 / 50% HP -10%",
                        Outcome = new EventOutcome
                        {
                            ResultText = "허브를 씹어 삼킵니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "정신이 맑아집니다! 리롤 토큰 2개를 획득했습니다.",
                                    RerollTokensBonus = 2
                                },
                                new EventOutcome
                                {
                                    ResultText = "혼란이 찾아왔습니다! 파티 전원 HP 10% 감소.",
                                    HPPercentChange = -10
                                }
                            }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "그냥 둔다",
                        ChoiceDescription = "위험을 피합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "허브를 원래 자리에 둡니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });
        }

        #endregion

        #region Phase E2-2: 저주 이벤트 3개 (강력한 보상 + 대가)

        private static void GenerateCurseEvents()
        {
            // 1. 피의 계약 — 영구 ATK +8 / 화상 5턴
            CreateEvent("Event_BloodPact", "피의 계약",
                "어둠 속에서 속삭이는 목소리가 들립니다. \"힘을 원하는가? 대가는 작지 않단다.\"",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "계약을 맺는다",
                        ChoiceDescription = "파티 ATK 영구 +8, 대가로 파티 전원 화상 5턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "강력한 힘이 주입되었습니다! 파티 공격력이 영구히 8 증가했지만, 영혼이 조금 타들어갑니다.",
                            PermanentAtkBonus = 8,
                            ApplyStatusEffect = StatusEffectType.Burn,
                            StatusEffectDuration = 5,
                            StatusEffectValue = 2
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "거절한다",
                        ChoiceDescription = "위험한 거래를 피합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "속삭임이 사라졌습니다. 하지만 작은 축복을 받았습니다.",
                            GoldChange = 0, HPPercentChange = 10
                        }
                    }
                });

            // 2. 그림자의 각인 — 영구 DEF +5 / 파티 HP -20%
            CreateEvent("Event_ShadowMark", "그림자의 각인",
                "그림자 정령이 파티원 한 명의 영혼에 각인을 새겨 영구적인 방어력을 제안합니다.",
                TeamLog.Event.EventType.Treasure,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "각인을 받아들인다",
                        ChoiceDescription = "파티 DEF 영구 +5, 대가로 파티 HP 20% 감소",
                        Outcome = new EventOutcome
                        {
                            ResultText = "그림자가 몸을 감쌉니다! 방어력이 영구히 5 증가했지만, 체력이 영구히 약해집니다.",
                            PermanentDefBonus = 5,
                            HPPercentChange = -20
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "각인을 거부한다",
                        ChoiceDescription = "그림자를 쫓아냅니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "그림자가 사라졌습니다. 평온한 마음으로 20 골드를 발견했습니다.",
                            GoldChange = 20, HPPercentChange = 0
                        }
                    }
                });

            // 3. 얼어붙은 심장 — 영구 ATK +2 / 빙결 3턴
            CreateEvent("Event_FrozenHeart", "얼어붙은 심장",
                "푸른 빛의 얼어붙은 심장이 당신을 부릅니다. 영원의 힘이 깃들어 있습니다.",
                TeamLog.Event.EventType.Treasure,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "심장을 흡수한다",
                        ChoiceDescription = "파티 ATK 영구 +2, 대가로 파티 빙결 3턴",
                        Outcome = new EventOutcome
                        {
                            ResultText = "차가운 힘이 스며듭니다! 공격력이 영구히 2 증가했지만, 몸이 굳어갑니다.",
                            PermanentAtkBonus = 2,
                            ApplyStatusEffect = StatusEffectType.Freeze,
                            StatusEffectDuration = 3,
                            StatusEffectValue = 0
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "심장을 녹인다",
                        ChoiceDescription = "해골을 정화합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "심장이 녹으며 작은 보석을 남겼습니다. 40 골드를 획득했습니다.",
                            GoldChange = 40, HPPercentChange = 5
                        }
                    }
                });
        }

        #endregion

        #region Phase E2-3: 영구 강화 투자 3개

        private static void GeneratePermanentBuffEvents()
        {
            // 1. 훈련용 허수아비 — 골드 -60 / 파티 영구 ATK +2
            CreateEvent("Event_TrainingDummy", "훈련용 허수아비",
                "오래된 훈련용 허수아비가 길가에 서 있습니다. 수련하면 전투 감각을 되찾을 수 있을 것 같습니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "수련한다",
                        ChoiceDescription = "60 골드를 내고 파티 ATK 영구 +2",
                        MinGoldRequired = 60,
                        Outcome = new EventOutcome
                        {
                            ResultText = "오랜 수련 끝에 전투 감각을 되찾았습니다! 파티 전원 공격력 영구 +2.",
                            GoldChange = -60,
                            PermanentAtkBonus = 2
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "무시한다",
                        ChoiceDescription = "시간을 아낍니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "허수아비를 지나칩니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });

            // 2. 명상의 봉우리 — HP -20% / 파티 영구 DEF +3
            CreateEvent("Event_MeditationPeak", "명상의 봉우리",
                "고요한 봉우리에 오르면 마음이 정화됩니다. 명상을 통해 내면의 힘을 기를 수 있습니다.",
                TeamLog.Event.EventType.Shrine,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "명상에 잠긴다",
                        ChoiceDescription = "파티 HP 20% 감소, 대가로 파티 DEF 영구 +3",
                        Outcome = new EventOutcome
                        {
                            ResultText = "명상을 통해 내면의 방패를 얻었습니다! 파티 전원 방어력 영구 +3.",
                            HPPercentChange = -20,
                            PermanentDefBonus = 3
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "경치를 즐긴다",
                        ChoiceDescription = "그냥 구경합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "평화로운 시간을 보냅니다. HP가 약간 회복되었습니다.",
                            GoldChange = 0, HPPercentChange = 10
                        }
                    }
                });

            // 3. 고대 대장장이 — 골드 -80 / 파티 영구 ATK +1, DEF +1
            CreateEvent("Event_AncientBlacksmith", "고대 대장장이",
                "오래된 대장장이에서 정령이 불꽃을 다루고 있습니다. 골드를 내면 장비를 강화해 준다고 합니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "장비를 강화한다",
                        ChoiceDescription = "80 골드를 내고 파티 ATK/DEF 영구 +1",
                        MinGoldRequired = 80,
                        Outcome = new EventOutcome
                        {
                            ResultText = "정령의 불꽃이 장비를 강화했습니다! 파티 전원 ATK/DEF 영구 +1.",
                            GoldChange = -80,
                            PermanentAtkBonus = 1,
                            PermanentDefBonus = 1
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "감상만 한다",
                        ChoiceDescription = "골드를 아낍니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "정령이 고개를 끄덕입니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });
        }

        #endregion

        #region Phase E2-4: 스토리 이벤트 3개 (Story 타입 신설)

        private static void GenerateStoryEvents()
        {
            // 1. 쓰러진 영웅의 일지 — 다음 보스 보상 +50% (구현: 리롤 토큰)
            CreateEvent("Event_FallenHeroLog", "쓰러진 영웅의 일지",
                "전사한 영웅의 일지를 발견했습니다. 마지막 페이지에는 보스전에 대한 단서가 적혀 있습니다.",
                TeamLog.Event.EventType.Story,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "일지를 읽는다",
                        ChoiceDescription = "보스전 단서를 얻어 리롤 토큰 +2",
                        Outcome = new EventOutcome
                        {
                            ResultText = "일지를 통해 보스의 약점을 파악했습니다! 다음 보스전에서 유리할 것입니다. 리롤 토큰 2개 획득.",
                            RerollTokensBonus = 2
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "일지를 태운다",
                        ChoiceDescription = "과거를 잊고 30 골드를 발견합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "일지를 태웁니다. 재 사이에서 30 골드가 나왔습니다.",
                            GoldChange = 30, HPPercentChange = 0
                        }
                    }
                });

            // 2. 과거의 환영 — 다음 치명적 데미지 방어 (구현: 쉴드 50)
            CreateEvent("Event_VisionOfPast", "과거의 환영",
                "거울에 비친 자신이 아닌 다른 인물이 보입니다. 그 인물이 다가와 속삭입니다.",
                TeamLog.Event.EventType.Story,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "환영을 받아들인다",
                        ChoiceDescription = "파티 전원 쉴드 50을 얻습니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "환영과 하나가 되었습니다. 보호막이 전원에게 생겼습니다!",
                            ApplyStatusEffect = StatusEffectType.Shield,
                            StatusEffectDuration = 1,
                            StatusEffectValue = 50
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "환영을 부정한다",
                        ChoiceDescription = "현실을 지키며 25 골드를 얻습니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "거울이 깨졌습니다. 그 속에서 25 골드를 발견했습니다.",
                            GoldChange = 25, HPPercentChange = 0
                        }
                    }
                });

            // 3. 정체불명의 편지 — 연쇄 이벤트 (NextEventId)
            CreateEvent("Event_MysteriousLetter", "정체불명의 편지",
                "바닥에 봉인된 편지가 떨어져 있습니다. 발신인이 적히지 않았지만, 묘한 힘이 느껴집니다.",
                TeamLog.Event.EventType.Story,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "편지를 읽는다",
                        ChoiceDescription = "편지의 내용을 확인합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "\"도움이 필요하다면 이 동전을 사용하시오.\" 편지와 함께 30 골드가 동봉되어 있습니다.",
                            GoldChange = 30,
                            NextEventId = "" // 필요 시 후속 이벤트 ID 지정
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "편지를 버린다",
                        ChoiceDescription = "관여하지 않습니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "편지를 강에 던졌습니다. 평화가 찾아옵니다. HP 5% 회복.",
                            GoldChange = 0, HPPercentChange = 5
                        }
                    }
                });
        }

        #endregion

        #region Phase E2-5: 조건부 이벤트 2개

        private static void GenerateConditionalEvents()
        {
            // 1. 절박한 도박 — 파티 HP < 40%일 때만 선택 가능 (MinPartyHPPercent는 최소 조건이므로 다른 방식 필요)
            // 실제 구현: 도박 선택지는 항상 표시하되, "도움 요청" 선택지는 HP가 낮을 때 활성화
            CreateEvent("Event_DesperateGamble", "절박한 도박",
                "길가에 쓰러진 시체에서 빛나는 물약을 발견했습니다. 반쯤 비어있고 정체를 알 수 없습니다.",
                TeamLog.Event.EventType.Treasure,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "마신다",
                        ChoiceDescription = "30% 파티 풀회복 / 70% 파티 HP -30%",
                        Outcome = new EventOutcome
                        {
                            ResultText = "물약을 들이킵니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "놀랍게도 강력한 회복약이었습니다! 파티 HP가 가득 찼습니다.",
                                    HPPercentChange = 100
                                },
                                new EventOutcome
                                {
                                    ResultText = "독이었습니다! 파티 전원이 큰 데미지를 입었습니다.",
                                    HPPercentChange = -30
                                }
                            },
                            OutcomeWeights = new List<float> { 30f, 70f }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "버린다",
                        ChoiceDescription = "위험을 피합니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "물약을 바닥에 버렸습니다. 안전이 최우선입니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });

            // 2. 부유한 상인 — 골드 150+ 필요
            CreateEvent("Event_RichMerchant", "부유한 상인",
                "화려한 옷을 입은 상인이 특급 유물을 판매합니다. 가격은 비싸지만 가치는 확실합니다.",
                TeamLog.Event.EventType.NPC,
                new[]
                {
                    new EventChoice
                    {
                        ChoiceText = "레어 유물을 구매한다",
                        ChoiceDescription = "150 골드로 확정 유물 획득",
                        MinGoldRequired = 150,
                        Outcome = new EventOutcome
                        {
                            ResultText = "레어 유물을 구매했습니다!",
                            GoldChange = -150,
                            GiveRandomItem = true
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "흥정한다",
                        ChoiceDescription = "50% 할인 성공 / 50% 상인 화남",
                        Outcome = new EventOutcome
                        {
                            ResultText = "흥정을 시도합니다...",
                            RandomOutcomes = new List<EventOutcome>
                            {
                                new EventOutcome
                                {
                                    ResultText = "성공! 30 골드를 깎았습니다. 그래도 구매는 하지 않았습니다.",
                                    GoldChange = 0
                                },
                                new EventOutcome
                                {
                                    ResultText = "상인이 불쾌해하며 떠납니다.",
                                    GoldChange = 0, HPPercentChange = 0
                                }
                            }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceText = "구경만 한다",
                        ChoiceDescription = "지나칩니다.",
                        Outcome = new EventOutcome
                        {
                            ResultText = "상인이 실망하며 떠납니다.",
                            GoldChange = 0, HPPercentChange = 0
                        }
                    }
                });
        }

        #endregion

        private static void CreateEvent(string fileName, string name, string desc,
            TeamLog.Event.EventType type, EventChoice[] choices,
            int weight = 10, string exclusiveThemeId = "")
        {
            var path = $"{EVENT_PATH}/{fileName}.asset";
            var eventData = GetOrCreateAsset<EventData>(path);
            eventData.name = fileName;

            SetPrivateField(eventData, "_eventName", name);
            SetPrivateField(eventData, "_description", desc);
            SetPrivateField(eventData, "_eventType", type);
            SetPrivateField(eventData, "_choices", new List<EventChoice>(choices));
            SetPrivateField(eventData, "_weight", weight);
            SetPrivateField(eventData, "_exclusiveThemeId", exclusiveThemeId);

            EditorUtility.SetDirty(eventData);
        }

        #endregion
    }
}
#endif
