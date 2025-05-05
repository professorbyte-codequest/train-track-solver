using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TrainTrackSolverLib.Common;

namespace TrainTrackSolverLib
{
    /// <summary>
    /// Generates random Train Tracks puzzles by carving a solvable path and exposing fixed pieces.
    /// </summary>
    public static class PuzzleGenerator
    {
        private static readonly Random _rand = new Random();

        /// <summary>
        /// Generates a puzzle with the given dimensions and number of fixed clues.
        /// </summary>
        public static Puzzle Generate(int rows, int cols, int fixedCount)
        {
            do {
                // Step 1: Carve a random solution path
                var fullGrid = CarveSolutionPath(rows, cols);

                // Step 2: Collect all path cells
                var pathCells = new List<Point>();
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++)
                        if (fullGrid.Board[r,c] != PieceType.Empty)
                            pathCells.Add(new Point(r,c));

                if (pathCells.Count <= fixedCount * 2)
                    continue;

                
                Puzzle puzzle = new Puzzle();
                
                puzzle.GridHeight = rows;
                puzzle.GridWidth = cols;

                // Randomly select fixed cells
                var fixedCells = pathCells.Where(fp => fullGrid.Entry != fp &&
                                                      fullGrid.Exit != fp)
                    .OrderBy(_ => _rand.Next()).Take(fixedCount).ToList();
                
                return puzzle;
            } while (true);
        }

        /// <summary>
        /// Carves a simple random path between two edge entries.
        /// </summary>
        private static Grid CarveSolutionPath(int rows, int cols)
        {
            var grid = new Grid(rows, cols);

            // Choose two random distinct edge cells
            var edges = grid.EnumerateEdges().OrderBy(_ => _rand.Next()).Take(2).ToArray();
            var (r0,c0) = edges[0];
            var (r1,c1) = edges[1];

            var piece = GetEntryPiece(grid, (r0,c0));

            // Prepare visited map and carve
            var visited = new bool[rows,cols];
            // Carve a path between the two edge cells
            BuildPath(grid, visited, (r0, c0), piece, (r1,c1), null);

            // Finalize board based on carved Board
            grid.FinalizeGrid();
            return grid;
        }

        /// <summary>
        /// Recursive DFS to carve a simple path, placing correct pieces for each segment.
        /// </summary>
        private static bool BuildPath(Grid grid, bool[,] visited,
                                      (int r, int c) pos,
                                      PieceType piece,
                                      (int tr,int tc) target,
                                      (int r,int c)? previous)
        {
            if (pos == target)
            {
                if (!previous.HasValue)
                {
                    return false;
                }

                // Reached the target cell
                // We need to place a piece that connects to the incoming direction
                // to an off-grid cell to complete the path
                var outDirs = new List<(int dr,int dc)> {
                    (1,0),
                    (-1,0),
                    (0,1),
                    (0,-1)
                }.Where(d => !grid.IsInBounds(pos.r + d.dr, pos.c + d.dc))
                .OrderBy(_ => _rand.Next());

                foreach (var (dr,dc) in outDirs)
                {
                    (int dr,int dc) inDir = (previous.Value.r - pos.r,
                                             previous.Value.c - pos.c);
                    
                    // Pick a random piece for the outgoing direction
                    var next_piece = TrackConnections.GetPieceForDirs(inDir, (dr,dc));
                    grid.Board[pos.r, pos.c] = next_piece;
                    grid.Print();
                    return true;
                }
            }
            visited[pos.r, pos.c] = true;
            grid.Board[pos.r, pos.c] = piece;

            // For the current cell (pos), we need to place a random piece
            // that connects to it
            var outgoing = TrackConnections.GetConnections(grid.Board[pos.r, pos.c])
                .Where(d => grid.IsInBounds(pos.r + d.dr, pos.c + d.dc) &&
                            !visited[pos.r + d.dr, pos.c + d.dc])
                .ToList()
                .OrderBy(_ => _rand.Next());

            foreach (var (dr,dc) in outgoing)
            {
                int nr = pos.r + dr, nc = pos.c + dc;

                // Pick a random piece for (nr,nc) that connects to (pos)
                
                // Pick a random outgoing direction
                var dirs = new List<(int dr,int dc)> {
                    (1,0),
                    (-1,0),
                    (0,1),
                    (0,-1)
                }.OrderBy(_ => _rand.Next());

                foreach (var (ddr,ddc) in dirs)
                {
                    int nrr = nr + ddr, ncc = nc + ddc;
                    if (grid.IsInBounds(nrr,ncc) && visited[nrr,ncc])
                        continue;

                    // Now chose a piece which includes both directions
                    (int dr,int dc) inDir = (-dr,-dc);
                    var next_piece = TrackConnections.GetPieceForDirs(inDir, (ddr,ddc));
                    if (BuildPath(grid, visited, (nr, nc), next_piece, target, pos))
                        return true;
                }

            }

            // Backtrack
            visited[pos.r,pos.c] = false;
            grid.Board[pos.r,pos.c] = PieceType.Empty;
            return false;
        }

        private static PieceType GetEntryPiece(Grid grid, (int r, int c) start)
        {
            // First cell: chose a random start piece
            // It must enter the grid from off-grid, and connect to a cell on the grid
            // chose a random piece
            var dirs = new List<(int dr,int dc)> {
                (1,0),
                (-1,0),
                (0,1),
                (0,-1)
            }.OrderBy(_ => _rand.Next());

            // Pick a random direction which is off-grid
            var dir = dirs.First(d => !grid.IsInBounds(start.r + d.dr, start.c + d.dc));
            // Pick a random direction which is on-grid
            var dir2 = dirs.First(d => grid.IsInBounds(start.r + d.dr, start.c + d.dc));
            // Now chose a piece which includes both directions
            var inDir = (dir.dr, dir.dc);
            var outDir = (dir2.dr, dir2.dc);
            var piece = TrackConnections.GetPieceForDirs(inDir, outDir);
            return piece;
        }

    }
}
