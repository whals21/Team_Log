using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Skill;
using TeamLog.UI;

namespace TeamLog.UI.PartySelection
{
    /// <summary>
    /// 스킬 상세 카드 (UI-B.2) — 웹 목업의 2×2 스킬 카드 유니티 재현.
    /// 좌측 타입별 컬러 띠 + 아이콘 + 이름 + AP + 타입/타겟/위력 배지 + 설명 + 조건부 보너스 박스.
    /// Initialize(SkillData, Color)로 데이터 주입.
    ///
    /// 레이아웃 (프리팹 기준 — SceneBuilder가 자동 생성 또는 인스펙터 수동 연결):
    /// SkillCard (Image 배경, 9-slice SlatePanel)
    /// ├── TypeColorBar  (Image — 좌측 3px 세로 띠, 타입 색상)
    /// ├── Head          (HorizontalLayoutGroup)
    /// │   ├── Icon      (32x32 Image — 자원색 원형)
    /// │   └── Title     (VerticalLayoutGroup)
    /// │       ├── Name  (TMP — Cinzel Bold)
    /// │       └── Cost  (TMP — 작은 AP 배지)
    /// ├── Badges        (HorizontalLayoutGroup + LayoutElement)
    /// │   ├── TypeBadge   (TMP — "[공격]")
    /// │   ├── TargetBadge (TMP — "[단일 적]")
    /// │   └── PowerBadge  (TMP — "[5 위력]")
    /// ├── DescText      (TMP — 본문, 한국어)
    /// └── BonusBox      (Image + TMP — ⚡ 골드 / ⚠ 핏빛)
    /// </summary>
    public class SkillDetailCard : MonoBehaviour
    {
        [Header("Background")]
        [SerializeField] private Image _background;
        [SerializeField] private Sprite _panelSprite;       // SlatePanel_9Slice (기본)
        [SerializeField] private Sprite _panelHoverSprite;  // SlatePanelLight_9Slice

        [Header("Type Bar (좌측 컬러 띠)")]
        [SerializeField] private Image _typeColorBar;

        [Header("Head")]
        [SerializeField] private Image _skillIcon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _costText;

        [Header("Badges")]
        [SerializeField] private TextMeshProUGUI _typeBadge;
        [SerializeField] private TextMeshProUGUI _targetBadge;
        [SerializeField] private TextMeshProUGUI _powerBadge;

        [Header("Body")]
        [SerializeField] private TextMeshProUGUI _descText;

        [Header("Bonus Box")]
        [SerializeField] private GameObject _bonusBox;
        [SerializeField] private Image _bonusBackground;
        [SerializeField] private TextMeshProUGUI _bonusText;

        [Header("Icon Sprites (선택 — 없으면 자원색 원형)")]
        [SerializeField] private Sprite _defaultSkillIconSprite;

        // 상태
        private SkillData _skill;
        private Color _resourceColor;

        /// <summary>
        /// 스킬 카드 초기화.
        /// </summary>
        public void Initialize(SkillData skill, Color resourceColor)
        {
            _skill = skill;
            _resourceColor = resourceColor;
            Render();
        }

        private void Render()
        {
            var palette = UIPalette.Default;
            bool hasResourceColor = _resourceColor != Color.clear && _resourceColor.a > 0.01f;
            Color accentColor = hasResourceColor ? _resourceColor : palette.DFGoldL;

            // 배경
            if (_background != null)
            {
                _background.sprite = _panelSprite ?? palette != null ? _panelSprite : null;
                _background.color = new Color(1, 1, 1, 1);
                if (_background.sprite == null)
                {
                    // Sprite 없으면 색상만
                    _background.sprite = null;
                    _background.color = palette.DFSlate;
                }
            }

            // 좌측 컬러 띠 (타입 색상)
            if (_typeColorBar != null)
            {
                Color typeColor = ResolveSkillTypeColor(_skill);
                _typeColorBar.color = typeColor;
            }

            // 아이콘
            if (_skillIcon != null)
            {
                if (_skill.Icon != null)
                {
                    _skillIcon.sprite = _skill.Icon;
                    _skillIcon.color = Color.white;
                }
                else if (_defaultSkillIconSprite != null)
                {
                    _skillIcon.sprite = _defaultSkillIconSprite;
                    _skillIcon.color = accentColor;
                }
                else
                {
                    // Sprite 없으면 자원색 원형 (WhiteSprite로 표현)
                    _skillIcon.color = accentColor;
                }
            }

            // 이름 (Cinzel Bold 스타일 — 폰트는 SceneBuilder에서 연결)
            if (_nameText != null)
            {
                _nameText.text = _skill.SkillName ?? "(unnamed)";
                _nameText.color = palette.DFGoldL;
            }

            // AP 비용
            if (_costText != null)
            {
                _costText.text = $"<b>AP</b> {_skill.Cost}";
                _costText.color = palette.DFGoldL;
            }

            // 배지: 타입
            if (_typeBadge != null)
            {
                string label = PartySelectionUIUtils.GetSkillTypeLabel(_skill.Type);
                _typeBadge.text = label;
                _typeBadge.color = PartySelectionUIUtils.GetSkillTypeColor(_skill.Type);
            }

            // 배지: 타겟
            if (_targetBadge != null)
            {
                _targetBadge.text = PartySelectionUIUtils.GetTargetLabel(_skill.Target);
                _targetBadge.color = PartySelectionUIUtils.GetTargetColor(_skill.Target);
            }

            // 배지: 위력
            if (_powerBadge != null)
            {
                bool showPower = _skill.Power > 0;
                _powerBadge.gameObject.SetActive(showPower);
                if (showPower)
                {
                    _powerBadge.text = $"<b>{_skill.Power}</b> 위력";
                    _powerBadge.color = palette.DFGoldL;
                }
            }

            // 설명 (자원/행동 키워드 종합)
            if (_descText != null)
            {
                _descText.text = PartySelectionUIUtils.BuildSkillDescription(_skill);
                _descText.color = palette.DFInk;
            }

            // 조건부 보너스 박스
            if (_bonusBox != null)
            {
                var (text, isRestriction) = PartySelectionUIUtils.BuildSkillBonusText(_skill);
                bool hasBonus = !string.IsNullOrEmpty(text);
                _bonusBox.SetActive(hasBonus);
                if (hasBonus)
                {
                    string prefix = isRestriction ? "! " : ">> ";
                    _bonusText.text = prefix + text;
                    _bonusText.color = isRestriction ? palette.DFBloodL : palette.DFParchment;

                    if (_bonusBackground != null)
                    {
                        Color bg = isRestriction
                            ? new Color(0.75f, 0.22f, 0.17f, 0.15f)
                            : new Color(0.83f, 0.69f, 0.22f, 0.15f);
                        _bonusBackground.color = bg;
                    }
                }
            }
        }

        /// <summary>
        /// 스킬 타입 색상 (Discover/Summon 특수 케이스 포함).
        /// </summary>
        private Color ResolveSkillTypeColor(SkillData skill)
        {
            // Discover 스킬 (Cael)
            if (typeof(SkillData).GetField("_isDiscover") != null)
            {
                var flag = typeof(SkillData).GetField("_isDiscover",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (flag != null && (bool)flag.GetValue(skill))
                    return UIPalette.Default.SkillSpecial;
            }
            return PartySelectionUIUtils.GetSkillTypeColor(skill.Type);
        }
    }
}
