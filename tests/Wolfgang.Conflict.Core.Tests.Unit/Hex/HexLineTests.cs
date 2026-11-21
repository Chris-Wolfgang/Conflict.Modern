using Wolfgang.Conflict.Core.Hex;

namespace Wolfgang.Conflict.Core.Tests.Unit.Hex;

public class HexLineTests
{
    [Fact]
    public void Draw_returns_a_single_hex_when_endpoints_are_equal()
    {
        var line = HexLine.Draw(new HexCoord(2, 3), new HexCoord(2, 3));

        Assert.Single(line);
        Assert.Equal(new HexCoord(2, 3), line[0]);
    }


    [Fact]
    public void Draw_returns_distance_plus_one_hexes()
    {
        var a = HexCoord.Origin;
        var b = new HexCoord(3, 0);

        var line = HexLine.Draw(a, b);

        Assert.Equal(a.DistanceTo(b) + 1, line.Count);
    }


    [Fact]
    public void Draw_includes_both_endpoints()
    {
        var a = new HexCoord(-1, 2);
        var b = new HexCoord(3, -1);

        var line = HexLine.Draw(a, b);

        Assert.Equal(a, line[0]);
        Assert.Equal(b, line[^1]);
    }


    [Fact]
    public void Draw_each_consecutive_pair_is_adjacent()
    {
        var a = new HexCoord(-2, 1);
        var b = new HexCoord(3, 2);

        var line = HexLine.Draw(a, b);

        for (var i = 0; i < line.Count - 1; i++)
        {
            Assert.Equal(1, line[i].DistanceTo(line[i + 1]));
        }
    }


    [Fact]
    public void Draw_along_the_east_axis_walks_east_only()
    {
        var line = HexLine.Draw(HexCoord.Origin, new HexCoord(4, 0));

        Assert.Equal
        (
            new[] { new HexCoord(0, 0), new HexCoord(1, 0), new HexCoord(2, 0), new HexCoord(3, 0), new HexCoord(4, 0) },
            line
        );
    }
}
