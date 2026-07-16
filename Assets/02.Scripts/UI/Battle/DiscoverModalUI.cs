using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Skill;
using TeamLog.UI;
using TeamLog.Characters;
using DG.Tweening;

using SkillData = TeamLog.Characters.SkillData;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 발견(Discover) 모달 UI — 하스스톤 발견 메커니즘.
    /// 스킬 시전 시 3-4개 선택지 팝업 → 플레이어 클릭 → Action&lt;SkillData&gt; 콜백.
    ///
    /// 설계 결정 (Phase CC-2E):
    /// - PlayerActionController는 순수 C#이라 코루틴 불가 → Action&lt;SkillData&gt; 콜백 사용
    /// - Time.timeScale 사용 금지 → CanvasGroup.blocksRaycast + overlay Button으로 입력 차단
    /// - 키보드 단축키 1/2/3/4 지원 (Update에서 검출)
    /// - RewardUI.ShowRewards / CharacterPopupUI overlay 패턴 차용
    /// </summary>
    public class DiscoverModalUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _cardContainer;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private GameObject _discoverCardPrefab;
        [SerializeField] private Button _backgroundButton; // 반투명 배경 클릭 방지용

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;

        private Action<SkillData> _onSelectedCallback;
        private List<DiscoverCard> _currentCards = new List<DiscoverCard>();
        private bool _isActive;
        private Character _caster;

        /// <summary>모달 활성화 여부.</summary>
        public bool IsActive => _isActive;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = UIAnimationHelper.EnsureCanvasGroup(gameObject);
            if (_backgroundButton != null)
                _backgroundButton.onClick.AddListener(OnBackgroundClicked);

            // 초기 비활성화
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_isActive || _currentCards.Count == 0) return;

            // 키보드 단축키 1/2/3/4 — 카드 선택
            for (int i = 0; i < _currentCards.Count && i < 9; i++)
            {
                KeyCode key = KeyCode.Alpha1 + i;
                if (Input.GetKeyDown(key))
                {
                    var card = _currentCards[i];
                    if (card != null)
                    {
                        PlaySelectFeedback();
                        card.TriggerClick();
                    }
                    return;
                }
            }

            // ESC — 취소 (첫 번째 카드 선택으로 폴백 — 발견은 취소 불가)
            // NOTE: 사용자 결정에 따라 ESC는 무시 (발견 스킬 시전 후 반드시 1개 선택해야 함)
        }

        /// <summary>
        /// 발견 모달 표시.
        /// </summary>
        /// <param name="entries">추출된 발견 항목들</param>
        /// <param name="title">모달 제목 (예: "회복 물약")</param>
        /// <param name="onSelected">선택 콜백 — 선택된 SkillData 전달</param>
        /// <param name="caster">시전자 (카드 설명용). null 가능.</param>
        public void Show(List<DiscoverEntry> entries, string title, Action<SkillData> onSelected, Character caster = null)
        {
            if (entries == null || entries.Count == 0)
            {
                Debug.LogWarning("[DiscoverModalUI] 빈 발견 항목으로 모달 표시 실패");
                onSelected?.Invoke(null);
                return;
            }

            _onSelectedCallback = onSelected;
            _caster = caster;
            _isActive = true;

            // 타이틀
            if (_titleLabel != null)
                _titleLabel.text = string.IsNullOrEmpty(title) ? "발견" : title;

            ClearCards();

            // 카드 생성
            int shortcut = 1;
            foreach (var entry in entries)
            {
                if (entry.Skill == null) continue;
                if (_discoverCardPrefab == null || _cardContainer == null) continue;

                var cardObj = Instantiate(_discoverCardPrefab, _cardContainer);
                var card = cardObj.GetComponent<DiscoverCard>();
                if (card != null)
                {
                    card.Setup(entry.Skill, shortcut, OnCardSelected, _caster);
                    _currentCards.Add(card);
                    shortcut++;
                }
            }

            // 페이드 인
            gameObject.SetActive(true);
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            _canvasGroup.alpha = 0f;

            UIAnimationHelper.FadeIn(_canvasGroup, 0.25f);
        }

        /// <summary>카드 선택 콜백 — 모달 닫고 결과 전달.</summary>
        private void OnCardSelected(SkillData selected)
        {
            if (!_isActive) return;

            PlaySelectFeedback();
            HideInternal();

            var cb = _onSelectedCallback;
            _onSelectedCallback = null;
            cb?.Invoke(selected);
        }

        /// <summary>배경 클릭 처리 — 발견은 취소 불가하므로 무시 (로그만).</summary>
        private void OnBackgroundClicked()
        {
            // 발견 모달은 반드시 1개를 선택해야 함 — 배경 클릭 무시
        }

        /// <summary>선택 시 짧은 효과음 재생 (AudioManager 있는 경우).</summary>
        private void PlaySelectFeedback()
        {
            try
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayUIConfirm();
            }
            catch { /* AudioManager 로드 전 무시 */ }
        }

        /// <summary>모달 숨기기 — 애니메이션 후 비활성화.</summary>
        private void HideInternal()
        {
            _isActive = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            var fadeOut = UIAnimationHelper.FadeOut(_canvasGroup, 0.20f);
            // FadeOut이 끝나면 gameObject 비활성화 (UIAnimationHelper가 처리)
            if (fadeOut != null)
            {
                fadeOut.OnComplete(() =>
                {
                    ClearCards();
                });
            }
            else
            {
                ClearCards();
            }
        }

        /// <summary>강제 닫기 — 외부에서 취소 필요 시 (현재 사용처 없음, 안전장치).</summary>
        public void ForceClose()
        {
            if (!_isActive) return;
            HideInternal();
            _onSelectedCallback = null;
        }

        private void ClearCards()
        {
            if (_cardContainer == null) return;
            for (int i = _cardContainer.childCount - 1; i >= 0; i--)
                Destroy(_cardContainer.GetChild(i).gameObject);
            _currentCards.Clear();
        }

        private void OnDestroy()
        {
            if (_backgroundButton != null)
                _backgroundButton.onClick.RemoveListener(OnBackgroundClicked);
        }
    }
}
