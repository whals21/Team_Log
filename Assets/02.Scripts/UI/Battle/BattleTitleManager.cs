using UnityEngine;
using TMPro;
using Michsky.UI.MTP;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 전투 타이틀 애니메이션 — Motion Titles Pack 연동
    /// 전투 시작 시 "BATTLE START", 승리/패배 시 타이틀 표시
    /// </summary>
    public class BattleTitleManager : MonoBehaviour
    {
        [Header("Title Prefabs (Motion Titles Pack)")]
        [SerializeField] private GameObject _battleStartPrefab;
        [SerializeField] private GameObject _victoryPrefab;
        [SerializeField] private GameObject _defeatPrefab;

        [Header("Canvas")]
        [SerializeField] private RectTransform _canvasRect;

        [Header("Font")]
        [SerializeField] private TMP_FontAsset _koreanFont;

        private GameObject _currentTitle;

        /// <summary>
        /// 전투 시작 타이틀 표시
        /// </summary>
        public void ShowBattleStart()
        {
            SpawnTitle(_battleStartPrefab, "전투 시작!");
        }

        /// <summary>
        /// 승리 타이틀 표시
        /// </summary>
        public void ShowVictory()
        {
            SpawnTitle(_victoryPrefab, "승리!");
        }

        /// <summary>
        /// 패배 타이틀 표시
        /// </summary>
        public void ShowDefeat()
        {
            SpawnTitle(_defeatPrefab, "패배...");
        }

        private void SpawnTitle(GameObject prefab, string text)
        {
            if (prefab == null || _canvasRect == null) return;

            // 이전 타이틀 제거
            if (_currentTitle != null)
                Destroy(_currentTitle);

            var instance = Instantiate(prefab, _canvasRect);
            instance.name = prefab.name;
            _currentTitle = instance;

            // 텍스트 설정
            SetTitleText(instance, text);

            // 한국어 폰트 적용
            if (_koreanFont != null)
                SetFont(instance, _koreanFont);

            // StyleManager 설정: 자동 재생, 2초 후 사라짐
            var style = instance.GetComponent<StyleManager>();
            if (style != null)
            {
                style.playOnEnable = true;
                style.showFor = 2f;
                style.playOutAnimation = true;
                style.disableOnOut = true;
            }

            // 일정 시간 후 제거
            Destroy(instance, 4f);
        }

        private void SetTitleText(GameObject root, string text)
        {
            foreach (var textItem in root.GetComponentsInChildren<TextItem>(true))
            {
                textItem.text = text;
                if (textItem.textObject != null)
                    textItem.textObject.text = text;
            }
        }

        private void SetFont(GameObject root, TMP_FontAsset font)
        {
            foreach (var textItem in root.GetComponentsInChildren<TextItem>(true))
            {
                textItem.selectedFont = font;
                if (textItem.textObject != null)
                    textItem.textObject.font = font;
            }
        }
    }
}
