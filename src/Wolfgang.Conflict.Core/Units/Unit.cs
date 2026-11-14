using Wolfgang.Conflict.Core.Hex;

namespace Wolfgang.Conflict.Core.Units;

/// <summary>
/// A runtime instance of a unit on the map. Static stats live on the
/// referenced <c>UnitTypeDefinition</c>; this record carries only the
/// per-unit mutable state that varies during play.
/// </summary>
/// <param name="Id">Stable per-game identifier (engine-assigned).</param>
/// <param name="UnitTypeId">Reference into the owning faction's unit catalog.</param>
/// <param name="OwnerFactionId">Faction id that controls this unit.</param>
/// <param name="Position">Current hex on the map.</param>
/// <param name="CurrentHp">Remaining hit points (0 means destroyed).</param>
/// <param name="CurrentFuel">Remaining fuel (0 transitions ground units to <see cref="UnitStatus.OutOfFuel"/>).</param>
/// <param name="HasMoved">Whether this unit has moved during the current turn.</param>
/// <param name="HasAttacked">Whether this unit has attacked during the current turn.</param>
/// <param name="Status">Operational status.</param>
public sealed record Unit
(
    int Id,
    string UnitTypeId,
    string OwnerFactionId,
    HexCoord Position,
    int CurrentHp,
    int CurrentFuel,
    bool HasMoved,
    bool HasAttacked,
    UnitStatus Status
);
