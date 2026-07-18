using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Skill;
using TeamLog.UI;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 전투 UI 공통 표시 유틸리티 — 중복 로직 통합
    /// </summary>
    public static class BattleDisplayUtil
    {
        // 공용 흰색 스프라이트 — sprite 없는 Image는 raycast가 무시되는 버그 방지 (ResourceBadge 원형 테두리용)
        private static Sprite _whiteSprite;
        public static Sprite WhiteSprite => _whiteSprite ??= Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, 4f, 4f),
            new Vector2(0.5f, 0.5f),
            100f, 0u, SpriteMeshType.FullRect, Vector4.zero);
        /// <summary>
        /// 상태이상 타입 → 한국어 라벨
        /// </summary>
        public static string GetEffectLabel(StatusEffectType type) => type switch
        {
            StatusEffectType.Poison => "독",
            StatusEffectType.Burn => "화상",
            StatusEffectType.Stun => "기절",
            StatusEffectType.Freeze => "빙결",
            StatusEffectType.Sleep => "수면",
            StatusEffectType.Bleed => "출혈",
            StatusEffectType.DefenseUp => "방어 증가",
            StatusEffectType.DefenseDown => "방어 감소",
            StatusEffectType.AttackUp => "공격 증가",
            StatusEffectType.AttackDown => "공격 감소",
            StatusEffectType.Regeneration => "재생",
            StatusEffectType.Shield => "보호막",
            StatusEffectType.Taunt => "도발",
            _ => type.ToString()
        };

        /// <summary>
        /// 상태이상 타입 → 뱃지 배경색 (UIPalette 토큰 참조)
        /// </summary>
        public static Color GetEffectColor(StatusEffectType type)
        {
            var p = UIPalette.Default;
            return type switch
            {
                // 디버프 (빨간/보라 계열)
                StatusEffectType.Poison => p.EffectPoison,
                StatusEffectType.Burn => p.EffectBurn,
                StatusEffectType.Stun => p.EffectStun,
                StatusEffectType.Freeze => p.EffectFreeze,
                StatusEffectType.Sleep => p.EffectSleep,
                StatusEffectType.Bleed => p.EffectBleed,
                StatusEffectType.DefenseDown => p.EffectDefenseDown,
                StatusEffectType.AttackDown => p.EffectAttackDown,
                // 버프 (녹색/청록 계열)
                StatusEffectType.DefenseUp => p.EffectDefenseUp,
                StatusEffectType.AttackUp => p.EffectAttackUp,
                StatusEffectType.Regeneration => p.EffectRegeneration,
                StatusEffectType.Shield => p.EffectShield,
                // 특수
                StatusEffectType.Taunt => p.EffectTaunt,
                _ => p.EffectDefault
            };
        }

        /// <summary>
        /// 상태이상 타입 → 한국어 이니셜 (색맹 지원용)
        /// </summary>
        public static string GetEffectInitial(StatusEffectType type) => type switch
        {
            StatusEffectType.Poison => "독",
            StatusEffectType.Burn => "화",
            StatusEffectType.Stun => "기",
            StatusEffectType.Freeze => "빙",
            StatusEffectType.Sleep => "수",
            StatusEffectType.Bleed => "출",
            StatusEffectType.DefenseUp => "방↑",
            StatusEffectType.DefenseDown => "방↓",
            StatusEffectType.AttackUp => "공↑",
            StatusEffectType.AttackDown => "공↓",
            StatusEffectType.Regeneration => "재",
            StatusEffectType.Shield => "쉴",
            StatusEffectType.Taunt => "도",
            _ => ""
        };

        /// <summary>
        /// 상태이상 타입 → 설명 문구
        /// </summary>
        public static string GetEffectDescription(StatusEffectType type) => type switch
        {
            StatusEffectType.Poison => "매 턴 시작 시 수치만큼 피해를 받습니다.",
            StatusEffectType.Burn => "매 턴 시작 시 수치만큼 화염 피해를 받습니다.",
            StatusEffectType.Stun => "행동할 수 없습니다. 턴을 건너뜁니다.",
            StatusEffectType.Freeze => "행동할 수 없으며, 받는 피해가 증가합니다.",
            StatusEffectType.Sleep => "피해를 받으면 깨어납니다. 그 전까지 행동 불가.",
            StatusEffectType.Bleed => "매 턴 시작 시 수치만큼 물리 피해를 받습니다.",
            StatusEffectType.DefenseUp => "방어력이 수치만큼 증가합니다.",
            StatusEffectType.DefenseDown => "방어력이 수치만큼 감소합니다.",
            StatusEffectType.AttackUp => "공격력이 수치만큼 증가합니다.",
            StatusEffectType.AttackDown => "공격력이 수치만큼 감소합니다.",
            StatusEffectType.Regeneration => "매 턴 시작 시 수치만큼 HP를 회복합니다.",
            StatusEffectType.Shield => "수치만큼 피해를 흡수하는 보호막이 생성됩니다.",
            StatusEffectType.Taunt => "적의 공격 대상이 이 캐릭터로 고정됩니다.",
            _ => "알 수 없는 상태 효과입니다."
        };

        #region Trait Display

        public static string GetTraitLabel(EnemyTrait trait) => trait switch
        {
            // 일반 적
            EnemyTrait.Regenerate => "재생",
            EnemyTrait.Opportunist => "약자 노림",
            EnemyTrait.PhaseShift => "위상 변이",
            EnemyTrait.Counter => "반격",
            EnemyTrait.Thorns => "가시",
            EnemyTrait.Shell => "껍질",
            // 엘리트
            EnemyTrait.Sturdy => "견고",
            EnemyTrait.ArcaneFury => "마력 폭주",
            EnemyTrait.Corrosive => "부식",
            // 보스
            EnemyTrait.Rally => "소집령",
            EnemyTrait.Rampage => "연소",
            EnemyTrait.Immortal => "불사",
            _ => ""
        };

        public static Color GetTraitColor(EnemyTrait trait)
        {
            var p = UIPalette.Default;
            return trait switch
            {
                // 일반 적
                EnemyTrait.Regenerate => p.TraitRegenerate,
                EnemyTrait.Opportunist => p.TraitOpportunist,
                EnemyTrait.PhaseShift => p.TraitPhaseShift,
                EnemyTrait.Counter => p.TraitCounter,
                EnemyTrait.Thorns => p.TraitThorns,
                EnemyTrait.Shell => p.TraitShell,
                // 엘리트
                EnemyTrait.Sturdy => p.TraitSturdy,
                EnemyTrait.ArcaneFury => p.TraitArcaneFury,
                EnemyTrait.Corrosive => p.TraitCorrosive,
                // 보스
                EnemyTrait.Rally => p.TraitRally,
                EnemyTrait.Rampage => p.TraitRampage,
                EnemyTrait.Immortal => p.TraitImmortal,
                _ => p.TraitDefault
            };
        }

        public static string GetTraitDescription(EnemyTrait trait) => trait switch
        {
            // 일반 적
            EnemyTrait.Regenerate => "턴 시작 시 HP 5 회복. 독/화상 상태면 회복 불가.",
            EnemyTrait.Opportunist => "항상 HP가 가장 낮은 대상을 공격합니다. (도발 무시)",
            EnemyTrait.PhaseShift => "홀수 턴: 방어력 +4 / 짝수 턴: 공격력 +4",
            EnemyTrait.Counter => "피격 시 공격자에게 3 고정 데미지 반격.",
            EnemyTrait.Thorns => "피격 시 받은 피해의 30%를 공격자에게 반사.",
            EnemyTrait.Shell => "매 턴 첫 번째 상태이상을 무효화합니다.",
            // 엘리트
            EnemyTrait.Sturdy => "매 턴 첫 번째 공격의 데미지를 50% 감소시킵니다.",
            EnemyTrait.ArcaneFury => "HP가 50% 이하가 되면 즉시 공격력 +5.",
            EnemyTrait.Corrosive => "피해를 입힌 대상에게 방어 감소 디버프를 부여합니다.",
            // 보스
            EnemyTrait.Rally => "HP 50% 이하 시 공격력 +8, 방어력 +4 획득 (2턴).",
            EnemyTrait.Rampage => "피해를 입지 않은 턴마다 공격력 +3 누적. 피해를 입으면 초기화.",
            EnemyTrait.Immortal => "치명적 피해 시 HP 1로 생존합니다. (1회)",
            _ => ""
        };

        #endregion

        // ═══════════════════════════════════════════
        // Phase CC 자원 (Resource) 헬퍼
        // ═══════════════════════════════════════════

        public static string GetResourceLabel(ResourceType type) => type switch
        {
            ResourceType.Ember => "잿빛",
            ResourceType.Vengeance => "복수",
            ResourceType.Frost => "서리",
            ResourceType.Prophecy => "예언",
            ResourceType.Charge => "전하",
            ResourceType.Shadows => "그림자",
            ResourceType.Combo => "연사",
            ResourceType.Mercy => "자비",
            ResourceType.Melody => "선율",
            _ => "자원",
        };

        public static string GetResourceInitial(ResourceType type) => type switch
        {
            ResourceType.Ember => "잿",
            ResourceType.Vengeance => "복",
            ResourceType.Frost => "설",
            ResourceType.Prophecy => "예",
            ResourceType.Charge => "전",
            ResourceType.Shadows => "그",
            ResourceType.Combo => "콤",
            ResourceType.Mercy => "엘",
            ResourceType.Melody => "선",
            _ => "?",
        };

        public static Color GetResourceColor(ResourceType type)
        {
            var p = UIPalette.Default;
            return type switch
            {
                ResourceType.Ember => p.ResourceEmber,
                ResourceType.Vengeance => p.ResourceVengeance,
                ResourceType.Frost => p.ResourceFrost,
                ResourceType.Prophecy => p.ResourceProphecy,
                ResourceType.Shadows => new Color(0.45f, 0.25f, 0.65f), // 보라/그림자 (Umbra)
                ResourceType.Combo => new Color(0.90f, 0.35f, 0.20f), // 주황/폭우 (Aster)
                ResourceType.Mercy => new Color(0.95f, 0.85f, 0.30f), // 황금/자비 (Elara)
                ResourceType.Melody => new Color(0.40f, 0.70f, 0.95f), // 청록/선율 (Calliope)
                _ => p.ResourceDefault,
            };
        }

        public static string GetResourceDescription(ResourceType type) => type switch
        {
            ResourceType.Ember => "매 턴 +1 축적, 턴 종료 시 현재 잿빛×2 자해. 최대 5.",
            ResourceType.Vengeance => "피격/쉴드 흡수 시 데미지 1:1 축적. 최대 20.",
            ResourceType.Frost => "매 턴 종료 시 절반 소실. 최대 3.",
            ResourceType.Prophecy => "1턴 뒤 발동 예약. 매 턴 시작 시 발동.",
            ResourceType.Charge => "매 턴 종료 시 다른 전하 적에게 스택 수만큼 도트.",
            ResourceType.Shadows => "안 맞을 때 +1, 맞으면 리셋. 3스택 시 치명타 100%/2배. (Umbra)",
            ResourceType.Combo => "스킬 사용 시 +1, 미사용 시 리셋. 최대 3. 다타수/Execute 위력. (Aster)",
            ResourceType.Mercy => "매 턴 자동 힐 + 축전. 15 도달 시 자동 ATK+3 버스트. (Elara)",
            ResourceType.Melody => "매 턴 주 선율 + 직전 부 선율(50%). 같은 스킬 연속 시 부 무효. (Calliope)",
            _ => "캐릭터 고유 자원",
        };

        /// <summary>
        /// 쉴드 바 앵커 갱신 — HP 바 끝점부터 겹쳐서 표시
        /// </summary>
        public static void UpdateShieldBar(Image shieldFill, float hpRatio, int shield, int maxHP)
        {
            if (shieldFill == null) return;

            if (shield > 0 && maxHP > 0)
            {
                float shieldEnd = Mathf.Min(1f, hpRatio + (float)shield / maxHP);
                shieldFill.rectTransform.anchorMin = new Vector2(hpRatio, 0f);
                shieldFill.rectTransform.anchorMax = new Vector2(shieldEnd, 1f);
                shieldFill.gameObject.SetActive(true);
            }
            else
            {
                shieldFill.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 스킬 수치 요약 문자열 생성 (위력, 상태이상, 자원, BehaviorTag 등).
        /// 공격 스킬은 캐릭터 ATK 포함하여 최종 위력 계산.
        /// Phase CC/UNIFIED-P: 자원 비례 위력, 자원 획득/소모, BehaviorTag(조건부 보너스/특수 효과), ShieldFlags 표시.
        /// </summary>
        public static string BuildSkillDescription(SkillData skill, Character caster, string separator = " | ")
        {
            var parts = new List<string>();

            // Phase CC-2E: 발견 스킬 — 풀 카테고리 표시 후 종료
            if (skill.IsDiscover && skill.DiscoverPool != null)
            {
                parts.Add($"발견: {GetDiscoverCategoryLabel(skill.DiscoverPool.Category)}");
                int poolSize = skill.DiscoverPool.EntryCount;
                int choices = DiscoverSystem.DEFAULT_CHOICE_COUNT;
                if (caster?.PlayerTraitHandler != null)
                    choices = DiscoverSystem.GetChoiceCount(caster);
                parts.Add($"{choices}개 선택지 (풀 {poolSize}개)");
                return string.Join(separator, parts);
            }

            if (skill.Power > 0)
            {
                int displayPower = skill.Type == SkillType.Attack && caster != null
                    ? caster.Stats.GetStat(StatType.ATK) + skill.Power
                    : skill.Power;

                string label = skill.Type switch
                {
                    SkillType.Attack => "위력",
                    SkillType.Shield => "쉴드",
                    SkillType.Heal => "회복",
                    SkillType.Buff => "수치",
                    SkillType.Debuff => "수치",
                    SkillType.Purify => "수치",
                    _ => "수치"
                };

                // Phase CC: 자원 비례 위력 표시 (+Ember×3 등)
                string powerSuffix = "";
                if (skill.ResourcePowerPerStack > 0)
                {
                    ResourceType resType = InferResourceType(skill, caster);
                    if (resType != ResourceType.None)
                        powerSuffix = $"+{GetResourceInitial(resType)}×{skill.ResourcePowerPerStack}";
                }
                parts.Add($"{label} {displayPower}{powerSuffix}");
            }

            if (skill.StatusEffect != StatusEffectType.None)
            {
                string effectName = GetEffectLabel(skill.StatusEffect);
                string duration = skill.EffectDuration > 0 ? $" ({skill.EffectDuration}턴)" : "";
                string value = skill.EffectValue > 0 ? $" {skill.EffectValue}" : "";
                parts.Add($"{effectName}{value}{duration}");
            }

            // Phase CC: 자원 획득
            if (skill.ResourceGainType != ResourceType.None && skill.ResourceGainAmount > 0)
                parts.Add($"{GetResourceLabel(skill.ResourceGainType)} +{skill.ResourceGainAmount}");

            // Phase CC: 자원 소모 (전량 소모 vs 고정)
            if (skill.ConsumeAllResource && skill.ResourceCostType != ResourceType.None)
                parts.Add($"{GetResourceLabel(skill.ResourceCostType)} 전량 소모");
            else if (skill.ResourceCostType != ResourceType.None && skill.ResourceCostAmount > 0)
                parts.Add($"{GetResourceLabel(skill.ResourceCostType)} {skill.ResourceCostAmount} 소모");

            // Phase CC: ShieldFlags (Taranis Grounding Field)
            if (skill.Type == SkillType.Shield && skill.ShieldFlags != ShieldFlag.None)
            {
                if ((skill.ShieldFlags & ShieldFlag.GivesChargeOnAbsorb) != 0)
                    parts.Add("흡수 시 전하 부여");
            }

            // Phase ARCH/CC: BehaviorTag 요약 (조건부 보너스/특수 효과)
            if (skill.Behaviors != null)
            {
                foreach (var tag in skill.Behaviors)
                {
                    string b = GetBehaviorLabel(tag);
                    if (!string.IsNullOrEmpty(b))
                        parts.Add(b);
                }
            }

            return string.Join(separator, parts);
        }

        /// <summary>자원 비례 위력 표시용 자원 종류 추론 — SkillData 우선, caster.Resource 차선.</summary>
        private static ResourceType InferResourceType(SkillData skill, Character caster)
        {
            if (skill.ResourceGainType != ResourceType.None) return skill.ResourceGainType;
            if (skill.ResourceCostType != ResourceType.None) return skill.ResourceCostType;
            if (caster?.Resource != null) return caster.Resource.Resource;
            return ResourceType.None;
        }

        /// <summary>발견 카테고리 → 한국어 라벨 (Phase CC-2E).</summary>
        public static string GetDiscoverCategoryLabel(DiscoverCategory category) => category switch
        {
            DiscoverCategory.Mending => "회복",
            DiscoverCategory.Strengthening => "버프",
            DiscoverCategory.Crippling => "디버프",
            DiscoverCategory.Catalytic => "유틸리티",
            _ => "발견"
        };

        /// <summary>
        /// BehaviorTag 한국어 라벨 (UI 요약용). 주요 Behavior만 표시, 기타는 null (표시 생략).
        /// rank가 의미 있는 경우 N으로 치환.
        /// </summary>
        public static string GetBehaviorLabel(TeamLog.Skill.BehaviorTag tag)
        {
            switch (tag.Keyword)
            {
                // Phase CC 핵심
                case BehaviorKeyword.Berserk: return "HP≤50% 2배";
                case BehaviorKeyword.Propagate: return "전파";
                case BehaviorKeyword.TargetFreeze: return $"빙결 적 +{tag.Rank}";
                case BehaviorKeyword.CleanseLowTarget: return "대상 HP≤50% 정화";
                case BehaviorKeyword.ResourceThresholdShield: return $"자원 {tag.Rank}+ 강화";

                // Phase BK/ARCH 핵심
                case BehaviorKeyword.HeavyHit: return "2배 (코스트+1)";
                case BehaviorKeyword.Pierce: return "방어/쉴드 무시";
                case BehaviorKeyword.Execution: return $"HP {tag.Rank}↓ 즉사";
                case BehaviorKeyword.Lifesteal: return "흡혈";
                case BehaviorKeyword.Chain: return $"연쇄 {tag.Rank}";
                case BehaviorKeyword.VenomTouch: return $"중독 {tag.Rank}스택";
                case BehaviorKeyword.BurningTouch: return $"화상 {tag.Rank}스택";
                case BehaviorKeyword.FreezeTouch: return $"빙결 {tag.Rank}스택";

                // Phase ARCH-4 조건부
                case BehaviorKeyword.FirstBlood: return $"풀피 적 +{tag.Rank}";
                case BehaviorKeyword.TargetFullHP: return $"풀피 적 +{tag.Rank}";
                case BehaviorKeyword.Cull: return $"절반 이하 +{tag.Rank}";
                case BehaviorKeyword.Desperation: return $"잃은 HP 비례 +";
                case BehaviorKeyword.Wound: return $"잃은 HP 비례 -";
                case BehaviorKeyword.GiantSlayer: return $"강적 +{tag.Rank}";
                case BehaviorKeyword.Dominance: return $"적 HP<나 +{tag.Rank}";
                case BehaviorKeyword.Bulwark: return $"쉴드 보유 +{tag.Rank}";
                case BehaviorKeyword.AllIn: return $"AP 0 시 +{tag.Rank}";
                case BehaviorKeyword.Bounty: return $"킬 시 회복";
                case BehaviorKeyword.FollowUp: return $"이미 맞은 적 +{tag.Rank}";
                case BehaviorKeyword.Echo: return "위력 절반 2회";
                case BehaviorKeyword.LimitBreak: return $"전투당 1회 +{tag.Rank}";
                case BehaviorKeyword.Explosion: return $"전하 폭발 (스택×3)";
                case BehaviorKeyword.Momentum: return $"사용 시 위력 +{tag.Rank} 누적";
                case BehaviorKeyword.Fatigue: return $"사용 시 위력 -{tag.Rank} 누적";

                default: return null; // PowerUp/Spread/Bounce/MultiHit 등은 별도 표기 또는 생략
            }
        }
    }
}
