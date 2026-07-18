using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Skill;
using TeamLog.UI;

namespace TeamLog.UI.PartySelection
{
    /// <summary>
    /// Party Selection UI 공통 표시 유틸리티 (UI-B.1).
    /// 자원/스킬타입/타겟 색상·라벨 매핑. UIPalette의 DF 토큰 기반.
    /// 기존 BattleDisplayUtil과 별개 — 파티 선택 화면 전용 디자인 토큰 사용.
    /// </summary>
    public static class PartySelectionUIUtils
    {
        // ── 자원 색상 (UIPalette 값 사용) ──
        public static Color GetResourceColor(ResourceType type)
        {
            var p = UIPalette.Default;
            return type switch
            {
                ResourceType.Ember     => p.ResourceEmber,
                ResourceType.Vengeance => p.ResourceVengeance,
                ResourceType.Frost     => p.ResourceFrost,
                ResourceType.Prophecy  => p.ResourceProphecy,
                ResourceType.Charge    => p.ResourceCharge,
                ResourceType.Shadows   => p.ResourceShadows,
                ResourceType.Combo     => p.ResourceCombo,
                ResourceType.Mercy     => p.ResourceMercy,
                ResourceType.Melody    => p.ResourceMelody,
                _ => p.ResourceDefault,
            };
        }

        /// <summary>
        /// 캐릭터 ID 기반 자원 색상 — Mortis(Corpse), Cael(Discover)은 아직 ResourceType enum에 없으므로
        /// 캐릭터 ID로 매핑. 향후 ResourceType 확장 시 제거 가능.
        /// </summary>
        public static Color GetResourceColorByCharId(string charId, ResourceType fallback = ResourceType.None)
        {
            if (string.IsNullOrEmpty(charId)) return UIPalette.Default.ResourceDefault;

            // 소문자 비교
            string id = charId.ToLowerInvariant();

            // Mortis / Cael 예외 처리
            if (id.Contains("mortis")) return UIPalette.Default.ResourceCorpse;
            if (id.Contains("cael"))   return UIPalette.Default.ResourceDiscover;

            return GetResourceColor(fallback);
        }

        // ── 자원 라벨 ──
        public static string GetResourceLabel(ResourceType type) => type switch
        {
            ResourceType.Ember     => "EMBER",
            ResourceType.Vengeance => "VENGEANCE",
            ResourceType.Frost     => "FROST",
            ResourceType.Prophecy  => "PROPHECY",
            ResourceType.Charge    => "CHARGE",
            ResourceType.Shadows   => "SHADOWS",
            ResourceType.Combo     => "COMBO",
            ResourceType.Mercy     => "MERCY",
            ResourceType.Melody    => "MELODY",
            _ => "—",
        };

        public static string GetResourceLabelByCharId(string charId)
        {
            if (string.IsNullOrEmpty(charId)) return "—";
            string id = charId.ToLowerInvariant();
            if (id.Contains("mortis")) return "CORPSE";
            if (id.Contains("cael"))   return "DISCOVER";
            return "—";
        }

        // ── 자원 이니셜 (배지 중앙 표시) ──
        public static string GetResourceInitial(ResourceType type) => type switch
        {
            ResourceType.Ember     => "E",
            ResourceType.Vengeance => "V",
            ResourceType.Frost     => "F",
            ResourceType.Prophecy  => "P",
            ResourceType.Charge    => "C",
            ResourceType.Shadows   => "S",
            ResourceType.Combo     => "C",
            ResourceType.Mercy     => "M",
            ResourceType.Melody    => "♪",
            _ => "?",
        };

        public static string GetResourceInitialByCharId(string charId)
        {
            if (string.IsNullOrEmpty(charId)) return "?";
            string id = charId.ToLowerInvariant();
            if (id.Contains("mortis")) return "X";
            if (id.Contains("cael"))   return "A";
            return "?";
        }

