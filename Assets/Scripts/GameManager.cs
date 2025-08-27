using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// GameScene ���Ǘ�����N���X
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private LineManager lineManager;

    private int matchedPairs = 0;
    private bool gameActive = false;

    private readonly List<Vector2Int> hoverPath = new(); // �h���b�O���̌o�H
    private bool isDragging = false;
    private Color dragColor = new Color(0.2f, 0.9f, 1f, 1f);
    private Color fixedColor = new Color(0.15f, 0.8f, 0.2f, 1f); // �m���

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

        // �^�C����1�����I������Ă���ꍇ�Ƀh���b�O���������
        if (selectedTiles.Count == 1)
        {
            var startTile = selectedTiles[0];
            var startCell = new Vector2Int(startTile.Row, startTile.Col);

            // �h���b�O�J�n
            if (!isDragging && Input.GetMouseButtonDown(0))
            {
                // �p�X�̏�����
                hoverPath.Clear();
                hoverPath.Add(startCell);
                isDragging = true;
                lineManager.DrawHoverPath(hoverPath, dragColor);
            }
            // �L�΂�(�h���b�O��)
            if (isDragging && Input.GetMouseButton(0))
            {
                if (lineManager.ScreenToCell(Input.mousePosition, out var cell))
                {
                    TryExtendOrBacktrack(cell, startTile);
                }
            }
            // �I��(�����m��)
            if (isDragging && Input.GetMouseButtonUp(0))
            {
                FinishDrag(startTile);
            }
        }
        else
        {
            // �I���������E2�ȏ�Ȃ�h���b�O����
            if (isDragging) { isDragging = false; hoverPath.Clear(); lineManager.ClearHoverLines(); }
        }
    }

    /// <summary>
    /// �h���b�N���̏���(�o�H�̉���,�o�H��߂鏈��)
    /// </summary>
    /// <param name="cell"> ���݃J�[�\���̂���Z�����W </param>
    /// <param name="startTile"> �h���b�N�J�n�Z�� </param>
    private void TryExtendOrBacktrack(Vector2Int cell, Tile startTile)
    {
        if (hoverPath.Count == 0) return;

        var last = hoverPath[hoverPath.Count - 1];
        if (cell == last) return;

        // 1�߂�
        if (hoverPath.Count >= 2 && cell == hoverPath[hoverPath.Count - 2])
        {
            hoverPath.RemoveAt(hoverPath.Count - 1);
            lineManager.DrawHoverPath(hoverPath, dragColor);
            return;
        }

        // �ߐڎ��̂݋���
        if (!IsAdjacent(last, cell)) return;

        // ���Ɋm������ʂ��Ă���Z���s��
        if (lineManager.HasFixedOnCell(cell)) return;

        // ���Ɏ����̃h���b�O�o�H�Œʂ��Ă���Z���s��
        for (int i = 0; i < hoverPath.Count - 1; i++)
            if (hoverPath[i] == cell) return;

        // �^�C���̑��ݔ���
        Tile t = boardManager.TileAt(cell);
        bool isStart = (cell.x == startTile.Row && cell.y == startTile.Col);

        if (t != null && !isStart)
        {
            // �}�b�`��� or �u���b�N
            if (!t.IsMatched && t.Type == startTile.Type)
            {
                // �I�_�͉�
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

        // �󂫃Z���Ȃ̂ŉ���
        hoverPath.Add(cell);
        lineManager.DrawHoverPath(hoverPath, dragColor);
    }

    /// <summary>
    /// �w�肵���^�C�����ׂɂ��邩
    /// </summary>
    /// <param name="a"> �Z��A���W </param>
    /// <param name="b"> �Z��B���W </param>
    /// <returns></returns>
    private static bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) == 1;
    }

    /// <summary>
    /// �h���b�N����I�����A�}�b�`����,���s����
    /// </summary>
    /// <param name="startTile"> �h���b�N�J�n�^�C�� </param>
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

        // ����^�C���Ȃ�m��
        if (endTile != null && !endTile.IsMatched && endTile != startTile && endTile.Type == startTile.Type)
        {
            ConfirmMatch(hoverPath);
            hoverPath.Clear();
            selectedTiles.Clear();
        }
        else
        {
            // ���s�F��������
            lineManager.ClearHoverLines();
            hoverPath.Clear();
        }
    }

    /// <summary>
    /// �o�H����������������񂾎��̂݌Ă΂��
    /// </summary>
    /// <param name="path"> �o�H�Z���� </param>
    private void ConfirmMatch(List<Vector2Int> path)
    {
        if (path == null || path.Count < 2) return;

        Tile first = boardManager.TileAt(path[0]);
        Tile last = boardManager.TileAt(path[path.Count - 1]);
        if (first == null || last == null) return;
        if (first.IsMatched || last.IsMatched) return;
        if (first.Type != last.Type) return;

        // �^�C���m��
        first.Match();
        last.Match();

        // �����m��
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
    /// �Ֆʂ̃��Z�b�g
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
    /// �N���b�N���̏���
    /// </summary>
    /// <param name="tile"></param>
    public void OnTileClicked(Tile tile)
    {
        if (!gameActive || tile == null || tile.IsMatched) return;

        // �I������������1�I��
        foreach (var t in selectedTiles) t.Deselect();
        selectedTiles.Clear();

        selectedTiles.Add(tile);
        tile.Select();
    }

    /// <summary>
    /// ResultScene �� �V�[���J��
    /// </summary>
    void ChangeResultScene()
    {
        SceneManager.LoadScene("ResultScene");
    }
}
