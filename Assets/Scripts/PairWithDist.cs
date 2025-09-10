using UnityEngine;

/// <summary>
/// ペアとその最短距離をまとめた構造体
/// </summary>
public class PairWithDist
{
    public PairPlacement _pair;
    public int _dist;
    public PairWithDist(PairPlacement p, int d)
    {
        _pair = p;
        _dist = d;
    }
}
