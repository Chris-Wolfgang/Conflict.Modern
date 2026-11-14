namespace Wolfgang.Conflict.Core.State;

/// <summary>
/// Rules selected for a single match. Many of these are stubs in v1 (the
/// engine only enforces a subset) but are defined now so saved games and
/// match-setup screens can carry them forward as features land.
/// </summary>
public sealed record RulesConfig
{
    /// <summary>Default config used when none is specified — classic IGOUGO with explored fog.</summary>
    public static RulesConfig Default { get; } = new();

    /// <summary>Order in which sides take turns (IGOUGO vs alternating activation).</summary>
    public TurnOrderMode TurnOrder { get; init; } = TurnOrderMode.Igougo;

    /// <summary>Per-unit action policy (classic move-or-attack vs free-flow).</summary>
    public UnitActionPolicy UnitActionPolicy { get; init; } = UnitActionPolicy.Classic;

    /// <summary>Strategic fog of war mode for the match.</summary>
    public FogMode FogMode { get; init; } = FogMode.Explored;

    /// <summary>Whether spotting reveals a unit's full identity or only its presence.</summary>
    public bool DetectionRevealsIdentity { get; init; } = true;

    /// <summary>Whether all units of a faction share visibility (the typical wargame default).</summary>
    public bool SharedVisionPerFaction { get; init; } = true;
}
