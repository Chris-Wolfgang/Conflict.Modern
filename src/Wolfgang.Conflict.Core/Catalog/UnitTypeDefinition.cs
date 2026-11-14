namespace Wolfgang.Conflict.Core.Catalog;

/// <summary>
/// Static definition of a unit type. Runtime instance state (current HP,
/// fuel, position, ammo counts) lives on a separate <c>Unit</c> record.
/// </summary>
/// <param name="Id">
/// Stable identifier (e.g. <c>"us-m1a2-abrams"</c>) referenced by
/// <see cref="FactionDefinition.UnitTypeIds"/> and by saved games.
/// </param>
/// <param name="DisplayName">Human-readable name shown in the UI.</param>
/// <param name="Archetype">Coarse role/movement-domain classification.</param>
/// <param name="MaxHp">Hit-point pool when fresh.</param>
/// <param name="FuelCapacity">
/// Maximum fuel when fully loaded. <c>0</c> means the unit does not consume fuel
/// (e.g. infantry).
/// </param>
/// <param name="SightRange">How many hexes the unit can see in clear conditions.</param>
/// <param name="MovementPoints">Movement points granted at the start of each turn.</param>
/// <param name="ArmorClass">Armor category presented to incoming weapons.</param>
/// <param name="Armor">Flat armor bonus contributing to the unit's effective AC.</param>
/// <param name="Dexterity">Evasion contribution to effective AC.</param>
/// <param name="Luck">Random contribution to effective AC.</param>
/// <param name="CanCapture">Whether this unit can capture city/HQ structures.</param>
/// <param name="CanFly">Whether this unit ignores ground-terrain movement costs and crashes when out of fuel.</param>
/// <param name="WeaponSystemIds">
/// IDs of weapon systems this unit carries. Each ID must resolve in the same
/// <see cref="FactionDefinition.WeaponSystems"/> table.
/// </param>
/// <param name="TerrainMovementCosts">
/// Cost in movement points to enter each <see cref="Terrain"/>. Terrains absent from
/// this map are impassable for this unit.
/// </param>
public sealed record UnitTypeDefinition
(
    string Id,
    string DisplayName,
    UnitArchetype Archetype,
    int MaxHp,
    int FuelCapacity,
    int SightRange,
    int MovementPoints,
    ArmorClass ArmorClass,
    int Armor,
    int Dexterity,
    int Luck,
    bool CanCapture,
    bool CanFly,
    IReadOnlyList<string> WeaponSystemIds,
    IReadOnlyDictionary<Terrain, int> TerrainMovementCosts
);
