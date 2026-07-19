using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// 좌측 Party 사이드바의 파티 멤버 1줄.
    /// 초상화 / 이름 / 클래스 / HP 바 / 자원 배지 표시.
    /// PartySelectionScene의 PartySlotItem 패턴 준거.
    /// </summary>
    public class PartyMemberRow : MonoBehaviour
    {
        [SerializeField] private Image _portrait;             // 자원색 원형 초상화
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _classText;  // "Pyromancer · The Ember"
        [SerializeField] private Image _hpBarFill;            // HP 바 채우기
        [SerializeField] private Image _hpBarBackground;
        [SerializeField] private TextMeshProUGUI _resourceValue;  // 자원 수치
        [SerializeField] private Image _resourceBadge;           // 자원 색 테두리 배지
        [SerializeField] private Image _frameBorder;             // 카드 외곽 (자원색)
        [SerializeField] private CanvasGroup _canvasGroup;        // dead 시 투명도 조절

        private Character _bound;
        private Color _memberColor = Color.white;

        private void Awake()
        {
            AutoBindMissingFields();
        }

        private void AutoBindMissingFields()
        {
            var root = transform;
            if (_portrait == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "Portrait");
                if (go != null) _portrait = go.GetComponent<Image>();
            }
            if (_nameText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "MemberName");
                if (go != null) _nameText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_classText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "MemberClass");
                if (go != null) _classText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_hpBarFill == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "HPFill");
                if (go != null) _hpBarFill = go.GetComponent<Image>();
            }
            if (_resourceValue == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "ResourceValue");
                if (go != null) _resourceValue = go.GetComponent<TextMeshProUGUI>();
            }
            if (_resourceBadge == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "ResourceBadge");
                if (go != null) _resourceBadge = go.GetComponent<Image>();
            }
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // 자식 raycast 비활성화 (오버레이 클릭 가로채기 방지)
            UIAutoBindHelper.DisableChildRaycastsExcept(transform);
        }

        /// <summary>
        /// 파티 멤버 데이터 바인딩.
        /// </summary>
        public void Initialize(Character character, Color memberColor)
        {
            _bound = character;
            _memberColor = memberColor;
            Render();
        }

        public void Refresh()
        {
            if (_bound != null) Render();
        }

        private void Render()
        {
            if (_bound == null) return;
            var data = _bound.Data;
            if (data == null) return;

            // 자원색 적용 — 초상화 테두리, 자원 배지, 프레임 보더
            ApplyColor(_portrait, _memberColor);
            ApplyColor(_resourceBadge, _memberColor);
            ApplyColor(_frameBorder, _memberColor);

            // 초상화 이니셜 — 캐릭터 이름 첫 글자
            if (_portrait != null && _portrait.GetComponentInChildren<TextMeshProUGUI>() is { } initial)
            {
                initial.text = string.IsNullOrEmpty(data.CharacterName) ? "?" : data.CharacterName.Substring(0, 1);
                initial.color = _memberColor;
            }

            if (_nameText != null)
                _nameText.text = data.CharacterName;

            if (_classText != null)
            {
                var (roleEn, roleKo) = PartySelection.PartySelectionUIUtils.GetCharacterRole(data.CharacterName);
                _classText.text = string.IsNullOrEmpty(roleEn) ? data.Class.ToString() : roleEn;
            }

            // HP 바
            if (_hpBarFill != null && _bound.Health != null)
            {
                float ratio = _bound.Health.MaxHP > 0
                    ? Mathf.Clamp01((float)_bound.Health.CurrentHP / _bound.Health.MaxHP)
                    : 0f;
                _hpBarFill.fillAmount = ratio;
                // HP 낮으면 빨강으로 전환 (UIPalette.HPLowThreshold)
                _hpBarFill.color = ratio < 0.3f ? UIPalette.Default.HPLow : UIPalette.Default.HPNormal;
            }

            // 자원 수치
            if (_resourceValue != null && _bound.Resource != null)
            {
                _resourceValue.text = _bound.Resource.CurrentStacks.ToString();
                _resourceValue.color = _memberColor;
            }

            // 사망자 처리 — 그레이스케일/투명도
            bool isDead = _bound.Health != null && _bound.Health.IsDead;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isDead ? 0.4f : 1f;
            }
        }

        private static void ApplyColor(Image image, Color color)
        {
            if (image != null) image.color = color;
        }
    }
}
