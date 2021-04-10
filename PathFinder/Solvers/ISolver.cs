using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace PathFinder.Solvers
{
    public interface ISolver<T>
    {
        /// <summary>
        ///     The current state of the solver. See <see cref="SolverState"/>
        /// </summary>
        SolverState State { get; }
        
        /// <summary>
        ///     The number of ticks this solver has performed.
        /// </summary>
        int Ticks { get; }
        
        /// <summary>
        ///     The maximum number of ticks this solver can perform before giving up.
        /// </summary>
        int MaxTicks { get; set; }
        
        /// <summary>
        ///     The last node to be checked
        /// </summary>
        T Current { get; }
        
        /// <summary>
        ///     The starting <see cref="T"/> to search from.
        /// </summary>
        T Origin { get; }
        
        /// <summary>
        ///     The destination <see cref="T"/> to search for.
        /// </summary>
        T Destination { get; }
        
        /// <summary>
        ///     The current best path to get as close as possible to the Destination.
        /// <remarks>Not guaranteed to be a "good" path, and may vary greatly each tick.</remarks>
        /// </summary>
        [CanBeNull] IList<T> CurrentBestPath { get; }
        
        /// <summary>
        ///     The path from the Origin to the Destination. Will be null if <see cref="State"/> is not <see cref="SolverState.Success"/>.
        /// </summary>
        [CanBeNull] IList<T> Path { get; }
        
        /// <summary>
        ///     A List of nodes which still need to be checked
        /// </summary>
        IEnumerable<T> Open { get; }
        
        /// <summary>
        ///     A list of nodes which have already been checked
        /// </summary>
        IEnumerable<T> Closed { get; }
        
        /// <summary>
        ///     Count of open nodes
        /// </summary>
        int OpenCount { get; }
        
        /// <summary>
        ///     Count of closed nodes
        /// </summary>
        int ClosedCount { get; }
        
        /// <summary>
        ///     The cost to get from the Origin to the Destination via the path
        /// </summary>
        double PathCost { get; }
        
        
        /// <summary>
        ///     Stop the current solver.
        /// </summary>
        void Stop();

        /// <summary>
        /// Starts the solver. Will run until a path is found or failure.
        /// </summary>
        /// <param name="ticks">Number of ticks to run. Any number less than zero means run until <see cref="MaxTicks"/></param>
        void Start(int ticks = -1);

        /// <summary>
        /// Async version of <see cref="Start"/>
        /// </summary> 
        /// <param name="ticks">Number of ticks to run. Any number less than zero means run until <see cref="MaxTicks"/></param>
        Task StartAsync(int ticks = -1);
    }

    public enum SolverState
    {
        Waiting = 0,
        Running = 1,
        Failure = 2,
        Success = 3
    }
}
