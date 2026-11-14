using Wolfgang.Conflict.Core.Events;
using Wolfgang.Conflict.Core.State;

namespace Wolfgang.Conflict.Core.Engine;

/// <summary>
/// The result of applying a command to a <see cref="GameState"/>: the new
/// state and the events that were emitted.
/// </summary>
/// <param name="State">The state after the command was applied.</param>
/// <param name="Events">Events emitted while applying the command, in order.</param>
public sealed record ApplyResult(GameState State, IReadOnlyList<GameEvent> Events);
