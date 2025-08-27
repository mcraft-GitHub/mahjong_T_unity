using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �Տ� ���Ǘ�����N���X
/// </summary>
public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("Game Settings")]
    public int gridSize = 8;
    public int maxTileTypes = 6;

    [Header("Tile / Grid UI")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private LineManager lineManager;

    [Header("Colors")]
    [SerializeField] private Color[] typeColors;

    private Tile[,] tiles;
    private List<Tile> selectedTiles = new();
    private int totalPairs = 0;

    private readonly List<string> allTypes = new() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
    private System.Random rand = new System.Random();
    private int maxPlacementAttempts = 800;
    private int maxPathsPerPair = 40;
    private int pathSlack = 6;


    /// <summary>
    /// �Q�[���̔Ֆʂ����Z�b�g
    /// </summary>
    public void ResetBoard()
    {
        if (tiles != null)
        {
            foreach (var t in tiles)
            {
                if (t != null && t.gameObject != null)
                    Destroy(t.gameObject);
            }
        }
        tiles = new Tile[gridSize, gridSize];
        selectedTiles.Clear();
        totalPairs = 0;

        ResetLine();

        GenerateBoardSafe();
    }

    /// <summary>
    /// �����_���z�u
    /// </summary>
    private void GenerateBoardSafe()
    {
        // ���肷���ސ�
        int typeCount = Mathf.Min(maxTileTypes, allTypes.Count);
        List<string> types = new List<string>(allTypes);
        Shuffle(types);
        types = types.GetRange(0, typeCount);

        bool success = false;
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            // �����_���� 2*typeCount �̃Z����I��
            List<Vector2Int> allCells = new List<Vector2Int>();
            for (int x = 0; x < gridSize; x++)
                for (int y = 0; y < gridSize; y++)
                    allCells.Add(new Vector2Int(x, y));
            Shuffle(allCells);

            List<Vector2Int> chosen = allCells.GetRange(0, typeCount * 2);

            // �����_���Ƀy�A�ɂ���
            Shuffle(chosen);
            var pairs = new List<PairPlacement>();
            for (int i = 0; i < typeCount; i++)
            {
                Vector2Int a = chosen[2 * i];
                Vector2Int b = chosen[2 * i + 1];
                pairs.Add(new PairPlacement(a, b, types[i]));
            }

            // �S�y�A���d�����Ȃ��p�X�Ōq���邩
            if (TryRouteAllPairs(pairs))
            {
                // �����FUI �ɔ��f���Ċ���
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
            // �t�H�[���o�b�N
            FallbackPlace(types);
        }
    }

    /// <summary>
    /// �o�H�𖳎����ă����_���Ƀy�A��z�u����t�H�[���o�b�N����
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
    /// �y�A���X�g �� UI �ɔ��f
    /// </summary>
    private void PlaceTilesFromPairs(List<PairPlacement> pairs)
    {
        // �Ֆʂ̃^�C���z���������
        tiles = new Tile[gridSize, gridSize];

        for (int i = 0; i < pairs.Count; i++)
        {
            var p = pairs[i];
            PlaceTile(p.a, p.type);
            PlaceTile(p.b, p.type);
        }
    }

    /// <summary>
    /// ���݂��Ɋ����Ȃ��o�H�Ōq���邩
    /// </summary>
    private bool TryRouteAllPairs(List<PairPlacement> pairs)
    {
        // �S�Ẵy�A�̗��[���L
        bool[,] tileOccupied = new bool[gridSize, gridSize];
        foreach (var p in pairs)
        {
            tileOccupied[p.a.x, p.a.y] = true;
            tileOccupied[p.b.x, p.b.y] = true;
        }

        bool[,] occupiedPaths = new bool[gridSize, gridSize]; // ��

        // ����y�A���珈�����������ǂ�(�ŒZ�������������̂����ɏ���)
        var pairInfo = new List<PairWithDist>();
        foreach (var p in pairs)
        {
            int d = ShortestDistance(tileOccupied, occupiedPaths, p.a, p.b);
            if (d < 0) d = int.MaxValue; // ���B�s��
            pairInfo.Add(new PairWithDist(p, d));
        }
        // ��������(���)����
        pairInfo.Sort((x, y) => y.dist.CompareTo(x.dist));

        // ��蒼���������� pairsOrdered �����
        var pairsOrdered = new List<PairPlacement>();
        foreach (var pw in pairInfo) pairsOrdered.Add(pw.pair);

        // �ċA�Ń��[�e�B���O
        var chosenPaths = new List<List<Vector2Int>>(); // optional: store chosen paths
        bool ok = RoutePairsRecursive(pairsOrdered, 0, tileOccupied, occupiedPaths, chosenPaths);
        return ok;
    }

    /// <summary>
    /// �ċA�I�Ƀy�A�̌o�H��T�����đS�ĂȂ���
    /// </summary>
    /// <param name="pairs"></param>
    /// <param name="idx"></param>
    /// <param name="tileOccupied"></param>
    /// <param name="occupiedPaths"></param>
    /// <param name="chosenPaths"></param>
    /// <returns></returns>
    private bool RoutePairsRecursive(List<PairPlacement> pairs, int idx, bool[,] tileOccupied, bool[,] occupiedPaths, List<List<Vector2Int>> chosenPaths)
    {
        // �S�y�A�����ς݂Ȃ琬��
        if (idx >= pairs.Count) return true;

        var p = pairs[idx];

        // ���p�X���
        var candidates = EnumeratePaths(p.a, p.b, tileOccupied, occupiedPaths, maxPathsPerPair, pathSlack);

        if (candidates == null || candidates.Count == 0)
        {
            // ���B�s��
            return false;
        }

        foreach (var path in candidates)
        {
            // �o�H�Z�����ꎞ�I�ɐ�L�����ɂ���
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
                else { }
            }

            chosenPaths.Add(path);
            // �ċA
            if (RoutePairsRecursive(pairs, idx + 1, tileOccupied, occupiedPaths, chosenPaths))
                return true;

            // ���[���o�b�N
            chosenPaths.RemoveAt(chosenPaths.Count - 1);
            foreach (var c in occupiedList) occupiedPaths[c.x, c.y] = false;
        }

        return false; // �S�o�H���s
    }

    /// <summary>
    /// �y�A�� �n�_����I�_�܂ł̌o�H��DFS�ŗ�
    /// </summary>
    /// <param name="start"> �J�n�Z�� </param>
    /// <param name="end"> �I�_�Z�� </param>
    /// <param name="tileOccupied"> �^�C�����u����Ă���Z����true�Ƃ���2�����z�� </param>
    /// <param name="occupiedPaths"> �m�肵�Ă���o�H�Ŏg�p���Ă���Z����true�Ƃ���2�����z�� </param>
    /// <param name="maxPaths"> �񋓂���o�H�̍ő吔 </param>
    /// <param name="slack"> �T���Ō������o�H(�Z�����W���X�g)�̃��X�g </param>
    /// <returns></returns>
    private List<List<Vector2Int>> EnumeratePaths(Vector2Int start, Vector2Int end, bool[,] tileOccupied, bool[,] occupiedPaths, int maxPaths, int slack)
    {
        // �܂��ŒZ������ BFS �ŋ��߂�
        int shortest = ShortestDistance(tileOccupied, occupiedPaths, start, end);
        if (shortest < 0) return new List<List<Vector2Int>>();

        int maxLen = shortest + slack;

        List<List<Vector2Int>> results = new List<List<Vector2Int>>();
        bool[,] visited = new bool[gridSize, gridSize];
        List<Vector2Int> cur = new List<Vector2Int>();
        cur.Add(start);
        visited[start.x, start.y] = true;

        // �q���[���X�e�B�b�N�ɋ߂���������T�����邽�߂ɗאڏ����\�[�g
        void Dfs(Vector2Int node)
        {
            if (results.Count >= maxPaths) return;
            if (cur.Count > maxLen + 1) return;

            if (node == end)
            {
                results.Add(new List<Vector2Int>(cur));
                return;
            }

            // �אڂ��擾
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

                // �ʍs�\������
                bool isEnd = (n == end);
                if (!isEnd)
                {
                    if (tileOccupied[n.x, n.y]) continue;
                    if (occupiedPaths[n.x, n.y]) continue;
                }

                // ���������̃`�F�b�N(�c��ŒZ����)
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
    /// BFS�� �n�_����I�_ �̍ŒZ���������߂�B
    /// </summary>
    /// <param name="tileOccupied"> �^�C�����u����Ă���Z����true�Ƃ���2�����z�� </param>
    /// <param name="occupiedPaths"> �m�肵�Ă���o�H�Ŏg�p���Ă���Z����true�Ƃ���2�����z�� </param>
    /// <param name="start"> �J�n�Z�� </param>
    /// <param name="end"> �I�_�Z�� </param>
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
    /// 2�̃Z���Ƃ��̎�ނ�ێ�
    /// </summary>
    /// <param name="pos"> �z�u��̃Z������ </param>
    /// <param name="type"> 2�̃Z���Ƃ��̎�ނ̔Ֆʂ�z�u </param>
    private void PlaceTile(Vector2Int pos, string type)
    {
        if (tilePrefab == null || gridParent == null) return;

        GameObject go = Instantiate(tilePrefab, gridParent);
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null && lineManager != null)
        {
            // LineManager �̃Z��->anchoredPosition ���g���Ĉʒu���킹
            rt.anchoredPosition = lineManager.CellToAnchored(pos);
            Vector2 cellSize = GetCellSize();
            rt.sizeDelta = cellSize;
        }

        Tile tile = go.GetComponent<Tile>();
        if (tile != null)
        {
            tile.Setup(pos.x, pos.y, type, gameManager);
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
    /// �Z���T�C�Y�̌v�Z
    /// </summary>
    /// <returns> �Z��1�̕��ƍ���(�f�t�H���g��(40,40)) </returns>
    private Vector2 GetCellSize()
    {
        if (gridParent == null) return new Vector2(40f, 40f);
        return new Vector2(gridParent.rect.width / gridSize, gridParent.rect.height / gridSize);
    }

    /// <summary>
    /// ���X�g�������_���ɃV���b�t��(�z�u�̕΂��h��)
    /// </summary>
    /// <typeparam name="T"> �V���b�t���̗v�f�^ </typeparam>
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

    /// <summary>
    /// line�̃��Z�b�g
    /// </summary>
    public void ResetLine()
    {
        if (lineManager == null) return;

        lineManager.rows = gridSize;
        lineManager.columns = gridSize;
        lineManager.ClearAllLines();
        lineManager.RecalcGrid();
    }

    /// <summary>
    /// �S�y�A�̐���n��
    /// </summary>
    /// <returns></returns>
    public int GetTotalPairs()
    {
        return totalPairs;
    }

    /// <summary>
    /// �w��Z����Ԃ�
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
}
