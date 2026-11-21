using Wolfgang.Conflict.Core.Catalog;
using Wolfgang.Conflict.Core.Commands;
using Wolfgang.Conflict.Core.Events;
using Wolfgang.Conflict.Core.Hex;
using Wolfgang.Conflict.Core.State;
using Wolfgang.Conflict.Core.Units;

namespace Wolfgang.Conflict.Core.Engine;

/// <summary>
/// The single source of truth for game rules. Both human input and AI
/// players go through the same legality checks (<see cref="GetLegalCommands"/>,
/// <see cref="GetLegalMoves"/>) and the same state transition (<see cref="Apply"/>).
/// This rules out drift between what humans and AI consider legal.
/// </summary>
/// <remarks>
/// The engine is constructed once per match with the faction catalogs that
/// are in play. State is never stored on the engine; everything flows through
/// <see cref="GameState"/>.
/// </remarks>
public sealed class GameEngine
{
    private readonly IReadOnlyDictionary<string, FactionDefinition> _factions;


    /// <param name="factions">All factions participating in the match, keyed by faction id.</param>
    /// <exception cref="ArgumentNullException"><paramref name="factions"/> is <see langword="null"/>.</exception>
    public GameEngine(IReadOnlyDictionary<string, FactionDefinition> factions)
    {
        ArgumentNullException.ThrowIfNull(factions);
        _factions = factions;
    }


    /// <summary>
    /// All legal commands the given faction may submit on its turn —
    /// every legal move for every still-eligible unit, plus an
    /// <see cref="EndTurnCommand"/>. The faction must be the current side.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="state"/> is <see langword="null"/>.</exception>
    public IEnumerable<Command> GetLegalCommands(GameState state, string factionId)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (!string.Equals(state.CurrentSideFactionId, factionId, StringComparison.Ordinal))
        {
            yield break;
        }

        foreach (var unit in state.UnitsOfFaction(factionId))
        {
            foreach (var move in GetLegalMoves(state, unit.Id))
            {
                yield return move;
            }
        }

