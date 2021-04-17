using System.Collections.Generic;
using JetBrains.Annotations;

namespace PathFinder.Solvers.Generic
{
    /// <summary>
    /// All Generic Graph Solvers implement this interface. This ensures solvers
    /// can be used interchangeably depending on need.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGraphSolver<T>
    {
        /// <summary>
        /// The current state of the solver. See <see cref="SolverState"/>
        /// </summary>
        SolverState State { get; }
        
        /// <summary>
        /// The number of ticks this solver has performed.
        /// </summary>
        int Ticks { get; }
        
        /// <summary>
        /// The maximum number of ticks this solver can perform before giving up.
        /// </summary>
        int MaxTicks { get; set; }
        
        /// <summary>
        /// The last node to be checked
        /// </summary>
        T Current { get; }
        
        /// <summary>
        /// The starting node to search from.
        /// </summary>
        T Origin { get; }
        
        /// <summary>
        /// The destination node to search for.
        /// </summary>
        T Destination { get; }
        
        /// <summary>
        /// The current best path to get as close as possible to the Destination.
        /// </summary>
        /// <remarks>
        /// Not guaranteed to be a "good" path, and may vary greatly each tick.
        /// </remarks>
        [CanBeNull] IList<T> CurrentBestPath { get; }
        
        /// <summary>
        /// The path from the Origin to the Destination. Will be null if <see cref="State"/> is not <see cref="SolverState.Success"/>.
        /// </summary>
        [CanBeNull] IList<T> Path { get; }
        
        /// <summary>
        /// A List of nodes which still need to be checked
        /// </summary>
        IEnumerable<T> Open { get; }
        
        /// <summary>
        /// A list of nodes which have already been checked
        /// </summary>
        IEnumerable<T> Closed { get; }
        
        /// <summary>
        /// Count of open nodes
        /// </summary>
        int OpenCount { get; }
        
        /// <summary>
        /// Count of closed nodes
        /// </summary>
        int ClosedCount { get; }
        
        /// <summary>
        /// The cost to get from the Origin to the Destination via the path
        /// </summary>
        double PathCost { get; }
        
        /// <summary>
        /// Stop the current solver.
        /// </summary>
        void Stop();

        /// <summary>
        /// Runs the solver until a path is found, the allotted ticks is exhausted, or a path could not be found.
        /// </summary>
        /// <param name="ticks">
        /// Number of ticks to run. Any number less than zero means run until <see cref="MaxTicks"/>
        /// </param>
        /// <returns>
        /// The <see cref="SolverState"/> the solver is in after the run.
        /// </returns>
        SolverState Run(int ticks = -1);
    }

    /// <summary>
    /// Denotes the state of a graph solver.
    /// </summary>
    public enum SolverState
    {
        /// <summary>
        /// The solver is waiting to run.
        /// </summary>
        Waiting = 0,
        
        /// <summary>
        /// The solver is currently running.
        /// </summary>
        Running = 1,
        
        /// <summary>
        /// The solver failed.
        /// </summary>
        Failure = 2,
        
        /// <summary>
        /// The solver succeeded and found a path.
        /// </summary>
        Success = 3
    }
}
