using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private TextMeshProUGUI _effectText;
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

        [Header("Skill Type Tag")]
        [SerializeField] private Image _typeTagImage;
        [SerializeField] private TextMeshProUGUI _typeTagText;

        private int _slotIndex;
        private SkillData _skill;
        private Character _caster;
        private ActionBarUI _parent;
        private Color _originalSkillColor;
        private Color _originalCostColor;
        private Color _originalBgColor;
        private bool _colorsStored;
        private Image _bgImage;
        private bool _isShuffling;
        private Coroutine _shuffleCoroutine;

        public SkillData Skill => _skill;
        public Character Caster => _caster;
        public bool IsShuffling => _isShuffling;

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

            // ★ Phase GF (2026-07-21): 활성 슬롯은 클릭 가능 (SetEmpty 반대 처리)
            if (_button != null) _button.interactable = true;

            SetSkillVisualsOnly(skill, caster);

            // 색상 캐싱 (Affordable 토글용)
            if (_skillIcon != null)
            {
                _originalSkillColor = _skillIcon.color;
                _colorsStored = true;
            }
            if (_costText != null)
                _originalCostColor = _costText.color;

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

        /// <summary>
        /// 시각적 요소만 갱신 (툴팁/색상 캐싱 없음) — 리롤 셔플 애니메이션용
        /// </summary>
        private void SetSkillVisualsOnly(SkillData skill, Character caster)
        {
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
            }

            // 스킬 타입별 배경 틴트
            if (_bgImage != null)
            {
                var skillColor = GetSkillColor(skill);
                _bgImage.color = new Color(skillColor.r * 0.25f, skillColor.g * 0.25f, skillColor.b * 0.25f, 0.9f);
            }

            // 스킬 타입 태그 업데이트
            UpdateTypeTag(skill);

            // 효과 설명 갱신 (BuildSkillDescription)
            if (_effectText != null)
            {
                string desc = skill != null ? BattleDisplayUtil.BuildSkillDescription(skill, caster) : "";
                _effectText.text = desc;
                _effectText.gameObject.SetActive(!string.IsNullOrEmpty(desc));
            }
        }

        /// <summary>스킬 타입에 따른 태그 색상/라벨 업데이트.</summary>
        private void UpdateTypeTag(SkillData skill)
        {
            if (_typeTagImage != null)
                _typeTagImage.color = GetSkillTypeColor(skill);
            if (_typeTagText != null)
                _typeTagText.text = GetSkillTypeLabel(skill);
        }

        private static Color GetSkillTypeColor(SkillData skill)
        {
            if (skill == null) return new Color(0.3f, 0.3f, 0.3f, 0.9f);
            var palette = UIPalette.Default;
            return skill.Type switch
            {
                SkillType.Attack => new Color(0.78f, 0.16f, 0.16f, 0.95f),
                SkillType.Heal => new Color(0.18f, 0.49f, 0.20f, 0.95f),
                SkillType.Shield => new Color(0.42f, 0.24f, 0.60f, 0.95f),
                SkillType.Buff => new Color(0.85f, 0.66f, 0.14f, 0.95f),
                SkillType.Debuff => new Color(0.37f, 0.21f, 0.69f, 0.95f),
                SkillType.Purify => new Color(0.18f, 0.55f, 0.62f, 0.95f),
                _ => new Color(0.3f, 0.3f, 0.3f, 0.9f)
            };
        }

        private static string GetSkillTypeLabel(SkillData skill)
        {
            if (skill == null) return "";
            return skill.Type switch
            {
                SkillType.Attack => "공격",
                SkillType.Heal => "치유",
                SkillType.Shield => "쉴드",
                SkillType.Buff => "강화",
                SkillType.Debuff => "약화",
                SkillType.Purify => "정화",
                _ => ""
            };
        }

        public void Clear()
        {
            _skill = null;
            _caster = null;

            // 툴팁 내용 비우기 — 이전 스킬 잔류 방지 (빈 슬롯에 툴팁 뜨는 것 차단)
            var tooltip = GetComponent<TooltipTarget>();
            if (tooltip != null) tooltip.SetContent("", "", "");

            if (_skillNameText != null)
                _skillNameText.text = "---";

            if (_costText != null)
                _costText.text = "";

            if (_casterNameText != null)
                _casterNameText.text = "";

            if (_effectText != null)
            {
                _effectText.text = "";
                _effectText.gameObject.SetActive(false);
            }

            if (_skillIcon != null)
            {
                _skillIcon.sprite = null;
                _skillIcon.color = Color.gray;
            }

            if (_bgImage != null)
                _bgImage.color = _originalBgColor;

            if (_typeTagImage != null)
                _typeTagImage.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
            if (_typeTagText != null)
                _typeTagText.text = "";

            SetSelected(false);

            // ★ Phase GF (2026-07-21): 빈 슬롯 클릭 차단
            if (_button != null) _button.interactable = false;
        }

        /// <summary>
        /// ★ Phase GF (2026-07-21): 빈 슬롯을 activeSelf=true로 유지하되 내용만 비움.
        /// LayoutGroup이 비활성 자식을 자리에서 제외하여 다른 슬롯이 커지는 것을 방지.
        /// 죽은 캐릭터의 슬롯 자리를 그대로 유지.
        /// </summary>
        public void SetEmpty()
        {
            Clear();
            // activeSelf=true 유지 (자리 차지). 내용은 Clear()로 비움.
            gameObject.SetActive(true);
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
            if (_isShuffling) return;
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
            if (_isShuffling) return;
            if (_skill != null && _parent != null)
            {
                _parent.SelectSlot(_slotIndex);
                AudioManager.Instance.PlayUIClick();
            }
        }

        private void OnRerollClick()
        {
            if (_isShuffling) return;
            OnSlotRerollRequested?.Invoke(_slotIndex);
        }

        // ── Reroll Shuffle Animation ──────────────────────────────

        /// <summary>
        /// 리롤 셔플 애니메이션 — 0.05초 간격으로 5회 랜덤 스킬 표시 후 최종 스킬로 안착
        /// </summary>
        public void PlayRerollShuffle(SkillData finalSkill, Character caster,
            IReadOnlyList<SkillData> shufflePool, System.Action onComplete = null)
        {
            if (_shuffleCoroutine != null)
                StopCoroutine(_shuffleCoroutine);
            _shuffleCoroutine = StartCoroutine(RerollShuffleRoutine(finalSkill, caster, shufflePool, onComplete));
        }

        private IEnumerator RerollShuffleRoutine(SkillData finalSkill, Character caster,
            IReadOnlyList<SkillData> shufflePool, System.Action onComplete)
        {
            _isShuffling = true;

            // 셔플 중 리롤 버튼 숨기기
            if (_rerollButton != null)
                _rerollButton.gameObject.SetActive(false);

            AudioManager.Instance.PlaySkillReroll();

            // 셔플 — 0.05초 간격으로 5회 랜덤 스킬 표시
            int shuffleCount = 5;
            for (int i = 0; i < shuffleCount; i++)
            {
                if (shufflePool != null && shufflePool.Count > 0)
                {
                    var randomSkill = shufflePool[Random.Range(0, shufflePool.Count)];
                    SetSkillVisualsOnly(randomSkill, caster);
                }
                yield return new WaitForSeconds(0.05f);
            }

            // 최종 안착 — SetSkill로 툴팁/색상 캐싱 등 모든 상태 갱신
            SetSkill(finalSkill, caster);

            _isShuffling = false;
            onComplete?.Invoke();
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
