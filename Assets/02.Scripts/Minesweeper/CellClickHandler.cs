using UnityEngine;
using UnityEngine.EventSystems;

namespace Minesweeper
{
    /// <summary>
    /// 셀 클릭 핸들러 (좌/우클릭 지원)
    /// </summary>
    [RequireComponent(typeof(Cell))]
    public class CellClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private Cell _cell;

        private void Awake()
        {
            _cell = GetComponent<Cell>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_cell == null)
                return;

            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    _cell.OnCellClicked();
                    break;
                case PointerEventData.InputButton.Right:
                    _cell.OnCellRightClicked();
                    break;
            }
        }
    }
}
