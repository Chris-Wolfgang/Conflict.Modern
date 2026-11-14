using Wolfgang.Conflict.Core.Hex;

namespace Wolfgang.Conflict.Core.Events;

/// <summary>
/// A unit moved along the given path, paying the given total cost.
/// </summary>
/// <param name="UnitId">The unit that moved.</param>
/// <param name="Path">Hexes traversed from start to destination, inclusive.</param>
/// <param name="TotalCost">Movement points spent on this move.</param>
public sealed record UnitMoved(int UnitId, IReadOnlyList<HexCoord> Path, int TotalCost) : GameEvent;
