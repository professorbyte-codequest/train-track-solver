namespace TrainTrackSolverLib;

using System.Text.Json;

public class PuzzleManager {
    private List<Grid> _puzzles = new List<Grid>();

    public Grid GetPuzzle(int index) => _puzzles[index].Clone();

    public int Count => _puzzles.Count;

    public static string Path { get; set; }

    private static readonly Lazy<PuzzleManager> Lazy = new(GetPuzzleManager);

    public static PuzzleManager Instance => Lazy.Value;

    private PuzzleManager()
    {
    }

    public IEnumerable<Grid> Puzzles()
    {
        foreach (var puzzle in _puzzles)
        {
            yield return puzzle.Clone();
        }
    }

    private static PuzzleManager GetPuzzleManager()
    {
        if (Path == null)
            throw new ArgumentNullException(nameof(Path), "Path cannot be null");

        var json = File.ReadAllText(Path);

        var puzzles = JsonSerializer.Deserialize<Puzzle[]>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var Grids = new List<Grid>();

        foreach (var puzzle in puzzles)
        {
            var grid = new Grid(puzzle);

            Grids.Add(grid);
        }

        var instance = new PuzzleManager
        {
            _puzzles = Grids
        };

        return instance;
    }
}