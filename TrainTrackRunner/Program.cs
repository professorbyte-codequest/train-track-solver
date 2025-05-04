// TrainTrackRunner - Console app to run the Part 1 solver
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TrainTrackSolverLib;
using TrainTrackSolverLib.Common;

namespace TrainTrackRunner
{
    class ConsoleProgressReporter : IProgressReporter
    {
        private Grid _grid;

        public long ProgressInterval { get; set; } = 1000;

        public ConsoleProgressReporter(Grid grid)
        {
            _grid = grid;
        }

        public void Report(long iterations)
        {
            Console.Clear();
            Console.WriteLine($"Progress: {iterations} iterations");
            _grid.Print();
        }
    }

    class Program
    {
        private static readonly TimeSpan SolverTimeout = TimeSpan.FromMinutes(1);

        static void Main(string[] args)
        {
            Console.WriteLine("Train Tracks Solver\n");
            if (args.Length < 1 || args.Length > 5)
            {
                Console.WriteLine("Usage: TrainTrackRunner <puzzle-file-path> [path|pq|astar|benchmark|generate] [rows] [cols] [fixedCount]");
                return;
            }

            var path = args[0];
            var mode = args.Length > 1 ? args[1].ToLower() : "path";
            if (mode != "path" && mode != "pq" && mode != "astar" && mode != "benchmark" && mode != "generate")
            {
                Console.WriteLine("Invalid mode. Use 'path', 'pq', 'astar', 'benchmark', or 'generate'.");
                return;
            }

            long interval;
            if (mode == "generate")
            {
                Console.Clear();
                Console.WriteLine("Generating a random puzzle...");

                var rows = args.Length > 2 ? int.Parse(args[2]) : 10;
                var cols = args.Length > 3 ? int.Parse(args[3]) : 10;
                var fixedCount = args.Length > 4 ? int.Parse(args[4]) : 7;
                var newgrid = PuzzleGenerator.Generate(rows, cols, fixedCount);
                newgrid.Print(true);

                newgrid.SaveToFile(path);
                Console.WriteLine($"Puzzle saved to {path}");
                return;
            }
            else if (mode == "benchmark")
            {
                RunBenchmark(path);
                return;
            } else if (mode == "path")
            {
                interval = 100L;
            }
            else if (mode == "pq")
            {
                interval = 10000L;
            }
            else if (mode == "astar")
            {
                interval = 100L;
            }
            else
            {
                Console.WriteLine("Invalid mode. Use 'path', 'pq', 'astar', 'benchmark', or 'generate'.");
                return;
            }
            
            Console.Clear();

            // Single-solver mode
            Grid grid;
            try { grid = Grid.LoadFromFile(path); grid.Print(); }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading puzzle: {ex.Message}");
                return;
            }

            Console.WriteLine($"Using {mode} solver. Press any key to start...");
            Console.ReadKey();
            var progressReporter = new ConsoleProgressReporter(grid);
            ISolver solver = mode switch
            {
                "pq"   => new Solver(grid, progressReporter),
                "astar"=>  new AStarSolver(grid, progressReporter),
                _       => new PathBuilderSolver(grid, progressReporter)
            };

            progressReporter.ProgressInterval = interval;

            Console.WriteLine($"Running solver with a timeout of {SolverTimeout.TotalMinutes} minutes...");
            var sw = Stopwatch.StartNew();
            var task = Task.Run(() => solver.Solve());
            bool completed = task.Wait(SolverTimeout);
            sw.Stop();

            var attempts = solver.IterationCount;
            Console.Clear();

            if (!completed)
            {
                Console.WriteLine($"Solver {mode} timed out after {SolverTimeout.TotalMinutes} minutes ({sw.ElapsedMilliseconds} ms, {attempts} iterations).");
                grid.Print();
                return;
            }

            bool solved = task.Result;
            if (solved)
            {
                Console.WriteLine($"Solver {mode} found a solution in {attempts} iterations ({sw.ElapsedMilliseconds} ms):");
                grid.Print();
            }
            else
            {
                Console.WriteLine($"Solver {mode} could not find a solution in {attempts} iterations ({sw.ElapsedMilliseconds} ms).");
                grid.Print();
            }
        }

        class NullProgressReporter : IProgressReporter
        {
            public long ProgressInterval { get; set; } = 10000000000000;

            public void Report(long iterations)
            {
                // No-op
            }
        }


        static void RunBenchmark(string path)
        {
            var reporters = new NullProgressReporter();
            var solvers = new[] { "path",  "astar", "pq", };

            // Output markdown table
            Console.WriteLine("| Solver | Iterations | Time (ms) | Solved |");
            Console.WriteLine("| ------ | ---------- | --------- | ------ |");

            foreach (var mode in solvers)
            {
                Grid grid;
                try { grid = Grid.LoadFromFile(path); }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading puzzle for {mode}: {ex.Message}");
                    continue;
                }

                ISolver solver = mode switch
                {
                    "pq"    => new Solver(grid, reporters),
                    "astar" => new AStarSolver(grid, reporters),
                    _        => new PathBuilderSolver(grid, reporters)
                };

                var sw = Stopwatch.StartNew();
                
                var task = Task.Run(() => solver.Solve());
                bool completed = task.Wait(SolverTimeout);

                sw.Stop();
                bool solved = completed && task.Result;
                Console.WriteLine($"| {mode} | {solver.IterationCount} | {sw.ElapsedMilliseconds} | {(solved ? "Yes" : "No")} |");
            }
        }
    }
}
