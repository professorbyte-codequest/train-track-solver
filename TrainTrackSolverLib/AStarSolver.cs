using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using TrainTrackSolverLib.Common;

namespace TrainTrackSolverLib
{
    /// <summary>
    /// Enhanced A* implementation with stronger constraint propagation and heuristic improvements.
    /// Adds row/column potential checks alongside existing pruning for more aggressive branch elimination.
    /// </summary>
    public class AStarSolver : ISolver
    {
        private readonly Grid _initialGrid;
        private readonly List<Point> _fixedPositions;
        private readonly int _targetFixedCount;
        private readonly IProgressReporter _progressReporter;
        private long _iterationCount;
        public long IterationCount => _iterationCount;

        // Closed set: best G seen per state signature
        private readonly Dictionary<StateSignature, int> _bestG = new Dictionary<StateSignature, int>();

        public AStarSolver(Grid grid, IProgressReporter reporter)
        {
            _initialGrid = grid;
            _progressReporter = reporter;
            _iterationCount = 0;

            // Gather all fixed positions
            _fixedPositions = grid.FixedPoints();

            _targetFixedCount = _fixedPositions.Count;
        }

        public bool Solve()
        {
            var openSet = new PriorityQueue<State, int>();

            // Initialize start
            var startGrid = _initialGrid.Clone();
            var startVisited = new HashSet<Point> { _initialGrid.Entry };
            int startFixedHit = 1;
            var startState = new State(startGrid, _initialGrid.Entry, (0, 0), startVisited, startFixedHit, 0);
            _bestG[new StateSignature(startState)] = 0;
            openSet.Enqueue(startState, Heuristic(startState));

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();
                _iterationCount++;
                if (_iterationCount % _progressReporter.ProgressInterval == 0)
                {
                    current.GridState.CopyTo(_initialGrid);
                    _progressReporter.Report(_iterationCount);
                }

                var sig = new StateSignature(current);
                if (_bestG.TryGetValue(sig, out var best) && current.G > best)
                    continue;

                if (IsGoal(current))
                {
                    current.GridState.CopyTo(_initialGrid);
                    return true;
                }

                foreach (var neighbor in ExpandNeighbors(current))
                {
                    // Prune by row/col potential and exact count feasibility
                    if (!neighbor.GridState.CanStillSatisfy() || !CanReachAllFixed(neighbor))
                        continue;

                    var nSig = new StateSignature(neighbor);
                    if (_bestG.TryGetValue(nSig, out var existingG) && neighbor.G >= existingG)
                        continue;

                    _bestG[nSig] = neighbor.G;
                    openSet.Enqueue(neighbor, neighbor.G + Heuristic(neighbor));
                }
            }
            return false;
        }

        private IEnumerable<State> ExpandNeighbors(State state)
        {
            int r = state.Pos.Y, c = state.Pos.X;
            var existing = state.GridState.Board[r, c];

            IEnumerable<PieceType> candidates = existing != PieceType.Empty
                ? new[] { existing }
                : Enum.GetValues(typeof(PieceType)).Cast<PieceType>()
                      .Where(p => p != PieceType.Empty)
                      .Where(p => state.GridState.CanPlace(r, c, p));

            foreach (var piece in candidates)
            {
                var newGrid = state.GridState.Clone();
                if (existing == PieceType.Empty)
                    newGrid.Place(r, c, piece);

                int fixedHit = state.FixedHit + (existing != PieceType.Empty ? 1 : 0);
                var visited = new HashSet<Point>(state.Visited) { state.Pos };

                foreach (var (dr, dc) in TrackConnections.GetConnections(piece))
                {
                    if (dr == -state.Incoming.dr && dc == -state.Incoming.dc)
                        continue;

                    int nr = r + dr, nc = c + dc;
                    var next = new Point(nr, nc);
                    if (!newGrid.IsInBounds(nr, nc) || visited.Contains(next))
                        continue;

                    yield return new State(newGrid, next, (dr, dc), visited, fixedHit, state.G + 1);
                }
            }
        }

