using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 盤面に描画される線を管理するクラス
/// </summary>

public class LineManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private RectTransform panel;         // ゲーム盤面のパネル
    [SerializeField] private RectTransform lineContainer; // 線を格納する RectTransform(LineContainer)
    [SerializeField] public int rows = 8;                // 行数
    [SerializeField] public int columns = 8;             // 列数

    [Header("Line Prefab")]
    [SerializeField] private GameObject linePrefab;

    [Header("Style")]
    [SerializeField] private float lineThickness = 6f; // 太さ

    // 確定線の仮描画
    private readonly Dictionary<(Vector2Int, Vector2Int), GameObject> fixedLines = new();
    private readonly HashSet<Vector2Int> fixedOccupiedCells = new();

    // ドラッグ中の線
    private readonly Dictionary<(Vector2Int, Vector2Int), GameObject> hoverLines = new();

    private float GridWidth => panel.rect.width;
    private float GridHeight => panel.rect.height;

    /// <summary>
    /// セル座標をUI座標に変換
    /// </summary>
    /// <param name="cell"> 列,行のセル座標 </param>
    /// <returns> セルの中心座標 </returns>
    public Vector2 CellToAnchored(Vector2Int cell)
    {
        float cw = GridWidth / columns;
        float ch = GridHeight / rows;
        float x = -GridWidth / 2f + cell.y * cw + cw / 2f;
        float y = GridHeight / 2f - cell.x * ch - ch / 2f;
        return new Vector2(x, y);
    }

    /// <summary>
    /// 画面座標をセル座標に変換
    /// </summary>
    /// <param name="screenPos"> スクリーン座標 </param>
    /// <param name="cell"> 変換後のセル座標 </param>
    /// <returns> 盤面内かどうか </returns>
    public bool ScreenToCell(Vector3 screenPos, out Vector2Int cell)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(lineContainer, screenPos, null, out local);

        float cw = GridWidth / columns;
        float ch = GridHeight / rows;

        int y = Mathf.FloorToInt((local.x + GridWidth / 2f) / cw);
        int x = Mathf.FloorToInt((GridHeight / 2f - local.y) / ch);

        cell = new Vector2Int(x, y);
        return Inside(cell);
    }

    /// <summary>
    /// セルが盤面内かどうか
    /// </summary>
    /// <param name="c"> 判定するセル </param>
    /// <returns> 範囲内なら true </returns>
    public bool Inside(Vector2Int c) => c.x >= 0 && c.x < rows && c.y >= 0 && c.y < columns;

    /// <summary>
    /// 確定線セルの通行不可チェック
    /// </summary>
    /// <param name="cell"> セル座標 </param>
    /// <returns> 確定線があれば true </returns>
    public bool HasFixedOnCell(Vector2Int cell) => fixedOccupiedCells.Contains(cell);

    /// <summary>
    /// 2セル間に線を生成して配置
    /// </summary>
    /// <param name="from"> 開始セル </param>
    /// <param name="to"> 終点セル </param>
    /// <param name="color"> 線の色 </param>
    /// <param name="isHover"> ドラック中なら true </param>
    /// <returns> 生成された線のオブジェクト </returns>
    private GameObject PlaceSegment(Vector2Int from, Vector2Int to, Color color, bool isHover)
    {
        GameObject go = Instantiate(linePrefab, lineContainer);
        RectTransform rt = go.GetComponent<RectTransform>();

        Vector2 p0 = CellToAnchored(from);
        Vector2 p1 = CellToAnchored(to);

        rt.anchoredPosition = (p0 + p1) * 0.5f;
        rt.sizeDelta = new Vector2(Vector2.Distance(p0, p1), lineThickness);
        float angle = Mathf.Atan2(p1.y - p0.y, p1.x - p0.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);

        Image img = go.GetComponent<Image>();
        if (img) img.color = color;

        (Vector2Int, Vector2Int) key = (from, to);
        if (isHover) hoverLines[key] = go; else fixedLines[key] = go;
        return go;
    }

    /// <summary>
    /// ドラッグ中の経路を描画
    /// </summary>
    /// <param name="path"> 経路のセルの列 </param>
    /// <param name="color"> 線の色 </param>
    public void DrawHoverPath(List<Vector2Int> path, Color color)
    {
        ClearHoverLines();
        if (path == null || path.Count < 2) return;
        for (int i = 0; i < path.Count - 1; i++)
        {
            PlaceSegment(path[i], path[i + 1], color, true);
        }
    }

    /// <summary>
    /// ドラッグ中の仮線を全削除
    /// </summary>
    public void ClearHoverLines()
    {
        foreach (var go in hoverLines.Values) Object.Destroy(go);
        hoverLines.Clear();
    }

    /// <summary>
    /// 線の確定化
    /// </summary>
    /// <param name="path"> 確定させる経路 </param>
    /// <param name="fixedColor"> 確定線の色 </param>
    public void CommitHoverPath(List<Vector2Int> path, Color fixedColor)
    {
        if (path == null || path.Count < 2) 
        {
            ClearHoverLines(); return;
        }

        // 既存線を消して、固定線として再描画
        ClearHoverLines();
        for (int i = 0; i < path.Count - 1; i++)
        {
            PlaceSegment(path[i], path[i + 1], fixedColor, false);
        }
        // 占有セルに追加
        foreach (var c in path) fixedOccupiedCells.Add(c);
    }

    /// <summary>
    /// 全ての線を削除
    /// </summary>
    public void ClearAllLines()
    {
        foreach (var go in fixedLines.Values) Object.Destroy(go);
        foreach (var go in hoverLines.Values) Object.Destroy(go);
        fixedLines.Clear();
        hoverLines.Clear();
        fixedOccupiedCells.Clear();
    }

    /// <summary>
    /// 盤面サイズ変更時などの再レイアウト
    /// </summary>
    public void RecalcGrid()
    {
        foreach (var kv in fixedLines)
        {
            Vector2Int from = kv.Key.Item1;
            Vector2Int to = kv.Key.Item2;
            RectTransform rt = kv.Value.GetComponent<RectTransform>();
            UpdateSegmentTransform(rt, from, to);
        }
        foreach (var kv in hoverLines)
        {
            Vector2Int from = kv.Key.Item1;
            Vector2Int to = kv.Key.Item2;
            RectTransform rt = kv.Value.GetComponent<RectTransform>();
            UpdateSegmentTransform(rt, from, to);
        }
    }

    /// <summary>
    /// RectTransform の線オブジェクトを fromカラオケto のセル間に合わせて変形
    /// </summary>
    /// <param name="rt"> 線オブジェクトの RectTransform </param>
    /// <param name="from"> 開始セル </param>
    /// <param name="to"> 終点セル </param>
    private void UpdateSegmentTransform(RectTransform rt, Vector2Int from, Vector2Int to)
    {
        Vector2 p0 = CellToAnchored(from);
        Vector2 p1 = CellToAnchored(to);

        // 中点に配置
        rt.anchoredPosition = (p0 + p1) * 0.5f;

        // 長さをセル間距離に、太さを lineThickness に設定
        rt.sizeDelta = new Vector2(Vector2.Distance(p0, p1), lineThickness);

        // from→to 方向の角度を計算して回転適用
        float angle = Mathf.Atan2(p1.y - p0.y, p1.x - p0.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);
    }
}