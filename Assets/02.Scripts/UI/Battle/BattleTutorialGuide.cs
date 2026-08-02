using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TeamLog.Map;
using TeamLog.UI;  // UIKoreanFont

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// ★ 2026-08-02 P0-3 (옵션 C 하이브리드): 전투 튜토리얼 3단계 인액티브 가이드.
    ///
    /// 목적: 새 플레이어가 첫 전투에서 슬롯 선택 → 타겟 지정 → 턴 종료 흐름을 이해.
    /// 방식: 하단 작은 안내 카드 + "다음"/"스킵" 버튼.
    ///
    /// ★ 2026-08-02 리뷰 반영 (P0-2, P1-1):
    ///   - 전체 화면 오버레이 제거 → Phase DIR 연출(턴 배너/슬롯 등장) 가시성 보장
    ///   - 카드만 하단에 표시, raycastTarget=true는 카드 영역에만 적용
    ///   - 게임 플레이/클릭은 카드 외 영역에서 정상 작동
    ///
    /// 한계 (옵션 C 하이브리드):
    ///   - 실제 클릭을 감지하지 않음 (이벤트 구독은 추후 강화 가능)
    ///   - "다음" 버튼으로 단계 진행
    ///   - 매 런 첫 전투(BattlesWon==0 && Floor==1)에서만 활성화
    ///     (BattleTestScene _useTestData=true일 때는 스킵)
    /// </summary>
    public class BattleTutorialGuide : MonoBehaviour
    {
        private static readonly string[] StepTitles =
        {
            "1단계: 스킬 선택",
            "2단계: 타겟 지정",
            "3단계: 턴 종료"
        };

        private static readonly string[] StepDescs =
        {
            "하단 액션 슬롯을 클릭해 스킬을 선택하세요. AP(행동력)가 필요합니다.",
            "단일 적 스킬은 적을 직접 클릭해 타겟을 지정합니다.",
            "행동이 끝나면 '턴 종료' 버튼을 눌러 적 턴으로 넘기세요."
        };

        private int _step = 0;
        private GameObject _card;
        private TextMeshProUGUI _titleTmp;
        private TextMeshProUGUI _descTmp;
        private Button _nextButton;
        private TextMeshProUGUI _nextLabel;

        /// <summary>
        /// 첫 전투에서만 가이드 활성화.
        /// BattleSceneSetup.InitializeBattle 끝에서 호출 권장 (_useTestData=false일 때만).
        /// </summary>
        public static void TryActivate(RectTransform parentCanvas)
        {
            if (parentCanvas == null) return;

            var runState = GameRunState.Instance;
            if (runState == null) return;
            // 첫 전투에서만 (F1 + 승리 없음)
            if (runState.BattlesWon > 0) return;
            if (runState.CurrentFloor != 1) return;

            // 이미 가이드가 있으면 중복 생성 방지
            if (parentCanvas.GetComponentInChildren<BattleTutorialGuide>(true) != null) return;

            var go = new GameObject("BattleTutorialGuide");
            go.transform.SetParent(parentCanvas, false);
            var guide = go.AddComponent<BattleTutorialGuide>();
            guide.BuildUI(parentCanvas);
            guide.ShowStep(0);

            Debug.Log("[BattleTutorialGuide] 첫 전투 감지 — 튜토리얼 가이드 활성화");
        }

        private void BuildUI(RectTransform parent)
        {
            // ★ 하단 작은 카드만 (전체 화면 오버레이 제거 — P0-2/P1-1 리뷰 반영)
            // 화면 중앙 하단에 안내 카드 배치. 게임 플레이/연출 방해 최소화.
            _card = new GameObject("TutorialCard");
            _card.transform.SetParent(parent, false);
            var cardRt = _card.AddComponent<RectTransform>();
            cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.18f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(520f, 150f);
            cardRt.anchoredPosition = Vector2.zero;

            var cardImg = _card.AddComponent<Image>();
            cardImg.color = new Color(0.05f, 0.05f, 0.1f, 0.94f);
            cardImg.raycastTarget = true;  // 카드 영역만 클릭 차단
            var outline = _card.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.25f, 0.95f);
            outline.effectDistance = new Vector2(3f, -3f);

            var vlg = _card.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6f;
            vlg.padding = new RectOffset(20, 20, 14, 14);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 제목 (명시적 preferredHeight — P1-2 리뷰 반영)
            _titleTmp = CreateText(_card.transform, "Title", 20, new Color(0.98f, 0.92f, 0.55f), 28f);
            // 설명
            _descTmp = CreateText(_card.transform, "Description", 14, Color.white, 40f);

            // 버튼 행 (명시적 preferredHeight)
            var btnRow = new GameObject("ButtonRow");
            btnRow.transform.SetParent(_card.transform, false);
            var btnHlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            btnHlg.spacing = 10f;
            btnHlg.childAlignment = TextAnchor.MiddleCenter;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = false;
            btnHlg.childForceExpandHeight = false;
            var btnRowLe = btnRow.AddComponent<LayoutElement>();
            btnRowLe.preferredHeight = 32f;  // ★ 명시적 높이 (P1-2)

            var skipBtn = CreateButton(btnRow.transform, "스킵", new Color(0.3f, 0.3f, 0.35f), 100f);
            skipBtn.onClick.AddListener(OnSkip);

            _nextButton = CreateButton(btnRow.transform, "다음", new Color(0.4f, 0.5f, 0.2f), 130f);
            _nextButton.onClick.AddListener(OnNext);
            _nextLabel = _nextButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, int size, Color color, float preferredHeight)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.richText = true;
            UIKoreanFont.EnsureFont(tmp);

            // ★ 항상 명시적 preferredHeight 부여 (P1-2 리뷰 반영 — VLG가 자동 preferredSize 보고에 의존 않도록)
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = preferredHeight;
            return tmp;
        }

        private Button CreateButton(Transform parent, string label, Color color, float width)
        {
            var go = new GameObject(label + "_Btn");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = 32f;

            // 라벨 (버튼 자식)
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.AddComponent<RectTransform>();
            labelRt.anchorMin = labelRt.anchorMax = new Vector2(0.5f, 0.5f);
            labelRt.pivot = new Vector2(0.5f, 0.5f);
            labelRt.sizeDelta = new Vector2(width, 32f);
            labelRt.anchoredPosition = Vector2.zero;
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.fontSize = 15;
            labelTmp.color = Color.white;
            UIKoreanFont.EnsureFont(labelTmp);

            return btn;
        }

        private void ShowStep(int idx)
        {
            if (idx < 0 || idx >= StepTitles.Length)
            {
                CloseGuide();
                return;
            }
            _step = idx;
            if (_titleTmp != null) _titleTmp.text = StepTitles[idx];
            if (_descTmp != null) _descTmp.text = StepDescs[idx];
            if (_nextLabel != null)
                _nextLabel.text = (idx == StepTitles.Length - 1) ? "완료" : "다음";
        }

        private void OnNext()
        {
            AudioManager.Instance?.PlayUIConfirm();
            ShowStep(_step + 1);
        }

        private void OnSkip()
        {
            AudioManager.Instance?.PlayUICancel();
            CloseGuide();
        }

        private void CloseGuide()
        {
            if (_card != null) Destroy(_card);
            Destroy(gameObject);
        }
    }
}
