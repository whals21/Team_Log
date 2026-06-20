using UnityEngine;

namespace TeamLog.Meta
{
    /// <summary>
    /// 어센션 modifier 유형 — 클리어 누적 난이도 상승.
    /// 런 클리어 시 자동 +1레벨 (최대 15).
    /// 7개 핵심 modifier × 2~3회 누적 + 최종 BossHpPercent = 총 15레벨.
    /// </summary>
    public enum AscensionModifierType
    {
        EnemyHpPercent,       // 적 HP ±%
        PlayerMaxHpPercent,   // 파티 MaxHP ±%
        RerollCount,          // 턴당 리롤 ±N
        StartGold,            // 시작 골드 ±N
        HealPercent,          // 휴식/힐 효율 ±%
        EnemyAtkPercent,      // 적 ATK ±%
        BossHpPercent         // 보스 HP ±% (어센션 15 전용)
    }

    /// <summary>
    /// 어센션 modifier 정적 데이터 (ScriptableObject).
    /// DataGenerator.Ascension.cs에서 7종 생성 (BossHpPercent 제외 — 레벨 15 고정값 하드코딩).
    /// </summary>
    [CreateAssetMenu(fileName = "AscensionModifier", menuName = "TeamLog/Ascension Modifier")]
    public class AscensionModifierData : ScriptableObject
    {
        [Header("식별자")]
        [SerializeField] private string _modifierId;
        [SerializeField] private string _displayName;
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        [Header("타입")]
        [SerializeField] private AscensionModifierType _modifierType;

        [Header("값 (누적 적용 시 per-stack 기준)")]
        [Tooltip("정수형 modifier(RerollCount/StartGold)에 사용. 음수 허용.")]
        [SerializeField] private int _intValue;
        [Tooltip("비율 modifier(*Percent)에 사용. 0.05 = 5%. 음수 허용.")]
        [SerializeField] private float _floatValue;

        public string ModifierId => _modifierId;
        public string DisplayName => _displayName;
        public string Description => _description;
        public AscensionModifierType ModifierType => _modifierType;
        public int IntValue => _intValue;
        public float FloatValue => _floatValue;
    }
}
