using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 전투 종료 오버레이 — 승리/패배 대형 텍스트 + 계속하기 버튼
    /// </summary>
    public class BattleEndOverlay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TextMeshProUGUI _continueLabel;

        public event Action OnContinueClicked;

        private void Awake()
        {
            if (_continueButton != null)
                _continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());

            // 시작 시에는 숨김
            gameObject.SetActive(false);
        }

        public void Show(bool victory)
        {
            if (_resultText != null)
            {
                _resultText.text = victory ? "승리!" : "패배...";
                _resultText.color = victory
                    ? new Color(0.96f, 0.82f, 0.25f)
                    : new Color(0.85f, 0.2f, 0.2f);
            }

            if (_continueLabel != null)
                _continueLabel.text = "계속하기";

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
