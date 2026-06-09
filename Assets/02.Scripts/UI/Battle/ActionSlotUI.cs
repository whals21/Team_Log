using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using TeamLog.Characters;
using TeamLog.UI;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 액션 슬롯 UI (스킬 아이콘 + 이름 + 코스트)
    /// </summary>
    public class ActionSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Visuals")]
        [SerializeField] private Image _skillIcon;
        [SerializeField] private TextMeshProUGUI _skillNameText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private TextMeshProUGUI _casterNameText;
        [SerializeField] private GameObject _selectionBorder;
        [SerializeField] private GameObject _executionOrderBadge;
        [SerializeField] private TextMeshProUGUI _executionOrderText;
        [SerializeField] private GameObject _assignedOverlay;

        [Header("AP Shortage")]
        [SerializeField] private GameObject _apShortageBorder;

        [Header("Button")]
        [SerializeField] private Button _button;

        [Header("Reroll")]
        [SerializeField] private Button _rerollButton;

        private int _slotIndex;
        private SkillData _skill;
        private Character _caster;
        private ActionBarUI _parent;
        private Color _originalSkillColor;
        private Color _originalCostColor;
        private Color _originalBgColor;
        private bool _colorsStored;
        private Image _bgImage;

        public SkillData Skill => _skill;
        public Character Caster => _caster;

        public event System.Action<int> OnSlotRerollRequested;

        public void Setup(int slotIndex, ActionBarUI parent)
        {
            _slotIndex = slotIndex;
            _parent = parent;

            _bgImage = GetComponent<Image>();
            if (_bgImage != null)
                _originalBgColor = _bgImage.color;

            if (_button != null)
                _button.onClick.AddListener(OnClick);

            if (_rerollButton != null)
                _rerollButton.onClick.AddListener(OnRerollClick);
        }

        public void SetSkill(SkillData skill, Character caster)
        {
            _skill = skill;
            _caster = caster;

            if (_skillNameText != null)
                _skillNameText.text = skill?.SkillName ?? "---";

            if (_costText != null)
                _costText.text = skill?.Cost > 0 ? skill.Cost.ToString() : "";

            if (_casterNameText != null)
                _casterNameText.text = caster != null ? caster.Name : "";

            if (_skillIcon != null)
            {
                _skillIcon.sprite = skill?.Icon;
                _skillIcon.color = GetSkillColor(skill);
                _originalSkillColor = _skillIcon.color;
                _colorsStored = true;
            }

            if (_costText != null)
                _originalCostColor = _costText.color;

            // 스킬 타입별 배경 틴트 (P1-1)
            if (_bgImage != null)
            {
                var skillColor = GetSkillColor(skill);
                _bgImage.color = new Color(skillColor.r * 0.25f, skillColor.g * 0.25f, skillColor.b * 0.25f, 0.9f);
            }

            // 툴팁 설정
            if (skill != null)
            {
                var tooltip = GetComponent<TooltipTarget>();
                if (tooltip == null) tooltip = gameObject.AddComponent<TooltipTarget>();

                string subtitle = BuildSkillSubtitle(skill);
                string desc = BattleDisplayUtil.BuildSkillDescription(skill, caster);
                string fullDesc = string.IsNullOrEmpty(skill.Description) ? desc : skill.Description;
                if (!string.IsNullOrEmpty(desc) && !string.IsNullOrEmpty(skill.Description) && skill.Description != desc)
                    fullDesc = skill.Description + "\n" + desc;

                tooltip.SetContent(skill.SkillName, subtitle, fullDesc);
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

            if (_casterNameText != null)
                _casterNameText.text = "";

            if (_skillIcon != null)
            {
                _skillIcon.sprite = null;
                _skillIcon.color = Color.gray;
            }

            if (_bgImage != null)
                _bgImage.color = _originalBgColor;

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

        public void SetAffordable(bool affordable)
        {
            if (_skillIcon != null && _colorsStored)
                _skillIcon.color = affordable ? _originalSkillColor : new Color(_originalSkillColor.r, _originalSkillColor.g, _originalSkillColor.b, 0.3f);

            if (_costText != null)
                _costText.color = affordable ? _originalCostColor : Color.red;

            if (_button != null)
                _button.interactable = affordable;

            if (_apShortageBorder != null)
                _apShortageBorder.SetActive(!affordable);
        }

        public void SetRerollAvailable(bool available)
        {
            if (_rerollButton != null)
                _rerollButton.gameObject.SetActive(available && _skill != null && !_isAssigned);
        }

        private bool _isAssigned;

        public void SetAssigned(bool assigned)
        {
            _isAssigned = assigned;
            if (_assignedOverlay != null)
                _assignedOverlay.SetActive(assigned);
            if (_rerollButton != null)
                _rerollButton.gameObject.SetActive(!assigned && _skill != null);
        }

        private Color GetSkillColor(SkillData skill)
        {
            if (skill == null) return Color.gray;
            var palette = UIPalette.Default;

            return skill.Type switch
            {
                SkillType.Attack => palette.SkillAttack,
                SkillType.Heal => palette.SkillHeal,
                SkillType.Buff => palette.SkillBuff,
                SkillType.Debuff => palette.SkillDebuff,
                SkillType.Shield => palette.SkillShield,
                SkillType.Purify => palette.SkillPurify,
                _ => Color.white
            };
        }

        private static string BuildSkillSubtitle(SkillData skill)
        {
            var parts = new System.Collections.Generic.List<string>();

            string typeLabel = skill.Type switch
            {
                SkillType.Attack => "공격",
                SkillType.Heal => "치유",
                SkillType.Buff => "강화",
                SkillType.Debuff => "약화",
                SkillType.Shield => "보호막",
                SkillType.Purify => "정화",
                _ => ""
            };
            if (!string.IsNullOrEmpty(typeLabel)) parts.Add(typeLabel);

            parts.Add($"비용 {skill.Cost}");

            string targetLabel = skill.Target switch
            {
                TargetType.SingleEnemy => "단일 적",
                TargetType.AllEnemies => "전체 적",
                TargetType.SingleAlly => "단일 아군",
                TargetType.AllAllies => "전체 아군",
                TargetType.Self => "자신",
                _ => ""
            };
            if (!string.IsNullOrEmpty(targetLabel)) parts.Add(targetLabel);

            return string.Join(" | ", parts);
        }

        private void OnClick()
        {
            if (_skill != null && _parent != null)
            {
                _parent.SelectSlot(_slotIndex);
                AudioManager.Instance.PlayUIClick();
            }
        }

        private void OnRerollClick()
        {
            OnSlotRerollRequested?.Invoke(_slotIndex);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // TooltipTarget이 툴팁을 자동 처리
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // TooltipTarget이 툴팁을 자동 처리
        }
    }
}
