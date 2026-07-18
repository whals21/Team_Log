using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI;

namespace TeamLog.UI.PartySelection
{
    /// <summary>
    /// 캐릭터 캐러셀 썸네일 항목 (UI-B.5) — 웹 목업의 하단 캐릭터 썸네일.
    /// 70×70 원형 배지 (자원색) + 이니셜 + 이름 + NEW/REWORK 표시 + 상태(active/in-party/locked).
    ///
    /// 레이아웃:
    /// CarouselItem (RectTransform + Button)
    /// ├── Portrait (70x70 Image — 9-slice/원형, 자원색 라디언 + 골드 테두리)
    /// │   └── InitialText (TMP — 거대 이니셜)
    /// ├── InPartyBadge (활성화 시 — 핏빛 원형 + 체크)
    /// ├── LockOverlay (잠금 시 — 검정 반투명 + 🔒)
    /// ├── TagMark (NEW/REWORK — 작은 표시)
    /// └── NameText (TMP — 작게)
    /// </summary>
    public class CharacterCarouselItem : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private Button _button;

        [Header("Visuals")]
        [SerializeField] private Image _portraitImage;
        [SerializeField] private TextMeshProUGUI _initialText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private GameObject _tagMark;
        [SerializeField] private TextMeshProUGUI _tagText;

        [Header("States")]
        [SerializeField] private GameObject _inPartyBadge;  // 파티 소속 시 체크
        [SerializeField] private GameObject _lockOverlay;   // 잠금 시 검정 오버레이
        [SerializeField] private GameObject _activeRing;     // 현재 선택된 캐릭터 강조 링

        [Header("Sprites")]
        [SerializeField] private Sprite _badgeSprite;  // ResourceBadge_*.png (자원별 동적 교체)

        // 상태
        private CharacterDisplayData _data;
        private Action<CharacterDisplayData> _onClicked;
        private bool _isActive;
        private bool _isInParty;

        /// <summary>
        /// 캐러셀 항목 초기화.
        /// </summary>
        public void Initialize(CharacterDisplayData data, Action<CharacterDisplayData> onClicked)
        {
            _data = data;
            _onClicked = onClicked;

            // ★ Button 자동 보완 — 인스펙터 바인딩 실패 시
            if (_button == null)
            {
                _button = GetComponent<Button>();
                if (_button == null)
                {
                    _button = gameObject.AddComponent<Button>();
                }
            }

            // ★ targetGraphic 보완 — Portrait Image가 Raycast 가능해야 클릭됨
            if (_button.targetGraphic == null && _portraitImage != null)
            {
                _portraitImage.raycastTarget = true;
                _button.targetGraphic = _portraitImage;
            }

            _button.interactable = data != null && !data.Locked;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClicked);

            // ★★ 핵심: 모든 자식 Image의 raycastTarget=false 강제
            // Button용 targetGraphic(_portraitImage)만 raycastTarget=true 유지
            // 나머지 자식(ActiveRing/LockOverlay/InPartyBadge 등)이 클릭 가로채지 못하게
            DisableChildRaycasts();

            Render();
        }

        /// <summary>
        /// 자식 Image 중 Button용 targetGraphic을 제외한 모든 Image의 raycastTarget=false.
        /// 부모 Button 클릭이 자식 UI에 가로채이지 않도록 보장.
        /// </summary>
        private void DisableChildRaycasts()
        {
            var images = GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img == _portraitImage) continue;  // 클릭 감지용 유지
                img.raycastTarget = false;
            }
        }

        private void Render()
        {
            var palette = UIPalette.Default;

            if (_data == null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            // 초상화 배경 — 자원색 Sprite 우선, 없으면 solid 컬러
            if (_portraitImage != null)
            {
                if (_data.ResourceBadgeSprite != null)
                {
                    _portraitImage.sprite = _data.ResourceBadgeSprite;
                    _portraitImage.color = Color.white;
                }
                else if (_badgeSprite != null)
                {
                    _portraitImage.sprite = _badgeSprite;
                    _portraitImage.color = _data.ResourceColor;
                }
                else
                {
                    _portraitImage.color = _data.ResourceColor;
                }
            }

            // 이니셜 — 자원색 라디언 중심에 흰색으로 표시
            if (_initialText != null)
            {
                _initialText.text = _data.Initial ?? "?";
                _initialText.color = Color.white;
            }

            // 이름
            if (_nameText != null)
            {
                string tagMark = _data.Tag == CharacterTag.New ? " ✦"
                              : _data.Tag == CharacterTag.Rework ? " ⟳"
                              : "";
                _nameText.text = _data.DisplayName + tagMark;
                _nameText.color = _isActive ? palette.DFGoldL : palette.DFInkDim;
            }

            // NEW/REWORK 태그 마크 (별도 표시 — 있을 경우)
            if (_tagMark != null)
            {
                bool showTag = _data.Tag != CharacterTag.None;
                _tagMark.SetActive(showTag);
                if (showTag && _tagText != null)
                {
                    _tagText.text = _data.Tag == CharacterTag.New ? "NEW" : "REWORK";
                    _tagText.color = _data.Tag == CharacterTag.New ? palette.DFGoldL : palette.DFBloodL;
                }
            }

            // 잠금 오버레이
            if (_lockOverlay != null)
            {
                _lockOverlay.SetActive(_data.Locked);
            }

            // 활성 링 (현재 선택된 캐릭터)
            if (_activeRing != null)
            {
                _activeRing.SetActive(_isActive);
            }

            // 파티 소속 배지
            if (_inPartyBadge != null)
            {
                _inPartyBadge.SetActive(_isInParty);
            }
        }

        private void OnClicked()
        {
            if (_data == null || _data.Locked) return;
            _onClicked?.Invoke(_data);
        }

        // ── 상태 업데이트 (외부에서 호출) ──
        public void SetActive(bool active)
        {
            if (_isActive == active) return;
            _isActive = active;
            Render();
        }

        public void SetInParty(bool inParty)
        {
            if (_isInParty == inParty) return;
            _isInParty = inParty;
            Render();
        }

        public CharacterDisplayData Data => _data;
    }
}
