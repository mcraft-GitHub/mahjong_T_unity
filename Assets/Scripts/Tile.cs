using UnityEngine;

/// <summary>
/// 盤上の1マスを表すタイルのクラス
/// </summary>
public class Tile : MonoBehaviour
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

    private bool _isSelected = false;

    private Renderer _renderer;
    private Collider _collider;

    private Color _originalColor = Color.white;
    [SerializeField] private Color _selectedColor = Color.yellow;
    [SerializeField] private Color _matchedColor = new Color(1f, 0.9f, 0.6f, 1f);
    [SerializeField] private Color _hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    private MaterialPropertyBlock _mpb;
    private static readonly int _colorPropId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();

        _mpb = new MaterialPropertyBlock();

        if (_renderer != null && _renderer.sharedMaterial != null && _renderer.sharedMaterial.HasProperty(_colorPropId))
            _originalColor = _renderer.sharedMaterial.GetColor(_colorPropId);

        ApplyColor(_originalColor);
    }

    /// <summary>
    /// タイル初期化
    /// </summary>
    /// <param name="row"> 行 </param>
    /// <param name="col"> 列 </param>
    /// <param name="type"> 種類 </param>
    public void Setup(int row, int col, string type)
    {
        _row = row;
        _col = col;
        _type = type;
        _isMatched = false;
        _isSelected = false;
        if (_collider != null) _collider.enabled = true;
        ApplyColor(_originalColor);
    }

    /// <summary>
    /// タイル選択時の表示変更
    /// </summary>
    public void Select()
    {
        if (_isMatched) return;
        _isSelected = true;
        ApplyColor(_selectedColor);
    }

    /// <summary>
    /// 選択解除時の表示変更
    /// </summary>
    public void Deselect()
    {
        if (_isMatched) return;
        _isSelected = false;
        ApplyColor(_originalColor);
    }

    /// <summary>
    /// タイルがマッチした時の処理
    /// </summary>
    public void Match()
    {
        _isMatched = true;
        _isSelected = false;
        ApplyColor(_matchedColor);
        if (_collider != null) _collider.enabled = false;

        transform.Rotate(180f, 0f, 0f, Space.Self);
    }

    /// <summary>
    /// 色の適応
    /// </summary>
    /// <param name="c"> 色 </param>
    private void ApplyColor(Color c)
    {
        if (_renderer == null) return;
        _mpb.SetColor(_colorPropId, c);
        _renderer.SetPropertyBlock(_mpb);
    }
}
