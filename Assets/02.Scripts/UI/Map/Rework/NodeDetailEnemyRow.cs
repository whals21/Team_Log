using UnityEngine;
using TMPro;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// ★ Node Detail Preview 파이프 — 적 1명 분량 행.
    /// EnemyRowPrefab에 부착. SetData 호출 시 자식 TMP 2종(Name/HP)을 자동 바인딩.
    /// </summary>
    public class NodeDetailEnemyRow : MonoBehaviour
    {
        private TextMeshProUGUI _nameText;
        private TextMeshProUGUI _hpText;

        private void Awake()
        {
            _nameText = FindChildText("Name");
            _hpText = FindChildText("HP");
        }

        private TextMeshProUGUI FindChildText(string childName)
        {
            var go = UIAutoBindHelper.FindDescendantByName(transform, childName);
            return go?.GetComponent<TextMeshProUGUI>();
        }

        public void SetData(EnemyPreviewInfo info)
        {
            if (info == null) return;
            if (_nameText == null) _nameText = FindChildText("Name");
            if (_hpText == null) _hpText = FindChildText("HP");

            if (_nameText != null)
            {
                _nameText.text = info.Name ?? "Unknown";
                _nameText.color = info.Tint;
            }
            if (_hpText != null)
            {
                _hpText.text = $"HP {info.EstimatedHP}";
            }
        }
    }
}
