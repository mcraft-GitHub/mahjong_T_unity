using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 盤面に描画される線を管理するクラス
/// </summary>
public class LineManager : MonoBehaviour
{
    private const float MIDPOINT_FACTOR = 0.5f;

    [Header("Grid Settings")]
    [SerializeField] public int _gridSize = 8;
    [SerializeField] private float _cellSpacing = 1.1f;
    [SerializeField] private Transform _lineContainer;

    [Header("Line Prefab")]
    [SerializeField] private GameObject _linePrefab;
    [SerializeField] private float _lineThickness = 0.1f;
    [SerializeField] private float _segmentThinness = 0.5f; 
    [SerializeField] private float _scaleCompensation = 0.5f;

    // 行・列
    public int _rows = 8;
    public int _columns = 8;

    // 固定線とHover線を管理
    private readonly Dictionary<(Vector2Int, Vector2Int), GameObject> _fixedLines = new();
    private readonly HashSet<Vector2Int> _fixedOccupiedCells = new();
    private readonly Dictionary<(Vector2Int, Vector2Int), GameObject> _hoverLines = new();

    /// <summary>
    /// セル座標 → ワールド座標
    /// </summary>
    public Vector3 CellToWorld(Vector2Int cell)
    {
        float halfGrid = (_gridSize - 1) * 0.5f;
        float x = (cell.x - halfGrid) * _cellSpacing;
        float y = (cell.y - halfGrid) * _cellSpacing;
        return new Vector3(x, y, 0f);
    }

    /// <summary>
    /// スクリーン座標 → セル座標
    /// </summary>
    public bool ScreenToCell(Vector3 screenPos, out Vector2Int cell)
    {
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 worldPos = ray.GetPoint(enter);
            int x = Mathf.RoundToInt(worldPos.x / _cellSpacing + (_gridSize - 1) * 0.5f);
            int y = Mathf.RoundToInt(worldPos.y / _cellSpacing + (_gridSize - 1) * 0.5f);
            cell = new Vector2Int(x, y);
            return Inside(cell);
        }

        cell = Vector2Int.zero;
        return false;
    }

    /// <summary>
    /// 盤面内かどうか
    /// </summary>
    public bool Inside(Vector2Int c)
    {
        return c.x >= 0 && c.x < _gridSize && c.y >= 0 && c.y < _gridSize;
    }

    /// <summary>
    /// 確定線がそのセルにあるか
    /// </summary>
    public bool HasFixedOnCell(Vector2Int cell)
    {
        return _fixedOccupiedCells.Contains(cell);
    }

    /// <summary>
    /// Hover線を描画
    /// </summary>
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
    /// Hover線を全消去
    /// </summary>
    public void ClearHoverLines()
    {
        foreach (var go in _hoverLines.Values)
            Destroy(go);
        _hoverLines.Clear();
    }

    /// <summary>
    /// Hover線を確定線に昇格
    /// </summary>
    public void CommitHoverPath(List<Vector2Int> path, Color color)
    {
        if (path == null || path.Count < 2)
        {
            ClearHoverLines();
            return;
        }

        ClearHoverLines();
        for (int i = 0; i < path.Count - 1; i++)
        {
            PlaceSegment(path[i], path[i + 1], color, false);
        }

        foreach (var c in path)
        {
            _fixedOccupiedCells.Add(c);
        }
    }

    /// <summary>
    /// 全ての線を削除
    /// </summary>
    public void ClearAllLines()
    {
        foreach (var go in _fixedLines.Values)
            Destroy(go);
        foreach (var go in _hoverLines.Values)
            Destroy(go);
        _fixedLines.Clear();
        _hoverLines.Clear();
        _fixedOccupiedCells.Clear();
    }

    /// <summary>
    /// Grid再計算時に全線を再配置
    /// </summary>
    public void RecalcGrid()
    {
        foreach (var kv in _fixedLines)
        {
            Vector2Int from = kv.Key.Item1;
            Vector2Int to = kv.Key.Item2;
            UpdateSegmentTransform(kv.Value.transform, from, to);
        }
        foreach (var kv in _hoverLines)
        {
            Vector2Int from = kv.Key.Item1;
            Vector2Int to = kv.Key.Item2;
            UpdateSegmentTransform(kv.Value.transform, from, to);
        }
    }

    /// <summary>
    /// 線オブジェクトを生成
    /// </summary>
    private GameObject PlaceSegment(Vector2Int from, Vector2Int to, Color color, bool isHover)
    {
        GameObject go = Instantiate(_linePrefab, _lineContainer);

        Vector3 p0 = CellToWorld(from);
        Vector3 p1 = CellToWorld(to);

        UpdateSegmentTransform(go.transform, from, to);

        if (go.TryGetComponent<Renderer>(out var renderer))
            renderer.material.color = color;

        if (isHover)
            _hoverLines[(from, to)] = go;
        else
            _fixedLines[(from, to)] = go;

        return go;
    }

    /// <summary>
    /// 線のTransformを from→to のセルに合わせて調整
    /// </summary>
    private void UpdateSegmentTransform(Transform tr, Vector2Int from, Vector2Int to)
    {
        Vector3 p0 = CellToWorld(from);
        Vector3 p1 = CellToWorld(to);
        Vector3 mid = (p0 + p1) * MIDPOINT_FACTOR;

        tr.position = mid;
        Vector3 dir = (p1 - p0).normalized;

        // 回転
        tr.rotation = Quaternion.FromToRotation(Vector3.right, dir) * Quaternion.Euler(0f, 0f, 90f);

        float length = Vector3.Distance(p0, p1);
        tr.localScale = new Vector3(length * _segmentThinness, _lineThickness, _lineThickness * _segmentThinness * _scaleCompensation);
    }
}