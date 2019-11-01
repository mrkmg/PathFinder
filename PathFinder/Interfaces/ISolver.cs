using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace PathFinder.Interfaces
{
    public interface ISolver<T>
    {
        SolverState State { get; }
        int Ticks { get; }
        T Current { get; }
        T Origin { get; }
        T Destination { get; }
        [CanBeNull] IList<T> Path { get; }
        IEnumerable<T> Open { get; }
        IEnumerable<T> Closed { get; }

        void Tick();
        void Cancel();
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
        Running,
        Success,
        Failed
    }
}
