namespace Wolfgang.Conflict.Core.Catalog;

/// <summary>
/// A complete faction content pack — display metadata plus the catalog of
/// weapon systems and unit types available to that faction.
/// </summary>
/// <remarks>
/// Factions are loaded from JSON files (one per faction) so new factions,
/// alternative-history rosters, or non-modern eras (WW2, sci-fi, fantasy)
/// can ship as data without engine changes.
/// </remarks>
/// <param name="Id">Stable identifier (e.g. <c>"blue"</c>, <c>"red"</c>).</param>
/// <param name="DisplayName">Human-readable name shown in the UI.</param>
/// <param name="Color">Faction color as a CSS hex string (e.g. <c>"#1e88e5"</c>).</param>
/// <param name="WeaponSystems">Weapon systems used by this faction's units, keyed by ID.</param>
/// <param name="UnitTypes">Unit types available to this faction, keyed by ID.</param>
public sealed record FactionDefinition
(
    string Id,
    string DisplayName,
    string Color,
    IReadOnlyDictionary<string, WeaponSystemDefinition> WeaponSystems,
    IReadOnlyDictionary<string, UnitTypeDefinition> UnitTypes
)
{
    /// <summary>
    /// IDs of all unit types in this faction's catalog.
    /// </summary>
    public IEnumerable<string> UnitTypeIds => UnitTypes.Keys;
}
