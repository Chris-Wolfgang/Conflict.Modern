namespace Wolfgang.Conflict.Core.Commands;

/// <summary>
/// End the current side's turn and advance to the next side. When the
/// last side ends its turn the round increments and per-unit turn flags
/// (HasMoved / HasAttacked) reset.
/// </summary>
public sealed record EndTurnCommand : Command;
