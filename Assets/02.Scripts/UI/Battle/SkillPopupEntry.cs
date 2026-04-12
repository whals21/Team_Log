using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 캐릭터 팝업 내 스킬 항목 UI
    /// </summary>
    public class SkillPopupEntry : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _typeText;
        [SerializeField] private Image _typeBg;

        [Header("Skill Type Colors")]
        [SerializeField] private Color _attackColor = new Color(0.77f, 0.12f, 0.23f);
        [SerializeField] private Color _healColor = new Color(0.15f, 0.68f, 0.38f);
        [SerializeField] private Color _buffColor = new Color(0.96f, 0.82f, 0.25f);
        [SerializeField] private Color _debuffColor = new Color(0.6f, 0.3f, 0.8f);

        public void Setup(SkillData skill, int level = 1)
        {
            if (_nameText != null)
                _nameText.text = skill.SkillName;

            if (_levelText != null)
                _levelText.text = $"Lv.{level}";

            if (_descriptionText != null)
                _descriptionText.text = skill.Description;

            if (_typeText != null)
                _typeText.text = GetSkillTypeLabel(skill.Type);

            if (_typeBg != null)
                _typeBg.color = GetSkillTypeColor(skill.Type);

            if (_iconImage != null)
                _iconImage.color = GetSkillTypeColor(skill.Type);
        }

        private string GetSkillTypeLabel(SkillType type) => type switch
        {
            SkillType.Attack => "공격",
            SkillType.Heal => "치유",
            SkillType.Buff => "강화",
            SkillType.Debuff => "약화",
            _ => type.ToString()
        };

        private Color GetSkillTypeColor(SkillType type) => type switch
        {
            SkillType.Attack => _attackColor,
            SkillType.Heal => _healColor,
            SkillType.Buff => _buffColor,
            SkillType.Debuff => _debuffColor,
            _ => Color.gray
        };
    }
}
