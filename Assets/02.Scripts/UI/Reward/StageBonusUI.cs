using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.UI;

namespace TeamLog.UI.Reward
{
    /// <summary>
    /// 엘리트/스테이지 클리어 보상 선택 오버레이 — StageDesign 5.2 / 6.1
    /// 동일 컴포넌트로 두 가지 모드(Elite / StageClear)를 모두 처리.
    /// 선택지 인덱스 == enum 값 매핑 (버튼 3개 고정).
    /// </summary>
    public class StageBonusUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _descLabel;
        [SerializeField] private Button[] _choiceButtons = new Button[3];
        [SerializeField] private TextMeshProUGUI[] _choiceLabels = new TextMeshProUGUI[3];

        private GameRunState _runState;
        private Action _onComplete;
        private bool _isEliteMode;

        // 정적 선택지 데이터 — enum 순서와 일치
        private static readonly (string title, string desc)[] EliteChoices =
        {
            ("추가 유물 수령", "일반 등급 유물 1개를 즉시 획득합니다."),
            ("파티 영구 강화", "전원 HP+15 / ATK+2 / DEF+2 중 무작위 1종 적용."),
            ("상점 할인 + 100G", "다음 상점 50% 할인 + 골드 100 획득."),
        };

        private static readonly (string title, string desc)[] StageChoices =
        {
            ("버스트 준비", "다음 스테이지 첫 전투에서 AP +2 추가 지급."),
            ("재충전", "파티 전원 HP 50% 회복."),
            ("정보 우위", "다음 상점 유물 +1, 증강 +1 진열 추가."),
        };

        public void Initialize(GameRunState runState, Action onComplete)
        {
            _runState = runState;
            _onComplete = onComplete;

            for (int i = 0; i < _choiceButtons.Length; i++)
            {
                int idx = i; // 클로저 캡처
                if (_choiceButtons[i] != null)
                    _choiceButtons[i].onClick.AddListener(() => OnChoiceClicked(idx));
            }

            if (_panel != null)
                _panel.SetActive(false);
        }

        public void ShowEliteBonus()
        {
            _isEliteMode = true;
            if (_titleLabel != null) _titleLabel.text = "엘리트 처치 보너스";
            if (_descLabel != null) _descLabel.text = "하나의 보너스를 선택하세요.";
            PopulateLabels(EliteChoices);
            ShowInternal();
        }

        public void ShowStageClearBonus()
        {
            _isEliteMode = false;
            if (_titleLabel != null) _titleLabel.text = "스테이지 클리어 보너스";
            if (_descLabel != null) _descLabel.text = "다음 스테이지 진입 전 하나를 선택하세요.";
            PopulateLabels(StageChoices);
            ShowInternal();
        }

        private void PopulateLabels((string title, string desc)[] choices)
        {
            for (int i = 0; i < choices.Length && i < _choiceLabels.Length; i++)
            {
                if (_choiceLabels[i] != null)
                    _choiceLabels[i].text = choices[i].title;
            }
        }

        private void ShowInternal()
        {
            if (_panel != null)
                _panel.SetActive(true);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                UIAnimationHelper.FadeIn(_canvasGroup);
            }

            AudioManager.Instance?.PlayUIConfirm();
        }

        private void OnChoiceClicked(int index)
        {
            if (_runState == null) return;

            if (_isEliteMode)
                _runState.ApplyEliteBonus((EliteBonusType)index);
            else
                _runState.ApplyStageClearBonus((StageClearBonusType)index);

            AudioManager.Instance?.PlayUIGoldEarn();

            HideAndNotify();
        }

        private void HideAndNotify()
        {
            // 콜백을 먼저 호출 — FadeOut 이후 SetActive(false) 때문에 코루틴 종료 위험 (CLAUDE.md 체크리스트 #3)
            _onComplete?.Invoke();

            if (_canvasGroup != null)
                UIAnimationHelper.FadeOut(_canvasGroup);
            else if (_panel != null)
                _panel.SetActive(false);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _choiceButtons.Length; i++)
            {
                if (_choiceButtons[i] != null)
                    _choiceButtons[i].onClick.RemoveAllListeners();
            }
        }
    }
}
