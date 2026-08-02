using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using TeamLog.UI;
using TeamLog.Map;
using DG.Tweening;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 전투 종료 오버레이 — 승리/패배 대형 텍스트 + 계속하기 버튼
    /// ★ 2026-08-02 P2-4: 승리 시 누적 보상 통계 표시 (유물/증강/골드)
    /// </summary>
    public class BattleEndOverlay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TextMeshProUGUI _continueLabel;
        private CanvasGroup _containerCanvasGroup;
        private RectTransform _container;
        private TextMeshProUGUI _statsText;  // ★ P2-4 lazy init

        public event Action OnContinueClicked;

        private void Awake()
        {
            if (_continueButton != null)
                _continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());

            // 주의: 여기서 gameObject.SetActive(false) 호출 금지
            // 씬 빌더가 이미 비활성 상태로 저장함.
            // 런타임에 Show()로 활성화 시 Awake()가 호출되는데,
            // 여기서 다시 비활성화하면 오버레이가 보이지 않음.
        }

        public void Show(bool victory)
        {
            if (_resultText != null)
            {
                _resultText.text = victory ? "승리!" : "패배...";
                _resultText.color = victory
                    ? new Color(0.96f, 0.82f, 0.25f)
                    : new Color(0.85f, 0.2f, 0.2f);
            }

            if (_continueLabel != null)
                _continueLabel.text = "계속하기";

            // ★ 2026-08-02 P2-4: 승리 시 누적 보상 통계 표시
            UpdateRewardStats(victory);

            gameObject.SetActive(true);

            // 사운드
            if (victory)
                AudioManager.Instance.PlayVictory();
            else
                AudioManager.Instance.PlayDefeat();

            // 컨테이너에 애니메이션 적용
            if (_container == null)
                _container = transform.Find("Container") as RectTransform;
            if (_container != null)
            {
                _containerCanvasGroup = UIAnimationHelper.EnsureCanvasGroup(_container.gameObject);
                var s = DOTween.Sequence().SetUpdate(true);
                s.Append(UIAnimationHelper.ScaleFromZero(_container, 0.4f));
                if (_containerCanvasGroup != null)
                    s.Append(UIAnimationHelper.FadeIn(_containerCanvasGroup, 0.3f));
            }
        }

        /// <summary>★ P2-4: 승리 시 누적 보상 통계를 resultText 아래에 표시.</summary>
        private void UpdateRewardStats(bool victory)
        {
            var stats = EnsureStatsText();
            if (stats == null) return;

            if (!victory)
            {
                stats.text = "";
                return;
            }

            var runState = GameRunState.Instance;
            if (runState == null)
            {
                stats.text = "";
                return;
            }

            int relics = runState.RelicHandler?.Relics.Count ?? 0;
            int augments = 0;
            foreach (var c in runState.PlayerParty)
            {
                if (c == null) continue;
                foreach (var inst in c.SkillInventory.SkillInstances)
                    augments += inst.Augments.Count;
            }
            stats.text = $"획득 유물 {relics}개 | 증강 {augments}개 | 골드 {runState.Gold}";
        }

        /// <summary>★ P2-4: 인스펙터 바인딩 없이 lazy init으로 stats 텍스트 생성.</summary>
        private TextMeshProUGUI EnsureStatsText()
        {
            if (_statsText != null) return _statsText;

            if (_container == null)
                _container = transform.Find("Container") as RectTransform;
            var parent = _container != null ? _container : transform;

            var go = new GameObject("RewardStatsText");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.32f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(500, 36);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.95f, 0.92f, 0.80f);
            UIKoreanFont.EnsureFont(tmp);
            _statsText = tmp;
            return _statsText;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
