using Wolfgang.Conflict.Core.Catalog;
using Wolfgang.Conflict.Core.Combat;

namespace Wolfgang.Conflict.Core.Tests.Unit.Combat;

/// <summary>
/// Sample weapons and quick attempt-builders for combat resolver tests.
/// Shaped to mirror the real catalog (M4, sabot, Hellfire, AMRAAM).
/// </summary>
internal static class CombatTestFixture
{
    public static WeaponSystemDefinition SmallArms() => new
    (
        Id: "smallarms",
        DisplayName: "M4 Carbine",
        MinRange: 1,
        MaxRange: 1,
        Accuracy: 70,
        DamageByArmor: new Dictionary<ArmorClass, int>
        {
            [ArmorClass.Infantry] = 8,
            [ArmorClass.LightArmor] = 2
        },
        AmmoCapacity: 30
    );


    public static WeaponSystemDefinition TankSabot() => new
    (
        Id: "sabot",
        DisplayName: "120mm Sabot",
        MinRange: 1,
        MaxRange: 3,
        Accuracy: 60,
        DamageByArmor: new Dictionary<ArmorClass, int>
        {
            [ArmorClass.Infantry] = 4,
            [ArmorClass.LightArmor] = 14,
            [ArmorClass.HeavyArmor] = 16
        },
        AmmoCapacity: 40
    );


    public static WeaponSystemDefinition AirToAirMissile() => new
    (
        Id: "amraam",
        DisplayName: "AIM-120",
        MinRange: 2,
        MaxRange: 6,
        Accuracy: 75,
        DamageByArmor: new Dictionary<ArmorClass, int>
        {
            [ArmorClass.Aircraft] = 16
        },
        AmmoCapacity: 4
    );


    public static CombatAttempt Attempt
    (
        WeaponSystemDefinition weapon,
        ArmorClass defenderArmorClass = ArmorClass.Infantry,
        int defenderArmor = 1,
        int defenderDexterity = 3,
        int defenderLuck = 1,
        Terrain defenderTerrain = Terrain.Plain,
        int distance = 1,
        bool hasLineOfSight = true
    ) => new
    (
        Weapon: weapon,
        DefenderArmorClass: defenderArmorClass,
        DefenderArmor: defenderArmor,
        DefenderDexterity: defenderDexterity,
        DefenderLuck: defenderLuck,
        DefenderTerrain: defenderTerrain,
        Distance: distance,
        HasLineOfSight: hasLineOfSight
    );
}
