namespace Wolfgang.Conflict.Core.Catalog;

/// <summary>
/// Coarse classification of a unit by role and movement domain. Used by AI heuristics,
/// matchmaking of weapon damage tables, and UI iconography.
/// </summary>
public enum UnitArchetype
{
    /// <summary>Foot infantry — slow, can capture objectives.</summary>
    Infantry,

    /// <summary>Main battle tank or equivalent heavy armor.</summary>
    Tank,

    /// <summary>Self-propelled or towed artillery (indirect fire).</summary>
    Artillery,

    /// <summary>Light wheeled scout — fast, weak, long sight range.</summary>
    Recon,

    /// <summary>Rotary-wing aircraft — flies, vulnerable to AA.</summary>
    Helicopter,

    /// <summary>Fixed-wing fighter — fastest unit, air-to-air primary.</summary>
    Fighter
}
