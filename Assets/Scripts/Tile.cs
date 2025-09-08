using UnityEngine;

/// <summary>
/// �Տ��1�}�X��\���^�C���̃N���X
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
    /// �^�C��������
    /// </summary>
    /// <param name="row"> �s </param>
    /// <param name="col"> �� </param>
    /// <param name="type"> ��� </param>
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
    /// �^�C���I�����̕\���ύX
    /// </summary>
    public void Select()
    {
        if (_isMatched) return;
        _isSelected = true;
        ApplyColor(_selectedColor);
    }

    /// <summary>
    /// �I���������̕\���ύX
    /// </summary>
    public void Deselect()
    {
        if (_isMatched) return;
        _isSelected = false;
        ApplyColor(_originalColor);
    }

    /// <summary>
    /// �^�C�����}�b�`�������̏���
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
    /// �F�̓K��
    /// </summary>
    /// <param name="c"> �F </param>
    private void ApplyColor(Color c)
    {
        if (_renderer == null) return;
        _mpb.SetColor(_colorPropId, c);
        _renderer.SetPropertyBlock(_mpb);
    }
}
