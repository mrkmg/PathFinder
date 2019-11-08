using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eto.Forms;
using Eto.Drawing;
using Eto.Threading;
using PathFinder.Interfaces;
using PathFinder.Solvers;
using SimpleWorld.Map;
using Thread = System.Threading.Thread;

namespace PathFinderGui
{
    public partial class MainForm : Form
    {
        private UITimer timer = null;
        private SolverRunnerThread _runnerThread = null;
        private MapWidget map;
        private Label tpf;
        private Label openPoints;
        private Label closedPoints;
        private int _markerSize = 10;

        public MainForm()
        {
            Title = "Path Finder Tester";
            ClientSize = new Size(750, 550);
            WindowStyle = WindowStyle.None;

            Closed += (sender, args) => KillRunning();

            var tSlider = new Slider
            {
                MinValue = 0,
                MaxValue = 100,
                Value = 50,
                
            };

            var scaleSlider = new Slider
            {
                MinValue = 1,
                MaxValue = 8,
                Value = 1,
            };

            tpf = new Label {Text = "TPF: N/A"};
            openPoints = new Label {Text = "Open Points: N/A"};
            closedPoints = new Label {Text = "Closed Points: N/A"};

            var cornerCut = new CheckBox {Checked = true };
            
            map = new MapWidget( 4);
            var goButton = new Button
            {
                Text = "Go"
            };

            var makeNewWorkButton = new Button
            {
                Text = "New World",
            };

            var newPointsButton = new Button
            {
                Text = "New Points"
            };

            World world = null;
            Position start = null;
            Position end = null;
            
            goButton.Click += (sender, args) =>
            {
                if (world != null)
                    Go(world, tSlider.Value, cornerCut.Checked.Value, start, end);
            };

            newPointsButton.Click += (sender, args) =>
            {
                if (world == null) return;
                var points = getRandomPoints(world);
                if (points != null)
                {
                    start = points.Item1;
                    end = points.Item2;
                }

                map.DrawAll(world.GetAllNodes().AsMapDrawPoints());
                map.DrawMarker(start.X, start.Y, _markerSize, Colors.Blue);
                map.DrawMarker(end.X, end.Y, _markerSize, Colors.Blue);
            };
            
            makeNewWorkButton.Click += (sender, args) =>
            {
                map.ChangeScale(scaleSlider.Value);
                world = MakeWorld();
                var points = getRandomPoints(world);
                if (points == null)
                {
                    world = null;
                }
                else
                {
                    start = points.Item1;
                    end = points.Item2;
                    map.DrawAll(world.GetAllNodes().AsMapDrawPoints());
                    map.DrawMarker(start.X, start.Y, _markerSize, Colors.Blue);
                    map.DrawMarker(end.X, end.Y, _markerSize, Colors.Blue);
                }
            };
            
            Content = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Items =
                {
                    new StackLayoutItem {
                        Control = map,
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
                            new StackLayoutItem { Control = scaleSlider, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayout
                            {
                                Orientation =  Orientation.Horizontal, 
                                Items = { makeNewWorkButton, newPointsButton }
                            },
                            new StackLayoutItem { Control = "Thoroughness", HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = tSlider, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayout { 
                                Orientation = Orientation.Horizontal, 
                                Items = {"Cornering", cornerCut}
                            },
                            new StackLayoutItem { Control = goButton, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = tpf, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = openPoints, HorizontalAlignment = HorizontalAlignment.Stretch},
                            new StackLayoutItem { Control = closedPoints, HorizontalAlignment = HorizontalAlignment.Stretch},
                        }
                    }
                },
            };

            Load += (sender, args) => Content.Height = Height;
            SizeChanged += (sender, args) =>
            {
                KillRunning();
                MakeWorld();
                Content.Height = Height;
            };
        }

        private Tuple<Position, Position> getRandomPoints(World world)
        {
            var rnd = new Random();
            
            var worldSize = Math.Sqrt(world.XSize * world.XSize + world.YSize * world.YSize);
            var targetSize = (int)(worldSize * 0.75);
                
            Position randomFromNode = null;
            Position randomToNode = null;

            IList<Position> path = null;

            var tries = 0;
            while (path == null)
            {
                tries++;

                if (tries > 100000) break;
                    
                do randomFromNode = world.GetNode(rnd.Next(0, world.XSize - 1), rnd.Next(0, world.YSize - 1));
                while (randomFromNode == null);
                    
                do randomToNode = world.GetNode(rnd.Next(0, world.XSize - 1), rnd.Next(0, world.YSize - 1));
                while (randomToNode == null);

                var x = Math.Abs(randomFromNode.X - randomToNode.X);
                var y = Math.Abs(randomFromNode.Y - randomToNode.Y);
                var dist = Math.Sqrt(x*x + y*y);
                    
                if (dist < targetSize) continue;
                path = AStar.Solve(randomFromNode, randomToNode, 0);
            }
                
            if (path == null) return null;
            
            return new Tuple<Position, Position>(randomFromNode, randomToNode);
        }

        private void KillRunning()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }

            if (_runnerThread != null)
            {
                _runnerThread.kill = true;
                _runnerThread = null;
            }
        }

        private World MakeWorld()
        {
            KillRunning();
            var world = new World(map.MapWidth, map.MapHeight, new Random());
            return world;
        }

        private void Go(World world, int thoroughness, bool corner, Position randomFromNode, Position randomToNode)
        {
            KillRunning();
            world.CanCutCorner = corner;
            map.DrawAll(world.GetAllNodes().AsMapDrawPoints());
            map.DrawMarker(randomFromNode.X, randomFromNode.Y, _markerSize, Colors.Blue);
            map.DrawMarker(randomToNode.X, randomToNode.Y, _markerSize, Colors.Blue);
            
            var t = (double) thoroughness / 100;

            var solver = new AStar<Position>(randomFromNode, randomToNode, t);

            _runnerThread = new SolverRunnerThread { Solver = solver };
            
            var solverThread = new Thread(_runnerThread.Run);
            
            timer = new UITimer { Interval = 1d / 20 };
            var lastTpf = 0;
            timer.Elapsed += (sender, args) =>
            {
                var checkPoints = _runnerThread.GetCheckedPositions();
                lastTpf = (int)(lastTpf * .9 + checkPoints.Length * .1);
                tpf.Text = $"TPF: {lastTpf}";
                openPoints.Text = $"Open Points: {solver.OpenCount}";
                closedPoints.Text = $"Closed Points: {solver.ClosedCount}";
                map.DrawAll(checkPoints.AsSearchDrawPoints());
                
                if (solver.State != SolverState.Success) return;
                
                map.DrawAll(world.GetAllNodes().AsMapDrawPoints());
                map.DrawAll(solver.Path.AsPathDrawPoints());
                KillRunning();
            };
            
            solverThread.Start();
            timer.Start();
        }
    }

    internal static class Ext
    {
        private static Color GetColorForLevel(int level)
        {
            var redStart = 0;
            var redDiff = 200;
            var greenStart = 160;
            var greenDiff = -160;
            var blueStart = 0;
            var blueDiff = 0;
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
            return positions.Where(p => p != null).Select(PositionToBlended(Color.FromArgb(0, 0, 255, 255)));
        }
    }
}