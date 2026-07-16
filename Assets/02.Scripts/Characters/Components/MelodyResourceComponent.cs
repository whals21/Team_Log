using System.Collections.Generic;
using System.Linq;
using TeamLog.Combat;
using TeamLog.Map;
using TeamLog.Skill;

namespace TeamLog.Characters
{
    /// <summary>
    /// Melody 자원 — Calliope (Bard) 고유 메카닉 (Phase CC-2D).
    ///
    /// 핵심 루프 (기획: ReworkDrafts/06_Bard.md):
    /// - 매 턴 스킬(곡) 시전 → CurrentMelody = 그 선율 (주 선율, 100% 효과)
    /// - 매 턴 시작 시 Current → Echo로 이동 → Echo 자동 발동 (주 선율의 50%)
    /// - 같은 스킬 2턴 연속 사용 시 부 선율 무효 (매 턴 다른 스킬 유도)
    /// - "용기의 화음" 특성 시 페널티 무시
    ///
    /// 전략: 매 턴 다른 선율 연주 → 주+부 동시 효과로 파티 강화. 리듬감 있는 플레이.
    /// </summary>
    public class MelodyResourceComponent : CharacterResourceComponent
    {
        public override ResourceType Resource => ResourceType.Melody;
        public override int MaxStacks => 1; // 단일 상태 (현재 선율 1개만)

        /// <summary>이번 턴 연주 중인 선율 (주 선율). 스킬 시전 시 설정.</summary>
        public MelodyType CurrentMelody { get; private set; } = MelodyType.None;

        /// <summary>직전 턴에 사용한 선율 (부 선율 발동 판별용).</summary>
        public MelodyType PrevTurnMelody { get; private set; } = MelodyType.None;

        /// <summary>부 선율 기본 배율 (주 선율의 50%).</summary>
        public const float DefaultEchoMul = 0.5f;

        /// <summary>주 선율 기본 위력.</summary>
        public const int ValorMainAtk = 3;
        public const int ValorEchoAtk = 1;
        public const int DissonanceMainAtk = 3;
        public const int DissonanceEchoAtk = 1;
        public const int HealingMainPower = 8;
        public const int HealingEchoPower = 4;
        public const int InspirationMainShield = 5;
        public const int InspirationEchoShield = 3;
        public const int InspirationMainAp = 1;

        public override void OnTurnStart(Character owner)
        {
            // 부 선율 자동 발동 검사
            // "같은 스킬 2턴 연속" 감지: 직전 턴 Current == 2턴 전 PrevTurnMelody
            bool isRepeat = (CurrentMelody != MelodyType.None && CurrentMelody == PrevTurnMelody);
            bool noPenalty = HasRepeatNoPenaltyTrait(owner);

            if (CurrentMelody != MelodyType.None && (!isRepeat || noPenalty))
            {
                float powerMul = GetEchoPowerMul(owner);
                ApplyEchoEffect(owner, CurrentMelody, powerMul);
            }

            // 턴 이동: PrevTurn = Current, Current = None
            PrevTurnMelody = CurrentMelody;
            CurrentMelody = MelodyType.None;
        }

        public override void OnTurnEnd(Character owner)
        {
            // Melody는 턴 종료 시 리셋 없음
        }

        /// <summary>스킬 시전 시 Behavior에서 호출 — 이번 턴 주 선율 설정.</summary>
        public void SetCurrentMelody(MelodyType type)
        {
            CurrentMelody = type;
        }

        /// <summary>부 선율 배율 조회 — 기본 0.5, "전투 노래" 특성(EchoPowerMul) 시 값.</summary>
        private float GetEchoPowerMul(Character owner)
        {
            if (owner?.PlayerTraitHandler == null || !owner.PlayerTraitHandler.HasTrait)
                return DefaultEchoMul;
            // EchoPowerMul은 Passive 키워드의 값 사용 (0.75 등). 0이면 기본 0.5.
            float mul = owner.PlayerTraitHandler.QueryKeywordSum(KeywordType.EchoPowerMul);
            return mul > 0f ? mul : DefaultEchoMul;
        }

        /// <summary>"용기의 화음" 특성(RepeatNoPenalty) 보유 여부.</summary>
        private bool HasRepeatNoPenaltyTrait(Character owner)
        {
            if (owner?.PlayerTraitHandler == null || !owner.PlayerTraitHandler.HasTrait)
                return false;
            return owner.PlayerTraitHandler.QueryKeywordSum(KeywordType.RepeatNoPenalty) > 0;
        }

        /// <summary>부 선율 자동 발동 — 직전 턴 CurrentMelody의 50% 효과.</summary>
        private void ApplyEchoEffect(Character owner, MelodyType type, float powerMul)
        {
            var party = GetPlayerParty();
            var enemies = GetEnemies();

            switch (type)
            {
                case MelodyType.Healing:
                    // 가장 부상당한 파티원 자동 힐
                    var target = FindMostInjuredPartyMember(owner, party);
                    if (target != null)
                    {
                        int healAmount = System.Math.Max(1, (int)(HealingEchoPower * (powerMul / DefaultEchoMul)));
                        target.Health?.Heal(healAmount);
                    }
                    break;

                case MelodyType.Valor:
                    // 파티 전체 ATK+1 (1턴)
                    if (party != null)
                    {
                        foreach (var member in party)
                        {
                            if (member == null || !member.IsAlive) continue;
                            int atkBonus = System.Math.Max(1, (int)(ValorEchoAtk * (powerMul / DefaultEchoMul)));
                            member.StatusEffects?.ApplyEffect(StatusEffectType.AttackUp, 1, atkBonus);
                        }
                    }
                    break;

                case MelodyType.Dissonance:
                    // 적 전체 ATK-1 (1턴)
                    if (enemies != null)
                    {
                        foreach (var enemy in enemies)
                        {
                            if (enemy == null || !enemy.IsAlive) continue;
                            int atkPenalty = System.Math.Max(1, (int)(DissonanceEchoAtk * (powerMul / DefaultEchoMul)));
                            enemy.StatusEffects?.ApplyEffect(StatusEffectType.AttackDown, 1, atkPenalty);
                        }
                    }
                    break;

                case MelodyType.Inspiration:
                    // 파티 전체 쉴드 3
                    if (party != null)
                    {
                        int shieldAmount = System.Math.Max(1, (int)(InspirationEchoShield * (powerMul / DefaultEchoMul)));
                        foreach (var member in party)
                        {
                            if (member == null || !member.IsAlive) continue;
                            member.Health?.AddShield(member, shieldAmount);
                        }
                    }
                    break;
            }
        }

        /// <summary>가장 부상당한 파티원 (HP 비율 가장 낮음). owner 제외.</summary>
        private Character FindMostInjuredPartyMember(Character owner, IReadOnlyList<Character> party)
        {
            if (party == null) return null;
            Character best = null;
            float bestRatio = 1f;
            foreach (var member in party)
            {
                if (member == null || !member.IsAlive || member == owner) continue;
                if (member.Health?.MaxHP <= 0) continue;
                float ratio = (float)member.Health.CurrentHP / member.Health.MaxHP;
                if (ratio < bestRatio)
                {
                    bestRatio = ratio;
                    best = member;
                }
            }
            return best;
        }

        private IReadOnlyList<Character> GetPlayerParty() => GameRunState.Instance?.PlayerParty;
        private IReadOnlyList<Character> GetEnemies() => GameRunState.Instance?.CurrentEnemies;

        public override string ToString() =>
            $"Melody(Current={CurrentMelody}, Prev={PrevTurnMelody})";
    }
}
