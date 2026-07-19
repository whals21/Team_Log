using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Event;
using TeamLog.Map;
using TeamLog.UI;

namespace TeamLog.UI.Event
{
    /// <summary>
    /// ★ Stained Glass Event UI — 선택지 1개 행.
    /// EventReworkView가 EventData.Choices[i]마다 인스턴스화.
    ///
    /// 자식 구조 (Builder가 생성):
    ///   ChoiceRow (Image 배경 + Button + HLG)
    ///   ├─ ChoiceText     (TMP, 메인 선택지 텍스트)
    ///   ├─ ChoiceDesc     (TMP, 작은 설명)
    ///   ├─ RiskTag        (TMP + Image 배경, 우측 상단 — RiskLevel 이모티콘+라벨)
    ///   └─ DisabledReason (TMP, 비활성 시 사유 — 비활성이면 노출, 활성이면 숨김)
    ///
    /// CLAUDE.md 가드레일 #17 준수 — 단일 MonoBehaviour.
    /// </summary>
    public class EventChoiceRowRework : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _choiceText;
        [SerializeField] private TextMeshProUGUI _choiceDesc;
        [SerializeField] private TextMeshProUGUI _riskTag;
        [SerializeField] private Image _riskTagBackground;
        [SerializeField] private GameObject _disabledReasonGo;
        [SerializeField] private TextMeshProUGUI _disabledReasonText;

        private bool _autoBound;

        private void Awake()
        {
            AutoBindMissingFields();
        }

        private void AutoBindMissingFields()
        {
            if (_autoBound) return;

            if (_background == null)
                _background = GetComponent<Image>();
            if (_button == null)
                _button = GetComponent<Button>();
            if (_choiceText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "ChoiceText");
                if (go != null) _choiceText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_choiceDesc == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "ChoiceDesc");
                if (go != null) _choiceDesc = go.GetComponent<TextMeshProUGUI>();
            }
            // ★ RiskTag GameObject는 Image를 가지고, 자식 "Label"이 TMP를 가짐
            // (CLAUDE.md 가드레일 #24 — GetComponentInChildren로 자손 검색)
            if (_riskTag == null || _riskTagBackground == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "RiskTag");
                if (go != null)
                {
                    if (_riskTagBackground == null) _riskTagBackground = go.GetComponent<Image>();
                    if (_riskTag == null) _riskTag = go.GetComponentInChildren<TextMeshProUGUI>(true);
                }
            }
            if (_disabledReasonGo == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "DisabledReason");
                if (go != null) _disabledReasonGo = go;
            }
            if (_disabledReasonText == null && _disabledReasonGo != null)
            {
                _disabledReasonText = _disabledReasonGo.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            _autoBound = true;
        }

        /// <summary>
        /// 선택지 데이터 바인딩.
        /// </summary>
        /// <param name="choice">이벤트 선택지 데이터</param>
        /// <param name="canChoose">선택 가능 여부 (EventManager.CanChoose 결과)</param>
        /// <param name="disabledReason">비활성 사유 텍스트 (canChoose=false일 때만 사용)</param>
        /// <param name="onClick">활성화 상태일 때 버튼 클릭 콜백</param>
        public void SetData(EventChoice choice, bool canChoose, string disabledReason, System.Action onClick)
        {
            AutoBindMissingFields();
            if (choice == null) return;

            var outcome = choice.Outcome;
            var risk = outcome != null ? outcome.GetRiskLevel() : EventRiskLevel.Normal;
            var riskVisual = EventRiskStyle.Get(risk);

            // 메인 텍스트
            if (_choiceText != null)
            {
                _choiceText.text = choice.ChoiceText;
                _choiceText.color = canChoose ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            // 설명
            if (_choiceDesc != null)
            {
                bool hasDesc = !string.IsNullOrEmpty(choice.ChoiceDescription);
                _choiceDesc.text = hasDesc ? choice.ChoiceDescription : "";
                _choiceDesc.gameObject.SetActive(hasDesc);
            }

            // RiskTag — 이모티콘 + 라벨
            if (_riskTag != null)
            {
                _riskTag.text = $"{riskVisual.EmblemSymbol}  {riskVisual.DisplayName}";
                _riskTag.color = canChoose ? riskVisual.TextColor : new Color(0.5f, 0.5f, 0.5f, 0.7f);
            }
            if (_riskTagBackground != null)
            {
                Color bg = riskVisual.BorderColor;
                bg.a = canChoose ? 0.25f : 0.1f;
                _riskTagBackground.color = bg;
            }

            // 비활성 사유
            if (_disabledReasonGo != null)
            {
                bool showReason = !canChoose && !string.IsNullOrEmpty(disabledReason);
                _disabledReasonGo.SetActive(showReason);
                if (showReason && _disabledReasonText != null)
                    _disabledReasonText.text = disabledReason;
            }

            // ★ Pulse 활성화 — Gamble/Dangerous만 (canChoose일 때만)
            _pulseActive = canChoose && riskVisual.Pulse;

            // 배경 — RiskLevel 색상 적용 (활성/비활성)
            if (_background != null)
            {
                if (canChoose)
                {
                    // 어두운 배경 + RiskLevel 테두리 느낌
                    Color bg = new Color(0.05f, 0.05f, 0.08f, 0.85f);
                    _background.color = bg;
                    _background.sprite = null; // Builder가 테두리는 자식 Image로 별도 제공
                }
                else
                {
                    _background.color = new Color(0.05f, 0.05f, 0.05f, 0.6f);
                }
            }

            // 버튼 클릭 리스너
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.interactable = canChoose;
                if (canChoose && onClick != null)
                    _button.onClick.AddListener(() => onClick.Invoke());
            }
        }

        /// <summary>
        /// ★ Pulse/Shake 애니메이션 — EventReworkView.Update()가 매 프레임 호출.
        /// phase는 0~1 (1Hz). RiskTag 배경 alpha를 phase 기반으로 변동.
        /// </summary>
        public void ApplyPulseVisual(float phase)
        {
            if (!_pulseActive || _riskTagBackground == null) return;

            // Pulse: alpha 0.15 ~ 0.5 범위에서 맥동
            float alpha = Mathf.Lerp(0.15f, 0.5f, phase);
            var c = _riskTagBackground.color;
            c.a = alpha;
            _riskTagBackground.color = c;
        }

        private bool _pulseActive;
    }
}
