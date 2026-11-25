namespace Wolfgang.Conflict.Core.Hex;

/// <summary>
/// A* pathfinder over a hex grid. Generic over the neighbor-and-cost
/// function so the same algorithm serves both unweighted distance
/// queries and movement on a fully terrain-costed map.
/// </summary>
public static class HexPathfinder
{
    /// <summary>
    /// Finds the cheapest path from <paramref name="start"/> to <paramref name="goal"/>
    /// using A* with the hex distance heuristic.
    /// </summary>
    /// <param name="start">The starting hex.</param>
    /// <param name="goal">The destination hex.</param>
    /// <param name="getNeighbors">
    /// Function returning the passable neighbors of a hex along with the
    /// cost of moving from that hex to each neighbor. Costs must be
    /// positive integers (&gt;= 1). Zero or negative costs break the
    /// distance heuristic's admissibility and would allow A* to return
    /// a non-optimal path.
    /// </param>
    /// <param name="maxCost">
    /// Optional upper bound on total path cost. Must be non-negative when
    /// supplied. If the cheapest path would exceed this, the search returns
    /// <see langword="null"/>.
    /// </param>
    /// <returns>
    /// The cheapest path as a <see cref="HexPath"/>, or <see langword="null"/>
    /// if no path exists within <paramref name="maxCost"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getNeighbors"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxCost"/> is negative.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="getNeighbors"/> returned a non-positive edge cost.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Accumulated path cost or A* priority overflows <see cref="int"/>.
    /// </exception>
    public static HexPath? FindPath
    (
        HexCoord start,
        HexCoord goal,
        Func<HexCoord, IEnumerable<(HexCoord Neighbor, int Cost)>> getNeighbors,
        int? maxCost = null
    )
    {
        ArgumentNullException.ThrowIfNull(getNeighbors);

        if (maxCost is < 0) throw new ArgumentOutOfRangeException(nameof(maxCost), maxCost, "maxCost must be non-negative.");

        if (start == goal)
        {
            return new HexPath([start], 0);
        }

        var cameFrom = new Dictionary<HexCoord, HexCoord>();
        var costSoFar = new Dictionary<HexCoord, int> { [start] = 0 };
        var frontier = new PriorityQueue<HexCoord, int>();
        frontier.Enqueue(start, start.DistanceTo(goal));

        while (frontier.TryDequeue(out var current, out _))
        {
            if (current == goal)
            {
                return Reconstruct(cameFrom, costSoFar, start, goal);
            }

            var currentCost = costSoFar[current];

            foreach (var (next, stepCost) in getNeighbors(current))
            {
                if (stepCost < 1)
                {
                    throw new InvalidOperationException
                    (
                        $"Non-positive edge cost from {current} to {next}: {stepCost}. "
                        + "A*'s distance heuristic requires stepCost >= 1 to remain admissible."
                    );
                }

                // checked: prevents int wraparound from corrupting cost comparisons
                // or maxCost pruning on pathological inputs.
                int newCost = checked(currentCost + stepCost);

                if (maxCost is { } cap && newCost > cap)
                {
                    continue;
                }

                if (costSoFar.TryGetValue(next, out var existing) && newCost >= existing)
                {
                    continue;
                }

                costSoFar[next] = newCost;
                cameFrom[next] = current;
                int priority = checked(newCost + next.DistanceTo(goal));
                frontier.Enqueue(next, priority);
            }
        }

        return null;
    }


    /// <summary>
    /// Enumerates every hex reachable from <paramref name="start"/> within
    /// <paramref name="maxCost"/> movement points, returning the cheapest
    /// path to each. The starting hex is included with cost 0.
    /// </summary>
    /// <param name="start">Origin hex.</param>
    /// <param name="getNeighbors">
    /// Function returning passable neighbors with edge costs (see <see cref="FindPath"/>).
    /// </param>
    /// <param name="maxCost">
    /// Inclusive upper bound on cumulative cost. Hexes whose cheapest path
    /// exceeds this are excluded.
    /// </param>
    /// <returns>
    /// Dictionary keyed by reachable hex; values are the cheapest path to that hex.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="getNeighbors"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxCost"/> is negative.</exception>
    /// <exception cref="InvalidOperationException">A returned edge cost is negative.</exception>
    public static IReadOnlyDictionary<HexCoord, HexPath> FindReachable
    (
        HexCoord start,
        Func<HexCoord, IEnumerable<(HexCoord Neighbor, int Cost)>> getNeighbors,
        int maxCost
    )
    {
        ArgumentNullException.ThrowIfNull(getNeighbors);
        ArgumentOutOfRangeException.ThrowIfNegative(maxCost);

        var cameFrom = new Dictionary<HexCoord, HexCoord>();
        var costSoFar = new Dictionary<HexCoord, int> { [start] = 0 };
        var frontier = new PriorityQueue<HexCoord, int>();
        frontier.Enqueue(start, 0);

        while (frontier.TryDequeue(out var current, out _))
        {
            var currentCost = costSoFar[current];

            foreach (var (next, stepCost) in getNeighbors(current))
            {
                if (stepCost < 0)
                {
                    throw new InvalidOperationException
                    (
                        $"Negative edge cost from {current} to {next}: {stepCost}."
                    );
                }

                var newCost = currentCost + stepCost;

                if (newCost > maxCost)
                {
                    continue;
                }

                if (costSoFar.TryGetValue(next, out var existing) && newCost >= existing)
                {
                    continue;
                }

                costSoFar[next] = newCost;
                cameFrom[next] = current;
                frontier.Enqueue(next, newCost);
            }
        }

        var result = new Dictionary<HexCoord, HexPath>(costSoFar.Count);
        foreach (var (hex, cost) in costSoFar)
        {
            result[hex] = ReconstructFrom(cameFrom, costSoFar, start, hex);
        }
        return result;
    }


    private static HexPath ReconstructFrom
    (
        Dictionary<HexCoord, HexCoord> cameFrom,
        Dictionary<HexCoord, int> costSoFar,
        HexCoord start,
        HexCoord goal
    )
    {
        var hexes = new List<HexCoord>();
        var node = goal;

        while (node != start)
        {
            hexes.Add(node);
            node = cameFrom[node];
        }

        hexes.Add(start);
        hexes.Reverse();

        return new HexPath(hexes, costSoFar[goal]);
    }


    private static HexPath Reconstruct
    (
        Dictionary<HexCoord, HexCoord> cameFrom,
        Dictionary<HexCoord, int> costSoFar,
        HexCoord start,
        HexCoord goal
    )
    {
        var hexes = new List<HexCoord>();
        var node = goal;

        while (node != start)
        {
            hexes.Add(node);
            node = cameFrom[node];
        }

        hexes.Add(start);
        hexes.Reverse();

        return new HexPath(hexes, costSoFar[goal]);
    }
}