        // ── 자원 메커니즘 설명 (웹 목업 기준) ──
        public static string GetResourceMechanicText(ResourceType type, string charId = null)
        {
            // charId 우선 (Mortis/Cael)
            if (!string.IsNullOrEmpty(charId))
            {
                string id = charId.ToLowerInvariant();
                if (id.Contains("mortis"))
                    return "적 처치 시 <b>시체 자원</b> 획득. 시체를 소환/폭발/흡수하여 전장 조작. 간접 딜러.";
                if (id.Contains("cael"))
                    return "<b>하스스톤式 발견</b> — 스킬마다 3개 중 1 랜덤 선택지. 매 시전 다른 효과. 도트+힐 하이브리드.";
            }

            return type switch
            {
                ResourceType.Ember =>
                    "매 턴 시작 시 <b>Ember +1</b> 자동 부여. 턴 종료 시 <b>Ember × 2 자해 피해</b>. 방치 시 약 11턴 후 자멸.",
                ResourceType.Vengeance =>
                    "피격 시 <b>Vengeance +1</b>. 일부 스킬로 추가 획득. 공격 스킬 사용 시 <b>Vengeance 소비</b>하여 위력 증폭.",
                ResourceType.Frost =>
                    "스킬로 <b>Frost 충전/소비</b>. Frost가 높을수록 얼음 마법 위력 증폭. 적을 <b>Freeze</b>로 행동 봉쇄.",
                ResourceType.Prophecy =>
                    "모든 스킬은 <b>1턴 뒤 발동</b>. Prophecy 충전으로 지연 효과 강화. 3턴마다 Hand of Fate 자동 시전.",
                ResourceType.Charge =>
                    "적에게 <b>Wire 부착</b> → 매 턴 종료 시 자동 <b>연쇄 번개</b>. Wire는 인접 적에게 전파. Charge로 연쇄 강화.",
                ResourceType.Shadows =>
                    "<b>Shadows 축적</strong> 시 치명타 확률 증가. 치명타 시 추가 효과. 그림자 속에서 은신/암살.",
                ResourceType.Combo =>
                    "매 턴 적에게 명중 시 <b>Combo +1</b>. 사용 시 <b>Momentum</b> 위력 보너스 (연속 사용 누적).",
                ResourceType.Mercy =>
                    "힐/정화 시 <b>Mercy 축적</b>. Mercy 소비하여 영구 버프 부여. 생명 순환 서포터.",
                ResourceType.Melody =>
                    "스킬 사용 시 <b>주 선율</b> 또는 <b>부 선율</b> 발동. Melody 4스택 시 <b>Grand Finale</b>로 폭발.",
                _ => "이 캐릭터는 고유 자원이 없습니다.",
            };
        }

        // ── 스킬 타입 색상 ──
        public static Color GetSkillTypeColor(SkillType type)
        {
            var p = UIPalette.Default;
            return type switch
            {
                SkillType.Attack => p.SkillAttack,
                SkillType.Heal   => p.SkillHeal,
                SkillType.Buff   => p.SkillBuff,
                SkillType.Debuff => p.SkillDebuff,
                SkillType.Shield => p.SkillShield,
                SkillType.Purify => p.SkillPurify,
                _ => Color.white,
            };
        }

        public static string GetSkillTypeLabel(SkillType type) => type switch
        {
            SkillType.Attack => "공격",
            SkillType.Heal   => "치유",
            SkillType.Buff   => "강화",
            SkillType.Debuff => "약화",
            SkillType.Shield => "방어",
            SkillType.Purify => "정화",
            _ => "?",
        };

        // 특수 스킬 타입 (Summon/Special) — SkillType enum에 없으므로 SkillData 플래그 기반
        public static Color GetSkillSpecialColor(bool isDiscover, bool isSummon)
        {
            var p = UIPalette.Default;
            if (isDiscover) return p.SkillSpecial;
            if (isSummon)  return p.SkillSummon;
            return p.SkillAttack;
        }

        // ── 타겟 색상 ──
        public static Color GetTargetColor(TargetType type)
        {
            var p = UIPalette.Default;
            return type switch
            {
                TargetType.SingleEnemy => p.AccentRed,
                TargetType.AllEnemies  => p.SkillSpecial,
                TargetType.SingleAlly  => p.AccentGreen,
                TargetType.AllAllies   => p.AccentGreen,
                TargetType.Self        => p.DFGoldL,
                _ => Color.white,
            };
        }

        public static string GetTargetLabel(TargetType type) => type switch
        {
            TargetType.SingleEnemy => "단일 적",
            TargetType.AllEnemies  => "전체 적",
            TargetType.SingleAlly  => "아군 1명",
            TargetType.AllAllies   => "전체 아군",
            TargetType.Self        => "자신",
            _ => "?",
        };

