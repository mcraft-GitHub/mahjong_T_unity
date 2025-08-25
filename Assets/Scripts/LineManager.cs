using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �Ֆʂɕ`�悳�������Ǘ�����N���X
/// </summary>

public class LineManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private RectTransform panel;         // �Q�[���Ֆʂ̃p�l��
    [SerializeField] private RectTransform lineContainer; // �����i�[���� RectTransform(LineContainer)
    [SerializeField] public int rows = 8;                // �s��
    [SerializeField] public int columns = 8;             // ��

    [Header("Line Prefab")]
    [SerializeField] private GameObject linePrefab;

    [Header("Style")]
    [SerializeField] private float lineThickness = 6f; // ����

    // �m����̉��`��
    private readonly Dictionary<(Vector2Int, Vector2Int), GameObject> fixedLines = new();
    private readonly HashSet<Vector2Int> fixedOccupiedCells = new();

    // �h���b�O���̐�
    private readonly Dictionary<(Vector2Int, Vector2Int), GameObject> hoverLines = new();

    private float GridWidth => panel.rect.width;
    private float GridHeight => panel.rect.height;

    /// <summary>
    /// �Z�����W��UI���W�ɕϊ�
    /// </summary>
    /// <param name="cell"> ��,�s�̃Z�����W </param>
    /// <returns> �Z���̒��S���W </returns>
    public Vector2 CellToAnchored(Vector2Int cell)
    {
        float cw = GridWidth / columns;
        float ch = GridHeight / rows;
        float x = -GridWidth / 2f + cell.y * cw + cw / 2f;
        float y = GridHeight / 2f - cell.x * ch - ch / 2f;
        return new Vector2(x, y);
    }

    /// <summary>
    /// ��ʍ��W���Z�����W�ɕϊ�
    /// </summary>
    /// <param name="screenPos"> �X�N���[�����W </param>
    /// <param name="cell"> �ϊ���̃Z�����W </param>
    /// <returns> �Ֆʓ����ǂ��� </returns>
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
    /// �Z�����Ֆʓ����ǂ���
    /// </summary>
    /// <param name="c"> ���肷��Z�� </param>
    /// <returns> �͈͓��Ȃ� true </returns>
    public bool Inside(Vector2Int c) => c.x >= 0 && c.x < rows && c.y >= 0 && c.y < columns;

    /// <summary>
    /// �m����Z���̒ʍs�s�`�F�b�N
    /// </summary>
    /// <param name="cell"> �Z�����W </param>
    /// <returns> �m���������� true </returns>
    public bool HasFixedOnCell(Vector2Int cell) => fixedOccupiedCells.Contains(cell);

    /// <summary>
    /// 2�Z���Ԃɐ��𐶐����Ĕz�u
    /// </summary>
    /// <param name="from"> �J�n�Z�� </param>
    /// <param name="to"> �I�_�Z�� </param>
    /// <param name="color"> ���̐F </param>
    /// <param name="isHover"> �h���b�N���Ȃ� true </param>
    /// <returns> �������ꂽ���̃I�u�W�F�N�g </returns>
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
    /// �h���b�O���̌o�H��`��
    /// </summary>
    /// <param name="path"> �o�H�̃Z���̗� </param>
    /// <param name="color"> ���̐F </param>
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
    /// �h���b�O���̉�����S�폜
    /// </summary>
    public void ClearHoverLines()
    {
        foreach (var go in hoverLines.Values) Object.Destroy(go);
        hoverLines.Clear();
    }

    /// <summary>
    /// ���̊m�艻
    /// </summary>
    /// <param name="path"> �m�肳����o�H </param>
    /// <param name="fixedColor"> �m����̐F </param>
    public void CommitHoverPath(List<Vector2Int> path, Color fixedColor)
    {
        if (path == null || path.Count < 2) 
        {
            ClearHoverLines(); return;
        }

        // �������������āA�Œ���Ƃ��čĕ`��
        ClearHoverLines();
        for (int i = 0; i < path.Count - 1; i++)
        {
            PlaceSegment(path[i], path[i + 1], fixedColor, false);
        }
        // ��L�Z���ɒǉ�
        foreach (var c in path) fixedOccupiedCells.Add(c);
    }

    /// <summary>
    /// �S�Ă̐����폜
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
    /// �ՖʃT�C�Y�ύX���Ȃǂ̍ă��C�A�E�g
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
    /// RectTransform �̐��I�u�W�F�N�g�� from�J���I�Pto �̃Z���Ԃɍ��킹�ĕό`
    /// </summary>
    /// <param name="rt"> ���I�u�W�F�N�g�� RectTransform </param>
    /// <param name="from"> �J�n�Z�� </param>
    /// <param name="to"> �I�_�Z�� </param>
    private void UpdateSegmentTransform(RectTransform rt, Vector2Int from, Vector2Int to)
    {
        Vector2 p0 = CellToAnchored(from);
        Vector2 p1 = CellToAnchored(to);

        // ���_�ɔz�u
        rt.anchoredPosition = (p0 + p1) * 0.5f;

        // �������Z���ԋ����ɁA������ lineThickness �ɐݒ�
        rt.sizeDelta = new Vector2(Vector2.Distance(p0, p1), lineThickness);

        // from��to �����̊p�x���v�Z���ĉ�]�K�p
        float angle = Mathf.Atan2(p1.y - p0.y, p1.x - p0.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);
    }
}