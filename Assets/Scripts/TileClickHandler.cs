using UnityEngine;

/// <summary>
/// �}�E�X���͂̌��m,�^�C���I���E�������s���N���X
/// </summary>
public class TileSelector : MonoBehaviour
{
    private Camera _mainCamera;
    private GameManager _gameManager;
    private Tile _draggingTile;

    private void Start()
    {
        GameObject camObj = GameObject.Find("Main Camera");
        _mainCamera = camObj.GetComponent<Camera>();
        _gameManager = Object.FindAnyObjectByType<GameManager>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ���N���b�N
        {
            HandleClick();
        }

        if (Input.GetMouseButton(0)) // �h���b�O
        {
            HandleDrag();
        }
    }

    /// <summary>
    /// �N���b�N���ꂽ�^�C�����擾��,GameManager�ɒʒm
    /// </summary>
    private void HandleClick()
    {
        Tile tile = GetTileUnderMouse();
        if (tile != null)
        {
            _gameManager.OnTileClicked(tile);
        }
    }

    /// <summary>
    /// �h���b�O���̃^�C�����擾��,GameManager�ɒʒm
    /// </summary>
    private void HandleDrag()
    {
        Tile tile = GetTileUnderMouse();
        if (tile == null || tile == _draggingTile) return;
        _draggingTile = tile;
    }

    /// <summary>
    /// �h���b�N���A�J�[�\�����̃^�C�����擾
    /// </summary>
    /// <returns></returns>
    private Tile GetTileUnderMouse()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null) return tile;
        }
        return null;
    }
}
