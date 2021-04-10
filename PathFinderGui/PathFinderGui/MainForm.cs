using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Timers;
using Eto.Drawing;
using Eto.Forms;
using PathFinder.Solvers;
using PathFinder.Solvers.Generic;
using SimpleWorld.Map;

namespace PathFinderGui
{
    public partial class MainForm
    {
        private SolverRunnerThread _runnerThread;
        private World _world;
        private Position _startPoint;
        private Position _endPoint;
        private double _lastTpf;
        private double _lastTps;
        private double _lastFps;
        private Stopwatch _frameStopwatch;
        private Stopwatch _overallStopwatch;
        private IGraphSolver<Position> _graphSolver;
        private IList<Position> _lastBestPath;

        private UiEventDebouncer<EventArgs> _scaleSelectorChangedDebounce;
        private UiEventDebouncer<EventArgs> _moveCostSelectorChangedDebounce;

        public MainForm()
        {
            WindowState = WindowState.Maximized;
            InitUi();
            Thread.Sleep(1);

            _timer = new UITimer {Interval = 1d / 20};
            _timer.Elapsed += (sender, args) => ProcessFrame();
            
            _scaleSelectorChangedDebounce = new UiEventDebouncer<EventArgs>(250);
            _moveCostSelectorChangedDebounce = new UiEventDebouncer<EventArgs>(250);
            
            Load += (sender, args) => Content.Height = Height;
            Closed += (sender, args) => KillRunning();
            KeyUp += CheckKeyupForExit;

            _solverSelector.DataStore = new []{"AStar", "Greedy", "Breadth First"};
            _solverSelector.Text = "AStar";
            _solverSelector.ReadOnly = true;
            
            _bitmapWidget.BackgroundColor = Colors.Black;
            _bitmapWidget.IsReady += OnBitmapWidgetIsReady;
            _bitmapWidget.MouseUp += OnBitmapWidgetMouseUp;
            
            _solverSelector.TextChanged += OnSolverSelectorChanged;
            
            _newWorld.Click += OnNewWorldClick;
            _newPoints.Click += OnNewPointsClick;
            _go.Click += OnGoClick;
            _stopButton.Click += OnStopButtonClick;
            _resetSolverButton.Click += OnResetButtonClick;
            
            _delayStepper.ValueChanged += OnDelayStepperChanged;
            _greedStepper.ValueChanged += OnGreedSelectorChanged;
            _scaleStepper.ValueChanged += _scaleSelectorChangedDebounce.Handle;
            _moveCostStepper.ValueChanged += _moveCostSelectorChangedDebounce.Handle;
            
            _scaleSelectorChangedDebounce.Fired += OnScaleSelectorChanged;
            _moveCostSelectorChangedDebounce.Fired += OnMoveCostSelectorChanged;
            _showSearchCheckbox.CheckedChanged += OnShowSearchCheckboxChanged;
            

            Closed += OnClosed;
        }

        private bool SetRandomPoints()
        {
            if (_world == null) return false;
            
            var rnd = new Random(int.Parse(_pointsSeed.Text));
            var worldSize = Math.Sqrt(_world.XSize * _world.XSize + _world.YSize * _world.YSize);
            var targetSize = (int)(worldSize * 0.75);
                
            ClearMarkers();
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
            return true;
        }

        private void Reset()
        {
            KillRunning();
            DrawEntireWorld();
        }

        private void KillRunning()
        {
            _lastBestPath = null;
            _graphSolver = null;
            
            if (_timer != null)
            {
                _timer.Stop();
            }

            if (_runnerThread != null)
            {
                _runnerThread.Kill();
                _runnerThread = null;
            }
        }

        private ICollection<DrawPoint> _cachedEntireMap;

        private void DrawEntireWorld()
        {
            if (_world == null) return;
            _bitmapWidget.Clear();
            if (_cachedEntireMap == null)
                _cachedEntireMap = _world.GetAllNodes().AsMapDrawPoints().ToArray();
            _bitmapWidget.DrawAll(_cachedEntireMap);
            DrawMarkers();
        }

