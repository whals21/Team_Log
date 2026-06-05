using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using TeamLog.UI;
using DG.Tweening;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 전투 종료 오버레이 — 승리/패배 대형 텍스트 + 계속하기 버튼
    /// </summary>
    public class BattleEndOverlay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TextMeshProUGUI _continueLabel;
        private CanvasGroup _containerCanvasGroup;
        private RectTransform _container;

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

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
