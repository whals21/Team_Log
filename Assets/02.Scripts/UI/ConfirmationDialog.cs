using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace TeamLog.UI
{
    /// <summary>
    /// 범용 확인 다이얼로그 (예/아니오)
    /// </summary>
    public class ConfirmationDialog : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _noButton;

        private Action _onConfirm;
        private Action _onCancel;

        private void Awake()
        {
            if (_yesButton != null) _yesButton.onClick.AddListener(OnYes);
            if (_noButton != null) _noButton.onClick.AddListener(OnNo);
            gameObject.SetActive(false);
        }

        public void Show(string message, Action onConfirm, Action onCancel = null)
        {
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            if (_messageText != null) _messageText.text = message;
            gameObject.SetActive(true);
        }

        private void OnYes()
        {
            gameObject.SetActive(false);
            _onConfirm?.Invoke();
            _onConfirm = null;
            _onCancel = null;
        }

        private void OnNo()
        {
            gameObject.SetActive(false);
            _onCancel?.Invoke();
            _onConfirm = null;
            _onCancel = null;
        }
    }
}
