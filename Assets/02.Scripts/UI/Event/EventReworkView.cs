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
    /// ★ Stained Glass Event UI — 메인 View 컴포넌트.
    /// 기존 EventUI 로직(ShowEvent / OnChoiceSelected / 연쇄 이벤트)을 그대로 유지하되,
    /// 시각적 구조를 "스테인드글라스 아치 + 하단 글라스 패널"로 재설계.
    ///
    /// 자식 구조 (EventSceneReworkBuilder가 생성):
    /// - EventReworkView (CanvasGroup + Dim 배경)
    /// - DimBackground (전체 화면 어둠)
    /// - GlassFrame (중앙 카드)
    ///   - GlassWindow (상단 — Image, EventType별 Sprite)
    ///     - Emblem (TMP, 중앙 엠블럼 기호)
    ///   - GlassPanel (하단 — Image, 어두운 패널)
    ///     - TopBar (HLG: ThemeTag + EventTypeTag)
    ///     - EventTitle (TMP, 메인 타이틀)
    ///     - Narrative (TMP, 이야기 묘사)
    ///     - ChoiceContainer (VLG — 선택지 행들이 인스턴스화)
    ///     - ResultPanel (초기 비활성 — ResultText + ResultConfirmButton)
    ///   - CloseButton (우측 상단 X 버튼 — 선택적 스킵)
    ///
    /// ★ 기존 EventUI와의 호환성: Initialize(GameRunState, Action) + ShowEvent(EventData) 동일.
    /// </summary>
    public class EventReworkView : MonoBehaviour
    {
        [Header("Refs — Glass Window (상단 스테인드글라스)")]
        [SerializeField] private Image _glassWindowImage;       // EventType별 Sprite
        [SerializeField] private TextMeshProUGUI _emblemText;   // 중앙 엠블럼 기호

        [Header("Refs — Glass Panel (하단 텍스트 영역)")]
        [SerializeField] private Image _glassPanelImage;
        [SerializeField] private TextMeshProUGUI _themeTag;       // 좌측 상단 — 현재 테마/층
        [SerializeField] private TextMeshProUGUI _eventTypeTag;   // 우측 상단 — EventType 라벨
        [SerializeField] private TextMeshProUGUI _eventTitle;     // 메인 타이틀
        [SerializeField] private TextMeshProUGUI _narrative;      // 이야기 묘사
        [SerializeField] private Transform _choiceContainer;      // 선택지 행 부모

        [Header("Refs — Result Panel")]
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _resultConfirmButton;
        [SerializeField] private TextMeshProUGUI _resultConfirmLabel;

        [Header("Refs — Choice Prefab")]
        [SerializeField] private GameObject _choiceRowPrefab;

        [Header("Refs — Close Button (선택적 스킵)")]
        [SerializeField] private Button _closeButton;

        [Header("Glass Window Sprite DB (EventType별)")]
        [SerializeField] private Sprite _glassWindowStory;
        [SerializeField] private Sprite _glassWindowTreasure;
        [SerializeField] private Sprite _glassWindowTrap;
        [SerializeField] private Sprite _glassWindowNPC;
        [SerializeField] private Sprite _glassWindowShrine;

        [Header("Panel Background")]
        [SerializeField] private Sprite _panelBackgroundSprite;

        // 컴포넌트 캐시
        private CanvasGroup _canvasGroup;

        // 상태
        private EventData _currentEvent;
        private EventManager _eventManager;
        private GameRunState _runState;
        private System.Action _onEventComplete;
        private List<EventChoiceRowRework> _spawnedRows = new();

        // Pulse 애니메이션 (Gamble/Dangerous 선택지용)
        private float _pulseTimer;

        [Header("Debug — All Events Pool (연쇄 이벤트용)")]
        [SerializeField] private List<EventData> _allEvents = new();

        private void Update()
        {
            // ★ Pulse/Shake 애니메이션 — _spawnedRows의 RiskTag에 적용.
            // Gamble: 1Hz 맥동 (alpha 펄스). Dangerous: 추가로 미세 shake.
            if (_spawnedRows == null || _spawnedRows.Count == 0) return;
            _pulseTimer += Time.deltaTime;
            float pulsePhase = (Mathf.Sin(_pulseTimer * Mathf.PI * 2f) + 1f) * 0.5f; // 0~1, 1Hz

            foreach (var row in _spawnedRows)
            {
                if (row == null) continue;
                row.ApplyPulseVisual(pulsePhase);
            }
        }

        /// <summary>
        /// 외부에서 연쇄 이벤트 검색용 EventData 풀 주입 (Resources.Load 의존성 제거).
        /// </summary>
        public void SetEventPool(List<EventData> events)
        {
            _allEvents = events ?? new();
        }

        private void Awake()
        {
            AutoBindMissingFields();
            EnsureCanvasGroup();

            if (_resultConfirmButton != null)
                _resultConfirmButton.onClick.AddListener(OnResultConfirmed);
            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnCloseButtonClicked);

            // ★ CLAUDE.md 가드레일 #2 — Awake에서 SetActive(false) 금지.
            // 대신 CanvasGroup으로 보이지 않게 처리 (GameObject는 활성 유지).
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            // 자식 ResultPanel은 비활성화 (자식이라 허용)
            if (_resultPanel != null)
                _resultPanel.SetActive(false);
        }

        private void EnsureCanvasGroup()
        {
            if (_canvasGroup == null)
                _canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void AutoBindMissingFields()
        {
            var root = transform;

            if (_glassWindowImage == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "GlassWindow");
                if (go != null) _glassWindowImage = go.GetComponent<Image>();
            }
            if (_emblemText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "Emblem");
                if (go != null) _emblemText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_glassPanelImage == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "GlassPanel");
                if (go != null) _glassPanelImage = go.GetComponent<Image>();
            }
            if (_themeTag == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "ThemeTag");
                if (go != null) _themeTag = go.GetComponent<TextMeshProUGUI>();
            }
            if (_eventTypeTag == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "EventTypeTag");
                if (go != null) _eventTypeTag = go.GetComponent<TextMeshProUGUI>();
            }
            if (_eventTitle == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "EventTitle");
                if (go != null) _eventTitle = go.GetComponent<TextMeshProUGUI>();
            }
            if (_narrative == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "Narrative");
                if (go != null) _narrative = go.GetComponent<TextMeshProUGUI>();
            }
            if (_choiceContainer == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "ChoiceContainer");
                if (go != null) _choiceContainer = go.transform;
            }
            if (_resultPanel == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "ResultPanel");
                if (go != null) _resultPanel = go;
            }
            if (_resultText == null && _resultPanel != null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(_resultPanel.transform, "ResultText");
                if (go != null) _resultText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_resultConfirmButton == null && _resultPanel != null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(_resultPanel.transform, "ResultConfirmButton");
                if (go != null) _resultConfirmButton = go.GetComponent<Button>();
            }
            if (_resultConfirmLabel == null && _resultConfirmButton != null)
            {
                _resultConfirmLabel = _resultConfirmButton.GetComponentInChildren<TextMeshProUGUI>(true);
            }
            if (_closeButton == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "CloseButton");
                if (go != null) _closeButton = go.GetComponent<Button>();
            }
        }

        /// <summary>
        /// 초기화 — 기존 EventUI와 동일한 시그니처 (호환성).
        /// </summary>
        public void Initialize(GameRunState runState, System.Action onEventComplete)
        {
            _runState = runState;
            _onEventComplete = onEventComplete;
            _eventManager = new EventManager();
        }

        /// <summary>
        /// 이벤트 화면 표시 — EventType별 스킨 자동 적용.
        /// </summary>
        public void ShowEvent(EventData eventData)
        {
            if (eventData == null) return;
            _currentEvent = eventData;

            EnsureCanvasGroup();
            // ★ CanvasGroup 활성화 (보이게 + 클릭 가능)
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            _canvasGroup.alpha = 0f;
            UIAnimationHelper.FadeIn(_canvasGroup);

            if (_resultPanel != null)
                _resultPanel.SetActive(false);

            ApplyEventSkin(eventData);

            // 텍스트
            if (_eventTitle != null)
                _eventTitle.text = eventData.EventName;
            if (_narrative != null)
                _narrative.text = eventData.Description;

            // 테마 태그 갱신
            UpdateThemeTag();

            // 선택지 생성
            ClearChoices();
            for (int i = 0; i < eventData.Choices.Count; i++)
            {
                CreateChoiceRow(eventData.Choices[i], i);
            }

            Debug.Log($"[EventReworkView] ShowEvent — '{eventData.EventName}' type:{eventData.Type} choices:{eventData.Choices.Count}");
        }

        /// <summary>
        /// EventType 스킨 적용 — GlassWindow Sprite + 엠블럼 + EventTypeTag.
        /// </summary>
        private void ApplyEventSkin(EventData eventData)
        {
            var skin = EventTypeSkinDatabase.Get(eventData.Type);

            // GlassWindow Sprite — EventType별
            if (_glassWindowImage != null)
            {
                var sprite = GetGlassSpriteForType(eventData.Type);
                if (sprite != null)
                {
                    _glassWindowImage.sprite = sprite;
                    _glassWindowImage.color = Color.white;
                    _glassWindowImage.enabled = true;
                }
                else
                {
                    // ★ Sprite가 null이면 Image를 비활성화 (분홍/검은 사각형 방지 — CLAUDE.md #16)
                    Debug.LogWarning($"[EventReworkView] GlassWindow Sprite for {eventData.Type} 없음");
                    _glassWindowImage.enabled = false;
                }
            }

            // 엠블럼 기호
            if (_emblemText != null)
            {
                _emblemText.text = skin.EmblemSymbol;
                _emblemText.color = skin.GlowColor;
                _emblemText.gameObject.SetActive(true);
            }

            // EventTypeTag — "✦ SHRINE" 형식
            if (_eventTypeTag != null)
            {
                _eventTypeTag.text = $"{skin.EmblemSymbol}  {skin.DisplayName}";
                _eventTypeTag.color = skin.GlowColor;
            }

            // 타이틀 색상 — GlowColor로 강조
            if (_eventTitle != null)
                _eventTitle.color = skin.GlowColor;
        }

        private Sprite GetGlassSpriteForType(TeamLog.Event.EventType type)
        {
            return type switch
            {
                TeamLog.Event.EventType.Story    => _glassWindowStory,
                TeamLog.Event.EventType.Treasure => _glassWindowTreasure,
                TeamLog.Event.EventType.Trap     => _glassWindowTrap,
                TeamLog.Event.EventType.NPC      => _glassWindowNPC,
                TeamLog.Event.EventType.Shrine   => _glassWindowShrine,
                _ => _glassWindowShrine
            };
        }

        private void UpdateThemeTag()
        {
            if (_themeTag == null) return;
            string themeName = _runState?.CurrentStageTheme?.displayName ?? "Unknown Path";
            int layer = _runState?.CurrentMap?.CurrentNode?.Layer ?? 0;
            _themeTag.text = $"— {themeName} · L{layer} —";
        }

        private void CreateChoiceRow(EventChoice choice, int index)
        {
            if (_choiceRowPrefab == null || _choiceContainer == null) return;

            var rowGo = Instantiate(_choiceRowPrefab, _choiceContainer);
            var row = rowGo.GetComponent<EventChoiceRowRework>();
            if (row == null) row = rowGo.AddComponent<EventChoiceRowRework>();

            bool canChoose = _eventManager != null && _eventManager.CanChoose(choice, _runState);
            string reason = canChoose ? "" : GetDisabledReason(choice);

            int capturedIndex = index;
            row.SetData(choice, canChoose, reason, () => OnChoiceSelected(capturedIndex));

            _spawnedRows.Add(row);
        }

        private string GetDisabledReason(EventChoice choice)
        {
            if (_runState == null) return "";
            if (choice.MinGoldRequired > 0 && _runState.Gold < choice.MinGoldRequired)
                return $"골드 {choice.MinGoldRequired} 필요";
            if (choice.MinPartyHPPercent > 0f)
            {
                float avg = GetAveragePartyHPRatio(_runState);
                if (avg < choice.MinPartyHPPercent)
                    return $"파티 HP {choice.MinPartyHPPercent * 100f:F0}% 이상 필요";
            }
            if (choice.RequiresAliveMembers > 0)
            {
                int alive = 0;
                foreach (var m in _runState.PlayerParty)
                    if (m.IsAlive) alive++;
                if (alive < choice.RequiresAliveMembers)
                    return $"생존자 {choice.RequiresAliveMembers}명 이상 필요";
            }
            return "선택 불가";
        }

        private float GetAveragePartyHPRatio(GameRunState runState)
        {
            if (runState == null || runState.PlayerParty == null) return 0f;
            int alive = 0, totalRatio = 0;
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
            if (_currentEvent == null || _eventManager == null) return;

            var outcome = _eventManager.ProcessChoice(_currentEvent, choiceIndex, _runState);
            if (outcome == null) return;

            AudioManager.Instance?.PlayUIClick();

            bool hasChain = !string.IsNullOrEmpty(outcome.NextEventId);

            ClearChoices();
            if (_resultPanel != null)
                _resultPanel.SetActive(true);
            if (_resultText != null)
                _resultText.text = outcome.ResultText;

            // 연쇄 이벤트 — 확인 버튼을 다음 이벤트 로드로 재사용
            if (hasChain && _resultConfirmButton != null)
            {
                _resultConfirmButton.onClick.RemoveAllListeners();
                string nextId = outcome.NextEventId;
                _resultConfirmButton.onClick.AddListener(() => OnContinueToNextEvent(nextId));

                if (_resultConfirmLabel != null)
                    _resultConfirmLabel.text = "계속";  // "Continue"
            }
            else
            {
                if (_resultConfirmLabel != null)
                    _resultConfirmLabel.text = "확인";  // "Confirm"
            }

            // 본문/타이틀은 결과 패널이 가리지 않도록 선택지 숨김 상태 유지
        }

        private void OnContinueToNextEvent(string nextEventId)
        {
            AudioManager.Instance?.PlayUIConfirm();
            var nextEvent = FindEventById(nextEventId);
            if (nextEvent != null)
            {
                if (_resultConfirmButton != null)
                {
                    _resultConfirmButton.onClick.RemoveAllListeners();
                    _resultConfirmButton.onClick.AddListener(OnResultConfirmed);
                }
                ShowEvent(nextEvent);
            }
            else
            {
                // ★ 다음 이벤트를 못 찾으면 일반 종료 + 라벨 리셋
                if (_resultConfirmButton != null)
                {
                    _resultConfirmButton.onClick.RemoveAllListeners();
                    _resultConfirmButton.onClick.AddListener(OnResultConfirmed);
                }
                if (_resultConfirmLabel != null)
                    _resultConfirmLabel.text = "확인";
                HideAndNotify();
            }
        }

        private EventData FindEventById(string eventId)
        {
            if (string.IsNullOrEmpty(eventId)) return null;
            // 1차: 런타임 주입된 풀에서 검색 (권장 — Builder가 AssetDatabase.FindAssets로 자동 주입)
            if (_allEvents != null)
            {
                foreach (var e in _allEvents)
                {
                    if (e != null && e.name == eventId) return e;
                }
            }
            // 2차: Resources/Events/{eventId} (레거시 호환)
            var found = Resources.Load<EventData>($"Events/{eventId}");
            return found;
        }

        private void OnResultConfirmed()
        {
            AudioManager.Instance?.PlayUIConfirm();
            HideAndNotify();
        }

        private void OnCloseButtonClicked()
        {
            // Close 버튼은 결과 패널이 떠 있을 때만 동작 (이벤트 진행 중 강제 닫기 금지)
            if (_resultPanel != null && _resultPanel.activeSelf)
                HideAndNotify();
        }

        /// <summary>
        /// ★ CLAUDE.md 함정 #1 — FadeOut 코루틴이 SetActive(false)하기 전에 콜백 먼저 실행.
        /// </summary>
        private void HideAndNotify()
        {
            _onEventComplete?.Invoke();
            EnsureCanvasGroup();
            UIAnimationHelper.FadeOut(_canvasGroup);
            // ★ FadeOut 이후 클릭 차단 (FadeIn이 다시 풀어줌)
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        private void ClearChoices()
        {
            if (_choiceContainer == null) return;
            for (int i = _choiceContainer.childCount - 1; i >= 0; i--)
            {
                var child = _choiceContainer.GetChild(i);
                if (child != null) Destroy(child.gameObject);
            }
            _spawnedRows.Clear();
        }

        private void OnDestroy()
        {
            if (_resultConfirmButton != null)
                _resultConfirmButton.onClick.RemoveListener(OnResultConfirmed);
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }
    }
}
