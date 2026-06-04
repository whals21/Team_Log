using UnityEngine;
using UnityEngine.EventSystems;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 툴팁 트리거 컴포넌트 — IPointerEnterHandler/ExitHandler, 툴팁 콘텐츠 설정
    /// </summary>
    public class TooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private string _title;
        private string _description;

        public void SetContent(string title, string description)
        {
            _title = title;
            _description = description;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (TooltipUI.Instance != null && !string.IsNullOrEmpty(_title))
                TooltipUI.Instance.Show(_title, _description);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Hide();
        }

        private void OnDisable()
        {
            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Hide();
        }
    }
}
