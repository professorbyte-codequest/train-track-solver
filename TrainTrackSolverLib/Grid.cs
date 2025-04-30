namespace TrainTrackSolverLib;

using System;
using System.Collections.Generic;
using System.Linq;

public class Grid
{
    public int Rows { get; }
    public int Cols { get; }
    public PieceType[,] Board { get; }
    public int[] RowCounts { get; }
    public int[] ColCounts { get; }

    private readonly int[] _placedInRow;
    private readonly int[] _placedInCol;

    private readonly int _totalCount;

    public int TotalCount => _totalCount;

    public int PlacedCount => _placedInRow.Sum();
    public int PlacedInRow(int r) { return _placedInRow[r]; }
    public int PlacedInCol(int c) { return _placedInCol[c]; }

    /// <summary>
    /// Create a new grid with the specified dimensions and row/column constraints.
    /// </summary>
    /// <param name="rows">Number of rows</param>
    /// <param name="cols">Number of columns</param>
    /// <param name="rowCounts">Constraints for each row</param>
    /// <param name="colCounts">Constraints for each column</param>
    public Grid(int rows, int cols, int[] rowCounts, int[] colCounts)
    {
        Rows = rows;
        Cols = cols;
        RowCounts = rowCounts;
        ColCounts = colCounts;
        Board = new PieceType[rows, cols];
        _placedInRow = new int[rows];
        _placedInCol = new int[cols];

        // Check if the grid is valid
        ValidateRowColCounts();

        // Calculate the total count of pieces
        _totalCount = 0;
        foreach (var count in rowCounts)
            _totalCount += count;
    }

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

    public void UpdateCounts() {
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
    }

    /// <summary>
    /// Check if a cell is within the bounds of the grid.
    /// </summary>
    /// <param name="r">Row index to check</param>
    /// <param name="c">Column index to check</param>
    /// <returns>True if the cell is inbounds, else false</returns>
    public bool IsInBounds(int r, int c) => r >= 0 && r < Rows && c >= 0 && c < Cols;

    /// <summary>
    /// Check if a cell is on the edge of the grid.
    /// </summary>
    /// <param name="r">Row index to check</param>
    /// <param name="c">Column index to check</param>
    /// <returns>True if the cell is on the edge, else false</returns>
    public bool IsOnEdge(int r, int c)
        => r == 0 || r == Rows - 1
            || c == 0 || c == Cols - 1;

    /// <summary>
    /// Check if a cell is filled with a piece.
    /// </summary>
    /// <param name="r">Row index to check</param>
    /// <param name="c">Column index to check</param>
    /// <returns>True if the cell is not empty</returns>
    public bool IsFilled(int r, int c) => Board[r, c] != PieceType.Empty;

    /// <summary>
    ///  Check if a cell empty.
    ///  This is used to check if a cell can be filled with a piece.
    /// </summary>
    /// <param name="r">Row index to check</param>
    /// <param name="c">Column index to check</param>
    /// <returns>True if the cell is empty</returns>
    public bool IsEmpty(int r, int c) =>
        Board[r, c] == PieceType.Empty;

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

        var dirs = new[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
        bool hasNeighbor = false;
        bool connectsToAny = false;

        // 2) Existing‐neighbor alignment
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

        // 3) look‐ahead: every direction this piece *will* connect must have capacity
        foreach (var (dr, dc) in TrackConnections.GetConnections(piece))
        {
            int nr = r + dr, nc = c + dc;
            if (!IsInBounds(nr, nc)) 
                return false;    // can’t connect off‐board
            if (Board[nr, nc] == PieceType.Empty)
            {
                // if that row/col is already at its count, we can't ever fill it
                if (TrackCountInRow(nr) >= RowCounts[nr] || TrackCountInCol(nc) >= ColCounts[nc])
                    return false;
            }
        }

        // 4) Edge‐entry rules
        if (r == 0 && (piece == PieceType.Vertical || piece == PieceType.CornerNW || piece == PieceType.CornerNE))
            return false;
        if (r == Rows-1 && (piece == PieceType.Vertical || piece == PieceType.CornerSW || piece == PieceType.CornerSE))
            return false;
        if (c == 0 && (piece == PieceType.Horizontal || piece == PieceType.CornerNW || piece == PieceType.CornerSW))
            return false;
        if (c == Cols-1 && (piece == PieceType.Horizontal || piece == PieceType.CornerNE || piece == PieceType.CornerSE))
            return false;

        return true;
    }

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

