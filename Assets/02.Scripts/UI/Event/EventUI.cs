using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Event;
using TeamLog.Map;

namespace TeamLog.UI.Event
{
    /// <summary>
    /// 이벤트 UI — 이야기 텍스트 + 선택지 버튼 + 결과 표시
    /// </summary>
    public class EventUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _eventTitleLabel;
        [SerializeField] private TextMeshProUGUI _eventDescLabel;
        [SerializeField] private Transform _choiceContainer;
        [SerializeField] private GameObject _choiceButtonPrefab;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultLabel;
        [SerializeField] private Button _resultConfirmButton;

        private EventData _currentEvent;
        private EventManager _eventManager;
        private GameRunState _runState;
        private System.Action _onEventComplete;

        private void Awake()
        {
            if (_resultConfirmButton != null)
                _resultConfirmButton.onClick.AddListener(OnResultConfirmed);
        }

        public void Initialize(GameRunState runState, System.Action onEventComplete)
        {
            _runState = runState;
            _onEventComplete = onEventComplete;
            _eventManager = new EventManager();
        }

        /// <summary>
        /// 이벤트 화면 표시
        /// </summary>
        public void ShowEvent(EventData eventData)
        {
            _currentEvent = eventData;
            gameObject.SetActive(true);

            if (_resultPanel != null)
                _resultPanel.SetActive(false);

            // 이벤트 정보 표시
            if (_eventTitleLabel != null)
                _eventTitleLabel.text = eventData.EventName;
            if (_eventDescLabel != null)
                _eventDescLabel.text = eventData.Description;

            // 선택지 버튼 생성
            ClearChoices();
            for (int i = 0; i < eventData.Choices.Count; i++)
            {
                if (_choiceButtonPrefab == null || _choiceContainer == null) continue;

                var choiceObj = Instantiate(_choiceButtonPrefab, _choiceContainer);
                var choiceText = choiceObj.GetComponentInChildren<TextMeshProUGUI>();

                if (choiceText != null)
                    choiceText.text = eventData.Choices[i].ChoiceText;

                var button = choiceObj.GetComponent<Button>();
                if (button != null)
                {
                    int index = i;
                    button.onClick.AddListener(() => OnChoiceSelected(index));
                }
            }
        }

        private void OnChoiceSelected(int choiceIndex)
        {
            var outcome = _eventManager.ProcessChoice(_currentEvent, choiceIndex, _runState);
            if (outcome == null) return;

            // 선택지 숨기고 결과 표시
            ClearChoices();

            if (_resultPanel != null)
                _resultPanel.SetActive(true);

            if (_resultLabel != null)
                _resultLabel.text = outcome.ResultText;
        }

        private void OnResultConfirmed()
        {
            gameObject.SetActive(false);
            _onEventComplete?.Invoke();
        }

        private void ClearChoices()
        {
            if (_choiceContainer == null) return;
            for (int i = _choiceContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_choiceContainer.GetChild(i).gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_resultConfirmButton != null)
                _resultConfirmButton.onClick.RemoveListener(OnResultConfirmed);
        }
    }
}
