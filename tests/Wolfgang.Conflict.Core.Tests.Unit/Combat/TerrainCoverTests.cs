using Wolfgang.Conflict.Core.Catalog;
using Wolfgang.Conflict.Core.Combat;

namespace Wolfgang.Conflict.Core.Tests.Unit.Combat;

public class TerrainCoverTests
{
    [Theory]
    [InlineData(Terrain.Plain,    0)]
    [InlineData(Terrain.Hills,    5)]
    [InlineData(Terrain.Forest,  15)]
    [InlineData(Terrain.Urban,   20)]
    [InlineData(Terrain.Mountain, 25)]
    [InlineData(Terrain.Water,    0)]
    public void CoverFor_returns_expected_percentage(Terrain terrain, int expected)
    {
        Assert.Equal(expected, TerrainCover.CoverFor(terrain));
    }


    [Fact]
    public void Cover_ranks_progressively_higher_for_denser_terrain()
    {
        Assert.True(TerrainCover.CoverFor(Terrain.Plain) < TerrainCover.CoverFor(Terrain.Hills));
        Assert.True(TerrainCover.CoverFor(Terrain.Hills) < TerrainCover.CoverFor(Terrain.Forest));
        Assert.True(TerrainCover.CoverFor(Terrain.Forest) < TerrainCover.CoverFor(Terrain.Urban));
        Assert.True(TerrainCover.CoverFor(Terrain.Urban) < TerrainCover.CoverFor(Terrain.Mountain));
    }
}
