using UnityEngine;

/// <summary>
/// �y�A�Ƃ��̍ŒZ�������܂Ƃ߂��\����
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
