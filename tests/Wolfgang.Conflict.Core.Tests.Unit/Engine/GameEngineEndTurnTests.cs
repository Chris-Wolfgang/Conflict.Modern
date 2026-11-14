using Wolfgang.Conflict.Core.Commands;
using Wolfgang.Conflict.Core.Events;
using Wolfgang.Conflict.Core.Hex;

namespace Wolfgang.Conflict.Core.Tests.Unit.Engine;

public class GameEngineEndTurnTests
{
    [Fact]
    public void EndTurn_advances_current_side_to_the_next_in_the_order()
    {
        var state = GameEngineTestFixture.CreateState();
        var engine = GameEngineTestFixture.CreateEngine();

        var result = engine.Apply(state, new EndTurnCommand());

        Assert.Equal(GameEngineTestFixture.RedFactionId, result.State.CurrentSideFactionId);
    }


    [Fact]
    public void EndTurn_does_not_increment_turn_number_until_the_round_completes()
    {
        var state = GameEngineTestFixture.CreateState();
        var engine = GameEngineTestFixture.CreateEngine();

        var afterBlue = engine.Apply(state, new EndTurnCommand());

        Assert.Equal(1, afterBlue.State.TurnNumber);
    }


    [Fact]
    public void EndTurn_increments_turn_number_when_the_round_completes()
    {
        var state = GameEngineTestFixture.CreateState();
        var engine = GameEngineTestFixture.CreateEngine();

        var afterBlue = engine.Apply(state, new EndTurnCommand());
        var afterRed = engine.Apply(afterBlue.State, new EndTurnCommand());

        Assert.Equal(2, afterRed.State.TurnNumber);
        Assert.Equal(GameEngineTestFixture.BlueFactionId, afterRed.State.CurrentSideFactionId);
    }


    [Fact]
    public void EndTurn_resets_HasMoved_and_HasAttacked_on_all_units()
    {
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, HexCoord.Origin)
            with { HasMoved = true, HasAttacked = true };
        var state = GameEngineTestFixture.CreateState(blue);
        var engine = GameEngineTestFixture.CreateEngine();

        var result = engine.Apply(state, new EndTurnCommand());

        var resetUnit = result.State.Units[blue.Id];
        Assert.False(resetUnit.HasMoved);
        Assert.False(resetUnit.HasAttacked);
    }


    [Fact]
    public void EndTurn_emits_a_TurnEnded_event_with_ended_and_next_side()
    {
        var state = GameEngineTestFixture.CreateState();
        var engine = GameEngineTestFixture.CreateEngine();

        var result = engine.Apply(state, new EndTurnCommand());

        var ended = Assert.Single(result.Events.OfType<TurnEnded>());
        Assert.Equal(GameEngineTestFixture.BlueFactionId, ended.EndedSideFactionId);
        Assert.Equal(GameEngineTestFixture.RedFactionId, ended.NextSideFactionId);
    }


    [Fact]
    public void GetLegalCommands_includes_an_EndTurnCommand_for_the_current_side()
    {
        var state = GameEngineTestFixture.CreateState();
        var engine = GameEngineTestFixture.CreateEngine();

        var commands = engine.GetLegalCommands(state, GameEngineTestFixture.BlueFactionId).ToList();

        Assert.Contains(commands, c => c is EndTurnCommand);
    }


    [Fact]
    public void GetLegalCommands_returns_nothing_for_the_non_active_side()
    {
        var state = GameEngineTestFixture.CreateState();
        var engine = GameEngineTestFixture.CreateEngine();

        var commands = engine.GetLegalCommands(state, GameEngineTestFixture.RedFactionId).ToList();

        Assert.Empty(commands);
    }


    [Fact]
    public void GetLegalCommands_includes_moves_for_each_eligible_unit_of_the_current_side()
    {
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, HexCoord.Origin);
        var state = GameEngineTestFixture.CreateState(blue);
        var engine = GameEngineTestFixture.CreateEngine();

        var commands = engine.GetLegalCommands(state, GameEngineTestFixture.BlueFactionId).ToList();

        Assert.Contains(commands, c => c is MoveCommand m && m.UnitId == blue.Id);
    }
}
