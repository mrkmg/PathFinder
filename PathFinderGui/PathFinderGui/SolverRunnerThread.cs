using System.Collections.Generic;
using System.Threading;
using PathFinder.Solvers;
using SimpleWorld.Map;

namespace PathFinderGui
{
    public class SolverRunnerThread
    {
        private LinkedList<Position> _checkedPositions = new LinkedList<Position>();
        public ISolver<Position> Solver { get; set; }
        public int Delay { get; set; }
        private readonly object _lock = new object();
        private bool _kill;
        public bool Running => _thread != null;
        public bool RunToSolve { get; set; }
        private Thread _thread;

        public void Kill()
        {
            _kill = true;
        }

        public void Start()
        {
            if (Running) return;
            _kill = false;
            _thread = new Thread(Main);
            _thread.Start();
        }

        private void Main()
        {
            while (!_kill)
            {
                if (Solver.State != SolverState.Waiting)
                    break;
                
                lock (_lock)
                {
                    if (Delay <= 0 || _checkedPositions.Count < Delay)
                    {
                        if (RunToSolve)
                            Solver.Start();
                        else
                        {
                            Solver.Start(1);
                            _checkedPositions.AddLast(Solver.Current);
                        }
                        continue;
                    }
                }

                Thread.Sleep(5);
            }
            _thread = null;
        }

        public (IReadOnlyCollection<Position>, IList<Position>)  GetFrameData()
        {
            lock (_lock)
            {
                var ret = (_checkedPositions, Solver.CurrentBestPath);
                _checkedPositions = new LinkedList<Position>();
                return ret;
            }
        }
    }
}