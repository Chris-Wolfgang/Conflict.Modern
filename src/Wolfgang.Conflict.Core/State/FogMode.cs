namespace Wolfgang.Conflict.Core.State;

/// <summary>Strategic fog of war mode.</summary>
public enum FogMode
{
    /// <summary>No fog — like the original game.</summary>
    Off,

    /// <summary>Once seen, terrain stays revealed; enemy units only show in active sight.</summary>
    Explored,

    /// <summary>Hexes revert to unknown once sight is lost.</summary>
    Strict
}
