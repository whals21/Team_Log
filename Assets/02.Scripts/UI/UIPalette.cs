using UnityEngine;

namespace TeamLog.UI
{
    /// <summary>
    /// UI 색상 설계 토큰 — 프로젝트 전체 UI 색상을 단일 SO로 관리
    /// </summary>
    [CreateAssetMenu(fileName = "UIPalette", menuName = "TeamLog/UIPalette")]
    public class UIPalette : ScriptableObject
    {
        [Header("Background")]
        public Color BgDark = new Color(0.08f, 0.08f, 0.16f);
        public Color BgPanel = new Color(0.04f, 0.04f, 0.1f, 0.8f);
        public Color BgPanelLight = new Color(0.06f, 0.06f, 0.14f, 0.95f);
        public Color BgTopBar = new Color(0.03f, 0.03f, 0.08f, 0.95f);

        [Header("Party Select — Dark Fantasy Gothic")]
        // 깊이 계층 5단계 (어두움 → 밝음)
        public Color DFVoid    = new Color(0.020f, 0.020f, 0.035f);  // #050509 가장 어두움
        public Color DFAbyss   = new Color(0.039f, 0.039f, 0.078f);  // #0a0a14
        public Color DFDepth   = new Color(0.067f, 0.067f, 0.122f);  // #11111f
        public Color DFSlate   = new Color(0.102f, 0.102f, 0.180f);  // #1a1a2e 기본 패널
        public Color DFSlate2  = new Color(0.137f, 0.137f, 0.278f);  // #232347 밝은 패널

        // 골드 4단계 (밝 → 어두움)
        public Color DFGoldL   = new Color(0.957f, 0.827f, 0.369f);  // #f4d35e 라이트 (강조)
        public Color DFGold    = new Color(0.831f, 0.686f, 0.216f);  // #d4af37 기본 골드
        public Color DFGoldD   = new Color(0.545f, 0.412f, 0.078f);  // #8b6914 딥 (테두리)
        public Color DFGoldX   = new Color(0.290f, 0.227f, 0.051f);  // #4a3a0d 흑금 (외곽)

        // 핏빛 3단계
        public Color DFBloodDeep = new Color(0.357f, 0f, 0f);        // #5a0000 매우 어두움 (그림자)
        public Color DFBlood     = new Color(0.545f, 0f, 0f);        // #8b0000 기본 핏빛
        public Color DFBloodL    = new Color(0.751f, 0.224f, 0.169f); // #c0392b 라이트 (강조)

        // 양피지 4단계
        public Color DFParchment  = new Color(0.788f, 0.706f, 0.522f); // #c9b485 본문 텍스트
        public Color DFParchmentD = new Color(0.541f, 0.467f, 0.322f); // #8a7752 디스크립션
        public Color DFParchmentDd= new Color(0.302f, 0.247f, 0.157f); // #4d3f28 딥
        public Color DFParchmentX = new Color(0.165f, 0.141f, 0.094f); // #2a2418 가장 어두움 (패널 배경)

        // 잉크 텍스트 3단계
        public Color DFInk      = new Color(0.941f, 0.902f, 0.816f);  // #f0e6d0 본문 밝은 텍스트
        public Color DFInkDim   = new Color(0.659f, 0.596f, 0.471f);  // #a89878 희미한 텍스트
        public Color DFInkFaint = new Color(0.420f, 0.369f, 0.267f);  // #6b5e44 거의 안 보이는 텍스트

        [Header("Accent")]
        public Color AccentRed = new Color(0.77f, 0.12f, 0.23f);
        public Color AccentGreen = new Color(0.15f, 0.68f, 0.38f);
        public Color AccentYellow = new Color(0.96f, 0.82f, 0.25f);

        [Header("Text")]
        public Color TextWhite = Color.white;
        public Color TextDim = new Color(0.82f, 0.82f, 0.87f);

