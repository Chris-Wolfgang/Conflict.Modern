using Wolfgang.Conflict.Core.Catalog;
using Wolfgang.Conflict.Core.Hex;
using Wolfgang.Conflict.Core.State;

namespace Wolfgang.Conflict.Core.Vision;

/// <summary>
/// Computes what each faction can see. Drives both the strategic fog of war
/// (UI rendering) and combat eligibility (mutual-LOS counter-attack rule).
/// </summary>
/// <remarks>
/// Per-unit visibility is the union of every hex within the unit's sight range
/// (extended by elevation bonus) that has a clear line of sight from the unit.
/// Faction visibility is the union across all that faction's units (i.e.
/// shared vision). The shared-vs-per-unit option from <see cref="RulesConfig"/>
/// is not yet enforced; v1 assumes shared.
/// </remarks>
public sealed class VisionSystem
{
    private readonly IReadOnlyDictionary<string, FactionDefinition> _factions;


    /// <param name="factions">All factions in the match, keyed by faction id.</param>
    /// <exception cref="ArgumentNullException"><paramref name="factions"/> is <see langword="null"/>.</exception>
    public VisionSystem(IReadOnlyDictionary<string, FactionDefinition> factions)
    {
        ArgumentNullException.ThrowIfNull(factions);
        _factions = factions;
    }


    /// <summary>
    /// Every hex visible to <paramref name="factionId"/> in the given state.
    /// </summary>
    /// <exception cref="ArgumentNullException">An argument is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="factionId"/> is not a known faction.</exception>
    public HashSet<HexCoord> ComputeVisibility(GameState state, string factionId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(factionId);

        if (!_factions.ContainsKey(factionId))
        {
            throw new InvalidOperationException
            (
                $"Unknown faction id '{factionId}'."
            );
        }

        var visible = new HashSet<HexCoord>();

        foreach (var unit in state.UnitsOfFaction(factionId))
        {
            var unitType = ResolveUnitType(unit.OwnerFactionId, unit.UnitTypeId);
            AddVisibleHexesFor(state, unit.Position, unitType, visible);
        }

        return visible;
    }


    /// <summary>
    /// Whether a straight line of sight exists from <paramref name="from"/>
    /// to <paramref name="to"/> on <paramref name="state"/>'s map. Intermediate
    /// hexes with sight-blocking terrain (forest, urban, mountain) interrupt
    /// LOS; off-map gaps along the line are treated as transparent.
    /// </summary>
    /// <param name="state">Current game state (carries the terrain map).</param>
    /// <param name="from">Observer hex.</param>
    /// <param name="to">Target hex.</param>
    /// <param name="observerIsAirborne">
    /// If <see langword="true"/>, ground-level terrain blockers are bypassed
    /// — aircraft see across forest, urban, and mountain.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="state"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="from"/> or <paramref name="to"/> is not on the map.</exception>
    public static bool HasLineOfSight(GameState state, HexCoord from, HexCoord to, bool observerIsAirborne)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (!state.Map.TryGetTile(from, out _))
        {
            throw new InvalidOperationException($"Observer hex {from} is not on the map.");
        }

        if (!state.Map.TryGetTile(to, out _))
        {
            throw new InvalidOperationException($"Target hex {to} is not on the map.");
        }

        if (from == to)
        {
            return true;
        }

        if (observerIsAirborne)
        {
            return true;
        }

        var line = HexLine.Draw(from, to);

        for (var i = 1; i < line.Count - 1; i++)
        {
            if (!state.Map.TryGetTile(line[i], out var tile))
            {
                continue;
            }

            if (TerrainVision.BlocksLineOfSight(tile.Terrain))
            {
                return false;
            }
        }

        return true;
    }


    private void AddVisibleHexesFor
    (
        GameState state,
        HexCoord origin,
        UnitTypeDefinition unitType,
        HashSet<HexCoord> visible
    )
    {
        if (!state.Map.TryGetTile(origin, out var originTile))
        {
            throw new InvalidOperationException($"Unit origin {origin} is not on the map.");
        }

        var bonus = TerrainVision.ElevationSightBonus(originTile.Terrain);
        var effectiveRange = unitType.SightRange + bonus;

        foreach (var hex in state.Map.Coords)
        {
            if (origin.DistanceTo(hex) > effectiveRange)
            {
                continue;
            }

            if (HasLineOfSight(state, origin, hex, unitType.CanFly))
            {
                visible.Add(hex);
            }
        }
    }


    private UnitTypeDefinition ResolveUnitType(string factionId, string unitTypeId)
    {
        if (!_factions.TryGetValue(factionId, out var faction))
        {
            throw new InvalidOperationException
            (
                $"Unit references unknown faction '{factionId}'."
            );
        }

        if (!faction.UnitTypes.TryGetValue(unitTypeId, out var unitType))
        {
            throw new InvalidOperationException
            (
                $"Faction '{factionId}' has no unit type '{unitTypeId}'."
            );
        }

        return unitType;
    }
}
