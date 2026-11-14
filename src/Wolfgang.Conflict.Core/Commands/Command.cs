namespace Wolfgang.Conflict.Core.Commands;

/// <summary>
/// Base type for an action a player (human or AI) submits to the engine.
/// Commands are validated and applied via <c>GameEngine.Apply</c>.
/// </summary>
public abstract record Command;
