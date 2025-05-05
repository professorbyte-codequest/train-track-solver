namespace TrainTrackSolverLib;

public class Puzzle {
    public int GridWidth { get; set; }
    public int GridHeight { get; set; }

    public Data Data { get; set; }

    public void Print()
    {
        Console.WriteLine($"GridWidth: {GridWidth}");
        Console.WriteLine($"GridHeight: {GridHeight}");

        Console.WriteLine("Data:");
        Console.WriteLine($"  verticalClues: {string.Join(", ", Data.verticalClues)}");
        Console.WriteLine($"  horizontalClues: {string.Join(", ", Data.horizontalClues)}");
        Console.WriteLine($"  startingGrid: {string.Join(", ", Data.startingGrid)}");
    }

    /// Load a puzzle from a TXT file
    /// The file format is:
    /// ```
    /// ROWS: 0 0 0 2 5 5 6 3 4 4
    /// COLS: 1 1 1 4 6 4 4 2 3 3
    /// FIXED:
    /// 4,4: CornerSE
    /// 5,6: CornerNW
    /// 5,9: CornerSW
    /// 6,0: Horizontal
    /// 6,3: CornerSW
    /// 7,9: CornerNE
    /// 8,6: CornerSW
    /// 9,5: Horizontal
    /// 9,6: CornerNW
    /// ```
    public static Puzzle LoadFromFile(string path) {
        Puzzle puzzle = new Puzzle();
        puzzle.Data = new Data();

        bool isFixed = false;

        List<(Point, PieceType)> fixedPieces = new List<(Point, PieceType)>();

        // Grid: RowCounts == verticalClues == Height
        // Grid: ColCounts == horizontalClues == Width

        foreach (string line in File.ReadLines(path)) {
            if (line.StartsWith("#")) {
                continue; // Skip comments
            }
            if (isFixed) {
                string[] parts = line.Split(':');
                if (parts.Length == 2) {
                    string[] coordinates = parts[0].Split(',');
                    int r = int.Parse(coordinates[0]);
                    int c = int.Parse(coordinates[1]);
                    string pieceType = parts[1].Trim();

                    fixedPieces.Add((new Point(r, c), Enum.Parse<PieceType>(pieceType)));
                } else {
                    isFixed = false;
                }
            }
            if (!isFixed) {
                if (line.StartsWith("ROWS:")) {
                    puzzle.Data.verticalClues = line[5..].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                } else if (line.StartsWith("COLS:")) {
                    puzzle.Data.horizontalClues = line[5..].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                } else if (line.StartsWith("FIXED:")) {
                    // Handle fixed pieces here
                    isFixed = true;
                }
            }
        }

        if (puzzle.Data.verticalClues.Length == 0 || puzzle.Data.horizontalClues.Length == 0) {
            throw new InvalidDataException("Invalid puzzle format. Missing ROWS or COLS.");
        }

        // Initialize the starting grid
        puzzle.GridWidth = puzzle.Data.horizontalClues.Length;
        puzzle.GridHeight = puzzle.Data.verticalClues.Length;
        puzzle.Data.startingGrid = new PieceType[puzzle.GridWidth * puzzle.GridHeight];
        for (int i = 0; i < puzzle.Data.startingGrid.Length; i++) {
            puzzle.Data.startingGrid[i] = PieceType.Empty;
        }
        // Set the fixed pieces in the starting grid
        foreach (var (point, pieceType) in fixedPieces) {
            int index = point.Y * puzzle.GridWidth + point.X;
            puzzle.Data.startingGrid[index] = pieceType;
        }

        return puzzle;
    }
}