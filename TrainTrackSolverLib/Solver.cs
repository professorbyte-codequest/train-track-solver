namespace TrainTrackSolverLib;

public class Solver
{
    private readonly Grid _grid;

    public Solver(Grid grid)
    {
        _grid = grid;
    }

    public bool Solve(int r = 0, int c = 0)
    {
        if (r == _grid.Rows)
            return IsComplete();

        int nextR = c == _grid.Cols - 1 ? r + 1 : r;
        int nextC = c == _grid.Cols - 1 ? 0 : c + 1;

        foreach (PieceType piece in Enum.GetValues(typeof(PieceType)))
        {
            if (piece == PieceType.Empty) continue;
            if (_grid.CanPlace(r, c, piece))
            {
                _grid.Place(r, c, piece);
                if (Solve(nextR, nextC)) return true;
                _grid.Remove(r, c);
            }
        }

        // try leaving the cell empty
        return Solve(nextR, nextC);
    }

    private bool IsComplete()
    {
        for (int r = 0; r < _grid.Rows; r++)
            if (_grid.TrackCountInRow(r) != _grid.RowCounts[r])
                return false;

        for (int c = 0; c < _grid.Cols; c++)
            if (_grid.TrackCountInCol(c) != _grid.ColCounts[c])
                return false;

        return true;
    }
}