namespace Wolfgang.Conflict.Core.Hex;

/// <summary>
/// Axial coordinate of a hex on a pointy-top hex grid.
/// </summary>
/// <remarks>
/// Uses the axial coordinate system (Q, R) where the third cube axis
/// <c>S = -Q - R</c> is implicit. R grows downward (screen-friendly).
/// Reference: <see href="https://www.redblobgames.com/grids/hexagons/"/>.
/// </remarks>
/// <param name="Q">The Q (column-ish) axis.</param>
/// <param name="R">The R (row-ish) axis. Grows downward on screen.</param>
public readonly record struct HexCoord(int Q, int R)
{
    /// <summary>
    /// The origin hex, <c>(0, 0)</c>.
    /// </summary>
    public static HexCoord Origin => new(0, 0);

    /// <summary>
    /// The implicit third cube coordinate, <c>S = -Q - R</c>.
    /// </summary>
    public int S => -Q - R;

    /// <summary>
    /// Returns the immediate neighbor in the given direction.
    /// </summary>
    public HexCoord Neighbor(HexDirection direction)
    {
        var (dq, dr) = AxialOffsets[(int)direction];
        return new HexCoord(Q + dq, R + dr);
    }

    /// <summary>
    /// Enumerates the six immediate neighbors, clockwise from East.
    /// </summary>
    public IEnumerable<HexCoord> Neighbors()
    {
        for (var i = 0; i < 6; i++)
        {
            var (dq, dr) = AxialOffsets[i];
            yield return new HexCoord(Q + dq, R + dr);
        }
    }

    /// <summary>
    /// Hex distance (number of steps along the grid) from this hex to <paramref name="other"/>.
    /// </summary>
    public int DistanceTo(HexCoord other)
    {
        var dq = Q - other.Q;
        var dr = R - other.R;
        return (Math.Abs(dq) + Math.Abs(dq + dr) + Math.Abs(dr)) / 2;
    }

    /// <inheritdoc/>
    public override string ToString() => $"({Q}, {R})";

    /// <summary>
    /// Axial (dq, dr) offsets for each <see cref="HexDirection"/>, indexed by enum value.
    /// </summary>
    private static readonly (int dq, int dr)[] AxialOffsets =
    [
        ( +1,  0), // East
        (  0, +1), // SouthEast
        ( -1, +1), // SouthWest
        ( -1,  0), // West
        (  0, -1), // NorthWest
        ( +1, -1)  // NorthEast
    ];
}
