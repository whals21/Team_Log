using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Event;
using TeamLog.Map;
using TeamLog.UI;

namespace TeamLog.UI.Event
{
    /// <summary>
    /// 이벤트 UI — 이야기 텍스트 + 선택지 버튼 + 결과 표시
    /// </summary>
    public class EventUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _eventTitleLabel;
        [SerializeField] private TextMeshProUGUI _eventDescLabel;
        [SerializeField] private Transform _choiceContainer;
        [SerializeField] private GameObject _choiceButtonPrefab;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultLabel;
        [SerializeField] private Button _resultConfirmButton;

        [Header("Risk Colors (위험도 색상 코딩)")]
        [SerializeField] private Color _riskColorSafe = new Color(0.22f, 0.32f, 0.22f, 1f);       // 어두운 녹색
        [SerializeField] private Color _riskColorNormal = new Color(0.18f, 0.18f, 0.26f, 1f);     // 기본 어두운 색
        [SerializeField] private Color _riskColorGamble = new Color(0.40f, 0.32f, 0.10f, 1f);     // 어두운 노랑
        [SerializeField] private Color _riskColorDanger = new Color(0.42f, 0.16f, 0.16f, 1f);     // 어두운 빨강
        [SerializeField] private Color _disabledColor = new Color(0.12f, 0.12f, 0.12f, 1f);       // 비활성 회색

        private EventData _currentEvent;
        private EventManager _eventManager;
        private GameRunState _runState;
        private System.Action _onEventComplete;

        private void Awake()
        {
            if (_resultConfirmButton != null)
                _resultConfirmButton.onClick.AddListener(OnResultConfirmed);
        }

        public void Initialize(GameRunState runState, System.Action onEventComplete)
        {
            _runState = runState;
            _onEventComplete = onEventComplete;
            _eventManager = new EventManager();
        }

        /// <summary>
        /// 이벤트 화면 표시
        /// </summary>
        public void ShowEvent(EventData eventData)
        {
            _currentEvent = eventData;
            gameObject.SetActive(true);
            var cg = UIAnimationHelper.EnsureCanvasGroup(gameObject);
            cg.alpha = 0f;
            UIAnimationHelper.FadeIn(cg);

            if (_resultPanel != null)
                _resultPanel.SetActive(false);

            // 이벤트 정보 표시
            if (_eventTitleLabel != null)
                _eventTitleLabel.text = eventData.EventName;
            if (_eventDescLabel != null)
                _eventDescLabel.text = eventData.Description;

            // 선택지 버튼 생성
            ClearChoices();
            for (int i = 0; i < eventData.Choices.Count; i++)
            {
                if (_choiceButtonPrefab == null || _choiceContainer == null) continue;
                CreateChoiceButton(eventData.Choices[i], i);
            }
        }

        private void CreateChoiceButton(EventChoice choice, int index)
        {
            var choiceObj = Instantiate(_choiceButtonPrefab, _choiceContainer);

            // 자식 TMP 구성 — 첫 번째는 ChoiceText, 두 번째는 ChoiceDescription (있으면)
            var tmps = choiceObj.GetComponentsInChildren<TextMeshProUGUI>();
            var choiceText = tmps.Length > 0 ? tmps[0] : choiceObj.GetComponentInChildren<TextMeshProUGUI>();

            if (choiceText != null)
                choiceText.text = choice.ChoiceText;

            // ChoiceDescription 표시 (데이터에 있을 경우)
            if (!string.IsNullOrEmpty(choice.ChoiceDescription))
            {
                TextMeshProUGUI descLabel;
                if (tmps.Length > 1)
                {
                    descLabel = tmps[1];
                }
                else
                {
                    // 두 번째 TMP가 없으면 동적으로 자식으로 추가
                    descLabel = CreateDescriptionLabel(choiceObj, choiceText);
                }
                if (descLabel != null)
                {
                    descLabel.text = choice.ChoiceDescription;
                    descLabel.gameObject.SetActive(true);
                }
            }

            // 버튼 설정
            var button = choiceObj.GetComponent<Button>();
            var buttonImage = choiceObj.GetComponent<Image>();
            if (button == null) return;

            // 위험도 색상 적용
            var risk = choice.Outcome?.GetRiskLevel() ?? EventRiskLevel.Normal;
            if (buttonImage != null)
                buttonImage.color = GetRiskColor(risk);

            // 조건부 선택지 비활성화
            bool canChoose = _eventManager.CanChoose(choice, _runState);
            button.interactable = canChoose;
            if (!canChoose && buttonImage != null)
                buttonImage.color = _disabledColor;

            // 비활성화 사유 툴팁 (간단한 텍스트 부가)
            if (!canChoose && choiceText != null)
            {
                string reason = GetDisabledReason(choice, _runState);
                if (!string.IsNullOrEmpty(reason))
                    choiceText.text += $" ({reason})";
            }

            if (canChoose)
            {
                int capturedIndex = index;
                button.onClick.AddListener(() => OnChoiceSelected(capturedIndex));
            }
        }

        private TextMeshProUGUI CreateDescriptionLabel(GameObject parentObj, TextMeshProUGUI referenceTmp)
        {
            // 부모 버튼 하위에 작은 설명 텍스트를 추가
            var descGo = new GameObject("ChoiceDescription", typeof(RectTransform));
            descGo.transform.SetParent(parentObj.transform, false);
            return SetupDescriptionLabel(descGo, referenceTmp);
        }