        yield return new EndTurnCommand();
    }


    /// <summary>
    /// All legal <see cref="MoveCommand"/>s for a single unit given the
    /// current state. Returns nothing if the unit cannot move (wrong side,
    /// already moved, surrendered, etc.).
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="state"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException">No unit with that id.</exception>
    public IEnumerable<MoveCommand> GetLegalMoves(GameState state, int unitId)
    {
        ArgumentNullException.ThrowIfNull(state);

        var unit = state.Units[unitId];

        if (!CanInitiateMove(state, unit, out var unitType))
        {
            yield break;
        }

        var reachable = HexPathfinder.FindReachable
        (
            unit.Position,
            from => NeighborsForUnit(state, unit, unitType, from),
            unitType.MovementPoints
        );

        foreach (var (hex, path) in reachable)
        {
            if (hex == unit.Position)
            {
                continue; // a "move" to where you started is not an action
            }
            yield return new MoveCommand(unit.Id, path.Hexes);
        }
    }


    /// <summary>
    /// Apply <paramref name="command"/> to <paramref name="state"/>, returning the
    /// resulting state and the events that occurred. Pure / deterministic.
    /// </summary>
    /// <exception cref="ArgumentNullException">An argument is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The command is illegal in the given state.</exception>
    public ApplyResult Apply(GameState state, Command command)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(command);

        return command switch
        {
            MoveCommand move => ApplyMove(state, move),
            EndTurnCommand => ApplyEndTurn(state),
            _ => throw new InvalidOperationException($"Unsupported command type: {command.GetType().Name}.")
        };
    }


    private ApplyResult ApplyMove(GameState state, MoveCommand command)
    {
        var unit = state.Units[command.UnitId];

        if (!CanInitiateMove(state, unit, out var unitType))
        {
            throw new InvalidOperationException
            (
                $"Unit {unit.Id} ({unit.UnitTypeId}) cannot move right now."
            );
        }

        var totalCost = ValidatePathAndComputeCost(state, unit, unitType, command.Path);

        var destination = command.Path[^1];
        var movedUnit = unit with { Position = destination, HasMoved = true };

        var newUnits = new Dictionary<int, Unit>(state.Units)
        {
            [movedUnit.Id] = movedUnit
        };

        var newState = state with { Units = newUnits };
        var events = new List<GameEvent> { new UnitMoved(unit.Id, command.Path, totalCost) };

        return new ApplyResult(newState, events);
    }


    private static int ValidatePathAndComputeCost
    (
        GameState state,
        Unit unit,
        UnitTypeDefinition unitType,
        IReadOnlyList<HexCoord> path
    )
    {
        if (path is null)
        {
            throw new InvalidOperationException("Move path is null.");
        }

        if (path.Count < 2)
        {
            throw new InvalidOperationException("Move path must include start and at least one destination hex.");
        }

        if (path[0] != unit.Position)
        {
            throw new InvalidOperationException
            (
                $"Move path starts at {path[0]} but unit is at {unit.Position}."
            );
        }

        var totalCost = 0;

        for (var i = 1; i < path.Count; i++)
        {
            totalCost += ValidatePathStep(state, unit, unitType, path[i - 1], path[i]);
        }

        if (totalCost > unitType.MovementPoints)
        {
            throw new InvalidOperationException
            (
                $"Move costs {totalCost} but unit has only {unitType.MovementPoints} movement points."
            );
        }

        return totalCost;
    }


    private static int ValidatePathStep
    (
        GameState state,
        Unit unit,
        UnitTypeDefinition unitType,
        HexCoord from,
        HexCoord to
    )
    {
        if (from.DistanceTo(to) != 1)
        {
            throw new InvalidOperationException($"Move path step {from} -> {to} is not a hex neighbor.");
        }

        if (!state.Map.TryGetTile(to, out var tile))
        {
            throw new InvalidOperationException($"Move path leaves the map at {to}.");
        }

        var occupant = state.UnitAt(to);
        if (occupant is not null && occupant.Id != unit.Id)
        {
            throw new InvalidOperationException($"Move path enters occupied hex {to}.");
        }

        if (!unitType.TerrainMovementCosts.TryGetValue(tile.Terrain, out var stepCost))
        {
            throw new InvalidOperationException
            (
                $"Unit type {unit.UnitTypeId} cannot enter {tile.Terrain} at {to}."
            );
        }

        return stepCost;
    }


    private static ApplyResult ApplyEndTurn(GameState state)
    {
        var endingSide = state.CurrentSideFactionId;
        var nextIndex = (state.CurrentSideIndex + 1) % state.SideOrder.Count;
        var roundCompleted = nextIndex == 0;
        var nextTurn = roundCompleted ? state.TurnNumber + 1 : state.TurnNumber;

        var resetUnits = new Dictionary<int, Unit>(state.Units.Count);
        foreach (var (id, unit) in state.Units)
        {
            resetUnits[id] = unit with { HasMoved = false, HasAttacked = false };
        }

        var newState = state with
        {
            Units = resetUnits,
            CurrentSideIndex = nextIndex,
            TurnNumber = nextTurn
        };

        var nextSide = newState.CurrentSideFactionId;
        var events = new List<GameEvent> { new TurnEnded(endingSide, nextSide, nextTurn) };

        return new ApplyResult(newState, events);
    }


    private bool CanInitiateMove(GameState state, Unit unit, out UnitTypeDefinition unitType)
    {
        unitType = ResolveUnitType(unit);

        if (!string.Equals(unit.OwnerFactionId, state.CurrentSideFactionId, StringComparison.Ordinal))
        {
            return false;
        }

        if (unit.HasMoved)
        {
            return false;
        }

        if (unit.Status != UnitStatus.Active)
        {
            return false;
        }

        return true;
    }


    private UnitTypeDefinition ResolveUnitType(Unit unit)
    {
        if (!_factions.TryGetValue(unit.OwnerFactionId, out var faction))
        {
            throw new InvalidOperationException
            (
                $"Unit {unit.Id} references unknown faction '{unit.OwnerFactionId}'."
            );
        }

        if (!faction.UnitTypes.TryGetValue(unit.UnitTypeId, out var unitType))
        {
            throw new InvalidOperationException
            (
                $"Faction '{unit.OwnerFactionId}' has no unit type '{unit.UnitTypeId}'."
            );
        }

        return unitType;
    }


    private static IEnumerable<(HexCoord Neighbor, int Cost)> NeighborsForUnit
    (
        GameState state,
        Unit unit,
        UnitTypeDefinition unitType,
        HexCoord from
    )
    {
        foreach (var neighbor in from.Neighbors())
        {
            if (!state.Map.TryGetTile(neighbor, out var tile))
            {
                continue;
            }

            if (!unitType.TerrainMovementCosts.TryGetValue(tile.Terrain, out var cost))
            {
                continue;
            }

            var occupant = state.UnitAt(neighbor);
            if (occupant is not null && occupant.Id != unit.Id)
            {
                continue;
            }

            yield return (neighbor, cost);
        }
    }
}
