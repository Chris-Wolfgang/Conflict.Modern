using Wolfgang.Conflict.Core.Catalog;

namespace Wolfgang.Conflict.Core.Tests.Unit.Catalog;

/// <summary>
/// Integration tests over the bundled <c>data/factions/*.json</c> files.
/// These catch JSON syntax errors, schema drift, and dangling weapon
/// references in the actual content packs we ship with v1.
/// </summary>
public class BundledFactionFilesTests
{
    [Theory]
    [InlineData("blue.json",   "blue",   "Blue",  6, 9)]
    [InlineData("red.json", "red",    "Red",         6, 9)]
    public async Task Bundled_faction_file_loads_with_expected_metadata_and_catalog_size
    (
        string fileName,
        string expectedId,
        string expectedDisplayName,
        int expectedUnitCount,
        int expectedWeaponCount
    )
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "factions", fileName);
        var json = await File.ReadAllTextAsync(path);

        var faction = FactionLoader.Load(json);

        Assert.Equal(expectedId, faction.Id);
        Assert.Equal(expectedDisplayName, faction.DisplayName);
        Assert.Equal(expectedUnitCount, faction.UnitTypes.Count);
        Assert.Equal(expectedWeaponCount, faction.WeaponSystems.Count);
    }


    [Theory]
    [InlineData("blue.json")]
    [InlineData("red.json")]
    public async Task Bundled_faction_file_includes_all_six_v1_archetypes(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "factions", fileName);
        var json = await File.ReadAllTextAsync(path);

        var faction = FactionLoader.Load(json);
        var archetypes = faction.UnitTypes.Values.Select(u => u.Archetype).ToHashSet();

        Assert.Contains(UnitArchetype.Infantry, archetypes);
        Assert.Contains(UnitArchetype.Tank, archetypes);
        Assert.Contains(UnitArchetype.Artillery, archetypes);
        Assert.Contains(UnitArchetype.Recon, archetypes);
        Assert.Contains(UnitArchetype.Helicopter, archetypes);
        Assert.Contains(UnitArchetype.Fighter, archetypes);
    }


    [Theory]
    [InlineData("blue.json")]
    [InlineData("red.json")]
    public async Task Bundled_faction_infantry_can_capture_and_other_archetypes_cannot(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "factions", fileName);
        var json = await File.ReadAllTextAsync(path);

        var faction = FactionLoader.Load(json);

        foreach (var unit in faction.UnitTypes.Values)
        {
            if (unit.Archetype == UnitArchetype.Infantry)
            {
                Assert.True(unit.CanCapture, $"{unit.Id} (infantry) should be able to capture.");
            }
            else
            {
                Assert.False(unit.CanCapture, $"{unit.Id} ({unit.Archetype}) should not capture.");
            }
        }
    }


    [Theory]
    [InlineData("blue.json")]
    [InlineData("red.json")]
    public async Task Bundled_faction_aircraft_can_fly_and_ground_units_cannot(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "factions", fileName);
        var json = await File.ReadAllTextAsync(path);

        var faction = FactionLoader.Load(json);

        foreach (var unit in faction.UnitTypes.Values)
        {
            var isAir = unit.Archetype is UnitArchetype.Helicopter or UnitArchetype.Fighter;
            Assert.Equal(isAir, unit.CanFly);
        }
    }
}