        private void ClearMarkers()
        {
            if (_startPoint != null)
                _bitmapWidget.DrawAll(
                    _startPoint
                        .ToMarkerPoints(8)
                        .Select(xy => _world.GetPosition(xy.x, xy.y))
                        .Where(p => p != null)
                        .AsMapDrawPoints()
                    );
            if (_endPoint != null)
                _bitmapWidget.DrawAll(
                    _endPoint
                        .ToMarkerPoints(8)
                        .Select(xy => _world.GetPosition(xy.x, xy.y))
                        .Where(p => p != null)
                        .AsMapDrawPoints()
                    );
        }

        private void DrawMarkers()
        {
            _bitmapWidget.DrawAll(
                _startPoint
                    .ToMarkerPoints(8)
                    .Where(xy => xy.x > 0 && xy.x < _bitmapWidget.BitmapWidth && xy.y > 0 && xy.y < _bitmapWidget.BitmapHeight)
                    .Select(xy => xy.AsMarkerDrawPoint()
                    )
                );
            _bitmapWidget.DrawAll(
                _endPoint
                    .ToMarkerPoints(8)
                    .Where(xy => xy.x > 0 && xy.x < _bitmapWidget.BitmapWidth && xy.y > 0 && xy.y < _bitmapWidget.BitmapHeight)
                    .Select(xy => xy.AsMarkerDrawPoint()
                    )
                );
        }

        private void MakeWorld()
        {
            KillRunning();
            if (_bitmapWidget.BitmapHeight == 0 || _bitmapWidget.BitmapWidth == 0) return;
            _world = new World(_bitmapWidget.BitmapWidth, _bitmapWidget.BitmapHeight, new Random(int.Parse(_worldSeed.Text)), _moveCostStepper.Value);
            _cachedEntireMap = null;
            SetRandomPoints();
            DrawEntireWorld();
        }

        private void Go()
        {
            if (_world == null) return;
            if (_graphSolver == null)
            {
                Reset();
                _world.CanCutCorner = _canCornerCut.Checked ?? false;
                _world.CornerIsFree = _showSearchCheckbox.Checked ?? false;

                switch (_solverSelector.Text)
                {
                    case "AStar":
                        _graphSolver = new AStar<Position>(_startPoint, _endPoint, _greedStepper.Value);
                        break;
                    case "Greedy":
                        _graphSolver = new Greedy<Position>(_startPoint, _endPoint);
                        break;
                    case "Breadth First":
                        _graphSolver = new BreadthFirst<Position>(_startPoint, _endPoint);
                        break;
                    default:
                        throw new ArgumentException("Unknown GraphSolver");
                }

                _frameStopwatch = new Stopwatch();
                _overallStopwatch = new Stopwatch();
                _runnerThread = new SolverRunnerThread {GraphSolver = _graphSolver, Delay = (int) _delayStepper.Value};
                _lastFps = 0;
                _lastTpf = 0;
                _lastTps = 0;
            }

            _runnerThread.RunToSolve = !(_showSearchCheckbox.Checked ?? false);
            _frameStopwatch.Start();
            _overallStopwatch.Start();
            _runnerThread.Start();
            _timer.Start();
        }

