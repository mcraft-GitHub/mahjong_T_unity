using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 盤上 を管理するクラス
/// </summary>
public class BoardManager : MonoBehaviour
{
    public event Action OnSceneChangeRequest;

    [Header("Game Settings")]
    public int _gridSize = 8;
    public int _maxTileTypes = 6;
    public float _cellSpacing = 1.1f;

    [Header("Tile / Grid Prefabs")]
    [SerializeField] private List<GameObject> _tilePrefabs;
    [SerializeField] private Transform _gridParent;
    [SerializeField] private LineManager _lineManager;

    private Tile[,] _tiles;
    private List<Tile> _selectedTiles = new();
    private int _totalPairs = 0;
    // 経路
    private HashSet<Vector2Int> _occupiedPathCells = new HashSet<Vector2Int>();

    private System.Random _rand = new System.Random();

    static readonly int MAX_PATHS_PER_PAIR = 40;
    static readonly int MAX_PLACEMENT_ATTEMPTS = 800;
    static readonly int PATH_SLACK = 6;

    // 初期通過不可タイル
    [SerializeField] private GameObject _blockPrefab;
    [SerializeField] private int _randomBlockCount = 5;
    private HashSet<Vector2Int> _blockCells = new HashSet<Vector2Int>();

    // 探索方向(上下左右)
    static readonly Vector2Int[] DIRS = 
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    /// <summary>
    /// ゲームの盤面をリセット
    /// </summary>
    public void ResetBoard()
    {
        ClearGrid();
        if (_tiles != null)
        {
            foreach (var tile in _tiles)
            {
                if (tile != null && tile.gameObject != null)
                {
                    Destroy(tile.gameObject);
                }
            }
        }

        _tiles = new Tile[_gridSize, _gridSize];
        _selectedTiles.Clear();
        _totalPairs = 0;
        Shuffle(_tilePrefabs);

        ResetLine();

        _blockCells.Clear();
        GenerateBoardSafe();
        // 通過不可タイルの設定
        AddRandomBlocks();
    }

    /// <summary>
    /// ランダムに通過不可タイルを追加(ペアタイルとその経路以外)
    /// </summary>
    private void AddRandomBlocks()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (var x = 0; x < _gridSize; x++)
        {
            for (var y = 0; y < _gridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                if (_blockCells.Contains(pos)) continue;
                if (_tiles[x, y] != null) continue;
                if (_occupiedPathCells.Contains(pos)) continue;

                candidates.Add(pos);
            }
        }

        Shuffle(candidates);

