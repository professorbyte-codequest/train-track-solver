namespace TrainTrackSolverLib;

public class Point : IComparable
{
    public int X { get; }
    public int Y { get; }

    public Point(int row, int col)
    {
        X = col;
        Y = row;
    }

    public int ManhattanDistance(Point other) =>
        Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

    public override string ToString() => $"({X}, {Y})";

    public override bool Equals(object? obj) => obj != null &&
        obj is Point other && X == other.X && Y == other.Y;

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static Point operator +(Point a, Point b) =>
        new(a.Y + b.Y, a.X + b.X);

    public static Point operator +(Point a, (int dr, int dc) b) =>
        new(a.Y + b.dr, a.X + b.dc);


    // Implement IComparable.

    public int CompareTo(object? obj)
    {
        if (obj is Point other)
        {
            if (Y == other.Y)
                return X.CompareTo(other.X);
            return Y.CompareTo(other.Y);
        }
        throw new ArgumentException("Object is not a Point");
    }
}