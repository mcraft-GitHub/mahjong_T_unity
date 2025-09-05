using UnityEngine;

public class TileClickHandler : MonoBehaviour
{
    private GameManager _gm;
    private Tile _tile;

    public void Setup(GameManager gm, Tile tile)
    {
        _gm = gm;
        _tile = tile;
    }

    void OnMouseDown()
    {
        _gm.OnTileClicked(_tile);
    }
}
