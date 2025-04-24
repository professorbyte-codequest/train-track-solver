using System;
using System.Collections.Generic;

namespace TrainTrackSolverLib.Common
{
    /// <summary>
    /// Defines a generic solver that finds a Solution for the given grid.
    /// </summary>
    public interface ISolver
    {
        /// <summary>
        /// Solves the grid.
        /// </summary>
        /// <returns>True if the grid can be solved, false otherwise.</returns>
        bool Solve();

        /// <summary>
        /// Gets the number of iterations performed during the solve operation.
        /// </summary>
        long IterationCount { get; }
    }

    /// <summary>
    /// Reports progress of processing during solve operations.
    /// </summary>
    public interface IProgressReporter
    {
        /// <summary>
        /// Reports the progress of the solver.
        /// </summary>
        /// <param name="iterations">Number of iterations so far.</param>
        void Report(long iterations);

        /// <summary>
        /// Gets or sets the interval for progress reporting.
        /// </summary>
        long ProgressInterval { get; set; }
    }
}