        [Header("HP")]
        public Color HPNormal = new Color(0.15f, 0.68f, 0.38f);
        public Color HPLow = new Color(1f, 0.5f, 0f);
        public Color HPLowThreshold = new Color(0.3f, 0.3f, 0.3f);
        public Color HPEnemy = new Color(0.77f, 0.12f, 0.23f);

        [Header("Shield / Damage / Heal")]
        public Color ShieldBrown = new Color(0.72f, 0.45f, 0.2f);
        public Color DamageColor = new Color(0.85f, 0.2f, 0.2f);
        public Color HealColor = new Color(0.15f, 0.68f, 0.38f);

        [Header("AP / Reroll")]
        public Color APNormal = new Color(0.96f, 0.82f, 0.25f);
        public Color APShortage = new Color(0.85f, 0.2f, 0.2f);
        public Color RerollNormal = Color.white;
        public Color RerollEmpty = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [Header("Skill Type Colors")]
        public Color SkillAttack = new Color(0.77f, 0.12f, 0.23f);
        public Color SkillHeal = new Color(0.15f, 0.68f, 0.38f);
        public Color SkillBuff = new Color(0.96f, 0.82f, 0.25f);
        public Color SkillDebuff = new Color(0.6f, 0.3f, 0.8f);
        public Color SkillShield = new Color(0.72f, 0.45f, 0.2f);
        public Color SkillPurify = new Color(0.4f, 0.8f, 0.95f);
        // Party Select 신규 — 소환/특수 타입
        public Color SkillSummon  = new Color(0.490f, 0.639f, 0.290f);  // #7da34a 시체/소환
        public Color SkillSpecial = new Color(0.702f, 0.533f, 1.000f);  // #b388ff Discover/특수

        [Header("Border")]
        public Color BorderRed = new Color(0.6f, 0.1f, 0.18f, 0.8f);

        [Header("Log Colors")]
        public Color LogDamage = new Color(0.95f, 0.4f, 0.4f);
        public Color LogHeal = new Color(0.4f, 0.9f, 0.6f);
        public Color LogBuff = new Color(0.9f, 0.8f, 0.3f);
        public Color LogDebuff = new Color(0.8f, 0.5f, 1.0f);
        public Color LogSystem = new Color(0.82f, 0.82f, 0.87f);

        [Header("Intent")]
        public Color IntentAttack = new Color(0.85f, 0.2f, 0.2f);
        public Color IntentShield = new Color(0.72f, 0.45f, 0.2f);
        public Color IntentHeal = new Color(0.15f, 0.68f, 0.38f);
        public Color IntentBuff = new Color(0.96f, 0.82f, 0.25f);
        public Color IntentDebuff = new Color(0.6f, 0.3f, 0.8f);

        [Header("Slot States")]
        public Color SlotAffordable = new Color(0.1f, 0.1f, 0.18f, 0.9f);
        public Color SlotExpensive = new Color(0.15f, 0.05f, 0.05f, 0.9f);

        [Header("Status Effect Colors")]
        public Color EffectPoison = new Color(0.55f, 0.1f, 0.55f);
        public Color EffectBurn = new Color(0.8f, 0.3f, 0.05f);
        public Color EffectStun = new Color(0.6f, 0.6f, 0.1f);
        public Color EffectFreeze = new Color(0.2f, 0.5f, 0.8f);
        public Color EffectBleed = new Color(0.7f, 0.05f, 0.05f);
        public Color EffectAttackUp = new Color(0.15f, 0.55f, 0.2f);
        public Color EffectAttackDown = new Color(0.6f, 0.15f, 0.15f);
        public Color EffectDefenseUp = new Color(0.1f, 0.5f, 0.3f);
        public Color EffectDefenseDown = new Color(0.7f, 0.2f, 0.1f);
        public Color EffectRegeneration = new Color(0.1f, 0.5f, 0.5f);
        public Color EffectTaunt = new Color(0.6f, 0.45f, 0.1f);
        public Color EffectSleep = new Color(0.4f, 0.3f, 0.6f);
        public Color EffectShield = new Color(0.5f, 0.35f, 0.15f);
        public Color EffectDefault = new Color(0.4f, 0.4f, 0.4f);

