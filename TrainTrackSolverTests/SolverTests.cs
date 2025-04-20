namespace TrainTrackSolverTests;

// TrainTrackSolverTests - Unit tests for Part 1 logic
using TrainTrackSolverLib;
using Xunit;

public class SolverTests
{
    [Fact]
    public void Solver_FindsValidSolution_ForSimpleGrid()
    {
        var rowCounts = new[] { 1, 1, 1 };
        var colCounts = new[] { 1, 1, 1 };
        var grid = new Grid(3, 3, rowCounts, colCounts);
        var solver = new Solver(grid);

        bool solved = solver.Solve();

        Assert.True(solved);

        int totalTracks = 0;
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                if (grid.Board[r, c] != PieceType.Empty)
                    totalTracks++;

        Assert.Equal(3, totalTracks);
    }
}
