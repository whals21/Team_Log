using TMPro;
using UnityEngine;
using TeamLog.Map;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// 맵 씬 헤더 런타임 컨트롤러 (Phase UI-2 — 2026-07-19).
    /// Stage / Floor / Gold / Ascension 4종 칩 값을 GameRunState에서 읽어 갱신.
    /// 씬 빌더는 정적 텍스트만 세팅하므로, 런타임에 반드시 Refresh() 호출 필요.
    /// </summary>
    public class HeaderController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stageChipValue;
        [SerializeField] private TextMeshProUGUI _floorChipValue;
        [SerializeField] private TextMeshProUGUI _goldChipValue;
        [SerializeField] private TextMeshProUGUI _ascensionValue;

        private void Awake()
        {
            AutoBindMissingFields();
        }

        private void AutoBindMissingFields()
        {
            // StageChip_Value, FloorChip_Value, GoldChip_Value, AscValue 이름 자동 매핑
            // (MapSceneReworkBuilder.Parts.cs CreateHeaderChip/CreateHeaderAscension 규칙)
            if (_stageChipValue == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "StageChip_Value");
                if (go != null) _stageChipValue = go.GetComponent<TextMeshProUGUI>();
            }
            if (_floorChipValue == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "FloorChip_Value");
                if (go != null) _floorChipValue = go.GetComponent<TextMeshProUGUI>();
            }
            if (_goldChipValue == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "GoldChip_Value");
                if (go != null) _goldChipValue = go.GetComponent<TextMeshProUGUI>();
            }
            if (_ascensionValue == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(transform, "AscValue");
                if (go != null) _ascensionValue = go.GetComponent<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// GameRunState 기반 헤더 칩 값 갱신. 런타임 + 씬 복귀 시 모두 호출.
        /// </summary>
        public void Refresh(GameRunState runState)
        {
            if (runState == null) return;

            // Stage — "I — GreyForest" 형식 (로마 숫자 + 테마 표시명)
            if (_stageChipValue != null)
            {
                string themeName = runState.CurrentStageTheme != null
                    ? runState.CurrentStageTheme.displayName
                    : "Unknown";
                _stageChipValue.text = $"{ToRoman(runState.CurrentFloor)} — {themeName}";
            }

            // Floor — "1 / 4" (현재 층 / 전체 층)
            if (_floorChipValue != null)
            {
                _floorChipValue.text = $"{runState.CurrentFloor} / {runState.TotalFloors}";
            }

            // Gold — 현재 보유 골드
            if (_goldChipValue != null)
            {
                _goldChipValue.text = $"{runState.Gold}";
            }

            // Ascension — 선택된 어센션 레벨
            if (_ascensionValue != null)
            {
                _ascensionValue.text = $"{runState.SelectedAscensionLevel}";
            }
        }

        /// <summary>
        /// 디버그 모드 — GameRunState 없이 더미 값으로 헤더 칩 채우기.
        /// MapReworkDebugInitializer가 사용.
        /// </summary>
        public void RefreshWithDummy(int floor, int totalFloors, int gold, int ascLevel, string themeName)
        {
            if (_stageChipValue != null)
                _stageChipValue.text = $"{ToRoman(floor)} — {themeName}";
            if (_floorChipValue != null)
                _floorChipValue.text = $"{floor} / {totalFloors}";
            if (_goldChipValue != null)
                _goldChipValue.text = $"{gold}";
            if (_ascensionValue != null)
                _ascensionValue.text = $"{ascLevel}";
        }

        private static string ToRoman(int n)
        {
            return n switch
            {
                1 => "I",
                2 => "II",
                3 => "III",
                4 => "IV",
                _ => n.ToString()
            };
        }
    }
}
