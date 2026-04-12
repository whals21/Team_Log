using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 턴 순서 표시 UI
    /// </summary>
    public class TurnOrderUI : MonoBehaviour
    {
        [Header("Turn Order Display")]
        [SerializeField] private Transform _turnOrderContainer;
        [SerializeField] private TurnOrderItem _turnItemPrefab;
        [SerializeField] private int _maxDisplayItems = 8;

        private List<TurnOrderItem> _turnItems = new List<TurnOrderItem>();

        public void UpdateTurnOrder(List<Character> turnOrder)
        {
            ClearItems();

            int count = Mathf.Min(turnOrder.Count, _maxDisplayItems);
            for (int i = 0; i < count; i++)
            {
                var item = Instantiate(_turnItemPrefab, _turnOrderContainer);
                item.Setup(turnOrder[i], i + 1);
                _turnItems.Add(item);
            }
        }

        public void HighlightCurrentTurn(int index)
        {
            for (int i = 0; i < _turnItems.Count; i++)
            {
                _turnItems[i].SetHighlight(i == index);
            }
        }

        private void ClearItems()
        {
            foreach (var item in _turnItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            _turnItems.Clear();
        }
    }
}
