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
    public Vector2Int Cell;
    public int Row { get; private set; }
    public int Col { get; private set; }
    public string Type { get; private set; }
    public bool IsMatched { get; private set; } = false;

    private JanChainGame game;
    private Image image;
    private Color originalColor;
    [SerializeField] private TMP_Text tileText;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image != null)
            originalColor = image.color;
        if (tileText == null)
            tileText = GetComponentInChildren<TMP_Text>();
    }

    /// <summary>
    /// タイル初期化
    /// </summary>
    /// <param name="row"> 行 </param>
    /// <param name="col"> 列 </param>
    /// <param name="type"> 種類 </param>
    /// <param name="gameRef"> ゲーム本体への参照 </param>
    public void Setup(int row, int col, string type, JanChainGame gameRef)
    {
        Row = row;
        Col = col;
        Type = type;
        game = gameRef;
        IsMatched = false;
        if (tileText != null) tileText.text = type;
        if (image != null) image.color = originalColor;
    }

    /// <summary>
    /// タイル選択時の表示変更
    /// </summary>
    public void Select()
    {
        if (image != null && !IsMatched)
            image.color = Color.yellow;
    }

    /// <summary>
    /// 選択解除時の表示変更
    /// </summary>
    public void Deselect()
    {
        if (image != null && !IsMatched)
            image.color = originalColor;
    }

    /// <summary>
    /// タイルがマッチした時の処理
    /// </summary>
    public void Match()
    {
        IsMatched = true;
        if (image != null)
            image.color = Color.gray;
        if (tileText != null)
            tileText.text = ""; // マッチ後は文字非表示
    }

    /// <summary>
    /// タイルがクリックされた時に呼ばれる処理
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (game == null) return;
        
        // 既に選択中のタイルがある場合は、選択タイルとのマッチ判定
        if (game.SelectedTilesCount == 0)
        {
            game.OnTileClicked(this);
        }
        else
        {
            // タイルクリックではドラッグ開始として選択を更新する
            game.OnTileClicked(this);
        }
    }
}