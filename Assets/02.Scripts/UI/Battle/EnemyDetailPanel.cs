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
        // 공용 흰색 스프라이트 — sprite 없는 Image는 raycast가 무시되는 버그 방지
        private static Sprite _whiteSprite;
        private static Sprite WhiteSprite
        {
            get
            {
                if (_whiteSprite == null)
                {
                    _whiteSprite = Sprite.Create(
                        Texture2D.whiteTexture,
                        new Rect(0f, 0f, 4f, 4f),
                        new Vector2(0.5f, 0.5f),
                        100f, 0u, SpriteMeshType.FullRect, Vector4.zero);
                }
                return _whiteSprite;
            }
        }

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

        [Header("Target Box (의도 타겟팅 정보)")]
        [SerializeField] private GameObject _targetBox;
        [SerializeField] private Image _targetPortrait;
        [SerializeField] private TextMeshProUGUI _targetNameText;

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
        private Tween _actingTween;
        private Color _defaultBgColor = Color.white;
        private bool _defaultBgCaptured;

        public int EnemyIndex => _enemyIndex;
        public Characters.Character Target => _character;
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
            if (_targetBox == null) _targetBox = transform.Find("TargetBox")?.gameObject;
            if (_targetPortrait == null) _targetPortrait = FindComponent<Image>("TargetBox/Portrait");
            if (_targetNameText == null) _targetNameText = FindComponent<TextMeshProUGUI>("TargetBox/Name");
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

            // 하이라이트 복원용 기본 배경색 캡처
            if (_panelBgImage != null)
            {
                _defaultBgColor = _panelBgImage.color;
                _defaultBgCaptured = true;
            }
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
                // 패널 중심의 화면 좌표 전달 → 팝업이 근처에 표시
                var rect = transform as RectTransform;
                Vector2 screenPos = rect != null
                    ? RectTransformUtility.WorldToScreenPoint(null, rect.position)
                    : Input.mousePosition;

                if (_character != null)
                    popup.Show(_character, _intent, screenPos);
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
            // ButtonArea가 없으면 자동 생성 (구버전 프리팹 호환)
            if (_buttonArea == null)
            {
                var btnObj = new GameObject("ButtonArea");
                btnObj.transform.SetParent(transform, false);
                var btnRect = btnObj.AddComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(0, 24);
                var hlg = btnObj.AddComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
                _buttonArea = btnObj.transform;
            }

            // 기존 자식(가디언/아크카 버튼) 제거
            for (int i = _buttonArea.childCount - 1; i >= 0; i--)
                Destroy(_buttonArea.GetChild(i).gameObject);

            var trait = character?.Data?.Trait ?? EnemyTrait.None;
            if (trait == EnemyTrait.None)
            {
                Debug.LogWarning($"[EnemyDetailPanel] Trait is None for {character?.Name ?? "null"} — trait label skipped");
                return;
            }

            // 특성 라벨
            var labelRect = new GameObject("TraitLabel").AddComponent<RectTransform>();
            labelRect.SetParent(_buttonArea, false);
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = new Vector2(160, 32);
            labelRect.SetAsLastSibling();

            // LayoutElement로 명시적 크기 보장 (HorizontalLayoutGroup에 의한 왜곡 방지)
            var le = labelRect.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 160;
            le.preferredHeight = 32;
            le.minWidth = 160;
            le.minHeight = 32;

            var bg = labelRect.gameObject.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = BattleDisplayUtil.GetTraitColor(trait);
            bg.raycastTarget = true; // hover 이벤트 수신 위해 필수

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

            // 타겟 박스 업데이트 — 공격 의도일 때만 타겟 표시
            UpdateTargetBox(intent);

            // Info 텍스트에도 표시 (특성 클릭 시 교체됨)
            if (hasIntent)
                SetInfoText(BuildIntentDisplay(intent));
        }

        /// <summary>적 의도의 타겟 정보를 TargetBox에 표시. 공격 의도일 때만 활성화.</summary>
        private void UpdateTargetBox(EnemyIntent intent)
        {
            if (_targetBox == null) return;

            // ★ D 시안: Attack/Debuff 의도일 때 항상 TargetBox 표시.
            // intent.Targets가 비어 있으면 첫 번째 살아있는 플레이어를 폴백으로 표시.
            bool hasTarget = intent != null
                && (intent.Type == EnemyIntentType.Attack || intent.Type == EnemyIntentType.Debuff);

            _targetBox.SetActive(hasTarget);

            if (!hasTarget) return;

            // 대상 결정 — intent.Targets 우선, 비어 있으면 _uiManager에서 폴백
            Characters.Character target = null;
            if (intent.Targets != null && intent.Targets.Count > 0)
            {
                target = intent.Targets[0];
            }
            else if (_uiManager != null)
            {
                target = _uiManager.GetFirstAlivePlayer();
            }

            if (_targetNameText != null)
                _targetNameText.text = target?.Data?.CharacterName ?? "?";

            // 타겟팅된 플레이어 패널에 붉은 테두리 표시
            if (_uiManager != null && target != null)
                _uiManager.SetPlayerTargetedByEnemy(target, true);
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

        // ── 순차 적 턴 — 행동 중인 적 하이라이트 ──
        // DOTween.To() 직접 사용 (asmdef 경계 규칙 — CLAUDE.md 참조)

        private static readonly Color ActingBgColor = new Color(1f, 0.92f, 0.65f);

        public void HighlightActing()
        {
            _actingTween?.Kill();
            var t = transform;
            float elapsed = 0f;
            const float dur = 0.18f;
            _actingTween = DOTween.To(
                () => elapsed,
                x =>
                {
                    elapsed = x;
                    // 펀치 곡선: 0→1→0
                    float curve = Mathf.Sin(Mathf.PI * Mathf.Clamp01(elapsed / dur));
                    t.localScale = Vector3.one * (1f + 0.08f * curve);
                },
                dur, dur);
            if (_panelBgImage != null)
                _panelBgImage.color = ActingBgColor;
        }

        public void ClearActingHighlight()
        {
            _actingTween?.Kill();
            transform.localScale = Vector3.one;
            if (_panelBgImage != null && _defaultBgCaptured)
                _panelBgImage.color = _defaultBgColor;
        }

    }
}
