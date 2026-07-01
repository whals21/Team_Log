#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TeamLog.Meta;

namespace TeamLog.Editor
{
    /// <summary>
    /// DataGenerator.Ascension — 어센션 modifier 에셋 자동 생성.
    /// 진입점/스킬/캐릭터/패턴/유틸리티: DataGenerator.cs
    /// 증강 데이터/스폰 패턴: DataGenerator.Augments.cs
    /// 이벤트 데이터: DataGenerator.Events.cs
    /// 유물 데이터: DataGenerator.Relics.cs
    /// 팔레트 (UI/오디오/VFX): DataGenerator.Palettes.cs
    /// 스테이지 테마: DataGenerator.Stages.cs
    /// 캐릭터 특성: DataGenerator.Traits.cs
    /// 메타 강화: DataGenerator.MetaUpgrades.cs
    /// 어센션 modifier: DataGenerator.Ascension.cs (본 파일)
    /// </summary>
    public static partial class DataGenerator
    {
        private const string ASCENSION_PATH = "Assets/03.Data/Ascension";

        /// <summary>
        /// 6개 핵심 modifier 에셋 생성.
        /// 각 에셋의 값은 per-stack 값(단일 적용 시 효과) — 누적은 AscensionManager가 stack × value로 계산.
        /// 참고: EnemyAtkPercent는 ATK=0 구조에서 무의미하여 제거됨 (2026-06-30). 레벨 6/12는 빈 레벨.
        /// </summary>
        [MenuItem("TeamLog/Generate Ascension Data", false, 112)]
        public static void GenerateAscensionModifiers()
        {
            EnsureFolder(ASCENSION_PATH);

            CreateModifier("AscMod_EnemyHp", "적 강화",
                "적 HP +5% (누적: 7렙 +10%, 13렙 +15%)",
                AscensionModifierType.EnemyHpPercent, intValue: 0, floatValue: 0.05f);

            CreateModifier("AscMod_PlayerHp", "파티 쇠약",
                "파티 MaxHP -5% (10렙 누적 시 -10%)",
                AscensionModifierType.PlayerMaxHpPercent, intValue: 0, floatValue: -0.05f);

            CreateModifier("AscMod_Heal", "회복 억제",
                "휴식/힐 효율 -10% (11렙 누적 시 -20%)",
                AscensionModifierType.HealPercent, intValue: 0, floatValue: -0.10f);

            CreateModifier("AscMod_Reroll", "리롤 제한",
                "턴당 리롤 -1회 (9렘 -2, 14렙 -3)",
                AscensionModifierType.RerollCount, intValue: -1, floatValue: 0f);

            CreateModifier("AscMod_StartGold", "시작 골드 감소",
                "시작 골드 -10 (8렙 누적 시 -20)",
                AscensionModifierType.StartGold, intValue: -10, floatValue: 0f);

            CreateModifier("AscMod_BossHp", "보스 강화 (최종)",
                "보스 HP +20% (어센션 15 전용)",
                AscensionModifierType.BossHpPercent, intValue: 0, floatValue: 0.20f);

            // EnemyAtk 제거로 orphan 된 레거시 에셋 정리
            string legacyPath = $"{ASCENSION_PATH}/AscMod_EnemyAtk.asset";
            if (AssetDatabase.LoadAssetAtPath<AscensionModifierData>(legacyPath) != null)
            {
                bool ok = AssetDatabase.DeleteAsset(legacyPath);
                if (ok) Debug.Log("[DataGenerator] 레거시 에셋 삭제: AscMod_EnemyAtk");
                else Debug.LogWarning("[DataGenerator] 레거시 에셋 삭제 실패: AscMod_EnemyAtk");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DataGenerator] 어센션 modifier 6종 생성 완료 (EnemyAtk 제거됨)");
        }

        private static void CreateModifier(string fileName, string displayName, string desc,
            AscensionModifierType type, int intValue, float floatValue)
        {
            EnsureFolder(ASCENSION_PATH);
            var path = $"{ASCENSION_PATH}/{fileName}.asset";
            var mod = GetOrCreateAsset<AscensionModifierData>(path);
            mod.name = fileName;

            SetPrivateField(mod, "_modifierId", fileName);
            SetPrivateField(mod, "_displayName", displayName);
            SetPrivateField(mod, "_description", desc);
            SetPrivateField(mod, "_modifierType", type);
            SetPrivateField(mod, "_intValue", intValue);
            SetPrivateField(mod, "_floatValue", floatValue);

            EditorUtility.SetDirty(mod);
        }
    }
}
#endif
