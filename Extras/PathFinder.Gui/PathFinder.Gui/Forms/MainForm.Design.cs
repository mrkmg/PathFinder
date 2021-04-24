using System;
using Eto.Drawing;
using Eto.Forms;
using PathFinder.Gui.Widgets;

namespace PathFinder.Gui.Forms
{
    public partial class MainForm : Form
    {
        private static bool IsGtk => Application.Instance.Platform.IsGtk;
        private static readonly Random StaticRandom = new ();
        
        private readonly MapWidget _mapWidget = new()
        {
            BackgroundColor = Colors.Black,
            Scale = 3
        };
        
        private readonly StatsWidget _statsWidget = new ();

        private readonly TextBox _worldSeed = new ()
        {
            Text = new Random().Next(100000, 999999).ToString(),
            Width = 90
        };
        
        private readonly TextBox _pointsSeed = new ()
        {
            Text = new Random().Next(100000, 999999).ToString(),
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
            Width = IsGtk ? 150 : 90,
            MinValue = 1,
            MaxValue = 8,
            Value = 3
        };
        private readonly NumericStepper _stepSizeStepper = new ()
        {
            Width = IsGtk ? 150 : 60,
            MinValue = 1,
            MaxValue = 8,
            Value = 3,
            Increment = 1,
            DecimalPlaces = 0,
            Enabled = false
        };
        private readonly NumericStepper _delayStepper = new ()
        {
            MinValue = 0,
            MaxValue = 10000,
            Value = 1000,
            ToolTip = "Max TPF (0 for unlimited)",
            Width = 60
        };
        private readonly NumericStepper _moveCostStepper = new ()
        {
            Value = 1.0,
            Width = IsGtk ? 150 : 90,
            MinValue = 0.1,
            MaxValue = 8,
            Increment = 0.1,
            DecimalPlaces = 1
        };

        private readonly Slider _initF1 = new () {
            MinValue = 0,
            MaxValue = StandardOptionsMax,
            Value = StaticRandom.Next(StandardOptionsMax),
            Width = 80,
            ToolTip = "Frequency Layer 1"
        }; 

        private readonly Slider _initF2 = new () {
            MinValue = 0,
            MaxValue = StandardOptionsMax,
            Value = StaticRandom.Next(StandardOptionsMax),
            Width = 80,
            ToolTip = "Frequency Layer 2"
        }; 

        private readonly Slider _initL1 = new () {
            MinValue = 0,
            MaxValue = StandardOptionsMax,
            Value = StaticRandom.Next(StandardOptionsMax),
            Width = 80,
            ToolTip = "Lacunarity Layer 1"
        }; 

        private readonly Slider _initL2 = new () {
            MinValue = 0,
            MaxValue = StandardOptionsMax,
            Value = StaticRandom.Next(StandardOptionsMax),
            Width = 80,
            ToolTip = "Lacunarity Layer 2"
        }; 

        private readonly Slider _initP1 = new () {
            MinValue = 0,
            MaxValue = StandardOptionsMax,
            Value = StaticRandom.Next(StandardOptionsMax),
            Width = 80,
            ToolTip = "Persistence Layer 1"
        }; 

        private readonly Slider _initP2 = new () {
            MinValue = 0,
            MaxValue = StandardOptionsMax,
            Value = StaticRandom.Next(StandardOptionsMax),
            Width = 80,
            ToolTip = "Persistence Layer 2"
        }; 

        private readonly Slider _initSX1 = new () {
            MinValue = 0,
            MaxValue = StandardOptionsMax,
            Value = StaticRandom.Next(StandardOptionsMax),
            Width = 80,
            ToolTip = "X Scale Layer 1"
        }; 

        private readonly Slider _initSX2 = new () {
            MinValue = 0,
            MaxValue = StandardOptionsMax,
            Value = StaticRandom.Next(StandardOptionsMax),
            Width = 80,
            ToolTip = "X Scale Layer 2"
        }; 

