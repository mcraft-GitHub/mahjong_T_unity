using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// 盤上の1マスを表すタイルのクラス
/// </summary>
public class Tile : MonoBehaviour, IPointerClickHandler
{
    public int _row
    {
        get;
        private set;
    }
    public int _col
    {
        get;
        private set;
    }
    public string _type
    {
        get;
        private set;
    }
    public bool _isMatched
    {
        get;
        private set;
    } = false;

    private GameManager _game;
    private Image _image;
    private Color _originalColor;
    [SerializeField] private TMP_Text _tileText;

    private void Awake()
    {
        _image = GetComponent<Image>();
        if (_image != null)
            _originalColor = _image.color;
        if (_tileText == null)
            _tileText = GetComponentInChildren<TMP_Text>();
    }

    /// <summary>
    /// タイル初期化
    /// </summary>
    /// <param name="row"> 行 </param>
    /// <param name="col"> 列 </param>
    /// <param name="type"> 種類 </param>
    /// <param name="gameRef"> ゲーム本体への参照 </param>
    public void Setup(int row, int col, string type, GameManager gameRef)
    {
        _row = row;
        _col = col;
        _type = type;
        _game = gameRef;
        _isMatched = false;
        if (_tileText != null)
            _tileText.text = type;
        if (_image != null)
            _image.color = _originalColor;
    }

    /// <summary>
    /// タイル選択時の表示変更
    /// </summary>
    public void Select()
    {
        if (_image != null && !_isMatched)
            _image.color = Color.yellow;
    }

    /// <summary>
    /// 選択解除時の表示変更
    /// </summary>
    public void Deselect()
    {
        if (_image != null && !_isMatched)
            _image.color = _originalColor;
    }

    /// <summary>
    /// タイルがマッチした時の処理
    /// </summary>
    public void Match()
    {
        _isMatched = true;
        if (_image != null)
            _image.color = new Color(1f, 0.9f, 0.6f, 1f);
        if (_tileText != null)
            _tileText.text = ""; // マッチ後は文字非表示
    }

    /// <summary>
    /// タイルがクリックされた時に呼ばれる処理
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_game == null) return;
        
        // 既に選択中のタイルがある場合は、選択タイルとのマッチ判定
        if (_game._selectedTilesCount == 0)
        {
            _game.OnTileClicked(this);
        }
        else
        {
            // タイルクリックではドラッグ開始として選択を更新する
            _game.OnTileClicked(this);
        }
    }
}