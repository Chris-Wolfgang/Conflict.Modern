using Wolfgang.Conflict.Core.Hex;

namespace Wolfgang.Conflict.Core.Tests.Unit.Hex;

public class HexPathfinderReachableTests
{
    private static IEnumerable<(HexCoord Neighbor, int Cost)> UniformNeighbors(HexCoord hex) =>
        hex.Neighbors().Select(n => (n, 1));


    [Fact]
    public void FindReachable_with_zero_budget_returns_only_the_origin()
    {
        var reachable = HexPathfinder.FindReachable(HexCoord.Origin, UniformNeighbors, maxCost: 0);

        Assert.Single(reachable);
        Assert.True(reachable.ContainsKey(HexCoord.Origin));
        Assert.Equal(0, reachable[HexCoord.Origin].TotalCost);
    }


    [Fact]
    public void FindReachable_uniform_cost_one_returns_seven_hexes_within_one_step()
    {
        var reachable = HexPathfinder.FindReachable(HexCoord.Origin, UniformNeighbors, maxCost: 1);

        // origin + 6 neighbors
        Assert.Equal(7, reachable.Count);
    }


    [Fact]
    public void FindReachable_uniform_cost_one_returns_nineteen_hexes_within_two_steps()
    {
        var reachable = HexPathfinder.FindReachable(HexCoord.Origin, UniformNeighbors, maxCost: 2);

        // 1 (self) + 6 (ring 1) + 12 (ring 2) = 19
        Assert.Equal(19, reachable.Count);
    }


    [Fact]
    public void FindReachable_assigns_cheapest_path_when_multiple_routes_exist()
    {
        // Make hex (1,0) cost 100 to enter; the cheapest path to (2,0) should detour.
        var blocked = new HexCoord(1, 0);

        IEnumerable<(HexCoord, int)> CostedNeighbors(HexCoord hex) =>
            hex.Neighbors().Select(n => (n, n == blocked ? 100 : 1));

        var reachable = HexPathfinder.FindReachable(HexCoord.Origin, CostedNeighbors, maxCost: 5);

        var pathToTwoZero = reachable[new HexCoord(2, 0)];
        Assert.DoesNotContain(blocked, pathToTwoZero.Hexes);
        Assert.True(pathToTwoZero.TotalCost < 100);
    }


    [Fact]
    public void FindReachable_throws_when_maxCost_is_negative()
    {
        Assert.Throws<ArgumentOutOfRangeException>
        (
            () => HexPathfinder.FindReachable(HexCoord.Origin, UniformNeighbors, maxCost: -1)
        );
    }
}
