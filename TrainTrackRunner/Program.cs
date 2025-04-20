// TrainTrackRunner - Console app to run the Part 1 solver
using System;
using TrainTrackSolverLib;

namespace TrainTrackRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Train Tracks Solver - Part 1\n");

            int[] rowCounts = { 2, 1, 3 };
            int[] colCounts = { 1, 2, 3 };

            var grid = new Grid(3, 3, rowCounts, colCounts);
            var solver = new Solver(grid);

            if (solver.Solve())
            {
                Console.WriteLine("Solved:");
                grid.Print();
            }
            else
            {
                Console.WriteLine("No solution found.");
            }
        }
    }
}
