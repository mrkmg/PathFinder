using System;
using Eto.Forms;
using PathFinder.Gui.Widgets;
using PathFinder.Solvers.Generic;
using SimpleWorld.Map;

namespace PathFinder.Gui.Forms
{
    public partial class MainForm
    {
        private readonly UiEventDebouncer<EventArgs> _scaleSelectorChangedDebounce = new (250);
        private readonly UiEventDebouncer<EventArgs> _moveCostSelectorChangedDebounce = new (250);
        private readonly UiEventDebouncer<EventArgs> _worldInitChanged = new(250);
        
        private void BindEvents()
        {
            Load += (_, _) => Content.Height = Height;
            Closed += OnClosed;
            KeyUp += CheckKeyupForExit;

            _mapWidget.IsReady += OnMapWidgetIsReady;
            _mapWidget.MouseUp += OnBitmapWidgetMouseUp;
            
            _solverSelector.TextChanged += OnSolverSelectorChanged;
            _traverserSelector.TextChanged += OnTraverserChanged;
            _worldGenType.TextChanged += WorldGenChanged;
            
            _newSeedButton.Click += OnNewSeedClick;
            _newPointsButton.Click += OnNewPointsClick;
            _go.Click += OnGoClick;
            _pauseButton.Click += OnPauseButtonClick;
            
            _delayStepper.ValueChanged += OnDelayStepperChanged;
            _greedStepper.ValueChanged += OnGreedSelectorChanged;
            _stepSizeStepper.ValueChanged += OnStepSizeChanged;
            _scaleStepper.ValueChanged += _scaleSelectorChangedDebounce.Handle;
            _moveCostStepper.ValueChanged += _moveCostSelectorChangedDebounce.Handle;
            
            _initF1.ValueChanged += _worldInitChanged.Handle;
            _initL1.ValueChanged += _worldInitChanged.Handle;
            _initP1.ValueChanged += _worldInitChanged.Handle;
            _initSX1.ValueChanged += _worldInitChanged.Handle;
            _initSY1.ValueChanged += _worldInitChanged.Handle;

            _initF2.ValueChanged += _worldInitChanged.Handle;
            _initL2.ValueChanged += _worldInitChanged.Handle;
            _initP2.ValueChanged += _worldInitChanged.Handle;
            _initSX2.ValueChanged += _worldInitChanged.Handle;
            _initSY2.ValueChanged += _worldInitChanged.Handle;
            
            _initRatio12.ValueChanged += _worldInitChanged.Handle;
            
            _initML.ValueChanged += _worldInitChanged.Handle;
            _initMT.ValueChanged += _worldInitChanged.Handle;
            _initMF.ValueChanged += _worldInitChanged.Handle;
            _initMFE.CheckedChanged += _worldInitChanged.Handle;
            _initMDR.CheckedChanged += _worldInitChanged.Handle;
            
            _newWorldButton.Click += OnNewWorldClick;
            
            _worldSeed.KeyUp += OnWorldSeedChanged;
            _pointsSeed.KeyUp += OnPointsSeedChanged;
            
            _showSearchCheckbox.CheckedChanged += OnShowSearchCheckboxChanged;
            _doBlindSearch.CheckedChanged += DoBlindSearchChanged;
            
            _scaleSelectorChangedDebounce.Fired += OnScaleSelectorChanged;
            _moveCostSelectorChangedDebounce.Fired += OnMoveCostSelectorChanged;
            _worldInitChanged.Fired += OnWorldInitChanged;
        }

        private void WorldGenChanged(object? sender, EventArgs e)
        {
            switch (_worldGenType.Text)
            {
                case "Standard":
                    _standardWorldOptions.Visible = true;
                    _initRatio12.Visible = true;
                    _mazeWorldOptions.Visible = false;
                    _traverserSelector.Text = "Default";
                    _traverserTable.Visible = true;
                    break;
                case "Maze":
                    _standardWorldOptions.Visible = false;
                    _initRatio12.Visible = false;
                    _mazeWorldOptions.Visible = true;
                    _traverserSelector.Text = "Grid";
                    _traverserTable.Visible = false;
                    break;
                default:
                    throw new ArgumentException();
            }
            
            KillRunning();
            MakeWorld();
        }

        private void DoBlindSearchChanged(object? sender, EventArgs e)
        {
            if (_world == null) return;
            Reset();
        }

        private void OnTraverserChanged(object? sender, EventArgs e)
        {
            _stepSizeStepper.Enabled = _traverserSelector.Text == "LargeStep";
            if (_world == null) return;
            Reset();
        }

        private void OnStepSizeChanged(object? sender, EventArgs e)
        {
            if (_world == null) return;
            if (_traverserSelector.Text != "LargeStep") return;

            Reset();
        }

        private void OnNewWorldClick(object? sender, EventArgs e)
        {
            _initF1.Value = StaticRandom.Next(StandardOptionsMax/2);
            _initF2.Value = StaticRandom.Next(StandardOptionsMax/2) + StandardOptionsMax/2;
            _initL1.Value = StaticRandom.Next(StandardOptionsMax);
            _initL2.Value = StaticRandom.Next(StandardOptionsMax);
            _initP1.Value = StaticRandom.Next(StandardOptionsMax);
            _initP2.Value = StaticRandom.Next(StandardOptionsMax);
            _initSX1.Value = StaticRandom.Next(StandardOptionsMax);
            _initSX2.Value = StaticRandom.Next(StandardOptionsMax);
            _initSY1.Value = StaticRandom.Next(StandardOptionsMax);
            _initSY2.Value = StaticRandom.Next(StandardOptionsMax);
            _initRatio12.Value = StaticRandom.Next(StandardOptionsMax);
            
            _initML.Value = StaticRandom.Next(MazeOptionsMax);
            _initMT.Value = StaticRandom.Next(MazeOptionsMax);
            _initMF.Value = StaticRandom.Next(MazeOptionsMax);
            
            KillRunning();
            _mapWidget.Clear();
        }

        private void OnWorldInitChanged(object? sender, EventArgs e)
        {
            MakeWorld();
        }

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
            MakeWorld();
        }
        private void OnShowSearchCheckboxChanged(object sender, EventArgs e)
        {
            if (_runnerThread != null)
                _runnerThread.RunToSolve = !ShowSearching;
        }
        private void OnPauseButtonClick(object sender, EventArgs e)
        {
            if (_runnerThread is {GraphSolver: AStar<Position> solver})
            {
                solver.Stop();
                _runnerThread.Kill();
                _mapWidget.DrawSearchPoints(_lastFrameData);
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
                    break;
                case "Greedy":
                case "Breadth First":
                    _greedStepper.Enabled = false;
                    break;
            }
        }
        private void OnMapWidgetIsReady(object sender, EventArgs args) => MakeWorld();
        private void OnMoveCostSelectorChanged(object sender, EventArgs args)
        {
            if (_world == null) return;
            KillRunning();
            MakeWorld();
        }
        private void OnScaleSelectorChanged(object sender, EventArgs args)
        {
            KillRunning();
            _mapWidget.ChangeScale((int)_scaleStepper.Value);
        }
        private void OnDelayStepperChanged(object sender, EventArgs args)
        {
            if (_runnerThread != null) _runnerThread.Delay = (int)_delayStepper.Value;
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
        private void OnNewSeedClick(object sender, EventArgs args)
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
    }
}