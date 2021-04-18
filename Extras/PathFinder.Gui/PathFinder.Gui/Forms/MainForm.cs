using System;
using Eto.Forms;
using PathFinder.Graphs;
using PathFinder.Solvers.Generic;
using SimpleWorld.Map;
using SimpleWorld.Traversers;

namespace PathFinder.Gui.Forms
{
    public partial class MainForm
    {
        private SolverRunnerThread _runnerThread;
        private World _world;
        private Position _startPoint;
        private Position _endPoint;
        private readonly UITimer _timer = new () { Interval = 1d / 60 };
        private FrameData _lastFrameData;

        private bool ShowSearching => !(_doBlindSearch.Checked ?? false) && (_showSearchCheckbox.Checked ?? false);
        private bool ShowBlindSearching => _doBlindSearch.Checked ?? false;

        public MainForm()
        {
            Title = "Path Finder Playground";
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
            Reset();
            if (_mapWidget.BitmapHeight == 0 || _mapWidget.BitmapWidth == 0) return;
            _world = new World(_mapWidget.BitmapWidth, _mapWidget.BitmapHeight, new Random(int.Parse(_worldSeed.Text)), new World.InitializationOptions
            {
                F1 = (double)_initF1.Value / 100 * 3d,
                L1 = (double)_initL1.Value / 100 * 3d + 1d,
                P1 = (double)_initP1.Value / 100,
                SX1 = (double)_initSX1.Value / 100 * 5d,
                SY1 = (double)_initSY1.Value / 100 * 5d,
                
                F2 = (double)_initF2.Value / 100 * 10d,
                L2 = (double)_initL2.Value / 100 * 3d + 1d,
                P2 = (double)_initP2.Value / 100,
                SX2 = (double)_initSX2.Value / 100 * 5d,
                SY2 = (double)_initSY2.Value / 100 * 5d,
                
                Ratio12 = (double)_initRatio12.Value / 100 * 4d - 2d
            } ,_moveCostStepper.Value);
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
                CreateNewRunner();
            }
            
            if (ShowBlindSearching) _mapWidget.ClearLayer(0);
            else _mapWidget.DrawWorld(_world);
            
            _runnerThread.RunToSolve = !(_showSearchCheckbox.Checked ?? false);
            _runnerThread.Start();
            _timer.Start();
        }
        
        private void CreateNewRunner()
        {
            Reset();

            INodeTraverser<Position> traverser = _traverserSelector.Text switch
            {
                "Default" => new DefaultTraverser<Position>(),
                "Grid" => new GridTraverser(),
                "LargeStep" => new LargerStepTraverser((int)_stepSizeStepper.Value),
                _ => throw new ArgumentException("Unknown traverser")
            };

            IGraphSolver<Position> graphSolver = _solverSelector.Text switch
            {
                "AStar" => new AStar<Position>(_startPoint, _endPoint, _greedStepper.Value, traverser),
                "Greedy" => new Greedy<Position>(_startPoint, _endPoint, traverser),
                "Breadth First" => new BreadthFirst<Position>(_startPoint, _endPoint, traverser),
                _ => throw new ArgumentException("Unknown GraphSolver")
            };

            _runnerThread = new SolverRunnerThread
            {
                GraphSolver = graphSolver, Delay = (int)_delayStepper.Value
            };
        }

        private void ProcessFrame()
        {
            var frameData = _runnerThread.GetFrameData();
            switch (frameData.State)
            {
                case SolverState.Waiting:
                case SolverState.Running:
                    _statsWidget.UpdateRunningStats(frameData);
                    if (ShowSearching)
                        _mapWidget.DrawSearchPoints(frameData);
                    else if (ShowBlindSearching)
                        _mapWidget.DrawWorldPoints(frameData);
                    _mapWidget.DrawBestBath(frameData);
                    break;
                case SolverState.Success:
                    _statsWidget.UpdateSuccessStats(frameData);
                    _mapWidget.ClearRunning();
                    if (ShowBlindSearching)
                        _mapWidget.DrawWorldPoints(frameData);
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
}