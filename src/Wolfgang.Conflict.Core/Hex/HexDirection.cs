namespace Wolfgang.Conflict.Core.Hex;

/// <summary>
/// The six directions of movement on a pointy-top hex grid, listed
/// clockwise starting from East.
/// </summary>
public enum HexDirection
{
    /// <summary>Due east.</summary>
    East = 0,

    /// <summary>Down and to the right.</summary>
    SouthEast = 1,

    /// <summary>Down and to the left.</summary>
    SouthWest = 2,

    /// <summary>Due west.</summary>
    West = 3,

    /// <summary>Up and to the left.</summary>
    NorthWest = 4,

    /// <summary>Up and to the right.</summary>
    NorthEast = 5
}
