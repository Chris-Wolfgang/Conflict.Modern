namespace Wolfgang.Conflict.Core.Catalog;

/// <summary>
/// Static definition of a weapon system carried by a unit. Runtime ammo
/// counts and target selection live elsewhere; this record is pure data.
/// </summary>
/// <param name="Id">
/// Stable identifier referenced by <see cref="UnitTypeDefinition.WeaponSystemIds"/>
/// (e.g. <c>"us-rifle-m4"</c>, <c>"us-aim120-amraam"</c>).
/// </param>
/// <param name="DisplayName">Human-readable name shown in the UI.</param>
/// <param name="MinRange">Minimum hex range (inclusive). Typically <c>1</c> for direct-fire weapons.</param>
/// <param name="MaxRange">Maximum hex range (inclusive).</param>
/// <param name="Accuracy">
/// Base to-hit accuracy in 0–100. The combat resolver derives final hit
/// chance by combining this with the defender's effective AC and terrain modifiers.
/// </param>
/// <param name="DamageByArmor">
/// Damage rolled on a successful hit, keyed by the defender's <see cref="ArmorClass"/>.
/// Missing entries mean the weapon cannot meaningfully damage that armor class
/// (treated as zero).
/// </param>
/// <param name="AmmoCapacity">
/// Maximum number of shots a unit can carry of this weapon when fully loaded.
/// </param>
public sealed record WeaponSystemDefinition
(
    string Id,
    string DisplayName,
    int MinRange,
    int MaxRange,
    int Accuracy,
    IReadOnlyDictionary<ArmorClass, int> DamageByArmor,
    int AmmoCapacity
);
