namespace TrainTrackSolverLib;
    
/// <summary>
/// Enum representing the different types of track pieces.
/// </summary>
public enum PieceType
{
    /// <summary>Empty piece, no track.</summary>
    Empty,
    /// <summary>Horizontal track piece.</summary>
    Horizontal,
    /// <summary>Vertical track piece.</summary>
    Vertical,
    /// <summary>Corner track piece, connecting north to east.</summary>
    CornerNE,
    /// <summary>Corner track piece, connecting north to west.</summary>
    CornerNW,
    /// <summary>Corner track piece, connecting south to east.</summary>
    CornerSE,
    /// <summary>Corner track piece, connecting south to west.</summary>
    CornerSW,
}