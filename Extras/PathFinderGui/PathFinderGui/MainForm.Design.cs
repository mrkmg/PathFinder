using System;
using Eto.Forms;
using Eto.Drawing;
using PathFinderGui.Widgets;
using SimpleWorld.Map;

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

        private readonly Slider _initF1 = new () {
            MinValue = 0,
            MaxValue = 100,
            Value = 50,
        }; 

        private readonly Slider _initF2 = new () {
            MinValue = 0,
            MaxValue = 100,
            Value = 50,
        }; 

        private readonly Slider _initL1 = new () {
            MinValue = 0,
            MaxValue = 100,
            Value = 50,
        }; 

        private readonly Slider _initL2 = new () {
            MinValue = 0,
            MaxValue = 100,
            Value = 50,
        }; 

        private readonly Slider _initP1 = new () {
            MinValue = 0,
            MaxValue = 100,
            Value = 50,
        }; 

        private readonly Slider _initP2 = new () {
            MinValue = 0,
            MaxValue = 100,
            Value = 50,
        }; 

        private readonly Slider _initRatio12 = new () {
            MinValue = 0,
            MaxValue = 100,
            Value = 50,
        }; 
        
        private readonly CheckBox _canCornerCut = new () { Checked = true };
        private readonly CheckBox _showSearchCheckbox = new() {Checked = true};

        private readonly ComboBox _solverSelector = new ()
        {
            DataStore = new[] {"AStar", "Breadth First", "Greedy"},
            Text = "AStar",
            ReadOnly = true
        };

        private readonly Button _newSeedButton = new () 
        {
            Text = "New Seed",
            Width = 90
        };
        private readonly Button _newPointsButton = new () 
        {
            Text = "New Points",
            Width = 90
        };
        private readonly Button _go = new()
        {
            Text = "Start",
            Width = 60
        };
        private readonly Button _pauseButton = new()
        {
            Text = "Stop",
            Width = 60
        };

        private readonly Button _newWorldButton = new()
        {
            Text = "New World Options",
        };

        private void InitUi()
        {

            static StackLayoutItem HStretched(Control c) =>
                new() {Control = c, HorizontalAlignment = HorizontalAlignment.Stretch};

            static StackLayoutItem LabelInput(int labelWidth, string label, Control input) =>
                HStretched(new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    Items =
                    {
                        new StackLayoutItem {Control = new Label {Text = label, Width = labelWidth}}, HStretched(input)
                    }
                });
            
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
                                Items = { _newSeedButton, _newPointsButton }
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
                                Items = {_go, _pauseButton}
                            },
                            HStretched(_delayStepper),
                            HStretched(_statsWidget),
                            LabelInput(50, "F1", _initF1),
                            LabelInput(50, "L1", _initL1),
                            LabelInput(50, "P1", _initP1),
                            LabelInput(50, "F2", _initF2),
                            LabelInput(50, "L2", _initL2),
                            LabelInput(50, "P2", _initP2),
                            LabelInput(50, "R12", _initRatio12),
                            HStretched(_newWorldButton)
                        }
                    }
                }
            };
        }

        
    }
}