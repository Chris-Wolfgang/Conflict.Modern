using Wolfgang.Conflict.Core.Commands;
using Wolfgang.Conflict.Core.Events;
using Wolfgang.Conflict.Core.Hex;
using Wolfgang.Conflict.Core.Units;

namespace Wolfgang.Conflict.Core.Tests.Unit.Engine;

public class GameEngineMoveTests
{
    [Fact]
    public void GetLegalMoves_returns_a_command_for_each_reachable_hex_within_movement_points()
    {
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, HexCoord.Origin);
        var state = GameEngineTestFixture.CreateState(blue);
        var engine = GameEngineTestFixture.CreateEngine();

        var moves = engine.GetLegalMoves(state, blue.Id).ToList();

        Assert.NotEmpty(moves);
        Assert.All(moves, m => Assert.Equal(blue.Id, m.UnitId));
        Assert.All(moves, m => Assert.Equal(HexCoord.Origin, m.Path[0]));
        Assert.All(moves, m => Assert.NotEqual(HexCoord.Origin, m.Path[^1]));
    }


    [Fact]
    public void GetLegalMoves_returns_nothing_when_unit_belongs_to_other_side()
    {
        var red = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.RedFactionId, HexCoord.Origin);
        var state = GameEngineTestFixture.CreateState(red);
        var engine = GameEngineTestFixture.CreateEngine();

        var moves = engine.GetLegalMoves(state, red.Id).ToList();

        Assert.Empty(moves);
    }


    [Fact]
    public void GetLegalMoves_returns_nothing_when_unit_has_already_moved()
    {
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, HexCoord.Origin)
            with { HasMoved = true };
        var state = GameEngineTestFixture.CreateState(blue);
        var engine = GameEngineTestFixture.CreateEngine();

        Assert.Empty(engine.GetLegalMoves(state, blue.Id));
    }


    [Fact]
    public void GetLegalMoves_returns_nothing_when_unit_is_out_of_fuel()
    {
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, HexCoord.Origin)
            with { Status = UnitStatus.OutOfFuel };
        var state = GameEngineTestFixture.CreateState(blue);
        var engine = GameEngineTestFixture.CreateEngine();

        Assert.Empty(engine.GetLegalMoves(state, blue.Id));
    }


    [Fact]
    public void GetLegalMoves_does_not_include_hexes_occupied_by_other_units()
    {
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, HexCoord.Origin);
        var blocker = GameEngineTestFixture.Infantry(2, GameEngineTestFixture.RedFactionId, new HexCoord(1, 0));
        var state = GameEngineTestFixture.CreateState(blue, blocker);
        var engine = GameEngineTestFixture.CreateEngine();

        var destinations = engine.GetLegalMoves(state, blue.Id).Select(m => m.Path[^1]).ToHashSet();

        Assert.DoesNotContain(new HexCoord(1, 0), destinations);
    }


    [Fact]
    public void Apply_moves_unit_to_destination_and_marks_HasMoved()
    {
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, HexCoord.Origin);
        var state = GameEngineTestFixture.CreateState(blue);
        var engine = GameEngineTestFixture.CreateEngine();

        var move = new MoveCommand(blue.Id, [HexCoord.Origin, new HexCoord(1, 0)]);
        var result = engine.Apply(state, move);

        var moved = result.State.Units[blue.Id];
        Assert.Equal(new HexCoord(1, 0), moved.Position);
        Assert.True(moved.HasMoved);
    }


    [Fact]
    public void Apply_emits_a_UnitMoved_event_with_path_and_total_cost()
    {
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, HexCoord.Origin);
        var state = GameEngineTestFixture.CreateState(blue);
        var engine = GameEngineTestFixture.CreateEngine();

        var path = new List<HexCoord> { HexCoord.Origin, new(1, 0), new(2, 0) };
        var result = engine.Apply(state, new MoveCommand(blue.Id, path));

        var moved = Assert.Single(result.Events.OfType<UnitMoved>());
        Assert.Equal(blue.Id, moved.UnitId);
        Assert.Equal(path, moved.Path);
        Assert.Equal(2, moved.TotalCost); // 2 plain hexes at cost 1 each
    }


    [Fact]
    public void Apply_throws_when_path_starts_at_wrong_hex()
    {
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, HexCoord.Origin);
        var state = GameEngineTestFixture.CreateState(blue);
        var engine = GameEngineTestFixture.CreateEngine();

        var move = new MoveCommand(blue.Id, [new HexCoord(5, 5), new HexCoord(5, 4)]);

        Assert.Throws<InvalidOperationException>(() => engine.Apply(state, move));
    }


    [Fact]
    public void Apply_throws_when_path_step_is_not_adjacent()
    {
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, HexCoord.Origin);
        var state = GameEngineTestFixture.CreateState(blue);
        var engine = GameEngineTestFixture.CreateEngine();

        var move = new MoveCommand(blue.Id, [HexCoord.Origin, new HexCoord(3, 0)]);

        Assert.Throws<InvalidOperationException>(() => engine.Apply(state, move));
    }


    [Fact]
    public void Apply_throws_when_path_exceeds_movement_points()
    {
        // Infantry has MovementPoints = 3 and Plain costs 1 per hex.
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, HexCoord.Origin);
        var state = GameEngineTestFixture.CreateState(blue);
        var engine = GameEngineTestFixture.CreateEngine();

        var path = new List<HexCoord>
        {
            HexCoord.Origin, new(1, 0), new(2, 0), new(3, 0), new(4, 0)
        };
        var move = new MoveCommand(blue.Id, path);

        Assert.Throws<InvalidOperationException>(() => engine.Apply(state, move));
    }


    [Fact]
    public void Apply_throws_when_path_enters_an_occupied_hex()
    {
        var blue = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, HexCoord.Origin);
        var blocker = GameEngineTestFixture.Infantry(2, GameEngineTestFixture.RedFactionId, new HexCoord(1, 0));
        var state = GameEngineTestFixture.CreateState(blue, blocker);
        var engine = GameEngineTestFixture.CreateEngine();

        var move = new MoveCommand(blue.Id, [HexCoord.Origin, new HexCoord(1, 0)]);

        Assert.Throws<InvalidOperationException>(() => engine.Apply(state, move));
    }


    [Fact]
    public void Apply_throws_when_unit_belongs_to_other_side()
    {
        var red = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.RedFactionId, HexCoord.Origin);
        var state = GameEngineTestFixture.CreateState(red);
        var engine = GameEngineTestFixture.CreateEngine();

        var move = new MoveCommand(red.Id, [HexCoord.Origin, new HexCoord(1, 0)]);

        Assert.Throws<InvalidOperationException>(() => engine.Apply(state, move));
    }


    [Fact]
    public void Apply_throws_when_command_argument_is_null()
    {
        var engine = GameEngineTestFixture.CreateEngine();
        var state = GameEngineTestFixture.CreateState();

        Assert.Throws<ArgumentNullException>(() => engine.Apply(state, null!));
    }


    [Fact]
    public void Apply_throws_when_state_argument_is_null()
    {
        var engine = GameEngineTestFixture.CreateEngine();

        Assert.Throws<ArgumentNullException>(() => engine.Apply(null!, new EndTurnCommand()));
    }


    [Fact]
    public void Constructor_throws_when_factions_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => new Core.Engine.GameEngine(null!));
    }
}
