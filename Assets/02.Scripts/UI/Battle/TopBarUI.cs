using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI;
using TeamLog.Combat;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 상단 바 UI (턴 카운터, 턴 종료 버튼, 속도 토글)
    /// </summary>
    public class TopBarUI : MonoBehaviour
    {
        [Header("AP Display")]
        [SerializeField] private TextMeshProUGUI _apText;
        [SerializeField] private Image _apFillImage;

        [Header("Speed Toggle")]
        [SerializeField] private Button _speedToggleButton;
        [SerializeField] private TextMeshProUGUI _speedLabel;

        private BattleSceneSetup _battleSetup;

        private static Color APNormalColor => UIPalette.Default.APNormal;
        private static Color APShortageColor => UIPalette.Default.APShortage;

        private void Awake()
        {
            if (_speedToggleButton != null)
                _speedToggleButton.onClick.AddListener(OnSpeedToggleClicked);

            // APFill이 명시적으로 연결되지 않은 경우 BottomBar에서 검색
            if (_apFillImage == null)
            {
                var bottomBar = transform.parent?.Find("BottomBar");
                if (bottomBar != null)
                {
                    var fill = bottomBar.Find("APBar/APFill");
                    if (fill != null) _apFillImage = fill.GetComponent<Image>();
                }
            }
        }

        public void Initialize(BattleSceneSetup battleSetup)
        {
            _battleSetup = battleSetup;
            if (_battleSetup != null)
                _battleSetup.OnBattleSpeedChanged += OnSpeedChanged;
            UpdateSpeedLabel(_battleSetup?.CurrentBattleSpeed ?? BattleSceneSetup.BattleSpeed.Normal);
        }

        private void OnSpeedToggleClicked()
        {
            _battleSetup?.ToggleBattleSpeed();
        }

        private void OnSpeedChanged(BattleSceneSetup.BattleSpeed speed)
        {
            UpdateSpeedLabel(speed);
        }

        private void UpdateSpeedLabel(BattleSceneSetup.BattleSpeed speed)
        {
            if (_speedLabel != null)
                _speedLabel.text = $"{(int)speed}x";
        }

        public void SetAP(int current, int max)
        {
            if (_apText == null) return;
            _apText.text = $"AP {current}/{max}";
            _apText.color = current == 0 ? APShortageColor : APNormalColor;

            if (_apFillImage != null)
            {
                float ratio = max > 0 ? (float)current / max : 0f;
                _apFillImage.rectTransform.anchorMax = new Vector2(ratio, 1f);
            }
        }

        private void OnDestroy()
        {
            if (_speedToggleButton != null)
                _speedToggleButton.onClick.RemoveListener(OnSpeedToggleClicked);
            if (_battleSetup != null)
                _battleSetup.OnBattleSpeedChanged -= OnSpeedChanged;
        }
    }
}
