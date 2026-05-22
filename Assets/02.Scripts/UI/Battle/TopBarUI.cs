using UnityEngine;
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

        [Header("Reroll Display")]
        [SerializeField] private TextMeshProUGUI _rerollText;

        private static readonly Color APNormalColor = new Color(0.96f, 0.82f, 0.25f);
        private static readonly Color APShortageColor = new Color(0.85f, 0.2f, 0.2f);
        private static readonly Color RerollNormalColor = new Color(0.72f, 0.45f, 0.2f);
        private static readonly Color RerollEmptyColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

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

        public void SetRerollCount(int remaining, int max)
        {
            if (_rerollText == null) return;
            _rerollText.text = $"리롤 {remaining}/{max}";
            _rerollText.color = remaining > 0 ? RerollNormalColor : RerollEmptyColor;
        }
    }
}
