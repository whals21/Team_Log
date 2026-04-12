using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 우측 사이드바 전투 로그 UI
    /// </summary>
    public class BattleLogUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _logText;
        [SerializeField] private int _maxLines = 50;

        private Queue<string> _logLines = new Queue<string>();

        public void AddLog(string message)
        {
            _logLines.Enqueue(message);

            while (_logLines.Count > _maxLines)
                _logLines.Dequeue();

            RefreshDisplay();
        }

        public void Clear()
        {
            _logLines.Clear();
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (_logText != null)
                _logText.text = string.Join("\n", _logLines);
        }
    }
}
