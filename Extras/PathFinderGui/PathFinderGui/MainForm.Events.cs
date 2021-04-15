using System;
using Eto.Drawing;
using Eto.Forms;
using PathFinder.Solvers.Generic;
using PathFinderGui.Widgets;
using SimpleWorld.Map;

namespace PathFinderGui
{
    public partial class MainForm
    {
        private readonly UiEventDebouncer<EventArgs> _scaleSelectorChangedDebounce = new (250);
        private readonly UiEventDebouncer<EventArgs> _moveCostSelectorChangedDebounce = new (250);
        
        private void BindEvents()
        {
            Load += (_, _) => Content.Height = Height;
            Closed += (_, _) => KillRunning();
            KeyUp += CheckKeyupForExit;

            _mapWidget.IsReady += OnMapWidgetIsReady;
            _mapWidget.MouseUp += OnBitmapWidgetMouseUp;
            
            _solverSelector.TextChanged += OnSolverSelectorChanged;
            
            _newWorld.Click += OnNewWorldClick;
            _newPoints.Click += OnNewPointsClick;
            _go.Click += OnGoClick;
            _pauseButton.Click += OnPauseButtonClick;
            
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
    }
}