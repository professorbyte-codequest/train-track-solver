namespace TrainTrackSolverTests;

using TrainTrackSolverLib;
using Xunit;

public class TrackConnectionsTests
{
    [Theory]
    // Horizontal should connect left and right
    [InlineData(PieceType.Horizontal, new[]{0,1}, new[]{0,-1})]
    [InlineData(PieceType.Vertical,   new[]{-1,0}, new[]{1,0})]
    [InlineData(PieceType.CornerNE,   new[]{-1,0}, new[]{0,1})]
    [InlineData(PieceType.CornerNW,   new[]{-1,0}, new[]{0,-1})]
    [InlineData(PieceType.CornerSE,   new[]{1,0}, new[]{0,1})]
    [InlineData(PieceType.CornerSW,   new[]{1,0}, new[]{0,-1})]
    public void ConnectsTo_ValidDirections(PieceType type, int[] dir1, int[] dir2)
    {
        Assert.True(TrackConnections.ConnectsTo(type, (dir1[0], dir1[1])));
        Assert.True(TrackConnections.ConnectsTo(type, (dir2[0], dir2[1])));
    }

    [Fact]
    public void GetConnections_ReturnsEmptyForEmptyPiece()
    {
        var conns = TrackConnections.GetConnections(PieceType.Empty);
        Assert.Empty(conns);
    }

    [Theory]
    // Horizontal should connect left and right
    [InlineData(PieceType.Horizontal, new[]{0,1}, new[]{0,-1})]
    [InlineData(PieceType.Vertical,   new[]{-1,0}, new[]{1,0})]
    [InlineData(PieceType.CornerNE,   new[]{-1,0}, new[]{0,1})]
    [InlineData(PieceType.CornerNW,   new[]{-1,0}, new[]{0,-1})]
    [InlineData(PieceType.CornerSE,   new[]{1,0}, new[]{0,1})]
    [InlineData(PieceType.CornerSW,   new[]{1,0}, new[]{0,-1})]
    public void GetPieceForDirs_ReturnsValidPieces(PieceType type, int[] dir1, int[] dir2)
    {
        var piece = TrackConnections.GetPieceForDirs((dir1[0], dir1[1]), (dir2[0], dir2[1]));
        Assert.Equal(type, piece);
    }
}
