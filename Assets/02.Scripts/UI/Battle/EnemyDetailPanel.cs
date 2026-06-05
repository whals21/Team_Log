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
    public class EnemyDetailPanel : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private Image _hpFillImage;
        [SerializeField] private Image _shieldFillImage;
        [SerializeField] private TextMeshProUGUI _infoText;
        [SerializeField] private TextMeshProUGUI _statText;
        [SerializeField] private Transform _statusEffectContainer;

        [Header("Action Buttons")]
        [SerializeField] private Button _guardianButton;
        [SerializeField] private Button _arcanaButton;

        [Header("Trait Area")]
        [SerializeField] private Transform _buttonArea;

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
        private CanvasGroup _canvasGroup;
        private Image _panelBgImage;
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
            if (_guardianButton == null) _guardianButton = FindComponent<Button>("ButtonArea/Btn_가디언");
            if (_arcanaButton == null) _arcanaButton = FindComponent<Button>("ButtonArea/Btn_아크카");
            if (_panelButton == null) _panelButton = GetComponent<Button>();

            // 자식 Graphic들의 raycastTarget을 꺼서 부모 Button이 클릭을 받도록 함
            // (Button이 있는 자식은 제외 - 가디언/아크카 버튼 등)
            foreach (var graphic in GetComponentsInChildren<Graphic>())
            {
                if (graphic.gameObject != gameObject && graphic.GetComponent<Button>() == null)
                    graphic.raycastTarget = false;
            }

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _panelBgImage = GetComponent<Image>();
        }

        private void Start()
        {
            if (_panelButton != null)
            {
                _panelButton.onClick.AddListener(() => OnPanelClicked?.Invoke(_enemyIndex));
            }
        }

        private T FindComponent<T>(string path) where T : Component
        {
            var t = transform.Find(path);
            return t != null ? t.GetComponent<T>() : null;
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

            var btn = labelRect.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;

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

            var capturedTrait = trait;
            bool showing = false;
            btn.onClick.AddListener(() =>
            {
                if (showing)
                {
                    SetInfoText(_intent?.GetDisplayText() ?? "");
                    showing = false;
                }
                else
                {
                    string label = BattleDisplayUtil.GetTraitLabel(capturedTrait);
                    string desc = BattleDisplayUtil.GetTraitDescription(capturedTrait);
                    SetInfoText($"[{label}] {desc}");
                    showing = true;
                }
            });
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
            SetInfoText(intent?.GetDisplayText() ?? "");
        }

        public void SetTargetMode(bool isTargetable)
        {
            if (_targetIndicator != null)
                _targetIndicator.SetActive(isTargetable);
        }

        public void SetDead(bool isDead)
        {
            if (_canvasGroup != null)
            {
                if (isDead)
                {
                    UIAnimationHelper.FadeToAlpha(_canvasGroup, 0.4f, 0.5f).OnComplete(() =>
                    {
                        _canvasGroup.interactable = false;
                        _canvasGroup.blocksRaycasts = false;
                    });
                }
                else
                {
                    _canvasGroup.alpha = 1f;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }
            }
        }

        public void FlashHit()
        {
            if (_panelBgImage != null)
                UIAnimationHelper.FlashColor(_panelBgImage, Color.white, 0.15f);
        }

        public void UpdateStats(int atk, int def)
        {
            if (_statText != null)
                _statText.text = $"ATK {atk}  DEF {def}";
        }

        public void UpdateStatusEffects(IEnumerable<ActiveEffect> effects)
        {
            if (_statusEffectContainer == null) return;

            for (int i = _statusEffectContainer.childCount - 1; i >= 0; i--)
                Destroy(_statusEffectContainer.GetChild(i).gameObject);

            if (effects == null) return;
            foreach (var effect in effects)
                StatusEffectBadge.Create(_statusEffectContainer, effect);
        }
    }
}
