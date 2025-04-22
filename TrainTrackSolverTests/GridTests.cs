namespace TrainTrackSolverTests;

// TrainTrackSolverTests - Unit tests for the TrainTrackSolverLib
using TrainTrackSolverLib;
using Xunit;

public class GridTests
{
    [Fact]
    public void CanPlace_RespectsBoundsAndFill()
    {
        var grid = new Grid(3, 3, new[] { 1, 1, 1 }, new[] { 1, 1, 1 });
        Assert.True(grid.CanPlace(1, 1, PieceType.Horizontal));
        grid.Place(1, 1, PieceType.Horizontal);
        Assert.False(grid.CanPlace(1, 1, PieceType.Vertical)); // already filled
    }

    [Fact]
    public void CanPlace_EnforcesRowAndColLimits()
    {
        var grid = new Grid(3, 3, new[] { 1, 1, 1 }, new[] { 2, 1, 1 });

        grid.Place(0, 0, PieceType.Vertical);

        Assert.False(grid.CanPlace(0, 1, PieceType.Horizontal)); // row full
        Assert.True(grid.CanPlace(1, 0, PieceType.Vertical));
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

    [Fact]
    public void CanPlace_DisallowsMismatchedNeighbor()
    {
        var grid = new Grid(2, 2, new[]{1,1}, new[]{1,1});
        grid.Place(0,0, PieceType.Horizontal);
        // Trying to place vertical at (1,0) adjacent to horizontal stub
        Assert.False(grid.CanPlace(1,0, PieceType.Vertical));
    }

    [Fact]
    public void CanPlace_AllowsMatchingNeighbor()
    {
        var grid = new Grid(3, 3, new[]{2,0,0}, new[]{1,1,1});
        grid.Place(0,0, PieceType.Horizontal);
        // Place horizontal at (0,1)
        Assert.True(grid.CanPlace(0,1, PieceType.Horizontal));
    }

    [Fact]
    public void IsSingleConnectedPath_DetectsConnectedLShape()
    {
        var grid = new Grid(2, 2, new[]{2,1}, new[]{1,2});
        // L shape: (0,0) horizontal, (0,1) cornerSE, (1,1) vertical
        grid.Place(0,0, PieceType.Horizontal);
        grid.Place(0,1, PieceType.CornerSW);
        grid.Place(1,1, PieceType.Vertical);
        Assert.True(grid.IsSingleConnectedPath());
    }

    [Fact]
    public void IsSingleConnectedPath_RejectsCycleIsolated()
    {
        var grid = new Grid(2, 2, new[]{2,2}, new[]{2,2});
        // place a 2x2 loop
        grid.Place(0,0, PieceType.CornerSE);
        grid.Place(0,1, PieceType.CornerSW);
        grid.Place(1,1, PieceType.CornerNW);
        grid.Place(1,0, PieceType.CornerNE);
        Assert.True(grid.IsSingleConnectedPath()); // loop is continuous
    }

    [Fact]
    public void IsSingleConnectedPath_FalseForDisconnected()
    {
        var grid = new Grid(3, 3, new[]{1,1,1}, new[]{1,1,1});
        grid.Place(0,0, PieceType.Horizontal);
        grid.Place(2,2, PieceType.Horizontal);
        Assert.False(grid.IsSingleConnectedPath());
    }

}
