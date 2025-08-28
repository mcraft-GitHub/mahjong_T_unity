using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// �Տ��1�}�X��\���^�C���̃N���X
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
    /// �^�C��������
    /// </summary>
    /// <param name="row"> �s </param>
    /// <param name="col"> �� </param>
    /// <param name="type"> ��� </param>
    /// <param name="gameRef"> �Q�[���{�̂ւ̎Q�� </param>
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
    /// �^�C���I�����̕\���ύX
    /// </summary>
    public void Select()
    {
        if (_image != null && !_isMatched)
            _image.color = Color.yellow;
    }

    /// <summary>
    /// �I���������̕\���ύX
    /// </summary>
    public void Deselect()
    {
        if (_image != null && !_isMatched)
            _image.color = _originalColor;
    }

    /// <summary>
    /// �^�C�����}�b�`�������̏���
    /// </summary>
    public void Match()
    {
        _isMatched = true;
        if (_image != null)
            _image.color = new Color(1f, 0.9f, 0.6f, 1f);
        if (_tileText != null)
            _tileText.text = ""; // �}�b�`��͕�����\��
    }

    /// <summary>
    /// �^�C�����N���b�N���ꂽ���ɌĂ΂�鏈��
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_game == null) return;
        
        // ���ɑI�𒆂̃^�C��������ꍇ�́A�I���^�C���Ƃ̃}�b�`����
        if (_game._selectedTilesCount == 0)
        {
            _game.OnTileClicked(this);
        }
        else
        {
            // �^�C���N���b�N�ł̓h���b�O�J�n�Ƃ��đI�����X�V����
            _game.OnTileClicked(this);
        }
    }
}