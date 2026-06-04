using UnityEngine;
using TMPro;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 파티 상태 위젯 — 총 HP, 골드, 층 표시
    /// </summary>
    public class PartyStatusWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private TextMeshProUGUI _floorText;

        public void UpdateDisplay(int totalHP, int maxHP, int gold, int floor)
        {
            if (_hpText != null)
                _hpText.text = $"HP {totalHP}/{maxHP}";
            if (_goldText != null)
                _goldText.text = $"{gold}G";
            if (_floorText != null)
                _floorText.text = $"F{floor}";
        }
    }
}
