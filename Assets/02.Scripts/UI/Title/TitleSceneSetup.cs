using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.UI;
using TeamLog.UI.Meta;

namespace TeamLog.UI.Title
{
    /// <summary>
    /// 타이틀 화면 컨트롤러 — 새 게임 / 이어하기 / 메타 상점 / 통계 표시
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

        private void Start()
        {
            _newGameButton.onClick.AddListener(OnNewGame);
            _continueButton.onClick.AddListener(OnContinue);
            if (_metaShopButton != null)
                _metaShopButton.onClick.AddListener(OnMetaShop);
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
        }

        private void OnNewGame()
        {
            SaveManager.DeleteSave();
            var meta = SaveManager.Meta;
            meta.HasPendingRun = false;
            SaveManager.SaveMeta();

            SceneTransition.Instance.FadeToScene("MapScene");
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
