using System.Data.Common;

using TrainTrackSolverLib.Common;

namespace TrainTrackSolverLib;
public class Solver : ISolver
{
    private readonly Grid _grid;

    // Progress reporting
    private long _attemptCount;
    IProgressReporter _progressReporter;

    public long IterationCount => _attemptCount;

    /// <summary>
    /// Constructor for the Solver class.
    /// </summary>
    /// <param name="grid">Grid to solve for</param>
    public Solver(Grid grid, IProgressReporter reporter)
    {
        _grid = grid;
        _progressReporter = reporter;
        _attemptCount = 0;
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
        if (_attemptCount % _progressReporter.ProgressInterval == 0)
            _progressReporter.Report(_attemptCount);

        // Prune infeasible branches early
        if (!_grid.CanStillSatisfy()) return false;

        if (IsComplete()) return true;

        // Build priority queue of empty cells by legal options count
        var pq = new PriorityQueue<Point, int>();
        var seen = new HashSet<Point>();
        var directions = new[] { (0,1),(1,0),(0,-1),(-1,0) };
        // Collect empties adjacent to filled track pieces
        for (int rr = 0; rr < _grid.Rows; rr++)
            for (int cc = 0; cc < _grid.Cols; cc++)
            {
                var pos = new Point(rr, cc);
                if (_grid.IsEmpty(pos)) continue;
                foreach (var (dr, dc) in directions)
                {
                    var next = pos + (dr, dc);
                    if (!_grid.IsInBounds(next) || _grid.IsFilled(next)) continue;
                    seen.Add(next);
                }
            }
        // Fallback to all empties if none adjacent to tracks
        if (seen.Count == 0)
        {
            for (int rr = 0; rr < _grid.Rows; rr++)
                for (int cc = 0; cc < _grid.Cols; cc++) {
                    var pos = new Point(rr, cc);
                    if (!_grid.IsFilled(pos))
                        seen.Add(pos);
                }
        }
        // Enqueue candidates by option count
        foreach (var pos in seen)
        {
            var options = _grid.GetLegalPieces(pos).Count();
            if (options == 0) continue; // dead end
            pq.Enqueue(pos, options);
        }

        if (pq.Count == 0) return false;

        // Dequeue most constrained cell
        var p = pq.Dequeue();

        foreach (var piece in _grid.GetLegalPieces(p)) {
            _grid.Place(p, piece);

            if (Solve()) {
                return true;
            }
            _grid.Remove(p);
        }
        return false;
    }

    /// <summary>
    /// Check if the grid is complete and satisfies all constraints.
    /// </summary>
    /// <returns></returns>
    private bool IsComplete()
    {
        // Ensure a single path exists and all constraints are satisfied
        return _grid.TrackCountInAllRowsColsMatch() && _grid.IsSingleConnectedPath();
    }
}