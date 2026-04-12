using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 좌측 사이드바 캐릭터 패널 (번호, 이름, HP, 스킬명)
    /// </summary>
    public class PlayerSidebarPanel : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _numberText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _hpPercentText;
        [SerializeField] private TextMeshProUGUI _skillNameText;
        [SerializeField] private Image _hpFillImage;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _selectionHighlight;
        [SerializeField] private Button _panelButton;

        [Header("HP Colors")]
        [SerializeField] private Color _hpNormalColor = new Color(0.15f, 0.68f, 0.38f);
        [SerializeField] private Color _hpLowColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private float _lowThreshold = 0.3f;

        private int _panelIndex;
        private Characters.Character _character;
        private BattleUIManager _uiManager;

        public int PanelIndex => _panelIndex;
        public event Action<int> OnPanelClicked;

        private void Awake()
        {
            // Auto-wire: Inspector에 할당되지 않은 필드를 자동으로 찾아 연결
            if (_numberText == null) _numberText = FindComponent<TextMeshProUGUI>("NumberBadge/T");
            if (_nameText == null) _nameText = FindComponent<TextMeshProUGUI>("Name");
            if (_hpText == null) _hpText = FindComponent<TextMeshProUGUI>("HPBar/Text");
            if (_hpPercentText == null) _hpPercentText = FindComponent<TextMeshProUGUI>("Pct");
            if (_skillNameText == null) _skillNameText = FindComponent<TextMeshProUGUI>("Skill");
            if (_hpFillImage == null) _hpFillImage = FindComponent<Image>("HPBar/Fill");
            if (_closeButton == null) _closeButton = FindComponent<Button>("CloseBtn");
            if (_panelButton == null) _panelButton = GetComponent<Button>();

            // 자식 Graphic들의 raycastTarget을 꺼서 부모 Button이 클릭을 받도록 함
            // (Button이 있는 자식은 제외 - CloseBtn 등)
            foreach (var graphic in GetComponentsInChildren<Graphic>())
            {
                if (graphic.gameObject != gameObject && graphic.GetComponent<Button>() == null)
                    graphic.raycastTarget = false;
            }

        }

        private void Start()
        {
            if (_panelButton != null)
            {
                _panelButton.onClick.AddListener(() => OnPanelClicked?.Invoke(_panelIndex));
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
                    popup.Show(_character);
                else
                    popup.ShowSample(_nameText?.text ?? "Unknown", _hpText?.text ?? "??");
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
                ShowPopup();
        }

        public void Setup(int index, string name, string skillName, Characters.Character character = null, BattleUIManager uiManager = null)
        {
            _panelIndex = index;
            _character = character;
            _uiManager = uiManager;

            if (_numberText != null)
                _numberText.text = (index + 1).ToString();

            if (_nameText != null)
                _nameText.text = name;

            if (_skillNameText != null)
                _skillNameText.text = skillName;
        }

        public void UpdateHP(int current, int max)
        {
            float ratio = max > 0 ? (float)current / max : 0f;

            if (_hpText != null)
                _hpText.text = $"{current}/{max}";

            if (_hpPercentText != null)
                _hpPercentText.text = $"{Mathf.RoundToInt(ratio * 100)}%";

            if (_hpFillImage != null)
            {
                _hpFillImage.rectTransform.anchorMax = new Vector2(ratio, 1f);
                _hpFillImage.color = ratio <= _lowThreshold ? _hpLowColor : _hpNormalColor;
            }

            if (_hpPercentText != null)
                _hpPercentText.color = ratio <= _lowThreshold ? _hpLowColor : _hpNormalColor;
        }

        public void SetSelected(bool selected)
        {
            if (_selectionHighlight != null)
                _selectionHighlight.SetActive(selected);
        }
    }
}
