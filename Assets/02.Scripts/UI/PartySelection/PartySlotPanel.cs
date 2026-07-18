using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI;

namespace TeamLog.UI.PartySelection
{
    /// <summary>
    /// 파티 슬롯 + 시작 버튼 패널 (UI-B.7) — 웹 목업의 하단 파티 영역 재현.
    /// 4 슬롯 + 슬롯 번호 배지 + RANDOM/CLEAR/EMBARK 버튼.
    ///
    /// 레이아웃:
    /// PartySlotPanel (Image — 9-slice SlatePanel, 골드 테두리)
    /// ├── LeftSection
    /// │   ├── PartyLabel (TMP — "PARTY")
    /// │   └── SlotsContainer (HorizontalLayoutGroup)
    /// │       └── PartySlotItem × 4
    /// └── RightSection
    ///       ├── RandomBtn (Button + TMP — "⚜ RANDOM")
    ///       ├── ClearBtn  (Button + TMP — "✕ CLEAR")
    ///       └── EmbarkBtn (Button + TMP — "EMBARK ▶", BloodButton 3-state)
    /// </summary>
    public class PartySlotPanel : MonoBehaviour
    {
        [Header("Slots")]
        [SerializeField] private Transform _slotsContainer;
        [SerializeField] private PartySlotItem _slotPrefab;
        [SerializeField] private int _slotCount = 4;

        [Header("Buttons")]
        [SerializeField] private Button _embarkButton;
        [SerializeField] private Button _randomButton;
        [SerializeField] private Button _clearButton;

        [Header("Sprites")]
        [SerializeField] private Sprite _normalSprite;
        [SerializeField] private Sprite _hoverSprite;
        [SerializeField] private Sprite _pressedSprite;
        [SerializeField] private Sprite _disabledSprite;

        // 상태
        private readonly List<PartySlotItem> _slots = new();
        private readonly CharacterDisplayData[] _slotData;
        private Action<List<CharacterDisplayData>> _onEmbark;
        private Action _onRandom;
        private Action _onClear;
        private Action<int> _onSlotClicked;

        public int SlotCount => _slotCount;

        private void Awake()
        {
            // ★ 자동 바인딩 — 빌더가 필드를 못 채웠을 때 자식에서 검색
            AutoBindChildFields();

            // 버튼 이벤트 바인딩
            if (_embarkButton != null) _embarkButton.onClick.AddListener(() => _onEmbark?.Invoke(GetFilledParty()));
            if (_randomButton != null) _randomButton.onClick.AddListener(() => _onRandom?.Invoke());
            if (_clearButton != null) _clearButton.onClick.AddListener(() => _onClear?.Invoke());
        }

        /// <summary>
        /// 자식 GameObject에서 버튼/슬롯 컨테이너/슬롯 템플릿 자동 검색.
        /// 빌더의 BindField가 실패했을 때의 폴백.
        /// </summary>
        private void AutoBindChildFields()
        {
            if (_embarkButton == null)
            {
                var go = FindDescendantByName(transform, "BtnEmbark");
                if (go != null) _embarkButton = go.GetComponent<Button>();
            }
            if (_randomButton == null)
            {
                var go = FindDescendantByName(transform, "BtnRandom");
                if (go != null) _randomButton = go.GetComponent<Button>();
            }
            if (_clearButton == null)
            {
                var go = FindDescendantByName(transform, "BtnClear");
                if (go != null) _clearButton = go.GetComponent<Button>();
            }
            if (_slotsContainer == null)
            {
                var go = FindDescendantByName(transform, "SlotsContainer");
                if (go != null) _slotsContainer = go.transform;
            }
            if (_slotPrefab == null)
            {
                var go = FindDescendantByName(transform, "SlotTemplate");
                if (go != null)
                {
                    _slotPrefab = go.GetComponent<PartySlotItem>();
                    if (_slotPrefab == null)
                    {
                        _slotPrefab = go.gameObject.AddComponent<PartySlotItem>();
                    }
                }
            }

            // AutoBind 완료 — 로그 제거됨 (GC 2026-07-18)
        }