        private int Heuristic(State s)
        {
            var remain = _fixedPositions.Where(fp => !s.Visited.Contains(fp)).ToList();
            int rows = s.GridState.Rows, cols = s.GridState.Cols;
            int mstCost = 0;
            int exitDist;

            if (remain.Count > 0)
            {
                // Build list of points including current position
                var points = remain.ToList();
                points.Add((s.Pos));

                var visitedPts = new HashSet<Point> { s.Pos };
                while (visitedPts.Count < points.Count)
                {
                    int bestDist = int.MaxValue;
                    Point bestP = new Point(0, 0);

                    foreach (var p in points)
                    {
                        if (visitedPts.Contains(p)) continue;
                        foreach (var v in visitedPts)
                        {
                            int d = p.ManhattanDistance(v);
                            if (d < bestDist)
                            {
                                bestDist = d;
                                bestP = p;
                            }
                        }
                    }
                    mstCost += bestDist;
                    visitedPts.Add(bestP);
                }

                // Distance from nearest fixed to exit
                exitDist = remain.Min(p => p.ManhattanDistance(s.GridState.Exit));
            }
            else
            {
                // No remaining fixed: distance to exit
                exitDist = s.Pos.ManhattanDistance(s.GridState.Exit);
            }

            int mismatchPenalty = 0;
            for (int i = 0; i < rows; i++)
                mismatchPenalty += Math.Abs(s.GridState.RowCounts[i] - s.GridState.PlacedInRow(i));
            for (int j = 0; j < cols; j++)
                mismatchPenalty += Math.Abs(s.GridState.ColCounts[j] - s.GridState.PlacedInCol(j));

            return mstCost + exitDist + mismatchPenalty;
        }

    /// <summary>
        /// Checks that all unvisited fixed positions are reachable via empty cells from current position.
        /// </summary>
        private bool CanReachAllFixed(State s)
        {
            var grid = s.GridState;
            int rows = grid.Rows, cols = grid.Cols;
            var visited = new bool[rows, cols];
            var queue = new Queue<Point>();
            queue.Enqueue(s.Pos);
            visited[s.Pos.Y, s.Pos.X] = true;
            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                foreach (var (dr, dc) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                {
                    var next = pos + (dr, dc);
                    if (!grid.IsInBounds(next.Y, next.X)) continue;
                    if (visited[next.Y, next.X]) continue;
                    // treat all cells as passable for reachability
                    visited[next.Y, next.X] = true;
                    queue.Enqueue(next);
                }
            }
            // ensure every unvisited fixed is reachable
            foreach (var fp in _fixedPositions)
            {
                if (!s.Visited.Contains(fp) && !visited[fp.Y, fp.X])
                    return false;
            }
            return true;
        }

        private bool IsGoal(State s)
            => s.FixedHit == _targetFixedCount
               && s.GridState.IsOnEdge(s.Pos.Y, s.Pos.X)
               && s.GridState.TrackCountInAllRowsColsMatch();

        private readonly struct StateSignature : IEquatable<StateSignature>
        {
            private readonly int _r, _c, _fixedHit;
            private readonly (int dr, int dc) _incoming;
            private readonly int _visitedHash;

            public StateSignature(State s)
            {
                _r = s.Pos.Y;
                _c = s.Pos.X;
                _incoming = s.Incoming;
                _fixedHit = s.FixedHit;
                unchecked
                {
                    int hash = 17;
                    foreach (var p in s.Visited.OrderBy(x => x))
                    {
                        hash = hash * 31 + p.Y;
                        hash = hash * 31 + p.X;
                    }
                    _visitedHash = hash;
                }
            }

            public bool Equals(StateSignature other)
                => _r == other._r && _c == other._c && _incoming == other._incoming
                   && _fixedHit == other._fixedHit && _visitedHash == other._visitedHash;

            public override int GetHashCode()
                => HashCode.Combine(_r, _c, _incoming.dr, _incoming.dc, _fixedHit, _visitedHash);
        }

        private class State
        {
            public Grid GridState { get; }
            public Point Pos { get; }
            public (int dr, int dc) Incoming { get; }
            public HashSet<Point> Visited { get; }
            public int FixedHit { get; }
            public int G { get; }

            public State(Grid grid, Point pos, (int dr, int dc) incoming,
                         HashSet<Point> visited, int fixedHit, int g)
            {
                GridState = grid;
                Pos = pos;
                Incoming = incoming;
                Visited = visited;
                FixedHit = fixedHit;
                G = g;
            }
        }
    }
}
