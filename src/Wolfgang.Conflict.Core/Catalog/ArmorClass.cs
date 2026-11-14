namespace Wolfgang.Conflict.Core.Catalog;

/// <summary>
/// The armor category a unit presents to incoming weapons. Damage tables
/// on weapons are keyed by this enum.
/// </summary>
public enum ArmorClass
{
    /// <summary>Soft targets — infantry and unarmored personnel.</summary>
    Infantry,

    /// <summary>Light armor — APCs, IFVs, recon vehicles, helicopters from above.</summary>
    LightArmor,

    /// <summary>Heavy armor — main battle tanks.</summary>
    HeavyArmor,

    /// <summary>Aircraft — fighters, bombers, helicopters from below.</summary>
    Aircraft
}
