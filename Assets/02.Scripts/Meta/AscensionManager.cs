using System.Collections.Generic;
using TeamLog.Map;

namespace TeamLog.Meta
{
    /// <summary>
    /// 어센션 시스템 중앙 관리자 — 순수 C# static.
    /// 클리어 시 자동 +1레벨(최대 15) 누적되는 난이도 modifier의 활성 스택 수와 누적 효과를 계산.
    /// MetaProgressionManager 패턴 재사용.
    ///
    /// 어센션 레벨별 활성화 순서:
    ///   1: EnemyHpPercent      (적 HP +5%)
    ///   2: StartGold           (시작 골드 -10)
    ///   3: RerollCount         (리롤 -1)
    ///   4: PlayerMaxHpPercent  (파티 MaxHP -5%)
    ///   5: HealPercent         (힐/휴식 -10%)
    ///   6: EnemyAtkPercent     (적 ATK +5%)
    ///   7: EnemyHpPercent x2   (적 HP 총 +10%)
    ///   8: StartGold x2        (시작 골드 총 -20)
    ///   9: RerollCount x2      (리롤 총 -2)
    ///  10: PlayerMaxHpPercent x2 (파티 MaxHP 총 -10%)
    ///  11: HealPercent x2      (힐/휴식 총 -20%)
    ///  12: EnemyAtkPercent x2  (적 ATK 총 +10%)
    ///  13: EnemyHpPercent x3   (적 HP 총 +15%)
    ///  14: RerollCount x3      (리롤 총 -3)
    ///  15: BossHpPercent       (보스 HP +20%)
    /// </summary>
    public static class AscensionManager
    {
        public const int MaxLevel = 15;

        // 각 modifier의 레벨 임계값 (오름차순 — stacks = count(thresholds where asc >= t))
        private static readonly int[] LvEnemyHp = { 1, 7, 13 };
        private static readonly int[] LvStartGold = { 2, 8 };
        private static readonly int[] LvReroll = { 3, 9, 14 };
        private static readonly int[] LvPlayerHp = { 4, 10 };
        private static readonly int[] LvHeal = { 5, 11 };
        private static readonly int[] LvEnemyAtk = { 6, 12 };
        private static readonly int[] LvBossHp = { 15 };

        // per-stack 값
        private const float StackEnemyHpMul = 0.05f;     // +5% per stack
        private const int StackStartGold = -10;          // -10 per stack
        private const int StackReroll = -1;              // -1 per stack
        private const float StackPlayerHpMul = -0.05f;   // -5% per stack
        private const float StackHealMul = -0.10f;       // -10% per stack
        private const float StackEnemyAtkMul = 0.05f;    // +5% per stack
        private const float StackBossHpMul = 0.20f;      // +20% (단일)

        /// <summary>어센션 레벨 조회 (0~15). MetaSaveData가 null이면 0.</summary>
        public static int GetAscensionLevel(MetaSaveData meta)
            => meta == null ? 0 : System.Math.Max(0, System.Math.Min(MaxLevel, meta.AscensionLevel));

        /// <summary>현재 활성 어센션 레벨을 지정된 값(0~현재 달성 레벨)으로 클램프.</summary>
        public static int ClampSelectedLevel(int selected, MetaSaveData meta)
        {
            int max = GetAscensionLevel(meta);
            if (selected < 0) return 0;
            if (selected > max) return max;
            return selected;
        }

        // ── 스택 수 계산 ──

        private static int CountStacks(int asc, int[] thresholds)
        {
            int count = 0;
            foreach (var t in thresholds)
                if (asc >= t) count++;
            return count;
        }

        /// <summary>지정한 modifier 타입의 활성 스택 수 (0~3).</summary>
        public static int GetStackCount(AscensionModifierType type, MetaSaveData meta)
            => GetStackCountByLevel(type, GetAscensionLevel(meta));

        /// <summary>레벨을 직접 지정하여 스택 수 계산 (시뮬레이터/UI 미리보기용).</summary>
        public static int GetStackCountByLevel(AscensionModifierType type, int ascensionLevel)
        {
            int asc = System.Math.Max(0, ascensionLevel);
            switch (type)
            {
                case AscensionModifierType.EnemyHpPercent:     return CountStacks(asc, LvEnemyHp);
                case AscensionModifierType.StartGold:          return CountStacks(asc, LvStartGold);
                case AscensionModifierType.RerollCount:        return CountStacks(asc, LvReroll);
                case AscensionModifierType.PlayerMaxHpPercent: return CountStacks(asc, LvPlayerHp);
                case AscensionModifierType.HealPercent:        return CountStacks(asc, LvHeal);
                case AscensionModifierType.EnemyAtkPercent:    return CountStacks(asc, LvEnemyAtk);
                case AscensionModifierType.BossHpPercent:      return CountStacks(asc, LvBossHp);
            }
            return 0;
        }

        // ── 누적 값 계산 (게임 로직에서 사용) ──

