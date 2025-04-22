using System.Data.Common;

namespace TrainTrackSolverLib;
public class Solver
{
    private readonly Grid _grid;

    // Progress reporting
    private long _attemptCount;
    public Action<long>? 
    ProgressCallback { get; set; }
    private const long ProgressInterval = 10000;

    public long AttemptCount => _attemptCount;

    /// <summary>
    /// Constructor for the Solver class.
    /// </summary>
    /// <param name="grid">Grid to solve for</param>
    public Solver(Grid grid)
    {
        _grid = grid;
    }

    /// <summary>
    /// Recursively solve the grid using backtracking.
    /// This method uses a priority queue to select the most constrained cell first,
    /// and prunes infeasible branches early.
    /// It also reports progress at regular intervals.
    /// </summary>
    /// <returns>True if the grid can be solved</returns>
    public bool Solve()
    {
        // Report progress
        _attemptCount++;
        if (ProgressCallback != null && _attemptCount % ProgressInterval == 0)
            ProgressCallback.Invoke(_attemptCount);

        // Prune infeasible branches early
        if (!_grid.CanStillSatisfy()) return false;

        if (IsComplete()) return true;

        // Build priority queue of empty cells by legal options count
        var pq = new PriorityQueue<(int r, int c), int>();
        var seen = new HashSet<(int r, int c)>();
        var directions = new[] { (0,1),(1,0),(0,-1),(-1,0) };
        // Collect empties adjacent to filled track pieces
        for (int rr = 0; rr < _grid.Rows; rr++)
            for (int cc = 0; cc < _grid.Cols; cc++)
            {
                if (_grid.IsEmpty(rr, cc)) continue;
                foreach (var (dr, dc) in directions)
                {
                    int nr = rr + dr, nc = cc + dc;
                    if (!_grid.IsInBounds(nr, nc) || _grid.IsFilled(nr, nc)) continue;
                    seen.Add((nr, nc));
                }
            }
        // Fallback to all empties if none adjacent to tracks
        if (seen.Count == 0)
        {
            for (int rr = 0; rr < _grid.Rows; rr++)
                for (int cc = 0; cc < _grid.Cols; cc++) {
                    if (!_grid.IsFilled(rr, cc))
                        seen.Add((rr, cc));
                }
        }
        // Enqueue candidates by option count
        foreach (var (r, c) in seen)
        {
            var options = _grid.GetLegalPieces(r, c).Count();
            if (options == 0) continue; // dead end
            pq.Enqueue((r, c), options);
        }

        if (pq.Count == 0) return false;

        // Dequeue most constrained cell
        var (row, col) = pq.Dequeue();

        foreach (var piece in _grid.GetLegalPieces(row, col)) {
            _grid.Place(row, col, piece);
            //BlockRowAndColIfFilled(row, col);

            if (Solve()) {
                return true;
            }
            _grid.Remove(row, col);
            //UnblockRowAndColIfNotFilled(row, col);
        }
        return false;
    }

    /// <summary>
    /// Check if the grid is complete and satisfies all constraints.
    /// </summary>
    /// <returns></returns>
    private bool IsComplete()
    {
        // Ensure row counts are satisfied
        for (int r = 0; r < _grid.Rows; r++)
            if (_grid.TrackCountInRow(r) != _grid.RowCounts[r])
                return false;

        // Ensure column counts are satisfied
        for (int c = 0; c < _grid.Cols; c++)
            if (_grid.TrackCountInCol(c) != _grid.ColCounts[c])
                return false;

        // Ensure a single path exists
        return _grid.IsSingleConnectedPath();
    }
}