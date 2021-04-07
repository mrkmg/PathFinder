using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using PathFinder.Interfaces;
using PathFinder.Solvers;
using SimpleWorld.Map;

namespace PathFinderGui
{
    public partial class MainForm
    {
        private ISolverRunner _runnerThread;
        private World _world;
        private Position _randomStartPoint;
        private Position _randomEndPoint;
        private double _lastTpf;
        private double _lastTps;
        private double _lastFps;
        private Stopwatch _frameStopwatch;
        private Stopwatch _overallStopwatch;
        private AStar<Position> _solver;
        private IList<Position> _lastBestPath = null;

        private const int MarkerSize = 10;

        public MainForm()
        {
            InitUi();
            
            Load += (sender, args) => Content.Height = Height;
            SizeChanged += (sender, args) =>
            {
                if (Height == 0 || Width == 0) return;
                KillRunning();
                Content.Height = Height;
            };
            
            Closed += (sender, args) => KillRunning();
            KeyUp += (sender, args) =>
            {
                if (args.Key == Keys.Q) Close();
            };
            _newWorld.Click += (sender, args) =>
            {
                _worldSeed.Text = (new Random()).Next(10000, 99999).ToString();
                MakeWorld();
            };
            _newPoints.Click += (sender, args) =>
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
            };
            _go.Click += (sender, args) => Go();
            _thoroughnessSlider.ValueChanged += (sender, args) =>
            {
                KillRunning();
                _thoroughnessSlider.ToolTip = $"Throroughness ({_thoroughnessSlider.Value}%)";
                Reset();
            };
            _delaySlider.ValueChanged += (sender, args) =>
            {
                if (_runnerThread != null) _runnerThread.Delay = (int)_delaySlider.Value;
            };
            _scaleSlider.ValueChanged += (sender, args) =>
            {
                KillRunning();
                _map.ChangeScale(_scaleSlider.Value);
                MakeWorld();
            };
            _moveCost.ValueChanged += (sender, args) =>
            {
                if (_world == null)
                    return;
                
                KillRunning();
                _world.MoveCost = _moveCost.Value;
                Reset();
            };

            _map.IsReady += (sender, args) => MakeWorld();
        }
        
        private bool SetRandomPoints()
        {
            if (_world == null) return false;
            
            var rnd = new Random(int.Parse(_pointsSeed.Text));
            var worldSize = Math.Sqrt(_world.XSize * _world.XSize + _world.YSize * _world.YSize);
            var targetSize = (int)(worldSize * 0.75);
                
            Position randomFromNode = null;
            Position randomToNode = null;

            IList<Position> path = null;

            var tries = 0;
            while (path == null)
            {
                tries++;

                if (tries > 100000) break;
                    
                do randomFromNode = _world.GetNode(rnd.Next(0, _world.XSize - 1), rnd.Next(0, _world.YSize - 1));
                while (randomFromNode == null);
                    
                do randomToNode = _world.GetNode(rnd.Next(0, _world.XSize - 1), rnd.Next(0, _world.YSize - 1));
                while (randomToNode == null);

                var x = Math.Abs(randomFromNode.X - randomToNode.X);
                var y = Math.Abs(randomFromNode.Y - randomToNode.Y);
                var dist = Math.Sqrt(x*x + y*y);
                    
                if (dist < targetSize) continue;
                path = AStar.Solve(randomFromNode, randomToNode, 0);
            }

            if (path == null) return false;

            _randomStartPoint = randomFromNode;
            _randomEndPoint = randomToNode;
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
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }

            if (_runnerThread != null)
            {
                _runnerThread = null;
            }
        }

        private void DrawEntireWorld()
        {
            if (_world == null) return;
            _map.Clear();
            _map.DrawAll(_world.GetAllNodes().AsMapDrawPoints());
            _map.DrawMarker(_randomStartPoint.X, _randomStartPoint.Y, MarkerSize, Colors.Red);
            _map.DrawMarker(_randomEndPoint.X, _randomEndPoint.Y, MarkerSize, Colors.Red);
        }

        private void MakeWorld()
        {
            KillRunning();
            if (_map.MapHeight == 0 || _map.MapWidth == 0) return;
            _world = new World(_map.MapWidth, _map.MapHeight, new Random(int.Parse(_worldSeed.Text)), _moveCost.Value);
            if (SetRandomPoints())
            {
                DrawEntireWorld();
            }
            else
            {
                _world = null;
            }
        }

        private void Go()
        {
            if (_world == null) return;
            
            KillRunning();
            _world.CanCutCorner = _canCornerCut.Checked ?? false;
            _world.CanRun = _canRun.Checked ?? false;
            
            var t = (double) _thoroughnessSlider.Value / 100;

            _solver = new AStar<Position>(_randomStartPoint, _randomEndPoint, t);

            _frameStopwatch = new Stopwatch();
            _overallStopwatch = new Stopwatch();
            _frameStopwatch.Start();
            _overallStopwatch.Start();
            _lastFps = 0;
            _lastTpf = 0;
            _lastTps = 0;
            
            _runnerThread = new SolverRunnerThread { Solver = _solver, Delay = (int)_delaySlider.Value};
            var solverThread = new Thread(_runnerThread.Run);
            solverThread.Start();
            
            _timer = new UITimer { Interval = 1d / 30 };
            _timer.Elapsed += (sender, args) => ProcessTick();
            _timer.Start();
        }

        private void ProcessTick()
        {
            var frameTime = _frameStopwatch.Elapsed.TotalSeconds;
            _frameStopwatch.Restart();
            var (checkPoints, bestPath) = _runnerThread.GetFrameData();
            _lastFps = (1 / frameTime) * 0.1d + _lastFps * 0.9d;
            _lastTpf = checkPoints.Count * 0.1d + _lastTpf * 0.9d;
            _lastTps = (checkPoints.Count / frameTime) * 0.1d + _lastTps * 0.9d;
            _tpf.Text = $"TPF: {_lastTpf:N0}";
            _tps.Text = $"TPS: {_lastTps:N0}";
            _fps.Text = $"FPS: {_lastFps:N0}";
            _openPoints.Text = $"Open Points: {_solver.OpenCount:N0}";
            _closedPoints.Text = $"Closed Points: {_solver.ClosedCount:N0}";
            if (checkPoints.Any())
                _map.DrawAll(checkPoints.AsSearchDrawPoints());
            
            if (_lastBestPath != null) 
                _map.DrawAll(_lastBestPath.AsSearchDrawPoints());
            _lastBestPath = bestPath;
            _map.DrawAll(_lastBestPath.AsCurrentPathDrawPoints());
            
            if (_runnerThread.Solver.State != SolverState.Success) return;

            _lastBestPath = null;
            var totalSeconds = _overallStopwatch.Elapsed.TotalSeconds;
            _frameStopwatch.Stop();
            _overallStopwatch.Stop();
            _tps.Text = $"O.TPS: {_solver.ClosedCount / totalSeconds:N2}";
            _openPoints.Text = $"Path Length {_runnerThread.Solver.Path.Count:N0}";
            _pathCost.Text = $"Path Cost: {_solver.Cost:N0}";
            DrawEntireWorld();
            _map.DrawAll(_runnerThread.Solver.Path.AsCurrentPathDrawPoints());
            KillRunning();
            _solver = null;
        }
    }
    
    internal static class Ext
    {
        private static Color GetColorForLevel(int level)
        {
            const int redStart = 0;
            const int redDiff = 0;
            const int greenStart = 200;
            const int greenDiff = -180;
            const int blueStart = 0;
            const int blueDiff = 0;
            var levelAmount = (level - 1) / 8d;

            return Color.FromArgb(redStart + (int)(levelAmount * redDiff), greenStart + (int)(levelAmount * greenDiff), blueStart + (int)(levelAmount * blueDiff));
            
        }

        private static Func<Position, DrawPoint> PositionToBlended(Color color)
        {
            return (p) => new DrawPoint {X = p.X, Y = p.Y, Color = Color.Blend(GetColorForLevel(p.Z), color)};
        }
        
        internal static IEnumerable<DrawPoint> AsMapDrawPoints(this IEnumerable<Position> positions)
        {
            return positions.Select(p => new DrawPoint {Color = GetColorForLevel(p.Z), X = p.X, Y = p.Y});
        }

        internal static IEnumerable<DrawPoint> AsSearchDrawPoints(this IEnumerable<Position> positions)
        {
            return positions.Where(p => p != null).Select(PositionToBlended(Color.FromArgb(0, 0, 255, 80)));
        }
        
        internal static IEnumerable<DrawPoint> AsCurrentPathDrawPoints(this IEnumerable<Position> positions)
        {
            return positions.Where(p => p != null).Select(PositionToBlended(Color.FromArgb(255, 0, 0)));
        }
    }
}