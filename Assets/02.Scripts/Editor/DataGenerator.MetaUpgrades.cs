#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TeamLog.Meta;
using TeamLog.Reward;
using TeamLog.Skill;

namespace TeamLog.Editor
{
    /// <summary>
    /// DataGenerator — 메타 강화 데이터 생성 (Phase 8A)
    /// 진입점/스킬/캐릭터/유틸리티: DataGenerator.cs
    /// 증강 데이터/스폰 패턴: DataGenerator.Augments.cs
    /// 이벤트 데이터: DataGenerator.Events.cs
    /// 유물 데이터: DataGenerator.Relics.cs
    /// 팔레트 (UI/오디오/VFX): DataGenerator.Palettes.cs
    /// 스테이지 테마: DataGenerator.Stages.cs
    /// 캐릭터 특성: DataGenerator.Traits.cs
    /// 메타 강화: DataGenerator.MetaUpgrades.cs
    /// </summary>
    public static partial class DataGenerator
    {
        private const string META_UPGRADE_PATH = "Assets/03.Data/MetaUpgrades";

        [MenuItem("TeamLog/Generate Meta Upgrades", false, 111)]
        public static void GenerateMetaUpgradeData()
        {
            EnsureFolder(META_UPGRADE_PATH);

            // ── RelicUnlock: 시너지 26종 (기본 16종은 자동 해금 — 에셋 불필요) ──

            // A: 성전의 루프
            CreateRelicUnlock("Meta_ReliquaryCross", "성유물 십자가 해금", "적 처치 시 2 골드 획득", "Relic_ReliquaryCross", 30, 0);
            CreateRelicUnlock("Meta_IndulgenceCoin", "면죄부 동전 해금", "스킬 사용 시 1 골드 획득", "Relic_IndulgenceCoin", 30, 0);

            // B: 쉴드 공명
            CreateRelicUnlock("Meta_AegisCharm", "이지스 부적 해금", "턴 시작 시 쉴드 +3", "Relic_AegisCharm", 30, 0);
            CreateRelicUnlock("Meta_AegisCounter", "이지스 반격 해금", "공격 성공 시 쉴드 +1", "Relic_AegisCounter", 60, 0);
            CreateRelicUnlock("Meta_AegisStrike", "이지스 일격 해금", "쉴드 획득 시 다음 공격 +2", "Relic_AegisStrike", 60, 0);

            // C: 생명 순환
            CreateRelicUnlock("Meta_VerdantSeed", "신록의 씨앗 해금", "힐 효과 +30%", "Relic_VerdantSeed", 60, 0);
            CreateRelicUnlock("Meta_SanguineBond", "혈연의 결속 해금", "힐 받은 캐릭터 쉴드 +2", "Relic_SanguineBond", 60, 0);

            // D: 학살자의 춤
            CreateRelicUnlock("Meta_SlayerSigil", "도살자 인장 해금", "적 처치당 다음 공격력 +2 누적", "Relic_SlayerSigil", 60, 0);
            CreateRelicUnlock("Meta_BloodFeast", "피의 향연 해금", "적 처치 시 파티 HP 2 회복", "Relic_BloodFeast", 60, 0);

            // E: 비전 공명
            CreateRelicUnlock("Meta_ArcaneCell", "비전 전지 해금", "매 턴 AP +1", "Relic_ArcaneCell", 100, 0);
            CreateRelicUnlock("Meta_ArcaneFocus", "비전 집중 해금", "스킬 사용 시 파티 HP 1 회복", "Relic_ArcaneFocus", 60, 0);
            CreateRelicUnlock("Meta_SpellWeaver", "주술사 해금", "스킬 사용 시 쉴드 +1", "Relic_SpellWeaver", 30, 0);

            // F: 집중 사격
            CreateRelicUnlock("Meta_DeadeyeLens", "명사수 렌즈 해금", "공격 시 추가 데미지 +3", "Relic_DeadeyeLens", 60, 0);
            CreateRelicUnlock("Meta_CheapShot", "싸구려 샷 해금", "공격 시 추가 데미지 +5", "Relic_CheapShot", 30, 0);
            CreateRelicUnlock("Meta_CriticalFocus", "치명적 집중 해금", "공격 시 추가 데미지 +4", "Relic_CriticalFocus", 60, 0);
            CreateRelicUnlock("Meta_ExecutionerBlade", "처형인의 칼날 해금", "적 HP 50% 미만 시 위력 x1.5", "Relic_ExecutionerBlade", 100, 1);

            // G: 황금 순환
            CreateRelicUnlock("Meta_GoldenIdol", "황금 우상 해금", "전투 시작 시 골드 +20 획득", "Relic_GoldenIdol", 60, 0);
            CreateRelicUnlock("Meta_CardShark", "카드 상어 해금", "리롤 시 파티 쉴드 +3", "Relic_CardShark", 60, 0);

            // H: 연대
            CreateRelicUnlock("Meta_BrothersInArms", "전우애 해금", "전투 시작 시 파티원 수만큼 ATK +1", "Relic_BrothersInArms", 60, 0);
            CreateRelicUnlock("Meta_VowOfGuardian", "수호의 맹세 해금", "공격 시 추가 데미지 +2", "Relic_VowOfGuardian", 60, 0);
            CreateRelicUnlock("Meta_UnitedFront", "연대전선 해금", "매 턴 종료 시 파티 전체 HP +1", "Relic_UnitedFront", 60, 0);

            // I: 리스크/보상 + 기타
            CreateRelicUnlock("Meta_BloodPact", "혈약 해금", "최대 HP -20, 매 턴 AP +1", "Relic_BloodPact", 100, 1);
            CreateRelicUnlock("Meta_CursedDoll", "저주받은 인형 해금", "턴당 HP -1, 적 처치 시 최대 HP +10", "Relic_CursedDoll", 60, 0);
            CreateRelicUnlock("Meta_RecklessFury", "무모한 분노 해금", "주는 피해 +15%, 받는 피해 +40%", "Relic_RecklessFury", 100, 1);
            CreateRelicUnlock("Meta_MercyBlade", "자비의 칼날 해금", "힐 적용 시 다음 공격 +1", "Relic_MercyBlade", 30, 0);
            CreateRelicUnlock("Meta_PilgrimCoin", "순례자의 동전 해금", "골드 획득 시 파티 HP 1 회복", "Relic_PilgrimCoin", 30, 0);

            // ── 일회성 글로벌 강화 4종 ──
            CreateSimpleUpgrade("Meta_StartingRelicSlot", "시작 유물 지급",
                "런 시작 시 유물 1개 자동 지급 (랜덤)", MetaUpgradeType.StartingRelicSlot, 100, 0);
            CreateSimpleUpgrade("Meta_StartingRelicChoice", "시작 유물 3选1",
                "런 시작 시 유물 3개 중 1개 선택", MetaUpgradeType.StartingRelicChoice, 150, 3);
            CreateSimpleUpgrade("Meta_ExtraReroll", "리롤 +1",
                "전투 턴당 리롤 횟수 +1", MetaUpgradeType.ExtraReroll, 50, 0);
            CreateSimpleUpgrade("Meta_PartyHealBoost", "휴식 힐 강화",
                "휴식지 파티 HP 회복량 +10%", MetaUpgradeType.PartyHealBoost, 80, 0);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DataGenerator] 메타 강화 30종 생성 완료 (RelicUnlock 26 + 글로벌 4)");
        }

        private static void CreateRelicUnlock(string fileName, string displayName, string desc,
            string targetRelicId, int memoryCost, int soulCost)
        {
            CreateSimpleUpgrade(fileName, displayName, desc, MetaUpgradeType.RelicUnlock, memoryCost, soulCost, targetRelicId);
        }

        private static void CreateSimpleUpgrade(string fileName, string displayName, string desc,
            MetaUpgradeType type, int memoryCost, int soulCost, string targetRelicId = "")
        {
            EnsureFolder(META_UPGRADE_PATH);
            var path = $"{META_UPGRADE_PATH}/{fileName}.asset";
            var upgrade = GetOrCreateAsset<MetaUpgradeData>(path);
            upgrade.name = fileName;

            SetPrivateField(upgrade, "_upgradeId", fileName);
            SetPrivateField(upgrade, "_displayName", displayName);
            SetPrivateField(upgrade, "_description", desc);
            SetPrivateField(upgrade, "_type", type);
            SetPrivateField(upgrade, "_memoryCost", memoryCost);
            SetPrivateField(upgrade, "_soulCost", soulCost);
            SetPrivateField(upgrade, "_targetRelicId", targetRelicId);

            EditorUtility.SetDirty(upgrade);
        }
    }
}
#endif
