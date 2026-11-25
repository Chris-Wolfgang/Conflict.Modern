namespace Wolfgang.Conflict.Core.Events;

/// <summary>
/// One side ended its turn. <see cref="NextSideFactionId"/> is now the
/// active side. <see cref="NewTurnNumber"/> only increments when the
/// last side in the order ends its turn (a full round completed).
/// </summary>
/// <param name="EndedSideFactionId">Faction whose turn just ended.</param>
/// <param name="NextSideFactionId">Faction whose turn just started.</param>
/// <param name="NewTurnNumber">Turn number after the transition.</param>
public sealed record TurnEnded
(
    string EndedSideFactionId,
    string NextSideFactionId,
    int NewTurnNumber
) : GameEvent;
