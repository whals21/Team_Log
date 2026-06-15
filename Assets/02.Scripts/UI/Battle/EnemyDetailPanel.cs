using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TeamLog.Combat.AI;
using TeamLog.Characters;
using TeamLog.UI;
using DG.Tweening;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 중앙 적 유닛 상세 패널 (아바타, 이름, HP, 스탯, 상태이상, 버튼)
    /// </summary>
    public class EnemyDetailPanel : BattlePanelBase, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private Image _hpFillImage;
        [SerializeField] private Image _shieldFillImage;
        [SerializeField] private TextMeshProUGUI _infoText;

        [Header("Trait Area")]
        [SerializeField] private Transform _buttonArea;

        [Header("Intent")]
        [SerializeField] private GameObject _intentSlot;
        [SerializeField] private Image _intentIcon;
        [SerializeField] private TextMeshProUGUI _intentValueText;
        [SerializeField] private TextMeshProUGUI _intentText;

        [Header("Selection")]
        [SerializeField] private GameObject _targetIndicator;

        [Header("Click")]
        [SerializeField] private Button _panelButton;

        [Header("HP Color")]
        [SerializeField] private Color _hpColor = new Color(0.77f, 0.12f, 0.23f);

        private int _enemyIndex;
        private Characters.Character _character;
        private EnemyIntent _intent;
        private BattleUIManager _uiManager;
        private Tween _hpTween;

        public int EnemyIndex => _enemyIndex;
        public event Action<int> OnPanelClicked;

        private void Awake()
        {
            // Auto-wire: Inspector에 할당되지 않은 필드를 자동으로 찾아 연결
            if (_avatarImage == null) _avatarImage = FindComponent<Image>("Avatar");
            if (_nameText == null) _nameText = FindComponent<TextMeshProUGUI>("Name");
            if (_hpText == null) _hpText = FindComponent<TextMeshProUGUI>("HPBarContainer/HPText");
            if (_hpFillImage == null) _hpFillImage = FindComponent<Image>("HPBarContainer/Fill");
            if (_shieldFillImage == null) _shieldFillImage = FindComponent<Image>("HPBarContainer/ShieldFill");
            if (_infoText == null) _infoText = FindComponent<TextMeshProUGUI>("Info");
            if (_statText == null) _statText = FindComponent<TextMeshProUGUI>("Stats");
            if (_statusEffectContainer == null) _statusEffectContainer = transform.Find("StatusContainer");
            if (_buttonArea == null) _buttonArea = transform.Find("ButtonArea");
            if (_intentSlot == null) _intentSlot = transform.Find("IntentSlot")?.gameObject;
            if (_intentIcon == null) _intentIcon = FindComponent<Image>("IntentSlot/IntentIcon");
            if (_intentValueText == null) _intentValueText = FindComponent<TextMeshProUGUI>("IntentSlot/IntentValue");
            if (_intentText == null) _intentText = FindComponent<TextMeshProUGUI>("IntentSlot/IntentText");
            if (_panelButton == null) _panelButton = GetComponent<Button>();

            // 색상 토큰을 UIPalette에서 초기화
            _hpColor = UIPalette.Default.HPEnemy;
            if (_targetIndicator == null) _targetIndicator = transform.Find("TargetIndicator")?.gameObject;

            // 자식 Graphic들의 raycastTarget을 꺼서 부모 Button이 클릭을 받도록 함
            // 단, IntentSlot 하위는 툴팁 hover 이벤트를 받아야 하므로 예외
            foreach (var graphic in GetComponentsInChildren<Graphic>())
            {
                if (graphic.gameObject != gameObject
                    && graphic.GetComponent<Button>() == null
                    && graphic.transform.parent?.name != "IntentSlot"
                    && graphic.gameObject.name != "IntentSlot")
                    graphic.raycastTarget = false;
            }

            InitPanelBase();
        }

        private void Start()
        {
            if (_panelButton != null)
            {
                _panelButton.onClick.AddListener(() => OnPanelClicked?.Invoke(_enemyIndex));
            }
        }

        private void ShowPopup()
        {
            var popup = _uiManager?.CharacterPopup;
            if (popup != null)
            {
                if (_character != null)
                    popup.Show(_character, _intent);
                else
                    popup.ShowSample(_nameText?.text ?? "Enemy", _hpText?.text ?? "??");
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
                ShowPopup();
        }

        public void Setup(int index, string enemyName, Sprite avatar = null, Characters.Character character = null, BattleUIManager uiManager = null)
        {
            _enemyIndex = index;
            _character = character;
            _uiManager = uiManager;

            if (_nameText != null)
                _nameText.text = enemyName;

            if (_avatarImage != null && avatar != null)
                _avatarImage.sprite = avatar;

            // 특성 표시: ButtonArea를 특성 전용으로 설정
            SetupTraitArea(character);
        }

        private void SetupTraitArea(Characters.Character character)
        {
            if (_buttonArea == null) return;

            // 기존 자식(가디언/아크카 버튼) 제거
            for (int i = _buttonArea.childCount - 1; i >= 0; i--)
                Destroy(_buttonArea.GetChild(i).gameObject);

            var trait = character?.Data.Trait ?? EnemyTrait.None;
            if (trait == EnemyTrait.None) return;

            // 특성 라벨
            var labelRect = new GameObject("TraitLabel").AddComponent<RectTransform>();
            labelRect.SetParent(_buttonArea, false);
            labelRect.sizeDelta = new Vector2(160, 32);

            var bg = labelRect.gameObject.AddComponent<Image>();
            bg.color = BattleDisplayUtil.GetTraitColor(trait);
            bg.raycastTarget = true;

            var labelObj = new GameObject("T").AddComponent<RectTransform>();
            labelObj.SetParent(labelRect, false);
            labelObj.anchorMin = Vector2.zero;
            labelObj.anchorMax = Vector2.one;
            labelObj.offsetMin = Vector2.zero;
            labelObj.offsetMax = Vector2.zero;

            var tmp = labelObj.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 14;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            tmp.text = BattleDisplayUtil.GetTraitLabel(trait);
            UIKoreanFont.EnsureFont(tmp);

            // 툴팁: 특성 이름 / 설명 (TooltipUI 통합 — 일관성)
            string traitLabel = BattleDisplayUtil.GetTraitLabel(trait);
            string traitDesc = BattleDisplayUtil.GetTraitDescription(trait);
            var tooltip = labelRect.gameObject.AddComponent<TooltipTarget>();
            tooltip.SetContent($"[{traitLabel}]", traitDesc);
        }

        public void UpdateHP(int current, int max, int shield = 0)
        {
            float ratio = max > 0 ? (float)current / max : 0f;

            if (_hpText != null)
            {
                string shieldText = shield > 0 ? $" (+{shield})" : "";
                _hpText.text = $"{current}/{max}{shieldText}";
            }

            if (_hpFillImage != null)
            {
                if (_hpTween != null) _hpTween.Kill();
                _hpTween = UIAnimationHelper.TweenAnchorMaxX(_hpFillImage.rectTransform, ratio, 0.3f);
                _hpFillImage.color = _hpColor;
            }

            // 쉴드 바: HP 바 끝점부터 겹쳐서 표시
            BattleDisplayUtil.UpdateShieldBar(_shieldFillImage, ratio, shield, max);
        }

        public void SetInfoText(string text)
        {
            if (_infoText != null)
                _infoText.text = text;
        }

        public void SetIntent(EnemyIntent intent)
        {
            _intent = intent;

            // 전용 Intent 슬롯 업데이트
            bool hasIntent = intent != null && intent.Type != EnemyIntentType.None;
            if (_intentSlot != null)
                _intentSlot.SetActive(hasIntent);

            if (_intentIcon != null)
            {
                var palette = UIPalette.Default;
                _intentIcon.color = intent?.Type switch
                {
                    EnemyIntentType.Attack => palette.AccentRed,
                    EnemyIntentType.Shield => palette.ShieldBrown,
                    EnemyIntentType.Heal => palette.AccentGreen,
                    EnemyIntentType.Buff => palette.AccentYellow,
                    EnemyIntentType.Debuff => palette.SkillDebuff,
                    _ => palette.TextDim
                };
            }

            // 큰 숫자 (위력/수치)
            if (_intentValueText != null)
            {
                _intentValueText.text = hasIntent && intent.Value > 0 ? intent.Value.ToString() : "";
                _intentValueText.color = intent?.Type switch
                {
                    EnemyIntentType.Attack => UIPalette.Default.AccentRed,
                    EnemyIntentType.Heal => UIPalette.Default.AccentGreen,
                    EnemyIntentType.Shield => UIPalette.Default.ShieldBrown,
                    _ => Color.white
                };
            }

            // 스킬명 텍스트
            if (_intentText != null)
            {
                _intentText.text = hasIntent && intent.Skill != null ? intent.Skill.SkillName : "";
            }

            // IntentSlot에 툴팁 설정
            if (hasIntent && intent.Skill != null && _intentSlot != null)
            {
                var tooltip = _intentSlot.GetComponent<TooltipTarget>();
                if (tooltip == null) tooltip = _intentSlot.AddComponent<TooltipTarget>();
                tooltip.SetContent(
                    intent.Skill.SkillName,
                    BuildIntentSubtitle(intent.Skill),
                    BuildIntentTooltipDesc(intent));
            }
            else if (_intentSlot != null)
            {
                var tooltip = _intentSlot.GetComponent<TooltipTarget>();
                if (tooltip != null) tooltip.SetContent("", "", "");
            }

            // Info 텍스트에도 표시 (특성 클릭 시 교체됨)
            if (hasIntent)
                SetInfoText(BuildIntentDisplay(intent));
        }

        /// <summary>
        /// "스킬명(수치) → 대상" 형식으로 Intent 표시 문자열 생성
        /// </summary>
        private static string BuildIntentDisplay(EnemyIntent intent)
        {
            if (intent.Skill == null) return "";

            string name = intent.Skill.SkillName;
            string valuePart = intent.Value > 0 ? $"({intent.Value})" : "";
            string target = !string.IsNullOrEmpty(intent.TargetDisplay) ? $" {intent.TargetDisplay}" : "";
            return $"{name}{valuePart}{target}";
        }

        private static string BuildIntentSubtitle(SkillData skill)
        {
            var parts = new List<string>();

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

        private static string BuildIntentTooltipDesc(EnemyIntent intent)
        {
            if (intent.Skill == null) return "";

            var desc = BattleDisplayUtil.BuildSkillDescription(intent.Skill, intent.Skill.Type == SkillType.Attack ? null : null);
            string skillDesc = string.IsNullOrEmpty(intent.Skill.Description) ? desc : intent.Skill.Description;
            if (!string.IsNullOrEmpty(desc) && !string.IsNullOrEmpty(intent.Skill.Description) && intent.Skill.Description != desc)
                skillDesc = intent.Skill.Description + "\n" + desc;

            // 위력 정보 추가
            if (intent.Value > 0)
                skillDesc = $"위력 {intent.Value}\n" + skillDesc;

            return skillDesc;
        }

        public void SetTargetMode(bool isTargetable)
        {
            if (_targetIndicator != null)
                _targetIndicator.SetActive(isTargetable);
        }

    }
}
