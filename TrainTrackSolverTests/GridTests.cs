namespace TrainTrackSolverTests;

// TrainTrackSolverTests - Unit tests for the TrainTrackSolverLib
using TrainTrackSolverLib;
using Xunit;

public class GridTests
{
    [Fact]
    public void CanPlace_RespectsBoundsAndFill()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile,
            """
            # Puzzle 3x3
            ROWS: 1 1 1
            COLS: 3 0 0
            FIXED:
            0,0: Vertical
            0,2: Vertical
            """);
        var puzzle = Puzzle.LoadFromFile(tempFile);
        var grid = new Grid(puzzle);
        var pos = new Point(1, 0);
        Assert.True(grid.CanPlace(pos, PieceType.Vertical));
        grid.Place(pos, PieceType.Vertical);
        Assert.False(grid.CanPlace(pos, PieceType.Vertical)); // already filled
    }

    [Fact]
    public void CanPlace_EnforcesRowAndColLimits()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile,
            """
            # Puzzle 3x3
            # |
            # ++
            #  |
            ROWS: 1 2 1
            COLS: 2 2 0
            FIXED:
            0,0: Vertical
            2,2: Vertical
            """);
        var puzzle = Puzzle.LoadFromFile(tempFile);
        var grid = new Grid(puzzle);

        Assert.False(grid.CanPlace(0, 1, PieceType.Horizontal)); // row full
        Assert.True(grid.CanPlace(1, 0, PieceType.CornerNE));
    }

    [Fact]
    public void LoadFromFile_ParsesValidInputCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile,
            """
            # Puzzle 3x3
            ROWS: 1 1 1
            COLS: 1 1 1
            FIXED:
            0,0: Horizontal
            1,1: Vertical
            2,2: CornerNE
            """);
        var puzzle = Puzzle.LoadFromFile(tempFile);
        var grid = new Grid(puzzle);

        Assert.Equal(3, grid.Rows);
        Assert.Equal(3, grid.Cols);
        Assert.Equal(PieceType.Horizontal, grid.Board[0, 0]);
        Assert.Equal(PieceType.Vertical, grid.Board[1, 1]);
        Assert.Equal(PieceType.CornerNE, grid.Board[2, 2]);
    }

    [Fact]
    public void LoadFromFile_ThrowsOnMissingRowsOrCols()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile,
            """
            FIXED:
            0,0: Horizontal
            """);
        Assert.Throws<InvalidDataException>(() => Puzzle.LoadFromFile(tempFile));
    }

    [Fact]
    public void CanPlace_DisallowsMismatchedNeighbor()
    {
       var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile,
            """
            # Puzzle 3x3
            ROWS: 1 1 1
            COLS: 3 0 0
            FIXED:
            0,0: Vertical
            0,2: Vertical
            """);
        var puzzle = Puzzle.LoadFromFile(tempFile);
        var grid = new Grid(puzzle);
        // Trying to place horizontal at (1,0) adjacent to vertical stub
        Assert.False(grid.CanPlace(1,0, PieceType.Horizontal));
    }

    [Fact]
    public void CanPlace_AllowsMatchingNeighbor()
    {
       var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile,
            """
            # Puzzle 3x3
            ROWS: 3 0 0
            COLS: 1 1 1
            FIXED:
            0,0: Horizontal
            0,2: Horizontal
            """);
        var puzzle = Puzzle.LoadFromFile(tempFile);
        var grid = new Grid(puzzle);
        Assert.True(grid.CanPlace(0,1, PieceType.Horizontal));
    }

    [Fact]
    public void IsSingleConnectedPath_DetectsConnectedLShape()
    {
       var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile,
            """
            # Puzzle 2x2
            ROWS: 2 1
            COLS: 1 2
            FIXED:
            0,0: Horizontal
            0,1: CornerSW
            1,1: Vertical
            """);
        var puzzle = Puzzle.LoadFromFile(tempFile);
        var grid = new Grid(puzzle);
        Assert.True(grid.IsSingleConnectedPath());
    }

    [Fact]
    public void IsSingleConnectedPath_FalseForDisconnected()
    {
       var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile,
            """
            # Puzzle 3 3
            ROWS: 1 1 1
            COLS: 1 1 1
            FIXED:
            0,0: Horizontal
            2,2: Horizontal
            """);
        var puzzle = Puzzle.LoadFromFile(tempFile);
        var grid = new Grid(puzzle);
        Assert.False(grid.IsSingleConnectedPath());
    }

}
