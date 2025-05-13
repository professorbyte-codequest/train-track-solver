// A new solver that starts from a fixed entry point (track piece on the edge with one off-grid connection)
// and builds one continuous path through fixed pieces while respecting row/column counts.
using System.Runtime.ExceptionServices;
using TrainTrackSolverLib.Common;

namespace TrainTrackSolverLib
{
    public class PathBuilderSolver : ISolver
    {
        private readonly Grid _grid;
        private readonly List<Point> _fixedPositions;
        private readonly int _targetFixedCount;
        private readonly int _totalCount;

        private (int dr, int dc) _entryIncoming;

        // Progress reporting
        private readonly IProgressReporter _progressReporter;
        private long _attemptCount;
        public long IterationCount => _attemptCount;

        public PathBuilderSolver(Grid grid, IProgressReporter reporter)
        {
            _grid = grid;
            _progressReporter = reporter;
            _attemptCount = 0;

            // Collect all pre-placed (fixed) track positions
            _fixedPositions = _grid.FixedPoints();

            _targetFixedCount = _fixedPositions.Count;

            _totalCount = grid.RowCounts.Sum();

            var connections = TrackConnections.GetConnections(_grid.Board[_grid.Entry.Y, _grid.Entry.X]);
            _entryIncoming = connections.Where(fp => !_grid.IsInBounds(_grid.Entry + fp))
                .Select(fp => (-fp.dr, -fp.dc))   
                .First();
        }

        /// <summary>
        /// Attempts to solve by building a single path from the fixed entry.
        /// </summary>
        public bool Solve()
            => TryBuild(_grid.Entry, _entryIncoming, new HashSet<Point>(), 0);

        private bool TryBuild(
            Point pos,
            (int dr, int dc) incoming,
            HashSet<Point> visited,
            int fixedHit)
        {
            // Report progress
            _attemptCount++;
            if (_attemptCount % _progressReporter.ProgressInterval == 0)
                _progressReporter.Report(_attemptCount);

            // Bounds & revisit check
            if (!_grid.IsInBounds(pos) || visited.Contains(pos))
                return false;

            // Early total-piece check: avoid growing past the required number of pieces
            // visited.Count here is count of nodes in current path thus far
            if (visited.Count >= _totalCount)
                return false;

            var existing = _grid.Board[pos.Y, pos.X];

            // If this cell is a fixed piece, check alignment
            if (existing != PieceType.Empty)
            {
                if (!TrackConnections.ConnectsTo(existing, (-incoming.dr, -incoming.dc)))
                    return false;
                fixedHit++;
            }

            visited.Add(pos);

            // If we've connected all fixed pieces, check counts
            if (fixedHit == _targetFixedCount
                && _grid.TrackCountInAllRowsColsMatch())
            {
                return _grid.IsSingleConnectedPath();
            }

            // Precompute remaining fixed positions
            var remaining = _fixedPositions
                .Where(fp => !visited.Contains(fp))
                .ToList();

            // Determine candidates: either the fixed piece or any placeable track
            IEnumerable<PieceType> candidates = existing != PieceType.Empty
                ? new[] { existing }
                : Enum.GetValues(typeof(PieceType))
                      .Cast<PieceType>()
                      .Where(p => p != PieceType.Empty)
                      .Where(p => _grid.CanPlace(pos, p))
                      .Reverse(); // Reverse to priortize branching, over straight lines

            foreach (var piece in candidates)
            {
                bool placed = false;
                if (existing == PieceType.Empty)
                {
                    _grid.Place(pos, piece);
                    placed = true;
                }

                // Sort outgoing by Manhattan distance to nearest remaining fixed
                var directions = TrackConnections.GetConnections(piece)
                    .Where(d => !(d.dr == -incoming.dr && d.dc == -incoming.dc))
                    .OrderBy(d =>
                    {
                        var next = pos + (d.dr, d.dc);
                        return remaining.Count == 0
                            ? 0
                            : remaining.Min(fp => fp.ManhattanDistance(next));
                    });

                // Explore outgoing connections, avoid going backward
                foreach (var (dr, dc) in directions)
                {
                    if (dr == -incoming.dr && dc == -incoming.dc)
                        continue;

                    if (TryBuild(pos + (dr, dc), (dr, dc), visited, fixedHit))
                        return true;
                }

                if (placed)
                    _grid.Remove(pos);
            }

            visited.Remove(pos);
            return false;
        }
    }
}