        private TextMeshProUGUI SetupDescriptionLabel(GameObject descGo, TextMeshProUGUI referenceTmp)
        {
            var rect = descGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0.4f);
            rect.offsetMin = new Vector2(10f, 4f);
            rect.offsetMax = new Vector2(-10f, -4f);

            var tmp = descGo.AddComponent<TextMeshProUGUI>();
            if (referenceTmp != null)
            {
                tmp.font = referenceTmp.font;
                tmp.fontSize = referenceTmp.fontSize - 4;
                tmp.alignment = TextAlignmentOptions.Left;
            }
            tmp.color = new Color(0.75f, 0.75f, 0.78f, 1f); // 약간 어두운 회색
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private Color GetRiskColor(EventRiskLevel risk)
        {
            switch (risk)
            {
                case EventRiskLevel.Safe: return _riskColorSafe;
                case EventRiskLevel.Gamble: return _riskColorGamble;
                case EventRiskLevel.Dangerous: return _riskColorDanger;
                default: return _riskColorNormal;
            }
        }

        private string GetDisabledReason(EventChoice choice, GameRunState runState)
        {
            if (choice.MinGoldRequired > 0 && runState.Gold < choice.MinGoldRequired)
                return $"골드 {choice.MinGoldRequired} 필요";
            if (choice.MinPartyHPPercent > 0f)
            {
                float avgHp = GetAveragePartyHPRatio(runState);
                if (avgHp < choice.MinPartyHPPercent)
                    return $"파티 HP {choice.MinPartyHPPercent * 100f:F0}% 이상 필요";
            }
            if (choice.RequiresAliveMembers > 0)
            {
                int alive = 0;
                foreach (var m in runState.PlayerParty)
                    if (m.IsAlive) alive++;
                if (alive < choice.RequiresAliveMembers)
                    return $"생존자 {choice.RequiresAliveMembers}명 이상 필요";
            }
            return "";
        }

        private float GetAveragePartyHPRatio(GameRunState runState)
        {
            if (runState == null || runState.PlayerParty == null) return 0f;
            int alive = 0;
            int totalRatio = 0;
            foreach (var m in runState.PlayerParty)
            {
                if (!m.IsAlive) continue;
                alive++;
                if (m.Health.MaxHP > 0)
                    totalRatio += m.Health.CurrentHP * 100 / m.Health.MaxHP;
            }
            return alive == 0 ? 0f : totalRatio / (alive * 100f);
        }

        private void OnChoiceSelected(int choiceIndex)
        {
            var outcome = _eventManager.ProcessChoice(_currentEvent, choiceIndex, _runState);
            if (outcome == null) return;

            AudioManager.Instance.PlayUIClick();

            // 연쇄 이벤트: NextEventId가 있으면 결과 확인 후 다음 이벤트 로드
            bool hasChain = !string.IsNullOrEmpty(outcome.NextEventId);

            // 선택지 숨기고 결과 표시
            ClearChoices();

            if (_resultPanel != null)
                _resultPanel.SetActive(true);

            if (_resultLabel != null)
                _resultLabel.text = outcome.ResultText;

            // 연쇄 이벤트 처리: 확인 클릭 시 다음 이벤트 표시
            if (hasChain && _resultConfirmButton != null)
            {
                // 기존 리스너 제거 후 연쇄 전용 리스너 추가
                _resultConfirmButton.onClick.RemoveAllListeners();
                string nextId = outcome.NextEventId;
                _resultConfirmButton.onClick.AddListener(() => OnContinueToNextEvent(nextId));
            }
        }

        /// <summary>
        /// 연쇄 이벤트 — 다음 이벤트 로드
        /// </summary>
        private void OnContinueToNextEvent(string nextEventId)
        {
            AudioManager.Instance.PlayUIConfirm();
            var nextEvent = FindEventById(nextEventId);
            if (nextEvent != null)
            {
                // 확인 버튼 리스너 복원
                if (_resultConfirmButton != null)
                {
                    _resultConfirmButton.onClick.RemoveAllListeners();
                    _resultConfirmButton.onClick.AddListener(OnResultConfirmed);
                }
                ShowEvent(nextEvent);
            }
            else
            {
                // 찾지 못하면 일반 종료
                if (_resultConfirmButton != null)
                {
                    _resultConfirmButton.onClick.RemoveAllListeners();
                    _resultConfirmButton.onClick.AddListener(OnResultConfirmed);
                }
                HideAndNotify();
            }
        }

        /// <summary>
        /// ID로 EventData 검색 — 런타임 풀에서 찾거나 Resources에서 로드
        /// </summary>
        private EventData FindEventById(string eventId)
        {
            // 1) MapSceneSetup에 설정된 _allEvents에서 검색 (외부 주입 필요)
            // 2) 폴백: Resources 로드
            var found = Resources.Load<EventData>($"Events/{eventId}");
            return found;
        }

        private void OnResultConfirmed()
        {
            AudioManager.Instance.PlayUIConfirm();
            HideAndNotify();
        }

        private void HideAndNotify()
        {
            _onEventComplete?.Invoke(); // FadeOut이 SetActive(false)하므로 콜백을 먼저 실행
            var cg = UIAnimationHelper.EnsureCanvasGroup(gameObject);
            UIAnimationHelper.FadeOut(cg);
        }

        private void ClearChoices()
        {
            if (_choiceContainer == null) return;
            for (int i = _choiceContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_choiceContainer.GetChild(i).gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_resultConfirmButton != null)
                _resultConfirmButton.onClick.RemoveListener(OnResultConfirmed);
        }
    }
}
