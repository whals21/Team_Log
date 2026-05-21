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

        [Header("AP Display")]
        [SerializeField] private TextMeshProUGUI _apText;

        [Header("End Turn")]
        [SerializeField] private Button _endTurnButton;

        private static readonly Color APNormalColor = new Color(0.96f, 0.82f, 0.25f);
        private static readonly Color APShortageColor = new Color(0.85f, 0.2f, 0.2f);

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

        public void SetAP(int current, int max)
        {
            if (_apText == null) return;
            _apText.text = $"AP {current}/{max}";
            _apText.color = current == 0 ? APShortageColor : APNormalColor;
        }

        private void OnEndTurnClicked()
        {
            // EndTurn 처리는 ActionBarUI의 EndTurn 버튼을 통해 수행됨
        }
    }
}
