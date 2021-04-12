using System;
using System.Timers;
using Eto.Drawing;
using Eto.Forms;
using PathFinder.Solvers;
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
        private FrameData _lastFrameData;

        private readonly UiEventDebouncer<EventArgs> _scaleSelectorChangedDebounce;
        private readonly UiEventDebouncer<EventArgs> _moveCostSelectorChangedDebounce;

        public MainForm()
        {
            WindowState = WindowState.Maximized;
            InitUi();

            _timer = new UITimer {Interval = 1d / 20};
            _timer.Elapsed += (_, _) => ProcessFrame();
            
            _scaleSelectorChangedDebounce = new UiEventDebouncer<EventArgs>(250);
            _moveCostSelectorChangedDebounce = new UiEventDebouncer<EventArgs>(250);
            
            Load += (_, _) => Content.Height = Height;
            Closed += (_, _) => KillRunning();
            KeyUp += CheckKeyupForExit;

            _solverSelector.DataStore = new []{"AStar", "Greedy", "Breadth First"};
            _solverSelector.Text = "AStar";
            _solverSelector.ReadOnly = true;
            
            _mapWidget.BackgroundColor = Colors.Black;
            _mapWidget.IsReady += OnMapWidgetIsReady;
            _mapWidget.MouseUp += OnBitmapWidgetMouseUp;
            
            _solverSelector.TextChanged += OnSolverSelectorChanged;
            
            _newWorld.Click += OnNewWorldClick;
            _newPoints.Click += OnNewPointsClick;
            _go.Click += OnGoClick;
            _pauseButton.Click += OnPauseButtonClick;
            _resetSolverButton.Click += OnResetButtonClick;
            
            _delayStepper.ValueChanged += OnDelayStepperChanged;
            _greedStepper.ValueChanged += OnGreedSelectorChanged;
            _scaleStepper.ValueChanged += _scaleSelectorChangedDebounce.Handle;
            _moveCostStepper.ValueChanged += _moveCostSelectorChangedDebounce.Handle;
            
            _worldSeed.KeyUp += OnWorldSeedChanged;
            _pointsSeed.KeyUp += OnPointsSeedChanged;
            
            _scaleSelectorChangedDebounce.Fired += OnScaleSelectorChanged;
            _moveCostSelectorChangedDebounce.Fired += OnMoveCostSelectorChanged;
            
            _showSearchCheckbox.CheckedChanged += OnShowSearchCheckboxChanged;
            
            Closed += OnClosed;
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

                if (tries > 100000) break;
                    
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
                _world.CornerIsFree = _showSearchCheckbox.Checked ?? false;

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

        #region EventHandlers

        private void OnPointsSeedChanged(object sender, KeyEventArgs e)
        {
            if (e.Key != Keys.Enter && e.Key != Keys.Tab) return;
            if (!int.TryParse(_pointsSeed.Text, out _)) return;
            KillRunning();
            _mapWidget.ClearMarkers(_startPoint, _endPoint);
            SetRandomPoints();
            _mapWidget.DrawMarkers(_startPoint, _endPoint);
        }

        private void OnWorldSeedChanged(object sender, KeyEventArgs e)
        {
            if (e.Key != Keys.Enter && e.Key != Keys.Tab) return;
            if (!int.TryParse(_worldSeed.Text, out _)) return;
            _mapWidget.Clear();
            Application.Instance.RunIteration();
            MakeWorld();
        }

        private void OnShowSearchCheckboxChanged(object sender, EventArgs e)
        {
            if (_runnerThread != null)
                _runnerThread.RunToSolve = !(_showSearchCheckbox.Checked ?? false);
        }

        private void OnResetButtonClick(object sender, EventArgs e)
        {
            if (_runnerThread is {GraphSolver: AStar<Position> solver})
                solver.Reset();
        }

        private void OnPauseButtonClick(object sender, EventArgs e)
        {
            if (_runnerThread is {GraphSolver: AStar<Position> solver})
            {
                solver.Stop();
                _runnerThread.Kill();
                _mapWidget.DrawRunning(_lastFrameData);
            }
            _timer.Stop();
        }

        private void OnClosed(object sender, EventArgs args)
        {
            KillRunning();
            _timer.Dispose();
            _moveCostSelectorChangedDebounce.Dispose();
            _scaleSelectorChangedDebounce.Dispose();
        }

        private void OnSolverSelectorChanged(object sender, EventArgs e)
        {
            switch (_solverSelector.Text)
            {
                case "AStar":
                    _greedStepper.Enabled = true;
                    _resetSolverButton.Enabled = true;
                    break;
                case "Greedy":
                case "Breadth First":
                    _greedStepper.Enabled = false;
                    _resetSolverButton.Enabled = false;
                    break;
            }
        }

        private void OnMapWidgetIsReady(object sender, EventArgs args) => MakeWorld();

        private void OnMoveCostSelectorChanged(object sender, EventArgs args)
        {
            if (_world == null) return;

            KillRunning();
            _world.MoveCost = _moveCostStepper.Value;
            Reset();
        }

        private void OnScaleSelectorChanged(object sender, EventArgs args)
        {
            KillRunning();
            _mapWidget.ChangeScale((int)_scaleStepper.Value);
        }

        private void OnDelayStepperChanged(object sender, EventArgs args)
        {
            if (_runnerThread != null) _runnerThread.Delay = (int) _delayStepper.Value;
        }

        private void OnGreedSelectorChanged(object sender, EventArgs args)
        {
            if (_runnerThread is {GraphSolver: AStar<Position> solver})
                solver.GreedFactor = _greedStepper.Value;
        }

        private void OnGoClick(object sender, EventArgs args) => Go();

        private void OnNewPointsClick(object sender, EventArgs args)
        {
            _pointsSeed.Text = new Random().Next(10000, 99999).ToString();
            KillRunning();
            _mapWidget.ClearMarkers(_startPoint, _endPoint);
            SetRandomPoints();
            _mapWidget.DrawMarkers(_startPoint, _endPoint);
        }

        private void OnNewWorldClick(object sender, EventArgs args)
        {
            _worldSeed.Text = new Random().Next(10000, 99999).ToString();
            _mapWidget.Clear();
            Application.Instance.RunIteration();
            _mapWidget.ClearMarkers(_startPoint, _endPoint);
            MakeWorld();
            _mapWidget.DrawMarkers(_startPoint, _endPoint);
        }

        private void CheckKeyupForExit(object sender, KeyEventArgs args)
        {
            if (args.Key == Keys.Q) Close();
        }

        private void OnBitmapWidgetMouseUp(object sender, BitmapMouseEventArgs args)
        {
            var position = _world?.GetPosition(args.MapLocation.X, args.MapLocation.Y);
            if (position == null) return;

            if (args.Buttons == MouseButtons.Primary)
            {
                _mapWidget.ClearMarkers(_startPoint, _endPoint);
                _startPoint = position;
                _mapWidget.DrawMarkers(_startPoint, _endPoint);
            }
            else
            {
                _mapWidget.ClearMarkers(_startPoint, _endPoint);
                _endPoint = position;
                _mapWidget.DrawMarkers(_startPoint, _endPoint);
            }

            if (_runnerThread != null)
            {
                Reset();
            }
        }

        #endregion
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