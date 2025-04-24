namespace TrainTrackSolverTests;

// TrainTrackSolverTests - Unit tests for the TrainTrackSolverLib
using TrainTrackSolverLib;
using TrainTrackSolverLib.Common;
using Xunit;

public class SolverTests
{ 

    private class NullProgressReporter : IProgressReporter
    {
        public long ProgressInterval { get; set; } = 1000;

        public void Report(long iterations)
        {
            // No-op
        }
    }

    private string WritePuzzle(string content)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, content);
        return path;
    }

    private void AssertSolvesGrid(string path)
    {
        var grid = Grid.LoadFromFile(path);
        var solver = new Solver(grid, new NullProgressReporter());
        Assert.True(solver.Solve());
        Assert.True(grid.IsSingleConnectedPath());

        grid = Grid.LoadFromFile(path);
        var solver2 = new AStarSolver(grid, new NullProgressReporter());
        Assert.True(solver2.Solve());
        Assert.True(grid.IsSingleConnectedPath());

        grid = Grid.LoadFromFile(path);
        var solver3 = new PathBuilderSolver(grid, new NullProgressReporter());
        Assert.True(solver3.Solve());
        Assert.True(grid.IsSingleConnectedPath());
    }

    [Fact]
    public void Solve_Easy3x3Straight()
    {
        string puzzle =
            """
            # 3x3 Straight Line
            ROWS: 1 1 1
            COLS: 0 3 0
            FIXED:
            0,1: Vertical
            2,1: Vertical
            """;
        AssertSolvesGrid(WritePuzzle(puzzle));
    }

    [Fact]
    public void Solve_Medium5x5T()
    {
        string puzzle =
            """
            # 5x5 L Shape
            ROWS: 5 1 1 1 1
            COLS: 1 1 1 1 5
            FIXED:
            0,0: CornerNE
            4,4: CornerNE
            """;
        AssertSolvesGrid(WritePuzzle(puzzle));
    }

    [Fact]
    public void Solve_Hard10x10Line()
    {
        string puzzle =
            """
            # 10x10 Horizontal Line
            ROWS: 0 0 0 0 0 10 0 0 0 0
            COLS: 1 1 1 1 1 1 1 1 1 1
            FIXED:
            5,0: Horizontal
            5,9: Horizontal
            """;
        AssertSolvesGrid(WritePuzzle(puzzle));
    }

    [Fact]
    public void Solve_Small()
    {
        string puzzle =
            """
            ROWS: 2 7 5 4 8 3 2
            COLS: 1 1 5 6 5 4 3 4 2
            FIXED:
            0, 6: CornerSW
            3, 4: CornerSW
            4, 4: Vertical
            4, 0: Horizontal
            6, 2: CornerSE
            """;
        AssertSolvesGrid(WritePuzzle(puzzle));
    }
}
