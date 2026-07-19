using System.Collections.Generic;
using UnityEngine;
using TeamLog.Skill;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// 좌측 Party 사이드바의 Augments 섹션.
    /// 현재 파티가 보유한 증강 풀을 표시 (Run Log를 대체 — 사용자 요청 2026-07-19).
    /// </summary>
    public class AugmentListPanel : MonoBehaviour
    {
        [SerializeField] private Transform _listContainer;
        [SerializeField] private GameObject _rowPrefab;

        private readonly List<AugmentRow> _rows = new();

        private void Awake()
        {
            AutoBindMissingFields();
        }

        private void AutoBindMissingFields()
        {
            // ★ Priority 8: 자기 자신이 AugmentListContainer일 수 있음.
            if (_listContainer == null)
            {
                if (gameObject.name == "AugmentListContainer")
                {
                    _listContainer = transform;
                }
                else
                {
                    var go = UIAutoBindHelper.FindDescendantByName(transform, "AugmentListContainer");
                    if (go != null) _listContainer = go.transform;
                }
            }
            Debug.Log($"[AugmentListPanel] AutoBind — listContainer:{(_listContainer != null)} rowPrefab:{(_rowPrefab != null)}");
        }

        /// <summary>
        /// 증강 리스트 갱신.
        /// </summary>
        public void Initialize(List<AugmentData> augments)
        {
            Render(augments);
        }

        public void Refresh(List<AugmentData> augments)
        {
            Render(augments);
        }

        private void Render(List<AugmentData> augments)
        {
            if (_listContainer == null || _rowPrefab == null) return;

            // 기존 행 제거
            foreach (var row in _rows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            _rows.Clear();

            if (augments == null) return;

            foreach (var aug in augments)
            {
                if (aug == null) continue;
                var rowGo = Instantiate(_rowPrefab, _listContainer);
                rowGo.gameObject.SetActive(true);
                var row = rowGo.GetComponent<AugmentRow>();
                if (row == null) row = rowGo.AddComponent<AugmentRow>();
                row.Initialize(aug);
                _rows.Add(row);
            }
        }
    }
}
