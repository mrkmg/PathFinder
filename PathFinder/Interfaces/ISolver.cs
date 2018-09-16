using System.Collections.Generic;

namespace PathFinder.Interfaces
{
    public interface ISolver<T>
    {
        SolverState State { get; }
        int Ticks { get; }
        T Origin { get; }
        T Destination { get; }

        IList<T> GetPath();
        void Tick();
        void Cancel();
    }

    public enum SolverState
    {
        Running,
        Success,
        Failed
    }
}
