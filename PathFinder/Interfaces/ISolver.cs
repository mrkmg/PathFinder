using System.Collections.Generic;
using JetBrains.Annotations;

namespace PathFinder.Interfaces
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
        ///     Perform one tick
        /// </summary>
        void Tick();
        
        /// <summary>
        ///     Stop the current solver.
        /// </summary>
        void Stop();
    }

    public static class SolverExtensions
    {
        [CanBeNull]
        public static IList<T> Solve<T>([NotNull] this ISolver<T> solver)
        {
            while (solver.State == SolverState.Running) 
                solver.Tick();
            
            return solver.State == SolverState.Success ? solver.Path : null;
        }
    }

    public enum SolverState
    {
        Stopped = 0,
        Running = 1,
        Failure = 2,
        Success = 3
    }
}
