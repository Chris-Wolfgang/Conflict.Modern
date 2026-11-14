namespace Wolfgang.Conflict.Core.Hex;

/// <summary>
/// A path through a hex grid, expressed as an ordered sequence of hexes
/// from start to goal (inclusive) and the total movement cost incurred.
/// </summary>
/// <param name="Hexes">The hexes traversed, starting at the start hex and ending at the goal (inclusive).</param>
/// <param name="TotalCost">The sum of edge costs along the path.</param>
public sealed record HexPath(IReadOnlyList<HexCoord> Hexes, int TotalCost);
