using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using PathFinder.Interfaces;
using PathFinder.Solvers;
using SimpleWorld.Map;

namespace PathFinderGui
{

    public interface ISolverRunner
    {
        ISolver<Position> Solver { get; set; }
        int Delay { get; set; }
        void Kill();
        void Run();
        (IReadOnlyCollection<Position>, IList<Position>) GetFrameData();
    }
    
    public class SolverRunnerThread : ISolverRunner
    {
        private Collection<Position> _checkedPositions = new Collection<Position>();
        public ISolver<Position> Solver { get; set; }
        public int Delay { get; set; }
        private readonly object _lock = new object();
        private bool _kill;

        public void Kill()
        {
            _kill = true;
        }

        public void Run()
        {
            while (Solver.State == SolverState.Running && !_kill)
            {
                if (Delay <= 0 || _checkedPositions.Count < Delay)
                {
                    lock (_lock)
                    {
                        Solver.Tick();
                        _checkedPositions.Add(Solver.Current);
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
                
            }
        }

        public (IReadOnlyCollection<Position>, IList<Position>)  GetFrameData()
        {
            lock (_lock)
            {
                try
                {
                    return (_checkedPositions, Solver.CurrentBestPath);
                }
                finally
                {
                    _checkedPositions = new Collection<Position>();
                }
            }
        }
    }
}