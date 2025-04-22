// TrainTrackRunner - Console app to run the Part 1 solver
using System;
using TrainTrackSolverLib;

namespace TrainTrackRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Train Tracks Solver\n");

            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TrainTrackRunner <puzzle-file-path>");
                return;
            }

            string path = args[0];
            Grid grid;
            try
            {
                grid = Grid.LoadFromFile(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading puzzle: {ex.Message}");
                return;
            }

            var solver = new Solver(grid);

            solver.ProgressCallback = count =>
            {
                Console.Clear();
                Console.WriteLine($"Attempted paths: {count}");
                grid.Print();
            };

            bool solved = solver.Solve();
            Console.Clear();

            if (solved)
            {
                Console.WriteLine($"Solved in {solver.AttemptCount}:");
                grid.Print();
            }
            else
            {
                Console.WriteLine($"No solution found in {solver.AttemptCount} attempts.");
            }
        }
    }
}
