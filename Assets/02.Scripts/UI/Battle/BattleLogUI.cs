using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TeamLog.UI;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 로그 항목 타입 — 색상 코딩용
    /// </summary>
    public enum LogEntryType
    {
        Damage,
        Heal,
        Buff,
        Debuff,
        System
    }

    /// <summary>
    /// 우측 사이드바 전투 로그 UI — 색상 코딩 + 스크롤 지원
    /// </summary>
    public class BattleLogUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _logText;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private int _maxLines = 50;

        private readonly Queue<(string message, LogEntryType type)> _logEntries = new();

        public void AddLog(string message, LogEntryType type = LogEntryType.System)
        {
            _logEntries.Enqueue((message, type));

            while (_logEntries.Count > _maxLines)
                _logEntries.Dequeue();

            RefreshDisplay();
        }

        public void Clear()
        {
            _logEntries.Clear();
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (_logText == null) return;

            var palette = UIPalette.Default;
            var lines = new List<string>(_logEntries.Count);

            foreach (var (message, type) in _logEntries)
            {
                string colorHex = type switch
                {
                    LogEntryType.Damage => ColorUtility.ToHtmlStringRGBA(palette.LogDamage),
                    LogEntryType.Heal => ColorUtility.ToHtmlStringRGBA(palette.LogHeal),
                    LogEntryType.Buff => ColorUtility.ToHtmlStringRGBA(palette.LogBuff),
                    LogEntryType.Debuff => ColorUtility.ToHtmlStringRGBA(palette.LogDebuff),
                    _ => ColorUtility.ToHtmlStringRGBA(palette.LogSystem)
                };
                lines.Add($"<color=#{colorHex}>{message}</color>");
            }

            _logText.text = string.Join("\n", lines);

            // 자동 스크롤 (다음 프레임에 실행)
            if (_scrollRect != null)
                StartCoroutine(ScrollToBottom());
        }

        private System.Collections.IEnumerator ScrollToBottom()
        {
            yield return null; // 한 프레임 대기
            if (_scrollRect != null)
                _scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
