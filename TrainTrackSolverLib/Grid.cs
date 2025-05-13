namespace TrainTrackSolverLib;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

public class Grid
{
    public int Rows { get; private set;}
    public int Cols { get; private set; }

    private Grid() {}
    
    public PieceType[,] Board { get; private set; }
    public PieceType this[int r, int c]
    {
        get => Board[r, c];
        set => Board[r, c] = value;
    }
    public PieceType this[Point p]
    {
        get => Board[p.X, p.Y];
        set => Board[p.X, p.Y] = value;
    }

    public Point Exit { get; private set; }
    public Point Entry { get; private set; }


    public int[] RowCounts { get; private set; }
    public int[] ColCounts { get;  private set;}

    private int[] _placedInRow;
    private int[] _placedInCol;

    private int _totalCount;

    public int TotalCount => _totalCount;

    public int PlacedCount => _placedInRow.Sum();
    public int PlacedInRow(int r) { return _placedInRow[r]; }
    public int PlacedInCol(int c) { return _placedInCol[c]; }

    /// <summary>
    /// Create a new grid with the specified dimensions.
    /// This constructor is used for generating a new grid without any constraints. 
    /// </summary>
    /// <param name="rows">Number of rows</param>
    /// <param name="cols">Number of columns</param>
    public Grid(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        RowCounts = new int[rows];
        ColCounts = new int[cols];
        Board = new PieceType[rows, cols];
        _placedInRow = new int[rows];
        _placedInCol = new int[cols];
    }

