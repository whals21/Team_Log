using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Meta;
using TeamLog.UI;
using TeamLog.UI.Meta;

namespace TeamLog.UI.Title
{
    /// <summary>
    /// 타이틀 화면 컨트롤러 — 새 게임 / 이어하기 / 메타 상점 / 통계 표시 / 어센션 선택
    /// </summary>
    public class TitleSceneSetup : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _metaShopButton;
        [SerializeField] private TextMeshProUGUI _statsLabel;
        [SerializeField] private GameObject _continueBlock;
        [SerializeField] private MetaShopUI _metaShopUI;

        [Header("All Characters (for dynamic default count)")]
        [SerializeField] private CharacterData[] _allCharacters;

        [Header("Ascension (optional — 연결 시에만 작동)")]
        [SerializeField] private Button _ascensionUpButton;
        [SerializeField] private Button _ascensionDownButton;
        [SerializeField] private TextMeshProUGUI _ascensionLabel;

        private void Start()
        {
            _newGameButton.onClick.AddListener(OnNewGame);
            _continueButton.onClick.AddListener(OnContinue);
            if (_metaShopButton != null)
                _metaShopButton.onClick.AddListener(OnMetaShop);
            if (_ascensionUpButton != null)
                _ascensionUpButton.onClick.AddListener(() => ChangeAscensionLevel(+1));
            if (_ascensionDownButton != null)
                _ascensionDownButton.onClick.AddListener(() => ChangeAscensionLevel(-1));
            RefreshUI();
        }

        private void RefreshUI()
        {
            bool hasSave = SaveManager.HasSave;
            _continueButton.interactable = hasSave;
            if (_continueBlock != null)
                _continueBlock.SetActive(!hasSave);

            var meta = SaveManager.Meta;
            if (_statsLabel != null && meta != null)
            {
                // Phase 8F: 하드코딩 4 대신 동적 계산
                int defaultCount = _allCharacters != null
                    ? _allCharacters.Count(c => c != null && c.IsDefault)
                    : 0;
                int unlockedCount = defaultCount + (meta.UnlockedCharacterIds?.Count ?? 0);
                int total = _allCharacters != null ? _allCharacters.Length : 8;
                _statsLabel.text = $"총 런: {meta.TotalRuns}  승리: {meta.Victories}\n최고 층: {meta.BestFloor}\n캐릭터: {unlockedCount}/{total}\n기억: {meta.MemoryFragments}  영혼: {meta.Souls}";
            }

            // 어센션 표시
            if (_ascensionLabel != null && meta != null)
            {
                int max = meta.AscensionLevel;
                int sel = Mathf.Clamp(meta.SelectedAscensionLevel, 0, max);
                _ascensionLabel.text = max > 0
                    ? $"어센션: {sel} / {max}"
                    : "어센션: 미개방 (클리어 시 개방)";
            }
            if (_ascensionUpButton != null)
                _ascensionUpButton.interactable = meta != null && meta.SelectedAscensionLevel < meta.AscensionLevel;
            if (_ascensionDownButton != null)
                _ascensionDownButton.interactable = meta != null && meta.SelectedAscensionLevel > 0;
        }

        /// <summary>어센션 레벨 선택 (다음 런에 적용). 달성 레벨 이하에서만.</summary>
        private void ChangeAscensionLevel(int delta)
        {
            if (delta == 0) return;
            var meta = SaveManager.Meta;
            int newVal = Mathf.Clamp(meta.SelectedAscensionLevel + delta, 0, meta.AscensionLevel);
            if (newVal == meta.SelectedAscensionLevel) return;
            meta.SelectedAscensionLevel = newVal;
            SaveManager.SaveMeta();
            AudioManager.Instance?.PlayUIConfirm();
            RefreshUI();
        }

        private void OnNewGame()
        {
            SaveManager.DeleteSave();
            var meta = SaveManager.Meta;
            meta.HasPendingRun = false;
            SaveManager.SaveMeta();

            // ★ 새 런 시작 — PartySelectionScene으로 이동 (캐릭터 선택 화면)
            SceneTransition.Instance.FadeToScene("PartySelectionScene");
        }

        private void OnContinue()
        {
            SceneTransition.Instance.FadeToScene("MapScene");
        }

        private void OnMetaShop()
        {
            AudioManager.Instance?.PlayUIConfirm();
            if (_metaShopUI != null)
            {
                _metaShopUI.Show();
                RefreshUI();
            }
        }

        private void OnDestroy()
        {
            if (_metaShopButton != null)
                _metaShopButton.onClick.RemoveListener(OnMetaShop);
        }
    }
}
