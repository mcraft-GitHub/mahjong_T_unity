using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// GameScene を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardManager _boardManager;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private LineManager _lineManager;

    private int _matchedPairs = 0;
    private bool _gameActive = false;

    // ドラッグ中の経路
    private readonly List<Vector2Int> _hoverPath = new();
    private bool _isDragging = false;
    private Color _dragColor = new Color(0.2f, 0.9f, 1f, 1f);
    // 確定緑
    private Color _fixedColor = new Color(0.15f, 0.8f, 0.2f, 1f);

    private List<Tile> _selectedTiles = new();
    public int _selectedTilesCount => _selectedTiles.Count;

    void Start()
    {
        if (_uiManager._restartButton != null)
            _uiManager._restartButton.onClick.AddListener(StartNewGame);

        StartNewGame();
    }

    private void Update()
    {
        if (!_gameActive || _lineManager == null) return;

        // タイルが1つだけ選択されている場合にドラッグ操作を許可
        if (_selectedTiles.Count == 1)
        {
            var startTile = _selectedTiles[0];
            var startCell = new Vector2Int(startTile._row, startTile._col);

            // ドラッグ開始
            if (!_isDragging && Input.GetMouseButtonDown(0))
            {
                // パスの初期化
                _hoverPath.Clear();
                _hoverPath.Add(startCell);
                _isDragging = true;
                _lineManager.DrawHoverPath(_hoverPath, _dragColor);
            }
            // 伸ばす(ドラッグ中)
            if (_isDragging && Input.GetMouseButton(0))
            {
                if (_lineManager.ScreenToCell(Input.mousePosition, out var cell))
                {
                    TryExtendOrBacktrack(cell, startTile);
                }
            }
            // 終了(処理確定)
            if (_isDragging && Input.GetMouseButtonUp(0))
            {
                FinishDrag(startTile);
            }
        }
        else
        {
            // 選択が無い・2つ以上ならドラッグ無効
            if (_isDragging)
            {
                _isDragging = false;
                _hoverPath.Clear();
                _lineManager.ClearHoverLines();
            }
        }
    }

    /// <summary>
    /// ドラック中の処理(経路の延長,経路を戻る処理)
    /// </summary>
    /// <param name="cell"> 現在カーソルのあるセル座標 </param>
    /// <param name="startTile"> ドラック開始セル </param>
    private void TryExtendOrBacktrack(Vector2Int cell, Tile startTile)
    {
        if (_hoverPath.Count == 0) return;

        var last = _hoverPath[_hoverPath.Count - 1];
        if (cell == last) return;

        // 1つ戻る
        if (_hoverPath.Count >= 2 && cell == _hoverPath[_hoverPath.Count - 2])
        {
            _hoverPath.RemoveAt(_hoverPath.Count - 1);
            _lineManager.DrawHoverPath(_hoverPath, _dragColor);
            return;
        }

        // 近接時のみ許可
        if (!IsAdjacent(last, cell)) return;

        // 既に確定線が通っているセル不可
        if (_lineManager.HasFixedOnCell(cell)) return;

        // 既に自分のドラッグ経路で通っているセル不可
        for (int i = 0; i < _hoverPath.Count - 1; i++)
            if (_hoverPath[i] == cell) return;

        // タイルの存在判定
        Tile t = _boardManager.TileAt(cell);
        bool isStart = (cell.x == startTile._row && cell.y == startTile._col);

        if (t != null && !isStart)
        {
            // マッチ候補 or ブロック
            if (!t._isMatched && t._type == startTile._type)
            {
                // 終点は可
                _hoverPath.Add(cell);
                _lineManager.DrawHoverPath(_hoverPath, _dragColor);
                FinishDrag(startTile);
                return;
            }
            else
            {
                return;
            }
        }

        // 空きセルなので延長
        _hoverPath.Add(cell);
        _lineManager.DrawHoverPath(_hoverPath, _dragColor);
    }

    /// <summary>
    /// 指定したタイルが隣にあるか
    /// </summary>
    /// <param name="a"> セルA座標 </param>
    /// <param name="b"> セルB座標 </param>
    /// <returns></returns>
    private static bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) == 1;
    }

    /// <summary>
    /// ドラック操作終了時、マッチ成功,失敗判定
    /// </summary>
    /// <param name="startTile"> ドラック開始タイル </param>
    private void FinishDrag(Tile startTile)
    {
        _isDragging = false;

        if (_hoverPath.Count < 2)
        {
            _lineManager.ClearHoverLines();
            _hoverPath.Clear();
            return;
        }

        var endCell = _hoverPath[_hoverPath.Count - 1];
        var endTile = _boardManager.TileAt(endCell);

        // 同種タイルなら確定
        if (endTile != null && !endTile._isMatched && endTile != startTile && endTile._type == startTile._type)
        {
            ConfirmMatch(_hoverPath);
            _hoverPath.Clear();
            _selectedTiles.Clear();
        }
        else
        {
            // 失敗：線を消す
            _lineManager.ClearHoverLines();
            _hoverPath.Clear();
        }
    }

    /// <summary>
    /// 経路が正しく同種を結んだ時のみ呼ばれる
    /// </summary>
    /// <param name="path"> 経路セル列 </param>
    private void ConfirmMatch(List<Vector2Int> path)
    {
        if (path == null || path.Count < 2) return;

        Tile first = _boardManager.TileAt(path[0]);
        Tile last = _boardManager.TileAt(path[path.Count - 1]);
        if (first == null || last == null) return;
        if (first._isMatched || last._isMatched) return;
        if (first._type != last._type) return;

        // タイル確定
        first.Match();
        last.Match();

        // 線を確定
        _lineManager.CommitHoverPath(path, _fixedColor);

        _matchedPairs++;
        _uiManager.UpdateUI(_boardManager.GetTotalPairs(), _matchedPairs);
        if (_matchedPairs >= _boardManager.GetTotalPairs())
        {
            GameResultKeeper.Instance.MakeResultTime();
            ChangeResultScene();
        }
    }

    /// <summary>
    /// 盤面のリセット
    /// </summary>
    public void StartNewGame()
    {
        _boardManager.ResetBoard();

        _matchedPairs = 0;
        GameResultKeeper.Instance.StartTime();
        _gameActive = true;
        _isDragging = false;
        _hoverPath.Clear();
        StartCoroutine(_uiManager.UpdateTimer(_gameActive));
        _uiManager.UpdateUI(_boardManager.GetTotalPairs(), _matchedPairs);
    }

    /// <summary>
    /// クリック時の処理
    /// </summary>
    /// <param name="tile"></param>
    public void OnTileClicked(Tile tile)
    {
        if (!_gameActive || tile == null || tile._isMatched) return;

        // 選択を解除して1つ選択
        foreach (var t in _selectedTiles)
            t.Deselect();
        _selectedTiles.Clear();

        _selectedTiles.Add(tile);
        tile.Select();
    }

    /// <summary>
    /// ResultScene へ シーン遷移
    /// </summary>
    void ChangeResultScene()
    {
        SceneManager.LoadScene("ResultScene");
    }
}
