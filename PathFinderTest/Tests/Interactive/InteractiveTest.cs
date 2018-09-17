using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using PathFinder.Interfaces;
using PathFinder.Solvers;
using PathFinderTest.Map;

namespace PathFinderTest.Tests.Many
{
    class InteractiveTest
    {
        private int TickDelay = 0;

        private IList<decimal> _thoroughnesses;

        private bool showSearch = true;
        private bool CanDiag = true;
        private bool UseCornerEstimate = true;

        private HashSet<Position> _seenClosed;
        private HashSet<Position> _seenOpen;

        private World _world;

        public InteractiveTest(IList<decimal> thoroughnesses)
        {
            _thoroughnesses = thoroughnesses;
        }

        public void Main()
        {

            var rnd = new Random();

            while (true)
            {
                if (ShowMenu()) break;
                MakeWorld();

                var randomFromNode = _world.GetAllNodes().OrderBy(n => rnd.Next()).First();
                var randomToNode = _world.GetAllNodes().OrderBy(n => rnd.Next()).First();

                var aStars = new Dictionary<decimal, AStar<Position>>();

                PrintMap(_world);

                var i = 0;
                foreach (var thoroughness in _thoroughnesses)
                {
                    aStars.Add(thoroughness, RunPathFinder(randomFromNode, randomToNode, thoroughness, GetColor(i)));
                    PrintResult(aStars[thoroughness], GetColor(i), Console.WindowHeight - i);
                    i++;
                }

                i = 0;
                foreach (var thoroughness in _thoroughnesses)
                {
                    if (aStars[thoroughness].State == SolverState.Success) PrintPath(aStars[thoroughness].Path, GetColor(i));
                    i++;
                }

                while (Console.ReadKey().Key != ConsoleKey.Enter) ;
            }
        }

        private void MakeWorld()
        {
            var rnd = new Random();
            var density = CanDiag ? 45 + rnd.Next(10) : rnd.Next(35);
            _world = new World(Console.WindowWidth - 1, Console.WindowHeight, density)
            {
                CanCutCorner = CanDiag,
                EstimateType = UseCornerEstimate ? EstimateType.Square : EstimateType.Absolute
            };
        }

        private void PrintPath(IList<Position> path, ConsoleColor color)
        {
            foreach (var node in path)
            {
                Console.CursorLeft = node.X;
                Console.CursorTop = node.Y;
                Console.BackgroundColor = color;
                Console.Write(" ");
            }
        }

        private void PrintMap(World map)
        {
            Console.CursorLeft = 0;
            Console.CursorTop = 0;
            for (var y = 0; y < map.YSize; y++)
            {
                for (var x = 0; x < map.XSize; x++)
                {
                    PrintNode(x, y, map);
                }
            }
        }

        private void PrintNode(int x, int y, World map)
        {
            Console.CursorLeft = x;
            Console.CursorTop = y;
            if ((map.AllNodes.ContainsKey(x) && map.AllNodes[x].ContainsKey(y)))
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(map.AllNodes[x][y].W);
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" ");
            }

            Console.ResetColor();
        }

        private bool ShowMenu()
        {
            while (true)
            {
                Console.ResetColor();
                Console.Clear();
                Console.WriteLine("(d) Can Diag: " + CanDiag);
                Console.WriteLine("(c) Use Corner: " + UseCornerEstimate);
                Console.WriteLine("(ENTER) Run");
                Console.WriteLine("(q) Quit");
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Q:
                        return true;
                    case ConsoleKey.Enter:
                        return false;
                    case ConsoleKey.D:
                        CanDiag = !CanDiag;
                        break;
                    case ConsoleKey.C:
                        UseCornerEstimate = !UseCornerEstimate;
                        break;
                }
            }
        }

        private AStar<Position> RunPathFinder(Position origin, Position dest, decimal thoroughness, ConsoleColor color)
        {
            var aStar = new AStar<Position>(origin, dest) { Thoroughness = (double)thoroughness };
            _seenClosed = new HashSet<Position> { origin };
            _seenOpen = new HashSet<Position> { origin };
            PrintEndPoints(aStar);

            while (aStar.State == SolverState.Running)
            {
                aStar.Tick();

                if (showSearch) PrintFinding(aStar, color);

                if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.N)
                {
                    aStar.Cancel();
                    break;
                }
            }

            foreach (var node in _seenOpen) PrintNode(node.X, node.Y, _world);
            foreach (var node in _seenClosed) PrintNode(node.X, node.Y, _world);
            PrintNode(aStar.Current.X, aStar.Current.Y, _world);

            return aStar;
        }

        private void PrintFinding(AStar<Position> astar, ConsoleColor color)
        {
            Thread.Sleep(TickDelay);
            var openNodes = astar.Open.Where(n => !_seenOpen.Contains(n));
            var closedNodes = astar.Closed.Where(n => !_seenClosed.Contains(n));

            foreach (var openNode in openNodes)
            {
                _seenOpen.Add(openNode);
                Console.CursorLeft = openNode.X;
                Console.CursorTop = openNode.Y;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Write(" ");
            }

            foreach (var closednode in closedNodes)
            {
                _seenOpen.Remove(closednode);
                Console.CursorLeft = closednode.X;
                Console.CursorTop = closednode.Y;
                Console.BackgroundColor = color;
                Console.Write(" ");

                if (!closednode.Equals(astar.Current)) _seenClosed.Add(closednode);
            }

            if (!astar.Current.Equals(astar.Origin))
            {
                Console.CursorLeft = astar.Current.X;
                Console.CursorTop = astar.Current.Y;
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.Write(" ");
            }
        }

        private static void PrintEndPoints(AStar<Position> astar)
        {
            Console.CursorLeft = astar.Origin.X;
            Console.CursorTop = astar.Origin.Y;
            Console.BackgroundColor = ConsoleColor.Magenta;
            Console.Write(" ");

            Console.CursorLeft = astar.Destination.X;
            Console.CursorTop = astar.Destination.Y;
            Console.BackgroundColor = ConsoleColor.Magenta;
            Console.Write(" ");
        }



        private void PrintResult(AStar<Position> aStar, ConsoleColor color, int offset)
        {
            Console.ResetColor();
            Console.BackgroundColor = color;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.CursorLeft = 0;
            Console.CursorTop = offset;
            Console.Write("                     ");
            Console.CursorLeft = 0;

            if (aStar.State == SolverState.Success)
            {;
                Console.Write(aStar.Thoroughness.ToString(CultureInfo.CurrentCulture).PadLeft(4) + " : " + aStar.Cost + " | " + aStar.Ticks);
            }
            else
            {
                Console.Write(aStar.Thoroughness.ToString(CultureInfo.CurrentCulture).PadLeft(4) + " No Path");
            }
        }

        private static ConsoleColor GetColor(int i)
        {
            switch (i % 5)
            {
                case 0: return ConsoleColor.Cyan;
                case 1: return ConsoleColor.Blue;
                case 2: return ConsoleColor.Green;
                case 3: return ConsoleColor.Yellow;
                default: return ConsoleColor.Magenta;
            }
        }
    }
}
