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
                grid.Print();
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading puzzle: {ex.Message}");
                return;
            }


            PathBuilderSolver solver = new PathBuilderSolver(grid);

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
                Console.WriteLine($"Path Builder Solver found a solution in {solver.AttemptCount}:");
                grid.Print();
            }
            else
            {
                Console.WriteLine($"Path Builder Solver could not find a solution in {solver.AttemptCount}.");
            
                Solver solver2 = new Solver(grid);
                solver2.ProgressCallback = count =>
                {
                    Console.Clear();
                    Console.WriteLine($"Attempted paths: {count}");
                    grid.Print();
                };
                solved = solver2.Solve();
                Console.Clear();

                if (solved) {
                    Console.WriteLine($"Solver found a solution in {solver2.AttemptCount}:");
                    grid.Print();
                }
                else {
                    Console.WriteLine($"Solver could not find a solution in {solver2.AttemptCount}.");
                }
            }
        }
    }
}
