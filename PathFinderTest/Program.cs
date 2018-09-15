using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using PathFinder.Interfaces;
using PathFinder.Solvers;

namespace PathFinderTest
{
    class Program
    {
         private static HashSet<TestNode> SeenClosed;
        private static HashSet<TestNode> SeenOpen;

        private static int tickDelay = 5;

        static void Main(string[] args)
        {
//            TestAndShow(true, new List<double>{0.1, 0.3, 0.4, 0.45, 0.5});
            RunManyTests();
        }

        static void RunManyTests()
        {
            var results = new List<Dictionary<double, AStar<TestNode>>>();
            var types = new List<double> { 0, 0.1, 0.2, 0.3, 0.4, 0.5 };
            var numTests = 20;

            int i;

            var rnd = new Random();
            for (i = 0; i < numTests; i++)
            {
                Console.Write(i);
                var map = new TestMap(500, 500, rnd.Next(40));
                var result = new Dictionary<double, AStar<TestNode>>();
                var o = map.GetAllNodes().OrderBy(n => rnd.Next()).First();
                var d = map.GetAllNodes().OrderBy(n => rnd.Next()).First();

                foreach (var t in types)
                {
                    Console.Write(" " + t);
                    var aStar = new AStar<TestNode>(o, d) {Thoroughness = t};
                    while (aStar.State == SolverState.Running) aStar.Tick();
                    result.Add(t, aStar);
                }

                results.Add(result);

                Console.WriteLine();
            }

            Console.Clear();
            i = 0;
            foreach (var result in results)
            {
                foreach (var typeResult in result)
                {
                    if (typeResult.Value.State == SolverState.Success)
                    Console.WriteLine(i + "," + typeResult.Key + "," + typeResult.Value.Ticks + "," + typeResult.Value.GetPath().Count);
                }
                i++;
            }

            Console.ReadKey();

        }

        static void TestAndShow(bool showSeach, List<double> thoroughnesses)
        {

            var rnd = new Random();

            while (true)
            {
                var map = new TestMap(290, 70, rnd.Next(40));

                var randomFromNode = map.GetAllNodes().OrderBy(n => rnd.Next()).First();
                var randomToNode = map.GetAllNodes().OrderBy(n => rnd.Next()).First();

                var aStars = new Dictionary<double, AStar<TestNode>>();


                var i = 0;
                foreach (var thoroughness in thoroughnesses)
                {
                    if(showSeach) PrintMap(map);
                    aStars.Add(thoroughness, RunPathFinder(randomFromNode, randomToNode, thoroughness, GetColor(i), showSeach));
                    PrintResult(aStars[thoroughness], GetColor(i), map.YSize + i);
                    i++;
                }

                PrintMap(map);

                i = 0;
                foreach (var thoroughness in thoroughnesses)
                {
                    if (aStars[thoroughness].State == SolverState.Success) PrintPath(aStars[thoroughness].GetPath(), GetColor(i));
                    i++;
                }


                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Q) break;
            }
        }

        static void PrintResult(AStar<TestNode> aStar, ConsoleColor color, int offset)
        {
            if (aStar.State == SolverState.Success)
            {
                var path = aStar.GetPath();
                Console.ResetColor();
                Console.BackgroundColor = color;
                Console.CursorLeft = 0;
                Console.CursorTop = offset;
                Console.Write("                     ");
                Console.CursorLeft = 0;
                Console.Write(aStar.Thoroughness.ToString(CultureInfo.CurrentCulture).PadLeft(4) + " : " + path.Count + " | " + aStar.Ticks);
            }
            else
            {
                Console.ResetColor();
                Console.BackgroundColor = color;
                Console.CursorLeft = 0;
                Console.CursorTop = offset;
                Console.Write("                     ");
                Console.CursorLeft = 0;
                Console.Write(aStar.Thoroughness.ToString(CultureInfo.CurrentCulture).PadLeft(4) + " No Path");
            }
        }

        static ConsoleColor GetColor(int i)
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

        static AStar<TestNode> RunPathFinder(TestNode origin, TestNode dest, double thoroughness, ConsoleColor color, bool print = true)
        {
            var aStar = new AStar<TestNode>(origin, dest) { Thoroughness = thoroughness };
            SeenClosed = new HashSet<TestNode> { origin };
            SeenOpen = new HashSet<TestNode> { origin };
            PrintEndPoints(aStar);

            while (aStar.State == SolverState.Runnning)
            {
                aStar.Tick();

                if (print) PrintFinding(aStar, color);
            }

            return aStar;
        }

        static void PrintFinding(AStar<TestNode> astar, ConsoleColor color)
        {
            Thread.Sleep(tickDelay);
            var openNodes = astar.OpenNodes.Where(n => !SeenOpen.Contains(n));
            var closedNodes = astar.ClosedNodes.Where(n => !SeenClosed.Contains(n));

            foreach (var openNode in openNodes)
            {
                SeenOpen.Add(openNode);
                Console.CursorLeft = openNode.X;
                Console.CursorTop = openNode.Y;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Write(" ");
            }

            foreach (var closednode in closedNodes)
            {
                SeenOpen.Remove(closednode);
                Console.CursorLeft = closednode.X;
                Console.CursorTop = closednode.Y;
                Console.BackgroundColor = color;
                Console.Write(" ");

                if (!closednode.Equals(astar.CurrentNode)) SeenClosed.Add(closednode);
            }

            if (!astar.CurrentNode.Equals(astar.OriginNode))
            {
                Console.CursorLeft = astar.CurrentNode.X;
                Console.CursorTop = astar.CurrentNode.Y;
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.Write(" ");
            }
        }

