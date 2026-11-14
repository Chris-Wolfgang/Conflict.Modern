namespace Wolfgang.Conflict.Core.Map;

/// <summary>
/// Optional structure occupying a hex. Structures can be captured by infantry
/// and may produce units (factories), grant income (cities), or define victory
/// conditions (HQ).
/// </summary>
public enum StructureType
{
    /// <summary>No structure on this hex.</summary>
    None,

    /// <summary>Generic city — typically captured for income.</summary>
    City,

    /// <summary>Headquarters — captured to win.</summary>
    Hq,

    /// <summary>Vehicle factory — produces ground vehicles.</summary>
    VehicleFactory,

    /// <summary>Airbase — produces aircraft.</summary>
    Airbase,

    /// <summary>Barracks — produces infantry.</summary>
    Barracks
}
