namespace Wolfgang.Conflict.Core.State;

/// <summary>How many actions a single unit can take per turn.</summary>
public enum UnitActionPolicy
{
    /// <summary>One move and one attack per turn, in either order, but neither repeats.</summary>
    Classic,

    /// <summary>Interleave moves and attacks until movement points / attack quota run out.</summary>
    FreeFlow
}