        [Header("Resource Colors (Phase CC)")]
        public Color ResourceEmber     = new Color(1.000f, 0.420f, 0.208f); // #ff6b35 Ashe — 주황/빨강 (화염)
        public Color ResourceVengeance = new Color(0.659f, 0.196f, 0.290f); // #a8324a Duran — 핏빛 보라 (복수)
        public Color ResourceFrost     = new Color(0.369f, 0.773f, 0.910f); // #5ec5e8 Lumi — 하늘 (냉기)
        public Color ResourceProphecy  = new Color(0.431f, 0.835f, 0.698f); // #6ed5b2 Sibyl — 청록 (예언/시간)
        public Color ResourceCharge    = new Color(0.969f, 0.816f, 0.275f); // #f7d046 Taranis — 번개 노랑
        public Color ResourceShadows   = new Color(0.608f, 0.431f, 0.761f); // #9b6ec2 Umbra — 보라 (치명타)
        public Color ResourceCombo     = new Color(0.831f, 0.627f, 0.090f); // #d4a017 Aster — 황금 (연속 사격)
        public Color ResourceCorpse    = new Color(0.490f, 0.639f, 0.290f); // #7da34a Mortis — 독녹 (시체)
        public Color ResourceDiscover  = new Color(0.702f, 0.533f, 1.000f); // #b388ff Cael — 연보라 (발견)
        public Color ResourceMelody    = new Color(1.000f, 0.561f, 0.671f); // #ff8fab Calliope — 분홍 (선율)
        public Color ResourceMercy     = new Color(1.000f, 0.878f, 0.510f); // #ffe082 Elara — 은금 (자비)
        public Color ResourceDefault   = new Color(0.6f, 0.6f, 0.6f);

        [Header("Trait Colors")]
        public Color TraitRegenerate = new Color(0.1f, 0.5f, 0.5f);
        public Color TraitOpportunist = new Color(0.6f, 0.15f, 0.15f);
        public Color TraitPhaseShift = new Color(0.3f, 0.3f, 0.7f);
        public Color TraitCounter = new Color(0.7f, 0.35f, 0.1f);
        public Color TraitThorns = new Color(0.15f, 0.55f, 0.2f);
        public Color TraitShell = new Color(0.5f, 0.45f, 0.35f);
        public Color TraitSturdy = new Color(0.4f, 0.4f, 0.5f);
        public Color TraitArcaneFury = new Color(0.4f, 0.15f, 0.6f);
        public Color TraitCorrosive = new Color(0.3f, 0.5f, 0.15f);
        public Color TraitRally = new Color(0.7f, 0.6f, 0.1f);
        public Color TraitRampage = new Color(0.7f, 0.15f, 0.1f);
        public Color TraitImmortal = new Color(0.55f, 0.1f, 0.55f);
        public Color TraitDefault = new Color(0.4f, 0.4f, 0.4f);

        [Header("Rarity / Grade Colors")]
        public Color RarityCommon = Color.white;
        public Color RarityRare = new Color(0.3f, 0.6f, 1f);
        public Color RarityUnique = new Color(0.7f, 0.3f, 0.9f);
        public Color GradeCursed = new Color(0.8f, 0.15f, 0.15f);

        // ── Static Access ──

        private static UIPalette _default;

        public static UIPalette Default
        {
            get
            {
                if (_default != null) return _default;
                _default = Resources.Load<UIPalette>("UIPalette");
#if UNITY_EDITOR
                if (_default == null)
                    _default = UnityEditor.AssetDatabase.LoadAssetAtPath<UIPalette>("Assets/03.Data/UIPalette.asset");
#endif
                if (_default != null) return _default;
                // Runtime fallback: create instance with defaults
                _default = CreateInstance<UIPalette>();
                return _default;
            }
        }
    }
}
