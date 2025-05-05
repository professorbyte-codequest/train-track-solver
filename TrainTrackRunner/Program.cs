// TrainTrackRunner - Console app to run the Part 1 solver
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TrainTrackSolverLib;
using TrainTrackSolverLib.Common;
using CommandLine;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TrainTrackRunner
{
    class ConsoleProgressReporter : IProgressReporter
    {
        private Grid _grid;

        public long ProgressInterval { get; set; } = 100000;

        public ConsoleProgressReporter(Grid grid)
        {
            _grid = grid;
        }

        public void Report(long iterations)
        {
            Console.CursorTop = 0;
            _grid.Print(true);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Progress: {iterations} iterations");
            Console.WriteLine($"Total Count: {_grid.TotalCount}");
            Console.WriteLine($"Placed Count: {_grid.PlacedCount}");
            Console.WriteLine($"Entry {_grid.Entry}, Exit {_grid.Exit}");
        }
    }

    class Program
    {
        private static readonly int DefaultTimeoutMinutes = 5;
        private static readonly int DefaultProgressInterval = 1000000;

        private TimeSpan SolverTimeout = TimeSpan.FromMinutes(DefaultTimeoutMinutes);

        public class Options {
            [Option('p', "puzzle", Required = true, HelpText = "Path to the puzzle file.")]
            public string PuzzlePath { get; set; } = string.Empty;
            [Option('x', "index", Required = false, Default = 0, HelpText = "Puzzle index to run.")]
            public int PuzzleIndex { get; set; } = 0;

            [Option('m', "mode", Required = false, Default = "path", HelpText = "Solver mode: path, astar, pq")]
            public string SolverMode { get; set; } = "path";

            [Option('i', "interval", Required = false, Default = 1000000, HelpText = "Progress interval in iterations.")]
            public long ProgressInterval { get; set; } = DefaultProgressInterval;

            [Option('b', "benchmark", Required = false, Default = false, HelpText = "Run benchmark on the puzzle.")]
            public bool Benchmark { get; set; } = false;
            [Option('g', "generate", Required = false, Default = false, HelpText = "Generate a new puzzle.")]
            public bool Generate { get; set; } = false;
            [Option('t', "timeout", Required = false, Default = 5, HelpText = "Solver timeout in minutes.")]
            public int TimeoutMinutes { get; set; } = DefaultTimeoutMinutes;

        }

        static void runSinglePuzzleMode(Options options)
        {
            var path = options.PuzzlePath;
            if (path.EndsWith(".txt"))
            {
                var puzzle = Puzzle.LoadFromFile(path);
                var modes = options.SolverMode.Split(',');
                foreach (var solverMode in modes)
                {
                    var grid = new Grid(puzzle);
                    Console.WriteLine($"Loaded puzzle from {path}");
                    Console.WriteLine($"Grid size: {grid.Rows} x {grid.Cols}");
                    Console.WriteLine($"Entry: {grid.Entry}, Exit: {grid.Exit}");
                    Console.WriteLine($"Total Count: {grid.TotalCount}");
                    Console.WriteLine($"Placed Count: {grid.PlacedCount}");
                    Console.WriteLine($"Solver mode: {solverMode}");
                    grid.Print();
                    Console.WriteLine("Press any key to start solving...");
                    Console.ReadKey();
                    Console.Clear();
                    Console.WriteLine();
                    solveGridWithSolver(grid, solverMode, options.ProgressInterval, options.TimeoutMinutes);
                }
            }
            else
            {
                PuzzleManager.Path = path;
                var puzzleManager = PuzzleManager.Instance;
                var modes = options.SolverMode.Split(',');

                foreach (var solverMode in modes)
                {
                    var grid = puzzleManager.GetPuzzle(options.PuzzleIndex);
                    Console.WriteLine($"Loaded puzzle {options.PuzzleIndex} from {path}");
                    Console.WriteLine($"Grid size: {grid.Rows} x {grid.Cols}");
                    Console.WriteLine($"Entry: {grid.Entry}, Exit: {grid.Exit}");
                    Console.WriteLine($"Total Count: {grid.TotalCount}");
                    Console.WriteLine($"Placed Count: {grid.PlacedCount}");
                    Console.WriteLine($"Solver mode: {solverMode}");
                    grid.Print();
                    Console.WriteLine("Press any key to start solving...");
                    Console.ReadKey();
                    Console.Clear();
                    Console.WriteLine();
                    solveGridWithSolver(grid, solverMode, options.ProgressInterval, options.TimeoutMinutes);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        static void runBenchmarkMode(Options options)
        {
            var path = options.PuzzlePath;
            if (path.EndsWith(".txt"))
            {
                var puzzle = Puzzle.LoadFromFile(path);
                var grid = new Grid(puzzle);
                Console.WriteLine($"Loaded puzzle from {path}");
                Console.WriteLine($"Grid size: {grid.Rows} x {grid.Cols}");
                Console.WriteLine($"Entry: {grid.Entry}, Exit: {grid.Exit}");
                Console.WriteLine($"Total Count: {grid.TotalCount}");
                Console.WriteLine($"Placed Count: {grid.PlacedCount}");
                grid.Print();
                Console.WriteLine("Press any key to start solving...");
                Console.ReadKey();
                Console.Clear();
                Console.WriteLine();
                PrintHeader();
                var modes = options.SolverMode.Split(',');
                foreach (var solverMode in modes)
                {
                    RunBenchmark(grid, options.TimeoutMinutes, false, solverMode);
                }
            }
            else
            {
                PuzzleManager.Path = path;
                var puzzleManager = PuzzleManager.Instance;
                Console.WriteLine();
                Console.WriteLine($"Loaded puzzle manager from {path}");
                Console.WriteLine($"Puzzle count: {puzzleManager.Count}");
                Console.WriteLine($"Solver mode: {options.SolverMode}");
                Console.WriteLine($"Timeout: {options.TimeoutMinutes} minutes");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                PrintHeader();
                var modes = options.SolverMode.Split(',');
                foreach (var grid in puzzleManager.Puzzles())
                {
                    foreach (var solverMode in modes)
                    {
                        RunBenchmark(grid, options.TimeoutMinutes, false, solverMode);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    if (options.Benchmark) {
                        runBenchmarkMode(options);
                        return;
                    } else if (options.Generate) {
                        //runGenerateMode(options);
                        return;
                    }
                    runSinglePuzzleMode(options);
                })
                .WithNotParsed(errors =>
                {
                    Console.WriteLine("Error parsing arguments.");
                });
        }

        class NullProgressReporter : IProgressReporter
        {
            public long ProgressInterval { get; set; } = 10000000000000;

            public void Report(long iterations)
            {
                // No-op
            }
        }

        static void solveGridWithSolver(Grid grid, string mode, long interval, int timeout)
        {
            Console.Clear();
            grid.Print();

            var progressReporter = new ConsoleProgressReporter(grid);
            ISolver solver = mode switch
            {
                "pq"   => new Solver(grid, progressReporter),
                "astar"=>  new AStarSolver(grid, progressReporter),
                _       => new PathBuilderSolver(grid, progressReporter)
            };

            progressReporter.ProgressInterval = interval;

            Console.WriteLine($"Running solver with a timeout of {timeout} minutes...");
            var sw = Stopwatch.StartNew();
            var task = Task.Run(() => solver.Solve());
            bool completed = task.Wait(TimeSpan.FromMinutes(timeout));
            sw.Stop();

            var attempts = solver.IterationCount;
            Console.Clear();
            grid.Print();
            Console.WriteLine();
            if (!completed)
            {
                Console.WriteLine($"Solver {mode} timed out after {sw.ElapsedMilliseconds} ms, {attempts} iterations.");
                return;
            }

            bool solved = task.Result;
            if (solved)
            {
                Console.WriteLine($"Solver {mode} found a solution in {attempts} iterations ({sw.ElapsedMilliseconds} ms):");
            }
            else
            {
                Console.WriteLine($"Solver {mode} could not find a solution in {attempts} iterations ({sw.ElapsedMilliseconds} ms).");
            }
        }

        static void PrintHeader()
        {
            Console.WriteLine("| Rows x Cols | Solver | Iterations | Time (ms) | Solved |");
            Console.WriteLine("|-------------|--------|------------|-----------|--------|");
        }

        static void RunBenchmark(Grid grid, int timeout, bool printHeader = true, string? solverMode = null)
        {
            if (printHeader)
            {
                PrintHeader();
            }

            // Run each solver
            var solvers = new[] { "path",  "astar", "pq", };
            var reporters = new NullProgressReporter();

            foreach (var mode in solvers)
            {
                if (solverMode != null && mode != solverMode)
                    continue;

                var g = grid.Clone();

                ISolver solver = mode switch
                {
                    "pq"    => new Solver(g, reporters),
                    "astar" => new AStarSolver(g, reporters),
                    _        => new PathBuilderSolver(g, reporters)
                };

                var sw = Stopwatch.StartNew();
                
                var task = Task.Run(() => solver.Solve());
                bool completed = task.Wait(TimeSpan.FromMinutes(timeout));

                sw.Stop();
                bool solved = completed && task.Result;
                Console.WriteLine($"| {grid.Rows}x{grid.Cols} | {mode} | {solver.IterationCount} | {sw.ElapsedMilliseconds} | {(solved ? "Yes" : "No")} |");
            }
        }
    }
}