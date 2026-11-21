using Wolfgang.Conflict.Core.Hex;

namespace Wolfgang.Conflict.Core.Tests.Unit.Hex;

public class HexPathfinderTests
{
    /// <summary>
    /// Uniform-cost neighbor function: every neighbor is reachable at cost 1.
    /// </summary>
    private static IEnumerable<(HexCoord Neighbor, int Cost)> UniformNeighbors(HexCoord hex) =>
        hex.Neighbors().Select(n => (n, 1));


    [Fact]
    public void FindPath_when_start_equals_goal_returns_single_hex_path_with_zero_cost()
    {
        var hex = new HexCoord(2, -1);

        var path = HexPathfinder.FindPath(hex, hex, UniformNeighbors);

        Assert.NotNull(path);
        Assert.Equal(0, path.TotalCost);
        Assert.Single(path.Hexes);
        Assert.Equal(hex, path.Hexes[0]);
    }


    [Fact]
    public void FindPath_with_uniform_costs_returns_path_of_length_equal_to_hex_distance()
    {
        var start = HexCoord.Origin;
        var goal = new HexCoord(3, 0);

        var path = HexPathfinder.FindPath(start, goal, UniformNeighbors);

        Assert.NotNull(path);
        Assert.Equal(3, path.TotalCost);
        Assert.Equal(4, path.Hexes.Count); // start + 3 steps
        Assert.Equal(start, path.Hexes[0]);
        Assert.Equal(goal, path.Hexes[^1]);
    }


    [Fact]
    public void FindPath_returns_path_whose_consecutive_hexes_are_neighbors()
    {
        var start = HexCoord.Origin;
        var goal = new HexCoord(2, 3);

        var path = HexPathfinder.FindPath(start, goal, UniformNeighbors);

        Assert.NotNull(path);

        for (var i = 0; i < path.Hexes.Count - 1; i++)
        {
            Assert.Equal(1, path.Hexes[i].DistanceTo(path.Hexes[i + 1]));
        }
    }


    [Fact]
    public void FindPath_returns_null_when_goal_is_unreachable()
    {
        var start = HexCoord.Origin;
        var goal = new HexCoord(5, 0);

        // No neighbors at all — graph is disconnected.
        IEnumerable<(HexCoord, int)> NoNeighbors(HexCoord _) => [];

        var path = HexPathfinder.FindPath(start, goal, NoNeighbors);

        Assert.Null(path);
    }


    [Fact]
    public void FindPath_respects_maxCost_returning_null_when_cheapest_path_exceeds_it()
    {
        var start = HexCoord.Origin;
        var goal = new HexCoord(5, 0); // distance 5

        var path = HexPathfinder.FindPath(start, goal, UniformNeighbors, maxCost: 4);

        Assert.Null(path);
    }


    [Fact]
    public void FindPath_returns_path_when_total_cost_equals_maxCost()
    {
        var start = HexCoord.Origin;
        var goal = new HexCoord(3, 0);

        var path = HexPathfinder.FindPath(start, goal, UniformNeighbors, maxCost: 3);

        Assert.NotNull(path);
        Assert.Equal(3, path.TotalCost);
    }


    [Fact]
    public void FindPath_prefers_cheaper_route_around_a_high_cost_barrier()
    {
        var start = HexCoord.Origin;
        var goal = new HexCoord(2, 0);
        var blocked = new HexCoord(1, 0); // direct route hex; expensive to enter

        IEnumerable<(HexCoord, int)> CostedNeighbors(HexCoord hex) =>
            hex.Neighbors().Select(n => (n, n == blocked ? 100 : 1));

        var path = HexPathfinder.FindPath(start, goal, CostedNeighbors);

        Assert.NotNull(path);
        Assert.DoesNotContain(blocked, path.Hexes);
        Assert.True(path.TotalCost < 100);
    }


    [Fact]
    public void FindPath_throws_when_neighbor_cost_is_negative()
    {
        var start = HexCoord.Origin;
        var goal = new HexCoord(1, 0);

        IEnumerable<(HexCoord, int)> BadNeighbors(HexCoord hex) =>
            hex.Neighbors().Select(n => (n, -1));

        Assert.Throws<InvalidOperationException>
        (
            () => HexPathfinder.FindPath(start, goal, BadNeighbors)
        );
    }


    [Fact]
    public void FindPath_throws_when_neighbor_cost_is_zero()
    {
        var start = HexCoord.Origin;
        var goal = new HexCoord(1, 0);

        IEnumerable<(HexCoord, int)> ZeroNeighbors(HexCoord hex) =>
            hex.Neighbors().Select(n => (n, 0));

        Assert.Throws<InvalidOperationException>
        (
            () => HexPathfinder.FindPath(start, goal, ZeroNeighbors)
        );
    }


    [Fact]
    public void FindPath_throws_when_maxCost_is_negative()
    {
        Assert.Throws<ArgumentOutOfRangeException>
        (
            () => HexPathfinder.FindPath(HexCoord.Origin, new HexCoord(1, 0), UniformNeighbors, maxCost: -1)
        );
    }


    [Fact]
    public void FindPath_throws_when_maxCost_is_negative_even_when_start_equals_goal()
    {
        Assert.Throws<ArgumentOutOfRangeException>
        (
            () => HexPathfinder.FindPath(HexCoord.Origin, HexCoord.Origin, UniformNeighbors, maxCost: -1)
        );
    }


    [Fact]
    public void FindPath_throws_when_getNeighbors_is_null()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => HexPathfinder.FindPath(HexCoord.Origin, new HexCoord(1, 0), null!)
        );
    }
}
