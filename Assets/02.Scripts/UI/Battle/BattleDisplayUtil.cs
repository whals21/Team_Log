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

            // ★ 2026-08-03 P1-Q5: CC(Stun/Freeze/Sleep) 상태 시 맨 앞에 경고 표시
            // StS/Hearthstone 표준 — CC 걸린 캐릭터 스킬은 행동 불가 명시
            if (caster?.StatusEffects != null && caster.StatusEffects.IsIncapacitated)
            {
                parts.Add(GetCCStatusLabel(caster.StatusEffects));
            }

            // ★ 2026-08-03 P0-R3 (개선): 슬롯용 수치+라벨 형식
            // 기존: "위력 8+5×3=23 | HP≤50% 2배 | Mercy 5 소모"
            // 신규: "23 데미지 | 2 화상 (2턴) | 잿빛 +2"
            // BehaviorTag/★활성/자원 비례 수식은 툴팁(BuildTooltipDescription)에서만.

            // 1. 위력/힐/쉴드/버프 (자원 비례 자동 반영)
            if (skill.Power > 0)
            {
                int displayPower = skill.Type == SkillType.Attack && caster != null
                    ? caster.Stats.GetStat(StatType.ATK) + skill.Power
                    : skill.Power;

                // 자원 비례 위력 — 최종값에 자동 포함 (수식 표시 X)
                if (skill.ResourcePowerPerStack > 0)
                {
                    ResourceType resType = InferResourceType(skill, caster);
                    if (resType != ResourceType.None)
                    {
                        int currentStacks = (caster?.Resource != null && caster.Resource.Resource == resType)
                            ? caster.Resource.CurrentStacks : 0;
                        displayPower += currentStacks * skill.ResourcePowerPerStack;
                    }
                }

                string valueLabel = skill.Type switch
                {
                    SkillType.Attack => "데미지",
                    SkillType.Heal => "회복",
                    SkillType.Shield => "쉴드",
                    SkillType.Buff => "강화",
                    SkillType.Debuff => "약화",
                    SkillType.Purify => "정화",
                    _ => "수치"
                };
                parts.Add($"{displayPower} {valueLabel}");
            }

            // 2. 상태이상 — "N 이펙트명 (M턴)"
            if (skill.StatusEffect != StatusEffectType.None)
            {
                string effectName = GetEffectLabel(skill.StatusEffect);
                string dur = skill.EffectDuration > 0 ? $" ({skill.EffectDuration}턴)" : "";
                if (skill.EffectValue > 0)
                    parts.Add($"{skill.EffectValue} {effectName}{dur}");
                else
                    parts.Add($"{effectName}{dur}");
            }

            // 3. 자원 획득
            if (skill.ResourceGainType != ResourceType.None && skill.ResourceGainAmount > 0)
                parts.Add($"{GetResourceLabel(skill.ResourceGainType)} +{skill.ResourceGainAmount}");

            // 4. 자원 소모
            if (skill.ConsumeAllResource && skill.ResourceCostType != ResourceType.None)
            {
                parts.Add($"{GetResourceLabel(skill.ResourceCostType)} 전량 소모");
            }
            else if (skill.ResourceCostType != ResourceType.None && skill.ResourceCostAmount > 0)
            {
                string label = $"{GetResourceLabel(skill.ResourceCostType)} {skill.ResourceCostAmount} 소모";
                if (caster?.Resource != null && caster.Resource.Resource == skill.ResourceCostType
                    && caster.Resource.CurrentStacks < skill.ResourceCostAmount)
                    label = $"⚠ {label}";
                parts.Add(label);
            }

            // 5. ShieldFlags
            if (skill.Type == SkillType.Shield && skill.ShieldFlags != ShieldFlag.None)
            {
                if ((skill.ShieldFlags & ShieldFlag.GivesChargeOnAbsorb) != 0)
                    parts.Add("흡수 시 전하 부여");
            }

            // ★ BehaviorTag는 슬롯에서 생략 — 툴팁(BuildTooltipDescription)에서만 상세 표시

            return string.Join(separator, parts);
        }

        /// <summary>★ 2026-08-03 P1-Q3: caster 상태 기반 BehaviorTag 활성 판단.</summary>
        /// <remarks>
        /// 슬롯 단계에서caster만으로 판단 가능한 조건부 보너스만 처리.
        /// HP 임계값 기반 (Berserk) — 현재 활성 시 "★활성" 마커로 사용자 인지.
        /// AP 기반 (AllIn) / UsesThisBattle 기반 (Momentum/LimitBreak)은
        /// 별도 경로(ActionBarUI.IsSlotAffordable)에서 처리.
        /// </remarks>
        private static bool IsBehaviorActiveNow(BehaviorTag tag, Character caster)
        {
            if (caster?.Health == null || caster.Health.MaxHP <= 0) return false;
            float hpRatio = (float)caster.Health.CurrentHP / caster.Health.MaxHP;

            switch (tag.Keyword)
            {
                case BehaviorKeyword.Berserk:
                    return hpRatio <= 0.5f;  // HP 50% 이하 시 2배 활성
                // 추후 확장: Desperation/Wound도 caster 상태로 판단 가능 (비례값이라 임계 설정 필요)
            }
            return false;
        }

        /// <summary>★ 2026-08-03 P1-Q5: CC 상태 라벨 — Stun/Freeze/Sleep 중 어떤 CC인지.</summary>
        private static string GetCCStatusLabel(StatusEffectComponent effects)
        {
            if (effects == null) return null;
            if (effects.HasEffect(StatusEffectType.Stun)) return "⚠ 기절 — 행동 불가";
            if (effects.HasEffect(StatusEffectType.Freeze)) return "⚠ 빙결 — 행동 불가";
            if (effects.HasEffect(StatusEffectType.Sleep)) return "⚠ 수면 — 행동 불가";
            return "⚠ 행동 불가";  // 폴백 (IsIncapacitated true인데 세 종류 아닌 경우)
        }

        // ════════════════════════════════════════════════════════════
        // ★ 2026-08-03 P0-R2: BuildTooltipDescription — 툴팁 전용 자연어 풀어쓰기
        // StS 한국어 표준 ("~줍니다", "~얻습니다") + rich text 색상 강조.
        // 기존 BuildSkillDescription(축약형)은 슬롯용으로 유지.
        // ════════════════════════════════════════════════════════════

        /// <summary>
        /// ★ 2026-08-03 P0-R2: 툴팁 전용 자연어 설명 (StS 한국어 표준).
        /// 슬롯용 BuildSkillDescription(축약형)과 달리 완전한 문장으로 풀어쓰기.
        /// 행동 동사 ("줍니다", "얻습니다", "부여합니다") + 항목별 줄바꿈 + 색상 강조.
        /// </summary>
        public static string BuildTooltipDescription(SkillData skill, Character caster)
        {
            if (skill == null) return "";

            // 발견 스킬은 기존 BuildSkillDescription 사용 (모달 형식)
            if (skill.IsDiscover && skill.DiscoverPool != null)
                return BuildSkillDescription(skill, caster);

            var lines = new List<string>();

            // CC 상태 경고
            if (caster?.StatusEffects != null && caster.StatusEffects.IsIncapacitated)
                lines.Add($"<color=#ff6060>{GetCCStatusLabel(caster.StatusEffects)}</color>");

            // 1. 핵심 효과 (위력/힐/쉴드)
            AppendCoreEffectLine(lines, skill, caster);

            // 2. 상태이상 부여
            if (skill.StatusEffect != StatusEffectType.None && skill.EffectValue > 0)
            {
                string effectVerb = GetEffectApplyVerb(skill.StatusEffect);
                string effectName = GetEffectLabel(skill.StatusEffect);
                int dur = skill.EffectDuration;
                string target = GetEffectTargetNoun(skill);
                if (dur > 0)
                    lines.Add($"{target} <color=#c0a0ff>{effectName} {skill.EffectValue}</color>을(를) {dur}턴 동안 {effectVerb}.");
                else
                    lines.Add($"{target} <color=#c0a0ff>{effectName} {skill.EffectValue}</color>을(를) {effectVerb}.");
            }

            // 3. 자원 획득
            if (skill.ResourceGainType != ResourceType.None && skill.ResourceGainAmount > 0)
            {
                string resLabel = GetResourceLabel(skill.ResourceGainType);
                lines.Add($"<color=#ff9a4a>{resLabel} {skill.ResourceGainAmount}</color>을(를) 얻습니다.");
            }

            // 4. 자원 소모
            AppendResourceCostLine(lines, skill, caster);

            // 5. ShieldFlags
            if (skill.Type == SkillType.Shield && skill.ShieldFlags != ShieldFlag.None)
            {
                if ((skill.ShieldFlags & ShieldFlag.GivesChargeOnAbsorb) != 0)
                    lines.Add("<i>보호막이 피해를 흡수할 때</i> <color=#ff9a4a>전하</color>를 얻습니다.");
            }

            // 6. BehaviorTag (자연어 템플릿)
            if (skill.Behaviors != null)
            {
                foreach (var tag in skill.Behaviors)
                {
                    string naturalLine = GetBehaviorTooltipLabel(tag, skill, caster);
                    if (!string.IsNullOrEmpty(naturalLine))
                        lines.Add(naturalLine);
                }
            }

            return string.Join("\n", lines);
        }

        /// <summary>핵심 효과(위력/힐/쉴드)를 자연어로. 자원 비례 위력은 별도 줄.</summary>
        private static void AppendCoreEffectLine(List<string> lines, SkillData skill, Character caster)
        {
            if (skill.Power <= 0) return;

            int displayPower = skill.Type == SkillType.Attack && caster != null
                ? caster.Stats.GetStat(StatType.ATK) + skill.Power
                : skill.Power;

            // 자원 비례 위력 정보
            int bonusPerStack = 0;
            int currentStacks = 0;
            ResourceType resType = ResourceType.None;
            if (skill.ResourcePowerPerStack > 0)
            {
                resType = InferResourceType(skill, caster);
                if (resType != ResourceType.None)
                {
                    bonusPerStack = skill.ResourcePowerPerStack;
                    currentStacks = (caster?.Resource != null && caster.Resource.Resource == resType)
                        ? caster.Resource.CurrentStacks : 0;
                }
            }

            string numColor = skill.Type switch
            {
                SkillType.Attack => "#ff7a4a",   // 공격 = 주황
                SkillType.Heal => "#a0ffa0",      // 힐 = 초록
                SkillType.Shield => "#c0a0ff",    // 쉴드 = 보라
                _ => "#ffd47a"
            };

            string line = skill.Type switch
            {
                SkillType.Attack => $"적에게 <color={numColor}><b>{displayPower}</b></color>의 피해를 줍니다.",
                SkillType.Heal => $"아군의 체력을 <color={numColor}><b>{displayPower}</b></color> 회복합니다.",
                SkillType.Shield => $"아군에게 <color={numColor}><b>{displayPower}</b></color>의 보호막을 부여합니다.",
                SkillType.Buff => $"<color={numColor}><b>{displayPower}</b></color>의 강화 효과를 줍니다.",
                SkillType.Debuff => $"<color={numColor}><b>{displayPower}</b></color>의 약화 효과를 줍니다.",
                _ => null
            };
            if (line != null) lines.Add(line);

            // 자원 비례 위력 — 별도 줄 (공격/힐/쉴드 공통)
            if (bonusPerStack > 0 && resType != ResourceType.None)
            {
                string resLabel = GetResourceLabel(resType);
                string effectNoun = skill.Type == SkillType.Heal ? "회복량" :
                                    skill.Type == SkillType.Shield ? "보호막" : "피해";
                if (currentStacks > 0)
                {
                    int total = currentStacks * bonusPerStack;
                    lines.Add($"<color=#ff9a4a>{resLabel}</color> 1스택마다 {effectNoun}이 <color={numColor}>+{bonusPerStack}</color> 증가합니다. <i>(현재 {currentStacks}스택 = +{total})</i>");
                }
                else
                {
                    lines.Add($"<color=#ff9a4a>{resLabel}</color> 1스택마다 {effectNoun}이 <color={numColor}>+{bonusPerStack}</color> 증가합니다.");
                }
            }
        }

        /// <summary>상태이상 적용 동사.</summary>
        private static string GetEffectApplyVerb(StatusEffectType type) => type switch
        {
            StatusEffectType.Poison => "부여합니다",
            StatusEffectType.Burn => "부여합니다",
            StatusEffectType.Bleed => "입힙니다",
            StatusEffectType.Stun => "부여합니다",
            StatusEffectType.Freeze => "부여합니다",
            StatusEffectType.Sleep => "부여합니다",
            StatusEffectType.AttackUp => "증가시킵니다",
            StatusEffectType.AttackDown => "감소시킵니다",
            StatusEffectType.DefenseUp => "증가시킵니다",
            StatusEffectType.DefenseDown => "감소시킵니다",
            StatusEffectType.Regeneration => "부여합니다",
            StatusEffectType.Taunt => "부여합니다",
            _ => "부여합니다"
        };

        /// <summary>상태이상 대상 명사 ("적에게"/"아군에게" 등).</summary>
        private static string GetEffectTargetNoun(SkillData skill)
        {
            if (skill.Target == TargetType.SingleEnemy || skill.Target == TargetType.AllEnemies)
                return "적에게";
            if (skill.Target == TargetType.SingleAlly || skill.Target == TargetType.AllAllies)
                return "아군에게";
            return "대상에게";
        }

        /// <summary>자원 소모를 자연어로. 부족 시 빨간색 강조.</summary>
        private static void AppendResourceCostLine(List<string> lines, SkillData skill, Character caster)
        {
            if (skill.ResourceCostType == ResourceType.None) return;
            string resLabel = GetResourceLabel(skill.ResourceCostType);
            int current = (caster?.Resource != null && caster.Resource.Resource == skill.ResourceCostType)
                ? caster.Resource.CurrentStacks : 0;

            if (skill.ConsumeAllResource)
            {
                // ConsumeAllResource는 자원 0이어도 사용 가능 (위력만 낮아짐)
                lines.Add($"<color=#ff9a4a>{resLabel}</color>을(를) 모두 소모합니다. <i>(현재 {current}스택)</i>");
            }
            else if (skill.ResourceCostAmount > 0)
            {
                bool isShort = current < skill.ResourceCostAmount;
                string color = isShort ? "#ff6060" : "#ff9a4a";
                string suffix = isShort ? $" <color=#ff6060>(보유 {current})</color>" : "";
                lines.Add($"<color={color}>{resLabel} {skill.ResourceCostAmount}</color>을(를) 소모합니다.{suffix}");
            }

            if (skill.MinResourceRequired > 0)
            {
                bool isShort = current < skill.MinResourceRequired;
                string color = isShort ? "#ff6060" : "#a0a0a0";
                lines.Add($"<i><color={color}>사용에 {resLabel} {skill.MinResourceRequired} 이상 필요</color></i>");
            }
        }

        /// <summary>★ P0-R1: BehaviorTag → 자연어 툴팁 라벨 (StS 한국어 표준).</summary>
        /// <remarks>
        /// 기존 GetBehaviorLabel(축약형 "HP≤50% 2배")과 달리 완전한 문장.
        /// 30종 주요 BehaviorTag 커버. 미지원 키워드는 null 반환 (표시 생략).
        /// </remarks>
        private static string GetBehaviorTooltipLabel(BehaviorTag tag, SkillData skill, Character caster)
        {
            if (tag.Keyword == BehaviorKeyword.None) return null;
            int rank = tag.Rank;

            return tag.Keyword switch
            {
                // 위력 변형
                BehaviorKeyword.Berserk => "<i>체력이 절반 이하일 때</i> 피해가 <color=#ffd47a><b>2배</b></color>가 됩니다.",
                BehaviorKeyword.HeavyHit => "피해가 <color=#ffd47a><b>2배</b></color>가 됩니다. <i>(사용 후 AP 1 추가 소모)</i>",

                // 상태이상 터치
                BehaviorKeyword.VenomTouch => $"적에게 <color=#a0ffa0>중독 {rank}스택</color>을 부여합니다.",
                BehaviorKeyword.BurningTouch => $"적에게 <color=#ffa050>화상 {rank}스택</color>을 부여합니다.",
                BehaviorKeyword.FreezeTouch => $"적에게 <color=#80c0ff>빙결 {rank}스택</color>을 부여합니다.",

                // 관통/처형
                BehaviorKeyword.Pierce => "<i>적의 방어와 보호막을 무시합니다.</i>",
                BehaviorKeyword.Execution => $"<i>적의 체력이 <color=#ff6060>{rank} 이하</color>일 때 즉시 처치합니다.</i>",

                // 연쇄/다타
                BehaviorKeyword.Chain => $"<color=#ffd47a>{rank}명</color>의 무작위 적에게 연쇄 피해를 줍니다.",

                // Phase ARCH-4 조건부
                BehaviorKeyword.FirstBlood => $"<i>적의 체력이 가득 차 있으면</i> 피해가 <color=#ffd47a>+{rank}</color> 증가합니다.",
                BehaviorKeyword.TargetFullHP => $"<i>적의 체력이 가득 차 있으면</i> 피해가 <color=#ffd47a>+{rank}</color> 증가합니다.",
                BehaviorKeyword.Cull => $"<i>적의 체력이 절반 이하일 때</i> 피해가 <color=#ffd47a>+{rank}</color> 증가합니다.",
                BehaviorKeyword.Desperation => "<i>잃은 체력 1당</i> 피해가 <color=#ffd47a>+1</color> 증가합니다.",
                BehaviorKeyword.Wound => "<i>잃은 체력 1당</i> 피해가 <color=#ff6060>-1</color> 감소합니다.",
                BehaviorKeyword.GiantSlayer => $"<i>적의 최대 체력이 높을수록</i> 피해가 <color=#ffd47a>+{rank}</color> 증가합니다.",
                BehaviorKeyword.Dominance => $"<i>적의 체력이 자신보다 낮을 때</i> 피해가 <color=#ffd47a>+{rank}</color> 증가합니다.",
                BehaviorKeyword.Bulwark => $"<i>보호막 보유 시</i> 피해가 <color=#ffd47a>+{rank}</color> 증가합니다.",
                BehaviorKeyword.AllIn => $"<i>AP가 0일 때</i> 피해가 <color=#ffd47a>+{rank}</color> 증가합니다.",
                BehaviorKeyword.Bounty => "적을 처치하면 체력을 회복합니다.",
                BehaviorKeyword.FollowUp => $"<i>이미 이번 턴에 피해를 입은 적에게</i> 피해가 <color=#ffd47a>+{rank}</color> 증가합니다.",
                BehaviorKeyword.Echo => "위력의 절반으로 2번 피해를 줍니다.",
                BehaviorKeyword.LimitBreak => $"<color=#ffd47a><b>전투당 1회</b></color> 사용 가능. 위력이 <color=#ffd47a>+{rank}</color> 증가합니다.",
                BehaviorKeyword.Explosion => "전하 스택당 <color=#ffd47a>3</color>의 추가 피해를 줍니다.",
                BehaviorKeyword.Momentum => $"사용할 때마다 위력이 <color=#ffd47a>+{rank}</color>씩 영구히 증가합니다.",
                BehaviorKeyword.Fatigue => $"사용할 때마다 위력이 <color=#ff6060>-{rank}</color>씩 영구히 감소합니다.",
                BehaviorKeyword.Escalation => $"사용할 때마다 AP 비용이 <color=#ff6060>+{rank}</color>씩 증가합니다.",
                BehaviorKeyword.Mastery => $"사용할 때마다 AP 비용이 <color=#ffd47a>-{rank}</color>씩 감소합니다.",

                // Phase CC 신규
                BehaviorKeyword.Propagate => "<i>전하를 인접한 적에게 전파합니다.</i>",
                BehaviorKeyword.TargetFreeze => $"<i>빙결 상태인 적에게</i> 피해가 <color=#ffd47a>+{rank}</color> 증가합니다.",
                BehaviorKeyword.CleanseLowTarget => "<i>대상의 체력이 절반 이하일 때</i> 독과 화상을 정화합니다.",
                BehaviorKeyword.ResourceThresholdShield => $"{GetResourceLabel(InferResourceType(skill, caster))} <color=#ff9a4a>{rank}스택 이상</color>일 때 보호막이 강화됩니다.",
                BehaviorKeyword.Lifesteal => "<i>입힌 피해의 절반만큼</i> 체력을 회복합니다.",

                _ => null  // 표시 생략 (PowerUp/Spread/Bounce/MultiHit/AOEAuto 등)
            };
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
