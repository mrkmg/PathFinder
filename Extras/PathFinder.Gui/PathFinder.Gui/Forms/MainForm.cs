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
        private const int MazeOptionsMax = 20;
        private const double MazeOptionsScale = 2.5;
        private const int StandardOptionsMax = 100;
        
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

                if (tries > 100) break;

                var t1 = 5000;
                do randomFromNode = _world.GetPosition(rnd.Next(0, _world.XSize - 1), rnd.Next(0, _world.YSize - 1));
                while (randomFromNode == null && --t1 > 0);

                t1 = 5000;
                do randomToNode = _world.GetPosition(rnd.Next(0, _world.XSize - 1), rnd.Next(0, _world.YSize - 1));
                while (randomToNode == null && --t1 > 0);

                if (randomFromNode == null || randomToNode == null) break;
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
            _mapWidget.ClearLayer(0);
            _mapWidget.ClearRunning();
            _mapWidget.ClearPath();

            _world = _worldGenType.SelectedValue switch
            {
                "Standard" => new World(_mapWidget.BitmapWidth, _mapWidget.BitmapHeight, _moveCostStepper.Value,
                    new World.StandardInitializationOptions
                    {
                        F1 = (double) _initF1.Value / StandardOptionsMax * 3d,
                        L1 = (double) _initL1.Value / StandardOptionsMax * 3d + 1d,
                        P1 = (double) _initP1.Value / StandardOptionsMax,
                        SX1 = (double) _initSX1.Value / StandardOptionsMax * 5d,
                        SY1 = (double) _initSY1.Value / StandardOptionsMax * 5d,
                        F2 = (double) _initF2.Value / StandardOptionsMax * 10d,
                        L2 = (double) _initL2.Value / StandardOptionsMax * 3d + 1d,
                        P2 = (double) _initP2.Value / StandardOptionsMax,
                        SX2 = (double) _initSX2.Value / StandardOptionsMax * 5d,
                        SY2 = (double) _initSY2.Value / StandardOptionsMax * 5d,
                        Ratio12 = (double) _initRatio12.Value / StandardOptionsMax * 4d - 2d,
                    }, new Random(int.Parse(_worldSeed.Text))),
                "Maze" => new World(_mapWidget.BitmapWidth, _mapWidget.BitmapHeight, _moveCostStepper.Value,
                    new World.MazeInitializationOptions
                    {
                        LineWeight = (int) Math.Pow(_mazeInitLineWeight.Value, MazeOptionsScale), 
                        TurnWeight = (int) Math.Pow(_mazeInitTurnWeight.Value, MazeOptionsScale), 
                        ForkWeight = (int) Math.Pow(_mazeInitForkWeight.Value, MazeOptionsScale),
                        FillEmpty = _mazeInitFillEmpty.Checked ?? false,
                        IncludeDemoRooms = _mazeInitDemoRooms.Checked ?? false,
                        WideWalls = _mazeInitDisplayType.Text == "1x2",
                        WidePaths = _mazeInitDisplayType.Text == "2x1"
                    }, new Random(int.Parse(_worldSeed.Text))),
                _ => _world
            };
            SetRandomPoints();
            _mapWidget.DrawWorld(_world);
        }

        private void Go()
        {
            if (_world == null) return;
            if (_runnerThread == null)
            {
                CreateNewRunner();
                if (ShowBlindSearching) _mapWidget.ClearLayer(0);
                else _mapWidget.DrawWorld(_world);
            }

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
                "Level" => new LevelTraverser(),
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
                case SolverState.Incomplete:
                case SolverState.Running:
                    _statsWidget.UpdateRunningStats(frameData);
                    if (ShowSearching)
                        _mapWidget.DrawSearchPoints(frameData);
                    else if (ShowBlindSearching)
                        _mapWidget.DrawWorldPoints(frameData, _world);
                    _mapWidget.DrawBestBath(frameData);
                    break;
                case SolverState.Success:
                    _statsWidget.UpdateSuccessStats(frameData);
                    _mapWidget.ClearRunning();
                    if (ShowBlindSearching)
                        _mapWidget.DrawWorldPoints(frameData, _world);
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