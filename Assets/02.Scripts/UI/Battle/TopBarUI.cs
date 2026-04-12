using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 상단 바 UI (턴 카운터, 턴 종료 버튼)
    /// </summary>
    public class TopBarUI : MonoBehaviour
    {
        [Header("Turn Counter")]
        [SerializeField] private TextMeshProUGUI _turnCounterText;

        [Header("End Turn")]
        [SerializeField] private Button _endTurnButton;

        private void Start()
        {
            if (_endTurnButton != null)
                _endTurnButton.onClick.AddListener(OnEndTurnClicked);
        }

        public void SetTurnCounter(int current, int total)
        {
            if (_turnCounterText != null)
                _turnCounterText.text = $"{current}/{total}";
        }

        private void OnEndTurnClicked()
        {
            // EndTurn 처리는 ActionBarUI의 EndTurn 버튼을 통해 수행됨
        }
    }
}