        private readonly Slider _initSY1 = new () {
            MinValue = 0,
            MaxValue = StandardOptionsMax,
            Value = StaticRandom.Next(StandardOptionsMax),
            Width = 80,
            ToolTip = "Y Scale Layer 1"
        }; 

        private readonly Slider _initSY2 = new () {
            MinValue = 0,
            MaxValue = StandardOptionsMax,
            Value = StaticRandom.Next(StandardOptionsMax),
            Width = 80,
            ToolTip = "Y Scale Layer 2"
        }; 

        private readonly Slider _initRatio12 = new () {
            MinValue = 0,
            MaxValue = StandardOptionsMax,
            Value = StaticRandom.Next(StandardOptionsMax),
            ToolTip = "Ratio between Layer 1 and 2"
        }; 
        
        

        private readonly Slider _mazeInitLineWeight = new () {
            MinValue = 0,
            MaxValue = MazeOptionsMax,
            Value = StaticRandom.Next(MazeOptionsMax),
            Width = 80,
            ToolTip = "Maze Line Weight"
        }; 

        private readonly Slider _mazeInitTurnWeight = new () {
            MinValue = 0,
            MaxValue = MazeOptionsMax,
            Value = StaticRandom.Next(MazeOptionsMax),
            Width = 80,
            ToolTip = "Maze Turn Weight"
        }; 

        private readonly Slider _madeInitForkWeight = new () {
            MinValue = 0,
            MaxValue = MazeOptionsMax,
            Value = StaticRandom.Next(MazeOptionsMax),
            ToolTip = "Maze Fork Weight"
        };

        private readonly CheckBox _mazeInitFillEmpty = new()
        {
            ToolTip = "Maze Fill Empty",
            Checked = true
        };

        private readonly CheckBox _mazeInitDemoRooms = new()
        {
            ToolTip = "Include Demo Rooms in Maze",
            Checked = true
        };

        private readonly CheckBox _mazeInitWideWalls = new()
        {
            ToolTip = "Wide Walls In Maze",
            Checked = false
        };
        
        private readonly CheckBox _showSearchCheckbox = new() {Checked = true};
        private readonly CheckBox _doBlindSearch = new() {Checked = false};

        private readonly ComboBox _solverSelector = new ()
        {
            DataStore = new[] {"AStar", "Breadth First", "Greedy"},
            Text = "AStar",
            ReadOnly = true
        };

        private readonly ComboBox _traverserSelector = new()
        {
            DataStore = new[] {"Default", "Grid", "LargeStep"},
            Text = "Default",
            ReadOnly = true
        };

