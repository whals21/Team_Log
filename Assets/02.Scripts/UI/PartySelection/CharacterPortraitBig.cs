using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI;

namespace TeamLog.UI.PartySelection
{
    /// <summary>
    /// 큰 캐릭터 초상화 (UI-B.6) — 웹 목업의 중앙 메인 초상화.
    /// 280×360 원형 상단 + 사각 하단 (마스크). 자원색 라디언 글로우 + 거대 이니셜 (플레이스홀더).
    /// 자원 배지(우상단). 이름 각인 패널.
    ///
    /// Portrait Sprite가 없으면 플레이스홀더 모드 (자원색 gradient + 이니셜).
    /// 추후 실제 초상화 에셋 추가 시 Sprite만 교체.
    ///
    /// 레이아웃:
    /// PortraitFrame (RectTransform)
    /// ├── Portrait (Image — 마스크 역할, 9-slice)
    /// │   ├── PortraitImage (Image — 실제 초상화, 있을 때만 활성)
    /// │   ├── GlowBackground (Image — 자원색 라디언)
    /// │   └── InitialText (TMP — 거대 이니셜, 플레이스홀더)
    /// ├── ResourceBadge (Image — 50x50 자원 배지 Sprite)
    /// │   ├── ResInitial (TMP)
    /// │   └── ResLabel (TMP)
    /// ├── LockMark (잠금 시)
    /// └── Plate (이름 각인)
    ///     ├── Name (TMP — Cinzel Bold)
    ///     └── Title (TMP — Cormorant Italic)
    /// </summary>
    public class CharacterPortraitBig : MonoBehaviour
    {
        [Header("Portrait Frame")]
        [SerializeField] private Image _portraitFrame;       // 마스크 역할 (자원색 외곽)
        [SerializeField] private Sprite _frameSprite;        // 9-slice 골드 테두리
        [SerializeField] private Image _innerBackground;     // 보이드 내부 배경

        [Header("Portrait Content")]
        [SerializeField] private GameObject _placeholderGroup;  // 플레이스홀더 (Glow + Initial)
        [SerializeField] private Image _glowImage;              // 자원색 라디언
        [SerializeField] private Sprite _glowSprite;            // 라디언 그라디언트 Sprite
        [SerializeField] private TextMeshProUGUI _initialText;  // 거대 이니셜
        [SerializeField] private Image _portraitImage;          // 실제 초상화 (있을 때만 활성)

        [Header("Resource Badge")]
        [SerializeField] private Image _resourceBadge;
        [SerializeField] private TextMeshProUGUI _resourceInitialText;
        [SerializeField] private TextMeshProUGUI _resourceLabelText;

        [Header("Lock")]
        [SerializeField] private GameObject _lockMark;

        [Header("Name Plate")]
        [SerializeField] private Image _plateBackground;
        [SerializeField] private Sprite _plateSprite;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _titleText;

        // 상태
        private CharacterDisplayData _data;

        /// <summary>
        /// CharacterDisplayData로 초기화.
        /// </summary>
        public void Initialize(CharacterDisplayData data)
        {
            _data = data;
            Render();
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

            Color resColor = _data.ResourceColor;

            // 초상화 프레임 외곽 (골드 테두리)
            if (_portraitFrame != null)
            {
                _portraitFrame.sprite = _frameSprite;
                _portraitFrame.color = Color.white;
            }

            // 내부 배경 (보이드)
            if (_innerBackground != null)
            {
                _innerBackground.color = palette.DFVoid;
            }

            // 플레이스홀더 vs 실제 초상화
            bool hasRealPortrait = _data.PortraitSprite != null;
            if (_placeholderGroup != null)
                _placeholderGroup.SetActive(!hasRealPortrait);
            if (_portraitImage != null)
            {
                _portraitImage.gameObject.SetActive(hasRealPortrait);
                if (hasRealPortrait) _portraitImage.sprite = _data.PortraitSprite;
            }

            // 플레이스홀더 — 자원색 라디언 글로우
            if (!hasRealPortrait)
            {
                if (_glowImage != null)
                {
                    _glowImage.sprite = _glowSprite;
                    _glowImage.color = new Color(resColor.r, resColor.g, resColor.b, 0.45f);
                }
                if (_initialText != null)
                {
                    _initialText.text = _data.Initial ?? "?";
                    _initialText.color = new Color(resColor.r, resColor.g, resColor.b, 0.25f);
                }
            }

            // 자원 배지 (우상단)
            if (_resourceBadge != null)
            {
                _resourceBadge.sprite = _data.ResourceBadgeSprite;
                if (_data.ResourceBadgeSprite == null)
                {
                    // Sprite 없으면 자원색 원형
                    _resourceBadge.color = resColor;
                }
                else
                {
                    _resourceBadge.color = Color.white;
                }
            }
            if (_resourceInitialText != null)
            {
                _resourceInitialText.text = _data.ResourceInitial ?? "?";
                _resourceInitialText.color = palette.DFInk;
            }
            if (_resourceLabelText != null)
            {
                _resourceLabelText.text = _data.ResourceLabel ?? "";
                _resourceLabelText.color = palette.DFInkDim;
            }

            // 잠금 표시
            if (_lockMark != null)
            {
                _lockMark.SetActive(_data.Locked);
            }

            // 이름 각인
            if (_plateBackground != null)
            {
                _plateBackground.sprite = _plateSprite;
                _plateBackground.color = Color.white;
            }
            if (_nameText != null)
            {
                _nameText.text = _data.DisplayName;
                _nameText.color = palette.DFGoldL;
            }
            if (_titleText != null)
            {
                _titleText.text = _data.Title;
                _titleText.color = palette.DFInkDim;
            }
        }

        /// <summary>
        /// 자원 글로우 색상 업데이트 (자원 스택 변화 시 호출 — 향후 전투 UI에서 활용 가능).
        /// </summary>
        public void SetGlowIntensity(float intensity)
        {
            if (_glowImage == null || _data == null) return;
            Color c = _data.ResourceColor;
            _glowImage.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(intensity));
        }
    }
}