        private static Transform FindDescendantByName(Transform root, string name)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == name) return child;
                var found = FindDescendantByName(child, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// 패널 초기화. 슬롯 아이템들을 동적으로 생성.
        /// </summary>
        public void Initialize(
            int slotCount,
            Action<List<CharacterDisplayData>> onEmbark,
            Action onRandom,
            Action onClear,
            Action<int> onSlotClicked)
        {
            _slotCount = Mathf.Max(1, slotCount);
            _onEmbark = onEmbark;
            _onRandom = onRandom;
            _onClear = onClear;
            _onSlotClicked = onSlotClicked;

            BuildSlots();
            UpdateEmbarkButton();
        }

        private void BuildSlots()
        {
            ClearSlots();

            if (_slotsContainer == null || _slotPrefab == null) return;

            for (int i = 0; i < _slotCount; i++)
            {
                var slot = Instantiate(_slotPrefab, _slotsContainer);
                slot.gameObject.SetActive(true);  // 비활성 템플릿 복제본 활성화
                slot.Initialize(i, OnSlotClickedInternal);
                _slots.Add(slot);
            }
        }

        private void ClearSlots()
        {
            foreach (var slot in _slots)
            {
                if (slot != null && slot.gameObject != null)
                    Destroy(slot.gameObject);
            }
            _slots.Clear();
        }

        /// <summary>
        /// 특정 슬롯에 캐릭터 배치 (빈 슬롯에 추가).
        /// </summary>
        public bool TryFillSlot(CharacterDisplayData data)
        {
            if (data == null) return false;
            if (_slots == null) return false;
            int emptyIdx = FindFirstEmptySlot();
            if (emptyIdx < 0) return false;
            SetSlot(emptyIdx, data);
            return true;
        }

        /// <summary>
        /// 특정 슬롯에 직접 설정.
        /// </summary>
        public void SetSlot(int index, CharacterDisplayData data)
        {
            if (index < 0 || index >= _slots.Count) return;
            _slots[index].SetData(data);
            UpdateEmbarkButton();
        }

        /// <summary>
        /// 특정 슬롯 비우기.
        /// </summary>
        public void ClearSlot(int index)
        {
            if (index < 0 || index >= _slots.Count) return;
            _slots[index].Clear();
            CompactSlots();
            UpdateEmbarkButton();
        }

        /// <summary>
        /// 모든 슬롯 비우기.
        /// </summary>
        public void ClearAll()
        {
            foreach (var slot in _slots) slot?.Clear();
            UpdateEmbarkButton();
        }

        /// <summary>
        /// 현재 채워진 파티 반환 (null 제외).
        /// </summary>
        public List<CharacterDisplayData> GetFilledParty()
        {
            var party = new List<CharacterDisplayData>();
            foreach (var slot in _slots)
            {
                if (slot != null && slot.Data != null)
                    party.Add(slot.Data);
            }
            return party;
        }

        public int FilledCount => GetFilledParty().Count;
        public bool IsFull => FilledCount >= _slotCount;
        public bool IsEmpty => FilledCount == 0;

        // ── 내부 ──
        private int FindFirstEmptySlot()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] != null && _slots[i].Data == null)
                    return i;
            }
            return -1;
        }

        private void CompactSlots()
        {
            // 빈 슬롯을 뒤로 밀기 (채워진 것을 앞으로 모음)
            var filled = GetFilledParty();
            ClearAll();
            for (int i = 0; i < filled.Count && i < _slotCount; i++)
            {
                _slots[i].SetData(filled[i]);
            }
        }

        private void OnSlotClickedInternal(int index)
        {
            _onSlotClicked?.Invoke(index);
        }

        private void UpdateEmbarkButton()
        {
            if (_embarkButton == null) return;
            bool canEmbark = FilledCount > 0;
            _embarkButton.interactable = canEmbark;
            // Button transition 자동 처리 (ColorTint/SpriteSwap)
        }
    }
}
