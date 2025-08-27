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
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private LineManager lineManager;

    private int matchedPairs = 0;
    private bool gameActive = false;

    private readonly List<Vector2Int> hoverPath = new(); // ドラッグ中の経路
    private bool isDragging = false;
    private Color dragColor = new Color(0.2f, 0.9f, 1f, 1f);
    private Color fixedColor = new Color(0.15f, 0.8f, 0.2f, 1f); // 確定緑

    private List<Tile> selectedTiles = new();
    public int SelectedTilesCount => selectedTiles.Count;

    void Start()
    {
        if (uiManager.restartButton != null)
            uiManager.restartButton.onClick.AddListener(StartNewGame);

        StartNewGame();
    }

    private void Update()
    {
        if (!gameActive || lineManager == null) return;

        // タイルが1つだけ選択されている場合にドラッグ操作を許可
        if (selectedTiles.Count == 1)
        {
            var startTile = selectedTiles[0];
            var startCell = new Vector2Int(startTile.Row, startTile.Col);

            // ドラッグ開始
            if (!isDragging && Input.GetMouseButtonDown(0))
            {
                // パスの初期化
                hoverPath.Clear();
                hoverPath.Add(startCell);
                isDragging = true;
                lineManager.DrawHoverPath(hoverPath, dragColor);
            }
            // 伸ばす(ドラッグ中)
            if (isDragging && Input.GetMouseButton(0))
            {
                if (lineManager.ScreenToCell(Input.mousePosition, out var cell))
                {
                    TryExtendOrBacktrack(cell, startTile);
                }
            }
            // 終了(処理確定)
            if (isDragging && Input.GetMouseButtonUp(0))
            {
                FinishDrag(startTile);
            }
        }
        else
        {
            // 選択が無い・2つ以上ならドラッグ無効
            if (isDragging) { isDragging = false; hoverPath.Clear(); lineManager.ClearHoverLines(); }
        }
    }

    /// <summary>
    /// ドラック中の処理(経路の延長,経路を戻る処理)
    /// </summary>
    /// <param name="cell"> 現在カーソルのあるセル座標 </param>
    /// <param name="startTile"> ドラック開始セル </param>
    private void TryExtendOrBacktrack(Vector2Int cell, Tile startTile)
    {
        if (hoverPath.Count == 0) return;

        var last = hoverPath[hoverPath.Count - 1];
        if (cell == last) return;

        // 1つ戻る
        if (hoverPath.Count >= 2 && cell == hoverPath[hoverPath.Count - 2])
        {
            hoverPath.RemoveAt(hoverPath.Count - 1);
            lineManager.DrawHoverPath(hoverPath, dragColor);
            return;
        }

        // 近接時のみ許可
        if (!IsAdjacent(last, cell)) return;

        // 既に確定線が通っているセル不可
        if (lineManager.HasFixedOnCell(cell)) return;

        // 既に自分のドラッグ経路で通っているセル不可
        for (int i = 0; i < hoverPath.Count - 1; i++)
            if (hoverPath[i] == cell) return;

        // タイルの存在判定
        Tile t = boardManager.TileAt(cell);
        bool isStart = (cell.x == startTile.Row && cell.y == startTile.Col);

        if (t != null && !isStart)
        {
            // マッチ候補 or ブロック
            if (!t.IsMatched && t.Type == startTile.Type)
            {
                // 終点は可
                hoverPath.Add(cell);
                lineManager.DrawHoverPath(hoverPath, dragColor);
                FinishDrag(startTile);
                return;
            }
            else
            {
                return;
            }
        }

        // 空きセルなので延長
        hoverPath.Add(cell);
        lineManager.DrawHoverPath(hoverPath, dragColor);
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
        isDragging = false;

        if (hoverPath.Count < 2)
        {
            lineManager.ClearHoverLines();
            hoverPath.Clear();
            return;
        }

        var endCell = hoverPath[hoverPath.Count - 1];
        var endTile = boardManager.TileAt(endCell);

        // 同種タイルなら確定
        if (endTile != null && !endTile.IsMatched && endTile != startTile && endTile.Type == startTile.Type)
        {
            ConfirmMatch(hoverPath);
            hoverPath.Clear();
            selectedTiles.Clear();
        }
        else
        {
            // 失敗：線を消す
            lineManager.ClearHoverLines();
            hoverPath.Clear();
        }
    }

    /// <summary>
    /// 経路が正しく同種を結んだ時のみ呼ばれる
    /// </summary>
    /// <param name="path"> 経路セル列 </param>
    private void ConfirmMatch(List<Vector2Int> path)
    {
        if (path == null || path.Count < 2) return;

        Tile first = boardManager.TileAt(path[0]);
        Tile last = boardManager.TileAt(path[path.Count - 1]);
        if (first == null || last == null) return;
        if (first.IsMatched || last.IsMatched) return;
        if (first.Type != last.Type) return;

        // タイル確定
        first.Match();
        last.Match();

        // 線を確定
        lineManager.CommitHoverPath(path, fixedColor);

        matchedPairs++;
        uiManager.UpdateUI(boardManager.GetTotalPairs());
        if (matchedPairs >= boardManager.GetTotalPairs())
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
        boardManager.ResetBoard();

        matchedPairs = 0;

        GameResultKeeper.Instance.StartTime();
        gameActive = true;
        isDragging = false;
        hoverPath.Clear();
        StartCoroutine(uiManager.UpdateTimer(gameActive));
        uiManager.UpdateUI(boardManager.GetTotalPairs());
    }

    /// <summary>
    /// クリック時の処理
    /// </summary>
    /// <param name="tile"></param>
    public void OnTileClicked(Tile tile)
    {
        if (!gameActive || tile == null || tile.IsMatched) return;

        // 選択を解除して1つ選択
        foreach (var t in selectedTiles) t.Deselect();
        selectedTiles.Clear();

        selectedTiles.Add(tile);
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
