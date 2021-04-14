using System;
using System.Timers;
using Eto.Drawing;
using Eto.Forms;
using PathFinder.Solvers.Generic;
using PathFinderGui.Widgets;
using SimpleWorld.Map;

namespace PathFinderGui
{
    /**
     * TODO - Separate into individual components
     *        MainForm - running the solver and delegating updates
     *        Map - drawing the map
     *        Options - all inputs
     **/
    public partial class MainForm
    {
        private SolverRunnerThread _runnerThread;
        private World _world;
        private Position _startPoint;
        private Position _endPoint;
        private readonly UITimer _timer = new () { Interval = 1d/20 };
        private FrameData _lastFrameData;

        public MainForm()
        {
            WindowState = WindowState.Maximized;
            
            InitUi();
            BindEvents();

            _timer.Elapsed += (_, _) => ProcessFrame();
        }

        private void SetRandomPoints()
        {
            if (_world == null) return;
            
            var rnd = new Random(int.Parse(_pointsSeed.Text));
            var worldSize = Math.Sqrt(_world.XSize * _world.XSize + _world.YSize * _world.YSize);
            var targetSize = (int)(worldSize * 0.75);
                
            _mapWidget.ClearMarkers(_startPoint, _endPoint);
            Position randomFromNode = null;
            Position randomToNode = null;

            var tries = 0;
            while (true)
            {
                tries++;

                if (tries > 5000) break;
                    
                do randomFromNode = _world.GetPosition(rnd.Next(0, _world.XSize - 1), rnd.Next(0, _world.YSize - 1));
                while (randomFromNode == null);
                    
                do randomToNode = _world.GetPosition(rnd.Next(0, _world.XSize - 1), rnd.Next(0, _world.YSize - 1));
                while (randomToNode == null);

                var x = Math.Abs(randomFromNode.X - randomToNode.X);
                var y = Math.Abs(randomFromNode.Y - randomToNode.Y);
                var dist = Math.Sqrt(x*x + y*y);
                    
                if (dist >= targetSize) break;
            }
            
            _startPoint = randomFromNode;
            _endPoint = randomToNode;
            _mapWidget.DrawMarkers(_startPoint, _endPoint);
        }

        private void Reset()
        {
            KillRunning();
            _mapWidget.ClearRunning();
            _mapWidget.ClearPath();
        }

        private void KillRunning()
        {
            _timer?.Stop();

            if (_runnerThread == null) return;
            _runnerThread.Kill();
            _runnerThread = null;
        }

        private void MakeWorld()
        {
            KillRunning();
            if (_mapWidget.BitmapHeight == 0 || _mapWidget.BitmapWidth == 0) return;
            _world = new World(_mapWidget.BitmapWidth, _mapWidget.BitmapHeight, new Random(int.Parse(_worldSeed.Text)), _moveCostStepper.Value);
            SetRandomPoints();
            _mapWidget.ClearPath();
            _mapWidget.ClearRunning();
            _mapWidget.DrawWorld(_world);
        }

        private void Go()
        {
            if (_world == null) return;
            if (_runnerThread == null)
            {
                Reset();
                _world.CanCutCorner = _canCornerCut.Checked ?? false;

                IGraphSolver<Position> graphSolver = _solverSelector.Text switch
                {
                    "AStar" => new AStar<Position>(_startPoint, _endPoint, _greedStepper.Value),
                    "Greedy" => new Greedy<Position>(_startPoint, _endPoint),
                    "Breadth First" => new BreadthFirst<Position>(_startPoint, _endPoint),
                    _ => throw new ArgumentException("Unknown GraphSolver")
                };

                _runnerThread = new SolverRunnerThread {GraphSolver = graphSolver, Delay = (int) _delayStepper.Value};
            }
            
            _runnerThread.RunToSolve = !(_showSearchCheckbox.Checked ?? false);
            _runnerThread.Start();
            _timer.Start();
        }

        private void ProcessFrame()
        {
            var frameData = _runnerThread.GetFrameData();
            switch (frameData.State)
            {
                case SolverState.Waiting:
                case SolverState.Running:
                    _statsWidget.UpdateRunningStats(frameData);
                    if (_showSearchCheckbox.Checked != null && _showSearchCheckbox.Checked.Value)
                        _mapWidget.DrawRunning(frameData);
                    break;
                case SolverState.Success:
                    _statsWidget.UpdateSuccessStats(frameData);
                    
                    _mapWidget.ClearRunning();
                    _mapWidget.DrawPath(frameData.Path);
                    KillRunning();
                    break;
                case SolverState.Failure:
                    
                    _statsWidget.UpdateFailureStats(frameData);
                    
                    KillRunning();
                    break;
                default:
                    KillRunning();
                    throw new ArgumentOutOfRangeException();
            }

            _lastFrameData = frameData;
        }

    }

    public class UiEventDebouncer<T> : IDisposable where T : class
    {
        public event EventHandler<T> Fired;
        private readonly Timer _timer;
        private T _lastArgs;
        private object _lastSender;
        
        public UiEventDebouncer(double timeMs)
        {
            _timer = new Timer(timeMs)
            {
                AutoReset = false,
                Enabled = false
            };
            _timer.Elapsed += TimerFired;
        }

        public void Handle(object sender, T args)
        {
            _lastSender = sender;
            _lastArgs = args;
            _timer.Stop();
            _timer.Start();
        }

        private void TimerFired(object sender, ElapsedEventArgs args) => Application.Instance.InvokeAsync(InvokeFire);

        private void InvokeFire()
        {
            Fired?.Invoke(_lastSender, _lastArgs);
            _lastSender = null;
            _lastArgs = null;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}