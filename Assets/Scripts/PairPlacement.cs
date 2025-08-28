using UnityEngine;

/// <summary>
/// PairîzóÒ ÇÃ èÓïÒ
/// </summary>
public class PairPlacement
{
    public Vector2Int _pairPlacementA;
    public Vector2Int _pairPlacementB;
    public string _type;
    public PairPlacement(Vector2Int a, Vector2Int b, string type)
    {
        this._pairPlacementA = a;
        this._pairPlacementB = b;
        this._type = type;
    }
}
