namespace TrainTrackSolverTests;

// TrainTrackSolverTests - Unit tests for Part 1 logic
using TrainTrackSolverLib;
using Xunit;

public class GridTests
{
    [Fact]
    public void CanPlace_RespectsBoundsAndFill()
    {
        var grid = new Grid(3, 3, new[] { 1, 1, 1 }, new[] { 1, 1, 1 });

        Assert.True(grid.CanPlace(0, 0, PieceType.Horizontal));
        grid.Place(0, 0, PieceType.Horizontal);
        Assert.False(grid.CanPlace(0, 0, PieceType.Vertical)); // already filled
    }

    [Fact]
    public void CanPlace_EnforcesRowAndColLimits()
    {
        var grid = new Grid(2, 2, new[] { 1, 1 }, new[] { 1, 1 });

        Assert.True(grid.CanPlace(0, 0, PieceType.Vertical));
        grid.Place(0, 0, PieceType.Vertical);

        Assert.False(grid.CanPlace(0, 1, PieceType.Horizontal)); // row full
        Assert.True(grid.CanPlace(1, 1, PieceType.Vertical));
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

        var grid = Grid.LoadFromFile(tempFile);

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

        Assert.Throws<InvalidDataException>(() => Grid.LoadFromFile(tempFile));
    }
}
