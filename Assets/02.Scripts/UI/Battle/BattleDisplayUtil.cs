using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.UI;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 전투 UI 공통 표시 유틸리티 — 중복 로직 통합
    /// </summary>
    public static class BattleDisplayUtil
    {
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
        /// 스킬 수치 요약 문자열 생성 (위력, 상태이상 등)
        /// 공격 스킬은 캐릭터 ATK 포함하여 최종 위력 계산
        /// </summary>
        public static string BuildSkillDescription(SkillData skill, Character caster, string separator = " | ")
        {
            var parts = new List<string>();

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
                parts.Add($"{label} {displayPower}");
            }

            if (skill.StatusEffect != StatusEffectType.None)
            {
                string effectName = GetEffectLabel(skill.StatusEffect);
                string duration = skill.EffectDuration > 0 ? $" ({skill.EffectDuration}턴)" : "";
                string value = skill.EffectValue > 0 ? $" {skill.EffectValue}" : "";
                parts.Add($"{effectName}{value}{duration}");
            }

            return string.Join(separator, parts);
        }
    }
}
