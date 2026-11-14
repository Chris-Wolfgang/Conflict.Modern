using Wolfgang.Conflict.Core.Catalog;
using Wolfgang.Conflict.Core.Hex;

namespace Wolfgang.Conflict.Core.Map;

/// <summary>
/// A hex grid map — a sparse dictionary of <see cref="Tile"/>s keyed by
/// <see cref="HexCoord"/>. Sparse storage supports irregular shapes
/// (rectangles, circles, hand-authored curves) without paying for unused cells.
/// </summary>
/// <remarks>
/// The map carries terrain only — units and runtime state live in
/// <c>GameState</c>. Map instances are immutable after construction.
/// </remarks>
public sealed class HexMap
{
    private readonly IReadOnlyDictionary<HexCoord, Tile> _tiles;


    /// <summary>
    /// Construct a map directly from a tile dictionary. Typically used by
    /// loaders and procedural generators; tests prefer <see cref="OfRectangle"/>.
    /// </summary>
    /// <param name="tiles">Hex coordinate → tile mapping. The map takes a snapshot.</param>
    public HexMap(IReadOnlyDictionary<HexCoord, Tile> tiles)
    {
        ArgumentNullException.ThrowIfNull(tiles);
        _tiles = tiles.ToDictionary(kv => kv.Key, kv => kv.Value);
    }


    /// <summary>
    /// All hex coordinates that exist on this map.
    /// </summary>
    public IEnumerable<HexCoord> Coords => _tiles.Keys;


    /// <summary>
    /// Number of tiles on the map.
    /// </summary>
    public int Count => _tiles.Count;


    /// <summary>
    /// Whether <paramref name="coord"/> is a valid hex on this map.
    /// </summary>
    public bool Contains(HexCoord coord) => _tiles.ContainsKey(coord);


    /// <summary>
    /// Returns the tile at <paramref name="coord"/>.
    /// </summary>
    /// <exception cref="KeyNotFoundException">No tile at that coordinate.</exception>
    public Tile this[HexCoord coord] => _tiles[coord];


    /// <summary>
    /// Try to fetch the tile at <paramref name="coord"/>; returns <see langword="false"/>
    /// if the hex is off the map.
    /// </summary>
    public bool TryGetTile(HexCoord coord, out Tile tile)
    {
        if (_tiles.TryGetValue(coord, out var found))
        {
            tile = found;
            return true;
        }

        tile = null!;
        return false;
    }


    /// <summary>
    /// Construct a rectangular map of <paramref name="width"/> × <paramref name="height"/> hexes,
    /// every tile filled with <paramref name="terrain"/>. Uses the standard
    /// pointy-top axial offset rectangle.
    /// </summary>
    /// <param name="width">Number of columns. Must be positive.</param>
    /// <param name="height">Number of rows. Must be positive.</param>
    /// <param name="terrain">Terrain for every tile.</param>
    /// <exception cref="ArgumentOutOfRangeException">Either dimension is &lt;= 0.</exception>
    public static HexMap OfRectangle(int width, int height, Terrain terrain)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        var tiles = new Dictionary<HexCoord, Tile>(capacity: width * height);

        for (var r = 0; r < height; r++)
        {
            var qOffset = -(r / 2);
            for (var q = qOffset; q < qOffset + width; q++)
            {
                tiles[new HexCoord(q, r)] = new Tile(terrain);
            }
        }

        return new HexMap(tiles);
    }
}
