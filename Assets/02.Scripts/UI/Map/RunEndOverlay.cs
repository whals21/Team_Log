using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TeamLog.Map;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 런 종료 오버레이 — 승리/패배 표시 후 타이틀로 복귀
    /// </summary>
    public class RunEndOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private TextMeshProUGUI _statsText;
        [SerializeField] private Button _toTitleButton;

        public event System.Action OnReturnToTitle;

        private void Awake()
        {
            if (_toTitleButton != null)
                _toTitleButton.onClick.AddListener(OnToTitleClicked);
        }

        public void Show(bool victory, int floor, int gold, int battlesWon)
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0f;

            if (_resultText != null)
                _resultText.text = victory ? "런 승리!" : "런 패배...";

            if (_statsText != null)
                _statsText.text = $"도달 층: {floor}\n획득 골드: {gold}\n전투 승리: {battlesWon}";

            // FadeIn
            DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1f, 0.5f);
        }

        private void OnToTitleClicked()
        {
            OnReturnToTitle?.Invoke();
        }
    }
}
