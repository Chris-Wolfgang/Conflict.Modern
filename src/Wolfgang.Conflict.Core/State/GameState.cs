using Wolfgang.Conflict.Core.Hex;
using Wolfgang.Conflict.Core.Map;
using Wolfgang.Conflict.Core.Units;

namespace Wolfgang.Conflict.Core.State;

/// <summary>
/// Immutable snapshot of an in-progress match. Every command produces a
/// new <see cref="GameState"/> via <c>GameEngine.Apply</c> — there is no
/// in-place mutation. This makes save/load, replay, undo, and network
/// state synchronization trivial.
/// </summary>
/// <param name="Map">Static terrain and structures.</param>
/// <param name="Units">All living units across all sides, keyed by unit id.</param>
/// <param name="SideOrder">Faction ids in the order they take turns.</param>
/// <param name="CurrentSideIndex">Index into <paramref name="SideOrder"/> of the side whose turn it is.</param>
/// <param name="TurnNumber">1-based round number; increments after every side has played.</param>
/// <param name="Rules">Match rules selected at setup.</param>
/// <param name="RngSeed">Initial seed for the deterministic combat RNG.</param>
/// <param name="RngStep">Number of random draws consumed so far (advances on every RNG call).</param>
public sealed record GameState
(
    HexMap Map,
    IReadOnlyDictionary<int, Unit> Units,
    IReadOnlyList<string> SideOrder,
    int CurrentSideIndex,
    int TurnNumber,
    RulesConfig Rules,
    int RngSeed,
    int RngStep
)
{
    /// <summary>Faction id of the side whose turn it currently is.</summary>
    public string CurrentSideFactionId => SideOrder[CurrentSideIndex];


    /// <summary>Returns the unit at the given hex, or <see langword="null"/> if the hex is empty.</summary>
    public Unit? UnitAt(HexCoord coord)
    {
        foreach (var unit in Units.Values)
        {
            if (unit.Position == coord)
            {
                return unit;
            }
        }
        return null;
    }


    /// <summary>Whether <paramref name="coord"/> is currently occupied by any unit.</summary>
    public bool IsHexOccupied(HexCoord coord) => UnitAt(coord) is not null;


    /// <summary>All units owned by the given faction.</summary>
    public IEnumerable<Unit> UnitsOfFaction(string factionId) =>
        Units.Values.Where(u => string.Equals(u.OwnerFactionId, factionId, StringComparison.Ordinal));
}
