using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Minesweeper
{
    /// <summary>
    /// 지뢰찾기 게임 메인 컨트롤러
    /// </summary>
    public class MinesweeperGame : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private int _rows = 9;
        [SerializeField] private int _columns = 9;
        [SerializeField] private int _mineCount = 10;

        [Header("UI References")]
        [SerializeField] private Transform _gridContainer;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private Text _mineCountText;
        [SerializeField] private Text _timerText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private Text _resultText;

        [Header("Grid Settings")]
        [SerializeField] private float _cellSize = 40f;
        [SerializeField] private float _cellSpacing = 2f;

        private Cell[,] _cells;
        private bool _gameStarted;
        private bool _gameOver;
        private float _gameTime;
        private int _flagsPlaced;
        private int _revealedCount;

        private void Awake()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(RestartGame);
        }

        private void Start()
        {
            InitializeGame();
        }

        private void Update()
        {
            if (_gameStarted && !_gameOver)
            {
                _gameTime += Time.deltaTime;
                UpdateTimerDisplay();
            }
        }

        public void InitializeGame()
        {
            _gameStarted = false;
            _gameOver = false;
            _gameTime = 0f;
            _flagsPlaced = 0;
            _revealedCount = 0;

            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(false);

            UpdateMineCountDisplay();
            UpdateTimerDisplay();

            CreateGrid();
        }

        private void CreateGrid()
        {
            // 기존 셀 제거
            if (_cells != null)
            {
                for (int r = 0; r < _cells.GetLength(0); r++)
                {
                    for (int c = 0; c < _cells.GetLength(1); c++)
                    {
                        if (_cells[r, c] != null)
                            Destroy(_cells[r, c].gameObject);
                    }
                }
            }

            // 그리드 레이아웃 설정
            if (_gridContainer != null)
            {
                var gridLayout = _gridContainer.GetComponent<GridLayoutGroup>();
                if (gridLayout != null)
                {
                    gridLayout.cellSize = new Vector2(_cellSize, _cellSize);
                    gridLayout.spacing = new Vector2(_cellSpacing, _cellSpacing);
                    gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    gridLayout.constraintCount = _columns;
                }
            }

            _cells = new Cell[_rows, _columns];

            // 셀 생성
            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _columns; c++)
                {
                    GameObject cellObj = Instantiate(_cellPrefab, _gridContainer);
                    cellObj.name = $"Cell_{r}_{c}";

                    Cell cell = cellObj.GetComponent<Cell>();
                    if (cell == null)
                        cell = cellObj.AddComponent<Cell>();

                    cell.Initialize(r, c, this);
                    _cells[r, c] = cell;
                }
            }
        }

        private void PlaceMines(int excludeRow, int excludeCol)
        {
            int placedMines = 0;
            int totalCells = _rows * _columns;

            while (placedMines < _mineCount)
            {
                int randomIndex = Random.Range(0, totalCells);
                int r = randomIndex / _columns;
                int c = randomIndex % _columns;

                // 첫 클릭 위치와 인접 셀에는 지뢰 배치 안 함
                if (Mathf.Abs(r - excludeRow) <= 1 && Mathf.Abs(c - excludeCol) <= 1)
                    continue;

                if (!_cells[r, c].IsMine)
                {
                    _cells[r, c].SetMine(true);
                    placedMines++;
                }
            }

            // 인접 지뢰 수 계산
            CalculateAdjacentMines();
        }

        private void CalculateAdjacentMines()
        {
            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _columns; c++)
                {
                    if (!_cells[r, c].IsMine)
                    {
                        int count = CountAdjacentMines(r, c);
                        _cells[r, c].SetAdjacentMines(count);
                    }
                }
            }
        }

        private int CountAdjacentMines(int row, int col)
        {
            int count = 0;

            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0)
                        continue;

                    int nr = row + dr;
                    int nc = col + dc;

                    if (IsValidCell(nr, nc) && _cells[nr, nc].IsMine)
                        count++;
                }
            }

            return count;
        }

        private bool IsValidCell(int row, int col)
        {
            return row >= 0 && row < _rows && col >= 0 && col < _columns;
        }

        public void OnCellClicked(Cell cell)
        {
            if (_gameOver || cell.IsFlagged || cell.IsRevealed)
                return;

            // 첫 클릭 시 지뢰 배치
            if (!_gameStarted)
            {
                _gameStarted = true;
                PlaceMines(cell.Row, cell.Column);
            }

            if (cell.IsMine)
            {
                // 게임 오버
                RevealAllMines();
                GameOver(false);
            }
            else
            {
                // 셀 공개 (빈 셀이면 확장)
                RevealCell(cell);
                CheckWin();
            }
        }

        public void OnCellRightClicked(Cell cell)
        {
            if (_gameOver || cell.IsRevealed)
                return;

            cell.ToggleFlag();

            if (cell.IsFlagged)
                _flagsPlaced++;
            else
                _flagsPlaced--;

            UpdateMineCountDisplay();
        }

        private void RevealCell(Cell cell)
        {
            if (cell.IsRevealed || cell.IsFlagged)
                return;

            cell.Reveal();
            _revealedCount++;

            // 빈 셀이면 인접 셀도 공개
            if (cell.AdjacentMines == 0 && !cell.IsMine)
            {
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr == 0 && dc == 0)
                            continue;

                        int nr = cell.Row + dr;
                        int nc = cell.Column + dc;

                        if (IsValidCell(nr, nc))
                            RevealCell(_cells[nr, nc]);
                    }
                }
            }
        }

        private void RevealAllMines()
        {
            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _columns; c++)
                {
                    if (_cells[r, c].IsMine)
                        _cells[r, c].Reveal();
                }
            }
        }

        private void CheckWin()
        {
            int totalSafeCells = (_rows * _columns) - _mineCount;

            if (_revealedCount >= totalSafeCells)
            {
                GameOver(true);
            }
        }

        private void GameOver(bool won)
        {
            _gameOver = true;

            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(true);

            if (_resultText != null)
            {
                _resultText.text = won ? "승리!" : "패배!";
                _resultText.color = won ? Color.green : Color.red;
            }
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void UpdateMineCountDisplay()
        {
            if (_mineCountText != null)
                _mineCountText.text = $"지뢰: {_mineCount - _flagsPlaced}";
        }

        private void UpdateTimerDisplay()
        {
            if (_timerText != null)
                _timerText.text = $"시간: {(int)_gameTime}s";
        }

        // 난이도 설정
        public void SetDifficulty(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    _rows = 9;
                    _columns = 9;
                    _mineCount = 10;
                    break;
                case Difficulty.Medium:
                    _rows = 16;
                    _columns = 16;
                    _mineCount = 40;
                    break;
                case Difficulty.Hard:
                    _rows = 16;
                    _columns = 30;
                    _mineCount = 99;
                    break;
            }
        }
    }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
}
