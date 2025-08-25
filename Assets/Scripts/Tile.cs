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
    /// �^�C��������
    /// </summary>
    /// <param name="row"> �s </param>
    /// <param name="col"> �� </param>
    /// <param name="type"> ��� </param>
    /// <param name="gameRef"> �Q�[���{�̂ւ̎Q�� </param>
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
    /// �^�C���I�����̕\���ύX
    /// </summary>
    public void Select()
    {
        if (image != null && !IsMatched)
            image.color = Color.yellow;
    }

    /// <summary>
    /// �I���������̕\���ύX
    /// </summary>
    public void Deselect()
    {
        if (image != null && !IsMatched)
            image.color = originalColor;
    }

    /// <summary>
    /// �^�C�����}�b�`�������̏���
    /// </summary>
    public void Match()
    {
        IsMatched = true;
        if (image != null)
            image.color = Color.gray;
        if (tileText != null)
            tileText.text = ""; // �}�b�`��͕�����\��
    }

    /// <summary>
    /// �^�C�����N���b�N���ꂽ���ɌĂ΂�鏈��
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (game == null) return;
        
        // ���ɑI�𒆂̃^�C��������ꍇ�́A�I���^�C���Ƃ̃}�b�`����
        if (game.SelectedTilesCount == 0)
        {
            game.OnTileClicked(this);
        }
        else
        {
            // �^�C���N���b�N�ł̓h���b�O�J�n�Ƃ��đI�����X�V����
            game.OnTileClicked(this);
        }
    }
}