        static void PrintEndPoints(AStar<TestNode> astar)
        {
            Console.CursorLeft = astar.OriginNode.X;
            Console.CursorTop = astar.OriginNode.Y;
            Console.BackgroundColor = ConsoleColor.Magenta;
            Console.Write(" ");

            Console.CursorLeft = astar.DestinationNode.X;
            Console.CursorTop = astar.DestinationNode.Y;
            Console.BackgroundColor = ConsoleColor.Magenta;
            Console.Write(" ");
        }

        static void PrintPath(IList<TestNode> path, ConsoleColor color)
        {
            foreach (var node in path)
            {
                Console.CursorLeft = node.X;
                Console.CursorTop = node.Y;
                Console.BackgroundColor = color;
                Console.Write(" ");
            }
        }

        static void PrintMap(TestMap map)
        {
            Console.CursorLeft = 0;
            Console.CursorTop = 0;
            for (var y = 0; y < map.YSize; y++)
            {
                for (var x = 0; x < map.XSize; x++)
                {
                    if ((map.AllNodes.ContainsKey(x) && map.AllNodes[x].ContainsKey(y)))
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                    } else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                    }

                    Console.Write(" ");

                    Console.ResetColor();
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                }

                Console.Write("\n");
            }
        }

        static void WriteResult(string name, int realDistance, Stopwatch timer)
        {
            Console.WriteLine(
                name.PadRight(20) + ": " +
                realDistance.ToString().PadLeft(6) + " " +
                timer.ElapsedTicks.ToString().PadLeft(12) +
                timer.ElapsedMilliseconds.ToString().PadLeft(6)
            );
        }
    }

    class TestMap
    {
        public int XSize;
        public int YSize;
        public Dictionary<int, Dictionary<int, TestNode>> AllNodes = new Dictionary<int, Dictionary<int, TestNode>>();

        public TestMap(int xSize, int ySize, int r)
        {
            XSize = xSize;
            YSize = ySize;
            var rand = new Random();
            for (var x = 0; x < XSize; x++)
                for (var y = 0; y < YSize; y++)
                    if (rand.Next(100) > r)
                    {
                        if (!AllNodes.ContainsKey(x)) AllNodes.Add(x, new Dictionary<int, TestNode>());
                        AllNodes[x].Add(y, new TestNode { Map = this, X = x, Y = y});
                    }
        }

        public IEnumerable<TestNode> GetAllNodes()
        {
            return AllNodes.Values.SelectMany(t => t.Values);
        }
    }

    class TestNode : INode
    {
        public int X;
        public int Y;
        public TestMap Map;

        public bool Equals(INode other) => other is TestNode n && X == n.X && Y == n.Y;

        public override string ToString() => X + "::" + Y;

        public bool Equals(INode x, INode y) => x is TestNode xn && y is TestNode yn && xn.Equals(yn);

        public int GetHashCode(INode obj)
        {
            if (!(obj is TestNode n)) return 0;
            var hash = 17;
            hash = hash * 23 + n.X.GetHashCode();
            hash = hash * 23 + n.Y.GetHashCode();
            return hash;
        }

        public double RealCostTo(INode node)
        {
            return 1;
        }

        public double EstimateDistanceTo(INode node)
        {
//            return node is TestNode n ? EstimateDistanceToSq(n) : double.MaxValue;
            return node is TestNode n ? (EstimateDistanceToAbs(n) + EstimateDistanceToSq(n)) / 2 : double.MaxValue;
        }

        private double EstimateDistanceToAbs(TestNode n) => Math.Abs(n.X - X) + Math.Abs(n.Y - Y);
        private double EstimateDistanceToSq(TestNode n) => Math.Sqrt(Math.Pow(n.X - X, 2) + Math.Pow(n.Y - Y, 2));

        public IEnumerable<INode> GetNeighbors()
        {
            var possibleLocations = new List<Tuple<int, int>>
            {
//                new Tuple<int, int>(X - 1, Y - 1),
                new Tuple<int, int>(X - 1, Y),
//                new Tuple<int, int>(X - 1, Y + 1),
                new Tuple<int, int>(X, Y - 1),
                new Tuple<int, int>(X, Y + 1),
//                new Tuple<int, int>(X + 1, Y - 1),
                new Tuple<int, int>(X + 1, Y),
//                new Tuple<int, int>(X + 1, Y + 1),
            };

            return possibleLocations
                .Where(l => Map.AllNodes.ContainsKey(l.Item1) && Map.AllNodes[l.Item1].ContainsKey(l.Item2))
                .Select(l => Map.AllNodes[l.Item1][l.Item2]);
        }
    }
    }
}