        private void ProcessFrame()
        {
            switch (_graphSolver.State)
            {
                case SolverState.Waiting:
                case SolverState.Running:
                    var frameTime = _frameStopwatch.Elapsed.TotalSeconds;
                    _lastFps = (1 / frameTime) * 0.1d + _lastFps * 0.9d;
                    _lastTps = _graphSolver.ClosedCount / _overallStopwatch.Elapsed.TotalSeconds;
                    _tpf.Text = _graphSolver.State == SolverState.Waiting ? "Waiting" : $"TPF: {_lastTpf:N0}";
                    _tps.Text = $"TPS: {_lastTps:N0}";
                    _fps.Text = $"FPS: {_lastFps:N0}";
                    _openPoints.Text = $"Open Points: {_graphSolver.OpenCount:N0}";
                    _closedPoints.Text = $"Closed Points: {_graphSolver.ClosedCount:N0}";
                    
                    // Break early if not doing any retrieval from the runner thread data
                    if (_showSearchCheckbox.Checked == null || !_showSearchCheckbox.Checked.Value) break;
                    
                    ProcessThreadFrameData();
                    break;
                case SolverState.Success:
                    _frameStopwatch.Stop();
                    _overallStopwatch.Stop();
                    _tpf.Text = "Path Found";
                    _fps.Text = $"Time: {_overallStopwatch.Elapsed.TotalSeconds:N3}";
                    _tps.Text = $"TPS: {_graphSolver.ClosedCount / _overallStopwatch.Elapsed.TotalSeconds:N2} ({_graphSolver.ClosedCount:N0})";
                    _openPoints.Text = $"Path Length {_runnerThread.GraphSolver.Path.Count:N0}";
                    _closedPoints.Text = $"Path Cost: {_graphSolver.PathCost:N2}";
                    DrawEntireWorld();
                    _bitmapWidget.DrawAll(_runnerThread.GraphSolver.Path.AsPathDrawPoints());
                    KillRunning();
                    break;
                case SolverState.Failure:
                    _tpf.Text = $"Failed to find a path";
                    _fps.Text = $"Time: {_overallStopwatch.Elapsed.TotalSeconds:N3}";
                    _tps.Text = $"TPS: {_graphSolver.ClosedCount / _overallStopwatch.Elapsed.TotalSeconds:N2}";
                    _openPoints.Text = $"Open Points: {_graphSolver.OpenCount:N0}";
                    _closedPoints.Text = $"Closed Points: {_graphSolver.ClosedCount:N0}";
                    KillRunning();
                    break;
                default:
                    KillRunning();
                    throw new ArgumentOutOfRangeException();
            }
            _frameStopwatch.Restart();
        }

        private void ProcessThreadFrameData()
        {
            var frameTime = _frameStopwatch.Elapsed.TotalSeconds;
            var (recentlyCheckedPoints, currentBestPath) = _runnerThread.GetFrameData();
            _lastTpf = recentlyCheckedPoints.Count * 0.1d + _lastTpf * 0.9d;
            _bitmapWidget.DrawAll(recentlyCheckedPoints.AsSearchDrawPoints());
            _bitmapWidget.DrawAll(_lastBestPath != null
                ? BestPathDiff(currentBestPath)
                : currentBestPath.AsPathDrawPoints());
            DrawMarkers();
            _lastBestPath = currentBestPath;
        }

        private IEnumerable<DrawPoint> BestPathDiff(IEnumerable<Position> newBestPath)
        {
            var i = -1;
            foreach (var position in newBestPath)
            {
                i++;
                
                if (i >= _lastBestPath.Count)
                {
                    yield return position.AsPathDrawPoint();
                    continue;
                }
                
                if (_lastBestPath[i].Equals(position))
                {
                    continue;
                }

                yield return _lastBestPath[i].AsSearchDrawPoint();
                yield return position.AsPathDrawPoint();
            }
            while (++i < _lastBestPath.Count)
                yield return _lastBestPath[i].AsSearchDrawPoint();
        }

        #region EventHandlers

        private void OnShowSearchCheckboxChanged(object sender, EventArgs e)
        {
            if (_runnerThread != null)
                _runnerThread.RunToSolve = !(_showSearchCheckbox.Checked ?? false);
        }

        private void OnResetButtonClick(object sender, EventArgs e)
        {
            if (_graphSolver is AStar<Position> solver)
                solver.Reset();
        }

        private void OnStopButtonClick(object sender, EventArgs e)
        {
            _graphSolver?.Stop();
            if (_runnerThread != null)
            {
                _runnerThread.Kill();
                ProcessThreadFrameData();
            }
            _timer.Stop();
            _frameStopwatch.Stop();
            _overallStopwatch.Stop();
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

        private void OnBitmapWidgetIsReady(object sender, EventArgs args) => MakeWorld();

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
            _bitmapWidget.ChangeScale((int)_scaleStepper.Value);
        }

