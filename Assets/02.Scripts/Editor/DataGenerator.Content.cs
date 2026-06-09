#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Combat.AI;
using TeamLog.Event;
using TeamLog.Reward;
using TeamLog.UI;

using TeamLog.Skill;

namespace TeamLog.Editor
{
    /// <summary>
    /// DataGenerator — 콘텐츠 데이터 생성 (이벤트, 유물, 팔레트)
    /// 진입점/스킬/캐릭터/유틸리티: DataGenerator.cs
    /// 증강 데이터/스폰 패턴: DataGenerator.Augments.cs
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

        #region UI Palette

        private static void GenerateUIPalette()
        {
            const string path = "Assets/03.Data/UIPalette.asset";
            var palette = GetOrCreateAsset<UIPalette>(path);
            palette.name = "UIPalette";
            EditorUtility.SetDirty(palette);
            Debug.Log("[DataGenerator] UIPalette asset ensured.");
        }

        #endregion

        #region Audio Palette

        private static void GenerateAudioPalette()
        {
            EnsureFolder("Assets/Resources");
            const string path = "Assets/Resources/AudioPalette.asset";
            var palette = GetOrCreateAsset<AudioPalette>(path);
            palette.name = "AudioPalette";

            // 28개 사운드 매핑: (키, Assets/ 이하 상대경로)
            var mappings = new (string key, string assetPath)[]
            {
                // 전투 SFX — 기본 (13)
                ("AttackHit",          "Assets/Epic Toon FX/Sound/etfx_target_hit.wav"),
                ("Heal",               "Assets/CombatMagicSpellsVIISFX/Heal1/SFX_Spell01Heal.wav"),
                ("ShieldApply",        "Assets/Fantasy UI SFX - Lite Edition/Armor 1-1.wav"),
                ("StatusEffectApply",  "Assets/CombatMagicSpellsVIISFX/Dark1/SFX_Spell01Dark.wav"),
                ("Purify",             "Assets/CombatMagicSpellsVIISFX/Ice1/SFX_Spell01Ice.wav"),
                ("Miss",               "Assets/Epic Toon FX/Sound/etfx_explosion_poof.wav"),
                ("CharacterDeath",     "Assets/Epic Toon FX/Sound/etfx_explosion_dark01.wav"),
                ("SkillDraw",          "Assets/Fantasy UI SFX - Lite Edition/Card Draw 1-1.wav"),
                ("SkillReroll",        "Assets/Fantasy UI SFX - Lite Edition/Card Place 1-1.wav"),
                ("TurnStart",          "Assets/Fantasy UI SFX - Lite Edition/Interface 1-1.wav"),
                ("BuffApply",          "Assets/CombatMagicSpellsVIISFX/Thunder1/SFX_Spell01Cast01.wav"),
                ("DebuffApply",        "Assets/CombatMagicSpellsVIISFX/Water1/SFX_Spell01Water.wav"),
                ("EnemyAttack",        "Assets/CombatMagicSpellsVIISFX/Fire1/SFX_Spell01Fire.wav"),
                // 전투 SFX — 스킬 타입별 (14)
                ("FireImpact",         "Assets/CombatMagicSpellsVIISFX/Fire5/SFX_Spell02Fire.wav"),
                ("IceImpact",          "Assets/CombatMagicSpellsVIISFX/Ice5/SFX_Spell01Ice.wav"),
                ("ThunderImpact",      "Assets/CombatMagicSpellsVIISFX/Thunder7/SFX_Spell01Thunder01.wav"),
                ("DarkImpact",         "Assets/CombatMagicSpellsVIISFX/Dark5/SFX_Spell01Dark.wav"),
                ("PoisonImpact",       "Assets/CombatMagicSpellsVIISFX/Earth5/SFX_Spell01Earth.wav"),
                ("BurnImpact",         "Assets/CombatMagicSpellsVIISFX/Fire3/SFX_Spell02Fire.wav"),
                ("FreezeImpact",       "Assets/CombatMagicSpellsVIISFX/Ice3/SFX_Spell01Ice.wav"),
                ("HealImpact",         "Assets/CombatMagicSpellsVIISFX/Heal5/SFX_Spell01Heal.wav"),
                ("BuffCast",           "Assets/CombatMagicSpellsVIISFX/Heal2/SFX_Spell01Heal.wav"),
                ("DebuffCast",         "Assets/CombatMagicSpellsVIISFX/Dark3/SFX_Spell01Dark.wav"),
                ("PurifyCast",         "Assets/CombatMagicSpellsVIISFX/Heal7/SFX_Spell01Swoosh01Heal.wav"),
                ("ShieldCast",         "Assets/CombatMagicSpellsVIISFX/Water3/SFX_Spell01Water.wav"),
                ("CriticalHit",        "Assets/CombatMagicSpellsVIISFX/Thunder9/SFX_Spell01Thunder.wav"),
                ("EnemySkillHit",      "Assets/CombatMagicSpellsVIISFX/Dark4/SFX_Spell01Dark.wav"),
                // 전투 결과 (2)
                ("Victory",            "Assets/Fantasy UI SFX - Lite Edition/Magical Texture Chimes 1-1.wav"),
                ("Defeat",             "Assets/Epic Toon FX/Sound/etfx_explosion_dark02.wav"),
                // UI SFX (13)
                ("UIClick",            "Assets/Fantasy UI SFX - Lite Edition/Interface 2-1.wav"),
                ("UIShopPurchase",     "Assets/UI SFX Mega Pack/Assets/Audio/Purchase/coins_1.wav"),
                ("UIShopOpen",         "Assets/Fantasy UI SFX - Lite Edition/Bag Handle 1-1.wav"),
                ("UIGoldEarn",         "Assets/Fantasy UI SFX - Lite Edition/Coins 1-5.wav"),
                ("UIGoldSpend",        "Assets/Fantasy UI SFX - Lite Edition/Coin Bag 1-1.wav"),
                ("UIWarning",          "Assets/UI SFX Mega Pack/Assets/Audio/Warning_Popup/warning_1.wav"),
                ("UICancel",           "Assets/UI SFX Mega Pack/Assets/Audio/Cancel/cancel_1.wav"),
                ("UIConfirm",          "Assets/UI SFX Mega Pack/Assets/Audio/Ok/ok_1.wav"),
                ("UITransition",       "Assets/Fantasy UI SFX - Lite Edition/Magical Interface 1-1.wav"),
                ("UIToast",            "Assets/Fantasy UI SFX - Lite Edition/Special Interface 1-1.wav"),
                ("UINodeClick",        "Assets/Fantasy UI SFX - Lite Edition/Building Interface 1-1.wav"),
                ("UIReroll",           "Assets/Fantasy UI SFX - Lite Edition/Card Draw 2-1.wav"),
                ("UIPotion",           "Assets/Fantasy UI SFX - Lite Edition/Potion Item 1-1.wav"),
            };

            // 기존 엔트리 클리어 후 재구성
            palette.entries.Clear();

            int loaded = 0;
            foreach (var (key, assetPath) in mappings)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip == null)
                    Debug.LogWarning($"[DataGenerator] AudioPalette: 클립을 찾을 수 없음 — {key}: {assetPath}");
                else
                    loaded++;

