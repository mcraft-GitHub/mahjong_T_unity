using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// ゲームの進行・UI・盤面・タイル制御を行うクラス
/// </summary>
public class JanChainGame : MonoBehaviour
{
    [Header("Game Settings")]
    public int gridSize = 8;
    public int maxTileTypes = 6;

    [Header("UI Elements")]
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private TMP_Text pairsLeftText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text finalTimeText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button backToTitleButton;

    [Header("Tile / Grid UI")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private LineManager lineManager;

    [Header("Colors")]
    [SerializeField] private Color[] typeColors;

    private Tile[,] tiles;
    private List<Tile> selectedTiles = new();
    private int matchedPairs = 0;
    private int totalPairs = 0;
    private bool gameActive = false;
    private float startTime;

    private readonly List<string> allTypes = new() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
    private System.Random rand = new System.Random();
    private int maxPlacementAttempts = 800;
    private int maxPathsPerPair = 40;
    private int pathSlack = 6;

    private readonly List<Vector2Int> hoverPath = new(); // ドラッグ中の経路
    private bool isDragging = false;
    private Color dragColor = new Color(0.2f, 0.9f, 1f, 1f);
    private Color fixedColor = new Color(0.15f, 0.8f, 0.2f, 1f); // 確定緑


    public int SelectedTilesCount => selectedTiles.Count;


    /// <summary>
    /// ペア配列情報
    /// </summary>
    private class PairPlacement
    {
        public Vector2Int a;
        public Vector2Int b;
        public string type;
        public PairPlacement(Vector2Int a, Vector2Int b, string type) { this.a = a; this.b = b; this.type = type; }
    }

    /// <summary>
    /// ペアとその最短距離をまとめた構造体
    /// </summary>
    private class PairWithDist
    {
        public PairPlacement pair;
        public int dist;
        public PairWithDist(PairPlacement p, int d) { pair = p; dist = d; }
    }

    /// <summary>
    /// ゲーム開始時にUI等が機能し、タイトル画面を表示する
    /// </summary>
    private void Start()
    {
        if (startButton != null) startButton.onClick.AddListener(StartNewGame);
        if (restartButton != null) restartButton.onClick.AddListener(StartNewGame);
        if (playAgainButton != null) playAgainButton.onClick.AddListener(StartNewGame);
        if (backToTitleButton != null) backToTitleButton.onClick.AddListener(() => ShowScreen(titleScreen));
        ShowScreen(titleScreen);
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
    /// 表示panel切り替え(Game・Title・Victory)
    /// </summary>
    /// <param name="screen"> 切り替えるPanel </param>
    private void ShowScreen(GameObject screen)
    {
        titleScreen.SetActive(false);
        gameScreen.SetActive(false);
        victoryScreen.SetActive(false);
        if (screen != null) screen.SetActive(true);
    }

    /// <summary>
    /// 盤面のリセット
    /// </summary>
    public void StartNewGame()
    {
        ResetGame();

        if (lineManager != null)
        {
            lineManager.rows = gridSize;
            lineManager.columns = gridSize;
            lineManager.ClearAllLines();
            lineManager.RecalcGrid();
        }

        GenerateBoardSafe();
        matchedPairs = 0;
        startTime = Time.time;
        gameActive = true;
        ShowScreen(gameScreen);
        StartCoroutine(UpdateTimer());
        UpdateUI();
    }

    /// <summary>
    /// ゲームの盤面をリセット
    /// </summary>
    private void ResetGame()
    {
        if (tiles != null)
        {
            foreach (var t in tiles)
                if (t != null && t.gameObject != null) Destroy(t.gameObject);
        }
        tiles = new Tile[gridSize, gridSize];
        selectedTiles.Clear();
        matchedPairs = 0;
        totalPairs = 0;
        hoverPath.Clear();
        isDragging = false;
    }

    /// <summary>
    /// 経過時間の更新
    /// </summary>
    private IEnumerator UpdateTimer()
    {
        while (gameActive)
        {
            float elapsed = Time.time - startTime;
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            if (timerText != null) timerText.text = $"{minutes:00}:{seconds:00}";
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// 残りペア数表示の更新
    /// </summary>
    private void UpdateUI()
    {
        if (pairsLeftText != null) pairsLeftText.text = (totalPairs - matchedPairs).ToString();
    }

    /// <summary>
    /// ゲーム完了時経過時間の表示とPanel切替
    /// </summary>
    private void HandleVictory()
    {
        gameActive = false;
        float elapsed = Time.time - startTime;
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);
        if (finalTimeText != null) finalTimeText.text = $"{minutes:00}:{seconds:00}";
        ShowScreen(victoryScreen);
    }

    /// <summary>
    /// タイルクリック時処理
    /// </summary>
    /// <param name="tile"> 対応タイル </param>
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
        Tile t = TileAt(cell);
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
        var endTile = TileAt(endCell);

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
    public void ConfirmMatch(List<Vector2Int> path)
    {
        if (path == null || path.Count < 2) return;

        Tile first = TileAt(path[0]);
        Tile last = TileAt(path[path.Count - 1]);
        if (first == null || last == null) return;
        if (first.IsMatched || last.IsMatched) return;
        if (first.Type != last.Type) return;

        // タイル確定
        first.Match();
        last.Match();

        // 線を確定
        lineManager.CommitHoverPath(path, fixedColor);

        matchedPairs++;
        UpdateUI();
        if (matchedPairs >= totalPairs) HandleVictory();
    }

    /// <summary>
    /// 指定セルを返す
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public Tile TileAt(Vector2Int cell)
    {
        if (tiles == null) return null;
        if (cell.x < 0 || cell.x >= tiles.GetLength(0) ||
            cell.y < 0 || cell.y >= tiles.GetLength(1)) return null;
        return tiles[cell.x, cell.y];
    }

    /// <summary>
    /// ランダム配置
    /// </summary>
    private void GenerateBoardSafe()
    {
        // 決定する種類数
        int typeCount = Mathf.Min(maxTileTypes, allTypes.Count);
        List<string> types = new List<string>(allTypes);
        Shuffle(types);
        types = types.GetRange(0, typeCount);

        bool success = false;
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            // ランダムに 2*typeCount 個のセルを選ぶ
            List<Vector2Int> allCells = new List<Vector2Int>();
            for (int x = 0; x < gridSize; x++)
                for (int y = 0; y < gridSize; y++)
                    allCells.Add(new Vector2Int(x, y));
            Shuffle(allCells);

            List<Vector2Int> chosen = allCells.GetRange(0, typeCount * 2);

            // ランダムにペアにする
            Shuffle(chosen);
            var pairs = new List<PairPlacement>();
            for (int i = 0; i < typeCount; i++)
            {
                Vector2Int a = chosen[2 * i];
                Vector2Int b = chosen[2 * i + 1];
                pairs.Add(new PairPlacement(a, b, types[i]));
            }

            // 全ペアを重複しないパスで繋げるか
            if (TryRouteAllPairs(pairs))
            {
                // 成功：UI に反映して完了
                PlaceTilesFromPairs(pairs);
                totalPairs = typeCount;
                success = true;
                Debug.Log($"[GenerateBoardSafe] success after {attempt + 1} attempts");
                break;
            }
        }

        if (!success)
        {
            Debug.LogError("[GenerateBoardSafe] Failed to create solvable board after attempts. Falling back to naive placement.");
            // フォールバック
            FallbackPlace(types);
        }
    }

    /// <summary>
    /// 経路を無視してランダムにペアを配置するフォールバック処理
    /// </summary>
    private void FallbackPlace(List<string> types)
    {
        List<Vector2Int> allCells = new List<Vector2Int>();
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                allCells.Add(new Vector2Int(x, y));
        Shuffle(allCells);
        int idx = 0;
        tiles = new Tile[gridSize, gridSize];
        for (int i = 0; i < types.Count; i++)
        {
            PlaceTile(allCells[idx++], types[i]);
            PlaceTile(allCells[idx++], types[i]);
        }
        totalPairs = types.Count;
    }

    /// <summary>
    /// ペアリスト を UI に反映
    /// </summary>
    private void PlaceTilesFromPairs(List<PairPlacement> pairs)
    {
        // 盤面のタイル配列を初期化
        tiles = new Tile[gridSize, gridSize];

        for (int i = 0; i < pairs.Count; i++)
        {
            var p = pairs[i];
            PlaceTile(p.a, p.type);
            PlaceTile(p.b, p.type);
        }
    }

    /// <summary>
    /// お互いに干渉しない経路で繋げるか
    /// </summary>
    private bool TryRouteAllPairs(List<PairPlacement> pairs)
    {
        // 全てのペアの両端を占有
        bool[,] tileOccupied = new bool[gridSize, gridSize];
        foreach (var p in pairs)
        {
            tileOccupied[p.a.x, p.a.y] = true;
            tileOccupied[p.b.x, p.b.y] = true;
        }

        bool[,] occupiedPaths = new bool[gridSize, gridSize]; // 空

        // 難しいペアから処理した方が良い(最短距離が長いものから先に処理)
        var pairInfo = new List<PairWithDist>();
        foreach (var p in pairs)
        {
            int d = ShortestDistance(tileOccupied, occupiedPaths, p.a, p.b);
            if (d < 0) d = int.MaxValue; // 到達不可
            pairInfo.Add(new PairWithDist(p, d));
        }
        // 長い距離(難しい)を先に
        pairInfo.Sort((x, y) => y.dist.CompareTo(x.dist));

        // 作り直した順序の pairsOrdered を作る
        var pairsOrdered = new List<PairPlacement>();
        foreach (var pw in pairInfo) pairsOrdered.Add(pw.pair);

        // 再帰でルーティング
        var chosenPaths = new List<List<Vector2Int>>(); // optional: store chosen paths
        bool ok = RoutePairsRecursive(pairsOrdered, 0, tileOccupied, occupiedPaths, chosenPaths);
        return ok;
    }

    /// <summary>
    /// 再帰的にペアの経路を探索して全てつなげる
    /// </summary>
    /// <param name="pairs"></param>
    /// <param name="idx"></param>
    /// <param name="tileOccupied"></param>
    /// <param name="occupiedPaths"></param>
    /// <param name="chosenPaths"></param>
    /// <returns></returns>
    private bool RoutePairsRecursive(List<PairPlacement> pairs, int idx, bool[,] tileOccupied, bool[,] occupiedPaths, List<List<Vector2Int>> chosenPaths)
    {
        // 全ペア処理済みなら成功
        if (idx >= pairs.Count) return true;

        var p = pairs[idx];

        // 候補パスを列挙
        var candidates = EnumeratePaths(p.a, p.b, tileOccupied, occupiedPaths, maxPathsPerPair, pathSlack);

        if (candidates == null || candidates.Count == 0)
        {
            // 到達不可
            return false;
        }

        foreach (var path in candidates)
        {
            // 経路セルを一時的に占有扱いにする
            var occupiedList = new List<Vector2Int>();
            for (int k = 0; k < path.Count; k++)
            {
                Vector2Int cell = path[k];
                if (cell == p.a || cell == p.b) continue;
                if (!occupiedPaths[cell.x, cell.y])
                {
                    occupiedPaths[cell.x, cell.y] = true;
                    occupiedList.Add(cell);
                }
                else {}
            }

            chosenPaths.Add(path);
            // 再帰
            if (RoutePairsRecursive(pairs, idx + 1, tileOccupied, occupiedPaths, chosenPaths))
                return true;

            // ロールバック
            chosenPaths.RemoveAt(chosenPaths.Count - 1);
            foreach (var c in occupiedList) occupiedPaths[c.x, c.y] = false;
        }

        return false; // 全経路失敗
    }

    /// <summary>
    /// ペアの 始点から終点までの経路をDFSで列挙
    /// </summary>
    /// <param name="start"> 開始セル </param>
    /// <param name="end"> 終点セル </param>
    /// <param name="tileOccupied"> タイルが置かれているセルをtrueとする2次元配列 </param>
    /// <param name="occupiedPaths"> 確定している経路で使用しているセルをtrueとする2次元配列 </param>
    /// <param name="maxPaths"> 列挙する経路の最大数 </param>
    /// <param name="slack"> 探索で見つけた経路(セル座標リスト)のリスト </param>
    /// <returns></returns>
    private List<List<Vector2Int>> EnumeratePaths(Vector2Int start, Vector2Int end, bool[,] tileOccupied, bool[,] occupiedPaths, int maxPaths, int slack)
    {
        // まず最短距離を BFS で求める
        int shortest = ShortestDistance(tileOccupied, occupiedPaths, start, end);
        if (shortest < 0) return new List<List<Vector2Int>>();

        int maxLen = shortest + slack;

        List<List<Vector2Int>> results = new List<List<Vector2Int>>();
        bool[,] visited = new bool[gridSize, gridSize];
        List<Vector2Int> cur = new List<Vector2Int>();
        cur.Add(start);
        visited[start.x, start.y] = true;

        // ヒューリスティックに近い方向から探索するために隣接順をソート
        void Dfs(Vector2Int node)
        {
            if (results.Count >= maxPaths) return;
            if (cur.Count > maxLen + 1) return;

            if (node == end)
            {
                results.Add(new List<Vector2Int>(cur));
                return;
            }

            // 隣接を取得
            var neighbors = new List<Vector2Int>()
            {
                node + new Vector2Int(1,0),
                node + new Vector2Int(-1,0),
                node + new Vector2Int(0,1),
                node + new Vector2Int(0,-1)
            };

            neighbors.Sort((a, b) => (Mathf.Abs(a.x - end.x) + Mathf.Abs(a.y - end.y)).CompareTo(Mathf.Abs(b.x - end.x) + Mathf.Abs(b.y - end.y)));

            foreach (var n in neighbors)
            {
                if (results.Count >= maxPaths) break;
                if (n.x < 0 || n.x >= gridSize || n.y < 0 || n.y >= gridSize) continue;
                if (visited[n.x, n.y]) continue;

                // 通行可能か判定
                bool isEnd = (n == end);
                if (!isEnd)
                {
                    if (tileOccupied[n.x, n.y]) continue;
                    if (occupiedPaths[n.x, n.y]) continue;
                }

                // 推測距離のチェック(残り最短距離)
                int remainingMan = Mathf.Abs(n.x - end.x) + Mathf.Abs(n.y - end.y);
                if (cur.Count + remainingMan > maxLen + 1) continue;

                visited[n.x, n.y] = true;
                cur.Add(n);
                Dfs(n);
                cur.RemoveAt(cur.Count - 1);
                visited[n.x, n.y] = false;
            }
        }

        Dfs(start);
        return results;
    }

    /// <summary>
    /// BFSで 始点から終点 の最短距離を求める。
    /// </summary>
    /// <param name="tileOccupied"> タイルが置かれているセルをtrueとする2次元配列 </param>
    /// <param name="occupiedPaths"> 確定している経路で使用しているセルをtrueとする2次元配列 </param>
    /// <param name="start"> 開始セル </param>
    /// <param name="end"> 終点セル </param>
    /// <returns></returns>
    private int ShortestDistance(bool[,] tileOccupied, bool[,] occupiedPaths, Vector2Int start, Vector2Int end)
    {
        if (start == end) return 0;
        bool[,] vis = new bool[gridSize, gridSize];
        Queue<(Vector2Int pos, int dist)> q = new Queue<(Vector2Int, int)>();
        q.Enqueue((start, 0));
        vis[start.x, start.y] = true;

        Vector2Int[] dirs = new Vector2Int[] {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1)
        };

        while (q.Count > 0)
        {
            var (pos, dist) = q.Dequeue();
            foreach (var d in dirs)
            {
                Vector2Int n = pos + d;
                if (n.x < 0 || n.x >= gridSize || n.y < 0 || n.y >= gridSize) continue;
                if (vis[n.x, n.y]) continue;

                if (n == end) return dist + 1;

                if (tileOccupied[n.x, n.y]) continue;
                if (occupiedPaths[n.x, n.y]) continue;

                vis[n.x, n.y] = true;
                q.Enqueue((n, dist + 1));
            }
        }
        return -1;
    }

    /// <summary>
    /// 2つのセルとその種類を保持
    /// </summary>
    /// <param name="pos"> 配置先のセル処理 </param>
    /// <param name="type"> 2つのセルとその種類の盤面を配置 </param>
    private void PlaceTile(Vector2Int pos, string type)
    {
        if (tilePrefab == null || gridParent == null) return;

        GameObject go = Instantiate(tilePrefab, gridParent);
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null && lineManager != null)
        {
            // LineManager のセル->anchoredPosition を使って位置合わせ
            rt.anchoredPosition = lineManager.CellToAnchored(pos);
            Vector2 cellSize = GetCellSize();
            rt.sizeDelta = cellSize;
        }

        Tile tile = go.GetComponent<Tile>();
        if (tile != null)
        {
            tile.Setup(pos.x, pos.y, type, this);
            tiles[pos.x, pos.y] = tile;

            int idx = allTypes.IndexOf(type);
            if (idx < 0) idx = 0;
            if (typeColors != null && typeColors.Length > 0)
            {
                UnityEngine.UI.Image img = go.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.color = typeColors[idx % typeColors.Length];
            }
        }
    }

    /// <summary>
    /// セルサイズの計算
    /// </summary>
    /// <returns> セル1つの幅と高さ(デフォルトは(40,40)) </returns>
    private Vector2 GetCellSize()
    {
        if (gridParent == null) return new Vector2(40f, 40f);
        return new Vector2(gridParent.rect.width / gridSize, gridParent.rect.height / gridSize);
    }

    /// <summary>
    /// リストをランダムにシャッフル(配置の偏りを防ぐ)
    /// </summary>
    /// <typeparam name="T"> シャッフルの要素型 </typeparam>
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = rand.Next(i, list.Count);
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}
