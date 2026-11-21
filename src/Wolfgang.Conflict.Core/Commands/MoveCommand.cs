using Wolfgang.Conflict.Core.Hex;

namespace Wolfgang.Conflict.Core.Commands;

/// <summary>
/// Move <see cref="UnitId"/> along <see cref="Path"/>, ending at the
/// final hex. The path must start at the unit's current position and
/// be a sequence of adjacent hexes the unit can afford.
/// </summary>
/// <param name="UnitId">Id of the unit to move.</param>
/// <param name="Path">
/// Hexes traversed, starting at the unit's current position and ending
/// at the destination. Costs are summed against the unit's movement points.
/// </param>
public sealed record MoveCommand(int UnitId, IReadOnlyList<HexCoord> Path) : Command;
