using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Map;
using TeamLog.UI;

namespace TeamLog.UI.Title
{
    /// <summary>
    /// 타이틀 화면 컨트롤러 — 새 게임 / 이어하기 / 통계 표시
    /// </summary>
    public class TitleSceneSetup : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TextMeshProUGUI _statsLabel;
        [SerializeField] private GameObject _continueBlock;

        private void Start()
        {
            _newGameButton.onClick.AddListener(OnNewGame);
            _continueButton.onClick.AddListener(OnContinue);
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
                _statsLabel.text = $"총 런: {meta.TotalRuns}  승리: {meta.Victories}\n최고 층: {meta.BestFloor}";
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
    }
}
