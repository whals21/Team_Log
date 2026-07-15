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
        [SerializeField] private Image[] _apPips;

        [Header("Turn / Floor Info")]
        [SerializeField] private TextMeshProUGUI _turnText;
        [SerializeField] private TextMeshProUGUI _floorInfoText;

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

            // AP 파이프 자동 검색 (BottomBar/RightColumn/APArea/PipRow/Pip1~5)
            if (_apPips == null || _apPips.Length == 0)
            {
                var bottomBar = transform.parent?.Find("BottomBar");
                if (bottomBar != null)
                {
                    var pipRow = bottomBar.Find("RightColumn/APArea/PipRow");
                    if (pipRow != null)
                    {
                        var pips = new System.Collections.Generic.List<Image>();
                        for (int i = 1; i <= 5; i++)
                        {
                            var pip = pipRow.Find($"Pip{i}");
                            if (pip != null) pips.Add(pip.GetComponent<Image>());
                        }
                        _apPips = pips.ToArray();
                    }
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
            if (_apText != null)
            {
                _apText.text = $"AP {current}/{max}";
                _apText.color = current == 0 ? APShortageColor : APNormalColor;
            }

            // 파이프 색상 업데이트 — 활성 AP 수만큼 파란색, 나머지는 어둡게
            if (_apPips != null)
            {
                Color activeColor = current > 0 ? APNormalColor : APShortageColor;
                Color inactiveColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);
                for (int i = 0; i < _apPips.Length; i++)
                {
                    if (_apPips[i] != null)
                        _apPips[i].color = i < current ? activeColor : inactiveColor;
                }
            }
        }

        public void SetTurn(int turnNumber)
        {
            if (_turnText != null)
                _turnText.text = $"Turn {turnNumber}";
        }

        public void SetFloorInfo(string info)
        {
            if (_floorInfoText != null)
                _floorInfoText.text = info ?? "";
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
