using Wolfgang.Conflict.Core.Catalog;
using Wolfgang.Conflict.Core.Hex;
using Wolfgang.Conflict.Core.Map;
using Wolfgang.Conflict.Core.State;
using Wolfgang.Conflict.Core.Tests.Unit.Engine;
using Wolfgang.Conflict.Core.Units;
using Wolfgang.Conflict.Core.Vision;
using GameUnit = Wolfgang.Conflict.Core.Units.Unit;

namespace Wolfgang.Conflict.Core.Tests.Unit.Vision;

public class VisionSystemTests
{
    private static GameState StateWithMap(HexMap map, params GameUnit[] units) => new
    (
        Map: map,
        Units: units.ToDictionary(u => u.Id),
        SideOrder: [GameEngineTestFixture.BlueFactionId, GameEngineTestFixture.RedFactionId],
        CurrentSideIndex: 0,
        TurnNumber: 1,
        Rules: RulesConfig.Default,
        RngSeed: 1,
        RngStep: 0
    );


    private static HexMap MapWithTerrainAt(int width, int height, params (HexCoord coord, Terrain terrain)[] overrides)
    {
        var tiles = new Dictionary<HexCoord, Tile>();
        var baseMap = HexMap.OfRectangle(width, height, Terrain.Plain);
        foreach (var coord in baseMap.Coords)
        {
            tiles[coord] = new Tile(Terrain.Plain);
        }
        foreach (var (coord, terrain) in overrides)
        {
            tiles[coord] = new Tile(terrain);
        }
        return new HexMap(tiles);
    }


    private static VisionSystem CreateSystem() =>
        new
        (
            new Dictionary<string, FactionDefinition>
            {
                [GameEngineTestFixture.BlueFactionId] = GameEngineTestFixture.Faction(GameEngineTestFixture.BlueFactionId),
                [GameEngineTestFixture.RedFactionId]  = GameEngineTestFixture.Faction(GameEngineTestFixture.RedFactionId)
            }
        );


    [Fact]
    public void HasLineOfSight_is_true_when_endpoints_are_equal()
    {
        var state = StateWithMap(HexMap.OfRectangle(5, 5, Terrain.Plain));

        Assert.True(VisionSystem.HasLineOfSight(state, new HexCoord(2, 2), new HexCoord(2, 2), observerIsAirborne: false));
    }


    [Fact]
    public void HasLineOfSight_is_true_across_clear_plain()
    {
        var state = StateWithMap(HexMap.OfRectangle(5, 5, Terrain.Plain));

        Assert.True(VisionSystem.HasLineOfSight(state, HexCoord.Origin, new HexCoord(3, 0), observerIsAirborne: false));
    }


    [Fact]
    public void HasLineOfSight_is_blocked_by_a_forest_intermediate_hex()
    {
        // Line from (0,0) to (3,0) passes through (1,0) and (2,0). Make (2,0) forest.
        var map = MapWithTerrainAt(5, 5, (new HexCoord(2, 0), Terrain.Forest));
        var state = StateWithMap(map);

        Assert.False(VisionSystem.HasLineOfSight(state, HexCoord.Origin, new HexCoord(3, 0), observerIsAirborne: false));
    }


    [Fact]
    public void HasLineOfSight_is_blocked_by_a_mountain_intermediate_hex()
    {
        var map = MapWithTerrainAt(5, 5, (new HexCoord(2, 0), Terrain.Mountain));
        var state = StateWithMap(map);

        Assert.False(VisionSystem.HasLineOfSight(state, HexCoord.Origin, new HexCoord(3, 0), observerIsAirborne: false));
    }


    [Fact]
    public void HasLineOfSight_is_not_blocked_by_a_hill_intermediate_hex()
    {
        var map = MapWithTerrainAt(5, 5, (new HexCoord(2, 0), Terrain.Hills));
        var state = StateWithMap(map);

        Assert.True(VisionSystem.HasLineOfSight(state, HexCoord.Origin, new HexCoord(3, 0), observerIsAirborne: false));
    }


    [Fact]
    public void HasLineOfSight_is_not_blocked_when_blocker_is_the_destination_itself()
    {
        // Forest at the target hex shouldn't prevent seeing INTO it from outside.
        var map = MapWithTerrainAt(5, 5, (new HexCoord(3, 0), Terrain.Forest));
        var state = StateWithMap(map);

        Assert.True(VisionSystem.HasLineOfSight(state, HexCoord.Origin, new HexCoord(3, 0), observerIsAirborne: false));
    }


