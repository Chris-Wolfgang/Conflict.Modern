using Wolfgang.Conflict.Core.Catalog;

namespace Wolfgang.Conflict.Core.Vision;

/// <summary>
/// Per-terrain vision properties: which terrains block line of sight when
/// stood between two hexes, and the sight-range bonus an observer earns
/// for occupying elevated terrain.
/// </summary>
public static class TerrainVision
{
    /// <summary>
    /// Whether <paramref name="terrain"/> blocks ground-level LOS passing
    /// through a hex with this terrain.
    /// </summary>
    public static bool BlocksLineOfSight(Terrain terrain) => terrain switch
    {
        Terrain.Forest => true,
        Terrain.Urban => true,
        Terrain.Mountain => true,
        _ => false
    };


    /// <summary>
    /// Bonus to an observer's sight range when standing on <paramref name="terrain"/>.
    /// Captures the "high ground sees farther" rule from the plan.
    /// </summary>
    public static int ElevationSightBonus(Terrain terrain) => terrain switch
    {
        Terrain.Hills => 1,
        Terrain.Mountain => 2,
        _ => 0
    };
}
