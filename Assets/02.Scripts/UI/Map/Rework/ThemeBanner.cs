using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Map;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// 맵 코덱스 상단의 테마 배너 — 현재 스테이지 테마 이름/태그라인/키워드 표시.
    /// PartySelectionScene의 Stage/TitlePanel 패턴 준거.
    /// </summary>
    public class ThemeBanner : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stageLabel;       // "— Stage II —"
        [SerializeField] private TextMeshProUGUI _themeNameText;    // "Crimson Chapel"
        [SerializeField] private TextMeshProUGUI _taglineText;      // 분위기 묘사
        [SerializeField] private Transform _keywordContainer;       // ThemeKeyword 칩들
        [SerializeField] private GameObject _keywordChipPrefab;     // 키워드 칩 프리팹

        // 키워드 칩 풀 (재사용)
        private readonly List<GameObject> _keywordChips = new();

        private void Awake()
        {
            AutoBindMissingFields();
        }

        private void AutoBindMissingFields()
        {
            var root = transform;
            if (_stageLabel == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "StageLabel");
                if (go != null) _stageLabel = go.GetComponent<TextMeshProUGUI>();
            }
            if (_themeNameText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "ThemeName");
                if (go != null) _themeNameText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_taglineText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "Tagline");
                if (go != null) _taglineText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_keywordContainer == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "KeywordContainer");
                if (go != null) _keywordContainer = go.transform;
            }
        }

        /// <summary>
        /// StageThemeData 기반 배너 렌더링.
        /// </summary>
        public void Initialize(StageThemeData theme, int stageNumber)
        {
            if (theme == null) return;

            if (_stageLabel != null)
                _stageLabel.text = $"— Stage {ToRoman(stageNumber)} —";

            if (_themeNameText != null)
                _themeNameText.text = theme.displayName;

            if (_taglineText != null)
                _taglineText.text = theme.description;

            RenderKeywords(theme.themeKeywords);
        }

        private void RenderKeywords(List<string> keywords)
        {
            // 기존 칩 제거
            foreach (var chip in _keywordChips)
            {
                if (chip != null) Destroy(chip);
            }
            _keywordChips.Clear();

            if (_keywordContainer == null || _keywordChipPrefab == null) return;
            if (keywords == null) return;

            foreach (var kw in keywords)
            {
                if (string.IsNullOrEmpty(kw)) continue;
                var chip = Instantiate(_keywordChipPrefab, _keywordContainer);
                chip.gameObject.SetActive(true);
                var label = chip.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = kw.ToUpper();
                _keywordChips.Add(chip);
            }
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