    public Grid(Puzzle puzzle)
    {
        Cols = puzzle.GridWidth;
        Rows = puzzle.GridHeight;
        Board = new PieceType[Rows, Cols];

        // Grid: RowCounts == verticalClues == Height
        // Grid: ColCounts == horizontalClues == Width

        ColCounts = puzzle.Data.verticalClues;
        RowCounts = puzzle.Data.horizontalClues;

        _placedInRow = new int[Rows];
        _placedInCol = new int[Cols];

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                var piece = puzzle.Data.startingGrid[r * Cols + c];
                if (piece == PieceType.Empty)
                    Board[r, c] = PieceType.Empty;
                else
                    Place(r, c, piece);
            }
        }

        // Find Entry and Exit
        ExtractEntryAndExit();

        // Check if the grid is valid
        ValidateRowColCounts();

        if (Entry == null || Exit == null)
            throw new InvalidOperationException("Entry and Exit points not found");

        // Calculate the total count of pieces
        _totalCount = 0;
        foreach (var count in RowCounts)
            _totalCount += count;
    }

    private void ExtractEntryAndExit()
    {
        var edgePoints = EnumerateEdges()
                    .Where(p => !IsEmpty(p.Item1, p.Item2))
                    .ToList();
        foreach (var point in edgePoints)
        {
            var conns = TrackConnections.GetConnections(Board[point.Item1, point.Item2]);
            var offDirs = conns.Where(d => !IsInBounds(point.Item1 + d.dr, point.Item2 + d.dc)).ToList();
            if (offDirs.Count == 1)
            {
                if (Entry == null) Entry = new Point(point.Item1, point.Item2);
                else Exit = new Point(point.Item1, point.Item2);
            }
        }
    }

    public List<Point> FixedPoints() {
        var fixedPoints = new List<Point>();
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (Board[r, c] != PieceType.Empty)
                    fixedPoints.Add(new Point(r, c));
        return fixedPoints;
    }

    public void FinalizeGrid() {
        for (int r = 0; r < Rows; r++)
            RowCounts[r] = 0;
        for (int c = 0; c < Cols; c++)
            ColCounts[c] = 0;

        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (Board[r, c] != PieceType.Empty)
                {
                    RowCounts[r]++;
                    ColCounts[c]++;
                }

        ExtractEntryAndExit();
    }

    /// <summary>
    /// Check if a cell is within the bounds of the grid.
    /// </summary>
    /// <param name="r">Row index to check</param>
    /// <param name="c">Column index to check</param>
    /// <returns>True if the cell is inbounds, else false</returns>
    public bool IsInBounds(int r, int c) => r >= 0 && r < Rows && c >= 0 && c < Cols;
    public bool IsInBounds(Point pos) => pos.Y >= 0 && pos.Y < Rows && pos.X >= 0 && pos.X < Cols;

    /// <summary>
    /// Check if a cell is on the edge of the grid.
    /// </summary>
    /// <param name="r">Row index to check</param>
    /// <param name="c">Column index to check</param>
    /// <returns>True if the cell is on the edge, else false</returns>
    public bool IsOnEdge(int r, int c)
        => r == 0 || r == Rows - 1
            || c == 0 || c == Cols - 1;

    public bool IsOnEdge(Point p) => IsOnEdge(p.Y, p.X);

    /// <summary>
    /// Check if a cell is filled with a piece.
    /// </summary>
    /// <param name="r">Row index to check</param>
    /// <param name="c">Column index to check</param>
    /// <returns>True if the cell is not empty</returns>
    public bool IsFilled(int r, int c) => Board[r, c] != PieceType.Empty;
    public bool IsFilled(Point p) => IsFilled(p.Y, p.X);

    /// <summary>
    ///  Check if a cell empty.
    ///  This is used to check if a cell can be filled with a piece.
    /// </summary>
    /// <param name="r">Row index to check</param>
    /// <param name="c">Column index to check</param>
    /// <returns>True if the cell is empty</returns>
    public bool IsEmpty(int r, int c) =>
        Board[r, c] == PieceType.Empty;
    public bool IsEmpty(Point p) => IsEmpty(p.Y, p.X);

    /// <summary>
    /// Get the number of pieces placed in a given row.
    /// </summary>
    /// <param name="row">Row index to check</param>
    /// <returns>The number of pieces placed in a row</returns>
    public int TrackCountInRow(int row) =>
        _placedInRow[row];

    /// <summary>
    /// Get the number of pieces placed in a given column.
    /// </summary>
    /// <param name="col">Column index to check</param>
    /// <returns>The number of pieces placed in a column</returns>
    public int TrackCountInCol(int col) =>
        _placedInCol[col];

    /// <summary>
    /// Get the number of empty cells in a given row.
    /// </summary>
    /// <param name="row">Row index to check</param>
    /// <returns>The number of empty cells in a row</returns>
    public int EmptyCountInRow(int row) =>
        Cols - _placedInRow[row];

    /// <summary>
    /// Get the number of empty cells in a given column.
    /// </summary>
    /// <param name="col">Column index to check</param>
    /// <returns>The number of empty  cells in a column</returns>
    public int EmptyCountInCol(int col) =>
        Rows - _placedInCol[col];

    /// <summary>
    /// Check if a piece can be placed at a given cell.
    /// This checks the following rules:
    /// 1) The cell is in bounds and not filled
    /// 2) The row and column counts are not exceeded
    /// 3) The piece connects to its neighbors
    /// 4) The piece does not violate edge-entry rules
    /// </summary>
    /// <param name="r">Row index to check</param>
    /// <param name="c">Column index to check</param>
    /// <param name="piece">Piece type to check</param>
    /// <returns>True if the piece can be placed, else false</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the row or column index is out of bounds</exception>
    /// <exception cref="ArgumentException">If the piece type is invalid</exception>
    /// <exception cref="InvalidOperationException">If the piece type is empty</exception>
    public bool CanPlace(int r, int c, PieceType piece)
    {
        // Part 1 rules
        if (!IsInBounds(r, c) || IsFilled(r, c)) return false;
        if (TrackCountInRow(r) >= RowCounts[r]) return false;
        if (TrackCountInCol(c) >= ColCounts[c]) return false;


        // 2) Edge‐entry rules
        if (r == 0 && (piece == PieceType.Vertical || piece == PieceType.CornerNW || piece == PieceType.CornerNE))
            return false;
        if (r == Rows-1 && (piece == PieceType.Vertical || piece == PieceType.CornerSW || piece == PieceType.CornerSE))
            return false;
        if (c == 0 && (piece == PieceType.Horizontal || piece == PieceType.CornerNW || piece == PieceType.CornerSW))
            return false;
        if (c == Cols-1 && (piece == PieceType.Horizontal || piece == PieceType.CornerNE || piece == PieceType.CornerSE))
            return false;

        var dirs = new[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
        bool hasNeighbor = false;
        bool connectsToAny = false;

        // 3) Existing‐neighbor alignment
        foreach (var (dr, dc) in dirs)
        {
            int nr = r + dr, nc = c + dc;
            if (!IsInBounds(nr, nc)) continue;
            var neighbor = Board[nr, nc];
            if (neighbor == PieceType.Empty) continue;

            hasNeighbor = true;
            bool thisConn  = TrackConnections.ConnectsTo(piece,  (dr, dc));
            bool neighConn = TrackConnections.ConnectsTo(neighbor, (-dr, -dc));
            if (thisConn != neighConn)      return false;
            if (thisConn) connectsToAny = true;
        }
        if (hasNeighbor && !connectsToAny) return false;

        // Assume we placed it
        int placedInRow = _placedInRow[r] + 1;
        int placedInCol = _placedInCol[c] + 1;
        // 4) look‐ahead: every direction this piece *will* connect must have capacity
        foreach (var (dr, dc) in TrackConnections.GetConnections(piece))
        {
            int nr = r + dr, nc = c + dc;
            if (!IsInBounds(nr, nc)) 
                return false;    // can’t connect off‐board
            if (Board[nr, nc] == PieceType.Empty)
            {
                int checkRow = dr == 0 ? placedInRow : _placedInRow[nr];
                int checkCol = dc == 0 ? placedInCol : _placedInCol[nc];
                // if that row/col is already at its count, we can't ever fill it
                if (checkRow >= RowCounts[nr] || checkCol >= ColCounts[nc])
                    return false;
            }
        }

        return true;
    }

    public bool CanPlace(Point p, PieceType piece) => CanPlace(p.Y, p.X, piece);

    /// <summary>
    /// Place a piece on the grid.
    /// </summary>
    /// <param name="r">Row to place piece at</param>
    /// <param name="c">Column to place piece at</param>
    /// <param name="piece">Piece to place</param>
    /// <exception cref="ArgumentException">Empty pieces cannot be placed</exception>
    public void Place(int r, int c, PieceType piece) {
        if (PieceType.Empty == piece)
            throw new ArgumentException("Cannot place empty piece");
        Board[r, c] = piece;
        _placedInCol[c]++;
        _placedInRow[r]++;
    }

    public void Place(Point p, PieceType piece) => Place(p.Y, p.X, piece);

    /// <summary>
    /// Remove a piece from the grid.
    /// </summary>
    /// <param name="r">Row to remove piece from</param>
    /// <param name="c">Column to remove piece from</param>
    public void Remove(int r, int c) {
        if (!IsEmpty(r, c)) {
            _placedInCol[c]--;
            _placedInRow[r]--;
        }
        Board[r, c] = PieceType.Empty;
    } 

    public void Remove(Point p) => Remove(p.Y, p.X);

    /// <summary>
    /// Print the grid to the console.
    /// </summary>
    public void Print(bool showCounts = false)
    {
        if (showCounts) Console.WriteLine("  " + string.Join(' ', ColCounts));
        else Console.WriteLine();
        for (int r = 0; r < Rows; r++)
        {
            if (showCounts) Console.Write(RowCounts[r] + " ");
            for (int c = 0; c < Cols; c++) {
                Console.Write(Symbol(Board[r,c]));
                if (showCounts) Console.Write(" ");
            }
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Convert a piece type to a symbol for display.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    private char Symbol(PieceType p) => p switch
    {
        PieceType.Empty => ' ',
        PieceType.Horizontal => '─',  // nicer horizontal line
        PieceType.Vertical   => '│',  // nicer vertical line
        PieceType.CornerNE   => '└',  // connects north & east
        PieceType.CornerNW   => '┘',  // connects north & west
        PieceType.CornerSE   => '┌',  // connects south & east
        PieceType.CornerSW   => '┐',  // connects south & west
        _ => '?',
    };

    /// <summary>
    /// Get all legal pieces that can be placed at a given cell.
    /// This includes all pieces that are not empty, and
    /// that can be placed without violating the rules.
    /// </summary>
    /// <param name="r">Row to check</param>
    /// <param name="c">Column to check</param>
    /// <returns>List of legal piece types</returns>
    public IEnumerable<PieceType> GetLegalPieces(int r, int c)
    {
        return Enum.GetValues(typeof(PieceType))
                    .Cast<PieceType>()
                    .Where(p => p != PieceType.Empty && CanPlace(r, c, p));
    }
    public IEnumerable<PieceType> GetLegalPieces(Point p) => GetLegalPieces(p.Y, p.X);

    /// <summary>
    /// Find the first non-empty cell in the grid.
    /// This is used to start the DFS for checking if the grid is a single connected path.
    /// </summary>
    /// <returns>Cell index to start the path search</returns>
    private (int r, int c)? FindFirst()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (!IsEmpty(r, c))
                    return (r, c);
        return null;
    }

    /// <summary>
    /// Check if the grid is a single connected path.
    /// This is done using a DFS to ensure all, non-empty cells are reachable from the first found cell.
    /// </summary>
    /// <returns>True if it is a single connected path, else false</returns>
    public bool IsSingleConnectedPath()
    {
        // Find the first, non-empty cell
        var start = FindFirst();
        if (start == null) return false;

        var visited = new bool[Rows, Cols];
        var dfsStack = new Stack<(int r, int c)>();
        // Push the starting cell onto the stack
        dfsStack.Push(start.Value);
        visited[start.Value.r, start.Value.c] = true;

        var dirs = new[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
        while (dfsStack.Count > 0)
        {
            var (r, c) = dfsStack.Pop();
            foreach (var (dr, dc) in dirs)
            {
                int nr = r + dr, nc = c + dc;
                // Check if the neighbor is in bounds and not visited,
                // the neighbor connects to the current piece, and the
                // current piece connects to the neighbor
                if (IsInBounds(nr, nc) && !visited[nr, nc]
                    && TrackConnections.ConnectsTo(Board[r, c], (dr, dc))
                    && TrackConnections.ConnectsTo(Board[nr, nc], (-dr, -dc)))
                {
                    visited[nr, nc] = true;
                    dfsStack.Push((nr, nc));
                }
            }
        }

        // Check if all, non-empty cells were visited
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (!IsEmpty(r, c) && !visited[r, c])
                    return false;
        return true;
    }

    /// <summary>
    /// Prune if any row or column cannot meet its required track count
    /// </summary>
    public bool CanStillSatisfy()
    {
        // Rows
        for (int r = 0; r < Rows; r++)
        {
            int placed = TrackCountInRow(r);
            int empty = EmptyCountInRow(r);
            if (placed > RowCounts[r] || placed + empty < RowCounts[r])
                return false;
        }
        // Columns
        for (int c = 0; c < Cols; c++)
        {
            int placed = TrackCountInCol(c);
            int empty = EmptyCountInCol(c);
            if (placed > ColCounts[c] || placed + empty < ColCounts[c])
                return false;
        }
        return true;
    }

    public bool TrackCountInAllRowsColsMatch()
    {
        for (int i = 0; i < Rows; i++) {
            if (TrackCountInRow(i) != RowCounts[i]) {
                return false;
            }
        }
        for (int j = 0; j < Cols; j++) {
            if (TrackCountInCol(j) != ColCounts[j]) {
                return false;
            }
        }
        return true;
    }

    public Grid Clone()
    {
        var clone = new Grid {
            Rows = Rows,
            Cols = Cols,
            RowCounts = (int[])RowCounts.Clone(),
            ColCounts = (int[])ColCounts.Clone(),
            Board = new PieceType[Rows, Cols],
            _placedInRow = new int[Rows],
            _placedInCol = new int[Cols]
        };
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                clone.Board[r, c] = Board[r, c];
        Array.Copy(_placedInRow, clone._placedInRow, Rows);
        Array.Copy(_placedInCol, clone._placedInCol, Cols);
        clone.Entry = Entry;
        clone.Exit = Exit;
        clone._totalCount = _totalCount;
        return clone;
    }

    public void CopyTo(Grid other)
    {
        if (Rows != other.Rows || Cols != other.Cols)
            throw new ArgumentException("Grids must be the same size");
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                other.Board[r, c] = Board[r, c];
        Array.Copy(_placedInRow, other._placedInRow, Rows);
        Array.Copy(_placedInCol, other._placedInCol, Cols);
    }

    private void ValidateRowColCounts()
    {
        if (RowCounts.Length != Rows)
            throw new ArgumentException($"RowCounts length {RowCounts.Length } must match number of rows {Rows}");
        if (ColCounts.Length != Cols)
            throw new ArgumentException($"ColCounts length {ColCounts.Length} must match number of columns {Cols}");

        long totalCount = 0;
        for (int i = 0; i < Rows; i++)
            if (RowCounts[i] < 0)
                throw new ArgumentOutOfRangeException($"RowCounts[{i}] must be non-negative");
            else
                totalCount += RowCounts[i];

        for (int j = 0; j < Cols; j++)
            if (ColCounts[j] < 0)
                throw new ArgumentOutOfRangeException($"ColCounts[{j}] must be non-negative");
            else
                totalCount -= ColCounts[j];

        if (totalCount != 0)
            throw new InvalidOperationException("Row and column counts do not match");
    }

    public IEnumerable<(int,int)> EnumerateEdges()
    {
        for (int c = 0; c < Cols; c++) yield return (0,c);
        for (int c = 0; c < Cols; c++) yield return (Rows-1,c);
        for (int r = 0; r < Rows; r++) yield return (r,0);
        for (int r = 0; r < Rows; r++) yield return (r,Cols-1);
    }
}