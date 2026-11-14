using Wolfgang.Conflict.Core.Hex;

namespace Wolfgang.Conflict.Core.Tests.Unit.Hex;

public class HexCoordTests
{
    [Fact]
    public void Origin_is_at_zero_zero()
    {
        var origin = HexCoord.Origin;

        Assert.Equal(0, origin.Q);
        Assert.Equal(0, origin.R);
    }


    [Fact]
    public void Equality_is_value_based()
    {
        var a = new HexCoord(3, -2);
        var b = new HexCoord(3, -2);

        Assert.Equal(a, b);
        Assert.True(a == b);
    }


    [Fact]
    public void Different_coordinates_are_not_equal()
    {
        var a = new HexCoord(1, 2);
        var b = new HexCoord(2, 1);

        Assert.NotEqual(a, b);
    }


    [Fact]
    public void S_is_negative_sum_of_Q_and_R()
    {
        var hex = new HexCoord(3, -1);

        Assert.Equal(-2, hex.S);
        Assert.Equal(0, hex.Q + hex.R + hex.S);
    }


    [Theory]
    [InlineData(HexDirection.East,      +1,  0)]
    [InlineData(HexDirection.SouthEast,  0, +1)]
    [InlineData(HexDirection.SouthWest, -1, +1)]
    [InlineData(HexDirection.West,      -1,  0)]
    [InlineData(HexDirection.NorthWest,  0, -1)]
    [InlineData(HexDirection.NorthEast, +1, -1)]
    public void Neighbor_returns_origin_offset_by_axial_direction(HexDirection direction, int expectedQ, int expectedR)
    {
        var neighbor = HexCoord.Origin.Neighbor(direction);

        Assert.Equal(new HexCoord(expectedQ, expectedR), neighbor);
    }


    [Fact]
    public void Neighbors_returns_six_unique_hexes()
    {
        var neighbors = HexCoord.Origin.Neighbors().ToList();

        Assert.Equal(6, neighbors.Count);
        Assert.Equal(6, neighbors.Distinct().Count());
    }


    [Fact]
    public void Neighbors_are_each_at_distance_one_from_origin()
    {
        foreach (var neighbor in HexCoord.Origin.Neighbors())
        {
            Assert.Equal(1, HexCoord.Origin.DistanceTo(neighbor));
        }
    }


    [Fact]
    public void DistanceTo_self_is_zero()
    {
        var hex = new HexCoord(4, -7);

        Assert.Equal(0, hex.DistanceTo(hex));
    }


    [Fact]
    public void DistanceTo_is_symmetric()
    {
        var a = new HexCoord(2, 3);
        var b = new HexCoord(-1, 5);

        Assert.Equal(a.DistanceTo(b), b.DistanceTo(a));
    }


    [Theory]
    [InlineData(0, 0,  3,  0, 3)]
    [InlineData(0, 0,  0,  3, 3)]
    [InlineData(0, 0,  3, -3, 3)]
    [InlineData(0, 0, -3,  0, 3)]
    [InlineData(0, 0,  2,  2, 4)]
    [InlineData(1, 2,  4,  6, 7)]
    public void DistanceTo_matches_expected_axial_distance(int q1, int r1, int q2, int r2, int expected)
    {
        var a = new HexCoord(q1, r1);
        var b = new HexCoord(q2, r2);

        Assert.Equal(expected, a.DistanceTo(b));
    }


    [Fact]
    public void Walking_six_steps_clockwise_returns_to_origin()
    {
        var hex = HexCoord.Origin;

        for (var i = 0; i < 6; i++)
        {
            hex = hex.Neighbor((HexDirection)i);
        }

        Assert.Equal(HexCoord.Origin, hex);
    }


    [Fact]
    public void ToString_emits_parenthesized_coordinates()
    {
        var hex = new HexCoord(2, -3);

        Assert.Equal("(2, -3)", hex.ToString());
    }
}
