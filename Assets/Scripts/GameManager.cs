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
    [SerializeField] private BoardManager _boardManager;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private LineManager _lineManager;

    private int _matchedPairs = 0;
    private bool _gameActive = false;

    // �h���b�O���̌o�H
    private readonly List<Vector2Int> _hoverPath = new();
    private bool _isDragging = false;
    private Color _dragColor = new Color(0.2f, 0.9f, 1f, 1f);
    // �m���
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

        // �^�C����1�����I������Ă���ꍇ�Ƀh���b�O���������
        if (_selectedTiles.Count == 1)
        {
            var startTile = _selectedTiles[0];
            var startCell = new Vector2Int(startTile._row, startTile._col);

            // �h���b�O�J�n
            if (!_isDragging && Input.GetMouseButtonDown(0))
            {
                // �p�X�̏�����
                _hoverPath.Clear();
                _hoverPath.Add(startCell);
                _isDragging = true;
                _lineManager.DrawHoverPath(_hoverPath, _dragColor);
            }
            // �L�΂�(�h���b�O��)
            if (_isDragging && Input.GetMouseButton(0))
            {
                if (_lineManager.ScreenToCell(Input.mousePosition, out var cell))
                {
                    TryExtendOrBacktrack(cell, startTile);
                }
            }
            // �I��(�����m��)
            if (_isDragging && Input.GetMouseButtonUp(0))
            {
                FinishDrag(startTile);
            }
        }
        else
        {
            // �I���������E2�ȏ�Ȃ�h���b�O����
            if (_isDragging)
            {
                _isDragging = false;
                _hoverPath.Clear();
                _lineManager.ClearHoverLines();
            }
        }
    }

    /// <summary>
    /// �h���b�N���̏���(�o�H�̉���,�o�H��߂鏈��)
    /// </summary>
    /// <param name="cell"> ���݃J�[�\���̂���Z�����W </param>
    /// <param name="startTile"> �h���b�N�J�n�Z�� </param>
    private void TryExtendOrBacktrack(Vector2Int cell, Tile startTile)
    {
        if (_hoverPath.Count == 0) return;

        var last = _hoverPath[_hoverPath.Count - 1];
        if (cell == last) return;

        // 1�߂�
        if (_hoverPath.Count >= 2 && cell == _hoverPath[_hoverPath.Count - 2])
        {
            _hoverPath.RemoveAt(_hoverPath.Count - 1);
            _lineManager.DrawHoverPath(_hoverPath, _dragColor);
            return;
        }

        // �ߐڎ��̂݋���
        if (!IsAdjacent(last, cell)) return;

        // ���Ɋm������ʂ��Ă���Z���s��
        if (_lineManager.HasFixedOnCell(cell)) return;

        // ���Ɏ����̃h���b�O�o�H�Œʂ��Ă���Z���s��
        for (int i = 0; i < _hoverPath.Count - 1; i++)
            if (_hoverPath[i] == cell) return;

        // �^�C���̑��ݔ���
        Tile t = _boardManager.TileAt(cell);
        bool isStart = (cell.x == startTile._row && cell.y == startTile._col);

        if (t != null && !isStart)
        {
            // �}�b�`��� or �u���b�N
            if (!t._isMatched && t._type == startTile._type)
            {
                // �I�_�͉�
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

        // �󂫃Z���Ȃ̂ŉ���
        _hoverPath.Add(cell);
        _lineManager.DrawHoverPath(_hoverPath, _dragColor);
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
        _isDragging = false;

        if (_hoverPath.Count < 2)
        {
            _lineManager.ClearHoverLines();
            _hoverPath.Clear();
            return;
        }

        var endCell = _hoverPath[_hoverPath.Count - 1];
        var endTile = _boardManager.TileAt(endCell);

        // ����^�C���Ȃ�m��
        if (endTile != null && !endTile._isMatched && endTile != startTile && endTile._type == startTile._type)
        {
            ConfirmMatch(_hoverPath);
            _hoverPath.Clear();
            _selectedTiles.Clear();
        }
        else
        {
            // ���s�F��������
            _lineManager.ClearHoverLines();
            _hoverPath.Clear();
        }
    }

    /// <summary>
    /// �o�H����������������񂾎��̂݌Ă΂��
    /// </summary>
    /// <param name="path"> �o�H�Z���� </param>
    private void ConfirmMatch(List<Vector2Int> path)
    {
        if (path == null || path.Count < 2) return;

        Tile first = _boardManager.TileAt(path[0]);
        Tile last = _boardManager.TileAt(path[path.Count - 1]);
        if (first == null || last == null) return;
        if (first._isMatched || last._isMatched) return;
        if (first._type != last._type) return;

        // �^�C���m��
        first.Match();
        last.Match();

        // �����m��
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
    /// �Ֆʂ̃��Z�b�g
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
    /// �N���b�N���̏���
    /// </summary>
    /// <param name="tile"></param>
    public void OnTileClicked(Tile tile)
    {
        if (!_gameActive || tile == null || tile._isMatched) return;

        // �I������������1�I��
        foreach (var t in _selectedTiles)
            t.Deselect();
        _selectedTiles.Clear();

        _selectedTiles.Add(tile);
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