        private void OnDelayStepperChanged(object sender, EventArgs args)
        {
            if (_runnerThread != null) _runnerThread.Delay = (int) _delayStepper.Value;
        }

        private void OnGreedSelectorChanged(object sender, EventArgs args)
        {
            if (_graphSolver != null && _graphSolver is AStar<Position> solver)
                solver.GreedFactor = _greedStepper.Value;
        }

        private void OnGoClick(object sender, EventArgs args) => Go();

        private void OnNewPointsClick(object sender, EventArgs args)
        {
            KillRunning();
            _pointsSeed.Text = (new Random()).Next(10000, 99999).ToString();
            if (!SetRandomPoints())
            {
                _world = null;
            }
            else
            {
                DrawEntireWorld();
            }
        }

        private void OnNewWorldClick(object sender, EventArgs args)
        {
            _worldSeed.Text = (new Random()).Next(10000, 99999).ToString();
            MakeWorld();
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
                ClearMarkers();
                _startPoint = position;
                DrawMarkers();
            }
            else
            {
                ClearMarkers();
                _endPoint = position;
                DrawMarkers();
            }

            if (_runnerThread != null)
            {
                Reset();
            }
        }

        #endregion
    }
    
    internal static class MainFormExtensions
    {
        internal static Color GetColorForLevel(int level)
        {
            const int redStart = 0;
            const int redDiff = 0;
            const int greenStart = 200;
            const int greenDiff = -180;
            const int blueStart = 0;
            const int blueDiff = 0;
            var levelAmount = (level - 1) / 4d;

            return Color.FromArgb(redStart + (int)(levelAmount * redDiff), greenStart + (int)(levelAmount * greenDiff), blueStart + (int)(levelAmount * blueDiff));
        }
        
        internal static IEnumerable<DrawPoint> AsMapDrawPoints(this IEnumerable<Position> positions)
        {
            return positions.Select(AsMapDrawPoint);
        }

        internal static IEnumerable<DrawPoint> AsSearchDrawPoints(this IEnumerable<Position> positions)
        {
            return positions.Where(p => p != null).Select(AsSearchDrawPoint);
        }
        
        internal static IEnumerable<DrawPoint> AsPathDrawPoints(this IEnumerable<Position> positions)
        {
            return positions.Where(p => p != null).Select(AsPathDrawPoint);
        }

        internal static DrawPoint AsPathDrawPoint(this Position p)
        {
            return new DrawPoint {X = p.X, Y = p.Y, Color = Color.Blend(GetColorForLevel(p.Z), Color.FromArgb(255, 0, 0, 200))};
        }

        internal static DrawPoint AsSearchDrawPoint(this Position p)
        {
            return new DrawPoint {X = p.X, Y = p.Y, Color = Color.Blend(GetColorForLevel(p.Z), Color.FromArgb(0, 0, 255, 80))};
        }

        internal static DrawPoint AsMapDrawPoint(this Position p)
        {
            return new DrawPoint {Color = GetColorForLevel(p.Z), X = p.X, Y = p.Y};
        }

        internal static IEnumerable<(int x, int y)> ToMarkerPoints(this Position position, int size)
        {
            var wx = position.World.XSize;
            var wy = position.World.YSize;
            for (var i = 0; i <= size; i++)
            {
                
                yield return (position.X + i, position.Y + i);
                yield return (position.X + i, position.Y - i);
                yield return (position.X - i, position.Y + i);
                yield return (position.X - i, position.Y - i);
            }
        }

        internal static DrawPoint AsMarkerDrawPoint(this (int x, int y) xy)
        {
            var (x, y) = xy;
            return new DrawPoint {X = x, Y = y, Color = Colors.Red};
        }
    }

    public class UiEventDebouncer<T> : IDisposable where T : class
    {
        public event EventHandler<T> Fired;
        private readonly System.Timers.Timer _timer;
        private T _lastArgs;
        private object _lastSender;
        
        public UiEventDebouncer(double timeMs)
        {
            _timer = new System.Timers.Timer(timeMs)
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