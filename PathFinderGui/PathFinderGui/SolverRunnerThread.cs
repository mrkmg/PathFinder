using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using PathFinder.Interfaces;
using SimpleWorld.Map;

namespace PathFinderGui
{
    public class SolverRunnerThread
    {
        private Collection<Position> _checkedPositions = new Collection<Position>();
        public ISolver<Position> Solver { get; set; }
        public int Delay { get; set; }
        private readonly object _lock = new object();
        private bool _kill;
        public bool Running => _thread != null;
        private Thread _thread;

        public void Kill()
        {
            _kill = true;
        }

        public void Start()
        {
            if (Running) return;
            _thread = new Thread(Main);
            _thread.Start();
        }

        private void Main() {
            while (Solver.State == SolverState.Running && !_kill)
            {
                lock (_lock)
                {
                    if (Delay <= 0 || _checkedPositions.Count < Delay)
                    {
                        Solver.Tick();
                        _checkedPositions.Add(Solver.Current);
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
                _checkedPositions = new Collection<Position>();
                return ret;
            }
        }
    }
}