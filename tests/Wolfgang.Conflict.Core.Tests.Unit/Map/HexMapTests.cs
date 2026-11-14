using Wolfgang.Conflict.Core.Catalog;
using Wolfgang.Conflict.Core.Hex;
using Wolfgang.Conflict.Core.Map;

namespace Wolfgang.Conflict.Core.Tests.Unit.Map;

public class HexMapTests
{
    [Fact]
    public void OfRectangle_produces_width_times_height_tiles()
    {
        var map = HexMap.OfRectangle(width: 8, height: 5, terrain: Terrain.Plain);

        Assert.Equal(40, map.Count);
    }


    [Fact]
    public void OfRectangle_fills_every_tile_with_the_specified_terrain()
    {
        var map = HexMap.OfRectangle(3, 3, Terrain.Forest);

        foreach (var coord in map.Coords)
        {
            Assert.Equal(Terrain.Forest, map[coord].Terrain);
        }
    }


    [Fact]
    public void OfRectangle_includes_the_origin_in_the_first_row()
    {
        var map = HexMap.OfRectangle(4, 4, Terrain.Plain);

        Assert.True(map.Contains(HexCoord.Origin));
    }


    [Theory]
    [InlineData(0, 4)]
    [InlineData(-1, 4)]
    [InlineData(4, 0)]
    [InlineData(4, -2)]
    public void OfRectangle_throws_for_non_positive_dimensions(int width, int height)
    {
        Assert.Throws<ArgumentOutOfRangeException>
        (
            () => HexMap.OfRectangle(width, height, Terrain.Plain)
        );
    }


    [Fact]
    public void TryGetTile_returns_false_for_an_off_map_coordinate()
    {
        var map = HexMap.OfRectangle(2, 2, Terrain.Plain);

        var found = map.TryGetTile(new HexCoord(100, 100), out _);

        Assert.False(found);
    }


    [Fact]
    public void Indexer_throws_for_an_off_map_coordinate()
    {
        var map = HexMap.OfRectangle(2, 2, Terrain.Plain);

        Assert.Throws<KeyNotFoundException>(() => map[new HexCoord(100, 100)]);
    }


    [Fact]
    public void Constructor_throws_when_tiles_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => new HexMap(null!));
    }


    [Fact]
    public void Constructor_takes_a_snapshot_of_the_input_dictionary()
    {
        var source = new Dictionary<HexCoord, Tile>
        {
            [HexCoord.Origin] = new(Terrain.Plain)
        };
        var map = new HexMap(source);

        source[new HexCoord(1, 0)] = new Tile(Terrain.Forest);

        Assert.False(map.Contains(new HexCoord(1, 0)));
    }
}
