namespace Wolfgang.Conflict.Core.State;

/// <summary>How sides interleave their turns.</summary>
public enum TurnOrderMode
{
    /// <summary>Each side takes a complete turn, then the next side takes one ("I Go, You Go").</summary>
    Igougo,

    /// <summary>Sides alternate one (or N) unit activations.</summary>
    AlternatingActivation
}
