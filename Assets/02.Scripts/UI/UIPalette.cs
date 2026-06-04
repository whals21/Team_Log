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

        [Header("Accent")]
        public Color AccentRed = new Color(0.77f, 0.12f, 0.23f);
        public Color AccentGreen = new Color(0.15f, 0.68f, 0.38f);
        public Color AccentYellow = new Color(0.96f, 0.82f, 0.25f);

        [Header("Text")]
        public Color TextWhite = Color.white;
        public Color TextDim = new Color(0.7f, 0.7f, 0.75f);

        [Header("HP")]
        public Color HPNormal = new Color(0.15f, 0.68f, 0.38f);
        public Color HPLow = new Color(1f, 0.5f, 0f);
        public Color HPLowThreshold = new Color(0.3f, 0.3f, 0.3f);
        public Color HPEnemy = new Color(0.77f, 0.12f, 0.23f);

        [Header("Shield / Damage / Heal")]
        public Color ShieldBrown = new Color(0.72f, 0.45f, 0.2f);
        public Color DamageColor = new Color(0.85f, 0.2f, 0.2f);
        public Color HealColor = new Color(0.15f, 0.68f, 0.38f);
        public Color ShieldColor = new Color(0.72f, 0.45f, 0.2f);

        [Header("AP / Reroll")]
        public Color APNormal = new Color(0.96f, 0.82f, 0.25f);
        public Color APShortage = new Color(0.85f, 0.2f, 0.2f);
        public Color RerollNormal = new Color(0.72f, 0.45f, 0.2f);
        public Color RerollEmpty = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [Header("Skill Type Colors")]
        public Color SkillAttack = new Color(0.77f, 0.12f, 0.23f);
        public Color SkillHeal = new Color(0.15f, 0.68f, 0.38f);
        public Color SkillBuff = new Color(0.96f, 0.82f, 0.25f);
        public Color SkillDebuff = new Color(0.6f, 0.3f, 0.8f);
        public Color SkillShield = new Color(0.72f, 0.45f, 0.2f);
        public Color SkillPurify = new Color(0.4f, 0.8f, 0.95f);

        [Header("Border")]
        public Color BorderRed = new Color(0.6f, 0.1f, 0.18f, 0.8f);

        [Header("Log Colors")]
        public Color LogDamage = new Color(0.85f, 0.3f, 0.3f);
        public Color LogHeal = new Color(0.3f, 0.8f, 0.5f);
        public Color LogBuff = new Color(0.9f, 0.8f, 0.3f);
        public Color LogDebuff = new Color(0.7f, 0.4f, 0.9f);
        public Color LogSystem = new Color(0.7f, 0.7f, 0.75f);

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

        public static void Initialize(UIPalette palette)
        {
            _default = palette;
        }
    }
}
