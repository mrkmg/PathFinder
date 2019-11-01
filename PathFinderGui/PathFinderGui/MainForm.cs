using System;
using System.Collections.Generic;
using System.Linq;
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
        private int speed = 1;
        private MapWidget map;
        
        public MainForm()
        {
            Title = "Path Finder Tester";
            ClientSize = new Size(750, 550);
            WindowStyle = WindowStyle.None;

            var tSlider = new Slider
            {
                MinValue = 0,
                MaxValue = 100,
                Width = 140,
                Value = 25,
            };

            var scaleSlider = new Slider
            {
                MinValue = 1,
                MaxValue = 8,
                Width = 140,
                Value = 2,
            };

            var speedSlider = new Slider
            {
                MinValue = 1,
                MaxValue = 1000,
                Width = 140,
                Value = 100,
            };

            speedSlider.ValueChanged += (sender, args) => speed = speedSlider.Value;
            
            map = new MapWidget( 4);
            var goButton = new Button
            {
                Text = "Go"
            };

            var makeNewWorkButton = new Button
            {
                Text = "New World",
            };

            World world = null;
            Position start = null;
            Position end = null;
            
            goButton.Click += (sender, args) =>
            {
                if (world != null)
                    Go(world, tSlider.Value, start, end);
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
                    map.DrawPoint(start.X, start.Y, Colors.Blue);
                    map.DrawPoint(end.X, end.Y, Colors.Blue);
                }
            };
            Content = new StackLayout
            {
                Padding = 10,
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
                        Width = 140,
                        Height = 500,
                        Orientation = Orientation.Vertical,
                        Items =
                        {
                            "Path Finder Testing",
                            "Scale",
                            scaleSlider,
                            makeNewWorkButton,
                            "Thoroughness",
                            tSlider,
                            goButton,
                            "Speed",
                            speedSlider
                        }
                    }
                },
            };

            Load += (sender, args) => Content.Height = Height;
            SizeChanged += (sender, args) => Content.Height = Height;
        }

        private Tuple<Position, Position> getRandomPoints(World world)
        {
            var rnd = new Random();
            
            var worldSize = Math.Sqrt(world.XSize * world.XSize + world.YSize + world.YSize);
            var targetSize = (int)(worldSize * 0.95);
                
            Position randomFromNode = null;
            Position randomToNode = null;

            IList<Position> path = null;

            var tries = 0;
            while (path == null)
            {
                tries++;

                if (tries > 2000) break;
                    
                do randomFromNode = world.GetNode(rnd.Next(0, world.XSize - 1), rnd.Next(0, world.YSize - 1));
                while (randomFromNode == null);
                    
                do randomToNode = world.GetNode(rnd.Next(0, world.XSize - 1), rnd.Next(0, world.YSize - 1));
                while (randomToNode == null);

                var x = Math.Abs(randomFromNode.X - randomToNode.X);
                var y = Math.Abs(randomFromNode.Y - randomToNode.Y);
                var dist = Math.Sqrt(x*x + y*y);
                    
                if (dist < targetSize) continue;
                path = AStar.Solve(randomFromNode, randomToNode, 1);
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
        }

        private World MakeWorld()
        {
            KillRunning();
            var world = new World(map.MapWidth, map.MapHeight, new Random());
            return world;
        }

        private void Go(World world, int thoroughness, Position randomFromNode, Position randomToNode)
        {
            KillRunning();
            map.DrawAll(world.GetAllNodes().AsMapDrawPoints());
            map.DrawPoint(randomFromNode.X, randomFromNode.Y, Colors.Blue);
            map.DrawPoint(randomToNode.X, randomToNode.Y, Colors.Blue);
            
            var t = (double) thoroughness / 100;

            var solver = new AStar<Position>(randomFromNode, randomToNode, t);

            timer = new UITimer { Interval = 1d/30d };
            timer.Elapsed += (sender, args) =>
            {
                if (solver.State == SolverState.Running)
                {
                    var ticks = new Position [speed];
                    for (var i = 0; i < speed && solver.State == SolverState.Running; i++)
                    {
                        solver.Tick();
                        ticks[i] = solver.Current;
                    }
                    map.DrawAll(ticks.AsSearchDrawPoints());
                }
                else
                {
                    map.DrawAll(solver.Path.AsPathDrawPoints());
                    KillRunning();
                }
            };

            timer.Start();
            
        }
    }

    internal static class Ext
    {
        internal static Color GetColorForLevel(int level)
        {
            var redStart = 0;
            var redDiff = 164;
            var greenStart = 105;
            var greenDiff = -19;
            var levelAmount = (level - 1) / 8d;

            return Color.FromArgb(redStart + (int)(levelAmount * redDiff), greenStart + (int)(levelAmount * greenDiff), 0);
            
        }
        
        internal static Func<Position, DrawPoint> PositionToBlended(Color color)
        {
            return (p) => new DrawPoint {X = p.X, Y = p.Y, Color = Color.Blend(GetColorForLevel(p.Z), color)};
        }
        
        internal static IEnumerable<DrawPoint> AsMapDrawPoints(this IEnumerable<Position> positions)
        {
            return positions.Select(p => new DrawPoint {Color = GetColorForLevel(p.Z), X = p.X, Y = p.Y});
        }

        internal static IEnumerable<DrawPoint> AsSearchDrawPoints(this IEnumerable<Position> positions)
        {
            return positions.Where(p => p != null).Select(PositionToBlended(Color.FromArgb(255, 255, 255, 100)));
        }
        
        internal static IEnumerable<DrawPoint> AsPathDrawPoints(this IEnumerable<Position> positions)
        {
            return positions.Where(p => p != null).Select(PositionToBlended(Color.FromArgb(0, 0, 255, 255)));
        }
    }
}