        // ── 스킬 설명 텍스트 빌드 (자원/행동 키워드 기반) ──
        /// <summary>
        /// SkillData에서 스킬 카드 설명 텍스트 생성.
        /// ResourceGain/Cost, ResourcePowerPerStack, StatusEffect, Behaviors를 종합.
        /// </summary>
        public static string BuildSkillDescription(SkillData skill)
        {
            var sb = new StringBuilder();
            var p = UIPalette.Default;

            // 기본 설명 (Description)
            if (!string.IsNullOrEmpty(skill.Description))
            {
                sb.Append(skill.Description);
            }
            else
            {
                // Description이 비었을 경우 자동 생성
                sb.Append(GetSkillTypeLabel(skill.Type));
                sb.Append(" — ");
                if (skill.Power > 0) sb.Append($"{skill.Power} 위력");
            }

            // 자원 변화
            var resParts = new List<string>();
            if (skill.ResourceGainType != ResourceType.None && skill.ResourceGainAmount > 0)
                resParts.Add($"자원 +{skill.ResourceGainAmount}");
            if (skill.ResourceCostType != ResourceType.None)
            {
                if (skill.ConsumeAllResource)
                    resParts.Add("자원 전부 소비");
                else if (skill.ResourceCostAmount > 0)
                    resParts.Add($"자원 -{skill.ResourceCostAmount}");
            }
            if (resParts.Count > 0)
            {
                sb.AppendLine();
                sb.Append(string.Join(" / ", resParts));
            }

            // 자원 비례 위력
            if (skill.ResourcePowerPerStack > 0)
            {
                sb.AppendLine();
                sb.Append($"자원 1당 위력 +{skill.ResourcePowerPerStack}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 조건부 보너스 텍스트 (있을 경우). BehaviorTag 기반.
        /// 반환값이 null이면 보너스 박스 표시 안 함.
        /// </summary>
        public static (string text, bool isRestriction) BuildSkillBonusText(SkillData skill)
        {
            // 최소 자원 요구 (사용 제약)
            if (skill.MinResourceRequired > 0)
            {
                return ($"사용 제약: {GetResourceLabel(skill.ResourceCostType)} {skill.MinResourceRequired} 필수", true);
            }

            // BehaviorTag 기반 보너스 텍스트 매핑
            var bonusParts = new List<string>();
            foreach (var bt in skill.Behaviors)
            {
                string part = bt.Keyword switch
                {
                    BehaviorKeyword.Desperation => "자신 HP 50% 이하 시 위력 2배",
                    BehaviorKeyword.Berserk     => "현재 자원에 비례하여 위력 증가",
                    BehaviorKeyword.FirstBlood  => "풀피 적 대상 시 위력 +",
                    BehaviorKeyword.Cull        => "대상 HP 절반 이하 시 위력 +",
                    BehaviorKeyword.TargetFullHP => "풀피 대상 +N 위력",
                    BehaviorKeyword.GiantSlayer => "적 MaxHP 비례 위력 증가",
                    BehaviorKeyword.AllIn       => "AP 0일 때 위력 대폭 증가",
                    BehaviorKeyword.Bulwark     => "쉴드 부여 + 추가 방어 효과",
                    BehaviorKeyword.Dominance   => "적 HP < 자신 HP 시 위력 +",
                    BehaviorKeyword.Bounty      => "처치 시 자원 획득",
                    BehaviorKeyword.FollowUp    => "이미 맞은 적 추가 타격",
                    BehaviorKeyword.Echo        => "위력 절반으로 2회 타격",
                    BehaviorKeyword.LimitBreak  => "전투당 1회 — 강력 효과",
                    BehaviorKeyword.Momentum    => "연속 사용 시 위력 증가",
                    BehaviorKeyword.Fatigue     => "연속 사용 시 위력 감소",
                    BehaviorKeyword.Escalation  => "연속 사용 시 비용 증가",
                    BehaviorKeyword.Mastery     => "연속 사용 시 비용 감소",
                    BehaviorKeyword.Explosion   => "광역 추가 데미지",
                    _ => null,
                };
                if (!string.IsNullOrEmpty(part)) bonusParts.Add(part);
            }

            if (bonusParts.Count == 0) return (null, false);
            return (string.Join(" / ", bonusParts), false);
        }

        // ── 자원 Sprite 로드 헬퍼 ──
        static readonly Dictionary<ResourceType, Sprite> _badgeSpriteCache = new();

        /// <summary>
        /// 자원별 배지 Sprite 로드 (PartySelectionSpriteGenerator가 생성한 에셋).
        /// </summary>
        public static Sprite GetResourceBadgeSprite(ResourceType type, string charId = null)
        {
            if (_badgeSpriteCache.TryGetValue(type, out var s)) return s;

            string resName = ResolveResourceBadgeName(type, charId);
            if (string.IsNullOrEmpty(resName)) return null;

            string path = $"Assets/03.Data/UI/PartySelection/ResourceBadge_{resName}.png";
#if UNITY_EDITOR
            s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
#else
            s = Resources.Load<Sprite>($"UI/PartySelection/ResourceBadge_{resName}");
#endif
            if (s != null) _badgeSpriteCache[type] = s;
            return s;
        }

        private static string ResolveResourceBadgeName(ResourceType type, string charId)
        {
            // charId 우선 (Mortis/Cael)
            if (!string.IsNullOrEmpty(charId))
            {
                string id = charId.ToLowerInvariant();
                if (id.Contains("mortis")) return "Corpse";
                if (id.Contains("cael"))   return "Discover";
            }
            return type switch
            {
                ResourceType.Ember     => "Ember",
                ResourceType.Vengeance => "Vengeance",
                ResourceType.Frost     => "Frost",
                ResourceType.Prophecy  => "Prophecy",
                ResourceType.Charge    => "Charge",
                ResourceType.Shadows   => "Shadows",
                ResourceType.Combo     => "Combo",
                ResourceType.Mercy     => "Mercy",
                ResourceType.Melody    => "Melody",
                _ => null,
            };
        }

        // ── 캐릭터 메타데이터 (웹 목업 데이터) ──
        // 향후 CharacterData SO에 추가되거나 별도 DisplayData SO로 분리 가능

        /// <summary>
        /// 캐릭터 ID → 거대 초상화 이니셜 (플레이스홀더).
        /// </summary>
        public static string GetCharacterInitial(string charId)
        {
            if (string.IsNullOrEmpty(charId)) return "?";
            string id = charId.ToLowerInvariant();
            if (id.Contains("ashe"))     return "A";
            if (id.Contains("duran"))    return "D";
            if (id.Contains("lumi"))     return "L";
            if (id.Contains("sibyl"))    return "S";
            if (id.Contains("taranis"))  return "T";
            if (id.Contains("umbra"))    return "U";
            if (id.Contains("aster") || id == "archer" || id.Contains("_archer")) return "A"; // Aster = 기존 Archer
            if (id.Contains("mortis") || id == "necromancer" || id.Contains("_necromancer")) return "M";
            if (id.Contains("cael") || id == "alchemist" || id.Contains("_alchemist")) return "C";
            if (id.Contains("calliope") || id == "bard" || id.Contains("_bard")) return "C";
            if (id.Contains("elara") || id == "healer" || id.Contains("_healer")) return "E";
            return charId.Length > 0 ? charId.Substring(0, 1).ToUpper() : "?";
        }

        /// <summary>
        /// 캐릭터 ID → 표시 이름 (영문).
        /// </summary>
        public static string GetCharacterDisplayName(string charId)
        {
            if (string.IsNullOrEmpty(charId)) return "UNKNOWN";
            string id = charId.ToLowerInvariant();
            if (id.Contains("ashe"))     return "ASHE";
            if (id.Contains("duran"))    return "DURAN";
            if (id.Contains("lumi"))     return "LUMI";
            if (id.Contains("sibyl"))    return "SIBYL";
            if (id.Contains("taranis"))  return "TARANIS";
            if (id.Contains("umbra"))    return "UMBRA";
            if (id.Contains("aster") || id == "archer" || id.Contains("_archer")) return "ASTER";
            if (id.Contains("mortis") || id == "necromancer" || id.Contains("_necromancer")) return "MORTIS";
            if (id.Contains("cael") || id == "alchemist" || id.Contains("_alchemist")) return "CAEL";
            if (id.Contains("calliope") || id == "bard" || id.Contains("_bard")) return "CALLIOPE";
            if (id.Contains("elara") || id == "healer" || id.Contains("_healer")) return "ELARA";
            return charId.ToUpperInvariant();
        }

        /// <summary>
        /// 캐릭터 ID → 부제 (the Pyromancer 등).
        /// </summary>
        public static string GetCharacterTitle(string charId)
        {
            if (string.IsNullOrEmpty(charId)) return "";
            string id = charId.ToLowerInvariant();
            if (id.Contains("ashe"))     return "the Pyromancer";
            if (id.Contains("duran"))    return "the Warrior";
            if (id.Contains("lumi"))     return "the Cryomancer";
            if (id.Contains("sibyl"))    return "the Oracle";
            if (id.Contains("taranis"))  return "the Stormcaller";
            if (id.Contains("umbra"))    return "the Rogue";
            if (id.Contains("aster") || id == "archer" || id.Contains("_archer")) return "the Archer";
            if (id.Contains("mortis") || id == "necromancer" || id.Contains("_necromancer")) return "the Necromancer";
            if (id.Contains("cael") || id == "alchemist" || id.Contains("_alchemist")) return "the Alchemist";
            if (id.Contains("calliope") || id == "bard" || id.Contains("_bard")) return "the Bard";
            if (id.Contains("elara") || id == "healer" || id.Contains("_healer")) return "the Healer";
            return "";
        }

        /// <summary>
        /// 캐릭터 ID → 정체성 한 문장.
        /// </summary>
        public static string GetCharacterIdentity(string charId)
        {
            if (string.IsNullOrEmpty(charId)) return "";
            string id = charId.ToLowerInvariant();
            if (id.Contains("ashe"))     return "타오르는 자, 자신을 재로 삼아 적을 불태운다 — 불을 쓰는 자가 아니라, 자신이 타고 있는 자.";
            if (id.Contains("duran"))    return "불멸의 성벽 — 받은 고통을 역심으로 전환하는 자. 맞을수록 더 깊이 벤다.";
            if (id.Contains("lumi"))     return "빙결의 통제자 — 적의 행동을 영원히 얼려 버리는 자.";
            if (id.Contains("sibyl"))    return "미래에 투자하는 예언자 — 오늘 외면, 내일의 강타.";
            if (id.Contains("taranis"))  return "네트워크에 투자하는 폭풍술사 — 한 번 얽힌 적은 끝까지 뇌격.";
            if (id.Contains("umbra"))    return "치명타 암살자 — 그림자 속에서 한 줄 빛이 되어 적을 가른다.";
            if (id.Contains("aster") || id == "archer" || id.Contains("_archer"))
                return "연속 사격의 달인 — 한 발 한 발이 다음 발을 위한 밑거름.";
            if (id.Contains("mortis") || id == "necromancer" || id.Contains("_necromancer"))
                return "영혼 수확자 — 죽은 자가 남긴 것으로 산 자를 벤다.";
            if (id.Contains("cael") || id == "alchemist" || id.Contains("_alchemist"))
                return "시약 반응 촉매자 — 매 스킬이 3가지 가능성을 품은 주사위.";
            if (id.Contains("calliope") || id == "bard" || id.Contains("_bard"))
                return "리듬 지휘자 — 곡이 쌓여 피날레로 터진다.";
            if (id.Contains("elara") || id == "healer" || id.Contains("_healer"))
                return "생명 순환 서포터 — 치유와 정화가 한 몸이 되어 파티를 지킨다.";
            return "";
        }

        /// <summary>
        /// 캐릭터 ID → 강점/약점 텍스트.
        /// </summary>
        public static (string strength, string weakness) GetCharacterStrengthWeakness(string charId)
        {
            if (string.IsNullOrEmpty(charId)) return ("—", "—");
            string id = charId.ToLowerInvariant();
            return id switch
            {
                string s when s.Contains("ashe") =>
                    ("화염 단일 폭딜 + Ember 축적 시 위력 증폭 (Brand of Ash 최대 46 데미지)",
                     "자해 (Ember × 2/턴 누적) — HP 관리 부담. 부활 시스템과 연계 필수"),
                string s when s.Contains("duran") =>
                    ("높은 HP + Vengeance 축적 시 버스트 딜 (Revenge Strike 최대 35)",
                     "비피격 시 출력 0 — 적의 어그로를 끌어야 위력 발휘"),
                string s when s.Contains("lumi") =>
                    ("Freeze 행동 봉쇄 + Frost × 스킬 위력 증폭 (자가 콤보)",
                     "도트 데미지 약함 — 단일 폭딜 부족. 보스전 Freeze 면역 주의"),
                string s when s.Contains("sibyl") =>
                    ("1턴 뒤 강력 효과 + 3턴 주기 콤보 (Hand of Fate 무작위 시전)",
                     "모든 스킬 1턴 뒤 발동 — 즉응 대응 불가, 첫 턴 부담"),
                string s when s.Contains("taranis") =>
                    ("광역 연쇄 (Wire 전파) + Grounding Field 서포트 (쉴드 흡수 시 Charge 역부여)",
                     "단일 보스전 약함 — 네트워크 구축 전 취약"),
                string s when s.Contains("umbra") =>
                    ("Shadows 축적 시 치명타 확률 증가 + FollowUp 연속타 (Backstab 치명타 시 추가 타)",
                     "자원 축전 전 취약 — 초반 딜 부족, 저HP 근접 캐릭터"),
                string s when s.Contains("aster") || s == "archer" || s.Contains("_archer") =>
                    ("Momentum 사용 시 위력 +, 연속 명중 시 폭발 (Quick Shot → Bullseye 콤보)",
                     "자원 획득 조건 엄격 — 매 턴 적중 필수, 빗나가면 콤보 초기화"),
                string s when s.Contains("mortis") || s == "necromancer" || s.Contains("_necromancer") =>
                    ("소환 시체 자동 전투 + 처치 폭발 + 영혼 자원 (자원 루프)",
                     "빈사 적에게 약함 — 본인 직접 딜이 낮아 처치 타이밍 애매"),
                string s when s.Contains("cael") || s == "alchemist" || s.Contains("_alchemist") =>
                    ("물약 효율 + 도트/힐 하이브리드 (상황에 맞춰 선택)",
                     "고AP 비용 + 랜덤 의존 — 원하는 효과 안 나오면 딜레이. 평균 3~4AP"),
                string s when s.Contains("calliope") || s == "bard" || s.Contains("_bard") =>
                    ("Melody 4스택 Grand Finale 폭발 + 다중 버프 (주/부 선율 메아리)",
                     "턴당 스킬 밀도 부족 — 매 턴 행동 설계 까다로움, 빌드업 시간 필요"),
                string s when s.Contains("elara") || s == "healer" || s.Contains("_healer") =>
                    ("힐 + 정화 동시 + 영구 버프 (Mercy 누적). 부활 시스템과 최고 시너지",
                     "자기 생존 약함 + 딜 능력 0 — 파티 완전 의존"),
                _ => ("—", "—"),
            };
        }

        /// <summary>
        /// 캐릭터 ID → 역할 (영문/한글).
        /// </summary>
        public static (string roleEn, string roleKo) GetCharacterRole(string charId)
        {
            if (string.IsNullOrEmpty(charId)) return ("?", "?");
            string id = charId.ToLowerInvariant();
            return id switch
            {
                string s when s.Contains("ashe")     => ("Single Nuke",       "단일 폭딜"),
                string s when s.Contains("duran")    => ("Tank / Burst",      "복수 탱커"),
                string s when s.Contains("lumi")     => ("Control",            "군중 제어"),
                string s when s.Contains("sibyl")    => ("Delayed Nuke",       "지연 폭딜"),
                string s when s.Contains("taranis")  => ("Chain Caster",       "연쇄 딜러"),
                string s when s.Contains("umbra")    => ("Critical Assassin",  "치명타 암살"),
                string s when s.Contains("aster") || s == "archer" || s.Contains("_archer")     => ("Ramp DPS",           "연속 딜러"),
                string s when s.Contains("mortis") || s == "necromancer" || s.Contains("_necromancer") => ("Indirect Caster",    "간접 딜러"),
                string s when s.Contains("cael") || s == "alchemist" || s.Contains("_alchemist") => ("Reaction Caster",    "반응 딜러"),
                string s when s.Contains("calliope") || s == "bard" || s.Contains("_bard")       => ("Buffer / Burst",     "버퍼"),
                string s when s.Contains("elara") || s == "healer" || s.Contains("_healer")      => ("Pure Support",       "순수 서포터"),
                _ => ("?", "?"),
            };
        }
    }
}
