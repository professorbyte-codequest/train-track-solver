// A new solver that starts from a fixed entry point (track piece on the edge with one off-grid connection)
// and builds one continuous path through fixed pieces while respecting row/column counts.

using System;
using System.Collections.Generic;
using System.Linq;

using TrainTrackSolverLib.Common;

namespace TrainTrackSolverLib
{
    public class PathBuilderSolver : ISolver
    {
        private readonly Grid _grid;
        private readonly (int r, int c, (int dr, int dc) incoming) _entry;
        private readonly List<(int r, int c)> _fixedPositions;
        private readonly int _targetFixedCount;
        private readonly int _totalCount;

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
            _fixedPositions = new List<(int, int)>();
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Cols; c++)
                {
                    var p = grid.Board[r, c];
                    if (p != PieceType.Empty)
                        _fixedPositions.Add((r, c));
                }
            }

            // Identify exactly two edge entries: fixed pieces with one off-grid connection
            var entries = new List<(int, int, (int, int))>();
            foreach (var (r, c) in _fixedPositions)
            {
                if (!IsOnEdge(r, c)) continue;
                var conns = TrackConnections.GetConnections(_grid.Board[r, c]);
                var offDirs = conns.Where(d => !IsInBounds(r + d.dr, c + d.dc)).ToList();
                if (offDirs.Count == 1)
                {
                    // Incoming direction is from off-grid into this cell
                    var incoming = (-offDirs[0].dr, -offDirs[0].dc);
                    entries.Add((r, c, incoming));
                }
            }

            if (entries.Count != 2)
                throw new InvalidOperationException(
                    $"Expected exactly 2 entry points, found {entries.Count}.");

            // Pick either entry - they are interchangeable
            _entry = entries[0];
            _targetFixedCount = _fixedPositions.Count;

            _totalCount = grid.RowCounts.Sum();
        }

        /// <summary>
        /// Attempts to solve by building a single path from the fixed entry.
        /// </summary>
        public bool Solve()
            => TryBuild(_entry.r, _entry.c, _entry.incoming, new HashSet<(int, int)>(), 0);

        private bool TryBuild(
            int r, int c,
            (int dr, int dc) incoming,
            HashSet<(int, int)> visited,
            int fixedHit)
        {
            // Report progress
            _attemptCount++;
            if (_attemptCount % _progressReporter.ProgressInterval == 0)
                _progressReporter.Report(_attemptCount);

            // Bounds & revisit check
            if (!_grid.IsInBounds(r, c) || visited.Contains((r, c)))
                return false;

            // Early total-piece check: avoid growing past the required number of pieces
            // visited.Count here is count of nodes in current path thus far
            if (visited.Count >= _totalCount)
                return false;

            var existing = _grid.Board[r, c];

            // If this cell is a fixed piece, check alignment
            if (existing != PieceType.Empty)
            {
                if (!TrackConnections.ConnectsTo(existing, (-incoming.dr, -incoming.dc)))
                    return false;
                fixedHit++;
            }

            visited.Add((r, c));

            // If we've connected all fixed pieces and are back at an edge, check counts
            if (fixedHit == _targetFixedCount && IsOnEdge(r, c)
                && _grid.TrackCountInAllRowsColsMatch())
            {
                return true;
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
                      .Where(p => _grid.CanPlace(r, c, p));

            foreach (var piece in candidates)
            {
                bool placed = false;
                if (existing == PieceType.Empty)
                {
                    _grid.Place(r, c, piece);
                    placed = true;
                }

                // Sort outgoing by Manhattan distance to nearest remaining fixed
                var directions = TrackConnections.GetConnections(piece)
                    .Where(d => !(d.dr == -incoming.dr && d.dc == -incoming.dc))
                    .OrderBy(d =>
                    {
                        var nr = r + d.dr;
                        var nc = c + d.dc;
                        return remaining.Count == 0
                            ? 0
                            : remaining.Min(fp => Math.Abs(fp.r - nr) + Math.Abs(fp.c - nc));
                    });

                // Explore outgoing connections, avoid going backward
                foreach (var (dr, dc) in directions)
                {
                    if (dr == -incoming.dr && dc == -incoming.dc)
                        continue;

                    if (TryBuild(r + dr, c + dc, (dr, dc), visited, fixedHit))
                        return true;
                }

                if (placed)
                    _grid.Remove(r, c);
            }

            visited.Remove((r, c));
            return false;
        }

        private bool IsOnEdge(int r, int c)
            => r == 0 || r == _grid.Rows - 1
               || c == 0 || c == _grid.Cols - 1;

        private bool IsInBounds(int r, int c)
            => r >= 0 && r < _grid.Rows && c >= 0 && c < _grid.Cols;
    }
}
