using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Map;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 튜토리얼 단계 — 진행 상태 추적
    /// </summary>
    public enum TutorialStep
    {
        None = 0,
        MapNavigation = 1,
        BattleBasics = 2,
        ShopBasics = 3,
        RestBasics = 4,
        Completed = 5
    }

    /// <summary>
    /// 인터랙티브 튜토리얼 오버레이 — 하이라이트 + 설명 + 단계 진행
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _overlay;
        [SerializeField] private Image _highlightArea;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descText;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private TextMeshProUGUI _nextLabel;

        private TutorialStep _currentStep = TutorialStep.None;
        private GameRunState _runState;
        private CanvasGroup _canvasGroup;

        public TutorialStep CurrentStep => _currentStep;

        private void Awake()
        {
            if (_nextButton != null)
                _nextButton.onClick.AddListener(OnNext);
            if (_skipButton != null)
                _skipButton.onClick.AddListener(OnSkip);
            _canvasGroup = UIAnimationHelper.EnsureCanvasGroup(gameObject);
        }

        public void Initialize(GameRunState runState)
        {
            _runState = runState;
        }

        /// <summary>
        /// 튜토리얼 시작 여부 확인 — MetaSaveData 기반
        /// </summary>
        public bool ShouldShowTutorial()
        {
            return !SaveManager.Meta.HasCompletedTutorial;
        }

        public void ShowStep(TutorialStep step)
        {
            if (step == TutorialStep.None || step == TutorialStep.Completed) return;

            _currentStep = step;
            gameObject.SetActive(true);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                UIAnimationHelper.FadeIn(_canvasGroup);
            }

            ConfigureStep(step);
        }

        private void ConfigureStep(TutorialStep step)
        {
            switch (step)
            {
                case TutorialStep.MapNavigation:
                    SetContent("맵 탐색",
                        "맵에서 이동할 노드를 선택하세요.\n" +
                        "각 노드는 전투, 상점, 이벤트, 휴식 등\n" +
                        "다양한 이벤트가 발생합니다.");
                    break;
                case TutorialStep.BattleBasics:
                    SetContent("전투 기본",
                        "전투에서 스킬을 드로우하고 AP를 관리하세요.\n" +
                        "AP는 파티 공유 자원이며,\n" +
                        "스킬 사용 시 비용이 차감됩니다.");
                    break;
                case TutorialStep.ShopBasics:
                    SetContent("상점 이용",
                        "상점에서 스킬과 아이템을 구매/판매하세요.\n" +
                        "골드를 잘 관리하는 것이 중요합니다.");
                    break;
                case TutorialStep.RestBasics:
                    SetContent("휴식 지점",
                        "캠프파이어에서 파티를 회복하세요.\n" +
                        "휴식(HP 회복), 수련(ATK 증가),\n" +
                        "명상(AP 보너스) 중 선택할 수 있습니다.");
                    break;
            }

            if (_nextLabel != null)
                _nextLabel.text = "다음";
        }

        private void SetContent(string title, string desc)
        {
            if (_titleText != null) _titleText.text = title;
            if (_descText != null) _descText.text = desc;
        }

        private void OnNext()
        {
            AudioManager.Instance?.PlayUIConfirm();

            // 현재 단계 완료 표시
            int nextStep = (int)_currentStep + 1;

            if (nextStep >= (int)TutorialStep.Completed)
            {
                CompleteTutorial();
                return;
            }

            Hide();

            // 다음 단계는 해당 이벤트 발생 시 ShowStep으로 트리거
            _currentStep = (TutorialStep)nextStep;
        }

        private void OnSkip()
        {
            AudioManager.Instance?.PlayUICancel();
            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            _currentStep = TutorialStep.Completed;
            var meta = SaveManager.Meta;
            meta.HasCompletedTutorial = true;
            SaveManager.SaveMeta();
            Hide();
        }

        public void Hide()
        {
            _canvasGroup = UIAnimationHelper.EnsureCanvasGroup(gameObject);
            UIAnimationHelper.FadeOut(_canvasGroup);
        }

        /// <summary>
        /// 특정 이벤트에서 튜토리얼 단계를 트리거해야 하는지 확인
        /// </summary>
        public void TryShowStep(TutorialStep triggerStep)
        {
            if (_currentStep == triggerStep && !SaveManager.Meta.HasCompletedTutorial)
                ShowStep(triggerStep);
        }

        private void OnDestroy()
        {
            if (_nextButton != null)
                _nextButton.onClick.RemoveListener(OnNext);
            if (_skipButton != null)
                _skipButton.onClick.RemoveListener(OnSkip);
        }
    }
}
