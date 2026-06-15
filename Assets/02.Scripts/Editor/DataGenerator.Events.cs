#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TeamLog.Event;

namespace TeamLog.Editor
{
    /// <summary>
    /// DataGenerator — 이벤트 데이터 생성 (GenerateEventData + CreateEvent)
    /// 진입점/스킬/캐릭터/유틸리티: DataGenerator.cs
    /// 증강 데이터/스폰 패턴: DataGenerator.Augments.cs
    /// 유물 데이터: DataGenerator.Relics.cs
    /// 팔레트 (UI/오디오/VFX): DataGenerator.Palettes.cs
    /// </summary>
    public static partial class DataGenerator
    {
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
    }
}
#endif
