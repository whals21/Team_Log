#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TeamLog.Reward;
using TeamLog.Skill;

namespace TeamLog.Editor
{
    /// <summary>
    /// DataGenerator — 유물 데이터 생성 (GenerateRelicData + CreateRelic + SetField 헬퍼)
    /// 진입점/스킬/캐릭터/유틸리티: DataGenerator.cs
    /// 증강 데이터/스폰 패턴: DataGenerator.Augments.cs
    /// 이벤트 데이터: DataGenerator.Events.cs
    /// 팔레트 (UI/오디오/VFX): DataGenerator.Palettes.cs
    /// </summary>
    public static partial class DataGenerator
    {
        #region Relic Data

        private const string RELIC_PATH = "Assets/03.Data/Relics";

        private static void GenerateRelicData()
        {
            EnsureFolder(RELIC_PATH);

            CreateRelic("Relic_BurningSword", "불타는 검", "공격 시 추가 데미지 +3",
                RelicTrigger.OnSkillUsed, 3, RewardRarity.Common, price: 80,
                new[] { Kw(KeywordType.BonusOutgoingDamage, 3) });

            CreateRelic("Relic_IronHide", "철가죽", "받는 피해 -2",
                RelicTrigger.OnDamageReceived, 2, RewardRarity.Common, price: 90,
                new[] { Kw(KeywordType.DamageReduction, 2) });

            CreateRelic("Relic_RegenRing", "재생의 반지", "매 턴 3 HP 회복",
                RelicTrigger.TurnEnd, 3, RewardRarity.Common, price: 60,
                new[] { Kw(KeywordType.HPPerTurn, 3, KeywordTrigger.OnTurnEnd) });

            CreateRelic("Relic_GoldCharm", "황금 부적", "골드 획득 시 +15 골드",
                RelicTrigger.OnGoldEarned, 15, RewardRarity.Rare, price: 120,
                new[] { Kw(KeywordType.BonusGold, 15, KeywordTrigger.OnGoldEarned) });

            CreateRelic("Relic_ShieldAmulet", "방패 부적", "방패 스킬 사용 시 +3 쉴드",
                RelicTrigger.OnShieldGained, 3, RewardRarity.Common, price: 70,
                new[] { Kw(KeywordType.ShieldPerTurn, 3, KeywordTrigger.OnShieldGained) });

            CreateRelic("Relic_VampireFang", "흡혈 송곳니", "적 처치 시 +5 HP 회복",
                RelicTrigger.OnKill, 5, RewardRarity.Rare, price: 100,
                new[] { Kw(KeywordType.OnKillHeal, 5, KeywordTrigger.OnKill) });

            CreateRelic("Relic_BerserkerMark", "광전사 인장", "적 처치당 공격력 +2 누적",
                RelicTrigger.OnKill, 2, RewardRarity.Unique, price: 180,
                new[] { Kw(KeywordType.StackingPowerOnKill, 2, KeywordTrigger.OnKill) });

            CreateRelic("Relic_LuckyClover", "네잎클로버", "드로우 가중치 +5",
                RelicTrigger.BattleStart, 5, RewardRarity.Rare, price: 110,
                new[] { Kw(KeywordType.DrawWeightAdd, 5) });

            CreateRelic("Relic_ThornArmor", "가시 갑옷", "피격 시 반사 데미지 2",
                RelicTrigger.OnDamageReceived, 2, RewardRarity.Rare, price: 130,
                new[] { Kw(KeywordType.CounterDamage, 2) });

            CreateRelic("Relic_SwiftBoots", "질풍 부츠", "매 턴 쉴드 +2",
                RelicTrigger.TurnStart, 2, RewardRarity.Rare, price: 100,
                new[] { Kw(KeywordType.ShieldPerTurn, 2, KeywordTrigger.OnTurnStart) });

            CreateRelic("Relic_WarBanner", "전투 깃발", "전투 시작 시 쉴드 +5 (전체)",
                RelicTrigger.BattleStart, 5, RewardRarity.Unique, price: 160,
                new[] { Kw(KeywordType.ShieldPerTurn, 5, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_HealingHerb", "치유 허브", "전투 시작 시 파티 HP 10 회복",
                RelicTrigger.BattleStart, 10, RewardRarity.Common, price: 60,
                new[] { Kw(KeywordType.HPPerTurn, 10, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_LifeCrystal", "생명력의 결정", "전투 시작 시 최대 HP +20",
                RelicTrigger.BattleStart, 20, RewardRarity.Common, price: 80,
                new[] { Kw(KeywordType.MaxHPUp, 20, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_WeaponStone", "무기 강화석", "전투 시작 시 공격력 +3",
                RelicTrigger.BattleStart, 3, RewardRarity.Rare, price: 100,
                new[] { Kw(KeywordType.ATKUp, 3, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_HardShell", "단단한 껍질", "전투 시작 시 방어력 +3",
                RelicTrigger.BattleStart, 3, RewardRarity.Common, price: 90,
                new[] { Kw(KeywordType.DEFUp, 3, KeywordTrigger.OnBattleStart) });

            CreateRelic("Relic_DragonHeart", "드래곤의 심장", "전투 시작 시 최대 HP +50",
                RelicTrigger.BattleStart, 50, RewardRarity.Unique, price: 200,
                new[] { Kw(KeywordType.MaxHPUp, 50, KeywordTrigger.OnBattleStart) });

            // ── Phase 6A: 시너지 기반 신규 유물 (기존 키워드만 사용) ──
            // 카테고리 A: 성전의 루프 (골드/처치)
            CreateRelic("Relic_ReliquaryCross", "성유물 십자가", "적 처치 시 2 골드 획득",
                RelicTrigger.OnKill, 2, RewardRarity.Rare, price: 110,
                new[] { Kw(KeywordType.BonusGold, 2, KeywordTrigger.OnKill) });

            CreateRelic("Relic_IndulgenceCoin", "면죄부 동전", "스킬 사용 시 1 골드 획득",
                RelicTrigger.OnSkillUsed, 1, RewardRarity.Common, price: 70,
                new[] { Kw(KeywordType.BonusGold, 1, KeywordTrigger.OnSkillUsed) });

            // 카테고리 B: 쉴드 공명
            CreateRelic("Relic_AegisCharm", "이지스 부적", "턴 시작 시 쉴드 +3",
                RelicTrigger.TurnStart, 3, RewardRarity.Common, price: 80,
                new[] { Kw(KeywordType.ShieldPerTurn, 3, KeywordTrigger.OnTurnStart) });

            // 카테고리 D: 학살자의 춤
            CreateRelic("Relic_SlayerSigil", "도살자 인장", "적 처치당 다음 공격력 +2 누적",
                RelicTrigger.OnKill, 2, RewardRarity.Rare, price: 130,
                new[] { Kw(KeywordType.StackingPowerOnKill, 2, KeywordTrigger.OnKill) });

            // 카테고리 E: 비전 공명
            CreateRelic("Relic_ArcaneCell", "비전 전지", "매 턴 AP +1",
                RelicTrigger.None, 1, RewardRarity.Rare, price: 140,
                new[] { Kw(KeywordType.ExtraAP, 1) });

            // 카테고리 I: 리스크/보상 (HP 페널티 강화: -10→-20)
            CreateRelic("Relic_BloodPact", "혈약", "최대 HP -20, 매 턴 AP +1",
                RelicTrigger.BattleStart, -20, RewardRarity.Unique, price: 180,
                new[] {
                    Kw(KeywordType.MaxHPUp, -20, KeywordTrigger.OnBattleStart),
                    Kw(KeywordType.ExtraAP, 1)
                });

            CreateRelic("Relic_CursedDoll", "저주받은 인형", "턴당 HP -1, 적 처치 시 최대 HP +10",
                RelicTrigger.TurnStart, -1, RewardRarity.Rare, price: 120,
                new[] {
                    Kw(KeywordType.HPPerTurn, -1, KeywordTrigger.OnTurnStart),
                    Kw(KeywordType.MaxHPUp, 10, KeywordTrigger.OnKill)
                });

            // 카테고리 C: 생명 순환
            CreateRelic("Relic_VerdantSeed", "신록의 씨앗", "힐 효과 +30%",
                RelicTrigger.None, 0, RewardRarity.Rare, price: 130,
                new[] { Kw(KeywordType.HealMul, 1.3f) });

            // 카테고리 F: 집중 사격 (단순화 버전 — 조건부는 Phase 6C에서)
            CreateRelic("Relic_DeadeyeLens", "명사수 렌즈", "공격 시 추가 데미지 +3",
                RelicTrigger.None, 3, RewardRarity.Rare, price: 110,
                new[] { Kw(KeywordType.PowerAdd, 3) });

            CreateRelic("Relic_CheapShot", "싸구려 샷", "공격 시 추가 데미지 +5",
                RelicTrigger.None, 5, RewardRarity.Common, price: 80,
                new[] { Kw(KeywordType.PowerAdd, 5) });

            // 카테고리 I: 리스크/보상 (2차 밸런스 튜닝: 1.2→1.15 / 1.35→1.4)
            CreateRelic("Relic_RecklessFury", "무모한 분노", "주는 피해 +15%, 받는 피해 +40%",
                RelicTrigger.None, 0, RewardRarity.Unique, price: 200,
                new[] {
                    Kw(KeywordType.PowerMul, 1.15f),
                    Kw(KeywordType.DamageTakenMul, 1.4f)
                });

            // ── Phase 6C-1: 기존 키워드 + 트리거 체인으로 작동하는 5종 ──
            // B3 AegisCounter: 공격 성공 시 쉴드 1 획득
            CreateRelic("Relic_AegisCounter", "이지스 반격", "공격 성공 시 쉴드 +1",
                RelicTrigger.OnDamageDealt, 1, RewardRarity.Rare, price: 130,
                new[] { Kw(KeywordType.ShieldPerTurn, 1, KeywordTrigger.OnDamageDealt) });

            // C2 SanguineBond: 힐 적용 시 쉴드 2 획득
            CreateRelic("Relic_SanguineBond", "혈연의 결속", "힐 받은 캐릭터 쉴드 +2",
                RelicTrigger.OnHealApplied, 2, RewardRarity.Rare, price: 140,
                new[] { Kw(KeywordType.ShieldPerTurn, 2, KeywordTrigger.OnHealApplied) });

            // E2 SpellWeaver: 스킬 사용 시 쉴드 1 획득
            CreateRelic("Relic_SpellWeaver", "주술사", "스킬 사용 시 쉴드 +1",
                RelicTrigger.OnSkillUsed, 1, RewardRarity.Common, price: 90,
                new[] { Kw(KeywordType.ShieldPerTurn, 1, KeywordTrigger.OnSkillUsed) });

            // H1 BrothersInArms: 전투 시작 시 파티원 수(4)만큼 ATK +1
            CreateRelic("Relic_BrothersInArms", "전우애", "전투 시작 시 파티원 수만큼 ATK +1",
                RelicTrigger.BattleStart, 4, RewardRarity.Rare, price: 150,
                new[] { Kw(KeywordType.ATKUp, 4, KeywordTrigger.OnBattleStart) });

            // H3 UnitedFront: 매 턴 종료 시 파티 전체 HP +1
            CreateRelic("Relic_UnitedFront", "연대전선", "매 턴 종료 시 파티 전체 HP +1",
                RelicTrigger.TurnEnd, 1, RewardRarity.Rare, price: 110,
                new[] { Kw(KeywordType.HPPerTurn, 1, KeywordTrigger.OnTurnEnd) });

            // ── Phase 6D: 트리거 체인 정식 구현 (일시적 "다음 공격 강화" 버프) ──
            // B2 AegisStrike: 쉴드 획득 시 다음 공강 +2 (BonusOutgoingDamage OnShieldGained)
            CreateRelic("Relic_AegisStrike", "이지스 일격", "쉴드 획득 시 다음 공격 +2",
                RelicTrigger.OnShieldGained, 2, RewardRarity.Rare, price: 100,
                new[] { Kw(KeywordType.BonusOutgoingDamage, 2, KeywordTrigger.OnShieldGained) });

            // C3 MercyBlade: 힐 받은 시 다음 공격 +1 (BonusOutgoingDamage OnHealApplied)
            CreateRelic("Relic_MercyBlade", "자비의 칼날", "힐 적용 시 다음 공격 +1",
                RelicTrigger.OnHealApplied, 1, RewardRarity.Common, price: 70,
                new[] { Kw(KeywordType.BonusOutgoingDamage, 1, KeywordTrigger.OnHealApplied) });

            // H2 VowOfGuardian: 항상 공격 +2 (PowerAdd Passive)
            CreateRelic("Relic_VowOfGuardian", "수호의 맹세", "공격 시 추가 데미지 +2",
                RelicTrigger.None, 2, RewardRarity.Rare, price: 130,
                new[] { Kw(KeywordType.PowerAdd, 2) });

            // ── Phase 6D: 신규 키워드/트리거 기반 7종 ──
            // A2 PilgrimCoin: 골드 획득 시 파티 HP +1
            CreateRelic("Relic_PilgrimCoin", "순례자의 동전", "골드 획득 시 파티 HP 1 회복",
                RelicTrigger.OnGoldEarned, 1, RewardRarity.Common, price: 80,
                new[] { Kw(KeywordType.HPPerTurn, 1, KeywordTrigger.OnGoldEarned) });

            // D3 BloodFeast: 적 처치 시 파티 HP +2
            CreateRelic("Relic_BloodFeast", "피의 향연", "적 처치 시 파티 HP 2 회복",
                RelicTrigger.OnKill, 2, RewardRarity.Rare, price: 120,
                new[] { Kw(KeywordType.HPPerTurn, 2, KeywordTrigger.OnKill) });

            // E3 ArcaneFocus: 스킬 사용 시 파티 HP +1
            CreateRelic("Relic_ArcaneFocus", "비전 집중", "스킬 사용 시 파티 HP 1 회복",
                RelicTrigger.OnSkillUsed, 1, RewardRarity.Rare, price: 130,
                new[] { Kw(KeywordType.HPPerTurn, 1, KeywordTrigger.OnSkillUsed) });

            // F2 CriticalFocus: 공격 시 추가 데미지 +4
            CreateRelic("Relic_CriticalFocus", "치명적 집중", "공격 시 추가 데미지 +4",
                RelicTrigger.None, 4, RewardRarity.Rare, price: 120,
                new[] { Kw(KeywordType.PowerAdd, 4) });

            // F3 ExecutionerBlade: 적 HP 50% 미만 시 위력 x1.5
            CreateRelic("Relic_ExecutionerBlade", "처형인의 칼날", "적 HP 50% 미만 시 위력 x1.5",
                RelicTrigger.None, 0, RewardRarity.Unique, price: 180,
                new[] { Kw(KeywordType.PowerMul, 1.5f, KeywordTrigger.OnEnemyLowHP, 0.5f) });

            // G1 GoldenIdol: 전투 시작 시 골드 +20
            CreateRelic("Relic_GoldenIdol", "황금 우상", "전투 시작 시 골드 +20 획득",
                RelicTrigger.BattleStart, 20, RewardRarity.Rare, price: 130,
                new[] { Kw(KeywordType.BonusGold, 20, KeywordTrigger.OnBattleStart) });

            // G2 CardShark: 리롤 시 파티 쉴드 +3
            CreateRelic("Relic_CardShark", "카드 상어", "리롤 시 파티 쉴드 +3",
                RelicTrigger.OnRerollUsed, 3, RewardRarity.Rare, price: 120,
                new[] { Kw(KeywordType.ShieldPerTurn, 3, KeywordTrigger.OnRerollUsed) });

            Debug.Log($"[DataGenerator] 유물 데이터 생성 완료 (기존 16 + 신규 26종 = 42종)");
        }

        private static void CreateRelic(string fileName, string relicName, string desc,
            RelicTrigger trigger, int effectValue, RewardRarity rarity, int price = 0,
            KeywordEntry[] keywords = null)
        {
            var path = $"{RELIC_PATH}/{fileName}.asset";
            var relic = GetOrCreateAsset<RelicData>(path);

            // 필드 설정 via SerializedObject
            var so = new SerializedObject(relic);
            SetField(so, "_relicName", relicName);
            SetField(so, "_description", desc);
            SetField(so, "_trigger", (int)trigger);
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
