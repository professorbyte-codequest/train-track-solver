namespace TrainTrackSolverLib;

/// <summary>
/// Static class to manage track connections.
/// Each track piece has specific directions it can connect to.
/// The connections are represented as a dictionary where the key is the piece type
/// and the value is an array of tuples representing the directions.
/// The directions are represented as (row change, column change) tuples.
/// </summary>
public static class TrackConnections
{
    /// <summary>
    /// Dictionary mapping each piece type to its possible connection directions.
    /// The directions are represented as (row change, column change) tuples.
    /// For example, (0, 1) means the piece can connect to the right.
    /// </summary>
    /// <remarks>
    /// The dictionary is static and readonly, meaning it is initialized once and cannot be changed.
    /// This is useful for performance and memory efficiency.
    /// </remarks>
    public static readonly Dictionary<PieceType, (int, int)[]> Directions = new()
    {
        [PieceType.Horizontal] = new[] { (0, -1), (0, 1) },
        [PieceType.Vertical] = new[] { (-1, 0), (1, 0) },
        [PieceType.CornerNE] = new[] { (-1, 0), (0, 1) },
        [PieceType.CornerNW] = new[] { (-1, 0), (0, -1) },
        [PieceType.CornerSE] = new[] { (1, 0), (0, 1) },
        [PieceType.CornerSW] = new[] { (1, 0), (0, -1) },
    };

    /// <summary>
    /// Get the directions that a piece can connect to.
    /// </summary>
    /// <param name="type">The type of the piece.</param>
    /// <returns>An array of tuples representing the directions.</returns>
    /// <remarks>
    /// The directions are represented as (row change, column change) tuples.
    /// For example, (0, 1) means the piece can connect to the right.
    /// </remarks>
    public static IEnumerable<(int dr, int dc)> GetConnections(PieceType type) =>
        Directions.TryGetValue(type, out var dirs) ? dirs : Array.Empty<(int, int)>();

    /// <summary>
    /// Finds a PieceType whose connections exactly include both directions.
    /// </summary>
    public static PieceType GetPieceForDirs((int dr,int dc) d1, (int dr,int dc) d2)
    {
        foreach (PieceType p in Enum.GetValues(typeof(PieceType)))
        {
            if (p == PieceType.Empty) continue;
            var conns = GetConnections(p);
            if (conns.Contains(d1) && conns.Contains(d2))
                return p;
        }
        throw new InvalidOperationException($"No piece connects dirs {d1} & {d2}");
    }


    /// <summary>
    /// Check if a piece type can connect to a given direction.
    /// </summary>
    /// <param name="type">The type of the piece.</param>
    /// <param name="dir">The direction to check.</param>
    /// <returns>True if the piece can connect to the direction, false otherwise.</returns>
    /// <remarks>
    /// The direction is represented as a tuple (row change, column change).
    /// For example, (0, 1) means the piece can connect to the right.
    /// </remarks>
    public static bool ConnectsTo(PieceType type, (int dr, int dc) dir) =>
        GetConnections(type).Contains(dir);
}