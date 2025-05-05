namespace TrainTrackSolverLib;
    
/// <summary>
/// Enum representing the different types of track pieces.
/// </summary>
public enum PieceType
{
    /// <summary>Empty piece, no track.</summary>
    Empty = 0,
    /// <summary>Horizontal track piece.</summary>
    Horizontal = 3,
    /// <summary>Vertical track piece.</summary>
    Vertical = 4,
    /// <summary>Corner track piece, connecting north to east.</summary>
    CornerNE = 5,
    /// <summary>Corner track piece, connecting south to east.</summary>
    CornerSE = 6,
    /// <summary>Corner track piece, connecting south to west.</summary>
    CornerSW = 7,
    /// <summary>Corner track piece, connecting north to west.</summary>
    CornerNW = 8,
}