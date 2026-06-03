using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 캠프파이어 휴식 선택지 UI — 휴식/수련/명상 3가지 선택 제공
    /// </summary>
    public class RestUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _restButton;
        [SerializeField] private Button _trainButton;
        [SerializeField] private Button _meditateButton;

        public void Initialize(System.Action<int> onChoiceSelected)
        {
            _restButton.onClick.AddListener(() => { Hide(); onChoiceSelected(0); });
            _trainButton.onClick.AddListener(() => { Hide(); onChoiceSelected(1); });
            _meditateButton.onClick.AddListener(() => { Hide(); onChoiceSelected(2); });
            _panel.SetActive(false);
        }

        public void Show()
        {
            _panel.SetActive(true);
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }
    }
}
