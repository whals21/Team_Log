using System.Collections.Generic;
using UnityEngine;
using TeamLog.Reward;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// 좌측 Party 사이드바의 유물 그리드 (5×2 슬롯).
    /// 보유 유물 아이콘 표시 + 시너지 발동 중인 유물 글로우 표시.
    /// </summary>
    public class RelicGridPanel : MonoBehaviour
    {
        [SerializeField] private Transform _gridContainer;
        [SerializeField] private GameObject _slotPrefab;        // RelicSlot 프리팹

        // 슬롯 풀 (고정 10개)
        private readonly List<RelicSlotCell> _slots = new();
        private const int SLOT_COUNT = 10;

        private void Awake()
        {
            AutoBindMissingFields();
            EnsureSlots();
        }

        private void AutoBindMissingFields()
        {
            // ★ Priority 8 (치명 수정): 자기 자신이 RelicGridContainer일 수 있음.
            if (_gridContainer == null)
            {
                if (gameObject.name == "RelicGridContainer")
                {
                    _gridContainer = transform;
                }
                else
                {
                    var go = UIAutoBindHelper.FindDescendantByName(transform, "RelicGridContainer");
                    if (go != null) _gridContainer = go.transform;
                }
            }
            Debug.Log($"[RelicGridPanel] AutoBind — gridContainer:{(_gridContainer != null)} slotPrefab:{(_slotPrefab != null)}");
        }

        private void EnsureSlots()
        {
            if (_gridContainer == null || _slotPrefab == null) return;
            if (_slots.Count > 0) return;

            for (int i = 0; i < SLOT_COUNT; i++)
            {
                var slotGo = Instantiate(_slotPrefab, _gridContainer);
                slotGo.gameObject.SetActive(true);
                var slot = slotGo.GetComponent<RelicSlotCell>();
                if (slot == null) slot = slotGo.AddComponent<RelicSlotCell>();
                _slots.Add(slot);
            }
        }

        /// <summary>
        /// 보유 유물 리스트로 그리드 갱신.
        /// </summary>
        public void Initialize(IReadOnlyList<RelicData> relics)
        {
            EnsureSlots();
            Render(relics);
        }

        public void Refresh(IReadOnlyList<RelicData> relics)
        {
            Render(relics);
        }

        private void Render(IReadOnlyList<RelicData> relics)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] == null) continue;
                if (relics != null && i < relics.Count && relics[i] != null)
                {
                    _slots[i].SetRelic(relics[i]);
                }
                else
                {
                    _slots[i].SetEmpty();
                }
            }
        }
    }
}
