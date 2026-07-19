using UnityEngine;
using TMPro;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// ★ Node Detail Preview 파이프 — 보상 1종 분량 행 (Gold/Augments/Relic/Souls 중 1개).
    /// RewardRowPrefab에 부착. SetData 호출 시 자식 TMP 2종(Label/Value)을 자동 바인딩.
    /// </summary>
    public class NodeDetailRewardRow : MonoBehaviour
    {
        private TextMeshProUGUI _labelText;
        private TextMeshProUGUI _valueText;

        private void Awake()
        {
            _labelText = FindChildText("Label");
            _valueText = FindChildText("Value");
        }

        private TextMeshProUGUI FindChildText(string childName)
        {
            var go = UIAutoBindHelper.FindDescendantByName(transform, childName);
            return go?.GetComponent<TextMeshProUGUI>();
        }

        public void SetData(string label, string value)
        {
            if (_labelText == null) _labelText = FindChildText("Label");
            if (_valueText == null) _valueText = FindChildText("Value");

            if (_labelText != null) _labelText.text = label ?? "";
            if (_valueText != null) _valueText.text = value ?? "";
        }
    }
}
