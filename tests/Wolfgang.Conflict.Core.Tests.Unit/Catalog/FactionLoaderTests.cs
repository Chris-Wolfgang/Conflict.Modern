using Wolfgang.Conflict.Core.Catalog;

namespace Wolfgang.Conflict.Core.Tests.Unit.Catalog;

public class FactionLoaderTests
{
    private const string MinimalFactionJson = """
        {
          "id": "blue",
          "displayName": "Blue",
          "color": "#1e88e5",
          "weapons": [
            {
              "id": "us-rifle-m4",
              "displayName": "M4 Carbine",
              "minRange": 1,
              "maxRange": 1,
              "accuracy": 70,
              "damageByArmor": { "infantry": 8, "lightArmor": 2 },
              "ammoCapacity": 30
            }
          ],
          "units": [
            {
              "id": "us-infantry",
              "displayName": "Infantry Squad",
              "archetype": "infantry",
              "maxHp": 10,
              "fuelCapacity": 0,
              "sightRange": 2,
              "movementPoints": 3,
              "armorClass": "infantry",
              "armor": 1,
              "dexterity": 3,
              "luck": 1,
              "canCapture": true,
              "canFly": false,
              "weaponSystemIds": ["us-rifle-m4"],
              "terrainMovementCosts": { "plain": 1, "forest": 2, "hills": 2, "urban": 1 }
            }
          ]
        }
        """;


    [Fact]
    public void Load_parses_top_level_faction_metadata()
    {
        var faction = FactionLoader.Load(MinimalFactionJson);

        Assert.Equal("blue", faction.Id);
        Assert.Equal("Blue", faction.DisplayName);
        Assert.Equal("#1e88e5", faction.Color);
    }


    [Fact]
    public void Load_indexes_weapon_systems_by_id()
    {
        var faction = FactionLoader.Load(MinimalFactionJson);

        var weapon = faction.WeaponSystems["us-rifle-m4"];

        Assert.Equal("M4 Carbine", weapon.DisplayName);
        Assert.Equal(1, weapon.MinRange);
        Assert.Equal(1, weapon.MaxRange);
        Assert.Equal(70, weapon.Accuracy);
        Assert.Equal(30, weapon.AmmoCapacity);
    }


    [Fact]
    public void Load_parses_damage_by_armor_with_enum_keys()
    {
        var faction = FactionLoader.Load(MinimalFactionJson);

        var damage = faction.WeaponSystems["us-rifle-m4"].DamageByArmor;

        Assert.Equal(8, damage[ArmorClass.Infantry]);
        Assert.Equal(2, damage[ArmorClass.LightArmor]);
        Assert.False(damage.ContainsKey(ArmorClass.HeavyArmor));
    }


    [Fact]
    public void Load_indexes_unit_types_by_id_and_parses_archetype()
    {
        var faction = FactionLoader.Load(MinimalFactionJson);

        var unit = faction.UnitTypes["us-infantry"];

        Assert.Equal("Infantry Squad", unit.DisplayName);
        Assert.Equal(UnitArchetype.Infantry, unit.Archetype);
        Assert.True(unit.CanCapture);
        Assert.False(unit.CanFly);
    }


    [Fact]
    public void Load_parses_terrain_movement_costs_with_enum_keys()
    {
        var faction = FactionLoader.Load(MinimalFactionJson);

        var costs = faction.UnitTypes["us-infantry"].TerrainMovementCosts;

        Assert.Equal(1, costs[Terrain.Plain]);
        Assert.Equal(2, costs[Terrain.Forest]);
        Assert.Equal(2, costs[Terrain.Hills]);
        Assert.Equal(1, costs[Terrain.Urban]);
        Assert.False(costs.ContainsKey(Terrain.Water)); // omitted = impassable
    }


    [Fact]
    public void Load_resolves_weapon_system_ids_referenced_by_units()
    {
        var faction = FactionLoader.Load(MinimalFactionJson);

        var unit = faction.UnitTypes["us-infantry"];

        Assert.Single(unit.WeaponSystemIds);
        Assert.Equal("us-rifle-m4", unit.WeaponSystemIds[0]);
        Assert.True(faction.WeaponSystems.ContainsKey(unit.WeaponSystemIds[0]));
    }


    [Fact]
    public void UnitTypeIds_enumerates_all_unit_types_in_catalog()
    {
        var faction = FactionLoader.Load(MinimalFactionJson);

        Assert.Equal(["us-infantry"], faction.UnitTypeIds);
    }


