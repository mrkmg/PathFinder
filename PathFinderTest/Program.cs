using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using PathFinder.Interfaces;
using PathFinder.Solvers;
using PathFinderTest.Sequencer;

namespace PathFinderTest
{
    internal class Program
    {
        private static HashSet<TestNode> _seenClosed;
        private static HashSet<TestNode> _seenOpen;

        private const int TickDelay = 0;
        public static bool CanDiag = true;
        public static bool UseCornerEstimate = true;
        public static int MapWidth = 600;
        public static int MapHeight = 600;
        public static int MaxSearchSpace = 50000;

        private static void Main(string[] args)
        {
            var seq = SequenceBuilder.Build((decimal) 0.5, (decimal) 0, (decimal) 0.05).ToList();
//            RunManyTests(100, seq);

            TestAndShow(true, seq);
        }

        private static void RunManyTests(int numTests, IList<decimal> thorougnesses)
        {
            var results = SequenceBuilder.Build(numTests).AsParallel().Select(i => RunTest(i, thorougnesses));
            ExportResults(results);
        }

        private static void ExportResults(IEnumerable<TestResult> results)
        {
            if (File.Exists("./test.csv")) File.Delete("./test.csv");
            using (var fileHandle = File.OpenWrite("./test.csv"))
            using (var streamWriter = new StreamWriter(fileHandle))
            {
                streamWriter.WriteLine("TestNum,Density,EstLength,Thorougness,Length,Cost");
                foreach (var result in results)
                foreach (var test in result.Tests)
                    streamWriter.WriteLine(result.TestNum + "," +
                                           result.Density + "," +
                                           result.EstLength + "," +
                                           test.Key + "," +
                                           test.Value.Item1 + "," +
                                           test.Value.Item2);
            }
        }

        private static readonly object ThreadLock = new object();

        private static void ThreadPrint(int x, int y, string o, ConsoleColor b = ConsoleColor.Black, ConsoleColor f = ConsoleColor.White)
        {
            lock (ThreadLock)
            {
                Console.BackgroundColor = b;
                Console.ForegroundColor = f;
                Console.CursorLeft = x;
                Console.CursorTop = y;
                Console.Write(o);
                Console.ResetColor();
            }
        }

        private static TestResult RunTest(int testNumber, IList<decimal> thorougnesses)
        {
            var rnd = new Random();
            var width = thorougnesses.Count + 5;
            var cols = Math.Floor((float)Console.WindowWidth / width);
            var left = (int) ((testNumber - 1) % cols) * width;
            var top = (int) Math.Floor((testNumber - 1) / cols) % Console.WindowHeight;
            var density = CanDiag ? 45 + rnd.Next(10) : rnd.Next(35);
            var map = new TestMap(MapWidth, MapHeight, density);
            var o = map.GetAllNodes().OrderBy(n => rnd.Next()).First();
            var d = map.GetAllNodes().OrderBy(n => rnd.Next()).First();
            var estLength = (int)o.EstimatedCostTo(d);
            var result = new TestResult
            {
                Density = density,
                EstLength = estLength,
                TestNum = testNumber,
                Tests = new Dictionary<decimal, Tuple<int, int>>()
            };

            ThreadPrint(left, top, testNumber.ToString().PadRight(3), ConsoleColor.Red, ConsoleColor.Black);
            ThreadPrint(left + 3, top,  new string(' ', width - 3));

            var i = 0;
            var cancelRest = false;
            foreach (var t in thorougnesses.Select(v => Math.Clamp(v, 0, 1)))
            {
                if (cancelRest) continue;

                var aStar = new AStar<TestNode>(o, d) { Thoroughness = (double) t };
                var keepGoing = true;
                while (keepGoing)
                {
                    switch (aStar.State)
                    {
                        case SolverState.Running:
                            aStar.Tick();
                            if (aStar.Ticks > MaxSearchSpace)
                            {
                                aStar.Cancel();
                                ThreadPrint(i + 3 + left, top, "*");
                                keepGoing = false;
                                cancelRest = true;
                            }
                            break;
                        case SolverState.Success:
                            result.Tests.Add(t, new Tuple<int, int>(aStar.GetPath().Count, aStar.Ticks));
                            ThreadPrint(i + 3 + left, top, "+");
                            keepGoing = false;
                            break;
                        case SolverState.Failed:
                            keepGoing = false;
                            ThreadPrint(i + 3 + left, top, "-");
                            break;
                    }
                }
                i++;
            }

            ThreadPrint(left, top, testNumber.ToString().PadRight(3), ConsoleColor.Green, ConsoleColor.Black);
            return result;
        }

        private static bool ShowMenu()
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

        private static void TestAndShow(bool showSeach, IList<decimal> thoroughnesses)
        {

            var rnd = new Random();

            while (true)
            {
                if (ShowMenu()) break;

                var density = CanDiag ? 45 + rnd.Next(10) : rnd.Next(35);
                var map = new TestMap(Console.WindowWidth - 1, Console.WindowHeight - thoroughnesses.Count, density);

                var randomFromNode = map.GetAllNodes().OrderBy(n => rnd.Next()).First();
                var randomToNode = map.GetAllNodes().OrderBy(n => rnd.Next()).First();

                var aStars = new Dictionary<decimal, AStar<TestNode>>();

                PrintMap(map);

                var i = 0;
                foreach (var thoroughness in thoroughnesses)
                {
                    aStars.Add(thoroughness, RunPathFinder(randomFromNode, randomToNode, thoroughness, GetColor(i), map, showSeach));
                    PrintResult(aStars[thoroughness], GetColor(i), map.YSize + i);
                    i++;
                }

                i = 0;
                foreach (var thoroughness in thoroughnesses)
                {
                    if (aStars[thoroughness].State == SolverState.Success) PrintPath(aStars[thoroughness].GetPath(), GetColor(i));
                    i++;
                }

                while (Console.ReadKey().Key != ConsoleKey.Enter) ;
            }
        }

