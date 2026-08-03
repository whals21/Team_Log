using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using TeamLog.Skill;
using TeamLog.UI;
using TeamLog.Characters;

using SkillData = TeamLog.Characters.SkillData;
using SkillType = TeamLog.Characters.SkillType;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 발견(Discover) 모달 카드 하나 — SkillData 표시 + 클릭 콜백.
    /// RewardCard 패턴 차용. 스킬 타입별 배경색으로 직관적 구분.
    /// 단축키 1/2/3/4 표시를 위해 번호 라벨 포함.
    /// </summary>
    public class DiscoverCard : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _shortcutLabel; // 1/2/3/4 단축키 번호
        [SerializeField] private TextMeshProUGUI _titleLabel;    // 스킬명
        [SerializeField] private TextMeshProUGUI _typeLabel;     // 타입 라벨 (공격/회복/버프 등)
        [SerializeField] private TextMeshProUGUI _descLabel;     // 효과 요약
        [SerializeField] private Button _button;

        private SkillData _skill;
        private Action<SkillData> _onSelected;
        private int _shortcutNumber;

        /// <summary>이 카드의 단축키 번호 (1부터 시작). 0 = 단축키 없음.</summary>
        public int ShortcutNumber => _shortcutNumber;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        /// <summary>
        /// 발견 카드 설정.
        /// </summary>
        /// <param name="skill">발견된 스킬</param>
        /// <param name="shortcutNumber">단축키 번호 (1~4). 0이면 표시 안 함.</param>
        /// <param name="onSelected">클릭 콜백</param>
        /// <param name="caster">시전자 (설명에 ATK 포함시 사용). null 가능.</param>
        public void Setup(SkillData skill, int shortcutNumber, Action<SkillData> onSelected, Character caster = null)
        {
            _skill = skill;
            _shortcutNumber = shortcutNumber;
            _onSelected = onSelected;

            // 아이콘
            if (_iconImage != null)
            {
                _iconImage.sprite = skill?.Icon;
                _iconImage.enabled = skill?.Icon != null;
            }

            // 단축키 번호
            if (_shortcutLabel != null)
            {
                _shortcutLabel.text = shortcutNumber > 0 ? shortcutNumber.ToString() : "";
                _shortcutLabel.gameObject.SetActive(shortcutNumber > 0);
            }

            // 스킬명
            if (_titleLabel != null)
                _titleLabel.text = skill != null ? skill.SkillName : "---";

            // 타입 라벨
            if (_typeLabel != null)
            {
                _typeLabel.text = GetTypeLabel(skill);
                _typeLabel.color = GetTypeColor(skill);
            }

            // 효과 요약 — ★ 2026-08-03 P0-R3: 자연어 풀어쓰기
            if (_descLabel != null && skill != null)
                _descLabel.text = BattleDisplayUtil.BuildTooltipDescription(skill, caster);

            // 배경색 — 스킬 타입별
            if (_backgroundImage != null && skill != null)
                _backgroundImage.color = GetTypeBgColor(skill);
        }

        /// <summary>외부에서 클릭 트리거 (단축키 입력 시).</summary>
        public void TriggerClick()
        {
            OnClicked();
        }

        private void OnClicked()
        {
            if (_skill == null) return;
            _onSelected?.Invoke(_skill);
        }

        /// <summary>스킬 타입 → 한국어 라벨.</summary>
        public static string GetTypeLabel(SkillData skill)
        {
            if (skill == null) return "";
            return skill.Type switch
            {
                SkillType.Attack => "공격",
                SkillType.Heal => "회복",
                SkillType.Buff => "버프",
                SkillType.Debuff => "디버프",
                SkillType.Shield => "쉴드",
                SkillType.Purify => "정화",
                _ => "기타"
            };
        }

        /// <summary>스킬 타입 → 강조 색상 (라벨).</summary>
        public static Color GetTypeColor(SkillData skill)
        {
            if (skill == null) return Color.white;
            return skill.Type switch
            {
                SkillType.Attack => new Color(0.95f, 0.35f, 0.30f),
                SkillType.Heal => new Color(0.30f, 0.85f, 0.45f),
                SkillType.Buff => new Color(0.95f, 0.82f, 0.25f),
                SkillType.Debuff => new Color(0.75f, 0.35f, 0.85f),
                SkillType.Shield => new Color(0.40f, 0.65f, 0.95f),
                SkillType.Purify => new Color(0.45f, 0.92f, 0.92f),
                _ => Color.white
            };
        }

        /// <summary>스킬 타입 → 배경색 (어두운 톤).</summary>
        public static Color GetTypeBgColor(SkillData skill)
        {
            if (skill == null) return new Color(0.12f, 0.12f, 0.16f);
            return skill.Type switch
            {
                SkillType.Attack => new Color(0.25f, 0.08f, 0.10f, 0.95f),
                SkillType.Heal => new Color(0.08f, 0.22f, 0.12f, 0.95f),
                SkillType.Buff => new Color(0.22f, 0.18f, 0.06f, 0.95f),
                SkillType.Debuff => new Color(0.20f, 0.08f, 0.22f, 0.95f),
                SkillType.Shield => new Color(0.08f, 0.16f, 0.25f, 0.95f),
                SkillType.Purify => new Color(0.08f, 0.20f, 0.22f, 0.95f),
                _ => new Color(0.12f, 0.12f, 0.16f, 0.95f)
            };
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClicked);
        }
    }
}
