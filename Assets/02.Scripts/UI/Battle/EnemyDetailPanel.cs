using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 중앙 적 유닛 상세 패널 (아바타, 이름, HP, 버튼)
    /// </summary>
    public class EnemyDetailPanel : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private Image _hpFillImage;
        [SerializeField] private TextMeshProUGUI _infoText;

        [Header("Action Buttons")]
        [SerializeField] private Button _guardianButton;
        [SerializeField] private Button _arcanaButton;

        [Header("Selection")]
        [SerializeField] private GameObject _targetIndicator;
        [SerializeField] private Outline _panelOutline;

        [Header("Click")]
        [SerializeField] private Button _panelButton;

        [Header("HP Color")]
        [SerializeField] private Color _hpColor = new Color(0.77f, 0.12f, 0.23f);

        private int _enemyIndex;
        private Characters.Character _character;
        private BattleUIManager _uiManager;

        public int EnemyIndex => _enemyIndex;
        public event Action<int> OnPanelClicked;

        private void Awake()
        {
            // Auto-wire: Inspector에 할당되지 않은 필드를 자동으로 찾아 연결
            if (_avatarImage == null) _avatarImage = FindComponent<Image>("Avatar");
            if (_nameText == null) _nameText = FindComponent<TextMeshProUGUI>("Name");
            if (_hpText == null) _hpText = FindComponent<TextMeshProUGUI>("HPBarContainer/HPText");
            if (_hpFillImage == null) _hpFillImage = FindComponent<Image>("HPBarContainer/Fill");
            if (_infoText == null) _infoText = FindComponent<TextMeshProUGUI>("Info");
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
        }

        private void Start()
        {
            if (_guardianButton != null)
                _guardianButton.onClick.AddListener(OnGuardianClicked);

            if (_arcanaButton != null)
                _arcanaButton.onClick.AddListener(OnArcanaClicked);

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
                    popup.Show(_character);
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
        }

        public void UpdateHP(int current, int max)
        {
            float ratio = max > 0 ? (float)current / max : 0f;

            if (_hpText != null)
                _hpText.text = $"{current}/{max}";

            if (_hpFillImage != null)
            {
                _hpFillImage.rectTransform.anchorMax = new Vector2(ratio, 1f);
                _hpFillImage.color = _hpColor;
            }
        }

        public void SetInfoText(string text)
        {
            if (_infoText != null)
                _infoText.text = text;
        }

        public void SetTargetMode(bool isTargetable)
        {
            if (_targetIndicator != null)
                _targetIndicator.SetActive(isTargetable);
        }

        private void OnGuardianClicked()
        {
            // TODO: 가디언 액션 구현 (Phase 4)
        }

        private void OnArcanaClicked()
        {
            // TODO: 아크카 액션 구현 (Phase 4)
        }
    }
}