    [Fact]
    public void HasLineOfSight_is_not_blocked_when_observer_is_airborne()
    {
        var map = MapWithTerrainAt(5, 5,
            (new HexCoord(1, 0), Terrain.Forest),
            (new HexCoord(2, 0), Terrain.Mountain));
        var state = StateWithMap(map);

        Assert.True(VisionSystem.HasLineOfSight(state, HexCoord.Origin, new HexCoord(3, 0), observerIsAirborne: true));
    }


    [Fact]
    public void HasLineOfSight_throws_when_state_is_null()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => VisionSystem.HasLineOfSight(null!, HexCoord.Origin, new HexCoord(1, 0), observerIsAirborne: false)
        );
    }


    [Fact]
    public void ComputeVisibility_returns_empty_when_faction_has_no_units()
    {
        var state = StateWithMap(HexMap.OfRectangle(5, 5, Terrain.Plain));
        var system = CreateSystem();

        var visible = system.ComputeVisibility(state, GameEngineTestFixture.BlueFactionId);

        Assert.Empty(visible);
    }


    [Fact]
    public void ComputeVisibility_returns_hexes_within_a_units_sight_range()
    {
        // Infantry has SightRange=2. Plain everywhere → 19 hexes within 2.
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, new HexCoord(3, 3));
        var state = StateWithMap(HexMap.OfRectangle(9, 9, Terrain.Plain), blue);
        var system = CreateSystem();

        var visible = system.ComputeVisibility(state, GameEngineTestFixture.BlueFactionId);

        Assert.Contains(new HexCoord(3, 3), visible);
        Assert.Contains(new HexCoord(5, 3), visible); // 2 east
        Assert.DoesNotContain(new HexCoord(6, 3), visible); // 3 east — out of range
    }


    [Fact]
    public void ComputeVisibility_extends_sight_when_observer_is_on_a_hill()
    {
        // Hill at (3,3) gives +1 sight → infantry SR=2 becomes 3.
        var map = MapWithTerrainAt(9, 9, (new HexCoord(3, 3), Terrain.Hills));
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, new HexCoord(3, 3));
        var state = StateWithMap(map, blue);
        var system = CreateSystem();

        var visible = system.ComputeVisibility(state, GameEngineTestFixture.BlueFactionId);

        Assert.Contains(new HexCoord(6, 3), visible);
    }


    [Fact]
    public void ComputeVisibility_blocks_hexes_behind_terrain_blockers()
    {
        // Forest blocks LOS along the east axis.
        var map = MapWithTerrainAt(9, 9, (new HexCoord(2, 0), Terrain.Forest));
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, HexCoord.Origin);
        // Bump sight range to 3 by putting unit on a hill at origin.
        map = MapWithTerrainAt(9, 9,
            (HexCoord.Origin, Terrain.Hills),
            (new HexCoord(2, 0), Terrain.Forest));
        var state = StateWithMap(map, blue);
        var system = CreateSystem();

        var visible = system.ComputeVisibility(state, GameEngineTestFixture.BlueFactionId);

        // (1,0) is visible (in front of the forest), (3,0) is not (behind the forest).
        Assert.Contains(new HexCoord(1, 0), visible);
        Assert.Contains(new HexCoord(2, 0), visible); // forest itself is visible
        Assert.DoesNotContain(new HexCoord(3, 0), visible);
    }


    [Fact]
    public void ComputeVisibility_is_the_union_of_every_friendly_units_sight()
    {
        var blue1 = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, new HexCoord(1, 1));
        var blue2 = GameEngineTestFixture.Infantry(2, GameEngineTestFixture.BlueFactionId, new HexCoord(4, 4));
        var state = StateWithMap(HexMap.OfRectangle(9, 9, Terrain.Plain), blue1, blue2);
        var system = CreateSystem();

        var visible = system.ComputeVisibility(state, GameEngineTestFixture.BlueFactionId);

        Assert.Contains(new HexCoord(1, 1), visible);
        Assert.Contains(new HexCoord(4, 4), visible);
        // Far enough apart that neither sees the other's hex
        Assert.True(blue1.Position.DistanceTo(blue2.Position) > 4);
    }


    [Fact]
    public void ComputeVisibility_ignores_units_of_other_factions()
    {
        var red = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.RedFactionId, new HexCoord(3, 3));
        var state = StateWithMap(HexMap.OfRectangle(9, 9, Terrain.Plain), red);
        var system = CreateSystem();

        var blueVisible = system.ComputeVisibility(state, GameEngineTestFixture.BlueFactionId);

        Assert.Empty(blueVisible);
    }


    [Fact]
    public void Constructor_throws_when_factions_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => new VisionSystem(null!));
    }


    [Fact]
    public void ComputeVisibility_throws_when_state_is_null()
    {
        var system = CreateSystem();

        Assert.Throws<ArgumentNullException>
        (
            () => system.ComputeVisibility(null!, GameEngineTestFixture.BlueFactionId)
        );
    }
}