        int blockCount = Mathf.Min(_randomBlockCount, candidates.Count);
        for (var i = 0; i < blockCount; i++)
        {
            Vector2Int pos = candidates[i];
            int x = pos.x;
            int y = pos.y;

            _blockCells.Add(pos);

            if (_blockPrefab != null)
            {
                GameObject go = Instantiate(_blockPrefab, _gridParent);

                float xOffset = (x - (_gridSize - 1) * 0.5f) * _cellSpacing;
                float yOffset = (y - (_gridSize - 1) * 0.5f) * _cellSpacing;

                go.transform.localPosition = new Vector3(xOffset, yOffset, 0f);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                Tile blockTile = go.GetComponent<Tile>();
                if (blockTile != null)
                {
                    blockTile.Setup(x, y, null);
                    _tiles[x, y] = blockTile;
                }
            }
        }
    }

    /// <summary>
    /// ランダム配置
    /// </summary>
    private void GenerateBoardSafe()
    {
        // 決定する種類数
        int typeCount = Mathf.Min(_maxTileTypes, _tilePrefabs.Count);

        // 使用するプレハブをシャッフルして、ペアごとに選択
        List<GameObject> prefabPool = new List<GameObject>(_tilePrefabs);
        Shuffle(prefabPool);
        prefabPool = prefabPool.GetRange(0, typeCount);

        for (var attempt = 0; attempt < MAX_PLACEMENT_ATTEMPTS; attempt++)
        {
            // 盤面セルリスト
            List<Vector2Int> allCells = new List<Vector2Int>();
            for (var x = 0; x < _gridSize; x++)
            {
                for (var y = 0; y < _gridSize; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (_blockCells.Contains(pos)) continue;
                    allCells.Add(pos);
                }
            }
            // ペアを作れるだけセルが残っていない場合はスキップ
            if (allCells.Count < typeCount * 2) continue;

            Shuffle(allCells);

            List<Vector2Int> chosen = allCells.GetRange(0, typeCount * 2);

            // ランダムにペアにする
            Shuffle(chosen);
            List<PairPlacement> pairs = new List<PairPlacement>();
            for (var i = 0; i < typeCount; i++)
            {
                Vector2Int a = chosen[i * 2];
                Vector2Int b = chosen[i * 2 + 1];
                pairs.Add(new PairPlacement(a, b, $"Tile{i}"));
            }

            // 全ペアを重複しないパスで繋げるか
            if (TryRouteAllPairs(pairs))
            {
                // 成功：UI に反映して完了
                PlaceTilesFromPairs(pairs, prefabPool);
                _totalPairs = typeCount;
                return;
            }
        }

        HandlePlacementFailure();
    }

    /// <summary>
    /// 配置失敗,タイトル画面に戻す
    /// </summary>
    private void HandlePlacementFailure()
    {
        OnSceneChangeRequest?.Invoke();
    }

    /// <summary>
    /// ペアリスト を UI に反映
    /// </summary>
    private void PlaceTilesFromPairs(List<PairPlacement> pairs, List<GameObject> types)
    {
        // 盤面のタイル配列を初期化
        _tiles = new Tile[_gridSize, _gridSize];

        for (var i = 0; i < pairs.Count; i++)
        {
            var p = pairs[i];
            GameObject prefab = _tilePrefabs[i % _tilePrefabs.Count];
            PlaceTile(p._pairPlacementA, p._type, prefab);
            PlaceTile(p._pairPlacementB, p._type, prefab);
        }
    }

    /// <summary>
    /// お互いに干渉しない経路で繋げるか
    /// </summary>
    private bool TryRouteAllPairs(List<PairPlacement> pairs)
    {
        // 経路セルの初期化
        _occupiedPathCells.Clear();

        // 全てのペアの両端を占有
        bool[,] tileOccupied = new bool[_gridSize, _gridSize];
        foreach (var p in pairs)
        {
            tileOccupied[p._pairPlacementA.x, p._pairPlacementA.y] = true;
            tileOccupied[p._pairPlacementB.x, p._pairPlacementB.y] = true;
        }

        bool[,] occupiedPaths = new bool[_gridSize, _gridSize]; // 空

        // 難しいペアから処理した方が良い(最短距離が長いものから先に処理)
        var pairInfo = new List<PairWithDist>();
        foreach (var p in pairs)
        {
            int d = ShortestDistance(tileOccupied, occupiedPaths, p._pairPlacementA, p._pairPlacementB);
            
            // 到達不可
            if (d < 0) d = int.MaxValue;
            pairInfo.Add(new PairWithDist(p, d));
        }
        // 長い距離(難しい)を先に
        pairInfo.Sort((x, y) => y._dist.CompareTo(x._dist));

        // 作り直した順序の pairsOrdered を作る
        var pairsOrdered = new List<PairPlacement>();
        foreach (var pw in pairInfo) pairsOrdered.Add(pw._pair);

        // 再帰でルーティング
        var chosenPaths = new List<List<Vector2Int>>(); // optional: store chosen paths
        bool ok = RoutePairsRecursive(pairsOrdered, 0, tileOccupied, occupiedPaths, chosenPaths);
        if (ok)
        {
            for (int i = 0; i < chosenPaths.Count; i++)
            {
                for (int j = 0; j < chosenPaths[i].Count; j++)
                {
                    _occupiedPathCells.Add(chosenPaths[i][j]);
                }
            }
        }
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
        if (idx >= pairs.Count) return true;

        var p = pairs[idx];
        var cellArray = EnumeratePaths(p._pairPlacementA, p._pairPlacementB, tileOccupied, occupiedPaths, MAX_PATHS_PER_PAIR, PATH_SLACK);

        if (cellArray == null || cellArray.Count == 0) return false;

        for (var x = 0; x < cellArray.Count; x++)
        {
            var occupiedList = new List<Vector2Int>();
            for (var y = 0; y < cellArray[x].Count; y++)
            {
                var cell = cellArray[x][y];
                if (cell == p._pairPlacementA || cell == p._pairPlacementB) continue;
                if (!occupiedPaths[cell.x, cell.y])
                {
                    occupiedPaths[cell.x, cell.y] = true;
                    occupiedList.Add(cell);
                }
            }

            // 次のペアを処理
            chosenPaths.Add(cellArray[x]);
            if (RoutePairsRecursive(pairs, idx + 1, tileOccupied, occupiedPaths, chosenPaths))
                return true;

            // バックトラック
            chosenPaths.RemoveAt(chosenPaths.Count - 1);
            foreach (var c in occupiedList)
                occupiedPaths[c.x, c.y] = false;
        }


        return false;
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
        int shortest = ShortestDistance(tileOccupied, occupiedPaths, start, end);
        if (shortest < 0) return new List<List<Vector2Int>>();

        int maxLen = shortest + slack;
        List<List<Vector2Int>> results = new List<List<Vector2Int>>();
        bool[,] visited = new bool[_gridSize, _gridSize];
        List<Vector2Int> cur = new List<Vector2Int> { start };
        visited[start.x, start.y] = true;

        void Dfs(Vector2Int node)
        {
            if (results.Count >= maxPaths) return;
            if (cur.Count > maxLen + 1) return;
            if (node == end)
            {
                results.Add(new List<Vector2Int>(cur));
                return;
            }

            // 近い順に並び変えて探索(ヒューリスティック)
            var neighbors = new List<Vector2Int>();
            foreach (var d in DIRS)
                neighbors.Add(node + d);

            neighbors.Sort((a, b) => (Mathf.Abs(a.x - end.x) + Mathf.Abs(a.y - end.y)).CompareTo(Mathf.Abs(b.x - end.x) + Mathf.Abs(b.y - end.y)));

            foreach (var n in neighbors)
            {
                if (CanVisit(n, end, tileOccupied, occupiedPaths, visited, cur, maxLen))
                {
                    // 再起探索
                    visited[n.x, n.y] = true;
                    cur.Add(n);
                    Dfs(n);
                    cur.RemoveAt(cur.Count - 1);
                    visited[n.x, n.y] = false;
                }
            }
        }

        Dfs(start);
        return results;
    }

    /// <summary>
    /// 再起探索の実行条件処理
    /// </summary>
    /// <param name="n"> 判定対象のセル座標 </param>
    /// <param name="end"> 探索の終点セル座標 </param>
    /// <param name="tileOccupied"> 盤面上で固定タイルが配置されているかを示すフラグ配列 </param>
    /// <param name="occupiedPaths"> 既に確定した経路セルを示すフラグ配列 </param>
    /// <param name="visited"> 今回の探索で既に訪問済みかどうかのフラグ配列 </param>
    /// <param name="cur"> 現在の探索経路リスト </param>
    /// <param name="maxLen"> 探索経路の許容最大長 </param>
    /// <returns></returns>
    private bool CanVisit(Vector2Int n, Vector2Int end, bool[,] tileOccupied, bool[,] occupiedPaths, bool[,] visited, List<Vector2Int> cur, int maxLen)
    {
        // 盤面外
        if (n.x < 0 || n.x >= _gridSize || n.y < 0 || n.y >= _gridSize)
            return false;

        // 訪問済み
        if (visited[n.x, n.y])
            return false;

        bool isEnd = (n == end);
        if (!isEnd)
        {
            // タイル配置セル・既存の経路セル
            if (tileOccupied[n.x, n.y])
                return false;
            if (occupiedPaths[n.x, n.y])
                return false;
        }

        // 上限オーバー判定
        int remainingMan = Mathf.Abs(n.x - end.x) + Mathf.Abs(n.y - end.y);
        if (cur.Count + remainingMan > maxLen + 1)
            return false;

        return true;
    }

    /// <summary>
    /// 始点から終点 の最短距離を求める。
    /// </summary>
    /// <param name="tileOccupied"> タイルが置かれているセルをtrueとする2次元配列 </param>
    /// <param name="occupiedPaths"> 確定している経路で使用しているセルをtrueとする2次元配列 </param>
    /// <param name="start"> 開始セル </param>
    /// <param name="end"> 終点セル </param>
    /// <returns></returns>
    private int ShortestDistance(bool[,] tileOccupied, bool[,] occupiedPaths, Vector2Int start, Vector2Int end)
    {
        if (start == end) return 0;
        bool[,] vis = new bool[_gridSize, _gridSize];
        Queue<(Vector2Int pos, int dist)> q = new Queue<(Vector2Int, int)>();
        q.Enqueue((start, 0));
        vis[start.x, start.y] = true;
        Vector2Int[] dirs = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        while (q.Count > 0)
        {
            var (pos, dist) = q.Dequeue();
            foreach (var d in dirs)
            {
                Vector2Int n = pos + d;
                if (n.x < 0 || n.x >= _gridSize || n.y < 0 || n.y >= _gridSize) continue;
                if (vis[n.x, n.y]) continue;
                if (n == end) return dist + 1;
                if (tileOccupied[n.x, n.y]) continue;
                if (occupiedPaths[n.x, n.y]) continue;
                if (_blockCells.Contains(n)) continue;

                vis[n.x, n.y] = true;
                q.Enqueue((n, dist + 1));
            }
        }
        return -1;
    }

    /// <summary>
    /// 指定プレハブを使ってタイルを配置する
    /// </summary>
    private void PlaceTile(Vector2Int pos, string type, GameObject prefab)
    {
        if (prefab == null || _gridParent == null) return;

        GameObject go = Instantiate(prefab, _gridParent);

        // 左下原点に揃える
        float xOffset = (pos.x - (_gridSize - 1) * 0.5f) * _cellSpacing;
        float yOffset = (pos.y - (_gridSize - 1) * 0.5f) * _cellSpacing;
        float zFixed = 0f;

        go.transform.localPosition = new Vector3(xOffset, yOffset, zFixed);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        Tile tile = go.GetComponent<Tile>();
        if (tile != null)
        {
            tile.Setup(pos.x, pos.y, type);
            _tiles[pos.x, pos.y] = tile;
        }
    }

    /// <summary>
    /// リストをランダムにシャッフル(配置の偏りを防ぐ)
    /// </summary>
    /// <typeparam name="T"> シャッフルの要素型 </typeparam>
    /// <param name="list"></param>
    private void Shuffle<T>(List<T> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            int j = _rand.Next(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// 指定セルが通過不可タイルかどうか
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public bool IsCellBlock(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= _gridSize || cell.y < 0 || cell.y >= _gridSize) return false;
        return _blockCells.Contains(cell);
    }

    /// <summary>
    /// lineのリセット
    /// </summary>
    public void ResetLine()
    {
        if (_lineManager == null) return;

        _lineManager._rows = _gridSize;
        _lineManager._columns = _gridSize;
        _lineManager.ClearAllLines();
        _lineManager.RecalcGrid();
    }

    /// <summary>
    /// 全ペアの数を渡す
    /// </summary>
    /// <returns></returns>
    public int GetTotalPairs()
    {
        return _totalPairs;
    }

    /// <summary>
    /// 指定セルを返す
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public Tile TileAt(Vector2Int cell)
    {
        if (_tiles == null) return null;
        if (cell.x < 0 || cell.x >= _tiles.GetLength(0) || cell.y < 0 || cell.y >= _tiles.GetLength(1)) return null;
        if (_blockCells.Contains(cell)) return null;
        return _tiles[cell.x, cell.y];
    }

    /// <summary>
    /// 全タイルの状態をリセット
    /// </summary>
    public void ResetTilesState()
    {
        if (_tiles == null) return;

        foreach (var tile in _tiles)
        {
            if (tile != null)
            {
                tile.ResetState();
            }
        }
    }

    /// <summary>
    /// 全タイルの削除
    /// </summary>
    private void ClearGrid()
    {
        if (_gridParent == null) return;

        foreach (Transform child in _gridParent)
        {
            Destroy(child.gameObject);
        }
    }
}