                palette.entries.Add(new AudioPalette.AudioEntry { name = key, clip = clip });
            }

            EditorUtility.SetDirty(palette);
            Debug.Log($"[DataGenerator] AudioPalette 생성 완료 — {loaded}/{mappings.Length} 클립 로드됨.");
        }

        #endregion

        #region VFX Palette

        private static void GenerateVFXPalette()
        {
            EnsureFolder("Assets/Resources");
            const string path = "Assets/Resources/VFXPalette.asset";
            var palette = GetOrCreateAsset<VFXPalette>(path);
            palette.name = "VFXPalette";

            var mappings = new (string key, string assetPath)[]
            {
                // 전투 이펙트
                ("Hit",       "Assets/Epic Toon FX/Prefabs/Combat/Sword/Hit/SwordHit/SwordHitRed.prefab"),
                ("Heal",      "Assets/Epic Toon FX/Prefabs/Interactive/Healing/HealOnce.prefab"),
                ("Shield",    "Assets/Epic Toon FX/Prefabs/Combat/Magic/Shield/MagicShieldBlue.prefab"),
                ("Death",     "Assets/Epic Toon FX/Prefabs/Combat/Death/Skulls/GenericDeath.prefab"),
                ("Buff",      "Assets/Epic Toon FX/Prefabs/Combat/Magic/Buff/MagicBuffBlue.prefab"),
                ("Debuff",    "Assets/Epic Toon FX/Prefabs/Combat/Magic/Enchant/MagicEnchantYellow.prefab"),
                ("Burn",      "Assets/Epic Toon FX/Prefabs/Combat/Explosions/FireballSoftExplosion/ExplosionFireballSoftFire.prefab"),
                ("Poison",    "Assets/Epic Toon FX/Prefabs/Combat/Explosions (Misc)/PoisonExplosion.prefab"),
                ("Freeze",    "Assets/Epic Toon FX/Prefabs/Combat/Explosions/FrostExplosion/FrostExplosion.prefab"),
                ("Critical",  "Assets/Epic Toon FX/Prefabs/Combat/Sword/Hit/SwordHitCritical/SwordHitRedCritical.prefab"),
                ("Purify",    "Assets/Epic Toon FX/Prefabs/Interactive/Healing/HealNova.prefab"),
                // 전투 결과
                ("Victory",   "Assets/Epic Toon FX/Prefabs/Combat/Explosions (Text)/Critical.prefab"),
                ("Defeat",    "Assets/Epic Toon FX/Prefabs/Combat/Death/Skulls/EvilDeath.prefab"),
            };

            palette.entries.Clear();

            int loaded = 0;
            foreach (var (key, assetPath) in mappings)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null)
                    Debug.LogWarning($"[DataGenerator] VFXPalette: 프리팹을 찾을 수 없음 — {key}: {assetPath}");
                else
                    loaded++;

                palette.entries.Add(new VFXPalette.VFXEntry { name = key, prefab = prefab });
            }

            EditorUtility.SetDirty(palette);
            Debug.Log($"[DataGenerator] VFXPalette 생성 완료 — {loaded}/{mappings.Length} 프리팹 로드됨.");
        }

        #endregion

        #region Relic Data

        private const string RELIC_PATH = "Assets/03.Data/Relics";

        private static void GenerateRelicData()
        {
            EnsureFolder(RELIC_PATH);

            CreateRelic("Relic_BurningSword", "불타는 검", "공격 시 추가 데미지 +3",
                RelicTrigger.OnSkillUsed, RelicEffectType.BonusDamage, 3, RewardRarity.Common, price: 80,
                new[] { Kw(KeywordType.BonusOutgoingDamage, 3) });

            CreateRelic("Relic_IronHide", "철가죽", "받는 피해 -2",
                RelicTrigger.OnDamageReceived, RelicEffectType.DamageReduction, 2, RewardRarity.Common, price: 90,
                new[] { Kw(KeywordType.DamageReduction, 2) });

            CreateRelic("Relic_RegenRing", "재생의 반지", "매 턴 3 HP 회복",
                RelicTrigger.TurnEnd, RelicEffectType.HealPerTurn, 3, RewardRarity.Common, price: 60,
                new[] { Kw(KeywordType.HPPerTurn, 3, KeywordTrigger.OnTurnEnd) });

            CreateRelic("Relic_GoldCharm", "황금 부적", "골드 획득 시 +15 골드",
                RelicTrigger.OnGoldEarned, RelicEffectType.BonusGold, 15, RewardRarity.Rare, price: 120,
                new[] { Kw(KeywordType.BonusGold, 15, KeywordTrigger.OnGoldEarned) });

            CreateRelic("Relic_ShieldAmulet", "방패 부적", "방패 스킬 사용 시 +3 쉴드",
                RelicTrigger.OnShieldGained, RelicEffectType.BonusShield, 3, RewardRarity.Common, price: 70,
                new[] { Kw(KeywordType.ShieldPerTurn, 3, KeywordTrigger.OnShieldGained) });

            CreateRelic("Relic_VampireFang", "흡혈 송곳니", "적 처치 시 +5 HP 회복",
                RelicTrigger.OnKill, RelicEffectType.HealOnKill, 5, RewardRarity.Rare, price: 100,
                new[] { Kw(KeywordType.OnKillHeal, 5, KeywordTrigger.OnKill) });

            CreateRelic("Relic_BerserkerMark", "광전사 인장", "적 처치당 공격력 +2 누적",
                RelicTrigger.OnKill, RelicEffectType.StackingPowerOnKill, 2, RewardRarity.Unique, price: 180,
                new[] { Kw(KeywordType.StackingPowerOnKill, 2, KeywordTrigger.OnKill) });

            CreateRelic("Relic_LuckyClover", "네잎클로버", "드로우 가중치 +5",
                RelicTrigger.BattleStart, RelicEffectType.BonusDrawWeight, 5, RewardRarity.Rare, price: 110,
                new[] { Kw(KeywordType.DrawWeightAdd, 5) });

            CreateRelic("Relic_ThornArmor", "가시 갑옷", "피격 시 반사 데미지 2",
                RelicTrigger.OnDamageReceived, RelicEffectType.CounterDamage, 2, RewardRarity.Rare, price: 130,
                new[] { Kw(KeywordType.CounterDamage, 2) });

            CreateRelic("Relic_SwiftBoots", "질풍 부츠", "매 턴 쉴드 +2",
                RelicTrigger.TurnStart, RelicEffectType.BonusShield, 2, RewardRarity.Rare, price: 100,
                new[] { Kw(KeywordType.ShieldPerTurn, 2, KeywordTrigger.OnTurnStart) });

            CreateRelic("Relic_WarBanner", "전투 깃발", "전투 시작 시 쉴드 +5 (전체)",
                RelicTrigger.BattleStart, RelicEffectType.BonusShield, 5, RewardRarity.Unique, price: 160,
                new[] { Kw(KeywordType.ShieldPerTurn, 5, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_HealingHerb", "치유 허브", "전투 시작 시 파티 HP 10 회복",
                RelicTrigger.BattleStart, RelicEffectType.HealPerTurn, 10, RewardRarity.Common, price: 60,
                new[] { Kw(KeywordType.HPPerTurn, 10, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_LifeCrystal", "생명력의 결정", "전투 시작 시 최대 HP +20",
                RelicTrigger.BattleStart, RelicEffectType.MaxHPUp, 20, RewardRarity.Common, price: 80,
                new[] { Kw(KeywordType.MaxHPUp, 20, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_WeaponStone", "무기 강화석", "전투 시작 시 공격력 +3",
                RelicTrigger.BattleStart, RelicEffectType.ATKUp, 3, RewardRarity.Rare, price: 100,
                new[] { Kw(KeywordType.ATKUp, 3, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_HardShell", "단단한 껍질", "전투 시작 시 방어력 +3",
                RelicTrigger.BattleStart, RelicEffectType.DEFUp, 3, RewardRarity.Common, price: 90,
                new[] { Kw(KeywordType.DEFUp, 3, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_DragonHeart", "드래곤의 심장", "전투 시작 시 최대 HP +50",
                RelicTrigger.BattleStart, RelicEffectType.MaxHPUp, 50, RewardRarity.Unique, price: 200,
                new[] { Kw(KeywordType.MaxHPUp, 50, KeywordTrigger.OnBattleStart) });

            Debug.Log($"[DataGenerator] 유물 데이터 생성 완료");
        }

        private static void CreateRelic(string fileName, string relicName, string desc,
            RelicTrigger trigger, RelicEffectType effectType, int effectValue, RewardRarity rarity, int price = 0,
            KeywordEntry[] keywords = null)
        {
            var path = $"{RELIC_PATH}/{fileName}.asset";
            var relic = GetOrCreateAsset<RelicData>(path);

            // 필드 설정 via SerializedObject
            var so = new SerializedObject(relic);
            SetField(so, "_relicName", relicName);
            SetField(so, "_description", desc);
            SetField(so, "_trigger", (int)trigger);
            SetField(so, "_effectType", (int)effectType);
            SetField(so, "_effectValue", effectValue);
            SetField(so, "_rarity", (int)rarity);
            SetField(so, "_price", price);
            so.ApplyModifiedProperties();

            // 키워드 설정 via SetPrivateField (SerializedObject 배열은 복잡하므로)
            if (keywords != null && keywords.Length > 0)
                SetPrivateField(relic, "_keywords", keywords);

            EditorUtility.SetDirty(relic);
        }

        private static void SetField(SerializedObject so, string fieldName, int value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null) prop.intValue = value;
        }

        private static void SetField(SerializedObject so, string fieldName, string value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null) prop.stringValue = value;
        }

        #endregion
    }
}
#endif
