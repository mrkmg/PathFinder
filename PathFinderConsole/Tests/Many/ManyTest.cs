using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using PathFinder.Solvers.Generic;
using SimpleWorld.Map;

namespace PathFinderConsole.Tests.Many
{
    internal class ManyTest
    {
        public IList<double> GreedyFactors = Enumerable.Sequence(0, 2, 0.25d).ToList();
        
        public string OutputFile
        {
            get => _outputFile;
            set
            {
                _outputFile = value.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                var path = Path.GetDirectoryName(_outputFile);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        public bool CanDiag = true;
        public int NumberOfTests;
        public int MapHeight;
        public int MapWidth;
        private readonly Random _random = new ();
        private string _outputFile;
        private readonly object _fileLock = new { };

        public void Run()
        {
            WriteFileHeader();

            Enumerable
                // ReSharper disable once InconsistentlySynchronizedField
                .Sequence(NumberOfTests)
                .SelectMany(BuildTest)
                .AsParallel()
                .Select(RunTest)
                .Where(r => r != null)
                .ForAll(WriteResult);
        }

        private IEnumerable<Test> BuildTest(int testId)
        {
            // ReSharper disable twice InconsistentlySynchronizedField
            var map = new World(MapWidth, MapHeight)
            {
                CanCutCorner = CanDiag
            };

            Position origin;
            Position destination;

            IGraphSolver<Position> solver;
            do
            {
                var allNodes = map.GetAllNodes().ToList();
                origin = allNodes[_random.Next(allNodes.Count - 1)];
                destination = allNodes[_random.Next(allNodes.Count - 1)];
                
                solver = new Greedy<Position>(origin, destination);
                solver.Start();
            } while (solver.State == SolverState.Failure);

            solver = new BreadthFirst<Position>(origin, destination);
            solver.Start();
            var bestCostTo = solver.PathCost;
            var estimatedCostTo = (int)origin.EstimatedCostTo(destination);

            var subTestNum = 0;
            foreach (var greed in GreedyFactors)
            {
                yield return new Test
                {
                    Id = testId,
                    SubId = subTestNum,
                    Origin = origin,
                    Destination = destination,
                    EstimatedCostTo = estimatedCostTo,
                    BestCostTo = bestCostTo,
                    Greed = greed,
                };

                subTestNum++;
            }
        }

        private TestResult RunTest(Test test)
        {
            var aStar = new AStar<Position>(test.Origin, test.Destination, test.Greed);
            var timer = new Stopwatch();
            timer.Start();
            aStar.Start();
            timer.Stop();

            if (aStar.State == SolverState.Failure) return null;

            return new TestResult
            {
                TestId = test.Id,
                SubId = test.SubId,
                BestCostTo = Math.Round(test.BestCostTo, 4),
                EstimatedCostTo = Math.Round(test.EstimatedCostTo, 4),
                Greed = test.Greed,
                PathCost = Math.Round(aStar.PathCost, 3),
                Checks = aStar.Ticks,
                Ticks = timer.ElapsedTicks,
                Time = Math.Round(timer.Elapsed.TotalMilliseconds, 4),
                CostRatio = Math.Round(test.BestCostTo / aStar.PathCost, 4),
                TicksRatio = Math.Round(aStar.PathCost / aStar.Ticks, 4)
            };
        }

        private void WriteFileHeader()
        {
            lock (Console.Out)
            {
                Console.Clear();
                Console.CursorLeft = 0;
                Console.CursorTop = 0;
                Console.WriteLine($"{MapWidth}x{MapHeight} - {NumberOfTests}");
                Console.WriteLine(
                    "TId".PadRight(5)
                    + "StId".PadRight(5)
                    + "T".PadRight(6)
                    + "ECT".PadRight(9)
                    + "BC".PadRight(9)
                    + "PC".PadRight(12)
                    + "C".PadRight(8)
                    + "Ti".PadRight(12)
                    + "Tm".PadRight(12)
                    + "Cr".PadRight(8)
                    + "Tr".PadRight(8)
                );
            }
            
            lock (_fileLock)
            {
                File.WriteAllText(OutputFile, "TestId,SubTestId,EstimatedCostTo,GreedFactor,BestCost,PathCost,Check,Ticks,Time,CostRatio,TicksRatio\n");
            }
        }

        private void WriteResult(TestResult result)
        {
            lock (_fileLock)
            {
                File.AppendAllText(OutputFile,  $"{result.TestId},{result.SubId},{result.EstimatedCostTo},{result.Greed},{result.BestCostTo},{result.PathCost},{result.Checks},{result.Ticks},{result.Time},{result.CostRatio},{result.TicksRatio}\n");
            }
            lock (Console.Out)
            {
                if (Console.CursorTop >= Console.BufferHeight - 2)
                    Console.CursorTop = 2;
                Console.WriteLine(
                    result.TestId.PadResult(5)
                    + result.SubId.PadResult(5)
                    + result.Greed.PadResult(6)
                    + result.EstimatedCostTo.PadResult(9)
                    + result.BestCostTo.PadResult(9)
                    + result.PathCost.PadResult(12)
                    + result.Checks.PadResult(8)
                    + result.Ticks.PadResult(12)
                    + result.Time.PadResult(12)
                    + result.CostRatio.PadResult(8)
                    + result.TicksRatio.PadResult(8)
                );
            }
        }
    }

    internal class Test
    {
        public int Id;
        public int SubId;
        public double EstimatedCostTo;
        public double BestCostTo;
        public Position Origin;
        public Position Destination;
        public double Greed;
    }

    internal class TestResult
    {
        public int TestId;
        public int SubId;
        public double EstimatedCostTo;
        public double BestCostTo;
        public double Greed;
        public double PathCost;
        public int Checks;
        public long Ticks;
        public double Time;
        public double CostRatio;
        public double TicksRatio;
    }

    internal static class TestFormats
    {
        public static string PadResult(this int result, int length) => result.ToString().PadRight(length);
        public static string PadResult(this float result, int length) => result.ToString(CultureInfo.InvariantCulture).PadRight(length);
        public static string PadResult(this double result, int length) => result.ToString(CultureInfo.InvariantCulture).PadRight(length);
        public static string PadResult(this decimal result, int length) => result.ToString(CultureInfo.InvariantCulture).PadRight(length);
        public static string PadResult(this long result, int length) => result.ToString(CultureInfo.InvariantCulture).PadRight(length);
    }
}
