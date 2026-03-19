using UnityEngine;
using UnityEngine.UI;

namespace Minesweeper
{
    /// <summary>
    /// 개별 셀 데이터 및 상태 관리
    /// </summary>
    public class Cell : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private Text _numberText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _flagIcon;
        [SerializeField] private Image _mineIcon;

        [Header("Colors")]
        [SerializeField] private Color _hiddenColor = new Color(0.7f, 0.7f, 0.7f);
        [SerializeField] private Color _revealedColor = new Color(0.9f, 0.9f, 0.9f);
        [SerializeField] private Color _mineColor = new Color(1f, 0.3f, 0.3f);

        // 셀 상태
        public int Row { get; private set; }
        public int Column { get; private set; }
        public bool IsMine { get; private set; }
        public bool IsRevealed { get; private set; }
        public bool IsFlagged { get; private set; }
        public int AdjacentMines { get; private set; }

        // 숫자별 색상
        private static readonly Color[] NumberColors = new Color[]
        {
            Color.clear,           // 0
            Color.blue,            // 1
            new Color(0, 0.5f, 0), // 2 (green)
            Color.red,             // 3
            new Color(0, 0, 0.5f), // 4 (dark blue)
            new Color(0.5f, 0, 0), // 5 (dark red)
            new Color(0, 0.5f, 0.5f), // 6 (cyan)
            Color.black,           // 7
            Color.gray             // 8
        };

        private MinesweeperGame _game;

        public void Initialize(int row, int col, MinesweeperGame game)
        {
            Row = row;
            Column = col;
            _game = game;
            Reset();
        }

        public void SetMine(bool isMine)
        {
            IsMine = isMine;
        }

        public void SetAdjacentMines(int count)
        {
            AdjacentMines = count;
        }

        public void Reset()
        {
            IsMine = false;
            IsRevealed = false;
            IsFlagged = false;
            AdjacentMines = 0;

            if (_backgroundImage != null)
                _backgroundImage.color = _hiddenColor;
            if (_numberText != null)
            {
                _numberText.text = "";
                _numberText.gameObject.SetActive(false);
            }
            if (_flagIcon != null)
                _flagIcon.gameObject.SetActive(false);
            if (_mineIcon != null)
                _mineIcon.gameObject.SetActive(false);
            if (_button != null)
                _button.interactable = true;
        }

        public void Reveal()
        {
            if (IsRevealed || IsFlagged)
                return;

            IsRevealed = true;

            if (_button != null)
                _button.interactable = false;

            if (IsMine)
            {
                // 지뢰 표시
                if (_backgroundImage != null)
                    _backgroundImage.color = _mineColor;
                if (_mineIcon != null)
                    _mineIcon.gameObject.SetActive(true);
            }
            else
            {
                // 숫자 표시
                if (_backgroundImage != null)
                    _backgroundImage.color = _revealedColor;

                if (AdjacentMines > 0 && _numberText != null)
                {
                    _numberText.text = AdjacentMines.ToString();
                    _numberText.color = NumberColors[Mathf.Min(AdjacentMines, 8)];
                    _numberText.gameObject.SetActive(true);
                }
            }
        }

        public void ToggleFlag()
        {
            if (IsRevealed)
                return;

            IsFlagged = !IsFlagged;

            if (_flagIcon != null)
                _flagIcon.gameObject.SetActive(IsFlagged);
        }

        public void OnCellClicked()
        {
            if (_game != null)
                _game.OnCellClicked(this);
        }

        public void OnCellRightClicked()
        {
            if (_game != null)
                _game.OnCellRightClicked(this);
        }

        // 버튼 이벤트용 (우클릭 지원을 위해 IPointerClickHandler 사용 권장)
        private void Start()
        {
            if (_button != null)
                _button.onClick.AddListener(OnCellClicked);
        }
    }
}