    [Fact]
    public void Load_throws_when_json_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => FactionLoader.Load(null!));
    }


    [Fact]
    public void Load_throws_when_json_is_malformed()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => FactionLoader.Load("{ not json"));

        Assert.Contains("could not be parsed", ex.Message, StringComparison.Ordinal);
    }


    [Fact]
    public void Load_throws_when_id_is_missing()
    {
        const string json = """
            {
              "displayName": "Blue",
              "color": "#1e88e5",
              "weapons": [],
              "units": []
            }
            """;

        var ex = Assert.Throws<InvalidOperationException>(() => FactionLoader.Load(json));

        Assert.Contains("Id", ex.Message, StringComparison.Ordinal);
    }


    [Fact]
    public void Load_throws_when_unit_references_unknown_weapon()
    {
        const string json = """
            {
              "id": "blue",
              "displayName": "Blue",
              "color": "#1e88e5",
              "weapons": [],
              "units": [
                {
                  "id": "us-infantry",
                  "displayName": "Infantry",
                  "archetype": "infantry",
                  "maxHp": 10, "fuelCapacity": 0, "sightRange": 2, "movementPoints": 3,
                  "armorClass": "infantry", "armor": 1, "dexterity": 3, "luck": 1,
                  "canCapture": true, "canFly": false,
                  "weaponSystemIds": ["us-imaginary-weapon"],
                  "terrainMovementCosts": { "plain": 1 }
                }
              ]
            }
            """;

        var ex = Assert.Throws<InvalidOperationException>(() => FactionLoader.Load(json));

        Assert.Contains("us-imaginary-weapon", ex.Message, StringComparison.Ordinal);
    }


    [Fact]
    public void Load_throws_when_two_weapons_share_an_id()
    {
        const string json = """
            {
              "id": "blue",
              "displayName": "Blue",
              "color": "#1e88e5",
              "weapons": [
                { "id": "dup", "displayName": "A", "minRange": 1, "maxRange": 1, "accuracy": 50, "damageByArmor": {}, "ammoCapacity": 1 },
                { "id": "dup", "displayName": "B", "minRange": 1, "maxRange": 1, "accuracy": 50, "damageByArmor": {}, "ammoCapacity": 1 }
              ],
              "units": []
            }
            """;

        var ex = Assert.Throws<InvalidOperationException>(() => FactionLoader.Load(json));

        Assert.Contains("Duplicate", ex.Message, StringComparison.Ordinal);
        Assert.Contains("dup", ex.Message, StringComparison.Ordinal);
    }


    [Fact]
    public void Save_then_Load_round_trips_to_an_equivalent_faction()
    {
        var original = FactionLoader.Load(MinimalFactionJson);

        var json = FactionLoader.Save(original);
        var roundTripped = FactionLoader.Load(json);

        Assert.Equal(original.Id, roundTripped.Id);
        Assert.Equal(original.DisplayName, roundTripped.DisplayName);
        Assert.Equal(original.Color, roundTripped.Color);
        Assert.Equal(original.WeaponSystems.Count, roundTripped.WeaponSystems.Count);
        Assert.Equal(original.UnitTypes.Count, roundTripped.UnitTypes.Count);

        var originalWeapon = original.WeaponSystems["us-rifle-m4"];
        var roundTrippedWeapon = roundTripped.WeaponSystems["us-rifle-m4"];
        Assert.Equal(originalWeapon.DisplayName, roundTrippedWeapon.DisplayName);
        Assert.Equal(originalWeapon.MaxRange, roundTrippedWeapon.MaxRange);
        Assert.Equal(originalWeapon.Accuracy, roundTrippedWeapon.Accuracy);
        Assert.Equal(originalWeapon.AmmoCapacity, roundTrippedWeapon.AmmoCapacity);
        Assert.Equal
        (
            originalWeapon.DamageByArmor.OrderBy(kv => kv.Key),
            roundTrippedWeapon.DamageByArmor.OrderBy(kv => kv.Key)
        );

        var originalUnit = original.UnitTypes["us-infantry"];
        var roundTrippedUnit = roundTripped.UnitTypes["us-infantry"];
        Assert.Equal(originalUnit.Archetype, roundTrippedUnit.Archetype);
        Assert.Equal(originalUnit.MaxHp, roundTrippedUnit.MaxHp);
        Assert.Equal(originalUnit.WeaponSystemIds, roundTrippedUnit.WeaponSystemIds);
        Assert.Equal
        (
            originalUnit.TerrainMovementCosts.OrderBy(kv => kv.Key),
            roundTrippedUnit.TerrainMovementCosts.OrderBy(kv => kv.Key)
        );
    }


    [Fact]
    public void Save_throws_when_faction_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => FactionLoader.Save(null!));
    }
}
