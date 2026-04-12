using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 액션 슬롯 UI (스킬 아이콘 + 이름 + 코스트)
    /// </summary>
    public class ActionSlotUI : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Image _skillIcon;
        [SerializeField] private TextMeshProUGUI _skillNameText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private GameObject _selectionBorder;
        [SerializeField] private GameObject _executionOrderBadge;
        [SerializeField] private TextMeshProUGUI _executionOrderText;
        [SerializeField] private GameObject _assignedOverlay;

        [Header("Colors")]
        [SerializeField] private Color _attackColor = new Color(0.77f, 0.12f, 0.23f);
        [SerializeField] private Color _healColor = new Color(0.15f, 0.68f, 0.38f);
        [SerializeField] private Color _buffColor = new Color(0.96f, 0.82f, 0.25f);
        [SerializeField] private Color _debuffColor = new Color(0.6f, 0.3f, 0.8f);

        [Header("Button")]
        [SerializeField] private Button _button;

        private int _slotIndex;
        private SkillData _skill;
        private Character _caster;
        private ActionBarUI _parent;

        public SkillData Skill => _skill;
        public Character Caster => _caster;

        public void Setup(int slotIndex, ActionBarUI parent)
        {
            _slotIndex = slotIndex;
            _parent = parent;

            if (_button != null)
                _button.onClick.AddListener(OnClick);
        }

        public void SetSkill(SkillData skill, Character caster)
        {
            _skill = skill;
            _caster = caster;

            if (_skillNameText != null)
                _skillNameText.text = skill?.SkillName ?? "---";

            if (_costText != null)
                _costText.text = skill?.Cost > 0 ? skill.Cost.ToString() : "";

            if (_skillIcon != null)
            {
                _skillIcon.color = GetSkillColor(skill);
            }
        }

        public void Clear()
        {
            _skill = null;
            _caster = null;

            if (_skillNameText != null)
                _skillNameText.text = "---";

            if (_costText != null)
                _costText.text = "";

            if (_skillIcon != null)
                _skillIcon.color = Color.gray;

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (_selectionBorder != null)
                _selectionBorder.SetActive(selected);
        }

        public void SetExecutionOrder(int order)
        {
            if (_executionOrderBadge != null)
                _executionOrderBadge.SetActive(order >= 0);

            if (_executionOrderText != null && order >= 0)
                _executionOrderText.text = (order + 1).ToString();
        }

        public void SetAssigned(bool assigned)
        {
            if (_assignedOverlay != null)
                _assignedOverlay.SetActive(assigned);
        }

        private Color GetSkillColor(SkillData skill)
        {
            if (skill == null) return Color.gray;

            return skill.Type switch
            {
                SkillType.Attack => _attackColor,
                SkillType.Heal => _healColor,
                SkillType.Buff => _buffColor,
                SkillType.Debuff => _debuffColor,
                _ => Color.white
            };
        }

        private void OnClick()
        {
            if (_skill != null && _parent != null)
            {
                _parent.SelectSlot(_slotIndex);
            }
        }
    }
}
