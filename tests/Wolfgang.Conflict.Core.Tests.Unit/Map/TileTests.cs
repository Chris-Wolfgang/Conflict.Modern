using Wolfgang.Conflict.Core.Catalog;
using Wolfgang.Conflict.Core.Map;

namespace Wolfgang.Conflict.Core.Tests.Unit.Map;

public class TileTests
{
    [Fact]
    public void Constructor_defaults_to_no_structure_and_no_owner()
    {
        var tile = new Tile(Terrain.Plain);

        Assert.Equal(StructureType.None, tile.Structure);
        Assert.Null(tile.OwnerFactionId);
    }


    [Fact]
    public void Constructor_keeps_the_structure_and_its_owner()
    {
        var structure = Enum.GetValues<StructureType>().First(s => s != StructureType.None);

        var tile = new Tile(Terrain.Urban, structure, "blue");

        Assert.Equal(structure, tile.Structure);
        Assert.Equal("blue", tile.OwnerFactionId);
    }
}