    /// <summary>
    /// Print the grid to the console.
    /// </summary>
    public void Print(bool showCounts = false)
    {
        if (showCounts) Console.WriteLine("  " + string.Join(' ', ColCounts));
        Console.WriteLine();
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
    /// Load a grid from a file.
    /// The file format is:
    /// ROWS: 1 2 3
    /// COLS: 1 2 3
    /// FIXED:
    /// 0,0: Horizontal
    /// 1,1: Vertical
    /// 2,2: CornerNE
    /// 3,3: CornerNW
    /// 4,4: CornerSE
    /// 5,5: CornerSW
    /// </summary>
    /// <param name="path">File path to load from</param>
    /// <returns>Deserialized grid</returns>
    /// <exception cref="InvalidDataException">If data format is incorrect</exception>
    public static Grid LoadFromFile(string path)
    {
        var lines = File.ReadAllLines(path)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("#"))
            .ToList();

        int[] rows = Array.Empty<int>();
        int[] cols = Array.Empty<int>();
        var fixedList = new List<(int r, int c, PieceType p)>();

        foreach (var line in lines)
        {
            if (line.StartsWith("ROWS:"))
                rows = line[5..].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            else if (line.StartsWith("COLS:"))
                cols = line[5..].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            else if (line.Equals("FIXED:", StringComparison.OrdinalIgnoreCase))
                continue;
            else if (line.Contains(':'))
            {
                var parts = line.Split(':', 2);
                var nums = parts[0].Split(',');
                int rr = int.Parse(nums[0]);
                int cc = int.Parse(nums[1]);
                var pt = Enum.Parse<PieceType>(parts[1].Trim());
                fixedList.Add((rr, cc, pt));
            }
        }
        if (rows.Length == 0 || cols.Length == 0)
            throw new InvalidDataException("Missing ROWS or COLS");

        var grid = new Grid(rows.Length, cols.Length, rows, cols);
        foreach (var (rr, cc, pt) in fixedList)
            grid.Place(rr, cc, pt);

        // Check if the grid is valid
        grid.ValidateRowColCounts();

        return grid;
    }

    public void SaveToFile(string path)
    {
        using var writer = new StreamWriter(path);
        writer.WriteLine($"ROWS: {string.Join(' ', RowCounts)}");
        writer.WriteLine($"COLS: {string.Join(' ', ColCounts)}");
        writer.WriteLine("FIXED:");
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (Board[r, c] != PieceType.Empty)
                    writer.WriteLine($"{r},{c}: {Board[r, c]}");
    }

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
        for (int i = 0; i < Rows; i++)
            if (TrackCountInRow(i) != RowCounts[i])
                return false;
        for (int j = 0; j < Cols; j++)
            if (TrackCountInCol(j) != ColCounts[j])
                return false;
        return true;
    }

    public Grid Clone()
    {
        var clone = new Grid(Rows, Cols, RowCounts, ColCounts);
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                clone.Board[r, c] = Board[r, c];
        Array.Copy(_placedInRow, clone._placedInRow, Rows);
        Array.Copy(_placedInCol, clone._placedInCol, Cols);
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
            throw new ArgumentException("RowCounts length must match number of rows");
        if (ColCounts.Length != Cols)
            throw new ArgumentException("ColCounts length must match number of columns");

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

    public List<(int, int, (int, int))> FindEntryPoints(List<(int r, int c)> fixedPositions)
    {
        // Find edge entry points
        var entries = new List<(int, int, (int, int))>();
        foreach (var (r, c) in fixedPositions)
        {
            if (!IsOnEdge(r, c)) continue;
            var conns = TrackConnections.GetConnections(Board[r, c]);
            var offDirs = conns.Where(d => !IsInBounds(r + d.dr, c + d.dc)).ToList();
            if (offDirs.Count == 1)
                entries.Add((r, c, (-offDirs[0].dr, -offDirs[0].dc)));
        }
        if (entries.Count != 2)
            throw new InvalidOperationException($"Expected 2 entry points, found {entries.Count}");
        return entries;
    }

    public IEnumerable<(int,int)> EnumerateEdges()
    {
        for (int c = 0; c < Cols; c++) yield return (0,c);
        for (int c = 0; c < Cols; c++) yield return (Rows-1,c);
        for (int r = 0; r < Rows; r++) yield return (r,0);
        for (int r = 0; r < Rows; r++) yield return (r,Cols-1);
    }
}