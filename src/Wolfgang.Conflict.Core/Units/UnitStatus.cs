namespace Wolfgang.Conflict.Core.Units;

/// <summary>
/// Runtime status of a unit. Most units are <see cref="Active"/>.
/// </summary>
public enum UnitStatus
{
    /// <summary>Unit is fully operational.</summary>
    Active,

    /// <summary>
    /// Unit has run out of fuel. Ground units in this state can defend if
    /// attacked but cannot initiate movement or attacks. Aircraft in this
    /// state crash and are removed.
    /// </summary>
    OutOfFuel,

    /// <summary>
    /// Unit has surrendered (typically because it ran out of viable options).
    /// Surrendered units take no further actions and may be captured.
    /// </summary>
    Surrendered
}