        /// <summary>적 HP 증감 비율 (1.0 = 변화 없음). asc 1 = 1.05, asc 7 = 1.10, asc 13 = 1.15.</summary>
        public static float GetEnemyHpMul(MetaSaveData meta)
            => GetEnemyHpMulByLevel(GetAscensionLevel(meta));

        /// <summary>적 ATK 증감 비율. asc 6 = 1.05, asc 12 = 1.10.</summary>
        public static float GetEnemyAtkMul(MetaSaveData meta)
            => GetEnemyAtkMulByLevel(GetAscensionLevel(meta));

        /// <summary>보스 HP 증감 비율. asc 15 = 1.20, 그 외 = 1.0.</summary>
        public static float GetBossHpMul(MetaSaveData meta)
            => GetBossHpMulByLevel(GetAscensionLevel(meta));

        /// <summary>파티 MaxHP 증감 비율. asc 4 = 0.95, asc 10 = 0.90.</summary>
        public static float GetPlayerMaxHpMul(MetaSaveData meta)
            => GetPlayerMaxHpMulByLevel(GetAscensionLevel(meta));

        /// <summary>힐/휴식 효율 증감 비율. asc 5 = 0.90, asc 11 = 0.80.</summary>
        public static float GetHealMul(MetaSaveData meta)
            => GetHealMulByLevel(GetAscensionLevel(meta));

        /// <summary>시작 골드 증감. asc 2 = -10, asc 8 = -20. 최소 0 (골드 비음수).</summary>
        public static int GetStartGoldDelta(MetaSaveData meta)
            => GetStartGoldDeltaByLevel(GetAscensionLevel(meta));

        /// <summary>턴당 리롤 횟수 증감. asc 3 = -1, asc 9 = -2, asc 14 = -3.</summary>
        public static int GetRerollDelta(MetaSaveData meta)
            => GetRerollDeltaByLevel(GetAscensionLevel(meta));

        // ── 레벨 직접 지정 변형 (시뮬레이터/UI 미리보기/BattleSceneSetup 캐시용) ──

        public static float GetEnemyHpMulByLevel(int ascensionLevel)
            => 1f + StackEnemyHpMul * GetStackCountByLevel(AscensionModifierType.EnemyHpPercent, ascensionLevel);

        public static float GetEnemyAtkMulByLevel(int ascensionLevel)
            => 1f + StackEnemyAtkMul * GetStackCountByLevel(AscensionModifierType.EnemyAtkPercent, ascensionLevel);

        public static float GetBossHpMulByLevel(int ascensionLevel)
            => 1f + StackBossHpMul * GetStackCountByLevel(AscensionModifierType.BossHpPercent, ascensionLevel);

        public static float GetPlayerMaxHpMulByLevel(int ascensionLevel)
            => 1f + StackPlayerHpMul * GetStackCountByLevel(AscensionModifierType.PlayerMaxHpPercent, ascensionLevel);

        public static float GetHealMulByLevel(int ascensionLevel)
            => 1f + StackHealMul * GetStackCountByLevel(AscensionModifierType.HealPercent, ascensionLevel);

        public static int GetStartGoldDeltaByLevel(int ascensionLevel)
            => StackStartGold * GetStackCountByLevel(AscensionModifierType.StartGold, ascensionLevel);

        public static int GetRerollDeltaByLevel(int ascensionLevel)
            => StackReroll * GetStackCountByLevel(AscensionModifierType.RerollCount, ascensionLevel);

        // ── 활성 modifier 목록 (UI 표시/테스트용) ──

        /// <summary>
        /// 지정된 어센션 레벨에서 활성화된 modifier 타입 목록 (중복 포함 — 스택 수만큼).
        /// 예: asc 7 → [EnemyHp, EnemyHp, StartGold, Reroll, PlayerHp, Heal, EnemyAtk]
        /// </summary>
        public static List<AscensionModifierType> GetActiveModifiers(int ascensionLevel)
        {
            var result = new List<AscensionModifierType>();
            int asc = System.Math.Max(0, ascensionLevel);
            AppendStacks(result, AscensionModifierType.EnemyHpPercent,     CountStacks(asc, LvEnemyHp));
            AppendStacks(result, AscensionModifierType.StartGold,          CountStacks(asc, LvStartGold));
            AppendStacks(result, AscensionModifierType.RerollCount,        CountStacks(asc, LvReroll));
            AppendStacks(result, AscensionModifierType.PlayerMaxHpPercent, CountStacks(asc, LvPlayerHp));
            AppendStacks(result, AscensionModifierType.HealPercent,        CountStacks(asc, LvHeal));
            AppendStacks(result, AscensionModifierType.EnemyAtkPercent,    CountStacks(asc, LvEnemyAtk));
            AppendStacks(result, AscensionModifierType.BossHpPercent,      CountStacks(asc, LvBossHp));
            return result;
        }

        private static void AppendStacks(List<AscensionModifierType> list, AscensionModifierType type, int count)
        {
            for (int i = 0; i < count; i++) list.Add(type);
        }
    }
}
