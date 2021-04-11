using System;
using Eto.Forms;
using Eto.Drawing;

namespace PathFinderGui
{
    public partial class MainForm : Form
    {
        private readonly UITimer _timer;
        private LayeredBitmapWidget _bitmapWidget;
        private Label _tpf;
        private Label _fps;
        private Label _tps;
        private Label _openPoints;
        private Label _closedPoints;
        private NumericStepper _greedStepper;
        private NumericStepper _scaleStepper;
        private NumericStepper _delayStepper;
        private NumericStepper _moveCostStepper;
        private CheckBox _canCornerCut;
        private CheckBox _showSearchCheckbox;
        private ComboBox _solverSelector;
        private Button _go;
        private Button _newWorld;
        private Button _newPoints;
        private Button _resetSolverButton;
        private Button _stopButton;
        private TextBox _worldSeed;
        private TextBox _pointsSeed;

        private void InitUi()
        {
            Title = "Path Finder Tester";
            ClientSize = new Size(750, 550);
            // WindowStyle = WindowStyle.None;

            _worldSeed = new TextBox {Text = new Random().Next(10000, 99999).ToString(), Width = 90};
            _pointsSeed = new TextBox {Text = new Random().Next(10000, 99999).ToString(), Width = 90};

            _moveCostStepper = new NumericStepper()
            {
                Value = 1.0,
                Width = 60,
                MinValue = 0.1,
                MaxValue = 8,
                Increment = 0.1,
                DecimalPlaces = 1,
            };

            _greedStepper = new NumericStepper
            {
                Width = 60,
                MinValue = 0,
                Increment = 0.05,
                DecimalPlaces = 2,
                Value = 1,
            };

            _scaleStepper = new NumericStepper
            {
                Width = 60,
                MinValue = 1,
                MaxValue = 8,
                Value = 1,
            };

            _delayStepper = new NumericStepper()
            {
                MinValue = 0,
                MaxValue = 10000,
                Value = 0,
                ToolTip = "Max ticks per frame"
            };

            _solverSelector = new ComboBox();

            _tpf = new Label {Text = "TPF: N/A"};
            _tps = new Label {Text = "TPS: N/A"};
            _fps = new Label {Text = "FPS: N/A"};
            _openPoints = new Label {Text = "Open Points: N/A"};
            _closedPoints = new Label {Text = "Closed Points: N/A"};
            
            _canCornerCut = new CheckBox {Checked = true};
            _showSearchCheckbox = new CheckBox {Checked = true};
            
            _bitmapWidget = new LayeredBitmapWidget(1, 4);

            _newPoints = new Button
            {
                Text = "New Points",
                Width = 90
            };

            _newWorld = new Button
            {
                Text = "New World",
                Width = 90
            };
            
            _go = new Button
            {
                Text = "Start",
                Width = 60
            };

            _resetSolverButton = new Button
            {
                Text = "Reset",
                Width = 60
            };

            _stopButton = new Button
            {
                Text = "Stop",
                Width = 60
            };

            StackLayoutItem HStretched(Control c) =>
                new StackLayoutItem {Control = c, HorizontalAlignment = HorizontalAlignment.Stretch};
            
            Content = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Items =
                {
                    new StackLayoutItem {
                        Control = _bitmapWidget,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Expand = true
                    },
                    new StackLayout
                    {
                        Width = 200,
                        Height = 600,
                        Orientation = Orientation.Vertical,
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
                                Orientation = Orientation.Horizontal,
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
                                Items = {"Cornering:", _canCornerCut, "Show Search:", _showSearchCheckbox}
                            },
                            HStretched(_solverSelector),
                            new StackLayout { 
                                Orientation = Orientation.Horizontal, 
                                Items = {_go, _stopButton, _resetSolverButton}
                            },
                            HStretched(_delayStepper),
                            HStretched(_tpf),
                            HStretched(_tps),
                            HStretched(_fps),
                            HStretched(_openPoints),
                            HStretched(_closedPoints)
                        }
                    }
                },
            };
        }

        
    }
}