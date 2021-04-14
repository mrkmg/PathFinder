using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;
using PathFinder.Solvers.Generic;
using SimpleWorld.Map;

namespace PathFinderGui
{
    public class SolverRunnerThread
    {
        private IList<Position> _checkedPositions = new List<Position>();
        public IGraphSolver<Position> GraphSolver { get; set; }
        public int Delay { get; set; }
        private readonly object _lock = new ();
        private bool _kill;
        public bool Running => _thread != null;
        public bool RunToSolve { get; set; }
        private Thread _thread;
        private readonly Stopwatch _frameStopwatch = new ();
        private readonly Stopwatch _overallStopwatch = new ();

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
            _frameStopwatch.Start();
        }

        private void Main()
        {
            while (!_kill)
            {
                if (GraphSolver.State != SolverState.Waiting)
                    break;
                lock (_lock)
                {
                    if (Delay <= 0 || _checkedPositions.Count < Delay)
                    {
                        _overallStopwatch.Start();
                        if (RunToSolve)
                            GraphSolver.Start();
                        else
                        {
                            GraphSolver.Start(1);
                            _checkedPositions.Add(GraphSolver.Current);
                        }
                        _overallStopwatch.Stop();
                        continue;
                    }
                }

                Thread.Sleep(5);
            }
            _frameStopwatch.Stop();
            _thread = null;
        }

        public FrameData GetFrameData()
        {
            lock (_lock)
            {
                var frameData = new FrameData(
                    GraphSolver, 
                    _checkedPositions, 
                    _frameStopwatch.Elapsed.TotalSeconds, 
                    _overallStopwatch.Elapsed.TotalSeconds
                );
                _checkedPositions = new List<Position>(_checkedPositions.Count);
                _frameStopwatch.Restart();
                return frameData;
            }
        }
    }

    public readonly struct FrameData
    {
        public readonly ICollection<Position> CheckedPositions;
        public readonly SolverState State;
        public readonly int OpenCount;
        public readonly int ClosedCount;
        public readonly IList<Position> CurrentBestPath;
        [CanBeNull] public readonly IList<Position> Path;
        public readonly double PathCost;
        public readonly double FrameSeconds;
        public readonly double OverallSeconds;

        public FrameData(IGraphSolver<Position> solver, ICollection<Position> checkedPositions, double frameSeconds,
            double overallSeconds)
        {
            CheckedPositions = checkedPositions;
            State = solver.State;
            OpenCount = solver.OpenCount;
            ClosedCount = solver.ClosedCount;
            CurrentBestPath = solver.CurrentBestPath;
            Path = solver.Path;
            PathCost = solver.PathCost;
            FrameSeconds = frameSeconds;
            OverallSeconds = overallSeconds;
        }
    }
}