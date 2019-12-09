using System.Collections.Generic;
using System.Collections.ObjectModel;
using PathFinder.Interfaces;
using PathFinder.Solvers;
using SimpleWorld.Map;

namespace PathFinderGui
{

    public interface ISolverRunner
    {
        AStar<Position> Solver { get; set; }
        bool Kill { get; set; }
        void Run();
        IReadOnlyCollection<Position> GetCheckedPositions();
    }
    
    public class SolverRunnerThread : ISolverRunner
    {
        private Collection<Position> _checkedPositions = new Collection<Position>();
        public AStar<Position> Solver { get; set; }
        public bool Kill { get; set; }
        private readonly object _lock = new object();

        public void Run()
        {
            while (Solver.State == SolverState.Running && !Kill)
            {
                    Solver.Tick();
                    lock (_lock)
                    {
                        _checkedPositions.Add(Solver.Current);
                    }
            }
        }

        public IReadOnlyCollection<Position> GetCheckedPositions()
        {
            lock (_lock)
            {
                try
                {
                    return _checkedPositions;
                }
                finally
                {
                    _checkedPositions = new Collection<Position>();
                }
            }
        }
    }
}