        private static void PrintResult(AStar<TestNode> aStar, ConsoleColor color, int offset)
        {
            Console.ResetColor();
            Console.BackgroundColor = color;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.CursorLeft = 0;
            Console.CursorTop = offset;
            Console.Write("                     ");
            Console.CursorLeft = 0;

            if (aStar.State == SolverState.Success)
            {
                var path = aStar.GetPath();
                Console.Write(aStar.Thoroughness.ToString(CultureInfo.CurrentCulture).PadLeft(4) + " : " + path.Count + " | " + aStar.Ticks);
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

        private static AStar<TestNode> RunPathFinder(TestNode origin, TestNode dest, decimal thoroughness, ConsoleColor color, TestMap map, bool print = true)
        {
            var aStar = new AStar<TestNode>(origin, dest) { Thoroughness = (double) thoroughness };
            _seenClosed = new HashSet<TestNode> { origin };
            _seenOpen = new HashSet<TestNode> { origin };
            PrintEndPoints(aStar);

            while (aStar.State == SolverState.Running)
            {
                aStar.Tick();

                if (print) PrintFinding(aStar, color);

                if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.N)
                {
                    aStar.Cancel();
                    break;
                }
            }

            foreach (var node in _seenOpen) PrintNode(node.X, node.Y, map);
            foreach (var node in _seenClosed) PrintNode(node.X, node.Y, map);
            PrintNode(aStar.Current.X, aStar.Current.Y, map);

            return aStar;
        }

        private static void PrintFinding(AStar<TestNode> astar, ConsoleColor color)
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

        private static void PrintEndPoints(AStar<TestNode> astar)
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

        private static void PrintPath(IList<TestNode> path, ConsoleColor color)
        {
            foreach (var node in path)
            {
                Console.CursorLeft = node.X;
                Console.CursorTop = node.Y;
                Console.BackgroundColor = color;
                Console.Write(" ");
            }
        }

        private static void PrintMap(TestMap map)
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

        private static void PrintNode(int x, int y, TestMap map)
        {
            Console.CursorLeft = x;
            Console.CursorTop = y;
            if ((map.AllNodes.ContainsKey(x) && map.AllNodes[x].ContainsKey(y)))
            {
                Console.BackgroundColor = ConsoleColor.White;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Black;
            }

            Console.Write(" ");
            Console.ResetColor();
        }
    }

    internal struct TestResult
    {
        public int TestNum;
        public int Density;
        public int EstLength;
        public Dictionary<decimal, Tuple<int, int>> Tests;
    }

    internal class TestMap
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
                        AllNodes[x].Add(y, new TestNode(x, y) { Map = this });
                    }
        }

        public IEnumerable<TestNode> GetAllNodes()
        {
            return AllNodes.Values.SelectMany(t => t.Values);
        }
    }

    internal class TestNode : INode
    {
        public readonly int X;
        public readonly int Y;
        public TestMap Map;

        public TestNode(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(INode other) => other is TestNode n && X == n.X && Y == n.Y;
        public override string ToString() => X + "::" + Y;
        public bool Equals(INode x, INode y) => x is TestNode xn && y is TestNode yn && xn.Equals(yn);

        public override int GetHashCode() => 17 * (23 + X.GetHashCode()) * (23 + Y.GetHashCode());
    
        public double RealCostTo(INode node) => 1;

        public double EstimatedCostTo(INode node)
            => !(node is TestNode n) ? double.MaxValue :
                Program.UseCornerEstimate ? EstimateDistanceToSq(n) : EstimateDistanceToAbs(n);

        private double EstimateDistanceToAbs(TestNode n) => Math.Abs(n.X - X) + Math.Abs(n.Y - Y);
        private double EstimateDistanceToSq(TestNode n) => Math.Sqrt(Math.Pow(n.X - X, 2) + Math.Pow(n.Y - Y, 2));

        public IEnumerable<INode> GetNeighbors()
        {
            var possibleLocations = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(X - 1, Y),
                new Tuple<int, int>(X, Y - 1),
                new Tuple<int, int>(X, Y + 1),
                new Tuple<int, int>(X + 1, Y)
            };

            if (Program.CanDiag)
            {
                possibleLocations.Add(new Tuple<int, int>(X + 1, Y - 1));
                possibleLocations.Add(new Tuple<int, int>(X - 1, Y - 1));
                possibleLocations.Add(new Tuple<int, int>(X - 1, Y + 1));
                possibleLocations.Add(new Tuple<int, int>(X + 1, Y + 1));
            }

            return possibleLocations
                .Where(l => Map.AllNodes.ContainsKey(l.Item1) && Map.AllNodes[l.Item1].ContainsKey(l.Item2))
                .Select(l => Map.AllNodes[l.Item1][l.Item2]);
        }
    }
}
