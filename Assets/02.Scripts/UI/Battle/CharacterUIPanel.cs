using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 캐릭터 UI 패널 (HP/MP 바, 초상화, 상태이상 표시)
    /// </summary>
    public class CharacterUIPanel : MonoBehaviour
    {
        [Header("Portrait")]
        [SerializeField] private Image _portraitImage;
        [SerializeField] private GameObject _turnIndicator;

        [Header("Bars")]
        [SerializeField] private HPBarUI _hpBar;
        [SerializeField] private MPBarUI _mpBar;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Transform _statusEffectContainer;

        [Header("Selection")]
        [SerializeField] private GameObject _selectionHighlight;
        [SerializeField] private GameObject _targetIndicator;

        private Character _character;
        private bool _isPlayer;

        public Character Character => _character;
        public bool IsPlayer => _isPlayer;

        public void Setup(Character character, bool isPlayer)
        {
            _character = character;
            _isPlayer = isPlayer;

            if (_nameText != null)
                _nameText.text = character.Name;

            // 초상화 설정 (임시: 클래스별 색상으로 구분)
            if (_portraitImage != null)
            {
                _portraitImage.color = GetClassColor(character.Class);
            }

            Refresh();
        }

        private Color GetClassColor(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => new Color(0.8f, 0.3f, 0.3f),
                CharacterClass.Mage => new Color(0.3f, 0.5f, 0.9f),
                CharacterClass.Healer => new Color(0.3f, 0.8f, 0.5f),
                CharacterClass.Rogue => new Color(0.8f, 0.7f, 0.2f),
                _ => Color.gray
            };
        }

        public void Refresh()
        {
            if (_character == null) return;

            if (_hpBar != null)
                _hpBar.UpdateHP(_character.Health.CurrentHP, _character.Health.MaxHP);

            // MP 바는 나중에 구현
            if (_mpBar != null)
                _mpBar.gameObject.SetActive(false);

            UpdateStatusEffects();
        }

        private void UpdateStatusEffects()
        {
            if (_statusEffectContainer == null) return;

            // 기존 상태이상 아이콘 제거
            foreach (Transform child in _statusEffectContainer)
                Destroy(child.gameObject);

            // 현재 상태이상 표시 (간단한 텍스트로)
            // TODO: 상태이상 아이콘 프리팹 사용
        }

        public void SetTurnIndicator(bool isActive)
        {
            if (_turnIndicator != null)
                _turnIndicator.SetActive(isActive);
        }

        public void SetSelected(bool isSelected)
        {
            if (_selectionHighlight != null)
                _selectionHighlight.SetActive(isSelected);
        }

        public void SetTargetMode(bool isTargetable)
        {
            if (_targetIndicator != null)
                _targetIndicator.SetActive(isTargetable);
        }

        public void OnClicked()
        {
            // 클릭 이벤트 처리 (타겟 선택 등)
        }
    }
}
