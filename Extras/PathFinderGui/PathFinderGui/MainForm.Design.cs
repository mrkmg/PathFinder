using System;
using Eto.Forms;
using Eto.Drawing;
using PathFinderGui.Widgets;

namespace PathFinderGui
{
    public partial class MainForm : Form
    {
        private static bool IsGtk => Application.Instance.Platform.IsGtk;

        private readonly MapWidget _mapWidget = new()
        {
            BackgroundColor = Colors.Black
        };
        
        private readonly StatsWidget _statsWidget = new ();

        private readonly TextBox _worldSeed = new ()
        {
            Text = new Random().Next(10000, 99999).ToString(),
            Width = 90
        };
        
        private readonly TextBox _pointsSeed = new ()
        {
            Text = new Random().Next(10000, 99999).ToString(),
            Width = 90
        };
        
        private readonly NumericStepper _greedStepper = new ()
        {
            Width = IsGtk ? 150 : 60,
            MinValue = 0,
            Increment = 0.05,
            DecimalPlaces = 2,
            Value = 1
        };
        private readonly NumericStepper _scaleStepper = new ()
        {
            Width = IsGtk ? 150 : 60,
            MinValue = 1,
            MaxValue = 8,
            Value = 1
        };
        private readonly NumericStepper _delayStepper = new ()
        {
            MinValue = 0,
            MaxValue = 10000,
            Value = 0,
            ToolTip = "Max ticks per frame"
        };
        private readonly NumericStepper _moveCostStepper = new ()
        {
            Value = 1.0,
            Width = IsGtk ? 150 : 60,
            MinValue = 0.1,
            MaxValue = 8,
            Increment = 0.1,
            DecimalPlaces = 1
        };
        private readonly CheckBox _canCornerCut = new () { Checked = true };
        private readonly CheckBox _showSearchCheckbox = new() {Checked = true};

        private readonly ComboBox _solverSelector = new ()
        {
            DataStore = new[] {"AStar", "Breadth First", "Greedy"},
            Text = "AStar",
            ReadOnly = true
        };

        private readonly Button _newWorld = new () 
        {
            Text = "New World",
            Width = 90
        };
        private readonly Button _newPoints = new () 
        {
            Text = "New Points",
            Width = 90
        };
        private readonly Button _go = new()
        {
            Text = "Start",
            Width = 60
        };
        private readonly Button _resetSolverButton = new()
        {
            Text = "Reset",
            Width = 60
        };
        
        private readonly Button _pauseButton = new()
        {
            Text = "Stop",
            Width = 60
        };

        private void InitUi()
        {
            Title = "Path Finder Tester";
            ClientSize = new Size(750, 600);
            

            static StackLayoutItem HStretched(Control c) =>
                new() {Control = c, HorizontalAlignment = HorizontalAlignment.Stretch};
            
            Content = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Items =
                {
                    new StackLayoutItem {
                        Control = _mapWidget,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Expand = true
                    },
                    new StackLayout
                    {
                        Width = -1,
                        Orientation = Orientation.Vertical,
                        VerticalContentAlignment = VerticalAlignment.Stretch,
                        Padding = 10,
                        Spacing = 10,
                        Items =
                        {
                            HStretched("Path Finder Testing"),
                            HStretched("Seeds"),
                            new StackLayout
                            {
                                Orientation = Orientation.Horizontal,
                                Items = { _worldSeed, _pointsSeed}
                            },
                            new StackLayout
                            {
                                Orientation =  Orientation.Horizontal, 
                                Items = { _newWorld, _newPoints }
                            },
                            new StackLayout
                            {
                                Orientation = IsGtk ? Orientation.Vertical : Orientation.Horizontal,
                                Items =
                                {
                                    new StackLayout { Items =
                                    {
                                        "Factor",
                                        _moveCostStepper
                                    }},new StackLayout { Items =
                                    {
                                        "Greed",
                                        _greedStepper
                                    }},new StackLayout { Items =
                                    {
                                        "Scale",
                                        _scaleStepper
                                    }}
                                }
                            },
                            new StackLayout { 
                                Orientation = Orientation.Horizontal, 
                                Items = {"Cornering:", _canCornerCut, "   ", "Show Search:", _showSearchCheckbox}
                            },
                            HStretched(_solverSelector),
                            new StackLayout { 
                                Orientation = Orientation.Horizontal, 
                                Items = {_go, _pauseButton, _resetSolverButton}
                            },
                            HStretched(_delayStepper),
                            HStretched(_statsWidget)
                        }
                    }
                }
            };
        }

        
    }
}