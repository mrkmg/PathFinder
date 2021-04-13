using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using PathFinder.Solvers.Generic;
using PathFinderConsole.Sequencer;
using SimpleWorld.Map;

namespace PathFinderConsole.Tests.Many
{
    internal class ManyTest
    {
        public IList<double> GreedyFactors = SequenceBuilder.Build(2, 0, 0.25d).ToList();
        
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
        public int NumberOfTests = 100;
        public int MapHeight = 400;
        public int MapWidth = 400;
        public readonly Random Random = new ();
        private string _outputFile = "./test.csv";
        private readonly object _fileLock = new { };

        public void Run()
        {
            WriteFileHeader();

            SequenceBuilder
                // ReSharper disable once InconsistentlySynchronizedField
                .Build(NumberOfTests)
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

            IGraphSolver<Position> aStarGraphSolver;
            do
            {
                var allNodes = map.GetAllNodes().ToList();
                origin = allNodes[Random.Next(allNodes.Count - 1)];
                destination = allNodes[Random.Next(allNodes.Count - 1)];
                
                aStarGraphSolver = new Greedy<Position>(origin, destination);
                aStarGraphSolver.Start();
            } while (aStarGraphSolver.State == SolverState.Failure);

            var bestCostTo = Math.Round(aStarGraphSolver.PathCost, 3);
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
                BestCostTo = test.BestCostTo,
                EstimatedCostTo = test.EstimatedCostTo,
                Greed = test.Greed,
                PathCost = Math.Round(aStar.PathCost, 3),
                Checks = aStar.Ticks,
                Ticks = timer.ElapsedTicks,
                Time = Math.Round(timer.Elapsed.TotalMilliseconds, 4),
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
                    + "ECT".PadRight(8)
                    + "BC".PadRight(12)
                    + "PC".PadRight(12)
                    + "C".PadRight(8)
                    + "Ti".PadRight(12)
                    + "Tm".PadRight(12)
                );
            }
            
            lock (_fileLock)
            {
                File.WriteAllText(OutputFile, "TestId,SubTestId,EstimatedCostTo,GreedFactor,BestCost,PathCost,Check,Ticks,Time\n");
            }
        }

        private void WriteResult(TestResult result)
        {
            lock (_fileLock)
            {
                File.AppendAllText(OutputFile,  $"{result.TestId},{result.SubId},{result.EstimatedCostTo},{result.Greed},{result.BestCostTo},{result.PathCost},{result.Checks},{result.Ticks},{result.Time}\n");
            }
            lock (Console.Out)
            {
                if (Console.CursorTop >= Console.BufferHeight - 2)
                    Console.CursorTop = 2;
                Console.WriteLine(
                    result.TestId.PadResult(5)
                    + result.SubId.PadResult(5)
                    + result.Greed.PadResult(6)
                    + result.EstimatedCostTo.PadResult(8)
                    + result.BestCostTo.PadResult(12)
                    + result.PathCost.PadResult(12)
                    + result.Checks.PadResult(8)
                    + result.Ticks.PadResult(12)
                    + result.Time.PadResult(12)
                );
            }
        }
    }

    internal class Test
    {
        public int Id;
        public int SubId;
        public int EstimatedCostTo;
        public double BestCostTo;
        public Position Origin;
        public Position Destination;
        public double Greed;
    }

    internal class TestResult
    {
        public int TestId;
        public int SubId;
        public int EstimatedCostTo;
        public double BestCostTo;
        public double Greed;
        public double PathCost;
        public int Checks;
        public long Ticks;
        public double Time;
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
