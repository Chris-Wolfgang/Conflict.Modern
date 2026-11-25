using Wolfgang.Conflict.Core.Hex;
using Wolfgang.Conflict.Core.Tests.Unit.Engine;

namespace Wolfgang.Conflict.Core.Tests.Unit.State;

public class GameStateTests
{
    [Fact]
    public void CurrentSideFactionId_returns_the_faction_at_the_current_index()
    {
        var state = GameEngineTestFixture.CreateState();

        Assert.Equal(GameEngineTestFixture.BlueFactionId, state.CurrentSideFactionId);
    }


    [Fact]
    public void UnitAt_returns_the_unit_when_a_hex_is_occupied()
    {
        var blueInfantry = GameEngineTestFixture.Infantry
        (
            id: 1,
            factionId: GameEngineTestFixture.BlueFactionId,
            position: new HexCoord(2, 1)
        );
        var state = GameEngineTestFixture.CreateState(blueInfantry);

        Assert.Equal(blueInfantry, state.UnitAt(new HexCoord(2, 1)));
    }


    [Fact]
    public void UnitAt_returns_null_when_a_hex_is_empty()
    {
        var state = GameEngineTestFixture.CreateState();

        Assert.Null(state.UnitAt(new HexCoord(2, 1)));
    }


    [Fact]
    public void IsHexOccupied_reflects_whether_a_unit_stands_on_the_hex()
    {
        var blueInfantry = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, new HexCoord(2, 1));
        var state = GameEngineTestFixture.CreateState(blueInfantry);

        Assert.True(state.IsHexOccupied(new HexCoord(2, 1)));
        Assert.False(state.IsHexOccupied(new HexCoord(0, 0)));
    }


    [Fact]
    public void UnitsOfFaction_returns_only_units_owned_by_that_faction()
    {
        var blue1 = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, new HexCoord(0, 0));
        var blue2 = GameEngineTestFixture.Infantry(2, GameEngineTestFixture.BlueFactionId, new HexCoord(1, 0));
        var red1 = GameEngineTestFixture.Infantry(3, GameEngineTestFixture.RedFactionId, new HexCoord(2, 0));

        var state = GameEngineTestFixture.CreateState(blue1, blue2, red1);

        var blueUnits = state.UnitsOfFaction(GameEngineTestFixture.BlueFactionId).ToList();

        Assert.Equal(2, blueUnits.Count);
        Assert.Contains(blue1, blueUnits);
        Assert.Contains(blue2, blueUnits);
    }
}
