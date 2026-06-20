using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TeamLog.Map;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 런 종료 오버레이 — 승리/패배 표시 후 타이틀로 복귀
    /// Phase 8B: 획득 메타 재화(기억의 조각/영혼) 표시 추가
    /// </summary>
    public class RunEndOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private TextMeshProUGUI _statsText;
        [SerializeField] private TextMeshProUGUI _rewardText;
        [SerializeField] private Button _toTitleButton;

        public event System.Action OnReturnToTitle;

        private void Awake()
        {
            if (_toTitleButton != null)
                _toTitleButton.onClick.AddListener(OnToTitleClicked);
        }

        /// <summary>
        /// 런 종료 오버레이 표시. earnedMemory/earnedSouls는 0 이상이어야 표시.
        /// ascensionNote는 어센션 상승 알림 문구 (예: "어센션 상승! 2 → 3"). null/빈 값이면 미표시.
        /// </summary>
        public void Show(bool victory, int floor, int gold, int battlesWon,
            int earnedMemoryFragments = 0, int earnedSouls = 0, string ascensionNote = null)
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0f;

            if (_resultText != null)
                _resultText.text = victory ? "런 승리!" : "런 패배...";

            if (_statsText != null)
                _statsText.text = $"도달 층: {floor}\n획득 골드: {gold}\n전투 승리: {battlesWon}";

            // Phase 8B: 메타 재화 표시 + 어센션 상승 알림
            if (_rewardText != null)
            {
                string ascLine = !string.IsNullOrEmpty(ascensionNote) ? ascensionNote : "";

                if (earnedMemoryFragments > 0 || earnedSouls > 0)
                {
                    string memoryPart = earnedMemoryFragments > 0
                        ? $"기억의 조각 +{earnedMemoryFragments}" : "";
                    string soulPart = earnedSouls > 0
                        ? $"영혼 +{earnedSouls}" : "";
                    string join = (earnedMemoryFragments > 0 && earnedSouls > 0) ? "  " : "";
                    string rewardLine = memoryPart + join + soulPart;

                    _rewardText.text = !string.IsNullOrEmpty(ascLine)
                        ? $"{ascLine}\n{rewardLine}"
                        : rewardLine;
                    _rewardText.gameObject.SetActive(true);
                }
                else if (!string.IsNullOrEmpty(ascLine))
                {
                    _rewardText.text = ascLine;
                    _rewardText.gameObject.SetActive(true);
                }
                else
                {
                    _rewardText.gameObject.SetActive(false);
                }
            }

            // FadeIn
            DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1f, 0.5f);
        }

        private void OnToTitleClicked()
        {
            OnReturnToTitle?.Invoke();
        }
    }
}
