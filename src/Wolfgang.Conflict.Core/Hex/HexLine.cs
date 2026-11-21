namespace Wolfgang.Conflict.Core.Hex;

/// <summary>
/// Hex grid line drawing — used by LOS algorithms to enumerate the hexes
/// a straight line crosses between two coordinates.
/// </summary>
/// <remarks>
/// Standard algorithm: convert both ends to cube coordinates, linearly
/// interpolate per axis, round each sample back to the nearest hex.
/// Reference: <see href="https://www.redblobgames.com/grids/hexagons/#line-drawing"/>.
/// </remarks>
public static class HexLine
{
    /// <summary>
    /// Hexes along the straight line from <paramref name="a"/> to <paramref name="b"/>,
    /// inclusive of both endpoints. A single-element list is returned when
    /// the endpoints are equal.
    /// </summary>
    public static IReadOnlyList<HexCoord> Draw(HexCoord a, HexCoord b)
    {
        var n = a.DistanceTo(b);
        var result = new List<HexCoord>(n + 1);

        for (var i = 0; i <= n; i++)
        {
            var t = n == 0 ? 0.0 : (double)i / n;
            var q = a.Q + (b.Q - a.Q) * t;
            var r = a.R + (b.R - a.R) * t;
            var s = a.S + (b.S - a.S) * t;
            result.Add(CubeRound(q, r, s));
        }

        return result;
    }


    private static HexCoord CubeRound(double q, double r, double s)
    {
        var rq = (int)Math.Round(q, MidpointRounding.AwayFromZero);
        var rr = (int)Math.Round(r, MidpointRounding.AwayFromZero);
        var rs = (int)Math.Round(s, MidpointRounding.AwayFromZero);

        var dq = Math.Abs(rq - q);
        var dr = Math.Abs(rr - r);
        var ds = Math.Abs(rs - s);

        if (dq > dr && dq > ds)
        {
            rq = -rr - rs;
        }
        else if (dr > ds)
        {
            rr = -rq - rs;
        }

        // S is implicit in (Q, R); no need to repair it explicitly.
        return new HexCoord(rq, rr);
    }
}
