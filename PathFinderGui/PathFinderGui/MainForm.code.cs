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
            _newSeed.Click += (sender, args) =>
            {
                _map.ChangeScale(_scaleSlider.Value);
                MakeWorld();
            };
            _newWorld.Click += (sender, args) =>
            {
                _worldSeed.Text = (new Random()).Next(10000, 99999).ToString();
                _map.ChangeScale(_scaleSlider.Value);
                MakeWorld();
            };
            _newPoints.Click += (sender, args) =>
            {
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
        }
        
        private bool SetRandomPoints()
        {
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

        private void KillRunning()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }

            if (_runnerThread != null)
            {
                _runnerThread.Kill = true;
                _runnerThread = null;
            }
        }

        private void DrawEntireWorld()
        {
            _map.DrawAll(_world.GetAllNodes().AsMapDrawPoints());
            _map.DrawMarker(_randomStartPoint.X, _randomStartPoint.Y, MarkerSize, Colors.Blue);
            _map.DrawMarker(_randomEndPoint.X, _randomEndPoint.Y, MarkerSize, Colors.Blue);
        }

        private void MakeWorld()
        {
            KillRunning();
            if (_map.MapHeight == 0 || _map.MapWidth == 0) return;
            _world = new World(_map.MapWidth, _map.MapHeight, new Random(int.Parse(_worldSeed.Text)));
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
            
            var t = (double) _thoroughnessSlider.Value / 100;

            var solver = new AStar<Position>(_randomStartPoint, _randomEndPoint, t);

            _frameStopwatch = new Stopwatch();
            _overallStopwatch = new Stopwatch();
            _frameStopwatch.Start();
            _overallStopwatch.Start();
            _lastFps = 0;
            _lastTpf = 0;
            _lastTps = 0;
            
            _runnerThread = new SolverRunnerThread { Solver = solver };
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
            var checkPoints = _runnerThread.GetCheckedPositions();
            _lastFps = (1 / frameTime) * 0.1d + _lastFps * 0.9d;
            _lastTpf = checkPoints.Count * 0.1d + _lastTpf * 0.9d;
            _lastTps = (checkPoints.Count / frameTime) * 0.1d + _lastTps * 0.9d;
            _tpf.Text = $"TPF: {_lastTpf:0}";
            _tps.Text = $"TPS: {_lastTps:0}";
            _fps.Text = $"FPS: {_lastFps:0}";
            _openPoints.Text = $"Open Points: {_runnerThread.Solver.OpenCount}";
            _closedPoints.Text = $"Closed Points: {_runnerThread.Solver.ClosedCount}";
            _map.DrawAll(checkPoints.AsSearchDrawPoints());
            if (_runnerThread.Solver.State != SolverState.Success) return;
            
            var totalSeconds = _overallStopwatch.Elapsed.TotalSeconds;
            _frameStopwatch.Stop();
            _overallStopwatch.Stop();
            _tps.Text = $"O.TPS: {_runnerThread.Solver.ClosedCount / totalSeconds:0}";
            DrawEntireWorld();
            _map.DrawAll(_runnerThread.Solver.Path.AsPathDrawPoints());
            KillRunning();
            
        }
    }
    
    internal static class Ext
    {
        private static Color GetColorForLevel(int level)
        {
            const int redStart = 0;
            const int redDiff = 200;
            const int greenStart = 160;
            const int greenDiff = -130;
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
        
        internal static IEnumerable<DrawPoint> AsPathDrawPoints(this IEnumerable<Position> positions)
        {
            return positions.Where(p => p != null).Select(PositionToBlended(Color.FromArgb(0, 0, 255)));
        }
    }
}