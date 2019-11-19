using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PathFinder.Interfaces;
using PathFinder.Solvers;
using SimpleWorld.Map;

namespace PathFinderGui
{

    public interface SolverRunner
    {
        AStar<Position> Solver { get; set; }
        bool kill { get; set; }
        void Run();
        IReadOnlyCollection<Position> GetCheckedPositions();
    }
    
    public class SolverRunnerThread : SolverRunner
    {
        private Collection<Position> CheckedPositions = new Collection<Position>();
        public AStar<Position> Solver { get; set; }
        public bool kill { get; set; }
        private readonly object _lock = new object();

        public void Run()
        {
            while (Solver.State == SolverState.Running && !kill)
            {
                    Solver.Tick();
                    lock (_lock)
                    {
                        CheckedPositions.Add(Solver.Current);
                    }
            }
        }

        public IReadOnlyCollection<Position> GetCheckedPositions()
        {
            lock (_lock)
            {
                try
                {
                    return CheckedPositions;
                }
                finally
                {
                    CheckedPositions = new Collection<Position>();
                }
            }
        }
    }
}