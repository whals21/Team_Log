using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;
using TeamLog.Characters;
using TeamLog.UI;
using DG.Tweening;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 하단 PlayerStrip 가로 카드 패널 (이름, HP, 스탯, 상태이상)
    /// </summary>
    public class PlayerSidebarPanel : BattlePanelBase, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _avatarBgImage;
        [SerializeField] private TextMeshProUGUI _avatarLabel;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _hpPercentText;
        [SerializeField] private Image _hpFillImage;
        [SerializeField] private Image _shieldFillImage;
        [SerializeField] private GameObject _selectionHighlight;
        [SerializeField] private Button _panelButton;

        [Header("HP Colors")]
        [SerializeField] private Color _hpNormalColor = new Color(0.15f, 0.68f, 0.38f);
        [SerializeField] private Color _hpLowColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private float _lowThreshold = 0.3f;

        // 공용 흰색 스프라이트 — sprite 없는 Image는 raycast가 무시되는 버그 방지
        private static Sprite _whiteSprite;
        private static Sprite WhiteSprite => _whiteSprite ??= Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, 4f, 4f),
            new Vector2(0.5f, 0.5f),
            100f, 0u, SpriteMeshType.FullRect, Vector4.zero);

        private int _panelIndex;
        private Characters.Character _character;
        private BattleUIManager _uiManager;
        private Tween _hpTween;
        private Tween _hpPulseTween;
        private Tween _scaleTween;
        private bool _isDead;
        private LayoutElement _layoutElement;
        private TooltipTarget _panelTooltip;

        // Phase CC: 자원 배지 + 장착 특성 배지
        private ResourceBadge _resourceBadge;
        private TraitBadge _traitBadge;

        public int PanelIndex => _panelIndex;
        public event Action<int> OnPanelClicked;

        private void Awake()
        {
            // Auto-wire: Inspector에 할당되지 않은 필드를 자동으로 찾아 연결
            if (_nameText == null) _nameText = FindComponent<TextMeshProUGUI>("RightSection/NameRow/Name");
            if (_avatarBgImage == null) _avatarBgImage = FindComponent<Image>("Avatar");
            if (_avatarLabel == null) _avatarLabel = FindComponent<TextMeshProUGUI>("Avatar/Label");
            if (_hpText == null) _hpText = FindComponent<TextMeshProUGUI>("RightSection/HPBar/HPText");
            if (_hpFillImage == null) _hpFillImage = FindComponent<Image>("RightSection/HPBar/Fill");
            if (_shieldFillImage == null) _shieldFillImage = FindComponent<Image>("RightSection/HPBar/ShieldFill");
            if (_statText == null) _statText = FindComponent<TextMeshProUGUI>("RightSection/NameRow/Stats");
            if (_statusEffectContainer == null) _statusEffectContainer = transform.Find("RightSection/StatusContainer");
            if (_panelButton == null) _panelButton = GetComponent<Button>();
            if (_selectionHighlight == null) _selectionHighlight = transform.Find("SelectionHighlight")?.gameObject;

            _layoutElement = GetComponent<LayoutElement>();

            // 색상 토큰을 UIPalette에서 초기화
            var palette = UIPalette.Default;
            _hpNormalColor = palette.HPNormal;
            _hpLowColor = palette.HPLow;

            // 자식 Graphic들의 raycastTarget을 꺼서 부모 Button이 클릭을 받도록 함
            foreach (var graphic in GetComponentsInChildren<Graphic>())
            {
                if (graphic.gameObject != gameObject && graphic.GetComponent<Button>() == null)
                    graphic.raycastTarget = false;
            }

            InitPanelBase();

            // 패널 전체에 hover 툴팁 부착 — 부모 Image가 raycast 받도록 보장
            // (sprite가 null이면 일부 환경에서 raycast가 무시되므로 흰색 스프라이트 보장)
            if (_panelBgImage != null)
            {
                if (_panelBgImage.sprite == null)
                    _panelBgImage.sprite = WhiteSprite;
                _panelBgImage.raycastTarget = true;
            }
            _panelTooltip = GetComponent<TooltipTarget>();
            if (_panelTooltip == null)
                _panelTooltip = gameObject.AddComponent<TooltipTarget>();
        }

        private void Start()
        {
            if (_panelButton != null)
                _panelButton.onClick.AddListener(() => OnPanelClicked?.Invoke(_panelIndex));
        }

        private void ShowPopup()
        {
            var popup = _uiManager?.CharacterPopup;
            if (popup != null)
            {
                // 패널 중심의 화면 좌표 전달
                var rect = transform as RectTransform;
                Vector2 screenPos = rect != null
                    ? RectTransformUtility.WorldToScreenPoint(null, rect.position)
                    : Input.mousePosition;

                if (_character != null)
                    popup.Show(_character, null, screenPos);
                else
                    popup.ShowSample(_nameText?.text ?? "Unknown", _hpText?.text ?? "??");
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // 좌클릭/우클릭 모두 팝업 표시 (좌클릭 시 Button.onClick도 함께 발생하지만
            // SelectingAlly 모드가 아닐 때는 OnPanelClicked가 무시되므로 문제 없음)
            ShowPopup();
        }

        public void Setup(int index, string name, string skillName, Characters.Character character = null, BattleUIManager uiManager = null)
        {
            _panelIndex = index;
            _character = character;
            _uiManager = uiManager;

            if (_nameText != null)
                _nameText.text = name;

            // 초상화 이니셜 + 클래스 색상 배경
            if (character != null)
            {
                if (_avatarLabel != null)
                {
                    _avatarLabel.text = character.Data.Class switch
                    {
                        CharacterClass.Warrior => "전",
                        CharacterClass.Mage => "마",
                        CharacterClass.Healer => "힐",
                        CharacterClass.Rogue => "도",
                        CharacterClass.Archer => "궁",
                        CharacterClass.Necromancer => "강",
                        CharacterClass.Alchemist => "연",
                        CharacterClass.Bard => "음",
                        _ => "?"
                    };
                }

                if (_avatarBgImage != null)
                    _avatarBgImage.color = GetClassColor(character.Data.Class);
            }

            UpdateTooltipContent();
        }

        public void UpdateHP(int current, int max, int shield = 0)
        {
            float ratio = max > 0 ? (float)current / max : 0f;

            if (_hpText != null)
            {
                string shieldText = shield > 0 ? $" (+{shield})" : "";
                _hpText.text = $"{current}/{max}{shieldText}";
            }

            if (_hpPercentText != null)
                _hpPercentText.gameObject.SetActive(false);

            if (_hpFillImage != null)
            {
                if (_hpTween != null) _hpTween.Kill();
                _hpTween = UIAnimationHelper.TweenAnchorMaxX(_hpFillImage.rectTransform, ratio, 0.3f);
            }

            // HP 위기 펄스 애니메이션
            bool isLow = ratio <= _lowThreshold && ratio > 0f;
            if (isLow)
            {
                if (_hpFillImage != null)
                    _hpFillImage.color = _hpLowColor;

                if (_hpPulseTween == null && _canvasGroup != null)
                    _hpPulseTween = UIAnimationHelper.PulseAlpha(_canvasGroup, 0.5f, 1f, 0.8f);
            }
            else
            {
                // 펄스 정지 + 정상 복구
                if (_hpPulseTween != null)
                {
                    _hpPulseTween.Kill();
                    _hpPulseTween = null;
                }
                if (_canvasGroup != null && !_isDead)
                    _canvasGroup.alpha = 1f;

                if (_hpFillImage != null)
                    _hpFillImage.color = _hpNormalColor;
            }

            // 쉴드 바
            BattleDisplayUtil.UpdateShieldBar(_shieldFillImage, ratio, shield, max);

            UpdateTooltipContent();
        }

        public override void UpdateStats(int atk, int def)
        {
            base.UpdateStats(atk, def);

            // Phase CC: 자원 배지 (Ember/Vengeance/Frost/Prophecy)
            // 기존 텍스트 한 줄 표시를 전용 배지로 교체 — 자원별 고유 색상 + 변화 시 펀치 애니 + 툴팁.
            if (_character?.Resource != null && _statusEffectContainer != null)
            {
                if (_resourceBadge == null)
                    _resourceBadge = ResourceBadge.Create(_statusEffectContainer, _character.Resource);
                else
                    _resourceBadge.Refresh();
            }

            // Phase CC: 장착 특성 배지 (최초 1회 생성, 이후 갱신)
            if (_character?.PlayerTraitHandler != null && _character.PlayerTraitHandler.HasTrait)
            {
                if (_traitBadge == null && _nameText != null)
                    _traitBadge = TraitBadge.Create(_nameText.transform.parent, _character.PlayerTraitHandler.Trait);
                else if (_traitBadge != null)
                    _traitBadge.Refresh(_character.PlayerTraitHandler.Trait);
            }

            UpdateTooltipContent();
        }

        public override void UpdateStatusEffects(IEnumerable<ActiveEffect> effects)
        {
            base.UpdateStatusEffects(effects);
            UpdateTooltipContent();
        }

        /// <summary>
        /// 패널 hover 시 표시할 툴팁 콘텐츠 갱신 (스탯/HP/상태이상/보유 스킬)
        /// </summary>
        private void UpdateTooltipContent()
        {
            if (_panelTooltip == null || _character == null) return;

            string className = GetClassLabel(_character.Data.Class);
            int hp = _character.Health.CurrentHP;
            int hpMax = _character.Health.MaxHP;
            int shield = _character.Health.CurrentShield;
            int atk = _character.Stats.GetStat(StatType.ATK);
            int def = _character.Stats.GetStat(StatType.DEF);

            string subtitle = $"{className}  |  HP {hp}/{hpMax}{(shield > 0 ? $" (+{shield})" : "")}";

            var sb = new StringBuilder();
            sb.AppendLine($"<b>ATK</b> {atk}   <b>DEF</b> {def}");

            var effects = _character.StatusEffects.GetAllEffects();
            if (effects != null)
            {
                bool hasEffect = false;
                foreach (var e in effects)
                {
                    if (!hasEffect)
                    {
                        sb.AppendLine();
                        sb.AppendLine("<b>상태이상</b>");
                        hasEffect = true;
                    }
                    sb.AppendLine($"• {BattleDisplayUtil.GetEffectLabel(e.Type)} {e.Value} ({e.RemainingTurns}턴)");
                }
            }

            var skills = _character.SkillInventory.Skills;
            if (skills != null && skills.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("<b>보유 스킬</b>");
                foreach (var s in skills)
                {
                    string detail = BattleDisplayUtil.BuildSkillDescription(s, _character, " | ");
                    sb.AppendLine(string.IsNullOrEmpty(detail) ? $"• {s.SkillName}" : $"• {s.SkillName} — {detail}");
                }
            }

            _panelTooltip.SetContent(_character.Name, subtitle, sb.ToString());
        }

        public void SetSelected(bool selected)
        {
            if (_selectionHighlight != null)
                _selectionHighlight.SetActive(selected);

            // 선택 시 카드 확대
            if (_scaleTween != null) _scaleTween.Kill();
            _scaleTween = UIAnimationHelper.ScaleTo(transform, selected ? 1.05f : 1f, 0.2f);

            // 선택 시 그림자 강화
            var shadow = GetComponent<Shadow>();
            if (shadow != null)
                shadow.effectDistance = selected ? new Vector2(4, -4) : new Vector2(2, -2);
        }

        public override void SetDead(bool isDead)
        {
            _isDead = isDead;

            if (_layoutElement != null)
                _layoutElement.preferredHeight = isDead ? 40f : 64f;

            if (isDead)
            {
                // 펄스 정지
                if (_hpPulseTween != null)
                {
                    _hpPulseTween.Kill();
                    _hpPulseTween = null;
                }

                if (_canvasGroup != null)
                {
                    UIAnimationHelper.FadeToAlpha(_canvasGroup, 0.4f, 0.5f).OnComplete(() =>
                    {
                        _canvasGroup.interactable = false;
                        _canvasGroup.blocksRaycasts = false;
                    });
                }

                // 흑백 변환 — 아바타 색상 회색화
                if (_avatarBgImage != null)
                    _avatarBgImage.color = new Color(0.3f, 0.3f, 0.3f);
            }
            else
            {
                // 부활 시 복구
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 1f;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }

                if (_character != null && _avatarBgImage != null)
                    _avatarBgImage.color = GetClassColor(_character.Data.Class);
            }
        }

        private static Color GetClassColor(CharacterClass cls) => cls switch
        {
            CharacterClass.Warrior => new Color(0.75f, 0.20f, 0.20f),
            CharacterClass.Mage => new Color(0.25f, 0.40f, 0.85f),
            CharacterClass.Healer => new Color(0.20f, 0.75f, 0.40f),
            CharacterClass.Rogue => new Color(0.75f, 0.65f, 0.20f),
            CharacterClass.Archer => new Color(0.50f, 0.75f, 0.25f),
            CharacterClass.Necromancer => new Color(0.50f, 0.20f, 0.70f),
            CharacterClass.Alchemist => new Color(0.85f, 0.55f, 0.15f),
            CharacterClass.Bard => new Color(0.75f, 0.35f, 0.65f),
            _ => new Color(0.4f, 0.4f, 0.4f),
        };

        private static string GetClassLabel(CharacterClass cls) => cls switch
        {
            CharacterClass.Warrior => "전사",
            CharacterClass.Mage => "마법사",
            CharacterClass.Healer => "치유사",
            CharacterClass.Rogue => "도적",
            CharacterClass.Archer => "궁수",
            CharacterClass.Necromancer => "강령술사",
            CharacterClass.Alchemist => "연금술사",
            CharacterClass.Bard => "음유시인",
            _ => cls.ToString()
        };
    }
}
