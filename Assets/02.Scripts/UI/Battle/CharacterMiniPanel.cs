using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 미니 캐릭터 패널 (액션 바 하단에 표시되는 요약 정보)
    /// </summary>
    public class CharacterMiniPanel : MonoBehaviour
    {
        [SerializeField] private Image _portrait;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _mpText;

        private Character _character;

        public void Setup(Character character)
        {
            _character = character;
            Refresh();
        }

        public void Refresh()
        {
            if (_character == null) return;

            if (_hpText != null)
                _hpText.text = $"{_character.Health.CurrentHP}/{_character.Health.MaxHP}";

            // TODO: MP 시스템 구현 후 연결 (Phase 3)
            if (_mpText != null)
                _mpText.gameObject.SetActive(false);
        }
    }
}
