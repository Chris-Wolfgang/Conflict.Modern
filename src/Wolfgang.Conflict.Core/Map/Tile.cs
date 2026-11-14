using Wolfgang.Conflict.Core.Catalog;

namespace Wolfgang.Conflict.Core.Map;

/// <summary>
/// A single hex on the map. Carries terrain, an optional structure, and the
/// faction id that currently controls the structure (if any).
/// </summary>
/// <param name="Terrain">Base terrain of the hex.</param>
/// <param name="Structure">Optional structure on the hex.</param>
/// <param name="OwnerFactionId">
/// Faction id controlling the structure, or <see langword="null"/> if the
/// structure is neutral or there is no structure.
/// </param>
public sealed record Tile
(
    Terrain Terrain,
    StructureType Structure = StructureType.None,
    string? OwnerFactionId = null
);