        private readonly ComboBox _worldGenType = new()
        {
            DataStore = new[] {"Standard", "Maze"},
            Text = "Standard",
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

        private TableLayout _standardWorldOptions;
        private TableLayout _mazeWorldOptions;
        private TableLayout _traverserTable;

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

            // is there a better way to do this?
            var sectionFont = Fonts.Sans(10f, FontStyle.Bold);
            var titleFont = Fonts.Sans(12f, FontStyle.Bold);
            
            _standardWorldOptions = new TableLayout { Rows = {
                new TableRow { Cells = {
                    new TableCell { Control = ""},
                    new TableCell { Control = new Label {Text = "1", TextAlignment = TextAlignment.Center, Width = 80}},
                    new TableCell { Control = new Label {Text = "2", TextAlignment = TextAlignment.Center, Width = 80}}}},
                new TableRow { Cells = {
                    new TableCell { Control = "F"},
                    new TableCell { Control = _initF1},
                    new TableCell { Control = _initF2}}},
                new TableRow { Cells = {
                    new TableCell { Control = "L"},
                    new TableCell { Control = _initL1},
                    new TableCell { Control = _initL2}}},
                new TableRow { Cells = {
                    new TableCell { Control = "P"},
                    new TableCell { Control = _initP1},
                    new TableCell { Control = _initP2}}},
                new TableRow { Cells = {
                    new TableCell { Control = "SX"},
                    new TableCell { Control = _initSX1},
                    new TableCell { Control = _initSX2}}},
                new TableRow { Cells = {
                    new TableCell { Control = "SY"},
                    new TableCell { Control = _initSY1},
                    new TableCell { Control = _initSY2}}}}};
            
            _mazeWorldOptions = new TableLayout {
                Visible = false,
                Rows ={
                    new TableRow { Cells = {
                        new TableCell { Control = "Line"},
                        new TableCell { Control = _mazeInitLineWeight}}},
                    new TableRow { Cells = {
                        new TableCell { Control = "Turn"},
                        new TableCell { Control = _mazeInitTurnWeight}}},
                    new TableRow { Cells = {
                        new TableCell { Control = "Fork"},
                        new TableCell { Control = _madeInitForkWeight}}},
                    new TableRow { Cells = {
                        new TableCell { Control = "Fill"},
                        new TableCell { Control = _mazeInitFillEmpty}}},
                    new TableRow { Cells = {
                        new TableCell { Control = "Rooms"},
                        new TableCell { Control = _mazeInitDemoRooms}}},
                    new TableRow { Cells = {
                        new TableCell { Control = "Wide"},
                        new TableCell { Control = _mazeInitWideWalls}}},}};
            
            _traverserTable = new TableLayout { Rows = {
                new TableRow { Cells = {
                    new TableCell { Control = "Traverser"}, 
                    new TableCell { Control = "Step Size"}}},
                new TableRow { Cells = {
                    new TableCell(_traverserSelector), new TableCell(_stepSizeStepper)}}}};
            
            // TODO: Separate this monster into smaller parts
            Content = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Items = {
                    new StackLayoutItem {
                        Control = _mapWidget,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Expand = true },
                    new StackLayout {
                        Width = -1,
                        Orientation = Orientation.Vertical,
                        VerticalContentAlignment = VerticalAlignment.Stretch,
                        Padding = 10,
                        Spacing = 10,
                        Items = {
                            new Label {
                                Text = "Path Finder Playground",
                                Font = titleFont},
                            new Label {
                                Text = "World Settings",
                                Font = sectionFont},
                            HStretched("Seeds"),
                            new StackLayout {
                                Orientation = Orientation.Horizontal,
                                Items = { _worldSeed, _pointsSeed}},
                            new StackLayout {
                                Orientation =  Orientation.Horizontal, 
                                Items = { _newSeedButton, _newPointsButton }},
                            new StackLayout {
                                Orientation = IsGtk ? Orientation.Vertical : Orientation.Horizontal,
                                Items = {
                                    new StackLayout { Items = {
                                        "Factor",
                                        _moveCostStepper }},
                                    new StackLayout { Items = {
                                        "Scale",
                                        _scaleStepper }}}},
                            _standardWorldOptions,
                            _mazeWorldOptions,
                            LabelInput(50, "Ratio", _initRatio12),
                            LabelInput(80, "World Type", _worldGenType),
                            HStretched(_newWorldButton),
                            new Label {
                                Text = "Solver Settings",
                                Font = sectionFont},
                            new TableLayout { Rows = {
                                    new TableRow { Cells = {
                                        new TableCell { Control = "Solver"}, 
                                        new TableCell { Control = "Greed"}}},
                                    new TableRow { Cells = {
                                        new TableCell(_solverSelector), new TableCell(_greedStepper)}}}},
                            _traverserTable,
                            new Label {
                                Text = "Commands",
                                Font = sectionFont},
                            LabelInput(130, "Show Search", _showSearchCheckbox),
                            LabelInput(130, "View as Solver", _doBlindSearch),
                            new StackLayout { 
                                Orientation = Orientation.Horizontal, 
                                Items = {_go, _pauseButton, _delayStepper}},
                            HStretched(_statsWidget)
                        }
                    }
                }
            };
        }

        
    }
}