namespace Wolfgang.Conflict.Core.Catalog;

/// <summary>
/// The base set of map terrain types. Each <see cref="UnitTypeDefinition"/>
/// declares its movement cost into each terrain (or marks it impassable
/// by omitting the entry).
/// </summary>
public enum Terrain
{
    /// <summary>Open plain — fast movement, no concealment.</summary>
    Plain,

    /// <summary>Forest — slow movement, partial cover.</summary>
    Forest,

    /// <summary>Rolling hills — moderate movement cost, extends sight from on top.</summary>
    Hills,

    /// <summary>Mountains — slow or impassable for ground units, extends sight.</summary>
    Mountain,

    /// <summary>Urban — slow movement, strong cover.</summary>
    Urban,

    /// <summary>Open water — impassable to most ground units.</summary>
    Water
}
