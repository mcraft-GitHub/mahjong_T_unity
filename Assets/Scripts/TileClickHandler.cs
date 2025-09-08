using UnityEngine;

/// <summary>
/// マウス入力の検知,タイル選択・処理を行うクラス
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
        if (Input.GetMouseButtonDown(0)) // 左クリック
        {
            HandleClick();
        }

        if (Input.GetMouseButton(0)) // ドラッグ
        {
            HandleDrag();
        }
    }

    /// <summary>
    /// クリックされたタイルを取得し,GameManagerに通知
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
    /// ドラッグ中のタイルを取得し,GameManagerに通知
    /// </summary>
    private void HandleDrag()
    {
        Tile tile = GetTileUnderMouse();
        if (tile == null || tile == _draggingTile) return;
        _draggingTile = tile;
    }

    /// <summary>
    /// ドラック中、カーソル下のタイルを取得
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
