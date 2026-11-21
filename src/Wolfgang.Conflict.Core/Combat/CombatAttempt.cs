using Wolfgang.Conflict.Core.Catalog;

namespace Wolfgang.Conflict.Core.Combat;

/// <summary>
/// Inputs to a single shot from one weapon at one defender. The
/// <c>GameEngine</c> assembles this from the current state and feeds it
/// to <see cref="CombatResolver.Resolve"/>; the resolver is otherwise
/// stateless and engine-agnostic.
/// </summary>
/// <param name="Weapon">The weapon system being fired.</param>
/// <param name="DefenderArmorClass">Defender's armor category — keys the weapon damage table.</param>
/// <param name="DefenderArmor">Flat armor value subtracted from damage on a successful hit.</param>
/// <param name="DefenderDexterity">Defender's evasion contribution to to-hit.</param>
/// <param name="DefenderLuck">Defender's luck contribution to to-hit.</param>
/// <param name="DefenderTerrain">Terrain at the defender's hex (provides cover).</param>
/// <param name="Distance">Hex distance between attacker and defender.</param>
/// <param name="HasLineOfSight">Whether the attacker has LOS to the defender.</param>
public sealed record CombatAttempt
(
    WeaponSystemDefinition Weapon,
    ArmorClass DefenderArmorClass,
    int DefenderArmor,
    int DefenderDexterity,
    int DefenderLuck,
    Terrain DefenderTerrain,
    int Distance,
    bool HasLineOfSight
);
