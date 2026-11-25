namespace Wolfgang.Conflict.Core.Events;

/// <summary>
/// Base type for an observable thing that happened during command application.
/// UIs consume events for animation, sound, and the in-game log; AI uses them
/// to react; replay/network systems serialize them as the canonical record
/// of what occurred.
/// </summary>
public abstract record GameEvent;
