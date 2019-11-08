using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Interfaces;
using SimpleWorld.Map;

namespace PathFinderGui
{
    public class SolverRunnerThread
    {
        public ConcurrentBag<Position> CheckedPositions = new ConcurrentBag<Position>();
        public ISolver<Position> Solver;
        public bool kill;

        public void Run()
        {
            while (Solver.State == SolverState.Running && !kill)
            {
                Solver.Tick();
                CheckedPositions.Add(Solver.Current);
            }
        }

        public Position[] GetCheckedPositions()
        {
            var positions = CheckedPositions.ToArray();
            CheckedPositions.Clear();
            return positions;
        }
    }
}