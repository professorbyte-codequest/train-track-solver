namespace TrainTrackSolverLib;

public class Data  {
    public PieceType[] startingGrid { get; set; } = [];

    // The verticalClues are the number of pieces in the column
    public int[] verticalClues { get; set; } = [];

    // The horizontalClues are the number of pieces in the row
    public int[] horizontalClues { get; set; } = [];
}