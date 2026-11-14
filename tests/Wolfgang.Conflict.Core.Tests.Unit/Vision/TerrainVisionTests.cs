using Wolfgang.Conflict.Core.Catalog;
using Wolfgang.Conflict.Core.Vision;

namespace Wolfgang.Conflict.Core.Tests.Unit.Vision;

public class TerrainVisionTests
{
    [Theory]
    [InlineData(Terrain.Plain,    false)]
    [InlineData(Terrain.Hills,    false)]
    [InlineData(Terrain.Water,    false)]
    [InlineData(Terrain.Forest,   true)]
    [InlineData(Terrain.Urban,    true)]
    [InlineData(Terrain.Mountain, true)]
    public void BlocksLineOfSight_returns_expected_value(Terrain terrain, bool blocks)
    {
        Assert.Equal(blocks, TerrainVision.BlocksLineOfSight(terrain));
    }


    [Theory]
    [InlineData(Terrain.Plain,    0)]
    [InlineData(Terrain.Forest,   0)]
    [InlineData(Terrain.Urban,    0)]
    [InlineData(Terrain.Water,    0)]
    [InlineData(Terrain.Hills,    1)]
    [InlineData(Terrain.Mountain, 2)]
    public void ElevationSightBonus_returns_expected_value(Terrain terrain, int bonus)
    {
        Assert.Equal(bonus, TerrainVision.ElevationSightBonus(terrain));
    }
}
