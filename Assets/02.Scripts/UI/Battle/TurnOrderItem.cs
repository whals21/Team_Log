using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 개별 턴 순서 아이템
    /// </summary>
    public class TurnOrderItem : MonoBehaviour
    {
        [SerializeField] private Image _portrait;
        [SerializeField] private TextMeshProUGUI _orderText;
        [SerializeField] private GameObject _highlight;

        private Character _character;

        public void Setup(Character character, int order)
        {
            _character = character;

            if (_orderText != null)
                _orderText.text = order.ToString();

            if (_portrait != null && character != null)
            {
                // 클래스에 따른 색상 설정
                _portrait.color = GetClassColor(character.Class);
            }
        }

        public void SetHighlight(bool active)
        {
            if (_highlight != null)
                _highlight.SetActive(active);
        }

        private Color GetClassColor(CharacterClass charClass)
        {
            return charClass switch
            {
                CharacterClass.Warrior => new Color(0.8f, 0.3f, 0.3f),
                CharacterClass.Mage => new Color(0.3f, 0.5f, 0.9f),
                CharacterClass.Healer => new Color(0.3f, 0.8f, 0.5f),
                CharacterClass.Rogue => new Color(0.8f, 0.7f, 0.2f),
                _ => Color.gray
            };
        }
    }
}
