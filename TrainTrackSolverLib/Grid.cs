namespace TrainTrackSolverLib;

using System;
using System.Collections.Generic;
using System.Linq;

public class Grid
{
    public int Rows, Cols;
    public PieceType[,] Board;
    public int[] RowCounts, ColCounts;

    public Grid(int rows, int cols, int[] rowCounts, int[] colCounts)
    {
        Rows = rows;
        Cols = cols;
        RowCounts = rowCounts;
        ColCounts = colCounts;
        Board = new PieceType[rows, cols];

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                Board[r, c] = PieceType.Empty;
    }

    public bool IsInBounds(int r, int c) => r >= 0 && r < Rows && c >= 0 && c < Cols;
    public bool IsFilled(int r, int c) => Board[r, c] != PieceType.Empty;

    public int TrackCountInRow(int row) => Enumerable.Range(0, Cols).Count(c => Board[row, c] != PieceType.Empty);
    public int TrackCountInCol(int col) => Enumerable.Range(0, Rows).Count(r => Board[r, col] != PieceType.Empty);

    public bool CanPlace(int r, int c, PieceType piece)
    {
        if (!IsInBounds(r, c) || IsFilled(r, c)) return false;
        if (TrackCountInRow(r) >= RowCounts[r]) return false;
        if (TrackCountInCol(c) >= ColCounts[c]) return false;
        return true;
    }

    public void Place(int r, int c, PieceType piece) => Board[r, c] = piece;
    public void Remove(int r, int c) => Board[r, c] = PieceType.Empty;

    public void Print()
    {
        Console.WriteLine("  " + string.Join(" ", ColCounts));
        for (int r = 0; r < Rows; r++)
        {
            Console.Write(RowCounts[r] + " ");
            for (int c = 0; c < Cols; c++)
            {
                Console.Write(PieceChar(Board[r, c]) + " ");
            }
            Console.WriteLine();
        }
    }

    private char PieceChar(PieceType piece) => piece switch
    {
        PieceType.Horizontal => '-',
        PieceType.Vertical => '|',
        PieceType.CornerNE => '/',
        PieceType.CornerNW => '\\',
        PieceType.CornerSE => '\\',
        PieceType.CornerSW => '/',
        _ => ' '
    };

    public static Grid LoadFromFile(string path)
    {
        var lines = File.ReadAllLines(path)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith("#"))
            .ToList();

        int[] rowCounts = Array.Empty<int>();
        int[] colCounts = Array.Empty<int>();
        List<(int r, int c, PieceType type)> fixedPieces = new();

        foreach (var line in lines)
        {
            if (line.StartsWith("ROWS:"))
            {
                rowCounts = line[5..].Trim().Split().Select(int.Parse).ToArray();
            }
            else if (line.StartsWith("COLS:"))
            {
                colCounts = line[5..].Trim().Split().Select(int.Parse).ToArray();
            }
            else if (line.Equals("FIXED:", StringComparison.OrdinalIgnoreCase))
            {
                // Marker for fixed section, skip line
                continue;
            }
            else if (line.Contains(':'))
            {
                var parts = line.Split(':');
                if (parts.Length != 2)
                    throw new InvalidDataException($"Invalid line spec: {line}");
                var coords = parts[0].Split(',');
                if (coords.Length != 2)
                    throw new InvalidDataException($"Invalid coordinates: {parts[0]}");
                int r = int.Parse(coords[0]);
                int c = int.Parse(coords[1]);
                PieceType type = Enum.Parse<PieceType>(parts[1].Trim());
                fixedPieces.Add((r, c, type));
            }
        }

        if (rowCounts.Length == 0 || colCounts.Length == 0)
            throw new InvalidDataException("Missing ROWS or COLS data.");

        var grid = new Grid(rowCounts.Length, colCounts.Length, rowCounts, colCounts);
        foreach (var (r, c, type) in fixedPieces)
        {
            grid.Place(r, c, type);
        }

        return grid;
    }
}
