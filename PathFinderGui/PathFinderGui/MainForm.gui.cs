using System;
using Eto.Forms;
using Eto.Drawing;

namespace PathFinderGui
{
    public partial class MainForm : Form
    {
        private UITimer _timer;
        private MapWidget _map;
        private Label _tpf;
        private Label _fps;
        private Label _tps;
        private Label _openPoints;
        private Label _closedPoints;
        private Slider _thoroughnessSlider;
        private Slider _scaleSlider;
        private CheckBox _canCornerCut;
        private Button _go;
        private Button _newWorld;
        private Button _newPoints;
        private Button _newSeed;
        private TextBox _worldSeed;
        private TextBox _pointsSeed;
        private TextBox _zUpCost;
        private TextBox _zDownCost;
        private TextBox _moveCost;

        private void InitUi()
        {
            Title = "Path Finder Tester";
            ClientSize = new Size(750, 550);
            // WindowStyle = WindowStyle.None;

            _worldSeed = new TextBox() {Text = (new Random()).Next(10000, 99999).ToString()};
            _pointsSeed = new TextBox() {Text = (new Random()).Next(10000, 99999).ToString()};

            _zUpCost = new TextBox() {Text = "50", Width = 50};
            _zDownCost = new TextBox() {Text = "10", Width = 50};
            _moveCost = new TextBox() {Text = "1", Width = 50};
            
            _thoroughnessSlider = new Slider
            {
                MinValue = 0,
                MaxValue = 100,
                Value = 50,
                
            };

            _scaleSlider = new Slider
            {
                MinValue = 1,
                MaxValue = 8,
                Value = 1,
            };
            _tpf = new Label {Text = "TPF: N/A"};
            _tps = new Label {Text = "TPS: N/A"};
            _fps = new Label {Text = "FPS: N/A"};
            _openPoints = new Label {Text = "Open Points: N/A"};
            _closedPoints = new Label {Text = "Closed Points: N/A"};
            _canCornerCut = new CheckBox {Checked = true };
            
            _map = new MapWidget( _scaleSlider.Value);
            _go = new Button
            {
                Text = "Go"
            };

            _newSeed = new Button
            {
                Text = "R"
            };

            _newWorld = new Button
            {
                Text = "New World",
            };

            _newPoints = new Button
            {
                Text = "New Points"
            };
            
            Content = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Items =
                {
                    new StackLayoutItem {
                        Control = _map,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Expand = true
                    },
                    new StackLayout
                    {
                        Width = 200,
                        Height = 500,
                        Orientation = Orientation.Vertical,
                        Padding = 10,
                        Items =
                        {
                            new StackLayoutItem { Control = "Path Finder Testing", HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = "Scale", HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = _scaleSlider, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = "World Seed", HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayout
                            {
                                Orientation = Orientation.Horizontal,
                                Items = { _worldSeed, _newSeed}
                            },
                            new StackLayoutItem { Control = _worldSeed, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayout
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                Items =
                                {
                                    new StackLayoutItem { Control = _zUpCost, HorizontalAlignment = HorizontalAlignment.Stretch },
                                    new StackLayoutItem { Control = _zDownCost, HorizontalAlignment = HorizontalAlignment.Stretch },
                                    new StackLayoutItem { Control = _moveCost, HorizontalAlignment = HorizontalAlignment.Stretch }
                                }
                            },
                            new StackLayoutItem { Control = "Points", HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = _pointsSeed, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayout
                            {
                                Orientation =  Orientation.Horizontal, 
                                Items = { _newWorld, (_newPoints) }
                            },
                            new StackLayoutItem { Control = "Thoroughness", HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = _thoroughnessSlider, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayout { 
                                Orientation = Orientation.Horizontal, 
                                Items = {"Cornering", _canCornerCut}
                            },
                            new StackLayoutItem { Control = _go, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = _tpf, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = _tps, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = _fps, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = _openPoints, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = _closedPoints, HorizontalAlignment = HorizontalAlignment.Stretch},
                        }
                    }
                },
            };
        }

        
    }
}